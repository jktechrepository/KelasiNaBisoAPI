using KelasiNaBiso.Models;

namespace KelasiNaBiso.Services
{
    /// <summary>
    /// Règles métier pour une inscription active (source de vérité parcours scolaire).
    /// </summary>
    public static class InscriptionActiveRules
    {
        public const string StatutConfirme = "Confirmé";

        public static bool IsActiveConfirmed(Inscription? inscription)
        {
            if (inscription == null || inscription.Statut != true)
                return false;

            if (string.IsNullOrWhiteSpace(inscription.StatutInscription))
                return false;

            return IsConfirmedStatus(inscription.StatutInscription);
        }

        /// <summary>
        /// Création / mise à jour : en attente et valeurs non confirmées → Confirmé ; Annulé conservé.
        /// </summary>
        public static string NormalizeStatutInscription(string? statutInscription)
        {
            if (string.IsNullOrWhiteSpace(statutInscription))
                return StatutConfirme;

            var trimmed = statutInscription.Trim();

            if (IsAnnuleStatus(trimmed))
                return trimmed;

            if (IsPendingStatus(trimmed) || !IsConfirmedStatus(trimmed))
                return StatutConfirme;

            return trimmed;
        }

        /// <summary>Alias conservé pour les appels existants (Excel, tests).</summary>
        public static string NormalizeStatutInscriptionForCreate(string? statutInscription) =>
            NormalizeStatutInscription(statutInscription);

        private static bool IsConfirmedStatus(string statut) =>
            statut.Equals(StatutConfirme, StringComparison.OrdinalIgnoreCase)
            || statut.Equals("Confirme", StringComparison.OrdinalIgnoreCase)
            || statut.StartsWith("Confirm", StringComparison.OrdinalIgnoreCase);

        private static bool IsAnnuleStatus(string statut) =>
            statut.Equals("Annulé", StringComparison.OrdinalIgnoreCase)
            || statut.Equals("Annule", StringComparison.OrdinalIgnoreCase)
            || statut.StartsWith("Annul", StringComparison.OrdinalIgnoreCase);

        private static bool IsPendingStatus(string statut)
        {
            if (statut.StartsWith("En attente", StringComparison.OrdinalIgnoreCase))
                return true;

            var normalized = statut
                .Replace(" ", "_", StringComparison.Ordinal)
                .ToUpperInvariant();

            return normalized is "EN_ATTENTE" or "ENATTENTE"
                || normalized.StartsWith("EN_ATTENTE", StringComparison.Ordinal);
        }
    }
}
