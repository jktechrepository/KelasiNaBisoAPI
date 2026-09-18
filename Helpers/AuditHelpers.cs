using KelasiNaBiso.Data;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        /// Extrait l'IdEleve du JWT (rôle Eleve).
        /// </summary>
        public static int? GetCurrentEleveId(this ControllerBase controller)
        {
            var claim = controller.User.FindFirst("EleveId")
                        ?? controller.User.FindFirst("IdEleve");

            if (claim != null && int.TryParse(claim.Value, out int idEleve) && idEleve > 0)
                return idEleve;

            return null;
        }

        /// <summary>
        /// Si le JWT contient EleveId, n'autorise que l'accès à cet élève (ReadOwn).
        /// Les autres rôles (sans claim EleveId) ne sont pas restreints ici.
        /// </summary>
        public static IActionResult? ForbidIfWrongEleve(this ControllerBase controller, int targetIdEleve)
        {
            var jwtEleveId = controller.GetCurrentEleveId();
            if (!jwtEleveId.HasValue)
                return null;

            if (jwtEleveId.Value != targetIdEleve)
            {
                return controller.StatusCode(
                    StatusCodes.Status403Forbidden,
                    new { message = "Accès refusé : vous ne pouvez consulter que vos propres données élève." });
            }

            return null;
        }

        /// <summary>
        /// Restreint l'accès aux notifications dont le destinataire est l'utilisateur courant.
        /// Bypass : Super-Admin / Admin / IT-Support.
        /// </summary>
        public static IActionResult? ForbidIfWrongNotificationDestinataire(
            this ControllerBase controller,
            int? idDestinataire)
        {
            if (controller.User.IsInRole(UserRoles.SUPER_ADMIN)
                || controller.User.IsInRole(UserRoles.ADMIN)
                || controller.User.IsInRole(UserRoles.IT_SUPPORT))
                return null;

            var userId = controller.GetCurrentUserId();
            if (!idDestinataire.HasValue || userId <= 0 || idDestinataire.Value != userId)
            {
                return controller.StatusCode(
                    StatusCodes.Status403Forbidden,
                    new { message = "Accès refusé : vous ne pouvez consulter que vos propres notifications." });
            }

            return null;
        }

        /// <summary>
        /// Parent (JWT IdTuteur ou rôle Parent) : n'autorise que si l'élève est rattaché à ce tuteur.
        /// Autres rôles : aucun filtre ici.
        /// </summary>
        public static async Task<IActionResult?> ForbidIfWrongChildAsync(
            this ControllerBase controller,
            int targetIdEleve,
            CancellationToken cancellationToken = default)
        {
            var isParent = controller.User.IsInRole(UserRoles.PARENT);
            var idTuteur = controller.GetCurrentTuteurId();

            if (!isParent && !idTuteur.HasValue)
                return null;

            if (!idTuteur.HasValue || idTuteur.Value <= 0)
            {
                return controller.StatusCode(
                    StatusCodes.Status403Forbidden,
                    new { message = "Accès refusé : identité tuteur absente du token (IdTuteur)." });
            }

            var context = controller.HttpContext.RequestServices.GetRequiredService<KelasiNaBisoDbContext>();
            var isChild = await context.Eleves.AsNoTracking()
                .AnyAsync(
                    e => e.IdEleve == targetIdEleve && e.IdTuteur == idTuteur.Value,
                    cancellationToken);

            if (!isChild)
            {
                return controller.StatusCode(
                    StatusCodes.Status403Forbidden,
                    new { message = "Accès refusé : vous ne pouvez consulter que les données de vos enfants." });
            }

            return null;
        }

        /// <summary>
        /// Variante matricule : si JWT EleveId, le matricule doit correspondre à cet élève.
        /// </summary>
        public static async Task<IActionResult?> ForbidIfWrongEleveMatriculeAsync(
            this ControllerBase controller,
            string matricule,
            CancellationToken cancellationToken = default)
        {
            var jwtEleveId = controller.GetCurrentEleveId();
            if (!jwtEleveId.HasValue)
                return null;

            var context = controller.HttpContext.RequestServices.GetRequiredService<KelasiNaBisoDbContext>();
            var ownMatricule = await context.Eleves.AsNoTracking()
                .Where(e => e.IdEleve == jwtEleveId.Value)
                .Select(e => e.Matricule)
                .FirstOrDefaultAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(ownMatricule)
                || !string.Equals(ownMatricule.Trim(), matricule?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return controller.StatusCode(
                    StatusCodes.Status403Forbidden,
                    new { message = "Accès refusé : vous ne pouvez consulter que vos propres données élève." });
            }

            return null;
        }

        /// <summary>
        /// Variante ReferenceEleve : si JWT EleveId, la référence doit correspondre à cet élève.
        /// </summary>
        public static async Task<IActionResult?> ForbidIfWrongEleveReferenceAsync(
            this ControllerBase controller,
            Guid referenceEleve,
            CancellationToken cancellationToken = default)
        {
            var jwtEleveId = controller.GetCurrentEleveId();
            if (!jwtEleveId.HasValue)
                return null;

            var context = controller.HttpContext.RequestServices.GetRequiredService<KelasiNaBisoDbContext>();
            var ownRef = await context.Eleves.AsNoTracking()
                .Where(e => e.IdEleve == jwtEleveId.Value)
                .Select(e => e.ReferenceEleve)
                .FirstOrDefaultAsync(cancellationToken);

            if (!ownRef.HasValue || ownRef.Value != referenceEleve)
            {
                return controller.StatusCode(
                    StatusCodes.Status403Forbidden,
                    new { message = "Accès refusé : vous ne pouvez consulter que vos propres données élève." });
            }

            return null;
        }

        /// <summary>
        /// Extrait l'IdAgent du JWT (personnel / enseignant).
        /// </summary>
        public static int? GetCurrentAgentId(this ControllerBase controller)
        {
            var claim = controller.User.FindFirst("IdAgent")
                        ?? controller.User.FindFirst("AgentId");

            if (claim != null && int.TryParse(claim.Value, out int idAgent) && idAgent > 0)
                return idAgent;

            return null;
        }

        /// <summary>
        /// Super-Admin / IT-Support / Admin / Directeur / Sous-Directeur : pas de filtre affectation (école gérée à part).
        /// </summary>
        public static bool IsCotationSchoolBypassRole(this ControllerBase controller)
        {
            return controller.User.IsInRole(UserRoles.SUPER_ADMIN)
                || controller.User.IsInRole(UserRoles.IT_SUPPORT)
                || controller.User.IsInRole(UserRoles.ADMIN)
                || controller.User.IsInRole(UserRoles.DIRECTEUR)
                || controller.User.IsInRole(UserRoles.SOUS_DIRECTEUR);
        }

        /// <summary>
        /// Scope cotation sur un cours : école JWT + (si Enseignant) affectation/titulaire.
        /// </summary>
        public static async Task<IActionResult?> ForbidIfHorsScopeCoursAsync(
            this ControllerBase controller,
            int idCours,
            int? idAnneeScolaire = null,
            CancellationToken cancellationToken = default)
        {
            if (controller.User.IsInRole(UserRoles.SUPER_ADMIN)
                || controller.User.IsInRole(UserRoles.IT_SUPPORT))
                return null;

            var context = controller.HttpContext.RequestServices.GetRequiredService<KelasiNaBisoDbContext>();
            var info = await context.Cours.AsNoTracking()
                .Where(c => c.IdCours == idCours)
                .Select(c => new
                {
                    c.IdClasse,
                    IdEcole = c.Classe != null && c.Classe.Direction != null
                        ? c.Classe.Direction.IdEcole
                        : null
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (info == null)
            {
                return controller.NotFound(new { message = $"Cours {idCours} introuvable." });
            }

            var denySchool = controller.ForbidIfWrongSchool(info.IdEcole);
            if (denySchool != null)
                return denySchool;

            if (controller.IsCotationSchoolBypassRole())
                return null;

            if (!controller.User.IsInRole(UserRoles.ENSEIGNANT))
                return null;

            var idAgent = controller.GetCurrentAgentId();
            if (!idAgent.HasValue)
            {
                return controller.StatusCode(
                    StatusCodes.Status403Forbidden,
                    new { message = "Accès refusé : IdAgent absent du token." });
            }

            var pedagogie = controller.HttpContext.RequestServices
                .GetRequiredService<IPedagogieAuthorizationService>();
            var ok = await pedagogie.AgentEnseigneCoursAsync(
                idAgent.Value, idCours, idAnneeScolaire, cancellationToken);
            if (!ok)
            {
                return controller.StatusCode(
                    StatusCodes.Status403Forbidden,
                    new { message = "Accès refusé : vous n'enseignez pas ce cours." });
            }

            return null;
        }

        /// <summary>
        /// Scope cotation sur une classe : école + (si Enseignant) titulaire ∪ affectation.
        /// </summary>
        public static async Task<IActionResult?> ForbidIfHorsScopeClasseAsync(
            this ControllerBase controller,
            int idClasse,
            int? idAnneeScolaire = null,
            CancellationToken cancellationToken = default)
        {
            if (controller.User.IsInRole(UserRoles.SUPER_ADMIN)
                || controller.User.IsInRole(UserRoles.IT_SUPPORT))
                return null;

            var context = controller.HttpContext.RequestServices.GetRequiredService<KelasiNaBisoDbContext>();
            var idEcole = await context.Classes.AsNoTracking()
                .Where(c => c.IdClasse == idClasse)
                .Select(c => c.Direction != null ? c.Direction.IdEcole : null)
                .FirstOrDefaultAsync(cancellationToken);

            if (!idEcole.HasValue)
            {
                return controller.NotFound(new { message = $"Classe {idClasse} introuvable." });
            }

            var denySchool = controller.ForbidIfWrongSchool(idEcole);
            if (denySchool != null)
                return denySchool;

            if (controller.IsCotationSchoolBypassRole())
                return null;

            if (!controller.User.IsInRole(UserRoles.ENSEIGNANT))
                return null;

            var idAgent = controller.GetCurrentAgentId();
            if (!idAgent.HasValue)
            {
                return controller.StatusCode(
                    StatusCodes.Status403Forbidden,
                    new { message = "Accès refusé : IdAgent absent du token." });
            }

            var pedagogie = controller.HttpContext.RequestServices
                .GetRequiredService<IPedagogieAuthorizationService>();
            var ok = await pedagogie.AgentEnseigneClasseAsync(
                idAgent.Value, idClasse, idAnneeScolaire, cancellationToken);
            if (!ok)
            {
                return controller.StatusCode(
                    StatusCodes.Status403Forbidden,
                    new { message = "Accès refusé : vous n'enseignez pas dans cette classe." });
            }

            return null;
        }

        /// <summary>
        /// Scope via IdEvaluation → IdCours.
        /// </summary>
        public static async Task<IActionResult?> ForbidIfHorsScopeEvaluationAsync(
            this ControllerBase controller,
            int idEvaluation,
            int? idAnneeScolaire = null,
            CancellationToken cancellationToken = default)
        {
            var context = controller.HttpContext.RequestServices.GetRequiredService<KelasiNaBisoDbContext>();
            var idCours = await context.Evaluations.AsNoTracking()
                .Where(e => e.IdEvaluation == idEvaluation)
                .Select(e => (int?)e.IdCours)
                .FirstOrDefaultAsync(cancellationToken);

            if (!idCours.HasValue)
            {
                return controller.NotFound(new { message = $"Évaluation {idEvaluation} introuvable." });
            }

            return await controller.ForbidIfHorsScopeCoursAsync(idCours.Value, idAnneeScolaire, cancellationToken);
        }

        /// <summary>
        /// Scope via IdNote → Evaluation → IdCours.
        /// </summary>
        public static async Task<IActionResult?> ForbidIfHorsScopeNoteAsync(
            this ControllerBase controller,
            int idNote,
            CancellationToken cancellationToken = default)
        {
            var context = controller.HttpContext.RequestServices.GetRequiredService<KelasiNaBisoDbContext>();
            var row = await context.Notes.AsNoTracking()
                .Where(n => n.IdNote == idNote)
                .Select(n => new { n.IdEvaluation, n.IdAnneeScolaire })
                .FirstOrDefaultAsync(cancellationToken);

            if (row == null)
            {
                return controller.NotFound(new { message = $"Note {idNote} introuvable." });
            }

            return await controller.ForbidIfHorsScopeEvaluationAsync(
                row.IdEvaluation, row.IdAnneeScolaire, cancellationToken);
        }

        /// <summary>
        /// Saisie décision / appréciation : Admin / Directeur / Sous-Directeur (école)
        /// ou enseignant titulaire de la classe de l'élève pour l'année.
        /// </summary>
        public static async Task<IActionResult?> ForbidIfNotTitulaireOuDirectionAsync(
            this ControllerBase controller,
            int idClasse,
            int? idAnneeScolaire = null,
            CancellationToken cancellationToken = default)
        {
            if (controller.User.IsInRole(UserRoles.SUPER_ADMIN)
                || controller.User.IsInRole(UserRoles.IT_SUPPORT))
                return null;

            var context = controller.HttpContext.RequestServices.GetRequiredService<KelasiNaBisoDbContext>();
            var idEcole = await context.Classes.AsNoTracking()
                .Where(c => c.IdClasse == idClasse)
                .Select(c => c.Direction != null ? c.Direction.IdEcole : null)
                .FirstOrDefaultAsync(cancellationToken);

            if (!idEcole.HasValue)
            {
                return controller.NotFound(new { message = $"Classe {idClasse} introuvable." });
            }

            var denySchool = controller.ForbidIfWrongSchool(idEcole);
            if (denySchool != null)
                return denySchool;

            if (controller.User.IsInRole(UserRoles.ADMIN)
                || controller.User.IsInRole(UserRoles.DIRECTEUR)
                || controller.User.IsInRole(UserRoles.SOUS_DIRECTEUR))
                return null;

            if (!controller.User.IsInRole(UserRoles.ENSEIGNANT))
            {
                return controller.StatusCode(
                    StatusCodes.Status403Forbidden,
                    new { message = "Accès refusé : seuls le titulaire ou la direction peuvent saisir la décision." });
            }

            var idAgent = controller.GetCurrentAgentId();
            if (!idAgent.HasValue)
            {
                return controller.StatusCode(
                    StatusCodes.Status403Forbidden,
                    new { message = "Accès refusé : IdAgent absent du token." });
            }

            var pedagogie = controller.HttpContext.RequestServices
                .GetRequiredService<IPedagogieAuthorizationService>();
            var isTitulaire = await pedagogie.AgentEstTitulaireClasseAsync(
                idAgent.Value, idClasse, idAnneeScolaire, cancellationToken);
            if (!isTitulaire)
            {
                return controller.StatusCode(
                    StatusCodes.Status403Forbidden,
                    new { message = "Accès refusé : vous n'êtes pas titulaire de cette classe." });
            }

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

