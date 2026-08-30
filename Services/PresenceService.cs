using System;
using System.Threading;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.DTOs.Pagination;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Services.Notifications;
using KelasiNaBiso.Extensions;
using KelasiNaBisoAPI.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class PresenceService : IPresenceRepository
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly ILogger<PresenceService> _logger;
        private readonly INotificationDispatcher _notificationDispatcher;
        private readonly INotificationJobQueue _notificationJobQueue;
        private readonly IDashboardHubService _dashboardHubService;
        private readonly IInscriptionActiveResolver _inscriptionResolver;
        private readonly EleveAnneeScopeHelper _scope;

        public PresenceService(
            KelasiNaBisoDbContext context,
            ILogger<PresenceService> logger,
            INotificationDispatcher notificationDispatcher,
            INotificationJobQueue notificationJobQueue,
            IDashboardHubService dashboardHubService,
            IInscriptionActiveResolver inscriptionResolver,
            EleveAnneeScopeHelper scope)
        {
            _context = context;
            _logger = logger;
            _notificationDispatcher = notificationDispatcher;
            _notificationJobQueue = notificationJobQueue;
            _dashboardHubService = dashboardHubService;
            _inscriptionResolver = inscriptionResolver;
            _scope = scope;
        }

        private IQueryable<Presence> ApplyAnneeEcoleFilter(
            IQueryable<Presence> query, int idEcole, int idAnneeScolaire)
        {
            var eleveIds = _scope.GetEleveIdsInEcoleAnnee(idEcole, idAnneeScolaire);
            return query.Where(p =>
                p.IdAgent != null
                || (p.IdEleve != null && eleveIds.Contains(p.IdEleve.Value)));
        }

        private IQueryable<Presence> ApplyAnneeEleveFilter(
            IQueryable<Presence> query, int idEleve, int idAnneeScolaire)
        {
            return query.Where(p => p.IdEleve == idEleve && _context.Inscriptions.Any(i =>
                i.IdEleve == idEleve
                && i.IdAnneeScolaire == idAnneeScolaire
                && i.Statut == true
                && i.StatutInscription != null
                && (i.StatutInscription == InscriptionActiveRules.StatutConfirme
                    || i.StatutInscription == "Confirme"
                    || i.StatutInscription.StartsWith("Confirm"))));
        }

        public async Task<ElevesAnneeScopedResult<PagedResult<Presence>>> GetAllPagedAsync(
            int idEcole, PagedRequest request, int? idAnneeScolaire = null)
        {
            var (ecole, annee) = await _scope.ResolveEcoleAnneeAsync(idEcole, idAnneeScolaire);
            var query = ApplyAnneeEcoleFilter(
                _context.Presences
                    .Include(p => p.Eleve)
                    .Include(p => p.Agent)
                    .AsQueryable(),
                ecole, annee);

            // Filtrer par statut
            if (!request.IncludeInactive)
            {
                query = query.Where(p => p.Statut == true);
            }

            // Appliquer la recherche (par observation ou type)
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(p =>
                    (p.Observation != null && p.Observation.ToLower().Contains(searchLower)) ||
                    (p.TypePresence != null && p.TypePresence.ToLower().Contains(searchLower))
                );
            }

            // Appliquer le tri
            if (!string.IsNullOrWhiteSpace(request.SortBy))
            {
                query = query.ApplySort(request.SortBy, request.SortDescending);
            }
            else
            {
                // Tri par défaut : DateDuJour DESC (plus récent en premier)
                query = request.SortDescending
                    ? query.OrderBy(p => p.DateDuJour).ThenBy(p => p.HeureArrivee)
                    : query.OrderByDescending(p => p.DateDuJour).ThenByDescending(p => p.HeureArrivee);
            }

            var paged = await query.ToPagedAsync(request);
            return EleveAnneeScopeHelper.Wrap(paged, ecole, annee);
        }

        public async Task<ElevesAnneeScopedResult<CursorPaginatedResult<Presence>>> GetAllCursorPagedAsync(
            int idEcole, CursorPaginationRequest request, int? idAnneeScolaire = null)
        {
            var (ecole, annee) = await _scope.ResolveEcoleAnneeAsync(idEcole, idAnneeScolaire);
            var query = ApplyAnneeEcoleFilter(
                _context.Presences
                    .Include(p => p.Eleve)
                    .Include(p => p.Agent)
                    .AsQueryable(),
                ecole, annee);

            if (!request.IncludeInactive)
                query = query.Where(p => p.Statut == true);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(p =>
                    (p.Observation != null && p.Observation.ToLower().Contains(searchLower)) ||
                    (p.TypePresence != null && p.TypePresence.ToLower().Contains(searchLower)));
            }

            var paged = await query.ToCursorPagedAsync(request, p => p.IdPresence);
            return EleveAnneeScopeHelper.Wrap(paged, ecole, annee);
        }

        public async Task<ElevesAnneeScopedResult<PagedResult<Presence>>> GetByElevePagedAsync(
            int idEleve, PagedRequest request, int? idAnneeScolaire = null)
        {
            var idEcole = await _inscriptionResolver.GetEcoleCouranteAsync(idEleve, idAnneeScolaire);
            if (!idEcole.HasValue)
                idEcole = await _inscriptionResolver.GetEcoleCouranteAsync(idEleve, null);
            if (!idEcole.HasValue)
                throw new InvalidOperationException($"Élève {idEleve} introuvable ou sans inscription confirmée.");

            var (ecole, annee) = await _scope.ResolveEcoleAnneeAsync(idEcole.Value, idAnneeScolaire);
            var query = ApplyAnneeEleveFilter(
                _context.Presences
                    .Include(p => p.Eleve)
                    .AsQueryable(),
                idEleve, annee);

            // Filtrer par statut
            if (!request.IncludeInactive)
            {
                query = query.Where(p => p.Statut == true);
            }

            var paged = await query.ToPagedAsync(request, p => p.DateDuJour);
            return EleveAnneeScopeHelper.Wrap(paged, ecole, annee);
        }

        public async Task<PagedResult<Presence>> GetByAgentPagedAsync(int idAgent, PagedRequest request)
        {
            var query = _context.Presences
                .Include(p => p.Agent)
                .Where(p => p.IdAgent == idAgent);

            // Filtrer par statut
            if (!request.IncludeInactive)
            {
                query = query.Where(p => p.Statut == true);
            }

            return await query.ToPagedAsync(request, p => p.DateDuJour);
        }

        public async Task<PagedResult<Presence>> GetByDatePagedAsync(DateTime date, PagedRequest request)
        {
            var query = _context.Presences
                .Include(p => p.Eleve)
                .Include(p => p.Agent)
               // .Include(p => p.Vacation)
                .Where(p => p.DateDuJour.Date == date.Date);

            // Filtrer par statut
            if (!request.IncludeInactive)
            {
                query = query.Where(p => p.Statut == true);
            }

            return await query.ToPagedAsync(request, p => p.HeureArrivee);
        }

        public async Task<ElevesAnneeScopedResult<PagedResult<Presence>>> GetByDateRangePagedAsync(
            int idEcole, DateTime dateDebut, DateTime dateFin, PagedRequest request, int? idAnneeScolaire = null)
        {
            var (ecole, annee) = await _scope.ResolveEcoleAnneeAsync(idEcole, idAnneeScolaire);
            var query = ApplyAnneeEcoleFilter(
                _context.Presences
                    .Include(p => p.Eleve)
                    .Include(p => p.Agent)
                    .AsQueryable(),
                ecole, annee)
                .Where(p => p.DateDuJour.Date >= dateDebut.Date && p.DateDuJour.Date <= dateFin.Date);

            if (!request.IncludeInactive)
                query = query.Where(p => p.Statut == true);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(p =>
                    (p.Observation != null && p.Observation.ToLower().Contains(searchLower)) ||
                    (p.TypePresence != null && p.TypePresence.ToLower().Contains(searchLower)));
            }

            var paged = await query.ToPagedAsync(request, p => p.DateDuJour);
            return EleveAnneeScopeHelper.Wrap(paged, ecole, annee);
        }

        public async Task<PagedResult<Presence>> GetByTypePersonnePagedAsync(string typePersonne, PagedRequest request)
        {
            var query = _context.Presences
                .Include(p => p.Eleve)
                .Include(p => p.Agent)
               // .Include(p => p.Vacation)
                .Where(p => p.TypePresence == typePersonne);

            // Filtrer par statut
            if (!request.IncludeInactive)
            {
                query = query.Where(p => p.Statut == true);
            }

            return await query.ToPagedAsync(request, p => p.DateDuJour);
        }

        // ⚠️ DEPRECATED: Anciennes méthodes (conserver pour rétrocompatibilité)
        public async Task<ElevesAnneeScopedResult<IEnumerable<Presence>>> GetAllAsync(
            int idEcole, int? idAnneeScolaire = null)
        {
            var (ecole, annee) = await _scope.ResolveEcoleAnneeAsync(idEcole, idAnneeScolaire);
            var data = await ApplyAnneeEcoleFilter(
                    _context.Presences
                        .Include(p => p.Eleve)
                        .Include(p => p.Agent)
                        .AsQueryable(),
                    ecole, annee)
                .Where(p => p.Statut == true)
                .ToListAsync();
            return EleveAnneeScopeHelper.Wrap<IEnumerable<Presence>>(data, ecole, annee);
        }

        public async Task<Presence> GetByIdAsync(int id)
        {
            return await _context.Presences
                .Include(p => p.Eleve)
                .Include(p => p.Agent) // ✅ POINTAGE AGENT: Inclure l'agent
              //  .Include(p => p.Vacation)
                .Where(p => p.Statut == true) // ✅ Filtrer uniquement les présences actives
                .FirstOrDefaultAsync(p => p.IdPresence == id);
        }

        public async Task<ElevesAnneeScopedResult<IEnumerable<Presence>>> GetByEleveAsync(
            int idEleve, int? idAnneeScolaire = null)
        {
            var idEcole = await _inscriptionResolver.GetEcoleCouranteAsync(idEleve, idAnneeScolaire);
            if (!idEcole.HasValue)
                idEcole = await _inscriptionResolver.GetEcoleCouranteAsync(idEleve, null);
            if (!idEcole.HasValue)
                throw new InvalidOperationException($"Élève {idEleve} introuvable ou sans inscription confirmée.");

            var (ecole, annee) = await _scope.ResolveEcoleAnneeAsync(idEcole.Value, idAnneeScolaire);
            var data = await ApplyAnneeEleveFilter(_context.Presences.AsQueryable(), idEleve, annee)
                .Where(p => p.Statut == true)
                .OrderByDescending(p => p.DateDuJour)
                .ToListAsync();
            return EleveAnneeScopeHelper.Wrap<IEnumerable<Presence>>(data, ecole, annee);
        }

        public async Task<IEnumerable<Presence>> GetByVacationAsync(int IdVacation)
        {
            return await _context.Presences
                .Include(p => p.Eleve)
                .Where(p => p.IdVacation == IdVacation)
                .Where(p => p.Statut == true) // ✅ Filtrer uniquement les présences actives
                .OrderBy(p => p.DateDuJour)
                .ToListAsync();
        }

        public async Task<IEnumerable<Presence>> GetByDateAsync(DateTime date)
        {
            return await _context.Presences
                .Include(p => p.Eleve)
                .Include(p => p.Agent) // ✅ POINTAGE AGENT
              //  .Include(p => p.Vacation)
                .Where(p => p.DateDuJour.Date == date.Date)
                .Where(p => p.Statut == true) // ✅ Filtrer uniquement les présences actives
                .ToListAsync();
        }

        public async Task<IEnumerable<Presence>> GetByEleveAndDateAsync(int idEleve, DateTime date)
        {
            return await _context.Presences
                .Include(p => p.Vacation)
                .Where(p => p.IdEleve == idEleve && p.DateDuJour.Date == date.Date)
                .Where(p => p.Statut == true) // ✅ Filtrer uniquement les présences actives
                .ToListAsync();
        }

        // ✅ POINTAGE AGENT: Récupérer les présences d'un agent
        public async Task<IEnumerable<Presence>> GetByAgentAsync(int idAgent)
        {
            return await _context.Presences
              //  .Include(p => p.Vacation)
                .Where(p => p.IdAgent == idAgent)
                .Where(p => p.Statut == true)
                .OrderByDescending(p => p.DateDuJour)
                .ToListAsync();
        }

        // ✅ POINTAGE AGENT: Récupérer les présences d'un agent pour une date
        public async Task<IEnumerable<Presence>> GetByAgentAndDateAsync(int idAgent, DateTime date)
        {
            return await _context.Presences
               // .Include(p => p.Vacation)
                .Where(p => p.IdAgent == idAgent && p.DateDuJour.Date == date.Date)
                .Where(p => p.Statut == true)
                .ToListAsync();
        }

        // ✅ FILTRAGE PAR TYPE: Récupérer les présences par type de présence
        public async Task<IEnumerable<Presence>> GetByTypePersonneAsync(string typePersonne)
        {
            return await _context.Presences
                .Include(p => p.Eleve)
                .Include(p => p.Agent)
               // .Include(p => p.Vacation)
                .Where(p => p.TypePresence == typePersonne)
                .Where(p => p.Statut == true)
                .OrderByDescending(p => p.DateDuJour)
                .ToListAsync();
        }

        // ✅ FILTRAGE PAR TYPE: Récupérer les présences par type et date
        public async Task<IEnumerable<Presence>> GetByTypePersonneAndDateAsync(string typePersonne, DateTime date)
        {
            return await _context.Presences
                .Include(p => p.Eleve)
                .Include(p => p.Agent)
              //  .Include(p => p.Vacation)
                .Where(p => p.TypePresence == typePersonne && p.DateDuJour.Date == date.Date)
                .Where(p => p.Statut == true)
                .OrderBy(p => p.HeureArrivee)
                .ToListAsync();
        }

        public async Task<Presence> CreateAsync(Presence presence)
        {
            // ✅ POINTAGE FLEXIBLE: Validation - Au moins un des deux doit être renseigné
            if (!presence.IdEleve.HasValue && !presence.IdAgent.HasValue)
            {
                throw new InvalidOperationException("Une présence doit concerner soit un élève, soit un agent.");
            }
            
            // ✅ POINTAGE FLEXIBLE: Validation - Pas les deux en même temps
            if (presence.IdEleve.HasValue && presence.IdAgent.HasValue)
            {
                throw new InvalidOperationException("Une présence ne peut pas concerner à la fois un élève et un agent.");
            }
            
            // ✅ BLOCAGE DOUBLE POINTAGE: Vérifier si la personne a déjà pointé aujourd'hui
            var hasAlreadyPointed = await HasAlreadyPointedTodayAsync(
                presence.IdEleve, 
                presence.IdAgent, 
                presence.DateDuJour
            );
            
            if (hasAlreadyPointed)
            {
                var existingPresence = await GetTodayPresenceAsync(
                    presence.IdEleve, 
                    presence.IdAgent, 
                    presence.DateDuJour
                );
                
                string personneType = presence.IdEleve.HasValue ? "L'élève" : "L'agent";
                string personneId = presence.IdEleve.HasValue 
                    ? $"ID: {presence.IdEleve}" 
                    : $"ID: {presence.IdAgent}";
                
                throw new InvalidOperationException(
                    $"{personneType} ({personneId}) a déjà pointé sa présence aujourd'hui " +
                    $"({presence.DateDuJour:dd/MM/yyyy}) à {existingPresence?.HeureArrivee:hh\\:mm}. " +
                    $"Un seul pointage par jour est autorisé."
                );
            }
            
            // ✅ TYPE DE PRÉSENCE: Remplissage automatique selon qui a pointé
            presence.TypePresence = presence.IdEleve.HasValue ? "ELEVE" : "AGENT";
            
            presence.DateCreation = DateTime.Now;
            
            _context.Presences.Add(presence);
            await _context.SaveChangesAsync();
            
            // 🔔 NOTIFICATION PUSH: Envoyer notification au tuteur si c'est un élève
            if (presence.IdEleve.HasValue)
            {
                await EnvoyerNotificationAuTuteurAsync(presence);
            }
            
            // 📊 DASHBOARD TEMPS RÉEL: Notifier la mise à jour du dashboard
            await NotifierMiseAJourDashboardAsync(presence);
            
            return presence;
        }

        public async Task<Presence> UpdateAsync(Presence presence)
        {
            var existingPresence = await _context.Presences.FindAsync(presence.IdPresence);
            if (existingPresence == null)
                return null;

            _context.Entry(existingPresence).CurrentValues.SetValues(presence);
            await _context.SaveChangesAsync();
            return existingPresence;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var presence = await _context.Presences.FindAsync(id);
            if (presence == null)
                return false;

            _context.Presences.Remove(presence);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Presences.AnyAsync(p => p.IdPresence == id);
        }

        // ✅ SOFT DELETE: Toggle le statut d'une présence (actif <-> inactif)
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var presence = await _context.Presences.FindAsync(id);
            if (presence == null)
                return false;

            presence.Statut = presence.Statut != true;
            await _context.SaveChangesAsync();
            return true;
        }

        // ✅ BLOCAGE DOUBLE POINTAGE: Vérifier si une personne a déjà pointé aujourd'hui
        public async Task<bool> HasAlreadyPointedTodayAsync(int? idEleve, int? idAgent, DateTime date)
        {
            var query = _context.Presences
                .Where(p => p.DateDuJour.Date == date.Date)
                .Where(p => p.Statut == true); // Seulement les présences actives

            if (idEleve.HasValue)
            {
                query = query.Where(p => p.IdEleve == idEleve.Value);
            }
            else if (idAgent.HasValue)
            {
                query = query.Where(p => p.IdAgent == idAgent.Value);
            }

            return await query.AnyAsync();
        }

        // ✅ BLOCAGE DOUBLE POINTAGE: Récupérer la présence du jour pour une personne
        public async Task<Presence?> GetTodayPresenceAsync(int? idEleve, int? idAgent, DateTime date)
        {
            var query = _context.Presences
                .Include(p => p.Eleve)
                .Include(p => p.Agent)
              //  .Include(p => p.Vacation)
                .Where(p => p.DateDuJour.Date == date.Date)
                .Where(p => p.Statut == true); // Seulement les présences actives

            if (idEleve.HasValue)
            {
                query = query.Where(p => p.IdEleve == idEleve.Value);
            }
            else if (idAgent.HasValue)
            {
                query = query.Where(p => p.IdAgent == idAgent.Value);
            }

            return await query.FirstOrDefaultAsync();
        }

        // 🔔 File d'attente: préparer et planifier l'envoi des notifications présence
        private async Task EnvoyerNotificationAuTuteurAsync(Presence presence, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!presence.IdEleve.HasValue)
                {
                    _logger.LogDebug("Notification présence ignorée car aucune liaison élève (presence {PresenceId})", presence.IdPresence);
                    return;
                }

                var preparation = await _notificationDispatcher.PreparePresenceAsync(presence.IdPresence, cancellationToken);
                if (preparation == null)
                {
                    return;
                }

                await _notificationJobQueue.EnqueueAsync(preparation, cancellationToken);
                _logger.LogInformation("🗂️ Notification présence {PresenceId} mise en file pour traitement", presence.IdPresence);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "❌ Erreur inattendue lors de la notification présence {PresenceId}",
                    presence.IdPresence);
            }
        }

        /// <summary>
        /// Notifier la mise à jour du dashboard en temps réel via SignalR
        /// </summary>
        private async Task NotifierMiseAJourDashboardAsync(Presence presence)
        {
            try
            {
                int? idEcole = null;

                // Récupérer l'idEcole selon le type de présence
                if (presence.IdEleve.HasValue)
                {
                    idEcole = await _inscriptionResolver.GetEcoleCouranteAsync(presence.IdEleve.Value);
                }
                else if (presence.IdAgent.HasValue)
                {
                    // Charger l'agent pour récupérer l'école
                    var agent = await _context.Agents
                        .FirstOrDefaultAsync(a => a.IdAgent == presence.IdAgent.Value);

                    if (agent != null)
                    {
                        idEcole = agent.IdEcole;
                    }
                }

                // Notifier la mise à jour du dashboard si on a trouvé l'école
                if (idEcole.HasValue)
                {
                    await _dashboardHubService.NotifyPresenceUpdateAsync(idEcole.Value);
                    _logger.LogInformation("📊 Notification dashboard présence envoyée pour l'école {IdEcole}", idEcole.Value);
                }
                else
                {
                    _logger.LogWarning("⚠️ Impossible de déterminer l'école pour la présence {PresenceId}", presence.IdPresence);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la notification dashboard pour la présence {PresenceId}", presence.IdPresence);
            }
        }
    }
}
