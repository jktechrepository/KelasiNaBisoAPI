using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Attributes;
using KelasiNaBiso.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace KelasiNaBiso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NoteController : ControllerBase
    {
        private readonly INoteRepository _noteRepository;
        private readonly IAuditService _auditService;
        private readonly IPedagogieAuthorizationService _pedagogie;

        public NoteController(
            INoteRepository noteRepository,
            IAuditService auditService,
            IPedagogieAuthorizationService pedagogie)
        {
            _noteRepository = noteRepository;
            _auditService = auditService;
            _pedagogie = pedagogie;
        }

        [HttpGet]
        [Permission("Note.ReadAll")]
        public async Task<ActionResult<IEnumerable<Note>>> GetNotes()
        {
            var notes = await _noteRepository.GetAllAsync();
            return Ok(await FilterNotesForEnseignantAsync(notes));
        }

        [HttpGet("{id}")]
        [Permission("Note.Read")]
        public async Task<IActionResult> GetNote(int id)
        {
            var note = await _noteRepository.GetByIdAsync(id);
            if (note == null)
                return NotFound();

            var deny = await this.ForbidIfHorsScopeNoteAsync(id);
            if (deny != null)
                return deny;

            return Ok(note);
        }

        [HttpGet("eleve/{idEleve}")]
        [Permission("Note.Read", "Note.ReadOwn", "Note.ReadChildren")]
        public async Task<IActionResult> GetNotesByEleve(int idEleve)
        {
            var denyOwn = this.ForbidIfWrongEleve(idEleve);
            if (denyOwn != null)
                return denyOwn;

            var denyChild = await this.ForbidIfWrongChildAsync(idEleve);
            if (denyChild != null)
                return denyChild;

            // Enseignant : seulement notes des évaluations de ses cours
            if (User.IsInRole(UserRoles.ENSEIGNANT) && !this.IsCotationSchoolBypassRole())
            {
                var notes = await _noteRepository.GetByEleveAsync(idEleve);
                return Ok(await FilterNotesForEnseignantAsync(notes));
            }

            var all = await _noteRepository.GetByEleveAsync(idEleve);
            return Ok(all);
        }

        [HttpGet("evaluation/{idEvaluation}")]
        [Permission("Note.Read")]
        [ProducesResponseType(typeof(IEnumerable<Note>), 200)]
        public async Task<IActionResult> GetNotesByEvaluation(int idEvaluation)
        {
            var deny = await this.ForbidIfHorsScopeEvaluationAsync(idEvaluation);
            if (deny != null)
                return deny;

            var notes = await _noteRepository.GetByEvaluationAsync(idEvaluation);
            return Ok(notes);
        }

        [HttpGet("cours/{idCours}")]
        [Permission("Note.Read")]
        [Obsolete("Utiliser GET /api/Note/evaluation/{idEvaluation} pour une meilleure précision")]
        public async Task<IActionResult> GetNotesByCours(int idCours)
        {
            var deny = await this.ForbidIfHorsScopeCoursAsync(idCours);
            if (deny != null)
                return deny;

            var notes = await _noteRepository.GetByCoursAsync(idCours);
            return Ok(notes);
        }

        [HttpGet("professeur/{idProfesseur}")]
        [Permission("Note.Read")]
        public async Task<IActionResult> GetNotesByProfesseur(int idProfesseur)
        {
            if (User.IsInRole(UserRoles.ENSEIGNANT) && !this.IsCotationSchoolBypassRole())
            {
                var idAgent = this.GetCurrentAgentId();
                if (!idAgent.HasValue || idAgent.Value != idProfesseur)
                {
                    return StatusCode(StatusCodes.Status403Forbidden,
                        new { message = "Accès refusé : vous ne pouvez consulter que vos propres notes enseignant." });
                }
            }

            var notes = await _noteRepository.GetByProfesseurAsync(idProfesseur);
            return Ok(notes);
        }

        [HttpGet("annee/{idAnneeScolaire}")]
        [Permission("Note.Read")]
        public async Task<ActionResult<IEnumerable<Note>>> GetNotesByAnneeScolaire(int idAnneeScolaire)
        {
            var notes = await _noteRepository.GetByAnneeScolaireAsync(idAnneeScolaire);
            return Ok(await FilterNotesForEnseignantAsync(notes));
        }

        [HttpGet("periode/{periode}")]
        [Permission("Note.Read")]
        [ProducesResponseType(typeof(IEnumerable<Note>), 200)]
        public async Task<ActionResult<IEnumerable<Note>>> GetNotesByPeriode(string periode)
        {
            var notes = await _noteRepository.GetByPeriodeAsync(periode);
            return Ok(await FilterNotesForEnseignantAsync(notes));
        }

        [HttpGet("session/{session}")]
        [Permission("Note.Read")]
        [Obsolete("Utiliser GET /api/Note/periode/{periode}")]
        public async Task<ActionResult<IEnumerable<Note>>> GetNotesBySession(string session)
        {
            var notes = await _noteRepository.GetByPeriodeAsync(session);
            return Ok(await FilterNotesForEnseignantAsync(notes));
        }

        [HttpPost("bulk")]
        [Permission("Note.Create", "Note.Update")]
        [ProducesResponseType(typeof(BulkNoteResultDto), 200)]
        public async Task<IActionResult> BulkUpsertNotes([FromBody] BulkNoteRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var deny = await this.ForbidIfHorsScopeEvaluationAsync(
                dto.IdEvaluation,
                dto.IdAnneeScolaire > 0 ? dto.IdAnneeScolaire : null);
            if (deny != null)
                return deny;

            var idProfesseur = this.GetCurrentUserId();
            if (idProfesseur <= 0)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "Accès refusé : utilisateur non identifié." });
            }

            try
            {
                var result = await _noteRepository.UpsertBulkAsync(dto, idProfesseur);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Permission("Note.Create")]
        public async Task<IActionResult> CreateNote([FromBody] CreateNoteDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var deny = await this.ForbidIfHorsScopeEvaluationAsync(dto.IdEvaluation, dto.IdAnneeScolaire);
            if (deny != null)
                return deny;

            // IdProfesseur = IdUtilisateur JWT (FK Notes → Utilisateurs)
            var idProfesseur = dto.IdProfesseur;
            if (User.IsInRole(UserRoles.ENSEIGNANT) && !this.IsCotationSchoolBypassRole())
            {
                var userId = this.GetCurrentUserId();
                if (userId <= 0)
                {
                    return StatusCode(StatusCodes.Status403Forbidden,
                        new { message = "Accès refusé : utilisateur non identifié." });
                }

                idProfesseur = userId;
            }

            var note = new Note
            {
                NoteObtenue = dto.NoteObtenue,
                Appreciation = dto.Appreciation,
                DateEvaluation = dto.DateEvaluation ?? DateTime.Now,
                IdProfesseur = idProfesseur,
                IdEleve = dto.IdEleve,
                IdEvaluation = dto.IdEvaluation,
                IdAnneeScolaire = dto.IdAnneeScolaire,
                Statut = true
            };

            try
            {
                var createdNote = await _noteRepository.CreateAsync(note);
                return CreatedAtAction(nameof(GetNote), new { id = createdNote.IdNote }, createdNote);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Permission("Note.Update")]
        [Authorize(Roles = "Admin,Super-Admin,Enseignant,Directeur,Sous-Directeur")]
        [ProducesResponseType(typeof(Note), 200)]
        public async Task<IActionResult> UpdateNote(int id, [FromBody] UpdateNoteDto dto)
        {
            if (id != dto.IdNote)
                return BadRequest(new { message = "L'ID ne correspond pas" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _noteRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = "Note non trouvée" });

            var deny = await this.ForbidIfHorsScopeNoteAsync(id);
            if (deny != null)
                return deny;

            var oldNote = new Note
            {
                IdNote = existing.IdNote,
                NoteObtenue = existing.NoteObtenue,
                Appreciation = existing.Appreciation
            };

            existing.NoteObtenue = dto.NoteObtenue;
            existing.Appreciation = dto.Appreciation;

            try
            {
                var updated = await _noteRepository.UpdateAsync(existing);

                var auditContext = this.GetAuditContext();
                await _auditService.LogUpdateAsync(
                    oldNote, updated,
                    auditContext.UserId, auditContext.UserName,
                    auditContext.UserRole, auditContext.IdEcole,
                    auditContext.IpAddress, auditContext.UserAgent,
                    "Modification de note"
                );

                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Permission("Note.Delete")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            var deny = await this.ForbidIfHorsScopeNoteAsync(id);
            if (deny != null)
                return deny;

            var success = await _noteRepository.DeleteAsync(id);
            if (!success)
                return NotFound();
            return NoContent();
        }

        [HttpPut("toggle-statut/{id}")]
        [Permission("Note.Update")]
        public async Task<IActionResult> ToggleStatut(int id)
        {
            try
            {
                var deny = await this.ForbidIfHorsScopeNoteAsync(id);
                if (deny != null)
                    return deny;

                var success = await _noteRepository.ToggleStatutAsync(id);
                if (!success)
                    return NotFound(new { message = "Note non trouvée" });

                var note = await _noteRepository.GetByIdAsync(id);
                return Ok(new
                {
                    message = "Statut modifié avec succès",
                    nouveauStatut = note != null,
                    note = note
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors du changement de statut", error = ex.Message });
            }
        }

        private async Task<IReadOnlyList<Note>> FilterNotesForEnseignantAsync(
            IEnumerable<Note> notes,
            CancellationToken cancellationToken = default)
        {
            if (!User.IsInRole(UserRoles.ENSEIGNANT) || this.IsCotationSchoolBypassRole())
                return notes.ToList();

            var idAgent = this.GetCurrentAgentId();
            if (!idAgent.HasValue)
                return Array.Empty<Note>();

            var idEcole = this.GetCurrentUserSchoolId();
            var classIds = await _pedagogie.GetClassesEnseignantAsync(
                idAgent.Value, idAnneeScolaire: null, idEcole: idEcole, cancellationToken);
            if (classIds.Count == 0)
                return Array.Empty<Note>();

            var list = notes.ToList();
            // Notes sans Evaluation chargée : on ne peut pas filtrer → exclure pour sécurité
            return list
                .Where(n => n.Evaluation != null && classIds.Contains(n.Evaluation.IdClasse))
                .ToList();
        }
    }
}
