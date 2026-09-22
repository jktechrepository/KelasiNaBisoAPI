using System.Globalization;

namespace KelasiNaBiso.Services.Sync
{
    /// <summary>Helpers cursor / watermark pour le pull sync.</summary>
    public static class SyncPullHelpers
    {
        /// <summary>
        /// Extrait une date UTC depuis un watermark <c>{ISO}_{ticks}</c> ou une date ISO seule.
        /// </summary>
        public static bool TryParseSince(string? since, out DateTime sinceUtc)
        {
            sinceUtc = default;
            if (string.IsNullOrWhiteSpace(since))
                return false;

            var raw = since.Trim();
            var underscore = raw.IndexOf('_');
            if (underscore > 0)
                raw = raw[..underscore];

            if (DateTime.TryParse(
                    raw,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out var parsed))
            {
                sinceUtc = parsed.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(parsed, DateTimeKind.Utc)
                    : parsed.ToUniversalTime();
                return true;
            }

            return false;
        }

        public static bool TryParseEleveCursor(string? cursor, out int idEleve)
            => int.TryParse(cursor, NumberStyles.Integer, CultureInfo.InvariantCulture, out idEleve)
               && idEleve > 0;

        public static string FormatEleveCursor(int idEleve)
            => idEleve.ToString(CultureInfo.InvariantCulture);

        public static bool TryParseFraisDuCursor(string? cursor, out int idEleve, out int idFrais)
        {
            idEleve = 0;
            idFrais = 0;
            if (string.IsNullOrWhiteSpace(cursor))
                return false;

            var parts = cursor.Split(':', 2);
            if (parts.Length != 2)
                return false;

            return int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out idEleve)
                   && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out idFrais)
                   && idEleve > 0
                   && idFrais > 0;
        }

        public static string FormatFraisDuCursor(int idEleve, int idFrais)
            => $"{idEleve.ToString(CultureInfo.InvariantCulture)}:{idFrais.ToString(CultureInfo.InvariantCulture)}";

        public static bool IsConfirmedPaymentStatus(string? statutPaiement)
            => statutPaiement != null
               && (statutPaiement == "Confirmé"
                   || statutPaiement == "Confirme"
                   || statutPaiement.StartsWith("Confirm", StringComparison.Ordinal));

        public static bool IsConfirmedInscriptionStatus(string? statutInscription)
            => statutInscription != null
               && (statutInscription == InscriptionActiveRules.StatutConfirme
                   || statutInscription == "Confirme"
                   || statutInscription.StartsWith("Confirm", StringComparison.Ordinal));
    }
}
