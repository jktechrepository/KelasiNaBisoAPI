using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class UpdateDocumentDto
    {
        [Required]
        public int IdDocument { get; set; }
        
        [Required]
        [StringLength(100)]
        public string? TypeDocument { get; set; }
    }
}
