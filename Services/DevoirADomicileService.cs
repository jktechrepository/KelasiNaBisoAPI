using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.DTOs.DevoirADomicile;
using KelasiNaBiso.Models.DTOs.Pagination;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Extensions;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    /// <summary>
    /// Service pour la gestion des devoirs à domicile avec vérifications d'accès intégrées
    /// </summary>
    public class DevoirADomicileService : IDevoirADomicileRepository
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<DevoirADomicileService> _logger;
        private readonly IInscriptionActiveResolver _inscriptionResolver;
        private readonly EleveAnneeScopeHelper _scope;

        public DevoirADomicileService(
            KelasiNaBisoDbContext context,
            ICurrentUserService currentUserService,
            ILogger<DevoirADomicileService> logger,
            IInscriptionActiveResolver inscriptionResolver,
            EleveAnneeScopeHelper scope)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
            _inscriptionResolver = inscriptionResolver;
            _scope = scope;
        }

        // ═══════════════════════════════════════════════════════════════════
        // CRUD DE BASE
        // ═══════════════════════════════════════════════════════════════════

        public async Task<DevoirADomicile> GetByIdAsync(int id)
        {
            return await _context.DevoirsADomicile
                .Include(d => d.Ecole)
                .Include(d => d.Direction)
                .Include(d => d.Agent)
                .Include(d => d.Classe)
                .Include(d => d.Cours)
                .FirstOrDefaultAsync(d => d.IdDevoirADomicile == id)
                ?? throw new KeyNotFoundException($"Devoir à domicile avec ID {id} introuvable");
        }

        public async Task<DevoirADomicile> CreateAsync(DevoirADomicile devoir)
        {
            if (devoir == null)
                throw new ArgumentNullException(nameof(devoir));

            devoir.DateCreation = DateTime.Now;
            devoir.DatePublication = DateTime.Now;
            devoir.NombreTelechargements = 0;
            devoir.Statut = true;

            _context.DevoirsADomicile.Add(devoir);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Devoir à domicile créé : ID {devoir.IdDevoirADomicile}, Titre: {devoir.Titre}, Classe: {devoir.IdClasse}");

            return devoir;
        }

        public async Task<DevoirADomicile> UpdateAsync(DevoirADomicile devoir)
        {
            if (devoir == null)
                throw new ArgumentNullException(nameof(devoir));

            var existing = await GetByIdAsync(devoir.IdDevoirADomicile);
            
            existing.Titre = devoir.Titre;
            existing.Description = devoir.Description;
            existing.DateLimite = devoir.DateLimite;
            existing.Statut = devoir.Statut;
            existing.DateModification = DateTime.Now;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Devoir à domicile mis à jour : ID {devoir.IdDevoirADomicile}");

            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var devoir = await GetByIdAsync(id);
            _context.DevoirsADomicile.Remove(devoir);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.DevoirsADomicile.AnyAsync(d => d.IdDevoirADomicile == id);
        }

        // ═══════════════════════════════════════════════════════════════════
        // RÉCUPÉRATION PAR CRITÈRES
        // ═══════════════════════════════════════════════════════════════════

        public async Task<IEnumerable<DevoirADomicile>> GetAllAsync()
        {
            return await _context.DevoirsADomicile
                .Include(d => d.Ecole)
                .Include(d => d.Direction)
                .Include(d => d.Agent)
                .Include(d => d.Classe)
                .Include(d => d.Cours)
                .Where(d => d.Statut == true)
                .OrderByDescending(d => d.DatePublication)
                .ToListAsync();
        }

        public async Task<IEnumerable<DevoirADomicile>> GetByClasseAsync(int idClasse, int idAnneeScolaire)
        {
            return await _context.DevoirsADomicile
                .Include(d => d.Agent)
                .Include(d => d.Classe)
                .Include(d => d.Cours)
                .Where(d => d.IdClasse == idClasse && d.IdAnneeScolaire == idAnneeScolaire && d.Statut == true)
                .OrderByDescending(d => d.DatePublication)
                .ToListAsync();
        }

        public async Task<IEnumerable<DevoirADomicile>> GetByAgentAsync(int idAgent)
        {
            return await _context.DevoirsADomicile
                .Include(d => d.Classe)
                .Include(d => d.Cours)
                .Where(d => d.IdAgent == idAgent && d.Statut == true)
                .OrderByDescending(d => d.DatePublication)
                .ToListAsync();
        }

        public async Task<IEnumerable<DevoirADomicile>> GetByEcoleAsync(int idEcole)
        {
            return await _context.DevoirsADomicile
                .Include(d => d.Agent)
                .Include(d => d.Classe)
                .Where(d => d.IdEcole == idEcole && d.Statut == true)
                .OrderByDescending(d => d.DatePublication)
                .ToListAsync();
        }

        public async Task<IEnumerable<DevoirADomicile>> GetByDirectionAsync(int idDirection)
        {
            return await _context.DevoirsADomicile
                .Include(d => d.Agent)
                .Include(d => d.Classe)
                .Where(d => d.IdDirection == idDirection && d.Statut == true)
                .OrderByDescending(d => d.DatePublication)
                .ToListAsync();
        }

        public async Task<IEnumerable<DevoirADomicile>> GetByCoursAsync(int idCours)
        {
            return await _context.DevoirsADomicile
                .Include(d => d.Agent)
                .Include(d => d.Classe)
                .Where(d => d.IdCours == idCours && d.Statut == true)
                .OrderByDescending(d => d.DatePublication)
                .ToListAsync();
        }

        public async Task<IEnumerable<DevoirADomicile>> GetByDatePublicationAsync(DateTime dateDebut, DateTime dateFin)
        {
            return await _context.DevoirsADomicile
                .Include(d => d.Agent)
                .Include(d => d.Classe)
                .Where(d => d.DatePublication >= dateDebut && d.DatePublication <= dateFin && d.Statut == true)
                .OrderByDescending(d => d.DatePublication)
                .ToListAsync();
        }

        // ═══════════════════════════════════════════════════════════════════
        // PAGINATION
        // ═══════════════════════════════════════════════════════════════════

        public async Task<PagedResult<DevoirADomicile>> GetAllPagedAsync(
            PagedRequest request,
            int idAnneeScolaire,
            int? idEcole = null,
            int? idClasse = null)
        {
            IQueryable<DevoirADomicile> query = _context.DevoirsADomicile
                .Include(d => d.Ecole)
                .Include(d => d.Direction)
                .Include(d => d.Agent)
                .Include(d => d.Classe)
                .Include(d => d.Cours)
                .Where(d => d.IdAnneeScolaire == idAnneeScolaire);

            // Filtrer par école si spécifié
            if (idEcole.HasValue && idEcole.Value > 0)
            {
                query = query.Where(d => d.IdEcole == idEcole.Value);
            }

            // Filtrer par classe si spécifié
            if (idClasse.HasValue && idClasse.Value > 0)
            {
                query = query.Where(d => d.IdClasse == idClasse.Value);
            }

            // Filtrer par statut
            if (!request.IncludeInactive)
            {
                query = query.Where(d => d.Statut == true);
            }

            // Recherche
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(d =>
                    (d.Titre != null && d.Titre.ToLower().Contains(searchLower)) ||
                    (d.Description != null && d.Description.ToLower().Contains(searchLower)) ||
                    (d.Ecole != null && d.Ecole.Nom != null && d.Ecole.Nom.ToLower().Contains(searchLower)) ||
                    (d.Classe != null && d.Classe.NomClasse != null && d.Classe.NomClasse.ToLower().Contains(searchLower))
                );
            }

            // Tri
            if (!string.IsNullOrWhiteSpace(request.SortBy))
            {
                query = query.ApplySort(request.SortBy, request.SortDescending);
            }
            else
            {
                query = request.SortDescending
                    ? query.OrderByDescending(d => d.DatePublication)
                    : query.OrderBy(d => d.DatePublication);
            }

            return await query.ToPagedAsync(request);
        }

        public async Task<PagedResult<DevoirADomicile>> GetByClassePagedAsync(int idClasse, PagedRequest request, int idAnneeScolaire)
        {
            var query = _context.DevoirsADomicile
                .Include(d => d.Agent)
                .Include(d => d.Classe)
                .Include(d => d.Cours)
                .Where(d => d.IdClasse == idClasse && d.IdAnneeScolaire == idAnneeScolaire);

            // Filtrer par statut
            if (!request.IncludeInactive)
            {
                query = query.Where(d => d.Statut == true);
            }

            // Recherche
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(d =>
                    (d.Titre != null && d.Titre.ToLower().Contains(searchLower)) ||
                    (d.Description != null && d.Description.ToLower().Contains(searchLower))
                );
            }

            // Tri
            if (!string.IsNullOrWhiteSpace(request.SortBy))
            {
                query = query.ApplySort(request.SortBy, request.SortDescending);
            }
            else
            {
                query = request.SortDescending
                    ? query.OrderByDescending(d => d.DatePublication)
                    : query.OrderBy(d => d.DatePublication);
            }

            return await query.ToPagedAsync(request);
        }

        public async Task<PagedResult<DevoirADomicile>> GetByAgentPagedAsync(
            int idAgent,
            PagedRequest request,
            int idAnneeScolaire,
            int? idClasse = null)
        {
            var query = _context.DevoirsADomicile
                .Include(d => d.Classe)
                .Include(d => d.Cours)
                .Where(d => d.IdAgent == idAgent && d.IdAnneeScolaire == idAnneeScolaire);

            // Filtrer par classe si spécifié
            if (idClasse.HasValue && idClasse.Value > 0)
            {
                query = query.Where(d => d.IdClasse == idClasse.Value);
            }

            if (!request.IncludeInactive)
            {
                query = query.Where(d => d.Statut == true);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(d =>
                    (d.Titre != null && d.Titre.ToLower().Contains(searchLower)) ||
                    (d.Description != null && d.Description.ToLower().Contains(searchLower))
                );
            }

            if (!string.IsNullOrWhiteSpace(request.SortBy))
            {
                query = query.ApplySort(request.SortBy, request.SortDescending);
            }
            else
            {
                query = request.SortDescending
                    ? query.OrderByDescending(d => d.DatePublication)
                    : query.OrderBy(d => d.DatePublication);
            }

            return await query.ToPagedAsync(request);
        }

        public async Task<PagedResult<DevoirADomicile>> GetByEcolePagedAsync(
            int idEcole,
            PagedRequest request,
            int idAnneeScolaire,
            int? idClasse = null)
        {
            var query = _context.DevoirsADomicile
                .Include(d => d.Agent)
                .Include(d => d.Classe)
                .Where(d => d.IdEcole == idEcole && d.IdAnneeScolaire == idAnneeScolaire);

            // Filtrer par classe si spécifié
            if (idClasse.HasValue && idClasse.Value > 0)
            {
                query = query.Where(d => d.IdClasse == idClasse.Value);
            }

            if (!request.IncludeInactive)
            {
                query = query.Where(d => d.Statut == true);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(d =>
                    (d.Titre != null && d.Titre.ToLower().Contains(searchLower)) ||
                    (d.Description != null && d.Description.ToLower().Contains(searchLower))
                );
            }

            if (!string.IsNullOrWhiteSpace(request.SortBy))
            {
                query = query.ApplySort(request.SortBy, request.SortDescending);
            }
            else
            {
                query = request.SortDescending
                    ? query.OrderByDescending(d => d.DatePublication)
                    : query.OrderBy(d => d.DatePublication);
            }

            return await query.ToPagedAsync(request);
        }

        public async Task<PagedResult<DevoirADomicilePourTuteurDto>> GetByTuteurPagedAsync(
            int idTuteur,
            PagedRequest request,
            string? libelleAnneeScolaire = null)
        {
            var pageNumber = Math.Max(1, request.PageNumber);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);

            var now = DateTime.Now;
            var inscriptions = await _context.Inscriptions
                .AsNoTracking()
                .Include(i => i.Eleve)
                .Include(i => i.Ecole)
                .Include(i => i.Classe)
                .Include(i => i.AnneeScolaire)
                .Where(i => i.Eleve != null
                    && i.Eleve.IdTuteur == idTuteur
                    && i.Eleve.Statut == true
                    && i.Statut == true
                    && i.StatutInscription != null
                    && (i.StatutInscription == InscriptionActiveRules.StatutConfirme
                        || i.StatutInscription == "Confirme"
                        || i.StatutInscription.StartsWith("Confirm")))
                .Where(i => string.IsNullOrWhiteSpace(libelleAnneeScolaire)
                    ? i.AnneeScolaire != null
                        && i.AnneeScolaire.Statut == true
                        && i.AnneeScolaire.DateDebut <= now
                        && i.AnneeScolaire.DateFin >= now
                    : i.AnneeScolaire != null
                        && i.AnneeScolaire.LibelleAnneeScolaire.ToLower()
                            == libelleAnneeScolaire.Trim().ToLower())
                .ToListAsync();

            var elevesParClasse = inscriptions
                .GroupBy(i => new { i.IdEleve, i.IdEcole, i.IdAnneeScolaire, i.IdClasse })
                .Select(g => g.OrderByDescending(i => i.DateInscription).First())
                .Select(i =>
                {
                    return new EleveConcerneDevoirDto
                    {
                        IdEleve = i.IdEleve,
                        NomComplet = i.Eleve!.NomComplet,
                        IdClasse = i.IdClasse,
                        NomClasse = i.Classe?.NomClasse
                    };
                })
                .ToList();

            if (inscriptions.Count == 0)
                return new PagedResult<DevoirADomicilePourTuteurDto>(
                    new List<DevoirADomicilePourTuteurDto>(), 0, pageNumber, pageSize);

            var classeKeys = inscriptions
                .Select(i => $"{i.IdEcole}:{i.IdAnneeScolaire}:{i.IdClasse}")
                .ToHashSet();

            var query = _context.DevoirsADomicile
                .AsNoTracking()
                .Include(d => d.Ecole)
                .Include(d => d.Direction)
                .Include(d => d.Agent)
                .Include(d => d.Classe)
                .Include(d => d.Cours)
                .Include(d => d.AnneeScolaire)
                .Where(d => d.Statut == true || request.IncludeInactive);

            if (!request.IncludeInactive)
                query = query.Where(d => d.Statut == true);

            var devoirs = (await query
                .OrderByDescending(d => d.DatePublication)
                .ToListAsync())
                .Where(d => classeKeys.Contains($"{d.IdEcole}:{d.IdAnneeScolaire}:{d.IdClasse}"))
                .ToList();

            var mapped = devoirs.Select(d =>
            {
                var dto = MapToTuteurDto(d);
                dto.ElevesConcernes = elevesParClasse
                    .Where(e => inscriptions.Any(i => i.IdEleve == e.IdEleve
                        && i.IdEcole == d.IdEcole
                        && i.IdAnneeScolaire == d.IdAnneeScolaire
                        && i.IdClasse == d.IdClasse))
                    .OrderBy(e => e.NomComplet)
                    .ToList();
                return dto;
            }).ToList();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim();
                if (term.Length > 200)
                    term = term[..200];
                var searchLower = term.ToLower();

                mapped = mapped.Where(d =>
                    (d.Titre != null && d.Titre.ToLower().Contains(searchLower))
                    || (d.Description != null && d.Description.ToLower().Contains(searchLower))
                    || (d.NomClasse != null && d.NomClasse.ToLower().Contains(searchLower))
                    || d.ElevesConcernes.Any(e =>
                        e.NomComplet != null && e.NomComplet.ToLower().Contains(searchLower)))
                    .ToList();
            }

            var total = mapped.Count;
            var page = mapped
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var idUser = _currentUserService.UserId;
            if (idUser > 0 && page.Count > 0)
            {
                var telecharges = await GetDevoirIdsTelechargesParUtilisateurAsync(
                    idUser, page.Select(d => d.IdDevoirADomicile));
                foreach (var d in page)
                    d.EstTelechargeParMoi = telecharges.Contains(d.IdDevoirADomicile);
            }

            return new PagedResult<DevoirADomicilePourTuteurDto>(page, total, pageNumber, pageSize);
        }

        private static DevoirADomicilePourTuteurDto MapToTuteurDto(DevoirADomicile d)
        {
            var agentNom = d.Agent == null
                ? null
                : $"{d.Agent.Nom} {d.Agent.Postnom} {d.Agent.Prenom}".Trim();

            return new DevoirADomicilePourTuteurDto
            {
                IdDevoirADomicile = d.IdDevoirADomicile,
                Titre = d.Titre,
                Description = d.Description,
                Contenu = d.Contenu,
                NomFichier = d.NomFichier,
                TailleFichier = d.TailleFichier,
                TypeMIME = d.TypeMIME,
                IdEcole = d.IdEcole,
                NomEcole = d.Ecole?.Nom,
                IdDirection = d.IdDirection,
                NomDirection = d.Direction?.NomDirection,
                IdAgent = d.IdAgent,
                NomAgent = string.IsNullOrWhiteSpace(agentNom) ? null : agentNom,
                IdClasse = d.IdClasse,
                NomClasse = d.Classe?.NomClasse,
                IdAnneeScolaire = d.IdAnneeScolaire,
                LibelleAnneeScolaire = d.AnneeScolaire?.LibelleAnneeScolaire,
                IdCours = d.IdCours,
                NomCours = d.Cours?.NomCours,
                DatePublication = d.DatePublication,
                DateLimite = d.DateLimite,
                NombreTelechargements = d.NombreTelechargements,
                Statut = d.Statut
            };
        }

        // ═══════════════════════════════════════════════════════════════════
        // VÉRIFICATIONS D'ACCÈS (INTÉGRÉES DANS LE SERVICE)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Vérifie si un agent peut publier un devoir pour une classe
        /// Règles : L'agent doit être enseignant (via AffectationCours ou TitulaireClasse) OU Directeur/Admin (son école) OU Super-Admin (toutes)
        /// </summary>
        public async Task<bool> AgentPeutPublierPourClasseAsync(int idAgent, int idClasse)
        {
            var agent = await _context.Agents.FindAsync(idAgent);
            if (agent == null)
                return false;

            // Récupérer le rôle de l'utilisateur associé à cet agent
            var utilisateur = await _context.Utilisateurs
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.IdAgent == idAgent);

            var role = utilisateur?.Role?.Nom ?? string.Empty;

            // Super-Admin : Peut publier pour toutes les classes
            if (role == UserRoles.SUPER_ADMIN)
            {
                _logger.LogInformation($"Super-Admin (agent {idAgent}) peut publier pour toutes les classes");
                return true;
            }

            // Admin : Peut publier pour toutes les classes de son école
            if (role == UserRoles.ADMIN)
            {
                var classe = await _context.Classes
                    .Include(c => c.Direction)
                    .FirstOrDefaultAsync(c => c.IdClasse == idClasse);

                if (classe == null)
                    return false;

                // Vérifier que la classe appartient à l'école de l'admin
                var adminEcoleId = utilisateur?.IdEcole ?? agent.IdEcole;
                if (adminEcoleId.HasValue && classe.Direction?.IdEcole == adminEcoleId.Value)
                {
                    _logger.LogInformation($"Admin (agent {idAgent}) peut publier pour la classe {idClasse} de son école");
                    return true;
                }
            }

            // Directeur : Peut publier pour toutes les classes de son école
            if (await EstDirecteurAsync(idAgent))
            {
                var classe = await _context.Classes
                    .Include(c => c.Direction)
                    .FirstOrDefaultAsync(c => c.IdClasse == idClasse);

                if (classe == null)
                    return false;

                // Vérifier que la classe appartient à l'école du directeur
                if (classe.Direction?.IdEcole == agent.IdEcole)
                {
                    _logger.LogInformation($"Directeur (agent {idAgent}) peut publier pour la classe {idClasse} de son école");
                    return true;
                }
            }

            // Vérifier si l'agent est titulaire de la classe (Maternelle/Primaire)
            var estTitulaire = await _context.TitulairesClasses
                .AnyAsync(tc => tc.IdAgent == idAgent 
                    && tc.IdClasse == idClasse 
                    && tc.Statut == true);

            if (estTitulaire)
            {
                _logger.LogInformation($"Agent {idAgent} est titulaire de la classe {idClasse}");
                return true;
            }

            // Vérifier si l'agent enseigne un cours dans cette classe (Secondaire)
            var enseigneCours = await _context.AffectationsCours
                .Include(ac => ac.Cours)
                .AnyAsync(ac => ac.IdAgent == idAgent 
                    && ac.Cours.IdClasse == idClasse 
                    && ac.Statut == true);

            if (enseigneCours)
            {
                _logger.LogInformation($"Agent {idAgent} enseigne un cours dans la classe {idClasse}");
                return true;
            }

            _logger.LogWarning($"Agent {idAgent} ne peut pas publier pour la classe {idClasse}");
            return false;
        }

        /// <summary>
        /// Vérifie si un utilisateur peut accéder à un devoir
        /// Règles : Parent (enfant dans la classe) OU Élève (dans la classe) OU Enseignant (enseigne à la classe) OU Directeur
        /// </summary>
        public async Task<bool> UserPeutAccederAuDevoirAsync(int idUtilisateur, int idDevoirADomicile)
        {
            var devoir = await GetByIdAsync(idDevoirADomicile);
            return await UserPeutAccederAClasseAsync(idUtilisateur, devoir.IdClasse);
        }

        /// <summary>
        /// Vérifie si un utilisateur peut accéder à une classe
        /// Règles : Parent (enfant dans la classe) OU Élève (dans la classe) OU Enseignant (enseigne à la classe) OU Directeur/Admin (son école) OU Super-Admin (toutes)
        /// </summary>
        public async Task<bool> UserPeutAccederAClasseAsync(int idUtilisateur, int idClasse)
        {
            var utilisateur = await _context.Utilisateurs
                .Include(u => u.Agent)
                .FirstOrDefaultAsync(u => u.IdUtilisateur == idUtilisateur);

            if (utilisateur == null)
                return false;

            // Vérifier le rôle
            var role = utilisateur.Role?.Nom ?? _currentUserService.UserRole;

            // Super-Admin : Accès à toutes les classes de toutes les écoles
            if (role == UserRoles.SUPER_ADMIN)
            {
                _logger.LogInformation($"Super-Admin {idUtilisateur} a accès à toutes les classes");
                return true;
            }

            // Admin : Accès à toutes les classes de son école
            if (role == UserRoles.ADMIN)
            {
                var classe = await _context.Classes
                    .Include(c => c.Direction)
                    .FirstOrDefaultAsync(c => c.IdClasse == idClasse);

                if (classe == null)
                    return false;

                // Vérifier que la classe appartient à l'école de l'admin
                var adminEcoleId = utilisateur.IdEcole;
                if (adminEcoleId.HasValue && classe.Direction?.IdEcole == adminEcoleId.Value)
                {
                    _logger.LogInformation($"Admin {idUtilisateur} a accès à la classe {idClasse} de son école");
                    return true;
                }
            }

            // Directeur : Accès à toutes les classes de son école
            if (role == UserRoles.DIRECTEUR && utilisateur.IdAgent.HasValue)
            {
                var agent = await _context.Agents.FindAsync(utilisateur.IdAgent.Value);
                if (agent == null)
                    return false;

                var classe = await _context.Classes
                    .Include(c => c.Direction)
                    .FirstOrDefaultAsync(c => c.IdClasse == idClasse);

                if (classe == null)
                    return false;

                if (classe.Direction?.IdEcole == agent.IdEcole)
                {
                    _logger.LogInformation($"Directeur {idUtilisateur} a accès à la classe {idClasse} de son école");
                    return true;
                }
            }

            // Enseignant : Vérifier AffectationCours ou TitulaireClasse
            if (role == UserRoles.ENSEIGNANT && utilisateur.IdAgent.HasValue)
            {
                return await AgentPeutPublierPourClasseAsync(utilisateur.IdAgent.Value, idClasse);
            }

            // Parent : Vérifier si un enfant est dans la classe
            if (role == UserRoles.PARENT && utilisateur.IdTuteur.HasValue)
            {
                var eleveDansClasse = await _inscriptionResolver
                    .FilterElevesInClasse(_context.Eleves, idClasse)
                    .AnyAsync(e => e.IdTuteur == utilisateur.IdTuteur.Value);

                if (eleveDansClasse)
                {
                    _logger.LogInformation($"Parent {idUtilisateur} a un enfant dans la classe {idClasse}");
                    return true;
                }
            }

            // Élève : inscription active dans la classe via IdEleve du compte
            if (role == UserRoles.ELEVE && utilisateur.IdEleve.HasValue)
            {
                var eleveDansClasse = await _inscriptionResolver
                    .FilterElevesInClasse(_context.Eleves, idClasse)
                    .AnyAsync(e => e.IdEleve == utilisateur.IdEleve.Value);

                if (eleveDansClasse)
                {
                    _logger.LogInformation($"Élève {idUtilisateur} (IdEleve={utilisateur.IdEleve}) a accès à la classe {idClasse}");
                    return true;
                }
            }

            _logger.LogWarning($"Utilisateur {idUtilisateur} n'a pas accès à la classe {idClasse}");
            return false;
        }

        /// <summary>
        /// Vérifie si un agent est Directeur
        /// </summary>
        public async Task<bool> EstDirecteurAsync(int idAgent)
        {
            var agent = await _context.Agents
                .FirstOrDefaultAsync(a => a.IdAgent == idAgent);

            if (agent == null)
                return false;

            // Vérifier via RoleAgent
            if (agent.RoleAgent == UserRoles.DIRECTEUR)
                return true;

            // Vérifier via Utilisateur.Role (premier utilisateur associé)
            var utilisateur = await _context.Utilisateurs
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.IdAgent == idAgent);

            if (utilisateur != null)
            {
                // Vérifier le rôle principal
                if (utilisateur.Role?.Nom == UserRoles.DIRECTEUR)
                    return true;

                // Vérifier via UserRoles (multi-rôles)
                var userRoles = await _context.UserRoles
                    .Include(ur => ur.Role)
                    .Where(ur => ur.IdUtilisateur == utilisateur.IdUtilisateur && ur.Statut == true)
                    .Select(ur => ur.Role.Nom)
                    .ToListAsync();

                return userRoles.Contains(UserRoles.DIRECTEUR);
            }

            return false;
        }

        /// <summary>
        /// Incrémente le nombre de téléchargements d'un devoir
        /// </summary>
        public async Task<bool> IncrementerTelechargementsAsync(int idDevoirADomicile)
        {
            var devoir = await _context.DevoirsADomicile.FindAsync(idDevoirADomicile);
            if (devoir == null)
                return false;

            devoir.NombreTelechargements++;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Téléchargement incrémenté pour devoir {idDevoirADomicile}. Total: {devoir.NombreTelechargements}");

            return true;
        }

        /// <inheritdoc />
        public async Task<bool> EnregistrerTelechargementAsync(int idDevoirADomicile, int idUtilisateur)
        {
            var devoir = await _context.DevoirsADomicile.FindAsync(idDevoirADomicile);
            if (devoir == null)
                return false;

            await using var tx = _context.Database.IsRelational()
                ? await _context.Database.BeginTransactionAsync()
                : null;
            try
            {
                devoir.NombreTelechargements++;

                if (idUtilisateur > 0)
                {
                    var deja = await _context.DevoirsADomicileTelechargements
                        .AnyAsync(t => t.IdDevoirADomicile == idDevoirADomicile
                            && t.IdUtilisateur == idUtilisateur);

                    if (!deja)
                    {
                        _context.DevoirsADomicileTelechargements.Add(new DevoirADomicileTelechargement
                        {
                            IdDevoirADomicile = idDevoirADomicile,
                            IdUtilisateur = idUtilisateur,
                            DateTelechargement = DateTime.UtcNow
                        });
                    }
                }

                await _context.SaveChangesAsync();
                if (tx != null)
                    await tx.CommitAsync();

                _logger.LogInformation(
                    "Téléchargement enregistré devoir {IdDevoir} user {IdUser}. Total global: {Total}",
                    idDevoirADomicile, idUtilisateur, devoir.NombreTelechargements);

                return true;
            }
            catch
            {
                if (tx != null)
                    await tx.RollbackAsync();
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<HashSet<int>> GetDevoirIdsTelechargesParUtilisateurAsync(
            int idUtilisateur,
            IEnumerable<int> idDevoirs)
        {
            var ids = idDevoirs?.Distinct().ToList() ?? new List<int>();
            if (idUtilisateur <= 0 || ids.Count == 0)
                return new HashSet<int>();

            var telecharges = await _context.DevoirsADomicileTelechargements
                .AsNoTracking()
                .Where(t => t.IdUtilisateur == idUtilisateur && ids.Contains(t.IdDevoirADomicile))
                .Select(t => t.IdDevoirADomicile)
                .ToListAsync();

            return telecharges.ToHashSet();
        }
    }
}

