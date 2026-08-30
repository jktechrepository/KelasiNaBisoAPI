using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace KelasiNaBiso.Models
{
    public class CommunicationSegment
    {
        [Key]
        public int IdSegment { get; set; }

        public int? IdCampaign { get; set; }

        public int? IdEcole { get; set; }

        [Required]
        [MaxLength(150)]
        public string NomSegment { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string TypeSegment { get; set; } = string.Empty;

        [Required]
        public string CriteriaJson { get; set; } = "{}";

        [Required]
        public bool IsReusable { get; set; }

        [Required]
        public int CreatedBy { get; set; }

        [Required]
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(IdCampaign))]
        public CommunicationCampaign? Campaign { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(IdEcole))]
        public Ecole? Ecole { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(CreatedBy))]
        public Utilisateur? Createur { get; set; }
    }
}

