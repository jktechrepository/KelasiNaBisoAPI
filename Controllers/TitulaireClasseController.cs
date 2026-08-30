using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.DTOs.Pagination;
using KelasiNaBiso.Services.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace KelasiNaBiso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔒 Titulaires - Token JWT requis
    public class TitulaireClasseController : ControllerBase
    {
        private readonly ITitulaireClasseRepository _titulaireRepository;
        private readonly ILogger<TitulaireClasseController> _logger;

        public TitulaireClasseController(
            ITitulaireClasseRepository titulaireRepository,
            ILogger<TitulaireClasseController> logger)
        {
            _titulaireRepository = titulaireRepository;
            _logger = logger;
        }

        // ============================================
        // ENDPOINTS PAGINÉS
        // ============================================

        /// <summary>
        /// Récupère tous les titulaires de classe avec pagination
        /// </summary>
        [HttpGet("paged")]
        [ProducesResponseType(typeof(PagedResult<TitulaireClasse>), 200)]
        public async Task<ActionResult<PagedResult<TitulaireClasse>>> GetTitulairesClassesPaged(
            [FromQuery] PagedRequest request)
        {
            var result = await _titulaireRepository.GetAllPagedAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Récupère les classes titularisées par un agent spécifique (paginé)
        /// </summary>
        [HttpGet("agent/{idAgent}/paged")]
        [ProducesResponseType(typeof(PagedResult<TitulaireClasse>), 200)]
        public async Task<ActionResult<PagedResult<TitulaireClasse>>> GetTitulairesByAgentPaged(
            int idAgent,
            [FromQuery] PagedRequest request)
        {
            var result = await _titulaireRepository.GetByAgentPagedAsync(idAgent, request);
            return Ok(result);
        }

        /// <summary>
        /// Récupère l'historique des titulaires d'une classe (paginé)
        /// </summary>
        [HttpGet("classe/{idClasse}/paged")]
        [ProducesResponseType(typeof(PagedResult<TitulaireClasse>), 200)]
        public async Task<ActionResult<PagedResult<TitulaireClasse>>> GetTitulairesByClassePaged(
            int idClasse,
            [FromQuery] PagedRequest request)
        {
            var result = await _titulaireRepository.GetByClassePagedAsync(idClasse, request);
            return Ok(result);
        }

        /// <summary>
        /// Récupère tous les titulaires d'une année scolaire (paginé)
        /// </summary>
        [HttpGet("annee/{idAnneeScolaire}/paged")]
        [ProducesResponseType(typeof(PagedResult<TitulaireClasse>), 200)]
        public async Task<ActionResult<PagedResult<TitulaireClasse>>> GetTitulairesByAnneePaged(
            int idAnneeScolaire,
            [FromQuery] PagedRequest request)
        {
            var result = await _titulaireRepository.GetByAnneeScolairePagedAsync(idAnneeScolaire, request);
            return Ok(result);
        }

        // ============================================
        // ENDPOINTS CRUD CLASSIQUES
        // ============================================

        /// <summary>
        /// Récupère tous les titulaires de classe (⚠️ non paginé)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TitulaireClasse>>> GetTitulairesClasses()
        {
            var titulaires = await _titulaireRepository.GetAllAsync();
            return Ok(titulaires);
        }

        /// <summary>
        /// Récupère un titulaire de classe par son ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<TitulaireClasse>> GetTitulaireClasse(int id)
        {
            var titulaire = await _titulaireRepository.GetByIdAsync(id);
            if (titulaire == null)
            {
                return NotFound(new { message = $"Titulaire classe {id} introuvable" });
            }
            return Ok(titulaire);
        }

        /// <summary>
        /// Récupère le titulaire actif d'une classe pour une année scolaire
        /// </summary>
        [HttpGet("classe/{idClasse}/annee/{idAnneeScolaire}/actif")]
        public async Task<ActionResult<TitulaireClasse>> GetTitulaireActif(int idClasse, int idAnneeScolaire)
        {
            var titulaire = await _titulaireRepository.GetTitulaireActifByClasseAsync(idClasse, idAnneeScolaire);
            if (titulaire == null)
            {
                return NotFound(new
                {
                    message = $"Aucun titulaire actif trouvé pour la classe {idClasse} " +
                              $"durant l'année scolaire {idAnneeScolaire}"
                });
            }
            return Ok(titulaire);
        }

        /// <summary>
        /// Récupère les classes titularisées par un agent
        /// </summary>
        [HttpGet("agent/{idAgent}")]
        public async Task<ActionResult<IEnumerable<TitulaireClasse>>> GetTitulairesByAgent(int idAgent)
        {
            var titulaires = await _titulaireRepository.GetByAgentAsync(idAgent);
            return Ok(titulaires);
        }

        /// <summary>
        /// Récupère l'historique des titulaires d'une classe
        /// </summary>
        [HttpGet("classe/{idClasse}")]
        public async Task<ActionResult<IEnumerable<TitulaireClasse>>> GetTitulairesByClasse(int idClasse)
        {
            var titulaires = await _titulaireRepository.GetByClasseAsync(idClasse);
            return Ok(titulaires);
        }

        /// <summary>
        /// Crée un nouveau titulaire de classe (Maternelle/Primaire)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<TitulaireClasse>> CreateTitulaireClasse(
            [FromBody] TitulaireClasse titulaireClasse)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var created = await _titulaireRepository.CreateAsync(titulaireClasse);
                return CreatedAtAction(
                    nameof(GetTitulaireClasse),
                    new { id = created.IdTitulaireClasse },
                    created);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Erreur création titulaire : {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création d'un titulaire de classe");
                return StatusCode(500, new
                {
                    message = "Une erreur est survenue lors de la création du titulaire",
                    detail = ex.Message
                });
            }
        }

        /// <summary>
        /// Met à jour un titulaire de classe
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Super-Admin")]
        public async Task<ActionResult<TitulaireClasse>> UpdateTitulaireClasse(
            int id,
            [FromBody] UpdateTitulaireClasseDto dto)
        {
            if (id != dto.IdTitulaireClasse)
            {
                return BadRequest(new { message = "L'ID dans l'URL ne correspond pas à l'ID du titulaire" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var existing = await _titulaireRepository.GetByIdAsync(id);
                if (existing == null)
                {
                    return NotFound(new { message = "Titulaire non trouvé" });
                }

                existing.IdAgent = dto.IdAgent;
                existing.IdClasse = dto.IdClasse;
                existing.IdAnneeScolaire = dto.IdAnneeScolaire;
                if (dto.DateDebut.HasValue)
                    existing.DateDebut = dto.DateDebut.Value;
                existing.DateFin = dto.DateFin;
                existing.Statut = dto.Statut;
                existing.Commentaire = dto.Commentaire;

                var updated = await _titulaireRepository.UpdateAsync(existing);
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Erreur mise à jour titulaire : {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la mise à jour du titulaire {id}");
                return StatusCode(500, new
                {
                    message = "Une erreur est survenue lors de la mise à jour",
                    detail = ex.Message
                });
            }
        }

        /// <summary>
        /// Supprime un titulaire de classe (hard delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTitulaireClasse(int id)
        {
            var success = await _titulaireRepository.DeleteAsync(id);
            if (!success)
            {
                return NotFound(new { message = $"Titulaire classe {id} introuvable" });
            }

            return NoContent();
        }

        /// <summary>
        /// Active/Désactive un titulaire de classe (soft delete)
        /// </summary>
        [HttpPatch("{id}/toggle-statut")]
        public async Task<ActionResult> ToggleStatut(int id)
        {
            var success = await _titulaireRepository.ToggleStatutAsync(id);
            if (!success)
            {
                return NotFound(new { message = $"Titulaire classe {id} introuvable" });
            }

            var titulaire = await _titulaireRepository.GetByIdAsync(id);
            return Ok(new
            {
                message = titulaire.Statut == true
                    ? "Titulaire réactivé avec succès"
                    : "Titulaire désactivé avec succès",
                statut = titulaire.Statut
            });
        }

        // ============================================
        // ENDPOINTS DE VALIDATION
        // ============================================

        /// <summary>
        /// Vérifie si une classe a déjà un titulaire actif pour une année scolaire
        /// </summary>
        [HttpGet("classe/{idClasse}/annee/{idAnneeScolaire}/has-titulaire")]
        public async Task<ActionResult<bool>> HasTitulaireActif(int idClasse, int idAnneeScolaire)
        {
            var hasTitulaire = await _titulaireRepository.HasTitulaireActifAsync(idClasse, idAnneeScolaire);
            return Ok(new
            {
                hasTitulaire,
                message = hasTitulaire
                    ? "Cette classe a déjà un titulaire actif"
                    : "Cette classe n'a pas de titulaire actif"
            });
        }

        /// <summary>
        /// Vérifie si un agent est disponible pour être titulaire (pas déjà titulaire d'une autre classe)
        /// </summary>
        [HttpGet("agent/{idAgent}/annee/{idAnneeScolaire}/disponible")]
        public async Task<ActionResult<bool>> AgentEstDisponible(int idAgent, int idAnneeScolaire)
        {
            var estDisponible = await _titulaireRepository.AgentEstDisponibleAsync(idAgent, idAnneeScolaire);
            return Ok(new
            {
                estDisponible,
                message = estDisponible
                    ? "L'agent est disponible pour être titulaire"
                    : "L'agent est déjà titulaire d'une autre classe pour cette année"
            });
        }
    }
}

