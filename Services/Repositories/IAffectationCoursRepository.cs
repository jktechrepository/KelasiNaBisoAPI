using KelasiNaBiso.Models;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IAffectationCoursRepository
    {
        Task<IEnumerable<AffectationCours>> GetAllAsync();
        Task<AffectationCours?> GetByIdAsync(int id);
        Task<AffectationCours> CreateAsync(AffectationCours affectationCours);
        Task<AffectationCours> UpdateAsync(AffectationCours affectationCours);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        
        // Méthodes spécifiques
        Task<IEnumerable<AffectationCours>> GetByAgentAsync(int idAgent);
        Task<IEnumerable<AffectationCours>> GetByCoursAsync(int idCours);
        Task<IEnumerable<AffectationCours>> GetByAnneeScolaireAsync(int idAnneeScolaire);
        Task<IEnumerable<AffectationCours>> GetByAgentAndAnneeScolaireAsync(int idAgent, int idAnneeScolaire);
        Task<IEnumerable<AffectationCours>> GetByCoursAndAnneeScolaireAsync(int idCours, int idAnneeScolaire);
        Task<IEnumerable<AffectationCours>> GetActivesAsync();
        Task<IEnumerable<AffectationCours>> GetActivesByAgentAsync(int idAgent);
        Task<IEnumerable<AffectationCours>> GetActivesByCoursAsync(int idCours);
        Task<bool> ExistsActiveAffectationAsync(int idAgent, int idCours, int idAnneeScolaire);
        
        // ✅ SOFT DELETE
        Task<bool> ToggleStatutAsync(int id);
    }
}
