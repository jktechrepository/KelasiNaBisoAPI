using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.DTOs.Pagination;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IPresenceRepository
    {
        Task<ElevesAnneeScopedResult<PagedResult<Presence>>> GetAllPagedAsync(
            int idEcole, PagedRequest request, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<CursorPaginatedResult<Presence>>> GetAllCursorPagedAsync(
            int idEcole, CursorPaginationRequest request, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<PagedResult<Presence>>> GetByElevePagedAsync(
            int idEleve, PagedRequest request, int? idAnneeScolaire = null);
        Task<PagedResult<Presence>> GetByAgentPagedAsync(int idAgent, PagedRequest request);
        Task<PagedResult<Presence>> GetByDatePagedAsync(DateTime date, PagedRequest request);
        Task<ElevesAnneeScopedResult<PagedResult<Presence>>> GetByDateRangePagedAsync(
            int idEcole, DateTime dateDebut, DateTime dateFin, PagedRequest request, int? idAnneeScolaire = null);
        Task<PagedResult<Presence>> GetByTypePersonnePagedAsync(string typePersonne, PagedRequest request);
        
        Task<ElevesAnneeScopedResult<IEnumerable<Presence>>> GetAllAsync(int idEcole, int? idAnneeScolaire = null);
        Task<Presence> GetByIdAsync(int id);
        Task<ElevesAnneeScopedResult<IEnumerable<Presence>>> GetByEleveAsync(int idEleve, int? idAnneeScolaire = null);
        Task<IEnumerable<Presence>> GetByVacationAsync(int IdHoraire);
        Task<IEnumerable<Presence>> GetByDateAsync(DateTime date);
        Task<IEnumerable<Presence>> GetByEleveAndDateAsync(int idEleve, DateTime date);
        Task<IEnumerable<Presence>> GetByAgentAsync(int idAgent);
        Task<IEnumerable<Presence>> GetByAgentAndDateAsync(int idAgent, DateTime date);
        Task<IEnumerable<Presence>> GetByTypePersonneAsync(string typePersonne);
        Task<IEnumerable<Presence>> GetByTypePersonneAndDateAsync(string typePersonne, DateTime date);
        Task<Presence> CreateAsync(Presence presence);
        Task<Presence> UpdateAsync(Presence presence);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ToggleStatutAsync(int id);
        Task<bool> HasAlreadyPointedTodayAsync(int? idEleve, int? idAgent, DateTime date);
        Task<Presence?> GetTodayPresenceAsync(int? idEleve, int? idAgent, DateTime date);
    }
}
