namespace KelasiNaBiso.Models.DTOs.Reporting
{
    /// <summary>
    /// DTO pour le calcul des pourcentages de présence d'un agent
    /// </summary>
    public class PourcentageAgentDto
    {
        public AgentInfoDto Agent { get; set; } = new();
        public PeriodeDto Periode { get; set; } = new();
        public DonneesBrutesDto Donnees { get; set; } = new();
        public PourcentagesDto Pourcentages { get; set; } = new();
        public HeuresDto? Heures { get; set; }
        public ComparaisonAgentDto? Comparaison { get; set; }
    }

    public class AgentInfoDto
    {
        public int IdAgent { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public string Matricule { get; set; } = string.Empty;
        public string Fonction { get; set; } = string.Empty;
        public string? EmailAgent { get; set; }
        public string? PhotoUrl { get; set; }
    }

    public class HeuresDto
    {
        public decimal TotalHeuresTravaillees { get; set; }
        public TimeSpan HeureMoyenneArrivee { get; set; }
        public TimeSpan HeureMoyenneDepart { get; set; }
        public TimeSpan DureeMoyenneJournaliere { get; set; }
    }

    public class ComparaisonAgentDto
    {
        public MoyenneFonctionDto MoyenneFonction { get; set; } = new();
        public MoyenneDto MoyenneEcole { get; set; } = new();
        public decimal EcartVsFonction { get; set; }
        public decimal EcartVsEcole { get; set; }
    }

    public class MoyenneFonctionDto
    {
        public string Fonction { get; set; } = string.Empty;
        public decimal TauxPresenceMoyen { get; set; }
    }
}

