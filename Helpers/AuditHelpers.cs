using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace KelasiNaBiso.Helpers
{
    /// <summary>
    /// Helpers pour faciliter l'audit dans les controllers
    /// </summary>
    public static class AuditHelpers
    {
        /// <summary>
        /// Extrait l'ID de l'utilisateur actuel depuis le JWT token
        /// </summary>
        public static int GetCurrentUserId(this ControllerBase controller)
        {
            var userIdClaim = controller.User.FindFirst(ClaimTypes.NameIdentifier)
                           ?? controller.User.FindFirst("IdUtilisateur")
                           ?? controller.User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            return 0; // Utilisateur non identifié
        }

        /// <summary>
        /// Extrait le nom complet de l'utilisateur actuel depuis le JWT token
        /// </summary>
        public static string GetCurrentUserName(this ControllerBase controller)
        {
            var nameClaim = controller.User.FindFirst(ClaimTypes.Name)
                         ?? controller.User.FindFirst("NomUtilisateur");

            return nameClaim?.Value ?? "Utilisateur Inconnu";
        }

        /// <summary>
        /// Extrait le rôle de l'utilisateur actuel depuis le JWT token
        /// </summary>
        public static string? GetCurrentUserRole(this ControllerBase controller)
        {
            var roleClaim = controller.User.FindFirst(ClaimTypes.Role)
                         ?? controller.User.FindFirst("Role");

            return roleClaim?.Value;
        }

        /// <summary>
        /// Extrait l'ID de l'école de l'utilisateur actuel depuis le JWT token
        /// </summary>
        public static int? GetCurrentUserSchoolId(this ControllerBase controller)
        {
            // Essayer plusieurs noms de claims possibles
            var schoolClaim = controller.User.FindFirst("IdEcole")
                           ?? controller.User.FindFirst("idEcole");

            if (schoolClaim != null && !string.IsNullOrWhiteSpace(schoolClaim.Value))
            {
                if (int.TryParse(schoolClaim.Value, out int idEcole) && idEcole > 0)
                {
                    return idEcole;
                }
            }

            return null;
        }

        /// <summary>
        /// Extrait l'IdTuteur du JWT (Parent).
        /// </summary>
        public static int? GetCurrentTuteurId(this ControllerBase controller)
        {
            var claim = controller.User.FindFirst("IdTuteur")
                        ?? controller.User.FindFirst("TuteurId");

            if (claim != null && int.TryParse(claim.Value, out int idTuteur) && idTuteur > 0)
                return idTuteur;

            return null;
        }

        /// <summary>
        /// Super-Admin / IT-Support : toutes les écoles.
        /// Autres rôles : uniquement leur IdEcole JWT (sync — sans exception Parent multi-écoles).
        /// Préférer <see cref="ForbidIfWrongSchoolAsync"/> pour les routes accessibles aux Parents.
        /// </summary>
        public static IActionResult? ForbidIfWrongSchool(this ControllerBase controller, int? targetEcoleId)
        {
            if (controller.User.IsInRole(UserRoles.SUPER_ADMIN)
                || controller.User.IsInRole(UserRoles.IT_SUPPORT))
                return null;

            var userEcole = controller.GetCurrentUserSchoolId();
            if (!userEcole.HasValue || !targetEcoleId.HasValue || userEcole.Value != targetEcoleId.Value)
            {
                return ForbiddenSchool(controller);
            }

            return null;
        }

        /// <summary>
        /// Comme <see cref="ForbidIfWrongSchool"/>, plus exception Parent :
        /// autorisé si IdTuteur JWT a au moins un enfant avec inscription confirmée dans l'école cible.
        /// </summary>
        public static async Task<IActionResult?> ForbidIfWrongSchoolAsync(
            this ControllerBase controller,
            int? targetEcoleId,
            IInscriptionActiveResolver? inscriptionResolver = null,
            CancellationToken cancellationToken = default)
        {
            if (controller.User.IsInRole(UserRoles.SUPER_ADMIN)
                || controller.User.IsInRole(UserRoles.IT_SUPPORT))
                return null;

            if (!targetEcoleId.HasValue || targetEcoleId.Value <= 0)
                return ForbiddenSchool(controller);

            var userEcole = controller.GetCurrentUserSchoolId();
            if (userEcole.HasValue && userEcole.Value == targetEcoleId.Value)
                return null;

            if (controller.User.IsInRole(UserRoles.PARENT))
            {
                var idTuteur = controller.GetCurrentTuteurId();
                if (idTuteur.HasValue)
                {
                    var resolver = inscriptionResolver
                        ?? controller.HttpContext.RequestServices.GetService<IInscriptionActiveResolver>();

                    if (resolver != null
                        && await resolver.IsTuteurInEcoleAsync(idTuteur.Value, targetEcoleId.Value, cancellationToken))
                    {
                        return null;
                    }
                }
            }

            return ForbiddenSchool(controller);
        }

        private static IActionResult ForbiddenSchool(ControllerBase controller) =>
            controller.StatusCode(
                StatusCodes.Status403Forbidden,
                new { message = "Acces refuse a cette ecole." });

        /// <summary>
        /// Rôle effectif pour le contrôle d’accès multi-écoles (priorité Super-Admin / IT-Support).
        /// </summary>
        public static string? GetSchoolAccessRole(this ControllerBase controller)
        {
            if (controller.User.IsInRole(UserRoles.SUPER_ADMIN))
                return UserRoles.SUPER_ADMIN;
            if (controller.User.IsInRole(UserRoles.IT_SUPPORT))
                return UserRoles.IT_SUPPORT;
            return controller.GetCurrentUserRole();
        }

        /// <summary>
        /// Extrait l'adresse IP du client
        /// </summary>
        public static string? GetClientIpAddress(this ControllerBase controller)
        {
            try
            {
                // Vérifier si derrière un proxy (ex: Nginx, Apache)
                var forwardedFor = controller.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    // Prendre la première IP si plusieurs (client, proxy1, proxy2)
                    return forwardedFor.Split(',')[0].Trim();
                }

                // Vérifier X-Real-IP
                var realIp = controller.Request.Headers["X-Real-IP"].FirstOrDefault();
                if (!string.IsNullOrEmpty(realIp))
                {
                    return realIp;
                }

                // Sinon, prendre l'IP de connexion directe
                return controller.HttpContext.Connection.RemoteIpAddress?.ToString();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Extrait le User-Agent (navigateur ou app mobile)
        /// </summary>
        public static string? GetUserAgent(this ControllerBase controller)
        {
            try
            {
                return controller.Request.Headers["User-Agent"].FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Extrait toutes les informations d'audit en une seule fois
        /// </summary>
        public static AuditContext GetAuditContext(this ControllerBase controller)
        {
            return new AuditContext
            {
                UserId = controller.GetCurrentUserId(),
                UserName = controller.GetCurrentUserName(),
                UserRole = controller.GetCurrentUserRole(),
                IdEcole = controller.GetCurrentUserSchoolId(),
                IpAddress = controller.GetClientIpAddress(),
                UserAgent = controller.GetUserAgent()
            };
        }
    }

    /// <summary>
    /// Contexte d'audit complet
    /// </summary>
    public class AuditContext
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserRole { get; set; }
        public int? IdEcole { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}

