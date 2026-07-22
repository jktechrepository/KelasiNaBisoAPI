using KelasiNaBiso.Data;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class EleveParEcoleService : IEleveParEcoleRepository
    {
        private readonly KelasiNaBisoDbContext _context;

        public EleveParEcoleService(KelasiNaBisoDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EleveParEcoleDTO>> GetAllAsync()
        {
            return await _context.EleveParEcole
                .OrderBy(e => e.NomCompletEleve)
                .ToListAsync();
        }

        public async Task<EleveParEcoleDTO?> GetByIdAsync(int idEleve)
        {
            return await _context.EleveParEcole
                .FirstOrDefaultAsync(e => e.IdEleve == idEleve);
        }

        public async Task<IEnumerable<EleveParEcoleDTO>> GetByEcoleAsync(int idEcole)
        {
            return await _context.EleveParEcole
                .Where(e => e.IdEcole == idEcole)
                .OrderBy(e => e.NomCompletEleve)
                .ToListAsync();
        }

        public async Task<IEnumerable<EleveParEcoleDTO>> GetByClasseAsync(int idClasse)
        {
            return await _context.EleveParEcole
                .Where(e => e.IdClasse == idClasse)
                .OrderBy(e => e.NomCompletEleve)
                .ToListAsync();
        }

        public async Task<IEnumerable<EleveParEcoleDTO>> GetByDirectionAsync(int idDirection)
        {
            return await _context.EleveParEcole
                .Where(e => e.IdDirection == idDirection)
                .OrderBy(e => e.NomCompletEleve)
                .ToListAsync();
        }

        public async Task<IEnumerable<EleveParEcoleDTO>> GetByOptionAsync(int idOption)
        {
            return await _context.EleveParEcole
                .Where(e => e.IdOption == idOption)
                .OrderBy(e => e.NomCompletEleve)
                .ToListAsync();
        }

        public async Task<IEnumerable<EleveParEcoleDTO>> GetByTuteurAsync(int idTuteur)
        {
            return await _context.EleveParEcole
                .Where(e => e.IdTuteur == idTuteur)
                .OrderBy(e => e.NomCompletEleve)
                .ToListAsync();
        }

        public async Task<IEnumerable<EleveParEcoleDTO>> GetByStatutAsync(bool statut)
        {
            return await _context.EleveParEcole
                .Where(e => e.Statut == statut)
                .OrderBy(e => e.NomCompletEleve)
                .ToListAsync();
        }

        public async Task<IEnumerable<EleveParEcoleDTO>> GetByGenreAsync(string genre)
        {
            return await _context.EleveParEcole
                .Where(e => e.Genre == genre)
                .OrderBy(e => e.NomCompletEleve)
                .ToListAsync();
        }

        public async Task<IEnumerable<EleveParEcoleDTO>> GetByAgeRangeAsync(int minAge, int maxAge)
        {
            return await _context.EleveParEcole
                .Where(e => e.Age >= minAge && e.Age <= maxAge)
                .OrderBy(e => e.NomCompletEleve)
                .ToListAsync();
        }

        public async Task<IEnumerable<EleveParEcoleDTO>> GetByProvinceAsync(string province)
        {
            return await _context.EleveParEcole
                .Where(e => e.ProvinceEleve == province)
                .OrderBy(e => e.NomCompletEleve)
                .ToListAsync();
        }

        public async Task<IEnumerable<EleveParEcoleDTO>> GetByVilleAsync(string ville)
        {
            return await _context.EleveParEcole
                .Where(e => e.VilleEleve == ville)
                .OrderBy(e => e.NomCompletEleve)
                .ToListAsync();
        }

        public async Task<IEnumerable<EleveParEcoleDTO>> GetByCommuneAsync(string commune)
        {
            return await _context.EleveParEcole
                .Where(e => e.CommuneEleve == commune)
                .OrderBy(e => e.NomCompletEleve)
                .ToListAsync();
        }

        public async Task<IEnumerable<EleveParEcoleDTO>> GetByTuteurContactAsync(string contact)
        {
            return await _context.EleveParEcole
                .Where(e => e.TelephoneTuteur == contact ||
                           e.EmailTuteur == contact ||
                           e.TelephoneRepresentant == contact)
                .OrderBy(e => e.NomCompletEleve)
                .ToListAsync();
        }

        public async Task<IEnumerable<EleveParEcoleDTO>> SearchAsync(string searchTerm)
        {
            return await _context.EleveParEcole
                .Where(e => e.NomCompletEleve.Contains(searchTerm) ||
                           e.Matricule.Contains(searchTerm) ||
                           e.NomEcole.Contains(searchTerm) ||
                           e.NomClasse.Contains(searchTerm))
                .OrderBy(e => e.NomCompletEleve)
                .ToListAsync();
        }

        public async Task<int> GetCountByEcoleAsync(int idEcole)
        {
            return await _context.EleveParEcole
                .CountAsync(e => e.IdEcole == idEcole);
        }

        public async Task<int> GetCountByClasseAsync(int idClasse)
        {
            return await _context.EleveParEcole
                .CountAsync(e => e.IdClasse == idClasse);
        }

        public async Task<int> GetCountByDirectionAsync(int idDirection)
        {
            return await _context.EleveParEcole
                .CountAsync(e => e.IdDirection == idDirection);
        }
    }
}
