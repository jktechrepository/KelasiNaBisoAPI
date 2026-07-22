using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class UpdateAgentDto
    {
        [Required]
        public int IdAgent { get; set; }
        
        [Required]
        [StringLength(100)]
        public string? Nom { get; set; }
        
        [Required]
        [StringLength(100)]
        public string? Postnom { get; set; }
        
        [Required]
        [StringLength(100)]
        public string? Prenom { get; set; }
        
        [StringLength(10)]
        public string? Genre { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? DateNaissance { get; set; }
        
        [StringLength(200)]
        public string? EmailAgent { get; set; }
        
        [StringLength(20)]
        public string? TelephoneAgent { get; set; }
        
      
        public string? PhotoUrl { get; set; }
        
        [StringLength(20)]
        public string? EtatCivil { get; set; }
        
        [StringLength(200)]
        public string? Fonction { get; set; }
        
        [StringLength(200)]
        public string? RoleAgent { get; set; }
        
        // Adresse
        [StringLength(100)]
        public string? Ville { get; set; }
        
        [StringLength(100)]
        public string? Commune { get; set; }
        
        [StringLength(100)]
        public string? Quartier { get; set; }
        
        [StringLength(100)]
        public string? Avenue { get; set; }
        
        [StringLength(20)]
        public string? Numero { get; set; }
    }
}
