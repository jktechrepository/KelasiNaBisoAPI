using KelasiNaBiso.Models;

namespace KelasiNaBiso.Models.DTOs
{
    /// <summary>
    /// Réponse détaillée d'un élève, incluant son tuteur lorsqu'il est affecté.
    /// </summary>
    public class EleveDetailDto
    {
        public int IdEleve { get; set; }
        public Guid? ReferenceEleve { get; set; }
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
        public int? IdTuteur { get; set; }
        public bool? Statut { get; set; }
        public string? SerialNumber { get; set; }

        public string? Province { get; set; }
        public string? Ville { get; set; }
        public string? Commune { get; set; }
        public string? Quartier { get; set; }
        public string? Avenue { get; set; }
        public string? Numero { get; set; }

        public int? IdClasse { get; set; }
        public string? NomClasse { get; set; }
        public TuteurDetailDto? Tuteur { get; set; }

        public static EleveDetailDto From(Eleve eleve) =>
            new()
            {
                IdEleve = eleve.IdEleve,
                ReferenceEleve = eleve.ReferenceEleve,
                Matricule = eleve.Matricule,
                Nom = eleve.Nom,
                Postnom = eleve.Postnom,
                Prenom = eleve.Prenom,
                NomComplet = eleve.NomComplet,
                Genre = eleve.Genre,
                DateNaissance = eleve.DateNaissance,
                LieuNaissance = eleve.LieuNaissance,
                PhotoUrl = eleve.PhotoUrl,
                Nationalite = eleve.Nationalite,
                Commentaire = eleve.Commentaire,
                IdTuteur = eleve.IdTuteur,
                Statut = eleve.Statut,
                SerialNumber = eleve.SerialNumber,
                Province = eleve.Province,
                Ville = eleve.Ville,
                Commune = eleve.Commune,
                Quartier = eleve.Quartier,
                Avenue = eleve.Avenue,
                Numero = eleve.Numero,
                IdClasse = eleve.IdClasse,
                NomClasse = eleve.NomClasse,
                Tuteur = TuteurDetailDto.From(eleve.Tuteur)
            };
    }

    /// <summary>
    /// Informations du tuteur incluses dans le détail d'un élève.
    /// </summary>
    public class TuteurDetailDto
    {
        public int IdTuteur { get; set; }
        public string? NomComplet { get; set; }
        public string? Genre { get; set; }
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public string? NomCompletRepresentant { get; set; }
        public string? TelephoneRepresentant { get; set; }
        public string? PhotoTuteurUrl { get; set; }
        public string? PieceIdentiteTuteur { get; set; }
        public bool? Statut { get; set; }
        public string? SerialNumber { get; set; }

        public static TuteurDetailDto? From(Tuteur? tuteur) =>
            tuteur == null
                ? null
                : new TuteurDetailDto
                {
                    IdTuteur = tuteur.IdTuteur,
                    NomComplet = tuteur.NomComplet,
                    Genre = tuteur.Genre,
                    Email = tuteur.Email,
                    Telephone = tuteur.Telephone,
                    NomCompletRepresentant = tuteur.NomCompletRepresentant,
                    TelephoneRepresentant = tuteur.TelephoneRepresentant,
                    PhotoTuteurUrl = tuteur.PhotoTuteurUrl,
                    PieceIdentiteTuteur = tuteur.PieceIdentiteTuteur,
                    Statut = tuteur.Statut,
                    SerialNumber = tuteur.SerialNumber
                };
    }
}
