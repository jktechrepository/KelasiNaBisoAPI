using System.ComponentModel.DataAnnotations;
using KelasiNaBiso.Models.Enums;

namespace KelasiNaBiso.Models.DTOs
{
    public class CreateFraisDto
    {
        [Required]
        [StringLength(200)]
        public string LibelleFrais { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public double Montant { get; set; }

        [Required]
        [StringLength(10)]
        public string Devise { get; set; } = "USD";

        [StringLength(200)]
        public string? TypeFrais { get; set; }

        [StringLength(200)]
        public string? Periodicite { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public int IdEcole { get; set; }

        /// <summary>0 = année courante de l'école.</summary>
        public int IdAnneeScolaire { get; set; }

        [Required]
        public PorteeFrais Portee { get; set; }

        public List<int>? IdDirections { get; set; }

        public List<int>? IdClasses { get; set; }

        public bool? Statut { get; set; } = true;
    }
}
