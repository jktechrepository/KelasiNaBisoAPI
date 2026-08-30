using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class CreateInscriptionDto
    {
        // Données de l'inscription
        [Required]
        public string Type { get; set; } // "Inscription" ou "Réinscription"
        
        [Required]
        public int IdEcole { get; set; }
        
        [Required]
        public int IdClasse { get; set; }
        
        [Required]
        public int IdAnneeScolaire { get; set; }
        
        [Required]
        public DateTime DateInscription { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string StatutInscription { get; set; } = "Confirmé";

        // Données de l'élève
        [Required]
        public string NomEleve { get; set; }
        
        [Required]
        public string PostnomEleve { get; set; }
        
        [Required]
        public string PrenomEleve { get; set; }
        public string? PhotoEleveUrl { get; set; }
        public string? MatriculeEleve { get; set; }

        [Required]
        public string GenreEleve { get; set; }
        
        [Required]
        public DateTime DateNaissanceEleve { get; set; }
        
        [Required]
        public string LieuNaissanceEleve { get; set; }
        
        [Required]
        public string NationaliteEleve { get; set; }
        public string? ProvinceEleve { get; set; }
        public string? VilleEleve { get; set; }
        public string? CommuneEleve { get; set; }
        public string? QuartierEleve { get; set; }
        public string? AvenueEleve { get; set; }
        public string? NumeroEleve { get; set; }
        public string? CommentaireEleve { get; set; }


        // Données du tuteur
        [Required]
        public string NomCompletTuteur { get; set; }
        [Required]
        public string GenreTuteur { get; set; }
    
        public string? EmailTuteur { get; set; }
        
        [Phone]
        public string? TelephoneTuteur { get; set; }
        
        public string? NomCompletRepresentant { get; set; }
        
        public string? TelephoneRepresentant { get; set; }

        public string? PhotoTuteurUrl { get; set; }
        public string? PieceIdentiteTuteur { get; set; }

        // Pour les cas de réinscription
        private int? _idEleveExistant;
        private int? _idTuteurExistant;

        public int? IdEleveExistant 
        { 
            get => _idEleveExistant;
            set => _idEleveExistant = value.HasValue && value.Value > 0 ? value : null;
        }

        public int? IdTuteurExistant 
        { 
            get => _idTuteurExistant;
            set => _idTuteurExistant = value.HasValue && value.Value > 0 ? value : null;
        }
    }
}
