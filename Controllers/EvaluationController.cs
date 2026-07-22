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
    [Authorize] // 🔒 Évaluations - Token JWT requis
    public class EvaluationController : ControllerBase
    {
        private readonly IEvaluationRepository _evaluationRepository;

        public EvaluationController(IEvaluationRepository evaluationRepository)
        {
            _evaluationRepository = evaluationRepository;
        }

        // GET: api/Evaluation
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Evaluation>>> GetEvaluations()
        {
            var evaluations = await _evaluationRepository.GetAllAsync();
            return Ok(evaluations);
        }

        // GET: api/Evaluation/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Evaluation>> GetEvaluation(int id)
        {
            var evaluation = await _evaluationRepository.GetByIdAsync(id);
            if (evaluation == null)
            {
                return NotFound();
            }
            return Ok(evaluation);
        }

        // GET: api/Evaluation/cours/5
        [HttpGet("cours/{idCours}")]
        public async Task<ActionResult<IEnumerable<Evaluation>>> GetEvaluationsByCours(int idCours)
        {
            var evaluations = await _evaluationRepository.GetByCoursAsync(idCours);
            return Ok(evaluations);
        }

        // GET: api/Evaluation/classe/5
        [HttpGet("classe/{idClasse}")]
        public async Task<ActionResult<IEnumerable<Evaluation>>> GetEvaluationsByClasse(int idClasse)
        {
            var evaluations = await _evaluationRepository.GetByClasseAsync(idClasse);
            return Ok(evaluations);
        }

        // GET: api/Evaluation/type/Examen
        [HttpGet("type/{type}")]
        public async Task<ActionResult<IEnumerable<Evaluation>>> GetEvaluationsByType(string type)
        {
            var evaluations = await _evaluationRepository.GetByTypeAsync(type);
            return Ok(evaluations);
        }

        // GET: api/Evaluation/statut/true
        [HttpGet("statut/{statut}")]
        //public async Task<ActionResult<IEnumerable<Evaluation>>> GetEvaluationsByStatut(bool statut)
        //{
        //    var evaluations = await _evaluationRepository.GetByStatutAsync(statut);
        //    return Ok(evaluations);
        //}

        // GET: api/Evaluation/exists/5
        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> EvaluationExists(int id)
        {
            var exists = await _evaluationRepository.ExistsAsync(id);
            return Ok(exists);
        }

        // POST: api/Evaluation
        [HttpPost]
        public async Task<ActionResult<Evaluation>> CreateEvaluation(Evaluation evaluation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdEvaluation = await _evaluationRepository.CreateAsync(evaluation);
            return CreatedAtAction(nameof(GetEvaluation), new { id = createdEvaluation.IdEvaluation }, createdEvaluation);
        }

        // PUT: api/Evaluation/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Super-Admin,Enseignant")]
        [ProducesResponseType(typeof(Evaluation), 200)]
        public async Task<ActionResult<Evaluation>> UpdateEvaluation(int id, [FromBody] UpdateEvaluationDto dto)
        {
            if (id != dto.IdEvaluation)
            {
                return BadRequest(new { message = "L'ID ne correspond pas" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing = await _evaluationRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { message = "Évaluation non trouvée" });
            }

            existing.TypeEvaluation = dto.TypeEvaluation;
            existing.Coefficient = dto.Coefficient;
            existing.IdCours = dto.IdCours;
            existing.IdClasse = dto.IdClasse;

            var updated = await _evaluationRepository.UpdateAsync(existing);
            return Ok(updated);
        }

        // DELETE: api/Evaluation/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvaluation(int id)
        {
            var success = await _evaluationRepository.DeleteAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        // PUT: api/Evaluation/toggle-statut/{id}
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _evaluationRepository.ToggleStatutAsync(id);
                if (!success)
                    return NotFound(new { message = "Évaluation non trouvée" });

                var evaluation = await _evaluationRepository.GetByIdAsync(id);
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = evaluation != null,
                    evaluation = evaluation
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur", error = ex.Message });
            }
        }
    }
}
