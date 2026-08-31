using KelasiNaBiso.Models.DTOs.Paiement;
using KelasiNaBiso.Models.Enums;

namespace KelasiNaBiso.Models.DTOs.Reporting
{
    /// <summary>
    /// Dashboard guichet — encaissements personnels du caissier connecté.
    /// </summary>
    public class DashboardCaissierDto
    {
        public EcoleInfoPaiementDto Ecole { get; set; } = new();
        public int IdAnneeScolaire { get; set; }
        public string? LibelleAnneeScolaire { get; set; }
        public PeriodeDto Periode { get; set; } = new();
        public ResumeCaissierJourDto Resume { get; set; } = new();
        public RepartitionModePaiementDto RepartitionParMode { get; set; } = new();
        public List<PaiementCaissierRecentDto> DerniersPaiements { get; set; } = new();
        public MokoCaissierResumeDto Moko { get; set; } = new();
        /// <summary>moi = encaissements du caissier connecté ; ecole = toute la caisse du jour.</summary>
        public string Scope { get; set; } = DashboardCaissierScopes.Moi;
    }

    public static class DashboardCaissierScopes
    {
        public const string Moi = "moi";
        public const string Ecole = "ecole";
    }

    /// <summary>Clôture de caisse — rapport complet du jour (impression / export front).</summary>
    public class DashboardCaissierClotureDto : DashboardCaissierDto
    {
        public DateTime GenereLe { get; set; } = DateTime.Now;
        public List<PaiementCaissierRecentDto> TousLesPaiements { get; set; } = new();
    }

    public class ResumeCaissierJourDto
    {
        public int NombrePaiements { get; set; }
        public decimal MontantTotal { get; set; }
        public int PayInsReussis { get; set; }
        public int PayInsEnAttente { get; set; }
        public int PayInsEchoues { get; set; }
    }

    public class PaiementCaissierRecentDto
    {
        public int IdPaiement { get; set; }
        public DateTime DatePaiement { get; set; }
        public decimal Montant { get; set; }
        public string ModePaiement { get; set; } = string.Empty;
        public string Statut { get; set; } = string.Empty;
        public string? NomEleve { get; set; }
        public string? LibelleFrais { get; set; }
        public string? ReferenceMoko { get; set; }
    }

    public class MokoCaissierResumeDto
    {
        public bool EstConfigure { get; set; }
        public int MesPayInsEnAttente { get; set; }
        public List<MokoPayInEnAttenteDto> PayInsEnAttente { get; set; } = new();
    }

    public class MokoPayInEnAttenteDto
    {
        public string Reference { get; set; } = string.Empty;
        public int? IdPaiement { get; set; }
        public decimal Montant { get; set; }
        public string? NomEleve { get; set; }
        public string? LibelleFrais { get; set; }
        public DateTime DateCreation { get; set; }
    }
}
