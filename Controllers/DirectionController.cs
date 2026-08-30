using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace KelasiNaBiso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔒 Directions - Token JWT requis
    public class DirectionController : ControllerBase
    {
        private readonly IDirectionRepository _directionRepository;

        public DirectionController(IDirectionRepository directionRepository)
        {
            _directionRepository = directionRepository;
        }

        // GET: api/Direction
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Direction>>> GetDirections()
        {
            var directions = await _directionRepository.GetAllAsync();
            return Ok(directions);
        }

        // GET: api/Direction/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Direction>> GetDirection(int id)
        {
            var direction = await _directionRepository.GetByIdAsync(id);
            if (direction == null)
            {
                return NotFound();
            }
            return Ok(direction);
        }

        // GET: api/Direction/ecole/5
        [HttpGet("ecole/{idEcole}")]
        public async Task<ActionResult<IEnumerable<Direction>>> GetDirectionsByEcole(int idEcole)
        {
            var directions = await _directionRepository.GetByEcoleAsync(idEcole);
            return Ok(directions);
        }

        // GET: api/Direction/nom/Informatique
        [HttpGet("nom/{nom}")]
        public async Task<ActionResult<Direction>> GetDirectionByNom(string nom)
        {
            var direction = await _directionRepository.GetByNomAsync(nom);
            if (direction == null)
            {
                return NotFound();
            }
            return Ok(direction);
        }

        // GET: api/Direction/exists/5
        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> DirectionExists(int id)
        {
            var exists = await _directionRepository.ExistsAsync(id);
            return Ok(exists);
        }

        // GET: api/Direction/5/classes
        [HttpGet("{id}/classes")]
        public async Task<ActionResult<IEnumerable<Classe>>> GetClassesByDirection(int id)
        {
            var classes = await _directionRepository.GetClassesAsync(id);
            return Ok(classes);
        }

        // POST: api/Direction
        [HttpPost]
        public async Task<ActionResult<Direction>> CreateDirection(Direction direction)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Vérifier si une direction avec le même nom existe déjà dans la même école
            if (await _directionRepository.ExistsByNomAndEcoleAsync(direction.NomDirection, direction.IdEcole ?? 0))
            {
                return BadRequest("Une direction avec ce nom existe déjà dans cette école.");
            }

            var createdDirection = await _directionRepository.CreateAsync(direction);
            return CreatedAtAction(nameof(GetDirection), new { id = createdDirection.IdDirection }, createdDirection);
        }

        // PUT: api/Direction/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Super-Admin")]
        public async Task<ActionResult<Direction>> UpdateDirection(int id, [FromBody] UpdateDirectionDto dto)
        {
            if (id != dto.IdDirection)
            {
                return BadRequest(new { message = "L'ID ne correspond pas" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing = await _directionRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { message = "Direction non trouvée" });
            }

            // Vérifier si une autre direction avec le même nom existe déjà dans la même école
            var directionWithSameName = await _directionRepository.GetByNomAsync(dto.NomDirection);
            if (directionWithSameName != null && directionWithSameName.IdDirection != id && directionWithSameName.IdEcole == existing.IdEcole)
            {
                return BadRequest(new { message = "Une direction avec ce nom existe déjà dans cette école" });
            }

            existing.NomDirection = dto.NomDirection;
            existing.NiveauEnseignement = dto.NiveauEnseignement;

            var updated = await _directionRepository.UpdateAsync(existing);
            return Ok(updated);
        }

        // DELETE: api/Direction/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDirection(int id)
        {
            var direction = await _directionRepository.GetByIdAsync(id);
            if (direction == null)
            {
                return NotFound();
            }

            // Vérifier s'il y a des classes associées
            var classes = await _directionRepository.GetClassesAsync(id);
            if (classes.Any())
            {
                return BadRequest("Impossible de supprimer cette direction car elle contient des classes.");
            }

            var deleted = await _directionRepository.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        // PUT: api/Direction/toggle-statut/{id}
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _directionRepository.ToggleStatutAsync(id);
                if (!success)
                    return NotFound(new { message = "Direction non trouvée" });

                var direction = await _directionRepository.GetByIdAsync(id);
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = direction != null,
                    direction = direction
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur", error = ex.Message });
            }
        }
    }
}
