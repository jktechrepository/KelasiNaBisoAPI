namespace KelasiNaBiso.Models.DTOs.Communication
{
    public class CommunicationHistoryDto
    {
        public long IdHistory { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Detail { get; set; }
        public DateTime DateAction { get; set; }
        public int? UserId { get; set; }
        public string? NomUtilisateur { get; set; }
        public string? AdresseIP { get; set; }
    }
}

