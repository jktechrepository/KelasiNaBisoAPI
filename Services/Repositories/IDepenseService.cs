using KelasiNaBiso.Models.DTOs.Depense;
using KelasiNaBiso.Models.DTOs.Pagination;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IDepenseService
    {
        Task<PagedResult<DepenseDto>> GetPagedAsync(
            int? idEcole,
            DateTime? dateDebut,
            DateTime? dateFin,
            int? idCategorieDepense,
            string? statut,
            PagedRequest request,
            CancellationToken cancellationToken = default);

        Task<DepenseMoisDto> GetMoisAsync(
            int idEcole,
            int mois,
            int annee,
            string? statut,
            CancellationToken cancellationToken = default);

        Task<DepenseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<DepenseDto> CreateAsync(
            CreateDepenseDto dto,
            int? idUtilisateurCreateur,
            CancellationToken cancellationToken = default);

        Task<DepenseDto> UpdateAsync(
            int id,
            UpdateDepenseDto dto,
            CancellationToken cancellationToken = default);

        Task<DepenseDto> AnnulerAsync(
            int id,
            string? motifAnnulation,
            int? idUtilisateurAnnulation,
            CancellationToken cancellationToken = default);

        Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
