using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs.Vitrine
{
    /// <summary>Formulaire de contact — site vitrine kelasinabiso.com</summary>
    public class ContactVitrineRequestDto
    {
        [Required(ErrorMessage = "Le nom complet est obligatoire.")]
        [MaxLength(150)]
        public string NomComplet { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Etablissement { get; set; }

        [MaxLength(30)]
        public string? Telephone { get; set; }

        [Required(ErrorMessage = "L'adresse email est obligatoire.")]
        [EmailAddress(ErrorMessage = "Adresse email invalide.")]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Effectif { get; set; }

        [MaxLength(150)]
        public string? Objet { get; set; }

        [Required(ErrorMessage = "Le message est obligatoire.")]
        [MinLength(10, ErrorMessage = "Le message doit contenir au moins 10 caractères.")]
        [MaxLength(5000)]
        public string Message { get; set; } = string.Empty;

        /// <summary>Champ honeypot anti-spam — doit rester vide.</summary>
        [MaxLength(200)]
        public string? Website { get; set; }
    }
}
