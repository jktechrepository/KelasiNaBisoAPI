using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;

namespace KelasiNaBiso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔒 Options - Token JWT requis
    public class OptionController : ControllerBase
    {
        private readonly IOptionRepository _optionRepository;

        public OptionController(IOptionRepository optionRepository)
        {
            _optionRepository = optionRepository;
        }

        // GET: api/Option
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Option>>> GetOptions()
        {
            var options = await _optionRepository.GetAllAsync();
            return Ok(options);
        }

        // GET: api/Option/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Option>> GetOption(int id)
        {
            var option = await _optionRepository.GetByIdAsync(id);
            if (option == null)
            {
                return NotFound();
            }
            return Ok(option);
        }

        // GET: api/Option/section/5
        [HttpGet("Ecole/{idEcole}")]
        public async Task<ActionResult<IEnumerable<Option>>> GetOptionsByEcole(int idEcole)
        {
            var options = await _optionRepository.GetAllAsync();
            var optionsByEcole = options.Where(o => o.Section.IdEcole == idEcole).ToList();

            return Ok(optionsByEcole);
        }

        // GET: api/Option/section/5
        [HttpGet("section/{idSection}")]
        public async Task<ActionResult<IEnumerable<Option>>> GetOptionsBySection(int idSection)
        {
            var options = await _optionRepository.GetBySectionAsync(idSection);
            return Ok(options);
        }

        // GET: api/Option/nom/Physique
        [HttpGet("nom/{nom}")]
        public async Task<ActionResult<Option>> GetOptionByNom(string nom)
        {
            var option = await _optionRepository.GetByNomAsync(nom);
            if (option == null)
            {
                return NotFound();
            }
            return Ok(option);
        }

        // GET: api/Option/statut/true
        [HttpGet("statut/{statut}")]
        //public async Task<ActionResult<IEnumerable<Option>>> GetOptionsByStatut(bool statut)
        //{
        //    var options = await _optionRepository.GetByStatutAsync(statut);
        //    return Ok(options);
        //}

        // GET: api/Option/exists/5
        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> OptionExists(int id)
        {
            var exists = await _optionRepository.ExistsAsync(id);
            return Ok(exists);
        }

        // POST: api/Option
        [HttpPost]
        public async Task<ActionResult<Option>> CreateOption(Option option)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdOption = await _optionRepository.CreateAsync(option);
            return CreatedAtAction(nameof(GetOption), new { id = createdOption.IdOption }, createdOption);
        }

        // PUT: api/Option/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Super-Admin")]
        public async Task<ActionResult<Option>> UpdateOption(int id, [FromBody] UpdateOptionDto dto)
        {
            if (id != dto.IdOption)
            {
                return BadRequest(new { message = "L'ID ne correspond pas" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing = await _optionRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { message = "Option non trouvée" });
            }

            existing.NomOption = dto.NomOption;
            existing.IdSection = dto.IdSection;

            var updated = await _optionRepository.UpdateAsync(existing);
            return Ok(updated);
        }

        // DELETE: api/Option/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOption(int id)
        {
            var success = await _optionRepository.DeleteAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        // PUT: api/Option/toggle-statut/{id}
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _optionRepository.ToggleStatutAsync(id);
                if (!success)
                    return NotFound(new { message = "Option non trouvée" });

                var option = await _optionRepository.GetByIdAsync(id);
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = option != null,
                    option = option
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur", error = ex.Message });
            }
        }
    }
}
