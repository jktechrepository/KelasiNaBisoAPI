using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    /// <summary>
    /// Trace audit de chaque appel gateway MOKO (PayIn, PayOut, Check).
    /// </summary>
    public class TransactionMoko
    {
        [Key]
        public int IdTransactionMoko { get; set; }

        [Required]
        [MaxLength(50)]
        public string Reference { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? ParentReference { get; set; }

        public int? IdPaiement { get; set; }

        [Required]
        public int IdEcole { get; set; }

        /// <summary>debit | credit | check</summary>
        [Required]
        [MaxLength(10)]
        public string Action { get; set; } = string.Empty;

        /// <summary>Montant envoyé au gateway.</summary>
        public decimal Amount { get; set; }

        /// <summary>Montant net métier (frais scolaires).</summary>
        public decimal? AmountNet { get; set; }

        public decimal? FraisCollecte { get; set; }

        public decimal? FraisDecaissement { get; set; }

        [Required]
        [MaxLength(10)]
        public string Devise { get; set; } = "CDF";

        [MaxLength(20)]
        public string? CustomerPhone { get; set; }

        [MaxLength(20)]
        public string? Method { get; set; }

        [MaxLength(30)]
        public string Status { get; set; } = "pending";

        [MaxLength(100)]
        public string? GatewayTransactionId { get; set; }

        [MaxLength(500)]
        public string? StatusDescription { get; set; }

        public string? RawRequest { get; set; }

        public string? RawResponse { get; set; }

        public string? RawCallback { get; set; }

        [JsonIgnore]
        public DateTime DateCreation { get; set; } = DateTime.Now;

        [JsonIgnore]
        public DateTime? DateModification { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public Paiement? Paiement { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public Ecole? Ecole { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<FilePayoutMoko> FilePayouts { get; set; } = new List<FilePayoutMoko>();
    }
}
