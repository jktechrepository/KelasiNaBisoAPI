using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class CategorieEleveTarifDto
    {
        public int IdCategorieEleveTarif { get; set; }
        public int IdEcole { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Libelle { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Statut { get; set; }
    }

    public class CreateCategorieEleveTarifDto
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Libelle { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool Statut { get; set; } = true;
    }

    public class UpdateCategorieEleveTarifDto
    {
        [Required]
        [MaxLength(150)]
        public string Libelle { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool Statut { get; set; } = true;
    }

    public class AffectationEleveCategorieTarifDto
    {
        public int IdAffectationEleveCategorieTarif { get; set; }
        public int IdEleve { get; set; }
        public int IdCategorieEleveTarif { get; set; }
        public string? CodeCategorie { get; set; }
        public string? LibelleCategorie { get; set; }
        public int IdAnneeScolaire { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public string? Motif { get; set; }
    }

    public class CreateAffectationEleveCategorieTarifDto
    {
        [Required]
        public int IdEleve { get; set; }

        [Required]
        public int IdCategorieEleveTarif { get; set; }

        [Required]
        public int IdAnneeScolaire { get; set; }

        public DateTime? DateDebut { get; set; }

        [MaxLength(500)]
        public string? Motif { get; set; }
    }

    public class RegleExonerationFraisDto
    {
        public int IdRegleExonerationFrais { get; set; }
        public int IdEcole { get; set; }
        public int IdAnneeScolaire { get; set; }
        public int IdCategorieEleveTarif { get; set; }
        public string? CodeCategorie { get; set; }
        public int IdFrais { get; set; }
        public string? LibelleFrais { get; set; }
        public string TypeRegle { get; set; } = string.Empty;
        public decimal Valeur { get; set; }
        public bool Statut { get; set; }
    }

    public class UpsertRegleExonerationFraisDto
    {
        [Required]
        public int IdAnneeScolaire { get; set; }

        [Required]
        public int IdCategorieEleveTarif { get; set; }

        [Required]
        public int IdFrais { get; set; }

        [Required]
        [MaxLength(30)]
        public string TypeRegle { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal Valeur { get; set; }

        public bool Statut { get; set; } = true;
    }

    /// <summary>Détail du dû pour un couple élève / frais (audit / UI).</summary>
    public class FraisDuDetailDto
    {
        public int IdEleve { get; set; }
        public int IdFrais { get; set; }
        public string? LibelleFrais { get; set; }
        public string? CodeDevise { get; set; }
        public decimal MontantCatalogue { get; set; }
        public decimal MontantDuEffectif { get; set; }
        public decimal MontantReduction { get; set; }
        public decimal MontantPaye { get; set; }
        public decimal ResteAPayer { get; set; }
        public int? IdCategorieEleveTarif { get; set; }
        public string? CodeCategorie { get; set; }
        public string? LibelleCategorie { get; set; }
        public string? TypeRegle { get; set; }
        public decimal? ValeurRegle { get; set; }
    }
}
