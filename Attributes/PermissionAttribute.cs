using KelasiNaBiso.Services.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace KelasiNaBiso.Attributes
{
    /// <summary>
    /// Attribut d'autorisation basé sur les permissions RBAC.
    /// Une seule permission : [Permission("Ecole.Create")]
    /// Plusieurs (OU logique) : [Permission("Note.Read", "Note.ReadOwn")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class PermissionAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string[] _permissions;

        /// <summary>
        /// Une ou plusieurs permissions acceptées (OU logique).
        /// </summary>
        public PermissionAttribute(params string[] permissions)
        {
            if (permissions == null || permissions.Length == 0)
                throw new ArgumentException("Au moins une permission est requise.", nameof(permissions));

            if (permissions.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException("Les noms de permission ne peuvent pas être vides.", nameof(permissions));

            _permissions = permissions;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)
                           ?? user.FindFirst("IdUtilisateur")
                           ?? user.FindFirst(JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                context.Result = new ForbidResult();
                return;
            }

            var permissionService = context.HttpContext.RequestServices
                .GetRequiredService<IPermissionService>();

            var hasPermission = false;
            foreach (var permission in _permissions)
            {
                if (permissionService.UserHasPermissionAsync(userId, permission)
                    .GetAwaiter()
                    .GetResult())
                {
                    hasPermission = true;
                    break;
                }
            }

            if (!hasPermission)
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILogger<PermissionAttribute>>();

                var userRole = user.FindFirst(ClaimTypes.Role)?.Value ?? "Inconnu";
                var required = string.Join(" | ", _permissions);
                logger.LogWarning(
                    "❌ Accès refusé: Utilisateur {UserId} ({Role}) n'a aucune des permissions [{Required}] pour {Method} {Path}",
                    userId, userRole, required, context.HttpContext.Request.Method, context.HttpContext.Request.Path);

                context.Result = new ForbidResult();
            }
        }
    }
}
