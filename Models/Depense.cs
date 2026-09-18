using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace KelasiNaBiso.Models
{
    /// <summary>Statuts workflow dépense (V1 : création → Validee uniquement).</summary>
    public static class DepenseStatuts
    {
        public const string EnAttente = "EnAttente";
        public const string Validee = "Validee";
        public const string Annulee = "Annulee";
        public const string Tous = "Tous";
    }

    public class Depense
    {
        [Key]
        public int IdDepense { get; set; }

        [Required]
        public int IdEcole { get; set; }

        [Required]
        public int IdCategorieDepense { get; set; }

        [Required]
        [MaxLength(200)]
        public string Libelle { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(200)]
        public string? Beneficiaire { get; set; }

        [MaxLength(100)]
        public string? ReferencePiece { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Montant { get; set; }

        [Required]
        [MaxLength(10)]
        public string CodeDeviseMontant { get; set; } = string.Empty;

        [MaxLength(10)]
        public string? CodeDevisePrincipale { get; set; }

        public decimal? TauxVersDevisePrincipale { get; set; }

        public decimal? MontantDevisePrincipale { get; set; }

        [MaxLength(50)]
        public string? ModePaiement { get; set; }

        public DateTime DateDepense { get; set; } = DateTime.UtcNow;

        /// <summary>Workflow : Validee | Annulee (| EnAttente réservé futur).</summary>
        [Required]
        [MaxLength(20)]
        public string StatutWorkflow { get; set; } = DepenseStatuts.Validee;

        /// <summary>Soft delete : false = masquée.</summary>
        public bool Actif { get; set; } = true;

        public int? IdUtilisateurCreateur { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? MotifAnnulation { get; set; }

        public int? IdUtilisateurAnnulation { get; set; }

        public DateTime? DateAnnulation { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(IdEcole))]
        public Ecole? Ecole { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(IdCategorieDepense))]
        public CategorieDepense? CategorieDepense { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(IdUtilisateurCreateur))]
        public Utilisateur? UtilisateurCreateur { get; set; }
    }
}
