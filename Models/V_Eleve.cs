using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models
{
    public class V_Eleve
    {
        [Key]
        public int IdEleve { get; set; }
        public Guid? ReferenceEleve { get; set; }
        
        // Champs Élève
        public string? Matricule { get; set; }
        public string? Nom { get; set; }
        public string? Postnom { get; set; }
        public string? Prenom { get; set; }
        public string? NomComplet { get; set; }
        public string? Genre { get; set; }
        public DateTime DateNaissance { get; set; }
        public string? LieuNaissance { get; set; }
        public string? PhotoUrl { get; set; }
        public string Nationalite { get; set; } = string.Empty;
        public string? Commentaire { get; set; }
        public bool? Statut { get; set; } // True et False
        public DateTime DateCreation { get; set; }

        // Champs Adresse Élève (hérités par Eleve)
        public string? Province { get; set; }
        public string? Ville { get; set; }
        public string? Commune { get; set; }
        public string? Quartier { get; set; }
        public string? Avenue { get; set; }
        public string? Numero { get; set; }

        // Champs Classe
        public int? IdClasse { get; set; }
        public string? NomClasse { get; set; }
        public DateTime? DateCreationClasse { get; set; }

        // Champs Section
        public int? IdSection { get; set; }
        public string? NomSection { get; set; }
        public DateTime? DateCreationSection { get; set; }

        // Champs Option
        public int? IdOption { get; set; }
        public string? NomOption { get; set; }
        public DateTime? DateCreationOption { get; set; }

        // Champs Tuteur
        public int? IdTuteur { get; set; }
        public string? NomCompletTuteur { get; set; }
        public string? GenreTuteur { get; set; }
        public string? EmailTuteur { get; set; }
        public string? TelephoneTuteur { get; set; }
        public string? NomCompletRepresentant { get; set; }
        public string? TelephoneRepresentant { get; set; }
        public string? PhotoTuteurUrl { get; set; }
        public string? PieceIdentiteTuteur { get; set; }
        public bool StatutTuteur { get; set; } // True et False
        public DateTime? DateCreationTuteur { get; set; }

        // Champs École (via Tuteur)
        public int? IdEcole { get; set; }
        public string? NomEcole { get; set; }
        public string? SloganEcole { get; set; }
        public string? TypeEcole { get; set; }
        public string? LogoUrlEcole { get; set; }
        public string? TelephoneEcole { get; set; }
        public string? EmailContactEcole { get; set; }
        public string? SiteWebEcole { get; set; }
        public string? DescriptionEcole { get; set; }
        public DateTime? DateCreationEcole { get; set; }

        // Champs Adresse École
        public string? ProvinceEcole { get; set; }
        public string? VilleEcole { get; set; }
        public string? CommuneEcole { get; set; }
        public string? QuartierEcole { get; set; }
        public string? AvenueEcole { get; set; }
        public string? NumeroEcole { get; set; }
    }
}
