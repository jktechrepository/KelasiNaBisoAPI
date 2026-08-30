using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    public class Inscription
    {
        [Key]
        public int IdInscription { get; set; }
        public string Type  { get; set; } // Inscription ou R�inscription
        [Required]
        public int IdEleve { get; set; }
        [Required]
        public int IdEcole { get; set; }
        [Required]
        public int IdClasse { get; set; }
        [Required]
        public int IdAnneeScolaire { get; set; }
        [Required]
        public DateTime DateInscription { get; set; }
        [Required]
        [MaxLength(20)]
        public string StatutInscription { get; set; } = "Confirmé"; // En attente, Annulé, Confirmé
        public bool? Statut { get; set; } = true;

        // Attributs Techniques
        [JsonIgnore]
        public DateTime DateCreation { get; set; } = DateTime.Now;

        // Navigation
        [JsonIgnore]
        [ValidateNever]
        public Eleve Eleve { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public Classe Classe { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public Ecole Ecole { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public AnneeScolaire AnneeScolaire { get; set; }
    }
}
