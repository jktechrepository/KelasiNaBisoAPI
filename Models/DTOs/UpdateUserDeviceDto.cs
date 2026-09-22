using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class UpdateUserDeviceDto
    {
        [Required]
        public int IdUserDevice { get; set; }
        
        [StringLength(500)]
        public string? FcmToken { get; set; }
        
        [StringLength(100)]
        public string? DeviceType { get; set; }
        
        [StringLength(100)]
        public string? DeviceModel { get; set; }
        
        [StringLength(50)]
        public string? OsVersion { get; set; }

        [StringLength(20)]
        public string? AppVersion { get; set; }
        
        public bool? Statut { get; set; } = true;
    }
}
