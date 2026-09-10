using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class CurrencyConversionService : ICurrencyConversionService
    {
        private readonly KelasiNaBisoDbContext _context;

        public CurrencyConversionService(KelasiNaBisoDbContext context)
        {
            _context = context;
        }

        public async Task<ConversionResult> ConvertToPrincipalAsync(
            int idEcole,
            string codeDeviseSource,
            decimal montant,
            DateTime dateReference,
            CancellationToken ct = default)
        {
            var ecole = await _context.Ecoles
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdEcole == idEcole && e.Statut == true, ct);

            if (ecole == null)
            {
                return new ConversionResult(
                    idEcole,
                    NormalizeCode(codeDeviseSource),
                    "USD",
                    "USD",
                    0m,
                    montant,
                    0m,
                    dateReference,
                    false,
                    $"École {idEcole} introuvable.");
            }

            var codeDevisePrincipale = NormalizeCode(ecole.CodeDevisePrincipale) ?? "USD";

            return await ConvertAsync(
                idEcole,
                codeDeviseSource,
                codeDevisePrincipale,
                montant,
                dateReference,
                ct);
        }

        public async Task<ConversionResult> ConvertAsync(
            int idEcole,
            string codeDeviseSource,
            string codeDeviseCible,
            decimal montant,
            DateTime dateReference,
            CancellationToken ct = default)
        {
            var source = NormalizeCode(codeDeviseSource);
            var cible = NormalizeCode(codeDeviseCible);

            if (string.IsNullOrWhiteSpace(source))
            {
                return Failure(idEcole, source ?? string.Empty, cible ?? string.Empty, montant, dateReference,
                    "La devise source est obligatoire.");
            }

            if (string.IsNullOrWhiteSpace(cible))
            {
                return Failure(idEcole, source, string.Empty, montant, dateReference,
                    "La devise cible est obligatoire.");
            }

            if (montant < 0)
            {
                return Failure(idEcole, source, cible, montant, dateReference,
                    "Le montant doit être positif ou nul.");
            }

            if (source.Equals(cible, StringComparison.OrdinalIgnoreCase))
            {
                return new ConversionResult(
                    idEcole,
                    source,
                    cible,
                    cible,
                    1m,
                    montant,
                    Math.Round(montant, 2, MidpointRounding.AwayFromZero),
                    dateReference,
                    true);
            }

            var taux = await ResolveTauxAsync(idEcole, source, cible, dateReference, ct);
            if (taux == null)
            {
                return Failure(idEcole, source, cible, montant, dateReference,
                    $"Aucun taux de change trouvé pour {source}->{cible} à la date {dateReference:O}.");
            }

            var montantConverti = Math.Round(montant * taux.Value, 2, MidpointRounding.AwayFromZero);

            return new ConversionResult(
                idEcole,
                source,
                cible,
                cible,
                taux.Value,
                montant,
                montantConverti,
                dateReference,
                true);
        }

        private async Task<decimal?> ResolveTauxAsync(
            int idEcole,
            string source,
            string cible,
            DateTime dateReference,
            CancellationToken ct)
        {
            var directRate = await _context.TauxChanges
                .AsNoTracking()
                .Where(t => t.IdEcole == idEcole
                    && t.Statut
                    && t.CodeDeviseSource == source
                    && t.CodeDeviseCible == cible
                    && t.DateEffet <= dateReference)
                .OrderByDescending(t => t.DateEffet)
                .ThenByDescending(t => t.IdTauxChange)
                .Select(t => t.Taux)
                .FirstOrDefaultAsync(ct);

            if (directRate > 0)
            {
                return directRate;
            }

            var inverseRate = await _context.TauxChanges
                .AsNoTracking()
                .Where(t => t.IdEcole == idEcole
                    && t.Statut
                    && t.CodeDeviseSource == cible
                    && t.CodeDeviseCible == source
                    && t.DateEffet <= dateReference)
                .OrderByDescending(t => t.DateEffet)
                .ThenByDescending(t => t.IdTauxChange)
                .Select(t => t.Taux)
                .FirstOrDefaultAsync(ct);

            if (inverseRate <= 0)
            {
                return null;
            }

            return 1m / inverseRate;
        }

        private static string? NormalizeCode(string? code)
        {
            return string.IsNullOrWhiteSpace(code)
                ? null
                : code.Trim().ToUpperInvariant();
        }

        private static ConversionResult Failure(
            int idEcole,
            string source,
            string cible,
            decimal montant,
            DateTime dateReference,
            string message)
        {
            return new ConversionResult(
                idEcole,
                source,
                cible,
                cible,
                0m,
                montant,
                0m,
                dateReference,
                false,
                message);
        }
    }
}
