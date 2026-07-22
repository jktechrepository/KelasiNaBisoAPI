using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.Pagination;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Extensions;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class TitulaireClasseService : ITitulaireClasseRepository
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly ILogger<TitulaireClasseService> _logger;

        public TitulaireClasseService(
            KelasiNaBisoDbContext context,
            ILogger<TitulaireClasseService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ============================================
        // MÉTHODES PAGINÉES
        // ============================================

        public async Task<PagedResult<TitulaireClasse>> GetAllPagedAsync(PagedRequest request)
        {
            var query = _context.TitulairesClasses
                .Include(tc => tc.Agent)
                .Include(tc => tc.Classe)
                .Include(tc => tc.AnneeScolaire)
                .AsQueryable();

            // Filtrer par statut
            if (!request.IncludeInactive)
            {
                query = query.Where(tc => tc.Statut == true);
            }

            // Appliquer la recherche
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(tc =>
                    (tc.Agent.Nom != null && tc.Agent.Nom.ToLower().Contains(searchLower)) ||
                    (tc.Agent.Postnom != null && tc.Agent.Postnom.ToLower().Contains(searchLower)) ||
                    (tc.Agent.Prenom != null && tc.Agent.Prenom.ToLower().Contains(searchLower)) ||
                    (tc.Classe.NomClasse != null && tc.Classe.NomClasse.ToLower().Contains(searchLower)) ||
                    (tc.Commentaire != null && tc.Commentaire.ToLower().Contains(searchLower))
                );
            }

            // Tri par défaut : DateDebut DESC
            if (!string.IsNullOrWhiteSpace(request.SortBy))
            {
                query = query.ApplySort(request.SortBy, request.SortDescending);
            }
            else
            {
                query = request.SortDescending
                    ? query.OrderBy(tc => tc.DateDebut)
                    : query.OrderByDescending(tc => tc.DateDebut);
            }

            return await query.ToPagedAsync(request);
        }

        public async Task<PagedResult<TitulaireClasse>> GetByAgentPagedAsync(int idAgent, PagedRequest request)
        {
            var query = _context.TitulairesClasses
                .Include(tc => tc.Agent)
                .Include(tc => tc.Classe)
                .Include(tc => tc.AnneeScolaire)
                .Where(tc => tc.IdAgent == idAgent);

            if (!request.IncludeInactive)
            {
                query = query.Where(tc => tc.Statut == true);
            }

            return await query.ToPagedAsync(request, tc => tc.DateDebut);
        }

        public async Task<PagedResult<TitulaireClasse>> GetByClassePagedAsync(int idClasse, PagedRequest request)
        {
            var query = _context.TitulairesClasses
                .Include(tc => tc.Agent)
                .Include(tc => tc.Classe)
                .Include(tc => tc.AnneeScolaire)
                .Where(tc => tc.IdClasse == idClasse);

            if (!request.IncludeInactive)
            {
                query = query.Where(tc => tc.Statut == true);
            }

            return await query.ToPagedAsync(request, tc => tc.DateDebut);
        }

        public async Task<PagedResult<TitulaireClasse>> GetByAnneeScolairePagedAsync(int idAnneeScolaire, PagedRequest request)
        {
            var query = _context.TitulairesClasses
                .Include(tc => tc.Agent)
                .Include(tc => tc.Classe)
                .Include(tc => tc.AnneeScolaire)
                .Where(tc => tc.IdAnneeScolaire == idAnneeScolaire);

            if (!request.IncludeInactive)
            {
                query = query.Where(tc => tc.Statut == true);
            }

            return await query.ToPagedAsync(request, tc => tc.DateDebut);
        }

        // ============================================
        // MÉTHODES CRUD
        // ============================================

        public async Task<IEnumerable<TitulaireClasse>> GetAllAsync()
        {
            return await _context.TitulairesClasses
                .Include(tc => tc.Agent)
                .Include(tc => tc.Classe)
                .Include(tc => tc.AnneeScolaire)
                .Where(tc => tc.Statut == true)
                .OrderByDescending(tc => tc.DateDebut)
                .ToListAsync();
        }

        public async Task<TitulaireClasse> GetByIdAsync(int id)
        {
            return await _context.TitulairesClasses
                .Include(tc => tc.Agent)
                .Include(tc => tc.Classe)
                .Include(tc => tc.AnneeScolaire)
                .FirstOrDefaultAsync(tc => tc.IdTitulaireClasse == id);
        }

        public async Task<TitulaireClasse> CreateAsync(TitulaireClasse titulaireClasse)
        {
            // ✅ VALIDATION : Vérifier qu'il n'y a pas déjà un titulaire actif pour cette classe
            var titulaireExistant = await HasTitulaireActifAsync(
                titulaireClasse.IdClasse,
                titulaireClasse.IdAnneeScolaire);

            if (titulaireExistant)
            {
                throw new InvalidOperationException(
                    $"Un titulaire actif existe déjà pour cette classe durant l'année scolaire {titulaireClasse.IdAnneeScolaire}. " +
                    "Veuillez d'abord désactiver l'ancien titulaire.");
            }

            // ✅ VALIDATION : Vérifier que l'agent est un enseignant
            var agent = await _context.Agents
                .FirstOrDefaultAsync(a => a.IdAgent == titulaireClasse.IdAgent);

            if (agent == null)
            {
                throw new InvalidOperationException("L'agent spécifié n'existe pas.");
            }

            var nomComplet = $"{agent.Nom} {agent.Postnom} {agent.Prenom}".Trim();

            if (agent.RoleAgent?.ToUpper() != "ENSEIGNANT")
            {
                throw new InvalidOperationException(
                    $"L'agent {nomComplet} n'est pas un enseignant. " +
                    $"Rôle actuel : {agent.RoleAgent}");
            }

            // ✅ Récupérer les informations de la classe pour le log
            var classe = await _context.Classes
                .Include(c => c.Direction)
                .FirstOrDefaultAsync(c => c.IdClasse == titulaireClasse.IdClasse);

            titulaireClasse.DateCreation = DateTime.Now;
            titulaireClasse.Statut = true;

            _context.TitulairesClasses.Add(titulaireClasse);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                $"✅ Titulaire créé : Agent {nomComplet} → Classe {classe?.NomClasse} " +
                $"(Niveau: {classe?.Direction?.NiveauEnseignement ?? "N/A"}) " +
                $"(Année {titulaireClasse.IdAnneeScolaire})");

            return titulaireClasse;
        }

        public async Task<TitulaireClasse> UpdateAsync(TitulaireClasse titulaireClasse)
        {
            var existant = await GetByIdAsync(titulaireClasse.IdTitulaireClasse);
            if (existant == null)
            {
                throw new InvalidOperationException("Titulaire classe introuvable.");
            }

            // ✅ Si changement de classe, vérifier qu'il n'y a pas déjà un titulaire
            if (existant.IdClasse != titulaireClasse.IdClasse ||
                existant.IdAnneeScolaire != titulaireClasse.IdAnneeScolaire)
            {
                var titulaireExistant = await _context.TitulairesClasses
                    .AnyAsync(tc =>
                        tc.IdClasse == titulaireClasse.IdClasse &&
                        tc.IdAnneeScolaire == titulaireClasse.IdAnneeScolaire &&
                        tc.Statut == true &&
                        tc.IdTitulaireClasse != titulaireClasse.IdTitulaireClasse);

                if (titulaireExistant)
                {
                    throw new InvalidOperationException(
                        "Un titulaire actif existe déjà pour cette classe durant cette année scolaire.");
                }
            }

            titulaireClasse.DateModification = DateTime.Now;

            _context.Entry(existant).CurrentValues.SetValues(titulaireClasse);
            await _context.SaveChangesAsync();

            return titulaireClasse;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var titulaireClasse = await GetByIdAsync(id);
            if (titulaireClasse == null)
            {
                return false;
            }

            _context.TitulairesClasses.Remove(titulaireClasse);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.TitulairesClasses.AnyAsync(tc => tc.IdTitulaireClasse == id);
        }

        public async Task<bool> ToggleStatutAsync(int id)
        {
            var titulaireClasse = await GetByIdAsync(id);
            if (titulaireClasse == null)
            {
                return false;
            }

            titulaireClasse.Statut = titulaireClasse.Statut != true;
            titulaireClasse.DateModification = DateTime.Now;

            if (titulaireClasse.Statut != true)
            {
                titulaireClasse.DateFin = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // ============================================
        // MÉTHODES SPÉCIFIQUES
        // ============================================

        public async Task<TitulaireClasse?> GetTitulaireActifByClasseAsync(int idClasse, int idAnneeScolaire)
        {
            return await _context.TitulairesClasses
                .Include(tc => tc.Agent)
                .Include(tc => tc.Classe)
                .Include(tc => tc.AnneeScolaire)
                .FirstOrDefaultAsync(tc =>
                    tc.IdClasse == idClasse &&
                    tc.IdAnneeScolaire == idAnneeScolaire &&
                    tc.Statut == true);
        }

        public async Task<IEnumerable<TitulaireClasse>> GetByAgentAsync(int idAgent)
        {
            return await _context.TitulairesClasses
                .Include(tc => tc.Agent)
                .Include(tc => tc.Classe)
                .Include(tc => tc.AnneeScolaire)
                .Where(tc => tc.IdAgent == idAgent && tc.Statut == true)
                .OrderByDescending(tc => tc.DateDebut)
                .ToListAsync();
        }

        public async Task<IEnumerable<TitulaireClasse>> GetByClasseAsync(int idClasse)
        {
            return await _context.TitulairesClasses
                .Include(tc => tc.Agent)
                .Include(tc => tc.Classe)
                .Include(tc => tc.AnneeScolaire)
                .Where(tc => tc.IdClasse == idClasse && tc.Statut == true)
                .OrderByDescending(tc => tc.DateDebut)
                .ToListAsync();
        }

        public async Task<IEnumerable<TitulaireClasse>> GetByAnneeScolaireAsync(int idAnneeScolaire)
        {
            return await _context.TitulairesClasses
                .Include(tc => tc.Agent)
                .Include(tc => tc.Classe)
                .Include(tc => tc.AnneeScolaire)
                .Where(tc => tc.IdAnneeScolaire == idAnneeScolaire && tc.Statut == true)
                .OrderByDescending(tc => tc.DateDebut)
                .ToListAsync();
        }

        // ============================================
        // VALIDATION MÉTIER
        // ============================================

        public async Task<bool> HasTitulaireActifAsync(int idClasse, int idAnneeScolaire)
        {
            return await _context.TitulairesClasses
                .AnyAsync(tc =>
                    tc.IdClasse == idClasse &&
                    tc.IdAnneeScolaire == idAnneeScolaire &&
                    tc.Statut == true);
        }

        public async Task<bool> AgentEstDisponibleAsync(int idAgent, int idAnneeScolaire)
        {
            // Un agent ne peut être titulaire que d'une seule classe par année scolaire
            return !await _context.TitulairesClasses
                .AnyAsync(tc =>
                    tc.IdAgent == idAgent &&
                    tc.IdAnneeScolaire == idAnneeScolaire &&
                    tc.Statut == true);
        }
    }
}

