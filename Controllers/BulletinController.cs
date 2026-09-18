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
        private readonly PeriodeCotationResolver _periodeResolver;

        public BulletinController(
            IBulletinService bulletinService,
            EleveAnneeScopeHelper scope,
            IInscriptionActiveResolver inscriptionResolver,
            IBulletinReportService reportService,
            PeriodeCotationResolver periodeResolver)
        {
            _bulletinService = bulletinService;
            _scope = scope;
            _inscriptionResolver = inscriptionResolver;
            _reportService = reportService;
            _periodeResolver = periodeResolver;
        }

        /// <summary>
        /// Bulletin JSON d'un élève pour une période (moyennes + rang).
        /// Fournir <c>idPeriode</c> et/ou <c>periode</c> (code T1 / libellé / alias).
        /// </summary>
        [HttpGet("eleve/{idEleve}")]
        [Permission("Note.Read", "Note.ReadOwn", "Note.ReadChildren", "Bulletin.Read", "Bulletin.ReadOwn", "Bulletin.ReadChildren")]
        [ProducesResponseType(typeof(BulletinEleveDto), 200)]
        public async Task<IActionResult> GetBulletinEleve(
            int idEleve,
            [FromQuery] string? periode = null,
            [FromQuery] int? idPeriode = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            if (!idPeriode.HasValue && string.IsNullOrWhiteSpace(periode))
                return BadRequest(new { message = "Le paramètre idPeriode ou periode est requis." });

            var denyOwn = this.ForbidIfWrongEleve(idEleve);
            if (denyOwn != null)
                return denyOwn;

            var denyChild = await this.ForbidIfWrongChildAsync(idEleve);
            if (denyChild != null)
                return denyChild;

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

                var deny = await this.ForbidIfWrongSchoolAsync(idEcole, _inscriptionResolver);
                if (deny != null)
                    return deny;

                var annee = await _scope.ResolveIdAnneeScolaireAsync(idEcole.Value, idAnneeScolaire);
                var bulletin = await _bulletinService.GetBulletinEleveAsync(idEleve, annee, periode, idPeriode);
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
        [Permission("Note.Read", "Bulletin.Read")]
        [ProducesResponseType(typeof(IReadOnlyList<BulletinEleveDto>), 200)]
        public async Task<IActionResult> GetBulletinsClasse(
            int idClasse,
            [FromQuery] string? periode = null,
            [FromQuery] int? idPeriode = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            if (!idPeriode.HasValue && string.IsNullOrWhiteSpace(periode))
                return BadRequest(new { message = "Le paramètre idPeriode ou periode est requis." });

            try
            {
                var idEcole = await _scope.ResolveIdEcoleForClasseAsync(idClasse);
                var deny = this.ForbidIfWrongSchool(idEcole);
                if (deny != null)
                    return deny;

                var annee = await _scope.ResolveIdAnneeScolaireAsync(idEcole, idAnneeScolaire);
                var bulletins = await _bulletinService.GetBulletinsClasseAsync(idClasse, annee, periode, idPeriode);
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
        [Permission("Note.Read", "Note.ReadOwn", "Note.ReadChildren", "Bulletin.Read", "Bulletin.ReadOwn", "Bulletin.ReadChildren")]
        [Produces("application/pdf")]
        public async Task<IActionResult> GetBulletinElevePdf(
            int idEleve,
            [FromQuery] string? periode = null,
            [FromQuery] int? idPeriode = null,
            [FromQuery] int? idAnneeScolaire = null,
            CancellationToken cancellationToken = default)
        {
            if (!idPeriode.HasValue && string.IsNullOrWhiteSpace(periode))
                return BadRequest(new { message = "Le paramètre idPeriode ou periode est requis." });

            var denyOwn = this.ForbidIfWrongEleve(idEleve);
            if (denyOwn != null)
                return denyOwn;

            var denyChild = await this.ForbidIfWrongChildAsync(idEleve, cancellationToken);
            if (denyChild != null)
                return denyChild;

            try
            {
                var idEcole = await _inscriptionResolver.GetEcoleCouranteAsync(idEleve, idAnneeScolaire);
                if (!idEcole.HasValue)
                    idEcole = await _inscriptionResolver.GetEcoleCouranteAsync(idEleve, null);

                if (!idEcole.HasValue)
                    return NotFound(new { message = $"Élève {idEleve} introuvable ou sans inscription confirmée." });

                var deny = await this.ForbidIfWrongSchoolAsync(idEcole, _inscriptionResolver, cancellationToken);
                if (deny != null)
                    return deny;

                var annee = await _scope.ResolveIdAnneeScolaireAsync(idEcole.Value, idAnneeScolaire);
                var pdf = await _reportService.GetElevePdfAsync(idEleve, annee, periode, idPeriode, cancellationToken);
                var safePeriode = (periode ?? idPeriode?.ToString() ?? "periode").Trim().Replace(' ', '-');
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

        /// <summary>
        /// Saisie manuelle décision / appréciation (titulaire ou direction). Texte libre.
        /// </summary>
        [HttpPut("eleve/{idEleve}/decision")]
        [Permission("Bulletin.Update", "Note.Update")]
        [ProducesResponseType(typeof(BulletinDecisionDto), 200)]
        public async Task<IActionResult> UpsertDecision(
            int idEleve,
            [FromBody] UpsertBulletinDecisionDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto == null)
                return BadRequest(new { message = "Corps de requête requis." });

            if (!dto.IdPeriode.HasValue && string.IsNullOrWhiteSpace(dto.Periode))
                return BadRequest(new { message = "Le paramètre idPeriode ou periode est requis." });

            try
            {
                var idEcole = await _inscriptionResolver.GetEcoleCouranteAsync(idEleve, dto.IdAnneeScolaire);
                if (!idEcole.HasValue)
                    idEcole = await _inscriptionResolver.GetEcoleCouranteAsync(idEleve, null);
                if (!idEcole.HasValue)
                    return NotFound(new { message = $"Élève {idEleve} introuvable ou sans inscription confirmée." });

                var denySchool = await this.ForbidIfWrongSchoolAsync(idEcole, _inscriptionResolver, cancellationToken);
                if (denySchool != null)
                    return denySchool;

                var annee = await _scope.ResolveIdAnneeScolaireAsync(idEcole.Value, dto.IdAnneeScolaire);
                var inscription = await _inscriptionResolver.GetInscriptionActiveAsync(idEleve, annee, cancellationToken);
                if (inscription == null)
                    return NotFound(new { message = $"Aucune inscription active pour l'élève {idEleve}." });

                var denyAcl = await this.ForbidIfNotTitulaireOuDirectionAsync(
                    inscription.IdClasse, annee, cancellationToken);
                if (denyAcl != null)
                    return denyAcl;

                var periode = await _periodeResolver.ResolveAsync(dto.IdPeriode, dto.Periode, cancellationToken);
                if (periode == null)
                    return BadRequest(new { message = "Période de cotation introuvable. Utilisez idPeriode (GET /api/PeriodeCotation)." });

                var result = await _bulletinService.UpsertDecisionAsync(
                    idEleve,
                    annee,
                    periode.IdPeriode,
                    dto.Decision,
                    dto.AppreciationGenerale,
                    this.GetCurrentUserId() > 0 ? this.GetCurrentUserId() : null,
                    cancellationToken);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Fige (valide) le bulletin d'un élève : snapshot immuable. Titulaire ou direction.
        /// </summary>
        [HttpPost("eleve/{idEleve}/figer")]
        [Permission("Bulletin.Update")]
        [ProducesResponseType(typeof(BulletinEleveDto), 200)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> FigerEleve(
            int idEleve,
            [FromBody] FigerBulletinRequestDto? dto,
            CancellationToken cancellationToken = default)
        {
            dto ??= new FigerBulletinRequestDto();
            if (!dto.IdPeriode.HasValue && string.IsNullOrWhiteSpace(dto.Periode))
                return BadRequest(new { message = "Le paramètre idPeriode ou periode est requis." });

            try
            {
                var idEcole = await _inscriptionResolver.GetEcoleCouranteAsync(idEleve, dto.IdAnneeScolaire);
                if (!idEcole.HasValue)
                    idEcole = await _inscriptionResolver.GetEcoleCouranteAsync(idEleve, null);
                if (!idEcole.HasValue)
                    return NotFound(new { message = $"Élève {idEleve} introuvable ou sans inscription confirmée." });

                var denySchool = await this.ForbidIfWrongSchoolAsync(idEcole, _inscriptionResolver, cancellationToken);
                if (denySchool != null)
                    return denySchool;

                var annee = await _scope.ResolveIdAnneeScolaireAsync(idEcole.Value, dto.IdAnneeScolaire);
                var inscription = await _inscriptionResolver.GetInscriptionActiveAsync(idEleve, annee, cancellationToken);
                if (inscription == null)
                    return NotFound(new { message = $"Aucune inscription active pour l'élève {idEleve}." });

                var denyAcl = await this.ForbidIfNotTitulaireOuDirectionAsync(
                    inscription.IdClasse, annee, cancellationToken);
                if (denyAcl != null)
                    return denyAcl;

                var result = await _bulletinService.FigerEleveAsync(
                    idEleve,
                    annee,
                    dto.Periode,
                    dto.IdPeriode,
                    this.GetCurrentUserId() > 0 ? this.GetCurrentUserId() : null,
                    cancellationToken);

                return Ok(result);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("déjà figé", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Fige tous les bulletins non encore figés d'une classe. Titulaire ou direction.
        /// </summary>
        [HttpPost("classe/{idClasse}/figer")]
        [Permission("Bulletin.Update")]
        [ProducesResponseType(typeof(FigerBulletinResultDto), 200)]
        public async Task<IActionResult> FigerClasse(
            int idClasse,
            [FromBody] FigerBulletinRequestDto? dto,
            CancellationToken cancellationToken = default)
        {
            dto ??= new FigerBulletinRequestDto();
            if (!dto.IdPeriode.HasValue && string.IsNullOrWhiteSpace(dto.Periode))
                return BadRequest(new { message = "Le paramètre idPeriode ou periode est requis." });

            try
            {
                var idEcole = await _scope.ResolveIdEcoleForClasseAsync(idClasse);
                var denySchool = this.ForbidIfWrongSchool(idEcole);
                if (denySchool != null)
                    return denySchool;

                var annee = await _scope.ResolveIdAnneeScolaireAsync(idEcole, dto.IdAnneeScolaire);
                var denyAcl = await this.ForbidIfNotTitulaireOuDirectionAsync(idClasse, annee, cancellationToken);
                if (denyAcl != null)
                    return denyAcl;

                var result = await _bulletinService.FigerClasseAsync(
                    idClasse,
                    annee,
                    dto.Periode,
                    dto.IdPeriode,
                    this.GetCurrentUserId() > 0 ? this.GetCurrentUserId() : null,
                    cancellationToken);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Déverrouille un bulletin figé (direction uniquement). Retour au calcul live.
        /// </summary>
        [HttpPost("eleve/{idEleve}/deverrouiller")]
        [Permission("Bulletin.Unlock")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> DeverrouillerEleve(
            int idEleve,
            [FromBody] FigerBulletinRequestDto? dto,
            CancellationToken cancellationToken = default)
        {
            dto ??= new FigerBulletinRequestDto();
            if (!dto.IdPeriode.HasValue && string.IsNullOrWhiteSpace(dto.Periode))
                return BadRequest(new { message = "Le paramètre idPeriode ou periode est requis." });

            try
            {
                var idEcole = await _inscriptionResolver.GetEcoleCouranteAsync(idEleve, dto.IdAnneeScolaire);
                if (!idEcole.HasValue)
                    idEcole = await _inscriptionResolver.GetEcoleCouranteAsync(idEleve, null);
                if (!idEcole.HasValue)
                    return NotFound(new { message = $"Élève {idEleve} introuvable ou sans inscription confirmée." });

                var denySchool = await this.ForbidIfWrongSchoolAsync(idEcole, _inscriptionResolver, cancellationToken);
                if (denySchool != null)
                    return denySchool;

                var annee = await _scope.ResolveIdAnneeScolaireAsync(idEcole.Value, dto.IdAnneeScolaire);
                await _bulletinService.DeverrouillerEleveAsync(
                    idEleve, annee, dto.Periode, dto.IdPeriode, cancellationToken);

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
