using System.Threading;
using System.Threading.Tasks;

namespace KelasiNaBiso.Services.Notifications
{
    public interface INotificationJobQueue
    {
        ValueTask EnqueueAsync(NotificationDispatchResult dispatchResult, CancellationToken cancellationToken = default);
        ValueTask<NotificationDispatchResult> DequeueAsync(CancellationToken cancellationToken);
    }
}

