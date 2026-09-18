using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;

namespace KelasiNaBiso.Services.Repositories
{
    public interface INoteRepository
    {
        Task<IEnumerable<Note>> GetAllAsync();
        Task<Note> GetByIdAsync(int id);
        Task<IEnumerable<Note>> GetByEleveAsync(int idEleve);
        Task<IEnumerable<Note>> GetByEvaluationAsync(int idEvaluation);
        Task<IEnumerable<Note>> GetByCoursAsync(int idCours); // ⚠️ DEPRECATED : Utiliser GetByEvaluationAsync. Fonctionne via Evaluation.IdCours
        Task<IEnumerable<Note>> GetByProfesseurAsync(int idProfesseur);
        Task<IEnumerable<Note>> GetByAnneeScolaireAsync(int idAnneeScolaire);
        Task<IEnumerable<Note>> GetByPeriodeAsync(string periode);
        Task<IEnumerable<Note>> GetBySessionAsync(string session); // ⚠️ DEPRECATED : Utiliser GetByPeriodeAsync
        Task<Note> CreateAsync(Note note);
        Task<Note> UpdateAsync(Note note);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// Upsert transactionnel des notes d'une évaluation (max 80 lignes).
        /// </summary>
        Task<BulkNoteResultDto> UpsertBulkAsync(
            BulkNoteRequestDto request,
            int idProfesseur,
            CancellationToken cancellationToken = default);

        // ✅ SOFT DELETE
        Task<bool> ToggleStatutAsync(int id);
    }
}
