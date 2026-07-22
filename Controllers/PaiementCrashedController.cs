using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services;
using KelasiNaBiso.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace KelasiNaBiso.Controllers
{
    /// <summary>
    /// Contrôleur pour gérer les paiements échoués lors du bulk insert Excel
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔒 Token JWT requis
    public class PaiementCrashedController : ControllerBase
    {
        private readonly PaiementCrashedService _paiementCrashedService;
        private readonly ILogger<PaiementCrashedController> _logger;

        public PaiementCrashedController(
            PaiementCrashedService paiementCrashedService,
            ILogger<PaiementCrashedController> logger)
        {
            _paiementCrashedService = paiementCrashedService;
            _logger = logger;
        }

        // GET: api/PaiementCrashed/ecole
        /// <summary>
        /// Récupère tous les paiements échoués de l'école de l'utilisateur connecté
        /// </summary>
        /// <param name="estResolu">Filtrer par statut (true = résolu, false = non résolu, null = tous)</param>
        [HttpGet("ecole")]
        [Authorize(Roles = "Admin,Super-Admin,Directeur,Financier")]
        [ProducesResponseType(typeof(IEnumerable<PaiementCrashedDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<IEnumerable<PaiementCrashedDto>>> GetAllByEcole(
            [FromQuery] bool? estResolu = null)
        {
            // Extraire l'ID de l'école depuis le token JWT
            var idEcole = this.GetCurrentUserSchoolId();
            if (!idEcole.HasValue || idEcole.Value <= 0)
            {
                return Unauthorized(new { 
                    message = "Token JWT invalide : aucune école associée à l'utilisateur"
                });
            }

            try
            {
                var paiementsCrashed = await _paiementCrashedService.GetAllByEcoleAsync(idEcole.Value, estResolu);
                return Ok(paiementsCrashed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des paiements échoués");
                return StatusCode(500, new { message = "Erreur serveur", error = ex.Message });
            }
        }

        // GET: api/PaiementCrashed/{id}
        /// <summary>
        /// Récupère un paiement échoué par son ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Super-Admin,Directeur,Financier")]
        [ProducesResponseType(typeof(PaiementCrashedDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PaiementCrashedDto>> GetById(int id)
        {
            try
            {
                var paiementCrashed = await _paiementCrashedService.GetByIdAsync(id);
                if (paiementCrashed == null)
                {
                    return NotFound(new { message = $"Paiement échoué avec l'ID {id} introuvable" });
                }

                return Ok(paiementCrashed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la récupération du paiement échoué {id}");
                return StatusCode(500, new { message = "Erreur serveur", error = ex.Message });
            }
        }

        // PUT: api/PaiementCrashed/{id}
        /// <summary>
        /// Modifie un paiement échoué
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Super-Admin,Directeur,Financier")]
        [ProducesResponseType(typeof(PaiementCrashedDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PaiementCrashedDto>> Update(int id, [FromBody] UpdatePaiementCrashedDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var paiementCrashed = await _paiementCrashedService.UpdateAsync(id, dto);
                if (paiementCrashed == null)
                {
                    return NotFound(new { message = $"Paiement échoué avec l'ID {id} introuvable" });
                }

                return Ok(paiementCrashed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la modification du paiement échoué {id}");
                return StatusCode(500, new { message = "Erreur serveur", error = ex.Message });
            }
        }

        // PUT: api/PaiementCrashed/bulk-update
        /// <summary>
        /// Modifie plusieurs paiements échoués en masse
        /// </summary>
        [HttpPut("bulk-update")]
        [Authorize(Roles = "Admin,Super-Admin,Directeur,Financier")]
        [ProducesResponseType(typeof(BulkUpdateResult), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<BulkUpdateResult>> BulkUpdate([FromBody] BulkUpdatePaiementCrashedDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (dto.Ids == null || dto.Ids.Count == 0)
            {
                return BadRequest(new { message = "La liste des IDs ne peut pas être vide" });
            }

            try
            {
                var result = await _paiementCrashedService.BulkUpdateAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la modification en masse des paiements échoués");
                return StatusCode(500, new { message = "Erreur serveur", error = ex.Message });
            }
        }

        // POST: api/PaiementCrashed/reinject
        /// <summary>
        /// Tente de réinjecter un ou plusieurs paiements échoués après validation
        /// </summary>
        [HttpPost("reinject")]
        [Authorize(Roles = "Admin,Super-Admin,Directeur,Financier")]
        [ProducesResponseType(typeof(ReinjectPaiementCrashedResult), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ReinjectPaiementCrashedResult>> Reinject([FromBody] ReinjectPaiementCrashedDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (dto.Ids == null || dto.Ids.Count == 0)
            {
                return BadRequest(new { message = "La liste des IDs ne peut pas être vide" });
            }

            // Extraire l'ID de l'utilisateur depuis le token JWT
            var idUtilisateur = this.GetCurrentUserId();
            if (idUtilisateur <= 0)
            {
                return Unauthorized(new { message = "Token JWT invalide ou utilisateur non identifié" });
            }

            try
            {
                var result = await _paiementCrashedService.ReinjectAsync(dto.Ids, idUtilisateur, dto.ForcerReinjection);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la réinjection des paiements échoués");
                return StatusCode(500, new { message = "Erreur serveur", error = ex.Message });
            }
        }

        // DELETE: api/PaiementCrashed/{id}
        /// <summary>
        /// Supprime un paiement échoué (après réinjection réussie)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Super-Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _paiementCrashedService.DeleteAsync(id);
                if (!deleted)
                {
                    return NotFound(new { message = $"Paiement échoué avec l'ID {id} introuvable" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la suppression du paiement échoué {id}");
                return StatusCode(500, new { message = "Erreur serveur", error = ex.Message });
            }
        }

        // DELETE: api/PaiementCrashed/ecole/resolved
        /// <summary>
        /// Supprime tous les paiements échoués résolus de l'école
        /// </summary>
        [HttpDelete("ecole/resolved")]
        [Authorize(Roles = "Admin,Super-Admin")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<ActionResult> DeleteResolved()
        {
            // Extraire l'ID de l'école depuis le token JWT
            var idEcole = this.GetCurrentUserSchoolId();
            if (!idEcole.HasValue || idEcole.Value <= 0)
            {
                return Unauthorized(new { 
                    message = "Token JWT invalide : aucune école associée à l'utilisateur"
                });
            }

            try
            {
                var count = await _paiementCrashedService.DeleteResolvedAsync(idEcole.Value);
                return Ok(new { 
                    message = $"{count} paiement(s) échoué(s) résolu(s) supprimé(s)",
                    count = count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression des paiements échoués résolus");
                return StatusCode(500, new { message = "Erreur serveur", error = ex.Message });
            }
        }
    }
}

