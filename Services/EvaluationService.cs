using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class EvaluationService : IEvaluationRepository
    {
        private readonly KelasiNaBisoDbContext _context;

        public EvaluationService(KelasiNaBisoDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Evaluation>> GetAllAsync()
        {
            return await _context.Evaluations
                .Include(e => e.Course)
                .Include(e => e.Classe)
                .Where(e => e.Statut == true) // ✅ Filtrer uniquement les évaluations actives
                .OrderByDescending(e => e.DateCreation)
                .ToListAsync();
        }

        public async Task<Evaluation> GetByIdAsync(int id)
        {
            return await _context.Evaluations
                .Include(e => e.Course)
                .Include(e => e.Classe)
                .FirstOrDefaultAsync(e => e.IdEvaluation == id);
        }

        //public async Task<Evaluation> GetByTitreAsync(string titre)
        //{
        //    return await _context.Evaluations
        //        .Include(e => e.Course)
        //        .Include(e => e.Classe)
        //        .FirstOrDefaultAsync(e => e.Titre == titre);
        //}

        public async Task<IEnumerable<Evaluation>> GetByCoursAsync(int idCours)
        {
            return await _context.Evaluations
                .Include(e => e.Classe)
                .Where(e => e.IdCours == idCours && e.Statut == true) // ✅ Filtrer actifs
                .OrderByDescending(e => e.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<Evaluation>> GetByClasseAsync(int idClasse)
        {
            return await _context.Evaluations
                .Include(e => e.Course)
                .Where(e => e.IdClasse == idClasse && e.Statut == true) // ✅ Filtrer actifs
                .OrderByDescending(e => e.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<Evaluation>> GetByTypeAsync(string type)
        {
            return await _context.Evaluations
                .Include(e => e.Course)
                .Include(e => e.Classe)
                .Where(e => e.TypeEvaluation == type && e.Statut == true) // ✅ Filtrer actifs
                .OrderByDescending(e => e.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<Evaluation>> GetByDateEvaluationAsync(DateTime date)
        {
            return await _context.Evaluations
                .Include(e => e.Course)
                .Include(e => e.Classe)
                .Where(e => e.DateCreation == date.Date && e.Statut == true) // ✅ Filtrer actifs
                .OrderByDescending(e => e.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<Evaluation>> GetByDateRangeAsync(DateTime dateDebut, DateTime dateFin)
        {
            return await _context.Evaluations
                .Include(e => e.Course)
                .Include(e => e.Classe)
                .Where(e => e.DateCreation >= dateDebut && e.DateCreation <= dateFin && e.Statut == true) // ✅ Filtrer actifs
                .OrderByDescending(e => e.DateCreation)
                .ToListAsync();
        }

        public async Task<Evaluation> CreateAsync(Evaluation evaluation)
        {
            evaluation.DateCreation = DateTime.Now;
            
            _context.Evaluations.Add(evaluation);
            await _context.SaveChangesAsync();
            return evaluation;
        }

        public async Task<Evaluation> UpdateAsync(Evaluation evaluation)
        {
            var existingEvaluation = await _context.Evaluations.FindAsync(evaluation.IdEvaluation);
            if (existingEvaluation == null)
                return null;

            _context.Entry(existingEvaluation).CurrentValues.SetValues(evaluation);
            await _context.SaveChangesAsync();
            return existingEvaluation;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var evaluation = await _context.Evaluations.FindAsync(id);
            if (evaluation == null)
                return false;

            _context.Evaluations.Remove(evaluation);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Evaluations.AnyAsync(e => e.IdEvaluation == id);
        }

        //public async Task<bool> ExistsByTitreAsync(string titre)
        //{
        //    return await _context.Evaluations.AnyAsync(e => e.Titre == titre);
        //}

        // ✅ SOFT DELETE: Toggle le statut d'une évaluation (actif <-> inactif)
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var evaluation = await _context.Evaluations.FindAsync(id);
            if (evaluation == null)
                return false;

            evaluation.Statut = evaluation.Statut != true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
