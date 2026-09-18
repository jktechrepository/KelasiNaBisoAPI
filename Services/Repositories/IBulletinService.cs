using KelasiNaBiso.Models.DTOs.Bulletin;

namespace KelasiNaBiso.Services.Repositories
{
    /// <summary>
    /// Conception du domaine Bulletin (pas encore d'implémentation PDF).
    /// Calcul : moyennes pondérées Note × Evaluation.Coefficient, regroupées par Cours,
    /// filtrées par Eleve + AnneeScolaire + IdPeriode (ou alias Periode).
    /// </summary>
    public interface IBulletinService
    {
        /// <summary>
        /// Agrège les notes actives d'un élève pour une période scolaire.
        /// Fournir <paramref name="idPeriode"/> et/ou <paramref name="periode"/> (code/libellé/alias).
        /// </summary>
        Task<BulletinEleveDto?> GetBulletinEleveAsync(
            int idEleve,
            int idAnneeScolaire,
            string? periode = null,
            int? idPeriode = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Bulletins de tous les élèves d'une classe pour une période.
        /// </summary>
        Task<IReadOnlyList<BulletinEleveDto>> GetBulletinsClasseAsync(
            int idClasse,
            int idAnneeScolaire,
            string? periode = null,
            int? idPeriode = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Génération PDF (FastReport) — à brancher sur un template Bulletin.frx.
        /// </summary>
        Task<byte[]> GenerateBulletinPdfAsync(
            int idEleve,
            int idAnneeScolaire,
            string? periode = null,
            int? idPeriode = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Upsert décision / appréciation manuelle (titulaire ou direction).
        /// Bloqué si le bulletin est figé.
        /// </summary>
        Task<BulletinDecisionDto> UpsertDecisionAsync(
            int idEleve,
            int idAnneeScolaire,
            int idPeriode,
            string? decision,
            string? appreciationGenerale,
            int? idAuteur,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Calcule le bulletin live et le fige (snapshot). Conflit si déjà figé.
        /// </summary>
        Task<BulletinEleveDto> FigerEleveAsync(
            int idEleve,
            int idAnneeScolaire,
            string? periode = null,
            int? idPeriode = null,
            int? idAuteurValidation = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Fige tous les bulletins non encore figés de la classe.
        /// </summary>
        Task<FigerBulletinResultDto> FigerClasseAsync(
            int idClasse,
            int idAnneeScolaire,
            string? periode = null,
            int? idPeriode = null,
            int? idAuteurValidation = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Supprime le snapshot figé (retour au calcul live).
        /// </summary>
        Task DeverrouillerEleveAsync(
            int idEleve,
            int idAnneeScolaire,
            string? periode = null,
            int? idPeriode = null,
            CancellationToken cancellationToken = default);

        /// <summary>True si un snapshot existe pour élève/année/période.</summary>
        Task<bool> EstFigeAsync(
            int idEleve,
            int idAnneeScolaire,
            int idPeriode,
            CancellationToken cancellationToken = default);
    }
}
