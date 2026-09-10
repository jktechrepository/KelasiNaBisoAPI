using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DeviseController : ControllerBase
    {
        private readonly ICurrencyConversionService _currencyConversionService;
        private readonly IEcoleRepository _ecoleRepository;
        private readonly KelasiNaBisoDbContext _context;

        public DeviseController(
            ICurrencyConversionService currencyConversionService,
            IEcoleRepository ecoleRepository,
            KelasiNaBisoDbContext context)
        {
            _currencyConversionService = currencyConversionService;
            _ecoleRepository = ecoleRepository;
            _context = context;
        }

        [HttpPost("taux-change")]
        public async Task<IActionResult> CreateTauxChange([FromBody] CreateTauxChangeDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ecole = await _ecoleRepository.GetByIdAsync(dto.IdEcole);
            if (ecole == null)
            {
                return NotFound(new { message = $"École {dto.IdEcole} introuvable." });
            }

            var source = dto.CodeDeviseSource?.Trim().ToUpperInvariant();
            var cible = dto.CodeDeviseCible?.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(cible))
            {
                return BadRequest(new { message = "Les codes devise source et cible sont obligatoires." });
            }

            if (source == cible)
            {
                return BadRequest(new { message = "La devise source et la devise cible doivent être différentes." });
            }

            if (dto.Taux <= 0)
            {
                return BadRequest(new { message = "Le taux doit être strictement positif." });
            }

            var existingTaux = await _context.TauxChanges
                .AsNoTracking()
                .AnyAsync(t => t.IdEcole == dto.IdEcole
                    && t.CodeDeviseSource == source
                    && t.CodeDeviseCible == cible
                    && t.DateEffet == dto.DateEffet
                    && t.Statut == dto.Statut);

            if (existingTaux)
            {
                return Conflict(new { message = "Un taux identique existe déjà pour cette école, cette paire et cette date." });
            }

            var taux = new TauxChange
            {
                IdEcole = dto.IdEcole,
                CodeDeviseSource = source,
                CodeDeviseCible = cible,
                Taux = dto.Taux,
                DateEffet = dto.DateEffet,
                Statut = dto.Statut,
                DateCreation = DateTime.UtcNow
            };

            _context.TauxChanges.Add(taux);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                idTauxChange = taux.IdTauxChange,
                idEcole = taux.IdEcole,
                codeDeviseSource = taux.CodeDeviseSource,
                codeDeviseCible = taux.CodeDeviseCible,
                taux = taux.Taux,
                dateEffet = taux.DateEffet,
                statut = taux.Statut
            });
        }

        [HttpGet("preview-conversion")]
        public async Task<IActionResult> PreviewConversion(
            [FromQuery] int? idEcole,
            [FromQuery] int? idSociete,
            [FromQuery] string? codeDeviseSource,
            [FromQuery] string? codeDeviseCible,
            [FromQuery] decimal? montant,
            [FromQuery] DateTime? dateReference,
            [FromQuery] DateTime? datePaiement)
        {
            var effectiveIdEcole = idEcole ?? idSociete;
            if (!effectiveIdEcole.HasValue)
            {
                return BadRequest(new { message = "Le paramètre idEcole est obligatoire." });
            }

            if (string.IsNullOrWhiteSpace(codeDeviseSource))
            {
                return BadRequest(new { message = "Le paramètre codeDeviseSource est obligatoire." });
            }

            if (!montant.HasValue)
            {
                return BadRequest(new { message = "Le paramètre montant est obligatoire." });
            }

            var ecole = await _ecoleRepository.GetByIdAsync(effectiveIdEcole.Value);
            if (ecole == null)
            {
                return NotFound(new { message = $"École {effectiveIdEcole.Value} introuvable." });
            }

            var referenceDate = dateReference ?? datePaiement ?? DateTime.UtcNow;
            var codeDevisePrincipale = string.IsNullOrWhiteSpace(ecole.CodeDevisePrincipale)
                ? "USD"
                : ecole.CodeDevisePrincipale.Trim().ToUpperInvariant();

            var codeCible = string.IsNullOrWhiteSpace(codeDeviseCible)
                ? codeDevisePrincipale
                : codeDeviseCible.Trim().ToUpperInvariant();

            var result = await _currencyConversionService.ConvertAsync(
                effectiveIdEcole.Value,
                codeDeviseSource,
                codeCible,
                montant.Value,
                referenceDate);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    message = result.ErrorMessage,
                    idEcole = effectiveIdEcole.Value,
                    codeDeviseSource = result.CodeDeviseSource,
                    codeDevisePrincipale = codeDevisePrincipale,
                    codeDeviseCible = codeCible
                });
            }

            return Ok(new
            {
                idEcole = result.IdEcole,
                codeDeviseSource = result.CodeDeviseSource,
                codeDevisePrincipale = result.CodeDevisePrincipale,
                codeDeviseCible = result.CodeDeviseCible,
                dateReference = result.DateReference,
                taux = result.Taux,
                montantSource = result.MontantSource,
                montantConverti = result.MontantConverti
            });
        }
    }
}
