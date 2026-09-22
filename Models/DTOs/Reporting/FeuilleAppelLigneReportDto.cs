namespace KelasiNaBiso.Models.DTOs.Reporting
{
    /// <summary>
    /// Ligne plate pour DataBand FastReport (feuille d'appel élèves ou agents).
    /// </summary>
    public class FeuilleAppelLigneReportDto
    {
        public string Index { get; set; } = string.Empty;
        public string Matricule { get; set; } = string.Empty;
        public string NomComplet { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string Fonction { get; set; } = string.Empty;
        public string StatutJour { get; set; } = string.Empty;
        public string HeureArrivee { get; set; } = string.Empty;
        public string HeureDepart { get; set; } = string.Empty;
        public string Observation { get; set; } = string.Empty;
    }

    /// <summary>
    /// En-tête commun pour templates FastReport feuille d'appel.
    /// </summary>
    public class FeuilleAppelEnteteReportDto
    {
        public string Titre { get; set; } = string.Empty;
        public string NomEcole { get; set; } = string.Empty;
        public string LigneContexte { get; set; } = string.Empty;
        public string DateTexte { get; set; } = string.Empty;
        public string EffectifTexte { get; set; } = string.Empty;
        public string NbPresentsTexte { get; set; } = string.Empty;
        public string NbAbsentsTexte { get; set; } = string.Empty;
        public string NbRetardsTexte { get; set; } = string.Empty;
    }
}
