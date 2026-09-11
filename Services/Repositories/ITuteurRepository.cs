using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;

namespace KelasiNaBiso.Services.Repositories
{
    public interface ITuteurRepository
    {
        Task<IEnumerable<Tuteur>> GetAllAsync();
        Task<Tuteur> GetByIdAsync(int id);
        Task<ElevesAnneeScopedResult<IEnumerable<Tuteur>>> GetByEcoleAsync(int idEcole, int? idAnneeScolaire = null);
        Task<Tuteur> CreateAsync(Tuteur tuteur);
        Task<Tuteur> UpdateAsync(Tuteur tuteur);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByEmailAsync(string email); // ✅ UNICITÉ EMAIL
        Task<IEnumerable<TuteurEleveListItemDto>> GetElevesAsync(
            int idTuteur,
            string? searchTerm = null,
            string? libelleAnneeScolaire = null);
        
        // ✅ SOFT DELETE
        Task<bool> ToggleStatutAsync(int id);
    }
}
