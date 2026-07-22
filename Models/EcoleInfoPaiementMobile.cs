using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    /// <summary>
    /// Configuration des paiements Mobile Money / carte MOKO pour une école (site).
    /// Une école = un site marchand côté reversement PayOut.
    /// </summary>
    public class EcoleInfoPaiementMobile
    {
        [Key]
        public int IdEcoleInfoPaiementMobile { get; set; }

        [Required]
        public int IdEcole { get; set; }

        /// <summary>Active les paiements Mobile Money via MOKO pour cette école.</summary>
        public bool MobileMoneyActif { get; set; }

        /// <summary>Active les paiements par carte via MOKO pour cette école.</summary>
        public bool CarteActif { get; set; }

        [Required]
        [MaxLength(10)]
        public string Devise { get; set; } = "CDF";

        /// <summary>Délai avant libération du solde wallet et déclenchement PayOut (minutes).</summary>
        public int DelaiReglementMinutes { get; set; } = 3;

        /// <summary>Lance automatiquement le PayOut après libération du solde.</summary>
        public bool PayoutAutomatique { get; set; } = true;

        public bool Statut { get; set; } = true;

        [JsonIgnore]
        public DateTime DateCreation { get; set; } = DateTime.Now;

        [JsonIgnore]
        public DateTime? DateModification { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public Ecole? Ecole { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<EcoleBeneficiaireMomo> Beneficiaires { get; set; } = new List<EcoleBeneficiaireMomo>();

        [JsonIgnore]
        [ValidateNever]
        public EcoleWallet? Wallet { get; set; }
    }
}
