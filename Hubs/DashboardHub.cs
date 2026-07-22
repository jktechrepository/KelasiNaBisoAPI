using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace KelasiNaBisoAPI.Hubs
{
    /// <summary>
    /// Hub SignalR pour les dashboards en temps réel
    /// Permet la mise à jour automatique des dashboards lors d'événements (pointage, paiement, etc.)
    /// </summary>
    [Authorize]
    public class DashboardHub : Hub
    {
        private readonly ILogger<DashboardHub> _logger;

        public DashboardHub(ILogger<DashboardHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Appelé quand un client se connecte
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
            var idEcole = Context.User?.FindFirst("IdEcole")?.Value;
            
            if (!string.IsNullOrEmpty(userId))
            {
                // Ajouter l'utilisateur à son groupe personnel
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                
                // Ajouter l'utilisateur au groupe de son école si disponible
                if (!string.IsNullOrEmpty(idEcole) && int.TryParse(idEcole, out int ecoleId))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"ecole_{ecoleId}");
                    _logger.LogInformation($"User {userName} (ID: {userId}) connected to DashboardHub and joined ecole_{ecoleId}. ConnectionId: {Context.ConnectionId}");
                }
                else
                {
                    _logger.LogInformation($"User {userName} (ID: {userId}) connected to DashboardHub. ConnectionId: {Context.ConnectionId}");
                }
            }
            
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Appelé quand un client se déconnecte
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
            
            if (!string.IsNullOrEmpty(userId))
            {
                _logger.LogInformation($"User {userName} (ID: {userId}) disconnected from DashboardHub. ConnectionId: {Context.ConnectionId}");
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Permet à un utilisateur de rejoindre le groupe d'une école spécifique
        /// Utile pour les Super-Admins qui veulent surveiller plusieurs écoles
        /// </summary>
        public async Task JoinEcoleGroup(int idEcole)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"ecole_{idEcole}");
            _logger.LogInformation($"User {Context.User?.Identity?.Name} joined ecole group: ecole_{idEcole}");
            
            // Confirmer l'ajout au groupe
            await Clients.Caller.SendAsync("JoinedEcoleGroup", idEcole);
        }

        /// <summary>
        /// Permet à un utilisateur de quitter le groupe d'une école
        /// </summary>
        public async Task LeaveEcoleGroup(int idEcole)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ecole_{idEcole}");
            _logger.LogInformation($"User {Context.User?.Identity?.Name} left ecole group: ecole_{idEcole}");
            
            // Confirmer la sortie du groupe
            await Clients.Caller.SendAsync("LeftEcoleGroup", idEcole);
        }

        /// <summary>
        /// Obtenir le statut de connexion
        /// </summary>
        public async Task GetConnectionStatus()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
            var idEcole = Context.User?.FindFirst("IdEcole")?.Value;
            
            await Clients.Caller.SendAsync("ConnectionStatus", new
            {
                IsConnected = true,
                UserId = userId,
                UserName = userName,
                IdEcole = idEcole,
                ConnectionId = Context.ConnectionId,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}

