using System.ComponentModel.DataAnnotations;
using KelasiNaBiso.Models;

namespace KelasiNaBiso.Models.DTOs
{
    /// <summary>DTO de création d'un tuteur (parent) avec compte utilisateur associé.</summary>
    public class CreateTuteurDto
    {
        [Required(ErrorMessage = "Le nom complet est obligatoire")]
        [StringLength(200, ErrorMessage = "Le nom complet ne peut pas dépasser 200 caractères")]
        public string NomComplet { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le genre est obligatoire")]
        [RegularExpression("^(M|F)$", ErrorMessage = "Le genre doit être 'M' ou 'F'")]
        public string Genre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le téléphone est obligatoire")]
        [Phone(ErrorMessage = "Format de téléphone invalide")]
        [StringLength(20, ErrorMessage = "Le téléphone ne peut pas dépasser 20 caractères")]
        public string Telephone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        [StringLength(256, ErrorMessage = "L'email ne peut pas dépasser 256 caractères")]
        public string? Email { get; set; }

        public string? PhotoTuteurUrl { get; set; }

        public string? PieceIdentiteTuteur { get; set; }

        [StringLength(200)]
        public string? NomCompletRepresentant { get; set; }

        public string? TelephoneRepresentant { get; set; }
    }

    /// <summary>Réponse POST /api/Tuteur : tuteur + compte Parent.</summary>
    public class CreateTuteurResultDto
    {
        public Tuteur Tuteur { get; set; } = null!;
        public UtilisateurInfo? CompteUtilisateur { get; set; }
    }
}
