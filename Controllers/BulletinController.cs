using KelasiNaBiso.Attributes;
using KelasiNaBiso.Helpers;
using KelasiNaBiso.Models.DTOs.Bulletin;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Reporting;
using KelasiNaBiso.Services.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KelasiNaBiso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BulletinController : ControllerBase
    {
        private readonly IBulletinService _bulletinService;
        private readonly EleveAnneeScopeHelper _scope;
        private readonly IInscriptionActiveResolver _inscriptionResolver;
        private readonly IBulletinReportService _reportService;

        public BulletinController(
            IBulletinService bulletinService,
            EleveAnneeScopeHelper scope,
            IInscriptionActiveResolver inscriptionResolver,
            IBulletinReportService reportService)
        {
            _bulletinService = bulletinService;
            _scope = scope;
            _inscriptionResolver = inscriptionResolver;
            _reportService = reportService;
        }

        /// <summary>
        /// Bulletin JSON d'un élève pour une période (moyennes + rang).
        /// </summary>
        [HttpGet("eleve/{idEleve}")]
        [Permission("Note.Read")]
        [ProducesResponseType(typeof(BulletinEleveDto), 200)]
        public async Task<IActionResult> GetBulletinEleve(
            int idEleve,
            [FromQuery] string periode,
            [FromQuery] int? idAnneeScolaire = null)
        {
            if (string.IsNullOrWhiteSpace(periode))
                return BadRequest(new { message = "Le paramètre periode est requis." });

            try
            {
                var idEcole = await _inscriptionResolver.GetEcoleCouranteAsync(idEleve, idAnneeScolaire);
                if (!idEcole.HasValue)
                {
                    // Essayer sans filtre année pour résoudre l'école (tenant)
                    idEcole = await _inscriptionResolver.GetEcoleCouranteAsync(idEleve, null);
                }

                if (!idEcole.HasValue)
                    return NotFound(new { message = $"Élève {idEleve} introuvable ou sans inscription confirmée." });

                var deny = this.ForbidIfWrongSchool(idEcole);
                if (deny != null)
                    return deny;

                var annee = await _scope.ResolveIdAnneeScolaireAsync(idEcole.Value, idAnneeScolaire);
                var bulletin = await _bulletinService.GetBulletinEleveAsync(idEleve, annee, periode);
                if (bulletin == null)
                    return NotFound(new { message = $"Bulletin introuvable pour l'élève {idEleve}." });

                return Ok(bulletin);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Bulletins JSON de tous les élèves d'une classe pour une période.
        /// </summary>
        [HttpGet("classe/{idClasse}")]
        [Permission("Note.Read")]
        [ProducesResponseType(typeof(IReadOnlyList<BulletinEleveDto>), 200)]
        public async Task<IActionResult> GetBulletinsClasse(
            int idClasse,
            [FromQuery] string periode,
            [FromQuery] int? idAnneeScolaire = null)
        {
            if (string.IsNullOrWhiteSpace(periode))
                return BadRequest(new { message = "Le paramètre periode est requis." });

            try
            {
                var idEcole = await _scope.ResolveIdEcoleForClasseAsync(idClasse);
                var deny = this.ForbidIfWrongSchool(idEcole);
                if (deny != null)
                    return deny;

                var annee = await _scope.ResolveIdAnneeScolaireAsync(idEcole, idAnneeScolaire);
                var bulletins = await _bulletinService.GetBulletinsClasseAsync(idClasse, annee, periode);
                return Ok(bulletins);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Bulletin PDF d'un élève pour une période.
        /// </summary>
        [HttpGet("eleve/{idEleve}/pdf")]
        [Permission("Note.Read")]
        [Produces("application/pdf")]
        public async Task<IActionResult> GetBulletinElevePdf(
            int idEleve,
            [FromQuery] string periode,
            [FromQuery] int? idAnneeScolaire = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(periode))
                return BadRequest(new { message = "Le paramètre periode est requis." });

            try
            {
                var idEcole = await _inscriptionResolver.GetEcoleCouranteAsync(idEleve, idAnneeScolaire);
                if (!idEcole.HasValue)
                    idEcole = await _inscriptionResolver.GetEcoleCouranteAsync(idEleve, null);

                if (!idEcole.HasValue)
                    return NotFound(new { message = $"Élève {idEleve} introuvable ou sans inscription confirmée." });

                var deny = this.ForbidIfWrongSchool(idEcole);
                if (deny != null)
                    return deny;

                var annee = await _scope.ResolveIdAnneeScolaireAsync(idEcole.Value, idAnneeScolaire);
                var pdf = await _reportService.GetElevePdfAsync(idEleve, annee, periode, cancellationToken);
                var safePeriode = periode.Trim().Replace(' ', '-');
                return File(pdf, "application/pdf", $"bulletin-{idEleve}-{safePeriode}.pdf");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
