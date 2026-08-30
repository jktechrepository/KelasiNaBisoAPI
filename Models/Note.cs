using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    public class Note
    {
        [Key]
        public int IdNote { get; set; }
        [Required]
        [Range(0, 100)]
        public double NoteObtenue { get; set; }
        [MaxLength(500)]
        public string Appreciation { get; set; }
        [Required]
        public DateTime DateEvaluation { get; set; }
        [Required]
        public int IdProfesseur { get; set; }    
        [Required]
        public int IdEleve { get; set; }
        [Required]
        public int IdEvaluation { get; set; } // Relation avec Evaluation au lieu de Cours
        [Required]
        public int IdAnneeScolaire { get; set; }
        public bool? Statut { get; set; } = true;

        // Attributs Techniques
        [JsonIgnore]
        public DateTime? DateCreation { get; set; }

        // Attributs de Navigation
        [JsonIgnore]
        [ValidateNever]
        public Eleve Eleve { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public Evaluation Evaluation { get; set; } // Relation avec Evaluation au lieu de Cours
        [JsonIgnore]
        [ValidateNever]
        public AnneeScolaire AnneeScolaire { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public Utilisateur Professeur { get; set; }
    }
}
