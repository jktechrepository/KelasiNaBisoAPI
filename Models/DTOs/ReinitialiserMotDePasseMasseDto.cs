using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    /// <summary>
    /// DTO pour réinitialiser le mot de passe en masse (tous les utilisateurs d'une école avec un rôle spécifique)
    /// </summary>
    public class ReinitialiserMotDePasseMasseDto
    {
        [Required(ErrorMessage = "L'ID de l'école est requis")]
        public int IdEcole { get; set; }

        [Required(ErrorMessage = "L'ID du rôle est requis")]
        public int IdRole { get; set; }

        [Required(ErrorMessage = "Le nouveau mot de passe est requis")]
        [MinLength(5, ErrorMessage = "Le mot de passe doit contenir au moins 5 caractères")]
        public string NouveauMotDePasse { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO pour réinitialiser le mot de passe d'un utilisateur spécifique
    /// </summary>
    public class ReinitialiserMotDePasseIndividuelDto
    {
        [Required(ErrorMessage = "L'ID de l'utilisateur est requis")]
        public int IdUtilisateur { get; set; }

        [Required(ErrorMessage = "Le nouveau mot de passe est requis")]
        [MinLength(5, ErrorMessage = "Le mot de passe doit contenir au moins 5 caractères")]
        public string NouveauMotDePasse { get; set; } = string.Empty;
    }

    /// <summary>
    /// Réponse de réinitialisation de mot de passe en masse
    /// </summary>
    public class ReinitialiserMotDePasseMasseResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int NombreUtilisateurs { get; set; }
        public DetailsReinitialisation Details { get; set; } = new();
    }

    /// <summary>
    /// Réponse de réinitialisation de mot de passe individuelle
    /// </summary>
    public class ReinitialiserMotDePasseIndividuelResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UtilisateurReinitialise? Utilisateur { get; set; }
    }

    /// <summary>
    /// Détails de la réinitialisation en masse
    /// </summary>
    public class DetailsReinitialisation
    {
        public string Ecole { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool MotDePasseChange { get; set; }
        public bool DoitChangerMotDePasse { get; set; }
    }

    /// <summary>
    /// Informations de l'utilisateur dont le mot de passe a été réinitialisé
    /// </summary>
    public class UtilisateurReinitialise
    {
        public int IdUtilisateur { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public bool DoitChangerMotDePasse { get; set; }
    }
}

