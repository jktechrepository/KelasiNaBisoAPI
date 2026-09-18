using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.MokoAfrika;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Services;
using KelasiNaBisoAPI.Services.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KelasiNaBiso.Services.MokoAfrika
{
    public interface IPaiementMokoOrchestrator
    {
        Task<PayInFraisScolaireResultDto> InitierPayInFraisScolaireAsync(
            PayInFraisScolaireRequestDto request,
            int? idUtilisateur,
            CancellationToken cancellationToken = default);

        Task<bool> ExecuterPayOutFileAsync(int idFilePayoutMoko, CancellationToken cancellationToken = default);

        Task<PayOutRetryResultDto> RetryPayOutAsync(string payInReference, CancellationToken cancellationToken = default);
    }

    public class PaiementMokoOrchestrator : IPaiementMokoOrchestrator
    {
        private const int MaxPayOutRetries = 5;

        private readonly KelasiNaBisoDbContext _context;
        private readonly IMokoAfrikaGatewayClient _gateway;
        private readonly IMokoFeeCalculator _feeCalculator;
        private readonly IMokoWalletService _walletService;
        private readonly IEcolePaiementMobileService _ecolePaiementService;
        private readonly IMokoAfrikaService _mokoService;
        private readonly IDashboardHubService _dashboardHubService;
        private readonly ICurrencyConversionService _currencyConversion;
        private readonly IEmailService? _emailService;
        private readonly MokoSettings _settings;
        private readonly ILogger<PaiementMokoOrchestrator> _logger;

        public PaiementMokoOrchestrator(
            KelasiNaBisoDbContext context,
            IMokoAfrikaGatewayClient gateway,
            IMokoFeeCalculator feeCalculator,
            IMokoWalletService walletService,
            IEcolePaiementMobileService ecolePaiementService,
            IMokoAfrikaService mokoService,
            IDashboardHubService dashboardHubService,
            ICurrencyConversionService currencyConversion,
            IOptions<MokoSettings> settings,
            ILogger<PaiementMokoOrchestrator> logger,
            IEmailService? emailService = null)
        {
            _context = context;
            _gateway = gateway;
            _feeCalculator = feeCalculator;
            _walletService = walletService;
            _ecolePaiementService = ecolePaiementService;
            _mokoService = mokoService;
            _dashboardHubService = dashboardHubService;
            _currencyConversion = currencyConversion;
            _settings = settings.Value;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<PayInFraisScolaireResultDto> InitierPayInFraisScolaireAsync(
            PayInFraisScolaireRequestDto request,
            int? idUtilisateur,
            CancellationToken cancellationToken = default)
        {
            var method = request.Method.Trim().ToLowerInvariant();
            if (!MomoOperators.All.Contains(method))
                throw new ArgumentException($"Méthode invalide : {request.Method}");

            if (string.IsNullOrWhiteSpace(request.TelephonePayeur))
                throw new ArgumentException("Le téléphone du payeur est requis.");

            var eleve = await _context.Eleves
                .Include(e => e.Inscriptions)
                    .ThenInclude(i => i.Classe)
                        .ThenInclude(c => c.Direction)
                .FirstOrDefaultAsync(e => e.IdEleve == request.IdEleve, cancellationToken)
                ?? throw new KeyNotFoundException($"Élève {request.IdEleve} introuvable.");

            var inscription = eleve.Inscriptions
                .Where(i => i.Statut == true &&
                    (i.StatutInscription == "Confirmé" || i.StatutInscription == "Confirme" || i.StatutInscription.StartsWith("Confirm")))
                .OrderByDescending(i => i.DateInscription)
                .FirstOrDefault();
            if (inscription?.Classe?.Direction == null)
                throw new InvalidOperationException("L'élève n'est pas rattaché à une école.");

            var idEcole = inscription.Classe.Direction.IdEcole
                ?? throw new InvalidOperationException("L'école de l'élève est indéfinie.");

            var frais = await _context.Frais
                .Include(f => f.FraisDirections)
                .Include(f => f.FraisClasses)
                .FirstOrDefaultAsync(f => f.IdFrais == request.IdFrais && f.Statut == true, cancellationToken)
                ?? throw new KeyNotFoundException($"Frais {request.IdFrais} introuvable.");

            if (frais.IdEcole != idEcole)
                throw new InvalidOperationException("Ce frais n'appartient pas à l'école de l'élève.");

            if (frais.IdAnneeScolaire != inscription.IdAnneeScolaire)
                throw new InvalidOperationException("Ce frais n'appartient pas à l'année scolaire de l'élève.");

            var idDirection = inscription.Classe.IdDirection
                ?? throw new InvalidOperationException("La direction de l'élève est indéfinie.");

            if (!FraisEligibility.IsEligibleForInscription(
                    frais, idEcole, idDirection, inscription.IdAnneeScolaire, inscription.IdClasse))
                throw new InvalidOperationException("Ce frais n'est pas applicable à la classe de l'élève.");

            var infoPaiement = await _context.EcolesInfoPaiementMobile
                .FirstOrDefaultAsync(i => i.IdEcole == idEcole && i.Statut, cancellationToken)
                ?? throw new InvalidOperationException($"Paiement mobile non configuré pour l'école {idEcole}.");

            var isCard = method == MomoOperators.Card;
            if (isCard && !infoPaiement.CarteActif)
                throw new InvalidOperationException("Paiement par carte désactivé pour cette école.");
            if (!isCard && !infoPaiement.MobileMoneyActif)
                throw new InvalidOperationException(
                    "Paiement Mobile non disponible. Veuillez contacter la direction de votre école.");

            var pendingPaiementExists = await _context.Paiements.AnyAsync(p =>
                p.IdEleve == request.IdEleve &&
                p.IdFrais == request.IdFrais &&
                p.StatutPaiement == "En attente" &&
                p.Statut == true, cancellationToken);

            if (pendingPaiementExists)
                throw new InvalidOperationException("Un paiement est déjà en attente pour ce frais et cet élève.");

            var pendingTxCandidates = await _context.TransactionsMoko
                .AsNoTracking()
                .Where(t => t.IdEcole == idEcole
                    && t.Action == MokoActions.Debit
                    && t.Status == MokoTransactionStatuses.Pending
                    && t.IdPaiement == null)
                .OrderByDescending(t => t.DateCreation)
                .Take(50)
                .ToListAsync(cancellationToken);

            if (pendingTxCandidates.Any(t =>
            {
                var intent = PayInRawRequestHelper.TryReadIntent(t.RawRequest);
                return intent != null
                    && intent.IdEleve == request.IdEleve
                    && intent.IdFrais == request.IdFrais;
            }))
            {
                throw new InvalidOperationException("Un paiement mobile est déjà en attente pour ce frais et cet élève.");
            }

            var montantNet = request.MontantNet ?? (decimal)frais.Montant;
            if (montantNet <= 0)
                throw new ArgumentException("Le montant net doit être positif.");

            var ecole = await _context.Ecoles
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdEcole == idEcole, cancellationToken);

            var deviseFrais = NormalizeDeviseCode(frais.Devise)
                ?? throw new InvalidOperationException($"Le frais {frais.IdFrais} n'a pas de devise.");
            var codePrincipale = NormalizeDeviseCode(ecole?.CodeDevisePrincipale)
                ?? NormalizeDeviseCode(infoPaiement.Devise)
                ?? deviseFrais;
            // Devise MM préférée = allowlist de règlement alternatif (pas gateway forcé).
            var deviseMmPreferee = NormalizeDeviseCode(infoPaiement.Devise) ?? codePrincipale;
            var deviseReglement = NormalizeDeviseCode(request.Devise ?? request.Currency) ?? deviseFrais;
            // PayIn / USSD = devise de règlement (défaut = devise du frais).
            var deviseGateway = deviseReglement;

            ValidateClientDevise(deviseReglement, deviseFrais, codePrincipale, deviseMmPreferee);

            if (!await _currencyConversion.IsActiveDeviseAsync(idEcole, deviseFrais, cancellationToken))
            {
                throw new InvalidOperationException(
                    $"La devise du frais {deviseFrais} est absente ou inactive pour l'école {idEcole}.");
            }

            if (!await _currencyConversion.IsActiveDeviseAsync(idEcole, codePrincipale, cancellationToken))
            {
                throw new InvalidOperationException(
                    $"La devise principale {codePrincipale} est absente ou inactive pour l'école {idEcole}.");
            }

            if (!await _currencyConversion.IsActiveDeviseAsync(idEcole, deviseGateway, cancellationToken))
            {
                throw new InvalidOperationException(
                    $"La devise de règlement {deviseGateway} est absente ou inactive pour l'école {idEcole}.");
            }

            if (!await _currencyConversion.IsActiveDeviseAsync(idEcole, deviseMmPreferee, cancellationToken))
            {
                throw new InvalidOperationException(
                    $"La devise Mobile Money {deviseMmPreferee} est absente ou inactive pour l'école {idEcole}.");
            }

            // Conversion uniquement si le client choisit une devise de règlement ≠ devise du frais.
            var montantReglement = deviseReglement == deviseFrais
                ? montantNet
                : await ConvertMontantAsync(idEcole, deviseFrais, deviseReglement, montantNet, cancellationToken);

            var montantGateway = montantReglement;

            var (montantPayePrincipale, tauxVersPrincipale) = await ConvertVersPrincipaleAsync(
                idEcole, deviseFrais, codePrincipale, montantNet, cancellationToken);

            var fees = _feeCalculator.Estimate(montantGateway, method, deviseGateway);
            var reference = MokoReferenceGenerator.NewReference();
            var phone = NormaliserTelephone(request.TelephonePayeur);
            var modePaiement = isCard ? "Carte" : "Mobile Money";

            var intent = new PayInIntentSnapshot
            {
                IdEleve = request.IdEleve,
                IdFrais = request.IdFrais,
                IdUtilisateur = idUtilisateur,
                MontantNet = montantNet,
                MontantCollecte = fees.MontantCollecte,
                MontantGatewayNet = montantGateway,
                CodeDeviseFrais = deviseFrais,
                CodeDevisePrincipale = codePrincipale,
                CodeDevisePaiement = deviseGateway,
                TauxVersDevisePrincipale = tauxVersPrincipale,
                MontantPayeDevisePrincipale = montantPayePrincipale,
                ModePaiement = modePaiement,
                OperateurMobileMoney = method,
                Commentaire = request.Commentaire,
                TelephonePayeur = phone
            };

            var payload = BuildGatewayPayload(MokoActions.Debit, reference, fees.MontantCollecte, deviseGateway, phone, method);

            var tx = new TransactionMoko
            {
                Reference = reference,
                IdPaiement = null,
                IdEcole = idEcole,
                Action = MokoActions.Debit,
                Amount = fees.MontantCollecte,
                AmountNet = montantGateway,
                FraisCollecte = fees.FraisCollecte,
                FraisDecaissement = fees.FraisDecaissement,
                Devise = deviseGateway,
                CustomerPhone = phone,
                Method = method,
                Status = MokoTransactionStatuses.Pending,
                RawRequest = PayInRawRequestHelper.Serialize(intent, payload),
                DateCreation = DateTime.Now
            };

            _context.TransactionsMoko.Add(tx);
            await _context.SaveChangesAsync(cancellationToken);

            var response = await _gateway.SendAsync(payload, cancellationToken);
            tx.RawResponse = response.RawBody;
            tx.GatewayTransactionId = response.TransactionId;
            tx.StatusDescription = response.Status ?? response.ErrorMessage;
            tx.DateModification = DateTime.Now;

            var result = new PayInFraisScolaireResultDto
            {
                IdPaiement = null,
                IdEcole = idEcole,
                Reference = reference,
                MontantNet = montantNet,
                MontantCollecte = fees.MontantCollecte,
                CodeDevisePrincipale = codePrincipale,
                CodeDevisePaiement = deviseGateway,
                TauxVersDevisePrincipale = tauxVersPrincipale,
                MontantPayeDevisePrincipale = montantPayePrincipale,
                Frais = fees,
                GatewayTransactionId = response.TransactionId
            };

            if (response.IsFailure || response.HttpStatusCode < 200 || response.HttpStatusCode >= 300)
            {
                tx.Status = MokoTransactionStatuses.Error;
                await _context.SaveChangesAsync(cancellationToken);

                result.StatutPaiement = "Echoue";
                result.StatutGateway = MokoTransactionStatuses.Error;
                result.Message = response.ErrorMessage ?? "Échec PayIn.";
                result.RequiresUssdConfirmation = false;
            }
            else if (!isCard)
            {
                // Mobile Money : confirmation uniquement via callback ou polling status/check
                tx.Status = MokoTransactionStatuses.Pending;
                await _context.SaveChangesAsync(cancellationToken);

                result.StatutPaiement = "En attente";
                result.StatutGateway = MokoTransactionStatuses.Pending;
                result.Message = response.IsSuccess
                    ? "Transaction initiée — validez sur votre téléphone (USSD)."
                    : (response.ErrorMessage ?? "Transaction initiée — en attente de confirmation USSD/callback.");
                result.RequiresUssdConfirmation = true;

                await NotifierPayInPendingSignalRAsync(idEcole, reference, intent.IdEleve, montantNet);
            }
            else if (response.IsSuccess)
            {
                tx.Status = MokoTransactionStatuses.Success;
                await _context.SaveChangesAsync(cancellationToken);

                var idPaiement = await _mokoService.ConfirmerPayInEtNotifierAsync(
                    reference, response.TransactionId, cancellationToken);

                result.IdPaiement = idPaiement;
                result.StatutPaiement = "Confirme";
                result.StatutGateway = MokoTransactionStatuses.Success;
                result.Message = "PayIn confirmé avec succès.";
                result.RequiresUssdConfirmation = false;
            }
            else
            {
                tx.Status = MokoTransactionStatuses.Pending;
                await _context.SaveChangesAsync(cancellationToken);

                result.StatutPaiement = "En attente";
                result.StatutGateway = MokoTransactionStatuses.Pending;
                result.Message = response.ErrorMessage ?? "Transaction initiée — en attente de confirmation.";
                result.RequiresUssdConfirmation = true;

                await NotifierPayInPendingSignalRAsync(idEcole, reference, intent.IdEleve, montantNet);
            }

            return result;
        }

        private async Task NotifierPayInPendingSignalRAsync(
            int idEcole,
            string reference,
            int idEleve,
            decimal montantNet)
        {
            await _dashboardHubService.NotifyPayInPendingAsync(idEcole, new PayInSignalRNotification
            {
                Reference = reference,
                IdPaiement = null,
                IdEleve = idEleve,
                MontantNet = montantNet,
                StatutPaiement = "En attente",
                StatutGateway = MokoTransactionStatuses.Pending
            });
        }

        public async Task<bool> ExecuterPayOutFileAsync(int idFilePayoutMoko, CancellationToken cancellationToken = default)
        {
            var file = await _context.FilePayoutsMoko
                .FirstOrDefaultAsync(f => f.IdFilePayoutMoko == idFilePayoutMoko, cancellationToken);

            if (file == null || file.Status is MokoPayoutQueueStatuses.Success or MokoPayoutQueueStatuses.Processing)
                return false;

            file.Status = MokoPayoutQueueStatuses.Processing;
            file.DateTraitement = DateTime.Now;
            file.DateModification = DateTime.Now;
            await _context.SaveChangesAsync(cancellationToken);

            try
            {
                var payInTx = await _context.TransactionsMoko
                    .FirstOrDefaultAsync(t => t.Reference == file.PayInReference, cancellationToken);

                if (payInTx == null)
                    throw new InvalidOperationException($"Transaction PayIn {file.PayInReference} introuvable.");

                var dejaLibere = await _context.EcolesWalletMouvements.AnyAsync(m =>
                    m.IdEcole == file.IdEcole &&
                    m.Reference == file.PayInReference &&
                    m.TypeMouvement == WalletMouvementTypes.PayInReleaseAvailable,
                    cancellationToken);

                if (!dejaLibere)
                {
                    await _walletService.LibererSoldeEnAttenteAsync(
                        file.IdEcole,
                        payInTx.IdTransactionMoko,
                        file.MontantNet,
                        file.PayInReference,
                        cancellationToken);
                }

                var methode = file.Methode ?? payInTx.Method ?? MomoOperators.Airtel;
                var beneficiaire = await _ecolePaiementService.GetBeneficiaireActifAsync(file.IdEcole, methode)
                    ?? throw new InvalidOperationException($"Aucun bénéficiaire actif pour l'école {file.IdEcole} ({methode}).");

                file.NumeroBeneficiaire = beneficiaire.Numero;
                file.Methode = methode;

                var payOutReference = MokoReferenceGenerator.NewReference();
                file.PayOutReference = payOutReference;

                var payload = BuildGatewayPayload(
                    MokoActions.Credit,
                    payOutReference,
                    file.MontantNet,
                    file.Devise,
                    beneficiaire.Numero,
                    methode);

                var payOutTx = new TransactionMoko
                {
                    Reference = payOutReference,
                    ParentReference = file.PayInReference,
                    IdPaiement = file.IdPaiement,
                    IdEcole = file.IdEcole,
                    Action = MokoActions.Credit,
                    Amount = file.MontantNet,
                    AmountNet = file.MontantNet,
                    Devise = file.Devise,
                    CustomerPhone = beneficiaire.Numero,
                    Method = methode,
                    Status = MokoTransactionStatuses.Pending,
                    RawRequest = JsonSerializer.Serialize(payload),
                    DateCreation = DateTime.Now
                };

                _context.TransactionsMoko.Add(payOutTx);
                await _context.SaveChangesAsync(cancellationToken);

                file.IdTransactionMokoPayOut = payOutTx.IdTransactionMoko;

                await _walletService.DebiterPourPayOutAsync(
                    file.IdEcole,
                    payOutTx.IdTransactionMoko,
                    file.IdPaiement,
                    file.MontantNet,
                    payOutReference,
                    cancellationToken);

                var response = await _gateway.SendAsync(payload, cancellationToken);
                payOutTx.RawResponse = response.RawBody;
                payOutTx.GatewayTransactionId = response.TransactionId;
                payOutTx.StatusDescription = response.Status ?? response.ErrorMessage;
                payOutTx.DateModification = DateTime.Now;

                if (response.IsSuccess)
                {
                    payOutTx.Status = MokoTransactionStatuses.Success;
                    file.Status = MokoPayoutQueueStatuses.Success;
                    file.ErrorMessage = null;
                }
                else
                {
                    payOutTx.Status = MokoTransactionStatuses.Pending;
                    file.Status = MokoPayoutQueueStatuses.Pending;
                    file.ScheduledAt = DateTime.Now.AddMinutes(2);
                    file.ErrorMessage = response.ErrorMessage;
                }

                file.DateModification = DateTime.Now;
                await _context.SaveChangesAsync(cancellationToken);

                if (!response.IsSuccess && response.HttpStatusCode >= 400)
                {
                    await HandlePayOutFailureAsync(file, payOutTx, response.ErrorMessage, cancellationToken);
                    return false;
                }

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur PayOut file {IdFilePayout}", idFilePayoutMoko);
                file.Status = MokoPayoutQueueStatuses.Failed;
                file.ErrorMessage = ex.Message;
                file.RetryCount++;
                file.DateModification = DateTime.Now;

                if (file.RetryCount < MaxPayOutRetries)
                {
                    file.Status = MokoPayoutQueueStatuses.Pending;
                    file.ScheduledAt = DateTime.Now.AddMinutes(5 * file.RetryCount);
                    file.PayOutReference = null;
                }

                await _context.SaveChangesAsync(cancellationToken);
                await EnvoyerAlertePayOutAsync(file, ex.Message, cancellationToken);
                return false;
            }
        }

        public async Task<PayOutRetryResultDto> RetryPayOutAsync(string payInReference, CancellationToken cancellationToken = default)
        {
            var file = await _context.FilePayoutsMoko
                .Where(f => f.PayInReference == payInReference)
                .OrderByDescending(f => f.DateCreation)
                .FirstOrDefaultAsync(cancellationToken);

            if (file == null)
            {
                var payInTx = await _context.TransactionsMoko
                    .FirstOrDefaultAsync(t => t.Reference == payInReference && t.Action == MokoActions.Debit, cancellationToken)
                    ?? throw new KeyNotFoundException($"Aucun PayIn trouvé pour {payInReference}.");

                var (montantWallet, deviseWallet) = await _walletService.ConvertirVersDeviseWalletAsync(
                    payInTx.IdEcole,
                    payInTx.AmountNet ?? payInTx.Amount,
                    payInTx.Devise,
                    cancellationToken);

                file = new FilePayoutMoko
                {
                    PayInReference = payInReference,
                    IdTransactionMokoPayIn = payInTx.IdTransactionMoko,
                    IdEcole = payInTx.IdEcole,
                    IdPaiement = payInTx.IdPaiement,
                    MontantNet = montantWallet,
                    Devise = deviseWallet,
                    Methode = payInTx.Method,
                    ScheduledAt = DateTime.Now,
                    Status = MokoPayoutQueueStatuses.Pending,
                    DateCreation = DateTime.Now
                };
                _context.FilePayoutsMoko.Add(file);
                await _context.SaveChangesAsync(cancellationToken);
            }
            else
            {
                file.Status = MokoPayoutQueueStatuses.Pending;
                file.ScheduledAt = DateTime.Now;
                file.PayOutReference = null;
                file.DateModification = DateTime.Now;
                await _context.SaveChangesAsync(cancellationToken);
            }

            var ok = await ExecuterPayOutFileAsync(file.IdFilePayoutMoko, cancellationToken);
            await _context.Entry(file).ReloadAsync(cancellationToken);

            return new PayOutRetryResultDto
            {
                PayInReference = payInReference,
                PayOutReference = file.PayOutReference,
                Status = file.Status,
                Message = ok ? "PayOut réussi." : file.ErrorMessage
            };
        }

        private async Task HandlePayOutFailureAsync(
            FilePayoutMoko file,
            TransactionMoko payOutTx,
            string? error,
            CancellationToken cancellationToken)
        {
            payOutTx.Status = MokoTransactionStatuses.Error;
            file.RetryCount++;
            file.ErrorMessage = error;

            if (file.IdTransactionMokoPayOut.HasValue)
            {
                try
                {
                    await _walletService.RecrediterPayOutEchoueAsync(
                        file.IdEcole,
                        payOutTx.IdTransactionMoko,
                        file.MontantNet,
                        payOutTx.Reference,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Recrédit wallet échoué pour PayOut {Ref}", payOutTx.Reference);
                }
            }

            if (file.RetryCount < MaxPayOutRetries)
            {
                file.Status = MokoPayoutQueueStatuses.Pending;
                file.ScheduledAt = DateTime.Now.AddMinutes(5 * file.RetryCount);
                file.PayOutReference = null;
            }
            else
            {
                file.Status = MokoPayoutQueueStatuses.Failed;
                await EnvoyerAlertePayOutAsync(file, error, cancellationToken);
            }

            file.DateModification = DateTime.Now;
            await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task EnvoyerAlertePayOutAsync(FilePayoutMoko file, string? error, CancellationToken cancellationToken)
        {
            if (_emailService == null || string.IsNullOrWhiteSpace(_settings.StaticCustomerEmail))
                return;

            try
            {
                var subject = $"[KelasiNaBiso] Échec PayOut MOKO — école {file.IdEcole}";
                var body = $"PayIn: {file.PayInReference}\nMontant net: {file.MontantNet} {file.Devise}\nErreur: {error}\nTentatives: {file.RetryCount}";
                await _emailService.SendGenericEmailAsync(
                    _settings.StaticCustomerEmail,
                    "Équipe technique",
                    subject,
                    body,
                    $"<pre>{body}</pre>");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Impossible d'envoyer l'alerte email PayOut");
            }
        }

        private static string? NormalizeDeviseCode(string? code) =>
            string.IsNullOrWhiteSpace(code) ? null : code.Trim().ToUpperInvariant();

        private static void ValidateClientDevise(
            string deviseReglement,
            string deviseFrais,
            string codePrincipale,
            string deviseMmPreferee)
        {
            if (deviseReglement == deviseFrais
                || deviseReglement == codePrincipale
                || deviseReglement == deviseMmPreferee)
                return;

            throw new InvalidOperationException(
                $"Devise non supportée pour ce PayIn : {deviseReglement}. " +
                $"Utilisez la devise du frais ({deviseFrais}), la devise principale ({codePrincipale}) " +
                $"ou la devise Mobile Money ({deviseMmPreferee}).");
        }

        private async Task<decimal> ConvertMontantAsync(
            int idEcole,
            string source,
            string cible,
            decimal montant,
            CancellationToken cancellationToken)
        {
            if (source == cible)
                return montant;

            var result = await _currencyConversion.ConvertAsync(
                idEcole,
                source,
                cible,
                montant,
                DateTime.UtcNow,
                cancellationToken);

            if (!result.Success)
            {
                throw new InvalidOperationException(
                    result.ErrorMessage
                    ?? $"Impossible de convertir {source} → {cible} pour le PayIn.");
            }

            return result.MontantConverti;
        }

        private async Task<(decimal MontantPrincipale, decimal TauxVersPrincipale)> ConvertVersPrincipaleAsync(
            int idEcole,
            string deviseFrais,
            string codePrincipale,
            decimal montantNet,
            CancellationToken cancellationToken)
        {
            if (deviseFrais == codePrincipale)
                return (montantNet, 1m);

            var toPrincipal = await _currencyConversion.ConvertAsync(
                idEcole,
                deviseFrais,
                codePrincipale,
                montantNet,
                DateTime.UtcNow,
                cancellationToken);

            if (!toPrincipal.Success)
            {
                throw new InvalidOperationException(
                    toPrincipal.ErrorMessage
                    ?? $"Impossible de convertir {deviseFrais} → {codePrincipale} pour la consolidation.");
            }

            return (toPrincipal.MontantConverti, toPrincipal.Taux);
        }

        private Dictionary<string, object?> BuildGatewayPayload(
            string action,
            string reference,
            decimal amount,
            string currency,
            string customerNumber,
            string method)
        {
            var payload = BuildMerchantPayload();
            payload["action"] = action;
            payload["amount"] = amount.ToString(System.Globalization.CultureInfo.InvariantCulture);
            payload["currency"] = currency;
            payload["customer_number"] = customerNumber;
            payload["firstname"] = _settings.StaticCustomerFirstName;
            payload["lastname"] = _settings.StaticCustomerLastName;
            payload["e-mail"] = _settings.StaticCustomerEmail;
            payload["reference"] = reference;
            payload["method"] = method;
            payload["callback_url"] = _settings.CallbackUrl;
            return payload;
        }

        private Dictionary<string, object?> BuildMerchantPayload() => new()
        {
            ["merchant_id"] = _settings.MerchantId,
            ["merchant_secrete"] = _settings.SecretKey,
            ["merchant_code"] = _settings.MerchantCode
        };

        private static string NormaliserTelephone(string numero) =>
            numero.Trim().Replace(" ", "").Replace("-", "");
    }
}
