using KelasiNaBiso.Models.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    public class Frais
    {
        [Key]
        public int IdFrais { get; set; }
        [Required]
        [MaxLength(200)]
        public string LibelleFrais { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public double Montant { get; set; }
        [Required]
        [MaxLength(10)]
        public string Devise { get; set; }

        public string? TypeFrais { get; set; }

        public string? Periodicite { get; set; }

        public string? Description { get; set; }

        public bool? Statut { get; set; } = true;

        [Required]
        public int IdEcole { get; set; }

        [Required]
        public int IdAnneeScolaire { get; set; }

        [Required]
        public PorteeFrais Portee { get; set; }

        [JsonIgnore]
        public DateTime DateCreation { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public Ecole Ecole { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public AnneeScolaire AnneeScolaire { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<FraisDirection> FraisDirections { get; set; } = new List<FraisDirection>();

        [JsonIgnore]
        [ValidateNever]
        public ICollection<FraisClasse> FraisClasses { get; set; } = new List<FraisClasse>();

        [JsonIgnore]
        [ValidateNever]
        public ICollection<Paiement> Paiements { get; set; }
    }
}
