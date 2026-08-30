using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    public class Frais
    {
        [Key]
        public int IdFrais { get; set; }
        [Required]
        [MaxLength(200)]
        public string LibelleFrais { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public double Montant { get; set; }
        [Required]
        [MaxLength(10)]
        public string Devise { get; set; }

        public string? TypeFrais { get; set; }  // Inscription, Scolaires, Transport, Cantine, Uniforme

        public string? Periodicite { get; set; }  // Unique, Mensuel, Trimestriel, Annuel

        public string? Description { get; set; } //(optionnel)

        public bool? Statut { get; set; } = true; // Actif/Inactif

        public int IdDirection { get; set; }

        /// <summary>Année scolaire obligatoire (une grille tarifaire = une année).</summary>
        [Required]
        public int IdAnneeScolaire { get; set; }

        /// <summary>Null = applicable à toute la direction ; sinon tarif spécifique à une classe.</summary>
        public int? IdClasse { get; set; }

        // Attributs Technique
        [JsonIgnore]
        public DateTime DateCreation { get; set; }

        // Navigation
        [JsonIgnore]
        [ValidateNever]
        public Direction Direction { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public AnneeScolaire AnneeScolaire { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public Classe? Classe { get; set; }
        
        // Collections
        [JsonIgnore]
        [ValidateNever]
        public ICollection<Paiement> Paiements { get; set; }
    }
}
