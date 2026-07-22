namespace KelasiNaBiso.Models
{
    /// <summary>
    /// Réponse retournée lors d'une authentification réussie avec JWT
    /// </summary>
    public class AuthentificationResponse
    {
        /// <summary>
        /// Indique si l'authentification a réussi
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Message descriptif
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Le JWT Access Token
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// Le Refresh Token (pour obtenir un nouvel access token sans ré-authentification)
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Type de token (toujours "Bearer")
        /// </summary>
        public string TokenType { get; set; } = "Bearer";

        /// <summary>
        /// Durée de validité du token en secondes
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Date d'expiration du token
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Informations de l'utilisateur authentifié
        /// </summary>
        public Utilisateur? Utilisateur { get; set; }

        /// <summary>
        /// Indique si l'utilisateur doit changer son mot de passe
        /// </summary>
        public bool DoitChangerMotDePasse { get; set; }
        
        /// <summary>
        /// Nom du rôle de l'utilisateur
        /// </summary>
        public string? NomRole { get; set; }
        
        /// <summary>
        /// Nom de l'école de l'utilisateur
        /// </summary>
        public string? NomEcole { get; set; }
        
        /// <summary>
        /// Indique si l'école a payé pour les notifications SMS.
        /// Si true, les SMS peuvent être envoyés aux parents.
        /// Si false, aucun SMS ne sera envoyé.
        /// </summary>
        public bool AcceptNotification { get; set; } = true;

        /// <summary>
        /// Liste des permissions de l'utilisateur (ex: ["Ecole.Create", "Paiement.Read"])
        /// </summary>
        public List<string>? Permissions { get; set; }

        // ═══════════════════════════════════════════════════════════════════
        // ✅ MULTI-RÔLES : Rôles de l'utilisateur
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Tous les rôles actifs de l'utilisateur
        /// </summary>
        public List<Role>? Roles { get; set; }

        /// <summary>
        /// Rôle principal de l'utilisateur
        /// </summary>
        public Role? PrimaryRole { get; set; }

    }

    /// <summary>
    /// Informations de l'utilisateur authentifié (pour JWT)
    /// </summary>
    public class UtilisateurAuthInfo
    {
        public int IdUtilisateur { get; set; }
        public int? IdAgent { get; set; }
        public int? IdEleve { get; set; }
        public Guid? ReferenceUtilisateur { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telephone { get; set; }
        public string? PhotoUrl { get; set; }
        public string Genre { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int IdRole { get; set; }
        public int? IdEcole { get; set; }
        public string? NomEcole { get; set; }
    }
}

