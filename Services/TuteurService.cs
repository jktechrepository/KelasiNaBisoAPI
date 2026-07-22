using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace KelasiNaBiso.Services
{
    public class TuteurService : ITuteurRepository
    {
        private readonly KelasiNaBisoDbContext _context;

        public TuteurService(KelasiNaBisoDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Tuteur>> GetAllAsync()
        {
            return await _context.Tuteurs
              //  .Include(t => t.Ecole)
              //  .Include(t => t.Eleves)
                .Where(t => t.Statut == true) // ✅ Filtrer uniquement les tuteurs actifs
                .ToListAsync();
        }

        public async Task<Tuteur> GetByIdAsync(int id)
        {
            return await _context.Tuteurs
              //  .Include(t => t.Ecole)
              //  .Include(t => t.Eleves)
                .Where(t => t.Statut == true) // ✅ Filtrer uniquement les tuteurs actifs
                .FirstOrDefaultAsync(t => t.IdTuteur == id);
        }

        public async Task<IEnumerable<Tuteur>> GetByEcoleAsync(int idEcole)
        {
            return await _context.Tuteurs
               // .Include(t => t.Ecole)
              //  .Include(t => t.Eleves)
                .Where(t => t.IdEcole == idEcole)
                .Where(t => t.Statut == true) // ✅ Filtrer uniquement les tuteurs actifs
                .ToListAsync();
        }

        public async Task<Tuteur> CreateAsync(Tuteur tuteur)
        {
            // ✅ UNICITÉ EMAIL TUTEUR: Vérifier que l'email n'existe pas déjà
            if (!string.IsNullOrEmpty(tuteur.Email))
            {
                var emailExists = await ExistsByEmailAsync(tuteur.Email);
                if (emailExists)
                {
                    throw new InvalidOperationException(
                        $"Un tuteur avec l'email '{tuteur.Email}' existe déjà. " +
                        $"Chaque email tuteur doit être unique dans le système."
                    );
                }
            }
            
            tuteur.DateCreation = DateTime.Now;
            
            _context.Tuteurs.Add(tuteur);
            await _context.SaveChangesAsync();
            return tuteur;
        }

        public async Task<Tuteur> UpdateAsync(Tuteur tuteur)
        {
            var existingTuteur = await _context.Tuteurs.FindAsync(tuteur.IdTuteur);
            if (existingTuteur == null)
                return null;

            // ✅ UNICITÉ EMAIL TUTEUR: Vérifier que le nouvel email n'est pas déjà utilisé par un autre tuteur
            if (!string.IsNullOrEmpty(tuteur.Email) && tuteur.Email != existingTuteur.Email)
            {
                var emailExistsByOtherTuteur = await _context.Tuteurs
                    .AnyAsync(t => t.Email == tuteur.Email && t.IdTuteur != tuteur.IdTuteur);
                
                if (emailExistsByOtherTuteur)
                {
                    throw new InvalidOperationException(
                        $"Un autre tuteur avec l'email '{tuteur.Email}' existe déjà. " +
                        $"Chaque email tuteur doit être unique dans le système."
                    );
                }
            }

            _context.Entry(existingTuteur).CurrentValues.SetValues(tuteur);

            await SyncTuteurUtilisateurAsync(existingTuteur);

            await _context.SaveChangesAsync();
            return existingTuteur;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var tuteur = await _context.Tuteurs.FindAsync(id);
            if (tuteur == null)
                return false;

            _context.Tuteurs.Remove(tuteur);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Tuteurs.AnyAsync(t => t.IdTuteur == id);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Tuteurs.AnyAsync(t => t.Email == email);
        }

        public async Task<IEnumerable<Eleve>> GetElevesAsync(int idTuteur)
        {
            return await _context.Eleves
                .Include(e => e.Classe)
                .Where(e => e.IdTuteur == idTuteur)
                .ToListAsync();
        }

        // ✅ SOFT DELETE: Toggle le statut d'un tuteur (actif <-> inactif)
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var tuteur = await _context.Tuteurs.FindAsync(id);
            if (tuteur == null)
                return false;

            tuteur.Statut = tuteur.Statut != true;
            await SyncTuteurUtilisateurAsync(tuteur);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task SyncTuteurUtilisateurAsync(Tuteur tuteur, CancellationToken cancellationToken = default)
        {
            var utilisateur = await _context.Utilisateurs
                .FirstOrDefaultAsync(u => u.IdTuteur == tuteur.IdTuteur, cancellationToken);

            if (utilisateur == null)
            {
                return;
            }

            utilisateur.NomUtilisateur = tuteur.NomComplet;
            utilisateur.PrenomUtilisateur = null;
            utilisateur.PostNomUtilisateur = null;
            utilisateur.Telephone = tuteur.Telephone;
            utilisateur.Email = tuteur.Email;
            utilisateur.PhotoUrl = tuteur.PhotoTuteurUrl;
            utilisateur.Genre = tuteur.Genre;
            utilisateur.Statut = tuteur.Statut ?? utilisateur.Statut;
            utilisateur.IdEcole = tuteur.IdEcole;
        }
    }
}
