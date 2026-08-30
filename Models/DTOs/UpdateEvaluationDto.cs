using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class UpdateEvaluationDto
    {
        [Required]
        public int IdEvaluation { get; set; }
        
        [StringLength(50)]
        public string? TypeEvaluation { get; set; }

        [StringLength(200)]
        public string? TitreEvaluation { get; set; }

        [StringLength(255)]
        public string? Periode { get; set; }
        
        public double? Coefficient { get; set; }
        
        [Required]
        public int IdCours { get; set; }
        
        [Required]
        public int IdClasse { get; set; }
    }
}
