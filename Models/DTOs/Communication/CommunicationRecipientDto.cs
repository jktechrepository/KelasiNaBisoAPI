namespace KelasiNaBiso.Models.DTOs.Communication
{
    public class CommunicationRecipientDto
    {
        public long IdRecipient { get; set; }
        public int IdUtilisateur { get; set; }
        public int? IdTuteur { get; set; }
        public int? IdEleve { get; set; }
        public string NomDestinataire { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? PreferredChannel { get; set; }
        public string? FinalChannel { get; set; }
        public DateTime DatePlanifie { get; set; }
        public DateTime? DateEnvoi { get; set; }
        public string? ErrorMessage { get; set; }
        public string? NomEleve { get; set; }
        public string? Classe { get; set; }
    }
}

