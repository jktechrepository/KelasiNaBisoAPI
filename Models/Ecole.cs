using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    public class Ecole : Adresse
    {
        [Key]
        public int IdEcole { get; set; }
        [Required]
        [MaxLength(150)]
        public string? Nom { get; set; }
        public string? Slogan { get; set; }
        public string? Longitute { get; set; }
        public string? Latitude { get; set; }

        [MaxLength(50)]
        public string? Type { get; set; } // Privee, Publique, Conventionnee
        public string? Logo { get; set; }
        public string? Telephone { get; set; }
        public string? EmailContact { get; set; }
        public string? SiteWeb { get; set; }
        public string? ProvinceEducationnel { get; set; }
        public string? NomCompletResponsable { get; set; }
        
        [MaxLength(10)]
        public string? GenreResponsable { get; set; } // Genre du responsable: Masculin, Feminin
        
        public string? Description { get; set; } // Description de l'ecole
        //[ValidateNever]
        //public IFormFile? Image { get; set; }
        public bool? Statut { get; set; } = true;
        
        /// <summary>
        /// Indique si l'école a payé pour les notifications SMS.
        /// Si true, les SMS peuvent être envoyés aux parents.
        /// Si false, aucun SMS ne sera envoyé.
        /// </summary>
        public bool? AcceptNotification { get; set; } = true;

        // Attributs Techniques
        [JsonIgnore]
        public DateTime DateCreation { get; set; } = DateTime.Now;

        // Navigation properties
        [JsonIgnore]
        [ValidateNever]
        public ICollection<Utilisateur> Utilisateurs { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public ICollection<Direction> Directions { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public ICollection<AnneeScolaire> AnneeScolaires { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public ICollection<Agent> Agents { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public ICollection<Section>? Sections { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public ICollection<GroupeMessage> GroupesMessages { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public ICollection<Inscription> Inscriptions { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public ICollection<Notification> Notifications { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public ICollection<Vacation> Vacations { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<CommunicationCampaign>? CommunicationCampaigns { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<CommunicationSegment>? SegmentsCommunication { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<CommunicationTemplate>? CommunicationTemplates { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public EcoleInfoPaiementMobile? InfoPaiementMobile { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public EcoleWallet? Wallet { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<EcoleBeneficiaireMomo> BeneficiairesMomo { get; set; }
    }
}
