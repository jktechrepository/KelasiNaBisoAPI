using KelasiNaBiso.Models;

namespace KelasiNaBiso.Services.Repositories
{
    /// <summary>
    /// Autorisation pédagogique unifiée : TitulaireClasse ∪ AffectationCours, scopée année scolaire.
    /// À utiliser par DevoirADomicile, Hub SignalR, et futurs contrôles Note/Evaluation.
    /// </summary>
    public interface IPedagogieAuthorizationService
    {
        /// <summary>
        /// True si l'agent est titulaire actif de la classe pour l'année (courante si null).
        /// </summary>
        Task<bool> AgentEstTitulaireClasseAsync(int idAgent, int idClasse, int? idAnneeScolaire = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// True si l'agent peut enseigner / publier pour la classe (titulaire ou affectation active).
        /// Si <paramref name="idAnneeScolaire"/> est null, utilise l'année courante de l'école de la classe.
        /// </summary>
        Task<bool> AgentEnseigneClasseAsync(int idAgent, int idClasse, int? idAnneeScolaire = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// True si l'agent a une affectation active sur le cours pour l'année (courante si null).
        /// </summary>
        Task<bool> AgentEnseigneCoursAsync(int idAgent, int idCours, int? idAnneeScolaire = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Ids de classes où l'agent enseigne (titulaire ∪ affectation) pour l'année résolue.
        /// </summary>
        Task<IReadOnlyList<int>> GetClassesEnseignantAsync(int idAgent, int? idAnneeScolaire = null, int? idEcole = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Résout l'IdAnneeScolaire : paramètre explicite, sinon année courante de l'école.
        /// </summary>
        Task<int?> ResolveIdAnneeScolairePourClasseAsync(int idClasse, int? idAnneeScolaire = null, CancellationToken cancellationToken = default);
    }
}
