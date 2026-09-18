using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace KelasiNaBiso.Models
{
    /// <summary>
    /// Premier téléchargement d'un devoir par un utilisateur (badge estTelechargeParMoi).
    /// </summary>
    public class DevoirADomicileTelechargement
    {
        [Key]
        public int IdDevoirADomicileTelechargement { get; set; }

        [Required]
        public int IdDevoirADomicile { get; set; }

        [Required]
        public int IdUtilisateur { get; set; }

        /// <summary>Date du premier téléchargement (re-download n'écrase pas).</summary>
        public DateTime DateTelechargement { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(IdDevoirADomicile))]
        [JsonIgnore]
        [ValidateNever]
        public DevoirADomicile DevoirADomicile { get; set; } = null!;

        [ForeignKey(nameof(IdUtilisateur))]
        [JsonIgnore]
        [ValidateNever]
        public Utilisateur Utilisateur { get; set; } = null!;
    }
}
