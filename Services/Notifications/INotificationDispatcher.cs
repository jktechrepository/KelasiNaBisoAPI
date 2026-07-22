using System.Threading;
using System.Threading.Tasks;

namespace KelasiNaBiso.Services.Notifications
{
    public interface INotificationDispatcher
    {
        Task<NotificationDispatchResult?> PreparePresenceAsync(int presenceId, CancellationToken cancellationToken = default);
        Task<NotificationDispatchResult?> PreparePaiementAsync(int paiementId, CancellationToken cancellationToken = default);
    }
}

