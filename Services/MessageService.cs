using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBisoAPI.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class MessageService : IMessageRepository
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly IFirebaseNotificationService _notificationService;
        private readonly ISmsNotificationService _smsService;
        private readonly ILogger<MessageService> _logger;

        public MessageService(
            KelasiNaBisoDbContext context,
            IFirebaseNotificationService notificationService,
            ISmsNotificationService smsService,
            ILogger<MessageService> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _smsService = smsService;
            _logger = logger;
        }

        public async Task<IEnumerable<Message>> GetAllAsync()
        {
            return await _context.Messages
                .Include(m => m.Expediteur)
                .Include(m => m.Destinateur)
                .Include(m => m.GroupeMessage)
                .Where(m => m.Statut == true) // ✅ Filtrer uniquement les messages actifs
                .ToListAsync();
        }

        public async Task<Message> GetByIdAsync(int id)
        {
            return await _context.Messages
                .Include(m => m.Expediteur)
                .Include(m => m.Destinateur)
                .Include(m => m.GroupeMessage)
                .FirstOrDefaultAsync(m => m.IdMessage == id);
        }

        public async Task<IEnumerable<Message>> GetByExpediteurAsync(int idExpediteur)
        {
            return await _context.Messages
                .Include(m => m.Destinateur)
                .Include(m => m.GroupeMessage)
                .Where(m => m.IdExpediteur == idExpediteur && m.Statut == true) // ✅ Filtrer actifs
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetByDestinateurAsync(int idDestinateur)
        {
            return await _context.Messages
                .Include(m => m.Expediteur)
                .Include(m => m.GroupeMessage)
                .Where(m => m.IdDestinateur == idDestinateur && m.Statut == true) // ✅ Filtrer actifs
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetByGroupeAsync(int idGroupe)
        {
            return await _context.Messages
                .Include(m => m.Expediteur)
                .Include(m => m.Destinateur)
                .Where(m => m.IdGroupe == idGroupe && m.Statut == true) // ✅ Filtrer actifs
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetConversationAsync(int idExpediteur, int idDestinateur)
        {
            return await _context.Messages
                .Include(m => m.Expediteur)
                .Include(m => m.Destinateur)
                .Where(m => ((m.IdExpediteur == idExpediteur && m.IdDestinateur == idDestinateur) ||
                           (m.IdExpediteur == idDestinateur && m.IdDestinateur == idExpediteur)) && m.Statut == true) // ✅ Filtrer actifs
                .OrderBy(m => m.DateEnvoi)
                .ToListAsync();
        }

        public async Task<Message> CreateAsync(Message message)
        {
            message.DateEnvoi = DateTime.Now;
            
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // 🔔 NOTIFICATION HYBRIDE: Envoyer PUSH/SMS au destinataire si message individuel
            if (message.IdDestinateur.HasValue && !message.IdGroupe.HasValue)
            {
                await EnvoyerNotificationMessageAsync(message);
            }
            
            return message;
        }

        public async Task<Message> UpdateAsync(Message message)
        {
            var existingMessage = await _context.Messages.FindAsync(message.IdMessage);
            if (existingMessage == null)
                return null;

            _context.Entry(existingMessage).CurrentValues.SetValues(message);
            await _context.SaveChangesAsync();
            return existingMessage;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
                return false;

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Messages.AnyAsync(m => m.IdMessage == id);
        }

        // ✅ SOFT DELETE: Toggle le statut d'un message (actif <-> inactif)
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
                return false;

            message.Statut = message.Statut != true;
            await _context.SaveChangesAsync();
            return true;
        }

        // ═══════════════════════════════════════════════════════════════
        // 🔔 SECTION NOTIFICATIONS HYBRIDES (PUSH + SMS)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Envoie une notification PUSH/SMS au destinataire d'un message individuel
        /// </summary>
        private async Task EnvoyerNotificationMessageAsync(Message message)
        {
            try
            {
                if (!message.IdDestinateur.HasValue)
                    return;

                // 1️⃣ Récupérer l'expéditeur et le destinataire
                var expediteur = message.IdExpediteur.HasValue 
                    ? await _context.Utilisateurs.FindAsync(message.IdExpediteur.Value)
                    : null;

                var destinataire = await _context.Utilisateurs.FindAsync(message.IdDestinateur.Value);

                if (destinataire == null)
                {
                    _logger.LogWarning($"Destinataire {message.IdDestinateur} introuvable pour notification message");
                    return;
                }

                // 2️⃣ Préparer le message de notification
                string nomExpediteur = expediteur != null 
                    ? $"{expediteur.NomUtilisateur} {expediteur.PostNomUtilisateur}"
                    : "École";

                string titre = $"💬 Message de {nomExpediteur}";
                string corps = message.ContenuMessage;

                // Tronquer si trop long pour notification
                if (corps.Length > 200)
                {
                    corps = corps.Substring(0, 197) + "...";
                }

                // 3️⃣ Préparer les données additionnelles
                var donnees = new Dictionary<string, string>
                {
                    { "type", "MESSAGE_INDIVIDUEL" },
                    { "idMessage", message.IdMessage.ToString() },
                    { "idExpediteur", message.IdExpediteur?.ToString() ?? "0" },
                    { "idDestinataire", message.IdDestinateur.Value.ToString() },
                    { "dateEnvoi", message.DateEnvoi.ToString("yyyy-MM-dd HH:mm:ss") }
                };

                // 4️⃣ Envoyer la notification PUSH
                var notificationEnvoyee = await _notificationService.EnvoyerNotificationAUtilisateurAsync(
                    destinataire.IdUtilisateur,
                    titre,
                    corps,
                    donnees
                );

                if (notificationEnvoyee)
                {
                    _logger.LogInformation(
                        $"✅ Notification PUSH message envoyée à {destinataire.NomUtilisateur} " +
                        $"(User ID: {destinataire.IdUtilisateur}) de {nomExpediteur}"
                    );
                }
                else
                {
                    _logger.LogWarning(
                        $"⚠️ Échec notification PUSH pour {destinataire.NomUtilisateur} " +
                        $"→ Tentative SMS..."
                    );

                    // 🆘 FALLBACK SMS
                    await EnvoyerSmsFallbackMessageAsync(destinataire.IdUtilisateur, nomExpediteur, message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Erreur notification message ID {message.IdMessage}: {ex.Message}");

                // 🆘 Tentative SMS en cas d'erreur critique
                try
                {
                    if (message.IdDestinateur.HasValue)
                    {
                        var destinataire = await _context.Utilisateurs.FindAsync(message.IdDestinateur.Value);
                        var expediteur = message.IdExpediteur.HasValue
                            ? await _context.Utilisateurs.FindAsync(message.IdExpediteur.Value)
                            : null;

                        string nomExpediteur = expediteur != null
                            ? $"{expediteur.NomUtilisateur} {expediteur.PostNomUtilisateur}"
                            : "École";

                        if (destinataire != null)
                        {
                            await EnvoyerSmsFallbackMessageAsync(destinataire.IdUtilisateur, nomExpediteur, message);
                        }
                    }
                }
                catch (Exception smsEx)
                {
                    _logger.LogError(smsEx, $"❌ Échec ultime SMS fallback pour message {message.IdMessage}");
                }
            }
        }

        /// <summary>
        /// Envoie un SMS de notification si la notification PUSH échoue
        /// </summary>
        private async Task EnvoyerSmsFallbackMessageAsync(int idUtilisateur, string nomExpediteur, Message message)
        {
            try
            {
                // Message SMS optimisé (max 160 caractères pour 1 segment)
                string contenuCourt = message.ContenuMessage.Length > 100
                    ? message.ContenuMessage.Substring(0, 97) + "..."
                    : message.ContenuMessage;

                string messageSms = $"Message de {nomExpediteur}: {contenuCourt}";

                // Envoyer le SMS
                var smsLog = await _smsService.EnvoyerSmsAUtilisateurAsync(
                    idUtilisateur,
                    messageSms,
                    "MESSAGE_INDIVIDUEL"
                );

                if (smsLog != null && smsLog.Statut != "failed")
                {
                    _logger.LogInformation(
                        $"✅ SMS FALLBACK message envoyé (MessageSid: {smsLog.MessageSid}, Coût: {smsLog.CoutUsd} USD)"
                    );
                }
                else if (smsLog != null && smsLog.Statut == "failed")
                {
                    _logger.LogWarning($"⚠️ SMS FALLBACK échoué: {smsLog.MessageErreur}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Erreur SMS fallback message: {ex.Message}");
            }
        }
    }
}
