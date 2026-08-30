using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    public class Message
    {
        [Key]
        public int IdMessage { get; set; }
        public int? IdExpediteur { get; set; }
        public int? IdDestinateur { get; set; }
        public int? IdGroupe { get; set; }
        [Required]
        [MaxLength(1000)]
        public string ContenuMessage { get; set; }
        [MaxLength(500)]
        public string FichierUrl { get; set; }
        public bool? Statut { get; set; } = true;

        // Attributs Techniques
        [JsonIgnore]
        public DateTime DateEnvoi { get; set; }

        // Attributs de Navigation
        [JsonIgnore]
        [ValidateNever]
        public Utilisateur Expediteur { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public Utilisateur Destinateur { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public GroupeMessage GroupeMessage { get; set; }
    }
}
