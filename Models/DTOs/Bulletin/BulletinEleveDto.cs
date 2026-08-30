namespace KelasiNaBiso.Models.DTOs.Bulletin
{
    /// <summary>
    /// Conception : bulletin scolaire d'un élève pour une période / année.
    /// Pas encore persisté — contrat API cible pour la génération PDF.
    /// </summary>
    public class BulletinEleveDto
    {
        public int IdEleve { get; set; }
        public string NomCompletEleve { get; set; } = string.Empty;
        public int IdClasse { get; set; }
        public string NomClasse { get; set; } = string.Empty;
        public int IdEcole { get; set; }
        public string NomEcole { get; set; } = string.Empty;
        public int IdAnneeScolaire { get; set; }
        public string LibelleAnneeScolaire { get; set; } = string.Empty;
        public string Periode { get; set; } = string.Empty;
        public List<BulletinLigneCoursDto> Lignes { get; set; } = new();
        public double? MoyenneGenerale { get; set; }
        public int? Rang { get; set; }
        public int EffectifClasse { get; set; }
        public string? Decision { get; set; }
        public string? AppreciationGenerale { get; set; }
    }

    public class BulletinLigneCoursDto
    {
        public int IdCours { get; set; }
        public string NomCours { get; set; } = string.Empty;
        public double Coefficient { get; set; }
        public List<BulletinNoteDetailDto> Notes { get; set; } = new();
        public double? MoyenneCours { get; set; }
    }

    public class BulletinNoteDetailDto
    {
        public int IdNote { get; set; }
        public int IdEvaluation { get; set; }
        public string? TitreEvaluation { get; set; }
        public string? TypeEvaluation { get; set; }
        public double NoteObtenue { get; set; }
        public double CoefficientEvaluation { get; set; }
        public string? Appreciation { get; set; }
    }
}
