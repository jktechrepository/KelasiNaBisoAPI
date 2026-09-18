using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace KelasiNaBiso.Models
{
    /// <summary>
    /// Snapshot immuable d'un bulletin après validation (figé).
    /// </summary>
    public class BulletinFige
    {
        [Key]
        public int IdBulletinFige { get; set; }

        [Required]
        public int IdEleve { get; set; }

        [Required]
        public int IdAnneeScolaire { get; set; }

        [Required]
        public int IdPeriode { get; set; }

        [Required]
        public string PayloadJson { get; set; } = string.Empty;

        public double? MoyenneGenerale { get; set; }
        public int? Rang { get; set; }
        public int EffectifClasse { get; set; }

        [MaxLength(100)]
        public string? Decision { get; set; }

        [MaxLength(1000)]
        public string? AppreciationGenerale { get; set; }

        public int? IdAuteurValidation { get; set; }
        public DateTime DateValidation { get; set; } = DateTime.UtcNow;

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
