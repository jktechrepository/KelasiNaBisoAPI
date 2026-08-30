using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Attributes;
using KelasiNaBiso.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace KelasiNaBiso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FraisController : ControllerBase
    {
        private readonly IFraisRepository _fraisRepository;

        public FraisController(IFraisRepository fraisRepository)
        {
            _fraisRepository = fraisRepository;
        }

        [HttpGet]
        [RequireGlobalAccess]
        public async Task<ActionResult<IEnumerable<Frais>>> GetFrais()
        {
            var frais = await _fraisRepository.GetAllAsync();
            return Ok(frais);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Frais>> GetFrais(int id)
        {
            var frais = await _fraisRepository.GetByIdAsync(id);
            if (frais == null)
                return NotFound();
            return Ok(frais);
        }

        [HttpGet("ecole/{idEcole}")]
        public async Task<IActionResult> GetFraisByEcole(
            int idEcole,
            [FromQuery] int? idAnneeScolaire = null,
            [FromQuery] int? idClasse = null)
        {
            var deny = this.ForbidIfWrongSchool(idEcole);
            if (deny != null)
                return deny;

            try
            {
                return Ok(await _fraisRepository.GetByEcoleAsync(idEcole, idAnneeScolaire, idClasse));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("ecole/{idEcole}/libelle")]
        public async Task<IActionResult> GetFraisByEcoleAndLibelle(
            int idEcole,
            [FromQuery] string libelleFrais,
            [FromQuery] int? idAnneeScolaire = null,
            [FromQuery] int? idClasse = null)
        {
            if (string.IsNullOrWhiteSpace(libelleFrais))
                return BadRequest(new { message = "Le paramètre libelleFrais est requis" });

            var deny = this.ForbidIfWrongSchool(idEcole);
            if (deny != null)
                return deny;

            try
            {
                var result = await _fraisRepository.GetByEcoleAndLibelleAsync(
                    idEcole, libelleFrais, idAnneeScolaire, idClasse);
                if (result.Data == null)
                {
                    return NotFound(new
                    {
                        message = $"Aucun frais trouvé avec le libellé '{libelleFrais}' dans l'école {idEcole}"
                    });
                }
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("direction/{idDirection}")]
        public async Task<IActionResult> GetFraisByDirection(
            int idDirection,
            [FromQuery] int? idAnneeScolaire = null,
            [FromQuery] int? idClasse = null)
        {
            try
            {
                return Ok(await _fraisRepository.GetByDirectionAsync(idDirection, idAnneeScolaire, idClasse));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("annee/{idAnneeScolaire}")]
        public async Task<ActionResult<IEnumerable<Frais>>> GetFraisByAnnee(int idAnneeScolaire)
        {
            var frais = await _fraisRepository.GetByAnneeAsync(idAnneeScolaire);
            return Ok(frais);
        }

        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> FraisExists(int id)
        {
            var exists = await _fraisRepository.ExistsAsync(id);
            return Ok(exists);
        }

        [HttpPost]
        [Permission("Frais.Create")]
        public async Task<ActionResult<Frais>> CreateFrais(Frais frais)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdFrais = await _fraisRepository.CreateAsync(frais);
                return CreatedAtAction(nameof(GetFrais), new { id = createdFrais.IdFrais }, createdFrais);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Super-Admin")]
        [Permission("Frais.Update")]
        public async Task<ActionResult<Frais>> UpdateFrais(int id, [FromBody] UpdateFraisDto dto)
        {
            if (id != dto.IdFrais)
                return BadRequest(new { message = "L'ID ne correspond pas" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _fraisRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = "Frais non trouvé" });

            existing.LibelleFrais = dto.LibelleFrais;
            existing.Montant = dto.Montant;
            existing.Devise = dto.Devise;
            existing.TypeFrais = dto.TypeFrais;
            existing.Periodicite = dto.Periodicite;
            existing.Description = dto.Description;

            if (dto.IdAnneeScolaire.HasValue && dto.IdAnneeScolaire.Value > 0)
                existing.IdAnneeScolaire = dto.IdAnneeScolaire.Value;

            if (dto.IdClasse.HasValue)
                existing.IdClasse = dto.IdClasse.Value > 0 ? dto.IdClasse : null;

            try
            {
                var updated = await _fraisRepository.UpdateAsync(existing);
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Permission("Frais.Delete")]
        public async Task<IActionResult> DeleteFrais(int id)
        {
            var success = await _fraisRepository.DeleteAsync(id);
            if (!success)
                return NotFound();
            return NoContent();
        }

        [HttpPut("toggle-statut/{id}")]
        [Permission("Frais.Update")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _fraisRepository.ToggleStatutAsync(id);
                if (!success)
                    return NotFound(new { message = "Frais non trouvé" });

                var frais = await _fraisRepository.GetByIdAsync(id);
                return Ok(new
                {
                    message = "Statut modifié avec succès",
                    nouveauStatut = frais != null,
                    frais = frais
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur", error = ex.Message });
            }
        }
    }
}
