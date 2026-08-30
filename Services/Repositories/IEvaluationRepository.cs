using KelasiNaBiso.Models;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IEvaluationRepository
    {
        Task<IEnumerable<Evaluation>> GetAllAsync();
        Task<Evaluation> GetByIdAsync(int id);
      //  Task<Evaluation> GetByTitreAsync(string titre);
        Task<IEnumerable<Evaluation>> GetByCoursAsync(int idCours);
        Task<IEnumerable<Evaluation>> GetByClasseAsync(int idClasse);
        Task<IEnumerable<Evaluation>> GetByTypeAsync(string type);
        Task<IEnumerable<Evaluation>> GetByPeriodeAsync(string periode);
        Task<IEnumerable<Evaluation>> GetByStatutAsync(bool statut);
        Task<IEnumerable<Evaluation>> GetByDateEvaluationAsync(DateTime date);
        Task<IEnumerable<Evaluation>> GetByDateRangeAsync(DateTime dateDebut, DateTime dateFin);
        Task<Evaluation> CreateAsync(Evaluation evaluation);
        Task<Evaluation> UpdateAsync(Evaluation evaluation);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
       // Task<bool> ExistsByTitreAsync(string titre);
        
        // ✅ SOFT DELETE
        Task<bool> ToggleStatutAsync(int id);
    }
}
