using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace KelasiNaBiso.Models
{
    public class CategorieDepense
    {
        [Key]
        public int IdCategorieDepense { get; set; }

        [Required]
        public int IdEcole { get; set; }

        [Required]
        [MaxLength(150)]
        public string NomCategorie { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>True = catégorie active (utilisable à la création).</summary>
        public bool Statut { get; set; } = true;

        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(IdEcole))]
        public Ecole? Ecole { get; set; }
    }
}
