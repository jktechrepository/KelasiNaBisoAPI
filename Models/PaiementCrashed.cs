using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    /// <summary>
    /// Modèle pour stocker les paiements qui ont échoué lors du bulk insert Excel
    /// Permet de les corriger et de les réinjecter ultérieurement
    /// </summary>
    public class PaiementCrashed
    {
        [Key]
        public int IdPaiementCrashed { get; set; }

        // Données du paiement (sérialisées depuis PaiementExcelDto)
        // ✅ CORRECTION : Retirer [Required] car le champ est nullable dans la base de données
        public DateTime? DatePaiement { get; set; }
        
        public double? Montant { get; set; }
        
        [MaxLength(10)]
        public string? Devise { get; set; } = "USD";
        
        [MaxLength(50)]
        public string? ModePaiement { get; set; }
        
        public bool? Statut { get; set; } = true;
        
        [MaxLength(50)]
        public string? StatutPaiement { get; set; } = "Confirmé";
        
        [MaxLength(200)]
        public string? ReferenceTransaction { get; set; }
        
        [MaxLength(1000)]
        public string? JustificatifUrl { get; set; }
        
        [MaxLength(1000)]
        public string? Commentaire { get; set; }
        
        // IDs (peuvent être null si la recherche a échoué)
        public int? IdEleve { get; set; }
        
        public int? IdFrais { get; set; }
        
        public int? IdUtilisateur { get; set; }
        
        // Données brutes du fichier Excel (pour faciliter la correction)
        [MaxLength(200)]
        public string? NomCompletEleve { get; set; } // Nom original du fichier Excel
        
        [MaxLength(200)]
        public string? LibelleFrais { get; set; } // Libellé original du fichier Excel
        
        // Informations sur l'erreur
        [Required]
        public string ErreursJson { get; set; } = "[]"; // Liste des erreurs sérialisées en JSON
        
        public int NumeroLigne { get; set; } // Numéro de ligne dans le fichier Excel original
        
        // Métadonnées
        [Required]
        public int IdEcole { get; set; } // École concernée
        
        [MaxLength(500)]
        public string? NomFichierOriginal { get; set; } // Nom du fichier Excel d'origine
        
        public DateTime? DateEchec { get; set; } = DateTime.Now; // Date de l'échec
        
        public DateTime? DateCorrection { get; set; } // Date de la dernière correction
        
        public DateTime? DateReinjection { get; set; } // Date de la dernière tentative de réinjection
        
        public int? IdPaiementCree { get; set; } // ID du paiement créé si réinjection réussie
        
        public bool EstResolu { get; set; } = false; // true si le paiement a été créé avec succès
        
        // Attributs Techniques
        [JsonIgnore]
        public DateTime? DateCreation { get; set; } = DateTime.Now;
        
        [JsonIgnore]
        public DateTime? DateModification { get; set; }

        // Navigation Properties
        [JsonIgnore]
        [ValidateNever]
        public Ecole? Ecole { get; set; }
        
        [JsonIgnore]
        [ValidateNever]
        public Eleve? Eleve { get; set; }
        
        [JsonIgnore]
        [ValidateNever]
        public Frais? Frais { get; set; }
        
        [JsonIgnore]
        [ValidateNever]
        public Utilisateur? Utilisateur { get; set; }
        
        [JsonIgnore]
        [ValidateNever]
        public Paiement? PaiementCree { get; set; } // Lien vers le paiement créé si réinjection réussie
    }
}

