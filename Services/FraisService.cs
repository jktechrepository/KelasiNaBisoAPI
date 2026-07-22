using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class FraisService : IFraisRepository
    {
        private readonly KelasiNaBisoDbContext _context;

        public FraisService(KelasiNaBisoDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Frais>> GetAllAsync()
        {
            return await _context.Frais
               // .Include(f => f.Direction)
               // .Include(f => f.Paiements)
                .Where(f => f.Statut == true) // ✅ Filtrer uniquement les frais actifs
                .OrderByDescending(f => f.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<Frais>> GetByEcoleAsync(int idEcole)
        {
            return await _context.Frais
                .Where(f => f.Direction.IdEcole == idEcole && f.Statut == true) // ✅ Filtrer actifs
                .OrderByDescending(f => f.DateCreation)
                .ToListAsync();
        }

        public async Task<Frais> GetByIdAsync(int id)
        {
            return await _context.Frais
                .Include(f => f.Direction)
                .Include(f => f.Paiements)
                .FirstOrDefaultAsync(f => f.IdFrais == id);
        }

        public async Task<Frais> GetByLibelleFraisAsync(string libelleFrais)
        {
            return await _context.Frais
                .Include(f => f.Direction)
                .Include(f => f.Paiements)
                .FirstOrDefaultAsync(f => f.LibelleFrais == libelleFrais);
        }

        public async Task<Frais> GetByEcoleAndLibelleAsync(int idEcole, string libelleFrais)
        {
            return await _context.Frais
                .Include(f => f.Direction)
                .Where(f => f.Direction.IdEcole == idEcole && 
                           f.LibelleFrais.ToLower() == libelleFrais.ToLower() && 
                           f.Statut == true)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Frais>> GetByDirectionAsync(int idDirection)
        {
            return await _context.Frais
                .Include(f => f.Paiements)
                .Where(f => f.IdDirection == idDirection && f.Statut == true) // ✅ Filtrer actifs
                .OrderByDescending(f => f.DateCreation)
                .ToListAsync();
        }


        //public async Task<IEnumerable<Frais>> GetByTypeAsync(string type)
        //{
        //    return await _context.Frais
        //        .Include(f => f.Classe)
        //        .Include(f => f.Paiements)
        //        .Where(f => f.Type == type)
        //        .OrderByDescending(f => f.DateCreation)
        //        .ToListAsync();
        //}

        public async Task<IEnumerable<Frais>> GetByMontantAsync(double montant)
        {
            return await _context.Frais
                .Include(f => f.Direction)
                .Include(f => f.Paiements)
                .Where(f => f.Montant == montant && f.Statut == true) // ✅ Filtrer actifs
                .OrderByDescending(f => f.DateCreation)
                .ToListAsync();
        }

        //public async Task<IEnumerable<Frais>> GetByStatutAsync(bool statut)
        //{
        //    return await _context.Frais
        //        .Include(f => f.Classe)
        //        .Include(f => f.Paiements)
        //        .Where(f => f.Statut == statut)
        //        .OrderByDescending(f => f.DateCreation)
        //        .ToListAsync();
        //}

        public async Task<Frais> CreateAsync(Frais frais)
        {
            frais.DateCreation = DateTime.Now;
            
            _context.Frais.Add(frais);
            await _context.SaveChangesAsync();
            return frais;
        }

        public async Task<Frais> UpdateAsync(Frais frais)
        {
            var existingFrais = await _context.Frais.FindAsync(frais.IdFrais);
            if (existingFrais == null)
                return null;

            _context.Entry(existingFrais).CurrentValues.SetValues(frais);
            await _context.SaveChangesAsync();
            return existingFrais;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var frais = await _context.Frais.FindAsync(id);
            if (frais == null)
                return false;

            _context.Frais.Remove(frais);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Frais.AnyAsync(f => f.IdFrais == id);
        }

        public async Task<bool> ExistsByLibelleFraisAsync(string libelleFrais)
        {
            return await _context.Frais.AnyAsync(f => f.LibelleFrais == libelleFrais);
        }

        public async Task<IEnumerable<Paiement>> GetPaiementsAsync(int idFrais)
        {
            return await _context.Paiements
                .Include(p => p.Eleve)
                .Include(p => p.Utilisateur)
                .Where(p => p.IdFrais == idFrais)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        // ✅ SOFT DELETE: Toggle le statut d'un frais (actif <-> inactif)
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var frais = await _context.Frais.FindAsync(id);
            if (frais == null)
                return false;

            frais.Statut = frais.Statut != true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
