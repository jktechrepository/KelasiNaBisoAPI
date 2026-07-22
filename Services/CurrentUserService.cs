using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace KelasiNaBiso.Services
{
    /// <summary>
    /// Service d'accès aux informations de l'utilisateur connecté via les claims JWT
    /// </summary>
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int UserId => GetClaimAsInt(ClaimTypes.NameIdentifier, JwtRegisteredClaimNames.Sub);

        public string UserRole => GetClaim(ClaimTypes.Role) ?? string.Empty;

        public int EcoleId => GetClaimAsInt("EcoleId", "idEcole");

        public string? EcoleNom => GetClaim("EcoleNom", "ecole");

        public int? TuteurId => GetClaimAsIntOrNull("IdTuteur", "TuteurId");

        public int? AgentId => GetClaimAsIntOrNull("IdAgent", "AgentId");

        public int? EleveId => GetClaimAsIntOrNull("EleveId");

        public string? Email => GetClaim(ClaimTypes.Email, JwtRegisteredClaimNames.Email);

        public string? UserName => GetClaim(ClaimTypes.Name, JwtRegisteredClaimNames.Name);

        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        public bool IsSuperAdmin =>
            _httpContextAccessor.HttpContext?.User?.IsInRole(UserRoles.SUPER_ADMIN) == true
            || string.Equals(UserRole, UserRoles.SUPER_ADMIN, StringComparison.OrdinalIgnoreCase);

        public bool CanAccessAllSchools =>
            IsSuperAdmin
            || _httpContextAccessor.HttpContext?.User?.IsInRole(UserRoles.IT_SUPPORT) == true
            || UserRoles.CanAccessAllSchools(UserRole);

        public bool IsAdmin => UserRoles.IsAdminRole(UserRole);

        public bool IsStaff => UserRoles.IsStaffRole(UserRole);

        public bool HasFinanceAccess => UserRoles.HasFinanceAccess(UserRole);

        public bool HasPedagogieAccess => UserRoles.HasPedagogieAccess(UserRole);

        // ═══════════════════════════════════════════════════════════════════
        // MÉTHODES PRIVÉES UTILITAIRES
        // ═══════════════════════════════════════════════════════════════════

        private string? GetClaim(params string[] claimTypes)
        {
            if (claimTypes == null || claimTypes.Length == 0)
            {
                return null;
            }

            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                return null;
            }

            foreach (var claimType in claimTypes)
            {
                var claim = user.Claims.FirstOrDefault(c =>
                    string.Equals(c.Type, claimType, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrWhiteSpace(claim?.Value))
                {
                    return claim!.Value;
                }
            }

            return null;
        }

        private int GetClaimAsInt(params string[] claimTypes)
        {
            var value = GetClaim(claimTypes);
            return int.TryParse(value, out int result) ? result : 0;
        }

        private int? GetClaimAsIntOrNull(params string[] claimTypes)
        {
            var value = GetClaim(claimTypes);
            return int.TryParse(value, out int result) && result > 0 ? result : null;
        }
    }
}

