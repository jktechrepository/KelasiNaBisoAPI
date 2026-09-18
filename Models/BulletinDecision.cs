using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace KelasiNaBiso.Models
{
    /// <summary>
    /// Décision / appréciation manuelle du bulletin (titulaire ou direction).
    /// </summary>
    public class BulletinDecision
    {
        [Key]
        public int IdBulletinDecision { get; set; }

        [Required]
        public int IdEleve { get; set; }

        [Required]
        public int IdAnneeScolaire { get; set; }

        [Required]
        public int IdPeriode { get; set; }

        [MaxLength(100)]
        public string? Decision { get; set; }

        [MaxLength(1000)]
        public string? AppreciationGenerale { get; set; }

        /// <summary>IdUtilisateur auteur de la dernière saisie.</summary>
        public int? IdAuteur { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.UtcNow;
        public DateTime? DateModification { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(IdEleve))]
        public Eleve? Eleve { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(IdAnneeScolaire))]
        public AnneeScolaire? AnneeScolaire { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(IdPeriode))]
        public PeriodeCotation? PeriodeCotation { get; set; }
    }
}
