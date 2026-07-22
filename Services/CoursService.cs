using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class CoursService : ICoursRepository
    {
        private readonly KelasiNaBisoDbContext _context;

        public CoursService(KelasiNaBisoDbContext context)
        {
            _context = context;
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
        public async Task<IEnumerable<Note>> GetNotesAsync(int idCours)
        {
            return await _context.Notes
                .Include(n => n.Evaluation)
              //  .Include(n => n.Eleve)
              //  .Include(n => n.Professeur)
              //  .Include(n => n.AnneeScolaire)
                .Where(n => n.Evaluation.IdCours == idCours)
                .Where(n => n.Statut == true) // ✅ Filtrer uniquement les notes actives
                .ToListAsync();
        }

        public async Task<IEnumerable<Evaluation>> GetEvaluationsAsync(int idCours)
        {
            return await _context.Evaluations
            //    .Include(e => e.Classe)
                .Where(e => e.IdCours == idCours)
                .ToListAsync();
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
