using KelasiNaBiso.Models.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    /// <summary>
    /// Mouvement comptable du wallet virtuel école (journal de suivi transaction).
    /// </summary>
    public class EcoleWalletMouvement
    {
        [Key]
        public int IdEcoleWalletMouvement { get; set; }

        [Required]
        public int IdEcoleWallet { get; set; }

        [Required]
        public int IdEcole { get; set; }

        public int? IdTransactionMoko { get; set; }

        public int? IdPaiement { get; set; }

        [Required]
        [MaxLength(50)]
        public string TypeMouvement { get; set; } = WalletMouvementTypes.PayInCreditPending;

        public decimal Montant { get; set; }

        public decimal SoldeEnAttenteApres { get; set; }

        public decimal SoldeDisponibleApres { get; set; }

        [MaxLength(100)]
        public string? Reference { get; set; }

        [MaxLength(500)]
        public string? Commentaire { get; set; }

        [JsonIgnore]
        public DateTime DateCreation { get; set; } = DateTime.Now;

        [JsonIgnore]
        [ValidateNever]
        public EcoleWallet? Wallet { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public TransactionMoko? TransactionMoko { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public Paiement? Paiement { get; set; }
    }
}
