namespace KelasiNaBiso.Models.DTOs
{
    /// <summary>
    /// Élève listé par école avec contexte d'inscription (classe / année).
    /// </summary>
    public class EleveParEcoleListItemDto
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
        public string? PhotoUrl { get; set; }
        public bool? Statut { get; set; }

        public int? IdTuteur { get; set; }
        public string? NomCompletTuteur { get; set; }
        public string? TelephoneTuteur { get; set; }

        public int? IdInscription { get; set; }
        public int? IdClasse { get; set; }
        public string? NomClasse { get; set; }
        public int? IdAnneeScolaire { get; set; }
        public string? LibelleAnneeScolaire { get; set; }
        public int? IdEcole { get; set; }
    }
}
