using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
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

        public NoteController(INoteRepository noteRepository, IAuditService auditService)
        {
            _noteRepository = noteRepository;
            _auditService = auditService;
        }

        [HttpGet]
        [Permission("Note.ReadAll")]
        public async Task<ActionResult<IEnumerable<Note>>> GetNotes()
        {
            var notes = await _noteRepository.GetAllAsync();
            return Ok(notes);
        }

        [HttpGet("{id}")]
        [Permission("Note.Read")]
        public async Task<ActionResult<Note>> GetNote(int id)
        {
            var note = await _noteRepository.GetByIdAsync(id);
            if (note == null)
                return NotFound();
            return Ok(note);
        }

        [HttpGet("eleve/{idEleve}")]
        [Permission("Note.Read")]
        public async Task<ActionResult<IEnumerable<Note>>> GetNotesByEleve(int idEleve)
        {
            var notes = await _noteRepository.GetByEleveAsync(idEleve);
            return Ok(notes);
        }

        [HttpGet("evaluation/{idEvaluation}")]
        [Permission("Note.Read")]
        [ProducesResponseType(typeof(IEnumerable<Note>), 200)]
        public async Task<ActionResult<IEnumerable<Note>>> GetNotesByEvaluation(int idEvaluation)
        {
            var notes = await _noteRepository.GetByEvaluationAsync(idEvaluation);
            return Ok(notes);
        }

        [HttpGet("cours/{idCours}")]
        [Permission("Note.Read")]
        [Obsolete("Utiliser GET /api/Note/evaluation/{idEvaluation} pour une meilleure précision")]
        public async Task<ActionResult<IEnumerable<Note>>> GetNotesByCours(int idCours)
        {
            var notes = await _noteRepository.GetByCoursAsync(idCours);
            return Ok(notes);
        }

        [HttpGet("professeur/{idProfesseur}")]
        [Permission("Note.Read")]
        public async Task<ActionResult<IEnumerable<Note>>> GetNotesByProfesseur(int idProfesseur)
        {
            var notes = await _noteRepository.GetByProfesseurAsync(idProfesseur);
            return Ok(notes);
        }

        [HttpGet("annee/{idAnneeScolaire}")]
        [Permission("Note.Read")]
        public async Task<ActionResult<IEnumerable<Note>>> GetNotesByAnneeScolaire(int idAnneeScolaire)
        {
            var notes = await _noteRepository.GetByAnneeScolaireAsync(idAnneeScolaire);
            return Ok(notes);
        }

        [HttpGet("periode/{periode}")]
        [Permission("Note.Read")]
        [ProducesResponseType(typeof(IEnumerable<Note>), 200)]
        public async Task<ActionResult<IEnumerable<Note>>> GetNotesByPeriode(string periode)
        {
            var notes = await _noteRepository.GetByPeriodeAsync(periode);
            return Ok(notes);
        }

        [HttpGet("session/{session}")]
        [Permission("Note.Read")]
        [Obsolete("Utiliser GET /api/Note/periode/{periode}")]
        public async Task<ActionResult<IEnumerable<Note>>> GetNotesBySession(string session)
        {
            var notes = await _noteRepository.GetByPeriodeAsync(session);
            return Ok(notes);
        }

        [HttpPost]
        [Permission("Note.Create")]
        public async Task<ActionResult<Note>> CreateNote([FromBody] CreateNoteDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var note = new Note
            {
                NoteObtenue = dto.NoteObtenue,
                Appreciation = dto.Appreciation,
                DateEvaluation = dto.DateEvaluation ?? DateTime.Now,
                IdProfesseur = dto.IdProfesseur,
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
        [Authorize(Roles = "Admin,Super-Admin,Enseignant")]
        [ProducesResponseType(typeof(Note), 200)]
        public async Task<ActionResult<Note>> UpdateNote(int id, [FromBody] UpdateNoteDto dto)
        {
            if (id != dto.IdNote)
                return BadRequest(new { message = "L'ID ne correspond pas" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _noteRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = "Note non trouvée" });

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
            var success = await _noteRepository.DeleteAsync(id);
            if (!success)
                return NotFound();
            return NoContent();
        }

        [HttpPut("toggle-statut/{id}")]
        [Permission("Note.Update")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
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
    }
}
