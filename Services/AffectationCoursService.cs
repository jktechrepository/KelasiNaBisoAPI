using KelasiNaBiso.Models;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Data;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class AffectationCoursService : IAffectationCoursRepository
    {
        private readonly KelasiNaBisoDbContext _context;

        public AffectationCoursService(KelasiNaBisoDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AffectationCours>> GetAllAsync()
        {
            return await _context.AffectationsCours
               // .Include(ac => ac.Agent)
               // .Include(ac => ac.Cours)
               // .Include(ac => ac.AnneeScolaire)
                .OrderByDescending(ac => ac.DateCreation)
                .ToListAsync();
        }

        public async Task<AffectationCours?> GetByIdAsync(int id)
        {
            return await _context.AffectationsCours
                //.Include(ac => ac.Agent)
               // .Include(ac => ac.Cours)
               // .Include(ac => ac.AnneeScolaire)
                .FirstOrDefaultAsync(ac => ac.IdAffectationCours == id);
        }

        public async Task<AffectationCours> CreateAsync(AffectationCours affectationCours)
        {
            // Vérifier si l'agent existe
            var agent = await _context.Agents.FindAsync(affectationCours.IdAgent);
            if (agent == null)
                throw new ArgumentException("L'agent spécifié n'existe pas.");

            // Vérifier si le cours existe
            var cours = await _context.Cours.FindAsync(affectationCours.IdCours);
            if (cours == null)
                throw new ArgumentException("Le cours spécifié n'existe pas.");

            // Vérifier si l'année scolaire existe
            var anneeScolaire = await _context.AnneeScolaires.FindAsync(affectationCours.IdAnneeScolaire);
            if (anneeScolaire == null)
                throw new ArgumentException("L'année scolaire spécifiée n'existe pas.");

            // Vérifier s'il n'y a pas déjà une affectation active pour cette combinaison
            var existingAffectation = await _context.AffectationsCours
                .FirstOrDefaultAsync(ac => ac.IdAgent == affectationCours.IdAgent 
                                          && ac.IdCours == affectationCours.IdCours 
                                          && ac.IdAnneeScolaire == affectationCours.IdAnneeScolaire 
                                          && ac.Statut == true);
            
            if (existingAffectation != null)
                throw new InvalidOperationException("Une affectation active existe déjà pour cet agent, ce cours et cette année scolaire.");

            affectationCours.DateCreation = DateTime.Now;
            affectationCours.DateAffectation = DateTime.Now;
            
            _context.AffectationsCours.Add(affectationCours);
            await _context.SaveChangesAsync();
            
            return await GetByIdAsync(affectationCours.IdAffectationCours);
        }

        public async Task<AffectationCours> UpdateAsync(AffectationCours affectationCours)
        {
            var existingAffectation = await _context.AffectationsCours.FindAsync(affectationCours.IdAffectationCours);
            if (existingAffectation == null)
                throw new ArgumentException("L'affectation de cours spécifiée n'existe pas.");

            // Vérifier si l'agent existe
            var agent = await _context.Agents.FindAsync(affectationCours.IdAgent);
            if (agent == null)
                throw new ArgumentException("L'agent spécifié n'existe pas.");

            // Vérifier si le cours existe
            var cours = await _context.Cours.FindAsync(affectationCours.IdCours);
            if (cours == null)
                throw new ArgumentException("Le cours spécifié n'existe pas.");

            // Vérifier si l'année scolaire existe
            var anneeScolaire = await _context.AnneeScolaires.FindAsync(affectationCours.IdAnneeScolaire);
            if (anneeScolaire == null)
                throw new ArgumentException("L'année scolaire spécifiée n'existe pas.");

            // Vérifier s'il n'y a pas déjà une affectation active pour cette combinaison (sauf celle qu'on modifie)
            var existingActiveAffectation = await _context.AffectationsCours
                .FirstOrDefaultAsync(ac => ac.IdAgent == affectationCours.IdAgent 
                                          && ac.IdCours == affectationCours.IdCours 
                                          && ac.IdAnneeScolaire == affectationCours.IdAnneeScolaire 
                                          && ac.Statut == true
                                          && ac.IdAffectationCours != affectationCours.IdAffectationCours);
            
            if (existingActiveAffectation != null)
                throw new InvalidOperationException("Une affectation active existe déjà pour cet agent, ce cours et cette année scolaire.");

            existingAffectation.IdAgent = affectationCours.IdAgent;
            existingAffectation.IdCours = affectationCours.IdCours;
            existingAffectation.IdAnneeScolaire = affectationCours.IdAnneeScolaire;
            existingAffectation.DateFinAffectation = affectationCours.DateFinAffectation;
            existingAffectation.Statut = affectationCours.Statut;
            existingAffectation.Commentaire = affectationCours.Commentaire;

            await _context.SaveChangesAsync();
            
            return await GetByIdAsync(affectationCours.IdAffectationCours);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var affectationCours = await _context.AffectationsCours.FindAsync(id);
            if (affectationCours == null)
                return false;

            _context.AffectationsCours.Remove(affectationCours);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.AffectationsCours.AnyAsync(ac => ac.IdAffectationCours == id);
        }

        public async Task<IEnumerable<AffectationCours>> GetByAgentAsync(int idAgent)
        {
            return await _context.AffectationsCours
                //.Include(ac => ac.Agent)
               // .Include(ac => ac.Cours)
               // .Include(ac => ac.AnneeScolaire)
                .Where(ac => ac.IdAgent == idAgent)
                .OrderByDescending(ac => ac.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<AffectationCours>> GetByCoursAsync(int idCours)
        {
            return await _context.AffectationsCours
               // .Include(ac => ac.Agent)
               // .Include(ac => ac.Cours)
              //  .Include(ac => ac.AnneeScolaire)
                .Where(ac => ac.IdCours == idCours)
                .OrderByDescending(ac => ac.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<AffectationCours>> GetByAnneeScolaireAsync(int idAnneeScolaire)
        {
            return await _context.AffectationsCours
               // .Include(ac => ac.Agent)
               // .Include(ac => ac.Cours)
               // .Include(ac => ac.AnneeScolaire)
                .Where(ac => ac.IdAnneeScolaire == idAnneeScolaire)
                .OrderByDescending(ac => ac.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<AffectationCours>> GetByAgentAndAnneeScolaireAsync(int idAgent, int idAnneeScolaire)
        {
            return await _context.AffectationsCours
               // .Include(ac => ac.Agent)
               // .Include(ac => ac.Cours)
               // .Include(ac => ac.AnneeScolaire)
                .Where(ac => ac.IdAgent == idAgent && ac.IdAnneeScolaire == idAnneeScolaire)
                .OrderByDescending(ac => ac.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<AffectationCours>> GetByCoursAndAnneeScolaireAsync(int idCours, int idAnneeScolaire)
        {
            return await _context.AffectationsCours
                //.Include(ac => ac.Agent)
               // .Include(ac => ac.Cours)
               // .Include(ac => ac.AnneeScolaire)
                .Where(ac => ac.IdCours == idCours && ac.IdAnneeScolaire == idAnneeScolaire)
                .OrderByDescending(ac => ac.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<AffectationCours>> GetActivesAsync()
        {
            return await _context.AffectationsCours
                // .Include(ac => ac.Agent)
               // .Include(ac => ac.Cours)
               // .Include(ac => ac.AnneeScolaire)
                .Where(ac => ac.Statut == true)
                .OrderByDescending(ac => ac.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<AffectationCours>> GetActivesByAgentAsync(int idAgent)
        {
            return await _context.AffectationsCours
               // .Include(ac => ac.Agent)
               // .Include(ac => ac.Cours)
               // .Include(ac => ac.AnneeScolaire)
                .Where(ac => ac.IdAgent == idAgent && ac.Statut == true)
                .OrderByDescending(ac => ac.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<AffectationCours>> GetActivesByCoursAsync(int idCours)
        {
            return await _context.AffectationsCours
               //
                .Include(ac => ac.Agent)
               // .Include(ac => ac.Cours)
               // .Include(ac => ac.AnneeScolaire)
                .Where(ac => ac.IdCours == idCours && ac.Statut == true)
                .OrderByDescending(ac => ac.DateCreation)
                .ToListAsync();
        }

        public async Task<bool> ExistsActiveAffectationAsync(int idAgent, int idCours, int idAnneeScolaire)
        {
            return await _context.AffectationsCours
                .AnyAsync(ac => ac.IdAgent == idAgent 
                               && ac.IdCours == idCours 
                               && ac.IdAnneeScolaire == idAnneeScolaire 
                               && ac.Statut == true);
        }

        // ✅ SOFT DELETE: Toggle le statut d'une affectation (actif <-> inactif)
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var affectation = await _context.AffectationsCours.FindAsync(id);
            if (affectation == null)
                return false;

            affectation.Statut = affectation.Statut != true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
