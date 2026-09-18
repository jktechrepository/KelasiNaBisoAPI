using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace KelasiNaBiso.Services
{
    /// <summary>
    /// Normalisation et alias des libellés de période → code T1/T2/T3.
    /// </summary>
    public static class PeriodeCotationAliases
    {
        public static readonly IReadOnlyDictionary<string, string[]> AliasesByCode =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["T1"] = new[]
                {
                    "T1", "Trimestre 1", "1er Trimestre", "1ère Trimestre", "Premier Trimestre",
                    "1 Trimestre", "Trim 1", "1er Trim", "1ère Trim", "1e Trimestre"
                },
                ["T2"] = new[]
                {
                    "T2", "Trimestre 2", "2ème Trimestre", "2eme Trimestre", "Deuxième Trimestre",
                    "Deuxieme Trimestre", "2 Trimestre", "Trim 2", "2e Trimestre", "2eme Trim"
                },
                ["T3"] = new[]
                {
                    "T3", "Trimestre 3", "3ème Trimestre", "3eme Trimestre", "Troisième Trimestre",
                    "Troisieme Trimestre", "3 Trimestre", "Trim 3", "3e Trimestre", "3eme Trim"
                }
            };

        public static string NormalizeKey(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var trimmed = value.Trim().ToLowerInvariant();
            var normalized = trimmed.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalized.Length);
            foreach (var c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                    continue;
                sb.Append(c);
            }

            var ascii = sb.ToString().Normalize(NormalizationForm.FormC);
            ascii = ascii.Replace('è', 'e').Replace('é', 'e').Replace('ê', 'e');
            ascii = Regex.Replace(ascii, @"\s+", " ");
            return ascii.Trim();
        }

        /// <summary>Retourne le code (T1/T2/T3) si l'entrée correspond à un alias connu.</summary>
        public static string? ResolveCode(string? periodeOuCode)
        {
            var key = NormalizeKey(periodeOuCode);
            if (string.IsNullOrEmpty(key))
                return null;

            foreach (var (code, aliases) in AliasesByCode)
            {
                if (NormalizeKey(code) == key)
                    return code;

                foreach (var alias in aliases)
                {
                    if (NormalizeKey(alias) == key)
                        return code;
                }
            }

            return null;
        }

        public static IReadOnlyList<string> GetAliases(string code)
        {
            if (AliasesByCode.TryGetValue(code, out var aliases))
                return aliases;
            return Array.Empty<string>();
        }
    }
}
