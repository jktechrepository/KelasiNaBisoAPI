using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs.Reporting
{
    public class CarteBatchRequestDto
    {
        [Required]
        public int IdEcole { get; set; }

        [Required]
        [MinLength(1)]
        public List<int> Ids { get; set; } = new();
    }
}
