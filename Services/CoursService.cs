using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class CoursService : ICoursRepository
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly EleveAnneeScopeHelper _scope;

        public CoursService(KelasiNaBisoDbContext context, EleveAnneeScopeHelper scope)
        {
            _context = context;
            _scope = scope;
        }
        

        public async Task<IEnumerable<Cours>> GetAllAsync()
        {
            return await _context.Cours
              //  .Include(c => c.Classe)
              //  .Include(c => c.AffectationsCours)
              //  .ThenInclude(ac => ac.Agent)
              //  .Include(c => c.Notes)
              //  .Include(c => c.Evaluations)
              //  .Include(c => c.RessourcesPedagogiques)
                .Where(c => c.Statut == true) // ✅ Filtrer uniquement les cours actifs
                .ToListAsync();
        }

        public async Task<IEnumerable<Cours>> GetAllByEcoleAsync(int idEcole)
        {
            return await _context.Cours
                                 .Where(c=> c.Classe.Direction.IdEcole == idEcole)
                                 .Where(c => c.Statut == true) // ✅ Filtrer uniquement les cours actifs
                                .ToListAsync();
        }

        public async Task<Cours> GetByIdAsync(int id)
        {
            return await _context.Cours
                .Include(c => c.Classe)
                .Include(c => c.AffectationsCours)
                    .ThenInclude(ac => ac.Agent)
               // .Include(c => c.Notes)
               // .Include(c => c.Evaluations)
               // .Include(c => c.RessourcesPedagogiques)
                .Where(c => c.Statut == true) // ✅ Filtrer uniquement les cours actifs
                .FirstOrDefaultAsync(c => c.IdCours == id);
        }

        public async Task<IEnumerable<Cours>> GetByClasseAsync(int idClasse)
        {
            return await _context.Cours
               // .Include(c => c.AffectationsCours)
              //  .ThenInclude(ac => ac.Agent)
                //.Include(c => c.Notes)
                //.Include(c => c.Evaluations)
                //.Include(c => c.RessourcesPedagogiques)
                .Where(c => c.IdClasse == idClasse)
                .Where(c => c.Statut == true) // ✅ Filtrer uniquement les cours actifs
                .ToListAsync();
        }

        // Cette méthode n'est plus nécessaire car nous utilisons maintenant AffectationCours
        // public async Task<IEnumerable<Cours>> GetByProfesseurAsync(int idProfesseur)

     

        public async Task<Cours> CreateAsync(Cours cours)
        {
            cours.DateCreation = DateTime.Now;
            
            _context.Cours.Add(cours);
            await _context.SaveChangesAsync();
            return cours;
        }

        public async Task<Cours> UpdateAsync(Cours cours)
        {
            var existingCours = await _context.Cours.FindAsync(cours.IdCours);
            if (existingCours == null)
                return null;

            _context.Entry(existingCours).CurrentValues.SetValues(cours);
            await _context.SaveChangesAsync();
            return existingCours;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var cours = await _context.Cours.FindAsync(id);
            if (cours == null)
                return false;

            _context.Cours.Remove(cours);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Cours.AnyAsync(c => c.IdCours == id);
        }

        // ⚠️ DEPRECATED : Les notes sont maintenant liées à Evaluation, pas directement à Cours
        // Cette méthode fonctionne via Evaluation.IdCours
        public async Task<IEnumerable<Note>> GetNotesAsync(int idCours, int? idAnneeScolaire = null)
        {
            var idEcole = await ResolveEcoleForCoursAsync(idCours);
            var annee = await _scope.ResolveIdAnneeScolaireAsync(idEcole, idAnneeScolaire);

            return await _context.Notes
                .Include(n => n.Evaluation)
                .Where(n => n.Evaluation.IdCours == idCours)
                .Where(n => n.IdAnneeScolaire == annee)
                .Where(n => n.Statut == true)
                .ToListAsync();
        }

        public async Task<IEnumerable<Evaluation>> GetEvaluationsAsync(int idCours, int? idAnneeScolaire = null)
        {
            var idEcole = await ResolveEcoleForCoursAsync(idCours);
            var annee = await _scope.ResolveIdAnneeScolaireAsync(idEcole, idAnneeScolaire);

            return await _context.Evaluations
                .Where(e => e.IdCours == idCours && e.Statut == true)
                .Where(e => _context.Notes.Any(n =>
                    n.IdEvaluation == e.IdEvaluation
                    && n.IdAnneeScolaire == annee
                    && n.Statut == true)
                    || !_context.Notes.Any(n =>
                        n.IdEvaluation == e.IdEvaluation && n.Statut == true))
                .ToListAsync();
        }

        private async Task<int> ResolveEcoleForCoursAsync(int idCours)
        {
            var idEcole = await _context.Cours
                .AsNoTracking()
                .Where(c => c.IdCours == idCours)
                .Select(c => c.Classe != null && c.Classe.Direction != null
                    ? c.Classe.Direction.IdEcole
                    : (int?)null)
                .FirstOrDefaultAsync();

            if (!idEcole.HasValue || idEcole.Value <= 0)
                throw new InvalidOperationException($"Cours {idCours} introuvable ou non rattaché à une école.");

            return idEcole.Value;
        }

        public async Task<IEnumerable<RessourcePedagogique>> GetRessourcesAsync(int idCours)
        {
            return await _context.RessourcePedagogiques
                .Where(r => r.IdCours == idCours)
                .ToListAsync();
        }

        // ✅ SOFT DELETE: Toggle le statut d'un cours (actif <-> inactif)
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var cours = await _context.Cours.FindAsync(id);
            if (cours == null)
                return false;

            cours.Statut = cours.Statut != true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
