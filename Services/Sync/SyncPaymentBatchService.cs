using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.Sync;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Services.Tarif;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KelasiNaBiso.Services.Sync
{
    /// <summary>
    /// Batch d'écritures paiements CASH-like offline (idempotent via SyncClientRequests).
    /// </summary>
    public class SyncPaymentBatchService : ISyncPaymentBatchService
    {
        public const int MaxItemsPerBatch = 100;

        private readonly KelasiNaBisoDbContext _context;
        private readonly IPaiementRepository _paiementRepository;
        private readonly IAnneeScolaireRepository _anneeScolaireRepository;
        private readonly ISyncIdempotencyService _idempotency;
        private readonly IFraisDuCalculator _fraisDuCalculator;
        private readonly ILogger<SyncPaymentBatchService> _logger;

        public SyncPaymentBatchService(
            KelasiNaBisoDbContext context,
            IPaiementRepository paiementRepository,
            IAnneeScolaireRepository anneeScolaireRepository,
            ISyncIdempotencyService idempotency,
            IFraisDuCalculator fraisDuCalculator,
            ILogger<SyncPaymentBatchService> logger)
        {
            _context = context;
            _paiementRepository = paiementRepository;
            _anneeScolaireRepository = anneeScolaireRepository;
            _idempotency = idempotency;
            _fraisDuCalculator = fraisDuCalculator;
            _logger = logger;
        }

        public async Task<PaymentBatchResultDto> ProcessBatchAsync(
            int idEcole,
            PaymentBatchRequestDto request,
            int? idUtilisateur,
            CancellationToken cancellationToken = default)
        {
            var results = new List<PaymentBatchItemResultDto>();
            var items = request.Items ?? new List<PaymentBatchItemDto>();

            if (items.Count == 0)
            {
                return new PaymentBatchResultDto
                {
                    Results = results,
                    Summary = PaymentBatchResultDto.BuildSummary(results)
                };
            }

            if (items.Count > MaxItemsPerBatch)
            {
                // Rejet global soft : chaque item rejected pour feedback client uniforme
                foreach (var item in items)
                {
                    results.Add(new PaymentBatchItemResultDto
                    {
                        ClientRequestId = item.ClientRequestId ?? string.Empty,
                        Status = SyncStatus.Rejected,
                        ErrorCode = "BATCH_TOO_LARGE",
                        Message = $"Maximum {MaxItemsPerBatch} items par batch."
                    });
                }

                return new PaymentBatchResultDto
                {
                    Results = results,
                    Summary = PaymentBatchResultDto.BuildSummary(results)
                };
            }

            var annee = await _anneeScolaireRepository.GetAnneeCouranteAsync(idEcole);

            foreach (var item in items)
            {
                results.Add(await ProcessOneAsync(
                    idEcole,
                    annee,
                    item,
                    idUtilisateur,
                    cancellationToken));
            }

            return new PaymentBatchResultDto
            {
                Results = results,
                Summary = PaymentBatchResultDto.BuildSummary(results)
            };
        }

        private async Task<PaymentBatchItemResultDto> ProcessOneAsync(
            int idEcole,
            AnneeScolaire? annee,
            PaymentBatchItemDto item,
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
                decimal? montantDu = null;
                if (existing.ResourceId.HasValue && item.IdEleve > 0 && item.IdFrais > 0)
                {
                    montantDu = await ComputeMontantDuAsync(item.IdEleve, item.IdFrais, cancellationToken);
                }

                return new PaymentBatchItemResultDto
                {
                    ClientRequestId = clientRequestId,
                    Status = SyncStatus.Duplicate,
                    IdPaiement = existing.ResourceId,
                    NewMontantDu = montantDu,
                    Message = existing.Message ?? "Requête déjà traitée (idempotence).",
                    ErrorCode = existing.ErrorCode
                };
            }

            if (item.IdEleve <= 0 || item.IdFrais <= 0)
            {
                return await PersistRejectAsync(
                    idEcole, clientRequestId, item.DeviceId, idUtilisateur,
                    "INVALID_REFERENCE", "idEleve et idFrais sont obligatoires.",
                    cancellationToken);
            }

            if (item.MontantPaye <= 0)
            {
                return await PersistRejectAsync(
                    idEcole, clientRequestId, item.DeviceId, idUtilisateur,
                    "INVALID_AMOUNT", "montantPaye doit être > 0.",
                    cancellationToken);
            }

            if (item.DatePaiementUtc == default)
            {
                return await PersistRejectAsync(
                    idEcole, clientRequestId, item.DeviceId, idUtilisateur,
                    "INVALID_DATE", "datePaiementUtc est obligatoire.",
                    cancellationToken);
            }

            if (!SyncCashPaymentMethods.TryNormalize(
                    item.MethodePaiement,
                    out var modeNormalise,
                    out var methodError,
                    out var methodMessage))
            {
                return await PersistRejectAsync(
                    idEcole, clientRequestId, item.DeviceId, idUtilisateur,
                    methodError!, methodMessage!,
                    cancellationToken);
            }

            if (annee == null)
            {
                return await PersistRejectAsync(
                    idEcole, clientRequestId, item.DeviceId, idUtilisateur,
                    "NO_SCHOOL_YEAR", "Aucune année scolaire courante pour cette école.",
                    cancellationToken);
            }

            try
            {
                var inscription = await _context.Inscriptions.AsNoTracking()
                    .Include(i => i.Classe)
                    .Where(i =>
                        i.Statut == true
                        && i.IdEleve == item.IdEleve
                        && i.IdEcole == idEcole
                        && i.IdAnneeScolaire == annee.IdAnneeScolaire
                        && i.StatutInscription != null
                        && (i.StatutInscription == InscriptionActiveRules.StatutConfirme
                            || i.StatutInscription == "Confirme"
                            || i.StatutInscription.StartsWith("Confirm")))
                    .OrderByDescending(i => i.DateInscription)
                    .FirstOrDefaultAsync(cancellationToken);

                if (inscription == null || inscription.Classe?.IdDirection == null)
                {
                    return await PersistRejectAsync(
                        idEcole, clientRequestId, item.DeviceId, idUtilisateur,
                        "ELEVE_NOT_IN_SCHOOL",
                        "Élève sans inscription confirmée dans l'école / année courante.",
                        cancellationToken);
                }

                var frais = await _context.Frais.AsNoTracking()
                    .Include(f => f.FraisClasses)
                    .Include(f => f.FraisDirections)
                    .FirstOrDefaultAsync(f => f.IdFrais == item.IdFrais, cancellationToken);

                if (frais == null || frais.Statut != true || frais.IdEcole != idEcole)
                {
                    return await PersistRejectAsync(
                        idEcole, clientRequestId, item.DeviceId, idUtilisateur,
                        "FRAIS_NOT_FOUND", "Frais introuvable pour cette école.",
                        cancellationToken);
                }

                if (!FraisEligibility.IsEligibleForInscription(
                        frais,
                        idEcole,
                        inscription.Classe.IdDirection.Value,
                        annee.IdAnneeScolaire,
                        inscription.IdClasse))
                {
                    return await PersistRejectAsync(
                        idEcole, clientRequestId, item.DeviceId, idUtilisateur,
                        "FRAIS_NOT_ELIGIBLE",
                        "Ce frais n'est pas éligible pour la classe de l'élève (année courante).",
                        cancellationToken);
                }

                var codeDevise = string.IsNullOrWhiteSpace(item.CodeDevisePaiement)
                    ? frais.Devise
                    : item.CodeDevisePaiement.Trim().ToUpperInvariant();

                var datePaiement = item.DatePaiementUtc.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(item.DatePaiementUtc, DateTimeKind.Utc).ToLocalTime()
                    : item.DatePaiementUtc.ToLocalTime();

                var paiement = new Paiement
                {
                    IdEleve = item.IdEleve,
                    IdFrais = item.IdFrais,
                    Montant = (double)item.MontantPaye,
                    Devise = codeDevise,
                    CodeDevisePaiement = codeDevise,
                    ModePaiement = modeNormalise,
                    Statut = true,
                    StatutPaiement = "Confirmé",
                    DatePaiement = datePaiement,
                    DateCreation = DateTime.UtcNow,
                    ReferenceTransaction = item.ReferenceTransaction,
                    Commentaire = string.IsNullOrWhiteSpace(item.Commentaire)
                        ? $"Sync offline ({item.DeviceId ?? "device"})"
                        : item.Commentaire,
                    IdUtilisateur = idUtilisateur
                };

                var created = await _paiementRepository.CreateAsync(paiement);

                var register = await _idempotency.TryRegisterCreatedAsync(
                    idEcole,
                    clientRequestId,
                    SyncResourceType.Payment,
                    created.IdPaiement,
                    idUtilisateur,
                    item.DeviceId,
                    message: "Paiement créé (sync offline).",
                    cancellationToken: cancellationToken);

                if (register.IsDuplicate)
                {
                    return new PaymentBatchItemResultDto
                    {
                        ClientRequestId = clientRequestId,
                        Status = SyncStatus.Duplicate,
                        IdPaiement = register.Record.ResourceId,
                        NewMontantDu = await ComputeMontantDuAsync(item.IdEleve, item.IdFrais, cancellationToken),
                        Message = register.Record.Message ?? "Requête déjà traitée (idempotence).",
                        ErrorCode = register.Record.ErrorCode
                    };
                }

                return new PaymentBatchItemResultDto
                {
                    ClientRequestId = clientRequestId,
                    Status = SyncStatus.Created,
                    IdPaiement = created.IdPaiement,
                    NewMontantDu = await ComputeMontantDuAsync(item.IdEleve, item.IdFrais, cancellationToken),
                    Message = "Paiement créé (sync offline)."
                };
            }
            catch (InvalidOperationException ex)
            {
                // Typiquement ValiderCreationManuelle / conversion devise
                var code = ex.Message.Contains("MokoAfrika", StringComparison.OrdinalIgnoreCase)
                    ? SyncCashPaymentMethods.ErrorMokoNotAllowed
                    : "BUSINESS_RULE";
                return await PersistRejectAsync(
                    idEcole, clientRequestId, item.DeviceId, idUtilisateur,
                    code, ex.Message,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erreur sync payment batch école {IdEcole} clientRequestId={ClientRequestId}",
                    idEcole, clientRequestId);
                return new PaymentBatchItemResultDto
                {
                    ClientRequestId = clientRequestId,
                    Status = SyncStatus.Error,
                    Message = "Erreur technique — conserver l'item en file et réessayer.",
                    ErrorCode = "TECHNICAL_ERROR"
                };
            }
        }

        private async Task<PaymentBatchItemResultDto> PersistRejectAsync(
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
                SyncResourceType.Payment,
                message,
                errorCode,
                idUtilisateur,
                deviceId,
                cancellationToken);

            return new PaymentBatchItemResultDto
            {
                ClientRequestId = clientRequestId,
                Status = outcome.IsDuplicate ? SyncStatus.Duplicate : SyncStatus.Rejected,
                IdPaiement = outcome.Record.ResourceId,
                Message = outcome.IsDuplicate
                    ? (outcome.Record.Message ?? message)
                    : message,
                ErrorCode = outcome.IsDuplicate
                    ? (outcome.Record.ErrorCode ?? errorCode)
                    : errorCode
            };
        }

        private static PaymentBatchItemResultDto Reject(string clientRequestId, string code, string message)
            => new()
            {
                ClientRequestId = clientRequestId,
                Status = SyncStatus.Rejected,
                ErrorCode = code,
                Message = message
            };

        private async Task<decimal?> ComputeMontantDuAsync(
            int idEleve,
            int idFrais,
            CancellationToken cancellationToken)
        {
            var duEffectif = await _fraisDuCalculator.GetMontantDuEffectifAsync(
                idEleve, idFrais, cancellationToken: cancellationToken);

            var fraisExists = await _context.Frais.AsNoTracking()
                .AnyAsync(f => f.IdFrais == idFrais, cancellationToken);
            if (!fraisExists)
                return null;

            var paye = await _context.Paiements.AsNoTracking()
                .Where(p => p.IdEleve == idEleve
                            && p.IdFrais == idFrais
                            && p.Statut == true
                            && p.StatutPaiement != null
                            && (p.StatutPaiement == "Confirmé"
                                || p.StatutPaiement == "Confirme"
                                || p.StatutPaiement.StartsWith("Confirm")))
                .SumAsync(p => (decimal)p.Montant, cancellationToken);

            return Math.Max(0m, duEffectif - paye);
        }
    }
}
