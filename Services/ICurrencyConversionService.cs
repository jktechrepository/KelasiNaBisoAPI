namespace KelasiNaBiso.Services
{
    public interface ICurrencyConversionService
    {
        Task<ConversionResult> ConvertToPrincipalAsync(
            int idEcole,
            string codeDeviseSource,
            decimal montant,
            DateTime dateReference,
            CancellationToken ct = default);

        Task<ConversionResult> ConvertAsync(
            int idEcole,
            string codeDeviseSource,
            string codeDeviseCible,
            decimal montant,
            DateTime dateReference,
            CancellationToken ct = default);
    }

    public record ConversionResult(
        int IdEcole,
        string CodeDeviseSource,
        string CodeDevisePrincipale,
        string CodeDeviseCible,
        decimal Taux,
        decimal MontantSource,
        decimal MontantConverti,
        DateTime DateReference,
        bool Success,
        string? ErrorMessage = null);
}
