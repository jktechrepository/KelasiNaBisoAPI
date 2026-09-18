namespace KelasiNaBiso.Models.DTOs.MokoAfrika
{
    /// <summary>
    /// Payload SignalR pour les événements PayIn Moko (guichet / dashboard école).
    /// </summary>
    public class PayInSignalRNotification
    {
        public string Reference { get; set; } = string.Empty;
        public int? IdPaiement { get; set; }
        public int? IdEleve { get; set; }
        public decimal MontantNet { get; set; }
        public string StatutPaiement { get; set; } = string.Empty;
        public string StatutGateway { get; set; } = string.Empty;
        /// <summary>Description gateway (annulation / erreur) pour l'UI.</summary>
        public string? StatusDescription { get; set; }
    }
}
