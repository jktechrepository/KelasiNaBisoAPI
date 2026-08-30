using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    /// <summary>
    /// DTO pour la modification des informations d'un tuteur (parent)
    /// </summary>
    public class UpdateTuteurDto
    {
        [Required(ErrorMessage = "L'ID tuteur est obligatoire")]
        public int IdTuteur { get; set; }
        
        // ═══════════════════════════════════════════════════════════
        // INFORMATIONS PERSONNELLES (Modifiables)
        // ═══════════════════════════════════════════════════════════
        
        [Required(ErrorMessage = "Le nom complet est obligatoire")]
        [StringLength(200, ErrorMessage = "Le nom complet ne peut pas dépasser 200 caractères")]
        public string? NomComplet { get; set; }
        
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        [StringLength(256, ErrorMessage = "L'email ne peut pas dépasser 256 caractères")]
        public string? Email { get; set; }
        
        [Required(ErrorMessage = "Le téléphone est obligatoire")]
        [Phone(ErrorMessage = "Format de téléphone invalide")]
        [StringLength(20, ErrorMessage = "Le téléphone ne peut pas dépasser 20 caractères")]
        public string? Telephone { get; set; }
        
        [RegularExpression("^(M|F)$", ErrorMessage = "Le genre doit être 'M' ou 'F'")]
        public string? Genre { get; set; }
        
      //  [Url(ErrorMessage = "Format d'URL invalide")]        
        public string? PhotoTuteurUrl { get; set; }
        
       
        public string? PieceIdentiteTuteur { get; set; }
        
        // ═══════════════════════════════════════════════════════════
        // INFORMATIONS REPRÉSENTANT (Optionnelles)
        // ═══════════════════════════════════════════════════════════
        
        [StringLength(200, ErrorMessage = "Le nom complet du représentant ne peut pas dépasser 200 caractères")]
        public string? NomCompletRepresentant { get; set; }
        
        //[Phone(ErrorMessage = "Format de téléphone invalide")]
       // [StringLength(20, ErrorMessage = "Le téléphone du représentant ne peut pas dépasser 20 caractères")]
        public string? TelephoneRepresentant { get; set; }
        
        // ═══════════════════════════════════════════════════════════
        // CHAMPS PROTÉGÉS (Non modifiables via cet endpoint)
        // ═══════════════════════════════════════════════════════════
        // ❌ SerialNumber       → Auto-généré ou endpoint dédié
        // ❌ IdEcole            → Obsolète : école dérivée des inscriptions des enfants
        // ❌ Statut             → Endpoint dédié (/toggle-statut)
        // ❌ DateCreation       → Immuable (audit)
        // ❌ IdUtilisateur      → Géré automatiquement
    }
}

