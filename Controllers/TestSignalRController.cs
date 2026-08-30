using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using KelasiNaBisoAPI.Hubs;

namespace KelasiNaBiso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔒 Authentification requise
    public class TestSignalRController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<TestSignalRController> _logger;

        public TestSignalRController(
            IHubContext<NotificationHub> hubContext,
            ILogger<TestSignalRController> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// Envoyer une notification de test à tous les clients connectés
        /// </summary>
        /// <returns></returns>
        [HttpPost("broadcast")]
        public async Task<ActionResult> SendBroadcastNotification([FromBody] TestNotificationRequest request)
        {
            try
            {
                _logger.LogInformation($"📢 Envoi d'une notification broadcast: {request.Message}");

                await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
                {
                    type = request.Type ?? "TEST",
                    titre = request.Titre ?? "Notification Test",
                    message = request.Message ?? "Ceci est un message de test",
                    timestamp = DateTime.UtcNow,
                    data = request.Data
                });

                return Ok(new
                {
                    success = true,
                    message = "Notification broadcast envoyée avec succès"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Erreur lors de l'envoi de la notification: {ex.Message}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Envoyer une notification à un groupe spécifique
        /// </summary>
        [HttpPost("group/{groupName}")]
        public async Task<ActionResult> SendGroupNotification(
            string groupName,
            [FromBody] TestNotificationRequest request)
        {
            try
            {
                _logger.LogInformation($"📢 Envoi d'une notification au groupe '{groupName}': {request.Message}");

                await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", new
                {
                    type = request.Type ?? "TEST",
                    titre = request.Titre ?? "Notification Test",
                    message = request.Message ?? "Ceci est un message de test",
                    groupe = groupName,
                    timestamp = DateTime.UtcNow,
                    data = request.Data
                });

                return Ok(new
                {
                    success = true,
                    message = $"Notification envoyée au groupe '{groupName}' avec succès"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Erreur lors de l'envoi de la notification au groupe: {ex.Message}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Envoyer une notification à un utilisateur spécifique (via son groupe user_X)
        /// </summary>
        [HttpPost("user/{userId}")]
        public async Task<ActionResult> SendUserNotification(
            int userId,
            [FromBody] TestNotificationRequest request)
        {
            try
            {
                var userGroup = $"user_{userId}";
                _logger.LogInformation($"📢 Envoi d'une notification à l'utilisateur {userId}: {request.Message}");

                await _hubContext.Clients.Group(userGroup).SendAsync("ReceiveNotification", new
                {
                    type = request.Type ?? "TEST",
                    titre = request.Titre ?? "Notification Test",
                    message = request.Message ?? "Ceci est un message de test",
                    userId = userId,
                    timestamp = DateTime.UtcNow,
                    data = request.Data
                });

                return Ok(new
                {
                    success = true,
                    message = $"Notification envoyée à l'utilisateur {userId} avec succès"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Erreur lors de l'envoi de la notification à l'utilisateur: {ex.Message}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Obtenir le statut du hub SignalR
        /// </summary>
        [HttpGet("status")]
        public ActionResult GetHubStatus()
        {
            return Ok(new
            {
                success = true,
                hubEndpoint = "/hubs/notifications",
                status = "active",
                timestamp = DateTime.UtcNow,
                message = "Le hub SignalR est opérationnel"
            });
        }
    }

    public class TestNotificationRequest
    {
        public string? Type { get; set; }
        public string? Titre { get; set; }
        public string? Message { get; set; }
        public Dictionary<string, object>? Data { get; set; }
    }
}

