namespace KelasiNaBiso.Models.DTOs.Reporting
{
    public class CarteEleveReportDto
    {
        public int IdEleve { get; set; }
        public int? IdEcole { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public string Matricule { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public DateTime DateNaissance { get; set; }
        public string NomClasse { get; set; } = string.Empty;
        public string NomEcole { get; set; } = string.Empty;
        public string SloganEcole { get; set; } = string.Empty;
        public string AnneeScolaire { get; set; } = string.Empty;
        public string TypeCarte { get; set; } = "ELEVE";
        public byte[]? PhotoBytes { get; set; }
        public byte[]? LogoBytes { get; set; }
    }
}
