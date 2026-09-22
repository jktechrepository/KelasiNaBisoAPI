using KelasiNaBiso.Models.DTOs.Sync;

namespace KelasiNaBiso.Services.Sync
{
    public interface ISyncPresenceBatchService
    {
        Task<PresenceBatchResultDto> ProcessBatchAsync(
            int idEcole,
            PresenceBatchRequestDto request,
            int? idUtilisateur,
            CancellationToken cancellationToken = default);
    }
}
