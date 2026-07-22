namespace KelasiNaBiso.Models.DTOs.Reporting
{
    /// <summary>
    /// DTO pour le reporting de présence d'une classe
    /// </summary>
    public class ClassePresenceDto
    {
        public ClasseInfoDto Classe { get; set; } = new();
        public PeriodeDto Periode { get; set; } = new();
        public StatistiquesClasseDto Statistiques { get; set; } = new();
        public StatistiquesRetardsClasseDto? StatistiquesRetards { get; set; }
        public List<ElevePresenceDto>? ParEleve { get; set; } // Si includeDetails=true
    }

    public class ClasseInfoDto
    {
        public int IdClasse { get; set; }
        public string NomClasse { get; set; } = string.Empty;
        public string? Section { get; set; }
        public string? Option { get; set; }
        public string? Direction { get; set; }
        public string? Ecole { get; set; }
        public int EffectifTotal { get; set; }
    }

    public class StatistiquesClasseDto
    {
        public int PresencesAttendue { get; set; }
        public int PresencesEffectives { get; set; }
        public int Absences { get; set; }
        public int Retards { get; set; }
        public decimal TauxPresence { get; set; }
        public decimal TauxAbsence { get; set; }
        public decimal TauxRetard { get; set; }
    }

    public class StatistiquesRetardsClasseDto
    {
        public int NombreRetards { get; set; }
        public int ElevesConcernes { get; set; }
        public decimal PourcentageElevesRetardataires { get; set; }
        public string DureeMoyenne { get; set; } = string.Empty;
        public Dictionary<string, int>? RepartitionParDuree { get; set; } // "0-10min": 10, etc.
        public Dictionary<string, int>? RepartitionParJour { get; set; }  // "Lundi": 5, etc.
    }

    public class ElevePresenceDto
    {
        public EleveInfoDto Eleve { get; set; } = new();
        public int Presences { get; set; }
        public int Absences { get; set; }
        public int Retards { get; set; }
        public decimal TauxPresence { get; set; }
        public decimal TauxRetard { get; set; }
    }
}

