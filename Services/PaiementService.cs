using System;
using System.Threading;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.Paiement;
using KelasiNaBiso.Models.DTOs.Pagination;
using KelasiNaBiso.Models.DTOs.Reporting;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Services.Notifications;
using KelasiNaBiso.Extensions;
using KelasiNaBiso.Services.MokoAfrika;
using KelasiNaBisoAPI.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class PaiementService : IPaiementRepository
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly ILogger<PaiementService> _logger;
        private readonly ICacheService _cacheService;
        private readonly INotificationDispatcher _notificationDispatcher;
        private readonly INotificationJobQueue _notificationJobQueue;
        private readonly IDashboardHubService _dashboardHubService;

        public PaiementService(
            KelasiNaBisoDbContext context,
            ILogger<PaiementService> logger,
            ICacheService cacheService,
            INotificationDispatcher notificationDispatcher,
            INotificationJobQueue notificationJobQueue,
            IDashboardHubService dashboardHubService)
        {
            _context = context;
            _cacheService = cacheService;
            _logger = logger;
            _notificationDispatcher = notificationDispatcher;
            _notificationJobQueue = notificationJobQueue;
            _dashboardHubService = dashboardHubService;
        }

        // ✅ NOUVELLES MÉTHODES PAGINÉES
        public async Task<PagedResult<Paiement>> GetAllPagedAsync(PagedRequest request)
        {
            var query = _context.Paiements
               // .Include(p => p.Eleve)
               // .Include(p => p.Utilisateur)
               // .Include(p => p.Frais)
                .AsQueryable();

            // Filtrer par statut
            if (!request.IncludeInactive)
            {
                query = query.Where(p => p.Statut == true);
            }

            // Appliquer la recherche
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(p =>
                    (p.ReferenceTransaction != null && p.ReferenceTransaction.ToLower().Contains(searchLower)) ||
                    (p.ModePaiement != null && p.ModePaiement.ToLower().Contains(searchLower)) ||
                    (p.Commentaire != null && p.Commentaire.ToLower().Contains(searchLower))
                );
            }

            // Appliquer le tri
            if (!string.IsNullOrWhiteSpace(request.SortBy))
            {
                query = query.ApplySort(request.SortBy, request.SortDescending);
            }
            else
            {
                // Tri par défaut : DatePaiement DESC (plus récent en premier)
                query = request.SortDescending
                    ? query.OrderBy(p => p.DatePaiement)
                    : query.OrderByDescending(p => p.DatePaiement);
            }

            return await query.ToPagedAsync(request);
        }

        public async Task<CursorPaginatedResult<Paiement>> GetAllCursorPagedAsync(CursorPaginationRequest request)
        {
            var query = _context.Paiements
              //  .Include(p => p.Eleve)
               // .Include(p => p.Utilisateur)
               // .Include(p => p.Frais)
                .AsQueryable();

            // Filtrer par statut
            if (!request.IncludeInactive)
            {
                query = query.Where(p => p.Statut == true);
            }

            // Appliquer la recherche
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(p =>
                    (p.ReferenceTransaction != null && p.ReferenceTransaction.ToLower().Contains(searchLower)) ||
                    (p.ModePaiement != null && p.ModePaiement.ToLower().Contains(searchLower))
                );
            }

            // Utiliser IdPaiement comme curseur
            return await query.ToCursorPagedAsync(request, p => p.IdPaiement);
        }

        public async Task<PagedResult<Paiement>> GetByElevePagedAsync(int idEleve, PagedRequest request)
        {
            var query = _context.Paiements
               // .Include(p => p.Eleve)
               // .Include(p => p.Frais)
               // .Include(p => p.Utilisateur)
                .Where(p => p.IdEleve == idEleve);

            // Filtrer par statut
            if (!request.IncludeInactive)
            {
                query = query.Where(p => p.Statut == true);
            }

            return await query.ToPagedAsync(request, p => p.DatePaiement);
        }

        public async Task<PagedResult<Paiement>> GetByEcolePagedAsync(int idEcole, PagedRequest request)
        {
            var query = _context.Paiements
               // .Include(p => p.Eleve)
                //    .ThenInclude(e => e.Classe)
                //        .ThenInclude(c => c.Direction)
               // .Include(p => p.Frais)
               // .Include(p => p.Utilisateur)
                .Where(p => p.Eleve.Classe.Direction.IdEcole == idEcole);

            // Filtrer par statut
            if (!request.IncludeInactive)
            {
                query = query.Where(p => p.Statut == true);
            }

            // Appliquer la recherche
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(p =>
                    (p.ReferenceTransaction != null && p.ReferenceTransaction.ToLower().Contains(searchLower)) ||
                    (p.ModePaiement != null && p.ModePaiement.ToLower().Contains(searchLower))
                );
            }

            return await query.ToPagedAsync(request, p => p.DatePaiement);
        }

        public async Task<PagedResult<Paiement>> GetByDateRangePagedAsync(DateTime dateDebut, DateTime dateFin, PagedRequest request)
        {
            var query = _context.Paiements
                //.Include(p => p.Eleve)
               // .Include(p => p.Frais)
               // .Include(p => p.Utilisateur)
                .Where(p => p.DatePaiement >= dateDebut && p.DatePaiement <= dateFin);

            // Filtrer par statut
            if (!request.IncludeInactive)
            {
                query = query.Where(p => p.Statut == true);
            }

            // Appliquer la recherche
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(p =>
                    (p.ReferenceTransaction != null && p.ReferenceTransaction.ToLower().Contains(searchLower)) ||
                    (p.ModePaiement != null && p.ModePaiement.ToLower().Contains(searchLower))
                );
            }

            return await query.ToPagedAsync(request, p => p.DatePaiement);
        }

        public async Task<PagedResult<Paiement>> GetByModePaiementPagedAsync(string modePaiement, PagedRequest request)
        {
            var query = _context.Paiements
                .Include(p => p.Eleve)
                .Include(p => p.Frais)
                .Include(p => p.Utilisateur)
                .Where(p => p.ModePaiement == modePaiement);

            // Filtrer par statut
            if (!request.IncludeInactive)
            {
                query = query.Where(p => p.Statut == true);
            }

            return await query.ToPagedAsync(request, p => p.DatePaiement);
        }

        public async Task<PagedResult<Paiement>> GetByStatutPaiementPagedAsync(string statut, PagedRequest request)
        {
            var query = _context.Paiements
               // .Include(p => p.Eleve)
              //  .Include(p => p.Frais)
              //  .Include(p => p.Utilisateur)
                .Where(p => p.StatutPaiement == statut);

            // Filtrer par statut actif/inactif
            if (!request.IncludeInactive)
            {
                query = query.Where(p => p.Statut == true);
            }

            return await query.ToPagedAsync(request, p => p.DatePaiement);
        }

        // ⚠️ DEPRECATED: Anciennes méthodes (conserver pour rétrocompatibilité)
        public async Task<IEnumerable<Paiement>> GetAllAsync()
        {
            return await _context.Paiements
               // .Include(p => p.Eleve)
              //  .Include(p => p.Utilisateur)
              //  .Include(p => p.Frais)
                .Where(p => p.Statut == true) // ✅ Filtrer uniquement les paiements actifs
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<Paiement> GetByIdAsync(int id)
        {
            return await _context.Paiements
              //  .Include(p => p.Eleve)
              //  .Include(p => p.Utilisateur)
              //  .Include(p => p.Frais)
                .FirstOrDefaultAsync(p => p.IdPaiement == id);
        }

        public async Task<Paiement> GetByReferenceAsync(string reference)
        {
            return await _context.Paiements
              //  .Include(p => p.Eleve)
              //  .Include(p => p.Utilisateur)
              //  .Include(p => p.Frais)
                .FirstOrDefaultAsync(p => p.ReferencePaiemenet == reference);
        }

        public async Task<IEnumerable<Paiement>> GetByEleveAsync(int idEleve)
        {
            return await _context.Paiements
               // .Include(p => p.Utilisateur)
              //  .Include(p => p.Frais)
                .Where(p => p.IdEleve == idEleve)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<IEnumerable<Paiement>> GetByUtilisateurAsync(int idUtilisateur)
        {
            return await _context.Paiements
               // .Include(p => p.Eleve)
              //  .Include(p => p.Frais)
                .Where(p => p.IdUtilisateur == idUtilisateur)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<IEnumerable<Paiement>> GetByFraisAsync(int idFrais)
        {
            return await _context.Paiements
              //  .Include(p => p.Eleve)
              //  .Include(p => p.Utilisateur)
                .Where(p => p.IdFrais == idFrais)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<IEnumerable<Paiement>> GetByEcoleAsync(int idEcole)
        {
            return await _context.Paiements
                .Include(p => p.Eleve)
                .Include(p => p.Utilisateur)
                .Include(p => p.Frais)
                .ThenInclude(f => f.Direction)
                .Where(p => p.Frais.Direction.IdEcole == idEcole)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<IEnumerable<Paiement>> GetByModePaiementAsync(string modePaiement)
        {
            return await _context.Paiements
             //   .Include(p => p.Eleve)
              //  .Include(p => p.Utilisateur)
              //  .Include(p => p.Frais)
                .Where(p => p.ModePaiement == modePaiement)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<IEnumerable<Paiement>> GetByStatutAsync(string statutPaiement)
        {
            return await _context.Paiements
              //  .Include(p => p.Eleve)
              //  .Include(p => p.Utilisateur)
              //  .Include(p => p.Frais)
                .Where(p => p.StatutPaiement == statutPaiement && p.Statut == true) // ✅ Filtrer actifs
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<IEnumerable<Paiement>> GetByDatePaiementAsync(DateTime date)
        {
            return await _context.Paiements
              //  .Include(p => p.Eleve)
              //  .Include(p => p.Utilisateur)
              //  .Include(p => p.Frais)
                .Where(p => p.DatePaiement.Date == date.Date)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<IEnumerable<Paiement>> GetByDateRangeAsync(DateTime dateDebut, DateTime dateFin)
        {
            return await _context.Paiements
               // .Include(p => p.Eleve)
              //  .Include(p => p.Utilisateur)
              //  .Include(p => p.Frais)
                .Where(p => p.DatePaiement >= dateDebut && p.DatePaiement <= dateFin)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<Paiement> CreateAsync(Paiement paiement)
        {
            paiement.DatePaiement = DateTime.Now;
            paiement.DateCreation = DateTime.Now;
            
            _context.Paiements.Add(paiement);
            await _context.SaveChangesAsync();
            
            // Notifications uniquement si le paiement n'attend pas confirmation gateway (Mobile Money / Carte MOKO)
            if (paiement.IdEleve.HasValue && !PaiementGatewayHelper.EstEnAttenteGateway(paiement))
            {
                await EnvoyerNotificationPaiementAuTuteurAsync(paiement);
            }
            
            // 📊 DASHBOARD TEMPS RÉEL: Notifier seulement si paiement déjà confirmé
            if (!PaiementGatewayHelper.EstEnAttenteGateway(paiement))
            {
                await NotifierMiseAJourDashboardAsync(paiement);
            }
            
            return paiement;
        }

        public async Task<IEnumerable<Paiement>> CreateBatchAsync(IEnumerable<Paiement> paiements)
        {
            var paiementList = paiements.ToList();
            var createdPaiements = new List<Paiement>();
            var errors = new List<string>();

            foreach (var paiement in paiementList)
            {
                try
                {
                    paiement.DatePaiement = DateTime.Now;
                    paiement.DateCreation = DateTime.Now;
                    
                    _context.Paiements.Add(paiement);
                    createdPaiements.Add(paiement);
                }
                catch (Exception ex)
                {
                    errors.Add($"Paiement: {ex.Message}");
                }
            }

            // Sauvegarder tous les paiements valides
            if (createdPaiements.Any())
            {
                await _context.SaveChangesAsync();
                
                // 🔔 Envoyer les notifications pour les paiements d'élèves
                foreach (var paiement in createdPaiements)
                {
                    if (paiement.IdEleve.HasValue && !PaiementGatewayHelper.EstEnAttenteGateway(paiement))
                    {
                        try
                        {
                            await EnvoyerNotificationPaiementAuTuteurAsync(paiement);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Erreur lors de l'envoi de notification pour le paiement {paiement.IdPaiement}: {ex.Message}");
                        }
                    }
                }
            }

            if (errors.Any())
            {
                Console.WriteLine($"⚠️ Erreurs lors de la création par lot: {string.Join("; ", errors)}");
            }

            return createdPaiements;
        }

        public async Task<Paiement> UpdateAsync(Paiement paiement)
        {
            var existingPaiement = await _context.Paiements.FindAsync(paiement.IdPaiement);
            if (existingPaiement == null)
                return null;

            _context.Entry(existingPaiement).CurrentValues.SetValues(paiement);
            await _context.SaveChangesAsync();
            return existingPaiement;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var paiement = await _context.Paiements.FindAsync(id);
            if (paiement == null)
                return false;

            _context.Paiements.Remove(paiement);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Paiements.AnyAsync(p => p.IdPaiement == id);
        }

        public async Task<bool> ExistsByReferenceAsync(string reference)
        {
            return await _context.Paiements.AnyAsync(p => p.ReferencePaiemenet == reference);
        }

        // ✅ SOFT DELETE: Toggle le statut d'un paiement (actif <-> inactif)
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var paiement = await _context.Paiements.FindAsync(id);
            if (paiement == null)
                return false;

            paiement.Statut = paiement.Statut != true;
            await _context.SaveChangesAsync();
            return true;
        }

        // ═══════════════════════════════════════════════════════
        // SECTION REPORTING PAIEMENT
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Helper: Résout la période de temps pour le reporting
        /// </summary>
        private PeriodeDto ResolvePeriode(DateTime? date, DateTime? dateDebut, DateTime? dateFin, string? periode)
        {
            DateTime debut, fin;
            string type, libelle;

            if (date.HasValue)
            {
                debut = fin = date.Value.Date;
                type = "jour";
                libelle = date.Value.ToString("dddd dd MMMM yyyy", new System.Globalization.CultureInfo("fr-FR"));
            }
            else if (dateDebut.HasValue && dateFin.HasValue)
            {
                debut = dateDebut.Value.Date;
                fin = dateFin.Value.Date;
                type = "intervalle";
                libelle = $"{debut:dd MMM} - {fin:dd MMM yyyy}";
            }
            else if (!string.IsNullOrEmpty(periode))
            {
                (debut, fin, type, libelle) = GetPeriodePredéfinie(periode);
            }
            else
            {
                var aujourdhui = DateTime.Now;
                debut = new DateTime(aujourdhui.Year, aujourdhui.Month, 1);
                fin = new DateTime(aujourdhui.Year, aujourdhui.Month, DateTime.DaysInMonth(aujourdhui.Year, aujourdhui.Month));
                type = "mois";
                libelle = aujourdhui.ToString("MMMM yyyy", new System.Globalization.CultureInfo("fr-FR"));
            }

            int joursOuvrables = CalculerJoursOuvrables(debut, fin);

            return new PeriodeDto
            {
                Type = type,
                Date = (type == "jour") ? debut : null,
                DateDebut = debut,
                DateFin = fin,
                JoursOuvrables = joursOuvrables,
                Libelle = libelle
            };
        }

        private (DateTime debut, DateTime fin, string type, string libelle) GetPeriodePredéfinie(string periode)
        {
            var aujourdhui = DateTime.Now.Date;

            return periode.ToLower() switch
            {
                "aujourdhui" or "aujourd'hui" => (
                    aujourdhui,
                    aujourdhui,
                    "jour",
                    aujourdhui.ToString("dddd dd MMMM yyyy", new System.Globalization.CultureInfo("fr-FR"))
                ),

                "semaine" => (
                    aujourdhui.AddDays(-(int)aujourdhui.DayOfWeek + (int)DayOfWeek.Monday),
                    aujourdhui.AddDays(7 - (int)aujourdhui.DayOfWeek),
                    "semaine",
                    $"Semaine du {aujourdhui.AddDays(-(int)aujourdhui.DayOfWeek + 1):dd MMM}"
                ),

                "mois" => (
                    new DateTime(aujourdhui.Year, aujourdhui.Month, 1),
                    new DateTime(aujourdhui.Year, aujourdhui.Month, DateTime.DaysInMonth(aujourdhui.Year, aujourdhui.Month)),
                    "mois",
                    aujourdhui.ToString("MMMM yyyy", new System.Globalization.CultureInfo("fr-FR"))
                ),

                "trimestre" => GetTrimestre(aujourdhui),

                "annee" => (
                    new DateTime(aujourdhui.Year, 1, 1),
                    new DateTime(aujourdhui.Year, 12, 31),
                    "annee",
                    aujourdhui.Year.ToString()
                ),

                _ => throw new ArgumentException($"Période invalide: {periode}. Valeurs acceptées: aujourdhui, semaine, mois, trimestre, annee")
            };
        }

        private (DateTime debut, DateTime fin, string type, string libelle) GetTrimestre(DateTime date)
        {
            int trimestre = ((date.Month - 1) / 3) + 1;
            int moisDebut = ((trimestre - 1) * 3) + 1;

            var debut = new DateTime(date.Year, moisDebut, 1);
            var fin = new DateTime(date.Year, moisDebut + 2, DateTime.DaysInMonth(date.Year, moisDebut + 2));

            return (debut, fin, "trimestre", $"Trimestre {trimestre} {date.Year}");
        }

        private int CalculerJoursOuvrables(DateTime debut, DateTime fin)
        {
            int jours = 0;
            for (var date = debut; date <= fin; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                {
                    jours++;
                }
            }
            return jours;
        }

        /// <summary>
        /// Obtient le dashboard de paiement pour une école
        /// </summary>
        public async Task<DashboardPaiementDto> GetDashboardEcoleAsync(
            int idEcole,
            DateTime? date,
            DateTime? dateDebut,
            DateTime? dateFin,
            string? periode)
        {
            var periodeDto = ResolvePeriode(date, dateDebut, dateFin, periode);

            // ✅ CACHE: Clé basée sur école + période
            string cacheKey = $"dashboard_paiement_{idEcole}_{periodeDto.DateDebut:yyyyMMdd}_{periodeDto.DateFin:yyyyMMdd}";
            
            return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await CalculerDashboardPaiementAsync(idEcole, periodeDto);
            }, TimeSpan.FromMinutes(5)); // Cache de 5 minutes
        }

        /// <summary>
        /// Calcule le dashboard de paiement (méthode privée pour le cache)
        /// </summary>
        private async Task<DashboardPaiementDto> CalculerDashboardPaiementAsync(
            int idEcole,
            PeriodeDto periodeDto)
        {
            var ecole = await _context.Ecoles.FindAsync(idEcole);
            if (ecole == null)
                throw new KeyNotFoundException($"École avec l'ID {idEcole} introuvable");

            // Récupérer tous les paiements de l'école sur la période
            var paiements = await _context.Paiements
                .Include(p => p.Eleve)
                .ThenInclude(e => e.Classe)
                .ThenInclude(c => c.Direction)
                .Include(p => p.Frais)
                .Where(p => p.Eleve.Classe.Direction.IdEcole == idEcole)
                .Where(p => p.DatePaiement >= periodeDto.DateDebut && p.DatePaiement <= periodeDto.DateFin)
                .Where(p => p.Statut == true)
                .ToListAsync();

            int nombrePaiements = paiements.Count;
            decimal montantTotal = (decimal)paiements.Sum(p => p.Montant);

            // Récupérer tous les frais attendus pour cette école
            var fraisEcole = await _context.Frais
                .Where(f => f.Direction.IdEcole == idEcole && f.Statut == true)
                .ToListAsync();

            // ✅ CORRECTION : Vérifier null et utiliser CountAsync (plus performant)
            // Note : Pour le dashboard paiement, on compte uniquement les élèves actifs
            // car seuls les élèves actifs doivent payer (logique métier)
            int nombreEleves = await _context.Eleves
                .Where(e => e.Classe != null &&
                           e.Classe.Direction != null &&
                           e.Classe.Direction.IdEcole == idEcole &&
                           e.Statut == true)
                .CountAsync();
            decimal montantAttendu = fraisEcole.Sum(f => (decimal)f.Montant) * nombreEleves;

            // Élèves ayant payé (au moins un paiement)
            var elevesAyantPaye = paiements.Select(p => p.IdEleve).Distinct().Count();

            // Répartition par mode de paiement
            var repartitionModes = new Dictionary<string, ModePaiementStatsDto>();
            var paiementsGroupes = paiements.GroupBy(p => p.ModePaiement ?? "Non spécifié");

            foreach (var groupe in paiementsGroupes)
            {
                var montant = (decimal)groupe.Sum(p => p.Montant);
                var pourcentage = montantTotal > 0 ? Math.Round((montant / montantTotal) * 100, 2) : 0;

                repartitionModes[groupe.Key] = new ModePaiementStatsDto
                {
                    Nombre = groupe.Count(),
                    Montant = montant,
                    Pourcentage = pourcentage
                };
            }

            // Top 5 des frais
            var top5Frais = paiements
                .GroupBy(p => new { p.IdFrais, p.Frais.LibelleFrais })
                .Select(g => new
                {
                    g.Key.LibelleFrais,
                    MontantTotal = (decimal)g.Sum(p => p.Montant),
                    NombrePaiements = g.Count()
                })
                .OrderByDescending(x => x.MontantTotal)
                .Take(5)
                .ToList();

            var top5List = new List<Top5FraisDto>();
            int rang = 1;
            foreach (var item in top5Frais)
            {
                var pourcentage = montantTotal > 0 ? Math.Round((item.MontantTotal / montantTotal) * 100, 2) : 0;
                top5List.Add(new Top5FraisDto
                {
                    Rang = rang++,
                    LibelleFrais = item.LibelleFrais ?? "",
                    MontantTotal = item.MontantTotal,
                    NombrePaiements = item.NombrePaiements,
                    Pourcentage = pourcentage
                });
            }

            var tauxRecouvrement = montantAttendu > 0 ? Math.Round((montantTotal / montantAttendu) * 100, 2) : 0;
            var elevesEnRetard = nombreEleves - elevesAyantPaye;
            var tauxPaiementEleves = nombreEleves > 0 ? Math.Round(((decimal)elevesAyantPaye / nombreEleves) * 100, 2) : 0;

            return new DashboardPaiementDto
            {
                Ecole = new EcoleInfoPaiementDto
                {
                    IdEcole = ecole.IdEcole,
                    NomEcole = ecole.Nom ?? "",
                    Logo = ecole.Logo
                },
                Periode = periodeDto,
                Resume = new ResumePaiementDto
                {
                    NombrePaiements = nombrePaiements,
                    MontantTotal = montantTotal,
                    MontantAttendu = montantAttendu,
                    TauxRecouvrement = tauxRecouvrement,
                    NombreEleves = nombreEleves,
                    ElevesAyantPaye = elevesAyantPaye,
                    ElevesEnRetard = elevesEnRetard,
                    TauxPaiementEleves = tauxPaiementEleves,
                    DevisePrincipale = "USD"
                },
                RepartitionParMode = new RepartitionModePaiementDto
                {
                    Modes = repartitionModes
                },
                Top5Frais = top5List
            };
        }

        /// <summary>
        /// Obtient le taux de paiement d'un élève sur une période
        /// </summary>
        public async Task<TauxPaiementEleveDto> GetTauxPaiementEleveAsync(
            int idEleve,
            DateTime? dateDebut,
            DateTime? dateFin,
            string? periode)
        {
            var periodeDto = ResolvePeriode(null, dateDebut, dateFin, periode);

            var eleve = await _context.Eleves
                .Include(e => e.Classe)
                .FirstOrDefaultAsync(e => e.IdEleve == idEleve);

            if (eleve == null)
                throw new KeyNotFoundException($"Élève avec l'ID {idEleve} introuvable");

            // Récupérer tous les frais attendus pour cet élève
            var fraisAttendus = await _context.Frais
                .Where(f => f.Direction.IdEcole == eleve.Classe.Direction.IdEcole)
                .Where(f => f.Statut == true)
                .ToListAsync();

            // Récupérer les paiements de l'élève sur la période
            var paiements = await _context.Paiements
                .Include(p => p.Frais)
                .Where(p => p.IdEleve == idEleve)
                .Where(p => p.DatePaiement >= periodeDto.DateDebut && p.DatePaiement <= periodeDto.DateFin)
                .Where(p => p.Statut == true)
                .ToListAsync();

            var fraisEleveList = new List<FraisEleveDto>();
            decimal montantTotal = 0;
            decimal montantPaye = 0;
            int nombreFraisPayes = 0;
            int nombreFraisEnAttente = 0;
            var joursRetards = new List<int>();

            foreach (var frais in fraisAttendus)
            {
                var paiementFrais = paiements.FirstOrDefault(p => p.IdFrais == frais.IdFrais);
                montantTotal += (decimal)frais.Montant;

                if (paiementFrais != null)
                {
                    montantPaye += (decimal)paiementFrais.Montant;
                    nombreFraisPayes++;

                    fraisEleveList.Add(new FraisEleveDto
                    {
                        IdFrais = frais.IdFrais,
                        LibelleFrais = frais.LibelleFrais ?? "",
                        MontantFrais = (decimal)frais.Montant,
                        Devise = frais.Devise ?? "USD",
                        StatutPaiement = "Payé",
                        DatePaiement = paiementFrais.DatePaiement,
                        MontantPaye = (decimal)paiementFrais.Montant
                    });
                }
                else
                {
                    nombreFraisEnAttente++;
                    fraisEleveList.Add(new FraisEleveDto
                    {
                        IdFrais = frais.IdFrais,
                        LibelleFrais = frais.LibelleFrais ?? "",
                        MontantFrais = (decimal)frais.Montant,
                        Devise = frais.Devise ?? "USD",
                        StatutPaiement = "En attente"
                    });
                }
            }

            decimal tauxPaiement = montantTotal > 0 ? Math.Round((montantPaye / montantTotal) * 100, 2) : 0;
            decimal montantRestant = montantTotal - montantPaye;

            return new TauxPaiementEleveDto
            {
                Eleve = new EleveInfoPaiementDto
                {
                    IdEleve = eleve.IdEleve,
                    NomComplet = eleve.NomComplet ?? $"{eleve.Prenom} {eleve.Nom}",
                    Matricule = eleve.Matricule ?? "",
                    Classe = eleve.Classe?.NomClasse ?? "",
                    PhotoUrl = eleve.PhotoUrl
                },
                Periode = periodeDto,
                FraisAttendus = fraisEleveList,
                Resume = new ResumePaiementEleveDto
                {
                    MontantTotal = montantTotal,
                    MontantPaye = montantPaye,
                    MontantRestant = montantRestant,
                    TauxPaiement = tauxPaiement,
                    NombreFraisPayes = nombreFraisPayes,
                    NombreFraisEnAttente = nombreFraisEnAttente,
                    NombreFraisPartiels = 0,
                    JoursRetardMoyen = 0
                }
            };
        }

        /// <summary>
        /// Obtient le taux de paiement d'une classe sur une période
        /// </summary>
        public async Task<TauxPaiementClasseDto> GetTauxPaiementClasseAsync(
            int idClasse,
            DateTime? date,
            DateTime? dateDebut,
            DateTime? dateFin,
            string? periode,
            bool includeDetails)
        {
            var periodeDto = ResolvePeriode(date, dateDebut, dateFin, periode);

            var classe = await _context.Classes
                .Include(c => c.Section)
                .Include(c => c.Option)
                .Include(c => c.Direction)
                .ThenInclude(d => d.Ecole)
                .FirstOrDefaultAsync(c => c.IdClasse == idClasse);

            if (classe == null)
                throw new KeyNotFoundException($"Classe avec l'ID {idClasse} introuvable");

            var elevesClasse = await _context.Eleves
                .Where(e => e.IdClasse == idClasse && e.Statut == true)
                .ToListAsync();

            int effectifTotal = elevesClasse.Count;

            var fraisEcole = await _context.Frais
                .Where(f => f.Direction.IdEcole == classe.Direction.IdEcole && f.Statut == true)
                .ToListAsync();

            decimal montantAttendu = fraisEcole.Sum(f => (decimal)f.Montant) * effectifTotal;

            var paiementsClasse = await _context.Paiements
                .Include(p => p.Eleve)
                .Include(p => p.Frais)
                .Where(p => p.Eleve.IdClasse == idClasse)
                .Where(p => p.DatePaiement >= periodeDto.DateDebut && p.DatePaiement <= periodeDto.DateFin)
                .Where(p => p.Statut == true)
                .ToListAsync();

            decimal montantPercu = (decimal)paiementsClasse.Sum(p => p.Montant);
            decimal montantRestant = montantAttendu - montantPercu;
            decimal tauxRecouvrement = montantAttendu > 0 ? Math.Round((montantPercu / montantAttendu) * 100, 2) : 0;

            var elevesAyantPaye = paiementsClasse.Select(p => p.IdEleve).Distinct().Count();
            var elevesEnRetard = effectifTotal - elevesAyantPaye;
            var elevesAJour = elevesAyantPaye;
            decimal tauxPaiementEleves = effectifTotal > 0 ? Math.Round(((decimal)elevesAyantPaye / effectifTotal) * 100, 2) : 0;

            var result = new TauxPaiementClasseDto
            {
                Classe = new ClasseInfoPaiementDto
                {
                    IdClasse = classe.IdClasse,
                    NomClasse = classe.NomClasse ?? "",
                    Section = classe.Section?.NomSection,
                    Option = classe.Option?.NomOption,
                    Direction = classe.Direction?.NomDirection,
                    Ecole = classe.Direction?.Ecole?.Nom,
                    EffectifTotal = effectifTotal
                },
                Periode = periodeDto,
                Statistiques = new StatistiquesClassePaiementDto
                {
                    MontantAttendu = montantAttendu,
                    MontantPercu = montantPercu,
                    MontantRestant = montantRestant,
                    TauxRecouvrement = tauxRecouvrement,
                    ElevesAyantPaye = elevesAyantPaye,
                    ElevesEnRetard = elevesEnRetard,
                    ElevesAJour = elevesAJour,
                    TauxPaiementEleves = tauxPaiementEleves
                }
            };

            // Ajouter les détails par élève si demandé
            if (includeDetails)
            {
                result.ParEleve = new List<ElevePaiementDto>();

                foreach (var elv in elevesClasse)
                {
                    var paiementsEleve = paiementsClasse.Where(p => p.IdEleve == elv.IdEleve).ToList();
                    decimal montantEleveAttendu = fraisEcole.Sum(f => (decimal)f.Montant);
                    decimal montantElevePaye = (decimal)paiementsEleve.Sum(p => p.Montant);
                    decimal montantEleveRestant = montantEleveAttendu - montantElevePaye;
                    decimal tauxElevePaiement = montantEleveAttendu > 0 ? Math.Round((montantElevePaye / montantEleveAttendu) * 100, 2) : 0;

                    string statut = tauxElevePaiement >= 100 ? "À jour" : tauxElevePaiement > 0 ? "Partiel" : "En retard";

                    result.ParEleve.Add(new ElevePaiementDto
                    {
                        Eleve = new EleveInfoPaiementDto
                        {
                            IdEleve = elv.IdEleve,
                            NomComplet = elv.NomComplet ?? $"{elv.Prenom} {elv.Nom}",
                            Matricule = elv.Matricule ?? "",
                            Classe = classe.NomClasse ?? ""
                        },
                        MontantAttendu = montantEleveAttendu,
                        MontantPaye = montantElevePaye,
                        MontantRestant = montantEleveRestant,
                        TauxPaiement = tauxElevePaiement,
                        Statut = statut
                    });
                }
            }

            return result;
        }

        // 🔔 File d'attente: préparer et planifier l'envoi des notifications paiement
        public async Task NotifierPaiementConfirmeAsync(int idPaiement, CancellationToken cancellationToken = default)
        {
            var paiement = await _context.Paiements.FindAsync(new object[] { idPaiement }, cancellationToken);
            if (paiement == null)
            {
                _logger.LogWarning("Notification post-PayIn ignorée : paiement {IdPaiement} introuvable", idPaiement);
                return;
            }

            if (paiement.IdEleve.HasValue)
            {
                await EnvoyerNotificationPaiementAuTuteurAsync(paiement, cancellationToken);
            }

            await NotifierMiseAJourDashboardAsync(paiement);
        }

        private async Task EnvoyerNotificationPaiementAuTuteurAsync(Paiement paiement, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!paiement.IdEleve.HasValue)
                {
                    _logger.LogDebug("Notification paiement ignorée car aucun élève lié (paiement {PaiementId})", paiement.IdPaiement);
                    return;
                }

                var preparation = await _notificationDispatcher.PreparePaiementAsync(paiement.IdPaiement, cancellationToken);
                if (preparation == null)
                {
                    return;
                }

                await _notificationJobQueue.EnqueueAsync(preparation, cancellationToken);
                _logger.LogInformation("🗂️ Notification paiement {PaiementId} mise en file pour traitement", paiement.IdPaiement);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "❌ Erreur inattendue lors de la notification paiement {PaiementId}",
                    paiement.IdPaiement);
            }
        }

        /// <summary>
        /// Notifier la mise à jour du dashboard en temps réel via SignalR
        /// </summary>
        private async Task NotifierMiseAJourDashboardAsync(Paiement paiement)
        {
            try
            {
                int? idEcole = null;

                // Récupérer l'idEcole depuis l'élève
                if (paiement.IdEleve.HasValue)
                {
                    // Charger l'élève avec ses relations pour récupérer l'école
                    var eleve = await _context.Eleves
                        .Include(e => e.Classe)
                            .ThenInclude(c => c.Direction)
                        .FirstOrDefaultAsync(e => e.IdEleve == paiement.IdEleve.Value);

                    if (eleve?.Classe?.Direction != null)
                    {
                        idEcole = eleve.Classe.Direction.IdEcole;
                    }
                }

                // Notifier la mise à jour du dashboard si on a trouvé l'école
                if (idEcole.HasValue)
                {
                    await _dashboardHubService.NotifyPaiementUpdateAsync(idEcole.Value);
                    _logger.LogInformation("📊 Notification dashboard paiement envoyée pour l'école {IdEcole}", idEcole.Value);
                }
                else
                {
                    _logger.LogWarning("⚠️ Impossible de déterminer l'école pour le paiement {PaiementId}", paiement.IdPaiement);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la notification dashboard pour le paiement {PaiementId}", paiement.IdPaiement);
            }
        }
    }
}
