using KelasiNaBiso.Helpers;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace KelasiNaBiso.Attributes
{
    /// <summary>
    /// Filtre global : refuse l'accès cross-école sur les routes contenant idEcole / ecoleId.
    /// Super-Admin et IT-Support : toutes les écoles.
    /// Parent : écoles où le tuteur a au moins un enfant inscrit (confirmé).
    /// </summary>
    public class SchoolTenantActionFilter : IAsyncActionFilter
    {
        private static readonly string[] SchoolRouteKeys = { "idEcole", "ecoleId", "IdEcole", "EcoleId" };

        private readonly IInscriptionActiveResolver _inscriptionResolver;

        public SchoolTenantActionFilter(IInscriptionActiveResolver inscriptionResolver)
        {
            _inscriptionResolver = inscriptionResolver;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.Controller is not ControllerBase controller)
            {
                await next();
                return;
            }

            // Endpoints anonymes / non authentifiés : ne pas appliquer
            if (controller.User.Identity?.IsAuthenticated != true)
            {
                await next();
                return;
            }

            int? targetEcoleId = null;

            foreach (var key in SchoolRouteKeys)
            {
                if (context.RouteData.Values.TryGetValue(key, out var routeVal)
                    && int.TryParse(routeVal?.ToString(), out var fromRoute)
                    && fromRoute > 0)
                {
                    targetEcoleId = fromRoute;
                    break;
                }

                if (context.ActionArguments.TryGetValue(key, out var argVal)
                    && argVal != null
                    && int.TryParse(argVal.ToString(), out var fromArg)
                    && fromArg > 0)
                {
                    targetEcoleId = fromArg;
                    break;
                }
            }

            if (targetEcoleId.HasValue)
            {
                var deny = await controller.ForbidIfWrongSchoolAsync(
                    targetEcoleId,
                    _inscriptionResolver,
                    context.HttpContext.RequestAborted);
                if (deny != null)
                {
                    context.Result = deny;
                    return;
                }
            }

            await next();
        }
    }

    /// <summary>
    /// Restreint un GetAll global aux Super-Admin / IT-Support (évite les dumps cross-tenant).
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class RequireGlobalAccessAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (user.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            if (user.IsInRole(UserRoles.SUPER_ADMIN) || user.IsInRole(UserRoles.IT_SUPPORT))
            {
                return;
            }

            context.Result = new ObjectResult(new { message = "Acces global reserve au Super-Admin / IT-Support." })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}
