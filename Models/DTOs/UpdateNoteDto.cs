using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class UpdateNoteDto
    {
        [Required]
        public int IdNote { get; set; }
        
        [Required]
        [Range(0, 100)]
        public double NoteObtenue { get; set; }
        
        [StringLength(500)]
        public string? Appreciation { get; set; }
    }
}
