using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs.Sync
{
    /// <summary>Résultat d'un item de batch (présence ou paiement).</summary>
    public class SyncBatchItemResultDto
    {
        public string ClientRequestId { get; set; } = string.Empty;

        /// <summary>created | duplicate | rejected | error</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>ID ressource créée (présence / paiement).</summary>
        public int? ResourceId { get; set; }

        public string? Message { get; set; }
        public string? ErrorCode { get; set; }
    }

    public class SyncBatchSummaryDto
    {
        public int Total { get; set; }
        public int Created { get; set; }
        public int Duplicates { get; set; }
        public int Rejected { get; set; }
        public int Errors { get; set; }
    }

    public class SyncBatchResultDto
    {
        public List<SyncBatchItemResultDto> Results { get; set; } = new();
        public SyncBatchSummaryDto Summary { get; set; } = new();

        public static SyncBatchSummaryDto BuildSummary(IReadOnlyList<SyncBatchItemResultDto> results)
        {
            return new SyncBatchSummaryDto
            {
                Total = results.Count,
                Created = results.Count(r => r.Status == SyncStatus.Created),
                Duplicates = results.Count(r => r.Status == SyncStatus.Duplicate),
                Rejected = results.Count(r => r.Status == SyncStatus.Rejected),
                Errors = results.Count(r => r.Status == SyncStatus.Error)
            };
        }
    }

    /// <summary>Item générique minimal pour valider clientRequestId (tests / squelette).</summary>
    public class SyncBatchItemBaseDto
    {
        [Required]
        [MaxLength(36)]
        public string ClientRequestId { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? DeviceId { get; set; }
    }

    /// <summary>Corps POST /api/sync/payments/batch.</summary>
    public class PaymentBatchRequestDto
    {
        [Required]
        [MinLength(1)]
        public List<PaymentBatchItemDto> Items { get; set; } = new();
    }

    /// <summary>
    /// Item d'écriture offline CASH-like.
    /// Mapping portable : idEleve ↔ idClient, idFrais ↔ idFacture / poste.
    /// </summary>
    public class PaymentBatchItemDto : SyncBatchItemBaseDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int IdEleve { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int IdFrais { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal MontantPaye { get; set; }

        [Required]
        public DateTime DatePaiementUtc { get; set; }

        /// <summary>Cash / Espèces / Chèque / Virement (Moko → rejected).</summary>
        [Required]
        [MaxLength(50)]
        public string MethodePaiement { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ReferenceTransaction { get; set; }

        [MaxLength(500)]
        public string? Commentaire { get; set; }

        [MaxLength(10)]
        public string? CodeDevisePaiement { get; set; }
    }

    public class PaymentBatchItemResultDto
    {
        public string ClientRequestId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? IdPaiement { get; set; }
        public decimal? NewMontantDu { get; set; }
        public string? Message { get; set; }
        public string? ErrorCode { get; set; }
    }

    public class PaymentBatchResultDto
    {
        public List<PaymentBatchItemResultDto> Results { get; set; } = new();
        public SyncBatchSummaryDto Summary { get; set; } = new();

        public static SyncBatchSummaryDto BuildSummary(IReadOnlyList<PaymentBatchItemResultDto> results)
        {
            return new SyncBatchSummaryDto
            {
                Total = results.Count,
                Created = results.Count(r => r.Status == SyncStatus.Created),
                Duplicates = results.Count(r => r.Status == SyncStatus.Duplicate),
                Rejected = results.Count(r => r.Status == SyncStatus.Rejected),
                Errors = results.Count(r => r.Status == SyncStatus.Error)
            };
        }
    }

    /// <summary>Corps POST /api/sync/presences/batch.</summary>
    public class PresenceBatchRequestDto
    {
        [Required]
        [MinLength(1)]
        public List<PresenceBatchItemDto> Items { get; set; } = new();
    }

    /// <summary>
    /// Item pointage offline (élève XOR agent).
    /// </summary>
    public class PresenceBatchItemDto : SyncBatchItemBaseDto
    {
        public int? IdEleve { get; set; }
        public int? IdAgent { get; set; }

        /// <summary>true = présent, false = absent (défaut true).</summary>
        public bool? IsPresent { get; set; } = true;

        /// <summary>Format HH:mm ou HHhmm (ex. 07:30).</summary>
        [Required]
        [MaxLength(20)]
        public string HeureArrivee { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? HeureDepart { get; set; }

        [Required]
        public DateTime DateDuJour { get; set; }

        [MaxLength(500)]
        public string? Observation { get; set; }

        [MaxLength(50)]
        public string? Longitude { get; set; }

        [MaxLength(50)]
        public string? Latitude { get; set; }

        public int? IdVacation { get; set; }
    }

    public class PresenceBatchItemResultDto
    {
        public string ClientRequestId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? IdPresence { get; set; }
        public string? Message { get; set; }
        public string? ErrorCode { get; set; }
    }

    public class PresenceBatchResultDto
    {
        public List<PresenceBatchItemResultDto> Results { get; set; } = new();
        public SyncBatchSummaryDto Summary { get; set; } = new();

        public static SyncBatchSummaryDto BuildSummary(IReadOnlyList<PresenceBatchItemResultDto> results)
        {
            return new SyncBatchSummaryDto
            {
                Total = results.Count,
                Created = results.Count(r => r.Status == SyncStatus.Created),
                Duplicates = results.Count(r => r.Status == SyncStatus.Duplicate),
                Rejected = results.Count(r => r.Status == SyncStatus.Rejected),
                Errors = results.Count(r => r.Status == SyncStatus.Error)
            };
        }
    }

    public class SyncDeletionsDto
    {
        public string Snapshot { get; set; } = string.Empty;
        public List<int> DeletedEleveIds { get; set; } = new();
        public List<int> DeactivatedEleveIds { get; set; } = new();
        public List<int> RemovedFraisIds { get; set; } = new();
        public List<int> DeletedPresenceIds { get; set; } = new();
        public List<int> DeletedPaymentIds { get; set; } = new();
        public string NextSince { get; set; } = string.Empty;
    }
}
