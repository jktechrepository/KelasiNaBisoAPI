using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using KelasiNaBiso.Data;
using KelasiNaBiso.Helpers;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBisoAPI.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class CommunicationDispatchWorker : ICommunicationDispatchWorker
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly IFirebaseNotificationService _firebaseNotificationService;
        private readonly IEmailService _emailService;
        private readonly ISmsNotificationService _smsService;
        private readonly ILogger<CommunicationDispatchWorker> _logger;

        private static readonly Regex MarkdownStripRegex = new(@"[*_`>#-]", RegexOptions.Compiled);

        public CommunicationDispatchWorker(
            KelasiNaBisoDbContext context,
            IFirebaseNotificationService firebaseNotificationService,
            IEmailService emailService,
            ISmsNotificationService smsService,
            ILogger<CommunicationDispatchWorker> logger)
        {
            _context = context;
            _firebaseNotificationService = firebaseNotificationService;
            _emailService = emailService;
            _smsService = smsService;
            _logger = logger;
        }

        public async Task ExecuteAsync(int idCampaign, int initiatedByUserId, CancellationToken cancellationToken = default)
        {
            try
            {
                var campaign = await _context.CommunicationCampaigns
                    .Include(c => c.Destinataires)
                    .Include(c => c.Ecole)
                    .FirstOrDefaultAsync(c => c.IdCampaign == idCampaign, cancellationToken);

                if (campaign == null)
                {
                    _logger.LogWarning("Campagne {IdCampaign} introuvable pour la diffusion.", idCampaign);
                    return;
                }

                var channels = CommunicationChannelHelper.DeserializeChannels(campaign.ChannelsJson);
                var orderedChannels = CommunicationChannelHelper.GetOrderedChannels(channels);

                var recipients = await _context.CampaignRecipients
                    .Where(r => r.IdCampaign == idCampaign && r.Status == "Planifie")
                    .Include(r => r.Utilisateur)
                    .ThenInclude(u => u.Tuteur)
                    .Include(r => r.Eleve)
                    .ThenInclude(e => e.Inscriptions)
                        .ThenInclude(i => i.Classe)
                    .ToListAsync(cancellationToken);

                if (recipients.Count == 0)
                {
                    _logger.LogInformation("Aucun destinataire à envoyer pour la campagne {IdCampaign}.", idCampaign);
                    campaign.Statut = CampaignStatuses.Envoye;
                    await _context.SaveChangesAsync(cancellationToken);
                    return;
                }

                var plainTextBody = RenderPlainText(campaign.ContenuMarkdown);
                var htmlBody = RenderHtmlBody(campaign.ContenuMarkdown);

                int successCount = 0;
                int failureCount = 0;

                foreach (var recipient in recipients)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var result = await SendToRecipientAsync(campaign, recipient, orderedChannels, plainTextBody, htmlBody, cancellationToken);
                        if (result)
                        {
                            successCount++;
                        }
                        else
                        {
                            failureCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        recipient.Status = "Echec";
                        recipient.ErrorMessage = ex.Message;
                        recipient.DateEnvoi = DateTime.UtcNow;
                        _logger.LogError(ex, "Erreur lors de l'envoi au destinataire {IdRecipient} pour la campagne {IdCampaign}", recipient.IdRecipient, idCampaign);
                    }
                }

                campaign.Statut = failureCount == 0
                    ? CampaignStatuses.Envoye
                    : successCount > 0
                        ? CampaignStatuses.Partiel
                        : CampaignStatuses.Annule;

                await AddHistoryAsync(idCampaign, initiatedByUserId, successCount, failureCount, cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Campagne {IdCampaign} envoyée : {Success} succès, {Failure} échecs.", idCampaign, successCount, failureCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur fatale lors de l'exécution de la campagne {IdCampaign}", idCampaign);
                throw;
            }
        }

        private async Task<bool> SendToRecipientAsync(
            CommunicationCampaign campaign,
            CampaignRecipient recipient,
            IReadOnlyList<string> orderedChannels,
            string plainTextBody,
            string htmlBody,
            CancellationToken cancellationToken)
        {
            var utilisateur = recipient.Utilisateur ?? await _context.Utilisateurs.FindAsync(new object?[] { recipient.IdUtilisateur }, cancellationToken: cancellationToken);
            if (utilisateur == null)
            {
                recipient.Status = "Echec";
                recipient.ErrorMessage = "Utilisateur introuvable";
                recipient.DateEnvoi = DateTime.UtcNow;
                return false;
            }

            var finalChannel = recipient.PreferredChannel;
            var success = false;
            var errorMessages = new List<string>();
            Notification? notification = null;

            if (orderedChannels.Contains("InApp"))
            {
                notification = BuildNotification(campaign, recipient, utilisateur, plainTextBody);
                await _context.Notifications.AddAsync(notification, cancellationToken);
                recipient.Notification = notification;
                notification.CampaignRecipients ??= new List<CampaignRecipient>();
                notification.CampaignRecipients.Add(recipient);
                finalChannel ??= "InApp";
                success = true; // la notification in-app est considérée comme succès
            }

            if (orderedChannels.Contains("Push"))
            {
                try
                {
                    var pushSuccess = await _firebaseNotificationService.EnvoyerNotificationAUtilisateurAsync(
                        recipient.IdUtilisateur,
                        campaign.Titre,
                        plainTextBody,
                        new Dictionary<string, string>
                        {
                            ["campaignId"] = campaign.IdCampaign.ToString(),
                            ["importance"] = campaign.Importance
                        });

                    if (pushSuccess)
                    {
                        success = true;
                        finalChannel = finalChannel == "InApp" ? "InApp+Push" : "Push";
                    }
                    else
                    {
                        errorMessages.Add("Push non délivré");
                    }
                }
                catch (Exception ex)
                {
                    errorMessages.Add($"Push: {ex.Message}");
                }
            }

            if (orderedChannels.Contains("Email") && !string.IsNullOrWhiteSpace(utilisateur.Email))
            {
                try
                {
                    var emailSuccess = await _emailService.SendGenericEmailAsync(
                        utilisateur.Email,
                        BuildNomComplet(utilisateur),
                        campaign.Titre,
                        plainTextBody,
                        htmlBody);

                    if (emailSuccess)
                    {
                        success = true;
                        finalChannel = finalChannel == null || finalChannel == "InApp" ? "Email" : $"{finalChannel}+Email";
                    }
                    else
                    {
                        errorMessages.Add("Email non envoyé");
                    }
                }
                catch (Exception ex)
                {
                    errorMessages.Add($"Email: {ex.Message}");
                }
            }

            if (orderedChannels.Contains("Sms") && !string.IsNullOrWhiteSpace(utilisateur.Telephone))
            {
                try
                {
                    var sms = await _smsService.EnvoyerSmsAUtilisateurAsync(
                        recipient.IdUtilisateur,
                        BuildSmsMessage(campaign, plainTextBody),
                        "COMMUNICATION");

                    if (sms != null && sms.Statut != "failed")
                    {
                        success = true;
                        finalChannel = finalChannel == null || finalChannel == "InApp" ? "Sms" : $"{finalChannel}+Sms";
                    }
                    else
                    {
                        errorMessages.Add("SMS non envoyé");
                    }
                }
                catch (Exception ex)
                {
                    errorMessages.Add($"SMS: {ex.Message}");
                }
            }

            recipient.Status = success ? "Envoye" : "Echec";
            recipient.FinalChannel = finalChannel;
            recipient.DateEnvoi = DateTime.UtcNow;
            recipient.ErrorMessage = success ? null : string.Join(" | ", errorMessages.Distinct());

            if (notification != null)
            {
                notification.StatutEnvoi = success ? "Envoye" : "Echec";
                notification.CanalUtilise = finalChannel ?? "InApp";
                notification.PayloadJson = BuildNotificationPayload(recipient, campaign);
            }

            return success;
        }

        private Notification BuildNotification(CommunicationCampaign campaign, CampaignRecipient recipient, Utilisateur utilisateur, string plainTextBody)
        {
            return new Notification
            {
                IdCampaign = campaign.IdCampaign,
                IdExpediteur = campaign.IdAuteur,
                IdDestinataire = recipient.IdUtilisateur,
                IdEcole = campaign.IdEcole,
                IdEleve = recipient.IdEleve,
                Titre = campaign.Titre,
                Contenu = plainTextBody.Length > 950 ? plainTextBody.Substring(0, 947) + "..." : plainTextBody,
                TypeNotification = "COMMUNICATION",
                Priorite = campaign.Importance.ToUpperInvariant(),
                EstLue = false,
                EstActive = true,
                Statut = true,
                DateCreation = DateTime.UtcNow
            };
        }

        private static string BuildSmsMessage(CommunicationCampaign campaign, string plainTextBody)
        {
            var builder = new StringBuilder();
            builder.Append(campaign.Titre.Trim());
            builder.Append(" - ");
            builder.Append(plainTextBody.Length > 120 ? plainTextBody[..117] + "..." : plainTextBody);
            return builder.ToString();
        }

        private static string BuildNomComplet(Utilisateur utilisateur)
        {
            var parts = new[]
            {
                utilisateur.PrenomUtilisateur,
                utilisateur.PostNomUtilisateur,
                utilisateur.NomUtilisateur
            }.Where(s => !string.IsNullOrWhiteSpace(s));

            return string.Join(" ", parts);
        }

        private static string RenderPlainText(string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
            {
                return string.Empty;
            }

            var text = MarkdownStripRegex.Replace(markdown, string.Empty);
            text = Regex.Replace(text, @"\[(.*?)\]\((.*?)\)", "$1");
            text = text.Replace("\r", string.Empty);
            return text.Trim();
        }

        private static string RenderHtmlBody(string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
            {
                return "<p></p>";
            }

            var lines = markdown.Replace("\r", string.Empty)
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(line => $"<p>{System.Net.WebUtility.HtmlEncode(line)}</p>");

            return string.Join(Environment.NewLine, lines);
        }

        private static string BuildNotificationPayload(CampaignRecipient recipient, CommunicationCampaign campaign)
        {
            var payload = new
            {
                campaignId = campaign.IdCampaign,
                recipientId = recipient.IdRecipient,
                finalChannel = recipient.FinalChannel,
                status = recipient.Status,
                sentAt = recipient.DateEnvoi
            };

            return JsonSerializer.Serialize(payload);
        }

        private async Task AddHistoryAsync(int idCampaign, int initiatedByUserId, int successCount, int failureCount, CancellationToken cancellationToken)
        {
            var history = new CommunicationHistory
            {
                IdCampaign = idCampaign,
                UserId = initiatedByUserId,
                Action = "EnvoiTermine",
                DetailJson = JsonSerializer.Serialize(new
                {
                    success = successCount,
                    failure = failureCount
                }),
                DateAction = DateTime.UtcNow
            };

            await _context.CommunicationHistory.AddAsync(history, cancellationToken);
        }
    }
}

