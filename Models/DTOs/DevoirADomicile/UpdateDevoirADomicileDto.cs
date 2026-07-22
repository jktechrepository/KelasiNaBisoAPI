using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs.DevoirADomicile
{
    /// <summary>
    /// DTO pour la mise à jour d'un devoir à domicile
    /// Note : Le fichier ne peut pas être modifié (supprimer + recréer si besoin)
    /// </summary>
    public class UpdateDevoirADomicileDto
    {
        [Required(ErrorMessage = "Le titre est obligatoire")]
        [MaxLength(200, ErrorMessage = "Le titre ne peut pas dépasser 200 caractères")]
        public string Titre { get; set; } = string.Empty;
        
        [MaxLength(1000, ErrorMessage = "La description ne peut pas dépasser 1000 caractères")]
        public string? Description { get; set; }
        
        public DateTime? DateLimite { get; set; } // Optionnel : date limite de remise
        
        public bool Statut { get; set; } = true; // true = actif, false = archivé
    }
}

