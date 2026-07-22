using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    public class AffectationCours
    {
        [Key]
        public int IdAffectationCours { get; set; }
        
        [Required]
        public int IdAgent { get; set; }
        
        [Required]
        public int IdCours { get; set; }
        
        [Required]
        public int IdAnneeScolaire { get; set; }
        
        [Required]
        public DateTime DateAffectation { get; set; } = DateTime.Now;
        
        public DateTime? DateFinAffectation { get; set; }
    
        public bool? Statut { get; set; } = true; // True = Actif, False = Inactif
        
        [MaxLength(500)]
        public string? Commentaire { get; set; }

        // Attributs Techniques
        [JsonIgnore]
        public DateTime DateCreation { get; set; } = DateTime.Now;

        // Navigation
        [JsonIgnore]
        [ValidateNever]
        public Agent Agent { get; set; }
        
        [JsonIgnore]
        [ValidateNever]
        public Cours Cours { get; set; }
        
        [JsonIgnore]
        [ValidateNever]
        public AnneeScolaire AnneeScolaire { get; set; }
    }
}
