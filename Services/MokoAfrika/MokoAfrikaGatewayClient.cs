using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace KelasiNaBiso.Services.MokoAfrika
{
    public interface IMokoAfrikaGatewayClient
    {
        Task<MokoGatewayResponse> SendAsync(Dictionary<string, object?> payload, CancellationToken cancellationToken = default);
    }

    public class MokoGatewayResponse
    {
        public int HttpStatusCode { get; set; }
        public string RawBody { get; set; } = string.Empty;
        public JsonDocument? Parsed { get; set; }
        public bool IsSuccess { get; set; }
        public bool IsPending { get; set; }
        public bool IsFailure { get; set; }
        public string? Reference { get; set; }
        public string? Status { get; set; }
        public string? TransactionId { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class MokoAfrikaGatewayClient : IMokoAfrikaGatewayClient
    {
        private readonly HttpClient _httpClient;
        private readonly MokoSettings _settings;
        private readonly ILogger<MokoAfrikaGatewayClient> _logger;

        public MokoAfrikaGatewayClient(
            HttpClient httpClient,
            IOptions<MokoSettings> settings,
            ILogger<MokoAfrikaGatewayClient> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<MokoGatewayResponse> SendAsync(
            Dictionary<string, object?> payload,
            CancellationToken cancellationToken = default)
        {
            var url = _settings.GatewayUrl;
            var json = JsonSerializer.Serialize(payload);
            _logger.LogInformation("MOKO gateway POST {Url} ref={Reference}", url, payload.GetValueOrDefault("reference"));

            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            using var response = await _httpClient.PostAsync(url, content, cancellationToken);
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken);

            var result = new MokoGatewayResponse
            {
                HttpStatusCode = (int)response.StatusCode,
                RawBody = rawBody
            };

            if (string.IsNullOrWhiteSpace(rawBody))
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Réponse gateway vide";
                return result;
            }

            try
            {
                result.Parsed = JsonDocument.Parse(rawBody);
                var root = result.Parsed.RootElement;
                result.IsSuccess = MokoGatewayResponseParser.IsDefinitiveSuccess(root);
                result.IsPending = MokoGatewayResponseParser.IsPending(root);
                result.IsFailure = MokoGatewayResponseParser.IsDefinitiveFailure(root);
                result.Reference = MokoGatewayResponseParser.GetString(root, "Reference", "reference");
                result.Status = MokoGatewayResponseParser.GetString(root, "Status", "trans_status", "status");
                result.TransactionId = MokoGatewayResponseParser.GetString(root, "Transaction_id", "transaction_id");
                if (result.IsFailure || (!result.IsSuccess && !result.IsPending))
                {
                    result.ErrorMessage = MokoGatewayResponseParser.GetString(root, "Comment", "trans_status_description", "resultCodeErrorDescription")
                        ?? rawBody;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Réponse MOKO non JSON");
                result.IsSuccess = false;
                result.ErrorMessage = rawBody;
            }

            return result;
        }
    }

    public static class MokoGatewayResponseParser
    {
        private static readonly HashSet<string> SuccessStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "success", "successful", "approved", "paid"
        };

        private static readonly HashSet<string> PendingStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "pending", "initiated", "processing", "in_progress"
        };

        /// <summary>
        /// Statuts gateway ambigus sur un check précoce (USSD encore en cours).
        /// Sans <c>resultCodeError</c>, ne constituent pas un échec définitif.
        /// </summary>
        private static readonly HashSet<string> SoftAmbiguousFailureStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "error", "failed"
        };

        /// <summary>Refus / annulation client ou timeout — échec définitif même sans resultCodeError.</summary>
        private static readonly HashSet<string> HardFailureStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "cancelled", "rejected", "declined", "timeout"
        };

        /// <summary>Alias de IsDefinitiveSuccess — ne traite plus resultCode "0" seul comme succès.</summary>
        public static bool IsSuccess(JsonElement root) => IsDefinitiveSuccess(root);

        public static string? GetTransactionStatus(JsonElement root) =>
            GetString(root, "Status", "trans_status", "status")?.ToLowerInvariant();

        public static bool IsDefinitiveSuccess(JsonElement root)
        {
            if (HasResultCodeError(root))
                return false;

            var status = GetTransactionStatus(root);
            return !string.IsNullOrEmpty(status) && SuccessStatuses.Contains(status);
        }

        public static bool IsPending(JsonElement root)
        {
            if (IsDefinitiveSuccess(root) || IsDefinitiveFailure(root))
                return false;

            var status = GetTransactionStatus(root);
            if (!string.IsNullOrEmpty(status) && PendingStatuses.Contains(status))
                return true;

            // Status Error/Failed sans resultCodeError = souvent check trop tôt (USSD en cours)
            if (!string.IsNullOrEmpty(status) && SoftAmbiguousFailureStatuses.Contains(status))
                return true;

            // resultCode "0" sans status explicite = requête acceptée (USSD envoyé), pas encore confirmée
            var resultCode = GetString(root, "resultCode");
            return resultCode == "0";
        }

        public static bool IsDefinitiveFailure(JsonElement root)
        {
            if (HasResultCodeError(root))
                return true;

            var status = GetTransactionStatus(root);
            return !string.IsNullOrEmpty(status) && HardFailureStatuses.Contains(status);
        }

        /// <summary>
        /// Échec « soft » : Status Error/Failed sans resultCodeError — ne doit pas tuer un PayIn pending USSD.
        /// </summary>
        public static bool IsSoftAmbiguousFailure(JsonElement root)
        {
            if (HasResultCodeError(root) || IsDefinitiveSuccess(root) || IsDefinitiveFailure(root))
                return false;

            var status = GetTransactionStatus(root);
            return !string.IsNullOrEmpty(status) && SoftAmbiguousFailureStatuses.Contains(status);
        }

        public static bool HasResultCodeError(JsonElement root)
        {
            var resultCodeError = GetString(root, "resultCodeError");
            return !string.IsNullOrEmpty(resultCodeError) && resultCodeError != "0";
        }

        public static string? GetString(JsonElement root, params string[] names)
        {
            foreach (var name in names)
            {
                if (root.TryGetProperty(name, out var prop) && prop.ValueKind == JsonValueKind.String)
                    return prop.GetString();

                if (root.TryGetProperty(name, out prop) && prop.ValueKind == JsonValueKind.Number)
                    return prop.GetRawText();
            }
            return null;
        }
    }

    public static class MokoReferenceGenerator
    {
        public static string NewReference()
        {
            var ts = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var suffix = Random.Shared.Next(1000, 9999);
            return $"MOKO_{ts}_{suffix}";
        }
    }
}
