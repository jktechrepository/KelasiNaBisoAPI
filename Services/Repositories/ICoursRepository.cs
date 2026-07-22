using KelasiNaBiso.Models;

namespace KelasiNaBiso.Services.Repositories
{
    public interface ICoursRepository
    {
        Task<IEnumerable<Cours>> GetAllAsync();
        Task<IEnumerable<Cours>> GetAllByEcoleAsync(int idEcole);
        Task<Cours> GetByIdAsync(int id);
        Task<IEnumerable<Cours>> GetByClasseAsync(int idClasse);
        // Task<IEnumerable<Cours>> GetByProfesseurAsync(int idProfesseur); // Supprimé car nous utilisons maintenant AffectationCours
        Task<Cours> CreateAsync(Cours cours);
        Task<Cours> UpdateAsync(Cours cours);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<Note>> GetNotesAsync(int idCours);
        Task<IEnumerable<Evaluation>> GetEvaluationsAsync(int idCours);
        Task<IEnumerable<RessourcePedagogique>> GetRessourcesAsync(int idCours);
        
        // ✅ SOFT DELETE
        Task<bool> ToggleStatutAsync(int id);
    }
}
