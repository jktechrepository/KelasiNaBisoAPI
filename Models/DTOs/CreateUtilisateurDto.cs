using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    /// <summary>
    /// DTO pour la création d'un nouvel utilisateur (Admin uniquement)
    /// </summary>
    public class CreateUtilisateurDto
    {
        // ═══════════════════════════════════════════════════════════
        // INFORMATIONS PERSONNELLES (Obligatoires)
        // ═══════════════════════════════════════════════════════════
        
        [Required(ErrorMessage = "Le nom est obligatoire")]
        [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
        public string NomUtilisateur { get; set; } = string.Empty;
        
        [StringLength(100, ErrorMessage = "Le post-nom ne peut pas dépasser 100 caractères")]
        public string? PostNomUtilisateur { get; set; }
        
        [Required(ErrorMessage = "Le prénom est obligatoire")]
        [StringLength(100, ErrorMessage = "Le prénom ne peut pas dépasser 100 caractères")]
        public string PrenomUtilisateur { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "L'email est obligatoire")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        [StringLength(256, ErrorMessage = "L'email ne peut pas dépasser 256 caractères")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Le mot de passe est obligatoire")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Le mot de passe doit contenir au moins 5 caractères")]
        public string MotDePasse { get; set; } = string.Empty;
        
        [Phone(ErrorMessage = "Format de téléphone invalide")]
        [StringLength(20, ErrorMessage = "Le téléphone ne peut pas dépasser 20 caractères")]
        public string? Telephone { get; set; }
        
        // ═══════════════════════════════════════════════════════════
        // INFORMATIONS COMPLÉMENTAIRES (Optionnelles)
        // ═══════════════════════════════════════════════════════════
        
        //[Url(ErrorMessage = "Format d'URL invalide")]
       // [StringLength(500, ErrorMessage = "L'URL de la photo ne peut pas dépasser 500 caractères")]
        public string? PhotoUrl { get; set; }
        
        [StringLength(100, ErrorMessage = "Le lieu de naissance ne peut pas dépasser 100 caractères")]
        public string? LieuNaissance { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? DateNaissance { get; set; }
        
        [RegularExpression("^(M|F|Autre)$", ErrorMessage = "Le genre doit être 'M', 'F' ou 'Autre'")]
        public string? Genre { get; set; }
        
        // ═══════════════════════════════════════════════════════════
        // INFORMATIONS ADMINISTRATIVES (Gérées par l'Admin)
        // ═══════════════════════════════════════════════════════════
        
        [Required(ErrorMessage = "Le rôle est obligatoire")]
        [Range(1, int.MaxValue, ErrorMessage = "L'ID du rôle doit être valide")]
        public int IdRole { get; set; }
        
        [Required(ErrorMessage = "L'école est obligatoire")]
        [Range(1, int.MaxValue, ErrorMessage = "L'ID de l'école doit être valide")]
        public int IdEcole { get; set; }
        
        public bool? Statut { get; set; } = true; // Actif par défaut
        
        // ═══════════════════════════════════════════════════════════
        // CHAMPS AUTO-GÉNÉRÉS (Non inclus - Gérés par le système)
        // ═══════════════════════════════════════════════════════════
        // ✅ ReferenceUtilisateur  → Généré automatiquement (Guid)
        // ✅ DefaultUsername       → Généré automatiquement (prenom.nom)
        // ✅ DateCreation          → DateTime.UtcNow
        // ✅ IsConnecte            → false par défaut
        // ✅ FcmToken              → null (ajouté à la connexion)
    }
}

