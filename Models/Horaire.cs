using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    public class Horaire
    {
        [Key]
        public int IdHoraire { get; set; }
        [Required]
        [MaxLength(50)]
        public string Vacation { get; set; }
        [Required]
        public TimeSpan HeureDebut { get; set; }
        [Required]
        public TimeSpan HeureFin { get; set; }
        public TimeSpan? HeureDebutPause { get; set; }
        public TimeSpan? HeureFinPause { get; set; }
        [Required]
        public int IdClasse { get; set; }
        public bool? Statut { get; set; } = true;

        // Attributs Techniques
        [JsonIgnore]
        public DateTime DateCreation { get; set; } = DateTime.Now;

        // Navigation
        [JsonIgnore]
        [ValidateNever]
        public Classe Classe { get; set; }

        // Collections
        [JsonIgnore]
        [ValidateNever]
        public ICollection<Presence> Presences { get; set; }
    }
}
