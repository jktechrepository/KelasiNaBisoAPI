using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services.MokoAfrika
{
    public interface IMokoWalletService
    {
        Task<EcoleWallet> GetOrCreateWalletAsync(int idEcole, CancellationToken cancellationToken = default);
        Task CrediterApresPayInAsync(int idEcole, int idPaiement, int idTransactionMoko, decimal montantNet, string reference, CancellationToken cancellationToken = default);
        Task LibererSoldeEnAttenteAsync(int idEcole, int idTransactionMoko, decimal montantNet, string reference, CancellationToken cancellationToken = default);
        Task DebiterPourPayOutAsync(int idEcole, int idTransactionMoko, int? idPaiement, decimal montantNet, string reference, CancellationToken cancellationToken = default);
        Task RecrediterPayOutEchoueAsync(int idEcole, int idTransactionMoko, decimal montantNet, string reference, CancellationToken cancellationToken = default);
    }

    public class MokoWalletService : IMokoWalletService
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly ILogger<MokoWalletService> _logger;

        public MokoWalletService(KelasiNaBisoDbContext context, ILogger<MokoWalletService> logger)
        {
            _context = context;
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

        public async Task CrediterApresPayInAsync(
            int idEcole,
            int idPaiement,
            int idTransactionMoko,
            decimal montantNet,
            string reference,
            CancellationToken cancellationToken = default)
        {
            var wallet = await GetOrCreateWalletAsync(idEcole, cancellationToken);
            wallet.SoldeEnAttente += montantNet;
            wallet.TotalRecu += montantNet;
            wallet.DateModification = DateTime.Now;

            _context.EcolesWalletMouvements.Add(new EcoleWalletMouvement
            {
                IdEcoleWallet = wallet.IdEcoleWallet,
                IdEcole = idEcole,
                IdTransactionMoko = idTransactionMoko,
                IdPaiement = idPaiement,
                TypeMouvement = WalletMouvementTypes.PayInCreditPending,
                Montant = montantNet,
                SoldeEnAttenteApres = wallet.SoldeEnAttente,
                SoldeDisponibleApres = wallet.SoldeDisponible,
                Reference = reference,
                Commentaire = "Crédit wallet après PayIn réussi (en attente de libération)",
                DateCreation = DateTime.Now
            });

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Wallet école {IdEcole} crédité {Montant} en attente (ref {Reference})", idEcole, montantNet, reference);
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
    }
}
