namespace KelasiNaBiso.Models.DTOs.Reporting
{
    public class BulletinEnteteReportDto
    {
        public string NomEcole { get; set; } = string.Empty;
        public string NomEleve { get; set; } = string.Empty;
        public string NomClasse { get; set; } = string.Empty;
        public string LibelleAnnee { get; set; } = string.Empty;
        public string Periode { get; set; } = string.Empty;
        public string MoyenneGeneraleTexte { get; set; } = "-";
        public string RangTexte { get; set; } = "-";
        public string EffectifTexte { get; set; } = "-";
        public string DecisionTexte { get; set; } = "-";
        public string AppreciationTexte { get; set; } = "-";
    }

    public class BulletinLigneReportDto
    {
        public string NomCours { get; set; } = string.Empty;
        public string MoyenneCoursTexte { get; set; } = "-";
        public string CoefficientTexte { get; set; } = "-";
        public string DetailNotes { get; set; } = string.Empty;
    }
}
