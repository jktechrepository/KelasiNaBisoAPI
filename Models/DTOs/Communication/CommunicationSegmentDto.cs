namespace KelasiNaBiso.Models.DTOs.Communication
{
    public class CommunicationSegmentDto
    {
        public int IdSegment { get; set; }
        public string NomSegment { get; set; } = string.Empty;
        public string TypeSegment { get; set; } = string.Empty;
        public bool IsReusable { get; set; }
        public Dictionary<string, object?> Criteria { get; set; } = new();
        public DateTime DateCreation { get; set; }
        public int CreatedBy { get; set; }
        public string? CreatedByNom { get; set; }
    }
}

