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
    [Authorize] // 🔒 Ressources pédagogiques - Token JWT requis
    public class RessourcePedagogiqueController : ControllerBase
    {
        private readonly IRessourcePedagogiqueRepository _ressourcePedagogiqueRepository;

        public RessourcePedagogiqueController(IRessourcePedagogiqueRepository ressourcePedagogiqueRepository)
        {
            _ressourcePedagogiqueRepository = ressourcePedagogiqueRepository;
        }

        // GET: api/RessourcePedagogique
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RessourcePedagogique>>> GetRessourcePedagogiques()
        {
            var ressourcePedagogiques = await _ressourcePedagogiqueRepository.GetAllAsync();
            return Ok(ressourcePedagogiques);
        }

        // GET: api/RessourcePedagogique/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RessourcePedagogique>> GetRessourcePedagogique(int id)
        {
            var ressourcePedagogique = await _ressourcePedagogiqueRepository.GetByIdAsync(id);
            if (ressourcePedagogique == null)
            {
                return NotFound();
            }
            return Ok(ressourcePedagogique);
        }

        // GET: api/RessourcePedagogique/cours/5
        [HttpGet("cours/{idCours}")]
        public async Task<ActionResult<IEnumerable<RessourcePedagogique>>> GetRessourcePedagogiquesByCours(int idCours)
        {
            var ressourcePedagogiques = await _ressourcePedagogiqueRepository.GetByCoursAsync(idCours);
            return Ok(ressourcePedagogiques);
        }

        // GET: api/RessourcePedagogique/agent/5
        [HttpGet("agent/{idAgent}")]
        //public async Task<ActionResult<IEnumerable<RessourcePedagogique>>> GetRessourcePedagogiquesByAgent(int idAgent)
        //{
        //    var ressourcePedagogiques = await _ressourcePedagogiqueRepository.GetByAgentAsync(idAgent);
        //    return Ok(ressourcePedagogiques);
        //}

        // GET: api/RessourcePedagogique/type/PDF
        [HttpGet("type/{type}")]
        //public async Task<ActionResult<IEnumerable<RessourcePedagogique>>> GetRessourcePedagogiquesByType(string type)
        //{
        //    var ressourcePedagogiques = await _ressourcePedagogiqueRepository.GetByTypeAsync(type);
        //    return Ok(ressourcePedagogiques);
        //}

        // GET: api/RessourcePedagogique/statut/true
        [HttpGet("statut/{statut}")]
        //public async Task<ActionResult<IEnumerable<RessourcePedagogique>>> GetRessourcePedagogiquesByStatut(bool statut)
        //{
        //    var ressourcePedagogiques = await _ressourcePedagogiqueRepository.GetByStatutAsync(statut);
        //    return Ok(ressourcePedagogiques);
        //}

        // GET: api/RessourcePedagogique/exists/5
        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> RessourcePedagogiqueExists(int id)
        {
            var exists = await _ressourcePedagogiqueRepository.ExistsAsync(id);
            return Ok(exists);
        }

        // POST: api/RessourcePedagogique
        [HttpPost]
        public async Task<ActionResult<RessourcePedagogique>> CreateRessourcePedagogique(RessourcePedagogique ressourcePedagogique)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdRessourcePedagogique = await _ressourcePedagogiqueRepository.CreateAsync(ressourcePedagogique);
            return CreatedAtAction(nameof(GetRessourcePedagogique), new { id = createdRessourcePedagogique.IdRessourcePedagogique }, createdRessourcePedagogique);
        }

        // PUT: api/RessourcePedagogique/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Super-Admin,Enseignant")]
        public async Task<ActionResult<RessourcePedagogique>> UpdateRessourcePedagogique(int id, [FromBody] UpdateRessourcePedagogiqueDto dto)
        {
            if (id != dto.IdRessourcePedagogique)
            {
                return BadRequest(new { message = "L'ID ne correspond pas" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing = await _ressourcePedagogiqueRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { message = "Ressource pédagogique non trouvée" });
            }

            existing.TitreRessourcePedagogique = dto.TitreRessourcePedagogique;
            existing.FormatRessourcePedagogique = dto.FormatRessourcePedagogique;
            existing.UrlRessourcePedagogique = dto.UrlRessourcePedagogique;
            existing.IdCours = dto.IdCours;

            var updated = await _ressourcePedagogiqueRepository.UpdateAsync(existing);
            return Ok(updated);
        }

        // DELETE: api/RessourcePedagogique/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRessourcePedagogique(int id)
        {
            var success = await _ressourcePedagogiqueRepository.DeleteAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        // PUT: api/RessourcePedagogique/toggle-statut/{id}
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _ressourcePedagogiqueRepository.ToggleStatutAsync(id);
                if (!success)
                    return NotFound(new { message = "Ressource pédagogique non trouvée" });

                var ressource = await _ressourcePedagogiqueRepository.GetByIdAsync(id);
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = ressource != null,
                    ressource = ressource
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur", error = ex.Message });
            }
        }
    }
}
