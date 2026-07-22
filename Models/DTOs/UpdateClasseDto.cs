using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class UpdateClasseDto
    {
        [Required]
        public int IdClasse { get; set; }
        
        [Required]
        [StringLength(100)]
        public string? NomClasse { get; set; }
        
        public int? IdDirection { get; set; }
        public int? IdSection { get; set; }
        public int? IdOption { get; set; }
    }
}
