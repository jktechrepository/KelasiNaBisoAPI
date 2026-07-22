using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class UpdateAnneeScolaireDto
    {
        [Required]
        public int IdAnneeScolaire { get; set; }
        
        [Required]
        [StringLength(50)]
        public string? LibelleAnneeScolaire { get; set; }
        
        [Required]
        [DataType(DataType.Date)]
        public DateTime DateDebut { get; set; }
        
        [Required]
        [DataType(DataType.Date)]
        public DateTime DateFin { get; set; }
    }
}

