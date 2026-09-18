using KelasiNaBiso.Models.DTOs.Depense;

namespace KelasiNaBiso.Services.Repositories
{
    public interface ICategorieDepenseService
    {
        Task<IReadOnlyList<CategorieDepenseDto>> GetByEcoleAsync(
            int idEcole,
            bool includeInactive = false,
            CancellationToken cancellationToken = default);

        Task<CategorieDepenseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<CategorieDepenseDto> CreateAsync(
            CreateCategorieDepenseDto dto,
            CancellationToken cancellationToken = default);

        Task<CategorieDepenseDto> UpdateAsync(
            int id,
            UpdateCategorieDepenseDto dto,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
