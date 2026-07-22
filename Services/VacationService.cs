using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class VacationService : IVacationRepository
    {
        private readonly KelasiNaBisoDbContext _context;

        public VacationService(KelasiNaBisoDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Vacation>> GetAllAsync()
        {
            return await _context.Vacations
                .Where(v => v.Statut == true) // ✅ Filtrer uniquement les vacations actives
               // .ThenBy(h => h.HeureDebut)
                .ToListAsync();
        }

        public async Task<Vacation> GetByIdAsync(int id)
        {
            return await _context.Vacations
                .FirstOrDefaultAsync(h => h.IdVacation == id);
        }

        public async Task<IEnumerable<Vacation>> GetByEcoleAsync(int idEcole)
        {
            return await _context.Vacations
                .Where(v => v.IdEcole == idEcole && v.Statut == true) // ✅ Filtrer actifs
                .ToListAsync();
        }

        public async Task<IEnumerable<Vacation>> GetByCoursAsync(int idCours)
        {
            return await _context.Vacations
                .Where(v => v.Statut == true) // ✅ Filtrer actifs
                .ToListAsync();
        }

        public async Task<IEnumerable<Vacation>> GetByJourAsync(int Nbjour)
        {
            return await _context.Vacations
                .Where(v => v.Statut == true) // ✅ Filtrer actifs
                .OrderBy(h => h.HeureDebut)
                .ToListAsync();
        }

        public async Task<IEnumerable<Vacation>> GetByHeureDebutAsync(TimeSpan heureDebut)
        {
            return await _context.Vacations
                .Where(v => v.Statut == true) // ✅ Filtrer actifs
                .ToListAsync();
        }

        public async Task<IEnumerable<Vacation>> GetByHeureFinAsync(TimeSpan heureFin)
        {
            return await _context.Vacations
                .Where(v => v.Statut == true) // ✅ Filtrer actifs
                .ToListAsync();
        }

        public async Task<IEnumerable<Vacation>> GetByDateRangeAsync(DateTime dateDebut, DateTime dateFin)
        {
            return await _context.Vacations
                .Where(h => h.DateCreation >= dateDebut && h.DateCreation <= dateFin && h.Statut == true) // ✅ Filtrer actifs
                .ToListAsync();
        }

        public async Task<Vacation> CreateAsync(Vacation Vacation)
        {
            Vacation.DateCreation = DateTime.Now;
            
            _context.Vacations.Add(Vacation);
            await _context.SaveChangesAsync();
            return Vacation;
        }

        public async Task<Vacation> UpdateAsync(Vacation Vacation)
        {
            var existingVacation = await _context.Vacations.FindAsync(Vacation.IdVacation);
            if (existingVacation == null)
                return null;

            _context.Entry(existingVacation).CurrentValues.SetValues(Vacation);
            await _context.SaveChangesAsync();
            return existingVacation;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var Vacation = await _context.Vacations.FindAsync(id);
            if (Vacation == null)
                return false;

            _context.Vacations.Remove(Vacation);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Vacations.AnyAsync(h => h.IdVacation == id);
        }

        // ✅ SOFT DELETE: Toggle le statut d'une vacation (actif <-> inactif)
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var vacation = await _context.Vacations.FindAsync(id);
            if (vacation == null)
                return false;

            vacation.Statut = vacation.Statut != true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
