namespace KelasiNaBiso.Models.DTOs.Reporting
{
    /// <summary>
    /// DTO pour le reporting de présence d'une fonction d'agents
    /// </summary>
    public class FonctionPresenceDto
    {
        public string Fonction { get; set; } = string.Empty;
        public string Ecole { get; set; } = string.Empty;
        public PeriodeDto Periode { get; set; } = new();
        public StatistiquesFonctionDto Statistiques { get; set; } = new();
        public StatistiquesRetardsFonctionDto? StatistiquesRetards { get; set; }
        public List<AgentPresenceDto>? ParAgent { get; set; }
    }

    public class StatistiquesFonctionDto
    {
        public int EffectifTotal { get; set; }
        public int PresencesAttendue { get; set; }
        public int PresencesEffectives { get; set; }
        public int Absences { get; set; }
        public int Retards { get; set; }
        public decimal TauxPresence { get; set; }
        public decimal TauxAbsence { get; set; }
        public decimal TauxRetard { get; set; }
        public decimal? TotalHeuresTravaillees { get; set; }
    }

    public class StatistiquesRetardsFonctionDto
    {
        public int NombreRetards { get; set; }
        public int AgentsConcernes { get; set; }
        public decimal PourcentageAgentsRetardataires { get; set; }
        public string DureeMoyenne { get; set; } = string.Empty;
    }

    public class AgentPresenceDto
    {
        public AgentInfoDto Agent { get; set; } = new();
        public int Presences { get; set; }
        public int Absences { get; set; }
        public int Retards { get; set; }
        public decimal TauxPresence { get; set; }
        public decimal TauxRetard { get; set; }
        public decimal? HeuresTravaillees { get; set; }
    }
}

