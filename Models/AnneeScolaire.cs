using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    public class AnneeScolaire
    {
        [Key]
        public int IdAnneeScolaire { get; set; }
        [Required]
        [MaxLength(50)]
        public string LibelleAnneeScolaire { get; set; }
        [Required]
        public DateTime DateDebut { get; set; }
        [Required]
        public DateTime DateFin { get; set; }
        public int? IdEcole { get; set; }
        public bool? Statut { get; set; } = true;

        // Attributs Techniques
        [JsonIgnore]
        public DateTime DateCreation { get; set; } = DateTime.Now;

        // Navigation
        [JsonIgnore]
        [ValidateNever]
        public Ecole? Ecole { get; set; }
        
        // Collections
        [JsonIgnore]
        [ValidateNever]
        public ICollection<Inscription> Inscriptions { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public ICollection<Note> Notes { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public ICollection<Notification> Notifications { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public ICollection<Frais> Frais { get; set; }
    }
}
