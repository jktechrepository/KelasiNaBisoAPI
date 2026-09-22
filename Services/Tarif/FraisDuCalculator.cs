using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services.Tarif
{
    public class FraisDuCalculator : IFraisDuCalculator
    {
        private readonly KelasiNaBisoDbContext _context;

        public FraisDuCalculator(KelasiNaBisoDbContext context)
        {
            _context = context;
        }

        public decimal Apply(decimal montantCatalogue, string? typeRegle, decimal valeur)
        {
            if (string.IsNullOrWhiteSpace(typeRegle))
                return RoundMoney(montantCatalogue);

            return typeRegle switch
            {
                TypeRegleExonerationFrais.Totale => 0m,
                TypeRegleExonerationFrais.Pourcentage => RoundMoney(
                    montantCatalogue * (1m - ClampPercent(valeur) / 100m)),
                TypeRegleExonerationFrais.MontantReduction => RoundMoney(
                    Math.Max(0m, montantCatalogue - Math.Max(0m, valeur))),
                TypeRegleExonerationFrais.MontantDuFixe => RoundMoney(Math.Max(0m, valeur)),
                _ => RoundMoney(montantCatalogue)
            };
        }

        public async Task<decimal> GetMontantDuEffectifAsync(
            int idEleve,
            int idFrais,
            DateTime? asOfUtc = null,
            CancellationToken cancellationToken = default)
        {
            var map = await GetMontantsDuBatchAsync(
                new[] { (idEleve, idFrais) },
                asOfUtc,
                cancellationToken);
            return map.TryGetValue((idEleve, idFrais), out var a)
                ? a.MontantDuEffectif
                : 0m;
        }

        public async Task<FraisDuDetailDto?> GetDetailAsync(
            int idEleve,
            int idFrais,
            DateTime? asOfUtc = null,
            CancellationToken cancellationToken = default)
        {
            var map = await GetMontantsDuBatchAsync(
                new[] { (idEleve, idFrais) },
                asOfUtc,
                cancellationToken);
            if (!map.TryGetValue((idEleve, idFrais), out var amounts))
                return null;

            var frais = await _context.Frais.AsNoTracking()
                .Where(f => f.IdFrais == idFrais)
                .Select(f => new { f.LibelleFrais, f.Devise })
                .FirstOrDefaultAsync(cancellationToken);

            var paye = await SumPayeAsync(idEleve, new[] { idFrais }, cancellationToken);
            paye.TryGetValue(idFrais, out var montantPaye);

            return ToDetail(idEleve, idFrais, frais?.LibelleFrais, frais?.Devise, amounts, montantPaye);
        }

        public async Task<IReadOnlyList<FraisDuDetailDto>> GetDetailsForEleveAsync(
            int idEleve,
            int? idAnneeScolaire = null,
            DateTime? asOfUtc = null,
            CancellationToken cancellationToken = default)
        {
            var fraisQuery = _context.Frais.AsNoTracking()
                .Where(f => f.Statut != false);

            if (idAnneeScolaire.HasValue && idAnneeScolaire.Value > 0)
            {
                fraisQuery = fraisQuery.Where(f => f.IdAnneeScolaire == idAnneeScolaire.Value);
            }
            else
            {
                // Limiter à l'école de l'élève via inscription confirmée la plus récente
                var idEcole = await _context.Inscriptions.AsNoTracking()
                    .Where(i => i.IdEleve == idEleve && i.Statut == true)
                    .OrderByDescending(i => i.IdInscription)
                    .Select(i => (int?)i.IdEcole)
                    .FirstOrDefaultAsync(cancellationToken);

                if (idEcole.HasValue)
                    fraisQuery = fraisQuery.Where(f => f.IdEcole == idEcole.Value);
            }

            var fraisList = await fraisQuery
                .Select(f => new { f.IdFrais, f.LibelleFrais, f.Devise })
                .ToListAsync(cancellationToken);

            if (fraisList.Count == 0)
                return Array.Empty<FraisDuDetailDto>();

            var pairs = fraisList.Select(f => (idEleve, f.IdFrais)).ToList();
            var amounts = await GetMontantsDuBatchAsync(pairs, asOfUtc, cancellationToken);
            var paye = await SumPayeAsync(idEleve, fraisList.Select(f => f.IdFrais).ToList(), cancellationToken);

            var result = new List<FraisDuDetailDto>(fraisList.Count);
            foreach (var f in fraisList)
            {
                if (!amounts.TryGetValue((idEleve, f.IdFrais), out var a))
                    continue;
                paye.TryGetValue(f.IdFrais, out var montantPaye);
                result.Add(ToDetail(idEleve, f.IdFrais, f.LibelleFrais, f.Devise, a, montantPaye));
            }

            return result;
        }

        public async Task<IReadOnlyDictionary<int, decimal>> GetMontantsDuEffectifsByFraisAsync(
            int idEleve,
            IReadOnlyList<int> idFraisList,
            DateTime? asOfUtc = null,
            CancellationToken cancellationToken = default)
        {
            if (idFraisList.Count == 0)
                return new Dictionary<int, decimal>();

            var pairs = idFraisList.Distinct().Select(id => (idEleve, id)).ToList();
            var map = await GetMontantsDuBatchAsync(pairs, asOfUtc, cancellationToken);
            return map.ToDictionary(kv => kv.Key.IdFrais, kv => kv.Value.MontantDuEffectif);
        }

        public async Task<IReadOnlyDictionary<(int IdEleve, int IdFrais), FraisDuAmounts>> GetMontantsDuBatchAsync(
            IReadOnlyList<(int IdEleve, int IdFrais)> pairs,
            DateTime? asOfUtc = null,
            CancellationToken cancellationToken = default)
        {
            var result = new Dictionary<(int IdEleve, int IdFrais), FraisDuAmounts>();
            if (pairs.Count == 0)
                return result;

            var asOf = asOfUtc ?? DateTime.UtcNow;
            var distinctPairs = pairs.Distinct().ToList();
            var fraisIds = distinctPairs.Select(p => p.IdFrais).Distinct().ToList();
            var eleveIds = distinctPairs.Select(p => p.IdEleve).Distinct().ToList();

            var fraisRows = await _context.Frais.AsNoTracking()
                .Where(f => fraisIds.Contains(f.IdFrais))
                .Select(f => new { f.IdFrais, f.Montant, f.IdAnneeScolaire, f.IdEcole })
                .ToListAsync(cancellationToken);

            var fraisById = fraisRows.ToDictionary(f => f.IdFrais);

            var anneeIds = fraisRows.Select(f => f.IdAnneeScolaire).Distinct().ToList();

            var affectations = await _context.AffectationsEleveCategorieTarif.AsNoTracking()
                .Where(a => eleveIds.Contains(a.IdEleve)
                            && anneeIds.Contains(a.IdAnneeScolaire)
                            && a.DateDebut <= asOf
                            && (a.DateFin == null || a.DateFin >= asOf))
                .Select(a => new
                {
                    a.IdEleve,
                    a.IdAnneeScolaire,
                    a.IdCategorieEleveTarif,
                    a.DateDebut,
                    a.DateFin
                })
                .ToListAsync(cancellationToken);

            // Une affectation active par (élève, année) : priorité DateFin null, puis DateDebut max
            var activeAff = affectations
                .GroupBy(a => (a.IdEleve, a.IdAnneeScolaire))
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.DateFin == null)
                        .ThenByDescending(x => x.DateDebut)
                        .First());

            var categorieIds = activeAff.Values.Select(a => a.IdCategorieEleveTarif).Distinct().ToList();

            var categories = categorieIds.Count == 0
                ? new Dictionary<int, (string Code, string Libelle)>()
                : await _context.CategoriesEleveTarif.AsNoTracking()
                    .Where(c => categorieIds.Contains(c.IdCategorieEleveTarif) && c.Statut)
                    .ToDictionaryAsync(
                        c => c.IdCategorieEleveTarif,
                        c => (c.Code, c.Libelle),
                        cancellationToken);

            var regles = categorieIds.Count == 0
                ? new List<RegleExonerationFrais>()
                : await _context.ReglesExonerationFrais.AsNoTracking()
                    .Where(r => r.Statut
                                && anneeIds.Contains(r.IdAnneeScolaire)
                                && categorieIds.Contains(r.IdCategorieEleveTarif)
                                && fraisIds.Contains(r.IdFrais))
                    .ToListAsync(cancellationToken);

            var regleByKey = regles
                .GroupBy(r => (r.IdAnneeScolaire, r.IdCategorieEleveTarif, r.IdFrais))
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var (idEleve, idFrais) in distinctPairs)
            {
                if (!fraisById.TryGetValue(idFrais, out var frais))
                    continue;

                var catalogue = (decimal)frais.Montant;
                string? typeRegle = null;
                decimal? valeurRegle = null;
                int? idCat = null;
                string? codeCat = null;
                string? libCat = null;

                if (activeAff.TryGetValue((idEleve, frais.IdAnneeScolaire), out var aff)
                    && categories.ContainsKey(aff.IdCategorieEleveTarif))
                {
                    idCat = aff.IdCategorieEleveTarif;
                    var cat = categories[aff.IdCategorieEleveTarif];
                    codeCat = cat.Code;
                    libCat = cat.Libelle;

                    if (regleByKey.TryGetValue(
                            (frais.IdAnneeScolaire, aff.IdCategorieEleveTarif, idFrais),
                            out var regle))
                    {
                        typeRegle = regle.TypeRegle;
                        valeurRegle = regle.Valeur;
                    }
                }

                var du = Apply(catalogue, typeRegle, valeurRegle ?? 0m);
                result[(idEleve, idFrais)] = new FraisDuAmounts
                {
                    MontantCatalogue = RoundMoney(catalogue),
                    MontantDuEffectif = du,
                    MontantReduction = RoundMoney(Math.Max(0m, catalogue - du)),
                    IdCategorieEleveTarif = idCat,
                    CodeCategorie = codeCat,
                    LibelleCategorie = libCat,
                    TypeRegle = typeRegle,
                    ValeurRegle = valeurRegle
                };
            }

            return result;
        }

        private async Task<Dictionary<int, decimal>> SumPayeAsync(
            int idEleve,
            IReadOnlyList<int> fraisIds,
            CancellationToken cancellationToken)
        {
            if (fraisIds.Count == 0)
                return new Dictionary<int, decimal>();

            var rows = await _context.Paiements.AsNoTracking()
                .Where(p => p.IdEleve == idEleve
                            && p.Statut == true
                            && p.IdFrais.HasValue
                            && fraisIds.Contains(p.IdFrais.Value)
                            && p.StatutPaiement != null
                            && (p.StatutPaiement == "Confirmé"
                                || p.StatutPaiement == "Confirme"
                                || p.StatutPaiement.StartsWith("Confirm")))
                .GroupBy(p => p.IdFrais!.Value)
                .Select(g => new { IdFrais = g.Key, Total = g.Sum(x => (decimal)x.Montant) })
                .ToListAsync(cancellationToken);

            return rows.ToDictionary(x => x.IdFrais, x => x.Total);
        }

        private static FraisDuDetailDto ToDetail(
            int idEleve,
            int idFrais,
            string? libelle,
            string? devise,
            FraisDuAmounts amounts,
            decimal montantPaye)
        {
            return new FraisDuDetailDto
            {
                IdEleve = idEleve,
                IdFrais = idFrais,
                LibelleFrais = libelle,
                CodeDevise = devise,
                MontantCatalogue = amounts.MontantCatalogue,
                MontantDuEffectif = amounts.MontantDuEffectif,
                MontantReduction = amounts.MontantReduction,
                MontantPaye = montantPaye,
                ResteAPayer = Math.Max(0m, amounts.MontantDuEffectif - montantPaye),
                IdCategorieEleveTarif = amounts.IdCategorieEleveTarif,
                CodeCategorie = amounts.CodeCategorie,
                LibelleCategorie = amounts.LibelleCategorie,
                TypeRegle = amounts.TypeRegle,
                ValeurRegle = amounts.ValeurRegle
            };
        }

        private static decimal ClampPercent(decimal valeur) =>
            Math.Clamp(valeur, 0m, 100m);

        private static decimal RoundMoney(decimal value) =>
            Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
