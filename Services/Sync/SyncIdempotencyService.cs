using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.Sync;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace KelasiNaBiso.Services.Sync
{
    public class SyncIdempotencyService : ISyncIdempotencyService
    {
        private readonly KelasiNaBisoDbContext _context;

        public SyncIdempotencyService(KelasiNaBisoDbContext context)
        {
            _context = context;
        }

        public Task<SyncClientRequest?> FindAsync(
            int idEcole,
            string clientRequestId,
            CancellationToken cancellationToken = default)
        {
            var normalized = NormalizeClientRequestId(clientRequestId);
            return _context.SyncClientRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.IdEcole == idEcole && x.ClientRequestId == normalized,
                    cancellationToken);
        }

        public async Task<SyncIdempotencyOutcome> TryRegisterCreatedAsync(
            int idEcole,
            string clientRequestId,
            string resourceType,
            int resourceId,
            int? idUtilisateur = null,
            string? deviceId = null,
            string? message = null,
            string? resultJson = null,
            CancellationToken cancellationToken = default)
        {
            return await TryRegisterAsync(
                idEcole,
                clientRequestId,
                resourceType,
                SyncStatus.Created,
                resourceId,
                message,
                errorCode: null,
                idUtilisateur,
                deviceId,
                resultJson,
                cancellationToken);
        }

        public async Task<SyncIdempotencyOutcome> TryRegisterRejectedAsync(
            int idEcole,
            string clientRequestId,
            string resourceType,
            string message,
            string? errorCode = null,
            int? idUtilisateur = null,
            string? deviceId = null,
            CancellationToken cancellationToken = default)
        {
            return await TryRegisterAsync(
                idEcole,
                clientRequestId,
                resourceType,
                SyncStatus.Rejected,
                resourceId: null,
                message,
                errorCode,
                idUtilisateur,
                deviceId,
                resultJson: null,
                cancellationToken);
        }

        public string CreateWatermark(DateTime? utcNow = null)
        {
            var now = utcNow ?? DateTime.UtcNow;
            return $"{now:O}_{now.Ticks}";
        }

        public string CreateSnapshot(int idEcole, DateTime? utcNow = null)
        {
            var now = utcNow ?? DateTime.UtcNow;
            return $"eco{idEcole}_{now:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}";
        }

        private async Task<SyncIdempotencyOutcome> TryRegisterAsync(
            int idEcole,
            string clientRequestId,
            string resourceType,
            string status,
            int? resourceId,
            string? message,
            string? errorCode,
            int? idUtilisateur,
            string? deviceId,
            string? resultJson,
            CancellationToken cancellationToken)
        {
            var normalized = NormalizeClientRequestId(clientRequestId);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                throw new ArgumentException("clientRequestId est obligatoire.", nameof(clientRequestId));
            }

            if (normalized.Length > 36)
            {
                throw new ArgumentException("clientRequestId ne peut pas dépasser 36 caractères.", nameof(clientRequestId));
            }

            var existing = await _context.SyncClientRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.IdEcole == idEcole && x.ClientRequestId == normalized,
                    cancellationToken);

            if (existing != null)
            {
                return new SyncIdempotencyOutcome { IsDuplicate = true, Record = existing };
            }

            var entity = new SyncClientRequest
            {
                IdEcole = idEcole,
                ClientRequestId = normalized,
                ResourceType = resourceType,
                ResourceId = resourceId,
                Status = status,
                Message = Truncate(message, 500),
                ErrorCode = Truncate(errorCode, 80),
                ResultJson = resultJson,
                IdUtilisateur = idUtilisateur,
                DeviceId = Truncate(deviceId, 100),
                DateCreation = DateTime.UtcNow
            };

            _context.SyncClientRequests.Add(entity);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                return new SyncIdempotencyOutcome { IsDuplicate = false, Record = entity };
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                _context.Entry(entity).State = EntityState.Detached;
                var raced = await FindAsync(idEcole, normalized, cancellationToken)
                    ?? throw new InvalidOperationException(
                        "Violation d'unicité SyncClientRequests sans enregistrement existant.");
                return new SyncIdempotencyOutcome { IsDuplicate = true, Record = raced };
            }
        }

        private static string NormalizeClientRequestId(string clientRequestId)
            => (clientRequestId ?? string.Empty).Trim();

        private static string? Truncate(string? value, int max)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= max ? value : value[..max];
        }

        private static bool IsUniqueViolation(DbUpdateException ex)
        {
            if (ex.InnerException is MySqlException mysql)
            {
                // 1062 = duplicate entry
                return mysql.Number == 1062;
            }

            var message = ex.InnerException?.Message ?? ex.Message;
            return message.Contains("Duplicate", StringComparison.OrdinalIgnoreCase)
                   || message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase);
        }
    }
}
