using System;
using System.Collections.Generic;
using KelasiNaBiso.Models;

namespace KelasiNaBiso.Services.Notifications
{
    public interface IPresenceNotificationBuilder
    {
        NotificationMessage Build(NotificationContext context);
    }

    public class PresenceNotificationBuilder : IPresenceNotificationBuilder
    {
        public NotificationMessage Build(NotificationContext context)
        {
            if (context.Presence == null || context.Eleve == null || context.Tuteur == null)
            {
                throw new InvalidOperationException("Le contexte de notification présence est incomplet.");
            }

            var presence = context.Presence;
            var eleve = context.Eleve;
            var tuteur = context.Tuteur;
            var ecole = context.Ecole;
            var classe = eleve.Classe;

            var heureArrivee = presence.HeureArrivee.ToString(@"hh\:mm");
            var dateFormatee = presence.DateDuJour.ToString("dd/MM/yyyy");
            var statutPresence = presence.IsPresent == true ? "✅ PRÉSENT" : "❌ ABSENT";
            
            // Informations supplémentaires pour les métadonnées
            var nomEcole = ecole?.Nom ?? "École";
            var nomClasse = classe?.NomClasse ?? "Classe";
            var section = classe?.Section?.NomSection;

            // Titre simple et clair
            var titrePush = "Pointage de Présences";
            
            // Corps simple et informatif selon la proposition
            var corpsPush = presence.IsPresent == true
                ? $"L'élève {eleve.NomComplet} a bien pointé sa présence à l'école. Heure d'arrivée: {heureArrivee}."
                : $"L'élève {eleve.NomComplet} est absent aujourd'hui ({dateFormatee}).";

            if (!string.IsNullOrWhiteSpace(presence.Observation))
            {
                corpsPush += $" Note: {presence.Observation}";
            }

            PushNotificationMessage? pushMessage = null;
            if (context.AllowPush)
            {
                pushMessage = new PushNotificationMessage
                {
                    IsEnabled = true,
                    Title = titrePush,
                    Body = corpsPush,
                    Type = "PRESENCE_ELEVE",
                    Data = new Dictionary<string, string>
                    {
                        { "type", "PRESENCE_ELEVE" },
                        { "idPresence", presence.IdPresence.ToString() },
                        { "idEleve", eleve.IdEleve.ToString() },
                        { "nomEleve", eleve.NomComplet ?? "N/A" },
                        { "isPresent", (presence.IsPresent == true).ToString() },
                        { "datePointage", presence.DateDuJour.ToString("yyyy-MM-dd") },
                        { "heureArrivee", heureArrivee },
                        { "nomEcole", nomEcole },
                        { "nomClasse", nomClasse },
                        { "section", section ?? "N/A" }
                    }
                };
            }

            // SMS avec format simple (sans emojis ni accents)
            var smsBody = presence.IsPresent == true
                ? $"Pointage de Presences\nL'eleve {eleve.NomComplet} a bien pointe sa presence a l'ecole. Heure d'arrivee: {heureArrivee}."
                : $"Pointage de Presences\nL'eleve {eleve.NomComplet} est absent aujourd'hui ({dateFormatee}).";

            if (!string.IsNullOrWhiteSpace(presence.Observation))
            {
                smsBody += $" Note: {presence.Observation}";
            }

            var smsMessage = new SmsNotificationMessage
            {
                IsEnabled = context.AllowSms,
                Body = smsBody
            };

            InAppNotificationMessage? inAppMessage = null;
            if (context.AllowInApp)
            {
                // Titre in-app simple
                var titreInApp = "Pointage de Présences";
                
                inAppMessage = new InAppNotificationMessage
                {
                    IsEnabled = true,
                    Title = titreInApp,
                    Content = corpsPush,
                    Type = "PRESENCE_ELEVE",
                    Icon = presence.IsPresent == true ? "✅" : "❌",
                    Metadata = new Dictionary<string, string>
                    {
                        { "idPresence", presence.IdPresence.ToString() },
                        { "idEleve", eleve.IdEleve.ToString() },
                        { "isPresent", (presence.IsPresent == true).ToString() },
                        { "datePointage", presence.DateDuJour.ToString("yyyy-MM-dd") },
                        { "heureArrivee", heureArrivee },
                        { "nomEcole", nomEcole },
                        { "nomClasse", nomClasse },
                        { "section", section ?? "N/A" }
                    }
                };
            }

            return new NotificationMessage
            {
                Push = pushMessage,
                Sms = smsMessage,
                InApp = inAppMessage
            };
        }
    }
}

