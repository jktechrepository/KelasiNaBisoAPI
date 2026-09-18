using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services;
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
    public class EvaluationController : ControllerBase
    {
        private readonly IEvaluationRepository _evaluationRepository;
        private readonly PeriodeCotationResolver _periodeResolver;
        private readonly IPedagogieAuthorizationService _pedagogie;

        public EvaluationController(
            IEvaluationRepository evaluationRepository,
            PeriodeCotationResolver periodeResolver,
            IPedagogieAuthorizationService pedagogie)
        {
            _evaluationRepository = evaluationRepository;
            _periodeResolver = periodeResolver;
            _pedagogie = pedagogie;
        }

        [HttpGet]
        [Permission("Evaluation.ReadAll")]
        public async Task<ActionResult<IEnumerable<Evaluation>>> GetEvaluations()
        {
            var evaluations = await _evaluationRepository.GetAllAsync();
            return Ok(await FilterEvaluationsForEnseignantAsync(evaluations));
        }

        [HttpGet("{id}")]
        [Permission("Evaluation.Read")]
        public async Task<IActionResult> GetEvaluation(int id)
        {
            var evaluation = await _evaluationRepository.GetByIdAsync(id);
            if (evaluation == null)
                return NotFound();

            var deny = await this.ForbidIfHorsScopeCoursAsync(evaluation.IdCours);
            if (deny != null)
                return deny;

            return Ok(evaluation);
        }

        [HttpGet("cours/{idCours}")]
        [Permission("Evaluation.Read")]
        public async Task<IActionResult> GetEvaluationsByCours(int idCours)
        {
            var deny = await this.ForbidIfHorsScopeCoursAsync(idCours);
            if (deny != null)
                return deny;

            var evaluations = await _evaluationRepository.GetByCoursAsync(idCours);
            return Ok(evaluations);
        }

        [HttpGet("classe/{idClasse}")]
        [Permission("Evaluation.Read")]
        public async Task<IActionResult> GetEvaluationsByClasse(int idClasse)
        {
            var deny = await this.ForbidIfHorsScopeClasseAsync(idClasse);
            if (deny != null)
                return deny;

            var evaluations = await _evaluationRepository.GetByClasseAsync(idClasse);
            return Ok(evaluations);
        }

        [HttpGet("type/{type}")]
        [Permission("Evaluation.Read")]
        public async Task<ActionResult<IEnumerable<Evaluation>>> GetEvaluationsByType(string type)
        {
            var evaluations = await _evaluationRepository.GetByTypeAsync(type);
            return Ok(await FilterEvaluationsForEnseignantAsync(evaluations));
        }

        [HttpGet("periode/{periode}")]
        [Permission("Evaluation.Read")]
        public async Task<ActionResult<IEnumerable<Evaluation>>> GetEvaluationsByPeriode(string periode)
        {
            var evaluations = await _evaluationRepository.GetByPeriodeAsync(periode);
            return Ok(await FilterEvaluationsForEnseignantAsync(evaluations));
        }

        [HttpGet("statut/{statut}")]
        [Permission("Evaluation.Read")]
        public async Task<ActionResult<IEnumerable<Evaluation>>> GetEvaluationsByStatut(bool statut)
        {
            var evaluations = await _evaluationRepository.GetByStatutAsync(statut);
            return Ok(await FilterEvaluationsForEnseignantAsync(evaluations));
        }

        [HttpGet("exists/{id}")]
        [Permission("Evaluation.Read")]
        public async Task<ActionResult<bool>> EvaluationExists(int id)
        {
            var evaluation = await _evaluationRepository.GetByIdAsync(id);
            if (evaluation == null)
                return Ok(false);

            var deny = await this.ForbidIfHorsScopeCoursAsync(evaluation.IdCours);
            if (deny != null)
                return Ok(false);

            return Ok(true);
        }

        [HttpPost]
        [Permission("Evaluation.Create")]
        public async Task<IActionResult> CreateEvaluation([FromBody] CreateEvaluationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var deny = await this.ForbidIfHorsScopeCoursAsync(dto.IdCours);
            if (deny != null)
                return deny;

            var evaluation = new Evaluation
            {
                TypeEvaluation = dto.TypeEvaluation,
                TitreEvaluation = dto.TitreEvaluation,
                Coefficient = dto.Coefficient,
                IdCours = dto.IdCours,
                IdClasse = dto.IdClasse,
                Statut = true
            };

            try
            {
                await _periodeResolver.ApplyToEvaluationAsync(evaluation, dto.IdPeriode, dto.Periode);
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
        [Authorize(Roles = "Admin,Super-Admin,Enseignant,Directeur,Sous-Directeur")]
        [ProducesResponseType(typeof(Evaluation), 200)]
        public async Task<IActionResult> UpdateEvaluation(int id, [FromBody] UpdateEvaluationDto dto)
        {
            if (id != dto.IdEvaluation)
                return BadRequest(new { message = "L'ID ne correspond pas" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _evaluationRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = "Évaluation non trouvée" });

            var denyExisting = await this.ForbidIfHorsScopeCoursAsync(existing.IdCours);
            if (denyExisting != null)
                return denyExisting;

            var denyTarget = await this.ForbidIfHorsScopeCoursAsync(dto.IdCours);
            if (denyTarget != null)
                return denyTarget;

            existing.TypeEvaluation = dto.TypeEvaluation;
            existing.TitreEvaluation = dto.TitreEvaluation;
            existing.Coefficient = dto.Coefficient;
            existing.IdCours = dto.IdCours;
            existing.IdClasse = dto.IdClasse;

            try
            {
                await _periodeResolver.ApplyToEvaluationAsync(existing, dto.IdPeriode, dto.Periode);
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
            var deny = await this.ForbidIfHorsScopeEvaluationAsync(id);
            if (deny != null)
                return deny;

            var success = await _evaluationRepository.DeleteAsync(id);
            if (!success)
                return NotFound();
            return NoContent();
        }

        [HttpPut("toggle-statut/{id}")]
        [Permission("Evaluation.Update")]
        public async Task<IActionResult> ToggleStatut(int id)
        {
            try
            {
                var deny = await this.ForbidIfHorsScopeEvaluationAsync(id);
                if (deny != null)
                    return deny;

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

        private async Task<IReadOnlyList<Evaluation>> FilterEvaluationsForEnseignantAsync(
            IEnumerable<Evaluation> evaluations,
            CancellationToken cancellationToken = default)
        {
            if (!User.IsInRole(Models.Enums.UserRoles.ENSEIGNANT) || this.IsCotationSchoolBypassRole())
                return evaluations.ToList();

            var idAgent = this.GetCurrentAgentId();
            if (!idAgent.HasValue)
                return Array.Empty<Evaluation>();

            var idEcole = this.GetCurrentUserSchoolId();
            var classIds = await _pedagogie.GetClassesEnseignantAsync(
                idAgent.Value, idAnneeScolaire: null, idEcole: idEcole, cancellationToken);
            if (classIds.Count == 0)
                return Array.Empty<Evaluation>();

            return evaluations.Where(e => classIds.Contains(e.IdClasse)).ToList();
        }
    }
}
