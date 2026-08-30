namespace KelasiNaBiso.Models
{
    public class CommunicationSegmentCriteria
    {
        public List<int>? ClasseIds { get; set; }
        public List<int>? DirectionIds { get; set; }
        public List<int>? UtilisateurIds { get; set; }
        public List<int>? TuteurIds { get; set; }
        public List<string>? Tags { get; set; }
        public string? Niveau { get; set; }
    }
}

