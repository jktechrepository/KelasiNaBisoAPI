using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services.MokoAfrika
{
    public record WalletCreditResult(
        decimal MontantWallet,
        string DeviseWallet,
        decimal MontantSource,
        string CodeDeviseSource,
        decimal Taux);

    public interface IMokoWalletService
    {
        Task<EcoleWallet> GetOrCreateWalletAsync(int idEcole, CancellationToken cancellationToken = default);
        /// <summary>
        /// Crédite le wallet en devise wallet : convertit <paramref name="montantNet"/> depuis
        /// <paramref name="codeDeviseSource"/> si différent de <c>wallet.Devise</c>.
        /// </summary>
        Task<WalletCreditResult> CrediterApresPayInAsync(
            int idEcole,
            int idPaiement,
            int idTransactionMoko,
            decimal montantNet,
            string reference,
            string? codeDeviseSource,
            CancellationToken cancellationToken = default);
        /// <summary>Convertit un montant gateway vers la devise du wallet (sans créditer).</summary>
        Task<(decimal MontantWallet, string DeviseWallet)> ConvertirVersDeviseWalletAsync(
            int idEcole,
            decimal montantSource,
            string? codeDeviseSource,
            CancellationToken cancellationToken = default);
        Task LibererSoldeEnAttenteAsync(int idEcole, int idTransactionMoko, decimal montantNet, string reference, CancellationToken cancellationToken = default);
        Task DebiterPourPayOutAsync(int idEcole, int idTransactionMoko, int? idPaiement, decimal montantNet, string reference, CancellationToken cancellationToken = default);
        Task RecrediterPayOutEchoueAsync(int idEcole, int idTransactionMoko, decimal montantNet, string reference, CancellationToken cancellationToken = default);
    }

    public class MokoWalletService : IMokoWalletService
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly ICurrencyConversionService _currencyConversion;
        private readonly ILogger<MokoWalletService> _logger;

        public MokoWalletService(
            KelasiNaBisoDbContext context,
            ICurrencyConversionService currencyConversion,
            ILogger<MokoWalletService> logger)
        {
            _context = context;
            _currencyConversion = currencyConversion;
            _logger = logger;
        }

        public async Task<EcoleWallet> GetOrCreateWalletAsync(int idEcole, CancellationToken cancellationToken = default)
        {
            var wallet = await _context.EcolesWallets.FirstOrDefaultAsync(w => w.IdEcole == idEcole, cancellationToken);
            if (wallet != null)
                return wallet;

            var info = await _context.EcolesInfoPaiementMobile.FirstOrDefaultAsync(i => i.IdEcole == idEcole, cancellationToken)
                ?? throw new InvalidOperationException($"Configuration paiement mobile absente pour l'école {idEcole}.");

            wallet = new EcoleWallet
            {
                IdEcole = idEcole,
                IdEcoleInfoPaiementMobile = info.IdEcoleInfoPaiementMobile,
                Devise = info.Devise,
                DateCreation = DateTime.Now
            };
            _context.EcolesWallets.Add(wallet);
            await _context.SaveChangesAsync(cancellationToken);
            return wallet;
        }

        public async Task<(decimal MontantWallet, string DeviseWallet)> ConvertirVersDeviseWalletAsync(
            int idEcole,
            decimal montantSource,
            string? codeDeviseSource,
            CancellationToken cancellationToken = default)
        {
            var wallet = await GetOrCreateWalletAsync(idEcole, cancellationToken);
            var deviseWallet = NormalizeDevise(wallet.Devise) ?? "CDF";
            var source = NormalizeDevise(codeDeviseSource) ?? deviseWallet;

            if (source == deviseWallet)
                return (montantSource, deviseWallet);

            var result = await _currencyConversion.ConvertAsync(
                idEcole,
                source,
                deviseWallet,
                montantSource,
                DateTime.UtcNow,
                cancellationToken);

            if (!result.Success)
            {
                throw new InvalidOperationException(
                    result.ErrorMessage
                    ?? $"Impossible de convertir {source} → {deviseWallet} pour le crédit wallet école {idEcole}.");
            }

            return (result.MontantConverti, deviseWallet);
        }

        public async Task<WalletCreditResult> CrediterApresPayInAsync(
            int idEcole,
            int idPaiement,
            int idTransactionMoko,
            decimal montantNet,
            string reference,
            string? codeDeviseSource,
            CancellationToken cancellationToken = default)
        {
            var wallet = await GetOrCreateWalletAsync(idEcole, cancellationToken);
            var deviseWallet = NormalizeDevise(wallet.Devise) ?? "CDF";
            var source = NormalizeDevise(codeDeviseSource) ?? deviseWallet;

            decimal montantWallet = montantNet;
            decimal taux = 1m;

            if (source != deviseWallet)
            {
                var result = await _currencyConversion.ConvertAsync(
                    idEcole,
                    source,
                    deviseWallet,
                    montantNet,
                    DateTime.UtcNow,
                    cancellationToken);

                if (!result.Success)
                {
                    throw new InvalidOperationException(
                        result.ErrorMessage
                        ?? $"Impossible de convertir {source} → {deviseWallet} pour le crédit wallet école {idEcole}.");
                }

                montantWallet = result.MontantConverti;
                taux = result.Taux;
            }

            wallet.SoldeEnAttente += montantWallet;
            wallet.TotalRecu += montantWallet;
            wallet.DateModification = DateTime.Now;

            var commentaire = source == deviseWallet
                ? "Crédit wallet après PayIn réussi (en attente de libération)"
                : $"Crédit wallet après PayIn réussi ({montantNet} {source} → {montantWallet} {deviseWallet}, taux {taux})";

            _context.EcolesWalletMouvements.Add(new EcoleWalletMouvement
            {
                IdEcoleWallet = wallet.IdEcoleWallet,
                IdEcole = idEcole,
                IdTransactionMoko = idTransactionMoko,
                IdPaiement = idPaiement,
                TypeMouvement = WalletMouvementTypes.PayInCreditPending,
                Montant = montantWallet,
                SoldeEnAttenteApres = wallet.SoldeEnAttente,
                SoldeDisponibleApres = wallet.SoldeDisponible,
                Reference = reference,
                Commentaire = commentaire,
                DateCreation = DateTime.Now
            });

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "Wallet école {IdEcole} crédité {Montant} {Devise} en attente (source {SourceMontant} {SourceDevise}, ref {Reference})",
                idEcole, montantWallet, deviseWallet, montantNet, source, reference);

            return new WalletCreditResult(montantWallet, deviseWallet, montantNet, source, taux);
        }

        public async Task LibererSoldeEnAttenteAsync(
            int idEcole,
            int idTransactionMoko,
            decimal montantNet,
            string reference,
            CancellationToken cancellationToken = default)
        {
            var wallet = await GetOrCreateWalletAsync(idEcole, cancellationToken);
            if (wallet.SoldeEnAttente < montantNet)
                throw new InvalidOperationException($"Solde en attente insuffisant pour l'école {idEcole}.");

            wallet.SoldeEnAttente -= montantNet;
            wallet.SoldeDisponible += montantNet;
            wallet.DateModification = DateTime.Now;

            _context.EcolesWalletMouvements.Add(new EcoleWalletMouvement
            {
                IdEcoleWallet = wallet.IdEcoleWallet,
                IdEcole = idEcole,
                IdTransactionMoko = idTransactionMoko,
                TypeMouvement = WalletMouvementTypes.PayInReleaseAvailable,
                Montant = montantNet,
                SoldeEnAttenteApres = wallet.SoldeEnAttente,
                SoldeDisponibleApres = wallet.SoldeDisponible,
                Reference = reference,
                Commentaire = "Libération solde après délai de règlement MOKO",
                DateCreation = DateTime.Now
            });

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DebiterPourPayOutAsync(
            int idEcole,
            int idTransactionMoko,
            int? idPaiement,
            decimal montantNet,
            string reference,
            CancellationToken cancellationToken = default)
        {
            var wallet = await GetOrCreateWalletAsync(idEcole, cancellationToken);
            if (wallet.SoldeDisponible < montantNet)
                throw new InvalidOperationException($"Solde disponible insuffisant pour PayOut école {idEcole}.");

            wallet.SoldeDisponible -= montantNet;
            wallet.TotalReverse += montantNet;
            wallet.DateModification = DateTime.Now;

            _context.EcolesWalletMouvements.Add(new EcoleWalletMouvement
            {
                IdEcoleWallet = wallet.IdEcoleWallet,
                IdEcole = idEcole,
                IdTransactionMoko = idTransactionMoko,
                IdPaiement = idPaiement,
                TypeMouvement = WalletMouvementTypes.PayOutDebit,
                Montant = montantNet,
                SoldeEnAttenteApres = wallet.SoldeEnAttente,
                SoldeDisponibleApres = wallet.SoldeDisponible,
                Reference = reference,
                Commentaire = "Débit wallet pour PayOut vers bénéficiaire école",
                DateCreation = DateTime.Now
            });

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task RecrediterPayOutEchoueAsync(
            int idEcole,
            int idTransactionMoko,
            decimal montantNet,
            string reference,
            CancellationToken cancellationToken = default)
        {
            var wallet = await GetOrCreateWalletAsync(idEcole, cancellationToken);
            wallet.SoldeDisponible += montantNet;
            wallet.TotalReverse -= montantNet;
            if (wallet.TotalReverse < 0)
                wallet.TotalReverse = 0;
            wallet.DateModification = DateTime.Now;

            _context.EcolesWalletMouvements.Add(new EcoleWalletMouvement
            {
                IdEcoleWallet = wallet.IdEcoleWallet,
                IdEcole = idEcole,
                IdTransactionMoko = idTransactionMoko,
                TypeMouvement = WalletMouvementTypes.PayOutReversal,
                Montant = montantNet,
                SoldeEnAttenteApres = wallet.SoldeEnAttente,
                SoldeDisponibleApres = wallet.SoldeDisponible,
                Reference = reference,
                Commentaire = "Recrédit wallet après échec PayOut",
                DateCreation = DateTime.Now
            });

            await _context.SaveChangesAsync(cancellationToken);
        }

        private static string? NormalizeDevise(string? code) =>
            string.IsNullOrWhiteSpace(code) ? null : code.Trim().ToUpperInvariant();
    }
}
