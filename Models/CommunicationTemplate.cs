using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace KelasiNaBiso.Models
{
    public class CommunicationTemplate
    {
        [Key]
        public int IdTemplate { get; set; }

        public int? IdEcole { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string Langue { get; set; } = "fr";

        [Required]
        [MaxLength(200)]
        public string Titre { get; set; } = string.Empty;

        [Required]
        public string ContenuMarkdown { get; set; } = string.Empty;

        public string? PlaceholdersAutorises { get; set; }

        [Required]
        public int Version { get; set; } = 1;

        [Required]
        public bool IsActif { get; set; } = true;

        [Required]
        public int CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        [Required]
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        public DateTime? DateMaj { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(IdEcole))]
        public Ecole? Ecole { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(CreatedBy))]
        public Utilisateur? Createur { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(UpdatedBy))]
        public Utilisateur? Modificateur { get; set; }
    }
}

