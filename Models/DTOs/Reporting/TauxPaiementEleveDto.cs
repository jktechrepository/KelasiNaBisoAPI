using KelasiNaBiso.Models.DTOs.Reporting;

namespace KelasiNaBiso.Models.DTOs.Paiement
{
    /// <summary>
    /// DTO pour le taux de paiement d'un élève
    /// </summary>
    public class TauxPaiementEleveDto
    {
        public EleveInfoPaiementDto Eleve { get; set; } = new();
        public PeriodeDto Periode { get; set; } = new();
        public List<FraisEleveDto> FraisAttendus { get; set; } = new();
        public ResumePaiementEleveDto Resume { get; set; } = new();
        public ComparaisonPaiementDto? Comparaison { get; set; }
    }

    public class EleveInfoPaiementDto
    {
        public int IdEleve { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public string Matricule { get; set; } = string.Empty;
        public string Classe { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
    }

    public class FraisEleveDto
    {
        public int IdFrais { get; set; }
        public string LibelleFrais { get; set; } = string.Empty;
        public decimal MontantFrais { get; set; }
        public string Devise { get; set; } = string.Empty;
        public string StatutPaiement { get; set; } = string.Empty; // "Payé", "En attente", "Partiel"
        public DateTime? DatePaiement { get; set; }
        public decimal? MontantPaye { get; set; }
        public int? JoursRetard { get; set; }
        public DateTime? DateEcheance { get; set; }
    }

    public class ResumePaiementEleveDto
    {
        public decimal MontantTotal { get; set; }
        public decimal MontantPaye { get; set; }
        public decimal MontantRestant { get; set; }
        public decimal TauxPaiement { get; set; }
        public int NombreFraisPayes { get; set; }
        public int NombreFraisEnAttente { get; set; }
        public int NombreFraisPartiels { get; set; }
        public decimal JoursRetardMoyen { get; set; }
    }

    public class ComparaisonPaiementDto
    {
        public MoyennePaiementDto MoyenneClasse { get; set; } = new();
        public MoyennePaiementDto MoyenneEcole { get; set; } = new();
        public decimal EcartVsClasse { get; set; }
        public decimal EcartVsEcole { get; set; }
        public string Position { get; set; } = string.Empty;
    }

    public class MoyennePaiementDto
    {
        public string Nom { get; set; } = string.Empty;
        public decimal TauxPaiementMoyen { get; set; }
    }
}

