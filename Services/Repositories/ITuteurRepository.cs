using KelasiNaBiso.Models;

namespace KelasiNaBiso.Services.Repositories
{
    public interface ITuteurRepository
    {
        Task<IEnumerable<Tuteur>> GetAllAsync();
        Task<Tuteur> GetByIdAsync(int id);
        Task<IEnumerable<Tuteur>> GetByEcoleAsync(int idEcole);
        Task<Tuteur> CreateAsync(Tuteur tuteur);
        Task<Tuteur> UpdateAsync(Tuteur tuteur);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByEmailAsync(string email); // ✅ UNICITÉ EMAIL
        Task<IEnumerable<Eleve>> GetElevesAsync(int idTuteur);
        
        // ✅ SOFT DELETE
        Task<bool> ToggleStatutAsync(int id);
    }
}
