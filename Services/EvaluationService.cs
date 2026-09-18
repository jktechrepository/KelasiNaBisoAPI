using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class EvaluationService : IEvaluationRepository
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly PeriodeCotationResolver _periodeResolver;

        public EvaluationService(KelasiNaBisoDbContext context, PeriodeCotationResolver periodeResolver)
        {
            _context = context;
            _periodeResolver = periodeResolver;
        }

        public async Task<IEnumerable<Evaluation>> GetAllAsync()
        {
            return await _context.Evaluations
                .Include(e => e.Course)
                .Include(e => e.Classe)
                .Include(e => e.PeriodeCotation)
                .Where(e => e.Statut == true) // ✅ Filtrer uniquement les évaluations actives
                .OrderByDescending(e => e.DateCreation)
                .ToListAsync();
        }

        public async Task<Evaluation> GetByIdAsync(int id)
        {
            return await _context.Evaluations
                .Include(e => e.Course)
                .Include(e => e.Classe)
                .Include(e => e.PeriodeCotation)
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
                .Include(e => e.PeriodeCotation)
                .Where(e => e.IdCours == idCours && e.Statut == true) // ✅ Filtrer actifs
                .OrderByDescending(e => e.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<Evaluation>> GetByClasseAsync(int idClasse)
        {
            return await _context.Evaluations
                .Include(e => e.Course)
                .Include(e => e.PeriodeCotation)
                .Where(e => e.IdClasse == idClasse && e.Statut == true) // ✅ Filtrer actifs
                .OrderByDescending(e => e.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<Evaluation>> GetByTypeAsync(string type)
        {
            return await _context.Evaluations
                .Include(e => e.Course)
                .Include(e => e.Classe)
                .Include(e => e.PeriodeCotation)
                .Where(e => e.TypeEvaluation == type && e.Statut == true) // ✅ Filtrer actifs
                .OrderByDescending(e => e.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<Evaluation>> GetByPeriodeAsync(string periode)
        {
            var resolved = await _periodeResolver.ResolveAsync(null, periode);
            if (resolved != null)
            {
                var aliases = PeriodeCotationAliases.GetAliases(resolved.Code);
                return await _context.Evaluations
                    .Include(e => e.Course)
                    .Include(e => e.Classe)
                    .Include(e => e.PeriodeCotation)
                    .Where(e => e.Statut == true
                        && (e.IdPeriode == resolved.IdPeriode
                            || (e.IdPeriode == null && e.Periode != null && aliases.Contains(e.Periode))))
                    .OrderByDescending(e => e.DateCreation)
                    .ToListAsync();
            }

            return await _context.Evaluations
                .Include(e => e.Course)
                .Include(e => e.Classe)
                .Include(e => e.PeriodeCotation)
                .Where(e => e.Periode == periode && e.Statut == true)
                .OrderByDescending(e => e.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<Evaluation>> GetByStatutAsync(bool statut)
        {
            return await _context.Evaluations
                .Include(e => e.Course)
                .Include(e => e.Classe)
                .Include(e => e.PeriodeCotation)
                .Where(e => e.Statut == statut)
                .OrderByDescending(e => e.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<Evaluation>> GetByDateEvaluationAsync(DateTime date)
        {
            return await _context.Evaluations
                .Include(e => e.Course)
                .Include(e => e.Classe)
                .Include(e => e.PeriodeCotation)
                .Where(e => e.DateCreation == date.Date && e.Statut == true) // ✅ Filtrer actifs
                .OrderByDescending(e => e.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<Evaluation>> GetByDateRangeAsync(DateTime dateDebut, DateTime dateFin)
        {
            return await _context.Evaluations
                .Include(e => e.Course)
                .Include(e => e.Classe)
                .Include(e => e.PeriodeCotation)
                .Where(e => e.DateCreation >= dateDebut && e.DateCreation <= dateFin && e.Statut == true) // ✅ Filtrer actifs
                .OrderByDescending(e => e.DateCreation)
                .ToListAsync();
        }

        public async Task<Evaluation> CreateAsync(Evaluation evaluation)
        {
            await ValidateRelationsAsync(evaluation);
            evaluation.DateCreation = DateTime.Now;
            evaluation.Statut ??= true;

            _context.Evaluations.Add(evaluation);
            await _context.SaveChangesAsync();
            return evaluation;
        }

        public async Task<Evaluation> UpdateAsync(Evaluation evaluation)
        {
            var existingEvaluation = await _context.Evaluations.FindAsync(evaluation.IdEvaluation);
            if (existingEvaluation == null)
                return null;

            await ValidateRelationsAsync(evaluation);

            existingEvaluation.TypeEvaluation = evaluation.TypeEvaluation;
            existingEvaluation.TitreEvaluation = evaluation.TitreEvaluation;
            existingEvaluation.Periode = evaluation.Periode;
            existingEvaluation.IdPeriode = evaluation.IdPeriode;
            existingEvaluation.Coefficient = evaluation.Coefficient;
            existingEvaluation.IdCours = evaluation.IdCours;
            existingEvaluation.IdClasse = evaluation.IdClasse;
            await _context.SaveChangesAsync();
            return existingEvaluation;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var evaluation = await _context.Evaluations.FindAsync(id);
            if (evaluation == null)
                return false;

            evaluation.Statut = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Evaluations.AnyAsync(e => e.IdEvaluation == id && e.Statut == true);
        }

        private async Task ValidateRelationsAsync(Evaluation evaluation)
        {
            if (evaluation.IdCours <= 0)
                throw new InvalidOperationException("IdCours est obligatoire.");
            if (evaluation.IdClasse <= 0)
                throw new InvalidOperationException("IdClasse est obligatoire.");

            var coursOk = await _context.Cours.AsNoTracking()
                .AnyAsync(c => c.IdCours == evaluation.IdCours && c.Statut == true);
            if (!coursOk)
                throw new InvalidOperationException($"Cours {evaluation.IdCours} introuvable ou inactif.");

            var classeOk = await _context.Classes.AsNoTracking()
                .AnyAsync(c => c.IdClasse == evaluation.IdClasse && c.Statut == true);
            if (!classeOk)
                throw new InvalidOperationException($"Classe {evaluation.IdClasse} introuvable ou inactive.");

            if (evaluation.Coefficient.HasValue && evaluation.Coefficient.Value < 0)
                throw new InvalidOperationException("Le coefficient ne peut pas être négatif.");
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
