using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class CreateDeviseMonetaireDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int IdEcole { get; set; }

        [Required]
        [StringLength(10)]
        public string? CodeDevise { get; set; }

        [Required]
        [StringLength(120)]
        public string? Libelle { get; set; }

        [StringLength(10)]
        public string? Symbole { get; set; }

        public bool Statut { get; set; } = true;
    }

    public class UpdateDeviseMonetaireDto
    {
        [Required]
        [StringLength(120)]
        public string? Libelle { get; set; }

        [StringLength(10)]
        public string? Symbole { get; set; }

        public bool Statut { get; set; } = true;
    }

    public class DeviseMonetaireDto
    {
        public int IdDeviseMonetaire { get; set; }
        public int IdEcole { get; set; }
        public string CodeDevise { get; set; } = string.Empty;
        public string Libelle { get; set; } = string.Empty;
        public string? Symbole { get; set; }
        public bool Statut { get; set; }
        public DateTime DateCreation { get; set; }
    }
}
