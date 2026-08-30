namespace KelasiNaBiso.Models.DTOs
{
    /// <summary>
    /// Diagnostic admin (sans exposer le hash) pour trancher introuvable vs inactif / mauvaise DB.
    /// </summary>
    public class UtilisateurAdminDiagnosticDto
    {
        public bool Existe { get; set; }
        public int? IdUtilisateur { get; set; }
        public string? Email { get; set; }
        public bool? Statut { get; set; }
        public int? IdEcole { get; set; }
        public string? NomEcole { get; set; }
        public bool? EcoleActive { get; set; }
        public int? IdRole { get; set; }
        public string? NomRole { get; set; }
        public int? MotDePasseHashLength { get; set; }
        public bool AMotDePasseConfigure { get; set; }
        public string? Telephone { get; set; }
        public string? DefaultUsername { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
