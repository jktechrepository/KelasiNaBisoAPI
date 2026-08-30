using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class CreateNoteDto
    {
        [Required]
        [Range(0, 100)]
        public double NoteObtenue { get; set; }

        [StringLength(500)]
        public string? Appreciation { get; set; }

        public DateTime? DateEvaluation { get; set; }

        [Required]
        public int IdProfesseur { get; set; }

        [Required]
        public int IdEleve { get; set; }

        [Required]
        public int IdEvaluation { get; set; }

        /// <summary>Optionnel : si ≤ 0, résolu depuis l'année courante de l'école de l'élève.</summary>
        public int IdAnneeScolaire { get; set; }
    }
}
