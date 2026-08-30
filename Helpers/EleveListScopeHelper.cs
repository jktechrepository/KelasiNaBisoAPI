using Microsoft.AspNetCore.Mvc;

namespace KelasiNaBiso.Helpers
{
    /// <summary>
    /// Résolution idEcole pour les listes élèves (query ou JWT).
    /// </summary>
    public static class EleveListScopeHelper
    {
        public static IActionResult? TryResolveListIdEcole(
            this ControllerBase controller,
            int? idEcoleQuery,
            out int idEcole)
        {
            idEcole = 0;
            if (idEcoleQuery.HasValue && idEcoleQuery.Value > 0)
            {
                var deny = controller.ForbidIfWrongSchool(idEcoleQuery.Value);
                if (deny != null)
                    return deny;

                idEcole = idEcoleQuery.Value;
                return null;
            }

            var fromToken = controller.GetCurrentUserSchoolId();
            if (fromToken.HasValue && fromToken.Value > 0)
            {
                idEcole = fromToken.Value;
                return null;
            }

            return controller.BadRequest(new
            {
                message = "idEcole est requis (paramètre query ou claim JWT). " +
                          "Les Super-Admin / IT-Support doivent passer ?idEcole=..."
            });
        }
    }
}
