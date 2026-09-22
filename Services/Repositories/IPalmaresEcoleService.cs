using KelasiNaBiso.Models.DTOs;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IPalmaresEcoleService
    {
        /// <summary>
        /// Palmarès école basé sur les bulletins figés.
        /// Période optionnelle : si absente, dernière période figée (Ordre max) pour l'école/année.
        /// </summary>
        Task<PalmaresEcoleDto> GetPalmaresEcoleAsync(
            int idEcole,
            int? idAnneeScolaire = null,
            int? idPeriode = null,
            string? periode = null,
            int? idClasse = null,
            int? idDirection = null,
            int limit = 20,
            CancellationToken cancellationToken = default);
    }
}
