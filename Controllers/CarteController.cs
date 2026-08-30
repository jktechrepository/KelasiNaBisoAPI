using KelasiNaBiso.Helpers;
using KelasiNaBiso.Models.DTOs.Reporting;
using KelasiNaBiso.Services.Reporting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KelasiNaBiso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Directeur,Super-Admin,IT-Support")]
    public class CarteController : ControllerBase
    {
        private readonly ICarteReportService _carteReportService;
        private readonly ILogger<CarteController> _logger;

        public CarteController(ICarteReportService carteReportService, ILogger<CarteController> logger)
        {
            _carteReportService = carteReportService;
            _logger = logger;
        }

        /// <summary>Aperçu HTML. Pas de [Produces] restrictif : le front envoie souvent Accept: application/json (sinon HTTP 406).</summary>
        [HttpGet("eleve/{idEleve:int}/apercu")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public Task<IActionResult> ApercuEleve(int idEleve, CancellationToken cancellationToken) =>
            RenderHtmlAsync(idEleve, "eleve", cancellationToken);

        [HttpGet("eleve/{idEleve:int}/pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public Task<IActionResult> PdfEleve(int idEleve, CancellationToken cancellationToken) =>
            RenderPdfAsync(idEleve, "eleve", $"carte-eleve-{idEleve}.pdf", cancellationToken);

        [HttpPost("eleves/pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> PdfElevesBatch([FromBody] CarteBatchRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                await _carteReportService.EnsureEcoleAccessAsync(
                    request.IdEcole, this.GetCurrentUserSchoolId(), this.GetSchoolAccessRole(), cancellationToken);

                var webReport = await _carteReportService.BuildElevesBatchReportAsync(
                    request.IdEcole, request.Ids, cancellationToken);
                var pdf = await _carteReportService.ExportToPdfAsync(webReport.Report, cancellationToken);
                return File(pdf, "application/pdf", $"cartes-eleves-ecole-{request.IdEcole}.pdf");
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or KeyNotFoundException)
            {
                return MapException(ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur PDF lot eleves ecole {IdEcole}", request.IdEcole);
                return StatusCode(500, new { message = "Erreur generation PDF lot.", error = ex.Message });
            }
        }

        [HttpGet("eleves/ecole/{idEcole:int}/pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> PdfElevesEcole(int idEcole, [FromQuery] int? idClasse,
            CancellationToken cancellationToken)
        {
            try
            {
                await _carteReportService.EnsureEcoleAccessAsync(
                    idEcole, this.GetCurrentUserSchoolId(), this.GetSchoolAccessRole(), cancellationToken);

                var webReport = await _carteReportService.BuildElevesEcoleReportAsync(
                    idEcole, idClasse, cancellationToken);
                var pdf = await _carteReportService.ExportToPdfAsync(webReport.Report, cancellationToken);
                var suffix = idClasse.HasValue ? $"-classe-{idClasse}" : string.Empty;
                return File(pdf, "application/pdf", $"cartes-eleves-ecole-{idEcole}{suffix}.pdf");
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or KeyNotFoundException)
            {
                return MapException(ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur PDF eleves ecole {IdEcole}", idEcole);
                return StatusCode(500, new { message = "Erreur generation PDF ecole.", error = ex.Message });
            }
        }

        /// <summary>Aperçu HTML agent — mêmes règles Accept que l’élève (pas de 406 si Accept: application/json).</summary>
        [HttpGet("agent/{idAgent:int}/apercu")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public Task<IActionResult> ApercuAgent(int idAgent, CancellationToken cancellationToken) =>
            RenderHtmlAsync(idAgent, "agent", cancellationToken);

        [HttpGet("agent/{idAgent:int}/pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public Task<IActionResult> PdfAgent(int idAgent, CancellationToken cancellationToken) =>
            RenderPdfAsync(idAgent, "agent", $"carte-agent-{idAgent}.pdf", cancellationToken);

        [HttpPost("agents/pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> PdfAgentsBatch([FromBody] CarteBatchRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                await _carteReportService.EnsureEcoleAccessAsync(
                    request.IdEcole, this.GetCurrentUserSchoolId(), this.GetSchoolAccessRole(), cancellationToken);

                var webReport = await _carteReportService.BuildAgentsBatchReportAsync(
                    request.IdEcole, request.Ids, cancellationToken);
                var pdf = await _carteReportService.ExportToPdfAsync(webReport.Report, cancellationToken);
                return File(pdf, "application/pdf", $"cartes-agents-ecole-{request.IdEcole}.pdf");
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or KeyNotFoundException)
            {
                return MapException(ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur PDF lot agents ecole {IdEcole}", request.IdEcole);
                return StatusCode(500, new { message = "Erreur generation PDF lot.", error = ex.Message });
            }
        }

        [HttpGet("eleve/{idEleve:int}/viewer")]
        public IActionResult ViewerEleve(int idEleve) =>
            Redirect($"/Carte/Apercu?type=eleve&id={idEleve}");

        [HttpGet("agent/{idAgent:int}/viewer")]
        public IActionResult ViewerAgent(int idAgent) =>
            Redirect($"/Carte/Apercu?type=agent&id={idAgent}");

        private async Task<IActionResult> RenderHtmlAsync(int id, string type, CancellationToken cancellationToken)
        {
            try
            {
                var webReport = await BuildReportAsync(id, type, cancellationToken);
                var html = await _carteReportService.ExportToHtmlAsync(webReport.Report, cancellationToken);
                return Content(html, "text/html; charset=utf-8");
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or KeyNotFoundException)
            {
                return MapException(ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur apercu carte {Type} {Id}", type, id);
                return StatusCode(500, new { message = "Erreur generation apercu carte.", error = ex.Message });
            }
        }

        private async Task<IActionResult> RenderPdfAsync(int id, string type, string fileName,
            CancellationToken cancellationToken)
        {
            try
            {
                var webReport = await BuildReportAsync(id, type, cancellationToken);
                var pdf = await _carteReportService.ExportToPdfAsync(webReport.Report, cancellationToken);
                return File(pdf, "application/pdf", fileName);
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or KeyNotFoundException)
            {
                return MapException(ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur PDF carte {Type} {Id}", type, id);
                return StatusCode(500, new { message = "Erreur generation PDF carte.", error = ex.Message });
            }
        }

        private Task<FastReport.Web.WebReport> BuildReportAsync(int id, string type, CancellationToken cancellationToken)
        {
            var userEcole = this.GetCurrentUserSchoolId();
            var role = this.GetSchoolAccessRole();
            return type == "agent"
                ? _carteReportService.BuildAgentReportForUserAsync(id, userEcole, role, cancellationToken)
                : _carteReportService.BuildEleveReportForUserAsync(id, userEcole, role, cancellationToken);
        }

        private IActionResult MapException(Exception ex) =>
            ex switch
            {
                UnauthorizedAccessException => Forbid(ex.Message),
                KeyNotFoundException => NotFound(new { message = ex.Message }),
                _ => StatusCode(500, new { message = ex.Message })
            };
    }
}
