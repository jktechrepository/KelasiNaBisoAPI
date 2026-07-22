using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    /// <summary>
    /// DTO pour la modification des informations d'un élève
    /// </summary>
    public class UpdateEleveDto
    {
        [Required(ErrorMessage = "L'ID élève est obligatoire")]
        public int IdEleve { get; set; }
        
        // ═══════════════════════════════════════════════════════════
        // INFORMATIONS PERSONNELLES (Modifiables)
        // ═══════════════════════════════════════════════════════════
        
        [Required(ErrorMessage = "Le nom est obligatoire")]
        [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
        public string? Nom { get; set; }
        
        [StringLength(100, ErrorMessage = "Le post-nom ne peut pas dépasser 100 caractères")]
        public string? Postnom { get; set; }
        
        [Required(ErrorMessage = "Le prénom est obligatoire")]
        [StringLength(100, ErrorMessage = "Le prénom ne peut pas dépasser 100 caractères")]
        public string? Prenom { get; set; }
        
        [Required(ErrorMessage = "Le genre est obligatoire")]
        [RegularExpression("^(M|F)$", ErrorMessage = "Le genre doit être 'M' ou 'F'")]
        public string? Genre { get; set; }
        
        [Required(ErrorMessage = "La date de naissance est obligatoire")]
        [DataType(DataType.Date)]
        public DateTime? DateNaissance { get; set; }
        
        [StringLength(100, ErrorMessage = "Le lieu de naissance ne peut pas dépasser 100 caractères")]
        public string? LieuNaissance { get; set; }
        
        [StringLength(50, ErrorMessage = "La nationalité ne peut pas dépasser 50 caractères")]
        public string? Nationalite { get; set; }
        
      //  [Url(ErrorMessage = "Format d'URL invalide")]
       // [StringLength(500, ErrorMessage = "L'URL de la photo ne peut pas dépasser 500 caractères")]
        public string? PhotoUrl { get; set; }
        
        // Adresse
        [StringLength(100)]
        public string? Ville { get; set; }
        
        [StringLength(100)]
        public string? Commune { get; set; }
        
        [StringLength(100)]
        public string? Quartier { get; set; }
        
        [StringLength(100)]
        public string? Avenue { get; set; }
        
        [StringLength(20)]
        public string? Numero { get; set; }
        
        [StringLength(100)]
        public string? Province { get; set; }
        
        [StringLength(500)]
        public string? Commentaire { get; set; }
        
        // Tuteur et Classe (modifiables par admin)
        public int? IdTuteur { get; set; }
        public int? IdClasse { get; set; }
        
        // ═══════════════════════════════════════════════════════════
        // CHAMPS PROTÉGÉS (Non modifiables via cet endpoint)
        // ═══════════════════════════════════════════════════════════
        // ❌ Matricule          → Auto-généré, immuable
        // ❌ SerialNumber       → Endpoint dédié si nécessaire
        // ❌ ReferenceEleve     → Auto-généré (Guid), immuable
        // ❌ NomComplet         → Calculé automatiquement
        // ❌ Statut             → Endpoint dédié (/toggle-statut)
        // ❌ DateCreation       → Immuable (audit)
        // ❌ IdUtilisateur      → Géré automatiquement
    }
}

