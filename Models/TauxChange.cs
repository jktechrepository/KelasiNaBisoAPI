using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models
{
    public class TauxChange
    {
        [Key]
        public int IdTauxChange { get; set; }

        [Required]
        public int IdEcole { get; set; }

        [Required]
        [StringLength(10)]
        public string CodeDeviseSource { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string CodeDeviseCible { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.00000001", "79228162514264337593543950335")]
        public decimal Taux { get; set; }

        public DateTime DateEffet { get; set; } = DateTime.UtcNow;

        public bool Statut { get; set; } = true;

        public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    }
}
