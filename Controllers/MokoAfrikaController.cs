using KelasiNaBiso.Helpers;
using KelasiNaBiso.Models.DTOs.MokoAfrika;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services.MokoAfrika;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KelasiNaBiso.Controllers
{
    [ApiController]
    [Route("api/MokoAfrika")]
    [Authorize]
    public class MokoAfrikaController : ControllerBase
    {
        private readonly IMokoAfrikaService _mokoService;
        private readonly IPaiementMokoOrchestrator _orchestrator;
        private readonly IMokoCallbackHandler _callbackHandler;

        public MokoAfrikaController(
            IMokoAfrikaService mokoService,
            IPaiementMokoOrchestrator orchestrator,
            IMokoCallbackHandler callbackHandler)
        {
            _mokoService = mokoService;
            _orchestrator = orchestrator;
            _callbackHandler = callbackHandler;
        }

        /// <summary>Estime les frais MOKO et le montant total à collecter (PayIn).</summary>
        [HttpGet("fees/estimate")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(MokoFeeEstimateDto), 200)]
        public ActionResult<MokoFeeEstimateDto> EstimateFees(
            [FromQuery] decimal amount,
            [FromQuery] string method = "airtel",
            [FromQuery] string currency = "CDF")
        {
            try
            {
                var estimate = _mokoService.EstimateFees(amount, method, currency);
                return Ok(estimate);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Lance un PayIn MOKO pour le paiement de frais scolaires.</summary>
        [HttpPost("payin/frais-scolaire")]
        [Authorize(Roles = UserRoles.CashierPayInRoles)]
        [ProducesResponseType(typeof(PayInFraisScolaireResultDto), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PayInFraisScolaireResultDto>> PayInFraisScolaire(
            [FromBody] PayInFraisScolaireRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var idUtilisateur = this.GetCurrentUserId();
                var result = await _orchestrator.InitierPayInFraisScolaireAsync(
                    request,
                    idUtilisateur > 0 ? idUtilisateur : null,
                    cancellationToken);

                if (result.StatutPaiement == "Echoue")
                    return BadRequest(result);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Webhook MOKO (PayIn / PayOut) — public, vérification HMAC X-Signature.</summary>
        [HttpPost("callback")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Callback(CancellationToken cancellationToken)
        {
            Request.EnableBuffering();
            string rawBody;
            using (var reader = new StreamReader(Request.Body, leaveOpen: true))
            {
                rawBody = await reader.ReadToEndAsync();
            }
            Request.Body.Position = 0;

            var signature = Request.Headers["X-Signature"].FirstOrDefault();
            var result = await _callbackHandler.HandleAsync(rawBody, signature, cancellationToken);

            if (!result.Accepted)
                return Unauthorized(new { message = result.Message });

            return Ok(new { message = result.Message, reference = result.Reference });
        }

        /// <summary>Consulte le statut d'une transaction MOKO par référence.</summary>
        [HttpGet("status/{reference}")]
        [ProducesResponseType(typeof(TransactionMokoDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<TransactionMokoDto>> GetStatus(string reference)
        {
            var tx = await _mokoService.GetTransactionByReferenceAsync(reference);
            if (tx == null)
                return NotFound(new { message = "Transaction introuvable." });
            return Ok(tx);
        }

        /// <summary>Interroge le gateway MOKO (action check) et met à jour la transaction locale.</summary>
        [HttpPost("status/{reference}/check")]
        [ProducesResponseType(typeof(TransactionMokoDto), 200)]
        public async Task<ActionResult<TransactionMokoDto>> CheckGatewayStatus(string reference, CancellationToken cancellationToken)
        {
            var tx = await _mokoService.CheckStatusAsync(reference, cancellationToken);
            if (tx == null)
                return NotFound(new { message = "Transaction introuvable." });

            if (tx.Status == MokoTransactionStatuses.Success && tx.IdPaiement.HasValue && tx.Action == MokoActions.Debit)
            {
                await _mokoService.ConfirmerPayInEtNotifierAsync(
                    tx.IdPaiement.Value, tx.Reference, tx.GatewayTransactionId, cancellationToken);
            }

            return Ok(await _mokoService.GetTransactionByReferenceAsync(reference));
        }

        /// <summary>Relance manuelle d'un PayOut échoué — réservé Super-Admin.</summary>
        [HttpPost("payout/retry")]
        [Authorize(Roles = "Super-Admin")]
        [ProducesResponseType(typeof(PayOutRetryResultDto), 200)]
        public async Task<ActionResult<PayOutRetryResultDto>> RetryPayOut(
            [FromQuery] string payInReference,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(payInReference))
                return BadRequest(new { message = "payInReference requis." });

            try
            {
                var result = await _orchestrator.RetryPayOutAsync(payInReference, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
