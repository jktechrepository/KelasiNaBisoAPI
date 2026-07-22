using KelasiNaBiso.Models;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IOptionRepository
    {
        Task<IEnumerable<Option>> GetAllAsync();
        Task<Option> GetByIdAsync(int id);
        Task<Option> GetByNomAsync(string nom);
        Task<IEnumerable<Option>> GetBySectionAsync(int idSection);
       // Task<IEnumerable<Option>> GetByCycleEnseignementAsync(string cycle);
        Task<Option> CreateAsync(Option option);
        Task<Option> UpdateAsync(Option option);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByNomAsync(string nom);
        Task<IEnumerable<Classe>> GetClassesAsync(int idOption);
        
        // ✅ SOFT DELETE
        Task<bool> ToggleStatutAsync(int id);
    }
}
