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
    [Authorize] // 🔒 Messages privés - Token JWT requis
    public class MessageController : ControllerBase
    {
        private readonly IMessageRepository _messageRepository;

        public MessageController(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        // GET: api/Message
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Message>>> GetMessages()
        {
            var messages = await _messageRepository.GetAllAsync();
            return Ok(messages);
        }

        // GET: api/Message/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Message>> GetMessage(int id)
        {
            var message = await _messageRepository.GetByIdAsync(id);
            if (message == null)
            {
                return NotFound();
            }
            return Ok(message);
        }

        // GET: api/Message/expediteur/5
        [HttpGet("expediteur/{idExpediteur}")]
        public async Task<ActionResult<IEnumerable<Message>>> GetMessagesByExpediteur(int idExpediteur)
        {
            var messages = await _messageRepository.GetByExpediteurAsync(idExpediteur);
            return Ok(messages);
        }

        // GET: api/Message/destinataire/5
        [HttpGet("destinataire/{idDestinataire}")]
        //public async Task<ActionResult<IEnumerable<Message>>> GetMessagesByDestinataire(int idDestinataire)
        //{
        //    var messages = await _messageRepository.GetByDestinataireAsync(idDestinataire);
        //    return Ok(messages);
        //}

        // GET: api/Message/groupe/5
        [HttpGet("groupe/{idGroupe}")]
        public async Task<ActionResult<IEnumerable<Message>>> GetMessagesByGroupe(int idGroupe)
        {
            var messages = await _messageRepository.GetByGroupeAsync(idGroupe);
            return Ok(messages);
        }

        // GET: api/Message/statut/true
        [HttpGet("statut/{statut}")]
        //public async Task<ActionResult<IEnumerable<Message>>> GetMessagesByStatut(bool statut)
        //{
        //    var messages = await _messageRepository.GetByStatutAsync(statut);
        //    return Ok(messages);
        //}

        // GET: api/Message/exists/5
        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> MessageExists(int id)
        {
            var exists = await _messageRepository.ExistsAsync(id);
            return Ok(exists);
        }

        // POST: api/Message
        [HttpPost]
        public async Task<ActionResult<Message>> CreateMessage(Message message)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdMessage = await _messageRepository.CreateAsync(message);
            return CreatedAtAction(nameof(GetMessage), new { id = createdMessage.IdMessage }, createdMessage);
        }

        // PUT: api/Message/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<Message>> UpdateMessage(int id, [FromBody] UpdateMessageDto dto)
        {
            if (id != dto.IdMessage)
            {
                return BadRequest(new { message = "L'ID ne correspond pas" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing = await _messageRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { message = "Message non trouvé" });
            }

            existing.ContenuMessage = dto.ContenuMessage;
            existing.FichierUrl = dto.FichierUrl;

            var updated = await _messageRepository.UpdateAsync(existing);
            return Ok(updated);
        }

        // DELETE: api/Message/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var success = await _messageRepository.DeleteAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        // PUT: api/Message/toggle-statut/{id}
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _messageRepository.ToggleStatutAsync(id);
                if (!success)
                    return NotFound(new { message = "Message non trouvé" });

                var message = await _messageRepository.GetByIdAsync(id);
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = message != null,
                    messageData = message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur", error = ex.Message });
            }
        }
    }
}
