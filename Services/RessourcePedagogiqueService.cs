using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class RessourcePedagogiqueService : IRessourcePedagogiqueRepository
    {
        private readonly KelasiNaBisoDbContext _context;

        public RessourcePedagogiqueService(KelasiNaBisoDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RessourcePedagogique>> GetAllAsync()
        {
            return await _context.RessourcePedagogiques
                .Where(r => r.Statut == true) // ✅ Filtrer uniquement les ressources actives
                .OrderByDescending(r => r.DateCreation)
                .ToListAsync();
        }

        public async Task<RessourcePedagogique> GetByIdAsync(int id)
        {
            return await _context.RessourcePedagogiques
                .FirstOrDefaultAsync(r => r.IdRessourcePedagogique == id);
        }

        public async Task<RessourcePedagogique> GetByTitreAsync(string titre)
        {
            return await _context.RessourcePedagogiques
                .FirstOrDefaultAsync(r => r.TitreRessourcePedagogique == titre);
        }

        public async Task<IEnumerable<RessourcePedagogique>> GetByCoursAsync(int idCours)
        {
            return await _context.RessourcePedagogiques
                .Where(r => r.IdCours == idCours && r.Statut == true) // ✅ Filtrer actifs
                .OrderByDescending(r => r.DateCreation)
                .ToListAsync();
        }

        //public async Task<IEnumerable<RessourcePedagogique>> GetByTypeAsync(string type)
        //{
        //    return await _context.RessourcePedagogiques
        //        .Include(r => r.Cours)
        //        .Where(r => r.Type == type)
        //        .OrderByDescending(r => r.DateCreation)
        //        .ToListAsync();
        //}

        public async Task<IEnumerable<RessourcePedagogique>> GetByDateCreationAsync(DateTime date)
        {
            return await _context.RessourcePedagogiques
                .Include(r => r.Cours)
                .Where(r => r.DateCreation.Date == date.Date && r.Statut == true) // ✅ Filtrer actifs
                .OrderByDescending(r => r.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<RessourcePedagogique>> GetByDateRangeAsync(DateTime dateDebut, DateTime dateFin)
        {
            return await _context.RessourcePedagogiques
                .Where(r => r.DateCreation >= dateDebut && r.DateCreation <= dateFin && r.Statut == true) // ✅ Filtrer actifs
                .OrderByDescending(r => r.DateCreation)
                .ToListAsync();
        }

        public async Task<RessourcePedagogique> CreateAsync(RessourcePedagogique ressource)
        {
            ressource.DateCreation = DateTime.Now;
            
            _context.RessourcePedagogiques.Add(ressource);
            await _context.SaveChangesAsync();
            return ressource;
        }

        public async Task<RessourcePedagogique> UpdateAsync(RessourcePedagogique ressource)
        {
            var existingRessource = await _context.RessourcePedagogiques.FindAsync(ressource.IdRessourcePedagogique);
            if (existingRessource == null)
                return null;

            _context.Entry(existingRessource).CurrentValues.SetValues(ressource);
            await _context.SaveChangesAsync();
            return existingRessource;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var ressource = await _context.RessourcePedagogiques.FindAsync(id);
            if (ressource == null)
                return false;

            _context.RessourcePedagogiques.Remove(ressource);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.RessourcePedagogiques.AnyAsync(r => r.IdRessourcePedagogique == id);
        }

        //public async Task<bool> ExistsByTitreAsync(string titre)
        //{
        //    return await _context.RessourcePedagogiques.AnyAsync(r => r.Titre == titre);
        //}

        // ✅ SOFT DELETE: Toggle le statut d'une ressource pédagogique (actif <-> inactif)
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var ressource = await _context.RessourcePedagogiques.FindAsync(id);
            if (ressource == null)
                return false;

            ressource.Statut = ressource.Statut != true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
