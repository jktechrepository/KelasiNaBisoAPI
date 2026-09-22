using KelasiNaBiso.Models.DTOs;

namespace KelasiNaBiso.Services.Tarif
{
    /// <summary>Montants catalogue / dû effectif pour un couple élève–frais.</summary>
    public class FraisDuAmounts
    {
        public decimal MontantCatalogue { get; set; }
        public decimal MontantDuEffectif { get; set; }
        public decimal MontantReduction { get; set; }
        public int? IdCategorieEleveTarif { get; set; }
        public string? CodeCategorie { get; set; }
        public string? LibelleCategorie { get; set; }
        public string? TypeRegle { get; set; }
        public decimal? ValeurRegle { get; set; }
    }

    /// <summary>
    /// Calcule le dû effectif à partir du catalogue <c>Frais.Montant</c>
    /// et des règles d'exonération de la catégorie active de l'élève.
    /// </summary>
    public interface IFraisDuCalculator
    {
        /// <summary>Applique une règle pure (sans I/O).</summary>
        decimal Apply(decimal montantCatalogue, string? typeRegle, decimal valeur);

        Task<decimal> GetMontantDuEffectifAsync(
            int idEleve,
            int idFrais,
            DateTime? asOfUtc = null,
            CancellationToken cancellationToken = default);

        Task<FraisDuDetailDto?> GetDetailAsync(
            int idEleve,
            int idFrais,
            DateTime? asOfUtc = null,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<FraisDuDetailDto>> GetDetailsForEleveAsync(
            int idEleve,
            int? idAnneeScolaire = null,
            DateTime? asOfUtc = null,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyDictionary<int, decimal>> GetMontantsDuEffectifsByFraisAsync(
            int idEleve,
            IReadOnlyList<int> idFraisList,
            DateTime? asOfUtc = null,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyDictionary<(int IdEleve, int IdFrais), FraisDuAmounts>> GetMontantsDuBatchAsync(
            IReadOnlyList<(int IdEleve, int IdFrais)> pairs,
            DateTime? asOfUtc = null,
            CancellationToken cancellationToken = default);
    }
}
