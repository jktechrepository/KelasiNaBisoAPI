using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace KelasiNaBiso.Models
{
    /// <summary>Catégorie tarifaire d'élève (référentiel par école).</summary>
    public class CategorieEleveTarif
    {
        [Key]
        public int IdCategorieEleveTarif { get; set; }

        [Required]
        public int IdEcole { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Libelle { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool Statut { get; set; } = true;

        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(IdEcole))]
        public Ecole? Ecole { get; set; }
    }

    /// <summary>Affectation d'un élève à une catégorie tarifaire pour une année.</summary>
    public class AffectationEleveCategorieTarif
    {
        [Key]
        public int IdAffectationEleveCategorieTarif { get; set; }

        [Required]
        public int IdEleve { get; set; }

        [Required]
        public int IdCategorieEleveTarif { get; set; }

        [Required]
        public int IdAnneeScolaire { get; set; }

        [Required]
        public DateTime DateDebut { get; set; }

        public DateTime? DateFin { get; set; }

        [MaxLength(500)]
        public string? Motif { get; set; }

        public int? IdAuteur { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(IdEleve))]
        public Eleve? Eleve { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(IdCategorieEleveTarif))]
        public CategorieEleveTarif? Categorie { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(IdAnneeScolaire))]
        public AnneeScolaire? AnneeScolaire { get; set; }
    }

    /// <summary>Règle d'exonération par catégorie et frais (année scolaire).</summary>
    public class RegleExonerationFrais
    {
        [Key]
        public int IdRegleExonerationFrais { get; set; }

        [Required]
        public int IdEcole { get; set; }

        [Required]
        public int IdAnneeScolaire { get; set; }

        [Required]
        public int IdCategorieEleveTarif { get; set; }

        [Required]
        public int IdFrais { get; set; }

        /// <summary>Totale | Pourcentage | MontantReduction | MontantDuFixe</summary>
        [Required]
        [MaxLength(30)]
        public string TypeRegle { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Valeur { get; set; }

        public bool Statut { get; set; } = true;

        public int? IdAuteur { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        public DateTime? DateModification { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(IdEcole))]
        public Ecole? Ecole { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(IdAnneeScolaire))]
        public AnneeScolaire? AnneeScolaire { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(IdCategorieEleveTarif))]
        public CategorieEleveTarif? Categorie { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(IdFrais))]
        public Frais? Frais { get; set; }
    }
}
