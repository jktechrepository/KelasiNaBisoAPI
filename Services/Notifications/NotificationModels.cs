using System.Collections.Generic;
using KelasiNaBiso.Models;

namespace KelasiNaBiso.Services.Notifications
{
    public enum NotificationKind
    {
        PresenceEleve,
        PaiementEleve
    }

    public class NotificationContext
    {
        public NotificationKind Kind { get; init; }
        public Presence? Presence { get; init; }
        public Paiement? Paiement { get; init; }
        public Eleve? Eleve { get; init; }
        public Tuteur? Tuteur { get; init; }
        public Utilisateur? UtilisateurTuteur { get; init; }
        public Ecole? Ecole { get; init; }
        public bool AcceptsSms { get; init; }
        public bool AllowPush { get; init; }
        public bool AllowInApp { get; init; }
        public bool AllowSms { get; init; }
        public bool TuteurActif { get; init; }
        public bool UtilisateurActif { get; init; }
        public IReadOnlyDictionary<string, bool>? Preferences { get; init; }
        public string? RaisonSkip { get; init; }
    }

    public class PushNotificationMessage
    {
        public string Title { get; init; } = string.Empty;
        public string Body { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public Dictionary<string, string> Data { get; init; } = new();
        public bool IsEnabled { get; init; }
    }

    public class SmsNotificationMessage
    {
        public string Body { get; init; } = string.Empty;
        public bool IsEnabled { get; init; }
    }

    public class InAppNotificationMessage
    {
        public string Title { get; init; } = string.Empty;
        public string Content { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public string? Icon { get; init; }
        public string? ActionLink { get; init; }
        public Dictionary<string, string> Metadata { get; init; } = new();
        public bool IsEnabled { get; init; }
    }

    public class NotificationMessage
    {
        public PushNotificationMessage? Push { get; init; }
        public SmsNotificationMessage? Sms { get; init; }
        public InAppNotificationMessage? InApp { get; init; }
    }

    public class NotificationDispatchResult
    {
        public NotificationDispatchResult(NotificationContext context, NotificationMessage message)
        {
            Context = context;
            Message = message;
        }

        public NotificationContext Context { get; }
        public NotificationMessage Message { get; }
    }
}

