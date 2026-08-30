using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace KelasiNaBiso.Models
{
    public class CommunicationCampaign
    {
        [Key]
        public int IdCampaign { get; set; }

        [Required]
        public int IdEcole { get; set; }

        [Required]
        public int IdAuteur { get; set; }

        [Required]
        [MaxLength(200)]
        public string Titre { get; set; } = string.Empty;

        [Required]
        public string ContenuMarkdown { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string Statut { get; set; } = "Brouillon";

        [Required]
        [MaxLength(20)]
        public string Importance { get; set; } = "Info";

        [Required]
        public string ChannelsJson { get; set; } = "{\"push\":true,\"email\":true,\"sms\":false,\"inApp\":true}";

        [Required]
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        public DateTime? PlanifiedAt { get; set; }

        public DateTime? ExpirationAt { get; set; }

        public bool RappelAuto { get; set; }

        public DateTime? ValidationAt { get; set; }

        public int? ValidatedBy { get; set; }

        // Navigation
        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(IdEcole))]
        public Ecole? Ecole { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(IdAuteur))]
        public Utilisateur? Auteur { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(ValidatedBy))]
        public Utilisateur? Validateur { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<CommunicationSegment>? Segments { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<CampaignRecipient>? Destinataires { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<CommunicationHistory>? Historique { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<Notification>? Notifications { get; set; }
    }
}

