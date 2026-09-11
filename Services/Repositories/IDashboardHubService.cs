using KelasiNaBiso.Models.DTOs.Reporting;
using KelasiNaBiso.Models.DTOs.Paiement;
using KelasiNaBiso.Models.DTOs.MokoAfrika;

namespace KelasiNaBisoAPI.Services.Repositories
{
    /// <summary>
    /// Interface pour le service de diffusion des dashboards en temps réel via SignalR
    /// </summary>
    public interface IDashboardHubService
    {
        /// <summary>
        /// Diffuser une mise à jour du dashboard global pour une école
        /// </summary>
        Task BroadcastDashboardGlobalAsync(int idEcole, DashboardGlobalDto dashboard);

        /// <summary>
        /// Diffuser une mise à jour du dashboard présence pour une école
        /// </summary>
        Task BroadcastDashboardPresenceAsync(int idEcole, DashboardPresenceDto dashboard);

        /// <summary>
        /// Diffuser une mise à jour du dashboard paiement pour une école
        /// </summary>
        Task BroadcastDashboardPaiementAsync(int idEcole, DashboardPaiementDto dashboard);

        /// <summary>
        /// Notifier qu'un événement a eu lieu et que le dashboard doit être rafraîchi
        /// (pointage, paiement, etc.) - Le client devra faire un appel API pour récupérer les nouvelles données
        /// </summary>
        Task NotifyDashboardUpdateAsync(int idEcole, string dashboardType, string eventType);

        /// <summary>
        /// Notifier une mise à jour de présence (pointage)
        /// </summary>
        Task NotifyPresenceUpdateAsync(int idEcole);

        /// <summary>
        /// Notifier une mise à jour de paiement
        /// </summary>
        Task NotifyPaiementUpdateAsync(int idEcole);

        /// <summary>
        /// PayIn Moko initié — en attente USSD (temps réel guichet).
        /// </summary>
        Task NotifyPayInPendingAsync(int idEcole, PayInSignalRNotification notification);

        /// <summary>
        /// PayIn Moko confirmé (callback ou polling).
        /// </summary>
        Task NotifyPayInConfirmedAsync(int idEcole, PayInSignalRNotification notification);
    }
}

