using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class UpdateInscriptionDto
    {
        [Required]
        public int IdInscription { get; set; }
        
        [StringLength(50)]
        public string? Type { get; set; }
        
        [Required]
        [StringLength(20)]
        public string? StatutInscription { get; set; }
        
        [Required]
        public int IdClasse { get; set; }
    }
}
