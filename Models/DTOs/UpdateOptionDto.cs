using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class UpdateOptionDto
    {
        [Required]
        public int IdOption { get; set; }
        
        [Required]
        [StringLength(100)]
        public string? NomOption { get; set; }
        
        [Required]
        public int IdSection { get; set; }
    }
}
