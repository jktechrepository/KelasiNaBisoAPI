using KelasiNaBiso.Models.DTOs.Sync;

namespace KelasiNaBiso.Services.Sync
{
    public interface ISyncPullService
    {
        Task<SyncBootstrapDto> GetBootstrapAsync(int idEcole, CancellationToken cancellationToken = default);

        Task<SyncPageDto<EleveSyncDto>> GetElevesPageAsync(
            int idEcole,
            SyncRequestDto request,
            CancellationToken cancellationToken = default);

        Task<SyncPageDto<FraisDuSyncDto>> GetFraisDusPageAsync(
            int idEcole,
            SyncFraisDusRequestDto request,
            CancellationToken cancellationToken = default);
    }
}
