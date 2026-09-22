using KelasiNaBiso.Models.DTOs.Sync;

namespace KelasiNaBiso.Services.Sync
{
    public interface ISyncPaymentBatchService
    {
        Task<PaymentBatchResultDto> ProcessBatchAsync(
            int idEcole,
            PaymentBatchRequestDto request,
            int? idUtilisateur,
            CancellationToken cancellationToken = default);
    }
}
