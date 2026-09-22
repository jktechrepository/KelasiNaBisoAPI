namespace KelasiNaBiso.Models.DTOs
{
    /// <summary>Palmarès officiel d'une école (bulletins figés uniquement).</summary>
    public class PalmaresEcoleDto
    {
        public int IdEcole { get; set; }
        public string? NomEcole { get; set; }
        public int IdAnneeScolaire { get; set; }
        public string? LibelleAnneeScolaire { get; set; }
        public int? IdPeriode { get; set; }
        public string? CodePeriode { get; set; }
        public string? LibellePeriode { get; set; }
        public int? IdClasse { get; set; }
        public string? NomClasse { get; set; }
        public int? IdDirection { get; set; }
        public string? NomDirection { get; set; }
        public int EffectifPrisEnCompte { get; set; }
        public int Limit { get; set; }
        public IReadOnlyList<PalmaresEcoleLigneDto> Lignes { get; set; } = Array.Empty<PalmaresEcoleLigneDto>();
    }

    public class PalmaresEcoleLigneDto
    {
        public int? Rang { get; set; }
        public int IdEleve { get; set; }
        public string? Matricule { get; set; }
        public string? Nom { get; set; }
        public string? Postnom { get; set; }
        public string? Prenom { get; set; }
        public string? NomComplet { get; set; }
        public int? IdClasse { get; set; }
        public string? NomClasse { get; set; }
        public double? MoyenneGenerale { get; set; }
        public string? Decision { get; set; }
        public string? AppreciationGenerale { get; set; }
    }
}
