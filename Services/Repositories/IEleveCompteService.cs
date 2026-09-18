using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IEleveCompteService
    {
        /// <summary>
        /// Crée (ou lie) un compte Utilisateur rôle Eleve : login = matricule, MDP = 123456.
        /// Idempotent si un compte est déjà lié à cet IdEleve.
        /// </summary>
        /// <param name="sendSmsToTuteur">Si true et Outcome=Created, SMS identifiants au téléphone du tuteur (AcceptNotification).</param>
        Task<EleveCompteCreateResult> CreateDefaultEleveUserAsync(Eleve eleve, int idEcole, bool sendSmsToTuteur = true);

        /// <summary>
        /// Backfill des comptes Élève manquants pour une école (inscriptions actives / confirmées).
        /// </summary>
        Task<EleveCompteBackfillResult> BackfillByEcoleAsync(int idEcole, bool dryRun = false, bool sendSms = false);
    }
}
