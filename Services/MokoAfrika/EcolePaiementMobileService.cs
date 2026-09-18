using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.MokoAfrika;
using KelasiNaBiso.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services.MokoAfrika
{
    public interface IEcolePaiementMobileService
    {
        Task<EcolePaiementMobileOverviewDto> GetOverviewAsync(int idEcole);
        Task<EcoleInfoPaiementMobileDto?> GetByEcoleAsync(int idEcole);
        Task<EcoleInfoPaiementMobileDto> CreateOrUpdateAsync(int idEcole, CreateEcoleInfoPaiementMobileDto dto);
        Task<EcoleInfoPaiementMobileDto> UpdateAsync(int idEcole, UpdateEcoleInfoPaiementMobileDto dto);
        Task<EcoleBeneficiaireMomoDto> AddBeneficiaireAsync(int idEcole, CreateEcoleBeneficiaireMomoDto dto);
        Task<EcoleBeneficiaireMomoDto?> UpdateBeneficiaireAsync(int idEcole, int idBeneficiaire, UpdateEcoleBeneficiaireMomoDto dto);
        Task<bool> DeleteBeneficiaireAsync(int idEcole, int idBeneficiaire);
        Task<EcoleBeneficiaireMomo?> GetBeneficiaireActifAsync(int idEcole, string methode);
        Task<List<EcoleWalletMouvementDto>> GetWalletMouvementsAsync(int idEcole, int limit = 50);
        Task<List<TransactionMokoListItemDto>> GetTransactionsAsync(int idEcole, string? status, string? action, int limit = 50, int offset = 0);
        Task<List<FilePayoutMokoDto>> GetPayoutsAsync(int idEcole, string? status, int limit = 50);
    }

    public class EcolePaiementMobileService : IEcolePaiementMobileService
    {
        private readonly KelasiNaBisoDbContext _context;

        public EcolePaiementMobileService(KelasiNaBisoDbContext context)
        {
            _context = context;
        }

        public async Task<EcolePaiementMobileOverviewDto> GetOverviewAsync(int idEcole)
        {
            var config = await GetByEcoleAsync(idEcole);
            var stats = await BuildWalletStatsAsync(idEcole, config?.Wallet);

            return new EcolePaiementMobileOverviewDto
            {
                IdEcole = idEcole,
                EstConfigure = config != null,
                Configuration = config,
                Stats = stats
            };
        }

        public async Task<EcoleInfoPaiementMobileDto?> GetByEcoleAsync(int idEcole)
        {
            var info = await _context.EcolesInfoPaiementMobile
                .Include(i => i.Beneficiaires)
                .Include(i => i.Wallet)
                .FirstOrDefaultAsync(i => i.IdEcole == idEcole);

            return info == null ? null : MapToDto(info);
        }

        public async Task<EcoleInfoPaiementMobileDto> CreateOrUpdateAsync(int idEcole, CreateEcoleInfoPaiementMobileDto dto)
        {
            var ecoleExists = await _context.Ecoles.AnyAsync(e => e.IdEcole == idEcole);
            if (!ecoleExists)
                throw new KeyNotFoundException($"École {idEcole} introuvable.");

            var info = await _context.EcolesInfoPaiementMobile
                .Include(i => i.Wallet)
                .Include(i => i.Beneficiaires)
                .FirstOrDefaultAsync(i => i.IdEcole == idEcole);

            if (info == null)
            {
                info = new EcoleInfoPaiementMobile
                {
                    IdEcole = idEcole,
                    MobileMoneyActif = dto.MobileMoneyActif,
                    CarteActif = dto.CarteActif,
                    Devise = dto.Devise,
                    DelaiReglementMinutes = dto.DelaiReglementMinutes,
                    PayoutAutomatique = dto.PayoutAutomatique,
                    DateCreation = DateTime.Now
                };
                _context.EcolesInfoPaiementMobile.Add(info);
                await _context.SaveChangesAsync();

                var wallet = new EcoleWallet
                {
                    IdEcole = idEcole,
                    IdEcoleInfoPaiementMobile = info.IdEcoleInfoPaiementMobile,
                    Devise = dto.Devise,
                    DateCreation = DateTime.Now
                };
                _context.EcolesWallets.Add(wallet);
                await _context.SaveChangesAsync();
            }
            else
            {
                info.MobileMoneyActif = dto.MobileMoneyActif;
                info.CarteActif = dto.CarteActif;
                info.Devise = dto.Devise;
                info.DelaiReglementMinutes = dto.DelaiReglementMinutes;
                info.PayoutAutomatique = dto.PayoutAutomatique;
                info.DateModification = DateTime.Now;

                var wallet = await _context.EcolesWallets.FirstOrDefaultAsync(w => w.IdEcole == idEcole);
                if (wallet != null && !string.Equals(wallet.Devise, dto.Devise, StringComparison.OrdinalIgnoreCase))
                {
                    wallet.Devise = dto.Devise;
                    wallet.DateModification = DateTime.Now;
                }

                await _context.SaveChangesAsync();
            }

            return (await GetByEcoleAsync(idEcole))!;
        }

        public async Task<EcoleInfoPaiementMobileDto> UpdateAsync(int idEcole, UpdateEcoleInfoPaiementMobileDto dto)
        {
            var info = await _context.EcolesInfoPaiementMobile.FirstOrDefaultAsync(i => i.IdEcole == idEcole)
                ?? throw new KeyNotFoundException($"Configuration paiement mobile introuvable pour l'école {idEcole}.");

            if (dto.MobileMoneyActif.HasValue) info.MobileMoneyActif = dto.MobileMoneyActif.Value;
            if (dto.CarteActif.HasValue) info.CarteActif = dto.CarteActif.Value;
            if (!string.IsNullOrWhiteSpace(dto.Devise))
            {
                info.Devise = dto.Devise;
                var wallet = await _context.EcolesWallets.FirstOrDefaultAsync(w => w.IdEcole == idEcole);
                if (wallet != null && !string.Equals(wallet.Devise, dto.Devise, StringComparison.OrdinalIgnoreCase))
                {
                    wallet.Devise = dto.Devise;
                    wallet.DateModification = DateTime.Now;
                }
            }
            if (dto.DelaiReglementMinutes.HasValue) info.DelaiReglementMinutes = dto.DelaiReglementMinutes.Value;
            if (dto.PayoutAutomatique.HasValue) info.PayoutAutomatique = dto.PayoutAutomatique.Value;
            if (dto.Statut.HasValue) info.Statut = dto.Statut.Value;
            info.DateModification = DateTime.Now;

            await _context.SaveChangesAsync();
            return (await GetByEcoleAsync(idEcole))!;
        }

        public async Task<EcoleBeneficiaireMomoDto> AddBeneficiaireAsync(int idEcole, CreateEcoleBeneficiaireMomoDto dto)
        {
            var info = await _context.EcolesInfoPaiementMobile.FirstOrDefaultAsync(i => i.IdEcole == idEcole)
                ?? throw new KeyNotFoundException($"Configuration paiement mobile introuvable pour l'école {idEcole}.");

            var methode = dto.Methode.ToLowerInvariant();
            if (!MomoOperators.All.Contains(methode))
                throw new ArgumentException($"Méthode invalide : {dto.Methode}");

            if (dto.EstPrincipal)
            {
                var existants = await _context.EcolesBeneficiairesMomo
                    .Where(b => b.IdEcole == idEcole && b.Methode == methode && b.EstPrincipal)
                    .ToListAsync();
                foreach (var b in existants)
                    b.EstPrincipal = false;
            }

            var beneficiaire = new EcoleBeneficiaireMomo
            {
                IdEcole = idEcole,
                IdEcoleInfoPaiementMobile = info.IdEcoleInfoPaiementMobile,
                Methode = methode,
                Numero = NormaliserTelephone(dto.Numero),
                NomTitulaire = dto.NomTitulaire,
                EstPrincipal = dto.EstPrincipal,
                Statut = EcoleBeneficiaireStatuts.Actif,
                DateCreation = DateTime.Now
            };

            _context.EcolesBeneficiairesMomo.Add(beneficiaire);
            await _context.SaveChangesAsync();

            return MapBeneficiaire(beneficiaire);
        }

        public async Task<EcoleBeneficiaireMomoDto?> UpdateBeneficiaireAsync(
            int idEcole,
            int idBeneficiaire,
            UpdateEcoleBeneficiaireMomoDto dto)
        {
            var beneficiaire = await _context.EcolesBeneficiairesMomo
                .FirstOrDefaultAsync(b => b.IdEcoleBeneficiaireMomo == idBeneficiaire && b.IdEcole == idEcole);

            if (beneficiaire == null)
                return null;

            if (!string.IsNullOrWhiteSpace(dto.Methode))
            {
                var methode = dto.Methode.ToLowerInvariant();
                if (!MomoOperators.All.Contains(methode))
                    throw new ArgumentException($"Méthode invalide : {dto.Methode}");
                beneficiaire.Methode = methode;
            }
            if (!string.IsNullOrWhiteSpace(dto.Numero))
                beneficiaire.Numero = NormaliserTelephone(dto.Numero);
            if (dto.NomTitulaire != null)
                beneficiaire.NomTitulaire = dto.NomTitulaire;
            if (dto.Statut != null)
                beneficiaire.Statut = dto.Statut;
            if (dto.EstPrincipal == true)
            {
                var autres = await _context.EcolesBeneficiairesMomo
                    .Where(b => b.IdEcole == idEcole && b.Methode == beneficiaire.Methode && b.IdEcoleBeneficiaireMomo != idBeneficiaire)
                    .ToListAsync();
                foreach (var b in autres)
                    b.EstPrincipal = false;
                beneficiaire.EstPrincipal = true;
            }
            else if (dto.EstPrincipal == false)
            {
                beneficiaire.EstPrincipal = false;
            }

            beneficiaire.DateModification = DateTime.Now;
            await _context.SaveChangesAsync();
            return MapBeneficiaire(beneficiaire);
        }

        public async Task<bool> DeleteBeneficiaireAsync(int idEcole, int idBeneficiaire)
        {
            var beneficiaire = await _context.EcolesBeneficiairesMomo
                .FirstOrDefaultAsync(b => b.IdEcoleBeneficiaireMomo == idBeneficiaire && b.IdEcole == idEcole);

            if (beneficiaire == null)
                return false;

            beneficiaire.Statut = EcoleBeneficiaireStatuts.Inactif;
            beneficiaire.DateModification = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<EcoleBeneficiaireMomo?> GetBeneficiaireActifAsync(int idEcole, string methode)
        {
            methode = methode.ToLowerInvariant();
            return await _context.EcolesBeneficiairesMomo
                .Where(b => b.IdEcole == idEcole
                    && b.Methode == methode
                    && b.Statut == EcoleBeneficiaireStatuts.Actif)
                .OrderByDescending(b => b.EstPrincipal)
                .FirstOrDefaultAsync();
        }

        public async Task<List<EcoleWalletMouvementDto>> GetWalletMouvementsAsync(int idEcole, int limit = 50)
        {
            return await _context.EcolesWalletMouvements
                .Where(m => m.IdEcole == idEcole)
                .OrderByDescending(m => m.DateCreation)
                .Take(limit)
                .Select(m => new EcoleWalletMouvementDto
                {
                    IdEcoleWalletMouvement = m.IdEcoleWalletMouvement,
                    TypeMouvement = m.TypeMouvement,
                    Montant = m.Montant,
                    SoldeEnAttenteApres = m.SoldeEnAttenteApres,
                    SoldeDisponibleApres = m.SoldeDisponibleApres,
                    Reference = m.Reference,
                    Commentaire = m.Commentaire,
                    DateCreation = m.DateCreation,
                    IdPaiement = m.IdPaiement,
                    IdTransactionMoko = m.IdTransactionMoko
                })
                .ToListAsync();
        }

        public async Task<List<TransactionMokoListItemDto>> GetTransactionsAsync(
            int idEcole,
            string? status,
            string? action,
            int limit = 50,
            int offset = 0)
        {
            var query = _context.TransactionsMoko
                .AsNoTracking()
                .Where(t => t.IdEcole == idEcole);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(t => t.Status == status.Trim().ToLowerInvariant());

            if (!string.IsNullOrWhiteSpace(action))
                query = query.Where(t => t.Action == action.Trim().ToLowerInvariant());

            var rows = await query
                .Include(t => t.Paiement!)
                    .ThenInclude(p => p.Eleve)
                .Include(t => t.Paiement!)
                    .ThenInclude(p => p.Frais)
                .OrderByDescending(t => t.DateCreation)
                .Skip(Math.Max(0, offset))
                .Take(Math.Clamp(limit, 1, 200))
                .ToListAsync();

            return rows.Select(t => new TransactionMokoListItemDto
            {
                IdTransactionMoko = t.IdTransactionMoko,
                Reference = t.Reference,
                ParentReference = t.ParentReference,
                IdPaiement = t.IdPaiement,
                IdEcole = t.IdEcole,
                Action = t.Action,
                Amount = t.Amount,
                AmountNet = t.AmountNet,
                Devise = t.Devise,
                Method = t.Method,
                Status = t.Status,
                GatewayTransactionId = t.GatewayTransactionId,
                DateCreation = t.DateCreation,
                StatusDescription = t.StatusDescription,
                CustomerPhone = t.CustomerPhone,
                StatutPaiement = t.Paiement?.StatutPaiement,
                NomEleve = t.Paiement?.Eleve != null
                    ? $"{t.Paiement.Eleve.Nom} {t.Paiement.Eleve.Prenom}".Trim()
                    : null,
                LibelleFrais = t.Paiement?.Frais?.LibelleFrais
            }).ToList();
        }

        public async Task<List<FilePayoutMokoDto>> GetPayoutsAsync(int idEcole, string? status, int limit = 50)
        {
            var query = _context.FilePayoutsMoko
                .AsNoTracking()
                .Where(f => f.IdEcole == idEcole);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(f => f.Status == status.Trim().ToLowerInvariant());

            return await query
                .OrderByDescending(f => f.DateCreation)
                .Take(Math.Clamp(limit, 1, 200))
                .Select(f => new FilePayoutMokoDto
                {
                    IdFilePayoutMoko = f.IdFilePayoutMoko,
                    PayInReference = f.PayInReference,
                    PayOutReference = f.PayOutReference,
                    IdEcole = f.IdEcole,
                    IdPaiement = f.IdPaiement,
                    MontantNet = f.MontantNet,
                    Devise = f.Devise,
                    Methode = f.Methode,
                    NumeroBeneficiaire = f.NumeroBeneficiaire,
                    ScheduledAt = f.ScheduledAt,
                    Status = f.Status,
                    RetryCount = f.RetryCount,
                    ErrorMessage = f.ErrorMessage,
                    DateCreation = f.DateCreation,
                    DateTraitement = f.DateTraitement
                })
                .ToListAsync();
        }

        private async Task<EcoleWalletStatsDto> BuildWalletStatsAsync(int idEcole, EcoleWalletDto? wallet)
        {
            var txs = await _context.TransactionsMoko
                .AsNoTracking()
                .Where(t => t.IdEcole == idEcole)
                .GroupBy(t => new { t.Action, t.Status })
                .Select(g => new { g.Key.Action, g.Key.Status, Count = g.Count() })
                .ToListAsync();

            var payouts = await _context.FilePayoutsMoko
                .AsNoTracking()
                .Where(f => f.IdEcole == idEcole)
                .GroupBy(f => f.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            int CountTx(string action, string status) =>
                txs.FirstOrDefault(x => x.Action == action && x.Status == status)?.Count ?? 0;

            int CountPayout(string status) =>
                payouts.FirstOrDefault(x => x.Status == status)?.Count ?? 0;

            return new EcoleWalletStatsDto
            {
                SoldeEnAttente = wallet?.SoldeEnAttente ?? 0,
                SoldeDisponible = wallet?.SoldeDisponible ?? 0,
                TotalRecu = wallet?.TotalRecu ?? 0,
                TotalReverse = wallet?.TotalReverse ?? 0,
                Devise = wallet?.Devise ?? "CDF",
                PayInsReussis = CountTx(MokoActions.Debit, MokoTransactionStatuses.Success),
                PayInsEnAttente = CountTx(MokoActions.Debit, MokoTransactionStatuses.Pending),
                PayInsEchoues = CountTx(MokoActions.Debit, MokoTransactionStatuses.Error)
                    + CountTx(MokoActions.Debit, MokoTransactionStatuses.Timeout),
                PayOutsReussis = CountTx(MokoActions.Credit, MokoTransactionStatuses.Success),
                PayOutsEnAttente = CountPayout(MokoPayoutQueueStatuses.Pending)
                    + CountPayout(MokoPayoutQueueStatuses.Processing),
                PayOutsEchoues = CountPayout(MokoPayoutQueueStatuses.Failed)
            };
        }

        private static EcoleInfoPaiementMobileDto MapToDto(EcoleInfoPaiementMobile info) => new()
        {
            IdEcoleInfoPaiementMobile = info.IdEcoleInfoPaiementMobile,
            IdEcole = info.IdEcole,
            MobileMoneyActif = info.MobileMoneyActif,
            CarteActif = info.CarteActif,
            Devise = info.Devise,
            DelaiReglementMinutes = info.DelaiReglementMinutes,
            PayoutAutomatique = info.PayoutAutomatique,
            Statut = info.Statut,
            Wallet = info.Wallet == null ? null : new EcoleWalletDto
            {
                IdEcoleWallet = info.Wallet.IdEcoleWallet,
                IdEcole = info.Wallet.IdEcole,
                Devise = info.Wallet.Devise,
                SoldeEnAttente = info.Wallet.SoldeEnAttente,
                SoldeDisponible = info.Wallet.SoldeDisponible,
                TotalRecu = info.Wallet.TotalRecu,
                TotalReverse = info.Wallet.TotalReverse
            },
            Beneficiaires = info.Beneficiaires
                .Where(b => b.Statut == EcoleBeneficiaireStatuts.Actif)
                .Select(MapBeneficiaire)
                .ToList()
        };

        private static EcoleBeneficiaireMomoDto MapBeneficiaire(EcoleBeneficiaireMomo b) => new()
        {
            IdEcoleBeneficiaireMomo = b.IdEcoleBeneficiaireMomo,
            IdEcole = b.IdEcole,
            Methode = b.Methode,
            Numero = b.Numero,
            NomTitulaire = b.NomTitulaire,
            EstPrincipal = b.EstPrincipal,
            Statut = b.Statut
        };

        private static string NormaliserTelephone(string numero) =>
            numero.Trim().Replace(" ", "").Replace("-", "");
    }
}
