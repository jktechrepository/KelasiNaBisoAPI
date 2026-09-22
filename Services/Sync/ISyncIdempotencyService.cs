using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.Sync;

namespace KelasiNaBiso.Services.Sync
{
    /// <summary>Résultat d'une tentative d'enregistrement idempotent.</summary>
    public sealed class SyncIdempotencyOutcome
    {
        public bool IsDuplicate { get; init; }
        public SyncClientRequest Record { get; init; } = null!;

        public SyncBatchItemResultDto ToBatchItemResult() => new()
        {
            ClientRequestId = Record.ClientRequestId,
            Status = IsDuplicate ? SyncStatus.Duplicate : Record.Status,
            ResourceId = Record.ResourceId,
            Message = IsDuplicate
                ? (Record.Message ?? "Requête déjà traitée (idempotence).")
                : Record.Message,
            ErrorCode = Record.ErrorCode
        };
    }

    public interface ISyncIdempotencyService
    {
        Task<SyncClientRequest?> FindAsync(int idEcole, string clientRequestId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Enregistre un résultat créé. Si (IdEcole, ClientRequestId) existe déjà → duplicate.
        /// </summary>
        Task<SyncIdempotencyOutcome> TryRegisterCreatedAsync(
            int idEcole,
            string clientRequestId,
            string resourceType,
            int resourceId,
            int? idUtilisateur = null,
            string? deviceId = null,
            string? message = null,
            string? resultJson = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Enregistre un rejet métier (ne doit pas être rejoué comme created).
        /// Si déjà présent → duplicate de l'enregistrement existant.
        /// </summary>
        Task<SyncIdempotencyOutcome> TryRegisterRejectedAsync(
            int idEcole,
            string clientRequestId,
            string resourceType,
            string message,
            string? errorCode = null,
            int? idUtilisateur = null,
            string? deviceId = null,
            CancellationToken cancellationToken = default);

        /// <summary>Construit un watermark opaque {utc:o}_{ticks}.</summary>
        string CreateWatermark(DateTime? utcNow = null);

        /// <summary>Construit un token snapshot de session.</summary>
        string CreateSnapshot(int idEcole, DateTime? utcNow = null);
    }
}
