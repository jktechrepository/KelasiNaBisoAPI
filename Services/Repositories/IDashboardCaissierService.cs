using KelasiNaBiso.Models.DTOs.Reporting;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IDashboardCaissierService
    {
        Task<DashboardCaissierDto> GetDashboardCaissierAsync(
            int idEcole,
            int idUtilisateur,
            int? idAnneeScolaire = null,
            DateTime? date = null,
            string scope = DashboardCaissierScopes.Moi,
            bool allowEcoleScope = false);

        Task<DashboardCaissierClotureDto> GetClotureCaissierAsync(
            int idEcole,
            int idUtilisateur,
            int? idAnneeScolaire = null,
            DateTime? date = null,
            string scope = DashboardCaissierScopes.Moi,
            bool allowEcoleScope = false);
    }
}
