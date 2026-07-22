using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.Pagination;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IPaiementRepository
    {
        // ✅ NOUVELLES MÉTHODES PAGINÉES
        Task<PagedResult<Paiement>> GetAllPagedAsync(PagedRequest request);
        Task<CursorPaginatedResult<Paiement>> GetAllCursorPagedAsync(CursorPaginationRequest request);
        Task<PagedResult<Paiement>> GetByElevePagedAsync(int idEleve, PagedRequest request);
        Task<PagedResult<Paiement>> GetByEcolePagedAsync(int idEcole, PagedRequest request);
        Task<PagedResult<Paiement>> GetByDateRangePagedAsync(DateTime dateDebut, DateTime dateFin, PagedRequest request);
        Task<PagedResult<Paiement>> GetByModePaiementPagedAsync(string modePaiement, PagedRequest request);
        Task<PagedResult<Paiement>> GetByStatutPaiementPagedAsync(string statut, PagedRequest request);
        
        // ⚠️ ANCIENNES MÉTHODES (DEPRECATED - Conserver pour rétrocompatibilité)
        Task<IEnumerable<Paiement>> GetAllAsync();
        Task<Paiement> GetByIdAsync(int id);
        Task<Paiement> GetByReferenceAsync(string reference);
        Task<IEnumerable<Paiement>> GetByEleveAsync(int idEleve);
        Task<IEnumerable<Paiement>> GetByUtilisateurAsync(int idUtilisateur);
        Task<IEnumerable<Paiement>> GetByFraisAsync(int idFrais);
        Task<IEnumerable<Paiement>> GetByEcoleAsync(int idEcole);
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
        
        // ✅ SOFT DELETE
        Task<bool> ToggleStatutAsync(int id);

        /// <summary>Envoie notification tuteur + dashboard après confirmation PayIn MOKO.</summary>
        Task NotifierPaiementConfirmeAsync(int idPaiement, CancellationToken cancellationToken = default);
    }
}
