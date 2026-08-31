using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.DTOs.Pagination;
using KelasiNaBiso.Models.DTOs.Reporting;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Reporting;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Attributes;
using KelasiNaBiso.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔒 Pointages/Présences - Token JWT requis
    public class PresenceController : ControllerBase
    {
        private readonly IPresenceRepository _presenceRepository;
        private readonly IAuditService _auditService;
        private readonly IPresenceReportingService _reportingService;
        private readonly IFeuilleAppelExcelExporter _feuilleAppelExcelExporter;
        private readonly KelasiNaBisoDbContext _context;

        public PresenceController(
            IPresenceRepository presenceRepository,
            IPresenceReportingService reportingService,
            IFeuilleAppelExcelExporter feuilleAppelExcelExporter,
            IAuditService auditService,
            KelasiNaBisoDbContext context)
        {
            _presenceRepository = presenceRepository;
            _reportingService = reportingService;
            _feuilleAppelExcelExporter = feuilleAppelExcelExporter;
            _auditService = auditService;
            _context = context;
        }

        [HttpGet("paged")]
        [ProducesResponseType(typeof(ElevesAnneeScopedResult<PagedResult<Presence>>), 200)]
        public async Task<IActionResult> GetPresencesPaged(
            [FromQuery] PagedRequest request,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _presenceRepository.GetAllPagedAsync(resolvedEcole, request, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("cursor-paged")]
        [ProducesResponseType(typeof(ElevesAnneeScopedResult<CursorPaginatedResult<Presence>>), 200)]
        public async Task<IActionResult> GetPresencesCursorPaged(
            [FromQuery] CursorPaginationRequest request,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _presenceRepository.GetAllCursorPagedAsync(resolvedEcole, request, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Obsolete("Cette méthode n'est pas paginée. Utilisez GET /api/Presence/paged")]
        public async Task<IActionResult> GetPresences(
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _presenceRepository.GetAllAsync(resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/Presence/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Presence>> GetPresence(int id)
        {
            var presence = await _presenceRepository.GetByIdAsync(id);
            if (presence == null)
            {
                return NotFound();
            }
            return Ok(presence);
        }

        // ✅ GET: api/Presence/eleve/5/paged (NOUVELLE VERSION PAGINÉE)
        /// <summary>
        /// Récupère les présences d'un élève avec pagination
        /// </summary>
        [HttpGet("eleve/{idEleve}/paged")]
        [ProducesResponseType(typeof(ElevesAnneeScopedResult<PagedResult<Presence>>), 200)]
        public async Task<IActionResult> GetPresencesByElevePaged(
            int idEleve,
            [FromQuery] PagedRequest request,
            [FromQuery] int? idAnneeScolaire = null)
        {
            try
            {
                return Ok(await _presenceRepository.GetByElevePagedAsync(idEleve, request, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ✅ POINTAGE AGENT: GET: api/Presence/agent/5/paged (NOUVELLE VERSION PAGINÉE)
        /// <summary>
        /// Récupère les présences d'un agent avec pagination
        /// </summary>
        [HttpGet("agent/{idAgent}/paged")]
        [ProducesResponseType(typeof(PagedResult<Presence>), 200)]
        public async Task<ActionResult<PagedResult<Presence>>> GetPresencesByAgentPaged(
            int idAgent,
            [FromQuery] PagedRequest request)
        {
            var result = await _presenceRepository.GetByAgentPagedAsync(idAgent, request);
            return Ok(result);
        }

        // ✅ GET: api/Presence/date/2025-01-27/paged (NOUVELLE VERSION PAGINÉE)
        /// <summary>
        /// Récupère les présences d'une date spécifique avec pagination
        /// </summary>
        [HttpGet("date/{date}/paged")]
        [ProducesResponseType(typeof(PagedResult<Presence>), 200)]
        public async Task<ActionResult<PagedResult<Presence>>> GetPresencesByDatePaged(
            DateTime date,
            [FromQuery] PagedRequest request)
        {
            var result = await _presenceRepository.GetByDatePagedAsync(date, request);
            return Ok(result);
        }

        [HttpGet("date-range/paged")]
        [ProducesResponseType(typeof(ElevesAnneeScopedResult<PagedResult<Presence>>), 200)]
        public async Task<IActionResult> GetPresencesByDateRangePaged(
            [FromQuery] DateTime dateDebut,
            [FromQuery] DateTime dateFin,
            [FromQuery] PagedRequest request,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _presenceRepository.GetByDateRangePagedAsync(
                    resolvedEcole, dateDebut, dateFin, request, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ⚠️ GET: api/Presence/eleve/5 (DEPRECATED - NON PAGINÉ)
        [HttpGet("eleve/{idEleve}")]
        [Obsolete("Utiliser GET /api/Presence/eleve/{idEleve}/paged")]
        public async Task<IActionResult> GetPresencesByEleve(
            int idEleve,
            [FromQuery] int? idAnneeScolaire = null)
        {
            try
            {
                return Ok(await _presenceRepository.GetByEleveAsync(idEleve, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ⚠️ POINTAGE AGENT: GET: api/Presence/agent/5 (DEPRECATED - NON PAGINÉ)
        [HttpGet("agent/{idAgent}")]
        [Obsolete("Utiliser GET /api/Presence/agent/{idAgent}/paged")]
        public async Task<ActionResult<IEnumerable<Presence>>> GetPresencesByAgent(int idAgent)
        {
            var presences = await _presenceRepository.GetByAgentAsync(idAgent);
            return Ok(presences);
        }

        // ✅ POINTAGE AGENT: GET: api/Presence/agent/5/date/2024-01-15
        [HttpGet("agent/{idAgent}/date/{date}")]
        public async Task<ActionResult<IEnumerable<Presence>>> GetPresencesByAgentAndDate(int idAgent, DateTime date)
        {
            var presences = await _presenceRepository.GetByAgentAndDateAsync(idAgent, date);
            return Ok(presences);
        }

        // ✅ FILTRAGE PAR TYPE: GET: api/Presence/type/ELEVE
        [HttpGet("type/{typePersonne}")]
        public async Task<ActionResult<IEnumerable<Presence>>> GetPresencesByType(string typePersonne)
        {
            // Validation du type
            if (typePersonne.ToUpper() != "ELEVE" && typePersonne.ToUpper() != "AGENT")
            {
                return BadRequest(new { message = "Le type doit être 'ELEVE' ou 'AGENT'" });
            }

            var presences = await _presenceRepository.GetByTypePersonneAsync(typePersonne.ToUpper());
            return Ok(presences);
        }

        // ✅ FILTRAGE PAR TYPE: GET: api/Presence/type/ELEVE/date/2024-01-15
        [HttpGet("type/{typePersonne}/date/{date}")]
        public async Task<ActionResult<IEnumerable<Presence>>> GetPresencesByTypeAndDate(string typePersonne, DateTime date)
        {
            // Validation du type
            if (typePersonne.ToUpper() != "ELEVE" && typePersonne.ToUpper() != "AGENT")
            {
                return BadRequest(new { message = "Le type doit être 'ELEVE' ou 'AGENT'" });
            }

            var presences = await _presenceRepository.GetByTypePersonneAndDateAsync(typePersonne.ToUpper(), date);
            return Ok(presences);
        }

        // GET: api/Presence/cours/5
        [HttpGet("cours/{idCours}")]
        //public async Task<ActionResult<IEnumerable<Presence>>> GetPresencesByCours(int idCours)
        //{
        //    var presences = await _presenceRepository.GetByCoursAsync(idCours);
        //    return Ok(presences);
        //}

        // GET: api/Presence/date/2024-01-15
        [HttpGet("date/{date}")]
        public async Task<ActionResult<IEnumerable<Presence>>> GetPresencesByDate(DateTime date)
        {
            var presences = await _presenceRepository.GetByDateAsync(date);
            return Ok(presences);
        }

        // GET: api/Presence/statut/true
        [HttpGet("statut/{statut}")]
        //public async Task<ActionResult<IEnumerable<Presence>>> GetPresencesByStatut(bool statut)
        //{
        //    var presences = await _presenceRepository.GetByStatutAsync(statut);
        //    return Ok(presences);
        //}

        // GET: api/Presence/exists/5
        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> PresenceExists(int id)
        {
            var exists = await _presenceRepository.ExistsAsync(id);
            return Ok(exists);
        }

        // POST: api/Presence
        [HttpPost]
        [Permission("Presence.Create")]
        public async Task<ActionResult<Presence>> CreatePresence(CreatePresenceDto presenceDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // ✅ POINTAGE FLEXIBLE: Validation - Au moins IdEleve OU IdAgent doit être renseigné
            if (!presenceDto.IdEleve.HasValue && !presenceDto.IdAgent.HasValue)
            {
                return BadRequest(new { message = "Une présence doit concerner soit un élève, soit un agent." });
            }

            // ✅ POINTAGE FLEXIBLE: Validation - Pas les deux en même temps
            if (presenceDto.IdEleve.HasValue && presenceDto.IdAgent.HasValue)
            {
                return BadRequest(new { message = "Une présence ne peut pas concerner à la fois un élève et un agent." });
            }

            try
            {
                var presence = new Presence
                {
                    IdEleve = presenceDto.IdEleve,
                    IdAgent = presenceDto.IdAgent, // ✅ POINTAGE AGENT: Support du pointage agent
                    IsPresent = presenceDto.IsPresent, // ✅ INDICATEUR DE PRÉSENCE
                    HeureArrivee = presenceDto.GetHeureArrivee(),
                    HeureDepart = presenceDto.GetHeureDepart(),
                    DateDuJour = presenceDto.DateDuJour,
                    Observation = presenceDto.Observation, // ✅ OBSERVATION: Note sur la présence
                    Statut = true, // Par défaut, la présence est active
                    Longitute = presenceDto.Longitute ?? string.Empty,
                    Latitude = presenceDto.Latitude ?? string.Empty,
                    IdVacation = presenceDto.IdVacation
                };

                var createdPresence = await _presenceRepository.CreateAsync(presence);
                return CreatedAtAction(nameof(GetPresence), new { id = createdPresence.IdPresence }, createdPresence);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/Presence/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Super-Admin,Enseignant")]
        [Permission("Presence.Update")]
        [ProducesResponseType(typeof(Presence), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Presence>> UpdatePresence(int id, [FromBody] UpdatePresenceDto dto)
        {
            if (id != dto.IdPresence)
            {
                return BadRequest(new { message = "L'ID dans l'URL ne correspond pas à l'ID dans le corps" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingPresence = await _presenceRepository.GetByIdAsync(id);
            if (existingPresence == null)
            {
                return NotFound(new { message = "Présence non trouvée" });
            }

            // 📸 AUDIT: Capturer l'état AVANT
            var oldPresence = new Presence
            {
                IdPresence = existingPresence.IdPresence,
                IsPresent = existingPresence.IsPresent,
                HeureArrivee = existingPresence.HeureArrivee,
                HeureDepart = existingPresence.HeureDepart
            };

            // Mettre à jour seulement les champs autorisés
            existingPresence.IsPresent = dto.IsPresent;
            existingPresence.HeureArrivee = dto.HeureArrivee;
            existingPresence.HeureDepart = dto.HeureDepart;

            var updatedPresence = await _presenceRepository.UpdateAsync(existingPresence);
            if (updatedPresence == null)
            {
                return StatusCode(500, new { message = "Erreur lors de la mise à jour" });
            }

            // 📝 AUDIT
            var ctx = this.GetAuditContext();
            await _auditService.LogUpdateAsync(oldPresence, updatedPresence, ctx.UserId, ctx.UserName, ctx.UserRole, ctx.IdEcole, ctx.IpAddress, ctx.UserAgent, "Modification présence");

            return Ok(updatedPresence);
        }

        // DELETE: api/Presence/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePresence(int id)
        {
            var success = await _presenceRepository.DeleteAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        // PUT: api/Presence/toggle-statut/{id}
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _presenceRepository.ToggleStatutAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Présence non trouvée" });
                }

                // Récupérer la présence mise à jour
                var presenceAvecRelations = await _presenceRepository.GetByIdAsync(id);
                
                return Ok(new { 
                    message = "Statut modifié avec succès", 
                    presence = presenceAvecRelations,
                    nouveauStatut = presenceAvecRelations?.Statut 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors du changement de statut", error = ex.Message });
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // SECTION REPORTING ÉLÈVES
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// ✨ Obtient les pourcentages de présence d'un élève sur une période
        /// </summary>
        /// <param name="idEleve">ID de l'élève</param>
        /// <param name="dateDebut">Date de début (optionnel)</param>
        /// <param name="dateFin">Date de fin (optionnel)</param>
        /// <param name="periode">Période prédéfinie: semaine, mois, trimestre, annee (optionnel)</param>
        [HttpGet("eleves/{idEleve}/pourcentage")]
        public async Task<ActionResult> GetElevePourcentage(
            int idEleve,
            [FromQuery] DateTime? dateDebut,
            [FromQuery] DateTime? dateFin,
            [FromQuery] string? periode)
        {
            try
            {
                var result = await _reportingService.GetElevePourcentageAsync(idEleve, dateDebut, dateFin, periode);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors du calcul des pourcentages", error = ex.Message });
            }
        }

        /// <summary>
        /// ✨ Analyse les retards d'un élève sur une période
        /// </summary>
        [HttpGet("eleves/{idEleve}/retards")]
        public async Task<ActionResult> GetEleveRetards(
            int idEleve,
            [FromQuery] DateTime? dateDebut,
            [FromQuery] DateTime? dateFin,
            [FromQuery] string? periode,
            [FromQuery] string? heureReference = "08:00")
        {
            try
            {
                var heureRef = TimeSpan.Parse(heureReference);
                var result = await _reportingService.GetEleveRetardsAsync(idEleve, dateDebut, dateFin, periode, heureRef);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de l'analyse des retards", error = ex.Message });
            }
        }

        /// <summary>
        /// ✨ Obtient le reporting de présence pour une classe
        /// </summary>
        [HttpGet("eleves/classe/{idClasse}")]
        public async Task<ActionResult> GetClassePresences(
            int idClasse,
            [FromQuery] DateTime? date,
            [FromQuery] DateTime? dateDebut,
            [FromQuery] DateTime? dateFin,
            [FromQuery] string? periode,
            [FromQuery] bool includeDetails = false)
        {
            try
            {
                var result = await _reportingService.GetClassePresencesAsync(idClasse, date, dateDebut, dateFin, periode, includeDetails);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la récupération des présences de classe", error = ex.Message });
            }
        }

        /// <summary>
        /// Feuille d'appel nominative : élèves de la classe pour une date (présent / absent / retard).
        /// Date optionnelle (défaut = aujourd'hui). Année scolaire courante par défaut ; idAnneeScolaire optionnel pour une année antérieure.
        /// </summary>
        [HttpGet("eleves/classe/{idClasse}/feuille-appel")]
        [ProducesResponseType(typeof(FeuilleAppelClasseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetFeuilleAppel(
            int idClasse,
            [FromQuery] DateTime? date = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            try
            {
                var jour = (date ?? DateTime.Today).Date;
                var result = await _reportingService.GetFeuilleAppelAsync(idClasse, jour, idAnneeScolaire);

                var deny = this.ForbidIfWrongSchool(result.IdEcole);
                if (deny != null)
                    return deny;

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la récupération de la feuille d'appel", error = ex.Message });
            }
        }

        /// <summary>
        /// Export de la feuille d'appel (Excel). format=xlsx par défaut ; pdf prévu en phase 2.
        /// Date optionnelle (défaut = aujourd'hui). Année scolaire courante par défaut.
        /// </summary>
        [HttpGet("eleves/classe/{idClasse}/feuille-appel/export")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ExportFeuilleAppel(
            int idClasse,
            [FromQuery] DateTime? date = null,
            [FromQuery] int? idAnneeScolaire = null,
            [FromQuery] string format = "xlsx")
        {
            try
            {
                var normalizedFormat = (format ?? "xlsx").Trim().ToLowerInvariant();
                if (normalizedFormat == "pdf")
                {
                    return BadRequest(new
                    {
                        message = "L'export PDF n'est pas encore disponible. Utilisez format=xlsx."
                    });
                }

                if (normalizedFormat != "xlsx")
                {
                    return BadRequest(new
                    {
                        message = $"Format '{format}' non supporté. Formats acceptés : xlsx."
                    });
                }

                var jour = (date ?? DateTime.Today).Date;
                var result = await _reportingService.GetFeuilleAppelAsync(idClasse, jour, idAnneeScolaire);

                var deny = this.ForbidIfWrongSchool(result.IdEcole);
                if (deny != null)
                    return deny;

                var bytes = _feuilleAppelExcelExporter.Export(result);
                var fileName = _feuilleAppelExcelExporter.GetFileName(result);
                return File(bytes, _feuilleAppelExcelExporter.ContentType, fileName);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de l'export de la feuille d'appel", error = ex.Message });
            }
        }

        /// <summary>
        /// 📋 Obtient les présences pour une école avec pagination
        /// </summary>
        [HttpGet("ecole/{idEcole}")]
        [ProducesResponseType(typeof(ElevesAnneeScopedResult<PagedResult<Presence>>), 200)]
        public async Task<IActionResult> GetPresencesByEcole(
            int idEcole,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 15,
            [FromQuery] int? idAnneeScolaire = null)
        {
            try
            {
                var deny = this.ForbidIfWrongSchool(idEcole);
                if (deny != null)
                    return deny;

                var request = new PagedRequest
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    SortBy = "DateDuJour",
                    SortDescending = false
                };

                return Ok(await _presenceRepository.GetAllPagedAsync(idEcole, request, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la récupération des présences", error = ex.Message });
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // SECTION REPORTING AGENTS
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// ✨ Obtient les pourcentages de présence d'un agent sur une période
        /// </summary>
        [HttpGet("agents/{idAgent}/pourcentage")]
        public async Task<ActionResult> GetAgentPourcentage(
            int idAgent,
            [FromQuery] DateTime? dateDebut,
            [FromQuery] DateTime? dateFin,
            [FromQuery] string? periode)
        {
            try
            {
                var result = await _reportingService.GetAgentPourcentageAsync(idAgent, dateDebut, dateFin, periode);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors du calcul des pourcentages", error = ex.Message });
            }
        }

        /// <summary>
        /// ✨ Obtient le reporting de présence pour une fonction d'agents
        /// </summary>
        [HttpGet("agents/fonction/{fonction}")]
        public async Task<ActionResult> GetFonctionPresences(
            string fonction,
            [FromQuery] int idEcole,
            [FromQuery] DateTime? date,
            [FromQuery] DateTime? dateDebut,
            [FromQuery] DateTime? dateFin,
            [FromQuery] string? periode)
        {
            try
            {
                var result = await _reportingService.GetFonctionPresencesAsync(fonction, idEcole, date, dateDebut, dateFin, periode);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la récupération des présences par fonction", error = ex.Message });
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // SECTION DASHBOARDS
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// ✨ Obtient le dashboard de présence pour une école
        /// </summary>
        [HttpGet("dashboard/ecole/{idEcole}")]
        public async Task<ActionResult> GetDashboardEcole(
            int idEcole,
            [FromQuery] DateTime? date,
            [FromQuery] DateTime? dateDebut,
            [FromQuery] DateTime? dateFin,
            [FromQuery] int? idAnneeScolaire = null)
        {
            try
            {
                var result = await _reportingService.GetDashboardEcoleAsync(
                    idEcole, date, dateDebut, dateFin, idAnneeScolaire);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la récupération du dashboard", error = ex.Message });
            }
        }
    }
}
