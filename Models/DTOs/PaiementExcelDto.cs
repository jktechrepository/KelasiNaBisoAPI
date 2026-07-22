using System.ComponentModel.DataAnnotations;
using KelasiNaBiso.Models;

namespace KelasiNaBiso.Models.DTOs
{
    /// <summary>
    /// DTO pour représenter une ligne de paiement depuis un fichier Excel
    /// </summary>
    public class PaiementExcelDto
    {
        // Numéro de ligne dans le fichier Excel (pour le rapport d'erreurs)
        public int NumeroLigne { get; set; }

        // Données du paiement
        [Required(ErrorMessage = "La date de paiement est obligatoire")]
        public DateTime? DatePaiement { get; set; }
        
        [Required(ErrorMessage = "Le montant est obligatoire")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Le montant doit être supérieur à 0")]
        public double? Montant { get; set; }
        
        public string? Devise { get; set; } = "USD";
        
        public string? ModePaiement { get; set; } // Cash, Carte, Mobile Money
        
        public bool? Statut { get; set; } = true;
        
        public string? StatutPaiement { get; set; } = "Confirmé"; // En attente, Confirmé, Echoué
        
        public string? ReferenceTransaction { get; set; }
        
        public string? JustificatifUrl { get; set; }
        
        public string? Commentaire { get; set; }
        
        [Required(ErrorMessage = "L'ID de l'élève est obligatoire")]
        public int? IdEleve { get; set; }
        
        [Required(ErrorMessage = "L'ID des frais est obligatoire")]
        public int? IdFrais { get; set; }
        
        public int? IdUtilisateur { get; set; }

        // Liste des erreurs de validation pour cette ligne
        public List<string> Erreurs { get; set; } = new List<string>();

        /// <summary>
        /// Convertit ce DTO Excel en Paiement standard
        /// </summary>
        public Models.Paiement ToPaiement()
        {
            return new Models.Paiement
            {
                DatePaiement = DatePaiement ?? DateTime.Now,
                Montant = Montant ?? 0,
                Devise = Devise ?? "USD",
                ModePaiement = ModePaiement,
                Statut = Statut ?? true,
                StatutPaiement = StatutPaiement ?? "Confirmé",
                ReferenceTransaction = ReferenceTransaction,
                JustificatifUrl = JustificatifUrl,
                Commentaire = Commentaire,
                IdEleve = IdEleve,
                IdFrais = IdFrais,
                IdUtilisateur = IdUtilisateur,
                DateEnregistrement = DateTime.Now,
                ReferencePaiemenet = DateTime.Now.TimeOfDay.ToString().Replace(":", "").Replace(".", ""),
                DateCreation = DateTime.Now
            };
        }
    }

    /// <summary>
    /// Résultat du traitement d'un fichier Excel de paiements
    /// </summary>
    public class BulkPaiementResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TotalLignes { get; set; }
        public int LignesReussies { get; set; }
        public int LignesEchouees { get; set; }
        public int DoublonsDetectes { get; set; }
        public List<PaiementExcelDto> LignesAvecErreurs { get; set; } = new List<PaiementExcelDto>();
        public List<Models.Paiement> PaiementsCrees { get; set; } = new List<Models.Paiement>();
        public DateTime DateTraitement { get; set; } = DateTime.Now;
    }
}

