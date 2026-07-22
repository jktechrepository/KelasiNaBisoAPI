using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs.Communication
{
    public class CreateCommunicationCampaignDto
    {
        /// <summary>
        /// Identifiant de l'école cible. Obligatoire pour les super-admins.
        /// Pour un admin, sera forcé à son école.
        /// </summary>
        public int? IdEcole { get; set; }

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

        /// <summary>
        /// Indique si la campagne doit être envoyée immédiatement après la création.
        /// Par défaut : true.
        /// </summary>
        public bool SendImmediately { get; set; } = true;

        public DateTime? PlanifiedAt { get; set; }

        public DateTime? ExpirationAt { get; set; }

        /// <summary>
        /// Segments utilisés pour cibler les destinataires.
        /// </summary>
        [Required]
        public List<CreateCommunicationSegmentDto> Segments { get; set; } = new();
    }
}

