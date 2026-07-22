namespace KelasiNaBiso.Helpers
{
    public static class ImageSourceHelper
    {
        /// <summary>
        /// Valeur utilisable directement dans src="" (data URI, URL http ou base64 brut).
        /// </summary>
        public static string? ToImageSrc(string? value, string defaultMimeType = "image/png")
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var trimmed = value.Trim();

            if (trimmed.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
                || trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return trimmed;
            }

            return $"data:{defaultMimeType};base64,{trimmed}";
        }

        /// <summary>Extrait le payload base64 brut (sans préfixe data:image/...;base64,).</summary>
        public static string? ToRawBase64(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var trimmed = value.Trim();

            if (trimmed.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                var commaIndex = trimmed.IndexOf(',');
                return commaIndex >= 0 ? trimmed[(commaIndex + 1)..] : null;
            }

            if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return trimmed;
        }

        public static byte[]? ToBytes(string? value)
        {
            var raw = ToRawBase64(value);
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            try
            {
                return Convert.FromBase64String(raw);
            }
            catch (FormatException)
            {
                return null;
            }
        }
    }
}
