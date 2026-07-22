using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs.Communication
{
    public class CancelCommunicationRequest
    {
        [MaxLength(500)]
        public string? Message { get; set; }
    }
}

