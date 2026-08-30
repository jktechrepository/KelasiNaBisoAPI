using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    public class Evaluation
    {
        [Key]
        public int IdEvaluation { get; set; }
        public string? TypeEvaluation { get; set; } //
        public string? TitreEvaluation { get; set; } // Titre de l'évaluation (ex: titre du devoir)
        [MaxLength(255)]
        public string? Periode { get; set; } // Période ou session (ex: "Trimestre 1", "Semestre 1")
        public double? Coefficient { get; set; }
        public int IdCours { get; set; }
        public int IdClasse { get; set; }
        public bool? Statut { get; set; } = true;

        // Attributs Techniques
        [JsonIgnore]
        public DateTime? DateCreation { get; set; }


        // Attributs de Navigation
        [JsonIgnore]
        [ValidateNever]
        public Cours? Course { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public Classe? Classe { get; set; }
        
        [JsonIgnore]
        [ValidateNever]
        public ICollection<Note> Notes { get; set; }
    }
}
