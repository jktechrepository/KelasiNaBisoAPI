using System.Linq;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.Enums;
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
    [Authorize] // 🔒 Gestion des écoles - Token JWT requis
    public class EcoleController : ControllerBase
    {
        private readonly IEcoleRepository _ecoleRepository;
        private readonly IAuditService _auditService;
        private readonly ICurrentUserService _currentUserService;

        public EcoleController(
            IEcoleRepository ecoleRepository,
            IAuditService auditService,
            ICurrentUserService currentUserService)
        {
            _ecoleRepository = ecoleRepository;
            _auditService = auditService;
            _currentUserService = currentUserService;
        }

        // GET: api/Ecole
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ecole>>> GetEcoles()
        {
            var ecoles = await _ecoleRepository.GetAllAsync();
            return Ok(ecoles);
        }

        // GET: api/Ecole/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Ecole>> GetEcole(int id)
        {
            var ecole = await _ecoleRepository.GetByIdAsync(id);
            if (ecole == null)
            {
                return NotFound();
            }
            return Ok(ecole);
        }

        // GET: api/Ecole/nom/{nom}
        [HttpGet("nom/{nom}")]
        public async Task<ActionResult<Ecole>> GetEcoleByNom(string nom)
        {
            var ecole = await _ecoleRepository.GetByNomAsync(nom);
            if (ecole == null)
            {
                return NotFound();
            }
            return Ok(ecole);
        }

        // GET: api/Ecole/code/{code}
        [HttpGet("code/{code}")]
        //public async Task<ActionResult<Ecole>> GetEcoleByCode(string code)
        //{
        //    var ecole = await _ecoleRepository.GetByCodeAsync(code);
        //    if (ecole == null)
        //    {
        //        return NotFound();
        //    }
        //    return Ok(ecole);
        //}

        // GET: api/Ecole/statut/{statut}
        [HttpGet("statut/{statut}")]
        //public async Task<ActionResult<IEnumerable<Ecole>>> GetEcolesByStatut(bool statut)
        //{
        //    var ecoles = await _ecoleRepository.GetByStatutAsync(statut);
        //    return Ok(ecoles);
        //}

        // GET: api/Ecole/5/classes
        [HttpGet("{id}/classes")]
        public async Task<ActionResult<IEnumerable<Classe>>> GetEcoleClasses(int id)
        {
            var classes = await _ecoleRepository.GetClassesAsync(id);
            return Ok(classes);
        }

        // GET: api/Ecole/5/utilisateurs
        [HttpGet("{id}/utilisateurs")]
        public async Task<ActionResult<IEnumerable<Utilisateur>>> GetEcoleUtilisateurs(int id)
        {
            var utilisateurs = await _ecoleRepository.GetUtilisateursAsync(id);
            return Ok(utilisateurs);
        }

        // GET: api/Ecole/5/tuteurs
        [HttpGet("{id}/tuteurs")]
        public async Task<ActionResult<IEnumerable<Tuteur>>> GetEcoleTuteurs(int id)
        {
            var tuteurs = await _ecoleRepository.GetTuteursAsync(id);
            return Ok(tuteurs);
        }

        // GET: api/Ecole/5/agents
        [HttpGet("{id}/agents")]
        public async Task<ActionResult<IEnumerable<Agent>>> GetEcoleAgents(int id)
        {
            var agents = await _ecoleRepository.GetAgentsAsync(id);
            return Ok(agents);
        }

        // GET: api/Ecole/5/agents/enseignants
        [HttpGet("{id}/agents/enseignants")]
        public async Task<ActionResult<PagedResult<Agent>>> GetEcoleEnseignants(
            int id,
            [FromQuery] PagedRequest request)
        {
            var enseignants = await _ecoleRepository.GetAgentsByRoleAsync(id, "Enseignant", request);
            return Ok(enseignants);
        }

        // GET: api/Ecole/5/sections
        [HttpGet("{id}/sections")]
        public async Task<ActionResult<IEnumerable<Section>>> GetEcoleSections(int id)
        {
            var sections = await _ecoleRepository.GetSectionsAsync(id);
            return Ok(sections);
        }

        // GET: api/Ecole/5/annees-scolaires
        [HttpGet("{id}/annees-scolaires")]
        public async Task<ActionResult<IEnumerable<AnneeScolaire>>> GetEcoleAnneeScolaires(int id)
        {
            var anneeScolaires = await _ecoleRepository.GetAnneeScolairesAsync(id);
            return Ok(anneeScolaires);
        }

        // GET: api/Ecole/5/inscriptions
        [HttpGet("{id}/inscriptions")]
        public async Task<ActionResult<IEnumerable<Inscription>>> GetEcoleInscriptions(int id)
        {
            var inscriptions = await _ecoleRepository.GetInscriptionsAsync(id);
            return Ok(inscriptions);
        }

        // GET: api/Ecole/5/groupes-messages
        [HttpGet("{id}/groupes-messages")]
        public async Task<ActionResult<IEnumerable<GroupeMessage>>> GetEcoleGroupesMessages(int id)
        {
            var groupesMessages = await _ecoleRepository.GetGroupesMessagesAsync(id);
            return Ok(groupesMessages);
        }

        // POST: api/Ecole
        [HttpPost]
        public async Task<ActionResult<object>> CreateEcole(Ecole ecole)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdEcole = await _ecoleRepository.CreateAsync(ecole);
            
            // Récupérer les informations de l'utilisateur Admin créé automatiquement
            var adminUser = await _ecoleRepository.GetUtilisateursAsync(createdEcole.IdEcole);
            var admin = adminUser.FirstOrDefault(u => u.Role?.Nom == "Admin");
            
            var response = new
            {
                ecole = createdEcole,
                adminUser = admin != null ? new
                {
                    email = admin.Email, // ✨ Maintenant : email de l'école (emailContact)
                    telephone = admin.Telephone,
                    motDePasse = "Admin", // Mot de passe par défaut
                    nomComplet = $"{admin.PrenomUtilisateur} {admin.NomUtilisateur} {admin.PostNomUtilisateur}".Trim(),
                    message = "Email de bienvenue envoyé automatiquement à l'administrateur"
                } : null
            };
            
            return CreatedAtAction(nameof(GetEcole), new { id = createdEcole.IdEcole }, response);
        }

        // PUT: api/Ecole/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Super-Admin,Admin,Directeur,Sous-Directeur")]
        [ProducesResponseType(typeof(Ecole), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Ecole>> UpdateEcole(int id, [FromBody] UpdateEcoleDto dto)
        {
            if (id != dto.IdEcole)
            {
                return BadRequest(new { message = "L'ID dans l'URL ne correspond pas à l'ID dans le corps" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingEcole = await _ecoleRepository.GetByIdAsync(id);
            if (existingEcole == null)
            {
                return NotFound(new { message = "École non trouvée" });
            }

            if (!_currentUserService.IsSuperAdmin)
            {
                if (_currentUserService.EcoleId == 0)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "Impossible de déterminer votre école. Veuillez-vous reconnecter." });
                }

                if (_currentUserService.EcoleId != existingEcole.IdEcole)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "Vous ne pouvez modifier que votre propre école." });
                }

                if (!new[] { UserRoles.ADMIN, UserRoles.DIRECTEUR, UserRoles.SOUS_DIRECTEUR }.Contains(_currentUserService.UserRole))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "Votre rôle ne permet pas de modifier les informations de l'école." });
                }
            }

            // 📸 AUDIT: Snapshot AVANT modification
            var oldEcole = new Ecole
            {
                IdEcole = existingEcole.IdEcole,
                Nom = existingEcole.Nom,
                Description = existingEcole.Description,
                Slogan = existingEcole.Slogan,
                Type = existingEcole.Type,
                Telephone = existingEcole.Telephone,
                EmailContact = existingEcole.EmailContact,
                AcceptNotification = existingEcole.AcceptNotification
            };

            // Mettre à jour seulement les champs autorisés
            existingEcole.Nom = dto.Nom;
            existingEcole.Description = dto.Description;
            existingEcole.Slogan = dto.Slogan;
            existingEcole.Type = dto.Type;
            existingEcole.Logo = dto.Logo;
            existingEcole.SiteWeb = dto.SiteWeb;
            existingEcole.Telephone = dto.Telephone;
            existingEcole.EmailContact = dto.EmailContact;
            existingEcole.NomCompletResponsable = dto.NomCompletResponsable;
            existingEcole.GenreResponsable = dto.GenreResponsable;
            existingEcole.Ville = dto.Ville;
            existingEcole.Commune = dto.Commune;
            existingEcole.Quartier = dto.Quartier;
            existingEcole.Avenue = dto.Avenue;
            existingEcole.Numero = dto.Numero;
            existingEcole.Province = dto.Province;
            existingEcole.ProvinceEducationnel = dto.ProvinceEducationnel;
            existingEcole.Latitude = dto.Latitude;
            existingEcole.Longitute = dto.Longitute;
            existingEcole.AcceptNotification = dto.AcceptNotification;

            var updatedEcole = await _ecoleRepository.UpdateAsync(existingEcole);
            if (updatedEcole == null)
            {
                return StatusCode(500, new { message = "Erreur lors de la mise à jour" });
            }

            // 📝 AUDIT: Enregistrer
            var ctx = this.GetAuditContext();
            await _auditService.LogUpdateAsync(oldEcole, updatedEcole, ctx.UserId, ctx.UserName, ctx.UserRole, ctx.IdEcole, ctx.IpAddress, ctx.UserAgent, "Modification école");

            return Ok(updatedEcole);
        }

        // DELETE: api/Ecole/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEcole(int id)
        {
            var exists = await _ecoleRepository.ExistsAsync(id);
            if (!exists)
            {
                return NotFound();
            }

            await _ecoleRepository.DeleteAsync(id);
            return NoContent();
        }

        // PUT: api/Ecole/toggle-statut/{id}
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _ecoleRepository.ToggleStatutAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "École non trouvée" });
                }

                // Récupérer l'école après le toggle pour connaître le nouveau statut
                // Note: GetByIdAsync retourne null si l'école est désactivée à cause du filtre Statut
                var ecoleApresToggle = await _ecoleRepository.GetByIdAsync(id);
                var nouveauStatut = ecoleApresToggle != null;
                
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = nouveauStatut,
                    statut = nouveauStatut
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors du changement de statut", error = ex.Message });
            }
        }
        
        // PUT: api/Ecole/set-statut/{id}
        [HttpPut("set-statut/{id}")]
        public async Task<ActionResult<object>> SetStatut(int id, [FromQuery] bool statut)
        {
            try
            {
                var success = await _ecoleRepository.SetStatutAsync(id, statut);
                if (!success)
                {
                    return NotFound(new { message = "École non trouvée" });
                }

                var ecole = await _ecoleRepository.GetByIdAsync(id);
                
                return Ok(new { 
                    message = $"Statut défini à {statut}",
                    statut = statut,
                    ecole = ecole
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la modification du statut", error = ex.Message });
            }
        }
        
        // PUT: api/Ecole/toggle-accept-notification/{id}
        [HttpPut("toggle-accept-notification/{id}")]
        public async Task<ActionResult<object>> ToggleAcceptNotification(int id)
        {
            try
            {
                var success = await _ecoleRepository.ToggleAcceptNotificationAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "École non trouvée" });
                }

                var ecole = await _ecoleRepository.GetByIdAsync(id);
                
                return Ok(new { 
                    message = "AcceptNotification modifié avec succès",
                    nouveauStatut = ecole?.AcceptNotification ?? false,
                    acceptNotification = ecole?.AcceptNotification ?? false,
                    ecole = ecole
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors du changement d'AcceptNotification", error = ex.Message });
            }
        }
        
        // PUT: api/Ecole/set-accept-notification/{id}
        [HttpPut("set-accept-notification/{id}")]
        public async Task<ActionResult<object>> SetAcceptNotification(int id, [FromQuery] bool acceptNotification)
        {
            try
            {
                var success = await _ecoleRepository.SetAcceptNotificationAsync(id, acceptNotification);
                if (!success)
                {
                    return NotFound(new { message = "École non trouvée" });
                }

                var ecole = await _ecoleRepository.GetByIdAsync(id);
                
                return Ok(new { 
                    message = $"AcceptNotification défini à {acceptNotification}",
                    acceptNotification = acceptNotification,
                    ecole = ecole
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la modification d'AcceptNotification", error = ex.Message });
            }
        }
    }
}
