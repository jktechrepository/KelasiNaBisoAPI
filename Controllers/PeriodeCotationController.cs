using KelasiNaBiso.Attributes;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PeriodeCotationController : ControllerBase
    {
        private readonly KelasiNaBisoDbContext _context;

        public PeriodeCotationController(KelasiNaBisoDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Catalogue des périodes de cotation (T1 / T2 / T3).
        /// </summary>
        [HttpGet]
        [Permission("Evaluation.Read", "Evaluation.ReadAll", "Note.Read", "Note.ReadOwn", "Note.ReadChildren")]
        [ProducesResponseType(typeof(IReadOnlyList<PeriodeCotationDto>), 200)]
        public async Task<ActionResult<IReadOnlyList<PeriodeCotationDto>>> GetPeriodes(
            [FromQuery] bool? statut = true,
            CancellationToken cancellationToken = default)
        {
            var query = _context.PeriodesCotation.AsNoTracking().AsQueryable();
            if (statut.HasValue)
                query = query.Where(p => p.Statut == statut.Value);

            var items = await query
                .OrderBy(p => p.Ordre)
                .ThenBy(p => p.Code)
                .Select(p => new PeriodeCotationDto
                {
                    IdPeriode = p.IdPeriode,
                    Code = p.Code,
                    Libelle = p.Libelle,
                    Ordre = p.Ordre,
                    Statut = p.Statut
                })
                .ToListAsync(cancellationToken);

            return Ok(items);
        }

        [HttpGet("{id:int}")]
        [Permission("Evaluation.Read", "Evaluation.ReadAll", "Note.Read", "Note.ReadOwn", "Note.ReadChildren")]
        [ProducesResponseType(typeof(PeriodeCotationDto), 200)]
        public async Task<ActionResult<PeriodeCotationDto>> GetPeriode(
            int id,
            CancellationToken cancellationToken = default)
        {
            var item = await _context.PeriodesCotation.AsNoTracking()
                .Where(p => p.IdPeriode == id)
                .Select(p => new PeriodeCotationDto
                {
                    IdPeriode = p.IdPeriode,
                    Code = p.Code,
                    Libelle = p.Libelle,
                    Ordre = p.Ordre,
                    Statut = p.Statut
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (item == null)
                return NotFound(new { message = $"Période {id} introuvable." });

            return Ok(item);
        }
    }
}
