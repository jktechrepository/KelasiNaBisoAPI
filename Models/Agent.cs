using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    public class Agent : Adresse
    {
        [Key]
        public int IdAgent { get; set; }
        
        [MaxLength(50)]
        public string? Matricule { get; set; } // ✨ Nullable - Généré automatiquement si non fourni
        
        [MaxLength(100)]
        public string? Nom { get; set; }
        [MaxLength(100)]
        public string? Postnom { get; set; }
        [MaxLength(100)]
        public string? Prenom { get; set; }
        [MaxLength(10)]
        public string? Genre { get; set; }
        [Required]
        public DateTime DateNaissance { get; set; }
        public string? TelephoneAgent { get; set; }
        public string? EmailAgent { get; set; }
        public bool? Statut { get; set; } = true; // True ou False
        [MaxLength(20)]
        public string? EtatCivil { get; set; }
        public string? SerialNumber { get; set; }
        public string? Fonction { get; set; }
        public string? RoleAgent { get; set; }

       // [ValidateNever]
       // public IFormFile? Image { get; set; }

 
        public string? PhotoUrl { get; set; }
        public int? IdEcole { get; set; }
        
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
        public ICollection<AffectationCours> AffectationsCours { get; set; } = new List<AffectationCours>();
        
        [JsonIgnore]
        [ValidateNever]
        public ICollection<Presence> Presences { get; set; } = new List<Presence>();
        
        // Relation avec Utilisateur (nullable)
        [JsonIgnore]
        [ValidateNever]
        public ICollection<Utilisateur>? Utilisateurs { get; set; } = new List<Utilisateur>();
    }
}

