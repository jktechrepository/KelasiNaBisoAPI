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
        private readonly IInscriptionActiveResolver _inscriptionResolver;

        public EleveController(
            IEleveRepository eleveRepository,
            IAuditService auditService,
            IInscriptionActiveResolver inscriptionResolver)
        {
            _eleveRepository = eleveRepository;
            _auditService = auditService;
            _inscriptionResolver = inscriptionResolver;
        }

        /// <summary>
        /// Résout idEcole depuis la query ou le claim JWT. Super-Admin/IT-Support doivent préciser idEcole en query.
        /// </summary>
        private IActionResult? TryResolveListIdEcole(int? idEcoleQuery, out int idEcole) =>
            EleveListScopeHelper.TryResolveListIdEcole(this, idEcoleQuery, out idEcole);

        // ✅ GET: api/Eleve/paged?idEcole=&idAnneeScolaire=&idClasse=&idDirection=
        /// <summary>
        /// Récupère les élèves de l'école (année scolaire en cours par défaut) avec pagination offset-based.
        /// Filtres optionnels : idClasse (prioritaire) ou idDirection via inscriptions confirmées de l'année.
        /// </summary>
        [HttpGet("paged")]
        [ProducesResponseType(typeof(ElevesAnneeScopedResult<PagedResult<V_Eleve>>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetElevesPaged(
            [FromQuery] PagedRequest request,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null,
            [FromQuery] int? idClasse = null,
            [FromQuery] int? idDirection = null)
        {
            var resolveError = TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                var result = await _eleveRepository.GetAllPagedAsync(
                    resolvedEcole, request, idAnneeScolaire, idClasse, idDirection);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ✅ GET: api/Eleve/cursor-paged?idEcole=&idAnneeScolaire=&idClasse=&idDirection=
        /// <summary>
        /// Récupère les élèves de l'école (année scolaire en cours par défaut) avec pagination cursor-based.
        /// Filtres optionnels : idClasse (prioritaire) ou idDirection via inscriptions confirmées de l'année.
        /// </summary>
        [HttpGet("cursor-paged")]
        [ProducesResponseType(typeof(ElevesAnneeScopedResult<CursorPaginatedResult<V_Eleve>>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetElevesCursorPaged(
            [FromQuery] CursorPaginationRequest request,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null,
            [FromQuery] int? idClasse = null,
            [FromQuery] int? idDirection = null)
        {
            var resolveError = TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                var result = await _eleveRepository.GetAllCursorPagedAsync(
                    resolvedEcole, request, idAnneeScolaire, idClasse, idDirection);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ⚠️ GET: api/Eleve?idEcole=&idAnneeScolaire= (DEPRECATED - À ÉVITER, NON PAGINÉ)
        /// <summary>
        /// [DEPRECATED] Récupère les élèves de l'école pour l'année scolaire en cours (ou idAnneeScolaire).
        /// Utiliser plutôt GET /api/Eleve/paged
        /// </summary>
        [HttpGet]
        [Obsolete("Cette méthode n'est pas paginée et peut causer des problèmes de performance. Utilisez GET /api/Eleve/paged")]
        [ProducesResponseType(typeof(ElevesAnneeScopedResult<IReadOnlyList<EleveParEcoleListItemDto>>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetEleves(
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                var result = await _eleveRepository.GetAllAsync(resolvedEcole, idAnneeScolaire);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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

            var idEcole = await _inscriptionResolver.GetEcoleCouranteAsync(eleve.IdEleve);
            var deny = this.ForbidIfWrongSchool(idEcole);
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

        // ✅ GET: api/Eleve/classe/5/paged?idAnneeScolaire=
        /// <summary>
        /// Récupère les élèves d'une classe (année scolaire en cours par défaut) avec pagination
        /// </summary>
        [HttpGet("classe/{idClasse}/paged")]
        [ProducesResponseType(typeof(ElevesAnneeScopedResult<PagedResult<Eleve>>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ElevesAnneeScopedResult<PagedResult<Eleve>>>> GetElevesByClassePaged(
            int idClasse,
            [FromQuery] PagedRequest request,
            [FromQuery] int? idAnneeScolaire = null)
        {
            try
            {
                var result = await _eleveRepository.GetByClassePagedAsync(idClasse, request, idAnneeScolaire);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ⚠️ GET: api/Eleve/classe/5?idAnneeScolaire= (DEPRECATED - NON PAGINÉ)
        [HttpGet("classe/{idClasse}")]
        [Obsolete("Utiliser GET /api/Eleve/classe/{idClasse}/paged pour pagination")]
        [ProducesResponseType(typeof(ElevesAnneeScopedResult<IReadOnlyList<Eleve>>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ElevesAnneeScopedResult<IReadOnlyList<Eleve>>>> GetElevesByClasse(
            int idClasse,
            [FromQuery] int? idAnneeScolaire = null)
        {
            try
            {
                var result = await _eleveRepository.GetByClasseAsync(idClasse, idAnneeScolaire);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/Eleve/tuteur/5
        [HttpGet("tuteur/{idTuteur}")]
        public async Task<ActionResult<IEnumerable<Eleve>>> GetElevesByTuteur(int idTuteur)
        {
            var eleves = await _eleveRepository.GetByTuteurAsync(idTuteur);
            return Ok(eleves);
        }

        // GET: api/Eleve/ecole/5?page=1&pageSize=15&idAnneeScolaire=12
        [HttpGet("ecole/{idEcole}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> GetElevesByEcole(
            int idEcole,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 15,
            [FromQuery] int? idAnneeScolaire = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 15;
            if (pageSize > 100) pageSize = 100;

            try
            {
                var scoped = await _eleveRepository.GetByEcoleAsync(idEcole, idAnneeScolaire);
                var allEleves = scoped.Data;

                var elevesPaginated = allEleves
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var totalCount = allEleves.Count;
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

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
                    },
                    idEcole = scoped.IdEcole,
                    idAnneeScolaire = scoped.IdAnneeScolaire
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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
        public async Task<ActionResult<IEnumerable<EleveParEcoleListItemDto>>> GetElevesByEcoleAllFromToken(
            [FromQuery] bool? statut = true,
            [FromQuery] int? idAnneeScolaire = null)
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
                var scoped = await _eleveRepository.GetByEcoleAsync(idEcole.Value, idAnneeScolaire);

                // Filtrer par statut si spécifié
                IEnumerable<EleveParEcoleListItemDto> filtered = scoped.Data;
                if (statut.HasValue)
                {
                    filtered = scoped.Data.Where(e => e.Statut == statut.Value);
                }

                var elevesList = filtered.ToList();

                // Retourner avec métadonnées pour information
                return Ok(new
                {
                    data = elevesList,
                    totalCount = elevesList.Count,
                    idEcole = scoped.IdEcole,
                    idAnneeScolaire = scoped.IdAnneeScolaire,
                    statut = statut.HasValue ? (statut.Value ? "Actifs" : "Inactifs") : "Tous",
                    exportedAt = DateTime.Now
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
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
        public async Task<ActionResult<IEnumerable<EleveParEcoleListItemDto>>> GetElevesByEcoleAll(
            int idEcole,
            [FromQuery] bool? statut = true,
            [FromQuery] int? idAnneeScolaire = null)
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
                var scoped = await _eleveRepository.GetByEcoleAsync(idEcole, idAnneeScolaire);

                IEnumerable<EleveParEcoleListItemDto> filtered = scoped.Data;
                if (statut.HasValue)
                {
                    filtered = scoped.Data.Where(e => e.Statut == statut.Value);
                }

                var elevesList = filtered.ToList();

                return Ok(new
                {
                    data = elevesList,
                    totalCount = elevesList.Count,
                    idEcole = scoped.IdEcole,
                    idAnneeScolaire = scoped.IdAnneeScolaire,
                    statut = statut.HasValue ? (statut.Value ? "Actifs" : "Inactifs") : "Tous",
                    exportedAt = DateTime.Now
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Erreur lors de la récupération des élèves", 
                    error = ex.Message 
                });
            }
        }

        // GET: api/Eleve/reinscription?matricule=... | ?nomComplet=...&idEcole=&idAnneeScolaire=&idClasse=
        /// <summary>
        /// Recherche un élève pour pré-remplir le formulaire de réinscription (école JWT, année N-1 par défaut).
        /// </summary>
        [HttpGet("reinscription")]
        [ProducesResponseType(typeof(EleveReinscriptionPrefillDto), 200)]
        [ProducesResponseType(typeof(ElevesAnneeScopedResult<PagedResult<EleveReinscriptionPrefillDto>>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetReinscriptionPrefill(
            [FromQuery] string? matricule,
            [FromQuery] string? nomComplet,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null,
            [FromQuery] int? idClasse = null,
            [FromQuery] PagedRequest request = null!)
        {
            var resolveError = TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            var hasMatricule = !string.IsNullOrWhiteSpace(matricule);
            var hasNomComplet = !string.IsNullOrWhiteSpace(nomComplet);

            if (hasMatricule && hasNomComplet)
            {
                return BadRequest(new
                {
                    message = "Fournissez soit matricule, soit nomComplet, pas les deux."
                });
            }

            if (!hasMatricule && !hasNomComplet)
            {
                return BadRequest(new
                {
                    message = "Le paramètre matricule ou nomComplet est requis."
                });
            }

            try
            {
                if (hasMatricule)
                {
                    var result = await _eleveRepository.GetReinscriptionPrefillByMatriculeAsync(
                        resolvedEcole,
                        matricule!.Trim(),
                        idAnneeScolaire,
                        idClasse);

                    if (result == null)
                    {
                        return NotFound(new
                        {
                            message = $"Aucun élève trouvé avec le matricule '{matricule}' pour cette école et l'année de référence."
                        });
                    }

                    return Ok(result);
                }

                var paged = await _eleveRepository.SearchReinscriptionPrefillByNomCompletPagedAsync(
                    resolvedEcole,
                    nomComplet!.Trim(),
                    request ?? new PagedRequest(),
                    idAnneeScolaire,
                    idClasse);

                return Ok(paged);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ✅ GET: api/Eleve/ecole/{idEcole}/nom-complet?nomComplet=...&PageNumber=1&PageSize=15&idAnneeScolaire=
        /// <summary>
        /// Récupère les élèves d'une école filtrés par nom complet avec pagination
        /// </summary>
        [HttpGet("ecole/{idEcole}/nom-complet")]
        [ProducesResponseType(typeof(PagedResult<EleveParEcoleListItemDto>), 200)]
        public async Task<ActionResult<PagedResult<EleveParEcoleListItemDto>>> GetElevesByEcoleByNomComplet(
            int idEcole,
            [FromQuery] string nomComplet,
            [FromQuery] PagedRequest request,
            [FromQuery] int? idAnneeScolaire = null)
        {
            if (string.IsNullOrWhiteSpace(nomComplet))
            {
                return BadRequest(new { message = "Le paramètre nomComplet est requis" });
            }

            try
            {
                var result = await _eleveRepository.GetByEcoleByNomCompletPagedAsync(idEcole, nomComplet, request, idAnneeScolaire);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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
                IdTuteur = existingEleve.IdTuteur
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

                var idEcole = await _inscriptionResolver.GetEcoleCouranteAsync(eleveAvant.IdEleve);
                var deny = this.ForbidIfWrongSchool(idEcole);
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

                var idEcole = await _inscriptionResolver.GetEcoleCouranteAsync(eleveAvant.IdEleve);
                var deny = this.ForbidIfWrongSchool(idEcole);
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
