namespace KelasiNaBiso.Models.DTOs.Reporting
{
    /// <summary>Champs alignés sur le template FastReport RectoAgent.frx</summary>
    public class CarteAgentRectoReportDto
    {
        public string Noms { get; set; } = string.Empty;
        public string Fonction { get; set; } = string.Empty;
        public string Matricule { get; set; } = string.Empty;
        public string Adresse { get; set; } = string.Empty;
        public string Ecole { get; set; } = string.Empty;
        public byte[]? Photo { get; set; }
        public byte[]? Logo { get; set; }
    }
}
