namespace KelasiNaBiso.Models.DTOs.MokoAfrika
{
    public class EcoleInfoPaiementMobileDto
    {
        public int IdEcoleInfoPaiementMobile { get; set; }
        public int IdEcole { get; set; }
        public bool MobileMoneyActif { get; set; }
        public bool CarteActif { get; set; }
        public string Devise { get; set; } = "CDF";
        public int DelaiReglementMinutes { get; set; }
        public bool PayoutAutomatique { get; set; }
        public bool Statut { get; set; }
        public EcoleWalletDto? Wallet { get; set; }
        public List<EcoleBeneficiaireMomoDto> Beneficiaires { get; set; } = new();
    }

    public class CreateEcoleInfoPaiementMobileDto
    {
        public bool MobileMoneyActif { get; set; }
        public bool CarteActif { get; set; }
        public string Devise { get; set; } = "CDF";
        public int DelaiReglementMinutes { get; set; } = 3;
        public bool PayoutAutomatique { get; set; } = true;
    }

    public class UpdateEcoleInfoPaiementMobileDto
    {
        public bool? MobileMoneyActif { get; set; }
        public bool? CarteActif { get; set; }
        public string? Devise { get; set; }
        public int? DelaiReglementMinutes { get; set; }
        public bool? PayoutAutomatique { get; set; }
        public bool? Statut { get; set; }
    }

    public class EcoleBeneficiaireMomoDto
    {
        public int IdEcoleBeneficiaireMomo { get; set; }
        public int IdEcole { get; set; }
        public string Methode { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string? NomTitulaire { get; set; }
        public bool EstPrincipal { get; set; }
        public string Statut { get; set; } = string.Empty;
    }

    public class CreateEcoleBeneficiaireMomoDto
    {
        public string Methode { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string? NomTitulaire { get; set; }
        public bool EstPrincipal { get; set; }
    }

    public class UpdateEcoleBeneficiaireMomoDto
    {
        public string? Methode { get; set; }
        public string? Numero { get; set; }
        public string? NomTitulaire { get; set; }
        public bool? EstPrincipal { get; set; }
        public string? Statut { get; set; }
    }

    public class EcoleWalletDto
    {
        public int IdEcoleWallet { get; set; }
        public int IdEcole { get; set; }
        public string Devise { get; set; } = "CDF";
        public decimal SoldeEnAttente { get; set; }
        public decimal SoldeDisponible { get; set; }
        public decimal TotalRecu { get; set; }
        public decimal TotalReverse { get; set; }
    }

    public class EcoleWalletMouvementDto
    {
        public int IdEcoleWalletMouvement { get; set; }
        public string TypeMouvement { get; set; } = string.Empty;
        public decimal Montant { get; set; }
        public decimal SoldeEnAttenteApres { get; set; }
        public decimal SoldeDisponibleApres { get; set; }
        public string? Reference { get; set; }
        public string? Commentaire { get; set; }
        public DateTime DateCreation { get; set; }
        public int? IdPaiement { get; set; }
        public int? IdTransactionMoko { get; set; }
    }

    public class MokoFeeEstimateDto
    {
        public decimal MontantNet { get; set; }
        public decimal FraisCollecte { get; set; }
        public decimal FraisDecaissement { get; set; }
        public decimal MontantCollecte { get; set; }
        public string Devise { get; set; } = "CDF";
        public string Method { get; set; } = string.Empty;
    }

    public class TransactionMokoDto
    {
        public int IdTransactionMoko { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string? ParentReference { get; set; }
        public int? IdPaiement { get; set; }
        public int IdEcole { get; set; }
        public string Action { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal? AmountNet { get; set; }
        public string Devise { get; set; } = "CDF";
        public string? Method { get; set; }
        public string Status { get; set; } = string.Empty;
        /// <summary>Status gateway brut ou message d'erreur (utile pour l'UI / debug).</summary>
        public string? StatusDescription { get; set; }
        /// <summary>true si success / error / timeout — le front peut arrêter le poll.</summary>
        public bool IsDefinitive { get; set; }
        public string? GatewayTransactionId { get; set; }
        public DateTime DateCreation { get; set; }
    }

    public class PayInFraisScolaireRequestDto
    {
        public int IdEleve { get; set; }
        public int IdFrais { get; set; }
        public decimal? MontantNet { get; set; }
        public string Method { get; set; } = "airtel";
        public string TelephonePayeur { get; set; } = string.Empty;
        /// <summary>Devise attendue par le client (principale école ou devise gateway MM).</summary>
        public string? Devise { get; set; }
        /// <summary>Alias front de <see cref="Devise"/> (ex. Flutter <c>currency</c>).</summary>
        public string? Currency { get; set; }
        public string? Commentaire { get; set; }
    }

    public class PayInFraisScolaireResultDto
    {
        public int IdPaiement { get; set; }
        public int IdEcole { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string StatutPaiement { get; set; } = string.Empty;
        public string StatutGateway { get; set; } = string.Empty;
        public decimal MontantNet { get; set; }
        public decimal MontantCollecte { get; set; }
        public string? CodeDevisePrincipale { get; set; }
        public string? CodeDevisePaiement { get; set; }
        public decimal? TauxVersDevisePrincipale { get; set; }
        public decimal? MontantPayeDevisePrincipale { get; set; }
        public MokoFeeEstimateDto Frais { get; set; } = new();
        public string? Message { get; set; }
        public string? GatewayTransactionId { get; set; }
        public bool RequiresUssdConfirmation { get; set; }
    }

    public class MokoCallbackResultDto
    {
        public bool Accepted { get; set; }
        public string? Reference { get; set; }
        public string? Message { get; set; }
    }

    public class PayOutRetryResultDto
    {
        public string PayInReference { get; set; } = string.Empty;
        public string? PayOutReference { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Message { get; set; }
    }

    /// <summary>Vue d'ensemble paiement mobile école (config + stats wallet).</summary>
    public class EcolePaiementMobileOverviewDto
    {
        public int IdEcole { get; set; }
        public bool EstConfigure { get; set; }
        public EcoleInfoPaiementMobileDto? Configuration { get; set; }
        public EcoleWalletStatsDto Stats { get; set; } = new();
    }

    public class EcoleWalletStatsDto
    {
        public decimal SoldeEnAttente { get; set; }
        public decimal SoldeDisponible { get; set; }
        public decimal TotalRecu { get; set; }
        public decimal TotalReverse { get; set; }
        public string Devise { get; set; } = "CDF";
        public int PayInsReussis { get; set; }
        public int PayInsEnAttente { get; set; }
        public int PayInsEchoues { get; set; }
        public int PayOutsReussis { get; set; }
        public int PayOutsEnAttente { get; set; }
        public int PayOutsEchoues { get; set; }
    }

    public class TransactionMokoListItemDto : TransactionMokoDto
    {
        public string? StatusDescription { get; set; }
        public string? CustomerPhone { get; set; }
        public string? NomEleve { get; set; }
        public string? LibelleFrais { get; set; }
        public string? StatutPaiement { get; set; }
    }

    public class FilePayoutMokoDto
    {
        public int IdFilePayoutMoko { get; set; }
        public string PayInReference { get; set; } = string.Empty;
        public string? PayOutReference { get; set; }
        public int IdEcole { get; set; }
        public int? IdPaiement { get; set; }
        public decimal MontantNet { get; set; }
        public string Devise { get; set; } = "CDF";
        public string? Methode { get; set; }
        public string? NumeroBeneficiaire { get; set; }
        public DateTime ScheduledAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public int RetryCount { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateTraitement { get; set; }
    }
}
