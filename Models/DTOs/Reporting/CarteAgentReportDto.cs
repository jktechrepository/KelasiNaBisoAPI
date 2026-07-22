namespace KelasiNaBiso.Models.DTOs.Reporting
{
    public class CarteAgentReportDto
    {
        public int IdAgent { get; set; }
        public int? IdEcole { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public string Matricule { get; set; } = string.Empty;
        public string Fonction { get; set; } = string.Empty;
        public string RoleAgent { get; set; } = string.Empty;
        public string NomEcole { get; set; } = string.Empty;
        public string SloganEcole { get; set; } = string.Empty;
        public string AnneeScolaire { get; set; } = string.Empty;
        public string TypeCarte { get; set; } = "PERSONNEL";
        public byte[]? PhotoBytes { get; set; }
        public byte[]? LogoBytes { get; set; }
    }
}
