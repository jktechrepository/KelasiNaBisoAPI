namespace KelasiNaBiso.Models.DTOs.Reporting
{
    /// <summary>
    /// DTO pour le calcul des pourcentages de présence d'un élève
    /// </summary>
    public class PourcentageEleveDto
    {
        public EleveInfoDto Eleve { get; set; } = new();
        public PeriodeDto Periode { get; set; } = new();
        public DonneesBrutesDto Donnees { get; set; } = new();
        public PourcentagesDto Pourcentages { get; set; } = new();
        public ComparaisonDto? Comparaison { get; set; }
    }

    public class EleveInfoDto
    {
        public int IdEleve { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public string Matricule { get; set; } = string.Empty;
        public string Classe { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
    }

    public class DonneesBrutesDto
    {
        public int JoursOuvrables { get; set; }
        public int Presences { get; set; }
        public int Absences { get; set; }
        public int Retards { get; set; }
        public int PresencesPonctuelles { get; set; } // Présences sans retard
    }

    public class PourcentagesDto
    {
        public decimal TauxPresence { get; set; }
        public decimal TauxAbsence { get; set; }
        public decimal TauxRetard { get; set; }
        public decimal TauxPonctualite { get; set; }
    }

    public class ComparaisonDto
    {
        public MoyenneDto MoyenneClasse { get; set; } = new();
        public MoyenneDto MoyenneEcole { get; set; } = new();
        public decimal EcartVsClasse { get; set; }
        public decimal EcartVsEcole { get; set; }
    }

    public class MoyenneDto
    {
        public string Nom { get; set; } = string.Empty;
        public decimal TauxPresenceMoyen { get; set; }
    }
}

