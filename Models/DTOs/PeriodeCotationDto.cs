namespace KelasiNaBiso.Models.DTOs
{
    public class PeriodeCotationDto
    {
        public int IdPeriode { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Libelle { get; set; } = string.Empty;
        public int Ordre { get; set; }
        public bool Statut { get; set; }
    }
}
