using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    /// <summary>
    /// DTO pour représenter une ligne d'inscription depuis un fichier Excel
    /// </summary>
    public class InscriptionExcelDto
    {
        // Numéro de ligne dans le fichier Excel (pour le rapport d'erreurs)
        public int NumeroLigne { get; set; }

        // Données de l'inscription
        [Required(ErrorMessage = "Le type d'inscription est obligatoire")]
        public string Type { get; set; } = "Inscription"; // "Inscription" ou "Réinscription"
        
        [Required(ErrorMessage = "L'ID de l'école est obligatoire")]
        public int? IdEcole { get; set; }
        
        [Required(ErrorMessage = "L'ID de la classe est obligatoire")]
        public int? IdClasse { get; set; }
        
        [Required(ErrorMessage = "L'ID de l'année scolaire est obligatoire")]
        public int? IdAnneeScolaire { get; set; }
        
        public DateTime? DateInscription { get; set; }
        
        public string StatutInscription { get; set; } = "Confirmé";

        // Données de l'élève
        [Required(ErrorMessage = "Le nom de l'élève est obligatoire")]
        public string NomEleve { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Le postnom de l'élève est obligatoire")]
        public string PostnomEleve { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Le prénom de l'élève est obligatoire")]
        public string PrenomEleve { get; set; } = string.Empty;
        
        public string? PhotoEleveUrl { get; set; }
        public string? MatriculeEleve { get; set; }

        [Required(ErrorMessage = "Le genre de l'élève est obligatoire")]
        public string GenreEleve { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "La date de naissance de l'élève est obligatoire")]
        public DateTime? DateNaissanceEleve { get; set; }
        
        [Required(ErrorMessage = "Le lieu de naissance de l'élève est obligatoire")]
        public string LieuNaissanceEleve { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "La nationalité de l'élève est obligatoire")]
        public string NationaliteEleve { get; set; } = string.Empty;
        
        public string? ProvinceEleve { get; set; }
        public string? VilleEleve { get; set; }
        public string? CommuneEleve { get; set; }
        public string? QuartierEleve { get; set; }
        public string? AvenueEleve { get; set; }
        public string? NumeroEleve { get; set; }
        public string? CommentaireEleve { get; set; }

        // Données du tuteur
        [Required(ErrorMessage = "Le nom complet du tuteur est obligatoire")]
        public string NomCompletTuteur { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Le genre du tuteur est obligatoire")]
        public string GenreTuteur { get; set; } = string.Empty;
    
        public string? EmailTuteur { get; set; }
        
        public string? TelephoneTuteur { get; set; }
        
        public string? NomCompletRepresentant { get; set; }
        
        public string? TelephoneRepresentant { get; set; }

        public string? PhotoTuteurUrl { get; set; }
        public string? PieceIdentiteTuteur { get; set; }

        // Pour les cas de réinscription
        public int? IdEleveExistant { get; set; }
        public int? IdTuteurExistant { get; set; }

        // Liste des erreurs de validation pour cette ligne
        public List<string> Erreurs { get; set; } = new List<string>();

        /// <summary>
        /// Convertit ce DTO Excel en CreateInscriptionDto standard
        /// </summary>
        public CreateInscriptionDto ToCreateInscriptionDto()
        {
            return new CreateInscriptionDto
            {
                Type = Type,
                IdEcole = IdEcole ?? 0,
                IdClasse = IdClasse ?? 0,
                IdAnneeScolaire = IdAnneeScolaire ?? 0,
                DateInscription = DateInscription ?? DateTime.Now,
                StatutInscription = StatutInscription,
                NomEleve = NomEleve,
                PostnomEleve = PostnomEleve,
                PrenomEleve = PrenomEleve,
                PhotoEleveUrl = PhotoEleveUrl,
                MatriculeEleve = MatriculeEleve,
                GenreEleve = GenreEleve,
                DateNaissanceEleve = DateNaissanceEleve ?? DateTime.MinValue,
                LieuNaissanceEleve = LieuNaissanceEleve,
                NationaliteEleve = NationaliteEleve,
                ProvinceEleve = ProvinceEleve,
                VilleEleve = VilleEleve,
                CommuneEleve = CommuneEleve,
                QuartierEleve = QuartierEleve,
                AvenueEleve = AvenueEleve,
                NumeroEleve = NumeroEleve,
                CommentaireEleve = CommentaireEleve,
                NomCompletTuteur = NomCompletTuteur,
                GenreTuteur = GenreTuteur,
                EmailTuteur = EmailTuteur,
                TelephoneTuteur = TelephoneTuteur,
                NomCompletRepresentant = NomCompletRepresentant,
                TelephoneRepresentant = TelephoneRepresentant,
                PhotoTuteurUrl = PhotoTuteurUrl,
                PieceIdentiteTuteur = PieceIdentiteTuteur,
                IdEleveExistant = IdEleveExistant,
                IdTuteurExistant = IdTuteurExistant
            };
        }
    }

    /// <summary>
    /// Résultat du traitement d'un fichier Excel d'inscriptions
    /// </summary>
    public class BulkInscriptionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TotalLignes { get; set; }
        public int LignesReussies { get; set; }
        public int LignesEchouees { get; set; }
        public int DoublonsDetectes { get; set; }
        public List<InscriptionExcelDto> LignesAvecErreurs { get; set; } = new List<InscriptionExcelDto>();
        public List<InscriptionResult> InscriptionsCrees { get; set; } = new List<InscriptionResult>();
        public DateTime DateTraitement { get; set; } = DateTime.Now;
    }
}

