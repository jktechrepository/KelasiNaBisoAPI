using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class AnneeScolaireService : IAnneeScolaireRepository
    {
        private readonly KelasiNaBisoDbContext _context;

        public AnneeScolaireService(KelasiNaBisoDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AnneeScolaire>> GetAllAsync()
        {
            return await _context.AnneeScolaires
              //  .Include(a => a.Ecole)
              //  .Include(a => a.Inscriptions)
              //  .Include(a => a.Notes)
               // .Include(a => a.Notifications)
                .Where(a => a.Statut == true) // ✅ Filtrer uniquement les années actives
                .OrderByDescending(a => a.DateCreation)
                .ToListAsync();
        }

        public async Task<AnneeScolaire> GetByIdAsync(int id)
        {
            return await _context.AnneeScolaires
              //  .Include(a => a.Ecole)
              //  .Include(a => a.Inscriptions)
              //  .Include(a => a.Notes)
              //  .Include(a => a.Notifications)
                .FirstOrDefaultAsync(a => a.IdAnneeScolaire == id);
        }

        public async Task<AnneeScolaire> GetByLibelleAsync(string libelle)
        {
            return await _context.AnneeScolaires
               // .Include(a => a.Ecole)
              //  .Include(a => a.Inscriptions)
              //  .Include(a => a.Notes)
              //  .Include(a => a.Notifications)
                .FirstOrDefaultAsync(a => a.LibelleAnneeScolaire == libelle);
        }

        public async Task<AnneeScolaire> GetByEcoleAndLibelleAsync(int idEcole, string libelleAnneeScolaire)
        {
            return await _context.AnneeScolaires
                .Where(a => a.IdEcole == idEcole && 
                           a.LibelleAnneeScolaire.ToLower() == libelleAnneeScolaire.ToLower() && 
                           a.Statut == true)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<AnneeScolaire>> GetByEcoleAsync(int idEcole)
        {
            return await _context.AnneeScolaires
               // .Include(a => a.Inscriptions)
               // .Include(a => a.Notes)
              //  .Include(a => a.Notifications)
                .Where(a => a.IdEcole == idEcole && a.Statut == true) // ✅ Filtrer actifs
                .OrderByDescending(a => a.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<AnneeScolaire>> GetByDateRangeAsync(DateTime dateDebut, DateTime dateFin)
        {
            return await _context.AnneeScolaires
              //  .Include(a => a.Ecole)
              //  .Include(a => a.Inscriptions)
              //  .Include(a => a.Notes)
              //  .Include(a => a.Notifications)
                .Where(a => a.DateDebut >= dateDebut && a.DateFin <= dateFin && a.Statut == true) // ✅ Filtrer actifs
                .OrderByDescending(a => a.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<AnneeScolaire>> GetActivesAsync()
        {
            var now = DateTime.Now;
            return await _context.AnneeScolaires
               // .Include(a => a.Ecole)
              //  .Include(a => a.Inscriptions)
              //  .Include(a => a.Notes)
              //  .Include(a => a.Notifications)
                .Where(a => a.DateDebut <= now && a.DateFin >= now && a.Statut == true) // ✅ Filtrer actifs
                .OrderByDescending(a => a.DateCreation)
                .ToListAsync();
        }

        /// <summary>
        /// Année scolaire courante pour une école.
        /// 1) Période de cours : DateDebut &lt;= now &lt;= DateFin.
        /// 2) Vacances (gap entre fin année A et début année B) : retourne l'année B à venir.
        /// </summary>
        public async Task<AnneeScolaire> GetAnneeCouranteAsync(int idEcole)
        {
            var now = DateTime.Now;
            var actives = _context.AnneeScolaires
                .Where(a => a.IdEcole == idEcole && a.Statut == true);

            var enSession = await actives
                .Where(a => a.DateDebut <= now && a.DateFin >= now)
                .FirstOrDefaultAsync();

            if (enSession != null)
                return enSession;

            var anneePrecedenteTerminee = await actives
                .Where(a => a.DateFin < now)
                .OrderByDescending(a => a.DateFin)
                .FirstOrDefaultAsync();

            if (anneePrecedenteTerminee == null)
                return null;

            var prochaineAnnee = await actives
                .Where(a => a.DateDebut > now)
                .OrderBy(a => a.DateDebut)
                .FirstOrDefaultAsync();

            return prochaineAnnee;
        }

        /// <summary>
        /// Année scolaire précédente pour une école (N-1 par rapport à l'année courante).
        /// </summary>
        public async Task<AnneeScolaire?> GetAnneePrecedenteAsync(int idEcole)
        {
            var actives = _context.AnneeScolaires
                .Where(a => a.IdEcole == idEcole && a.Statut == true);

            var courante = await GetAnneeCouranteAsync(idEcole);

            if (courante != null)
            {
                return await actives
                    .Where(a => a.DateFin < courante.DateDebut)
                    .OrderByDescending(a => a.DateFin)
                    .FirstOrDefaultAsync();
            }

            var now = DateTime.Now;
            return await actives
                .Where(a => a.DateFin < now)
                .OrderByDescending(a => a.DateFin)
                .FirstOrDefaultAsync();
        }

        public async Task<AnneeScolaire> CreateAsync(AnneeScolaire anneeScolaire)
        {
            anneeScolaire.DateCreation = DateTime.Now;
            
            _context.AnneeScolaires.Add(anneeScolaire);
            await _context.SaveChangesAsync();
            return anneeScolaire;
        }

        public async Task<AnneeScolaire> UpdateAsync(AnneeScolaire anneeScolaire)
        {
            var existingAnneeScolaire = await _context.AnneeScolaires.FindAsync(anneeScolaire.IdAnneeScolaire);
            if (existingAnneeScolaire == null)
                return null;

            _context.Entry(existingAnneeScolaire).CurrentValues.SetValues(anneeScolaire);
            await _context.SaveChangesAsync();
            return existingAnneeScolaire;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var anneeScolaire = await _context.AnneeScolaires.FindAsync(id);
            if (anneeScolaire == null)
                return false;

            _context.AnneeScolaires.Remove(anneeScolaire);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.AnneeScolaires.AnyAsync(a => a.IdAnneeScolaire == id);
        }

        public async Task<bool> ExistsByLibelleAsync(string libelle)
        {
            return await _context.AnneeScolaires.AnyAsync(a => a.LibelleAnneeScolaire == libelle);
        }

        public async Task<IEnumerable<Inscription>> GetInscriptionsAsync(int idAnneeScolaire)
        {
            return await _context.Inscriptions
              //  .Include(i => i.Eleve)
              //  .Include(i => i.Classe)
              //  .Include(i => i.Ecole)
                .Where(i => i.IdAnneeScolaire == idAnneeScolaire)
                .OrderByDescending(i => i.DateInscription)
                .ToListAsync();
        }

        public async Task<IEnumerable<Note>> GetNotesAsync(int idAnneeScolaire)
        {
            return await _context.Notes
               // .Include(n => n.Eleve)
               // .Include(n => n.Cours)
               // .Include(n => n.Professeur)
                .Where(n => n.IdAnneeScolaire == idAnneeScolaire)
                .OrderByDescending(n => n.DateEvaluation)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetNotificationsAsync(int idAnneeScolaire)
        {
            return await _context.Notifications
               // .Include(n => n.Expediteur)
              //  .Include(n => n.Destinataire)
               // .Include(n => n.Ecole)
               // .Include(n => n.Classe)
               // .Include(n => n.Eleve)
               // .Include(n => n.Cours)
                .Where(n => n.IdAnneeScolaire == idAnneeScolaire)
                .OrderByDescending(n => n.DateCreation)
                .ToListAsync();
        }

        // ✅ SOFT DELETE: Toggle le statut d'une année scolaire (actif <-> inactif)
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var anneeScolaire = await _context.AnneeScolaires.FindAsync(id);
            if (anneeScolaire == null)
                return false;

            anneeScolaire.Statut = anneeScolaire.Statut != true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
