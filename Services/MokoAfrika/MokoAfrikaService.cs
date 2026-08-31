using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.MokoAfrika;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services.Repositories;
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
    }

    public class MokoAfrikaService : IMokoAfrikaService
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly IMokoAfrikaGatewayClient _gateway;
        private readonly IMokoFeeCalculator _feeCalculator;
        private readonly IMokoWalletService _walletService;
        private readonly IEcolePaiementMobileService _ecolePaiementService;
        private readonly IPaiementRepository _paiementRepository;
        private readonly MokoSettings _settings;
        private readonly ILogger<MokoAfrikaService> _logger;

        public MokoAfrikaService(
            KelasiNaBisoDbContext context,
            IMokoAfrikaGatewayClient gateway,
            IMokoFeeCalculator feeCalculator,
            IMokoWalletService walletService,
            IEcolePaiementMobileService ecolePaiementService,
            IPaiementRepository paiementRepository,
            IOptions<MokoSettings> settings,
            ILogger<MokoAfrikaService> logger)
        {
            _context = context;
            _gateway = gateway;
            _feeCalculator = feeCalculator;
            _walletService = walletService;
            _ecolePaiementService = ecolePaiementService;
            _paiementRepository = paiementRepository;
            _settings = settings.Value;
            _logger = logger;
        }

        public MokoFeeEstimateDto EstimateFees(decimal montantNet, string method, string currency = "CDF") =>
            _feeCalculator.Estimate(montantNet, method, currency);

        public async Task<TransactionMokoDto?> GetTransactionByReferenceAsync(string reference)
        {
            var tx = await _context.TransactionsMoko.FirstOrDefaultAsync(t => t.Reference == reference);
            return tx == null ? null : MapTransaction(tx);
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
                tx.RawResponse = response.RawBody;
                tx.Status = response.IsSuccess ? MokoTransactionStatuses.Success : MokoTransactionStatuses.Error;
                tx.StatusDescription = response.Status ?? response.ErrorMessage;
                tx.GatewayTransactionId = response.TransactionId ?? tx.GatewayTransactionId;
                tx.DateModification = DateTime.Now;
                await _context.SaveChangesAsync(cancellationToken);
            }

            return tx == null ? null : MapTransaction(tx);
        }

        /// <summary>
        /// Appelé après succès PayIn (callback ou réponse synchrone) :
        /// confirme le paiement, crédite le wallet, planifie PayOut, envoie notification tuteur.
        /// </summary>
        public async Task ConfirmerPayInEtNotifierAsync(
            int idPaiement,
            string mokoReference,
            string? gatewayTransactionId,
            CancellationToken cancellationToken = default)
        {
            var paiement = await _context.Paiements
                .Include(p => p.Eleve)
                    .ThenInclude(e => e!.Inscriptions)
                        .ThenInclude(i => i.Classe)
                            .ThenInclude(c => c.Direction)
                .FirstOrDefaultAsync(p => p.IdPaiement == idPaiement, cancellationToken);

            if (paiement == null)
                throw new KeyNotFoundException($"Paiement {idPaiement} introuvable.");

            var tx = await _context.TransactionsMoko.FirstOrDefaultAsync(t => t.Reference == mokoReference, cancellationToken)
                ?? throw new KeyNotFoundException($"Transaction MOKO {mokoReference} introuvable.");

            if (tx.Status == MokoTransactionStatuses.Success && paiement.StatutPaiement == "Confirme")
            {
                _logger.LogInformation("PayIn déjà confirmé pour paiement {IdPaiement} ref {Reference}", idPaiement, mokoReference);
                return;
            }

            tx.Status = MokoTransactionStatuses.Success;
            tx.GatewayTransactionId = gatewayTransactionId ?? tx.GatewayTransactionId;
            tx.DateModification = DateTime.Now;

            paiement.StatutPaiement = "Confirme";
            paiement.ReferenceTransaction = mokoReference;
            paiement.DatePaiement = DateTime.Now;

            var montantNet = tx.AmountNet ?? (decimal)paiement.Montant;
            var idEcole = tx.IdEcole;

            await _walletService.CrediterApresPayInAsync(idEcole, idPaiement, tx.IdTransactionMoko, montantNet, mokoReference, cancellationToken);

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
                    MontantNet = montantNet,
                    Devise = tx.Devise,
                    Methode = tx.Method,
                    NumeroBeneficiaire = beneficiaire?.Numero,
                    ScheduledAt = DateTime.Now.AddMinutes(delai),
                    Status = MokoPayoutQueueStatuses.Pending,
                    DateCreation = DateTime.Now
                });
            }

            await _context.SaveChangesAsync(cancellationToken);

            await _paiementRepository.NotifierPaiementConfirmeAsync(idPaiement, cancellationToken);
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
