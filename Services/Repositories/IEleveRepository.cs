using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.DTOs.Pagination;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IEleveRepository
    {
        Task<ElevesAnneeScopedResult<PagedResult<V_Eleve>>> GetAllPagedAsync(
            int idEcole, PagedRequest request, int? idAnneeScolaire = null, int? idClasse = null, int? idDirection = null);
        Task<ElevesAnneeScopedResult<CursorPaginatedResult<V_Eleve>>> GetAllCursorPagedAsync(
            int idEcole, CursorPaginationRequest request, int? idAnneeScolaire = null, int? idClasse = null, int? idDirection = null);
        Task<ElevesAnneeScopedResult<PagedResult<Eleve>>> GetByClassePagedAsync(int idClasse, PagedRequest request, int? idAnneeScolaire = null);
        Task<PagedResult<Eleve>> GetByTuteurPagedAsync(int idTuteur, PagedRequest request);
        Task<ElevesAnneeScopedResult<PagedResult<EleveParEcoleListItemDto>>> GetByEcolePagedAsync(int idEcole, PagedRequest request, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<PagedResult<EleveParEcoleListItemDto>>> GetByEcoleByNomCompletPagedAsync(int idEcole, string nomComplet, PagedRequest request, int? idAnneeScolaire = null);

        Task<ElevesAnneeScopedResult<IReadOnlyList<EleveParEcoleListItemDto>>> GetAllAsync(int idEcole, int? idAnneeScolaire = null);
        Task<Eleve> GetByIdAsync(int id);
        Task<Eleve> GetByReferenceAsync(Guid reference);
        Task<Eleve> CreateAsync(Eleve eleve);
        Task<IEnumerable<Eleve>> CreateBatchAsync(IEnumerable<Eleve> eleves);
        Task<Eleve> UpdateAsync(Eleve eleve);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByReferenceAsync(Guid reference);
        Task<bool> ExistsBySerialNumberAsync(string serialNumber);

        Task<ElevesAnneeScopedResult<IReadOnlyList<Eleve>>> GetByClasseAsync(int idClasse, int? idAnneeScolaire = null);
        Task<IReadOnlyList<EleveParEcoleListItemDto>> GetByTuteurAsync(
            int idTuteur,
            string? libelleAnneeScolaire = null,
            int? idEleve = null);
        Task<ElevesAnneeScopedResult<IReadOnlyList<EleveParEcoleListItemDto>>> GetByEcoleAsync(int idEcole, int? idAnneeScolaire = null);
        Task<IEnumerable<Eleve>> GetByStatutAsync(bool statut);

        Task<IEnumerable<Note>> GetNotesAsync(int idEleve);
        Task<IEnumerable<Inscription>> GetInscriptionsAsync(int idEleve);
        Task<IEnumerable<PaiementElevePagedItemDto>> GetPaiementsAsync(
            int idEleve,
            string? libelleAnneeScolaire = null);
        Task<IEnumerable<Document>> GetDocumentsAsync(int idEleve);

        Task<bool> ToggleStatutAsync(int id);

        Task<bool> UpdateSerialNumberByIdAsync(int idEleve, string serialNumber);
        Task<bool> UpdateSerialNumberByMatriculeAsync(string matricule, string serialNumber);
        Task<Eleve> GetByMatriculeAsync(string matricule);
        Task<Eleve> GetBySerialNumberAsync(string serialNumber);
        Task<EleveSerialLookupDto?> GetBySerialNumberLookupAsync(string serialNumber);

        Task<EleveReinscriptionPrefillDto?> GetReinscriptionPrefillByMatriculeAsync(
            int idEcole,
            string matricule,
            int? idAnneeScolaire = null,
            int? idClasse = null);

        Task<ElevesAnneeScopedResult<PagedResult<EleveReinscriptionPrefillDto>>> SearchReinscriptionPrefillByNomCompletPagedAsync(
            int idEcole,
            string nomComplet,
            PagedRequest request,
            int? idAnneeScolaire = null,
            int? idClasse = null);
    }
}
