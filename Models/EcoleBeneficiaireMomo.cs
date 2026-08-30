using KelasiNaBiso.Models.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    /// <summary>
    /// Numéro mobile money bénéficiaire d'une école pour le PayOut MOKO (customer_number au credit).
    /// </summary>
    public class EcoleBeneficiaireMomo
    {
        [Key]
        public int IdEcoleBeneficiaireMomo { get; set; }

        [Required]
        public int IdEcoleInfoPaiementMobile { get; set; }

        [Required]
        public int IdEcole { get; set; }

        /// <summary>Opérateur MOKO : airtel, orange, mpesa, africell, card.</summary>
        [Required]
        [MaxLength(20)]
        public string Methode { get; set; } = MomoOperators.Airtel;

        [Required]
        [MaxLength(20)]
        public string Numero { get; set; } = string.Empty;

        /// <summary>Nom affiché en back-office uniquement (jamais envoyé à MOKO en firstname/lastname).</summary>
        [MaxLength(150)]
        public string? NomTitulaire { get; set; }

        public bool EstPrincipal { get; set; }

        [MaxLength(20)]
        public string Statut { get; set; } = EcoleBeneficiaireStatuts.Actif;

        [JsonIgnore]
        public DateTime DateCreation { get; set; } = DateTime.Now;

        [JsonIgnore]
        public DateTime? DateModification { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public EcoleInfoPaiementMobile? InfoPaiementMobile { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public Ecole? Ecole { get; set; }
    }
}
