using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KelasiNaBiso.Services.Notifications
{
    public class NotificationDispatcher : INotificationDispatcher
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly IPresenceNotificationBuilder _presenceBuilder;
        private readonly IPaiementNotificationBuilder _paiementBuilder;
        private readonly ILogger<NotificationDispatcher> _logger;

        public NotificationDispatcher(
            KelasiNaBisoDbContext context,
            IPresenceNotificationBuilder presenceBuilder,
            IPaiementNotificationBuilder paiementBuilder,
            ILogger<NotificationDispatcher> logger)
        {
            _context = context;
            _presenceBuilder = presenceBuilder;
            _paiementBuilder = paiementBuilder;
            _logger = logger;
        }

        public async Task<NotificationDispatchResult?> PreparePresenceAsync(int presenceId, CancellationToken cancellationToken = default)
        {
            var presence = await _context.Presences
                .AsNoTracking()
                .Include(p => p.Eleve)
                    .ThenInclude(e => e.Tuteur)
                .Include(p => p.Eleve)
                    .ThenInclude(e => e.Inscriptions)
                        .ThenInclude(i => i.Ecole)
                .Include(p => p.Eleve)
                    .ThenInclude(e => e.Inscriptions)
                        .ThenInclude(i => i.Classe)
                            .ThenInclude(c => c.Direction)
                                .ThenInclude(d => d.Ecole)
                .FirstOrDefaultAsync(p => p.IdPresence == presenceId, cancellationToken);

            if (presence == null)
            {
                _logger.LogWarning("Impossible de préparer la notification présence : présence {PresenceId} introuvable", presenceId);
                return null;
            }

            if (!presence.IdEleve.HasValue || presence.Eleve == null)
            {
                _logger.LogInformation("Pas de notification présence pour {PresenceId} car aucun élève lié", presenceId);
                return null;
            }

            if (presence.Eleve.Tuteur == null)
            {
                _logger.LogInformation("Pas de notification présence pour l'élève {EleveId} car aucun tuteur lié", presence.Eleve.IdEleve);
                return null;
            }

            var tuteur = presence.Eleve.Tuteur;
            var tuteurActif = tuteur.Statut != false;
            if (!tuteurActif)
            {
                _logger.LogInformation("Notification présence ignorée : tuteur {TuteurId} inactif", tuteur.IdTuteur);
                return null;
            }

            var utilisateurTuteur = await _context.Utilisateurs
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    u => u.IdTuteur == presence.Eleve.IdTuteur &&
                         u.Statut == true,
                    cancellationToken);

            var utilisateurActif = utilisateurTuteur != null;

            var ecole = presence.Eleve.Inscriptions
                .Where(i => i.Statut == true &&
                    (i.StatutInscription == "Confirmé" || i.StatutInscription == "Confirme" || i.StatutInscription.StartsWith("Confirm")))
                .OrderByDescending(i => i.DateInscription)
                .Select(i => i.Ecole ?? i.Classe.Direction.Ecole)
                .FirstOrDefault();
            var acceptsSms = ecole?.AcceptNotification == true;

            var preferences = await LoadPreferencesAsync(tuteur.IdTuteur, cancellationToken);

            bool PrefAllows(string key) => !preferences.TryGetValue(key, out var optIn) || optIn;

            var allowPush = utilisateurActif && PrefAllows("push");
            var allowInApp = utilisateurActif && PrefAllows("inapp");
            var allowSms = PrefAllows("sms") && acceptsSms && !string.IsNullOrWhiteSpace(tuteur.Telephone);

            var context = new NotificationContext
            {
                Kind = NotificationKind.PresenceEleve,
                Presence = presence,
                Eleve = presence.Eleve,
                Tuteur = tuteur,
                UtilisateurTuteur = utilisateurTuteur,
                Ecole = ecole,
                AcceptsSms = acceptsSms,
                AllowPush = allowPush,
                AllowInApp = allowInApp,
                AllowSms = allowSms,
                TuteurActif = tuteurActif,
                UtilisateurActif = utilisateurActif,
                Preferences = preferences
            };

            var message = _presenceBuilder.Build(context);

            if (!HasAnyEnabledChannel(message))
            {
                _logger.LogInformation("Notification présence ignorée : aucun canal autorisé pour tuteur {TuteurId}", tuteur.IdTuteur);
                return null;
            }

            return new NotificationDispatchResult(context, message);
        }

        public async Task<NotificationDispatchResult?> PreparePaiementAsync(int paiementId, CancellationToken cancellationToken = default)
        {
            var paiement = await _context.Paiements
                .AsNoTracking()
                .Include(p => p.Eleve)
                    .ThenInclude(e => e.Tuteur)
                .Include(p => p.Eleve)
                    .ThenInclude(e => e.Inscriptions)
                        .ThenInclude(i => i.Ecole)
                .Include(p => p.Eleve)
                    .ThenInclude(e => e.Inscriptions)
                        .ThenInclude(i => i.Classe)
                            .ThenInclude(c => c.Direction)
                                .ThenInclude(d => d.Ecole)
                .Include(p => p.Frais)
                .FirstOrDefaultAsync(p => p.IdPaiement == paiementId, cancellationToken);

            if (paiement == null)
            {
                _logger.LogWarning("Impossible de préparer la notification paiement : paiement {PaiementId} introuvable", paiementId);
                return null;
            }

            if (!paiement.IdEleve.HasValue || paiement.Eleve == null)
            {
                _logger.LogInformation("Pas de notification paiement pour {PaiementId} car aucun élève lié", paiementId);
                return null;
            }

            if (paiement.Eleve.Tuteur == null)
            {
                _logger.LogInformation("Pas de notification paiement pour l'élève {EleveId} car aucun tuteur lié", paiement.Eleve.IdEleve);
                return null;
            }

            var tuteur = paiement.Eleve.Tuteur;
            var tuteurActif = tuteur.Statut != false;
            if (!tuteurActif)
            {
                _logger.LogInformation("Notification paiement ignorée : tuteur {TuteurId} inactif", tuteur.IdTuteur);
                return null;
            }

            var utilisateurTuteur = await _context.Utilisateurs
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    u => u.IdTuteur == paiement.Eleve.IdTuteur &&
                         u.Statut == true,
                    cancellationToken);

            var utilisateurActif = utilisateurTuteur != null;

            var ecole = paiement.Eleve.Inscriptions
                .Where(i => i.Statut == true &&
                    (i.StatutInscription == "Confirmé" || i.StatutInscription == "Confirme" || i.StatutInscription.StartsWith("Confirm")))
                .OrderByDescending(i => i.DateInscription)
                .Select(i => i.Ecole ?? i.Classe.Direction.Ecole)
                .FirstOrDefault();
            var acceptsSms = ecole?.AcceptNotification == true;

            var preferences = await LoadPreferencesAsync(tuteur.IdTuteur, cancellationToken);

            bool PrefAllows(string key) => !preferences.TryGetValue(key, out var optIn) || optIn;

            var allowPush = utilisateurActif && PrefAllows("push");
            var allowInApp = utilisateurActif && PrefAllows("inapp");
            var allowSms = PrefAllows("sms") && acceptsSms && !string.IsNullOrWhiteSpace(tuteur.Telephone);

            var context = new NotificationContext
            {
                Kind = NotificationKind.PaiementEleve,
                Paiement = paiement,
                Eleve = paiement.Eleve,
                Tuteur = tuteur,
                UtilisateurTuteur = utilisateurTuteur,
                Ecole = ecole,
                AcceptsSms = acceptsSms,
                AllowPush = allowPush,
                AllowInApp = allowInApp,
                AllowSms = allowSms,
                TuteurActif = tuteurActif,
                UtilisateurActif = utilisateurActif,
                Preferences = preferences
            };

            var message = _paiementBuilder.Build(context);

            if (!HasAnyEnabledChannel(message))
            {
                _logger.LogInformation("Notification paiement ignorée : aucun canal autorisé pour tuteur {TuteurId}", tuteur.IdTuteur);
                return null;
            }

            return new NotificationDispatchResult(context, message);
        }

        private async Task<Dictionary<string, bool>> LoadPreferencesAsync(int idTuteur, CancellationToken cancellationToken)
        {
            var preferences = await _context.ParentCommunicationPreferences
                .AsNoTracking()
                .Where(p => p.IdTuteur == idTuteur)
                .ToListAsync(cancellationToken);

            var dict = new Dictionary<string, bool>(preferences.Count, System.StringComparer.OrdinalIgnoreCase);
            foreach (var preference in preferences)
            {
                var key = preference.Canal?.Trim().ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                dict[key] = preference.OptIn;
            }

            return dict;
        }

        private static bool HasAnyEnabledChannel(NotificationMessage message)
        {
            return (message.Push?.IsEnabled == true) ||
                   (message.InApp?.IsEnabled == true) ||
                   (message.Sms?.IsEnabled == true);
        }
    }
}

