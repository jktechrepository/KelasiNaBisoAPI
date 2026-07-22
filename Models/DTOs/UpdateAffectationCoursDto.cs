using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class UpdateAffectationCoursDto
    {
        [Required]
        public int IdAffectationCours { get; set; }
        
        [Required]
        public int IdAgent { get; set; }
        
        [Required]
        public int IdCours { get; set; }
        
        [Required]
        public int IdAnneeScolaire { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? DateAffectation { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? DateFinAffectation { get; set; }
        
        public bool? Statut { get; set; } = true;
        
        [StringLength(500)]
        public string? Commentaire { get; set; }
    }
}

