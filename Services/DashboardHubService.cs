using KelasiNaBisoAPI.Hubs;
using KelasiNaBiso.Models.DTOs.Reporting;
using KelasiNaBiso.Models.DTOs.Paiement;
using KelasiNaBiso.Models.DTOs.MokoAfrika;
using KelasiNaBisoAPI.Services.Repositories;
using Microsoft.AspNetCore.SignalR;

namespace KelasiNaBisoAPI.Services
{
    /// <summary>
    /// Service pour diffuser les mises à jour des dashboards en temps réel via SignalR
    /// </summary>
    public class DashboardHubService : IDashboardHubService
    {
        private readonly IHubContext<DashboardHub> _hubContext;
        private readonly ILogger<DashboardHubService> _logger;

        public DashboardHubService(
            IHubContext<DashboardHub> hubContext,
            ILogger<DashboardHubService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// Diffuser une mise à jour du dashboard global pour une école
        /// </summary>
        public async Task BroadcastDashboardGlobalAsync(int idEcole, DashboardGlobalDto dashboard)
        {
            try
            {
                await _hubContext.Clients
                    .Group($"ecole_{idEcole}")
                    .SendAsync("DashboardGlobalUpdated", dashboard);

                _logger.LogInformation($"📊 Dashboard global diffusé pour l'école {idEcole} via SignalR");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Erreur lors de la diffusion du dashboard global pour l'école {idEcole}");
            }
        }

        /// <summary>
        /// Diffuser une mise à jour du dashboard présence pour une école
        /// </summary>
        public async Task BroadcastDashboardPresenceAsync(int idEcole, DashboardPresenceDto dashboard)
        {
            try
            {
                await _hubContext.Clients
                    .Group($"ecole_{idEcole}")
                    .SendAsync("DashboardPresenceUpdated", dashboard);

                _logger.LogInformation($"📊 Dashboard présence diffusé pour l'école {idEcole} via SignalR");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Erreur lors de la diffusion du dashboard présence pour l'école {idEcole}");
            }
        }

        /// <summary>
        /// Diffuser une mise à jour du dashboard paiement pour une école
        /// </summary>
        public async Task BroadcastDashboardPaiementAsync(int idEcole, DashboardPaiementDto dashboard)
        {
            try
            {
                await _hubContext.Clients
                    .Group($"ecole_{idEcole}")
                    .SendAsync("DashboardPaiementUpdated", dashboard);

                _logger.LogInformation($"📊 Dashboard paiement diffusé pour l'école {idEcole} via SignalR");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Erreur lors de la diffusion du dashboard paiement pour l'école {idEcole}");
            }
        }

        /// <summary>
        /// Notifier qu'un événement a eu lieu et que le dashboard doit être rafraîchi
        /// </summary>
        public async Task NotifyDashboardUpdateAsync(int idEcole, string dashboardType, string eventType)
        {
            try
            {
                var notification = new
                {
                    ecoleId = idEcole,
                    dashboardType = dashboardType, // "global", "presence", "paiement"
                    eventType = eventType, // "presence_created", "paiement_created", etc.
                    timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients
                    .Group($"ecole_{idEcole}")
                    .SendAsync("DashboardUpdateNotification", notification);

                _logger.LogInformation($"📢 Notification de mise à jour dashboard ({dashboardType}) envoyée pour l'école {idEcole}, événement: {eventType}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Erreur lors de la notification de mise à jour dashboard pour l'école {idEcole}");
            }
        }

        /// <summary>
        /// Notifier une mise à jour de présence (pointage)
        /// </summary>
        public async Task NotifyPresenceUpdateAsync(int idEcole)
        {
            await NotifyDashboardUpdateAsync(idEcole, "presence", "presence_created");
        }

        /// <summary>
        /// Notifier une mise à jour de paiement
        /// </summary>
        public async Task NotifyPaiementUpdateAsync(int idEcole)
        {
            await NotifyDashboardUpdateAsync(idEcole, "paiement", "paiement_created");
        }

        public async Task NotifyPayInPendingAsync(int idEcole, PayInSignalRNotification notification)
        {
            await BroadcastPayInEventAsync(idEcole, "payin_pending", notification);
        }

        public async Task NotifyPayInConfirmedAsync(int idEcole, PayInSignalRNotification notification)
        {
            await BroadcastPayInEventAsync(idEcole, "payin_confirmed", notification);
        }

        private async Task BroadcastPayInEventAsync(int idEcole, string eventType, PayInSignalRNotification notification)
        {
            try
            {
                var payload = new
                {
                    ecoleId = idEcole,
                    dashboardType = "paiement",
                    eventType,
                    reference = notification.Reference,
                    idPaiement = notification.IdPaiement,
                    idEleve = notification.IdEleve,
                    montantNet = notification.MontantNet,
                    statutPaiement = notification.StatutPaiement,
                    statutGateway = notification.StatutGateway,
                    timestamp = DateTime.UtcNow
                };

                var group = _hubContext.Clients.Group($"ecole_{idEcole}");
                await group.SendAsync("DashboardUpdateNotification", payload);
                await group.SendAsync("PayInStatusUpdated", payload);

                _logger.LogInformation(
                    "📡 SignalR PayIn {EventType} diffusé pour école {IdEcole} ref {Reference}",
                    eventType,
                    idEcole,
                    notification.Reference);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur SignalR PayIn {EventType} pour école {IdEcole}", eventType, idEcole);
            }
        }
    }
}

