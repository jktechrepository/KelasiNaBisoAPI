using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models
{
    public class V_Utilisateur
    {
        [Key]
        public int IdUtilisateur { get; set; }
        public Guid? ReferenceUtilisateur { get; set; }
        
        // Champs Utilisateur
        public string? NomUtilisateur { get; set; }
        public string? PostNomUtilisateur { get; set; }
        public string? PrenomUtilisateur { get; set; }
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public string? PhotoUrl { get; set; }
        public string? LieuNaissance { get; set; }
        public DateTime? DateNaissance { get; set; }
        public string? Genre { get; set; }
        public bool? Statut { get; set; }
        public DateTime DateCreation { get; set; }
        public bool IsConnecte { get; set; }

        // Champs Adresse (hérités par Utilisateur)
        public string? Province { get; set; }
        public string? Ville { get; set; }
        public string? Commune { get; set; }
        public string? Quartier { get; set; }
        public string? Avenue { get; set; }
        public string? Numero { get; set; }

        // Champs Role
        public int? IdRole { get; set; } // Nullable car LEFT JOIN peut retourner NULL
        public string? NomRole { get; set; } // Nullable car LEFT JOIN peut retourner NULL
        public DateTime? DateCreationRole { get; set; } // Nullable car LEFT JOIN peut retourner NULL

        // Champs Ecole
        public int? IdEcole { get; set; }
        public string? NomEcole { get; set; }
        public string? SloganEcole { get; set; }
        public string? TypeEcole { get; set; }
        public string? LogoUrl { get; set; }
        public string? TelephoneEcole { get; set; }
        public string? EmailContactEcole { get; set; }
        public string? SiteWebEcole { get; set; }
        public string? ProvinceEducationnel { get; set; }
        public string? NomCompletResponsable { get; set; }
        public string? DescriptionEcole { get; set; }
        public DateTime? DateCreationEcole { get; set; } // Nullable car LEFT JOIN peut retourner NULL

        // Champs Adresse Ecole
        public string? ProvinceEcole { get; set; }
        public string? VilleEcole { get; set; }
        public string? CommuneEcole { get; set; }
        public string? QuartierEcole { get; set; }
        public string? AvenueEcole { get; set; }
        public string? NumeroEcole { get; set; }
    }
} 
