using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.Pagination;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IPresenceRepository
    {
        // ✅ NOUVELLES MÉTHODES PAGINÉES
        Task<PagedResult<Presence>> GetAllPagedAsync(PagedRequest request);
        Task<CursorPaginatedResult<Presence>> GetAllCursorPagedAsync(CursorPaginationRequest request);
        Task<PagedResult<Presence>> GetByElevePagedAsync(int idEleve, PagedRequest request);
        Task<PagedResult<Presence>> GetByAgentPagedAsync(int idAgent, PagedRequest request);
        Task<PagedResult<Presence>> GetByDatePagedAsync(DateTime date, PagedRequest request);
        Task<PagedResult<Presence>> GetByDateRangePagedAsync(DateTime dateDebut, DateTime dateFin, PagedRequest request);
        Task<PagedResult<Presence>> GetByTypePersonnePagedAsync(string typePersonne, PagedRequest request);
        
        // ⚠️ ANCIENNES MÉTHODES (DEPRECATED - Conserver pour rétrocompatibilité)
        Task<IEnumerable<Presence>> GetAllAsync();
        Task<Presence> GetByIdAsync(int id);
        Task<IEnumerable<Presence>> GetByEleveAsync(int idEleve);
        Task<IEnumerable<Presence>> GetByVacationAsync(int IdHoraire);
        Task<IEnumerable<Presence>> GetByDateAsync(DateTime date);
        Task<IEnumerable<Presence>> GetByEleveAndDateAsync(int idEleve, DateTime date);
        // ✅ POINTAGE AGENT: Nouvelles méthodes pour les agents
        Task<IEnumerable<Presence>> GetByAgentAsync(int idAgent);
        Task<IEnumerable<Presence>> GetByAgentAndDateAsync(int idAgent, DateTime date);
        // ✅ FILTRAGE PAR TYPE: Récupérer les présences par type de personne
        Task<IEnumerable<Presence>> GetByTypePersonneAsync(string typePersonne);
        Task<IEnumerable<Presence>> GetByTypePersonneAndDateAsync(string typePersonne, DateTime date);
        Task<Presence> CreateAsync(Presence presence);
        Task<Presence> UpdateAsync(Presence presence);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ToggleStatutAsync(int id); // ✅ SOFT DELETE
        
        // ✅ BLOCAGE DOUBLE POINTAGE: Vérifier si une personne a déjà pointé aujourd'hui
        Task<bool> HasAlreadyPointedTodayAsync(int? idEleve, int? idAgent, DateTime date);
        Task<Presence?> GetTodayPresenceAsync(int? idEleve, int? idAgent, DateTime date);
    }
}
