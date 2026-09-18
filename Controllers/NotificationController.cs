using KelasiNaBiso.Models;
using KelasiNaBisoAPI.Services.Repositories;
using KelasiNaBiso.Attributes;
using KelasiNaBiso.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace KelasiNaBisoAPI.Controllers
{
    /// <summary>
    /// Contrôleur pour la gestion des notifications
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            INotificationRepository notificationRepository,
            ILogger<NotificationController> logger)
        {
            _notificationRepository = notificationRepository;
            _logger = logger;
        }

        /// <summary>
        /// Récupère toutes les notifications
        /// </summary>
        [HttpGet]
        [RequireGlobalAccess]
        public async Task<ActionResult<IEnumerable<Notification>>> GetAll()
        {
            try
            {
                var notifications = await _notificationRepository.GetAllAsync();
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des notifications");
                return StatusCode(500, new { message = "Erreur interne du serveur", error = ex.Message });
            }
        }

        /// <summary>
        /// Récupère une notification par son ID (destinataire = soi, sauf admin).
        /// </summary>
        [HttpGet("{id}")]
        [Permission("Notification.ReadOwn")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var notification = await _notificationRepository.GetByIdAsync(id);
                if (notification == null)
                {
                    return NotFound(new { message = "Notification non trouvée" });
                }

                var deny = this.ForbidIfWrongNotificationDestinataire(notification.IdDestinataire);
                if (deny != null)
                    return deny;

                return Ok(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la récupération de la notification {id}");
                return StatusCode(500, new { message = "Erreur interne du serveur", error = ex.Message });
            }
        }

        /// <summary>
        /// Récupère les notifications d'un destinataire
        /// </summary>
        [HttpGet("destinataire/{idDestinataire}")]
        [Permission("Notification.ReadOwn")]
        public async Task<IActionResult> GetByDestinataire(int idDestinataire)
        {
            var deny = this.ForbidIfWrongNotificationDestinataire(idDestinataire);
            if (deny != null)
                return deny;

            try
            {
                var notifications = await _notificationRepository.GetByDestinataireAsync(idDestinataire);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la récupération des notifications du destinataire {idDestinataire}");
                return StatusCode(500, new { message = "Erreur interne du serveur", error = ex.Message });
            }
        }

        /// <summary>
        /// Récupère les notifications d'un expéditeur
        /// </summary>
        [HttpGet("expediteur/{idExpediteur}")]
        [RequireGlobalAccess]
        public async Task<ActionResult<IEnumerable<Notification>>> GetByExpediteur(int idExpediteur)
        {
            try
            {
                var notifications = await _notificationRepository.GetByExpediteurAsync(idExpediteur);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la récupération des notifications de l'expéditeur {idExpediteur}");
                return StatusCode(500, new { message = "Erreur interne du serveur", error = ex.Message });
            }
        }

        /// <summary>
        /// Récupère les notifications d'une école
        /// </summary>
        [HttpGet("ecole/{idEcole}")]
        [RequireGlobalAccess]
        public async Task<ActionResult<IEnumerable<Notification>>> GetByEcole(int idEcole)
        {
            try
            {
                var notifications = await _notificationRepository.GetByEcoleAsync(idEcole);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la récupération des notifications de l'école {idEcole}");
                return StatusCode(500, new { message = "Erreur interne du serveur", error = ex.Message });
            }
        }

        /// <summary>
        /// Récupère les notifications d'une classe
        /// </summary>
        [HttpGet("classe/{idClasse}")]
        [RequireGlobalAccess]
        public async Task<ActionResult<IEnumerable<Notification>>> GetByClasse(int idClasse)
        {
            try
            {
                var notifications = await _notificationRepository.GetByClasseAsync(idClasse);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la récupération des notifications de la classe {idClasse}");
                return StatusCode(500, new { message = "Erreur interne du serveur", error = ex.Message });
            }
        }

        /// <summary>
        /// Récupère les notifications par type
        /// </summary>
        [HttpGet("type/{type}")]
        [RequireGlobalAccess]
        public async Task<ActionResult<IEnumerable<Notification>>> GetByType(string type)
        {
            try
            {
                var notifications = await _notificationRepository.GetByTypeAsync(type);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la récupération des notifications de type {type}");
                return StatusCode(500, new { message = "Erreur interne du serveur", error = ex.Message });
            }
        }

        /// <summary>
        /// Récupère les notifications non lues d'un destinataire
        /// </summary>
        [HttpGet("destinataire/{idDestinataire}/non-lues")]
        [Permission("Notification.ReadOwn")]
        public async Task<IActionResult> GetNonLues(int idDestinataire)
        {
            var deny = this.ForbidIfWrongNotificationDestinataire(idDestinataire);
            if (deny != null)
                return deny;

            try
            {
                var notifications = await _notificationRepository.GetNonLuesAsync(idDestinataire);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la récupération des notifications non lues du destinataire {idDestinataire}");
                return StatusCode(500, new { message = "Erreur interne du serveur", error = ex.Message });
            }
        }

        /// <summary>
        /// Crée une nouvelle notification
        /// </summary>
        [HttpPost]
        [RequireGlobalAccess]
        public async Task<ActionResult<Notification>> Create([FromBody] Notification notification)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdNotification = await _notificationRepository.CreateAsync(notification);
                return CreatedAtAction(nameof(GetById), new { id = createdNotification.IdNotification }, createdNotification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la notification");
                return StatusCode(500, new { message = "Erreur interne du serveur", error = ex.Message });
            }
        }

        /// <summary>
        /// Marque une notification comme lue
        /// </summary>
        [HttpPut("{id}/marquer-lue")]
        [Permission("Notification.UpdateOwn")]
        public async Task<IActionResult> MarquerCommeLue(int id)
        {
            try
            {
                var notification = await _notificationRepository.GetByIdAsync(id);
                if (notification == null)
                {
                    return NotFound(new { message = "Notification non trouvée" });
                }

                var deny = this.ForbidIfWrongNotificationDestinataire(notification.IdDestinataire);
                if (deny != null)
                    return deny;

                var success = await _notificationRepository.MarquerCommeLueAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Notification non trouvée" });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors du marquage de la notification {id} comme lue");
                return StatusCode(500, new { message = "Erreur interne du serveur", error = ex.Message });
            }
        }

        /// <summary>
        /// Marque toutes les notifications d'un destinataire comme lues
        /// </summary>
        [HttpPut("destinataire/{idDestinataire}/marquer-toutes-lues")]
        [Permission("Notification.UpdateOwn")]
        public async Task<IActionResult> MarquerToutesCommeLues(int idDestinataire)
        {
            var deny = this.ForbidIfWrongNotificationDestinataire(idDestinataire);
            if (deny != null)
                return deny;

            try
            {
                var success = await _notificationRepository.MarquerToutesCommeLuesAsync(idDestinataire);
                if (!success)
                {
                    return NotFound(new { message = "Aucune notification trouvée" });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors du marquage de toutes les notifications du destinataire {idDestinataire} comme lues");
                return StatusCode(500, new { message = "Erreur interne du serveur", error = ex.Message });
            }
        }

        /// <summary>
        /// Supprime une notification
        /// </summary>
        [HttpDelete("{id}")]
        [RequireGlobalAccess]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _notificationRepository.DeleteAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Notification non trouvée" });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la suppression de la notification {id}");
                return StatusCode(500, new { message = "Erreur interne du serveur", error = ex.Message });
            }
        }
    }
}
