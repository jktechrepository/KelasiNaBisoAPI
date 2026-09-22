using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KelasiNaBiso.Models
{
    /// <summary>
    /// Journal d'idempotence pour les écritures offline (présences, paiements CASH).
    /// Unicité : (IdEcole, ClientRequestId).
    /// </summary>
    public class SyncClientRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long IdSyncClientRequest { get; set; }

        /// <summary>Tenant école (équivalent idSociete du guide portable).</summary>
        [Required]
        public int IdEcole { get; set; }

        /// <summary>UUID stable côté client (max 36 caractères).</summary>
        [Required]
        [MaxLength(36)]
        public string ClientRequestId { get; set; } = string.Empty;

        /// <summary>Type de ressource : presence | payment.</summary>
        [Required]
        [MaxLength(40)]
        public string ResourceType { get; set; } = string.Empty;

        /// <summary>ID de l'entité créée côté serveur (si status = created).</summary>
        public int? ResourceId { get; set; }

        /// <summary>Statut final : created | duplicate | rejected | error.</summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = string.Empty;

        /// <summary>Message optionnel (rejet / erreur) pour rejeu.</summary>
        [MaxLength(500)]
        public string? Message { get; set; }

        /// <summary>Code d'erreur métier optionnel.</summary>
        [MaxLength(80)]
        public string? ErrorCode { get; set; }

        /// <summary>Payload résultat JSON optionnel (rejeu duplicate).</summary>
        [Column(TypeName = "TEXT")]
        public string? ResultJson { get; set; }

        public int? IdUtilisateur { get; set; }

        [MaxLength(100)]
        public string? DeviceId { get; set; }

        [Required]
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    }
}
