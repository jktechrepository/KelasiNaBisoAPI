using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace KelasiNaBiso.Models
{
    public class CommunicationHistory
    {
        [Key]
        public long IdHistory { get; set; }

        [Required]
        public int IdCampaign { get; set; }

        public int? UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = string.Empty;

        public string? DetailJson { get; set; }

        [Required]
        public DateTime DateAction { get; set; } = DateTime.UtcNow;

        [MaxLength(45)]
        public string? AdresseIP { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(IdCampaign))]
        public CommunicationCampaign? Campaign { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(UserId))]
        public Utilisateur? Utilisateur { get; set; }
    }
}

