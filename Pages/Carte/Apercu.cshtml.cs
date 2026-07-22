using FastReport.Web;
using KelasiNaBiso.Services.Reporting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KelasiNaBiso.Pages.Carte
{
    [Authorize(Roles = "Admin,Directeur,Super-Admin,IT-Support")]
    public class ApercuModel : PageModel
    {
        private readonly ICarteReportService _carteReportService;

        public ApercuModel(ICarteReportService carteReportService)
        {
            _carteReportService = carteReportService;
        }

        public WebReport? WebReport { get; private set; }
        public string TypeLabel { get; private set; } = string.Empty;
        public string PdfUrl { get; private set; } = string.Empty;
        public string? ErrorMessage { get; private set; }

        public async Task<IActionResult> OnGetAsync(string type, int id, CancellationToken cancellationToken)
        {
            if (id <= 0 || string.IsNullOrWhiteSpace(type))
            {
                ErrorMessage = "Parametres invalides (type, id).";
                return Page();
            }

            try
            {
                var normalized = type.Trim().ToLowerInvariant();
                if (normalized == "eleve")
                {
                    TypeLabel = "eleve";
                    PdfUrl = $"/api/Carte/eleve/{id}/pdf";
                    WebReport = await _carteReportService.BuildEleveReportForUserAsync(
                        id, GetUserEcoleId(), GetUserRole(), cancellationToken);
                }
                else if (normalized == "agent")
                {
                    TypeLabel = "personnel";
                    PdfUrl = $"/api/Carte/agent/{id}/pdf";
                    WebReport = await _carteReportService.BuildAgentReportForUserAsync(
                        id, GetUserEcoleId(), GetUserRole(), cancellationToken);
                }
                else
                {
                    ErrorMessage = "Type inconnu. Utilisez eleve ou agent.";
                }
            }
            catch (KeyNotFoundException ex)
            {
                ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur : {ex.Message}";
            }

            return Page();
        }

        private int? GetUserEcoleId()
        {
            var claim = User.FindFirst("IdEcole") ?? User.FindFirst("idEcole");
            return claim != null && int.TryParse(claim.Value, out var id) ? id : null;
        }

        private string? GetUserRole() =>
            User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
            ?? User.FindFirst("Role")?.Value;
    }
}
