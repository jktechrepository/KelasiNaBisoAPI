namespace KelasiNaBiso.Models.DTOs.Reporting
{
    public class DashboardTuteurDto
    {
        /// <summary>Renseigné en mono-école ; null en agrégat multi-écoles.</summary>
        public EcoleInfoDto? Ecole { get; set; }
        /// <summary>Écoles couvertes (1 en mono, N en multi).</summary>
        public List<EcoleInfoDto> Ecoles { get; set; } = new();
        /// <summary>Année locale en mono-école ; null en multi (IDs différents).</summary>
        public int? IdAnneeScolaire { get; set; }
        public string? LibelleAnneeScolaire { get; set; }
        public PeriodeDto Periode { get; set; } = new();
        public ResumeTuteurDto Resume { get; set; } = new();
        public List<EnfantTuteurDto> Enfants { get; set; } = new();
        public List<AlerteDto> Alertes { get; set; } = new();
    }

    public class ResumeTuteurDto
    {
        public int NombreEnfants { get; set; }
        public int ElevesActifs { get; set; }
        public int ElevesAyantPaye { get; set; }
        public int ElevesEnRetardPaiement { get; set; }
        public decimal MontantTotalPaye { get; set; }
        public int NombreClasses { get; set; }
    }

    public class EnfantTuteurDto
    {
        public int IdEleve { get; set; }
        public string? Matricule { get; set; }
        public string? NomComplet { get; set; }
        public int IdEcole { get; set; }
        public string? NomEcole { get; set; }
        public int IdAnneeScolaire { get; set; }
        public string? NomClasse { get; set; }
        public int? IdClasse { get; set; }
        public string? StatutInscription { get; set; }
        public int NombrePaiements { get; set; }
        public decimal MontantPaye { get; set; }
        public string? AlertePaiement { get; set; }
    }

    public class DashboardEnseignantDto
    {
        public EcoleInfoDto Ecole { get; set; } = new();
        public int IdAnneeScolaire { get; set; }
        public string? LibelleAnneeScolaire { get; set; }
        public PeriodeDto Periode { get; set; } = new();
        public ResumeEnseignantDto Resume { get; set; } = new();
        public List<ClasseEnseignantDto> Classes { get; set; } = new();
        public List<AlerteDto> Alertes { get; set; } = new();
    }

    public class ResumeEnseignantDto
    {
        public int NombreClasses { get; set; }
        public int ElevesSuivis { get; set; }
        public int Presences { get; set; }
        public int Absences { get; set; }
        public int ClassesCritiques { get; set; }
        public decimal TauxPresenceGeneral { get; set; }
    }

    public class ClasseEnseignantDto
    {
        public int IdClasse { get; set; }
        public string NomClasse { get; set; } = string.Empty;
        public int NombreEleves { get; set; }
        public int Presences { get; set; }
        public int Absences { get; set; }
        public decimal TauxPresence { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class DashboardEleveDto
    {
        public EcoleInfoDto? Ecole { get; set; }
        public int? IdAnneeScolaire { get; set; }
        public string? LibelleAnneeScolaire { get; set; }
        public PeriodeDto Periode { get; set; } = new();
        public EleveProfilDto? Profil { get; set; }
        public ResumeEleveDto Resume { get; set; } = new();
        public List<AlerteDto> Alertes { get; set; } = new();
    }

    public class EleveProfilDto
    {
        public int IdEleve { get; set; }
        public string? Matricule { get; set; }
        public string? NomComplet { get; set; }
        public int IdEcole { get; set; }
        public string? NomEcole { get; set; }
        public int IdAnneeScolaire { get; set; }
        public int? IdClasse { get; set; }
        public string? NomClasse { get; set; }
        public string? StatutInscription { get; set; }
    }

    public class ResumeEleveDto
    {
        public int NombrePaiements { get; set; }
        public decimal MontantPaye { get; set; }
        public bool AlertePaiement { get; set; }
        public string? MessagePaiement { get; set; }
    }
}
