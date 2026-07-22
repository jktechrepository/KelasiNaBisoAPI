using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    /// <summary>
    /// DTO simplifié pour l'import Excel des paiements
    /// Format minimaliste avec nom complet de l'élève et libellé du frais
    /// </summary>
    public class PaiementExcelDtoSimple
    {
        [Required(ErrorMessage = "La date de paiement est obligatoire")]
        public DateTime DatePaiement { get; set; }

        [Required(ErrorMessage = "Le montant est obligatoire")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Le montant doit être supérieur à 0")]
        public decimal Montant { get; set; }

        [Required(ErrorMessage = "La devise est obligatoire")]
        [RegularExpression("^(USD|CDF|EUR)$", ErrorMessage = "La devise doit être USD, CDF ou EUR")]
        public string Devise { get; set; } = "USD";

        [Required(ErrorMessage = "Le mode de paiement est obligatoire")]
        [RegularExpression("^(Cash|Mobile Money|Carte|Virement|Chèque)$", 
            ErrorMessage = "Le mode de paiement doit être: Cash, Mobile Money, Carte, Virement ou Chèque")]
        public string ModePaiement { get; set; } = "Cash";

        [Required(ErrorMessage = "Le nom complet de l'élève est obligatoire")]
        [MinLength(3, ErrorMessage = "Le nom complet de l'élève doit contenir au moins 3 caractères")]
        public string NomCompletEleve { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le libellé du frais est obligatoire")]
        [MinLength(3, ErrorMessage = "Le libellé du frais doit contenir au moins 3 caractères")]
        public string LibelleFrais { get; set; } = string.Empty;

        // Propriétés calculées après validation
        public int? IdEleve { get; set; }
        public int? IdFrais { get; set; }
    }
}

