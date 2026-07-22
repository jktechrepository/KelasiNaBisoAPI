using KelasiNaBiso.Models;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IDirectionRepository
    {
        Task<IEnumerable<Direction>> GetAllAsync();
        Task<Direction> GetByIdAsync(int id);
        Task<Direction> GetByNomAsync(string nom);
        Task<IEnumerable<Direction>> GetByEcoleAsync(int idEcole);
        Task<Direction> CreateAsync(Direction direction);
        Task<Direction> UpdateAsync(Direction direction);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByNomAsync(string nom);
        Task<bool> ExistsByNomAndEcoleAsync(string nom, int idEcole);
        Task<IEnumerable<Classe>> GetClassesAsync(int idDirection);
        
        // ✅ SOFT DELETE
        Task<bool> ToggleStatutAsync(int id);
    }
}
