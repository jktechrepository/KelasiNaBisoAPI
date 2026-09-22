using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBisoAPI.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class AppUpdateService : IAppUpdateService
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly IFirebaseNotificationService _firebaseService;

        public AppUpdateService(
            KelasiNaBisoDbContext context,
            IFirebaseNotificationService firebaseService)
        {
            _context = context;
            _firebaseService = firebaseService;
        }

        public async Task<AppUpdateCheckResponseDto> CheckPolicyAsync(
            string platform,
            string appVersion,
            CancellationToken cancellationToken = default)
        {
            if (!AppUpdatePlatforms.TryNormalize(platform, out var normalizedPlatform))
                throw new InvalidOperationException("platform doit être Android ou iOS.");

            if (string.IsNullOrWhiteSpace(appVersion))
                throw new InvalidOperationException("appVersion est obligatoire (semver, ex. 1.2.3).");

            var clientVersion = appVersion.Trim();
            var policy = await _context.MobileAppVersionPolicies.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Platform == normalizedPlatform, cancellationToken);

            if (policy == null || !policy.IsActive)
            {
                return new AppUpdateCheckResponseDto
                {
                    Platform = normalizedPlatform,
                    AppVersion = clientVersion,
                    UpdateLevel = AppUpdateLevels.None,
                    ForceUpdate = false,
                    Message = null,
                    StoreUrl = policy?.StoreUrl,
                    MinSupportedVersion = policy?.MinSupportedVersion,
                    LatestVersion = policy?.LatestVersion,
                    RecommendFromVersion = policy?.RecommendFromVersion
                };
            }

            var level = ResolveUpdateLevel(
                clientVersion,
                policy.MinSupportedVersion,
                policy.RecommendFromVersion,
                policy.LatestVersion);

            return new AppUpdateCheckResponseDto
            {
                Platform = normalizedPlatform,
                AppVersion = clientVersion,
                UpdateLevel = level,
                ForceUpdate = level == AppUpdateLevels.Force,
                Message = level == AppUpdateLevels.None ? null : policy.Message,
                StoreUrl = policy.StoreUrl,
                MinSupportedVersion = policy.MinSupportedVersion,
                LatestVersion = policy.LatestVersion,
                RecommendFromVersion = policy.RecommendFromVersion
            };
        }

        public async Task<IReadOnlyList<AppUpdatePolicyDto>> GetAllPoliciesAsync(
            CancellationToken cancellationToken = default)
        {
            var rows = await _context.MobileAppVersionPolicies.AsNoTracking()
                .OrderBy(p => p.Platform)
                .ToListAsync(cancellationToken);

            return rows.Select(MapPolicy).ToList();
        }

        public async Task<AppUpdatePolicyDto> UpsertPolicyAsync(
            string platform,
            UpsertAppUpdatePolicyDto dto,
            int? idAuteur,
            CancellationToken cancellationToken = default)
        {
            if (!AppUpdatePlatforms.TryNormalize(platform, out var normalizedPlatform))
                throw new InvalidOperationException("platform doit être Android ou iOS.");

            ValidateSemverField(dto.MinSupportedVersion, nameof(dto.MinSupportedVersion));
            ValidateSemverField(dto.LatestVersion, nameof(dto.LatestVersion));
            ValidateSemverField(dto.RecommendFromVersion, nameof(dto.RecommendFromVersion));

            var entity = await _context.MobileAppVersionPolicies
                .FirstOrDefaultAsync(p => p.Platform == normalizedPlatform, cancellationToken);

            if (entity == null)
            {
                entity = new MobileAppVersionPolicy
                {
                    Platform = normalizedPlatform,
                    DateCreation = DateTime.UtcNow
                };
                _context.MobileAppVersionPolicies.Add(entity);
            }

            entity.MinSupportedVersion = dto.MinSupportedVersion.Trim();
            entity.LatestVersion = dto.LatestVersion.Trim();
            entity.RecommendFromVersion = dto.RecommendFromVersion.Trim();
            entity.Message = string.IsNullOrWhiteSpace(dto.Message) ? null : dto.Message.Trim();
            entity.StoreUrl = string.IsNullOrWhiteSpace(dto.StoreUrl) ? null : dto.StoreUrl.Trim();
            entity.IsActive = dto.IsActive;
            entity.DateModification = DateTime.UtcNow;
            entity.IdAuteur = idAuteur;

            await _context.SaveChangesAsync(cancellationToken);
            return MapPolicy(entity);
        }

        public async Task<NotifyAppUpdateResultDto> NotifyAsync(
            NotifyAppUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            string? platformFilter = null;
            if (!string.IsNullOrWhiteSpace(dto.Platform))
            {
                if (!AppUpdatePlatforms.TryNormalize(dto.Platform, out platformFilter))
                    throw new InvalidOperationException("platform doit être Android ou iOS.");
            }

            var level = string.IsNullOrWhiteSpace(dto.UpdateLevel)
                ? AppUpdateLevels.Important
                : dto.UpdateLevel.Trim().ToLowerInvariant();

            if (level is not (AppUpdateLevels.Soft or AppUpdateLevels.Important or AppUpdateLevels.Force))
                throw new InvalidOperationException("updateLevel doit être soft, important ou force.");

            var query = _context.UserDevices.AsNoTracking()
                .Where(d => d.Statut == true && d.FcmToken != null && d.FcmToken != "");

            if (platformFilter != null)
                query = query.Where(d => d.DeviceType != null && d.DeviceType.ToLower() == platformFilter.ToLower());

            var devices = await query.ToListAsync(cancellationToken);

            string? storeUrl = null;
            if (platformFilter != null)
            {
                storeUrl = await _context.MobileAppVersionPolicies.AsNoTracking()
                    .Where(p => p.Platform == platformFilter)
                    .Select(p => p.StoreUrl)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            var data = new Dictionary<string, string>
            {
                ["type"] = "app_update",
                ["updateLevel"] = level
            };
            if (!string.IsNullOrWhiteSpace(storeUrl))
                data["storeUrl"] = storeUrl;

            var success = 0;
            var failure = 0;
            foreach (var device in devices)
            {
                try
                {
                    var ok = await _firebaseService.EnvoyerNotificationATokenAsync(
                        device.FcmToken,
                        dto.Titre.Trim(),
                        dto.Corps.Trim(),
                        data);
                    if (ok) success++;
                    else failure++;
                }
                catch
                {
                    failure++;
                }
            }

            return new NotifyAppUpdateResultDto
            {
                DevicesTargeted = devices.Count,
                SuccessCount = success,
                FailureCount = failure
            };
        }

        /// <summary>Expose pour tests unitaires.</summary>
        public static string ResolveUpdateLevel(
            string clientVersion,
            string minSupported,
            string recommendFrom,
            string latest)
        {
            if (CompareSemver(clientVersion, minSupported) < 0)
                return AppUpdateLevels.Force;

            if (CompareSemver(clientVersion, recommendFrom) < 0)
                return AppUpdateLevels.Important;

            if (CompareSemver(clientVersion, latest) < 0)
                return AppUpdateLevels.Soft;

            return AppUpdateLevels.None;
        }

        /// <summary>
        /// Compare a et b (semver major.minor.patch). Retourne &lt;0 si a&lt;b, 0 si égal, &gt;0 si a&gt;b.
        /// </summary>
        public static int CompareSemver(string a, string b)
        {
            var pa = ParseSemver(a);
            var pb = ParseSemver(b);
            for (var i = 0; i < 3; i++)
            {
                if (pa[i] != pb[i])
                    return pa[i].CompareTo(pb[i]);
            }
            return 0;
        }

        private static int[] ParseSemver(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
                return new[] { 0, 0, 0 };

            var cleaned = version.Trim();
            if (cleaned.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                cleaned = cleaned[1..];

            // Ignorer suffixe pré-release (+build)
            var plus = cleaned.IndexOf('+');
            if (plus >= 0) cleaned = cleaned[..plus];
            var dash = cleaned.IndexOf('-');
            if (dash >= 0) cleaned = cleaned[..dash];

            var parts = cleaned.Split('.', StringSplitOptions.RemoveEmptyEntries);
            var result = new[] { 0, 0, 0 };
            for (var i = 0; i < Math.Min(3, parts.Length); i++)
            {
                var numeric = new string(parts[i].TakeWhile(char.IsDigit).ToArray());
                if (int.TryParse(numeric, out var n))
                    result[i] = n;
            }
            return result;
        }

        private static void ValidateSemverField(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException($"{fieldName} est obligatoire.");

            var parts = ParseSemver(value);
            if (parts[0] == 0 && parts[1] == 0 && parts[2] == 0
                && !value.Trim().StartsWith("0"))
            {
                // "abc" → 0.0.0 — rejeter si rien de numérique
                var cleaned = value.Trim().TrimStart('v', 'V');
                if (!cleaned.Any(char.IsDigit))
                    throw new InvalidOperationException($"{fieldName} doit être un semver (ex. 1.2.3).");
            }
        }

        private static AppUpdatePolicyDto MapPolicy(MobileAppVersionPolicy p) => new()
        {
            IdMobileAppVersionPolicy = p.IdMobileAppVersionPolicy,
            Platform = p.Platform,
            MinSupportedVersion = p.MinSupportedVersion,
            LatestVersion = p.LatestVersion,
            RecommendFromVersion = p.RecommendFromVersion,
            Message = p.Message,
            StoreUrl = p.StoreUrl,
            IsActive = p.IsActive,
            DateModification = p.DateModification
        };
    }
}
