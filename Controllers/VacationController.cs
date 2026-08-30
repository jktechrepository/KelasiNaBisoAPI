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
    [Authorize] // 🔒 Horaires/Vacations - Token JWT requis
    public class VacationController : ControllerBase
    {
        private readonly IVacationRepository _VacationRepository;

        public VacationController(IVacationRepository VacationRepository)
        {
            _VacationRepository = VacationRepository;
        }

        /// <summary>
        /// Créneaux horaires (vacations) par école — configuration transversale aux années.
        /// Le filtre année scolaire s'applique aux endpoints Présence (pointages élèves).
        /// </summary>
        // GET: api/Vacation
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vacation>>> GetVacations()
        {
            var Vacations = await _VacationRepository.GetAllAsync();
            return Ok(Vacations);
        }

        // GET: api/Vacation/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Vacation>> GetVacation(int id)
        {
            var Vacation = await _VacationRepository.GetByIdAsync(id);
            if (Vacation == null)
            {
                return NotFound();
            }
            return Ok(Vacation);
        }

        // GET: api/Vacation/classe/5
        [HttpGet("ecole/{idEcole}")]
        public async Task<ActionResult<IEnumerable<Vacation>>> GetVacationsByEcole(int idEcole)
        {
            var Vacations = await _VacationRepository.GetByEcoleAsync(idEcole);
            return Ok(Vacations);
        }

        // GET: api/Vacation/cours/5
        [HttpGet("cours/{idCours}")]
        public async Task<ActionResult<IEnumerable<Vacation>>> GetVacationsByCours(int idCours)
        {
            var Vacations = await _VacationRepository.GetByCoursAsync(idCours);
            return Ok(Vacations);
        }

        // GET: api/Vacation/jour/Lundi
        [HttpGet("jour/{jour}")]
        public async Task<ActionResult<IEnumerable<Vacation>>> GetVacationsByJour(int Nbjour)
        {
            var Vacations = await _VacationRepository.GetByJourAsync(Nbjour);
            return Ok(Vacations);
        }

        // GET: api/Vacation/statut/true
        [HttpGet("statut/{statut}")]
        ////public async Task<ActionResult<IEnumerable<Vacation>>> GetVacationsByStatut(bool statut)
        ////{
        ////    var Vacations = await _VacationRepository.GetByStatutAsync(statut);
        ////    return Ok(Vacations);
        ////}

        // GET: api/Vacation/exists/5
        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> VacationExists(int id)
        {
            var exists = await _VacationRepository.ExistsAsync(id);
            return Ok(exists);
        }

        // POST: api/Vacation
        [HttpPost]
        public async Task<ActionResult<Vacation>> CreateVacation(CreateVacationDto vacationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var vacation = new Vacation
                {
                    NomVacation = vacationDto.NomVacation,
                    HeureDebut = vacationDto.GetHeureDebut(),
                    HeureFin = vacationDto.GetHeureFin(),
                    HeureDebutPause = vacationDto.GetHeureDebutPause(),
                    HeureFinPause = vacationDto.GetHeureFinPause(),
                    NombreJoursParSemaine = vacationDto.NombreJoursParSemaine,
                    IdEcole = vacationDto.IdEcole
                };

                var createdVacation = await _VacationRepository.CreateAsync(vacation);
                return CreatedAtAction(nameof(GetVacation), new { id = createdVacation.IdVacation }, createdVacation);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // PUT: api/Vacation/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Super-Admin,Enseignant")]
        public async Task<ActionResult<Vacation>> UpdateVacation(int id, [FromBody] UpdateVacationDto dto)
        {
            if (id != dto.IdVacation)
            {
                return BadRequest(new { message = "L'ID ne correspond pas" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing = await _VacationRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { message = "Vacation non trouvée" });
            }

            existing.NomVacation = dto.NomVacation;
            existing.HeureDebut = dto.HeureDebut;
            existing.HeureFin = dto.HeureFin;
            existing.HeureDebutPause = dto.HeureDebutPause;
            existing.HeureFinPause = dto.HeureFinPause;
            existing.NombreJoursParSemaine = dto.NombreJoursParSemaine;

            var updated = await _VacationRepository.UpdateAsync(existing);
            return Ok(updated);
        }

        // DELETE: api/Vacation/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVacation(int id)
        {
            var success = await _VacationRepository.DeleteAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        // PUT: api/Vacation/toggle-statut/{id}
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _VacationRepository.ToggleStatutAsync(id);
                if (!success)
                    return NotFound(new { message = "Vacation non trouvée" });

                var vacation = await _VacationRepository.GetByIdAsync(id);
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = vacation != null,
                    vacation = vacation
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur", error = ex.Message });
            }
        }
    }
}
