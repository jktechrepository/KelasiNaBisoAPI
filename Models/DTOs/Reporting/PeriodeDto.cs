namespace KelasiNaBiso.Models.DTOs.Reporting
{
    /// <summary>
    /// DTO représentant une période de temps pour les reportings
    /// </summary>
    public class PeriodeDto
    {
        public string Type { get; set; } = string.Empty; // "jour", "intervalle", "semaine", "mois", "trimestre", "annee"
        public DateTime? Date { get; set; } // Pour type "jour"
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public int JoursOuvrables { get; set; }
        public string Libelle { get; set; } = string.Empty; // Ex: "Janvier 2025", "Semaine 3", etc.
    }
}

