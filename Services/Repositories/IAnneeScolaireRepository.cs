using KelasiNaBiso.Models;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IAnneeScolaireRepository
    {
        Task<IEnumerable<AnneeScolaire>> GetAllAsync();
        Task<AnneeScolaire> GetByIdAsync(int id);
        Task<AnneeScolaire> GetByLibelleAsync(string libelle);
        Task<AnneeScolaire> GetByEcoleAndLibelleAsync(int idEcole, string libelleAnneeScolaire);
        Task<IEnumerable<AnneeScolaire>> GetByEcoleAsync(int idEcole);
        Task<IEnumerable<AnneeScolaire>> GetByDateRangeAsync(DateTime dateDebut, DateTime dateFin);
        Task<IEnumerable<AnneeScolaire>> GetActivesAsync();
        Task<AnneeScolaire> GetAnneeCouranteAsync(int idEcole);
        Task<AnneeScolaire> CreateAsync(AnneeScolaire anneeScolaire);
        Task<AnneeScolaire> UpdateAsync(AnneeScolaire anneeScolaire);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByLibelleAsync(string libelle);
        Task<IEnumerable<Inscription>> GetInscriptionsAsync(int idAnneeScolaire);
        Task<IEnumerable<Note>> GetNotesAsync(int idAnneeScolaire);
        Task<IEnumerable<Notification>> GetNotificationsAsync(int idAnneeScolaire);
        
        // ✅ SOFT DELETE
        Task<bool> ToggleStatutAsync(int id);
    }
}
