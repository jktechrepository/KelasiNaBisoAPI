namespace KelasiNaBiso.Models.DTOs.Reporting
{
    /// <summary>
    /// Feuille d'appel nominative des agents d'une école pour une date donnée.
    /// </summary>
    public class FeuilleAppelAgentsDto
    {
        public int IdEcole { get; set; }
        public string? NomEcole { get; set; }
        public string? FonctionFiltre { get; set; }
        public DateTime Date { get; set; }
        public int Effectif { get; set; }
        public int NbPresents { get; set; }
        public int NbAbsents { get; set; }
        public int NbRetards { get; set; }
        public List<FeuilleAppelAgentLigneDto> Lignes { get; set; } = new();
    }

    public class FeuilleAppelAgentLigneDto
    {
        public int IdAgent { get; set; }
        public string? Matricule { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public string? Fonction { get; set; }
        public string? Genre { get; set; }

        /// <summary>Present | Absent | Retard</summary>
        public string StatutJour { get; set; } = FeuilleAppelStatutJour.Absent;

        public int? IdPresence { get; set; }
        public bool? IsPresent { get; set; }
        public TimeSpan? HeureArrivee { get; set; }
        public TimeSpan? HeureDepart { get; set; }
        public string? Observation { get; set; }
    }
}
