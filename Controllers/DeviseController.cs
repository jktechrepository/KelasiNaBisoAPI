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

        /// <summary>Liste les devises monétaires d'une école.</summary>
        [HttpGet]
        public async Task<IActionResult> GetDevises(
            [FromQuery] int idEcole,
            [FromQuery] bool? statut = null)
        {
            if (idEcole <= 0)
            {
                return BadRequest(new { message = "Le paramètre idEcole est obligatoire." });
            }

            var ecole = await _ecoleRepository.GetByIdAsync(idEcole);
            if (ecole == null)
            {
                return NotFound(new { message = $"École {idEcole} introuvable." });
            }

            var query = _context.DevisesMonetaires
                .AsNoTracking()
                .Where(d => d.IdEcole == idEcole);

            if (statut.HasValue)
            {
                query = query.Where(d => d.Statut == statut.Value);
            }

            var items = await query
                .OrderBy(d => d.CodeDevise)
                .Select(d => new DeviseMonetaireDto
                {
                    IdDeviseMonetaire = d.IdDeviseMonetaire,
                    IdEcole = d.IdEcole,
                    CodeDevise = d.CodeDevise,
                    Libelle = d.Libelle,
                    Symbole = d.Symbole,
                    Statut = d.Statut,
                    DateCreation = d.DateCreation
                })
                .ToListAsync();

            return Ok(items);
        }

        /// <summary>Crée une devise monétaire pour une école.</summary>
        [HttpPost]
        public async Task<IActionResult> CreateDevise([FromBody] CreateDeviseMonetaireDto dto)
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

            var code = DeviseMonetaireSeedHelper.NormalizeCode(dto.CodeDevise);
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest(new { message = "Le code devise est obligatoire." });
            }

            if (string.IsNullOrWhiteSpace(dto.Libelle))
            {
                return BadRequest(new { message = "Le libellé est obligatoire." });
            }

            var exists = await _context.DevisesMonetaires
                .AnyAsync(d => d.IdEcole == dto.IdEcole && d.CodeDevise == code);

            if (exists)
            {
                return Conflict(new { message = $"La devise {code} existe déjà pour l'école {dto.IdEcole}." });
            }

            var entity = new DeviseMonetaire
            {
                IdEcole = dto.IdEcole,
                CodeDevise = code,
                Libelle = dto.Libelle.Trim(),
                Symbole = string.IsNullOrWhiteSpace(dto.Symbole) ? null : dto.Symbole.Trim(),
                Statut = dto.Statut,
                DateCreation = DateTime.UtcNow
            };

            _context.DevisesMonetaires.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDevises), new { idEcole = entity.IdEcole }, new DeviseMonetaireDto
            {
                IdDeviseMonetaire = entity.IdDeviseMonetaire,
                IdEcole = entity.IdEcole,
                CodeDevise = entity.CodeDevise,
                Libelle = entity.Libelle,
                Symbole = entity.Symbole,
                Statut = entity.Statut,
                DateCreation = entity.DateCreation
            });
        }

        /// <summary>Met à jour libellé, symbole et statut d'une devise.</summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateDevise(int id, [FromBody] UpdateDeviseMonetaireDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var entity = await _context.DevisesMonetaires.FirstOrDefaultAsync(d => d.IdDeviseMonetaire == id);
            if (entity == null)
            {
                return NotFound(new { message = $"Devise {id} introuvable." });
            }

            if (string.IsNullOrWhiteSpace(dto.Libelle))
            {
                return BadRequest(new { message = "Le libellé est obligatoire." });
            }

            entity.Libelle = dto.Libelle.Trim();
            entity.Symbole = string.IsNullOrWhiteSpace(dto.Symbole) ? null : dto.Symbole.Trim();
            entity.Statut = dto.Statut;

            await _context.SaveChangesAsync();

            return Ok(new DeviseMonetaireDto
            {
                IdDeviseMonetaire = entity.IdDeviseMonetaire,
                IdEcole = entity.IdEcole,
                CodeDevise = entity.CodeDevise,
                Libelle = entity.Libelle,
                Symbole = entity.Symbole,
                Statut = entity.Statut,
                DateCreation = entity.DateCreation
            });
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

            if (!await _currencyConversionService.IsActiveDeviseAsync(dto.IdEcole, source))
            {
                return BadRequest(new { message = $"La devise source {source} est absente ou inactive pour l'école {dto.IdEcole}." });
            }

            if (!await _currencyConversionService.IsActiveDeviseAsync(dto.IdEcole, cible))
            {
                return BadRequest(new { message = $"La devise cible {cible} est absente ou inactive pour l'école {dto.IdEcole}." });
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
