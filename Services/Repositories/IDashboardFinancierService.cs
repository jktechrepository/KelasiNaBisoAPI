using KelasiNaBiso.Models.DTOs.Reporting;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IDashboardFinancierService
    {
        Task<DashboardFinancierDto> GetDashboardFinancierAsync(
            int idEcole,
            int idUtilisateur,
            int? idAnneeScolaire = null,
            DateTime? date = null,
            string scope = DashboardCaissierScopes.Moi,
            bool allowEcoleScope = false);

        Task<DashboardFinancierClotureDto> GetClotureFinancierAsync(
            int idEcole,
            int idUtilisateur,
            int? idAnneeScolaire = null,
            DateTime? date = null,
            string scope = DashboardCaissierScopes.Moi,
            bool allowEcoleScope = false);
    }
}
