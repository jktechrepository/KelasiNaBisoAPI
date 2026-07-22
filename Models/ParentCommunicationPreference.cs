using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace KelasiNaBiso.Models
{
    public class ParentCommunicationPreference
    {
        [Key]
        public int IdPreference { get; set; }

        [Required]
        public int IdTuteur { get; set; }

        [Required]
        [MaxLength(20)]
        public string Canal { get; set; } = string.Empty;

        [Required]
        public bool OptIn { get; set; }

        public DateTime? DateAcceptation { get; set; }

        public DateTime? DateRefus { get; set; }

        [MaxLength(50)]
        public string? Source { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(IdTuteur))]
        public Tuteur? Tuteur { get; set; }
    }
}

