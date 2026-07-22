using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class UpdateTitulaireClasseDto
    {
        [Required]
        public int IdTitulaireClasse { get; set; }
        
        [Required]
        public int IdAgent { get; set; }
        
        [Required]
        public int IdClasse { get; set; }
        
        [Required]
        public int IdAnneeScolaire { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? DateDebut { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? DateFin { get; set; }
        
        public bool? Statut { get; set; } = true;
        
        [StringLength(500)]
        public string? Commentaire { get; set; }
    }
}

