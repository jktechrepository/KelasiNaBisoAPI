namespace KelasiNaBiso.Models.DTOs
{
    /// <summary>
    /// Données de pré-remplissage pour une réinscription (compatible CreateInscriptionDto + contexte).
    /// </summary>
    public class EleveReinscriptionPrefillDto
    {
        public string Type { get; set; } = "Réinscription";

        public int? IdEleveExistant { get; set; }
        public int? IdTuteurExistant { get; set; }

        public int IdEcole { get; set; }

        public string? NomEleve { get; set; }
        public string? PostnomEleve { get; set; }
        public string? PrenomEleve { get; set; }
        public string? PhotoEleveUrl { get; set; }
        public string? MatriculeEleve { get; set; }
        public string? GenreEleve { get; set; }
        public DateTime DateNaissanceEleve { get; set; }
        public string? LieuNaissanceEleve { get; set; }
        public string? NationaliteEleve { get; set; }
        public string? ProvinceEleve { get; set; }
        public string? VilleEleve { get; set; }
        public string? CommuneEleve { get; set; }
        public string? QuartierEleve { get; set; }
        public string? AvenueEleve { get; set; }
        public string? NumeroEleve { get; set; }
        public string? CommentaireEleve { get; set; }

        public string? NomCompletTuteur { get; set; }
        public string? GenreTuteur { get; set; }
        public string? EmailTuteur { get; set; }
        public string? TelephoneTuteur { get; set; }
        public string? NomCompletRepresentant { get; set; }
        public string? TelephoneRepresentant { get; set; }
        public string? PhotoTuteurUrl { get; set; }
        public string? PieceIdentiteTuteur { get; set; }

        public int IdAnneeScolaireReference { get; set; }
        public string? LibelleAnneeScolaireReference { get; set; }
        public int? IdInscriptionReference { get; set; }
        public int? IdClassePrecedente { get; set; }
        public string? NomClassePrecedente { get; set; }

        public bool DejaInscritAnneeCourante { get; set; }
        public int? IdInscriptionAnneeCourante { get; set; }
    }
}
