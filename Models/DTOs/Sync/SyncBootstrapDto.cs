namespace KelasiNaBiso.Models.DTOs.Sync
{
    /// <summary>
    /// Snapshot initial offline (étape 0 : listes vides ; peuplement en étape 1).
    /// Mapping portable : eleves ↔ clients, fraisDus ↔ arrears.
    /// </summary>
    public class SyncBootstrapDto
    {
        public string Watermark { get; set; } = string.Empty;
        public string Snapshot { get; set; } = string.Empty;
        public int IdEcole { get; set; }
        public List<EleveSyncDto> Eleves { get; set; } = new();
        public List<FraisDuSyncDto> FraisDus { get; set; } = new();
    }

    /// <summary>Fiche élève pour store local (lecture seule offline). Contenu enrichi en P1.</summary>
    public class EleveSyncDto
    {
        public int IdEleve { get; set; }
        public string? Matricule { get; set; }
        public string? Nom { get; set; }
        public string? Postnom { get; set; }
        public string? Prenom { get; set; }
        public int? IdClasse { get; set; }
        public string? NomClasse { get; set; }
        public bool IsActif { get; set; } = true;
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>Poste à encaisser (frais dus). Contenu enrichi en P1.</summary>
    public class FraisDuSyncDto
    {
        public int IdFrais { get; set; }
        public int IdEleve { get; set; }
        public string? Libelle { get; set; }
        /// <summary>Dû effectif (catalogue après exonération). Rétrocompat : total à payer avant paiements.</summary>
        public decimal MontantTotal { get; set; }
        public decimal MontantPaye { get; set; }
        /// <summary>Reste = max(0, dû effectif − payé).</summary>
        public decimal MontantDu { get; set; }
        public string? CodeDevise { get; set; }
        public DateTime? DateModification { get; set; }
        /// <summary>Montant catalogue <c>Frais.Montant</c> (optionnel, enrichissement exonération).</summary>
        public decimal? MontantCatalogue { get; set; }
        /// <summary>Réduction appliquée (catalogue − dû effectif).</summary>
        public decimal? MontantReduction { get; set; }
        public string? CodeCategorie { get; set; }
    }
}
