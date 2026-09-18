using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace KelasiNaBiso.Models
{
    /// <summary>
    /// Référentiel global des périodes de cotation (trimestriel RDC : T1 / T2 / T3).
    /// </summary>
    public class PeriodeCotation
    {
        [Key]
        public int IdPeriode { get; set; }

        /// <summary>Code court normalisé (ex. T1).</summary>
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>Libellé affiché (ex. Trimestre 1).</summary>
        [Required]
        [MaxLength(100)]
        public string Libelle { get; set; } = string.Empty;

        /// <summary>Ordre d'affichage / chronologique.</summary>
        public int Ordre { get; set; }

        public bool Statut { get; set; } = true;

        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        [ValidateNever]
        public ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
    }
}
