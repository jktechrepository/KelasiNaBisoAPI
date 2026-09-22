using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public static class AppUpdateLevels
    {
        public const string None = "none";
        public const string Soft = "soft";
        public const string Important = "important";
        public const string Force = "force";
    }

    public static class AppUpdatePlatforms
    {
        public const string Android = "Android";
        public const string Ios = "iOS";

        public static bool TryNormalize(string? platform, out string normalized)
        {
            normalized = string.Empty;
            if (string.IsNullOrWhiteSpace(platform))
                return false;

            var p = platform.Trim();
            if (p.Equals(Android, StringComparison.OrdinalIgnoreCase)
                || p.Equals("android", StringComparison.OrdinalIgnoreCase))
            {
                normalized = Android;
                return true;
            }

            if (p.Equals(Ios, StringComparison.OrdinalIgnoreCase)
                || p.Equals("ios", StringComparison.OrdinalIgnoreCase)
                || p.Equals("iphone", StringComparison.OrdinalIgnoreCase))
            {
                normalized = Ios;
                return true;
            }

            return false;
        }
    }

    public class AppUpdatePolicyDto
    {
        public int IdMobileAppVersionPolicy { get; set; }
        public string Platform { get; set; } = string.Empty;
        public string MinSupportedVersion { get; set; } = string.Empty;
        public string LatestVersion { get; set; } = string.Empty;
        public string RecommendFromVersion { get; set; } = string.Empty;
        public string? Message { get; set; }
        public string? StoreUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime? DateModification { get; set; }
    }

    public class UpsertAppUpdatePolicyDto
    {
        [Required]
        [MaxLength(20)]
        public string MinSupportedVersion { get; set; } = "1.0.0";

        [Required]
        [MaxLength(20)]
        public string LatestVersion { get; set; } = "1.0.0";

        [Required]
        [MaxLength(20)]
        public string RecommendFromVersion { get; set; } = "1.0.0";

        [MaxLength(500)]
        public string? Message { get; set; }

        [MaxLength(500)]
        public string? StoreUrl { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class AppUpdateCheckResponseDto
    {
        public string Platform { get; set; } = string.Empty;
        public string AppVersion { get; set; } = string.Empty;
        public string UpdateLevel { get; set; } = AppUpdateLevels.None;
        public bool ForceUpdate { get; set; }
        public string? Message { get; set; }
        public string? StoreUrl { get; set; }
        public string? MinSupportedVersion { get; set; }
        public string? LatestVersion { get; set; }
        public string? RecommendFromVersion { get; set; }
    }

    public class NotifyAppUpdateDto
    {
        [Required]
        [MaxLength(200)]
        public string Titre { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Corps { get; set; } = string.Empty;

        /// <summary>Android | iOS — optionnel ; si omis, tous les devices actifs.</summary>
        [MaxLength(20)]
        public string? Platform { get; set; }

        /// <summary>soft | important | force — transmis dans data FCM.</summary>
        [MaxLength(20)]
        public string UpdateLevel { get; set; } = AppUpdateLevels.Important;
    }

    public class NotifyAppUpdateResultDto
    {
        public int DevicesTargeted { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
    }
}
