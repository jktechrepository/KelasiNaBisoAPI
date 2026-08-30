using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.DTOs.Pagination;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IInscriptionRepository
    {
        string GenerateMatriculeEleve(string nomEcole, CreateInscriptionDto inscriptionDto);
        Task<IEnumerable<Inscription>> GetAllAsync();
        Task<Inscription> GetByIdAsync(int id);
        Task<IEnumerable<Inscription>> GetByEleveAsync(int idEleve);
        Task<ElevesAnneeScopedResult<IEnumerable<Inscription>>> GetByEcoleAsync(int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<Inscription>>> GetByClasseAsync(int idClasse, int? idAnneeScolaire = null);
        Task<IEnumerable<Inscription>> GetByAnneeScolaireAsync(int idAnneeScolaire);
        Task<IEnumerable<Inscription>> GetByStatutAsync(bool statut);
        [Obsolete("Utiliser CreateInscriptionAsync. La SP sp_CreateInscription reposait sur Eleves.IdClasse / Tuteurs.IdEcole.")]
        Task<InscriptionResult> CreateInscriptionWithStoredProcedureAsync(CreateInscriptionDto inscriptionDto);
        Task<InscriptionResult> CreateInscriptionAsync(CreateInscriptionDto inscriptionDto);
        Task<Inscription> UpdateAsync(Inscription inscription);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        
        // ✅ SOFT DELETE
        Task<bool> ToggleStatutAsync(int id);
        
        // ✅ CASCADE SOFT DELETE : Désactiver toutes les inscriptions d'un élève
        Task<int> DesactiverInscriptionsParEleveAsync(int idEleve);

        // ✅ PAGINATION
        Task<PagedResult<Inscription>> GetAllPagedAsync(PagedRequest request);
        Task<PagedResult<Inscription>> GetByElevePagedAsync(int idEleve, PagedRequest request);
        Task<ElevesAnneeScopedResult<PagedResult<Inscription>>> GetByEcolePagedAsync(
            int idEcole, PagedRequest request, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<PagedResult<Inscription>>> GetByClassePagedAsync(
            int idClasse, PagedRequest request, int? idAnneeScolaire = null);
        Task<PagedResult<Inscription>> GetByStatutPagedAsync(bool statut, PagedRequest request);
    }
}
