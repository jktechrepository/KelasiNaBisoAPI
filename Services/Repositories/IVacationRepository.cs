using KelasiNaBiso.Models;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IVacationRepository
    {
        Task<IEnumerable<Vacation>> GetAllAsync();
        Task<Vacation> GetByIdAsync(int id);
        Task<IEnumerable<Vacation>> GetByEcoleAsync(int idClasse);
        Task<IEnumerable<Vacation>> GetByCoursAsync(int idCours);
        Task<IEnumerable<Vacation>> GetByJourAsync(int jour);
        Task<IEnumerable<Vacation>> GetByHeureDebutAsync(TimeSpan heureDebut);
        Task<IEnumerable<Vacation>> GetByHeureFinAsync(TimeSpan heureFin);
        Task<IEnumerable<Vacation>> GetByDateRangeAsync(DateTime dateDebut, DateTime dateFin);
        Task<Vacation> CreateAsync(Vacation Vacation);
        Task<Vacation> UpdateAsync(Vacation Vacation);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        
        // ✅ SOFT DELETE
        Task<bool> ToggleStatutAsync(int id);
    }
}
