using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.Pagination;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IEleveRepository
    {
        // ✅ MÉTHODES PAGINÉES (NOUVELLES)
        Task<PagedResult<V_Eleve>> GetAllPagedAsync(PagedRequest request);
        Task<CursorPaginatedResult<V_Eleve>> GetAllCursorPagedAsync(CursorPaginationRequest request);
        Task<PagedResult<Eleve>> GetByClassePagedAsync(int idClasse, PagedRequest request);
        Task<PagedResult<Eleve>> GetByTuteurPagedAsync(int idTuteur, PagedRequest request);
        Task<PagedResult<Eleve>> GetByEcolePagedAsync(int idEcole, PagedRequest request);
        Task<PagedResult<Eleve>> GetByEcoleByNomCompletPagedAsync(int idEcole, string nomComplet, PagedRequest request);
        // Méthodes de base CRUD
        Task<IEnumerable<V_Eleve>> GetAllAsync(); // ⚠️ DEPRECATED: Utiliser GetAllPagedAsync
        Task<Eleve> GetByIdAsync(int id);
        Task<Eleve> GetByReferenceAsync(Guid reference);
        Task<Eleve> CreateAsync(Eleve eleve);
        Task<IEnumerable<Eleve>> CreateBatchAsync(IEnumerable<Eleve> eleves);
        Task<Eleve> UpdateAsync(Eleve eleve);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByReferenceAsync(Guid reference);
        Task<bool> ExistsBySerialNumberAsync(string serialNumber); // ✅ UNICITÉ SERIAL NUMBER

        // Méthodes de recherche par critères (⚠️ DEPRECATED: Utiliser versions paginées)
        Task<IEnumerable<Eleve>> GetByClasseAsync(int idClasse);
        Task<IEnumerable<Eleve>> GetByTuteurAsync(int idTuteur);
        Task<IEnumerable<Eleve>> GetByEcoleAsync(int idEcole);
        Task<IEnumerable<Eleve>> GetByStatutAsync(bool statut);

        // Méthodes pour récupérer les données associées
        Task<IEnumerable<Note>> GetNotesAsync(int idEleve);
        Task<IEnumerable<Inscription>> GetInscriptionsAsync(int idEleve);
        Task<IEnumerable<Paiement>> GetPaiementsAsync(int idEleve);
      //  Task<IEnumerable<Presence>> GetPresencesAsync(int idEleve);
        Task<IEnumerable<Document>> GetDocumentsAsync(int idEleve);
        
        // ✅ SOFT DELETE
        Task<bool> ToggleStatutAsync(int id);

        // ✅ MISE À JOUR DU SERIAL NUMBER
        Task<bool> UpdateSerialNumberByIdAsync(int idEleve, string serialNumber);
        Task<bool> UpdateSerialNumberByMatriculeAsync(string matricule, string serialNumber);
        Task<Eleve> GetByMatriculeAsync(string matricule);
        Task<Eleve> GetBySerialNumberAsync(string serialNumber);
    }
}
