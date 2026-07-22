using KelasiNaBiso.Models.DTOs.Reporting;

namespace KelasiNaBiso.Models.DTOs.Paiement
{
    /// <summary>
    /// DTO pour le taux de paiement d'une classe
    /// </summary>
    public class TauxPaiementClasseDto
    {
        public ClasseInfoPaiementDto Classe { get; set; } = new();
        public PeriodeDto Periode { get; set; } = new();
        public StatistiquesClassePaiementDto Statistiques { get; set; } = new();
        public RepartitionFraisClasseDto? RepartitionParFrais { get; set; }
        public List<ElevePaiementDto>? ParEleve { get; set; }
    }

    public class ClasseInfoPaiementDto
    {
        public int IdClasse { get; set; }
        public string NomClasse { get; set; } = string.Empty;
        public string? Section { get; set; }
        public string? Option { get; set; }
        public string? Direction { get; set; }
        public string? Ecole { get; set; }
        public int EffectifTotal { get; set; }
    }

    public class StatistiquesClassePaiementDto
    {
        public decimal MontantAttendu { get; set; }
        public decimal MontantPercu { get; set; }
        public decimal MontantRestant { get; set; }
        public decimal TauxRecouvrement { get; set; }
        public int ElevesAyantPaye { get; set; }
        public int ElevesEnRetard { get; set; }
        public int ElevesAJour { get; set; }
        public decimal TauxPaiementEleves { get; set; }
    }

    public class RepartitionFraisClasseDto
    {
        public Dictionary<string, FraisClasseStatsDto> Frais { get; set; } = new();
    }

    public class FraisClasseStatsDto
    {
        public string LibelleFrais { get; set; } = string.Empty;
        public decimal MontantAttendu { get; set; }
        public decimal MontantPercu { get; set; }
        public decimal TauxRecouvrement { get; set; }
        public int NombrePaiements { get; set; }
        public int NombreEnAttente { get; set; }
    }

    public class ElevePaiementDto
    {
        public EleveInfoPaiementDto Eleve { get; set; } = new();
        public decimal MontantAttendu { get; set; }
        public decimal MontantPaye { get; set; }
        public decimal MontantRestant { get; set; }
        public decimal TauxPaiement { get; set; }
        public string Statut { get; set; } = string.Empty; // "À jour", "En retard", "Partiel"
        public int? JoursRetardMoyen { get; set; }
    }
}

