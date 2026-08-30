namespace KelasiNaBiso.Models.DTOs.Reporting
{
    /// <summary>
    /// Feuille d'appel nominative d'une classe pour une date donnée.
    /// </summary>
    public class FeuilleAppelClasseDto
    {
        public int IdClasse { get; set; }
        public string NomClasse { get; set; } = string.Empty;
        public int IdEcole { get; set; }
        public string? NomEcole { get; set; }
        public int IdAnneeScolaire { get; set; }
        public DateTime Date { get; set; }
        public int Effectif { get; set; }
        public int NbPresents { get; set; }
        public int NbAbsents { get; set; }
        public int NbRetards { get; set; }
        public List<FeuilleAppelLigneDto> Lignes { get; set; } = new();
    }

    public class FeuilleAppelLigneDto
    {
        public int IdEleve { get; set; }
        public string? Matricule { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public string? Genre { get; set; }

        /// <summary>Present | Absent | Retard</summary>
        public string StatutJour { get; set; } = FeuilleAppelStatutJour.Absent;

        public int? IdPresence { get; set; }
        public bool? IsPresent { get; set; }
        public TimeSpan? HeureArrivee { get; set; }
        public TimeSpan? HeureDepart { get; set; }
        public string? Observation { get; set; }
    }

    public static class FeuilleAppelStatutJour
    {
        public const string Present = "Present";
        public const string Absent = "Absent";
        public const string Retard = "Retard";
    }
}
