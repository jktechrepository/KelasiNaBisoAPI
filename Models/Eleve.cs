
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    public class Eleve : Adresse
    {
          [Key]
          public int IdEleve { get; set; }
          public Guid? ReferenceEleve { get; set; }
          public string? Matricule { get; set; } // Matricule de l'eleve, peut �tre vide si pas encore attribu�
          public string?  Nom { get; set; }
          public string? Postnom { get; set; }
          public string? Prenom { get; set; }
          [ValidateNever]
          public string? NomComplet { get; set; }
          public string? Genre { get; set; }
          public DateTime DateNaissance { get; set; }
          public string? LieuNaissance { get; set; }       
          public string? PhotoUrl { get; set; }
          public string Nationalite { get; set; }
          public string? Commentaire { get; set; } //On peut signaler une alergie ou autre chose
          public int? IdTuteur { get; set; }
          public bool? Statut { get; set; } = true;//True et False
          public string? SerialNumber { get; set; }

        // Attributs Techniques
        [JsonIgnore]
          public DateTime DateCreation { get; set; } = DateTime.Now;

         // Navigation : classe courante via Inscription (pas de FK directe)
          [JsonIgnore]
          [ValidateNever]
          public Tuteur? Tuteur { get; set; }

          // Collections
          [JsonIgnore]
          [ValidateNever]
          public ICollection<Note> Notes { get; set; }
          [JsonIgnore]
          [ValidateNever]
          public ICollection<Inscription> Inscriptions { get; set; }
          [JsonIgnore]
          [ValidateNever]
          public ICollection<Paiement> Paiements { get; set; }
          [JsonIgnore]
          [ValidateNever]
          public ICollection<Presence> Presences { get; set; }
          [JsonIgnore]
          [ValidateNever]
          public ICollection<Document> Documents { get; set; }
          [JsonIgnore]
          [ValidateNever]
          public ICollection<Notification> Notifications { get; set; }

          [JsonIgnore]
          [ValidateNever]
          public ICollection<CampaignRecipient>? CampaignRecipients { get; set; }
    }
}
