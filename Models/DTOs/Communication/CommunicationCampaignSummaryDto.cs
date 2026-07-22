namespace KelasiNaBiso.Models.DTOs.Communication
{
    public class CommunicationCampaignSummaryDto
    {
        public int IdCampaign { get; set; }
        public int IdEcole { get; set; }
        public string NomEcole { get; set; } = string.Empty;
        public string Titre { get; set; } = string.Empty;
        public string Importance { get; set; } = string.Empty;
        public string Statut { get; set; } = string.Empty;
        public DateTime DateCreation { get; set; }
        public DateTime? PlanifiedAt { get; set; }
        public DateTime? ExpirationAt { get; set; }
        public bool RappelAuto { get; set; }
        public int TotalDestinataires { get; set; }
        public int Envoyes { get; set; }
        public int Echecs { get; set; }
        public string Auteur { get; set; } = string.Empty;
        public string? Validateur { get; set; }
    }
}

