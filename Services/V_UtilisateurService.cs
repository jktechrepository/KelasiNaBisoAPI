using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class V_UtilisateurService : IV_UtilisateurRepository
    {
        private readonly KelasiNaBisoDbContext _context;

        public V_UtilisateurService(KelasiNaBisoDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<V_Utilisateur>> GetAllAsync()
        {
            return await _context.V_Utilisateurs
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<V_Utilisateur> GetByIdAsync(int id)
        {
            return await _context.V_Utilisateurs
                .FirstOrDefaultAsync(v => v.IdUtilisateur == id);
        }

        public async Task<V_Utilisateur> GetByReferenceAsync(Guid reference)
        {
            return await _context.V_Utilisateurs
                .FirstOrDefaultAsync(v => v.ReferenceUtilisateur == reference);
        }

        public async Task<V_Utilisateur> GetByEmailAsync(string email)
        {
            return await _context.V_Utilisateurs
                .FirstOrDefaultAsync(v => v.Email == email);
        }

        public async Task<IEnumerable<V_Utilisateur>> GetByEcoleAsync(int idEcole)
        {
            return await _context.V_Utilisateurs
                .Where(v => v.IdEcole == idEcole)
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<V_Utilisateur>> GetByRoleAsync(int idRole)
        {
            return await _context.V_Utilisateurs
                .Where(v => v.IdRole == idRole)
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<V_Utilisateur>> GetByRoleNameAsync(string nomRole)
        {
            return await _context.V_Utilisateurs
                .Where(v => v.NomRole == nomRole)
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<V_Utilisateur>> GetByStatutAsync(bool statut)
        {
            return await _context.V_Utilisateurs
                .Where(v => v.Statut == statut)
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<V_Utilisateur>> GetByConnecteAsync(bool isConnecte)
        {
            return await _context.V_Utilisateurs
                .Where(v => v.IsConnecte == isConnecte)
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<V_Utilisateur>> GetByProvinceAsync(string province)
        {
            return await _context.V_Utilisateurs
                .Where(v => v.Province == province)
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<V_Utilisateur>> GetByVilleAsync(string ville)
        {
            return await _context.V_Utilisateurs
                .Where(v => v.Ville == ville)
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<V_Utilisateur>> GetByCommuneAsync(string commune)
        {
            return await _context.V_Utilisateurs
                .Where(v => v.Commune == commune)
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<V_Utilisateur>> GetByDateCreationAsync(DateTime date)
        {
            return await _context.V_Utilisateurs
                .Where(v => v.DateCreation.Date == date.Date)
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<V_Utilisateur>> GetByDateCreationRangeAsync(DateTime dateDebut, DateTime dateFin)
        {
            return await _context.V_Utilisateurs
                .Where(v => v.DateCreation >= dateDebut && v.DateCreation <= dateFin)
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<V_Utilisateur>> GetByNomCompletAsync(string nomComplet)
        {
            return await _context.V_Utilisateurs
                .Where(v => (v.NomUtilisateur + " " + v.PostNomUtilisateur + " " + v.PrenomUtilisateur)
                    .Contains(nomComplet, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<V_Utilisateur>> GetByTelephoneAsync(string telephone)
        {
            return await _context.V_Utilisateurs
                .Where(v => v.Telephone == telephone)
                .OrderByDescending(v => v.DateCreation)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.V_Utilisateurs.AnyAsync(v => v.IdUtilisateur == id);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.V_Utilisateurs.AnyAsync(v => v.Email == email);
        }

        public async Task<bool> ExistsByReferenceAsync(Guid reference)
        {
            return await _context.V_Utilisateurs.AnyAsync(v => v.ReferenceUtilisateur == reference);
        }

        public async Task<int> GetCountAsync()
        {
            return await _context.V_Utilisateurs.CountAsync();
        }

        public async Task<int> GetCountByEcoleAsync(int idEcole)
        {
            return await _context.V_Utilisateurs.CountAsync(v => v.IdEcole == idEcole);
        }

        public async Task<int> GetCountByRoleAsync(int idRole)
        {
            return await _context.V_Utilisateurs.CountAsync(v => v.IdRole == idRole);
        }

        public async Task<int> GetCountByStatutAsync(bool statut)
        {
            return await _context.V_Utilisateurs.CountAsync(v => v.Statut == statut);
        }
    }
}
