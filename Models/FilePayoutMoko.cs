using KelasiNaBiso.Models.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    /// <summary>
    /// File d'attente des reversements PayOut vers le bénéficiaire école.
    /// </summary>
    public class FilePayoutMoko
    {
        [Key]
        public int IdFilePayoutMoko { get; set; }

        [Required]
        [MaxLength(50)]
        public string PayInReference { get; set; } = string.Empty;

        public int? IdTransactionMokoPayIn { get; set; }

        public int? IdTransactionMokoPayOut { get; set; }

        [Required]
        public int IdEcole { get; set; }

        public int? IdPaiement { get; set; }

        public decimal MontantNet { get; set; }

        [Required]
        [MaxLength(10)]
        public string Devise { get; set; } = "CDF";

        [MaxLength(20)]
        public string? Methode { get; set; }

        [MaxLength(20)]
        public string? NumeroBeneficiaire { get; set; }

        public DateTime ScheduledAt { get; set; }

        [MaxLength(50)]
        public string? PayOutReference { get; set; }

        [MaxLength(30)]
        public string Status { get; set; } = MokoPayoutQueueStatuses.Pending;

        public int RetryCount { get; set; }

        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }

        [JsonIgnore]
        public DateTime DateCreation { get; set; } = DateTime.Now;

        [JsonIgnore]
        public DateTime? DateModification { get; set; }

        [JsonIgnore]
        public DateTime? DateTraitement { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public TransactionMoko? TransactionMokoPayIn { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public TransactionMoko? TransactionMokoPayOut { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public Ecole? Ecole { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public Paiement? Paiement { get; set; }
    }
}
