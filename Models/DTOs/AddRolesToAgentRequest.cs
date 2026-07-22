using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    /// <summary>
    /// DTO pour ajouter plusieurs rôles à un agent en une seule requête
    /// Le RoleAgent correspond au Nom du rôle dans la table Roles
    /// </summary>
    public class AddRolesToAgentRequest
    {
        /// <summary>
        /// Liste des rôles à ajouter à l'agent
        /// Chaque élément contient le RoleAgent et IsPrimary
        /// </summary>
        [Required(ErrorMessage = "La liste des rôles est obligatoire")]
        [MinLength(1, ErrorMessage = "Au moins un rôle doit être fourni")]
        public List<AddRoleToAgentRequest> Roles { get; set; } = new List<AddRoleToAgentRequest>();
    }
}

