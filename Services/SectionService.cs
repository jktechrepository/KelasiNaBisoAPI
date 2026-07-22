using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class SectionService : ISectionRepository
    {
        private readonly KelasiNaBisoDbContext _context;

        public SectionService(KelasiNaBisoDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Section>> GetAllAsync()
        {
            return await _context.Sections
                .Include(s => s.Ecole)
                .Include(s => s.Options)
                .Include(s => s.Classes)
                .Where(s => s.Statut == true) // ✅ Filtrer uniquement les sections actives
                .OrderBy(s => s.NomSection)
                .ToListAsync();
        }

        public async Task<Section> GetByIdAsync(int id)
        {
            return await _context.Sections
                .Include(s => s.Ecole)
                .Include(s => s.Options)
                .Include(s => s.Classes)
                .FirstOrDefaultAsync(s => s.IdSection == id);
        }

        public async Task<Section> GetByNomAsync(string nom)
        {
            return await _context.Sections
                .Include(s => s.Ecole)
                .Include(s => s.Options)
                .Include(s => s.Classes)
                .FirstOrDefaultAsync(s => s.NomSection == nom);
        }

        public async Task<IEnumerable<Section>> GetByEcoleAsync(int idEcole)
        {
            return await _context.Sections
                .Include(s => s.Options)
                .Include(s => s.Classes)
                .Where(s => s.IdEcole == idEcole && s.Statut == true) // ✅ Filtrer actifs
                .OrderBy(s => s.NomSection)
                .ToListAsync();
        }

        public async Task<IEnumerable<Section>> GetByCycleEnseignementAsync(string cycle)
        {
            return await _context.Sections
                .Include(s => s.Ecole)
                .Include(s => s.Options)
                .Include(s => s.Classes)
                .Where(s => s.Statut == true) // ✅ Filtrer actifs
                .OrderBy(s => s.NomSection)
                .ToListAsync();
        }

        public async Task<Section> CreateAsync(Section section)
        {
            section.DateCreation = DateTime.Now;
            
            _context.Sections.Add(section);
            await _context.SaveChangesAsync();
            return section;
        }

        public async Task<Section> UpdateAsync(Section section)
        {
            var existingSection = await _context.Sections.FindAsync(section.IdSection);
            if (existingSection == null)
                return null;

            _context.Entry(existingSection).CurrentValues.SetValues(section);
            await _context.SaveChangesAsync();
            return existingSection;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var section = await _context.Sections.FindAsync(id);
            if (section == null)
                return false;

            _context.Sections.Remove(section);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Sections.AnyAsync(s => s.IdSection == id);
        }

        public async Task<bool> ExistsByNomAsync(string nom)
        {
            return await _context.Sections.AnyAsync(s => s.NomSection == nom);
        }

        public async Task<IEnumerable<Option>> GetOptionsAsync(int idSection)
        {
            return await _context.Options
                .Include(o => o.Classes)
                .Where(o => o.IdSection == idSection)
                .OrderBy(o => o.NomOption)
                .ToListAsync();
        }

        public async Task<IEnumerable<Classe>> GetClassesAsync(int idSection)
        {
            return await _context.Classes
               // .Include(c => c.Ecole)
                .Include(c => c.Option)
                .Where(c => c.IdSection == idSection)
                .OrderBy(c => c.NomClasse)
                .ToListAsync();
        }

        // ✅ SOFT DELETE: Toggle le statut d'une section (actif <-> inactif)
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var section = await _context.Sections.FindAsync(id);
            if (section == null)
                return false;

            section.Statut = section.Statut != true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
