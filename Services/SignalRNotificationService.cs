using KelasiNaBisoAPI.Hubs;
using KelasiNaBiso.Models;
using KelasiNaBisoAPI.Services.Repositories;
using Microsoft.AspNetCore.SignalR;

namespace KelasiNaBisoAPI.Services
{
    /// <summary>
    /// Service pour envoyer des notifications en temps réel via SignalR
    /// </summary>
    public class SignalRNotificationService : ISignalRNotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<SignalRNotificationService> _logger;

        public SignalRNotificationService(
            IHubContext<NotificationHub> hubContext,
            ILogger<SignalRNotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// Envoyer une notification en temps réel à un utilisateur spécifique
        /// </summary>
        public async Task SendNotificationToUserAsync(int userId, Notification notification)
        {
            try
            {
                var notificationData = new
                {
                    id = notification.IdNotification,
                    title = notification.Titre,
                    message = notification.Contenu,
                    type = notification.TypeNotification,
                    isRead = notification.EstLue,
                    dateCreation = notification.DateCreation,
                    expediteur = notification.Expediteur != null ? new
                    {
                        id = notification.Expediteur.IdUtilisateur,
                        nom = notification.Expediteur.NomUtilisateur,
                        prenom = notification.Expediteur.PrenomUtilisateur,
                        photo = notification.Expediteur.PhotoUrl
                    } : null,
                    metadata = new
                    {
                        ecoleId = notification.IdEcole,
                        classeId = notification.IdClasse,
                        coursId = notification.IdCours,
                        eleveId = notification.IdEleve,
                        agentId = notification.IdAgent
                    }
                };

                // Envoyer au groupe de l'utilisateur
                await _hubContext.Clients
                    .Group($"user_{userId}")
                    .SendAsync("ReceiveNotification", notificationData);

                _logger.LogInformation($"Notification {notification.IdNotification} sent to user {userId} via SignalR");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending notification to user {userId}");
            }
        }

        /// <summary>
        /// Envoyer une notification en temps réel à plusieurs utilisateurs
        /// </summary>
        public async Task SendNotificationToUsersAsync(List<int> userIds, Notification notification)
        {
            try
            {
                var tasks = userIds.Select(userId => SendNotificationToUserAsync(userId, notification));
                await Task.WhenAll(tasks);

                _logger.LogInformation($"Notification {notification.IdNotification} sent to {userIds.Count} users via SignalR");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending notification to multiple users");
            }
        }

        /// <summary>
        /// Envoyer une notification à tous les utilisateurs d'une école
        /// </summary>
        public async Task SendNotificationToEcoleAsync(int ecoleId, Notification notification)
        {
            try
            {
                var notificationData = CreateNotificationData(notification);

                await _hubContext.Clients
                    .Group($"ecole_{ecoleId}")
                    .SendAsync("ReceiveNotification", notificationData);

                _logger.LogInformation($"Notification {notification.IdNotification} sent to ecole {ecoleId} via SignalR");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending notification to ecole {ecoleId}");
            }
        }

        /// <summary>
        /// Envoyer une notification à tous les utilisateurs d'une classe
        /// </summary>
        public async Task SendNotificationToClasseAsync(int classeId, Notification notification)
        {
            try
            {
                var notificationData = CreateNotificationData(notification);

                await _hubContext.Clients
                    .Group($"classe_{classeId}")
                    .SendAsync("ReceiveNotification", notificationData);

                _logger.LogInformation($"Notification {notification.IdNotification} sent to classe {classeId} via SignalR");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending notification to classe {classeId}");
            }
        }

        /// <summary>
        /// Envoyer une notification à tous les utilisateurs connectés
        /// </summary>
        public async Task SendNotificationToAllAsync(Notification notification)
        {
            try
            {
                var notificationData = CreateNotificationData(notification);

                await _hubContext.Clients.All
                    .SendAsync("ReceiveNotification", notificationData);

                _logger.LogInformation($"Notification {notification.IdNotification} sent to all users via SignalR");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to all users");
            }
        }

        /// <summary>
        /// Envoyer une notification personnalisée à un utilisateur
        /// </summary>
        public async Task SendCustomNotificationAsync(int userId, string title, string message, string type = "info")
        {
            try
            {
                var notificationData = new
                {
                    title = title,
                    message = message,
                    type = type,
                    timestamp = DateTime.UtcNow,
                    isCustom = true
                };

                await _hubContext.Clients
                    .Group($"user_{userId}")
                    .SendAsync("ReceiveNotification", notificationData);

                _logger.LogInformation($"Custom notification sent to user {userId}: {title}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending custom notification to user {userId}");
            }
        }

        /// <summary>
        /// Notifier un changement de statut (présence, paiement, etc.)
        /// </summary>
        public async Task NotifyStatusChangeAsync(int userId, string entityType, int entityId, string newStatus)
        {
            try
            {
                var statusData = new
                {
                    entityType = entityType,
                    entityId = entityId,
                    newStatus = newStatus,
                    timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients
                    .Group($"user_{userId}")
                    .SendAsync("StatusChanged", statusData);

                _logger.LogInformation($"Status change notification sent to user {userId}: {entityType} {entityId} -> {newStatus}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending status change notification to user {userId}");
            }
        }

        /// <summary>
        /// Notifier un nouveau message
        /// </summary>
        public async Task NotifyNewMessageAsync(int recipientId, int senderId, string senderName, string messageContent)
        {
            try
            {
                var messageData = new
                {
                    senderId = senderId,
                    senderName = senderName,
                    message = messageContent,
                    timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients
                    .Group($"user_{recipientId}")
                    .SendAsync("NewMessage", messageData);

                _logger.LogInformation($"New message notification sent to user {recipientId} from {senderName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending new message notification to user {recipientId}");
            }
        }

        /// <summary>
        /// Notifier une nouvelle note publiée
        /// </summary>
        public async Task NotifyNewGradeAsync(int studentId, string courseName, decimal? grade)
        {
            try
            {
                var gradeData = new
                {
                    courseName = courseName,
                    grade = grade,
                    timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients
                    .Group($"user_{studentId}")
                    .SendAsync("NewGrade", gradeData);

                _logger.LogInformation($"New grade notification sent to student {studentId} for {courseName}: {grade}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending new grade notification to student {studentId}");
            }
        }

        /// <summary>
        /// Méthode helper pour créer les données de notification
        /// </summary>
        private object CreateNotificationData(Notification notification)
        {
            return new
            {
                id = notification.IdNotification,
                title = notification.Titre,
                message = notification.Contenu,
                type = notification.TypeNotification,
                isRead = notification.EstLue,
                dateCreation = notification.DateCreation,
                expediteur = notification.Expediteur != null ? new
                {
                    id = notification.Expediteur.IdUtilisateur,
                    nom = notification.Expediteur.NomUtilisateur,
                    prenom = notification.Expediteur.PrenomUtilisateur,
                    photo = notification.Expediteur.PhotoUrl
                } : null,
                metadata = new
                {
                    ecoleId = notification.IdEcole,
                    classeId = notification.IdClasse,
                    coursId = notification.IdCours,
                    eleveId = notification.IdEleve,
                    agentId = notification.IdAgent
                }
            };
        }
    }
}
