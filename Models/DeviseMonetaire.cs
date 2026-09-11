using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models
{
    public class DeviseMonetaire
    {
        [Key]
        public int IdDeviseMonetaire { get; set; }

        [Required]
        public int IdEcole { get; set; }

        [Required]
        [StringLength(10)]
        public string CodeDevise { get; set; } = string.Empty;

        [Required]
        [StringLength(120)]
        public string Libelle { get; set; } = string.Empty;

        [StringLength(10)]
        public string? Symbole { get; set; }

        public bool Statut { get; set; } = true;

        public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    }
}
