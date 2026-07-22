namespace KelasiNaBiso.Models.Enums
{
    public static class MokoActions
    {
        public const string Debit = "debit";
        public const string Credit = "credit";
        public const string Check = "check";
    }

    public static class MokoTransactionStatuses
    {
        public const string Pending = "pending";
        public const string Success = "success";
        public const string Error = "error";
        public const string Timeout = "timeout";
    }

    public static class MokoPayoutQueueStatuses
    {
        public const string Pending = "pending";
        public const string Processing = "processing";
        public const string Success = "success";
        public const string Failed = "failed";
        public const string Cancelled = "cancelled";
    }

    public static class WalletMouvementTypes
    {
        /// <summary>Crédit en attente après PayIn réussi (délai MOKO).</summary>
        public const string PayInCreditPending = "PAYIN_CREDIT_PENDING";

        /// <summary>Libération du solde en attente vers disponible.</summary>
        public const string PayInReleaseAvailable = "PAYIN_RELEASE_AVAILABLE";

        /// <summary>Débit lors d'un PayOut vers le bénéficiaire école.</summary>
        public const string PayOutDebit = "PAYOUT_DEBIT";

        /// <summary>Recrédit si PayOut échoué (solde remis en disponible).</summary>
        public const string PayOutReversal = "PAYOUT_REVERSAL";

        /// <summary>Ajustement manuel admin.</summary>
        public const string Ajustement = "AJUSTEMENT";
    }

    public static class MomoOperators
    {
        public const string Airtel = "airtel";
        public const string Orange = "orange";
        public const string Mpesa = "mpesa";
        public const string Africell = "africell";
        public const string Card = "card";

        public static readonly string[] All = { Airtel, Orange, Mpesa, Africell, Card };
    }

    public static class EcoleBeneficiaireStatuts
    {
        public const string Actif = "ACTIF";
        public const string Inactif = "INACTIF";
    }
}
