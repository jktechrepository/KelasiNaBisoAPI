using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    /// <summary>
    /// Liaison Frais ↔ Direction (portée Direction).
    /// </summary>
    public class FraisDirection
    {
        public int IdFrais { get; set; }
        public int IdDirection { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public Frais Frais { get; set; } = null!;

        [JsonIgnore]
        [ValidateNever]
        public Direction Direction { get; set; } = null!;
    }
}
