using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KelasiNaBiso.Models
{
    public class Utilisateur : Adresse
    {
        [Key]
        public int IdUtilisateur { get; set; }
        public Guid? ReferenceUtilisateur { get; set; }
        [Required]
        [MaxLength(100)]
        public string? NomUtilisateur { get; set; }
        public string? PostNomUtilisateur { get; set; }
        public string? PrenomUtilisateur { get; set; }
        [EmailAddress]
        public string? Email { get; set; }
        public string? Telephone { get; set; }
       // [ValidateNever]
       // public IFormFile? Image { get; set; }
        public string? PhotoUrl { get; set; }
        public string? LieuNaissance { get; set; }
        public DateTime? DateNaissance { get; set; }
        public string? Genre { get; set; }

        [Required]
        public string? MotDePasseHash { get; set; }
        public string? DefaultUsername { get; set; } // Nom d'utilisateur par défaut généré automatiquement
        public bool DoitChangerMotDePasse { get; set; } = false; // Force le changement de mot de passe à la première connexion
        public bool? Statut { get; set; } = true;
        public int? IdRole { get; set; } // ✅ Rendu nullable pour le système multi-rôles (rétrocompatibilité)
        public int? IdEcole { get; set; }

        // Attributs Techniques
        [JsonIgnore]
        public DateTime DateCreation { get; set; } = DateTime.Now;
        [JsonIgnore]
        public bool IsConnecte { get; set; } = false; // Par defaut, l'utilisateur n'est pas connecte
        [JsonIgnore]
        [ValidateNever]
        public ICollection<Message> MessagesEnvoyes { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public ICollection<Message> MessagesRecus { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public ICollection<GroupeMessage> GroupeMessages { get; set; }


        [JsonIgnore]
        [ValidateNever]
        public Ecole Ecole { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public Role Role { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<Paiement>? Paiements { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<Notification>? NotificationsEnvoyees { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<Notification>? NotificationsRecues { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<PasswordResetToken>? PasswordResetTokens { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<CommunicationCampaign>? CampaignsCreees { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<CommunicationCampaign>? CampaignsValidees { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<CommunicationSegment>? SegmentsCree { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<CampaignRecipient>? CampaignRecipients { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<CommunicationTemplate>? TemplatesCree { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<CommunicationTemplate>? TemplatesMisAJour { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<CommunicationHistory>? CampaignHistoryActions { get; set; }

        // Relations avec Agent et Tuteur (nullable)
        public int? IdAgent { get; set; }
        public int? IdTuteur { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey("IdAgent")]
        public Agent? Agent { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey("IdTuteur")]
        public Tuteur? Tuteur { get; set; }

        // ═══════════════════════════════════════════════════════════════════
        // ✅ MULTI-RÔLES : Relation N-N avec Role via UserRole
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Collection des rôles de l'utilisateur (relation N-N)
        /// Permet à un utilisateur d'avoir plusieurs rôles simultanément
        /// </summary>
        [JsonIgnore]
        [ValidateNever]
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        /// <summary>
        /// Propriété calculée : Rôles actifs de l'utilisateur
        /// </summary>
        [NotMapped]
        public IEnumerable<Role> Roles => UserRoles
            .Where(ur => ur.Statut == true)
            .Select(ur => ur.Role);

        /// <summary>
        /// Propriété calculée : Rôle principal de l'utilisateur
        /// Retourne le rôle marqué comme principal, ou le rôle avec le niveau le plus élevé
        /// </summary>
        [NotMapped]
        public Role? PrimaryRole => UserRoles
            .Where(ur => ur.Statut == true && ur.IsPrimary)
            .Select(ur => ur.Role)
            .FirstOrDefault() 
            ?? UserRoles
                .Where(ur => ur.Statut == true)
                .OrderBy(ur => ur.Role.Niveau ?? 999)
                .Select(ur => ur.Role)
                .FirstOrDefault();

    }
}
