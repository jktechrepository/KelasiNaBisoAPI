using KelasiNaBiso.Attributes;
using KelasiNaBiso.Helpers;
using KelasiNaBiso.Models.DTOs.Sync;
using KelasiNaBiso.Services.Sync;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KelasiNaBiso.Controllers
{
    /// <summary>
    /// Endpoints offline / sync (contrat portable adapté KelasiNaBiso).
    /// Étape 1 : pull lecture (bootstrap, eleves, frais-dus).
    /// </summary>
    [ApiController]
    [Route("api/sync")]
    [Authorize]
    public class SyncController : ControllerBase
    {
        private readonly ISyncPullService _pull;
        private readonly ISyncPaymentBatchService _paymentBatch;
        private readonly ISyncPresenceBatchService _presenceBatch;
        private readonly ISyncDeletionsService _deletions;
        private readonly ILogger<SyncController> _logger;

        public SyncController(
            ISyncPullService pull,
            ISyncPaymentBatchService paymentBatch,
            ISyncPresenceBatchService presenceBatch,
            ISyncDeletionsService deletions,
            ILogger<SyncController> logger)
        {
            _pull = pull;
            _paymentBatch = paymentBatch;
            _presenceBatch = presenceBatch;
            _deletions = deletions;
            _logger = logger;
        }

        /// <summary>
        /// Snapshot initial + watermark (année courante, max 5000 / collection).
        /// Si volumes &gt; 5000 : enchaîner <c>/eleves</c> et <c>/frais-dus</c>.
        /// </summary>
        [HttpGet("bootstrap")]
        [ProducesResponseType(typeof(SyncBootstrapDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<SyncBootstrapDto>> Bootstrap(CancellationToken cancellationToken)
        {
            var idEcole = ResolveIdEcole(out var error);
            if (error != null)
                return error;

            var dto = await _pull.GetBootstrapAsync(idEcole, cancellationToken);
            _logger.LogInformation(
                "Sync bootstrap école {IdEcole}: {NbEleves} élèves, {NbFrais} frais dus",
                idEcole,
                dto.Eleves.Count,
                dto.FraisDus.Count);
            return Ok(dto);
        }

        /// <summary>Delta / pages fiches élèves (lecture seule offline).</summary>
        [HttpGet("eleves")]
        [ProducesResponseType(typeof(SyncPageDto<EleveSyncDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SyncPageDto<EleveSyncDto>>> GetEleves(
            [FromQuery] SyncRequestDto request,
            CancellationToken cancellationToken)
        {
            var idEcole = ResolveIdEcole(out var error);
            if (error != null)
                return error;

            var page = await _pull.GetElevesPageAsync(idEcole, request ?? new SyncRequestDto(), cancellationToken);
            return Ok(page);
        }

        /// <summary>Alias portable de <c>/eleves</c> (clients).</summary>
        [HttpGet("clients")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType(typeof(SyncPageDto<EleveSyncDto>), StatusCodes.Status200OK)]
        public Task<ActionResult<SyncPageDto<EleveSyncDto>>> GetClients(
            [FromQuery] SyncRequestDto request,
            CancellationToken cancellationToken)
            => GetEleves(request, cancellationToken);

        /// <summary>Delta / pages postes à encaisser (frais dus).</summary>
        [HttpGet("frais-dus")]
        [ProducesResponseType(typeof(SyncPageDto<FraisDuSyncDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SyncPageDto<FraisDuSyncDto>>> GetFraisDus(
            [FromQuery] SyncFraisDusRequestDto request,
            CancellationToken cancellationToken)
        {
            var idEcole = ResolveIdEcole(out var error);
            if (error != null)
                return error;

            var page = await _pull.GetFraisDusPageAsync(
                idEcole,
                request ?? new SyncFraisDusRequestDto(),
                cancellationToken);
            return Ok(page);
        }

        /// <summary>Alias portable de <c>/frais-dus</c> (arrears).</summary>
        [HttpGet("arrears")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType(typeof(SyncPageDto<FraisDuSyncDto>), StatusCodes.Status200OK)]
        public Task<ActionResult<SyncPageDto<FraisDuSyncDto>>> GetArrears(
            [FromQuery] SyncFraisDusRequestDto request,
            CancellationToken cancellationToken)
            => GetFraisDus(request, cancellationToken);

        /// <summary>
        /// Upload batch paiements CASH-like offline (idempotent).
        /// Moko / Mobile Money / Carte → <c>rejected</c> (<c>MOKO_NOT_ALLOWED</c>).
        /// </summary>
        [HttpPost("payments/batch")]
        [Permission("Paiement.Create")]
        [ProducesResponseType(typeof(PaymentBatchResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaymentBatchResultDto>> PaymentsBatch(
            [FromBody] PaymentBatchRequestDto request,
            CancellationToken cancellationToken)
        {
            var idEcole = ResolveIdEcole(out var error);
            if (error != null)
                return error;

            if (request?.Items == null || request.Items.Count == 0)
            {
                return BadRequest(new { message = "La liste items est obligatoire et ne peut pas être vide." });
            }

            int? idUtilisateur = null;
            var uid = this.GetCurrentUserId();
            if (uid > 0) idUtilisateur = uid;

            var result = await _paymentBatch.ProcessBatchAsync(
                idEcole,
                request,
                idUtilisateur,
                cancellationToken);

            _logger.LogInformation(
                "Sync payments/batch école {IdEcole}: total={Total} created={Created} dup={Dup} rejected={Rejected} errors={Errors}",
                idEcole,
                result.Summary.Total,
                result.Summary.Created,
                result.Summary.Duplicates,
                result.Summary.Rejected,
                result.Summary.Errors);

            return Ok(result);
        }

        /// <summary>
        /// Upload batch pointages offline (élève XOR agent), idempotent.
        /// Double pointage le même jour → <c>rejected</c> (<c>ALREADY_POINTED</c>).
        /// </summary>
        [HttpPost("presences/batch")]
        [Permission("Presence.Create")]
        [ProducesResponseType(typeof(PresenceBatchResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PresenceBatchResultDto>> PresencesBatch(
            [FromBody] PresenceBatchRequestDto request,
            CancellationToken cancellationToken)
        {
            var idEcole = ResolveIdEcole(out var error);
            if (error != null)
                return error;

            if (request?.Items == null || request.Items.Count == 0)
            {
                return BadRequest(new { message = "La liste items est obligatoire et ne peut pas être vide." });
            }

            int? idUtilisateur = null;
            var uid = this.GetCurrentUserId();
            if (uid > 0) idUtilisateur = uid;

            var result = await _presenceBatch.ProcessBatchAsync(
                idEcole,
                request,
                idUtilisateur,
                cancellationToken);

            _logger.LogInformation(
                "Sync presences/batch école {IdEcole}: total={Total} created={Created} dup={Dup} rejected={Rejected} errors={Errors}",
                idEcole,
                result.Summary.Total,
                result.Summary.Created,
                result.Summary.Duplicates,
                result.Summary.Rejected,
                result.Summary.Errors);

            return Ok(result);
        }

        /// <summary>
        /// Purge cache local (deletions / désactivations) depuis un watermark.
        /// <c>since</c> obligatoire.
        /// </summary>
        [HttpGet("deletions")]
        [ProducesResponseType(typeof(SyncDeletionsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SyncDeletionsDto>> GetDeletions(
            [FromQuery] SyncDeletionsRequestDto request,
            CancellationToken cancellationToken)
        {
            var idEcole = ResolveIdEcole(out var error);
            if (error != null)
                return error;

            if (request == null || string.IsNullOrWhiteSpace(request.Since))
            {
                return BadRequest(new { message = "Le paramètre since (watermark) est obligatoire." });
            }

            try
            {
                var dto = await _deletions.GetDeletionsAsync(
                    idEcole,
                    request,
                    cancellationToken);

                _logger.LogInformation(
                    "Sync deletions école {IdEcole}: deletedEleves={D} deactivated={A} removedFrais={F}",
                    idEcole,
                    dto.DeletedEleveIds.Count,
                    dto.DeactivatedEleveIds.Count,
                    dto.RemovedFraisIds.Count);

                return Ok(dto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private int ResolveIdEcole(out ActionResult? error)
        {
            error = null;
            var idEcole = this.GetCurrentUserSchoolId();
            if (!idEcole.HasValue || idEcole.Value <= 0)
            {
                error = BadRequest(new
                {
                    message = "Le claim IdEcole est absent ou invalide. Reconnectez-vous avec un compte rattaché à une école."
                });
                return 0;
            }

            return idEcole.Value;
        }
    }
}
