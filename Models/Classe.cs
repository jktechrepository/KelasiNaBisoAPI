using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    public class Classe
    {
        [Key]
        public int IdClasse { get; set; }
        [Required]
        [MaxLength(100)]
        public string NomClasse { get; set; }
        public int? IdDirection { get; set; }
        public int? IdSection { get; set; }
        public int? IdOption { get; set; }
        public bool? Statut { get; set; } = true;

        // Attributs Techniques
        [JsonIgnore]
        public DateTime? DateCreation { get; set; }

        // Attributs de Navigation
        [JsonIgnore]
        [ValidateNever]
        public Direction? Direction { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public Section? Section { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public Option? Option { get; set; }
        
        // Collections
        [JsonIgnore]
        [ValidateNever]
        public ICollection<Eleve> Eleves { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public ICollection<Cours> Cours { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public ICollection<Inscription> Inscriptions { get; set; }


        [JsonIgnore]
        [ValidateNever]
        public ICollection<Evaluation> Evaluations { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public ICollection<Notification> Notifications { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<Horaire> Horaires { get; set; }
    }
}
