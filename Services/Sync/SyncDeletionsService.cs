using KelasiNaBiso.Data;
using KelasiNaBiso.Models.DTOs.Sync;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services.Sync
{
    /// <summary>
    /// Purge cache local : AuditLog DELETE/UPDATE + réconciliation soft-statut.
    /// Sans colonne UpdatedAt, le delta soft-statut est une réconciliation (idempotente côté client).
    /// </summary>
    public class SyncDeletionsService : ISyncDeletionsService
    {
        public const int MaxIds = 5000;

        private readonly KelasiNaBisoDbContext _context;
        private readonly ISyncIdempotencyService _idempotency;

        public SyncDeletionsService(
            KelasiNaBisoDbContext context,
            ISyncIdempotencyService idempotency)
        {
            _context = context;
            _idempotency = idempotency;
        }

        public async Task<SyncDeletionsDto> GetDeletionsAsync(
            int idEcole,
            SyncDeletionsRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Since)
                || !SyncPullHelpers.TryParseSince(request.Since, out var sinceUtc))
            {
                throw new ArgumentException(
                    "Le paramètre since (watermark) est obligatoire et doit être valide.",
                    nameof(request));
            }

            // Comparaison locale : AuditLog.DateAction est en heure serveur (souvent locale).
            var sinceLocal = sinceUtc.Kind == DateTimeKind.Utc
                ? sinceUtc.ToLocalTime()
                : sinceUtc;

            var snapshot = string.IsNullOrWhiteSpace(request.Snapshot)
                ? _idempotency.CreateSnapshot(idEcole)
                : request.Snapshot!;
            var nextSince = _idempotency.CreateWatermark();

            var auditRows = await _context.AuditLogs.AsNoTracking()
                .Where(a =>
                    a.DateAction > sinceLocal
                    && (a.IdEcole == null || a.IdEcole == idEcole)
                    && (a.TableName == "Eleve"
                        || a.TableName == "Frais"
                        || a.TableName == "Presence"
                        || a.TableName == "Paiement")
                    && (a.Action == "DELETE" || a.Action == "UPDATE"))
                .Select(a => new
                {
                    a.TableName,
                    a.RecordId,
                    a.Action,
                    a.ChangedFields,
                    a.NewValues
                })
                .ToListAsync(cancellationToken);

            var deletedEleves = new HashSet<int>();
            var deactivatedEleves = new HashSet<int>();
            var removedFrais = new HashSet<int>();
            var deletedPresences = new HashSet<int>();
            var deletedPayments = new HashSet<int>();

            foreach (var row in auditRows)
            {
                if (row.Action == "DELETE")
                {
                    switch (row.TableName)
                    {
                        case "Eleve":
                            deletedEleves.Add(row.RecordId);
                            break;
                        case "Frais":
                            removedFrais.Add(row.RecordId);
                            break;
                        case "Presence":
                            deletedPresences.Add(row.RecordId);
                            break;
                        case "Paiement":
                            deletedPayments.Add(row.RecordId);
                            break;
                    }
                    continue;
                }

                // UPDATE Statut → false (soft-delete)
                if (row.TableName == "Eleve"
                    && LooksLikeDeactivation(row.ChangedFields, row.NewValues))
                {
                    deactivatedEleves.Add(row.RecordId);
                }
                else if (row.TableName == "Frais"
                         && LooksLikeDeactivation(row.ChangedFields, row.NewValues))
                {
                    removedFrais.Add(row.RecordId);
                }
            }

            // Réconciliation soft-statut (idempotente côté client)
            var softEleves = await _context.Eleves.AsNoTracking()
                .Where(e => e.Statut != true
                            && e.Inscriptions.Any(i => i.IdEcole == idEcole))
                .Select(e => e.IdEleve)
                .Take(MaxIds)
                .ToListAsync(cancellationToken);
            foreach (var id in softEleves)
                deactivatedEleves.Add(id);

            var softFrais = await _context.Frais.AsNoTracking()
                .Where(f => f.IdEcole == idEcole && f.Statut != true)
                .Select(f => f.IdFrais)
                .Take(MaxIds)
                .ToListAsync(cancellationToken);
            foreach (var id in softFrais)
                removedFrais.Add(id);

            // Ne pas lister un élève à la fois deleted et deactivated
            deactivatedEleves.ExceptWith(deletedEleves);

            return new SyncDeletionsDto
            {
                Snapshot = snapshot,
                NextSince = nextSince,
                DeletedEleveIds = Cap(deletedEleves),
                DeactivatedEleveIds = Cap(deactivatedEleves),
                RemovedFraisIds = Cap(removedFrais),
                DeletedPresenceIds = Cap(deletedPresences),
                DeletedPaymentIds = Cap(deletedPayments)
            };
        }

        private static bool LooksLikeDeactivation(string? changedFields, string? newValues)
        {
            if (string.IsNullOrWhiteSpace(changedFields)
                || !changedFields.Contains("Statut", StringComparison.OrdinalIgnoreCase))
                return false;

            if (string.IsNullOrWhiteSpace(newValues))
                return false;

            return newValues.Contains("\"Statut\":false", StringComparison.OrdinalIgnoreCase)
                   || newValues.Contains("\"statut\":false", StringComparison.OrdinalIgnoreCase)
                   || newValues.Contains("\"Statut\": false", StringComparison.OrdinalIgnoreCase);
        }

        private static List<int> Cap(HashSet<int> ids)
            => ids.OrderBy(x => x).Take(MaxIds).ToList();
    }
}
