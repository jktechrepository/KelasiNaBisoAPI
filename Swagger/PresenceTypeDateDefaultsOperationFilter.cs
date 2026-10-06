using System.Globalization;
using Microsoft.OpenApi.Any;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace KelasiNaBisoAPI.Swagger
{
    public sealed class PresenceTypeDateDefaultsOperationFilter : IOperationFilter
    {
        private const string PresenceTypeRoute = "api/Presence/type/{typePersonne}";
        private const string PresenceTypeDateRoute = "api/Presence/type/{typePersonne}/date/{date}";
        private const string PresenceDateRoute = "api/Presence/date/{date}";
        private const string PresenceDatePagedRoute = "api/Presence/date/{date}/paged";
        private const string PresenceDateRangePagedRoute = "api/Presence/date-range/paged";
        private const string PresenceAgentDateRoute = "api/Presence/agent/{idAgent}/date/{date}";
        private const string PresenceEleveFeuilleAppelRoute = "api/Presence/eleves/classe/{idClasse}/feuille-appel";
        private const string PresenceEleveFeuilleAppelExportRoute = "api/Presence/eleves/classe/{idClasse}/feuille-appel/export";
        private const string PresenceAgentsFeuilleAppelRoute = "api/Presence/agents/ecole/{idEcole}/feuille-appel";
        private const string PresenceAgentsFeuilleAppelExportRoute = "api/Presence/agents/ecole/{idEcole}/feuille-appel/export";
        private const string PresenceAgentsFonctionRoute = "api/Presence/agents/fonction/{fonction}";
        private const string PresenceDashboardEcoleRoute = "api/Presence/dashboard/ecole/{idEcole}";

        public void Apply(Microsoft.OpenApi.Models.OpenApiOperation operation, OperationFilterContext context)
        {
            var route = context.ApiDescription.RelativePath?.Trim('/');
            var isTypeRoute = string.Equals(route, PresenceTypeRoute, StringComparison.OrdinalIgnoreCase);
            var isTypeDateRoute = string.Equals(route, PresenceTypeDateRoute, StringComparison.OrdinalIgnoreCase);
            var isDateRoute = string.Equals(route, PresenceDateRoute, StringComparison.OrdinalIgnoreCase);
            var isPagedDateRoute = string.Equals(route, PresenceDatePagedRoute, StringComparison.OrdinalIgnoreCase);
            var isDateRangePagedRoute = string.Equals(route, PresenceDateRangePagedRoute, StringComparison.OrdinalIgnoreCase);
            var isAgentDateRoute = string.Equals(route, PresenceAgentDateRoute, StringComparison.OrdinalIgnoreCase);
            var isPresenceEleveFeuilleAppelRoute = string.Equals(route, PresenceEleveFeuilleAppelRoute, StringComparison.OrdinalIgnoreCase);
            var isPresenceEleveFeuilleAppelExportRoute = string.Equals(route, PresenceEleveFeuilleAppelExportRoute, StringComparison.OrdinalIgnoreCase);
            var isPresenceAgentsFeuilleAppelRoute = string.Equals(route, PresenceAgentsFeuilleAppelRoute, StringComparison.OrdinalIgnoreCase);
            var isPresenceAgentsFeuilleAppelExportRoute = string.Equals(route, PresenceAgentsFeuilleAppelExportRoute, StringComparison.OrdinalIgnoreCase);
            var isPresenceAgentsFonctionRoute = string.Equals(route, PresenceAgentsFonctionRoute, StringComparison.OrdinalIgnoreCase);
            var isPresenceDashboardEcoleRoute = string.Equals(route, PresenceDashboardEcoleRoute, StringComparison.OrdinalIgnoreCase);
            if (!isTypeRoute && !isTypeDateRoute && !isDateRoute && !isPagedDateRoute &&
                !isDateRangePagedRoute && !isAgentDateRoute && !isPresenceEleveFeuilleAppelRoute &&
                !isPresenceEleveFeuilleAppelExportRoute && !isPresenceAgentsFeuilleAppelRoute &&
                !isPresenceAgentsFeuilleAppelExportRoute && !isPresenceAgentsFonctionRoute &&
                !isPresenceDashboardEcoleRoute)
            {
                return;
            }

            foreach (var parameter in operation.Parameters)
            {
                if ((isTypeRoute || isTypeDateRoute) &&
                    parameter.Name.Equals("typePersonne", StringComparison.OrdinalIgnoreCase))
                {
                    parameter.Schema.Default = new OpenApiString("Agent");
                }
                else if (isDateRangePagedRoute &&
                    (parameter.Name.Equals("dateDebut", StringComparison.OrdinalIgnoreCase) ||
                     parameter.Name.Equals("dateFin", StringComparison.OrdinalIgnoreCase)))
                {
                    parameter.Required = false;
                    parameter.Schema.Type = "string";
                    parameter.Schema.Format = "date";
                    parameter.Schema.Default = new OpenApiString(
                        DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                }
                else if ((isPresenceEleveFeuilleAppelRoute || isPresenceEleveFeuilleAppelExportRoute ||
                          isPresenceAgentsFeuilleAppelRoute || isPresenceAgentsFeuilleAppelExportRoute) &&
                    parameter.Name.Equals("date", StringComparison.OrdinalIgnoreCase))
                {
                    parameter.Required = false;
                    parameter.Schema.Type = "string";
                    parameter.Schema.Format = "date";
                    parameter.Schema.Default = new OpenApiString(
                        DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                }
                else if ((isPresenceEleveFeuilleAppelRoute || isPresenceEleveFeuilleAppelExportRoute ||
                          isPresenceAgentsFeuilleAppelRoute || isPresenceAgentsFeuilleAppelExportRoute) &&
                    (parameter.Name.Equals("dateDebut", StringComparison.OrdinalIgnoreCase) ||
                     parameter.Name.Equals("dateFin", StringComparison.OrdinalIgnoreCase)))
                {
                    parameter.Required = false;
                    parameter.Schema.Type = "string";
                    parameter.Schema.Format = "date";
                }
                else if ((isPresenceAgentsFonctionRoute || isPresenceDashboardEcoleRoute) &&
                    (parameter.Name.Equals("date", StringComparison.OrdinalIgnoreCase) ||
                     parameter.Name.Equals("dateDebut", StringComparison.OrdinalIgnoreCase) ||
                     parameter.Name.Equals("dateFin", StringComparison.OrdinalIgnoreCase)))
                {
                    parameter.Required = false;
                    parameter.Schema.Type = "string";
                    parameter.Schema.Format = "date";
                }
                else if (parameter.Name.Equals("date", StringComparison.OrdinalIgnoreCase))
                {
                    parameter.Schema.Type = "string";
                    parameter.Schema.Format = "date";
                    parameter.Schema.Default = new OpenApiString(
                        DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                }
            }
        }
    }
}
