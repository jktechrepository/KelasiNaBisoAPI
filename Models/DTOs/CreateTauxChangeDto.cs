using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class CreateTauxChangeDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int IdEcole { get; set; }

        [Required]
        [StringLength(10)]
        public string? CodeDeviseSource { get; set; }

        [Required]
        [StringLength(10)]
        public string? CodeDeviseCible { get; set; }

        [Required]
        [Range(typeof(decimal), "0.00000001", "79228162514264337593543950335")]
        public decimal Taux { get; set; }

        public DateTime DateEffet { get; set; } = DateTime.UtcNow;

        public bool Statut { get; set; } = true;
    }
}
