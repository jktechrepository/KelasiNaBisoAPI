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
    [Authorize]
    public class EvaluationController : ControllerBase
    {
        private readonly IEvaluationRepository _evaluationRepository;

        public EvaluationController(IEvaluationRepository evaluationRepository)
        {
            _evaluationRepository = evaluationRepository;
        }

        [HttpGet]
        [Permission("Evaluation.ReadAll")]
        public async Task<ActionResult<IEnumerable<Evaluation>>> GetEvaluations()
        {
            var evaluations = await _evaluationRepository.GetAllAsync();
            return Ok(evaluations);
        }

        [HttpGet("{id}")]
        [Permission("Evaluation.Read")]
        public async Task<ActionResult<Evaluation>> GetEvaluation(int id)
        {
            var evaluation = await _evaluationRepository.GetByIdAsync(id);
            if (evaluation == null)
                return NotFound();
            return Ok(evaluation);
        }

        [HttpGet("cours/{idCours}")]
        [Permission("Evaluation.Read")]
        public async Task<ActionResult<IEnumerable<Evaluation>>> GetEvaluationsByCours(int idCours)
        {
            var evaluations = await _evaluationRepository.GetByCoursAsync(idCours);
            return Ok(evaluations);
        }

        [HttpGet("classe/{idClasse}")]
        [Permission("Evaluation.Read")]
        public async Task<ActionResult<IEnumerable<Evaluation>>> GetEvaluationsByClasse(int idClasse)
        {
            var evaluations = await _evaluationRepository.GetByClasseAsync(idClasse);
            return Ok(evaluations);
        }

        [HttpGet("type/{type}")]
        [Permission("Evaluation.Read")]
        public async Task<ActionResult<IEnumerable<Evaluation>>> GetEvaluationsByType(string type)
        {
            var evaluations = await _evaluationRepository.GetByTypeAsync(type);
            return Ok(evaluations);
        }

        [HttpGet("periode/{periode}")]
        [Permission("Evaluation.Read")]
        public async Task<ActionResult<IEnumerable<Evaluation>>> GetEvaluationsByPeriode(string periode)
        {
            var evaluations = await _evaluationRepository.GetByPeriodeAsync(periode);
            return Ok(evaluations);
        }

        [HttpGet("statut/{statut}")]
        [Permission("Evaluation.Read")]
        public async Task<ActionResult<IEnumerable<Evaluation>>> GetEvaluationsByStatut(bool statut)
        {
            var evaluations = await _evaluationRepository.GetByStatutAsync(statut);
            return Ok(evaluations);
        }

        [HttpGet("exists/{id}")]
        [Permission("Evaluation.Read")]
        public async Task<ActionResult<bool>> EvaluationExists(int id)
        {
            var exists = await _evaluationRepository.ExistsAsync(id);
            return Ok(exists);
        }

        [HttpPost]
        [Permission("Evaluation.Create")]
        public async Task<ActionResult<Evaluation>> CreateEvaluation([FromBody] CreateEvaluationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var evaluation = new Evaluation
            {
                TypeEvaluation = dto.TypeEvaluation,
                TitreEvaluation = dto.TitreEvaluation,
                Periode = dto.Periode,
                Coefficient = dto.Coefficient,
                IdCours = dto.IdCours,
                IdClasse = dto.IdClasse,
                Statut = true
            };

            try
            {
                var createdEvaluation = await _evaluationRepository.CreateAsync(evaluation);
                return CreatedAtAction(nameof(GetEvaluation), new { id = createdEvaluation.IdEvaluation }, createdEvaluation);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Permission("Evaluation.Update")]
        [Authorize(Roles = "Admin,Super-Admin,Enseignant")]
        [ProducesResponseType(typeof(Evaluation), 200)]
        public async Task<ActionResult<Evaluation>> UpdateEvaluation(int id, [FromBody] UpdateEvaluationDto dto)
        {
            if (id != dto.IdEvaluation)
                return BadRequest(new { message = "L'ID ne correspond pas" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _evaluationRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = "Évaluation non trouvée" });

            existing.TypeEvaluation = dto.TypeEvaluation;
            existing.TitreEvaluation = dto.TitreEvaluation;
            existing.Periode = dto.Periode;
            existing.Coefficient = dto.Coefficient;
            existing.IdCours = dto.IdCours;
            existing.IdClasse = dto.IdClasse;

            try
            {
                var updated = await _evaluationRepository.UpdateAsync(existing);
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Permission("Evaluation.Delete")]
        public async Task<IActionResult> DeleteEvaluation(int id)
        {
            var success = await _evaluationRepository.DeleteAsync(id);
            if (!success)
                return NotFound();
            return NoContent();
        }

        [HttpPut("toggle-statut/{id}")]
        [Permission("Evaluation.Update")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _evaluationRepository.ToggleStatutAsync(id);
                if (!success)
                    return NotFound(new { message = "Évaluation non trouvée" });

                var evaluation = await _evaluationRepository.GetByIdAsync(id);
                return Ok(new
                {
                    message = "Statut modifié avec succès",
                    nouveauStatut = evaluation != null && evaluation.Statut == true,
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
