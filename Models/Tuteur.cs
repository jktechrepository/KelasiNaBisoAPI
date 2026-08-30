using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    public class Tuteur 
    {
        [Key]
        public int IdTuteur { get; set; }
        [Required]
        [MaxLength(150)]
        public string? NomComplet { get; set; }
        [Required]
        [MaxLength(10)]
        public string? Genre { get; set; }
        public string? Email { get; set; }
        [Phone]
        public string? Telephone { get; set; }

        public string? NomCompletRepresentant { get; set; }
        public string? TelephoneRepresentant { get; set; }
        public string? PhotoTuteurUrl { get; set; }
        public string? PieceIdentiteTuteur { get; set; }
        public bool? Statut { get; set; } = true; // True et False
        public string? SerialNumber { get; set; }
       // [ValidateNever]
       // public IFormFile? Image { get; set; }

        //Attributs Techniques
        [JsonIgnore]
        public DateTime? DateCreation { get; set; }

        // École via Inscription des enfants (pas de FK directe)
        // Collections
        [JsonIgnore]
        [ValidateNever]
        public ICollection<Eleve> Eleves { get; set; }
        
        // Relation avec Utilisateur (nullable)
        [JsonIgnore]
        [ValidateNever]
        public ICollection<Utilisateur>? Utilisateurs { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<ParentCommunicationPreference>? PreferencesCommunication { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<CampaignRecipient>? CampaignRecipients { get; set; }
    }
}
