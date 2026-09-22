using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services.Tarif;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    /// <summary>
    /// Enrichit des lignes de paiement élève avec le solde restant par frais
    /// (paged, Eleve/paiements, Vue paiements), en utilisant le dû effectif
    /// (catalogue − exonération catégorie).
    /// </summary>
    public static class PaiementEleveResteEnricher
    {
        public static async Task<List<PaiementElevePagedItemDto>> MapAsync(
            KelasiNaBisoDbContext context,
            int idEleve,
            IReadOnlyList<Paiement> paiements,
            CancellationToken cancellationToken = default,
            IFraisDuCalculator? fraisDuCalculator = null)
        {
            if (paiements.Count == 0)
                return new List<PaiementElevePagedItemDto>();

            var fraisIds = paiements
                .Where(p => p.IdFrais.HasValue && p.IdFrais.Value > 0)
                .Select(p => p.IdFrais!.Value)
                .Distinct()
                .ToList();

            var fraisById = fraisIds.Count == 0
                ? new Dictionary<int, (string Libelle, double Montant, string Devise)>()
                : await context.Frais.AsNoTracking()
                    .Where(f => fraisIds.Contains(f.IdFrais))
                    .ToDictionaryAsync(
                        f => f.IdFrais,
                        f => (Libelle: f.LibelleFrais, Montant: f.Montant, Devise: f.Devise),
                        cancellationToken);

            var totalPayeByFrais = await LoadTotalPayeByFraisAsync(
                context, idEleve, fraisIds, cancellationToken);

            IReadOnlyDictionary<int, decimal>? dusByFrais = null;
            if (fraisDuCalculator != null && fraisIds.Count > 0)
            {
                dusByFrais = await fraisDuCalculator.GetMontantsDuEffectifsByFraisAsync(
                    idEleve, fraisIds, cancellationToken: cancellationToken);
            }

            return paiements.Select(p =>
            {
                if (!p.IdFrais.HasValue || !fraisById.TryGetValue(p.IdFrais.Value, out var frais))
                    return PaiementElevePagedItemDto.FromEntity(p);

                totalPayeByFrais.TryGetValue(p.IdFrais.Value, out var totalPaye);
                var dto = PaiementElevePagedItemDto.FromEntity(
                    p,
                    frais.Libelle,
                    frais.Montant,
                    frais.Devise,
                    totalPaye);

                if (dusByFrais != null && dusByFrais.TryGetValue(p.IdFrais.Value, out var duEffectif))
                    dto.ResteAPayer = Math.Max(0m, duEffectif - totalPaye);

                return dto;
            }).ToList();
        }

        /// <summary>
        /// Remplit TotalPayeSurFrais / ResteAPayer / CodeDeviseReste sur des lignes Vue
        /// (MontantFrais / DeviseFrais déjà fournis par la vue = catalogue).
        /// </summary>
        public static async Task EnrichVueAsync(
            KelasiNaBisoDbContext context,
            int idEleve,
            IList<VuePaiementsFraisParEcoleDTO> rows,
            CancellationToken cancellationToken = default,
            IFraisDuCalculator? fraisDuCalculator = null)
        {
            if (rows.Count == 0)
                return;

            var fraisIds = rows
                .Where(r => r.IdFrais.HasValue && r.IdFrais.Value > 0)
                .Select(r => r.IdFrais!.Value)
                .Distinct()
                .ToList();

            var totalPayeByFrais = await LoadTotalPayeByFraisAsync(
                context, idEleve, fraisIds, cancellationToken);

            IReadOnlyDictionary<int, decimal>? dusByFrais = null;
            if (fraisDuCalculator != null && fraisIds.Count > 0)
            {
                dusByFrais = await fraisDuCalculator.GetMontantsDuEffectifsByFraisAsync(
                    idEleve, fraisIds, cancellationToken: cancellationToken);
            }

            foreach (var row in rows)
            {
                if (!row.IdFrais.HasValue || !row.MontantFrais.HasValue)
                {
                    row.TotalPayeSurFrais = null;
                    row.ResteAPayer = null;
                    row.CodeDeviseReste = null;
                    continue;
                }

                totalPayeByFrais.TryGetValue(row.IdFrais.Value, out var totalPaye);
                row.TotalPayeSurFrais = totalPaye;
                row.CodeDeviseReste = row.DeviseFrais;

                if (dusByFrais != null && dusByFrais.TryGetValue(row.IdFrais.Value, out var duEffectif))
                    row.ResteAPayer = Math.Max(0m, duEffectif - totalPaye);
                else
                    row.ResteAPayer = Math.Max(0m, (decimal)row.MontantFrais.Value - totalPaye);
            }
        }

        private static async Task<Dictionary<int, decimal>> LoadTotalPayeByFraisAsync(
            KelasiNaBisoDbContext context,
            int idEleve,
            List<int> fraisIds,
            CancellationToken cancellationToken)
        {
            if (fraisIds.Count == 0)
                return new Dictionary<int, decimal>();

            var confirmed = await context.Paiements.AsNoTracking()
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

            return confirmed.ToDictionary(x => x.IdFrais, x => x.Total);
        }
    }
}
