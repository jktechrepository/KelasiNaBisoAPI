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
    [Authorize] // 🔒 Groupes de messages - Token JWT requis
    public class GroupeMessageController : ControllerBase
    {
        private readonly IGroupeMessageRepository _groupeMessageRepository;

        public GroupeMessageController(IGroupeMessageRepository groupeMessageRepository)
        {
            _groupeMessageRepository = groupeMessageRepository;
        }

        // GET: api/GroupeMessage
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GroupeMessage>>> GetGroupeMessages()
        {
            var groupeMessages = await _groupeMessageRepository.GetAllAsync();
            return Ok(groupeMessages);
        }

        // GET: api/GroupeMessage/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GroupeMessage>> GetGroupeMessage(int id)
        {
            var groupeMessage = await _groupeMessageRepository.GetByIdAsync(id);
            if (groupeMessage == null)
            {
                return NotFound();
            }
            return Ok(groupeMessage);
        }

        // GET: api/GroupeMessage/ecole/5
        [HttpGet("ecole/{idEcole}")]
        public async Task<ActionResult<IEnumerable<GroupeMessage>>> GetGroupeMessagesByEcole(int idEcole)
        {
            var groupeMessages = await _groupeMessageRepository.GetByEcoleAsync(idEcole);
            return Ok(groupeMessages);
        }

        // GET: api/GroupeMessage/classe/5
        [HttpGet("classe/{idClasse}")]
        //public async Task<ActionResult<IEnumerable<GroupeMessage>>> GetGroupeMessagesByClasse(int idClasse)
        //{
        //    var groupeMessages = await _groupeMessageRepository.GetByClasseAsync(idClasse);
        //    return Ok(groupeMessages);
        //}

        // GET: api/GroupeMessage/nom/Classe6A
        [HttpGet("nom/{nom}")]
        public async Task<ActionResult<GroupeMessage>> GetGroupeMessageByNom(string nom)
        {
            var groupeMessage = await _groupeMessageRepository.GetByNomAsync(nom);
            if (groupeMessage == null)
            {
                return NotFound();
            }
            return Ok(groupeMessage);
        }

        // GET: api/GroupeMessage/statut/true
        [HttpGet("statut/{statut}")]
        //public async Task<ActionResult<IEnumerable<GroupeMessage>>> GetGroupeMessagesByStatut(bool statut)
        //{
        //    var groupeMessages = await _groupeMessageRepository.GetByStatutAsync(statut);
        //    return Ok(groupeMessages);
        //}

        // GET: api/GroupeMessage/exists/5
        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> GroupeMessageExists(int id)
        {
            var exists = await _groupeMessageRepository.ExistsAsync(id);
            return Ok(exists);
        }

        // POST: api/GroupeMessage
        [HttpPost]
        public async Task<ActionResult<GroupeMessage>> CreateGroupeMessage(GroupeMessage groupeMessage)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdGroupeMessage = await _groupeMessageRepository.CreateAsync(groupeMessage);
            return CreatedAtAction(nameof(GetGroupeMessage), new { id = createdGroupeMessage.IdGroupe }, createdGroupeMessage);
        }

        // PUT: api/GroupeMessage/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<GroupeMessage>> UpdateGroupeMessage(int id, [FromBody] UpdateGroupeMessageDto dto)
        {
            if (id != dto.IdGroupe)
            {
                return BadRequest(new { message = "L'ID ne correspond pas" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing = await _groupeMessageRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { message = "Groupe de messages non trouvé" });
            }

            existing.NomGroupe = dto.NomGroupe;

            var updated = await _groupeMessageRepository.UpdateAsync(existing);
            return Ok(updated);
        }

        // DELETE: api/GroupeMessage/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGroupeMessage(int id)
        {
            var success = await _groupeMessageRepository.DeleteAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        // PUT: api/GroupeMessage/toggle-statut/{id}
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _groupeMessageRepository.ToggleStatutAsync(id);
                if (!success)
                    return NotFound(new { message = "Groupe de messages non trouvé" });

                var groupe = await _groupeMessageRepository.GetByIdAsync(id);
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = groupe != null,
                    groupeMessage = groupe
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur", error = ex.Message });
            }
        }
    }
}
