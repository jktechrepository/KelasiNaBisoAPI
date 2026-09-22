using System.Diagnostics.CodeAnalysis;
using KelasiNaBiso.Services.MokoAfrika;

namespace KelasiNaBiso.Services.Sync
{
    /// <summary>
    /// Whitelist CASH-like pour sync offline (alignée PaiementGatewayHelper + doc caissier).
    /// </summary>
    public static class SyncCashPaymentMethods
    {
        public const string ErrorMokoNotAllowed = "MOKO_NOT_ALLOWED";
        public const string ErrorMethodNotAllowed = "METHOD_NOT_ALLOWED";

        /// <summary>
        /// Normalise une méthode CASH-like. Retourne false si interdite (Moko) ou hors whitelist.
        /// </summary>
        public static bool TryNormalize(
            string? methodePaiement,
            [NotNullWhen(true)] out string? normalizedMode,
            out string? errorCode,
            out string? message)
        {
            normalizedMode = null;
            errorCode = null;
            message = null;

            if (string.IsNullOrWhiteSpace(methodePaiement))
            {
                errorCode = ErrorMethodNotAllowed;
                message = "methodePaiement est obligatoire (Cash, Espèces, Chèque ou Virement).";
                return false;
            }

            if (PaiementGatewayHelper.EstModeMokoInterdit(methodePaiement))
            {
                errorCode = ErrorMokoNotAllowed;
                message =
                    "Les paiements Mobile Money / Carte (Moko) sont exclus de la file offline. " +
                    "Utilisez POST /api/MokoAfrika/payin/frais-scolaire en ligne.";
                return false;
            }

            var key = NormalizeKey(methodePaiement);
            normalizedMode = key switch
            {
                "cash" or "especes" or "espèces" => "Cash",
                "cheque" or "chèque" => "Chèque",
                "virement" => "Virement",
                _ => null
            };

            if (normalizedMode == null)
            {
                errorCode = ErrorMethodNotAllowed;
                message =
                    $"Méthode '{methodePaiement.Trim()}' non autorisée offline. " +
                    "Autorisées : Cash, Espèces, Chèque, Virement.";
                return false;
            }

            return true;
        }

        private static string NormalizeKey(string value)
        {
            var s = value.Trim().ToLowerInvariant();
            // Retirer accents courants pour matching
            return s
                .Replace('é', 'e')
                .Replace('è', 'e')
                .Replace('ê', 'e')
                .Replace('à', 'a')
                .Replace('ù', 'u')
                .Replace('ç', 'c');
        }
    }
}
