using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class V_EleveService : IV_EleveRepository
    {
        private readonly KelasiNaBisoDbContext _context;

        public V_EleveService(KelasiNaBisoDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<V_Eleve>> GetAllAsync()
        {
            return await _context.V_Eleves
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<V_Eleve> GetByIdAsync(int id)
        {
            return await _context.V_Eleves
                .FirstOrDefaultAsync(v => v.IdEleve == id);
        }

        public async Task<V_Eleve> GetByReferenceAsync(Guid reference)
        {
            return await _context.V_Eleves
                .FirstOrDefaultAsync(v => v.ReferenceEleve == reference);
        }

        public async Task<V_Eleve> GetByMatriculeAsync(string matricule)
        {
            return await _context.V_Eleves
                .FirstOrDefaultAsync(v => v.Matricule == matricule);
        }

        public async Task<IEnumerable<V_Eleve>> GetByEcoleAsync(int idEcole)
        {
            return await _context.V_Eleves
                .Where(v => v.IdEcole == idEcole)
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<V_Eleve>> GetByClasseAsync(int idClasse)
        {
            return await _context.V_Eleves
                .Where(v => v.IdClasse == idClasse)
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<V_Eleve>> GetByTuteurAsync(int idTuteur)
        {
            return await _context.V_Eleves
                .Where(v => v.IdTuteur == idTuteur)
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<V_Eleve>> GetByStatutAsync(bool statut)
        {
            return await _context.V_Eleves
                .Where(v => v.Statut == statut)
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<V_Eleve>> GetByGenreAsync(string genre)
        {
            return await _context.V_Eleves
                .Where(v => v.Genre == genre)
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<V_Eleve>> GetByNationaliteAsync(string nationalite)
        {
            return await _context.V_Eleves
                .Where(v => v.Nationalite == nationalite)
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<V_Eleve>> GetByProvinceAsync(string province)
        {
            return await _context.V_Eleves
                .Where(v => v.Province == province)
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<V_Eleve>> GetByVilleAsync(string ville)
        {
            return await _context.V_Eleves
                .Where(v => v.Ville == ville)
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<V_Eleve>> GetByCommuneAsync(string commune)
        {
            return await _context.V_Eleves
                .Where(v => v.Commune == commune)
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<V_Eleve>> GetByDateCreationAsync(DateTime date)
        {
            return await _context.V_Eleves
                .Where(v => v.DateCreation.Date == date.Date)
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<V_Eleve>> GetByDateCreationRangeAsync(DateTime dateDebut, DateTime dateFin)
        {
            return await _context.V_Eleves
                .Where(v => v.DateCreation >= dateDebut && v.DateCreation <= dateFin)
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<V_Eleve>> GetByDateNaissanceAsync(DateTime dateNaissance)
        {
            return await _context.V_Eleves
                .Where(v => v.DateNaissance.Date == dateNaissance.Date)
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<V_Eleve>> GetByDateNaissanceRangeAsync(DateTime dateDebut, DateTime dateFin)
        {
            return await _context.V_Eleves
                .Where(v => v.DateNaissance >= dateDebut && v.DateNaissance <= dateFin)
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<V_Eleve>> GetByNomCompletAsync(string nomComplet)
        {
            return await _context.V_Eleves
                .Where(v => v.NomComplet.Contains(nomComplet))
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<V_Eleve>> GetByNomAsync(string nom)
        {
            return await _context.V_Eleves
                .Where(v => v.Nom.Contains(nom))
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<V_Eleve>> GetByPrenomAsync(string prenom)
        {
            return await _context.V_Eleves
                .Where(v => v.Prenom.Contains(prenom))
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<V_Eleve>> GetByLieuNaissanceAsync(string lieuNaissance)
        {
            return await _context.V_Eleves
                .Where(v => v.LieuNaissance.Contains(lieuNaissance))
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<V_Eleve>> GetBySectionAsync(int idSection)
        {
            return await _context.V_Eleves
                .Where(v => v.IdSection == idSection)
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<V_Eleve>> GetByOptionAsync(int idOption)
        {
            return await _context.V_Eleves
                .Where(v => v.IdOption == idOption)
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<V_Eleve>> GetByStatutTuteurAsync(bool statutTuteur)
        {
            return await _context.V_Eleves
                .Where(v => v.StatutTuteur == statutTuteur)
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<V_Eleve>> GetByEmailTuteurAsync(string emailTuteur)
        {
            return await _context.V_Eleves
                .Where(v => v.EmailTuteur == emailTuteur)
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<V_Eleve>> GetByTelephoneTuteurAsync(string telephoneTuteur)
        {
            return await _context.V_Eleves
                .Where(v => v.TelephoneTuteur == telephoneTuteur)
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.V_Eleves.AnyAsync(v => v.IdEleve == id);
        }

        public async Task<bool> ExistsByReferenceAsync(Guid reference)
        {
            return await _context.V_Eleves.AnyAsync(v => v.ReferenceEleve == reference);
        }

        public async Task<bool> ExistsByMatriculeAsync(string matricule)
        {
            return await _context.V_Eleves.AnyAsync(v => v.Matricule == matricule);
        }

        public async Task<int> GetCountAsync()
        {
            return await _context.V_Eleves.CountAsync();
        }

        public async Task<int> GetCountByEcoleAsync(int idEcole)
        {
            return await _context.V_Eleves.CountAsync(v => v.IdEcole == idEcole);
        }

        public async Task<int> GetCountByClasseAsync(int idClasse)
        {
            return await _context.V_Eleves.CountAsync(v => v.IdClasse == idClasse);
        }

        public async Task<int> GetCountByTuteurAsync(int idTuteur)
        {
            return await _context.V_Eleves.CountAsync(v => v.IdTuteur == idTuteur);
        }

        public async Task<int> GetCountByStatutAsync(bool statut)
        {
            return await _context.V_Eleves.CountAsync(v => v.Statut == statut);
        }

        public async Task<int> GetCountByGenreAsync(string genre)
        {
            return await _context.V_Eleves.CountAsync(v => v.Genre == genre);
        }
    }
}
