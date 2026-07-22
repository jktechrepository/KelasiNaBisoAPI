using KelasiNaBiso.Helpers;
using Microsoft.Extensions.Caching.Memory;

namespace KelasiNaBiso.Services.Reporting
{
    public class ReportImageResolver : IReportImageResolver
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(5);

        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ReportImageResolver> _logger;

        public ReportImageResolver(
            HttpClient httpClient,
            IMemoryCache cache,
            ILogger<ReportImageResolver> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
            _httpClient.Timeout = RequestTimeout;
        }

        public async Task<byte[]?> ResolveAsync(string? url, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            var normalized = url.Trim();

            var base64Bytes = ImageSourceHelper.ToBytes(normalized);
            if (base64Bytes != null)
            {
                _cache.Set(normalized, base64Bytes, CacheDuration);
                return base64Bytes;
            }

            if (_cache.TryGetValue(normalized, out byte[]? cached))
                return cached;

            try
            {
                using var response = await _httpClient.GetAsync(normalized, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Image inaccessible ({StatusCode}) : {Url}", response.StatusCode, normalized);
                    return null;
                }

                var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                if (bytes.Length == 0)
                    return null;

                _cache.Set(normalized, bytes, CacheDuration);
                return bytes;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Echec telechargement image : {Url}", normalized);
                return null;
            }
        }
    }
}
