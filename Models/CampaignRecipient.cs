using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace KelasiNaBiso.Models
{
    public class CampaignRecipient
    {
        [Key]
        public long IdRecipient { get; set; }

        [Required]
        public int IdCampaign { get; set; }

        [Required]
        public int IdUtilisateur { get; set; }

        public int? IdTuteur { get; set; }

        public int? IdEleve { get; set; }

        public int? IdNotification { get; set; }

        [MaxLength(20)]
        public string? PreferredChannel { get; set; }

        [MaxLength(20)]
        public string? FinalChannel { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Planifie";

        [Required]
        public DateTime DatePlanifie { get; set; } = DateTime.UtcNow;

        public DateTime? DateEnvoi { get; set; }

        [MaxLength(500)]
        public string? ErrorMessage { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(IdCampaign))]
        public CommunicationCampaign? Campaign { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(IdUtilisateur))]
        public Utilisateur? Utilisateur { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(IdTuteur))]
        public Tuteur? Tuteur { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(IdEleve))]
        public Eleve? Eleve { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(IdNotification))]
        public Notification? Notification { get; set; }
    }
}

