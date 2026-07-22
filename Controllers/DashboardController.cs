using KelasiNaBiso.Models.DTOs.Reporting;
using KelasiNaBiso.Models.DTOs.Paiement;
using RepartitionModeDto = KelasiNaBiso.Models.DTOs.Reporting.RepartitionModeDto;
using KelasiNaBiso.Services.Repositories;
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

        public DashboardController(
            IPresenceReportingService presenceReportingService,
            IPaiementRepository paiementRepository,
            KelasiNaBisoDbContext context,
            ILogger<DashboardController> logger,
            ICurrentUserService currentUserService)
        {
            _presenceReportingService = presenceReportingService;
            _paiementRepository = paiementRepository;
            _context = context;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// 📊 Dashboard global combiné (Présence + Paiement) pour une école
        /// </summary>
        /// <param name="idEcole">ID de l'école (requis)</param>
        /// <returns>Dashboard combiné avec présence et paiement sur un mois</returns>
        [HttpGet("global")]
        [ProducesResponseType(typeof(DashboardGlobalDto), 200)]
        public async Task<ActionResult<DashboardGlobalDto>> GetDashboardGlobal(
            [FromQuery] int idEcole)
        {
            try
            {
                _logger.LogInformation($"📊 Récupération du dashboard global pour l'école {idEcole}");

                // ✅ Période : Mois en cours (du 1er au dernier jour du mois)
                var aujourdhui = DateTime.Now;
                var debutMois = new DateTime(aujourdhui.Year, aujourdhui.Month, 1);
                var finMois = debutMois.AddMonths(1).AddDays(-1);

                // Récupérer le dashboard de présence (mois complet)
                var dashboardPresence = await _presenceReportingService.GetDashboardEcoleAsync(
                    idEcole, null, debutMois, finMois);

                // Récupérer le dashboard de paiement (mois en cours)
                var paiementService = _paiementRepository as KelasiNaBiso.Services.PaiementService;
                if (paiementService == null)
                {
                    return StatusCode(500, new { message = "Service de paiement non disponible" });
                }

                var dashboardPaiement = await paiementService.GetDashboardEcoleAsync(
                    idEcole, null, debutMois, finMois, "mois");

                // ✅ Calculer les statistiques générales (Classes, Élèves, Enseignants, Directions)
                var statistiques = await CalculerStatistiquesGeneralesAsync(idEcole);

                // ✅ Calculer la répartition des élèves (par direction, section, option)
                var repartitionEleves = await CalculerRepartitionElevesAsync(idEcole);

                // Combiner les deux dashboards
                var dashboardGlobal = new DashboardGlobalDto
                {
                    Ecole = dashboardPresence.Ecole,
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
            catch (Exception ex)
            {
                _logger.LogError($"❌ Erreur lors de la récupération du dashboard global : {ex.Message}");
                return StatusCode(500, new { message = "Erreur lors de la récupération du dashboard global", error = ex.Message });
            }
        }

        /// <summary>
        /// ✅ Helper : Récupère les requêtes de base pour compter les élèves d'une école
        /// Retourne deux requêtes : une pour le total (tous statuts) et une pour les actifs uniquement
        /// </summary>
        private (IQueryable<Eleve> Total, IQueryable<Eleve> Actifs) GetElevesQueries(int idEcole)
        {
            var baseQuery = _context.Eleves
                .Where(e => e.Classe != null && 
                           e.Classe.Direction != null && 
                           e.Classe.Direction.IdEcole == idEcole);
            
            var actifsQuery = baseQuery
                .Where(e => e.Statut == true);
            
            return (baseQuery, actifsQuery);
        }

        /// <summary>
        /// Calculer les statistiques générales de l'école (Classes, Élèves, Enseignants, Directions)
        /// </summary>
        private async Task<StatistiquesGeneralesDto> CalculerStatistiquesGeneralesAsync(int idEcole)
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
            var (elevesQueryTotal, elevesQueryActifs) = GetElevesQueries(idEcole);

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
        private async Task<RepartitionElevesDto> CalculerRepartitionElevesAsync(int idEcole)
        {
            // ✅ CORRECTION : Base query pour tous les élèves de l'école (tous statuts)
            var (elevesQueryBase, elevesQueryActifsBase) = GetElevesQueries(idEcole);
            
            // Ajouter les Includes pour les répartitions
            var elevesQueryTotal = elevesQueryBase
                .Include(e => e.Classe)
                    .ThenInclude(c => c.Direction)
                .Include(e => e.Classe)
                    .ThenInclude(c => c.Section)
                .Include(e => e.Classe)
                    .ThenInclude(c => c.Option)
                        .ThenInclude(o => o.Section);

            var elevesQueryActifs = elevesQueryActifsBase
                .Include(e => e.Classe)
                    .ThenInclude(c => c.Direction)
                .Include(e => e.Classe)
                    .ThenInclude(c => c.Section)
                .Include(e => e.Classe)
                    .ThenInclude(c => c.Option)
                        .ThenInclude(o => o.Section);

            var totalEleves = await elevesQueryTotal.CountAsync();
            var totalElevesActifs = await elevesQueryActifs.CountAsync();

            // ✅ CORRECTION : Répartition par Direction (utiliser elevesQueryActifs pour ne compter que les élèves actifs)
            var repartitionParDirection = await elevesQueryActifs
                .Where(e => e.Classe != null && e.Classe.Direction != null)
                .GroupBy(e => new
                {
                    IdDirection = e.Classe.Direction.IdDirection,
                    NomDirection = e.Classe.Direction.NomDirection
                })
                .Select(g => new RepartitionDirectionDto
                {
                    IdDirection = g.Key.IdDirection,
                    NomDirection = g.Key.NomDirection ?? "Non défini",
                    NombreEleves = g.Count(), // Tous les élèves de cette requête sont actifs
                    NombreElevesActifs = g.Count() // Identique car on filtre déjà sur les actifs
                })
                .ToListAsync();

            // Calculer les pourcentages pour les directions (basé sur le total d'élèves actifs)
            foreach (var direction in repartitionParDirection)
            {
                direction.Pourcentage = totalElevesActifs > 0 
                    ? Math.Round((decimal)direction.NombreEleves * 100 / totalElevesActifs, 2)
                    : 0;
            }

            // ✅ CORRECTION : Répartition par Section (utiliser elevesQueryActifs pour ne compter que les élèves actifs)
            var repartitionParSection = await elevesQueryActifs
                .Where(e => e.Classe != null && e.Classe.Section != null)
                .GroupBy(e => new
                {
                    IdSection = e.Classe.Section.IdSection,
                    NomSection = e.Classe.Section.NomSection
                })
                .Select(g => new RepartitionSectionDto
                {
                    IdSection = g.Key.IdSection,
                    NomSection = g.Key.NomSection ?? "Non défini",
                    NombreEleves = g.Count(), // Tous les élèves de cette requête sont actifs
                    NombreElevesActifs = g.Count() // Identique car on filtre déjà sur les actifs
                })
                .ToListAsync();

            // Calculer les pourcentages pour les sections (basé sur le total d'élèves actifs)
            foreach (var section in repartitionParSection)
            {
                section.Pourcentage = totalElevesActifs > 0 
                    ? Math.Round((decimal)section.NombreEleves * 100 / totalElevesActifs, 2)
                    : 0;
            }

            // ✅ CORRECTION : Répartition par Option (utiliser elevesQueryActifs pour ne compter que les élèves actifs)
            var repartitionParOption = await elevesQueryActifs
                .Where(e => e.Classe != null && e.Classe.Option != null)
                .GroupBy(e => new
                {
                    IdOption = e.Classe.Option.IdOption,
                    NomOption = e.Classe.Option.NomOption,
                    IdSection = e.Classe.Option.Section != null ? e.Classe.Option.Section.IdSection : (int?)null,
                    NomSection = e.Classe.Option.Section != null ? e.Classe.Option.Section.NomSection : null
                })
                .Select(g => new RepartitionOptionDto
                {
                    IdOption = g.Key.IdOption,
                    NomOption = g.Key.NomOption ?? "Non défini",
                    IdSection = g.Key.IdSection,
                    NomSection = g.Key.NomSection,
                    NombreEleves = g.Count(), // Tous les élèves de cette requête sont actifs
                    NombreElevesActifs = g.Count() // Identique car on filtre déjà sur les actifs
                })
                .ToListAsync();

            // Calculer les pourcentages pour les options (basé sur le total d'élèves actifs)
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
            [FromQuery] DateTime? dateFin = null)
        {
            try
            {
                var result = await _presenceReportingService.GetDashboardEcoleAsync(idEcole, date, dateDebut, dateFin);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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
            [FromQuery] string? periode = null)
        {
            try
            {
                var paiementService = _paiementRepository as KelasiNaBiso.Services.PaiementService;
                if (paiementService == null)
                {
                    return StatusCode(500, new { message = "Service de paiement non disponible" });
                }

                var result = await paiementService.GetDashboardEcoleAsync(idEcole, date, dateDebut, dateFin, periode);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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
            
            // ✅ CORRECTION : Requête de base SANS filtre Statut pour le total
            var elevesQueryBase = _context.Eleves
                .Where(e => e.Classe != null && 
                           e.Classe.Direction != null && 
                           e.Classe.Direction.IdEcole != null);
            
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
            // ✅ Base query pour tous les élèves de toutes les écoles (tous statuts)
            var elevesQueryBase = _context.Eleves
                .Where(e => e.Classe != null && 
                           e.Classe.Direction != null && 
                           e.Classe.Direction.IdEcole != null)
                .Include(e => e.Classe)
                    .ThenInclude(c => c.Direction)
                        .ThenInclude(d => d.Ecole);

            var totalEleves = await elevesQueryBase.CountAsync();

            // ✅ Répartition par École
            var repartitionParEcole = await elevesQueryBase
                .Where(e => e.Classe != null && 
                           e.Classe.Direction != null && 
                           e.Classe.Direction.Ecole != null)
                .GroupBy(e => new
                {
                    IdEcole = e.Classe.Direction.Ecole.IdEcole,
                    NomEcole = e.Classe.Direction.Ecole.Nom,
                    Province = e.Classe.Direction.Ecole.Province,
                    Ville = e.Classe.Direction.Ecole.Ville
                })
                .Select(g => new RepartitionEcoleDto
                {
                    IdEcole = g.Key.IdEcole,
                    NomEcole = g.Key.NomEcole ?? "Non défini",
                    Province = g.Key.Province,
                    Ville = g.Key.Ville,
                    NombreEleves = g.Count(),
                    NombreElevesActifs = g.Count(e => e.Statut == true),
                    NombreElevesFilles = g.Count(e => e.Genre != null && (e.Genre.ToLower() == "féminin" || e.Genre.ToLower() == "feminin" || e.Genre.ToLower() == "f")),
                    NombreElevesGarcons = g.Count(e => e.Genre != null && (e.Genre.ToLower() == "masculin" || e.Genre.ToLower() == "m")),
                    NombreElevesFillesActives = g.Count(e => e.Statut == true && e.Genre != null && (e.Genre.ToLower() == "féminin" || e.Genre.ToLower() == "feminin" || e.Genre.ToLower() == "f")),
                    NombreElevesGarconsActifs = g.Count(e => e.Statut == true && e.Genre != null && (e.Genre.ToLower() == "masculin" || e.Genre.ToLower() == "m"))
                })
                .ToListAsync();

            // Calculer les pourcentages pour les écoles
            foreach (var ecole in repartitionParEcole)
            {
                ecole.Pourcentage = totalEleves > 0 
                    ? Math.Round((decimal)ecole.NombreEleves * 100 / totalEleves, 2)
                    : 0;
                
                ecole.PourcentageFilles = ecole.NombreEleves > 0 
                    ? Math.Round((decimal)ecole.NombreElevesFilles * 100 / ecole.NombreEleves, 2)
                    : 0;
                
                ecole.PourcentageGarcons = ecole.NombreEleves > 0 
                    ? Math.Round((decimal)ecole.NombreElevesGarcons * 100 / ecole.NombreEleves, 2)
                    : 0;
            }

            // Retourner trié par nombre d'élèves décroissant
            return repartitionParEcole.OrderByDescending(e => e.NombreEleves).ToList();
        }

        /// <summary>
        /// Calculer les répartitions géographiques (école, province, ville)
        /// </summary>
        private async Task<(List<RepartitionEcoleDto>, List<RepartitionProvinceDto>, List<RepartitionVilleDto>)> CalculerRepartitionsGeographiquesAsync()
        {
            // ✅ CORRECTION : Base query pour tous les élèves de toutes les écoles (tous statuts)
            var elevesQueryBase = _context.Eleves
                .Where(e => e.Classe != null && 
                           e.Classe.Direction != null && 
                           e.Classe.Direction.IdEcole != null)
                .Include(e => e.Classe)
                    .ThenInclude(c => c.Direction)
                        .ThenInclude(d => d.Ecole);

            var elevesQueryActifs = elevesQueryBase
                .Where(e => e.Statut == true);

            var totalEleves = await elevesQueryBase.CountAsync();

            // ✅ CORRECTION : Répartition par École (utiliser elevesQueryBase pour le total)
            var repartitionParEcole = await elevesQueryBase
                .Where(e => e.Classe != null && 
                           e.Classe.Direction != null && 
                           e.Classe.Direction.Ecole != null)
                .GroupBy(e => new
                {
                    IdEcole = e.Classe.Direction.Ecole.IdEcole,
                    NomEcole = e.Classe.Direction.Ecole.Nom,
                    Province = e.Classe.Direction.Ecole.Province,
                    Ville = e.Classe.Direction.Ecole.Ville
                })
                .Select(g => new RepartitionEcoleDto
                {
                    IdEcole = g.Key.IdEcole,
                    NomEcole = g.Key.NomEcole ?? "Non défini",
                    Province = g.Key.Province,
                    Ville = g.Key.Ville,
                    NombreEleves = g.Count(),
                    NombreElevesActifs = g.Count(e => e.Statut == true),
                    NombreElevesFilles = g.Count(e => e.Genre != null && (e.Genre.ToLower() == "féminin" || e.Genre.ToLower() == "feminin" || e.Genre.ToLower() == "f")),
                    NombreElevesGarcons = g.Count(e => e.Genre != null && (e.Genre.ToLower() == "masculin" || e.Genre.ToLower() == "m")),
                    NombreElevesFillesActives = g.Count(e => e.Statut == true && e.Genre != null && (e.Genre.ToLower() == "féminin" || e.Genre.ToLower() == "feminin" || e.Genre.ToLower() == "f")),
                    NombreElevesGarconsActifs = g.Count(e => e.Statut == true && e.Genre != null && (e.Genre.ToLower() == "masculin" || e.Genre.ToLower() == "m"))
                })
                .ToListAsync();

            // Calculer les pourcentages pour les écoles
            foreach (var ecole in repartitionParEcole)
            {
                ecole.Pourcentage = totalEleves > 0 
                    ? Math.Round((decimal)ecole.NombreEleves * 100 / totalEleves, 2)
                    : 0;
                
                ecole.PourcentageFilles = ecole.NombreEleves > 0 
                    ? Math.Round((decimal)ecole.NombreElevesFilles * 100 / ecole.NombreEleves, 2)
                    : 0;
                
                ecole.PourcentageGarcons = ecole.NombreEleves > 0 
                    ? Math.Round((decimal)ecole.NombreElevesGarcons * 100 / ecole.NombreEleves, 2)
                    : 0;
            }

            // ✅ CORRECTION : Répartition par Province (utiliser elevesQueryBase pour le total)
            var repartitionParProvince = await elevesQueryBase
                .Where(e => !string.IsNullOrEmpty(e.Province))
                .GroupBy(e => new
                {
                    Province = e.Province
                })
                .Select(g => new RepartitionProvinceDto
                {
                    Province = g.Key.Province ?? "Non défini",
                    NombreEleves = g.Count(),
                    NombreElevesActifs = g.Count(e => e.Statut == true),
                    NombreElevesFilles = g.Count(e => e.Genre != null && (e.Genre.ToLower() == "féminin" || e.Genre.ToLower() == "feminin" || e.Genre.ToLower() == "f")),
                    NombreElevesGarcons = g.Count(e => e.Genre != null && (e.Genre.ToLower() == "masculin" || e.Genre.ToLower() == "m")),
                    NombreEcoles = g.Select(e => e.Classe.Direction.Ecole.IdEcole).Distinct().Count()
                })
                .ToListAsync();

            // Calculer les pourcentages pour les provinces
            foreach (var province in repartitionParProvince)
            {
                province.Pourcentage = totalEleves > 0 
                    ? Math.Round((decimal)province.NombreEleves * 100 / totalEleves, 2)
                    : 0;
                
                province.PourcentageFilles = province.NombreEleves > 0 
                    ? Math.Round((decimal)province.NombreElevesFilles * 100 / province.NombreEleves, 2)
                    : 0;
                
                province.PourcentageGarcons = province.NombreEleves > 0 
                    ? Math.Round((decimal)province.NombreElevesGarcons * 100 / province.NombreEleves, 2)
                    : 0;
            }

            // ✅ CORRECTION : Répartition par Ville (utiliser elevesQueryBase pour le total)
            var repartitionParVille = await elevesQueryBase
                .Where(e => !string.IsNullOrEmpty(e.Ville))
                .GroupBy(e => new
                {
                    Ville = e.Ville,
                    Province = e.Province
                })
                .Select(g => new RepartitionVilleDto
                {
                    Ville = g.Key.Ville ?? "Non défini",
                    Province = g.Key.Province,
                    NombreEleves = g.Count(),
                    NombreElevesActifs = g.Count(e => e.Statut == true),
                    NombreElevesFilles = g.Count(e => e.Genre != null && (e.Genre.ToLower() == "féminin" || e.Genre.ToLower() == "feminin" || e.Genre.ToLower() == "f")),
                    NombreElevesGarcons = g.Count(e => e.Genre != null && (e.Genre.ToLower() == "masculin" || e.Genre.ToLower() == "m")),
                    NombreEcoles = g.Select(e => e.Classe.Direction.Ecole.IdEcole).Distinct().Count()
                })
                .ToListAsync();

            // Calculer les pourcentages pour les villes
            foreach (var ville in repartitionParVille)
            {
                ville.Pourcentage = totalEleves > 0 
                    ? Math.Round((decimal)ville.NombreEleves * 100 / totalEleves, 2)
                    : 0;
                
                ville.PourcentageFilles = ville.NombreEleves > 0 
                    ? Math.Round((decimal)ville.NombreElevesFilles * 100 / ville.NombreEleves, 2)
                    : 0;
                
                ville.PourcentageGarcons = ville.NombreEleves > 0 
                    ? Math.Round((decimal)ville.NombreElevesGarcons * 100 / ville.NombreEleves, 2)
                    : 0;
            }

            return (
                repartitionParEcole.OrderByDescending(e => e.NombreEleves).ToList(),
                repartitionParProvince.OrderByDescending(p => p.NombreEleves).ToList(),
                repartitionParVille.OrderByDescending(v => v.NombreEleves).ToList()
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
