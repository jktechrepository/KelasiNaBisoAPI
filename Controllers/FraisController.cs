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
    [Authorize] // 🔒 Frais scolaires - Token JWT requis
    public class FraisController : ControllerBase
    {
        private readonly IFraisRepository _fraisRepository;

        public FraisController(IFraisRepository fraisRepository)
        {
            _fraisRepository = fraisRepository;
        }

        // GET: api/Frais
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Frais>>> GetFrais()
        {
            var frais = await _fraisRepository.GetAllAsync();
            return Ok(frais);
        }

        // GET: api/Frais/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Frais>> GetFrais(int id)
        {
            var frais = await _fraisRepository.GetByIdAsync(id);
            if (frais == null)
            {
                return NotFound();
            }
            return Ok(frais);
        }

        // GET: api/Frais/eleve/5
        [HttpGet("eleve/{idEleve}")]
        //public async Task<ActionResult<IEnumerable<Frais>>> GetFraisByEleve(int idEleve)
        //{
        //    var frais = await _fraisRepository.GetByEleveAsync(idEleve);
        //    return Ok(frais);
        //}

        // GET: api/Frais/direction/5
        [HttpGet("ecole/{idEcole}")]
        public async Task<ActionResult<IEnumerable<Frais>>> GetFraisByEcole(int idEcole)
        {
            var frais = await _fraisRepository.GetByEcoleAsync(idEcole);
            return Ok(frais);
        }

        // GET: api/Frais/ecole/{idEcole}/libelle?libelleFrais=Minerval
        /// <summary>
        /// Récupère un frais par son libellé dans une école spécifique
        /// </summary>
        [HttpGet("ecole/{idEcole}/libelle")]
        public async Task<ActionResult<Frais>> GetFraisByEcoleAndLibelle(int idEcole, [FromQuery] string libelleFrais)
        {
            if (string.IsNullOrWhiteSpace(libelleFrais))
            {
                return BadRequest(new { message = "Le paramètre libelleFrais est requis" });
            }

            var frais = await _fraisRepository.GetByEcoleAndLibelleAsync(idEcole, libelleFrais);
            if (frais == null)
            {
                return NotFound(new { message = $"Aucun frais trouvé avec le libellé '{libelleFrais}' dans l'école {idEcole}" });
            }
            return Ok(frais);
        }

        // GET: api/Frais/direction/5
        [HttpGet("direction/{idDirection}")]
        public async Task<ActionResult<IEnumerable<Frais>>> GetFraisByDirection(int idDirection)
        {
            var frais = await _fraisRepository.GetByDirectionAsync(idDirection);
            return Ok(frais);
        }

        // GET: api/Frais/annee/2024
        [HttpGet("annee/{anneeScolaire}")]
        //public async Task<ActionResult<IEnumerable<Frais>>> GetFraisByAnnee(int anneeScolaire)
        //{
        //    var frais = await _fraisRepository.GetByAnneeAsync(anneeScolaire);
        //    return Ok(frais);
        //}

        // GET: api/Frais/statut/true
        [HttpGet("statut/{statut}")]
        //public async Task<ActionResult<IEnumerable<Frais>>> GetFraisByStatut(bool statut)
        //{
        //    var frais = await _fraisRepository.GetByStatutAsync(statut);
        //    return Ok(frais);
        //}

        // GET: api/Frais/exists/5
        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> FraisExists(int id)
        {
            var exists = await _fraisRepository.ExistsAsync(id);
            return Ok(exists);
        }

        // POST: api/Frais
        [HttpPost]
        public async Task<ActionResult<Frais>> CreateFrais(Frais frais)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdFrais = await _fraisRepository.CreateAsync(frais);
            return CreatedAtAction(nameof(GetFrais), new { id = createdFrais.IdFrais }, createdFrais);
        }

        // PUT: api/Frais/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Super-Admin")]
        public async Task<ActionResult<Frais>> UpdateFrais(int id, [FromBody] UpdateFraisDto dto)
        {
            if (id != dto.IdFrais)
            {
                return BadRequest(new { message = "L'ID ne correspond pas" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing = await _fraisRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { message = "Frais non trouvé" });
            }

            existing.LibelleFrais = dto.LibelleFrais;
            existing.Montant = dto.Montant;
            existing.Devise = dto.Devise;
            existing.TypeFrais = dto.TypeFrais;
            existing.Periodicite = dto.Periodicite;
            existing.Description = dto.Description;

            var updated = await _fraisRepository.UpdateAsync(existing);
            return Ok(updated);
        }

        // DELETE: api/Frais/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFrais(int id)
        {
            var success = await _fraisRepository.DeleteAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        // PUT: api/Frais/toggle-statut/{id}
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _fraisRepository.ToggleStatutAsync(id);
                if (!success)
                    return NotFound(new { message = "Frais non trouvé" });

                var frais = await _fraisRepository.GetByIdAsync(id);
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = frais != null,
                    frais = frais
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur", error = ex.Message });
            }
        }
    }
}
