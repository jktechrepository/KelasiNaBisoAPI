using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class UpdateDirectionDto
    {
        [Required]
        public int IdDirection { get; set; }
        
        [Required]
        [StringLength(200)]
        public string? NomDirection { get; set; }
        
      //  [StringLength(20)]
        public string? NiveauEnseignement { get; set; }
    }
}
