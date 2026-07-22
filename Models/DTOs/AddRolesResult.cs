namespace KelasiNaBiso.Models.DTOs
{
    /// <summary>
    /// Résultat de l'ajout de plusieurs rôles à un agent
    /// </summary>
    public class AddRolesResult
    {
        /// <summary>
        /// Indique si au moins un rôle a été ajouté avec succès
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Nombre total de rôles dans la requête
        /// </summary>
        public int TotalRoles { get; set; }

        /// <summary>
        /// Nombre de rôles ajoutés avec succès
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// Nombre de rôles qui ont échoué
        /// </summary>
        public int FailureCount { get; set; }

        /// <summary>
        /// Liste des rôles ajoutés avec succès
        /// </summary>
        public List<RoleOperationResult> SuccessRoles { get; set; } = new List<RoleOperationResult>();

        /// <summary>
        /// Liste des rôles qui ont échoué avec les raisons
        /// </summary>
        public List<RoleOperationResult> FailedRoles { get; set; } = new List<RoleOperationResult>();

        /// <summary>
        /// Message de résumé
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Résultat d'une opération sur un rôle individuel
    /// </summary>
    public class RoleOperationResult
    {
        /// <summary>
        /// Nom du rôle (RoleAgent)
        /// </summary>
        public string RoleAgent { get; set; } = string.Empty;

        /// <summary>
        /// Indique si ce rôle était marqué comme principal
        /// </summary>
        public bool IsPrimary { get; set; }

        /// <summary>
        /// Message de succès ou d'erreur
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}

