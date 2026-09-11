using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public static class DeviseMonetaireSeedHelper
    {
        public static async Task EnsureDefaultsAsync(
            KelasiNaBisoDbContext context,
            int idEcole,
            string? codeDevisePrincipale = null,
            CancellationToken ct = default)
        {
            await UpsertAsync(context, idEcole, "USD", "Dollar américain", "$", ct);
            await UpsertAsync(context, idEcole, "CDF", "Franc congolais", "FC", ct);

            var principale = NormalizeCode(codeDevisePrincipale);
            if (!string.IsNullOrEmpty(principale) && principale is not ("USD" or "CDF"))
            {
                await UpsertAsync(context, idEcole, principale, principale, null, ct);
            }

            await context.SaveChangesAsync(ct);
        }

        private static async Task UpsertAsync(
            KelasiNaBisoDbContext context,
            int idEcole,
            string code,
            string libelle,
            string? symbole,
            CancellationToken ct)
        {
            var existing = await context.DevisesMonetaires
                .FirstOrDefaultAsync(d => d.IdEcole == idEcole && d.CodeDevise == code, ct);

            if (existing != null)
            {
                if (!existing.Statut)
                {
                    existing.Statut = true;
                }

                return;
            }

            context.DevisesMonetaires.Add(new DeviseMonetaire
            {
                IdEcole = idEcole,
                CodeDevise = code,
                Libelle = libelle,
                Symbole = symbole,
                Statut = true,
                DateCreation = DateTime.UtcNow
            });
        }

        public static string? NormalizeCode(string? code) =>
            string.IsNullOrWhiteSpace(code) ? null : code.Trim().ToUpperInvariant();
    }
}
