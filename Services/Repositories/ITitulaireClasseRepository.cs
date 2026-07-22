using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.Pagination;

namespace KelasiNaBiso.Services.Repositories
{
    public interface ITitulaireClasseRepository
    {
        // ✅ MÉTHODES PAGINÉES
        Task<PagedResult<TitulaireClasse>> GetAllPagedAsync(PagedRequest request);
        Task<PagedResult<TitulaireClasse>> GetByAgentPagedAsync(int idAgent, PagedRequest request);
        Task<PagedResult<TitulaireClasse>> GetByClassePagedAsync(int idClasse, PagedRequest request);
        Task<PagedResult<TitulaireClasse>> GetByAnneeScolairePagedAsync(int idAnneeScolaire, PagedRequest request);
        
        // ✅ MÉTHODES DE BASE CRUD
        Task<IEnumerable<TitulaireClasse>> GetAllAsync();
        Task<TitulaireClasse> GetByIdAsync(int id);
        Task<TitulaireClasse> CreateAsync(TitulaireClasse titulaireClasse);
        Task<TitulaireClasse> UpdateAsync(TitulaireClasse titulaireClasse);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ToggleStatutAsync(int id); // Soft delete
        
        // ✅ MÉTHODES SPÉCIFIQUES
        Task<TitulaireClasse?> GetTitulaireActifByClasseAsync(int idClasse, int idAnneeScolaire);
        Task<IEnumerable<TitulaireClasse>> GetByAgentAsync(int idAgent);
        Task<IEnumerable<TitulaireClasse>> GetByClasseAsync(int idClasse);
        Task<IEnumerable<TitulaireClasse>> GetByAnneeScolaireAsync(int idAnneeScolaire);
        
        // ✅ VALIDATION MÉTIER
        Task<bool> HasTitulaireActifAsync(int idClasse, int idAnneeScolaire);
        Task<bool> AgentEstDisponibleAsync(int idAgent, int idAnneeScolaire);
    }
}

