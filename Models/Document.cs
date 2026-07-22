using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    public class Document
    {
        [Key]
        public int IdDocument { get; set; }
        [Required]
        public int IdEleve { get; set; }
        [Required]
        [MaxLength(100)]
        public string TypeDocument { get; set; }
        [Required]
        public int IdUtilisateur { get; set; }
        public bool? Statut { get; set; } = true;
        
        // Attributs Techniques
        [JsonIgnore]
        public DateTime DateCreation { get; set; } = DateTime.Now;
        
        // Navigation
        [JsonIgnore]
        [ValidateNever]
        public Utilisateur Utilisateur { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public Eleve Eleve { get; set; }
    }
}
