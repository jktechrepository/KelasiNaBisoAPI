using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    /// <summary>
    /// Wallet virtuel par école pour suivre les flux PayIn / PayOut MOKO.
    /// </summary>
    public class EcoleWallet
    {
        [Key]
        public int IdEcoleWallet { get; set; }

        [Required]
        public int IdEcole { get; set; }

        [Required]
        public int IdEcoleInfoPaiementMobile { get; set; }

        [Required]
        [MaxLength(10)]
        public string Devise { get; set; } = "CDF";

        /// <summary>Solde crédité après PayIn, en attente de libération (délai MOKO).</summary>
        public decimal SoldeEnAttente { get; set; }

        /// <summary>Solde disponible pour PayOut vers le bénéficiaire école.</summary>
        public decimal SoldeDisponible { get; set; }

        /// <summary>Total cumulé reçu via PayIn (net).</summary>
        public decimal TotalRecu { get; set; }

        /// <summary>Total cumulé reversé via PayOut.</summary>
        public decimal TotalReverse { get; set; }

        [JsonIgnore]
        public DateTime DateCreation { get; set; } = DateTime.Now;

        [JsonIgnore]
        public DateTime? DateModification { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public Ecole? Ecole { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public EcoleInfoPaiementMobile? InfoPaiementMobile { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<EcoleWalletMouvement> Mouvements { get; set; } = new List<EcoleWalletMouvement>();
    }
}
