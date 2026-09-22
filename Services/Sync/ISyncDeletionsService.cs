using KelasiNaBiso.Models.DTOs.Sync;

namespace KelasiNaBiso.Services.Sync
{
    public interface ISyncDeletionsService
    {
        Task<SyncDeletionsDto> GetDeletionsAsync(
            int idEcole,
            SyncDeletionsRequestDto request,
            CancellationToken cancellationToken = default);
    }
}
