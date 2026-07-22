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
    [Authorize] // 🔒 Documents - Token JWT requis
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentRepository _documentRepository;

        public DocumentController(IDocumentRepository documentRepository)
        {
            _documentRepository = documentRepository;
        }

        // GET: api/Document
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Document>>> GetDocuments()
        {
            var documents = await _documentRepository.GetAllAsync();
            return Ok(documents);
        }

        // GET: api/Document/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Document>> GetDocument(int id)
        {
            var document = await _documentRepository.GetByIdAsync(id);
            if (document == null)
            {
                return NotFound();
            }
            return Ok(document);
        }

        // GET: api/Document/eleve/5
        [HttpGet("eleve/{idEleve}")]
        public async Task<ActionResult<IEnumerable<Document>>> GetDocumentsByEleve(int idEleve)
        {
            var documents = await _documentRepository.GetByEleveAsync(idEleve);
            return Ok(documents);
        }

        // GET: api/Document/agent/5
        [HttpGet("agent/{idAgent}")]
        //public async Task<ActionResult<IEnumerable<Document>>> GetDocumentsByAgent(int idAgent)
        //{
        //    var documents = await _documentRepository.GetByAgentAsync(idAgent);
        //    return Ok(documents);
        //}

        // GET: api/Document/type/Bulletin
        [HttpGet("type/{type}")]
        //public async Task<ActionResult<IEnumerable<Document>>> GetDocumentsByType(string type)
        //{
        //    var documents = await _documentRepository.GetByTypeAsync(type);
        //    return Ok(documents);
        //}

        // GET: api/Document/statut/true
        [HttpGet("statut/{statut}")]
        //public async Task<ActionResult<IEnumerable<Document>>> GetDocumentsByStatut(bool statut)
        //{
        //    var documents = await _documentRepository.GetByStatutAsync(statut);
        //    return Ok(documents);
        //}

        // GET: api/Document/exists/5
        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> DocumentExists(int id)
        {
            var exists = await _documentRepository.ExistsAsync(id);
            return Ok(exists);
        }

        // POST: api/Document
        [HttpPost]
        public async Task<ActionResult<Document>> CreateDocument(Document document)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdDocument = await _documentRepository.CreateAsync(document);
            return CreatedAtAction(nameof(GetDocument), new { id = createdDocument.IdDocument }, createdDocument);
        }

        // PUT: api/Document/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Super-Admin,Enseignant")]
        public async Task<ActionResult<Document>> UpdateDocument(int id, [FromBody] UpdateDocumentDto dto)
        {
            if (id != dto.IdDocument)
            {
                return BadRequest(new { message = "L'ID ne correspond pas" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing = await _documentRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { message = "Document non trouvé" });
            }

            existing.TypeDocument = dto.TypeDocument;

            var updated = await _documentRepository.UpdateAsync(existing);
            return Ok(updated);
        }

        // DELETE: api/Document/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var success = await _documentRepository.DeleteAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        // PUT: api/Document/toggle-statut/{id}
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _documentRepository.ToggleStatutAsync(id);
                if (!success)
                    return NotFound(new { message = "Document non trouvé" });

                var document = await _documentRepository.GetByIdAsync(id);
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = document != null,
                    document = document
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur", error = ex.Message });
            }
        }
    }
}
