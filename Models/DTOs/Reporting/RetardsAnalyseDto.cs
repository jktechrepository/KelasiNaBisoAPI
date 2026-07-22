namespace KelasiNaBiso.Models.DTOs.Reporting
{
    /// <summary>
    /// DTO pour l'analyse globale des retards (élèves ou agents)
    /// </summary>
    public class RetardsAnalyseDto
    {
        public string Ecole { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "ELEVE" ou "AGENT"
        public PeriodeDto Periode { get; set; } = new();
        public TimeSpan HeureReference { get; set; }
        public StatistiquesGlobalesRetardsDto StatistiquesGlobales { get; set; } = new();
        public Dictionary<string, RepartitionDto>? RepartitionParDuree { get; set; }
        public Dictionary<string, RepartitionDto>? RepartitionParJourSemaine { get; set; }
        public List<TopRetardataireDto>? TopRetardataires { get; set; }
        public List<GroupeRetardsDto>? ParGroupe { get; set; } // Par classe ou fonction
    }

    public class StatistiquesGlobalesRetardsDto
    {
        public int NombreTotalRetards { get; set; }
        public int PersonnesRetardataires { get; set; }
        public decimal PourcentagePersonnesRetardataires { get; set; }
        public decimal TauxRetardGlobal { get; set; }
        public string DureeMoyenne { get; set; } = string.Empty;
        public string DureeMediane { get; set; } = string.Empty;
        public string DureeMin { get; set; } = string.Empty;
        public string DureeMax { get; set; } = string.Empty;
    }

    public class RepartitionDto
    {
        public int Nombre { get; set; }
        public decimal Pourcentage { get; set; }
        public string? DureeMoyenne { get; set; }
    }

    public class TopRetardataireDto
    {
        public int Rang { get; set; }
        public PersonneInfoDto Personne { get; set; } = new();
        public int NombreRetards { get; set; }
        public decimal PourcentageRetards { get; set; }
        public string DureeMoyenne { get; set; } = string.Empty;
        public string DureeMax { get; set; } = string.Empty;
        public string? JourSemaineFrequent { get; set; }
    }

    public class PersonneInfoDto
    {
        public string Type { get; set; } = string.Empty; // "ELEVE" ou "AGENT"
        public int Id { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public string Matricule { get; set; } = string.Empty;
        public string? Groupe { get; set; } // Classe pour élève, Fonction pour agent
    }

    public class GroupeRetardsDto
    {
        public string Nom { get; set; } = string.Empty;
        public int NombreRetards { get; set; }
        public decimal TauxRetard { get; set; }
        public string DureeMoyenne { get; set; } = string.Empty;
    }
}

