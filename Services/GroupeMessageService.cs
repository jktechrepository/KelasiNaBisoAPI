using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBisoAPI.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class GroupeMessageService : IGroupeMessageRepository
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly IFirebaseNotificationService _notificationService;
        private readonly ISmsNotificationService _smsService;
        private readonly IInscriptionActiveResolver _inscriptionResolver;
        private readonly ILogger<GroupeMessageService> _logger;

        public GroupeMessageService(
            KelasiNaBisoDbContext context,
            IFirebaseNotificationService notificationService,
            ISmsNotificationService smsService,
            IInscriptionActiveResolver inscriptionResolver,
            ILogger<GroupeMessageService> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _smsService = smsService;
            _inscriptionResolver = inscriptionResolver;
            _logger = logger;
        }

        public async Task<IEnumerable<GroupeMessage>> GetAllAsync()
        {
            return await _context.GroupeMessages
                .Include(g => g.Ecole)
                .Include(g => g.Utilisateur)
                .Include(g => g.Messages)
                .Where(g => g.Statut == true) // ✅ Filtrer uniquement les groupes actifs
                .OrderByDescending(g => g.DateCreation)
                .ToListAsync();
        }

        public async Task<GroupeMessage> GetByIdAsync(int id)
        {
            return await _context.GroupeMessages
                .Include(g => g.Ecole)
                .Include(g => g.Utilisateur)
                .Include(g => g.Messages)
                .FirstOrDefaultAsync(g => g.IdGroupe == id);
        }

        public async Task<GroupeMessage> GetByNomAsync(string nom)
        {
            return await _context.GroupeMessages
                .Include(g => g.Ecole)
                .Include(g => g.Utilisateur)
                .Include(g => g.Messages)
                .FirstOrDefaultAsync(g => g.NomGroupe == nom);
        }

        public async Task<IEnumerable<GroupeMessage>> GetByEcoleAsync(int idEcole)
        {
            return await _context.GroupeMessages
                .Include(g => g.Utilisateur)
                .Include(g => g.Messages)
                .Where(g => g.IdEcole == idEcole && g.Statut == true) // ✅ Filtrer actifs
                .OrderByDescending(g => g.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<GroupeMessage>> GetByUtilisateurAsync(int idUtilisateur)
        {
            return await _context.GroupeMessages
                .Include(g => g.Ecole)
                .Include(g => g.Messages)
                .Where(g => g.CreePar == idUtilisateur && g.Statut == true) // ✅ Filtrer actifs
                .OrderByDescending(g => g.DateCreation)
                .ToListAsync();
        }

        //public async Task<IEnumerable<GroupeMessage>> GetByTypeAsync(string type)
        //{
        //    return await _context.GroupeMessages
        //        .Include(g => g.Ecole)
        //        .Include(g => g.Utilisateur)
        //        .Include(g => g.Messages)
        //        .Where(g => g.Type == type)
        //        .OrderByDescending(g => g.DateCreation)
        //        .ToListAsync();
        //}

        public async Task<IEnumerable<GroupeMessage>> GetByDateCreationAsync(DateTime date)
        {
            return await _context.GroupeMessages
                .Include(g => g.Ecole)
                .Include(g => g.Utilisateur)
                .Include(g => g.Messages)
                .Where(g => g.DateCreation.Date == date.Date && g.Statut == true) // ✅ Filtrer actifs
                .OrderByDescending(g => g.DateCreation)
                .ToListAsync();
        }

        public async Task<GroupeMessage> CreateAsync(GroupeMessage groupeMessage)
        {
            groupeMessage.DateCreation = DateTime.Now;
            
            _context.GroupeMessages.Add(groupeMessage);
            await _context.SaveChangesAsync();
            return groupeMessage;
        }

        public async Task<GroupeMessage> UpdateAsync(GroupeMessage groupeMessage)
        {
            var existingGroupeMessage = await _context.GroupeMessages.FindAsync(groupeMessage.IdGroupe);
            if (existingGroupeMessage == null)
                return null;

            _context.Entry(existingGroupeMessage).CurrentValues.SetValues(groupeMessage);
            await _context.SaveChangesAsync();
            return existingGroupeMessage;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var groupeMessage = await _context.GroupeMessages.FindAsync(id);
            if (groupeMessage == null)
                return false;

            _context.GroupeMessages.Remove(groupeMessage);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.GroupeMessages.AnyAsync(g => g.IdGroupe == id);
        }

        public async Task<bool> ExistsByNomAsync(string nom)
        {
            return await _context.GroupeMessages.AnyAsync(g => g.NomGroupe == nom);
        }

        public async Task<IEnumerable<Message>> GetMessagesAsync(int idGroupe)
        {
            return await _context.Messages
                .Include(m => m.Expediteur)
                .Include(m => m.GroupeMessage)
                .Where(m => m.IdGroupe == idGroupe)
                .OrderByDescending(m => m.DateEnvoi)
                .ToListAsync();
        }

        // ✅ SOFT DELETE: Toggle le statut d'un groupe de messages (actif <-> inactif)
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var groupe = await _context.GroupeMessages.FindAsync(id);
            if (groupe == null)
                return false;

            groupe.Statut = groupe.Statut != true;
            await _context.SaveChangesAsync();
            return true;
        }

        // ═══════════════════════════════════════════════════════════════
        // 🔔 NOUVELLES MÉTHODES: NOTIFICATIONS HYBRIDES POUR GROUPES
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Envoie un message à un groupe avec notifications PUSH/SMS à tous les membres
        /// </summary>
        public async Task<Message> EnvoyerMessageGroupeAvecNotificationAsync(
            int idGroupe,
            int idExpediteur,
            string contenuMessage,
            string? fichierUrl = null,
            string priorite = "NORMALE")
        {
            try
            {
                // 1️⃣ Vérifier que le groupe existe
                var groupe = await _context.GroupeMessages
                    .Include(g => g.Ecole)
                    .FirstOrDefaultAsync(g => g.IdGroupe == idGroupe);

                if (groupe == null)
                {
                    _logger.LogWarning($"Groupe {idGroupe} introuvable");
                    return null;
                }

                // 2️⃣ Créer le message dans le groupe
                var message = new Message
                {
                    IdExpediteur = idExpediteur,
                    IdGroupe = idGroupe,
                    ContenuMessage = contenuMessage,
                    FichierUrl = fichierUrl ?? string.Empty,
                    DateEnvoi = DateTime.Now,
                    Statut = true
                };

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                // 3️⃣ Envoyer les notifications à tous les membres du groupe
                await EnvoyerNotificationAuGroupeAsync(message, groupe, priorite);

                return message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Erreur envoi message groupe {idGroupe}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Envoie des notifications PUSH/SMS à tous les membres d'un groupe
        /// </summary>
        private async Task EnvoyerNotificationAuGroupeAsync(Message message, GroupeMessage groupe, string priorite)
        {
            try
            {
                // Récupérer l'expéditeur
                var expediteur = message.IdExpediteur.HasValue
                    ? await _context.Utilisateurs.FindAsync(message.IdExpediteur.Value)
                    : null;

                string nomExpediteur = expediteur != null
                    ? $"{expediteur.NomUtilisateur} {expediteur.PostNomUtilisateur}"
                    : "École";

                // Récupérer tous les tuteurs de l'école (membres potentiels du groupe)
                var idsUtilisateurs = await _inscriptionResolver
                    .FilterUtilisateursParentsInEcole(_context.Utilisateurs, groupe.IdEcole)
                    .Select(u => u.IdUtilisateur)
                    .Distinct()
                    .ToListAsync();

                if (idsUtilisateurs.Count == 0)
                {
                    _logger.LogWarning($"Aucun membre trouvé pour le groupe {groupe.NomGroupe}");
                    return;
                }

                // Préparer le message
                string titre = $"💬 {groupe.NomGroupe} - {nomExpediteur}";
                string corps = message.ContenuMessage;

                // Tronquer si trop long
                if (corps.Length > 200)
                {
                    corps = corps.Substring(0, 197) + "...";
                }

                var donnees = new Dictionary<string, string>
                {
                    { "type", "MESSAGE_GROUPE" },
                    { "idMessage", message.IdMessage.ToString() },
                    { "idGroupe", groupe.IdGroupe.ToString() },
                    { "nomGroupe", groupe.NomGroupe },
                    { "idExpediteur", message.IdExpediteur?.ToString() ?? "0" },
                    { "dateEnvoi", message.DateEnvoi.ToString("yyyy-MM-dd HH:mm:ss") }
                };

                // Envoyer aux membres du groupe selon la priorité
                int countPush = 0, countSms = 0;

                foreach (var idUtilisateur in idsUtilisateurs)
                {
                    // Ne pas notifier l'expéditeur lui-même
                    if (message.IdExpediteur.HasValue && idUtilisateur == message.IdExpediteur.Value)
                        continue;

                    if (priorite == "HAUTE")
                    {
                        // PUSH + SMS systématique
                        var pushSuccess = await _notificationService.EnvoyerNotificationAUtilisateurAsync(
                            idUtilisateur, titre, corps, donnees);
                        if (pushSuccess) countPush++;

                        string messageSms = $"{groupe.NomGroupe} - {nomExpediteur}: {(message.ContenuMessage.Length > 100 ? message.ContenuMessage.Substring(0, 97) + "..." : message.ContenuMessage)}";
                        var smsLog = await _smsService.EnvoyerSmsAUtilisateurAsync(
                            idUtilisateur, messageSms, "MESSAGE_GROUPE");
                        if (smsLog != null && smsLog.Statut != "failed") countSms++;
                    }
                    else if (priorite == "NORMALE")
                    {
                        // PUSH + SMS fallback
                        var pushSuccess = await _notificationService.EnvoyerNotificationAUtilisateurAsync(
                            idUtilisateur, titre, corps, donnees);

                        if (pushSuccess)
                        {
                            countPush++;
                        }
                        else
                        {
                            // Fallback SMS
                            string messageSms = $"{groupe.NomGroupe} - {nomExpediteur}: {(message.ContenuMessage.Length > 100 ? message.ContenuMessage.Substring(0, 97) + "..." : message.ContenuMessage)}";
                            var smsLog = await _smsService.EnvoyerSmsAUtilisateurAsync(
                                idUtilisateur, messageSms, "MESSAGE_GROUPE");
                            if (smsLog != null && smsLog.Statut != "failed") countSms++;
                        }
                    }
                    else // BASSE
                    {
                        // PUSH uniquement (économie SMS)
                        var pushSuccess = await _notificationService.EnvoyerNotificationAUtilisateurAsync(
                            idUtilisateur, titre, corps, donnees);
                        if (pushSuccess) countPush++;
                    }
                }

                _logger.LogInformation(
                    $"✅ Notifications groupe '{groupe.NomGroupe}': " +
                    $"{countPush} PUSH, {countSms} SMS envoyés (Total membres: {idsUtilisateurs.Count - 1})"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Erreur notifications groupe {groupe.NomGroupe}: {ex.Message}");
            }
        }
    }
}
