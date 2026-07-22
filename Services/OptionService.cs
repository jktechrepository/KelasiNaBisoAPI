using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class OptionService : IOptionRepository
    {
        private readonly KelasiNaBisoDbContext _context;

        public OptionService(KelasiNaBisoDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Option>> GetAllAsync()
        {
            return await _context.Options
                .Include(o => o.Section)
                .Include(o => o.Classes)
                .Where(o => o.Statut == true) // ✅ Filtrer uniquement les options actives
                .OrderBy(o => o.NomOption)
                .ToListAsync();
        }

        public async Task<Option> GetByIdAsync(int id)
        {
            return await _context.Options
                .Include(o => o.Section)
                .Include(o => o.Classes)
                .FirstOrDefaultAsync(o => o.IdOption == id);
        }

        public async Task<Option> GetByNomAsync(string nom)
        {
            return await _context.Options
                .Include(o => o.Section)
                .Include(o => o.Classes)
                .FirstOrDefaultAsync(o => o.NomOption == nom);
        }

        public async Task<IEnumerable<Option>> GetBySectionAsync(int idSection)
        {
            return await _context.Options
                .Include(o => o.Classes)
                .Where(o => o.IdSection == idSection && o.Statut == true) // ✅ Filtrer actifs
                .OrderBy(o => o.NomOption)
                .ToListAsync();
        }

        public async Task<Option> CreateAsync(Option option)
        {
            option.DateCreation = DateTime.Now;
            
            _context.Options.Add(option);
            await _context.SaveChangesAsync();
            return option;
        }

        public async Task<Option> UpdateAsync(Option option)
        {
            var existingOption = await _context.Options.FindAsync(option.IdOption);
            if (existingOption == null)
                return null;

            _context.Entry(existingOption).CurrentValues.SetValues(option);
            await _context.SaveChangesAsync();
            return existingOption;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var option = await _context.Options.FindAsync(id);
            if (option == null)
                return false;

            _context.Options.Remove(option);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Options.AnyAsync(o => o.IdOption == id);
        }

        public async Task<bool> ExistsByNomAsync(string nom)
        {
            return await _context.Options.AnyAsync(o => o.NomOption == nom);
        }

        public async Task<IEnumerable<Classe>> GetClassesAsync(int idOption)
        {
            return await _context.Classes
               // .Include(c => c.Ecole)
                .Include(c => c.Section)
                .Where(c => c.IdOption == idOption)
                .OrderBy(c => c.NomClasse)
                .ToListAsync();
        }

        // ✅ SOFT DELETE: Toggle le statut d'une option (actif <-> inactif)
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var option = await _context.Options.FindAsync(id);
            if (option == null)
                return false;

            option.Statut = option.Statut != true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
