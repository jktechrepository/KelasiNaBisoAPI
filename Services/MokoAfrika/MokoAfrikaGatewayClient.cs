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
            "success", "successful", "approved", "paid", "completed", "succeeded"
        };

        private static readonly HashSet<string> PendingStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "pending", "initiated", "processing", "in_progress"
        };

        /// <summary>
        /// Statuts gateway ambigus sur un check précoce (USSD encore en cours).
        /// Uniquement <c>Status: Error|Failed</c> <b>sans</b> <c>Trans_Status</c> terminal.
        /// </summary>
        private static readonly HashSet<string> SoftAmbiguousFailureStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "error", "failed"
        };

        /// <summary>Refus / annulation explicite au niveau Status (sans Trans_Status).</summary>
        private static readonly HashSet<string> HardFailureStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "cancelled", "rejected", "declined", "timeout"
        };

        /// <summary>Statuts finaux négatifs dans <c>Trans_Status</c> (annulation / échec MOKO).</summary>
        private static readonly HashSet<string> TerminalTransFailureStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "failed", "cancelled", "rejected", "declined", "timeout"
        };

        /// <summary>Alias de IsDefinitiveSuccess — ne traite plus resultCode "0" seul comme succès.</summary>
        public static bool IsSuccess(JsonElement root) => IsDefinitiveSuccess(root);

        public static string? GetTransactionStatus(JsonElement root) =>
            GetString(root, "Trans_Status", "trans_Status", "TransStatus", "Status", "trans_status", "status")
                ?.ToLowerInvariant();

        /// <summary>Valeur brute de <c>Trans_Status</c> uniquement (statut final MOKO).</summary>
        public static string? GetTransStatus(JsonElement root) =>
            GetString(root, "Trans_Status", "trans_Status", "TransStatus", "trans_status")?.ToLowerInvariant();

        /// <summary>
        /// Refus explicite côté Status (cancelled/rejected/…) — distinct du soft Error précoce.
        /// </summary>
        public static bool IsClientRefusalStatus(JsonElement root)
        {
            var statusOnly = GetString(root, "Status", "status")?.ToLowerInvariant();
            return !string.IsNullOrEmpty(statusOnly) && HardFailureStatuses.Contains(statusOnly);
        }

        /// <summary>
        /// Échec terminal : <c>Trans_Status</c> Failed/cancelled/… ou Status cancelled/rejected/…,
        /// y compris pendant la fenêtre USSD (annulation client).
        /// </summary>
        public static bool IsTerminalFailureStatus(JsonElement root)
        {
            var trans = GetTransStatus(root);
            if (!string.IsNullOrEmpty(trans) && TerminalTransFailureStatuses.Contains(trans))
                return true;

            return IsClientRefusalStatus(root);
        }

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

            // Status Error/Failed sans Trans_Status terminal = souvent check trop tôt (USSD en cours)
            if (IsSoftAmbiguousFailure(root))
                return true;

            // resultCode "0" sans status explicite = requête acceptée (USSD envoyé), pas encore confirmée
            var resultCode = GetString(root, "resultCode");
            return resultCode == "0";
        }

        public static bool IsDefinitiveFailure(JsonElement root)
        {
            if (HasResultCodeError(root))
                return true;

            return IsTerminalFailureStatus(root);
        }

        /// <summary>
        /// Échec « soft » : Status Error/Failed sans Trans_Status terminal ni resultCodeError.
        /// </summary>
        public static bool IsSoftAmbiguousFailure(JsonElement root)
        {
            if (HasResultCodeError(root) || IsDefinitiveSuccess(root) || IsDefinitiveFailure(root))
                return false;

            // Soft uniquement si Status (pas Trans_Status) est Error/Failed
            var statusOnly = GetString(root, "Status", "status")?.ToLowerInvariant();
            return !string.IsNullOrEmpty(statusOnly) && SoftAmbiguousFailureStatuses.Contains(statusOnly);
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
