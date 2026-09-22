using KelasiNaBiso.Models.DTOs;

namespace KelasiNaBiso.Services.Tarif
{
    public interface IEleveTarifService
    {
        Task<IReadOnlyList<CategorieEleveTarifDto>> GetCategoriesAsync(
            int idEcole, bool includeInactive = false, CancellationToken cancellationToken = default);

        Task<CategorieEleveTarifDto?> GetCategorieByIdAsync(
            int id, CancellationToken cancellationToken = default);

        Task<CategorieEleveTarifDto> CreateCategorieAsync(
            int idEcole, CreateCategorieEleveTarifDto dto, CancellationToken cancellationToken = default);

        Task<CategorieEleveTarifDto> UpdateCategorieAsync(
            int id, UpdateCategorieEleveTarifDto dto, CancellationToken cancellationToken = default);

        Task SoftDeleteCategorieAsync(int id, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<AffectationEleveCategorieTarifDto>> GetAffectationsEleveAsync(
            int idEleve, int? idAnneeScolaire = null, CancellationToken cancellationToken = default);

        Task<AffectationEleveCategorieTarifDto?> GetAffectationByIdAsync(
            int idAffectation, CancellationToken cancellationToken = default);

        Task<AffectationEleveCategorieTarifDto?> GetAffectationActiveAsync(
            int idEleve, int idAnneeScolaire, DateTime? asOfUtc = null, CancellationToken cancellationToken = default);

        Task<AffectationEleveCategorieTarifDto> AffecterAsync(
            CreateAffectationEleveCategorieTarifDto dto,
            int? idAuteur,
            CancellationToken cancellationToken = default);

        Task<AffectationEleveCategorieTarifDto> CloturerAffectationAsync(
            int idAffectation,
            DateTime? dateFinUtc = null,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<RegleExonerationFraisDto>> GetReglesAsync(
            int idEcole,
            int? idAnneeScolaire = null,
            int? idCategorie = null,
            bool includeInactive = false,
            CancellationToken cancellationToken = default);

        Task<RegleExonerationFraisDto?> GetRegleByIdAsync(
            int id, CancellationToken cancellationToken = default);

        Task<RegleExonerationFraisDto> UpsertRegleAsync(
            int idEcole,
            UpsertRegleExonerationFraisDto dto,
            int? idAuteur,
            CancellationToken cancellationToken = default);

        Task SoftDeleteRegleAsync(int id, CancellationToken cancellationToken = default);
    }
}
