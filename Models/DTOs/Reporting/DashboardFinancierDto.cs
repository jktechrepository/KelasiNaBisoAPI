using KelasiNaBiso.Models.DTOs.Paiement;
using KelasiNaBiso.Models.Enums;

namespace KelasiNaBiso.Models.DTOs.Reporting
{
    public class DashboardFinancierDto
    {
        public EcoleInfoPaiementDto Ecole { get; set; } = new();
        public int IdAnneeScolaire { get; set; }
        public string? LibelleAnneeScolaire { get; set; }
        public PeriodeDto Periode { get; set; } = new();
        public ResumeFinancierDto Resume { get; set; } = new();
        public RepartitionModePaiementDto RepartitionParMode { get; set; } = new();
        public List<PaiementCaissierRecentDto> DerniersPaiements { get; set; } = new();
        public MokoCaissierResumeDto Moko { get; set; } = new();
        public string Scope { get; set; } = DashboardCaissierScopes.Moi;
    }

    public class DashboardFinancierClotureDto : DashboardFinancierDto
    {
        public DateTime GenereLe { get; set; } = DateTime.Now;
        public List<PaiementCaissierRecentDto> TousLesPaiements { get; set; } = new();
    }

    public class ResumeFinancierDto
    {
        public int NombrePaiements { get; set; }
        public decimal MontantTotal { get; set; }
        public decimal MontantConfirme { get; set; }
        public decimal MontantEnAttente { get; set; }
        public decimal MontantEchoue { get; set; }
        public decimal SoldeNet { get; set; }
    }
}
