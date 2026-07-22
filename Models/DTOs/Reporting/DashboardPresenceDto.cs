namespace KelasiNaBiso.Models.DTOs.Reporting
{
    /// <summary>
    /// DTO pour le dashboard de présence d'une école
    /// </summary>
    public class DashboardPresenceDto
    {
        public EcoleInfoDto Ecole { get; set; } = new();
        public PeriodeDto Periode { get; set; } = new();
        public ResumePresenceDto ResumeEleves { get; set; } = new();
        public ResumePresenceDto ResumeAgents { get; set; } = new();
        public List<AlerteDto>? Alertes { get; set; }
        public List<ClasseProblematiqueDto>? ClassesProblematiques { get; set; }
        public List<AgentAbsentDto>? AgentsAbsents { get; set; }
    }

    public class EcoleInfoDto
    {
        public int IdEcole { get; set; }
        public string NomEcole { get; set; } = string.Empty;
        public string? Logo { get; set; }
    }

    public class ResumePresenceDto
    {
        public int EffectifTotal { get; set; }
        public int Presents { get; set; }
        public int Absents { get; set; }
        public int Retards { get; set; }
        public decimal TauxPresence { get; set; }
        public decimal TauxAbsence { get; set; }
        public decimal TauxRetard { get; set; }
    }

    public class AlerteDto
    {
        public string Type { get; set; } = string.Empty; // "danger", "warning", "info"
        public string Message { get; set; } = string.Empty;
        public string? Action { get; set; }
    }

    public class ClasseProblematiqueDto
    {
        public int IdClasse { get; set; }
        public string NomClasse { get; set; } = string.Empty;
        public int Presents { get; set; }
        public int Absents { get; set; }
        public decimal TauxPresence { get; set; }
        public string Status { get; set; } = string.Empty; // "critique", "attention", "normal"
    }

    public class AgentAbsentDto
    {
        public int IdAgent { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public string Fonction { get; set; } = string.Empty;
        public int CoursAffectes { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}

