using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.Pagination;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IClasseRepository
    {
        Task<PagedResult<Classe>> GetAllPagedAsync(PagedRequest request);
        Task<IEnumerable<Classe>> GetAllAsync(); // ⚠️ DEPRECATED: Utiliser GetAllPagedAsync
        Task<Classe> GetByIdAsync(int id);
        Task<Classe> GetByNomAsync(string nom);
        Task<Classe> GetByEcoleAndNomAsync(int idEcole, string nomClasse);
        Task<IEnumerable<Classe>> GetByEcoleAsync(int idEcole);
        Task<IEnumerable<Classe>> GetBySectionAsync(int idSection);
        Task<IEnumerable<Classe>> GetByOptionAsync(int idOption);
        Task<IEnumerable<Classe>> GetWithoutSectionAsync();
        Task<IEnumerable<Classe>> GetWithoutOptionAsync();
        //Task<IEnumerable<Classe>> GetByStatutAsync(bool statut);
        Task<Classe> CreateAsync(Classe classe);
        Task<Classe> UpdateAsync(Classe classe);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByNomAsync(string nom);
        Task<IEnumerable<Eleve>> GetElevesAsync(int idClasse);
        Task<IEnumerable<Cours>> GetCoursAsync(int idClasse);
        Task<IEnumerable<Inscription>> GetInscriptionsAsync(int idClasse);
      //  Task<IEnumerable<Frais>> GetFraisAsync(int idClasse);
        Task<IEnumerable<Evaluation>> GetEvaluationsAsync(int idClasse);
        
        // ✅ SOFT DELETE
        Task<bool> ToggleStatutAsync(int id);
    }
}
