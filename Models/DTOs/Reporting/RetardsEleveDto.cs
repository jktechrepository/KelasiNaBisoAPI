namespace KelasiNaBiso.Models.DTOs.Reporting
{
    /// <summary>
    /// DTO pour l'analyse des retards d'un élève
    /// </summary>
    public class RetardsEleveDto
    {
        public EleveInfoDto Eleve { get; set; } = new();
        public PeriodeDto Periode { get; set; } = new();
        public TimeSpan HeureReference { get; set; }
        public StatistiquesRetardsDto Statistiques { get; set; } = new();
        public List<DetailRetardDto> ListeRetards { get; set; } = new();
        public ComparaisonRetardsDto? Comparaison { get; set; }
    }

    public class StatistiquesRetardsDto
    {
        public int NombreTotalRetards { get; set; }
        public int NombrePresences { get; set; }
        public decimal PourcentageRetards { get; set; }
        public string DureeMoyenneRetard { get; set; } = string.Empty; // Ex: "15 minutes"
        public string DureeMaxRetard { get; set; } = string.Empty;
        public string? JourSemaineFrequent { get; set; }
    }

    public class DetailRetardDto
    {
        public DateTime Date { get; set; }
        public string JourSemaine { get; set; } = string.Empty;
        public TimeSpan HeureReference { get; set; }
        public TimeSpan HeureArrivee { get; set; }
        public string DureeRetard { get; set; } = string.Empty; // Ex: "15 minutes"
        public string? Observation { get; set; }
    }

    public class ComparaisonRetardsDto
    {
        public MoyenneRetardsDto MoyenneClasse { get; set; } = new();
        public string Position { get; set; } = string.Empty;
    }

    public class MoyenneRetardsDto
    {
        public decimal TauxRetard { get; set; }
        public string DureeMoyenne { get; set; } = string.Empty;
    }
}

