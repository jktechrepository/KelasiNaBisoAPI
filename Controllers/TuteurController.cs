using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Attributes;
using KelasiNaBiso.Services;
using KelasiNaBiso.Models.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;

namespace KelasiNaBiso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔒 Données sensibles - Token JWT requis
    public class TuteurController : ControllerBase
    {
        private readonly ITuteurRepository _tuteurRepository;
        private readonly ICurrentUserService _currentUserService;

        public TuteurController(
            ITuteurRepository tuteurRepository,
            ICurrentUserService currentUserService)
        {
            _tuteurRepository = tuteurRepository;
            _currentUserService = currentUserService;
        }

        // GET: api/Tuteur
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tuteur>>> GetTuteurs()
        {
            var tuteurs = await _tuteurRepository.GetAllAsync();
            return Ok(tuteurs);
        }

        // GET: api/Tuteur/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Tuteur>> GetTuteur(int id)
        {
            var tuteur = await _tuteurRepository.GetByIdAsync(id);
            if (tuteur == null)
            {
                return NotFound();
            }
            return Ok(tuteur);
        }

        // GET: api/Tuteur/ecole/5
        [HttpGet("ecole/{idEcole}")]
        public async Task<ActionResult<IEnumerable<Tuteur>>> GetTuteursByEcole(int idEcole)
        {
            var tuteurs = await _tuteurRepository.GetByEcoleAsync(idEcole);
            return Ok(tuteurs);
        }

        // GET: api/Tuteur/5/eleves
        [HttpGet("{id}/eleves")]
        public async Task<ActionResult<IEnumerable<Eleve>>> GetTuteurEleves(int id)
        {
            var eleves = await _tuteurRepository.GetElevesAsync(id);
            return Ok(eleves);
        }

        // POST: api/Tuteur
        //////[HttpPost]
        //////public async Task<ActionResult<Tuteur>> CreateTuteur(Tuteur tuteur)
        //////{
        //////    if (!ModelState.IsValid)
        //////    {
        //////        return BadRequest(ModelState);
        //////    }

        //////    var createdTuteur = await _tuteurRepository.CreateAsync(tuteur);
        //////    return CreatedAtAction(nameof(GetTuteur), new { id = createdTuteur.IdTuteur }, createdTuteur);
        //////}

        // PUT: api/Tuteur/5
        /// <summary>
        /// Modifier les informations d'un tuteur (Admin uniquement)
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Tuteur), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Tuteur>> UpdateTuteur(int id, [FromBody] UpdateTuteurDto dto)
        {
            if (id != dto.IdTuteur)
            {
                return BadRequest(new { message = "L'ID dans l'URL ne correspond pas à l'ID dans le corps" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingTuteur = await _tuteurRepository.GetByIdAsync(id);
            if (existingTuteur == null)
            {
                return NotFound(new { message = "Tuteur non trouvé" });
            }

            var currentRole = _currentUserService.UserRole;
            var isSuperAdmin = _currentUserService.IsSuperAdmin;
            var currentTuteurId = _currentUserService.TuteurId;
            var currentEcoleId = _currentUserService.EcoleId;
            var isSelf = currentTuteurId.HasValue && currentTuteurId.Value == existingTuteur.IdTuteur;

            if (!isSuperAdmin)
            {
                if (existingTuteur.Statut != true)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "Seul un Super-Admin peut modifier un tuteur désactivé." });
                }

                if (!isSelf)
                {
                    if (string.Equals(currentRole, UserRoles.ADMIN, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(currentRole, UserRoles.DIRECTEUR, StringComparison.OrdinalIgnoreCase))
                    {
                        if (currentEcoleId == 0 || existingTuteur.IdEcole != currentEcoleId)
                        {
                            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Vous ne pouvez modifier que les tuteurs de votre école." });
                        }
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status403Forbidden, new { message = "Vous n'êtes pas autorisé à modifier ce profil." });
                    }
                }
            }

            // Mettre à jour seulement les champs autorisés
            existingTuteur.NomComplet = dto.NomComplet;
            existingTuteur.Email = dto.Email;
            existingTuteur.Telephone = dto.Telephone;
            existingTuteur.Genre = dto.Genre;
            existingTuteur.PhotoTuteurUrl = dto.PhotoTuteurUrl;
            existingTuteur.PieceIdentiteTuteur = dto.PieceIdentiteTuteur;
            existingTuteur.NomCompletRepresentant = dto.NomCompletRepresentant;
            existingTuteur.TelephoneRepresentant = dto.TelephoneRepresentant;

            Tuteur? updatedTuteur;
            try
            {
                updatedTuteur = await _tuteurRepository.UpdateAsync(existingTuteur);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            if (updatedTuteur == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erreur lors de la mise à jour" });
            }

            return Ok(updatedTuteur);
        }

        // DELETE: api/Tuteur/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTuteur(int id)
        {
            var exists = await _tuteurRepository.ExistsAsync(id);
            if (!exists)
            {
                return NotFound();
            }

            await _tuteurRepository.DeleteAsync(id);
            return NoContent();
        }

        // PUT: api/Tuteur/toggle-statut/{id}
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _tuteurRepository.ToggleStatutAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Tuteur non trouvé" });
                }

                var tuteur = await _tuteurRepository.GetByIdAsync(id);
                var estActif = tuteur != null;
                
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = estActif,
                    tuteur = tuteur
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors du changement de statut", error = ex.Message });
            }
        }
    }
}
