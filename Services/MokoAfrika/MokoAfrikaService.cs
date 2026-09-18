using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.MokoAfrika;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBisoAPI.Services.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KelasiNaBiso.Services.MokoAfrika
{
    public interface IMokoAfrikaService
    {
        MokoFeeEstimateDto EstimateFees(decimal montantNet, string method, string currency = "CDF");
        Task<TransactionMokoDto?> GetTransactionByReferenceAsync(string reference);
        Task<TransactionMokoDto?> CheckStatusAsync(string reference, CancellationToken cancellationToken = default);
        Task ConfirmerPayInEtNotifierAsync(int idPaiement, string mokoReference, string? gatewayTransactionId, CancellationToken cancellationToken = default);
        /// <summary>
        /// Confirme un PayIn par référence MOKO : crée le Paiement (Confirmé) s'il n'existe pas encore, puis wallet / payout / notifs.
        /// </summary>
        Task<int> ConfirmerPayInEtNotifierAsync(string mokoReference, string? gatewayTransactionId, CancellationToken cancellationToken = default);
    }

    public class MokoAfrikaService : IMokoAfrikaService
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly IMokoAfrikaGatewayClient _gateway;
        private readonly IMokoFeeCalculator _feeCalculator;
        private readonly IMokoWalletService _walletService;
        private readonly IEcolePaiementMobileService _ecolePaiementService;
        private readonly IPaiementRepository _paiementRepository;
        private readonly IDashboardHubService _dashboardHubService;
        private readonly MokoSettings _settings;
        private readonly ILogger<MokoAfrikaService> _logger;

        public MokoAfrikaService(
            KelasiNaBisoDbContext context,
            IMokoAfrikaGatewayClient gateway,
            IMokoFeeCalculator feeCalculator,
            IMokoWalletService walletService,
            IEcolePaiementMobileService ecolePaiementService,
            IPaiementRepository paiementRepository,
            IDashboardHubService dashboardHubService,
            IOptions<MokoSettings> settings,
            ILogger<MokoAfrikaService> logger)
        {
            _context = context;
            _gateway = gateway;
            _feeCalculator = feeCalculator;
            _walletService = walletService;
            _ecolePaiementService = ecolePaiementService;
            _paiementRepository = paiementRepository;
            _dashboardHubService = dashboardHubService;
            _settings = settings.Value;
            _logger = logger;
        }

        public MokoFeeEstimateDto EstimateFees(decimal montantNet, string method, string currency = "CDF") =>
            _feeCalculator.Estimate(montantNet, method, currency);

        public async Task<TransactionMokoDto?> GetTransactionByReferenceAsync(string reference)
        {
            var tx = await _context.TransactionsMoko.FirstOrDefaultAsync(t => t.Reference == reference);
            if (tx == null)
                return null;

            var dto = MapTransaction(tx);
            EnrichGatewayDiagnostics(dto, root: null, tx.RawCallback, tx.RawResponse);
            return dto;
        }

        public async Task<TransactionMokoDto?> CheckStatusAsync(string reference, CancellationToken cancellationToken = default)
        {
            var payload = BuildMerchantPayload();
            payload["action"] = MokoActions.Check;
            payload["reference"] = reference;

            var response = await _gateway.SendAsync(payload, cancellationToken);
            var tx = await _context.TransactionsMoko.FirstOrDefaultAsync(t => t.Reference == reference, cancellationToken);
            if (tx != null)
            {
                var previousStatus = tx.Status;
                tx.RawResponse = response.RawBody;
                tx.StatusDescription = response.Status ?? response.ErrorMessage;
                tx.GatewayTransactionId = response.TransactionId ?? tx.GatewayTransactionId;
                tx.DateModification = DateTime.Now;

                var withinUssdWindow = previousStatus == MokoTransactionStatuses.Pending
                    && tx.Action == MokoActions.Debit
                    && (DateTime.Now - tx.DateCreation).TotalSeconds < _settings.PayInUssdWindowSeconds;

                var softAmbiguous = false;
                var hardFailure = false;
                var terminalFailure = false;
                if (response.Parsed != null)
                {
                    var root = response.Parsed.RootElement;
                    softAmbiguous = MokoGatewayResponseParser.IsSoftAmbiguousFailure(root);
                    hardFailure = MokoGatewayResponseParser.IsDefinitiveFailure(root);
                    terminalFailure = MokoGatewayResponseParser.IsTerminalFailureStatus(root);
                }
                else
                {
                    hardFailure = response.IsFailure;
                }

                if (response.IsSuccess)
                {
                    tx.Status = MokoTransactionStatuses.Success;
                }
                else if (withinUssdWindow)
                {
                    // Fenêtre USSD : succès, ou échec terminal (Trans_Status Failed / cancelled).
                    // resultCodeError technique seul → pending conservé.
                    if (terminalFailure)
                    {
                        tx.Status = MokoTransactionStatuses.Error;
                        _logger.LogWarning(
                            "MOKO check échec terminal dans fenêtre USSD pour {Reference}. Description={Desc} Body={Body}",
                            reference, tx.StatusDescription, Truncate(response.RawBody, 500));
                    }
                    else
                    {
                        tx.Status = MokoTransactionStatuses.Pending;
                        if (hardFailure || softAmbiguous || response.IsFailure)
                        {
                            _logger.LogWarning(
                                "MOKO check échec technique ignoré dans fenêtre USSD ({Seconds}s) pour {Reference} — pending conservé. Body={Body}",
                                _settings.PayInUssdWindowSeconds, reference, Truncate(response.RawBody, 500));
                        }
                    }
                }
                else if (response.IsPending && !softAmbiguous && !hardFailure)
                {
                    // Pending explicite hors fenêtre (processing…) — on laisse pending.
                    tx.Status = MokoTransactionStatuses.Pending;
                }
                else if (softAmbiguous || hardFailure || response.IsFailure)
                {
                    // Hors fenêtre USSD : soft Error/Failed ne reste plus pending indéfiniment.
                    if (previousStatus == MokoTransactionStatuses.Pending)
                    {
                        _logger.LogWarning(
                            "MOKO check pending→error hors fenêtre USSD pour {Reference}. Soft={Soft} Hard={Hard} Desc={Desc} Body={Body}",
                            reference, softAmbiguous, hardFailure, tx.StatusDescription, Truncate(response.RawBody, 500));
                    }

                    tx.Status = MokoTransactionStatuses.Error;
                }
                else
                {
                    tx.Status = MokoTransactionStatuses.Pending;
                }

                await _context.SaveChangesAsync(cancellationToken);

                if (previousStatus == MokoTransactionStatuses.Pending
                    && tx.Status == MokoTransactionStatuses.Error
                    && tx.Action == MokoActions.Debit)
                {
                    await NotifierPayInFailedAsync(tx, cancellationToken);
                }

                var dto = MapTransaction(tx);
                EnrichGatewayDiagnostics(dto, response.Parsed?.RootElement, tx.RawCallback, response.RawBody);
                return dto;
            }

            return null;
        }

        private async Task NotifierPayInFailedAsync(TransactionMoko tx, CancellationToken cancellationToken)
        {
            var intent = PayInRawRequestHelper.TryReadIntent(tx.RawRequest);
            await _dashboardHubService.NotifyPayInFailedAsync(tx.IdEcole, new PayInSignalRNotification
            {
                Reference = tx.Reference,
                IdPaiement = null,
                IdEleve = intent?.IdEleve,
                MontantNet = intent?.MontantNet ?? tx.AmountNet ?? tx.Amount,
                StatutPaiement = "Echoue",
                StatutGateway = MokoTransactionStatuses.Error,
                StatusDescription = tx.StatusDescription
            });
        }

        private static void EnrichGatewayDiagnostics(
            TransactionMokoDto dto,
            System.Text.Json.JsonElement? root,
            string? rawCallback,
            string? rawResponse)
        {
            dto.HasCallback = !string.IsNullOrWhiteSpace(rawCallback);

            System.Text.Json.JsonElement el;
            if (root is { ValueKind: System.Text.Json.JsonValueKind.Object } r)
            {
                el = r;
            }
            else if (!string.IsNullOrWhiteSpace(rawResponse))
            {
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(rawResponse);
                    el = doc.RootElement.Clone();
                }
                catch (System.Text.Json.JsonException)
                {
                    return;
                }
            }
            else
            {
                return;
            }

            dto.GatewayStatusRaw = MokoGatewayResponseParser.GetString(
                el, "Trans_Status", "trans_Status", "TransStatus", "Status", "trans_status", "status");
            dto.ResultCodeError = MokoGatewayResponseParser.GetString(el, "resultCodeError");
            dto.ResultCodeErrorDescription = MokoGatewayResponseParser.GetString(
                el, "resultCodeErrorDescription", "Comment", "trans_status_description", "Trans_Status_Description");
        }

        private static string Truncate(string? value, int max)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            return value.Length <= max ? value : value[..max] + "…";
        }

        public async Task ConfirmerPayInEtNotifierAsync(
            int idPaiement,
            string mokoReference,
            string? gatewayTransactionId,
            CancellationToken cancellationToken = default)
        {
            await ConfirmerPayInEtNotifierAsync(mokoReference, gatewayTransactionId, cancellationToken);
        }

        /// <summary>
        /// Appelé après succès PayIn (callback ou check) :
        /// crée le Paiement Confirmé si besoin, crédite le wallet, planifie PayOut, notifie.
        /// </summary>
        public async Task<int> ConfirmerPayInEtNotifierAsync(
            string mokoReference,
            string? gatewayTransactionId,
            CancellationToken cancellationToken = default)
        {
            var tx = await _context.TransactionsMoko.FirstOrDefaultAsync(t => t.Reference == mokoReference, cancellationToken)
                ?? throw new KeyNotFoundException($"Transaction MOKO {mokoReference} introuvable.");

            Paiement? paiement = null;
            if (tx.IdPaiement.HasValue)
            {
                paiement = await _context.Paiements
                    .Include(p => p.Eleve)
                        .ThenInclude(e => e!.Inscriptions)
                            .ThenInclude(i => i.Classe)
                                .ThenInclude(c => c.Direction)
                    .FirstOrDefaultAsync(p => p.IdPaiement == tx.IdPaiement.Value, cancellationToken);
            }

            if (paiement != null
                && tx.Status == MokoTransactionStatuses.Success
                && paiement.StatutPaiement == "Confirme")
            {
                _logger.LogInformation("PayIn déjà confirmé pour paiement {IdPaiement} ref {Reference}", paiement.IdPaiement, mokoReference);
                return paiement.IdPaiement;
            }

            if (paiement == null)
            {
                var intent = PayInRawRequestHelper.TryReadIntent(tx.RawRequest)
                    ?? throw new InvalidOperationException(
                        $"Impossible de créer le paiement : intent absent pour la référence {mokoReference}.");

                paiement = PayInRawRequestHelper.CreateConfirmedPaiement(intent, mokoReference);
                _context.Paiements.Add(paiement);
                await _context.SaveChangesAsync(cancellationToken);
                tx.IdPaiement = paiement.IdPaiement;
            }
            else
            {
                paiement.StatutPaiement = "Confirme";
                paiement.ReferenceTransaction = mokoReference;
                paiement.DatePaiement = DateTime.Now;
            }

            tx.Status = MokoTransactionStatuses.Success;
            tx.GatewayTransactionId = gatewayTransactionId ?? tx.GatewayTransactionId;
            tx.DateModification = DateTime.Now;

            var idPaiement = paiement.IdPaiement;
            var montantNetGateway = tx.AmountNet ?? paiement.MontantNet ?? (decimal)paiement.Montant;
            var idEcole = tx.IdEcole;

            var credit = await _walletService.CrediterApresPayInAsync(
                idEcole,
                idPaiement,
                tx.IdTransactionMoko,
                montantNetGateway,
                mokoReference,
                tx.Devise,
                cancellationToken);

            var info = await _context.EcolesInfoPaiementMobile.FirstOrDefaultAsync(i => i.IdEcole == idEcole, cancellationToken);
            var delai = info?.DelaiReglementMinutes ?? _settings.PayoutSettlementDelayMinutes;

            if (info?.PayoutAutomatique == true)
            {
                var beneficiaire = await _ecolePaiementService.GetBeneficiaireActifAsync(idEcole, tx.Method ?? MomoOperators.Airtel);
                _context.FilePayoutsMoko.Add(new FilePayoutMoko
                {
                    PayInReference = mokoReference,
                    IdTransactionMokoPayIn = tx.IdTransactionMoko,
                    IdEcole = idEcole,
                    IdPaiement = idPaiement,
                    MontantNet = credit.MontantWallet,
                    Devise = credit.DeviseWallet,
                    Methode = tx.Method,
                    NumeroBeneficiaire = beneficiaire?.Numero,
                    ScheduledAt = DateTime.Now.AddMinutes(delai),
                    Status = MokoPayoutQueueStatuses.Pending,
                    DateCreation = DateTime.Now
                });
            }

            await _context.SaveChangesAsync(cancellationToken);

            await _dashboardHubService.NotifyPayInConfirmedAsync(idEcole, new PayInSignalRNotification
            {
                Reference = mokoReference,
                IdPaiement = idPaiement,
                IdEleve = paiement.IdEleve,
                MontantNet = IntentMontantNetPourSignalR(paiement, tx),
                StatutPaiement = "Confirme",
                StatutGateway = MokoTransactionStatuses.Success
            });

            await _paiementRepository.NotifierPaiementConfirmeAsync(idPaiement, cancellationToken);
            return idPaiement;
        }

        private static decimal IntentMontantNetPourSignalR(Paiement paiement, TransactionMoko tx)
        {
            // Préférer le montant métier (devise frais) pour l'UI ; fallback gateway.
            if (paiement.MontantNet.HasValue && paiement.MontantNet.Value > 0)
                return paiement.MontantNet.Value;
            if (paiement.Montant > 0)
                return (decimal)paiement.Montant;
            return tx.AmountNet ?? tx.Amount;
        }

        private Dictionary<string, object?> BuildMerchantPayload() => new()
        {
            ["merchant_id"] = _settings.MerchantId,
            ["merchant_secrete"] = _settings.SecretKey,
            ["merchant_code"] = _settings.MerchantCode
        };

        private static TransactionMokoDto MapTransaction(TransactionMoko tx) => new()
        {
            IdTransactionMoko = tx.IdTransactionMoko,
            Reference = tx.Reference,
            ParentReference = tx.ParentReference,
            IdPaiement = tx.IdPaiement,
            IdEcole = tx.IdEcole,
            Action = tx.Action,
            Amount = tx.Amount,
            AmountNet = tx.AmountNet,
            Devise = tx.Devise,
            Method = tx.Method,
            Status = tx.Status,
            StatusDescription = tx.StatusDescription,
            IsDefinitive = tx.Status is MokoTransactionStatuses.Success
                or MokoTransactionStatuses.Error
                or MokoTransactionStatuses.Timeout,
            GatewayTransactionId = tx.GatewayTransactionId,
            DateCreation = tx.DateCreation
        };
    }

    /// <summary>
    /// Détermine si un paiement attend confirmation gateway avant notification.
    /// </summary>
    public static class PaiementGatewayHelper
    {
        private static readonly string[] ModesMokoInterdits =
        {
            "mobile money", "mobilemoney", "carte", "card", "momo"
        };

        /// <summary>
        /// Modes réservés au flux MOKO PayIn — interdits sur POST /api/Paiement manuel.
        /// </summary>
        public static bool EstModeMokoInterdit(Paiement paiement)
        {
            if (!string.IsNullOrWhiteSpace(paiement.OperateurMobileMoney))
                return true;

            if (paiement.MontantCollecte.HasValue)
                return true;

            return EstModeMokoInterdit(paiement.ModePaiement);
        }

        public static bool EstModeMokoInterdit(string? modePaiement)
        {
            if (string.IsNullOrWhiteSpace(modePaiement))
                return false;

            var normalized = modePaiement.Trim().ToLowerInvariant();
            return ModesMokoInterdits.Any(m => normalized.Contains(m, StringComparison.Ordinal));
        }

        public static void ValiderCreationManuelle(Paiement paiement)
        {
            if (!EstModeMokoInterdit(paiement))
                return;

            throw new InvalidOperationException(
                "Les paiements Mobile Money et Carte doivent passer par POST /api/MokoAfrika/payin/frais-scolaire. " +
                "POST /api/Paiement est réservé à Cash, Chèque et Virement.");
        }

        public static bool EstEnAttenteGateway(Paiement paiement)
        {
            if (paiement.StatutPaiement != "En attente")
                return false;

            var mode = paiement.ModePaiement?.Trim();
            return mode is "Mobile Money" or "Carte";
        }
    }
}
