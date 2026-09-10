using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    /// <summary>
    /// Liaison Frais ↔ Classe (portée Classe).
    /// </summary>
    public class FraisClasse
    {
        public int IdFrais { get; set; }
        public int IdClasse { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public Frais Frais { get; set; } = null!;

        [JsonIgnore]
        [ValidateNever]
        public Classe Classe { get; set; } = null!;
    }
}
