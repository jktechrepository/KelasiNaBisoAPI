using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class BulkNoteRequestDto
    {
        [Required]
        public int IdEvaluation { get; set; }

        /// <summary>Optionnel : si ≤ 0, résolu depuis l'école de la classe de l'évaluation.</summary>
        public int IdAnneeScolaire { get; set; }

        public DateTime? DateEvaluation { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Au moins une ligne est requise.")]
        [MaxLength(80, ErrorMessage = "Maximum 80 notes par lot.")]
        public List<BulkNoteLigneDto> Lignes { get; set; } = new();
    }

    public class BulkNoteLigneDto
    {
        [Required]
        public int IdEleve { get; set; }

        [Required]
        [Range(0, 100)]
        public double NoteObtenue { get; set; }

        [StringLength(500)]
        public string? Appreciation { get; set; }
    }

    public class BulkNoteResultDto
    {
        public int IdEvaluation { get; set; }
        public int IdAnneeScolaire { get; set; }
        public int Created { get; set; }
        public int Updated { get; set; }
        public IReadOnlyList<NoteSummaryDto> Notes { get; set; } = Array.Empty<NoteSummaryDto>();
    }

    public class NoteSummaryDto
    {
        public int IdNote { get; set; }
        public int IdEleve { get; set; }
        public double NoteObtenue { get; set; }
        public string? Appreciation { get; set; }
        public string Action { get; set; } = string.Empty; // created | updated
    }
}
