namespace KelasiNaBiso.Models.DTOs
{
    public enum ParcoursScolaireAccessStatus
    {
        Ok = 0,
        NotFound = 1,
        Forbidden = 2
    }

    /// <summary>Contexte appelant pour les règles d'accès ParcoursScolaire.</summary>
    public class ParcoursScolaireCallerContext
    {
        public string Role { get; set; } = string.Empty;
        public bool IsSuperAdmin { get; set; }
        public int? EleveId { get; set; }
        public int? TuteurId { get; set; }
        public int EcoleId { get; set; }
    }

    public class ParcoursScolaireResult
    {
        public ParcoursScolaireAccessStatus Status { get; set; }
        public ParcoursScolaireDto? Data { get; set; }

        public static ParcoursScolaireResult Ok(ParcoursScolaireDto data) =>
            new() { Status = ParcoursScolaireAccessStatus.Ok, Data = data };

        public static ParcoursScolaireResult NotFound() =>
            new() { Status = ParcoursScolaireAccessStatus.NotFound };

        public static ParcoursScolaireResult Forbidden() =>
            new() { Status = ParcoursScolaireAccessStatus.Forbidden };
    }

    /// <summary>Dossier scolaire complet d'un élève (endpoint authentifié).</summary>
    public class ParcoursScolaireDto
    {
        public ParcoursEleveIdentiteDto Eleve { get; set; } = new();
        public IReadOnlyList<ParcoursEtapeDto> Etapes { get; set; } = Array.Empty<ParcoursEtapeDto>();
    }

    public class ParcoursEleveIdentiteDto
    {
        public int IdEleve { get; set; }
        public string? Matricule { get; set; }
        public string? Nom { get; set; }
        public string? Postnom { get; set; }
        public string? Prenom { get; set; }
        public string? Genre { get; set; }
        public DateTime DateNaissance { get; set; }
        public string? Nationalite { get; set; }
        public int? IdTuteur { get; set; }
    }

    public class ParcoursEtapeDto
    {
        public int IdInscription { get; set; }
        public string? Type { get; set; }
        public DateTime DateInscription { get; set; }
        public string? StatutInscription { get; set; }

        public int IdEcole { get; set; }
        public string? NomEcole { get; set; }
        public int IdClasse { get; set; }
        public string? NomClasse { get; set; }
        public int IdAnneeScolaire { get; set; }
        public string? LibelleAnneeScolaire { get; set; }
        public DateTime? DateDebutAnnee { get; set; }
        public DateTime? DateFinAnnee { get; set; }

        public IReadOnlyList<ParcoursBulletinItemDto> Bulletins { get; set; } = Array.Empty<ParcoursBulletinItemDto>();
        public IReadOnlyList<ParcoursNoteItemDto> Notes { get; set; } = Array.Empty<ParcoursNoteItemDto>();
        public IReadOnlyList<ParcoursPaiementItemDto> Paiements { get; set; } = Array.Empty<ParcoursPaiementItemDto>();
        public IReadOnlyList<ParcoursPresenceItemDto> Presences { get; set; } = Array.Empty<ParcoursPresenceItemDto>();
    }

    public class ParcoursBulletinItemDto
    {
        /// <summary>Fige ou Decision.</summary>
        public string Source { get; set; } = string.Empty;
        public int IdPeriode { get; set; }
        public string? CodePeriode { get; set; }
        public string? LibellePeriode { get; set; }
        public double? MoyenneGenerale { get; set; }
        public int? Rang { get; set; }
        public int? EffectifClasse { get; set; }
        public string? Decision { get; set; }
        public string? AppreciationGenerale { get; set; }
        public DateTime? DateValidation { get; set; }
    }

    public class ParcoursNoteItemDto
    {
        public int IdNote { get; set; }
        public double NoteObtenue { get; set; }
        public string? Appreciation { get; set; }
        public DateTime DateEvaluation { get; set; }
        public int IdEvaluation { get; set; }
        public string? TitreEvaluation { get; set; }
        public string? TypeEvaluation { get; set; }
        public string? NomCours { get; set; }
        public string? Periode { get; set; }
        public double? Coefficient { get; set; }
    }

    public class ParcoursPaiementItemDto
    {
        public int IdPaiement { get; set; }
        public DateTime DatePaiement { get; set; }
        public double Montant { get; set; }
        public string? Devise { get; set; }
        public string? ModePaiement { get; set; }
        public string? StatutPaiement { get; set; }
        public string? ReferenceTransaction { get; set; }
        public string? ReferencePaiemenet { get; set; }
        public int? IdFrais { get; set; }
        public string? LibelleFrais { get; set; }
        public double? MontantFrais { get; set; }
    }

    public class ParcoursPresenceItemDto
    {
        public int IdPresence { get; set; }
        public DateTime DateDuJour { get; set; }
        public TimeSpan HeureArrivee { get; set; }
        public TimeSpan? HeureDepart { get; set; }
        public bool? IsPresent { get; set; }
        public string? Observation { get; set; }
        public string? TypePresence { get; set; }
    }
}
