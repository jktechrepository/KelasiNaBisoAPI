using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class UpdateGroupeMessageDto
    {
        [Required]
        public int IdGroupe { get; set; }
        
        [Required]
        [StringLength(100)]
        public string? NomGroupe { get; set; }
    }
}
