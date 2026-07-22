using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class UpdateVacationDto
    {
        [Required]
        public int IdVacation { get; set; }
        
        [Required]
        [StringLength(50)]
        public string? NomVacation { get; set; }
        
        [Required]
        public TimeSpan HeureDebut { get; set; }
        
        [Required]
        public TimeSpan HeureFin { get; set; }
        
        public TimeSpan? HeureDebutPause { get; set; }
        public TimeSpan? HeureFinPause { get; set; }
        public int NombreJoursParSemaine { get; set; } = 5;
    }
}

