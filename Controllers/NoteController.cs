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
    [Authorize] // 🔒 Notes des élèves - Token JWT requis
    public class NoteController : ControllerBase
    {
        private readonly INoteRepository _noteRepository;
        private readonly IAuditService _auditService;

        public NoteController(INoteRepository noteRepository, IAuditService auditService)
        {
            _noteRepository = noteRepository;
            _auditService = auditService;
        }

        // GET: api/Note
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Note>>> GetNotes()
        {
            var notes = await _noteRepository.GetAllAsync();
            return Ok(notes);
        }

        // GET: api/Note/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Note>> GetNote(int id)
        {
            var note = await _noteRepository.GetByIdAsync(id);
            if (note == null)
            {
                return NotFound();
            }
            return Ok(note);
        }

        // GET: api/Note/eleve/5
        [HttpGet("eleve/{idEleve}")]
        public async Task<ActionResult<IEnumerable<Note>>> GetNotesByEleve(int idEleve)
        {
            var notes = await _noteRepository.GetByEleveAsync(idEleve);
            return Ok(notes);
        }

        // ✅ GET: api/Note/evaluation/5
        /// <summary>
        /// Récupère les notes d'une évaluation spécifique
        /// </summary>
        [HttpGet("evaluation/{idEvaluation}")]
        [ProducesResponseType(typeof(IEnumerable<Note>), 200)]
        public async Task<ActionResult<IEnumerable<Note>>> GetNotesByEvaluation(int idEvaluation)
        {
            var notes = await _noteRepository.GetByEvaluationAsync(idEvaluation);
            return Ok(notes);
        }

        // ⚠️ GET: api/Note/cours/5 (DEPRECATED - Utiliser /api/Note/evaluation/{idEvaluation})
        /// <summary>
        /// [DEPRECATED] Récupère les notes d'un cours (fonctionne via Evaluation.IdCours)
        /// Utiliser plutôt GET /api/Note/evaluation/{idEvaluation}
        /// </summary>
        [HttpGet("cours/{idCours}")]
        [Obsolete("Utiliser GET /api/Note/evaluation/{idEvaluation} pour une meilleure précision")]
        public async Task<ActionResult<IEnumerable<Note>>> GetNotesByCours(int idCours)
        {
            var notes = await _noteRepository.GetByCoursAsync(idCours);
            return Ok(notes);
        }

        // GET: api/Note/professeur/5
        [HttpGet("professeur/{idProfesseur}")]
        public async Task<ActionResult<IEnumerable<Note>>> GetNotesByProfesseur(int idProfesseur)
        {
            var notes = await _noteRepository.GetByProfesseurAsync(idProfesseur);
            return Ok(notes);
        }

        // GET: api/Note/annee/5
        [HttpGet("annee/{idAnneeScolaire}")]
        public async Task<ActionResult<IEnumerable<Note>>> GetNotesByAnneeScolaire(int idAnneeScolaire)
        {
            var notes = await _noteRepository.GetByAnneeScolaireAsync(idAnneeScolaire);
            return Ok(notes);
        }

        // ✅ GET: api/Note/periode/{periode}
        /// <summary>
        /// Récupère les notes d'une période spécifique (via Evaluation.Periode)
        /// </summary>
        [HttpGet("periode/{periode}")]
        [ProducesResponseType(typeof(IEnumerable<Note>), 200)]
        public async Task<ActionResult<IEnumerable<Note>>> GetNotesByPeriode(string periode)
        {
            var notes = await _noteRepository.GetByPeriodeAsync(periode);
            return Ok(notes);
        }

        // ⚠️ GET: api/Note/session/{session} (DEPRECATED - Utiliser /api/Note/periode/{periode})
        /// <summary>
        /// [DEPRECATED] Récupère les notes d'une session (fonctionne via Evaluation.Periode)
        /// Utiliser plutôt GET /api/Note/periode/{periode}
        /// </summary>
        [HttpGet("session/{session}")]
        [Obsolete("Utiliser GET /api/Note/periode/{periode}")]
        public async Task<ActionResult<IEnumerable<Note>>> GetNotesBySession(string session)
        {
            var notes = await _noteRepository.GetByPeriodeAsync(session);
            return Ok(notes);
        }

        // POST: api/Note
        [HttpPost]
        public async Task<ActionResult<Note>> CreateNote(Note note)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdNote = await _noteRepository.CreateAsync(note);
            return CreatedAtAction(nameof(GetNote), new { id = createdNote.IdNote }, createdNote);
        }

        // PUT: api/Note/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Super-Admin,Enseignant")]
        [ProducesResponseType(typeof(Note), 200)]
        public async Task<ActionResult<Note>> UpdateNote(int id, [FromBody] UpdateNoteDto dto)
        {
            if (id != dto.IdNote)
            {
                return BadRequest(new { message = "L'ID ne correspond pas" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing = await _noteRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { message = "Note non trouvée" });
            }

            // 📸 AUDIT: Capturer l'état AVANT modification
            var oldNote = new Note
            {
                IdNote = existing.IdNote,
                NoteObtenue = existing.NoteObtenue,
                Appreciation = existing.Appreciation
            };

            existing.NoteObtenue = dto.NoteObtenue;
            existing.Appreciation = dto.Appreciation;

            var updated = await _noteRepository.UpdateAsync(existing);

            // 📝 AUDIT: Enregistrer la modification
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

        // DELETE: api/Note/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            var exists = await _noteRepository.ExistsAsync(id);
            if (!exists)
            {
                return NotFound();
            }

            await _noteRepository.DeleteAsync(id);
            return NoContent();
        }

        // PUT: api/Note/toggle-statut/{id}
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _noteRepository.ToggleStatutAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Note non trouvée" });
                }

                var note = await _noteRepository.GetByIdAsync(id);
                var estActif = note != null;
                
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = estActif,
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
