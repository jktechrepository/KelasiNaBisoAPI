using KelasiNaBiso.Models;

namespace KelasiNaBiso.Services.Repositories
{
    public interface ISectionRepository
    {
        Task<IEnumerable<Section>> GetAllAsync();
        Task<Section> GetByIdAsync(int id);
        Task<Section> GetByNomAsync(string nom);
        Task<IEnumerable<Section>> GetByEcoleAsync(int idEcole);
        Task<IEnumerable<Section>> GetByCycleEnseignementAsync(string cycle);
        Task<Section> CreateAsync(Section section);
        Task<Section> UpdateAsync(Section section);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByNomAsync(string nom);
        Task<IEnumerable<Option>> GetOptionsAsync(int idSection);
        Task<IEnumerable<Classe>> GetClassesAsync(int idSection);
        
        // ✅ SOFT DELETE
        Task<bool> ToggleStatutAsync(int id);
    }
}
