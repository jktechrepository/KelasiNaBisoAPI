using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class EleveParEcoleDTO
    {
        // �l�ve
        public int IdEleve { get; set; }
        public Guid? ReferenceEleve { get; set; }
        public string? NomCompletEleve { get; set; }
        public string? Genre { get; set; }
        public DateTime? DateNaissance { get; set; }
        public int? Age { get; set; }
        public string? LieuNaissance { get; set; }
        public string? PhotoUrl { get; set; }
        public string? Nationalite { get; set; }
        public string? Matricule { get; set; }
        public string? ProvinceEleve { get; set; }
        public string? VilleEleve { get; set; }
        public string? CommuneEleve { get; set; }
        public string? QuartierEleve { get; set; }
        public string? AvenueEleve { get; set; }
        public string? NumeroEleve { get; set; }
        public string? Commentaire { get; set; }
        public bool? Statut { get; set; }

        // Classe
        public int? IdClasse { get; set; }
        public string? NomClasse { get; set; }

        // Direction
        public int? IdDirection { get; set; }
        public string? NomDirection { get; set; }

        // Option
        public int? IdOption { get; set; }
        public string? NomOption { get; set; }

        // Tuteur
        public int? IdTuteur { get; set; }
        public string? NomCompletTuteur { get; set; }
        public string? GenreTuteur { get; set; }
        public string? EmailTuteur { get; set; }
        public string? TelephoneTuteur { get; set; }
        public string? NomCompletRepresentant { get; set; }
        public string? TelephoneRepresentant { get; set; }
        public bool? StatutTuteur { get; set; } // True et False
        public string? PhotoTuteurUrl { get; set; }
        public string? PieceIdentiteTuteur { get; set; }

        // �cole
        public int? IdEcole { get; set; }
        public string? NomEcole { get; set; }
        public string? Slogan { get; set; }
        public string? Longitute { get; set; }
        public string? Latitude { get; set; }
        public string? Type { get; set; }
        public string? LogoUrl { get; set; }
        public string? TelephoneEcole { get; set; }
        public string? EmailContact { get; set; }
        public string? SiteWeb { get; set; }
        //public int? CapaciteEleve { get; set; }
        //public int? NombreSalles { get; set; }
        public string? Description { get; set; }
        public string? ProvinceEcole { get; set; }
        public string? VilleEcole { get; set; }
        public string? CommuneEcole { get; set; }
        public string? QuartierEcole { get; set; }
        public string? AvenueEcole { get; set; }
        public string? NumeroEcole { get; set; }
    }
}
