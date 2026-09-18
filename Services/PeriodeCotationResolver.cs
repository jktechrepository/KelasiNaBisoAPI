using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    /// <summary>
    /// Résolution IdPeriode / alias + seed idempotent T1–T3.
    /// </summary>
    public class PeriodeCotationResolver
    {
        private readonly KelasiNaBisoDbContext _context;

        public PeriodeCotationResolver(KelasiNaBisoDbContext context)
        {
            _context = context;
        }

        public async Task EnsureSeedAsync(CancellationToken cancellationToken = default)
        {
            var seeds = new[]
            {
                new PeriodeCotation { Code = "T1", Libelle = "Trimestre 1", Ordre = 1, Statut = true },
                new PeriodeCotation { Code = "T2", Libelle = "Trimestre 2", Ordre = 2, Statut = true },
                new PeriodeCotation { Code = "T3", Libelle = "Trimestre 3", Ordre = 3, Statut = true }
            };

            foreach (var seed in seeds)
            {
                var exists = await _context.PeriodesCotation.AsNoTracking()
                    .AnyAsync(p => p.Code == seed.Code, cancellationToken);
                if (exists)
                    continue;

                seed.DateCreation = DateTime.UtcNow;
                _context.PeriodesCotation.Add(seed);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<PeriodeCotation?> GetByIdAsync(int idPeriode, CancellationToken cancellationToken = default)
        {
            return await _context.PeriodesCotation.AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdPeriode == idPeriode && p.Statut, cancellationToken);
        }

        /// <summary>
        /// Résout une période via idPeriode et/ou libellé/code/alias.
        /// </summary>
        public async Task<PeriodeCotation?> ResolveAsync(
            int? idPeriode,
            string? periodeOuCode,
            CancellationToken cancellationToken = default)
        {
            if (idPeriode.HasValue && idPeriode.Value > 0)
            {
                var byId = await GetByIdAsync(idPeriode.Value, cancellationToken);
                if (byId != null)
                    return byId;
            }

            if (string.IsNullOrWhiteSpace(periodeOuCode))
                return null;

            var code = PeriodeCotationAliases.ResolveCode(periodeOuCode);
            if (!string.IsNullOrEmpty(code))
            {
                var byCode = await _context.PeriodesCotation.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Code == code && p.Statut, cancellationToken);
                if (byCode != null)
                    return byCode;
            }

            var trimmed = periodeOuCode.Trim();
            return await _context.PeriodesCotation.AsNoTracking()
                .FirstOrDefaultAsync(
                    p => p.Statut && (p.Libelle == trimmed || p.Code == trimmed),
                    cancellationToken);
        }

        /// <summary>
        /// Applique IdPeriode + miroir Libelle sur une évaluation (création/màj).
        /// </summary>
        public async Task ApplyToEvaluationAsync(
            Evaluation evaluation,
            int? idPeriode,
            string? periodeOuCode,
            CancellationToken cancellationToken = default)
        {
            var resolved = await ResolveAsync(idPeriode, periodeOuCode, cancellationToken);
            if (resolved != null)
            {
                evaluation.IdPeriode = resolved.IdPeriode;
                evaluation.Periode = resolved.Libelle;
                return;
            }

            if (idPeriode.HasValue && idPeriode.Value > 0)
                throw new InvalidOperationException($"Période de cotation {idPeriode.Value} introuvable ou inactive.");

            evaluation.IdPeriode = null;
            evaluation.Periode = string.IsNullOrWhiteSpace(periodeOuCode) ? evaluation.Periode : periodeOuCode.Trim();
        }
    }
}
