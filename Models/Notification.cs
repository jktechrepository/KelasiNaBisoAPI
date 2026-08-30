using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace KelasiNaBiso.Models
{
    /// <summary>
    /// Modèle pour les notifications du système KelasiNaBiso
    /// </summary>
    public class Notification
    {
        [Key]
        public int IdNotification { get; set; }

        [Required]
        [MaxLength(200)]
        public string Titre { get; set; } = string.Empty;

        public bool? Statut { get; set; } = true;

        [Required]
        [MaxLength(1000)]
        public string Contenu { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string TypeNotification { get; set; } = string.Empty; // "INFO", "WARNING", "ERROR", "SUCCESS"

        [Required]
        public bool EstLue { get; set; } = false;

        [Required]
        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateLecture { get; set; }

        [MaxLength(100)]
        public string? LienAction { get; set; }

        [MaxLength(50)]
        public string? Icone { get; set; }

        [Required]
        public bool EstActive { get; set; } = true;

        // Clés étrangères - Adaptées pour KelasiNaBiso
        public int? IdExpediteur { get; set; }
        public int? IdDestinataire { get; set; }
        public int? IdEcole { get; set; } // Remplacé IdSite par IdEcole
        public int? IdClasse { get; set; } // Remplacé IdPromotion par IdClasse
        public int? IdEleve { get; set; } // Remplacé IdEtudiant par IdEleve
        public int? IdAgent { get; set; } // Remplacé IdEnseignant par IdAgent
        public int? IdCours { get; set; }
        public int? IdAnneeScolaire { get; set; }

        // Nouvelles métadonnées pour la communication avancée
        public int? IdCampaign { get; set; }

        [MaxLength(20)]
        public string? CanalUtilise { get; set; }

        [Required]
        [MaxLength(20)]
        public string Priorite { get; set; } = "INFO";

        public string? PayloadJson { get; set; }

        [Required]
        [MaxLength(20)]
        public string StatutEnvoi { get; set; } = "Envoye";

        [MaxLength(100)]
        public string? TrackingId { get; set; }

        // Attributs de navigation
        [JsonIgnore]
        [ValidateNever]
        public virtual Utilisateur? Expediteur { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public virtual Utilisateur? Destinataire { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public Ecole? Ecole { get; set; } // Remplacé Site par Ecole
        [JsonIgnore]
        [ValidateNever]
        public Classe? Classe { get; set; } // Remplacé Promotion par Classe
        [JsonIgnore]
        [ValidateNever]
        public Eleve? Eleve { get; set; } // Remplacé Etudiant par Eleve
        [JsonIgnore]
        [ValidateNever]
        public Agent? Agent { get; set; } // Remplacé Enseignant par Agent
        [JsonIgnore]
        [ValidateNever]
        public Cours? Cours { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public AnneeScolaire? AnneeScolaire { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(IdCampaign))]
        public CommunicationCampaign? Campaign { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<CampaignRecipient>? CampaignRecipients { get; set; }
    }
}