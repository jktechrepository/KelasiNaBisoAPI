using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.DTOs.Pagination;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IPaiementRepository
    {
        Task<ElevesAnneeScopedResult<PagedResult<Paiement>>> GetAllPagedAsync(
            int idEcole, PagedRequest request, int? idAnneeScolaire = null, int? idUtilisateur = null);
        Task<ElevesAnneeScopedResult<CursorPaginatedResult<Paiement>>> GetAllCursorPagedAsync(
            int idEcole, CursorPaginationRequest request, int? idAnneeScolaire = null, int? idUtilisateur = null);
        Task<ElevesAnneeScopedResult<PagedResult<Paiement>>> GetByElevePagedAsync(
            int idEleve, PagedRequest request, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<PagedResult<Paiement>>> GetByEcolePagedAsync(
            int idEcole, PagedRequest request, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<PagedResult<Paiement>>> GetByDateRangePagedAsync(
            int idEcole, DateTime dateDebut, DateTime dateFin, PagedRequest request, int? idAnneeScolaire = null);
        Task<PagedResult<Paiement>> GetByModePaiementPagedAsync(string modePaiement, PagedRequest request);
        Task<PagedResult<Paiement>> GetByStatutPaiementPagedAsync(string statut, PagedRequest request);
        
        Task<ElevesAnneeScopedResult<IEnumerable<Paiement>>> GetAllAsync(int idEcole, int? idAnneeScolaire = null);
        Task<Paiement> GetByIdAsync(int id);
        Task<Paiement> GetByReferenceAsync(string reference);
        Task<ElevesAnneeScopedResult<IEnumerable<Paiement>>> GetByEleveAsync(int idEleve, int? idAnneeScolaire = null);
        Task<IEnumerable<Paiement>> GetByUtilisateurAsync(int idUtilisateur);
        Task<IEnumerable<Paiement>> GetByFraisAsync(int idFrais);
        Task<ElevesAnneeScopedResult<IEnumerable<Paiement>>> GetByEcoleAsync(int idEcole, int? idAnneeScolaire = null);
        Task<IEnumerable<Paiement>> GetByModePaiementAsync(string modePaiement);
        Task<IEnumerable<Paiement>> GetByStatutAsync(string statut);
        Task<IEnumerable<Paiement>> GetByDatePaiementAsync(DateTime date);
        Task<IEnumerable<Paiement>> GetByDateRangeAsync(DateTime dateDebut, DateTime dateFin);
        Task<Paiement> CreateAsync(Paiement paiement);
        Task<IEnumerable<Paiement>> CreateBatchAsync(IEnumerable<Paiement> paiements);
        Task<Paiement> UpdateAsync(Paiement paiement);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByReferenceAsync(string reference);
        Task<bool> ToggleStatutAsync(int id);
        Task NotifierPaiementConfirmeAsync(int idPaiement, CancellationToken cancellationToken = default);
    }
}
