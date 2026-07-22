using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    /// <summary>
    /// DTO pour la requête de changement de mot de passe
    /// Adapté d'AkademiaAPI
    /// </summary>
    public class ChangerMotDePasseRequest
    {
        [Required(ErrorMessage = "L'ID utilisateur est requis")]
        public int IdUtilisateur { get; set; }

        [Required(ErrorMessage = "L'ancien mot de passe est requis")]
        public string? AncienMotDePasse { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nouveau mot de passe est requis")]
        [MinLength(5, ErrorMessage = "Le nouveau mot de passe doit contenir au moins 5 caractères")]
        public string? NouveauMotDePasse { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmation du nouveau mot de passe est requise")]
        [Compare("NouveauMotDePasse", ErrorMessage = "La confirmation du mot de passe ne correspond pas")]
        public string? ConfirmerNouveauMotDePasse { get; set; } = string.Empty;
    }
}

