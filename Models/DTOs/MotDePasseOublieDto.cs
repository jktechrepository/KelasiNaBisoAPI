using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class MotDePasseOublieRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class ConfirmerMotDePasseOublieRequest
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [MinLength(5, ErrorMessage = "Le mot de passe doit contenir au moins 5 caractères.")]
        public string NouveauMotDePasse { get; set; } = string.Empty;
    }
}


