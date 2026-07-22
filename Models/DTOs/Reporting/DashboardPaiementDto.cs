using KelasiNaBiso.Models.DTOs.Reporting;

namespace KelasiNaBiso.Models.DTOs.Paiement
{
    /// <summary>
    /// DTO pour le dashboard de paiement d'une école
    /// </summary>
    public class DashboardPaiementDto
    {
        public EcoleInfoPaiementDto Ecole { get; set; } = new();
        public PeriodeDto Periode { get; set; } = new();
        public ResumePaiementDto Resume { get; set; } = new();
        public RepartitionModePaiementDto RepartitionParMode { get; set; } = new();
        public List<Top5FraisDto>? Top5Frais { get; set; }
        public List<StatutPaiementDto>? ParStatut { get; set; }
    }

    public class EcoleInfoPaiementDto
    {
        public int IdEcole { get; set; }
        public string NomEcole { get; set; } = string.Empty;
        public string? Logo { get; set; }
    }

    public class ResumePaiementDto
    {
        public int NombrePaiements { get; set; }
        public decimal MontantTotal { get; set; }
        public decimal MontantAttendu { get; set; }
        public decimal TauxRecouvrement { get; set; }
        public int NombreEleves { get; set; }
        public int ElevesAyantPaye { get; set; }
        public int ElevesEnRetard { get; set; }
        public decimal TauxPaiementEleves { get; set; }
        public string DevisePrincipale { get; set; } = "USD";
    }

    public class RepartitionModePaiementDto
    {
        public Dictionary<string, ModePaiementStatsDto> Modes { get; set; } = new();
    }

    public class ModePaiementStatsDto
    {
        public int Nombre { get; set; }
        public decimal Montant { get; set; }
        public decimal Pourcentage { get; set; }
    }

    public class Top5FraisDto
    {
        public int Rang { get; set; }
        public string LibelleFrais { get; set; } = string.Empty;
        public decimal MontantTotal { get; set; }
        public int NombrePaiements { get; set; }
        public decimal Pourcentage { get; set; }
    }

    public class StatutPaiementDto
    {
        public string Statut { get; set; } = string.Empty;
        public int Nombre { get; set; }
        public decimal Montant { get; set; }
        public decimal Pourcentage { get; set; }
    }
}

