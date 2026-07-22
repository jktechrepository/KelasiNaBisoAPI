using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class UpdateMessageDto
    {
        [Required]
        public int IdMessage { get; set; }
        
        [Required]
        [StringLength(1000)]
        public string? ContenuMessage { get; set; }
        
        [StringLength(500)]
        public string? FichierUrl { get; set; }
    }
}
