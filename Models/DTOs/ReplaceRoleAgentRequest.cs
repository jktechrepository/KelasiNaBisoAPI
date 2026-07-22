using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    /// <summary>
    /// DTO pour remplacer un RoleAgent par un autre
    /// </summary>
    public class ReplaceRoleAgentRequest
    {
        /// <summary>
        /// Ancien RoleAgent à remplacer (correspond au champ Nom de la table Roles)
        /// </summary>
        [Required(ErrorMessage = "L'ancien RoleAgent est requis")]
        public string AncienRoleAgent { get; set; } = string.Empty;

        /// <summary>
        /// Nouveau RoleAgent à affecter (correspond au champ Nom de la table Roles)
        /// </summary>
        [Required(ErrorMessage = "Le nouveau RoleAgent est requis")]
        public string NouveauRoleAgent { get; set; } = string.Empty;
    }
}

