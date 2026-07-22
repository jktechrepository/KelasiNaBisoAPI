using System.Threading;
using System.Threading.Tasks;

namespace KelasiNaBiso.Services.Notifications
{
    public interface INotificationSender
    {
        Task SendAsync(NotificationDispatchResult dispatchResult, CancellationToken cancellationToken = default);
    }
}

