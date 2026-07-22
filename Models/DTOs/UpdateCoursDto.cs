using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class UpdateCoursDto
    {
        [Required]
        public int IdCours { get; set; }
        
        [Required]
        [StringLength(100)]
        public string? NomCours { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
       // [Range(1, 10)]
        public int? Ponderation { get; set; }
        
        public int? IdClasse { get; set; }
    }
}
