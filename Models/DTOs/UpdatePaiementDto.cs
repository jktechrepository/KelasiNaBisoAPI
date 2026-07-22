using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class UpdatePaiementDto
    {
        [Required]
        public int IdPaiement { get; set; }
        
        [Range(0, double.MaxValue)]
        public double Montant { get; set; }
        
        [StringLength(500)]
        public string? Commentaire { get; set; }
        
        [StringLength(500)]
        public string? JustificatifUrl { get; set; }
        
        [StringLength(100)]
        public string? ReferenceTransaction { get; set; }
        
        [StringLength(50)]
        public string? ModePaiement { get; set; }
        
        [StringLength(50)]
        public string? StatutPaiement { get; set; }
    }
}
