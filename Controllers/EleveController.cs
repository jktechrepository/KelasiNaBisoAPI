using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.DTOs.Pagination;
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
    [Authorize] // 🔒 Données sensibles - Token JWT requis
    public class EleveController : ControllerBase
    {
        private readonly IEleveRepository _eleveRepository;
        private readonly IAuditService _auditService;

        public EleveController(IEleveRepository eleveRepository, IAuditService auditService)
        {
            _eleveRepository = eleveRepository;
            _auditService = auditService;
        }

        // ✅ GET: api/Eleve/paged (NOUVELLE VERSION PAGINÉE - RECOMMANDÉE)
        /// <summary>
        /// Récupère tous les élèves avec pagination offset-based (par pages)
        /// </summary>
        /// <param name="request">Paramètres de pagination (PageNumber, PageSize, SortBy, SearchTerm)</param>
        /// <returns>Liste paginée des élèves avec métadonnées</returns>
        [HttpGet("paged")]
        [ProducesResponseType(typeof(PagedResult<V_Eleve>), 200)]
        public async Task<ActionResult<PagedResult<V_Eleve>>> GetElevesPaged([FromQuery] PagedRequest request)
        {
            var result = await _eleveRepository.GetAllPagedAsync(request);
            return Ok(result);
        }

        // ✅ GET: api/Eleve/cursor-paged (PAGINATION CURSOR - MOBILE/SCROLL INFINI)
        /// <summary>
        /// Récupère tous les élèves avec pagination cursor-based (scroll infini)
        /// Idéal pour apps mobiles et performances constantes
        /// </summary>
        /// <param name="request">Paramètres de pagination (Cursor, Limit, SearchTerm)</param>
        /// <returns>Liste paginée des élèves avec curseur suivant</returns>
        [HttpGet("cursor-paged")]
        [ProducesResponseType(typeof(CursorPaginatedResult<V_Eleve>), 200)]
        public async Task<ActionResult<CursorPaginatedResult<V_Eleve>>> GetElevesCursorPaged([FromQuery] CursorPaginationRequest request)
        {
            var result = await _eleveRepository.GetAllCursorPagedAsync(request);
            return Ok(result);
        }

        // ⚠️ GET: api/Eleve (DEPRECATED - À ÉVITER, NON PAGINÉ)
        /// <summary>
        /// [DEPRECATED] Récupère TOUS les élèves sans pagination
        /// ⚠️ ATTENTION: Peut causer des problèmes de performance avec beaucoup d'élèves
        /// Utiliser plutôt GET /api/Eleve/paged
        /// </summary>
        [HttpGet]
        [Obsolete("Cette méthode n'est pas paginée et peut causer des problèmes de performance. Utilisez GET /api/Eleve/paged")]
        public async Task<ActionResult<IEnumerable<V_Eleve>>> GetEleves()
        {
            var eleves = await _eleveRepository.GetAllAsync();
            return Ok(eleves);
        }

        // GET: api/Eleve/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Eleve>> GetEleve(int id)
        {
            var eleve = await _eleveRepository.GetByIdAsync(id);
            if (eleve == null)
            {
                return NotFound();
            }
            return Ok(eleve);
        }

        // ✅ GET: api/Eleve/serial-number/{serialNumber}
        // Récupérer un élève par son numéro de série
        [HttpGet("serial-number/{serialNumber}")]
        [Authorize(Roles = "Admin,Directeur,Super-Admin,IT-Support")]
        public async Task<IActionResult> GetEleveBySerialNumber(string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                return BadRequest(new { message = "Le numéro de série ne peut pas être vide" });
            }

            var eleve = await _eleveRepository.GetBySerialNumberAsync(serialNumber);
            if (eleve == null)
            {
                return NotFound(new { message = $"Aucun élève trouvé avec le numéro de série '{serialNumber}'" });
            }

            var deny = this.ForbidIfWrongSchool(eleve.Classe?.Direction?.IdEcole);
            if (deny != null) return deny;

            return Ok(eleve);
        }

        // GET: api/Eleve/reference/ELEVE-20240814-ABC12345
        [HttpGet("reference/{reference}")]
        public async Task<ActionResult<Eleve>> GetEleveByReference(Guid reference)
        {
            var eleve = await _eleveRepository.GetByReferenceAsync(reference);
            if (eleve == null)
            {
                return NotFound();
            }
            return Ok(eleve);
        }

        // ✅ GET: api/Eleve/classe/5/paged (NOUVELLE VERSION PAGINÉE)
        /// <summary>
        /// Récupère les élèves d'une classe avec pagination
        /// </summary>
        [HttpGet("classe/{idClasse}/paged")]
        [ProducesResponseType(typeof(PagedResult<Eleve>), 200)]
        public async Task<ActionResult<PagedResult<Eleve>>> GetElevesByClassePaged(int idClasse, [FromQuery] PagedRequest request)
        {
            var result = await _eleveRepository.GetByClassePagedAsync(idClasse, request);
            return Ok(result);
        }

        // ⚠️ GET: api/Eleve/classe/5 (DEPRECATED - NON PAGINÉ)
        [HttpGet("classe/{idClasse}")]
        [Obsolete("Utiliser GET /api/Eleve/classe/{idClasse}/paged pour pagination")]
        public async Task<ActionResult<IEnumerable<Eleve>>> GetElevesByClasse(int idClasse)
        {
            var eleves = await _eleveRepository.GetByClasseAsync(idClasse);
            return Ok(eleves);
        }

        // GET: api/Eleve/tuteur/5
        [HttpGet("tuteur/{idTuteur}")]
        public async Task<ActionResult<IEnumerable<Eleve>>> GetElevesByTuteur(int idTuteur)
        {
            var eleves = await _eleveRepository.GetByTuteurAsync(idTuteur);
            return Ok(eleves);
        }

        // GET: api/Eleve/ecole/5?page=1&pageSize=15
        [HttpGet("ecole/{idEcole}")]
        public async Task<ActionResult<IEnumerable<Eleve>>> GetElevesByEcole(
            int idEcole,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 15)
        {
            // Validation des paramètres de pagination
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 15;
            if (pageSize > 100) pageSize = 100; // Limite max pour éviter surcharge

            var allEleves = await _eleveRepository.GetByEcoleAsync(idEcole);
            
            // Appliquer la pagination
            var elevesPaginated = allEleves
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Calculer les métadonnées de pagination
            var totalCount = allEleves.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Retourner avec les métadonnées
            return Ok(new
            {
                data = elevesPaginated,
                pagination = new
                {
                    currentPage = page,
                    pageSize = pageSize,
                    totalCount = totalCount,
                    totalPages = totalPages,
                    hasNextPage = page < totalPages,
                    hasPreviousPage = page > 1
                }
            });
        }

        // ✅ GET: api/Eleve/ecole/all
        /// <summary>
        /// Récupère TOUS les élèves de l'école de l'utilisateur connecté sans pagination (pour exportation)
        /// L'ID de l'école est automatiquement extrait du token JWT
        /// ⚠️ ATTENTION: Peut retourner un grand nombre d'élèves. Utiliser uniquement pour l'exportation.
        /// </summary>
        /// <param name="statut">Filtrer par statut (true = actifs uniquement, false = inactifs uniquement, null = tous). Par défaut : true (actifs uniquement)</param>
        /// <returns>Liste complète des élèves de l'école de l'utilisateur connecté</returns>
        [HttpGet("ecole/all")]
        [Authorize(Roles = "Admin,Super-Admin,Directeur")]
        [ProducesResponseType(typeof(IEnumerable<Eleve>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<Eleve>>> GetElevesByEcoleAllFromToken(
            [FromQuery] bool? statut = true)
        {
            // Extraire l'ID de l'école depuis le token JWT
            var idEcole = this.GetCurrentUserSchoolId();
            if (!idEcole.HasValue || idEcole.Value <= 0)
            {
                // Vérifier si le claim existe mais est vide
                var schoolClaim = User.FindFirst("IdEcole") ?? User.FindFirst("idEcole");
                if (schoolClaim != null)
                {
                    return Unauthorized(new { 
                        message = "Aucune école associée à l'utilisateur. Veuillez contacter l'administrateur pour associer une école à votre compte.",
                        details = $"Claim IdEcole trouvé mais valeur invalide: '{schoolClaim.Value}'"
                    });
                }
                else
                {
                    return Unauthorized(new { 
                        message = "Token JWT invalide : aucune école associée à l'utilisateur. Le token ne contient pas d'information sur l'école.",
                        details = "Le claim IdEcole est absent du token. L'utilisateur doit se reconnecter après avoir été associé à une école."
                    });
                }
            }

            try
            {
                // Récupérer tous les élèves de l'école
                var allEleves = await _eleveRepository.GetByEcoleAsync(idEcole.Value);

                // Filtrer par statut si spécifié
                if (statut.HasValue)
                {
                    allEleves = allEleves.Where(e => e.Statut == statut.Value);
                }

                var elevesList = allEleves.ToList();

                // Retourner avec métadonnées pour information
                return Ok(new
                {
                    data = elevesList,
                    totalCount = elevesList.Count,
                    idEcole = idEcole.Value,
                    statut = statut.HasValue ? (statut.Value ? "Actifs" : "Inactifs") : "Tous",
                    exportedAt = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Erreur lors de la récupération des élèves", 
                    error = ex.Message 
                });
            }
        }

        // ✅ GET: api/Eleve/ecole/{idEcole}/all
        /// <summary>
        /// Récupère TOUS les élèves d'une école spécifiée sans pagination (pour exportation)
        /// ⚠️ ATTENTION: Peut retourner un grand nombre d'élèves. Utiliser uniquement pour l'exportation.
        /// ⚠️ Super-Admin / IT-Support : toutes les écoles ; sinon utilisateur de la même école
        /// </summary>
        /// <param name="idEcole">ID de l'école</param>
        /// <param name="statut">Filtrer par statut (true = actifs uniquement, false = inactifs uniquement, null = tous). Par défaut : true (actifs uniquement)</param>
        /// <returns>Liste complète des élèves de l'école</returns>
        [HttpGet("ecole/{idEcole}/all")]
        [Authorize(Roles = "Admin,Super-Admin,Directeur,IT-Support")]
        [ProducesResponseType(typeof(IEnumerable<Eleve>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<IEnumerable<Eleve>>> GetElevesByEcoleAll(
            int idEcole,
            [FromQuery] bool? statut = true)
        {
            // Validation de l'ID école
            if (idEcole <= 0)
            {
                return BadRequest(new { message = "L'ID de l'école est requis et doit être supérieur à 0" });
            }

            // Vérifier que l'utilisateur a accès à cette école (sauf Super-Admin / IT-Support)
            var currentUserEcoleId = this.GetCurrentUserSchoolId();
            var canAccessAllSchools = User.IsInRole(UserRoles.SUPER_ADMIN) || User.IsInRole(UserRoles.IT_SUPPORT);

            if (!canAccessAllSchools && (!currentUserEcoleId.HasValue || currentUserEcoleId.Value != idEcole))
            {
                return StatusCode(403, new { message = "Vous n'avez pas accès à cette école" });
            }

            try
            {
                // Récupérer tous les élèves de l'école
                var allEleves = await _eleveRepository.GetByEcoleAsync(idEcole);

                // Filtrer par statut si spécifié
                if (statut.HasValue)
                {
                    allEleves = allEleves.Where(e => e.Statut == statut.Value);
                }

                var elevesList = allEleves.ToList();

                // Retourner avec métadonnées pour information
                return Ok(new
                {
                    data = elevesList,
                    totalCount = elevesList.Count,
                    idEcole = idEcole,
                    statut = statut.HasValue ? (statut.Value ? "Actifs" : "Inactifs") : "Tous",
                    exportedAt = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Erreur lors de la récupération des élèves", 
                    error = ex.Message 
                });
            }
        }

        // ✅ GET: api/Eleve/ecole/{idEcole}/nom-complet?nomComplet=...&PageNumber=1&PageSize=15
        /// <summary>
        /// Récupère les élèves d'une école filtrés par nom complet avec pagination
        /// </summary>
        [HttpGet("ecole/{idEcole}/nom-complet")]
        [ProducesResponseType(typeof(PagedResult<Eleve>), 200)]
        public async Task<ActionResult<PagedResult<Eleve>>> GetElevesByEcoleByNomComplet(
            int idEcole,
            [FromQuery] string nomComplet,
            [FromQuery] PagedRequest request)
        {
            if (string.IsNullOrWhiteSpace(nomComplet))
            {
                return BadRequest(new { message = "Le paramètre nomComplet est requis" });
            }

            var result = await _eleveRepository.GetByEcoleByNomCompletPagedAsync(idEcole, nomComplet, request);
            return Ok(result);
        }

        // GET: api/Eleve/statut/true
        [HttpGet("statut/{statut}")]
        public async Task<ActionResult<IEnumerable<Eleve>>> GetElevesByStatut(bool statut)
        {
            var eleves = await _eleveRepository.GetByStatutAsync(statut);
            return Ok(eleves);
        }

        // GET: api/Eleve/5/notes
        [HttpGet("{id}/notes")]
        public async Task<ActionResult<IEnumerable<Note>>> GetNotesByEleve(int id)
        {
            var notes = await _eleveRepository.GetNotesAsync(id);
            return Ok(notes);
        }

        // GET: api/Eleve/5/inscriptions
        [HttpGet("{id}/inscriptions")]
        public async Task<ActionResult<IEnumerable<Inscription>>> GetInscriptionsByEleve(int id)
        {
            var inscriptions = await _eleveRepository.GetInscriptionsAsync(id);
            return Ok(inscriptions);
        }

        // GET: api/Eleve/5/paiements
        [HttpGet("{id}/paiements")]
        public async Task<ActionResult<IEnumerable<Paiement>>> GetPaiementsByEleve(int id)
        {
            var paiements = await _eleveRepository.GetPaiementsAsync(id);
            return Ok(paiements);
        }

        // GET: api/Eleve/5/presences
        //[HttpGet("{id}/presences")]
        //public async Task<ActionResult<IEnumerable<Presence>>> GetPresencesByEleve(int id)
        //{
        //    var presences = await _eleveRepository.GetPresencesAsync(id);
        //    return Ok(presences);
        //}

        // GET: api/Eleve/5/documents
        [HttpGet("{id}/documents")]
        public async Task<ActionResult<IEnumerable<Document>>> GetDocumentsByEleve(int id)
        {
            var documents = await _eleveRepository.GetDocumentsAsync(id);
            return Ok(documents);
        }

        // GET: api/Eleve/exists/5
        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> EleveExists(int id)
        {
            var exists = await _eleveRepository.ExistsAsync(id);
            return Ok(exists);
        }

        // GET: api/Eleve/exists/reference/ELEVE-20240814-ABC12345
        [HttpGet("exists/reference/{reference}")]
        public async Task<ActionResult<bool>> EleveExistsByReference(Guid reference)
        {
            var exists = await _eleveRepository.ExistsByReferenceAsync(reference);
            return Ok(exists);
        }

        // POST: api/Eleve
        [HttpPost]
        public async Task<ActionResult<Eleve>> CreateEleve(Eleve eleve)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdEleve = await _eleveRepository.CreateAsync(eleve);
                return CreatedAtAction(nameof(GetEleve), new { id = createdEleve.IdEleve }, createdEleve);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la création de l'élève", error = ex.Message });
            }
        }

        // POST: api/Eleve/batch
        [HttpPost("batch")]
        public async Task<ActionResult<object>> CreateElevesBatch(IEnumerable<Eleve> eleves)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var eleveList = eleves.ToList();
            if (!eleveList.Any())
            {
                return BadRequest(new { message = "La liste des élèves est vide" });
            }

            try
            {
                var createdEleves = await _eleveRepository.CreateBatchAsync(eleveList);
                return Ok(new { 
                    message = $"{createdEleves.Count()} élève(s) créé(s) avec succès",
                    total = eleveList.Count,
                    success = createdEleves.Count(),
                    failed = eleveList.Count - createdEleves.Count(),
                    eleves = createdEleves
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la création par lot", error = ex.Message });
            }
        }

        // PUT: api/Eleve/5
        /// <summary>
        /// Modifier les informations d'un élève (Admin uniquement)
        /// </summary>
        /// <remarks>
        /// Permet de modifier les informations d'un élève.
        /// 
        /// Champs modifiables :
        /// - Informations personnelles (nom, prénom, date naissance, etc.)
        /// - Adresse complète
        /// - Tuteur et Classe (assignation)
        /// 
        /// Champs protégés :
        /// - Matricule (auto-généré)
        /// - SerialNumber (endpoint dédié)
        /// - ReferenceEleve (Guid, immuable)
        /// - Statut (endpoint toggle-statut)
        /// </remarks>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Super-Admin")]
        [ProducesResponseType(typeof(Eleve), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Eleve>> UpdateEleve(int id, [FromBody] UpdateEleveDto dto)
        {
            if (id != dto.IdEleve)
            {
                return BadRequest(new { message = "L'ID dans l'URL ne correspond pas à l'ID dans le corps" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingEleve = await _eleveRepository.GetByIdAsync(id);
            if (existingEleve == null)
            {
                return NotFound(new { message = "Élève non trouvé" });
            }

            // 📸 AUDIT: Snapshot AVANT
            var oldEleve = new Eleve
            {
                IdEleve = existingEleve.IdEleve,
                Nom = existingEleve.Nom,
                Postnom = existingEleve.Postnom,
                Prenom = existingEleve.Prenom,
                Genre = existingEleve.Genre,
                PhotoUrl = existingEleve.PhotoUrl,
                Commentaire = existingEleve.Commentaire,
                IdTuteur = existingEleve.IdTuteur,
                IdClasse = existingEleve.IdClasse
            };

            // Mettre à jour seulement les champs autorisés
            existingEleve.Nom = dto.Nom;
            existingEleve.Postnom = dto.Postnom;
            existingEleve.Prenom = dto.Prenom;
            existingEleve.Genre = dto.Genre;
            if (dto.DateNaissance.HasValue)
                existingEleve.DateNaissance = dto.DateNaissance.Value;
            existingEleve.LieuNaissance = dto.LieuNaissance;
            existingEleve.Nationalite = dto.Nationalite;
            existingEleve.PhotoUrl = dto.PhotoUrl;
            existingEleve.Commentaire = dto.Commentaire;
            
            // Adresse
            existingEleve.Ville = dto.Ville;
            existingEleve.Commune = dto.Commune;
            existingEleve.Quartier = dto.Quartier;
            existingEleve.Avenue = dto.Avenue;
            existingEleve.Numero = dto.Numero;
            existingEleve.Province = dto.Province;
            
            // Relations
            existingEleve.IdTuteur = dto.IdTuteur;
            existingEleve.IdClasse = dto.IdClasse;
            
            // Recalculer NomComplet automatiquement (format standardisé : Nom Postnom Prenom)
            existingEleve.NomComplet = $"{dto.Nom} {dto.Postnom} {dto.Prenom}".Trim();
            
            // Champs protégés (JAMAIS modifiés ici)
            // ❌ Matricule → Immuable
            // ❌ SerialNumber → Endpoint dédié
            // ❌ ReferenceEleve → Immuable
            // ❌ Statut → Endpoint toggle-statut

            var updatedEleve = await _eleveRepository.UpdateAsync(existingEleve);
            if (updatedEleve == null)
            {
                return StatusCode(500, new { message = "Erreur lors de la mise à jour" });
            }

            // 📝 AUDIT
            var ctx = this.GetAuditContext();
            await _auditService.LogUpdateAsync(oldEleve, updatedEleve, ctx.UserId, ctx.UserName, ctx.UserRole, ctx.IdEcole, ctx.IpAddress, ctx.UserAgent, "Modification élève");

            return Ok(updatedEleve);
        }

        // DELETE: api/Eleve/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEleve(int id)
        {
            var eleve = await _eleveRepository.GetByIdAsync(id);
            if (eleve == null)
            {
                return NotFound();
            }

            // Vérifier s'il y a des inscriptions associées
            var inscriptions = await _eleveRepository.GetInscriptionsAsync(id);
            if (inscriptions.Any())
            {
                return BadRequest("Impossible de supprimer cet élève car il a des inscriptions associées.");
            }

            var success = await _eleveRepository.DeleteAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        // PUT: api/Eleve/toggle-statut/{id}
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _eleveRepository.ToggleStatutAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Élève non trouvé" });
                }

                // Note: GetByIdAsync retournera null si l'élève est désactivé (filtre Statut = true)
                // Utiliser GetByStatutAsync pour confirmer le nouveau statut
                var eleveActif = await _eleveRepository.GetByIdAsync(id);
                var estActif = eleveActif != null;
                
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = estActif,
                    eleve = eleveActif
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors du changement de statut", error = ex.Message });
            }
        }

        // ✅ PUT: api/Eleve/{idEleve}/serial-number
        // Mise à jour du Serial Number par IdEleve
        [HttpPut("{idEleve}/serial-number")]
        [Authorize(Roles = "Admin,Directeur,Super-Admin,IT-Support")]
        public async Task<IActionResult> UpdateSerialNumberById(int idEleve, [FromBody] UpdateSerialNumberDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var eleveAvant = await _eleveRepository.GetByIdAsync(idEleve);
                if (eleveAvant == null)
                {
                    return NotFound(new { message = $"Élève avec l'ID {idEleve} non trouvé" });
                }

                var deny = this.ForbidIfWrongSchool(eleveAvant.Classe?.Direction?.IdEcole);
                if (deny != null) return deny;

                var success = await _eleveRepository.UpdateSerialNumberByIdAsync(idEleve, dto.SerialNumber);
                if (!success)
                {
                    return NotFound(new { message = $"Élève avec l'ID {idEleve} non trouvé" });
                }

                var eleve = await _eleveRepository.GetByIdAsync(idEleve);
                return Ok(new
                {
                    message = "Numéro de série mis à jour avec succès",
                    idEleve = idEleve,
                    serialNumber = dto.SerialNumber,
                    eleve = eleve
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la mise à jour du numéro de série", error = ex.Message });
            }
        }

        // ✅ PUT: api/Eleve/matricule/{matricule}/serial-number
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
                var eleveAvant = await _eleveRepository.GetByMatriculeAsync(matricule);
                if (eleveAvant == null)
                {
                    return NotFound(new { message = $"Élève avec le matricule '{matricule}' non trouvé" });
                }

                var deny = this.ForbidIfWrongSchool(eleveAvant.Classe?.Direction?.IdEcole);
                if (deny != null) return deny;

                var success = await _eleveRepository.UpdateSerialNumberByMatriculeAsync(matricule, dto.SerialNumber);
                if (!success)
                {
                    return NotFound(new { message = $"Élève avec le matricule '{matricule}' non trouvé" });
                }

                var eleve = await _eleveRepository.GetByMatriculeAsync(matricule);
                return Ok(new
                {
                    message = "Numéro de série mis à jour avec succès",
                    matricule = matricule,
                    serialNumber = dto.SerialNumber,
                    eleve = eleve
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la mise à jour du numéro de série", error = ex.Message });
            }
        }
    }
}
