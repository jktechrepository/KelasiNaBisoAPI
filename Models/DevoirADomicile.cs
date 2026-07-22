using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    /// <summary>
    /// Modèle représentant un devoir à domicile publié par un enseignant
    /// </summary>
    public class DevoirADomicile
    {
        [Key]
        public int IdDevoirADomicile { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Titre { get; set; }
        
        [MaxLength(1000)]
        public string? Description { get; set; }
        
        // Contenu textuel du devoir (optionnel si fichier fourni)
        [MaxLength(5000)]
        public string? Contenu { get; set; } // Contenu textuel du devoir
        
        // Informations sur le fichier (optionnel si contenu textuel fourni)
        [MaxLength(500)]
        public string? NomFichier { get; set; } // Nom original du fichier
        
        [MaxLength(1000)]
        public string? CheminFichier { get; set; } // Chemin sur le serveur
        
        public long? TailleFichier { get; set; } // En bytes
        
        [MaxLength(100)]
        public string? TypeMIME { get; set; } // application/pdf, image/jpeg, image/png
        
        // Relations avec les entités
        [Required]
        public int IdEcole { get; set; }
        
        [Required]
        public int IdDirection { get; set; }
        
        [Required]
        public int IdAgent { get; set; } // Enseignant qui publie
        
        [Required]
        public int IdClasse { get; set; } // Classe concernée
        
        public int? IdCours { get; set; } // Optionnel : cours spécifique
        
        public int CoefficientDevoir { get; set; } = 1; // Coefficient pour l'évaluation associée
        
        // Dates
        public DateTime DatePublication { get; set; } = DateTime.Now;
        
        public DateTime? DateLimite { get; set; } // Optionnel : date limite de remise
        
        // Statistiques
        public int NombreTelechargements { get; set; } = 0;
        
        // Statut
        public bool Statut { get; set; } = true; // true = actif, false = archivé
        
        // Attributs Techniques
        [JsonIgnore]
        public DateTime DateCreation { get; set; } = DateTime.Now;
        
        [JsonIgnore]
        public DateTime? DateModification { get; set; }
        
        // Navigation Properties
        [JsonIgnore]
        [ValidateNever]
        public Ecole Ecole { get; set; }
        
        [JsonIgnore]
        [ValidateNever]
        public Direction Direction { get; set; }
        
        [JsonIgnore]
        [ValidateNever]
        public Agent Agent { get; set; }
        
        [JsonIgnore]
        [ValidateNever]
        public Classe Classe { get; set; }
        
        [JsonIgnore]
        [ValidateNever]
        public Cours? Cours { get; set; }
    }
}

