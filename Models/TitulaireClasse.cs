using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    /// <summary>
    /// Représente l'affectation d'un enseignant titulaire à une classe
    /// Utilisé pour MATERNELLE et PRIMAIRE où un seul enseignant enseigne tous les cours
    /// </summary>
    public class TitulaireClasse
    {
        [Key]
        public int IdTitulaireClasse { get; set; }
        
        /// <summary>
        /// ID de l'agent (enseignant) titulaire de la classe
        /// </summary>
        [Required(ErrorMessage = "L'agent est obligatoire")]
        public int IdAgent { get; set; }
        
        /// <summary>
        /// ID de la classe dont l'agent est titulaire
        /// </summary>
        [Required(ErrorMessage = "La classe est obligatoire")]
        public int IdClasse { get; set; }
        
        /// <summary>
        /// Année scolaire de cette affectation
        /// </summary>
        [Required(ErrorMessage = "L'année scolaire est obligatoire")]
        public int IdAnneeScolaire { get; set; }
        
        /// <summary>
        /// Date de début de l'affectation
        /// </summary>
        [Required]
        public DateTime DateDebut { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Date de fin de l'affectation (null si toujours en cours)
        /// </summary>
        public DateTime? DateFin { get; set; }
        
        /// <summary>
        /// Statut de l'affectation (true = actif, false = inactif)
        /// </summary>
        public bool? Statut { get; set; } = true;
        
        /// <summary>
        /// Commentaire ou remarque sur cette affectation
        /// </summary>
        [MaxLength(500)]
        public string? Commentaire { get; set; }

        // ============================================
        // ATTRIBUTS TECHNIQUES
        // ============================================
        
        [JsonIgnore]
        public DateTime DateCreation { get; set; } = DateTime.Now;
        
        [JsonIgnore]
        public DateTime? DateModification { get; set; }

        // ============================================
        // NAVIGATION PROPERTIES
        // ============================================
        
        /// <summary>
        /// L'agent titulaire
        /// </summary>
        [JsonIgnore]
        [ValidateNever]
        public Agent Agent { get; set; }
        
        /// <summary>
        /// La classe concernée
        /// </summary>
        [JsonIgnore]
        [ValidateNever]
        public Classe Classe { get; set; }
        
        /// <summary>
        /// L'année scolaire
        /// </summary>
        [JsonIgnore]
        [ValidateNever]
        public AnneeScolaire AnneeScolaire { get; set; }
    }
}

