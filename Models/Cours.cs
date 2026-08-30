using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    public class Cours
    { 
        [Key]
        public int IdCours { get; set; }
        [Required]
        [MaxLength(100)]
        public string? NomCours { get; set; }
        public string? Description { get; set; }
        [Range(1, 10)]
        public int? Ponderation { get; set; }
        public int? IdClasse { get; set; }
        public bool? Statut { get; set; } = true;

        // Attributs Technique
        [JsonIgnore]
        public DateTime? DateCreation { get; set; }

        // Attributs de Navigation
        [JsonIgnore]
        [ValidateNever]
        public Classe? Classe { get; set; }
        
        // Collections
        [JsonIgnore]
        [ValidateNever]
        public ICollection<AffectationCours> AffectationsCours { get; set; }
        // ⚠️ DEPRECATED : Les notes sont maintenant liées à Evaluation, pas directement à Cours
        // Utiliser Evaluation.Notes pour accéder aux notes via les évaluations
        [JsonIgnore]
        [ValidateNever]
        [Obsolete("Les notes sont maintenant liées à Evaluation. Utiliser Evaluation.Notes")]
        public ICollection<Note> Notes { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public ICollection<Evaluation> Evaluations { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public ICollection<RessourcePedagogique> RessourcesPedagogiques { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public ICollection<Notification> Notifications { get; set; }
    }
}
