using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    public class GroupeMessage
    {
        [Key]
        public int IdGroupe { get; set; }
        [Required]
        [MaxLength(100)]
        public string NomGroupe { get; set; }
        public int? CreePar { get; set; }
        [Required]
        public int IdEcole { get; set; }
        public bool? Statut { get; set; } = true;

        // Attributs Techniques
        [JsonIgnore]
        public DateTime DateCreation { get; set; }

        // Navigation
        [JsonIgnore]
        [ValidateNever]
        public Utilisateur Utilisateur { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public Ecole Ecole { get; set; }
        
        // Collections
        [JsonIgnore]
        [ValidateNever]
        public ICollection<Message> Messages { get; set; }
    }
}
