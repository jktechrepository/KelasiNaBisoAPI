using KelasiNaBiso.Helpers;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Services.Tarif;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KelasiNaBiso.Controllers
{
    /// <summary>
    /// Catégories tarifaires, affectations élève et règles d'exonération de frais.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EleveTarifController : ControllerBase
    {
        private readonly IEleveTarifService _tarifService;
        private readonly IFraisDuCalculator _fraisDuCalculator;
        private readonly IInscriptionActiveResolver _inscriptionResolver;

        public EleveTarifController(
            IEleveTarifService tarifService,
            IFraisDuCalculator fraisDuCalculator,
            IInscriptionActiveResolver inscriptionResolver)
        {
            _tarifService = tarifService;
            _fraisDuCalculator = fraisDuCalculator;
            _inscriptionResolver = inscriptionResolver;
        }

        // ── Catégories ──────────────────────────────────────────────────────

        [HttpGet("categories")]
        [Authorize(Roles = UserRoles.EleveTarifManageRoles)]
        [ProducesResponseType(typeof(IReadOnlyList<CategorieEleveTarifDto>), 200)]
        public async Task<IActionResult> GetCategories(
            [FromQuery] int? idEcole = null,
            [FromQuery] bool includeInactive = false,
            CancellationToken cancellationToken = default)
        {
            if (!TryResolveIdEcole(idEcole, out var resolved, out var error))
                return error!;

            var deny = this.ForbidIfWrongSchool(resolved);
            if (deny != null) return deny;

            var list = await _tarifService.GetCategoriesAsync(resolved, includeInactive, cancellationToken);
            return Ok(list);
        }

        [HttpGet("categories/{id:int}")]
        [Authorize(Roles = UserRoles.EleveTarifManageRoles)]
        [ProducesResponseType(typeof(CategorieEleveTarifDto), 200)]
        public async Task<IActionResult> GetCategorie(int id, CancellationToken cancellationToken = default)
        {
            var cat = await _tarifService.GetCategorieByIdAsync(id, cancellationToken);
            if (cat == null)
                return NotFound(new { message = $"Catégorie {id} introuvable." });

            var deny = this.ForbidIfWrongSchool(cat.IdEcole);
            if (deny != null) return deny;

            return Ok(cat);
        }

        [HttpPost("categories")]
        [Authorize(Roles = UserRoles.EleveTarifManageRoles)]
        [ProducesResponseType(typeof(CategorieEleveTarifDto), 201)]
        public async Task<IActionResult> CreateCategorie(
            [FromQuery] int? idEcole,
            [FromBody] CreateCategorieEleveTarifDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto == null)
                return BadRequest(new { message = "Corps de requête requis." });

            if (!TryResolveIdEcole(idEcole, out var resolved, out var error))
                return error!;

            var deny = this.ForbidIfWrongSchool(resolved);
            if (deny != null) return deny;

            try
            {
                var created = await _tarifService.CreateCategorieAsync(resolved, dto, cancellationToken);
                return CreatedAtAction(nameof(GetCategorie), new { id = created.IdCategorieEleveTarif }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("categories/{id:int}")]
        [Authorize(Roles = UserRoles.EleveTarifManageRoles)]
        [ProducesResponseType(typeof(CategorieEleveTarifDto), 200)]
        public async Task<IActionResult> UpdateCategorie(
            int id,
            [FromBody] UpdateCategorieEleveTarifDto dto,
            CancellationToken cancellationToken = default)
        {
            var existing = await _tarifService.GetCategorieByIdAsync(id, cancellationToken);
            if (existing == null)
                return NotFound(new { message = $"Catégorie {id} introuvable." });

            var deny = this.ForbidIfWrongSchool(existing.IdEcole);
            if (deny != null) return deny;

            try
            {
                var updated = await _tarifService.UpdateCategorieAsync(
                    id, dto ?? new UpdateCategorieEleveTarifDto(), cancellationToken);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("categories/{id:int}")]
        [Authorize(Roles = UserRoles.EleveTarifManageRoles)]
        [ProducesResponseType(204)]
        public async Task<IActionResult> DeleteCategorie(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _tarifService.GetCategorieByIdAsync(id, cancellationToken);
            if (existing == null)
                return NotFound(new { message = $"Catégorie {id} introuvable." });

            var deny = this.ForbidIfWrongSchool(existing.IdEcole);
            if (deny != null) return deny;

            try
            {
                await _tarifService.SoftDeleteCategorieAsync(id, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ── Affectations ────────────────────────────────────────────────────

        [HttpGet("eleves/{idEleve:int}/affectations")]
        [Authorize(Roles = UserRoles.EleveTarifManageRoles)]
        [ProducesResponseType(typeof(IReadOnlyList<AffectationEleveCategorieTarifDto>), 200)]
        public async Task<IActionResult> GetAffectations(
            int idEleve,
            [FromQuery] int? idAnneeScolaire = null,
            CancellationToken cancellationToken = default)
        {
            var deny = await ForbidForEleveAsync(idEleve, cancellationToken);
            if (deny != null) return deny;

            var list = await _tarifService.GetAffectationsEleveAsync(idEleve, idAnneeScolaire, cancellationToken);
            return Ok(list);
        }

        [HttpGet("eleves/{idEleve:int}/affectation-active")]
        [Authorize(Roles = UserRoles.EleveTarifReadRoles)]
        [ProducesResponseType(typeof(AffectationEleveCategorieTarifDto), 200)]
        public async Task<IActionResult> GetAffectationActive(
            int idEleve,
            [FromQuery] int idAnneeScolaire,
            CancellationToken cancellationToken = default)
        {
            if (idAnneeScolaire <= 0)
                return BadRequest(new { message = "idAnneeScolaire est obligatoire." });

            var deny = await ForbidForEleveAsync(idEleve, cancellationToken);
            if (deny != null) return deny;

            var aff = await _tarifService.GetAffectationActiveAsync(idEleve, idAnneeScolaire, cancellationToken: cancellationToken);
            if (aff == null)
                return NotFound(new { message = "Aucune affectation active pour cet élève / année." });

            return Ok(aff);
        }

        [HttpPost("affectations")]
        [Authorize(Roles = UserRoles.EleveTarifManageRoles)]
        [ProducesResponseType(typeof(AffectationEleveCategorieTarifDto), 201)]
        public async Task<IActionResult> Affecter(
            [FromBody] CreateAffectationEleveCategorieTarifDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto == null)
                return BadRequest(new { message = "Corps de requête requis." });

            var deny = await ForbidForEleveAsync(dto.IdEleve, cancellationToken);
            if (deny != null) return deny;

            var cat = await _tarifService.GetCategorieByIdAsync(dto.IdCategorieEleveTarif, cancellationToken);
            if (cat == null)
                return NotFound(new { message = "Catégorie introuvable." });

            var denyCat = this.ForbidIfWrongSchool(cat.IdEcole);
            if (denyCat != null) return denyCat;

            try
            {
                var created = await _tarifService.AffecterAsync(
                    dto, this.GetCurrentUserId() > 0 ? this.GetCurrentUserId() : null, cancellationToken);
                return CreatedAtAction(
                    nameof(GetAffectations),
                    new { idEleve = created.IdEleve },
                    created);
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

        [HttpPost("affectations/{id:int}/cloturer")]
        [Authorize(Roles = UserRoles.EleveTarifManageRoles)]
        [ProducesResponseType(typeof(AffectationEleveCategorieTarifDto), 200)]
        public async Task<IActionResult> CloturerAffectation(
            int id,
            [FromQuery] DateTime? dateFin = null,
            CancellationToken cancellationToken = default)
        {
            var existing = await _tarifService.GetAffectationByIdAsync(id, cancellationToken);
            if (existing == null)
                return NotFound(new { message = $"Affectation {id} introuvable." });

            var cat = await _tarifService.GetCategorieByIdAsync(existing.IdCategorieEleveTarif, cancellationToken);
            if (cat != null)
            {
                var deny = this.ForbidIfWrongSchool(cat.IdEcole);
                if (deny != null) return deny;
            }

            try
            {
                var closed = await _tarifService.CloturerAffectationAsync(id, dateFin, cancellationToken);
                return Ok(closed);
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

        // ── Règles ──────────────────────────────────────────────────────────

        [HttpGet("regles")]
        [Authorize(Roles = UserRoles.EleveTarifReadRoles)]
        [ProducesResponseType(typeof(IReadOnlyList<RegleExonerationFraisDto>), 200)]
        public async Task<IActionResult> GetRegles(
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null,
            [FromQuery] int? idCategorie = null,
            [FromQuery] bool includeInactive = false,
            CancellationToken cancellationToken = default)
        {
            if (!TryResolveIdEcole(idEcole, out var resolved, out var error))
                return error!;

            var deny = this.ForbidIfWrongSchool(resolved);
            if (deny != null) return deny;

            var list = await _tarifService.GetReglesAsync(
                resolved, idAnneeScolaire, idCategorie, includeInactive, cancellationToken);
            return Ok(list);
        }

        [HttpGet("regles/{id:int}")]
        [Authorize(Roles = UserRoles.EleveTarifReadRoles)]
        [ProducesResponseType(typeof(RegleExonerationFraisDto), 200)]
        public async Task<IActionResult> GetRegle(int id, CancellationToken cancellationToken = default)
        {
            var regle = await _tarifService.GetRegleByIdAsync(id, cancellationToken);
            if (regle == null)
                return NotFound(new { message = $"Règle {id} introuvable." });

            var deny = this.ForbidIfWrongSchool(regle.IdEcole);
            if (deny != null) return deny;

            return Ok(regle);
        }

        [HttpPut("regles")]
        [Authorize(Roles = UserRoles.EleveTarifReglesWriteRoles)]
        [ProducesResponseType(typeof(RegleExonerationFraisDto), 200)]
        public async Task<IActionResult> UpsertRegle(
            [FromQuery] int? idEcole,
            [FromBody] UpsertRegleExonerationFraisDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto == null)
                return BadRequest(new { message = "Corps de requête requis." });

            if (!TryResolveIdEcole(idEcole, out var resolved, out var error))
                return error!;

            var deny = this.ForbidIfWrongSchool(resolved);
            if (deny != null) return deny;

            try
            {
                var result = await _tarifService.UpsertRegleAsync(
                    resolved,
                    dto,
                    this.GetCurrentUserId() > 0 ? this.GetCurrentUserId() : null,
                    cancellationToken);
                return Ok(result);
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

        [HttpDelete("regles/{id:int}")]
        [Authorize(Roles = UserRoles.EleveTarifReglesWriteRoles)]
        [ProducesResponseType(204)]
        public async Task<IActionResult> DeleteRegle(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _tarifService.GetRegleByIdAsync(id, cancellationToken);
            if (existing == null)
                return NotFound(new { message = $"Règle {id} introuvable." });

            var deny = this.ForbidIfWrongSchool(existing.IdEcole);
            if (deny != null) return deny;

            try
            {
                await _tarifService.SoftDeleteRegleAsync(id, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ── Lecture dus enrichis ─────────────────────────────────────────────

        [HttpGet("eleves/{idEleve:int}/frais-dus")]
        [Authorize(Roles = UserRoles.EleveTarifReadRoles)]
        [ProducesResponseType(typeof(IReadOnlyList<FraisDuDetailDto>), 200)]
        public async Task<IActionResult> GetFraisDus(
            int idEleve,
            [FromQuery] int? idAnneeScolaire = null,
            CancellationToken cancellationToken = default)
        {
            var deny = await ForbidForEleveAsync(idEleve, cancellationToken);
            if (deny != null) return deny;

            var details = await _fraisDuCalculator.GetDetailsForEleveAsync(
                idEleve, idAnneeScolaire, cancellationToken: cancellationToken);
            return Ok(details);
        }

        [HttpGet("eleves/{idEleve:int}/frais-dus/{idFrais:int}")]
        [Authorize(Roles = UserRoles.EleveTarifReadRoles)]
        [ProducesResponseType(typeof(FraisDuDetailDto), 200)]
        public async Task<IActionResult> GetFraisDuDetail(
            int idEleve,
            int idFrais,
            CancellationToken cancellationToken = default)
        {
            var deny = await ForbidForEleveAsync(idEleve, cancellationToken);
            if (deny != null) return deny;

            var detail = await _fraisDuCalculator.GetDetailAsync(idEleve, idFrais, cancellationToken: cancellationToken);
            if (detail == null)
                return NotFound(new { message = "Frais introuvable." });

            return Ok(detail);
        }

        private async Task<IActionResult?> ForbidForEleveAsync(int idEleve, CancellationToken cancellationToken)
        {
            var idEcole = await _inscriptionResolver.GetEcoleCouranteAsync(idEleve);
            return this.ForbidIfWrongSchool(idEcole);
        }

        private bool TryResolveIdEcole(int? idEcoleQuery, out int idEcole, out IActionResult? error)
        {
            idEcole = 0;
            error = null;

            if (idEcoleQuery.HasValue && idEcoleQuery.Value > 0)
            {
                idEcole = idEcoleQuery.Value;
                return true;
            }

            var fromJwt = this.GetCurrentUserSchoolId();
            if (fromJwt.HasValue && fromJwt.Value > 0)
            {
                idEcole = fromJwt.Value;
                return true;
            }

            error = BadRequest(new
            {
                message = "Le paramètre idEcole est obligatoire (Super-Admin / IT-Support)."
            });
            return false;
        }
    }
}
