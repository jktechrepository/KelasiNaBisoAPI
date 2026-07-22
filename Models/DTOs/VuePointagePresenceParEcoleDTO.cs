using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class VuePointagePresenceParEcoleDTO
    {
        // Presence
        public int IdPresence { get; set; }
        public DateTime? DateDuJour { get; set; }
        public TimeSpan? HeureArrivee { get; set; }
        public TimeSpan? HeureDepart { get; set; }
        public string? StatutPresence { get; set; }
        public string? Longitute { get; set; }
        public string? Latitude { get; set; }
        public DateTime? DateCreationPresence { get; set; }

        // Eleve
        public int IdEleve { get; set; }
        public Guid? ReferenceEleve { get; set; }
        public string? Prenom { get; set; }
        public string? Nom { get; set; }
        public string? Postnom { get; set; }
        public string? NomCompletFormate { get; set; }
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
        public bool? StatutEleve { get; set; }
        public DateTime? DateCreationEleve { get; set; }

        // Classe
        public int? IdClasse { get; set; }
        public string? NomClasse { get; set; }
        public DateTime? DateCreationClasse { get; set; }

        // Option
        public int? IdOption { get; set; }
        public string? NomOption { get; set; }
        public DateTime? DateCreationOption { get; set; }

        // Section
        public int? IdSection { get; set; }
        public string? NomSection { get; set; }
        public DateTime? DateCreationSection { get; set; }

        // Direction
        public int? IdDirection { get; set; }
        public string? NomDirection { get; set; }
        public DateTime? DateCreationDirection { get; set; }

        // Ecole
        public int? IdEcole { get; set; }
        public string? NomEcole { get; set; }
        public string? Slogan { get; set; }
        public string? LongituteEcole { get; set; }
        public string? LatitudeEcole { get; set; }
        public string? TypeEcole { get; set; }
        public string? LogoUrl { get; set; }
        public string? TelephoneEcole { get; set; }
        public string? EmailContact { get; set; }
        public string? SiteWeb { get; set; }
        public string? ProvinceEducationnel { get; set; }
        public string? NomCompletResponsable { get; set; }
        public string? DescriptionEcole { get; set; }
        public string? ProvinceEcole { get; set; }
        public string? VilleEcole { get; set; }
        public string? CommuneEcole { get; set; }
        public string? QuartierEcole { get; set; }
        public string? AvenueEcole { get; set; }
        public string? NumeroEcole { get; set; }
        public DateTime? DateCreationEcole { get; set; }

        // Horaire / Vacation
        public int? IdHoraire { get; set; }
        public string? NomVacation { get; set; }
        public TimeSpan? HeureDebut { get; set; }
        public TimeSpan? HeureFin { get; set; }
        public TimeSpan? HeureDebutPause { get; set; }
        public TimeSpan? HeureFinPause { get; set; }
        public int? NombreJoursParSemaine { get; set; }
        public DateTime? DateCreationVacation { get; set; }

        // Tuteur
        public int? IdTuteur { get; set; }
        public string? NomTuteur { get; set; }
        public string? GenreTuteur { get; set; }
        public string? EmailTuteur { get; set; }
        public string? TelephoneTuteur { get; set; }
        public string? NomCompletRepresentant { get; set; }
        public string? TelephoneRepresentant { get; set; }
        public bool? StatutTuteur { get; set; } // True et False
        public string? PhotoTuteurUrl { get; set; }
        public string? PieceIdentiteTuteur { get; set; }
        public DateTime? DateCreationTuteur { get; set; }
    }
}
