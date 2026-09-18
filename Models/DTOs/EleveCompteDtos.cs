using KelasiNaBiso.Models.DTOs;

namespace KelasiNaBiso.Models.DTOs
{
    public enum EleveCompteCreateOutcome
    {
        Created,
        AlreadyLinked,
        LinkedExistingUsername,
        ConflictUsername,
        NoMatricule,
        InvalidEleve,
        Error
    }

    public class EleveCompteCreateResult
    {
        public EleveCompteCreateOutcome Outcome { get; set; }
        public UtilisateurInfo? Info { get; set; }
        public string? Message { get; set; }
        public int? IdEleve { get; set; }
        public string? Matricule { get; set; }
    }

    public class EleveCompteBackfillResult
    {
        public int IdEcole { get; set; }
        public bool DryRun { get; set; }
        public int Created { get; set; }
        public int SkippedAlreadyLinked { get; set; }
        public int SkippedNoMatricule { get; set; }
        public int ConflictsUsername { get; set; }
        public int Errors { get; set; }
        public int Candidates { get; set; }
        public List<string> ErrorDetails { get; set; } = new();
        public List<EleveCompteCreateResult> Details { get; set; } = new();
    }
}
