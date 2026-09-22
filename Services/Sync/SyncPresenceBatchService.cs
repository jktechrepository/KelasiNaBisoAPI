using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.DTOs.Sync;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KelasiNaBiso.Services.Sync
{
    /// <summary>
    /// Batch pointages offline (élève XOR agent), idempotent via SyncClientRequests.
    /// </summary>
    public class SyncPresenceBatchService : ISyncPresenceBatchService
    {
        public const int MaxItemsPerBatch = 200;

        private readonly KelasiNaBisoDbContext _context;
        private readonly IPresenceRepository _presenceRepository;
        private readonly IAnneeScolaireRepository _anneeScolaireRepository;
        private readonly ISyncIdempotencyService _idempotency;
        private readonly ILogger<SyncPresenceBatchService> _logger;

        public SyncPresenceBatchService(
            KelasiNaBisoDbContext context,
            IPresenceRepository presenceRepository,
            IAnneeScolaireRepository anneeScolaireRepository,
            ISyncIdempotencyService idempotency,
            ILogger<SyncPresenceBatchService> logger)
        {
            _context = context;
            _presenceRepository = presenceRepository;
            _anneeScolaireRepository = anneeScolaireRepository;
            _idempotency = idempotency;
            _logger = logger;
        }

        public async Task<PresenceBatchResultDto> ProcessBatchAsync(
            int idEcole,
            PresenceBatchRequestDto request,
            int? idUtilisateur,
            CancellationToken cancellationToken = default)
        {
            var results = new List<PresenceBatchItemResultDto>();
            var items = request.Items ?? new List<PresenceBatchItemDto>();

            if (items.Count == 0)
            {
                return new PresenceBatchResultDto
                {
                    Results = results,
                    Summary = PresenceBatchResultDto.BuildSummary(results)
                };
            }

            if (items.Count > MaxItemsPerBatch)
            {
                foreach (var item in items)
                {
                    results.Add(new PresenceBatchItemResultDto
                    {
                        ClientRequestId = item.ClientRequestId ?? string.Empty,
                        Status = SyncStatus.Rejected,
                        ErrorCode = "BATCH_TOO_LARGE",
                        Message = $"Maximum {MaxItemsPerBatch} items par batch."
                    });
                }

                return new PresenceBatchResultDto
                {
                    Results = results,
                    Summary = PresenceBatchResultDto.BuildSummary(results)
                };
            }

            var annee = await _anneeScolaireRepository.GetAnneeCouranteAsync(idEcole);

            foreach (var item in items)
            {
                results.Add(await ProcessOneAsync(
                    idEcole, annee, item, idUtilisateur, cancellationToken));
            }

            return new PresenceBatchResultDto
            {
                Results = results,
                Summary = PresenceBatchResultDto.BuildSummary(results)
            };
        }

        private async Task<PresenceBatchItemResultDto> ProcessOneAsync(
            int idEcole,
            AnneeScolaire? annee,
            PresenceBatchItemDto item,
            int? idUtilisateur,
            CancellationToken cancellationToken)
        {
            var clientRequestId = (item.ClientRequestId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(clientRequestId) || clientRequestId.Length > 36)
            {
                return Reject(clientRequestId, "INVALID_CLIENT_REQUEST_ID",
                    "clientRequestId est obligatoire (UUID, max 36 caractères).");
            }

            var existing = await _idempotency.FindAsync(idEcole, clientRequestId, cancellationToken);
            if (existing != null)
            {
                return new PresenceBatchItemResultDto
                {
                    ClientRequestId = clientRequestId,
                    Status = SyncStatus.Duplicate,
                    IdPresence = existing.ResourceId,
                    Message = existing.Message ?? "Requête déjà traitée (idempotence).",
                    ErrorCode = existing.ErrorCode
                };
            }

            var hasEleve = item.IdEleve.HasValue && item.IdEleve.Value > 0;
            var hasAgent = item.IdAgent.HasValue && item.IdAgent.Value > 0;
            if (!hasEleve && !hasAgent)
            {
                return await PersistRejectAsync(
                    idEcole, clientRequestId, item.DeviceId, idUtilisateur,
                    "INVALID_TARGET",
                    "Une présence doit concerner soit un élève, soit un agent.",
                    cancellationToken);
            }

            if (hasEleve && hasAgent)
            {
                return await PersistRejectAsync(
                    idEcole, clientRequestId, item.DeviceId, idUtilisateur,
                    "INVALID_TARGET",
                    "Une présence ne peut pas concerner à la fois un élève et un agent.",
                    cancellationToken);
            }

            if (item.DateDuJour == default)
            {
                return await PersistRejectAsync(
                    idEcole, clientRequestId, item.DeviceId, idUtilisateur,
                    "INVALID_DATE",
                    "dateDuJour est obligatoire.",
                    cancellationToken);
            }

            TimeSpan heureArrivee;
            TimeSpan? heureDepart;
            try
            {
                var parseDto = new CreatePresenceDto
                {
                    HeureArrivee = item.HeureArrivee,
                    HeureDepart = item.HeureDepart
                };
                heureArrivee = parseDto.GetHeureArrivee();
                heureDepart = parseDto.GetHeureDepart();
            }
            catch (ArgumentException ex)
            {
                return await PersistRejectAsync(
                    idEcole, clientRequestId, item.DeviceId, idUtilisateur,
                    "INVALID_TIME",
                    ex.Message,
                    cancellationToken);
            }

            if (annee == null)
            {
                return await PersistRejectAsync(
                    idEcole, clientRequestId, item.DeviceId, idUtilisateur,
                    "NO_SCHOOL_YEAR",
                    "Aucune année scolaire courante pour cette école.",
                    cancellationToken);
            }

            try
            {
                if (hasEleve)
                {
                    var inSchool = await _context.Inscriptions.AsNoTracking()
                        .AnyAsync(i =>
                            i.Statut == true
                            && i.IdEleve == item.IdEleve!.Value
                            && i.IdEcole == idEcole
                            && i.IdAnneeScolaire == annee.IdAnneeScolaire
                            && i.StatutInscription != null
                            && (i.StatutInscription == InscriptionActiveRules.StatutConfirme
                                || i.StatutInscription == "Confirme"
                                || i.StatutInscription.StartsWith("Confirm")),
                            cancellationToken);

                    if (!inSchool)
                    {
                        return await PersistRejectAsync(
                            idEcole, clientRequestId, item.DeviceId, idUtilisateur,
                            "ELEVE_NOT_IN_SCHOOL",
                            "Élève sans inscription confirmée dans l'école / année courante.",
                            cancellationToken);
                    }
                }
                else
                {
                    var agentOk = await _context.Agents.AsNoTracking()
                        .AnyAsync(a =>
                            a.IdAgent == item.IdAgent!.Value
                            && a.IdEcole == idEcole
                            && a.Statut == true,
                            cancellationToken);

                    if (!agentOk)
                    {
                        return await PersistRejectAsync(
                            idEcole, clientRequestId, item.DeviceId, idUtilisateur,
                            "AGENT_NOT_IN_SCHOOL",
                            "Agent introuvable ou hors école.",
                            cancellationToken);
                    }
                }

                var presence = new Presence
                {
                    IdEleve = hasEleve ? item.IdEleve : null,
                    IdAgent = hasAgent ? item.IdAgent : null,
                    IsPresent = item.IsPresent ?? true,
                    HeureArrivee = heureArrivee,
                    HeureDepart = heureDepart,
                    DateDuJour = item.DateDuJour.Date,
                    Observation = string.IsNullOrWhiteSpace(item.Observation)
                        ? (string.IsNullOrWhiteSpace(item.DeviceId) ? null : $"Sync offline ({item.DeviceId})")
                        : item.Observation,
                    Statut = true,
                    Longitute = item.Longitude ?? string.Empty,
                    Latitude = item.Latitude ?? string.Empty,
                    IdVacation = item.IdVacation
                };

                var created = await _presenceRepository.CreateAsync(presence);

                var register = await _idempotency.TryRegisterCreatedAsync(
                    idEcole,
                    clientRequestId,
                    SyncResourceType.Presence,
                    created.IdPresence,
                    idUtilisateur,
                    item.DeviceId,
                    message: "Présence créée (sync offline).",
                    cancellationToken: cancellationToken);

                if (register.IsDuplicate)
                {
                    return new PresenceBatchItemResultDto
                    {
                        ClientRequestId = clientRequestId,
                        Status = SyncStatus.Duplicate,
                        IdPresence = register.Record.ResourceId,
                        Message = register.Record.Message ?? "Requête déjà traitée (idempotence).",
                        ErrorCode = register.Record.ErrorCode
                    };
                }

                return new PresenceBatchItemResultDto
                {
                    ClientRequestId = clientRequestId,
                    Status = SyncStatus.Created,
                    IdPresence = created.IdPresence,
                    Message = "Présence créée (sync offline)."
                };
            }
            catch (InvalidOperationException ex)
            {
                int? existingId = null;
                var code = "BUSINESS_RULE";
                if (ex.Message.Contains("déjà pointé", StringComparison.OrdinalIgnoreCase))
                {
                    code = "ALREADY_POINTED";
                    var today = await _presenceRepository.GetTodayPresenceAsync(
                        hasEleve ? item.IdEleve : null,
                        hasAgent ? item.IdAgent : null,
                        item.DateDuJour.Date);
                    existingId = today?.IdPresence;
                }

                var rejected = await PersistRejectAsync(
                    idEcole, clientRequestId, item.DeviceId, idUtilisateur,
                    code, ex.Message,
                    cancellationToken);
                if (existingId.HasValue)
                    rejected.IdPresence = existingId;
                return rejected;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erreur sync presences/batch école {IdEcole} clientRequestId={ClientRequestId}",
                    idEcole, clientRequestId);
                return new PresenceBatchItemResultDto
                {
                    ClientRequestId = clientRequestId,
                    Status = SyncStatus.Error,
                    Message = "Erreur technique — conserver l'item en file et réessayer.",
                    ErrorCode = "TECHNICAL_ERROR"
                };
            }
        }

        private async Task<PresenceBatchItemResultDto> PersistRejectAsync(
            int idEcole,
            string clientRequestId,
            string? deviceId,
            int? idUtilisateur,
            string errorCode,
            string message,
            CancellationToken cancellationToken)
        {
            var outcome = await _idempotency.TryRegisterRejectedAsync(
                idEcole,
                clientRequestId,
                SyncResourceType.Presence,
                message,
                errorCode,
                idUtilisateur,
                deviceId,
                cancellationToken);

            return new PresenceBatchItemResultDto
            {
                ClientRequestId = clientRequestId,
                Status = outcome.IsDuplicate ? SyncStatus.Duplicate : SyncStatus.Rejected,
                IdPresence = outcome.Record.ResourceId,
                Message = outcome.IsDuplicate
                    ? (outcome.Record.Message ?? message)
                    : message,
                ErrorCode = outcome.IsDuplicate
                    ? (outcome.Record.ErrorCode ?? errorCode)
                    : errorCode
            };
        }

        private static PresenceBatchItemResultDto Reject(string clientRequestId, string code, string message)
            => new()
            {
                ClientRequestId = clientRequestId,
                Status = SyncStatus.Rejected,
                ErrorCode = code,
                Message = message
            };
    }
}
