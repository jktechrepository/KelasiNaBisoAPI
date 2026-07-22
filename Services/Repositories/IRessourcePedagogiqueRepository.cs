using KelasiNaBiso.Models;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IRessourcePedagogiqueRepository
    {
        Task<IEnumerable<RessourcePedagogique>> GetAllAsync();
        Task<RessourcePedagogique> GetByIdAsync(int id);
       // Task<RessourcePedagogique> GetByTitreAsync(string titre);
        Task<IEnumerable<RessourcePedagogique>> GetByCoursAsync(int idCours);
     //   Task<IEnumerable<RessourcePedagogique>> GetByTypeAsync(string type);
        Task<IEnumerable<RessourcePedagogique>> GetByDateCreationAsync(DateTime date);
        Task<IEnumerable<RessourcePedagogique>> GetByDateRangeAsync(DateTime dateDebut, DateTime dateFin);
        Task<RessourcePedagogique> CreateAsync(RessourcePedagogique ressource);
        Task<RessourcePedagogique> UpdateAsync(RessourcePedagogique ressource);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
      //  Task<bool> ExistsByTitreAsync(string titre);
        
        // ✅ SOFT DELETE
        Task<bool> ToggleStatutAsync(int id);
    }
}
