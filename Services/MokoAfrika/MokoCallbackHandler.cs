using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Models.DTOs.MokoAfrika;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KelasiNaBiso.Services.MokoAfrika
{
    public interface IMokoCallbackHandler
    {
        Task<MokoCallbackResultDto> HandleAsync(string rawBody, string? signature, CancellationToken cancellationToken = default);
        bool VerifySignature(string rawBody, string? signature);
    }

    public class MokoCallbackHandler : IMokoCallbackHandler
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly IMokoAfrikaService _mokoService;
        private readonly IPaiementMokoOrchestrator _orchestrator;
        private readonly IMokoWalletService _walletService;
        private readonly MokoSettings _settings;
        private readonly ILogger<MokoCallbackHandler> _logger;

        public MokoCallbackHandler(
            KelasiNaBisoDbContext context,
            IMokoAfrikaService mokoService,
            IPaiementMokoOrchestrator orchestrator,
            IMokoWalletService walletService,
            IOptions<MokoSettings> settings,
            ILogger<MokoCallbackHandler> logger)
        {
            _context = context;
            _mokoService = mokoService;
            _orchestrator = orchestrator;
            _walletService = walletService;
            _settings = settings.Value;
            _logger = logger;
        }

        public bool VerifySignature(string rawBody, string? signature)
        {
            if (string.IsNullOrWhiteSpace(_settings.HmacKey) || string.IsNullOrWhiteSpace(signature))
                return string.IsNullOrWhiteSpace(_settings.HmacKey);

            var keyBytes = Encoding.UTF8.GetBytes(_settings.HmacKey);
            var bodyBytes = Encoding.UTF8.GetBytes(rawBody);
            var hash = HMACSHA256.HashData(keyBytes, bodyBytes);
            var computed = Convert.ToHexString(hash).ToLowerInvariant();
            return string.Equals(computed, signature.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        public async Task<MokoCallbackResultDto> HandleAsync(
            string rawBody,
            string? signature,
            CancellationToken cancellationToken = default)
        {
            if (!VerifySignature(rawBody, signature))
            {
                _logger.LogWarning("Callback MOKO : signature HMAC invalide");
                return new MokoCallbackResultDto { Accepted = false, Message = "Signature invalide" };
            }

            JsonDocument doc;
            try
            {
                doc = JsonDocument.Parse(rawBody);
            }
            catch (JsonException)
            {
                return new MokoCallbackResultDto { Accepted = false, Message = "Corps JSON invalide" };
            }

            using (doc)
            {
                var root = doc.RootElement;
                var reference = MokoGatewayResponseParser.GetString(root, "reference", "Reference");
                if (string.IsNullOrWhiteSpace(reference))
                    return new MokoCallbackResultDto { Accepted = false, Message = "Référence absente" };

                var isSuccess = MokoGatewayResponseParser.IsDefinitiveSuccess(root);
                var isSoftAmbiguous = MokoGatewayResponseParser.IsSoftAmbiguousFailure(root);
                var isHardFailure = MokoGatewayResponseParser.IsDefinitiveFailure(root);
                var isPendingLike = MokoGatewayResponseParser.IsPending(root) || isSoftAmbiguous;

                var tx = await _context.TransactionsMoko
                    .FirstOrDefaultAsync(t => t.Reference == reference, cancellationToken);

                if (tx == null)
                {
                    _logger.LogWarning("Callback MOKO pour référence inconnue : {Reference}", reference);
                    return new MokoCallbackResultDto { Accepted = true, Reference = reference, Message = "Référence inconnue (ignoré)" };
                }

                if (tx.Status == MokoTransactionStatuses.Success && isSuccess)
                    return new MokoCallbackResultDto { Accepted = true, Reference = reference, Message = "Déjà traité" };

                tx.RawCallback = rawBody;
                tx.StatusDescription = MokoGatewayResponseParser.GetString(root, "trans_status_description", "Comment", "Status", "trans_status", "status")
                    ?? tx.StatusDescription;
                tx.DateModification = DateTime.Now;

                string outcomeMessage;
                if (tx.Action == MokoActions.Debit)
                {
                    outcomeMessage = await HandlePayInCallbackAsync(
                        tx, isSuccess, isSoftAmbiguous, isHardFailure, isPendingLike, cancellationToken);
                }
                else if (tx.Action == MokoActions.Credit)
                {
                    outcomeMessage = await HandlePayOutCallbackAsync(
                        tx, isSuccess, isSoftAmbiguous, isHardFailure, isPendingLike, cancellationToken);
                }
                else
                {
                    if (isSuccess)
                    {
                        tx.Status = MokoTransactionStatuses.Success;
                        outcomeMessage = "Callback traité (succès)";
                    }
                    else if (isPendingLike && !isHardFailure)
                    {
                        tx.Status = MokoTransactionStatuses.Pending;
                        outcomeMessage = "Callback soft/pending — statut conservé";
                    }
                    else
                    {
                        tx.Status = MokoTransactionStatuses.Error;
                        outcomeMessage = "Callback traité (échec)";
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);

                return new MokoCallbackResultDto
                {
                    Accepted = true,
                    Reference = reference,
                    Message = outcomeMessage
                };
            }
        }

        private async Task<string> HandlePayInCallbackAsync(
            Models.TransactionMoko tx,
            bool isSuccess,
            bool isSoftAmbiguous,
            bool isHardFailure,
            bool isPendingLike,
            CancellationToken cancellationToken)
        {
            if (isSuccess)
            {
                tx.Status = MokoTransactionStatuses.Success;
                if (tx.IdPaiement.HasValue)
                {
                    await _mokoService.ConfirmerPayInEtNotifierAsync(
                        tx.IdPaiement.Value,
                        tx.Reference,
                        tx.GatewayTransactionId,
                        cancellationToken);
                }

                return "Callback traité (succès)";
            }

            // Soft Error/Failed (sans resultCodeError) ou pending explicite : ne pas tuer un PayIn USSD.
            if ((isPendingLike || isSoftAmbiguous) && !isHardFailure)
            {
                tx.Status = MokoTransactionStatuses.Pending;
                _logger.LogWarning(
                    "Callback MOKO PayIn soft/pending pour {Reference} (desc={Desc}) — pending conservé",
                    tx.Reference, tx.StatusDescription);
                return "Callback soft/pending — pending conservé (USSD)";
            }

            if (isHardFailure || !isPendingLike)
            {
                tx.Status = MokoTransactionStatuses.Error;
                if (tx.IdPaiement.HasValue)
                {
                    var paiement = await _context.Paiements.FindAsync(new object[] { tx.IdPaiement.Value }, cancellationToken);
                    if (paiement != null)
                        paiement.StatutPaiement = "Echoue";
                }

                _logger.LogWarning(
                    "Callback MOKO PayIn échec définitif pour {Reference} (desc={Desc})",
                    tx.Reference, tx.StatusDescription);
                return "Callback traité (échec)";
            }

            tx.Status = MokoTransactionStatuses.Pending;
            return "Callback soft/pending — pending conservé (USSD)";
        }

        private async Task<string> HandlePayOutCallbackAsync(
            Models.TransactionMoko tx,
            bool isSuccess,
            bool isSoftAmbiguous,
            bool isHardFailure,
            bool isPendingLike,
            CancellationToken cancellationToken)
        {
            var file = await _context.FilePayoutsMoko
                .FirstOrDefaultAsync(f => f.PayOutReference == tx.Reference || f.IdTransactionMokoPayOut == tx.IdTransactionMoko, cancellationToken);

            if (isSuccess)
            {
                tx.Status = MokoTransactionStatuses.Success;
                if (file != null)
                {
                    file.Status = MokoPayoutQueueStatuses.Success;
                    file.ErrorMessage = null;
                    file.DateModification = DateTime.Now;
                }

                return "Callback traité (succès)";
            }

            if ((isPendingLike || isSoftAmbiguous) && !isHardFailure)
            {
                tx.Status = MokoTransactionStatuses.Pending;
                _logger.LogWarning(
                    "Callback MOKO PayOut soft/pending pour {Reference} — pending conservé",
                    tx.Reference);
                return "Callback soft/pending — pending conservé";
            }

            tx.Status = MokoTransactionStatuses.Error;
            if (file != null)
            {
                try
                {
                    await _walletService.RecrediterPayOutEchoueAsync(
                        file.IdEcole,
                        tx.IdTransactionMoko,
                        file.MontantNet,
                        tx.Reference,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Recrédit wallet callback PayOut {Ref}", tx.Reference);
                }

                file.RetryCount++;
                file.ErrorMessage = tx.StatusDescription;
                if (file.RetryCount < 5)
                {
                    file.Status = MokoPayoutQueueStatuses.Pending;
                    file.ScheduledAt = DateTime.Now.AddMinutes(5 * file.RetryCount);
                    file.PayOutReference = null;
                }
                else
                {
                    file.Status = MokoPayoutQueueStatuses.Failed;
                }
                file.DateModification = DateTime.Now;
            }

            return "Callback traité (échec)";
        }
    }
}
