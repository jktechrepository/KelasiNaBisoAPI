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
                result.IsSuccess = MokoGatewayResponseParser.IsSuccess(result.Parsed.RootElement);
                result.Reference = MokoGatewayResponseParser.GetString(result.Parsed.RootElement, "Reference", "reference");
                result.Status = MokoGatewayResponseParser.GetString(result.Parsed.RootElement, "Status", "trans_status", "status");
                result.TransactionId = MokoGatewayResponseParser.GetString(result.Parsed.RootElement, "Transaction_id", "transaction_id");
                if (!result.IsSuccess)
                {
                    result.ErrorMessage = MokoGatewayResponseParser.GetString(result.Parsed.RootElement, "Comment", "trans_status_description", "resultCodeErrorDescription")
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

    internal static class MokoGatewayResponseParser
    {
        public static bool IsSuccess(JsonElement root)
        {
            var resultCode = GetString(root, "resultCode");
            if (resultCode == "0")
                return true;

            var resultCodeError = GetString(root, "resultCodeError");
            if (!string.IsNullOrEmpty(resultCodeError) && resultCodeError != "0")
                return false;

            var status = GetString(root, "Status", "trans_status", "status")?.ToLowerInvariant();
            if (string.IsNullOrEmpty(status))
                return false;

            return status is "success" or "successful" or "approved" or "paid";
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
