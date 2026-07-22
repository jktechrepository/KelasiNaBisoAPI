using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class UpdateSectionDto
    {
        [Required]
        public int IdSection { get; set; }
        
        [Required]
        [StringLength(100)]
        public string? NomSection { get; set; }
    }
}
