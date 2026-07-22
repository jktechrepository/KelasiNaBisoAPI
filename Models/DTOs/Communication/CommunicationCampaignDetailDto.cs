namespace KelasiNaBiso.Models.DTOs.Communication
{
    public class CommunicationCampaignDetailDto
    {
        public int IdCampaign { get; set; }
        public int IdEcole { get; set; }
        public string NomEcole { get; set; } = string.Empty;
        public string Titre { get; set; } = string.Empty;
        public string ContenuMarkdown { get; set; } = string.Empty;
        public string Importance { get; set; } = string.Empty;
        public string Statut { get; set; } = string.Empty;
        public CommunicationChannelsDto Canaux { get; set; } = new();
        public bool RappelAuto { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? PlanifiedAt { get; set; }
        public DateTime? ExpirationAt { get; set; }
        public DateTime? ValidationAt { get; set; }
        public int IdAuteur { get; set; }
        public string NomAuteur { get; set; } = string.Empty;
        public int? ValidatedBy { get; set; }
        public string? NomValidateur { get; set; }
        public List<CommunicationSegmentDto> Segments { get; set; } = new();
        public int TotalDestinataires { get; set; }
        public int Envoyes { get; set; }
        public int Echecs { get; set; }
    }
}

