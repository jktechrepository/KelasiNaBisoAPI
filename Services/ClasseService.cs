using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.Pagination;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Extensions;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class ClasseService : IClasseRepository
    {
        private readonly KelasiNaBisoDbContext _context;

        public ClasseService(KelasiNaBisoDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Classe>> GetAllPagedAsync(PagedRequest request)
        {
            var query = _context.Classes
                .Include(c => c.Direction)
                    .ThenInclude(d => d.Ecole)
                .Include(c => c.Section)
                .Include(c => c.Option)
                .AsQueryable();

            // Filtrer par statut
            if (!request.IncludeInactive)
            {
                query = query.Where(c => c.Statut == true);
            }

            // Appliquer la recherche si présente
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(c =>
                    (c.NomClasse != null && c.NomClasse.ToLower().Contains(searchLower)) ||
                    (c.Direction != null && c.Direction.Ecole != null && c.Direction.Ecole.Nom != null && c.Direction.Ecole.Nom.ToLower().Contains(searchLower)) ||
                    (c.Section != null && c.Section.NomSection != null && c.Section.NomSection.ToLower().Contains(searchLower)) ||
                    (c.Option != null && c.Option.NomOption != null && c.Option.NomOption.ToLower().Contains(searchLower))
                );
            }

            // Appliquer le tri
            if (!string.IsNullOrWhiteSpace(request.SortBy))
            {
                query = query.ApplySort(request.SortBy, request.SortDescending);
            }
            else
            {
                // Tri par défaut : NomClasse
                query = request.SortDescending
                    ? query.OrderByDescending(c => c.NomClasse)
                    : query.OrderBy(c => c.NomClasse);
            }

            return await query.ToPagedAsync(request);
        }

        public async Task<IEnumerable<Classe>> GetAllAsync()
        {
            return await _context.Classes
                //.Include(c => c.Ecole)
                //.Include(c => c.Section)
                //.Include(c => c.Option)
                //.Include(c => c.Eleves)
                //.Include(c => c.Cours)
                //.Include(c => c.Inscriptions)
                //.Include(c => c.Frais)
                //.Include(c => c.Vacations)
                //.Include(c => c.Evaluations)
                .Where(c => c.Statut == true) // ✅ Filtrer uniquement les classes actives
                .ToListAsync();
        }

        public async Task<Classe> GetByIdAsync(int id)
        {
            return await _context.Classes
                //.Include(c => c.Ecole)
                //.Include(c => c.Section)
                //.Include(c => c.Option)
                //.Include(c => c.Eleves)
                //.Include(c => c.Cours)
                //.Include(c => c.Inscriptions)
                //.Include(c => c.Frais)
                //.Include(c => c.Vacations)
                //.Include(c => c.Evaluations)
                .Where(c => c.Statut == true) // ✅ Filtrer uniquement les classes actives
                .FirstOrDefaultAsync(c => c.IdClasse == id);
        }

        public async Task<Classe> GetByNomAsync(string nom)
        {
            return await _context.Classes
                //.Include(c => c.Ecole)
                //.Include(c => c.Section)
                //.Include(c => c.Option)
                //.Include(c => c.Eleves)
                //.Include(c => c.Cours)
                //.Include(c => c.Inscriptions)
                //.Include(c => c.Frais)
                //.Include(c => c.Vacations)
                //.Include(c => c.Evaluations)
                .FirstOrDefaultAsync(c => c.NomClasse == nom);
        }

        public async Task<Classe> GetByEcoleAndNomAsync(int idEcole, string nomClasse)
        {
            return await _context.Classes
                .Include(c => c.Direction)
                .Where(c => c.Direction.IdEcole == idEcole && 
                           c.NomClasse.ToLower() == nomClasse.ToLower() && 
                           c.Statut == true)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Classe>> GetByEcoleAsync(int idEcole)
        {
            return await _context.Classes
                 .Include(c => c.Direction)
                .Where(c => c.Direction.IdEcole == idEcole)
                .ToListAsync();
        }

        public async Task<IEnumerable<Classe>> GetBySectionAsync(int idSection)
        {
            return await _context.Classes
                .Where(c => c.IdSection == idSection)
                .ToListAsync();
        }

        public async Task<IEnumerable<Classe>> GetByOptionAsync(int idOption)
        {
            return await _context.Classes
                //.Include(c => c.Ecole)
                //.Include(c => c.Section)
                //.Include(c => c.Eleves)
                //.Include(c => c.Cours)
                //.Include(c => c.Inscriptions)
                //.Include(c => c.Frais)
                //.Include(c => c.Vacations)
                //.Include(c => c.Evaluations)
                .Where(c => c.IdOption == idOption)
                .ToListAsync();
        }

        public async Task<IEnumerable<Classe>> GetWithoutSectionAsync()
        {
            return await _context.Classes
                .Where(c => c.IdSection == null)
                .ToListAsync();
        }

        public async Task<IEnumerable<Classe>> GetWithoutOptionAsync()
        {
            return await _context.Classes
                .Where(c => c.IdOption == null)
                .ToListAsync();
        }

        //public async Task<IEnumerable<Classe>> GetByStatutAsync(bool statut)
        //{
        //    return await _context.Classes
        //        .Include(c => c.Ecole)
        //        .Include(c => c.Section)
        //        .Include(c => c.Option)
        //        .Include(c => c.Eleves)
        //        .Include(c => c.Cours)
        //        .Include(c => c.Inscriptions)
        //        .Include(c => c.Frais)
        //        .Include(c => c.Vacations)
        //        .Include(c => c.Evaluations)
        //        .Where(c => c.Statut == statut)
        //        .ToListAsync();
        //}

        public async Task<Classe> CreateAsync(Classe classe)
        {
            classe.DateCreation = DateTime.Now;
            
            _context.Classes.Add(classe);
            await _context.SaveChangesAsync();
            return classe;
        }

        public async Task<Classe> UpdateAsync(Classe classe)
        {
            var existingClasse = await _context.Classes.FindAsync(classe.IdClasse);
            if (existingClasse == null)
                return null;

            _context.Entry(existingClasse).CurrentValues.SetValues(classe);
            await _context.SaveChangesAsync();
            return existingClasse;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var classe = await _context.Classes.FindAsync(id);
            if (classe == null)
                return false;

            _context.Classes.Remove(classe);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Classes.AnyAsync(c => c.IdClasse == id);
        }

        public async Task<bool> ExistsByNomAsync(string nom)
        {
            return await _context.Classes.AnyAsync(c => c.NomClasse == nom);
        }

        public async Task<IEnumerable<Eleve>> GetElevesAsync(int idClasse)
        {
            return await _context.Eleves
               // .Include(e => e.Tuteur)
               // .Include(e => e.Notes)
               // .Include(e => e.Inscriptions)
               // .Include(e => e.Paiements)
               // .Include(e => e.Presences)
               // .Include(e => e.Documents)
                .Where(e => e.IdClasse == idClasse)
                .ToListAsync();
        }

        public async Task<IEnumerable<Cours>> GetCoursAsync(int idClasse)
        {
            return await _context.Cours
                .Include(c => c.AffectationsCours)
                    .ThenInclude(ac => ac.Agent)
              //  .Include(c => c.Notes)
              //  .Include(c => c.Evaluations)
               // .Include(c => c.RessourcesPedagogiques)
                .Where(c => c.IdClasse == idClasse)
                .ToListAsync();
        }

        public async Task<IEnumerable<Inscription>> GetInscriptionsAsync(int idClasse)
        {
            return await _context.Inscriptions
               // .Include(i => i.Eleve)
               // .Include(i => i.Ecole)
               // .Include(i => i.AnneeScolaire)
                .Where(i => i.IdClasse == idClasse)
                .ToListAsync();
        }

        //public async Task<IEnumerable<Frais>> GetFraisAsync(int idClasse)
        //{
        //    return await _context.Frais
        //        .Include(f => f.Paiements)
        //        .Where(f => f.IdClasse == idClasse)
        //        .ToListAsync();
        //}


        public async Task<IEnumerable<Evaluation>> GetEvaluationsAsync(int idClasse)
        {
            return await _context.Evaluations
               // .Include(e => e.Course)
                .Where(e => e.IdClasse == idClasse)
                .ToListAsync();
        }

        // ✅ SOFT DELETE: Toggle le statut d'une classe (actif <-> inactif)
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var classe = await _context.Classes.FindAsync(id);
            if (classe == null)
                return false;

            classe.Statut = classe.Statut != true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
