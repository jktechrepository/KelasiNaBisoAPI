using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class UpdateEcoleDto
    {
        [Required]
        public int IdEcole { get; set; }
        
        [Required]
        [StringLength(150)]
        public string? Nom { get; set; }
        
        [StringLength(500)]
        public string? Slogan { get; set; }
        
        public string? Longitute { get; set; }
        public string? Latitude { get; set; }
        
        [StringLength(50)]
        public string? Type { get; set; }
        
      //  [StringLength(500)]
        public string? Logo { get; set; }
        
        [StringLength(20)]
        public string? Telephone { get; set; }
        
        [StringLength(200)]
        public string? EmailContact { get; set; }
        
        [StringLength(200)]
        public string? SiteWeb { get; set; }
        
        [StringLength(200)]
        public string? ProvinceEducationnel { get; set; }
        
        [StringLength(200)]
        public string? NomCompletResponsable { get; set; }
        
        [StringLength(10)]
        public string? GenreResponsable { get; set; }
        
        public string? Description { get; set; }
        
        public bool AcceptNotification { get; set; } = true;
        
        // Adresse (héritée de la classe Adresse)
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
        
        [StringLength(100)]
        public string? Province { get; set; }
    }
}
