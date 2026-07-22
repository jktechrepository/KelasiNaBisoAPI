using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class UpdateRessourcePedagogiqueDto
    {
        [Required]
        public int IdRessourcePedagogique { get; set; }
        
        [Required]
        [StringLength(200)]
        public string? TitreRessourcePedagogique { get; set; }
        
        [StringLength(50)]
        public string? FormatRessourcePedagogique { get; set; }
        
        [StringLength(500)]
        public string? UrlRessourcePedagogique { get; set; }
        
        [Required]
        public int IdCours { get; set; }
    }
}
