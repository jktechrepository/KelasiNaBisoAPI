using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    /// <summary>
    /// DTO pour ajouter un rôle à un agent
    /// Le RoleAgent correspond au Nom du rôle dans la table Roles
    /// </summary>
    public class AddRoleToAgentRequest
    {
        /// <summary>
        /// Le nom du rôle à ajouter (correspond au champ Nom de la table Roles)
        /// Exemples: "Enseignant", "Directeur", "Financier", "Admin", etc.
        /// </summary>
        [Required(ErrorMessage = "Le RoleAgent est obligatoire")]
        [MaxLength(100, ErrorMessage = "Le RoleAgent ne peut pas dépasser 100 caractères")]
        public string RoleAgent { get; set; } = string.Empty;

        /// <summary>
        /// Indique si ce rôle doit être défini comme rôle principal pour l'utilisateur associé à l'agent.
        /// Par défaut : false (le rôle sera ajouté comme rôle secondaire)
        /// </summary>
        public bool IsPrimary { get; set; } = false;
    }
}

