namespace KelasiNaBiso.Models.DTOs.Vitrine
{
    /// <summary>École partenaire affichée sur le site vitrine (logo + libellé).</summary>
    public class EcolePartenaireDto
    {
        public int IdEcole { get; set; }
        public string Nom { get; set; } = string.Empty;
        /// <summary>Logo encodé en base64 (sans préfixe data URI).</summary>
        public string? LogoBase64 { get; set; }
        /// <summary>Valeur prête pour src="" (data:image/...;base64,...).</summary>
        public string? LogoSrc { get; set; }
        public string? Type { get; set; }
        public string? Ville { get; set; }
    }
}
