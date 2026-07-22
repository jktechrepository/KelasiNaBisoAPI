using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class VueRepertoireAgentsParParentDTO
    {
        // Agent
        public int IdAgent { get; set; }
        public string? NomCompletAgent { get; set; }
        public string? GenreAgent { get; set; }
        public string? TelephoneAgent { get; set; }
        public string? EmailAgent { get; set; }
        public string? PhotoAgent { get; set; }
        public DateTime? DateCreationAgent { get; set; }

        // Cours
        public int IdCours { get; set; }
        public string? NomCours { get; set; }
        public string? DescriptionCours { get; set; }
        public DateTime? DateCreationCours { get; set; }

        // Classe
        public int IdClasse { get; set; }
        public string? NomClasse { get; set; }
        public DateTime? DateCreationClasse { get; set; }

        // Année Scolaire
        public int? IdAnneeScolaire { get; set; }
        public string? LibelleAnneeScolaire { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public DateTime? DateCreationAnnee { get; set; }

        // Élève
        public int IdEleve { get; set; }
        public string? NomCompletEleve { get; set; }
        public string? GenreEleve { get; set; }
        public string? Matricule { get; set; }
        public bool? StatutEleve { get; set; }
        public DateTime? DateCreationEleve { get; set; }

        // Tuteur
        public int IdTuteur { get; set; }
        public string? NomCompletTuteur { get; set; }
        public string? GenreTuteur { get; set; }
        public string? TelephoneTuteur { get; set; }
        public string? EmailTuteur { get; set; }
        public string? NomCompletRepresentant { get; set; }
        public string? TelephoneRepresentant { get; set; }
        public bool? StatutTuteur { get; set; } // True et False
        public DateTime? DateCreationTuteur { get; set; }

        // École
        public int IdEcole { get; set; }
        public string? NomEcole { get; set; }
        public string? TypeEcole { get; set; }
        public DateTime? DateCreationEcole { get; set; }
    }
}

