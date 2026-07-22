using KelasiNaBiso.Data;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace KelasiNaBiso.Controllers
{
    /// <summary>
    /// Contrôleur pour exposer les métriques de monitoring de l'API
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MetricsController : ControllerBase
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly ILogger<MetricsController> _logger;
        private readonly ICurrentUserService _currentUserService;

        public MetricsController(
            KelasiNaBisoDbContext context,
            ILogger<MetricsController> logger,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// 📊 Métriques générales du système (toutes écoles confondues)
        /// Accessible à tous les utilisateurs authentifiés
        /// </summary>
        [HttpGet("general")]
        [Authorize]
        [ProducesResponseType(typeof(GeneralMetricsDto), 200)]
        public async Task<ActionResult<GeneralMetricsDto>> GetGeneralMetrics()
        {
            try
            {
                _logger.LogInformation("📊 Récupération des métriques générales");

                var metrics = new GeneralMetricsDto
                {
                    Timestamp = DateTime.UtcNow,
                    Ecoles = new EntityCountDto
                    {
                        Total = await _context.Ecoles.Where(e => e.Statut == true).CountAsync(),
                        Actifs = await _context.Ecoles.Where(e => e.Statut == true).CountAsync()
                    },
                    Utilisateurs = new EntityCountDto
                    {
                        Total = await _context.Utilisateurs.CountAsync(),
                        Actifs = await _context.Utilisateurs.Where(u => u.Statut == true).CountAsync()
                    },
                    Eleves = new EntityCountDto
                    {
                        Total = await _context.Eleves.CountAsync(),
                        Actifs = await _context.Eleves.Where(e => e.Statut == true).CountAsync()
                    },
                    Agents = new EntityCountDto
                    {
                        Total = await _context.Agents.CountAsync(),
                        Actifs = await _context.Agents.Where(a => a.Statut == true).CountAsync()
                    },
                    Classes = new EntityCountDto
                    {
                        Total = await _context.Classes.CountAsync(),
                        Actifs = await _context.Classes.Where(c => c.Statut == true).CountAsync()
                    },
                    Tuteurs = new EntityCountDto
                    {
                        Total = await _context.Tuteurs.CountAsync(),
                        Actifs = await _context.Tuteurs.Where(t => t.Statut == true).CountAsync()
                    },
                    Inscriptions = new EntityCountDto
                    {
                        Total = await _context.Inscriptions.CountAsync(),
                        Actifs = await _context.Inscriptions.Where(i => i.StatutInscription == "Confirmé").CountAsync()
                    }
                };

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la récupération des métriques générales");
                return StatusCode(500, new { message = "Erreur lors de la récupération des métriques", error = ex.Message });
            }
        }

        /// <summary>
        /// 💰 Métriques financières (paiements et frais)
        /// Accessible à tous les utilisateurs authentifiés
        /// </summary>
        [HttpGet("financial")]
        [Authorize]
        [ProducesResponseType(typeof(FinancialMetricsDto), 200)]
        public async Task<ActionResult<FinancialMetricsDto>> GetFinancialMetrics(
            [FromQuery] DateTime? dateDebut = null,
            [FromQuery] DateTime? dateFin = null)
        {
            try
            {
                _logger.LogInformation("💰 Récupération des métriques financières");

                // Par défaut : mois en cours
                if (!dateDebut.HasValue || !dateFin.HasValue)
                {
                    var aujourdhui = DateTime.Now;
                    dateDebut = new DateTime(aujourdhui.Year, aujourdhui.Month, 1);
                    dateFin = dateDebut.Value.AddMonths(1).AddDays(-1);
                }

                var paiementsQuery = _context.Paiements
                    .Where(p => p.DatePaiement >= dateDebut && p.DatePaiement <= dateFin);

                var totalPaiements = await paiementsQuery.CountAsync();
                var montantTotal = await paiementsQuery.SumAsync(p => (decimal?)p.Montant) ?? 0;
                var montantMoyen = totalPaiements > 0 ? montantTotal / totalPaiements : 0;

                // Répartition par devise
                var repartitionDevise = await paiementsQuery
                    .GroupBy(p => p.Devise ?? "USD")
                    .Select(g => new RepartitionDeviseDto
                    {
                        Devise = g.Key,
                        Nombre = g.Count(),
                        Montant = (decimal)g.Sum(p => p.Montant),
                        Pourcentage = totalPaiements > 0 ? Math.Round((decimal)g.Count() * 100 / totalPaiements, 2) : 0
                    })
                    .ToListAsync();

                // Répartition par mode de paiement
                var repartitionMode = await paiementsQuery
                    .GroupBy(p => p.ModePaiement ?? "Non défini")
                    .Select(g => new RepartitionModePaiementMetricsDto
                    {
                        Mode = g.Key,
                        Nombre = g.Count(),
                        Montant = (decimal)g.Sum(p => p.Montant),
                        Pourcentage = totalPaiements > 0 ? Math.Round((decimal)g.Count() * 100 / totalPaiements, 2) : 0
                    })
                    .ToListAsync();

                // Statistiques des frais
                var totalFrais = await _context.Frais.Where(f => f.Statut == true).CountAsync();
                var fraisActifs = await _context.Frais.Where(f => f.Statut == true).CountAsync();

                // Paiements échoués (PaiementCrashed)
                var paiementsEchoues = await _context.PaiementsCrashed
                    .Where(p => p.DateEchec >= dateDebut && p.DateEchec <= dateFin && !p.EstResolu)
                    .CountAsync();

                var metrics = new FinancialMetricsDto
                {
                    Timestamp = DateTime.UtcNow,
                    Periode = new PeriodeMetricsDto
                    {
                        DateDebut = dateDebut.Value,
                        DateFin = dateFin.Value
                    },
                    Paiements = new PaiementsMetricsDto
                    {
                        Total = totalPaiements,
                        MontantTotal = montantTotal,
                        MontantMoyen = montantMoyen,
                        RepartitionDevise = repartitionDevise,
                        RepartitionMode = repartitionMode
                    },
                    Frais = new FraisMetricsDto
                    {
                        Total = totalFrais,
                        Actifs = fraisActifs
                    },
                    PaiementsEchoues = paiementsEchoues
                };

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la récupération des métriques financières");
                return StatusCode(500, new { message = "Erreur lors de la récupération des métriques financières", error = ex.Message });
            }
        }

        /// <summary>
        /// 📚 Métriques académiques (inscriptions, notes, cours)
        /// Accessible à tous les utilisateurs authentifiés
        /// </summary>
        [HttpGet("academic")]
        [Authorize]
        [ProducesResponseType(typeof(AcademicMetricsDto), 200)]
        public async Task<ActionResult<AcademicMetricsDto>> GetAcademicMetrics(
            [FromQuery] DateTime? dateDebut = null,
            [FromQuery] DateTime? dateFin = null)
        {
            try
            {
                _logger.LogInformation("📚 Récupération des métriques académiques");

                // Par défaut : année scolaire en cours ou 12 derniers mois
                if (!dateDebut.HasValue || !dateFin.HasValue)
                {
                    dateFin = DateTime.Now;
                    dateDebut = dateFin.Value.AddMonths(-12);
                }

                // Inscriptions
                var inscriptionsQuery = _context.Inscriptions
                    .Where(i => i.DateInscription >= dateDebut && i.DateInscription <= dateFin);

                var totalInscriptions = await inscriptionsQuery.CountAsync();
                var inscriptionsParType = await inscriptionsQuery
                    .GroupBy(i => i.Type ?? "Inscription")
                    .Select(g => new RepartitionTypeDto
                    {
                        Type = g.Key,
                        Nombre = g.Count(),
                        Pourcentage = totalInscriptions > 0 ? Math.Round((decimal)g.Count() * 100 / totalInscriptions, 2) : 0
                    })
                    .ToListAsync();

                // Notes
                var totalNotes = await _context.Notes.CountAsync();
                var notesMoyennes = await _context.Notes
                    .Where(n => n.NoteObtenue != null)
                    .AverageAsync(n => (double?)n.NoteObtenue) ?? 0;

                // Cours
                var totalCours = await _context.Cours.CountAsync();
                var coursActifs = await _context.Cours.Where(c => c.Statut == true).CountAsync();

                // Évaluations
                var totalEvaluations = await _context.Evaluations.CountAsync();
                var evaluationsActives = await _context.Evaluations.Where(e => e.Statut == true).CountAsync();

                var metrics = new AcademicMetricsDto
                {
                    Timestamp = DateTime.UtcNow,
                    Periode = new PeriodeMetricsDto
                    {
                        DateDebut = dateDebut.Value,
                        DateFin = dateFin.Value
                    },
                    Inscriptions = new InscriptionsMetricsDto
                    {
                        Total = totalInscriptions,
                        RepartitionParType = inscriptionsParType
                    },
                    Notes = new NotesMetricsDto
                    {
                        Total = totalNotes,
                        MoyenneGenerale = (decimal)notesMoyennes
                    },
                    Cours = new CoursMetricsDto
                    {
                        Total = totalCours,
                        Actifs = coursActifs
                    },
                    Evaluations = new EvaluationsMetricsDto
                    {
                        Total = totalEvaluations,
                        Actives = evaluationsActives
                    }
                };

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la récupération des métriques académiques");
                return StatusCode(500, new { message = "Erreur lors de la récupération des métriques académiques", error = ex.Message });
            }
        }

        /// <summary>
        /// 🏥 Métriques de santé du système (health check)
        /// Accessible à tous (pas d'authentification requise pour le monitoring)
        /// </summary>
        [HttpGet("health")]
        [ProducesResponseType(typeof(HealthMetricsDto), 200)]
        public async Task<ActionResult<HealthMetricsDto>> GetHealthMetrics()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();

                // Test de connexion à la base de données
                var dbHealthy = false;
                var dbResponseTime = 0L;
                try
                {
                    var dbStopwatch = Stopwatch.StartNew();
                    await _context.Database.ExecuteSqlRawAsync("SELECT 1");
                    dbStopwatch.Stop();
                    dbResponseTime = dbStopwatch.ElapsedMilliseconds;
                    dbHealthy = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Erreur de connexion à la base de données");
                    dbHealthy = false;
                }

                stopwatch.Stop();

                // Informations système
                var process = Process.GetCurrentProcess();
                var memoryUsage = process.WorkingSet64 / 1024 / 1024; // MB
                var cpuTime = process.TotalProcessorTime.TotalMilliseconds;

                var metrics = new HealthMetricsDto
                {
                    Timestamp = DateTime.UtcNow,
                    Status = dbHealthy ? "Healthy" : "Unhealthy",
                    Database = new DatabaseHealthDto
                    {
                        Connected = dbHealthy,
                        ResponseTimeMs = dbResponseTime
                    },
                    System = new SystemHealthDto
                    {
                        MemoryUsageMB = (int)memoryUsage,
                        CpuTimeMs = (long)cpuTime,
                        UptimeSeconds = (long)(DateTime.Now - process.StartTime).TotalSeconds
                    },
                    ResponseTimeMs = stopwatch.ElapsedMilliseconds
                };

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la récupération des métriques de santé");
                return StatusCode(500, new { message = "Erreur lors de la récupération des métriques de santé", error = ex.Message });
            }
        }

        /// <summary>
        /// 🏫 Métriques spécifiques à une école
        /// Accessible aux utilisateurs authentifiés (filtre automatique par école de l'utilisateur)
        /// </summary>
        [HttpGet("ecole")]
        [Authorize]
        [ProducesResponseType(typeof(EcoleMetricsDto), 200)]
        public async Task<ActionResult<EcoleMetricsDto>> GetEcoleMetrics(
            [FromQuery] int? idEcole = null)
        {
            try
            {
                // Si idEcole n'est pas fourni, utiliser l'école de l'utilisateur connecté
                if (!idEcole.HasValue)
                {
                    idEcole = _currentUserService.EcoleId > 0 ? _currentUserService.EcoleId : null;
                }

                if (!idEcole.HasValue)
                {
                    return BadRequest(new { message = "ID de l'école requis" });
                }

                _logger.LogInformation($"🏫 Récupération des métriques pour l'école {idEcole}");

                // Vérifier que l'école existe
                var ecole = await _context.Ecoles.FindAsync(idEcole.Value);
                if (ecole == null)
                {
                    return NotFound(new { message = $"École {idEcole.Value} non trouvée" });
                }

                // Statistiques de l'école
                var nombreEleves = await _context.Eleves
                    .Where(e => e.Classe != null && 
                               e.Classe.Direction != null && 
                               e.Classe.Direction.IdEcole == idEcole.Value && 
                               e.Statut == true)
                    .CountAsync();

                var nombreAgents = await _context.Agents
                    .Where(a => a.IdEcole == idEcole.Value && a.Statut == true)
                    .CountAsync();

                var nombreClasses = await _context.Classes
                    .Where(c => c.Direction != null && 
                               c.Direction.IdEcole == idEcole.Value && 
                               c.Statut == true)
                    .CountAsync();

                // Paiements du mois en cours
                var aujourdhui = DateTime.Now;
                var debutMois = new DateTime(aujourdhui.Year, aujourdhui.Month, 1);
                var finMois = debutMois.AddMonths(1).AddDays(-1);

                var paiementsMois = await _context.Paiements
                    .Where(p => p.DatePaiement >= debutMois && 
                               p.DatePaiement <= finMois &&
                               p.IdEleve != null &&
                               _context.Eleves.Any(e => e.IdEleve == p.IdEleve &&
                                                       e.Classe != null &&
                                                       e.Classe.Direction != null &&
                                                       e.Classe.Direction.IdEcole == idEcole.Value))
                    .CountAsync();

                var montantMois = await _context.Paiements
                    .Where(p => p.DatePaiement >= debutMois && 
                               p.DatePaiement <= finMois &&
                               p.IdEleve != null &&
                               _context.Eleves.Any(e => e.IdEleve == p.IdEleve &&
                                                       e.Classe != null &&
                                                       e.Classe.Direction != null &&
                                                       e.Classe.Direction.IdEcole == idEcole.Value))
                    .SumAsync(p => (decimal?)p.Montant) ?? 0;

                // Inscriptions de l'année
                var debutAnnee = new DateTime(aujourdhui.Year, 1, 1);
                var inscriptionsAnnee = await _context.Inscriptions
                    .Where(i => i.DateInscription >= debutAnnee &&
                               i.IdEcole == idEcole.Value)
                    .CountAsync();

                var metrics = new EcoleMetricsDto
                {
                    Timestamp = DateTime.UtcNow,
                    Ecole = new EcoleInfoMetricsDto
                    {
                        IdEcole = idEcole.Value,
                        Nom = ecole.Nom ?? "Non défini"
                    },
                    Statistiques = new EcoleStatistiquesDto
                    {
                        NombreEleves = nombreEleves,
                        NombreAgents = nombreAgents,
                        NombreClasses = nombreClasses
                    },
                    PaiementsMois = new PaiementsMoisDto
                    {
                        Nombre = paiementsMois,
                        Montant = montantMois
                    },
                    InscriptionsAnnee = inscriptionsAnnee
                };

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la récupération des métriques de l'école");
                return StatusCode(500, new { message = "Erreur lors de la récupération des métriques de l'école", error = ex.Message });
            }
        }

        /// <summary>
        /// 📈 Métriques complètes (toutes les métriques combinées)
        /// Accessible aux Super-Admin uniquement
        /// </summary>
        [HttpGet("complete")]
        [Authorize(Roles = "Super-Admin")]
        [ProducesResponseType(typeof(CompleteMetricsDto), 200)]
        public async Task<ActionResult<CompleteMetricsDto>> GetCompleteMetrics()
        {
            try
            {
                _logger.LogInformation("📈 Récupération des métriques complètes");

                // Récupérer toutes les métriques en parallèle
                var generalTask = GetGeneralMetricsInternal();
                var financialTask = GetFinancialMetricsInternal();
                var academicTask = GetAcademicMetricsInternal();
                var healthTask = GetHealthMetricsInternal();

                await Task.WhenAll(generalTask, financialTask, academicTask, healthTask);

                var metrics = new CompleteMetricsDto
                {
                    Timestamp = DateTime.UtcNow,
                    General = await generalTask,
                    Financial = await financialTask,
                    Academic = await academicTask,
                    Health = await healthTask
                };

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la récupération des métriques complètes");
                return StatusCode(500, new { message = "Erreur lors de la récupération des métriques complètes", error = ex.Message });
            }
        }

        // Méthodes internes pour récupérer les métriques sans HTTP
        private async Task<GeneralMetricsDto> GetGeneralMetricsInternal()
        {
            return new GeneralMetricsDto
            {
                Timestamp = DateTime.UtcNow,
                Ecoles = new EntityCountDto
                {
                    Total = await _context.Ecoles.Where(e => e.Statut == true).CountAsync(),
                    Actifs = await _context.Ecoles.Where(e => e.Statut == true).CountAsync()
                },
                Utilisateurs = new EntityCountDto
                {
                    Total = await _context.Utilisateurs.CountAsync(),
                    Actifs = await _context.Utilisateurs.Where(u => u.Statut == true).CountAsync()
                },
                Eleves = new EntityCountDto
                {
                    Total = await _context.Eleves.CountAsync(),
                    Actifs = await _context.Eleves.Where(e => e.Statut == true).CountAsync()
                },
                Agents = new EntityCountDto
                {
                    Total = await _context.Agents.CountAsync(),
                    Actifs = await _context.Agents.Where(a => a.Statut == true).CountAsync()
                },
                Classes = new EntityCountDto
                {
                    Total = await _context.Classes.CountAsync(),
                    Actifs = await _context.Classes.Where(c => c.Statut == true).CountAsync()
                },
                Tuteurs = new EntityCountDto
                {
                    Total = await _context.Tuteurs.CountAsync(),
                    Actifs = await _context.Tuteurs.Where(t => t.Statut == true).CountAsync()
                },
                Inscriptions = new EntityCountDto
                {
                    Total = await _context.Inscriptions.CountAsync(),
                    Actifs = await _context.Inscriptions.Where(i => i.StatutInscription == "Confirmé").CountAsync()
                }
            };
        }

        private async Task<FinancialMetricsDto> GetFinancialMetricsInternal()
        {
            var aujourdhui = DateTime.Now;
            var dateDebut = new DateTime(aujourdhui.Year, aujourdhui.Month, 1);
            var dateFin = dateDebut.AddMonths(1).AddDays(-1);

            var paiementsQuery = _context.Paiements
                .Where(p => p.DatePaiement >= dateDebut && p.DatePaiement <= dateFin);

            var totalPaiements = await paiementsQuery.CountAsync();
            var montantTotal = await paiementsQuery.SumAsync(p => (decimal?)p.Montant) ?? 0;
            var montantMoyen = totalPaiements > 0 ? montantTotal / totalPaiements : 0;

            return new FinancialMetricsDto
            {
                Timestamp = DateTime.UtcNow,
                Periode = new PeriodeMetricsDto { DateDebut = dateDebut, DateFin = dateFin },
                Paiements = new PaiementsMetricsDto
                {
                    Total = totalPaiements,
                    MontantTotal = montantTotal,
                    MontantMoyen = montantMoyen,
                    RepartitionDevise = new List<RepartitionDeviseDto>(),
                    RepartitionMode = new List<RepartitionModePaiementMetricsDto>()
                },
                Frais = new FraisMetricsDto
                {
                    Total = await _context.Frais.CountAsync(),
                    Actifs = await _context.Frais.Where(f => f.Statut == true).CountAsync()
                },
                PaiementsEchoues = 0
            };
        }

        private async Task<AcademicMetricsDto> GetAcademicMetricsInternal()
        {
            var dateFin = DateTime.Now;
            var dateDebut = dateFin.AddMonths(-12);

            return new AcademicMetricsDto
            {
                Timestamp = DateTime.UtcNow,
                Periode = new PeriodeMetricsDto { DateDebut = dateDebut, DateFin = dateFin },
                Inscriptions = new InscriptionsMetricsDto
                {
                    Total = await _context.Inscriptions.CountAsync(),
                    RepartitionParType = new List<RepartitionTypeDto>()
                },
                Notes = new NotesMetricsDto
                {
                    Total = await _context.Notes.CountAsync(),
                    MoyenneGenerale = 0
                },
                Cours = new CoursMetricsDto
                {
                    Total = await _context.Cours.CountAsync(),
                    Actifs = await _context.Cours.Where(c => c.Statut == true).CountAsync()
                },
                Evaluations = new EvaluationsMetricsDto
                {
                    Total = await _context.Evaluations.CountAsync(),
                    Actives = await _context.Evaluations.Where(e => e.Statut == true).CountAsync()
                }
            };
        }

        private async Task<HealthMetricsDto> GetHealthMetricsInternal()
        {
            var dbHealthy = false;
            var dbResponseTime = 0L;
            try
            {
                var dbStopwatch = Stopwatch.StartNew();
                await _context.Database.ExecuteSqlRawAsync("SELECT 1");
                dbStopwatch.Stop();
                dbResponseTime = dbStopwatch.ElapsedMilliseconds;
                dbHealthy = true;
            }
            catch
            {
                dbHealthy = false;
            }

            var process = Process.GetCurrentProcess();
            var memoryUsage = process.WorkingSet64 / 1024 / 1024;

            return new HealthMetricsDto
            {
                Timestamp = DateTime.UtcNow,
                Status = dbHealthy ? "Healthy" : "Unhealthy",
                Database = new DatabaseHealthDto
                {
                    Connected = dbHealthy,
                    ResponseTimeMs = dbResponseTime
                },
                System = new SystemHealthDto
                {
                    MemoryUsageMB = (int)memoryUsage,
                    CpuTimeMs = (long)process.TotalProcessorTime.TotalMilliseconds,
                    UptimeSeconds = (long)(DateTime.Now - process.StartTime).TotalSeconds
                },
                ResponseTimeMs = 0
            };
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // DTOs pour les métriques
    // ═══════════════════════════════════════════════════════════════════

    public class GeneralMetricsDto
    {
        public DateTime Timestamp { get; set; }
        public EntityCountDto Ecoles { get; set; } = new();
        public EntityCountDto Utilisateurs { get; set; } = new();
        public EntityCountDto Eleves { get; set; } = new();
        public EntityCountDto Agents { get; set; } = new();
        public EntityCountDto Classes { get; set; } = new();
        public EntityCountDto Tuteurs { get; set; } = new();
        public EntityCountDto Inscriptions { get; set; } = new();
    }

    public class EntityCountDto
    {
        public int Total { get; set; }
        public int Actifs { get; set; }
    }

    public class FinancialMetricsDto
    {
        public DateTime Timestamp { get; set; }
        public PeriodeMetricsDto Periode { get; set; } = new();
        public PaiementsMetricsDto Paiements { get; set; } = new();
        public FraisMetricsDto Frais { get; set; } = new();
        public int PaiementsEchoues { get; set; }
    }

    public class PeriodeMetricsDto
    {
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
    }

    public class PaiementsMetricsDto
    {
        public int Total { get; set; }
        public decimal MontantTotal { get; set; }
        public decimal MontantMoyen { get; set; }
        public List<RepartitionDeviseDto> RepartitionDevise { get; set; } = new();
        public List<RepartitionModePaiementMetricsDto> RepartitionMode { get; set; } = new();
    }

    public class RepartitionDeviseDto
    {
        public string Devise { get; set; } = string.Empty;
        public int Nombre { get; set; }
        public decimal Montant { get; set; }
        public decimal Pourcentage { get; set; }
    }

    /// <summary>
    /// ✅ Renommé pour éviter le conflit avec KelasiNaBiso.Models.DTOs.Paiement.RepartitionModePaiementDto
    /// </summary>
    public class RepartitionModePaiementMetricsDto
    {
        public string Mode { get; set; } = string.Empty;
        public int Nombre { get; set; }
        public decimal Montant { get; set; }
        public decimal Pourcentage { get; set; }
    }

    public class FraisMetricsDto
    {
        public int Total { get; set; }
        public int Actifs { get; set; }
    }

    public class AcademicMetricsDto
    {
        public DateTime Timestamp { get; set; }
        public PeriodeMetricsDto Periode { get; set; } = new();
        public InscriptionsMetricsDto Inscriptions { get; set; } = new();
        public NotesMetricsDto Notes { get; set; } = new();
        public CoursMetricsDto Cours { get; set; } = new();
        public EvaluationsMetricsDto Evaluations { get; set; } = new();
    }

    public class InscriptionsMetricsDto
    {
        public int Total { get; set; }
        public List<RepartitionTypeDto> RepartitionParType { get; set; } = new();
    }

    public class RepartitionTypeDto
    {
        public string Type { get; set; } = string.Empty;
        public int Nombre { get; set; }
        public decimal Pourcentage { get; set; }
    }

    public class NotesMetricsDto
    {
        public int Total { get; set; }
        public decimal MoyenneGenerale { get; set; }
    }

    public class CoursMetricsDto
    {
        public int Total { get; set; }
        public int Actifs { get; set; }
    }

    public class EvaluationsMetricsDto
    {
        public int Total { get; set; }
        public int Actives { get; set; }
    }

    public class HealthMetricsDto
    {
        public DateTime Timestamp { get; set; }
        public string Status { get; set; } = string.Empty;
        public DatabaseHealthDto Database { get; set; } = new();
        public SystemHealthDto System { get; set; } = new();
        public long ResponseTimeMs { get; set; }
    }

    public class DatabaseHealthDto
    {
        public bool Connected { get; set; }
        public long ResponseTimeMs { get; set; }
    }

    public class SystemHealthDto
    {
        public int MemoryUsageMB { get; set; }
        public long CpuTimeMs { get; set; }
        public long UptimeSeconds { get; set; }
    }

    public class EcoleMetricsDto
    {
        public DateTime Timestamp { get; set; }
        public EcoleInfoMetricsDto Ecole { get; set; } = new();
        public EcoleStatistiquesDto Statistiques { get; set; } = new();
        public PaiementsMoisDto PaiementsMois { get; set; } = new();
        public int InscriptionsAnnee { get; set; }
    }

    /// <summary>
    /// ✅ Renommé pour éviter le conflit avec KelasiNaBiso.Models.DTOs.Reporting.EcoleInfoDto
    /// </summary>
    public class EcoleInfoMetricsDto
    {
        public int IdEcole { get; set; }
        public string Nom { get; set; } = string.Empty;
    }

    public class EcoleStatistiquesDto
    {
        public int NombreEleves { get; set; }
        public int NombreAgents { get; set; }
        public int NombreClasses { get; set; }
    }

    public class PaiementsMoisDto
    {
        public int Nombre { get; set; }
        public decimal Montant { get; set; }
    }

    public class CompleteMetricsDto
    {
        public DateTime Timestamp { get; set; }
        public GeneralMetricsDto General { get; set; } = new();
        public FinancialMetricsDto Financial { get; set; } = new();
        public AcademicMetricsDto Academic { get; set; } = new();
        public HealthMetricsDto Health { get; set; } = new();
    }
}
