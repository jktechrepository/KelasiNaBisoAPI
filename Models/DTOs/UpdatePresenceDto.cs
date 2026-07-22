using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class UpdatePresenceDto
    {
        [Required]
        public int IdPresence { get; set; }
        
        public bool? IsPresent { get; set; }
        
        [Required]
        public TimeSpan HeureArrivee { get; set; }
        
        public TimeSpan? HeureDepart { get; set; }
    }
}
