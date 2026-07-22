using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs.DevoirADomicile
{
    /// <summary>
    /// DTO pour la création d'un devoir à domicile
    /// Le fichier sera dans FormData, pas dans le DTO
    /// </summary>
    public class CreateDevoirADomicileDto
    {
        [Required(ErrorMessage = "Le titre est obligatoire")]
        [MaxLength(200, ErrorMessage = "Le titre ne peut pas dépasser 200 caractères")]
        public string Titre { get; set; } = string.Empty;
        
        [MaxLength(1000, ErrorMessage = "La description ne peut pas dépasser 1000 caractères")]
        public string? Description { get; set; }
        
        [MaxLength(5000, ErrorMessage = "Le contenu ne peut pas dépasser 5000 caractères")]
        public string? Contenu { get; set; } // Contenu textuel du devoir (optionnel si fichier fourni)
        
        [Required(ErrorMessage = "L'ID de l'école est obligatoire")]
        public int IdEcole { get; set; }
        
        [Required(ErrorMessage = "L'ID de la direction est obligatoire")]
        public int IdDirection { get; set; }
        
        [Required(ErrorMessage = "L'ID de la classe est obligatoire")]
        public int IdClasse { get; set; }
        
        public int? IdCours { get; set; } // Optionnel
        
        [Range(1, int.MaxValue, ErrorMessage = "Le coefficient doit être supérieur à 0")]
        public int CoefficientDevoir { get; set; } = 1; // Coefficient pour l'évaluation associée
        
        public DateTime? DateLimite { get; set; } // Optionnel : date limite de remise
    }
}

