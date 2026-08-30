using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.DTOs.Reporting;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Attributes;
using KelasiNaBiso.Helpers;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Reporting;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Globalization;
using System.Linq;

namespace KelasiNaBiso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔒 Données sensibles - Token JWT requis
    public class AgentController : ControllerBase
    {
        private readonly IAgentRepository _agentRepository;
        private readonly IAuditService _auditService;
        private readonly ICurrentUserService _currentUserService;
        private readonly KelasiNaBisoDbContext _context;
        private readonly IReportImageResolver _imageResolver;

        public AgentController(
            IAgentRepository agentRepository,
            IAuditService auditService,
            ICurrentUserService currentUserService,
            KelasiNaBisoDbContext context,
            IReportImageResolver imageResolver)
        {
            _agentRepository = agentRepository;
            _auditService = auditService;
            _currentUserService = currentUserService;
            _context = context;
            _imageResolver = imageResolver;
        }

        // GET: api/Agent
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Agent>>> GetAgents()
        {
            var agents = await _agentRepository.GetAllAsync();
            return Ok(agents);
        }

        // ═══════════════════════════════════════════════════════════════════
        // ✅ MULTI-RÔLES : Gestion des rôles pour les agents
        // ═══════════════════════════════════════════════════════════════════
        // ⚠️ IMPORTANT : Ces routes doivent être AVANT la route {id} pour éviter les conflits

        /// <summary>
        /// Ajoute un ou plusieurs rôles à un agent
        /// Les rôles sont ajoutés à l'utilisateur associé à cet agent
        /// Le RoleAgent correspond au Nom du rôle dans la table Roles
        /// </summary>
        /// <param name="idAgent">ID de l'agent</param>
        /// <param name="request">Requête contenant la liste des rôles à ajouter (chaque rôle contient RoleAgent et IsPrimary)</param>
        /// <returns>Résultat détaillé de l'opération avec les rôles ajoutés avec succès et ceux qui ont échoué</returns>
        [HttpPost("{idAgent}/add-role")]
        [Authorize(Roles = "Admin,Super-Admin")]
        [ProducesResponseType(typeof(AddRolesResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<AddRolesResult>> AddRoleToAgent(int idAgent, [FromBody] AddRolesToAgentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Vérifier que l'agent existe
                var agent = await _agentRepository.GetByIdAsync(idAgent);
                if (agent == null)
                {
                    return NotFound(new AddRolesResult 
                    { 
                        Success = false,
                        Message = $"Agent avec l'ID {idAgent} non trouvé",
                        TotalRoles = request.Roles.Count
                    });
                }

                // Convertir la liste de DTOs en tuples pour le service
                var roles = request.Roles.Select(r => (r.RoleAgent, r.IsPrimary));

                // Ajouter les rôles à l'agent
                var result = await _agentRepository.AddRolesToAgentAsync(
                    idAgent, 
                    roles,
                    _currentUserService.UserId > 0 ? _currentUserService.UserId : null
                );

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new AddRolesResult
                { 
                    Success = false,
                    Message = $"Erreur lors de l'ajout des rôles à l'agent: {ex.Message}",
                    TotalRoles = request.Roles.Count
                });
            }
        }

            /// <summary>
            /// Remplace un RoleAgent par un autre pour un agent
            /// Le statut IsPrimary est conservé lors du remplacement
            /// </summary>
            /// <param name="idAgent">ID de l'agent</param>
            /// <param name="request">Requête contenant l'ancien et le nouveau RoleAgent</param>
            /// <returns>Résultat de l'opération</returns>
            [HttpPut("{idAgent}/replace-role")]
            [Authorize(Roles = "Admin,Super-Admin,Directeur")]
            [ProducesResponseType(typeof(object), 200)]
            [ProducesResponseType(400)]
            [ProducesResponseType(404)]
            [ProducesResponseType(401)]
            public async Task<ActionResult<object>> ReplaceRoleAgent(int idAgent, [FromBody] ReplaceRoleAgentRequest request)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Vérifier que l'ancien et le nouveau rôle sont différents
                if (request.AncienRoleAgent.Trim().Equals(request.NouveauRoleAgent.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new 
                    { 
                        success = false,
                        message = "L'ancien et le nouveau RoleAgent doivent être différents"
                    });
                }

                try
                {
                    // Vérifier que l'agent existe
                    var agent = await _agentRepository.GetByIdAsync(idAgent);
                    if (agent == null)
                    {
                        return NotFound(new { message = $"Agent avec l'ID {idAgent} non trouvé" });
                    }

                    // Remplacer le rôle
                    var success = await _agentRepository.ReplaceRoleAgentAsync(
                        idAgent, 
                        request.AncienRoleAgent.Trim(), 
                        request.NouveauRoleAgent.Trim(),
                        _currentUserService.UserId > 0 ? _currentUserService.UserId : null
                    );

                    if (success)
                    {
                        return Ok(new 
                        { 
                            success = true,
                            message = $"Rôle '{request.AncienRoleAgent}' remplacé par '{request.NouveauRoleAgent}' avec succès pour l'agent {idAgent}",
                            idAgent = idAgent,
                            ancienRoleAgent = request.AncienRoleAgent,
                            nouveauRoleAgent = request.NouveauRoleAgent
                        });
                    }
                    else
                    {
                        return BadRequest(new 
                        { 
                            success = false,
                            message = $"Impossible de remplacer le rôle '{request.AncienRoleAgent}' par '{request.NouveauRoleAgent}' pour l'agent {idAgent}. " +
                                     $"Vérifiez que l'agent a bien l'ancien rôle et que les rôles existent dans la base de données."
                        });
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new 
                    { 
                        success = false,
                        message = "Erreur lors du remplacement du rôle",
                        error = ex.Message 
                    });
                }
            }

            // GET: api/Agent/5
            [HttpGet("{id}")]
        public async Task<ActionResult<Agent>> GetAgent(int id)
        {
            var agent = await _agentRepository.GetByIdAsync(id);
            if (agent == null)
            {
                return NotFound();
            }
            return Ok(agent);
        }

        // GET: api/Agent/ecole/5
        [HttpGet("ecole/{idEcole}")]
        public async Task<ActionResult<IEnumerable<Agent>>> GetAgentsByEcole(int idEcole)
        {
            var agents = await _agentRepository.GetByEcoleAsync(idEcole);
            return Ok(agents);
        }

        // GET: api/Agent/ecole/5/carte-data
        [HttpGet("ecole/{idEcole}/carte-data")]
        [ProducesResponseType(typeof(IEnumerable<CarteAgentDataDto>), 200)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<IEnumerable<CarteAgentDataDto>>> GetAgentCarteDataByEcole(int idEcole, CancellationToken cancellationToken)
        {
            if (!_currentUserService.CanAccessAllSchools && _currentUserService.EcoleId != idEcole)
                return Forbid("Accès refusé à cette école.");

            var ecole = await _context.Ecoles
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdEcole == idEcole, cancellationToken);

            if (ecole == null)
                return NotFound(new { message = $"École {idEcole} introuvable." });

            var logoBytes = await _imageResolver.ResolveAsync(ecole.Logo, cancellationToken);

            var agents = await _context.Agents
                .AsNoTracking()
                .Where(a => a.IdEcole == idEcole && a.Statut == true)
                .OrderByDescending(a => a.DateCreation)
                .Select(a => new
                {
                    a.IdAgent,
                    a.IdEcole,
                    a.Matricule,
                    a.Nom,
                    a.Postnom,
                    a.Prenom,
                    a.Fonction,
                    a.RoleAgent,
                    a.PhotoUrl
                })
                .ToListAsync(cancellationToken);

            var result = new List<CarteAgentDataDto>(agents.Count);
            foreach (var agent in agents)
            {
                var photoBytes = await _imageResolver.ResolveAsync(agent.PhotoUrl, cancellationToken);
                var logoBase64 = ImageSourceHelper.ToRawBase64(ecole.Logo)
                    ?? (logoBytes != null ? Convert.ToBase64String(logoBytes) : null);
                var photoBase64 = ImageSourceHelper.ToRawBase64(agent.PhotoUrl)
                    ?? (photoBytes != null ? Convert.ToBase64String(photoBytes) : null);
                result.Add(new CarteAgentDataDto
                {
                    IdAgent = agent.IdAgent,
                    IdEcole = agent.IdEcole,
                    Matricule = ToCardUpper(agent.Matricule ?? agent.IdAgent.ToString()),
                    NomComplet = ToCardUpper($"{agent.Nom} {agent.Postnom} {agent.Prenom}"),
                    Fonction = ToCardUpper(agent.Fonction),
                    RoleAgent = ToCardUpper(agent.RoleAgent),
                    NomEcole = ToCardUpper(ecole.Nom),
                    SloganEcole = ToCardUpper(ecole.Slogan),
                    LogoBase64 = logoBase64,
                    PhotoBase64 = photoBase64,
                    LogoSrc = ImageSourceHelper.ToImageSrc(ecole.Logo) ?? ImageSourceHelper.ToImageSrc(logoBase64),
                    PhotoSrc = ImageSourceHelper.ToImageSrc(agent.PhotoUrl) ?? ImageSourceHelper.ToImageSrc(photoBase64),
                    LogoBytesBase64 = logoBase64,
                    PhotoBytesBase64 = photoBase64
                });
            }

            return Ok(result);
        }

        // GET: api/Agent/5/carte-data
        [HttpGet("{idAgent}/carte-data")]
        [ProducesResponseType(typeof(CarteAgentDataDto), 200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<CarteAgentDataDto>> GetAgentCarteData(int idAgent, CancellationToken cancellationToken)
        {
            var agent = await _context.Agents
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.IdAgent == idAgent && a.Statut == true, cancellationToken);

            if (agent == null)
                return NotFound(new { message = $"Agent {idAgent} introuvable." });

            if (!_currentUserService.CanAccessAllSchools && _currentUserService.EcoleId != agent.IdEcole)
                return Forbid("Accès refusé à cet agent.");

            var ecole = agent.IdEcole.HasValue
                ? await _context.Ecoles.AsNoTracking().FirstOrDefaultAsync(e => e.IdEcole == agent.IdEcole.Value, cancellationToken)
                : null;

            var logoBytes = await _imageResolver.ResolveAsync(ecole?.Logo, cancellationToken);
            var photoBytes = await _imageResolver.ResolveAsync(agent.PhotoUrl, cancellationToken);
            var logoBase64 = ImageSourceHelper.ToRawBase64(ecole?.Logo)
                ?? (logoBytes != null ? Convert.ToBase64String(logoBytes) : null);
            var photoBase64 = ImageSourceHelper.ToRawBase64(agent.PhotoUrl)
                ?? (photoBytes != null ? Convert.ToBase64String(photoBytes) : null);

            var dto = new CarteAgentDataDto
            {
                IdAgent = agent.IdAgent,
                IdEcole = agent.IdEcole,
                Matricule = ToCardUpper(agent.Matricule ?? agent.IdAgent.ToString()),
                NomComplet = ToCardUpper($"{agent.Nom} {agent.Postnom} {agent.Prenom}"),
                Fonction = ToCardUpper(agent.Fonction),
                RoleAgent = ToCardUpper(agent.RoleAgent),
                NomEcole = ToCardUpper(ecole?.Nom),
                SloganEcole = ToCardUpper(ecole?.Slogan),
                LogoBase64 = logoBase64,
                PhotoBase64 = photoBase64,
                LogoSrc = ImageSourceHelper.ToImageSrc(ecole?.Logo) ?? ImageSourceHelper.ToImageSrc(logoBase64),
                PhotoSrc = ImageSourceHelper.ToImageSrc(agent.PhotoUrl) ?? ImageSourceHelper.ToImageSrc(photoBase64),
                LogoBytesBase64 = logoBase64,
                PhotoBytesBase64 = photoBase64
            };

            return Ok(dto);
        }

        // GET: api/Agent/matiere/5
        [HttpGet("matiere/{idMatiere}")]
        //public async Task<ActionResult<IEnumerable<Agent>>> GetAgentsByMatiere(int idMatiere)
        //{
        //    var agents = await _agentRepository.GetByMatiereAsync(idMatiere);
        //    return Ok(agents);
        //}

        // GET: api/Agent/statut/true
        [HttpGet("statut/{statut}")]
        public async Task<ActionResult<IEnumerable<Agent>>> GetAgentsByStatut(bool statut)
        {
            var agents = await _agentRepository.GetByStatutAsync(statut);
            return Ok(agents);
        }

        // GET: api/Agent/exists/5
        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> AgentExists(int id)
        {
            var exists = await _agentRepository.ExistsAsync(id);
            return Ok(exists);
        }

        // POST: api/Agent
        [HttpPost]
        [Permission("Agent.Create")]
        public async Task<ActionResult<Agent>> CreateAgent(Agent agent)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdAgent = await _agentRepository.CreateAsync(agent);
            return CreatedAtAction(nameof(GetAgent), new { id = createdAgent.IdAgent }, createdAgent);
        }

        // POST: api/Agent/batch
        [HttpPost("batch")]
        [Permission("Agent.Create")]
        public async Task<ActionResult<object>> CreateAgentsBatch(IEnumerable<Agent> agents)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var agentList = agents.ToList();
            if (!agentList.Any())
            {
                return BadRequest(new { message = "La liste des agents est vide" });
            }

            try
            {
                var createdAgents = await _agentRepository.CreateBatchAsync(agentList);
                return Ok(new { 
                    message = $"{createdAgents.Count()} agent(s) créé(s) avec succès",
                    total = agentList.Count,
                    success = createdAgents.Count(),
                    failed = agentList.Count - createdAgents.Count(),
                    agents = createdAgents
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la création par lot", error = ex.Message });
            }
        }

        // PUT: api/Agent/5
        /// <summary>
        /// Modifier les informations d'un agent
        /// </summary>
        /// <remarks>
        /// Permet de modifier les informations personnelles et professionnelles d'un agent.
        /// 
        /// Champs modifiables :
        /// - Informations personnelles (nom, prénom, email, téléphone, etc.)
        /// - Informations professionnelles (fonction, grade, date d'embauche)
        /// - Adresse complète
        /// 
        /// Champs protégés :
        /// - Matricule (auto-généré, immuable)
        /// - SerialNumber (endpoint dédié)
        /// - École (immuable)
        /// - Statut (endpoint toggle-statut)
        /// </remarks>
        [HttpPut("{id}")]
        [Permission("Agent.Update")]
        [ProducesResponseType(typeof(Agent), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Agent>> UpdateAgent(int id, [FromBody] UpdateAgentDto dto)
        {
            if (id != dto.IdAgent)
            {
                return BadRequest(new { message = "L'ID dans l'URL ne correspond pas à l'ID dans le corps" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Récupérer l'agent existant
            var existingAgent = await _agentRepository.GetByIdAsync(id);
            if (existingAgent == null)
            {
                return NotFound(new { message = "Agent non trouvé" });
            }

            var currentRole = _currentUserService.UserRole;
            var isSuperAdmin = _currentUserService.IsSuperAdmin;
            var currentAgentId = _currentUserService.AgentId;
            var currentEcoleId = _currentUserService.EcoleId;
            var isSelf = currentAgentId.HasValue && currentAgentId.Value == existingAgent.IdAgent;
            var existingRoleAgent = existingAgent.RoleAgent ?? string.Empty;

            if (!isSuperAdmin)
            {
                if (existingAgent.Statut != true)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "Seul un Super-Admin peut modifier un agent désactivé." });
                }

                if (!isSelf)
                {
                    if (string.Equals(existingRoleAgent, UserRoles.SUPER_ADMIN, StringComparison.OrdinalIgnoreCase))
                    {
                        return StatusCode(StatusCodes.Status403Forbidden, new { message = "Seul un Super-Admin peut modifier ce profil." });
                    }

                    if (string.Equals(currentRole, UserRoles.ADMIN, StringComparison.OrdinalIgnoreCase))
                    {
                        if (currentEcoleId == 0 || existingAgent.IdEcole != currentEcoleId)
                        {
                            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Vous ne pouvez modifier que les agents de votre école." });
                        }
                    }
                    else if (string.Equals(currentRole, UserRoles.DIRECTEUR, StringComparison.OrdinalIgnoreCase))
                    {
                        if (currentEcoleId == 0 || existingAgent.IdEcole != currentEcoleId)
                        {
                            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Vous ne pouvez modifier que les agents de votre école." });
                        }

                        if (string.Equals(existingRoleAgent, UserRoles.ADMIN, StringComparison.OrdinalIgnoreCase))
                        {
                            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Un Directeur ne peut pas modifier le profil d'un Admin." });
                        }
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status403Forbidden, new { message = "Vous n'êtes pas autorisé à modifier ce profil." });
                    }
                }
            }

            var requestedRoleAgent = string.IsNullOrWhiteSpace(dto.RoleAgent)
                ? existingAgent.RoleAgent
                : dto.RoleAgent.Trim();

            if (isSelf && !isSuperAdmin)
            {
                requestedRoleAgent = existingAgent.RoleAgent;
            }
            else if (!isSuperAdmin && !string.IsNullOrWhiteSpace(requestedRoleAgent))
            {
                if (string.Equals(requestedRoleAgent, UserRoles.SUPER_ADMIN, StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "Seul un Super-Admin peut attribuer le rôle Super-Admin." });
                }

                if (string.Equals(requestedRoleAgent, UserRoles.ADMIN, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(currentRole, UserRoles.ADMIN, StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "Seuls un Super-Admin ou un Admin peuvent attribuer le rôle Admin." });
                }
            }

            // 📸 AUDIT: Snapshot AVANT
            var oldAgent = new Agent
            {
                IdAgent = existingAgent.IdAgent,
                Nom = existingAgent.Nom,
                Postnom = existingAgent.Postnom,
                Prenom = existingAgent.Prenom,
                EmailAgent = existingAgent.EmailAgent,
                TelephoneAgent = existingAgent.TelephoneAgent,
                Fonction = existingAgent.Fonction,
                RoleAgent = existingAgent.RoleAgent
            };

            // Mettre à jour seulement les champs autorisés
            existingAgent.Nom = dto.Nom;
            existingAgent.Postnom = dto.Postnom;
            existingAgent.Prenom = dto.Prenom;
            existingAgent.EmailAgent = dto.EmailAgent;
            existingAgent.TelephoneAgent = TelephoneNormalizer.Normalize(dto.TelephoneAgent);
            existingAgent.PhotoUrl = dto.PhotoUrl;
            if (dto.DateNaissance.HasValue)
                existingAgent.DateNaissance = dto.DateNaissance.Value;
            existingAgent.Genre = dto.Genre;
            existingAgent.EtatCivil = dto.EtatCivil;
            existingAgent.Fonction = dto.Fonction;
            existingAgent.RoleAgent = requestedRoleAgent;
            
            // Adresse
            existingAgent.Ville = dto.Ville;
            existingAgent.Commune = dto.Commune;
            existingAgent.Quartier = dto.Quartier;
            existingAgent.Avenue = dto.Avenue;
            existingAgent.Numero = dto.Numero;
            
            // Champs protégés (JAMAIS modifiés ici)
            // ❌ Matricule → Immuable
            // ❌ SerialNumber → Endpoint dédié
            // ❌ IdEcole → Immuable
            // ❌ Statut → Endpoint toggle-statut
            // ❌ DateCreation → Immuable
            // ❌ RoleAgent → Endpoint dédié

            Agent? updatedAgent;
            try
            {
                updatedAgent = await _agentRepository.UpdateAsync(existingAgent);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            if (updatedAgent == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erreur lors de la mise à jour" });
            }

            // 📝 AUDIT
            var ctx = this.GetAuditContext();
            await _auditService.LogUpdateAsync(oldAgent, updatedAgent, ctx.UserId, ctx.UserName, ctx.UserRole, ctx.IdEcole, ctx.IpAddress, ctx.UserAgent, "Modification agent");

            return Ok(updatedAgent);
        }

        // DELETE: api/Agent/5
        [HttpDelete("{id}")]
        [Permission("Agent.Delete")]
        public async Task<IActionResult> DeleteAgent(int id)
        {
            var success = await _agentRepository.DeleteAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        // PUT: api/Agent/toggle-statut/{id}
        [HttpPut("toggle-statut/{id}")]
        [Permission("Agent.Update")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _agentRepository.ToggleStatutAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Agent non trouvé" });
                }

                var agent = await _agentRepository.GetByIdAsync(id);
                var estActif = agent != null;
                
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = estActif,
                    agent = agent
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors du changement de statut", error = ex.Message });
            }
        }

        // ✅ GET: api/Agent/serial-number/{serialNumber}
        // Récupérer un agent par son numéro de série
        [HttpGet("serial-number/{serialNumber}")]
        [Authorize(Roles = "Admin,Directeur,Super-Admin,IT-Support")]
        public async Task<IActionResult> GetAgentBySerialNumber(string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                return BadRequest(new { message = "Le numéro de série ne peut pas être vide" });
            }

            var agent = await _agentRepository.GetBySerialNumberAsync(serialNumber);
            if (agent == null)
            {
                return NotFound(new { message = $"Aucun agent trouvé avec le numéro de série '{serialNumber}'" });
            }

            var deny = this.ForbidIfWrongSchool(agent.IdEcole);
            if (deny != null) return deny;

            return Ok(agent);
        }

        // ✅ PUT: api/Agent/{idAgent}/serial-number
        // Mise à jour du Serial Number par IdAgent
        [HttpPut("{idAgent}/serial-number")]
        [Authorize(Roles = "Admin,Directeur,Super-Admin,IT-Support")]
        public async Task<IActionResult> UpdateSerialNumberById(int idAgent, [FromBody] UpdateSerialNumberDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var agentAvant = await _agentRepository.GetByIdAsync(idAgent);
                if (agentAvant == null)
                {
                    return NotFound(new { message = $"Agent avec l'ID {idAgent} non trouvé" });
                }

                var deny = this.ForbidIfWrongSchool(agentAvant.IdEcole);
                if (deny != null) return deny;

                var success = await _agentRepository.UpdateSerialNumberByIdAsync(idAgent, dto.SerialNumber);
                if (!success)
                {
                    return NotFound(new { message = $"Agent avec l'ID {idAgent} non trouvé" });
                }

                var agent = await _agentRepository.GetByIdAsync(idAgent);
                return Ok(new
                {
                    message = "Numéro de série mis à jour avec succès",
                    idAgent = idAgent,
                    serialNumber = dto.SerialNumber,
                    agent = agent
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la mise à jour du numéro de série", error = ex.Message });
            }
        }

        // ✅ PUT: api/Agent/matricule/{matricule}/serial-number
        // Mise à jour du Serial Number par Matricule
        [HttpPut("matricule/{matricule}/serial-number")]
        [Authorize(Roles = "Admin,Directeur,Super-Admin,IT-Support")]
        public async Task<IActionResult> UpdateSerialNumberByMatricule(string matricule, [FromBody] UpdateSerialNumberDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(matricule))
            {
                return BadRequest(new { message = "Le matricule ne peut pas être vide" });
            }

            try
            {
                var agentAvant = await _agentRepository.GetByMatriculeAsync(matricule);
                if (agentAvant == null)
                {
                    return NotFound(new { message = $"Agent avec le matricule '{matricule}' non trouvé" });
                }

                var deny = this.ForbidIfWrongSchool(agentAvant.IdEcole);
                if (deny != null) return deny;

                var success = await _agentRepository.UpdateSerialNumberByMatriculeAsync(matricule, dto.SerialNumber);
                if (!success)
                {
                    return NotFound(new { message = $"Agent avec le matricule '{matricule}' non trouvé" });
                }

                var agent = await _agentRepository.GetByMatriculeAsync(matricule);
                return Ok(new
                {
                    message = "Numéro de série mis à jour avec succès",
                    matricule = matricule,
                    serialNumber = dto.SerialNumber,
                    agent = agent
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la mise à jour du numéro de série", error = ex.Message });
            }
        }

        private static string ToCardUpper(string? value) =>
            string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().ToUpper(CultureInfo.GetCultureInfo("fr-FR"));
    }
}

