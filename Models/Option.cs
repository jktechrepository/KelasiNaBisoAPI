using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    public class Option
    {
        [Key]
        public int IdOption { get; set; }
        [Required]
        [MaxLength(100)]
        public string NomOption { get; set; }
        [Required]
        public int IdSection { get; set; }
        public bool? Statut { get; set; } = true;

        // Attibuts Technique
        [JsonIgnore]
        public DateTime DateCreation { get; set; }

        // Navigation
        [JsonIgnore]
        [ValidateNever]
        public Section Section { get; set; }
        
        // Collections
        [JsonIgnore]
        [ValidateNever]
        public ICollection<Classe> Classes { get; set; }
    }
}
