namespace KelasiNaBiso.Models.DTOs.Reporting
{
    public class CarteAgentDataDto
    {
        public int IdAgent { get; set; }
        public int? IdEcole { get; set; }
        public string Matricule { get; set; } = string.Empty;
        public string NomComplet { get; set; } = string.Empty;
        public string Fonction { get; set; } = string.Empty;
        public string RoleAgent { get; set; } = string.Empty;
        public string NomEcole { get; set; } = string.Empty;
        public string SloganEcole { get; set; } = string.Empty;
        /// <summary>Logo école en base64 brut (stockage base).</summary>
        public string? LogoBase64 { get; set; }
        /// <summary>Photo agent en base64 brut (stockage base).</summary>
        public string? PhotoBase64 { get; set; }
        /// <summary>Valeur prête pour src="" (data URI).</summary>
        public string? LogoSrc { get; set; }
        public string? PhotoSrc { get; set; }
        public string? LogoBytesBase64 { get; set; }
        public string? PhotoBytesBase64 { get; set; }
    }
}
