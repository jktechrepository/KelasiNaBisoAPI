using KelasiNaBiso.Attributes;
using KelasiNaBiso.Helpers;
using KelasiNaBiso.Models.DTOs.Depense;
using KelasiNaBiso.Services.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KelasiNaBiso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategorieDepenseController : ControllerBase
    {
        private readonly ICategorieDepenseService _service;

        public CategorieDepenseController(ICategorieDepenseService service)
        {
            _service = service;
        }

        [HttpGet("ecole/{idEcole}")]
        [Permission("CategorieDepense.ReadAll")]
        [ProducesResponseType(typeof(IReadOnlyList<CategorieDepenseDto>), 200)]
        public async Task<IActionResult> GetByEcole(
            int idEcole,
            [FromQuery] bool includeInactive = false,
            CancellationToken cancellationToken = default)
        {
            var deny = this.ForbidIfWrongSchool(idEcole);
            if (deny != null)
                return deny;

            var list = await _service.GetByEcoleAsync(idEcole, includeInactive, cancellationToken);
            return Ok(list);
        }

        [HttpGet("{id:int}")]
        [Permission("CategorieDepense.Read")]
        [ProducesResponseType(typeof(CategorieDepenseDto), 200)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
        {
            var cat = await _service.GetByIdAsync(id, cancellationToken);
            if (cat == null)
                return NotFound(new { message = $"Catégorie {id} introuvable." });

            var deny = this.ForbidIfWrongSchool(cat.IdEcole);
            if (deny != null)
                return deny;

            return Ok(cat);
        }

        [HttpPost]
        [Permission("CategorieDepense.Create")]
        [ProducesResponseType(typeof(CategorieDepenseDto), 201)]
        public async Task<IActionResult> Create(
            [FromBody] CreateCategorieDepenseDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto == null)
                return BadRequest(new { message = "Corps de requête requis." });

            var deny = this.ForbidIfWrongSchool(dto.IdEcole);
            if (deny != null)
                return deny;

            try
            {
                var created = await _service.CreateAsync(dto, cancellationToken);
                return CreatedAtAction(nameof(GetById), new { id = created.IdCategorieDepense }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        [Permission("CategorieDepense.Update")]
        [ProducesResponseType(typeof(CategorieDepenseDto), 200)]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateCategorieDepenseDto dto,
            CancellationToken cancellationToken = default)
        {
            var existing = await _service.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                return NotFound(new { message = $"Catégorie {id} introuvable." });

            var deny = this.ForbidIfWrongSchool(existing.IdEcole);
            if (deny != null)
                return deny;

            try
            {
                var updated = await _service.UpdateAsync(id, dto ?? new UpdateCategorieDepenseDto(), cancellationToken);
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

        [HttpDelete("{id:int}")]
        [Permission("CategorieDepense.Delete")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _service.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                return NotFound(new { message = $"Catégorie {id} introuvable." });

            var deny = this.ForbidIfWrongSchool(existing.IdEcole);
            if (deny != null)
                return deny;

            try
            {
                await _service.DeleteAsync(id, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
