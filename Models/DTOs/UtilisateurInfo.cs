namespace KelasiNaBiso.Models.DTOs
{
    /// <summary>
    /// DTO pour retourner les informations d'un compte utilisateur créé automatiquement
    /// </summary>
    public class UtilisateurInfo
    {
        public int IdUtilisateur { get; set; }
        public int? IdEleve { get; set; }
        public int? IdAgent { get; set; }
        public int? IdTuteur { get; set; }
        public string Email { get; set; } = string.Empty;
        public string DefaultUsername { get; set; } = string.Empty;
        public string Telephone { get; set; } = string.Empty;
        public string MotDePasseParDefaut { get; set; } = string.Empty;
        public string NomComplet { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}

