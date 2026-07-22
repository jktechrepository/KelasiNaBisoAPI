using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class UpdateSerialNumberDto
    {
        [Required(ErrorMessage = "Le numéro de série est requis")]
        [MaxLength(100, ErrorMessage = "Le numéro de série ne peut pas dépasser 100 caractères")]
        public string SerialNumber { get; set; } = string.Empty;
    }
}

