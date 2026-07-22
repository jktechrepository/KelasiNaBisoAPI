using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    /// <summary>
    /// DTO pour afficher un paiement échoué
    /// </summary>
    public class PaiementCrashedDto
    {
        public int IdPaiementCrashed { get; set; }
        public DateTime? DatePaiement { get; set; }
        public double? Montant { get; set; }
        public string? Devise { get; set; }
        public string? ModePaiement { get; set; }
        public string? StatutPaiement { get; set; }
        public string? ReferenceTransaction { get; set; }
        public string? Commentaire { get; set; }
        public int? IdEleve { get; set; }
        public int? IdFrais { get; set; }
        public string? NomCompletEleve { get; set; }
        public string? LibelleFrais { get; set; }
        public List<string> Erreurs { get; set; } = new List<string>();
        public int NumeroLigne { get; set; }
        public string? NomFichierOriginal { get; set; }
        public DateTime? DateEchec { get; set; }
        public DateTime? DateCorrection { get; set; }
        public DateTime? DateReinjection { get; set; }
        public int? IdPaiementCree { get; set; }
        public bool EstResolu { get; set; }
        
        // Informations enrichies (optionnel)
        public string? NomEleve { get; set; }
        public string? NomFrais { get; set; }
    }

    /// <summary>
    /// DTO pour modifier un paiement échoué
    /// </summary>
    public class UpdatePaiementCrashedDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "L'ID de l'élève doit être supérieur à 0")]
        public int? IdEleve { get; set; }
        
        [Range(1, int.MaxValue, ErrorMessage = "L'ID des frais doit être supérieur à 0")]
        public int? IdFrais { get; set; }
        
        public DateTime? DatePaiement { get; set; }
        
        [Range(0.01, double.MaxValue, ErrorMessage = "Le montant doit être supérieur à 0")]
        public double? Montant { get; set; }
        
        [MaxLength(10)]
        public string? Devise { get; set; }
        
        [MaxLength(50)]
        public string? ModePaiement { get; set; }
        
        [MaxLength(50)]
        public string? StatutPaiement { get; set; }
        
        [MaxLength(200)]
        public string? ReferenceTransaction { get; set; }
        
        [MaxLength(1000)]
        public string? Commentaire { get; set; }
    }

    /// <summary>
    /// DTO pour modification globale de plusieurs paiements échoués
    /// </summary>
    public class BulkUpdatePaiementCrashedDto
    {
        [Required(ErrorMessage = "La liste des IDs est obligatoire")]
        public List<int> Ids { get; set; } = new List<int>();
        
        // Champs qui peuvent être modifiés en masse
        public int? IdEleve { get; set; }
        public int? IdFrais { get; set; }
        public DateTime? DatePaiement { get; set; }
        public double? Montant { get; set; }
        public string? Devise { get; set; }
        public string? ModePaiement { get; set; }
        public string? StatutPaiement { get; set; }
    }

    /// <summary>
    /// DTO pour réinjection de paiements échoués
    /// </summary>
    public class ReinjectPaiementCrashedDto
    {
        [Required(ErrorMessage = "La liste des IDs est obligatoire")]
        public List<int> Ids { get; set; } = new List<int>();
        
        // Optionnel : forcer la réinjection même si des erreurs persistent
        public bool ForcerReinjection { get; set; } = false;
    }

    /// <summary>
    /// Résultat de la réinjection
    /// </summary>
    public class ReinjectPaiementCrashedResult
    {
        public int TotalTentes { get; set; }
        public int Reussis { get; set; }
        public int Echoues { get; set; }
        public List<int> IdsReussis { get; set; } = new List<int>();
        public List<PaiementCrashedDto> PaiementsEchoues { get; set; } = new List<PaiementCrashedDto>();
        public string Message { get; set; } = string.Empty;
    }
}

