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
    [Authorize] // 🔒 Sections - Token JWT requis
    public class SectionController : ControllerBase
    {
        private readonly ISectionRepository _sectionRepository;

        public SectionController(ISectionRepository sectionRepository)
        {
            _sectionRepository = sectionRepository;
        }

        // GET: api/Section
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Section>>> GetSections()
        {
            var sections = await _sectionRepository.GetAllAsync();
            return Ok(sections);
        }

        // GET: api/Section/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Section>> GetSection(int id)
        {
            var section = await _sectionRepository.GetByIdAsync(id);
            if (section == null)
            {
                return NotFound();
            }
            return Ok(section);
        }

        // GET: api/Section/ecole/5
        [HttpGet("ecole/{idEcole}")]
        public async Task<ActionResult<IEnumerable<Section>>> GetSectionsByEcole(int idEcole)
        {
            var sections = await _sectionRepository.GetByEcoleAsync(idEcole);
            return Ok(sections);
        }

        // GET: api/Section/nom/Scientifique
        [HttpGet("nom/{nom}")]
        public async Task<ActionResult<Section>> GetSectionByNom(string nom)
        {
            var section = await _sectionRepository.GetByNomAsync(nom);
            if (section == null)
            {
                return NotFound();
            }
            return Ok(section);
        }

        // GET: api/Section/statut/true
        [HttpGet("statut/{statut}")]
        //public async Task<ActionResult<IEnumerable<Section>>> GetSectionsByStatut(bool statut)
        //{
        //    var sections = await _sectionRepository.GetByStatutAsync(statut);
        //    return Ok(sections);
        //}

        // GET: api/Section/exists/5
        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> SectionExists(int id)
        {
            var exists = await _sectionRepository.ExistsAsync(id);
            return Ok(exists);
        }

        // POST: api/Section
        [HttpPost]
        public async Task<ActionResult<Section>> CreateSection(Section section)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdSection = await _sectionRepository.CreateAsync(section);
            return CreatedAtAction(nameof(GetSection), new { id = createdSection.IdSection }, createdSection);
        }

        // PUT: api/Section/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Super-Admin")]
        public async Task<ActionResult<Section>> UpdateSection(int id, [FromBody] UpdateSectionDto dto)
        {
            if (id != dto.IdSection)
            {
                return BadRequest(new { message = "L'ID ne correspond pas" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing = await _sectionRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { message = "Section non trouvée" });
            }

            existing.NomSection = dto.NomSection;

            var updated = await _sectionRepository.UpdateAsync(existing);
            return Ok(updated);
        }

        // DELETE: api/Section/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSection(int id)
        {
            var success = await _sectionRepository.DeleteAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        // PUT: api/Section/toggle-statut/{id}
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _sectionRepository.ToggleStatutAsync(id);
                if (!success)
                    return NotFound(new { message = "Section non trouvée" });

                var section = await _sectionRepository.GetByIdAsync(id);
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = section != null,
                    section = section
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur", error = ex.Message });
            }
        }
    }
}
