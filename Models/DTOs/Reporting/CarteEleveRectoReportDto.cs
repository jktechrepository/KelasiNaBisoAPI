namespace KelasiNaBiso.Models.DTOs.Reporting
{
    /// <summary>Champs alignés sur le template FastReport RectoEleve.frx (recto + verso).</summary>
    public class CarteEleveRectoReportDto
    {
        public string Ecole { get; set; } = string.Empty;
        public string NomComplet { get; set; } = string.Empty;
        public string Matricule { get; set; } = string.Empty;
        public string Classe { get; set; } = string.Empty;
        public string Adresse { get; set; } = string.Empty;
        public string AnneeScolaire { get; set; } = string.Empty;
        public byte[]? Photo { get; set; }
        public byte[]? Logo { get; set; }
    }
}
