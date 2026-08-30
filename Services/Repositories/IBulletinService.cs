using KelasiNaBiso.Models.DTOs.Bulletin;

namespace KelasiNaBiso.Services.Repositories
{
    /// <summary>
    /// Conception du domaine Bulletin (pas encore d'implémentation PDF).
    /// Calcul prévu : moyennes pondérées Note × Evaluation.Coefficient, regroupées par Cours,
    /// filtrées par Eleve + AnneeScolaire + Evaluation.Periode.
    /// </summary>
    public interface IBulletinService
    {
        /// <summary>
        /// Agrège les notes actives d'un élève pour une période scolaire.
        /// </summary>
        Task<BulletinEleveDto?> GetBulletinEleveAsync(
            int idEleve,
            int idAnneeScolaire,
            string periode,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Bulletins de tous les élèves d'une classe pour une période.
        /// </summary>
        Task<IReadOnlyList<BulletinEleveDto>> GetBulletinsClasseAsync(
            int idClasse,
            int idAnneeScolaire,
            string periode,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Génération PDF (FastReport) — à brancher sur un template Bulletin.frx.
        /// </summary>
        Task<byte[]> GenerateBulletinPdfAsync(
            int idEleve,
            int idAnneeScolaire,
            string periode,
            CancellationToken cancellationToken = default);
    }
}
