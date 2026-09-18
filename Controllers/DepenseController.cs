using KelasiNaBiso.Attributes;
using KelasiNaBiso.Helpers;
using KelasiNaBiso.Models.DTOs.Depense;
using KelasiNaBiso.Models.DTOs.Pagination;
using KelasiNaBiso.Services.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KelasiNaBiso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DepenseController : ControllerBase
    {
        private readonly IDepenseService _service;

        public DepenseController(IDepenseService service)
        {
            _service = service;
        }

        [HttpGet]
        [Permission("Depense.ReadAll")]
        [ProducesResponseType(typeof(PagedResult<DepenseDto>), 200)]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int? idEcole = null,
            [FromQuery] DateTime? dateDebut = null,
            [FromQuery] DateTime? dateFin = null,
            [FromQuery] int? idCategorieDepense = null,
            [FromQuery] string? statut = null,
            [FromQuery] PagedRequest? request = null,
            CancellationToken cancellationToken = default)
        {
            request ??= new PagedRequest();

            var resolvedEcole = ResolveIdEcole(idEcole);
            if (!resolvedEcole.HasValue)
                return BadRequest(new { message = "idEcole est requis (query ou JWT)." });

            var deny = this.ForbidIfWrongSchool(resolvedEcole);
            if (deny != null)
                return deny;

            try
            {
                var result = await _service.GetPagedAsync(
                    resolvedEcole, dateDebut, dateFin, idCategorieDepense, statut, request, cancellationToken);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("mois")]
        [Permission("Depense.ReadAll")]
        [ProducesResponseType(typeof(DepenseMoisDto), 200)]
        public async Task<IActionResult> GetMois(
            [FromQuery] int? mois = null,
            [FromQuery] int? annee = null,
            [FromQuery] int? idEcole = null,
            [FromQuery] string? statut = null,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var m = mois ?? now.Month;
            var a = annee ?? now.Year;

            var resolvedEcole = ResolveIdEcole(idEcole);
            if (!resolvedEcole.HasValue)
                return BadRequest(new { message = "idEcole est requis (query ou JWT)." });

            var deny = this.ForbidIfWrongSchool(resolvedEcole);
            if (deny != null)
                return deny;

            try
            {
                var result = await _service.GetMoisAsync(resolvedEcole.Value, m, a, statut, cancellationToken);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id:int}")]
        [Permission("Depense.Read")]
        [ProducesResponseType(typeof(DepenseDto), 200)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
        {
            var depense = await _service.GetByIdAsync(id, cancellationToken);
            if (depense == null)
                return NotFound(new { message = $"Dépense {id} introuvable." });

            var deny = this.ForbidIfWrongSchool(depense.IdEcole);
            if (deny != null)
                return deny;

            return Ok(depense);
        }

        [HttpPost]
        [Permission("Depense.Create")]
        [ProducesResponseType(typeof(DepenseDto), 201)]
        public async Task<IActionResult> Create(
            [FromBody] CreateDepenseDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto == null)
                return BadRequest(new { message = "Corps de requête requis." });

            var deny = this.ForbidIfWrongSchool(dto.IdEcole);
            if (deny != null)
                return deny;

            try
            {
                var userId = this.GetCurrentUserId();
                var created = await _service.CreateAsync(
                    dto,
                    userId > 0 ? userId : null,
                    cancellationToken);
                return CreatedAtAction(nameof(GetById), new { id = created.IdDepense }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        [Permission("Depense.Update")]
        [ProducesResponseType(typeof(DepenseDto), 200)]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateDepenseDto dto,
            CancellationToken cancellationToken = default)
        {
            var existing = await _service.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                return NotFound(new { message = $"Dépense {id} introuvable." });

            var deny = this.ForbidIfWrongSchool(existing.IdEcole);
            if (deny != null)
                return deny;

            try
            {
                var updated = await _service.UpdateAsync(id, dto ?? new UpdateDepenseDto(), cancellationToken);
                return Ok(updated);
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

        [HttpPost("{id:int}/annuler")]
        [Permission("Depense.Update")]
        [ProducesResponseType(typeof(DepenseDto), 200)]
        public async Task<IActionResult> Annuler(
            int id,
            [FromBody] AnnulerDepenseDto? dto,
            CancellationToken cancellationToken = default)
        {
            var existing = await _service.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                return NotFound(new { message = $"Dépense {id} introuvable." });

            var deny = this.ForbidIfWrongSchool(existing.IdEcole);
            if (deny != null)
                return deny;

            try
            {
                var userId = this.GetCurrentUserId();
                var result = await _service.AnnulerAsync(
                    id,
                    dto?.MotifAnnulation,
                    userId > 0 ? userId : null,
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

        [HttpDelete("{id:int}")]
        [Permission("Depense.Delete")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _service.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                return NotFound(new { message = $"Dépense {id} introuvable." });

            var deny = this.ForbidIfWrongSchool(existing.IdEcole);
            if (deny != null)
                return deny;

            try
            {
                await _service.SoftDeleteAsync(id, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        private int? ResolveIdEcole(int? idEcoleQuery)
        {
            if (idEcoleQuery.HasValue && idEcoleQuery.Value > 0)
                return idEcoleQuery.Value;
            return this.GetCurrentUserSchoolId();
        }
    }
}
