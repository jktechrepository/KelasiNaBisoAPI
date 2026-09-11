using KelasiNaBiso.Models;

namespace KelasiNaBiso.Models.DTOs
{
    /// <summary>
    /// Élève d'un tuteur avec contexte école/classe (inscription active pour l'année résolue).
    /// </summary>
    public class TuteurEleveListItemDto
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

        public int? IdEcole { get; set; }
        public string? NomEcole { get; set; }
        public int? IdClasse { get; set; }
        public string? NomClasse { get; set; }
        public int? IdAnneeScolaire { get; set; }
        public string? LibelleAnneeScolaire { get; set; }

        public static TuteurEleveListItemDto FromEleve(
            Eleve eleve,
            Inscription? inscription,
            int idEcole,
            string? nomEcole) => new()
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
            IdEcole = inscription?.IdEcole ?? idEcole,
            NomEcole = nomEcole,
            IdClasse = inscription?.IdClasse,
            NomClasse = inscription?.Classe?.NomClasse,
            IdAnneeScolaire = inscription?.IdAnneeScolaire,
            LibelleAnneeScolaire = inscription?.AnneeScolaire?.LibelleAnneeScolaire
        };
    }
}
