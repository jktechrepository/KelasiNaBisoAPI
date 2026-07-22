using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    public class Section
    {
        [Key]
        public int IdSection { get; set; }
        [Required]
        [MaxLength(100)]
        public string NomSection { get; set; }
      
        public int? IdEcole { get; set; }
        public bool? Statut { get; set; } = true;

        // Attributs Technique
        [JsonIgnore]
        public DateTime DateCreation { get; set; }

        // Navigation
        [JsonIgnore]
        [ValidateNever]
        public Ecole? Ecole { get; set; }
        
        // Collections
        [JsonIgnore]
        [ValidateNever]
        public ICollection<Option> Options { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public ICollection<Classe> Classes { get; set; }
    }
}
