using System.Globalization;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models.DTOs.Paiement;
using KelasiNaBiso.Models.DTOs.Reporting;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services.MokoAfrika;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class DashboardFinancierService : IDashboardFinancierService
    {
        private const int MaxRecentPaiements = 15;

        private readonly KelasiNaBisoDbContext _context;
        private readonly EleveAnneeScopeHelper _scope;
        private readonly IEcolePaiementMobileService _ecolePaiementMobileService;

        public DashboardFinancierService(
            KelasiNaBisoDbContext context,
            EleveAnneeScopeHelper scope,
            IEcolePaiementMobileService ecolePaiementMobileService)
        {
            _context = context;
            _scope = scope;
            _ecolePaiementMobileService = ecolePaiementMobileService;
        }

        public Task<DashboardFinancierDto> GetDashboardFinancierAsync(
            int idEcole,
            int idUtilisateur,
            int? idAnneeScolaire = null,
            DateTime? date = null,
            string scope = DashboardCaissierScopes.Moi,
            bool allowEcoleScope = false) =>
            BuildDashboardAsync(idEcole, idUtilisateur, idAnneeScolaire, date, scope, allowEcoleScope);

        public async Task<DashboardFinancierClotureDto> GetClotureFinancierAsync(
            int idEcole,
            int idUtilisateur,
            int? idAnneeScolaire = null,
            DateTime? date = null,
            string scope = DashboardCaissierScopes.Moi,
            bool allowEcoleScope = false)
        {
            var dashboard = await BuildDashboardAsync(
                idEcole, idUtilisateur, idAnneeScolaire, date, scope, allowEcoleScope);

            var effectiveScope = ResolveScope(scope, allowEcoleScope);
            int? filterUtilisateur = effectiveScope == DashboardCaissierScopes.Ecole ? null : idUtilisateur;
            var (resolvedEcole, idAnnee) = await _scope.ResolveEcoleAnneeAsync(idEcole, idAnneeScolaire);
            var targetDate = (date ?? DateTime.Now).Date;
            var allPaiements = await GetPaiementsDuJourAsync(resolvedEcole, idAnnee, filterUtilisateur, targetDate);

            return new DashboardFinancierClotureDto
            {
                Ecole = dashboard.Ecole,
                IdAnneeScolaire = dashboard.IdAnneeScolaire,
                LibelleAnneeScolaire = dashboard.LibelleAnneeScolaire,
                Periode = dashboard.Periode,
                Resume = dashboard.Resume,
                RepartitionParMode = dashboard.RepartitionParMode,
                DerniersPaiements = dashboard.DerniersPaiements,
                Moko = dashboard.Moko,
                Scope = dashboard.Scope,
                GenereLe = DateTime.Now,
                TousLesPaiements = BuildDerniersPaiements(allPaiements, int.MaxValue)
            };
        }

        private async Task<DashboardFinancierDto> BuildDashboardAsync(
            int idEcole,
            int idUtilisateur,
            int? idAnneeScolaire,
            DateTime? date,
            string scope,
            bool allowEcoleScope)
        {
            if (idUtilisateur <= 0)
                throw new InvalidOperationException("Utilisateur non identifié.");

            var effectiveScope = ResolveScope(scope, allowEcoleScope);
            int? filterUtilisateur = effectiveScope == DashboardCaissierScopes.Ecole ? null : idUtilisateur;

            var (resolvedEcole, idAnnee) = await _scope.ResolveEcoleAnneeAsync(idEcole, idAnneeScolaire);
            var targetDate = (date ?? DateTime.Now).Date;
            var periode = BuildJourPeriode(targetDate);

            var ecole = await _context.Ecoles.AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdEcole == resolvedEcole);
            if (ecole == null)
                throw new KeyNotFoundException($"École avec l'ID {resolvedEcole} introuvable");

            var libelleAnnee = await _context.AnneeScolaires.AsNoTracking()
                .Where(a => a.IdAnneeScolaire == idAnnee)
                .Select(a => a.LibelleAnneeScolaire)
                .FirstOrDefaultAsync();

            var paiements = await GetPaiementsDuJourAsync(resolvedEcole, idAnnee, filterUtilisateur, targetDate);
            var (moko, payInsReussis, payInsEnAttente, payInsEchoues) =
                await BuildMokoResumeAsync(resolvedEcole, filterUtilisateur, targetDate);

            var recents = BuildDerniersPaiements(paiements, MaxRecentPaiements);

            var resume = new ResumeFinancierDto
            {
                NombrePaiements = paiements.Count,
                MontantTotal = paiements.Sum(p => p.Montant),
                MontantConfirme = paiements.Count(p => string.Equals(p.Statut, "Confirme", StringComparison.OrdinalIgnoreCase) || string.Equals(p.Statut, "Confirmé", StringComparison.OrdinalIgnoreCase)),
                MontantEnAttente = paiements.Count(p => string.Equals(p.Statut, "En attente", StringComparison.OrdinalIgnoreCase)),
                MontantEchoue = paiements.Count(p => string.Equals(p.Statut, "Echoue", StringComparison.OrdinalIgnoreCase) || string.Equals(p.Statut, "Échoué", StringComparison.OrdinalIgnoreCase)),
                SoldeNet = paiements.Sum(p => p.Montant)
            };

            return new DashboardFinancierDto
            {
                Ecole = new EcoleInfoPaiementDto
                {
                    IdEcole = ecole.IdEcole,
                    NomEcole = ecole.Nom ?? string.Empty,
                    Logo = ecole.Logo
                },
                IdAnneeScolaire = idAnnee,
                LibelleAnneeScolaire = libelleAnnee,
                Periode = periode,
                Scope = effectiveScope,
                Resume = resume,
                RepartitionParMode = BuildRepartitionParMode(paiements),
                DerniersPaiements = recents,
                Moko = moko
            };
        }

        private static string ResolveScope(string? scope, bool allowEcoleScope)
        {
            if (allowEcoleScope
                && string.Equals(scope, DashboardCaissierScopes.Ecole, StringComparison.OrdinalIgnoreCase))
            {
                return DashboardCaissierScopes.Ecole;
            }

            return DashboardCaissierScopes.Moi;
        }

        private async Task<List<PaiementRow>> GetPaiementsDuJourAsync(
            int idEcole,
            int idAnneeScolaire,
            int? idUtilisateur,
            DateTime targetDate)
        {
            var debut = targetDate.Date;
            var finExclusive = debut.AddDays(1);
            var eleveIds = _scope.GetEleveIdsInEcoleAnnee(idEcole, idAnneeScolaire);

            var query = _context.Paiements
                .AsNoTracking()
                .Include(p => p.Eleve)
                .Include(p => p.Frais)
                .Where(p => p.Statut == true)
                .Where(p => p.DatePaiement >= debut && p.DatePaiement < finExclusive)
                .Where(p => p.IdEleve != null && eleveIds.Contains(p.IdEleve.Value))
                .Where(p => p.IdFrais != null && _context.Frais.Any(f =>
                    f.IdFrais == p.IdFrais && f.IdAnneeScolaire == idAnneeScolaire));

            if (idUtilisateur.HasValue)
                query = query.Where(p => p.IdUtilisateur == idUtilisateur.Value);

            var rows = await query
                .OrderByDescending(p => p.DatePaiement)
                .Select(p => new PaiementRow
                {
                    IdPaiement = p.IdPaiement,
                    DatePaiement = p.DatePaiement,
                    Montant = (decimal)p.Montant,
                    ModePaiement = p.ModePaiement ?? "Non spécifié",
                    Statut = p.StatutPaiement ?? string.Empty,
                    NomEleve = p.Eleve != null ? p.Eleve.NomComplet : null,
                    LibelleFrais = p.Frais != null ? p.Frais.LibelleFrais : null,
                    ReferenceMoko = p.ReferenceTransaction
                })
                .ToListAsync();

            await EnrichMokoReferencesAsync(rows);
            return rows;
        }

        private async Task EnrichMokoReferencesAsync(List<PaiementRow> rows)
        {
            var ids = rows.Select(r => r.IdPaiement).ToList();
            if (ids.Count == 0)
                return;

            var mokoRefs = await _context.TransactionsMoko
                .AsNoTracking()
                .Where(t => t.IdPaiement != null && ids.Contains(t.IdPaiement.Value))
                .Where(t => t.Action == MokoActions.Debit)
                .GroupBy(t => t.IdPaiement)
                .Select(g => new { IdPaiement = g.Key, Reference = g.OrderByDescending(t => t.DateCreation).First().Reference })
                .ToListAsync();

            var refByPaiement = mokoRefs.ToDictionary(x => x.IdPaiement!.Value, x => x.Reference);
            foreach (var row in rows)
            {
                if (refByPaiement.TryGetValue(row.IdPaiement, out var reference))
                    row.ReferenceMoko = reference;
            }
        }

        private async Task<(MokoCaissierResumeDto Moko, int Reussis, int EnAttente, int Echoues)> BuildMokoResumeAsync(
            int idEcole,
            int? idUtilisateur,
            DateTime targetDate)
        {
            var overview = await _ecolePaiementMobileService.GetOverviewAsync(idEcole);
            var debut = targetDate.Date;
            var finExclusive = debut.AddDays(1);

            var pendingQuery =
                from t in _context.TransactionsMoko.AsNoTracking()
                join p in _context.Paiements on t.IdPaiement equals p.IdPaiement
                where t.IdEcole == idEcole
                      && t.Action == MokoActions.Debit
                      && t.Status == MokoTransactionStatuses.Pending
                select new { t, p };

            if (idUtilisateur.HasValue)
                pendingQuery = pendingQuery.Where(x => x.p.IdUtilisateur == idUtilisateur.Value);

            var pendingTx = await pendingQuery
                .OrderByDescending(x => x.t.DateCreation)
                .Select(x => new
                {
                    x.t.Reference,
                    x.t.IdPaiement,
                    x.t.AmountNet,
                    x.t.Amount,
                    x.t.DateCreation,
                    NomEleve = x.p.Eleve != null ? x.p.Eleve.NomComplet : null,
                    LibelleFrais = x.p.Frais != null ? x.p.Frais.LibelleFrais : null
                })
                .ToListAsync();

            var payInsDuJourQuery =
                from t in _context.TransactionsMoko.AsNoTracking()
                join p in _context.Paiements on t.IdPaiement equals p.IdPaiement
                where t.IdEcole == idEcole
                      && t.Action == MokoActions.Debit
                      && t.DateCreation >= debut
                      && t.DateCreation < finExclusive
                select new { t.Status, p.IdUtilisateur };

            if (idUtilisateur.HasValue)
                payInsDuJourQuery = payInsDuJourQuery.Where(x => x.IdUtilisateur == idUtilisateur.Value);

            var payInsDuJour = await payInsDuJourQuery.Select(x => x.Status).ToListAsync();

            var moko = new MokoCaissierResumeDto
            {
                EstConfigure = overview.EstConfigure,
                MesPayInsEnAttente = pendingTx.Count,
                PayInsEnAttente = pendingTx.Select(t => new MokoPayInEnAttenteDto
                {
                    Reference = t.Reference,
                    IdPaiement = t.IdPaiement,
                    Montant = t.AmountNet ?? t.Amount,
                    NomEleve = t.NomEleve,
                    LibelleFrais = t.LibelleFrais,
                    DateCreation = t.DateCreation
                }).ToList()
            };

            return (
                moko,
                payInsDuJour.Count(s => s == MokoTransactionStatuses.Success),
                payInsDuJour.Count(s => s == MokoTransactionStatuses.Pending),
                payInsDuJour.Count(s => s == MokoTransactionStatuses.Error || s == MokoTransactionStatuses.Timeout));
        }

        private static RepartitionModePaiementDto BuildRepartitionParMode(List<PaiementRow> paiements)
        {
            var montantTotal = paiements.Sum(p => p.Montant);
            var modes = new Dictionary<string, ModePaiementStatsDto>();

            foreach (var groupe in paiements.GroupBy(p => p.ModePaiement))
            {
                var montant = groupe.Sum(p => p.Montant);
                modes[groupe.Key] = new ModePaiementStatsDto
                {
                    Nombre = groupe.Count(),
                    Montant = montant,
                    Pourcentage = montantTotal > 0 ? Math.Round(montant / montantTotal * 100, 2) : 0
                };
            }

            return new RepartitionModePaiementDto { Modes = modes };
        }

        private static List<PaiementCaissierRecentDto> BuildDerniersPaiements(List<PaiementRow> paiements, int max) =>
            paiements
                .Take(max)
                .Select(p => new PaiementCaissierRecentDto
                {
                    IdPaiement = p.IdPaiement,
                    DatePaiement = p.DatePaiement,
                    Montant = p.Montant,
                    ModePaiement = p.ModePaiement,
                    Statut = p.Statut,
                    NomEleve = p.NomEleve,
                    LibelleFrais = p.LibelleFrais,
                    ReferenceMoko = p.ReferenceMoko
                })
                .ToList();

        private static PeriodeDto BuildJourPeriode(DateTime targetDate)
        {
            var culture = new CultureInfo("fr-FR");
            return new PeriodeDto
            {
                Type = "jour",
                Date = targetDate,
                DateDebut = targetDate,
                DateFin = targetDate,
                JoursOuvrables = targetDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday ? 0 : 1,
                Libelle = targetDate.ToString("dddd dd MMMM yyyy", culture)
            };
        }

        private sealed class PaiementRow
        {
            public int IdPaiement { get; set; }
            public DateTime DatePaiement { get; set; }
            public decimal Montant { get; set; }
            public string ModePaiement { get; set; } = string.Empty;
            public string Statut { get; set; } = string.Empty;
            public string? NomEleve { get; set; }
            public string? LibelleFrais { get; set; }
            public string? ReferenceMoko { get; set; }
        }
    }
}
