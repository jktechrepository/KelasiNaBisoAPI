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
    [Authorize] // 🔒 Gestion des cours - Token JWT requis
    public class CoursController : ControllerBase
    {
        private readonly ICoursRepository _coursRepository;

        public CoursController(ICoursRepository coursRepository)
        {
            _coursRepository = coursRepository;
        }

        /// <summary>
        /// Catalogue des cours (structure pédagogique par classe). Pas de filtre année scolaire :
        /// pour l'enseignement par année, utiliser GET /api/AffectationCours/annee-scolaire/{idAnneeScolaire}.
        /// </summary>
        // GET: api/Cours
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cours>>> GetCours()
        {
            var cours = await _coursRepository.GetAllAsync();
            return Ok(cours);
        }

        // GET: api/Cours/Ecole/5
        [HttpGet("ecole/{idEcole}")]
        public async Task<ActionResult<IEnumerable<Cours>>> GetCoursByEcole(int idEcole)
        {
            var cours = await _coursRepository.GetAllByEcoleAsync(idEcole);
            return Ok(cours);
        }

        // GET: api/Cours/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Cours>> GetCours(int id)
        {
            var cours = await _coursRepository.GetByIdAsync(id);
            if (cours == null)
            {
                return NotFound();
            }
            return Ok(cours);
        }

        // GET: api/Cours/classe/5
        [HttpGet("classe/{idClasse}")]
        public async Task<ActionResult<IEnumerable<Cours>>> GetCoursByClasse(int idClasse)
        {
            var cours = await _coursRepository.GetByClasseAsync(idClasse);
            return Ok(cours);
        }

        // GET: api/Cours/professeur/5
        //[HttpGet("professeur/{idProfesseur}")]
        //public async Task<ActionResult<IEnumerable<Cours>>> GetCoursByProfesseur(int idProfesseur)
        //{
        //    var cours = await _coursRepository.GetByProfesseurAsync(idProfesseur);
        //    return Ok(cours);
        //}

        // GET: api/Cours/5/notes
        [HttpGet("{id}/notes")]
        public async Task<ActionResult<IEnumerable<Note>>> GetCoursNotes(
            int id,
            [FromQuery] int? idAnneeScolaire = null)
        {
            try
            {
                return Ok(await _coursRepository.GetNotesAsync(id, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}/evaluations")]
        public async Task<ActionResult<IEnumerable<Evaluation>>> GetCoursEvaluations(
            int id,
            [FromQuery] int? idAnneeScolaire = null)
        {
            try
            {
                return Ok(await _coursRepository.GetEvaluationsAsync(id, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/Cours/5/ressources
        [HttpGet("{id}/ressources")]
        public async Task<ActionResult<IEnumerable<RessourcePedagogique>>> GetCoursRessources(int id)
        {
            var ressources = await _coursRepository.GetRessourcesAsync(id);
            return Ok(ressources);
        }

        // POST: api/Cours
        [HttpPost]
        public async Task<ActionResult<Cours>> CreateCours(Cours cours)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdCours = await _coursRepository.CreateAsync(cours);
            return CreatedAtAction(nameof(GetCours), new { id = createdCours.IdCours }, createdCours);
        }

        // PUT: api/Cours/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Super-Admin")]
        [ProducesResponseType(typeof(Cours), 200)]
        public async Task<ActionResult<Cours>> UpdateCours(int id, [FromBody] UpdateCoursDto dto)
        {
            if (id != dto.IdCours)
            {
                return BadRequest(new { message = "L'ID ne correspond pas" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing = await _coursRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { message = "Cours non trouvé" });
            }

            existing.NomCours = dto.NomCours;
            existing.Description = dto.Description;
            existing.Ponderation = dto.Ponderation;
            existing.IdClasse = dto.IdClasse;

            var updated = await _coursRepository.UpdateAsync(existing);
            return Ok(updated);
        }

        // DELETE: api/Cours/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCours(int id)
        {
            var exists = await _coursRepository.ExistsAsync(id);
            if (!exists)
            {
                return NotFound();
            }

            await _coursRepository.DeleteAsync(id);
            return NoContent();
        }

        // PUT: api/Cours/toggle-statut/{id}
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _coursRepository.ToggleStatutAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Cours non trouvé" });
                }

                var cours = await _coursRepository.GetByIdAsync(id);
                var estActif = cours != null;
                
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = estActif,
                    cours = cours
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors du changement de statut", error = ex.Message });
            }
        }
    }
}
