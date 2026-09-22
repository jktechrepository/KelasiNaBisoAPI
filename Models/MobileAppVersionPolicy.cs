using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models
{
    /// <summary>
    /// Policy de version pour l'app mobile (une ligne par plateforme Android / iOS).
    /// </summary>
    public class MobileAppVersionPolicy
    {
        [Key]
        public int IdMobileAppVersionPolicy { get; set; }

        /// <summary>Android | iOS</summary>
        [Required]
        [MaxLength(20)]
        public string Platform { get; set; } = string.Empty;

        /// <summary>Sous ce seuil → force update.</summary>
        [Required]
        [MaxLength(20)]
        public string MinSupportedVersion { get; set; } = "1.0.0";

        /// <summary>Version publiée en store.</summary>
        [Required]
        [MaxLength(20)]
        public string LatestVersion { get; set; } = "1.0.0";

        /// <summary>
        /// Si client &gt;= MinSupported et &lt; RecommendFrom → important ;
        /// si &lt; Latest → soft.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string RecommendFromVersion { get; set; } = "1.0.0";

        [MaxLength(500)]
        public string? Message { get; set; }

        [MaxLength(500)]
        public string? StoreUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime DateCreation { get; set; } = DateTime.UtcNow;
        public DateTime? DateModification { get; set; }
        public int? IdAuteur { get; set; }
    }
}
