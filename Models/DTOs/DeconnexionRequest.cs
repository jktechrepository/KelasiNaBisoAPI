namespace KelasiNaBiso.Models.DTOs
{
    /// <summary>
    /// Requête de déconnexion d'un utilisateur
    /// </summary>
    public class DeconnexionRequest
    {
        /// <summary>
        /// Token FCM à supprimer (optionnel)
        /// </summary>
        public string? FcmToken { get; set; }

        /// <summary>
        /// Identifiant du device à désactiver (optionnel)
        /// </summary>
        public int? IdUserDevice { get; set; }

        /// <summary>
        /// Désactiver tous les devices enregistrés pour l'utilisateur
        /// </summary>
        public bool SupprimerTousLesDevices { get; set; } = false;
    }

    /// <summary>
    /// Réponse standard pour la déconnexion
    /// </summary>
    public class DeconnexionResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int DevicesDesactives { get; set; }
    }
}

