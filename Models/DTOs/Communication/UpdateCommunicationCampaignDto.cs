using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs.Communication
{
    public class UpdateCommunicationCampaignDto
    {
        [Required]
        [MaxLength(200)]
        public string Titre { get; set; } = string.Empty;

        [Required]
        public string ContenuMarkdown { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Importance { get; set; } = "Info";

        [Required]
        public CommunicationChannelsDto Canaux { get; set; } = new();

        public bool RappelAuto { get; set; }

        public DateTime? PlanifiedAt { get; set; }

        public DateTime? ExpirationAt { get; set; }

        public List<CreateCommunicationSegmentDto>? Segments { get; set; }
    }
}

