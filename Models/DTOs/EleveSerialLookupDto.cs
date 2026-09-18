using KelasiNaBiso.Models;

namespace KelasiNaBiso.Models.DTOs
{
    /// <summary>
    /// Réponse de lookup élève par SerialNumber (scan carte / contrôleur),
    /// avec classe de l'inscription active.
    /// </summary>
    public class EleveSerialLookupDto
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

        public Tuteur? Tuteur { get; set; }

        public static EleveSerialLookupDto From(Eleve eleve, Inscription? inscription) =>
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
                IdClasse = inscription?.IdClasse,
                NomClasse = inscription?.Classe?.NomClasse,
                Tuteur = eleve.Tuteur
            };
    }
}
