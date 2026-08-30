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
    [Authorize] // 🔒 Années scolaires - Token JWT requis
    public class AnneeScolaireController : ControllerBase
    {
        private readonly IAnneeScolaireRepository _anneeScolaireRepository;

        public AnneeScolaireController(IAnneeScolaireRepository anneeScolaireRepository)
        {
            _anneeScolaireRepository = anneeScolaireRepository;
        }

        // GET: api/AnneeScolaire
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AnneeScolaire>>> GetAnneeScolaires()
        {
            var anneeScolaires = await _anneeScolaireRepository.GetAllAsync();
            return Ok(anneeScolaires);
        }

        // GET: api/AnneeScolaire/5
        [HttpGet("ecole/{idEcole}")]
        public async Task<ActionResult<IEnumerable<AnneeScolaire>>> GetAnneeScolaireByEcole(int idEcole)
        {
            var annees = await _anneeScolaireRepository.GetAllAsync();
            var anneesByEcole = annees.Where(a => a.IdEcole == idEcole).ToList();
            return Ok(anneesByEcole);
        }

        // GET: api/AnneeScolaire/ecole/{idEcole}/libelle?libelleAnneeScolaire=2024-2025
        /// <summary>
        /// Récupère une année scolaire par son libellé dans une école spécifique
        /// </summary>
        [HttpGet("ecole/{idEcole}/libelle")]
        public async Task<ActionResult<AnneeScolaire>> GetAnneeScolaireByEcoleAndLibelle(int idEcole, [FromQuery] string libelleAnneeScolaire)
        {
            if (string.IsNullOrWhiteSpace(libelleAnneeScolaire))
            {
                return BadRequest(new { message = "Le paramètre libelleAnneeScolaire est requis" });
            }

            var anneeScolaire = await _anneeScolaireRepository.GetByEcoleAndLibelleAsync(idEcole, libelleAnneeScolaire);
            if (anneeScolaire == null)
            {
                return NotFound(new { message = $"Aucune année scolaire trouvée avec le libellé '{libelleAnneeScolaire}' dans l'école {idEcole}" });
            }
            return Ok(anneeScolaire);
        }

        // GET: api/AnneeScolaire/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AnneeScolaire>> GetAnneeScolaire(int id)
        {
            var anneeScolaire = await _anneeScolaireRepository.GetByIdAsync(id);
            if (anneeScolaire == null)
            {
                return NotFound();
            }
            return Ok(anneeScolaire);
        }

        // GET: api/AnneeScolaire/annee/2024
        [HttpGet("annee/{annee}")]
        //public async Task<ActionResult<AnneeScolaire>> GetAnneeScolaireByAnnee(int annee)
        //{
        //    var anneeScolaire = await _anneeScolaireRepository.GetByAnneeAsync(annee);
        //    if (anneeScolaire == null)
        //    {
        //        return NotFound();
        //    }
        //    return Ok(anneeScolaire);
        //}

        // GET: api/AnneeScolaire/actuelle
        [HttpGet("actuelle")]
        //public async Task<ActionResult<AnneeScolaire>> GetAnneeScolaireActuelle()
        //{
        //    var anneeScolaire = await _anneeScolaireRepository.GetActuelleAsync();
        //    if (anneeScolaire == null)
        //    {
        //        return NotFound();
        //    }
        //    return Ok(anneeScolaire);
        //}

        // GET: api/AnneeScolaire/statut/true
        [HttpGet("statut/{statut}")]
        //public async Task<ActionResult<IEnumerable<AnneeScolaire>>> GetAnneeScolairesByStatut(bool statut)
        //{
        //    var anneeScolaires = await _anneeScolaireRepository.GetByStatutAsync(statut);
        //    return Ok(anneeScolaires);
        //}

        // GET: api/AnneeScolaire/exists/5
        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> AnneeScolaireExists(int id)
        {
            var exists = await _anneeScolaireRepository.ExistsAsync(id);
            return Ok(exists);
        }

        // GET: api/AnneeScolaire/exists/annee/2024
        [HttpGet("exists/annee/{annee}")]
        //public async Task<ActionResult<bool>> AnneeScolaireExistsByAnnee(int annee)
        //{
        //    var exists = await _anneeScolaireRepository.ExistsByAnneeAsync(annee);
        //    return Ok(exists);
        //}

        // POST: api/AnneeScolaire
        [HttpPost]
        public async Task<ActionResult<AnneeScolaire>> CreateAnneeScolaire(AnneeScolaire anneeScolaire)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdAnneeScolaire = await _anneeScolaireRepository.CreateAsync(anneeScolaire);
            return CreatedAtAction(nameof(GetAnneeScolaire), new { id = createdAnneeScolaire.IdAnneeScolaire }, createdAnneeScolaire);
        }

        // PUT: api/AnneeScolaire/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Super-Admin")]
        public async Task<ActionResult<AnneeScolaire>> UpdateAnneeScolaire(int id, [FromBody] UpdateAnneeScolaireDto dto)
        {
            if (id != dto.IdAnneeScolaire)
            {
                return BadRequest(new { message = "L'ID ne correspond pas" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing = await _anneeScolaireRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { message = "Année scolaire non trouvée" });
            }

            existing.LibelleAnneeScolaire = dto.LibelleAnneeScolaire;
            existing.DateDebut = dto.DateDebut;
            existing.DateFin = dto.DateFin;

            var updated = await _anneeScolaireRepository.UpdateAsync(existing);
            return Ok(updated);
        }

        // DELETE: api/AnneeScolaire/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnneeScolaire(int id)
        {
            var success = await _anneeScolaireRepository.DeleteAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        // PUT: api/AnneeScolaire/toggle-statut/{id}
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _anneeScolaireRepository.ToggleStatutAsync(id);
                if (!success)
                    return NotFound(new { message = "Année scolaire non trouvée" });

                var annee = await _anneeScolaireRepository.GetByIdAsync(id);
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = annee != null,
                    anneeScolaire = annee
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur", error = ex.Message });
            }
        }
    }
}
