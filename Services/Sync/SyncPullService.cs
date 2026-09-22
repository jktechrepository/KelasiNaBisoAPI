using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.Sync;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Services.Tarif;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services.Sync
{
    /// <summary>
    /// Pull lecture offline : élèves + frais dus (année scolaire courante).
    /// Delta approximatif via DateCreation / derniers paiements (pas de UpdatedAt natif).
    /// </summary>
    public class SyncPullService : ISyncPullService
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly IAnneeScolaireRepository _anneeScolaireRepository;
        private readonly ISyncIdempotencyService _idempotency;
        private readonly IFraisDuCalculator _fraisDuCalculator;

        public SyncPullService(
            KelasiNaBisoDbContext context,
            IAnneeScolaireRepository anneeScolaireRepository,
            ISyncIdempotencyService idempotency,
            IFraisDuCalculator fraisDuCalculator)
        {
            _context = context;
            _anneeScolaireRepository = anneeScolaireRepository;
            _idempotency = idempotency;
            _fraisDuCalculator = fraisDuCalculator;
        }

        public async Task<SyncBootstrapDto> GetBootstrapAsync(
            int idEcole,
            CancellationToken cancellationToken = default)
        {
            var watermark = _idempotency.CreateWatermark();
            var snapshot = _idempotency.CreateSnapshot(idEcole);

            var eleves = await GetElevesPageAsync(
                idEcole,
                new SyncRequestDto { PageSize = SyncRequestDto.MaxPageSize, Snapshot = snapshot },
                cancellationToken);

            var frais = await GetFraisDusPageAsync(
                idEcole,
                new SyncFraisDusRequestDto
                {
                    PageSize = SyncRequestDto.MaxPageSize,
                    Snapshot = snapshot,
                    OnlyOutstanding = true
                },
                cancellationToken);

            // Bootstrap : pages max ; si hasMore, le client doit enchaîner /eleves et /frais-dus.
            return new SyncBootstrapDto
            {
                IdEcole = idEcole,
                Watermark = watermark,
                Snapshot = snapshot,
                Eleves = eleves.Items,
                FraisDus = frais.Items
            };
        }

        public async Task<SyncPageDto<EleveSyncDto>> GetElevesPageAsync(
            int idEcole,
            SyncRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var pageSize = request.ResolvePageSize();
            var snapshot = string.IsNullOrWhiteSpace(request.Snapshot)
                ? _idempotency.CreateSnapshot(idEcole)
                : request.Snapshot!;
            var nextSince = _idempotency.CreateWatermark();

            var annee = await _anneeScolaireRepository.GetAnneeCouranteAsync(idEcole);
            if (annee == null)
            {
                return EmptyPage<EleveSyncDto>(snapshot, nextSince);
            }

            SyncPullHelpers.TryParseSince(request.Since, out var sinceUtc);
            var hasSince = !string.IsNullOrWhiteSpace(request.Since)
                           && SyncPullHelpers.TryParseSince(request.Since, out sinceUtc);

            var inscriptionsQuery = ConfirmedInscriptionsQuery(idEcole, annee.IdAnneeScolaire);

            // Une inscription confirmée par élève (la plus récente).
            var inscriptionRows = await inscriptionsQuery
                .Select(i => new
                {
                    i.IdEleve,
                    i.IdClasse,
                    NomClasse = i.Classe != null ? i.Classe.NomClasse : null,
                    i.DateInscription,
                    i.DateCreation
                })
                .ToListAsync(cancellationToken);

            var bestInscription = inscriptionRows
                .GroupBy(i => i.IdEleve)
                .Select(g => g.OrderByDescending(x => x.DateInscription).First())
                .ToDictionary(x => x.IdEleve);

            if (bestInscription.Count == 0)
                return EmptyPage<EleveSyncDto>(snapshot, nextSince);

            var eleveIds = bestInscription.Keys.ToList();
            var eleves = await _context.Eleves.AsNoTracking()
                .Where(e => eleveIds.Contains(e.IdEleve))
                .ToListAsync(cancellationToken);

            IEnumerable<Eleve> filtered = eleves;
            if (hasSince)
            {
                filtered = eleves.Where(e =>
                {
                    if (e.DateCreation > sinceUtc)
                        return true;
                    if (!bestInscription.TryGetValue(e.IdEleve, out var insc))
                        return false;
                    return insc.DateInscription > sinceUtc || insc.DateCreation > sinceUtc;
                });
            }

            if (SyncPullHelpers.TryParseEleveCursor(request.Cursor, out var cursorId))
            {
                filtered = filtered.Where(e => e.IdEleve > cursorId);
            }

            var ordered = filtered
                .OrderBy(e => e.IdEleve)
                .Take(pageSize + 1)
                .ToList();

            var hasMore = ordered.Count > pageSize;
            if (hasMore)
                ordered = ordered.Take(pageSize).ToList();

            var items = ordered.Select(e =>
            {
                bestInscription.TryGetValue(e.IdEleve, out var insc);
                return new EleveSyncDto
                {
                    IdEleve = e.IdEleve,
                    Matricule = e.Matricule,
                    Nom = e.Nom,
                    Postnom = e.Postnom,
                    Prenom = e.Prenom,
                    IdClasse = insc?.IdClasse,
                    NomClasse = insc?.NomClasse,
                    IsActif = e.Statut != false,
                    UpdatedAt = MaxDate(e.DateCreation, insc?.DateInscription, insc?.DateCreation)
                };
            }).ToList();

            return new SyncPageDto<EleveSyncDto>
            {
                Snapshot = snapshot,
                Items = items,
                HasMore = hasMore,
                NextCursor = hasMore && items.Count > 0
                    ? SyncPullHelpers.FormatEleveCursor(items[^1].IdEleve)
                    : null,
                NextSince = nextSince
            };
        }

        public async Task<SyncPageDto<FraisDuSyncDto>> GetFraisDusPageAsync(
            int idEcole,
            SyncFraisDusRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var pageSize = request.ResolvePageSize();
            var snapshot = string.IsNullOrWhiteSpace(request.Snapshot)
                ? _idempotency.CreateSnapshot(idEcole)
                : request.Snapshot!;
            var nextSince = _idempotency.CreateWatermark();

            var annee = await _anneeScolaireRepository.GetAnneeCouranteAsync(idEcole);
            if (annee == null)
                return EmptyPage<FraisDuSyncDto>(snapshot, nextSince);

            var hasSince = SyncPullHelpers.TryParseSince(request.Since, out var sinceUtc);

            var inscriptions = await ConfirmedInscriptionsQuery(idEcole, annee.IdAnneeScolaire)
                .Select(i => new
                {
                    i.IdEleve,
                    i.IdClasse,
                    IdDirection = i.Classe != null ? i.Classe.IdDirection : (int?)null
                })
                .ToListAsync(cancellationToken);

            var bestByEleve = inscriptions
                .Where(i => i.IdDirection.HasValue)
                .GroupBy(i => i.IdEleve)
                .Select(g => g.First())
                .ToList();

            if (bestByEleve.Count == 0)
                return EmptyPage<FraisDuSyncDto>(snapshot, nextSince);

            var fraisList = await _context.Frais.AsNoTracking()
                .Include(f => f.FraisClasses)
                .Include(f => f.FraisDirections)
                .Where(f => f.Statut == true
                            && f.IdEcole == idEcole
                            && f.IdAnneeScolaire == annee.IdAnneeScolaire)
                .ToListAsync(cancellationToken);

            var posts = new List<(int IdEleve, Frais Frais)>();
            foreach (var insc in bestByEleve)
            {
                foreach (var frais in fraisList)
                {
                    if (FraisEligibility.IsEligibleForInscription(
                            frais,
                            idEcole,
                            insc.IdDirection!.Value,
                            annee.IdAnneeScolaire,
                            insc.IdClasse))
                    {
                        posts.Add((insc.IdEleve, frais));
                    }
                }
            }

            if (posts.Count == 0)
                return EmptyPage<FraisDuSyncDto>(snapshot, nextSince);

            var eleveIds = posts.Select(p => p.IdEleve).Distinct().ToList();
            var fraisIds = posts.Select(p => p.Frais.IdFrais).Distinct().ToList();

            var paiements = await _context.Paiements.AsNoTracking()
                .Where(p => p.Statut == true
                            && p.IdEleve.HasValue
                            && p.IdFrais.HasValue
                            && eleveIds.Contains(p.IdEleve!.Value)
                            && fraisIds.Contains(p.IdFrais!.Value)
                            && p.StatutPaiement != null
                            && (p.StatutPaiement == "Confirmé"
                                || p.StatutPaiement == "Confirme"
                                || p.StatutPaiement.StartsWith("Confirm")))
                .Select(p => new { IdEleve = p.IdEleve!.Value, IdFrais = p.IdFrais!.Value, p.Montant, p.DateCreation })
                .ToListAsync(cancellationToken);

            var payeByKey = paiements
                .GroupBy(p => (p.IdEleve, p.IdFrais))
                .ToDictionary(
                    g => g.Key,
                    g => (
                        Total: g.Sum(x => (decimal)x.Montant),
                        LastPay: g.Max(x => x.DateCreation)
                    ));

            var pairs = posts.Select(p => (p.IdEleve, p.Frais.IdFrais)).Distinct().ToList();
            var dus = await _fraisDuCalculator.GetMontantsDuBatchAsync(pairs, cancellationToken: cancellationToken);

            var rows = new List<FraisDuSyncDto>();
            foreach (var (idEleve, frais) in posts)
            {
                payeByKey.TryGetValue((idEleve, frais.IdFrais), out var pay);
                dus.TryGetValue((idEleve, frais.IdFrais), out var amounts);
                var montantCatalogue = amounts?.MontantCatalogue ?? (decimal)frais.Montant;
                var montantTotal = amounts?.MontantDuEffectif ?? montantCatalogue;
                var montantPaye = pay.Total;
                var montantDu = Math.Max(0m, montantTotal - montantPaye);
                if (request.OnlyOutstanding && montantDu <= 0m)
                    continue;

                var dateMod = MaxDate(frais.DateCreation, pay.LastPay == default ? null : pay.LastPay);
                if (hasSince && dateMod.HasValue && dateMod.Value <= sinceUtc)
                    continue;
                if (hasSince && !dateMod.HasValue)
                    continue;

                rows.Add(new FraisDuSyncDto
                {
                    IdFrais = frais.IdFrais,
                    IdEleve = idEleve,
                    Libelle = frais.LibelleFrais,
                    MontantTotal = montantTotal,
                    MontantPaye = montantPaye,
                    MontantDu = montantDu,
                    CodeDevise = frais.Devise,
                    DateModification = dateMod,
                    MontantCatalogue = montantCatalogue,
                    MontantReduction = amounts?.MontantReduction ?? 0m,
                    CodeCategorie = amounts?.CodeCategorie
                });
            }

            IEnumerable<FraisDuSyncDto> ordered = rows
                .OrderBy(r => r.IdEleve)
                .ThenBy(r => r.IdFrais);

            if (SyncPullHelpers.TryParseFraisDuCursor(request.Cursor, out var cEleve, out var cFrais))
            {
                ordered = ordered.Where(r =>
                    r.IdEleve > cEleve
                    || (r.IdEleve == cEleve && r.IdFrais > cFrais));
            }

            var page = ordered.Take(pageSize + 1).ToList();
            var hasMore = page.Count > pageSize;
            if (hasMore)
                page = page.Take(pageSize).ToList();

            return new SyncPageDto<FraisDuSyncDto>
            {
                Snapshot = snapshot,
                Items = page,
                HasMore = hasMore,
                NextCursor = hasMore && page.Count > 0
                    ? SyncPullHelpers.FormatFraisDuCursor(page[^1].IdEleve, page[^1].IdFrais)
                    : null,
                NextSince = nextSince
            };
        }

        private IQueryable<Inscription> ConfirmedInscriptionsQuery(int idEcole, int idAnneeScolaire)
        {
            return _context.Inscriptions.AsNoTracking()
                .Include(i => i.Classe)
                .Where(i =>
                    i.Statut == true
                    && i.IdEcole == idEcole
                    && i.IdAnneeScolaire == idAnneeScolaire
                    && i.StatutInscription != null
                    && (i.StatutInscription == InscriptionActiveRules.StatutConfirme
                        || i.StatutInscription == "Confirme"
                        || i.StatutInscription.StartsWith("Confirm")));
        }

        private static SyncPageDto<T> EmptyPage<T>(string snapshot, string nextSince) => new()
        {
            Snapshot = snapshot,
            Items = new List<T>(),
            HasMore = false,
            NextCursor = null,
            NextSince = nextSince
        };

        private static DateTime? MaxDate(params DateTime?[] values)
        {
            DateTime? max = null;
            foreach (var v in values)
            {
                if (!v.HasValue) continue;
                if (!max.HasValue || v.Value > max.Value)
                    max = v.Value;
            }
            return max;
        }
    }
}
