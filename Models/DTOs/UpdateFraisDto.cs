using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class UpdateFraisDto
    {
        [Required]
        public int IdFrais { get; set; }
        
        [Required]
        [StringLength(200)]
        public string? LibelleFrais { get; set; }
        
        [Range(0, double.MaxValue)]
        public double Montant { get; set; }
        
        [StringLength(10)]
        public string? Devise { get; set; }
        
        [StringLength(200)]
        public string? TypeFrais { get; set; }
        
        [StringLength(200)]
        public string? Periodicite { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }

        public int? IdAnneeScolaire { get; set; }

        public int? IdClasse { get; set; }
    }
}
