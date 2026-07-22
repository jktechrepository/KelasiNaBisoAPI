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
    [Authorize] // 🔒 Affectation des cours - Token JWT requis
    public class AffectationCoursController : ControllerBase
    {
        private readonly IAffectationCoursRepository _affectationCoursRepository;

        public AffectationCoursController(IAffectationCoursRepository affectationCoursRepository)
        {
            _affectationCoursRepository = affectationCoursRepository;
        }

        // GET: api/AffectationCours
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AffectationCours>>> GetAll()
        {
            try
            {
                var affectations = await _affectationCoursRepository.GetAllAsync();
                return Ok(affectations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne du serveur: {ex.Message}");
            }
        }

        // GET: api/AffectationCours/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AffectationCours>> GetById(int id)
        {
            try
            {
                var affectation = await _affectationCoursRepository.GetByIdAsync(id);
                if (affectation == null)
                {
                    return NotFound($"Affectation de cours avec l'ID {id} non trouvée.");
                }

                return Ok(affectation);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne du serveur: {ex.Message}");
            }
        }

        // POST: api/AffectationCours
        [HttpPost]
        public async Task<ActionResult<AffectationCours>> Create(AffectationCours affectationCours)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdAffectation = await _affectationCoursRepository.CreateAsync(affectationCours);
                return CreatedAtAction(nameof(GetById), new { id = createdAffectation.IdAffectationCours }, createdAffectation);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne du serveur: {ex.Message}");
            }
        }

        // PUT: api/AffectationCours/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Super-Admin")]
        public async Task<ActionResult<AffectationCours>> Update(int id, [FromBody] UpdateAffectationCoursDto dto)
        {
            try
            {
                if (id != dto.IdAffectationCours)
                {
                    return BadRequest(new { message = "L'ID dans l'URL ne correspond pas à l'ID dans le corps de la requête" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existing = await _affectationCoursRepository.GetByIdAsync(id);
                if (existing == null)
                {
                    return NotFound(new { message = "Affectation non trouvée" });
                }

                existing.IdAgent = dto.IdAgent;
                existing.IdCours = dto.IdCours;
                existing.IdAnneeScolaire = dto.IdAnneeScolaire;
                if (dto.DateAffectation.HasValue)
                    existing.DateAffectation = dto.DateAffectation.Value;
                existing.DateFinAffectation = dto.DateFinAffectation;
                existing.Statut = dto.Statut;
                existing.Commentaire = dto.Commentaire;

                var updated = await _affectationCoursRepository.UpdateAsync(existing);
                return Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne du serveur: {ex.Message}");
            }
        }

        // DELETE: api/AffectationCours/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _affectationCoursRepository.DeleteAsync(id);
                if (!success)
                {
                    return NotFound($"Affectation de cours avec l'ID {id} non trouvée.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne du serveur: {ex.Message}");
            }
        }

        // GET: api/AffectationCours/agent/5
        [HttpGet("agent/{idAgent}")]
        public async Task<ActionResult<IEnumerable<AffectationCours>>> GetByAgent(int idAgent)
        {
            try
            {
                var affectations = await _affectationCoursRepository.GetByAgentAsync(idAgent);
                return Ok(affectations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne du serveur: {ex.Message}");
            }
        }

        // GET: api/AffectationCours/cours/5
        [HttpGet("cours/{idCours}")]
        public async Task<ActionResult<IEnumerable<AffectationCours>>> GetByCours(int idCours)
        {
            try
            {
                var affectations = await _affectationCoursRepository.GetByCoursAsync(idCours);
                return Ok(affectations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne du serveur: {ex.Message}");
            }
        }

        // GET: api/AffectationCours/annee-scolaire/5
        [HttpGet("annee-scolaire/{idAnneeScolaire}")]
        public async Task<ActionResult<IEnumerable<AffectationCours>>> GetByAnneeScolaire(int idAnneeScolaire)
        {
            try
            {
                var affectations = await _affectationCoursRepository.GetByAnneeScolaireAsync(idAnneeScolaire);
                return Ok(affectations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne du serveur: {ex.Message}");
            }
        }

        // GET: api/AffectationCours/agent/5/annee-scolaire/3
        [HttpGet("agent/{idAgent}/annee-scolaire/{idAnneeScolaire}")]
        public async Task<ActionResult<IEnumerable<AffectationCours>>> GetByAgentAndAnneeScolaire(int idAgent, int idAnneeScolaire)
        {
            try
            {
                var affectations = await _affectationCoursRepository.GetByAgentAndAnneeScolaireAsync(idAgent, idAnneeScolaire);
                return Ok(affectations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne du serveur: {ex.Message}");
            }
        }

        // GET: api/AffectationCours/cours/5/annee-scolaire/3
        [HttpGet("cours/{idCours}/annee-scolaire/{idAnneeScolaire}")]
        public async Task<ActionResult<IEnumerable<AffectationCours>>> GetByCoursAndAnneeScolaire(int idCours, int idAnneeScolaire)
        {
            try
            {
                var affectations = await _affectationCoursRepository.GetByCoursAndAnneeScolaireAsync(idCours, idAnneeScolaire);
                return Ok(affectations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne du serveur: {ex.Message}");
            }
        }

        // GET: api/AffectationCours/actives
        [HttpGet("actives")]
        public async Task<ActionResult<IEnumerable<AffectationCours>>> GetActives()
        {
            try
            {
                var affectations = await _affectationCoursRepository.GetActivesAsync();
                return Ok(affectations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne du serveur: {ex.Message}");
            }
        }

        // GET: api/AffectationCours/actives/agent/5
        [HttpGet("actives/agent/{idAgent}")]
        public async Task<ActionResult<IEnumerable<AffectationCours>>> GetActivesByAgent(int idAgent)
        {
            try
            {
                var affectations = await _affectationCoursRepository.GetActivesByAgentAsync(idAgent);
                return Ok(affectations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne du serveur: {ex.Message}");
            }
        }

        // GET: api/AffectationCours/actives/cours/5
        [HttpGet("actives/cours/{idCours}")]
        public async Task<ActionResult<IEnumerable<AffectationCours>>> GetActivesByCours(int idCours)
        {
            try
            {
                var affectations = await _affectationCoursRepository.GetActivesByCoursAsync(idCours);
                return Ok(affectations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne du serveur: {ex.Message}");
            }
        }

        // POST: api/AffectationCours/desactiver/5
        [HttpPost("desactiver/{id}")]
        public async Task<IActionResult> Desactiver(int id)
        {
            try
            {
                var affectation = await _affectationCoursRepository.GetByIdAsync(id);
                if (affectation == null)
                {
                    return NotFound($"Affectation de cours avec l'ID {id} non trouvée.");
                }

                affectation.Statut = false;
                affectation.DateFinAffectation = DateTime.Now;

                var updatedAffectation = await _affectationCoursRepository.UpdateAsync(affectation);
                return Ok(updatedAffectation);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne du serveur: {ex.Message}");
            }
        }

        // POST: api/AffectationCours/reactiver/5
        [HttpPost("reactiver/{id}")]
        public async Task<IActionResult> Reactiver(int id)
        {
            try
            {
                var affectation = await _affectationCoursRepository.GetByIdAsync(id);
                if (affectation == null)
                {
                    return NotFound($"Affectation de cours avec l'ID {id} non trouvée.");
                }

                affectation.Statut = true;
                affectation.DateFinAffectation = null;

                var updatedAffectation = await _affectationCoursRepository.UpdateAsync(affectation);
                return Ok(updatedAffectation);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne du serveur: {ex.Message}");
            }
        }

        // PUT: api/AffectationCours/toggle-statut/{id}
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _affectationCoursRepository.ToggleStatutAsync(id);
                if (!success)
                    return NotFound(new { message = "Affectation non trouvée" });

                var affectation = await _affectationCoursRepository.GetByIdAsync(id);
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = affectation != null,
                    affectation = affectation
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur", error = ex.Message });
            }
        }
    }
}
