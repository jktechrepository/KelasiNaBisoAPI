using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.DTOs.DevoirADomicile;
using KelasiNaBiso.Models.DTOs.Pagination;

namespace KelasiNaBiso.Services.Repositories
{
    /// <summary>
    /// Repository pour la gestion des devoirs à domicile
    /// </summary>
    public interface IDevoirADomicileRepository
    {
        // CRUD de base
        Task<DevoirADomicile> GetByIdAsync(int id);
        Task<DevoirADomicile> CreateAsync(DevoirADomicile devoir);
        Task<DevoirADomicile> UpdateAsync(DevoirADomicile devoir);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        
        // Récupération par critères
        Task<IEnumerable<DevoirADomicile>> GetAllAsync(); // Pour Super-Admin
        Task<IEnumerable<DevoirADomicile>> GetByClasseAsync(int idClasse, int idAnneeScolaire);
        Task<IEnumerable<DevoirADomicile>> GetByAgentAsync(int idAgent);
        Task<IEnumerable<DevoirADomicile>> GetByEcoleAsync(int idEcole);
        Task<IEnumerable<DevoirADomicile>> GetByDirectionAsync(int idDirection);
        Task<IEnumerable<DevoirADomicile>> GetByCoursAsync(int idCours);
        Task<IEnumerable<DevoirADomicile>> GetByDatePublicationAsync(DateTime dateDebut, DateTime dateFin);
        
        // Pagination
        Task<PagedResult<DevoirADomicile>> GetAllPagedAsync(PagedRequest request, int idAnneeScolaire, int? idEcole = null, int? idClasse = null); // Pour Super-Admin avec filtres optionnels
        Task<PagedResult<DevoirADomicile>> GetByClassePagedAsync(int idClasse, PagedRequest request, int idAnneeScolaire);
        Task<PagedResult<DevoirADomicile>> GetByAgentPagedAsync(int idAgent, PagedRequest request, int idAnneeScolaire, int? idClasse = null); // Avec filtre classe optionnel
        Task<PagedResult<DevoirADomicile>> GetByEcolePagedAsync(int idEcole, PagedRequest request, int idAnneeScolaire, int? idClasse = null); // Avec filtre classe optionnel
        Task<PagedResult<DevoirADomicilePourTuteurDto>> GetByTuteurPagedAsync(
            int idTuteur, PagedRequest request,
            string? libelleAnneeScolaire = null);
        
        // Vérifications d'accès (intégrées dans le service)
        Task<bool> AgentPeutPublierPourClasseAsync(int idAgent, int idClasse);
        Task<bool> UserPeutAccederAuDevoirAsync(int idUtilisateur, int idDevoirADomicile);
        Task<bool> UserPeutAccederAClasseAsync(int idUtilisateur, int idClasse);
        Task<bool> EstDirecteurAsync(int idAgent);
        
        // Incrémenter le nombre de téléchargements
        Task<bool> IncrementerTelechargementsAsync(int idDevoirADomicile);
    }
}


