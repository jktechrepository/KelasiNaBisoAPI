using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class DirectionService : IDirectionRepository
    {
        private readonly KelasiNaBisoDbContext _context;

        public DirectionService(KelasiNaBisoDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Direction>> GetAllAsync()
        {
            return await _context.Directions
              //  .Include(d => d.Ecole)
              //  .Include(d => d.Classes)
                .Where(d => d.Statut == true) // ✅ Filtrer uniquement les directions actives
                .OrderBy(d => d.NomDirection)
                .ToListAsync();
        }

        public async Task<Direction> GetByIdAsync(int id)
        {
            return await _context.Directions
              //  .Include(d => d.Ecole)
              //  .Include(d => d.Classes)
                .FirstOrDefaultAsync(d => d.IdDirection == id);
        }

        public async Task<Direction> GetByNomAsync(string nom)
        {
            return await _context.Directions
              //  .Include(d => d.Ecole)
              //  .Include(d => d.Classes)
                .FirstOrDefaultAsync(d => d.NomDirection == nom);
        }

        public async Task<IEnumerable<Direction>> GetByEcoleAsync(int idEcole)
        {
            return await _context.Directions
              //  .Include(d => d.Classes)
                .Where(d => d.IdEcole == idEcole && d.Statut == true) // ✅ Filtrer actifs
                .OrderBy(d => d.NomDirection)
                .ToListAsync();
        }

        public async Task<Direction> CreateAsync(Direction direction)
        {
            direction.DateCreation = DateTime.Now;
            
            _context.Directions.Add(direction);
            await _context.SaveChangesAsync();
            return direction;
        }

        public async Task<Direction> UpdateAsync(Direction direction)
        {
            var existingDirection = await _context.Directions.FindAsync(direction.IdDirection);
            if (existingDirection == null)
                return null;

            _context.Entry(existingDirection).CurrentValues.SetValues(direction);
            await _context.SaveChangesAsync();
            return existingDirection;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var direction = await _context.Directions.FindAsync(id);
            if (direction == null)
                return false;

            _context.Directions.Remove(direction);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Directions.AnyAsync(d => d.IdDirection == id);
        }

        public async Task<bool> ExistsByNomAsync(string nom)
        {
            return await _context.Directions.AnyAsync(d => d.NomDirection == nom);
        }

        public async Task<bool> ExistsByNomAndEcoleAsync(string nom, int idEcole)
        {
            return await _context.Directions.AnyAsync(d => d.NomDirection == nom && d.IdEcole == idEcole);
        }

        public async Task<IEnumerable<Classe>> GetClassesAsync(int idDirection)
        {
            return await _context.Classes
               // .Include(c => c.Section)
               // .Include(c => c.Option)
                .Where(c => c.IdDirection == idDirection)
                .OrderBy(c => c.NomClasse)
                .ToListAsync();
        }

        // ✅ SOFT DELETE: Toggle le statut d'une direction (actif <-> inactif)
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var direction = await _context.Directions.FindAsync(id);
            if (direction == null)
                return false;

            direction.Statut = direction.Statut != true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
