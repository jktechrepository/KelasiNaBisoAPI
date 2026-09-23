using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;

namespace KelasiNaBiso.Services.Repositories
{
    /// <summary>
    /// Création / liaison du compte Parent lié à un tuteur
    /// (partagé inscription + POST /api/Tuteur).
    /// </summary>
    public interface ITuteurCompteService
    {
        /// <summary>
        /// Assure un Utilisateur rôle Parent pour le tuteur.
        /// Si un compte existe (IdTuteur / email / téléphone), ajoute le rôle Parent et lie IdTuteur.
        /// Sinon crée le compte (MDP initial 123456, DoitChangerMotDePasse).
        /// </summary>
        /// <param name="sendInscriptionNotifications">
        /// Si true et <paramref name="eleve"/> fourni, envoie les notifications d'inscription
        /// (comportement historique InscriptionService).
        /// </param>
        Task<UtilisateurInfo?> CreateDefaultTuteurUserAsync(
            Tuteur tuteur,
            int idEcole,
            Eleve? eleve = null,
            bool sendInscriptionNotifications = true,
            CancellationToken cancellationToken = default);
    }
}
