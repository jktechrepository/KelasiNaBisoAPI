using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KelasiNaBiso.Models
{
    public class Paiement
    {
        [Key]
        public int IdPaiement { get; set; }
        public DateTime DatePaiement { get; set; }
        public double Montant { get; set; }
        public string? Devise { get; set; } = "USD";
        public string? CodeDevisePaiement { get; set; }
        public string? CodeDevisePrincipale { get; set; }
        [Column(TypeName = "decimal(18,8)")]
        public decimal? TauxVersDevisePrincipale { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MontantPayeDevisePrincipale { get; set; }
        public string? ModePaiement { get; set; }  // Par defaut, "Cash, Carte, Mobile Money"
        
        // ✅ SOFT DELETE: Statut actif/inactif (true = actif, false = désactivé)
        public bool? Statut { get; set; } = true;
        
        public string? StatutPaiement { get; set; } = string.Empty; // En attente, Confirme, Echoue
        public string? ReferenceTransaction { get; set; }

        /// <summary>Montant net frais scolaire (hors frais MOKO).</summary>
        public decimal? MontantNet { get; set; }

        /// <summary>Montant total collecté au PayIn (net + frais MOKO).</summary>
        public decimal? MontantCollecte { get; set; }

        /// <summary>Opérateur MOKO : airtel, orange, mpesa, africell, card.</summary>
        [MaxLength(20)]
        public string? OperateurMobileMoney { get; set; }

        public string? JustificatifUrl { get; set; }
        public string? Commentaire { get; set; }
        public DateTime DateEnregistrement { get; set; } = DateTime.Now;
        public string ReferencePaiemenet { get; set; } = DateTime.Now.TimeOfDay.ToString().Replace(":", "").Replace(".","");
        public int? IdFrais { get; set; }
        public int? IdEleve { get; set; }
        public int? IdUtilisateur { get; set; }

        // Attributs Techniques
        [JsonIgnore]
        public DateTime DateCreation { get; set; } = DateTime.Now;

        // Navigation properties
        [JsonIgnore]
        [ValidateNever]
        public Utilisateur? Utilisateur { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public Eleve? Eleve { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public Frais? Frais { get; set; }


    }
}
