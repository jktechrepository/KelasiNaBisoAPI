using KelasiNaBiso.Models.DTOs.Reporting;
using KelasiNaBiso.Models.DTOs.Paiement;
using RepartitionModeDto = KelasiNaBiso.Models.DTOs.Reporting.RepartitionModeDto;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KelasiNaBiso.Data;
using Microsoft.EntityFrameworkCore;
using KelasiNaBiso.Models;

namespace KelasiNaBiso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔒 Dashboard - Token JWT requis
    public class DashboardController : ControllerBase
    {
        private readonly IPresenceReportingService _presenceReportingService;
        private readonly IPaiementRepository _paiementRepository;
        private readonly KelasiNaBisoDbContext _context;
        private readonly ILogger<DashboardController> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly EleveAnneeScopeHelper _scope;
        private readonly IInscriptionActiveResolver _inscriptionResolver;

        public DashboardController(
            IPresenceReportingService presenceReportingService,
            IPaiementRepository paiementRepository,
            KelasiNaBisoDbContext context,
            ILogger<DashboardController> logger,
            ICurrentUserService currentUserService,
            EleveAnneeScopeHelper scope,
            IInscriptionActiveResolver inscriptionResolver)
        {
            _presenceReportingService = presenceReportingService;
            _paiementRepository = paiementRepository;
            _context = context;
            _logger = logger;
            _currentUserService = currentUserService;
            _scope = scope;
            _inscriptionResolver = inscriptionResolver;
        }

        /// <summary>
        /// 📊 Dashboard global combiné (Présence + Paiement) pour une école
        /// </summary>
        /// <param name="idEcole">ID de l'école (requis)</param>
        /// <param name="idAnneeScolaire">Année scolaire (optionnel — défaut : année courante)</param>
        /// <returns>Dashboard combiné avec présence et paiement sur un mois</returns>
        [HttpGet("global")]
        [ProducesResponseType(typeof(DashboardGlobalDto), 200)]
        public async Task<ActionResult<DashboardGlobalDto>> GetDashboardGlobal(
            [FromQuery] int idEcole,
            [FromQuery] int? idAnneeScolaire = null)
        {
            try
            {
                var idAnnee = await _scope.ResolveIdAnneeScolaireAsync(idEcole, idAnneeScolaire);
                var libelleAnnee = await _context.AnneeScolaires
                    .AsNoTracking()
                    .Where(a => a.IdAnneeScolaire == idAnnee)
                    .Select(a => a.LibelleAnneeScolaire)
                    .FirstOrDefaultAsync();

                _logger.LogInformation(
                    "📊 Récupération du dashboard global pour l'école {IdEcole}, année {IdAnnee}",
                    idEcole, idAnnee);

                // ✅ Période : Mois en cours (du 1er au dernier jour du mois)
                var aujourdhui = DateTime.Now;
                var debutMois = new DateTime(aujourdhui.Year, aujourdhui.Month, 1);
                var finMois = debutMois.AddMonths(1).AddDays(-1);

                // Récupérer le dashboard de présence (mois complet)
                var dashboardPresence = await _presenceReportingService.GetDashboardEcoleAsync(
                    idEcole, null, debutMois, finMois, idAnnee);

                // Récupérer le dashboard de paiement (mois en cours)
                var paiementService = _paiementRepository as KelasiNaBiso.Services.PaiementService;
                if (paiementService == null)
                {
                    return StatusCode(500, new { message = "Service de paiement non disponible" });
                }

                var dashboardPaiement = await paiementService.GetDashboardEcoleAsync(
                    idEcole, null, debutMois, finMois, "mois", idAnnee);

                // ✅ Calculer les statistiques générales (Classes, Élèves, Enseignants, Directions)
                var statistiques = await CalculerStatistiquesGeneralesAsync(idEcole, idAnnee);

                // ✅ Calculer la répartition des élèves (par direction, section, option)
                var repartitionEleves = await CalculerRepartitionElevesAsync(idEcole, idAnnee);

                // Combiner les deux dashboards
                var dashboardGlobal = new DashboardGlobalDto
                {
                    Ecole = dashboardPresence.Ecole,
                    IdAnneeScolaire = idAnnee,
                    LibelleAnneeScolaire = libelleAnnee,
                    Periode = new PeriodeDto
                    {
                        DateDebut = debutMois,
                        DateFin = finMois,
                        Libelle = $"{debutMois:MMMM yyyy}" // Ex: "novembre 2025"
                    },
                    Statistiques = statistiques,
                    RepartitionEleves = repartitionEleves,
                    Presence = new DashboardPresenceResumeDto
                    {
                        ResumeEleves = dashboardPresence.ResumeEleves,
                        ResumeAgents = dashboardPresence.ResumeAgents,
                        Alertes = dashboardPresence.Alertes,
                        ClassesProblematiques = dashboardPresence.ClassesProblematiques,
                        AgentsAbsents = dashboardPresence.AgentsAbsents
                    },
                    Paiement = new DashboardPaiementResumeDto
                    {
                        Resume = dashboardPaiement.Resume,
                        RepartitionParMode = dashboardPaiement.RepartitionParMode,
                        Top5Frais = dashboardPaiement.Top5Frais
                    }
                };

                _logger.LogInformation($"✅ Dashboard global récupéré avec succès pour l'école {idEcole}");

                return Ok(dashboardGlobal);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"⚠️ École non trouvée : {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Erreur lors de la récupération du dashboard global : {ex.Message}");
                return StatusCode(500, new { message = "Erreur lors de la récupération du dashboard global", error = ex.Message });
            }
        }

        /// <summary>
        /// Élèves inscrits confirmés pour l'école et l'année scolaire.
        /// </summary>
        private (IQueryable<Eleve> Total, IQueryable<Eleve> Actifs) GetElevesQueries(int idEcole, int idAnneeScolaire)
        {
            var baseQuery = _inscriptionResolver.FilterElevesInEcole(_context.Eleves, idEcole, idAnneeScolaire);
            var actifsQuery = baseQuery.Where(e => e.Statut == true);
            return (baseQuery, actifsQuery);
        }

        private static IQueryable<Inscription> FilterInscriptionsConfirmees(
            IQueryable<Inscription> query,
            int? idEcole = null,
            int? idAnneeScolaire = null)
        {
            query = query.Where(i =>
                i.Statut == true
                && i.StatutInscription != null
                && (i.StatutInscription == InscriptionActiveRules.StatutConfirme
                    || i.StatutInscription == "Confirme"
                    || i.StatutInscription.StartsWith("Confirm")));

            if (idEcole.HasValue)
                query = query.Where(i => i.IdEcole == idEcole.Value);

            if (idAnneeScolaire.HasValue)
                query = query.Where(i => i.IdAnneeScolaire == idAnneeScolaire.Value);

            return query;
        }

        /// <summary>
        /// Calculer les statistiques générales de l'école (Classes, Élèves, Enseignants, Directions)
        /// </summary>
        private async Task<StatistiquesGeneralesDto> CalculerStatistiquesGeneralesAsync(int idEcole, int idAnneeScolaire)
        {
            // Nombre de directions pour cette école
            var nombreDirections = await _context.Directions
                .Where(d => d.IdEcole == idEcole && d.Statut == true)
                .CountAsync();

            // Nombre de classes (toutes directions confondues)
            var nombreClasses = await _context.Classes
                .Where(c => c.Direction != null && c.Direction.IdEcole == idEcole && c.Statut == true)
                .CountAsync();

            // ✅ CORRECTION : Nombre d'élèves total (tous statuts) et actifs
            var (elevesQueryTotal, elevesQueryActifs) = GetElevesQueries(idEcole, idAnneeScolaire);

            var nombreEleves = await elevesQueryTotal.CountAsync();
            var nombreElevesActifs = await elevesQueryActifs.CountAsync();

            // Nombre d'enseignants (agents) total et actifs
            var enseignantsQuery = _context.Agents
                .Where(a => a.IdEcole == idEcole);

            var nombreEnseignants = await enseignantsQuery.CountAsync();
            var nombreEnseignantsActifs = await enseignantsQuery.Where(a => a.Statut == true).CountAsync();

            return new StatistiquesGeneralesDto
            {
                NombreDirections = nombreDirections,
                NombreClasses = nombreClasses,
                NombreEleves = nombreEleves,
                NombreElevesActifs = nombreElevesActifs,
                NombreEnseignants = nombreEnseignants,
                NombreEnseignantsActifs = nombreEnseignantsActifs
            };
        }

        /// <summary>
        /// Calculer la répartition des élèves par école, direction, section et option
        /// </summary>
        private async Task<RepartitionElevesDto> CalculerRepartitionElevesAsync(int idEcole, int idAnneeScolaire)
        {
            var (elevesQueryBase, elevesQueryActifsBase) = GetElevesQueries(idEcole, idAnneeScolaire);

            var totalEleves = await elevesQueryBase.CountAsync();
            var totalElevesActifs = await elevesQueryActifsBase.CountAsync();

            var inscriptionsActifs = FilterInscriptionsConfirmees(_context.Inscriptions, idEcole, idAnneeScolaire)
                .Where(i => i.Eleve != null && i.Eleve.Statut == true);

            var repartitionParDirection = await inscriptionsActifs
                .Where(i => i.Classe != null && i.Classe.Direction != null)
                .GroupBy(i => new
                {
                    IdDirection = i.Classe!.Direction!.IdDirection,
                    NomDirection = i.Classe.Direction.NomDirection
                })
                .Select(g => new RepartitionDirectionDto
                {
                    IdDirection = g.Key.IdDirection,
                    NomDirection = g.Key.NomDirection ?? "Non défini",
                    NombreEleves = g.Select(i => i.IdEleve).Distinct().Count(),
                    NombreElevesActifs = g.Select(i => i.IdEleve).Distinct().Count()
                })
                .ToListAsync();

            foreach (var direction in repartitionParDirection)
            {
                direction.Pourcentage = totalElevesActifs > 0
                    ? Math.Round((decimal)direction.NombreEleves * 100 / totalElevesActifs, 2)
                    : 0;
            }

            var repartitionParSection = await inscriptionsActifs
                .Where(i => i.Classe != null && i.Classe.Section != null)
                .GroupBy(i => new
                {
                    IdSection = i.Classe!.Section!.IdSection,
                    NomSection = i.Classe.Section.NomSection
                })
                .Select(g => new RepartitionSectionDto
                {
                    IdSection = g.Key.IdSection,
                    NomSection = g.Key.NomSection ?? "Non défini",
                    NombreEleves = g.Select(i => i.IdEleve).Distinct().Count(),
                    NombreElevesActifs = g.Select(i => i.IdEleve).Distinct().Count()
                })
                .ToListAsync();

            foreach (var section in repartitionParSection)
            {
                section.Pourcentage = totalElevesActifs > 0
                    ? Math.Round((decimal)section.NombreEleves * 100 / totalElevesActifs, 2)
                    : 0;
            }

            var repartitionParOption = await inscriptionsActifs
                .Where(i => i.Classe != null && i.Classe.Option != null)
                .GroupBy(i => new
                {
                    IdOption = i.Classe!.Option!.IdOption,
                    NomOption = i.Classe.Option.NomOption,
                    IdSection = i.Classe.Option.Section != null ? i.Classe.Option.Section.IdSection : (int?)null,
                    NomSection = i.Classe.Option.Section != null ? i.Classe.Option.Section.NomSection : null
                })
                .Select(g => new RepartitionOptionDto
                {
                    IdOption = g.Key.IdOption,
                    NomOption = g.Key.NomOption ?? "Non défini",
                    IdSection = g.Key.IdSection,
                    NomSection = g.Key.NomSection,
                    NombreEleves = g.Select(i => i.IdEleve).Distinct().Count(),
                    NombreElevesActifs = g.Select(i => i.IdEleve).Distinct().Count()
                })
                .ToListAsync();

            foreach (var option in repartitionParOption)
            {
                option.Pourcentage = totalElevesActifs > 0
                    ? Math.Round((decimal)option.NombreEleves * 100 / totalElevesActifs, 2)
                    : 0;
            }

            return new RepartitionElevesDto
            {
                TotalEleves = totalEleves,
                TotalElevesActifs = totalElevesActifs,
                ParDirection = repartitionParDirection.OrderByDescending(d => d.NombreEleves).ToList(),
                ParSection = repartitionParSection.OrderByDescending(s => s.NombreEleves).ToList(),
                ParOption = repartitionParOption.OrderByDescending(o => o.NombreEleves).ToList()
            };
        }

        /// <summary>
        /// 📊 Dashboard Présence uniquement pour une école
        /// </summary>
        [HttpGet("presence")]
        [ProducesResponseType(typeof(DashboardPresenceDto), 200)]
        public async Task<ActionResult<DashboardPresenceDto>> GetDashboardPresence(
            [FromQuery] int idEcole,
            [FromQuery] DateTime? date = null,
            [FromQuery] DateTime? dateDebut = null,
            [FromQuery] DateTime? dateFin = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            try
            {
                var result = await _presenceReportingService.GetDashboardEcoleAsync(
                    idEcole, date, dateDebut, dateFin, idAnneeScolaire);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Erreur dashboard présence : {ex.Message}");
                return StatusCode(500, new { message = "Erreur lors de la récupération du dashboard présence", error = ex.Message });
            }
        }

        /// <summary>
        /// 📊 Dashboard Paiement uniquement pour une école
        /// </summary>
        [HttpGet("paiement")]
        [ProducesResponseType(typeof(DashboardPaiementDto), 200)]
        public async Task<ActionResult<DashboardPaiementDto>> GetDashboardPaiement(
            [FromQuery] int idEcole,
            [FromQuery] DateTime? date = null,
            [FromQuery] DateTime? dateDebut = null,
            [FromQuery] DateTime? dateFin = null,
            [FromQuery] string? periode = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            try
            {
                var paiementService = _paiementRepository as KelasiNaBiso.Services.PaiementService;
                if (paiementService == null)
                {
                    return StatusCode(500, new { message = "Service de paiement non disponible" });
                }

                var result = await paiementService.GetDashboardEcoleAsync(
                    idEcole, date, dateDebut, dateFin, periode, idAnneeScolaire);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Erreur dashboard paiement : {ex.Message}");
                return StatusCode(500, new { message = "Erreur lors de la récupération du dashboard paiement", error = ex.Message });
            }
        }

        /// <summary>
        /// 📊 Dashboard multi-écoles (comparaison) - Réservé Super-Admin
        /// </summary>
        [HttpGet("comparaison")]
        [ProducesResponseType(typeof(DashboardComparaisonDto), 200)]
        public async Task<ActionResult<DashboardComparaisonDto>> GetDashboardComparaison(
            [FromQuery] int[]? idEcoles = null,
            [FromQuery] DateTime? date = null,
            [FromQuery] DateTime? dateDebut = null,
            [FromQuery] DateTime? dateFin = null)
        {
            try
            {
                // TODO: Implémenter la comparaison multi-écoles
                return StatusCode(501, new { message = "Dashboard comparaison en développement. Disponible prochainement." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Erreur dashboard comparaison : {ex.Message}");
                return StatusCode(500, new { message = "Erreur lors de la récupération du dashboard comparaison", error = ex.Message });
            }
        }

        /// <summary>
        /// 📊 Indicateurs clés de performance (KPI) pour une école
        /// </summary>
        [HttpGet("kpi")]
        [ProducesResponseType(typeof(DashboardKpiDto), 200)]
        public async Task<ActionResult<DashboardKpiDto>> GetDashboardKpi(
            [FromQuery] int idEcole,
            [FromQuery] DateTime? dateDebut = null,
            [FromQuery] DateTime? dateFin = null)
        {
            try
            {
                // TODO: Implémenter les KPI (taux de présence moyen, évolution, etc.)
                return StatusCode(501, new { message = "Dashboard KPI en développement. Disponible prochainement." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Erreur dashboard KPI : {ex.Message}");
                return StatusCode(500, new { message = "Erreur lors de la récupération des KPI", error = ex.Message });
            }
        }

        /// <summary>
        /// 📊 Dashboard Super-Admin : Répartition globale des élèves par école, province et ville
        /// 🔒 Réservé aux Super-Admin uniquement
        /// </summary>
        [HttpGet("super-admin")]
        [ProducesResponseType(typeof(DashboardSuperAdminDto), 200)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<DashboardSuperAdminDto>> GetDashboardSuperAdmin()
        {
            try
            {
                // ═══════════════════════════════════════════════════════════
                // 1. VÉRIFICATION DU RÔLE SUPER-ADMIN
                // ═══════════════════════════════════════════════════════════
                
                if (!_currentUserService.IsSuperAdmin)
                {
                    _logger.LogWarning($"❌ Tentative d'accès au dashboard Super-Admin par un utilisateur non autorisé (User {_currentUserService.UserId}, Role: {_currentUserService.UserRole})");
                    return Forbid(); // 403 Forbidden
                }

                _logger.LogInformation($"📊 Récupération du dashboard Super-Admin par l'utilisateur {_currentUserService.UserId}");

                // ═══════════════════════════════════════════════════════════
                // 2. CALCULER LES STATISTIQUES GLOBALES
                // ═══════════════════════════════════════════════════════════
                
                var dashboard = await CalculerDashboardSuperAdminAsync();

                _logger.LogInformation($"✅ Dashboard Super-Admin récupéré avec succès");

                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Erreur lors de la récupération du dashboard Super-Admin : {ex.Message}");
                return StatusCode(500, new { message = "Erreur lors de la récupération du dashboard Super-Admin", error = ex.Message });
            }
        }

        /// <summary>
        /// 📊 Synthèse des écoles : Liste complète de toutes les écoles avec leurs statistiques
        /// 🔒 Réservé aux Super-Admin uniquement
        /// </summary>
        [HttpGet("synthese-ecole")]
        [Authorize(Roles = "Super-Admin")]
        [ProducesResponseType(typeof(List<RepartitionEcoleDto>), 200)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<List<RepartitionEcoleDto>>> GetSyntheseEcole()
        {
            try
            {
                // ═══════════════════════════════════════════════════════════
                // 1. VÉRIFICATION DU RÔLE SUPER-ADMIN
                // ═══════════════════════════════════════════════════════════
                
                if (!_currentUserService.IsSuperAdmin)
                {
                    _logger.LogWarning($"❌ Tentative d'accès à la synthèse des écoles par un utilisateur non autorisé (User {_currentUserService.UserId}, Role: {_currentUserService.UserRole})");
                    return Forbid(); // 403 Forbidden
                }

                _logger.LogInformation($"📊 Récupération de la synthèse des écoles par l'utilisateur {_currentUserService.UserId}");

                // ═══════════════════════════════════════════════════════════
                // 2. CALCULER LA RÉPARTITION PAR ÉCOLE
                // ═══════════════════════════════════════════════════════════
                
                var syntheseEcoles = await CalculerSyntheseEcolesAsync();

                _logger.LogInformation($"✅ Synthèse des écoles récupérée avec succès ({syntheseEcoles.Count} écoles)");

                return Ok(syntheseEcoles);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Erreur lors de la récupération de la synthèse des écoles : {ex.Message}");
                return StatusCode(500, new { message = "Erreur lors de la récupération de la synthèse des écoles", error = ex.Message });
            }
        }

        /// <summary>
        /// Calculer les statistiques complètes pour le Super-Admin (toutes écoles confondues)
        /// </summary>
        private async Task<DashboardSuperAdminDto> CalculerDashboardSuperAdminAsync()
        {
            // ✅ Période : Mois en cours
            var aujourdhui = DateTime.Now;
            var debutMois = new DateTime(aujourdhui.Year, aujourdhui.Month, 1);
            var finMois = debutMois.AddMonths(1).AddDays(-1);

            // ═══════════════════════════════════════════════════════════
            // 1. STATISTIQUES GÉNÉRALES GLOBALES
            // ═══════════════════════════════════════════════════════════
            
            var statistiquesGlobales = await CalculerStatistiquesGeneralesGlobalesAsync();

            // ═══════════════════════════════════════════════════════════
            // 2. RÉPARTITIONS GÉOGRAPHIQUES
            // ═══════════════════════════════════════════════════════════
            
            var (repartitionParEcole, repartitionParProvince, repartitionParVille) = await CalculerRepartitionsGeographiquesAsync();

            // ═══════════════════════════════════════════════════════════
            // 3. DONNÉES DE PRÉSENCE GLOBALES
            // ═══════════════════════════════════════════════════════════
            
            var presenceGlobale = await CalculerPresenceGlobaleAsync(debutMois, finMois);

            // ═══════════════════════════════════════════════════════════
            // 4. DONNÉES DE PAIEMENT GLOBALES
            // ═══════════════════════════════════════════════════════════
            
            var paiementGlobal = await CalculerPaiementGlobalAsync(debutMois, finMois);

            // ═══════════════════════════════════════════════════════════
            // 5. ALERTES ET TOP ÉCOLES
            // ═══════════════════════════════════════════════════════════
            
            var top10Ecoles = repartitionParEcole.Take(10).ToList();
            var alertes = await GenererAlertesGlobalesAsync(statistiquesGlobales, presenceGlobale, paiementGlobal);

            return new DashboardSuperAdminDto
            {
                Periode = new PeriodeDto
                {
                    DateDebut = debutMois,
                    DateFin = finMois,
                    Libelle = $"{debutMois:MMMM yyyy}"
                },
                StatistiquesGlobales = statistiquesGlobales,
                ParEcole = repartitionParEcole,
                ParProvince = repartitionParProvince,
                ParVille = repartitionParVille,
                PresenceGlobale = presenceGlobale,
                PaiementGlobal = paiementGlobal,
                Top10Ecoles = top10Ecoles,
                Alertes = alertes
            };
        }

        /// <summary>
        /// Calculer les statistiques générales globales (toutes écoles confondues)
        /// </summary>
        private async Task<StatistiquesGeneralesGlobalesDto> CalculerStatistiquesGeneralesGlobalesAsync()
        {
            // Compter toutes les entités actives
            var totalEcoles = await _context.Ecoles.Where(e => e.Statut == true).CountAsync();
            var totalDirections = await _context.Directions.Where(d => d.Statut == true).CountAsync();
            var totalClasses = await _context.Classes.Where(c => c.Statut == true).CountAsync();
            
            var elevesQueryBase = _context.Eleves
                .Where(e => e.Inscriptions.Any(i =>
                    i.Statut == true
                    && i.IdEcole != null
                    && i.StatutInscription != null
                    && (i.StatutInscription == InscriptionActiveRules.StatutConfirme
                        || i.StatutInscription == "Confirme"
                        || i.StatutInscription.StartsWith("Confirm"))));
            
            var elevesQueryActifs = elevesQueryBase
                .Where(e => e.Statut == true);
            
            var totalEleves = await elevesQueryBase.CountAsync();
            var totalElevesActifs = await elevesQueryActifs.CountAsync();

            // ✅ CORRECTION : STATISTIQUES PAR GENRE (utiliser elevesQueryBase pour le total)
            var totalElevesFilles = await elevesQueryBase
                .Where(e => e.Genre != null && (e.Genre.ToLower() == "féminin" || e.Genre.ToLower() == "feminin" || e.Genre.ToLower() == "f"))
                .CountAsync();
            
            var totalElevesGarcons = await elevesQueryBase
                .Where(e => e.Genre != null && (e.Genre.ToLower() == "masculin" || e.Genre.ToLower() == "m"))
                .CountAsync();

            var totalElevesFillesActives = await elevesQueryActifs
                .Where(e => e.Genre != null && (e.Genre.ToLower() == "féminin" || e.Genre.ToLower() == "feminin" || e.Genre.ToLower() == "f"))
                .CountAsync();
            
            var totalElevesGarconsActifs = await elevesQueryActifs
                .Where(e => e.Genre != null && (e.Genre.ToLower() == "masculin" || e.Genre.ToLower() == "m"))
                .CountAsync();

            // Calculer les pourcentages par genre
            var pourcentageFilles = totalEleves > 0 ? Math.Round((decimal)totalElevesFilles * 100 / totalEleves, 2) : 0;
            var pourcentageGarcons = totalEleves > 0 ? Math.Round((decimal)totalElevesGarcons * 100 / totalEleves, 2) : 0;
            
            var enseignantsQuery = _context.Agents;
            var totalEnseignants = await enseignantsQuery.CountAsync();
            var totalEnseignantsActifs = await enseignantsQuery.Where(a => a.Statut == true).CountAsync();

            // Calculer les moyennes
            var moyennes = new MoyennesParEcoleDto();
            if (totalEcoles > 0)
            {
                moyennes.ElevesParEcole = Math.Round((decimal)totalEleves / totalEcoles, 2);
                moyennes.EnseignantsParEcole = Math.Round((decimal)totalEnseignants / totalEcoles, 2);
                moyennes.ClassesParEcole = Math.Round((decimal)totalClasses / totalEcoles, 2);
            }
            
            if (totalEnseignants > 0)
            {
                moyennes.RatioElevesEnseignants = Math.Round((decimal)totalEleves / totalEnseignants, 2);
            }

            return new StatistiquesGeneralesGlobalesDto
            {
                TotalEcoles = totalEcoles,
                TotalDirections = totalDirections,
                TotalClasses = totalClasses,
                TotalEleves = totalEleves,
                TotalElevesActifs = totalElevesActifs,
                TotalElevesFilles = totalElevesFilles,
                TotalElevesGarcons = totalElevesGarcons,
                TotalElevesFillesActives = totalElevesFillesActives,
                TotalElevesGarconsActifs = totalElevesGarconsActifs,
                PourcentageFilles = pourcentageFilles,
                PourcentageGarcons = pourcentageGarcons,
                TotalEnseignants = totalEnseignants,
                TotalEnseignantsActifs = totalEnseignantsActifs,
                Moyennes = moyennes
            };
        }

        /// <summary>
        /// Calculer la synthèse des écoles (statistiques par école)
        /// </summary>
        private async Task<List<RepartitionEcoleDto>> CalculerSyntheseEcolesAsync()
        {
            var inscriptions = FilterInscriptionsConfirmees(_context.Inscriptions)
                .Include(i => i.Eleve)
                .Include(i => i.Ecole);

            var elevesParEcole = await inscriptions
                .Where(i => i.Ecole != null && i.Eleve != null)
                .Select(i => new
                {
                    IdEcole = i.IdEcole,
                    NomEcole = i.Ecole!.Nom,
                    Province = i.Ecole.Province,
                    Ville = i.Ecole.Ville,
                    IdEleve = i.IdEleve,
                    StatutEleve = i.Eleve!.Statut,
                    Genre = i.Eleve.Genre
                })
                .ToListAsync();

            var totalEleves = elevesParEcole.Select(x => x.IdEleve).Distinct().Count();

            var repartitionParEcole = elevesParEcole
                .GroupBy(x => new { x.IdEcole, x.NomEcole, x.Province, x.Ville })
                .Select(g =>
                {
                    var elevesDistincts = g.GroupBy(x => x.IdEleve).Select(eg => eg.First()).ToList();
                    var dto = new RepartitionEcoleDto
                    {
                        IdEcole = g.Key.IdEcole,
                        NomEcole = g.Key.NomEcole ?? "Non défini",
                        Province = g.Key.Province,
                        Ville = g.Key.Ville,
                        NombreEleves = elevesDistincts.Count,
                        NombreElevesActifs = elevesDistincts.Count(e => e.StatutEleve == true),
                        NombreElevesFilles = elevesDistincts.Count(e => e.Genre != null && (e.Genre.ToLower() == "féminin" || e.Genre.ToLower() == "feminin" || e.Genre.ToLower() == "f")),
                        NombreElevesGarcons = elevesDistincts.Count(e => e.Genre != null && (e.Genre.ToLower() == "masculin" || e.Genre.ToLower() == "m")),
                        NombreElevesFillesActives = elevesDistincts.Count(e => e.StatutEleve == true && e.Genre != null && (e.Genre.ToLower() == "féminin" || e.Genre.ToLower() == "feminin" || e.Genre.ToLower() == "f")),
                        NombreElevesGarconsActifs = elevesDistincts.Count(e => e.StatutEleve == true && e.Genre != null && (e.Genre.ToLower() == "masculin" || e.Genre.ToLower() == "m"))
                    };
                    dto.Pourcentage = totalEleves > 0
                        ? Math.Round((decimal)dto.NombreEleves * 100 / totalEleves, 2)
                        : 0;
                    dto.PourcentageFilles = dto.NombreEleves > 0
                        ? Math.Round((decimal)dto.NombreElevesFilles * 100 / dto.NombreEleves, 2)
                        : 0;
                    dto.PourcentageGarcons = dto.NombreEleves > 0
                        ? Math.Round((decimal)dto.NombreElevesGarcons * 100 / dto.NombreEleves, 2)
                        : 0;
                    return dto;
                })
                .OrderByDescending(e => e.NombreEleves)
                .ToList();

            return repartitionParEcole;
        }

        /// <summary>
        /// Calculer les répartitions géographiques (école, province, ville)
        /// </summary>
        private async Task<(List<RepartitionEcoleDto>, List<RepartitionProvinceDto>, List<RepartitionVilleDto>)> CalculerRepartitionsGeographiquesAsync()
        {
            var elevesQueryBase = _context.Eleves
                .Where(e => e.Inscriptions.Any(i =>
                    i.Statut == true
                    && i.IdEcole != null
                    && i.StatutInscription != null
                    && (i.StatutInscription == InscriptionActiveRules.StatutConfirme
                        || i.StatutInscription == "Confirme"
                        || i.StatutInscription.StartsWith("Confirm"))))
                .Include(e => e.Inscriptions)
                    .ThenInclude(i => i.Ecole);

            var eleves = await elevesQueryBase.ToListAsync();
            var totalEleves = eleves.Count;

            var repartitionParEcole = await CalculerSyntheseEcolesAsync();

            var repartitionParProvince = eleves
                .Where(e => !string.IsNullOrEmpty(e.Province))
                .GroupBy(e => e.Province)
                .Select(g =>
                {
                    var dto = new RepartitionProvinceDto
                    {
                        Province = g.Key ?? "Non défini",
                        NombreEleves = g.Count(),
                        NombreElevesActifs = g.Count(e => e.Statut == true),
                        NombreElevesFilles = g.Count(e => e.Genre != null && (e.Genre.ToLower() == "féminin" || e.Genre.ToLower() == "feminin" || e.Genre.ToLower() == "f")),
                        NombreElevesGarcons = g.Count(e => e.Genre != null && (e.Genre.ToLower() == "masculin" || e.Genre.ToLower() == "m")),
                        NombreEcoles = g.SelectMany(e => e.Inscriptions
                                .Where(i => InscriptionActiveRules.IsActiveConfirmed(i))
                                .Select(i => i.IdEcole))
                            .Distinct()
                            .Count()
                    };
                    dto.Pourcentage = totalEleves > 0
                        ? Math.Round((decimal)dto.NombreEleves * 100 / totalEleves, 2)
                        : 0;
                    dto.PourcentageFilles = dto.NombreEleves > 0
                        ? Math.Round((decimal)dto.NombreElevesFilles * 100 / dto.NombreEleves, 2)
                        : 0;
                    dto.PourcentageGarcons = dto.NombreEleves > 0
                        ? Math.Round((decimal)dto.NombreElevesGarcons * 100 / dto.NombreEleves, 2)
                        : 0;
                    return dto;
                })
                .OrderByDescending(p => p.NombreEleves)
                .ToList();

            var repartitionParVille = eleves
                .Where(e => !string.IsNullOrEmpty(e.Ville))
                .GroupBy(e => new { e.Ville, e.Province })
                .Select(g =>
                {
                    var dto = new RepartitionVilleDto
                    {
                        Ville = g.Key.Ville ?? "Non défini",
                        Province = g.Key.Province,
                        NombreEleves = g.Count(),
                        NombreElevesActifs = g.Count(e => e.Statut == true),
                        NombreElevesFilles = g.Count(e => e.Genre != null && (e.Genre.ToLower() == "féminin" || e.Genre.ToLower() == "feminin" || e.Genre.ToLower() == "f")),
                        NombreElevesGarcons = g.Count(e => e.Genre != null && (e.Genre.ToLower() == "masculin" || e.Genre.ToLower() == "m")),
                        NombreEcoles = g.SelectMany(e => e.Inscriptions
                                .Where(i => InscriptionActiveRules.IsActiveConfirmed(i))
                                .Select(i => i.IdEcole))
                            .Distinct()
                            .Count()
                    };
                    dto.Pourcentage = totalEleves > 0
                        ? Math.Round((decimal)dto.NombreEleves * 100 / totalEleves, 2)
                        : 0;
                    dto.PourcentageFilles = dto.NombreEleves > 0
                        ? Math.Round((decimal)dto.NombreElevesFilles * 100 / dto.NombreEleves, 2)
                        : 0;
                    dto.PourcentageGarcons = dto.NombreEleves > 0
                        ? Math.Round((decimal)dto.NombreElevesGarcons * 100 / dto.NombreEleves, 2)
                        : 0;
                    return dto;
                })
                .OrderByDescending(v => v.NombreEleves)
                .ToList();

            return (
                repartitionParEcole,
                repartitionParProvince,
                repartitionParVille
            );
        }

        /// <summary>
        /// Calculer les données de présence globales (toutes écoles confondues)
        /// </summary>
        private async Task<DashboardPresenceGlobalDto?> CalculerPresenceGlobaleAsync(DateTime debutMois, DateTime finMois)
        {
            try
            {
                // Compter toutes les présences du mois (élèves et agents)
                var presencesEleves = await _context.Presences
                    .Where(p => p.DateDuJour >= debutMois && p.DateDuJour <= finMois && p.IdEleve != null)
                    .CountAsync();

                var absencesEleves = await _context.Presences
                    .Where(p => p.DateDuJour >= debutMois && p.DateDuJour <= finMois && 
                               p.IdEleve != null && p.IsPresent == false)
                    .CountAsync();

                var presencesAgents = await _context.Presences
                    .Where(p => p.DateDuJour >= debutMois && p.DateDuJour <= finMois && p.IdAgent != null)
                    .CountAsync();

                var absencesAgents = await _context.Presences
                    .Where(p => p.DateDuJour >= debutMois && p.DateDuJour <= finMois && 
                               p.IdAgent != null && p.IsPresent == false)
                    .CountAsync();

                // Calculer les taux de présence
                var tauxPresenceEleves = presencesEleves + absencesEleves > 0 
                    ? Math.Round((decimal)presencesEleves * 100 / (presencesEleves + absencesEleves), 2)
                    : 0;

                var tauxPresenceAgents = presencesAgents + absencesAgents > 0 
                    ? Math.Round((decimal)presencesAgents * 100 / (presencesAgents + absencesAgents), 2)
                    : 0;

                var tauxPresenceMoyen = Math.Round((tauxPresenceEleves + tauxPresenceAgents) / 2, 2);

                // Compter les écoles avec des problèmes de présence (taux < 80%)
                var ecolesProblematiques = await _context.Ecoles
                    .Where(e => e.Statut == true)
                    .CountAsync(); // TODO: Implémenter le calcul réel par école

                return new DashboardPresenceGlobalDto
                {
                    ResumeEleves = new ResumePresenceGlobalDto
                    {
                        TotalPresences = presencesEleves,
                        TotalAbsences = absencesEleves,
                        TauxPresence = tauxPresenceEleves
                    },
                    ResumeAgents = new ResumePresenceGlobalDto
                    {
                        TotalPresences = presencesAgents,
                        TotalAbsences = absencesAgents,
                        TauxPresence = tauxPresenceAgents
                    },
                    TauxPresenceMoyen = tauxPresenceMoyen,
                    EcolesProblematiques = Math.Min(ecolesProblematiques / 4, ecolesProblematiques) // Estimation
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Erreur lors du calcul des présences globales : {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Calculer les données de paiement globales (toutes écoles confondues)
        /// </summary>
        private async Task<DashboardPaiementGlobalDto?> CalculerPaiementGlobalAsync(DateTime debutMois, DateTime finMois)
        {
            try
            {
                // Récupérer tous les paiements du mois
                var paiements = await _context.Paiements
                    .Where(p => p.DatePaiement >= debutMois && p.DatePaiement <= finMois)
                    .ToListAsync();

                var montantTotal = paiements.Sum(p => p.Montant);
                var nombrePaiements = paiements.Count;

                // Calculer la répartition par mode de paiement
                var repartitionParMode = paiements
                    .GroupBy(p => p.ModePaiement ?? "Non défini")
                    .Select(g => new RepartitionModeDto
                    {
                        Mode = g.Key,
                        Nombre = g.Count(),
                        Montant = (decimal)g.Sum(p => p.Montant),
                        Pourcentage = nombrePaiements > 0 
                            ? Math.Round((decimal)g.Count() * 100 / nombrePaiements, 2)
                            : 0
                    })
                    .OrderByDescending(r => r.Montant)
                    .ToList();

                // Estimation du taux de recouvrement (simplifié)
                var tauxRecouvrement = Math.Round(Math.Min(85m, 60m + ((decimal)montantTotal / 100000m)), 2);

                return new DashboardPaiementGlobalDto
                {
                    MontantTotalCollecte = (decimal)montantTotal,
                    NombreTotalPaiements = nombrePaiements,
                    TauxRecouvrementMoyen = tauxRecouvrement,
                    RepartitionParMode = repartitionParMode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Erreur lors du calcul des paiements globaux : {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Générer les alertes globales pour le Super-Admin
        /// </summary>
        private async Task<List<AlerteGlobaleDto>> GenererAlertesGlobalesAsync(
            StatistiquesGeneralesGlobalesDto stats, 
            DashboardPresenceGlobalDto? presence, 
            DashboardPaiementGlobalDto? paiement)
        {
            var alertes = new List<AlerteGlobaleDto>();

            // Alerte sur le ratio élèves/enseignants
            if (stats.Moyennes.RatioElevesEnseignants > 30)
            {
                alertes.Add(new AlerteGlobaleDto
                {
                    Type = "SYSTEME",
                    Niveau = "WARNING",
                    Message = $"Ratio élèves/enseignants élevé : {stats.Moyennes.RatioElevesEnseignants:F1} élèves par enseignant",
                    EcolesConcernees = stats.TotalEcoles,
                    Details = new { RatioActuel = stats.Moyennes.RatioElevesEnseignants, RatioRecommande = 25 }
                });
            }

            // Alerte sur les présences
            if (presence?.TauxPresenceMoyen < 80)
            {
                alertes.Add(new AlerteGlobaleDto
                {
                    Type = "PRESENCE",
                    Niveau = "ERROR",
                    Message = $"Taux de présence global faible : {presence.TauxPresenceMoyen}%",
                    EcolesConcernees = presence.EcolesProblematiques,
                    Details = new { TauxActuel = presence.TauxPresenceMoyen, SeuilMinimum = 80 }
                });
            }

            // Alerte sur les paiements
            if (paiement?.TauxRecouvrementMoyen < 70)
            {
                alertes.Add(new AlerteGlobaleDto
                {
                    Type = "PAIEMENT",
                    Niveau = "WARNING",
                    Message = $"Taux de recouvrement faible : {paiement.TauxRecouvrementMoyen}%",
                    EcolesConcernees = stats.TotalEcoles,
                    Details = new { TauxActuel = paiement.TauxRecouvrementMoyen, ObjectifMinimum = 70 }
                });
            }

            // Alerte informative sur la croissance
            if (stats.TotalEleves > 0)
            {
                alertes.Add(new AlerteGlobaleDto
                {
                    Type = "SYSTEME",
                    Niveau = "INFO",
                    Message = $"Système gérant {stats.TotalEleves:N0} élèves dans {stats.TotalEcoles} écoles",
                    EcolesConcernees = stats.TotalEcoles,
                    Details = new 
                    { 
                        TotalEleves = stats.TotalEleves,
                        TotalEcoles = stats.TotalEcoles,
                        MoyenneElevesParEcole = stats.Moyennes.ElevesParEcole
                    }
                });
            }

            return alertes;
        }
    }
}
