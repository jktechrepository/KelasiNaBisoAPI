using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.DTOs.Pagination;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Attributes;
using KelasiNaBiso.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace KelasiNaBiso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔒 Gestion des classes - Token JWT requis
    public class ClasseController : ControllerBase
    {
        private readonly IClasseRepository _classeRepository;
        private readonly IAuditService _auditService;

        public ClasseController(IClasseRepository classeRepository, IAuditService auditService)
        {
            _classeRepository = classeRepository;
            _auditService = auditService;
        }

        // ✅ GET: api/Classe/paged (NOUVELLE VERSION PAGINÉE - RECOMMANDÉE)
        /// <summary>
        /// Récupère toutes les classes avec pagination offset-based (par pages)
        /// </summary>
        /// <param name="request">Paramètres de pagination (PageNumber, PageSize, SortBy, SearchTerm)</param>
        /// <returns>Liste paginée des classes avec métadonnées</returns>
        [HttpGet("paged")]
        [ProducesResponseType(typeof(PagedResult<Classe>), 200)]
        public async Task<ActionResult<PagedResult<Classe>>> GetClassesPaged([FromQuery] PagedRequest request)
        {
            var result = await _classeRepository.GetAllPagedAsync(request);
            return Ok(result);
        }

        // ⚠️ GET: api/Classe (DEPRECATED - À ÉVITER, NON PAGINÉ)
        /// <summary>
        /// [DEPRECATED] Récupère TOUTES les classes sans pagination
        /// ⚠️ ATTENTION: Peut causer des problèmes de performance avec beaucoup de classes
        /// Utiliser plutôt GET /api/Classe/paged
        /// </summary>
        [HttpGet]
        [Obsolete("Cette méthode n'est pas paginée et peut causer des problèmes de performance. Utilisez GET /api/Classe/paged")]
        [RequireGlobalAccess]
        public async Task<ActionResult<IEnumerable<Classe>>> GetClasses()
        {
            var classes = await _classeRepository.GetAllAsync();
            return Ok(classes);
        }

        // GET: api/Classe/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Classe>> GetClasse(int id)
        {
            var classe = await _classeRepository.GetByIdAsync(id);
            if (classe == null)
            {
                return NotFound();
            }
            return Ok(classe);
        }

        // GET: api/Classe/nom/{nom}
        [HttpGet("nom/{nom}")]
        public async Task<ActionResult<Classe>> GetClasseByNom(string nom)
        {
            var classe = await _classeRepository.GetByNomAsync(nom);
            if (classe == null)
            {
                return NotFound();
            }
            return Ok(classe);
        }

        // GET: api/Classe/ecole/5
        [HttpGet("ecole/{idEcole}")]
        public async Task<ActionResult<IEnumerable<Classe>>> GetClassesByEcole(int idEcole)
        {
            var classes = await _classeRepository.GetByEcoleAsync(idEcole);
            return Ok(classes);
        }

        // GET: api/Classe/ecole/{idEcole}/nomClasse?nomClasse=1ère Primaire
        /// <summary>
        /// Récupère une classe par son nom dans une école spécifique
        /// </summary>
        [HttpGet("ecole/{idEcole}/nomClasse")]
        public async Task<ActionResult<Classe>> GetClasseByEcoleAndNom(int idEcole, [FromQuery] string nomClasse)
        {
            if (string.IsNullOrWhiteSpace(nomClasse))
            {
                return BadRequest(new { message = "Le paramètre nomClasse est requis" });
            }

            var classe = await _classeRepository.GetByEcoleAndNomAsync(idEcole, nomClasse);
            if (classe == null)
            {
                return NotFound(new { message = $"Aucune classe trouvée avec le nom '{nomClasse}' dans l'école {idEcole}" });
            }
            return Ok(classe);
        }

        // GET: api/Classe/section/5
        [HttpGet("section/{idSection}")]
        public async Task<ActionResult<IEnumerable<Classe>>> GetClassesBySection(int idSection)
        {
            var classes = await _classeRepository.GetBySectionAsync(idSection);
            return Ok(classes);
        }

        // GET: api/Classe/option/5
        [HttpGet("option/{idOption}")]
        public async Task<ActionResult<IEnumerable<Classe>>> GetClassesByOption(int idOption)
        {
            var classes = await _classeRepository.GetByOptionAsync(idOption);
            return Ok(classes);
        }

        // GET: api/Classe/sans-section
        [HttpGet("sans-section")]
        public async Task<ActionResult<IEnumerable<Classe>>> GetClassesWithoutSection()
        {
            var classes = await _classeRepository.GetWithoutSectionAsync();
            return Ok(classes);
        }

        // GET: api/Classe/sans-option
        [HttpGet("sans-option")]
        public async Task<ActionResult<IEnumerable<Classe>>> GetClassesWithoutOption()
        {
            var classes = await _classeRepository.GetWithoutOptionAsync();
            return Ok(classes);
        }

        // GET: api/Classe/statut/{statut}
        [HttpGet("statut/{statut}")]
        //public async Task<ActionResult<IEnumerable<Classe>>> GetClassesByStatut(bool statut)
        //{
        //    var classes = await _classeRepository.GetByStatutAsync(statut);
        //    return Ok(classes);
        //}

        // GET: api/Classe/5/eleves?idAnneeScolaire=
        [HttpGet("{id}/eleves")]
        [ProducesResponseType(typeof(ElevesAnneeScopedResult<IEnumerable<Eleve>>), 200)]
        public async Task<IActionResult> GetClasseEleves(int id, [FromQuery] int? idAnneeScolaire = null)
        {
            try
            {
                return Ok(await _classeRepository.GetElevesAsync(id, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/Classe/5/cours
        [HttpGet("{id}/cours")]
        public async Task<ActionResult<IEnumerable<Cours>>> GetClasseCours(int id)
        {
            var cours = await _classeRepository.GetCoursAsync(id);
            return Ok(cours);
        }

        // GET: api/Classe/5/inscriptions?idAnneeScolaire=
        [HttpGet("{id}/inscriptions")]
        [ProducesResponseType(typeof(ElevesAnneeScopedResult<IEnumerable<Inscription>>), 200)]
        public async Task<IActionResult> GetClasseInscriptions(int id, [FromQuery] int? idAnneeScolaire = null)
        {
            try
            {
                return Ok(await _classeRepository.GetInscriptionsAsync(id, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //// GET: api/Classe/5/frais
        //[HttpGet("{id}/frais")]
        //public async Task<ActionResult<IEnumerable<Frais>>> GetClasseFrais(int id)
        //{
        //    var frais = await _classeRepository.GetFraisAsync(id);
        //    return Ok(frais);
        //}


        // GET: api/Classe/5/evaluations
        [HttpGet("{id}/evaluations")]
        public async Task<ActionResult<IEnumerable<Evaluation>>> GetClasseEvaluations(int id)
        {
            var evaluations = await _classeRepository.GetEvaluationsAsync(id);
            return Ok(evaluations);
        }

        // POST: api/Classe
        [HttpPost]
        public async Task<ActionResult<Classe>> CreateClasse(Classe classe)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdClasse = await _classeRepository.CreateAsync(classe);
            return CreatedAtAction(nameof(GetClasse), new { id = createdClasse.IdClasse }, createdClasse);
        }

        // PUT: api/Classe/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Super-Admin")]
        [ProducesResponseType(typeof(Classe), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Classe>> UpdateClasse(int id, [FromBody] UpdateClasseDto dto)
        {
            if (id != dto.IdClasse)
            {
                return BadRequest(new { message = "L'ID dans l'URL ne correspond pas à l'ID dans le corps" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingClasse = await _classeRepository.GetByIdAsync(id);
            if (existingClasse == null)
            {
                return NotFound(new { message = "Classe non trouvée" });
            }

            // 📸 AUDIT: Snapshot AVANT
            var oldClasse = new Classe
            {
                IdClasse = existingClasse.IdClasse,
                NomClasse = existingClasse.NomClasse,
                IdDirection = existingClasse.IdDirection,
                IdSection = existingClasse.IdSection,
                IdOption = existingClasse.IdOption
            };

            // Mettre à jour seulement les champs autorisés
            existingClasse.NomClasse = dto.NomClasse;
            existingClasse.IdDirection = dto.IdDirection;
            existingClasse.IdSection = dto.IdSection;
            existingClasse.IdOption = dto.IdOption;

            var updatedClasse = await _classeRepository.UpdateAsync(existingClasse);
            if (updatedClasse == null)
            {
                return StatusCode(500, new { message = "Erreur lors de la mise à jour" });
            }

            // 📝 AUDIT
            var ctx = this.GetAuditContext();
            await _auditService.LogUpdateAsync(oldClasse, updatedClasse, ctx.UserId, ctx.UserName, ctx.UserRole, ctx.IdEcole, ctx.IpAddress, ctx.UserAgent, "Modification classe");

            return Ok(updatedClasse);
        }

        // DELETE: api/Classe/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClasse(int id)
        {
            var exists = await _classeRepository.ExistsAsync(id);
            if (!exists)
            {
                return NotFound();
            }

            await _classeRepository.DeleteAsync(id);
            return NoContent();
        }

        // PUT: api/Classe/toggle-statut/{id}
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _classeRepository.ToggleStatutAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Classe non trouvée" });
                }

                var classe = await _classeRepository.GetByIdAsync(id);
                var estActif = classe != null;
                
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = estActif,
                    classe = classe
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors du changement de statut", error = ex.Message });
            }
        }
    }
}
