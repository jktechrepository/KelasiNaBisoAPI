using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.DTOs.Pagination;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Attributes;
using KelasiNaBiso.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace KelasiNaBiso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔒 Paiements - Token JWT requis
    public class PaiementController : ControllerBase
    {
        private readonly IPaiementRepository _paiementRepository;
        private readonly IAuditService _auditService;

        public PaiementController(IPaiementRepository paiementRepository, IAuditService auditService)
        {
            _paiementRepository = paiementRepository;
            _auditService = auditService;
        }

        // ✅ GET: api/Paiement/paged?idEcole=&idAnneeScolaire=&idUtilisateur=
        [HttpGet("paged")]
        [ProducesResponseType(typeof(ElevesAnneeScopedResult<PagedResult<Paiement>>), 200)]
        public async Task<IActionResult> GetPaiementsPaged(
            [FromQuery] PagedRequest request,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null,
            [FromQuery] int? idUtilisateur = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _paiementRepository.GetAllPagedAsync(
                    resolvedEcole, request, idAnneeScolaire, idUtilisateur));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ✅ GET: api/Paiement/cursor-paged?idEcole=&idAnneeScolaire=&idUtilisateur=
        [HttpGet("cursor-paged")]
        [ProducesResponseType(typeof(ElevesAnneeScopedResult<CursorPaginatedResult<Paiement>>), 200)]
        public async Task<IActionResult> GetPaiementsCursorPaged(
            [FromQuery] CursorPaginationRequest request,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null,
            [FromQuery] int? idUtilisateur = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _paiementRepository.GetAllCursorPagedAsync(
                    resolvedEcole, request, idAnneeScolaire, idUtilisateur));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ✅ GET: api/Paiement/eleve/5/paged?idAnneeScolaire=
        [HttpGet("eleve/{idEleve}/paged")]
        [ProducesResponseType(typeof(ElevesAnneeScopedResult<PagedResult<Paiement>>), 200)]
        public async Task<IActionResult> GetPaiementsByElevePaged(
            int idEleve,
            [FromQuery] PagedRequest request,
            [FromQuery] int? idAnneeScolaire = null)
        {
            try
            {
                return Ok(await _paiementRepository.GetByElevePagedAsync(idEleve, request, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ✅ GET: api/Paiement/date-range/paged?idEcole=&idAnneeScolaire=
        [HttpGet("date-range/paged")]
        [ProducesResponseType(typeof(ElevesAnneeScopedResult<PagedResult<Paiement>>), 200)]
        public async Task<IActionResult> GetPaiementsByDateRangePaged(
            [FromQuery] DateTime dateDebut,
            [FromQuery] DateTime dateFin,
            [FromQuery] PagedRequest request,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _paiementRepository.GetByDateRangePagedAsync(
                    resolvedEcole, dateDebut, dateFin, request, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ⚠️ GET: api/Paiement?idEcole=&idAnneeScolaire= (DEPRECATED)
        [HttpGet]
        [Obsolete("Cette méthode n'est pas paginée. Utilisez GET /api/Paiement/paged")]
        [RequireGlobalAccess]
        public async Task<IActionResult> GetPaiements(
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _paiementRepository.GetAllAsync(resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/Paiement/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Paiement>> GetPaiement(int id)
        {
            var paiement = await _paiementRepository.GetByIdAsync(id);
            if (paiement == null)
            {
                return NotFound();
            }
            return Ok(paiement);
        }

        // GET: api/Paiement/eleve/5?idAnneeScolaire=
        [HttpGet("eleve/{idEleve}")]
        public async Task<IActionResult> GetPaiementsByEleve(
            int idEleve,
            [FromQuery] int? idAnneeScolaire = null)
        {
            try
            {
                return Ok(await _paiementRepository.GetByEleveAsync(idEleve, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/Paiement/frais/5
        [HttpGet("frais/{idFrais}")]
        public async Task<ActionResult<IEnumerable<Paiement>>> GetPaiementsByFrais(int idFrais)
        {
            var paiements = await _paiementRepository.GetByFraisAsync(idFrais);
            return Ok(paiements);
        }

        // GET: api/Paiement/ecole/5?idAnneeScolaire=
        [HttpGet("ecole/{idEcole}")]
        public async Task<IActionResult> GetPaiementsByEcole(
            int idEcole, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 15,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var deny = this.ForbidIfWrongSchool(idEcole);
            if (deny != null)
                return deny;

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 15;
            if (pageSize > 100) pageSize = 100;

            try
            {
                var scoped = await _paiementRepository.GetByEcoleAsync(idEcole, idAnneeScolaire);
                var allPaiements = scoped.Data.ToList();
            
                var paiementsPaginated = allPaiements
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var totalCount = allPaiements.Count;
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                return Ok(new
                {
                    data = paiementsPaginated.Select(PaiementListItemDto.FromEntity).ToList(),
                    idEcole = scoped.IdEcole,
                    idAnneeScolaire = scoped.IdAnneeScolaire,
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
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/Paiement/mode/Carte
        [HttpGet("mode/{modePaiement}")]
        //public async Task<ActionResult<IEnumerable<Paiement>>> GetPaiementsByMode(string modePaiement)
        //{
        //    var paiements = await _paiementRepository.GetByModeAsync(modePaiement);
        //    return Ok(paiements);
        //}

        // GET: api/Paiement/statut/true
        [HttpGet("statut/{statut}")]
        //public async Task<ActionResult<IEnumerable<Paiement>>> GetPaiementsByStatut(bool statut)
        //{
        //    var paiements = await _paiementRepository.GetByStatutAsync(statut);
        //    return Ok(paiements);
        //}

        // GET: api/Paiement/date/2024-01-15
        [HttpGet("date/{date}")]
        //public async Task<ActionResult<IEnumerable<Paiement>>> GetPaiementsByDate(DateTime date)
        //{
        //    var paiements = await _paiementRepository.GetByDateAsync(date);
        //    return Ok(paiements);
        //}

        // GET: api/Paiement/exists/5
        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> PaiementExists(int id)
        {
            var exists = await _paiementRepository.ExistsAsync(id);
            return Ok(exists);
        }

        // POST: api/Paiement
        [HttpPost]
        [Permission("Paiement.Create")]
        public async Task<ActionResult<Paiement>> CreatePaiement(Paiement paiement)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdPaiement = await _paiementRepository.CreateAsync(paiement);
                return CreatedAtAction(nameof(GetPaiement), new { id = createdPaiement.IdPaiement }, createdPaiement);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message, code = "MOKO_PAYIN_REQUIRED" });
            }
        }

        // POST: api/Paiement/batch
        [HttpPost("batch")]
        [Permission("Paiement.Create")]
        public async Task<ActionResult<object>> CreatePaiementsBatch(IEnumerable<Paiement> paiements)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var paiementList = paiements.ToList();
            if (!paiementList.Any())
            {
                return BadRequest(new { message = "La liste des paiements est vide" });
            }

            try
            {
                var createdPaiements = await _paiementRepository.CreateBatchAsync(paiementList);
                return Ok(new { 
                    message = $"{createdPaiements.Count()} paiement(s) créé(s) avec succès",
                    total = paiementList.Count,
                    success = createdPaiements.Count(),
                    failed = paiementList.Count - createdPaiements.Count(),
                    paiements = createdPaiements
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la création par lot", error = ex.Message });
            }
        }

        // GET: api/Paiement/template-excel
        /// <summary>
        /// Télécharge un template Excel pour les paiements en lot
        /// </summary>
        [HttpGet("template-excel")]
        [Authorize(Roles = "Admin,Super-Admin,Financier,Caissier")]
        [Permission("Paiement.Create")]
        [ProducesResponseType(typeof(FileResult), 200)]
        public IActionResult DownloadExcelTemplate()
        {
            try
            {
                var excelService = HttpContext.RequestServices.GetRequiredService<ExcelPaiementServiceV2>();
                var fileBytes = excelService.GenerateExcelTemplate();
                
                return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Template_Paiements_{DateTime.Now:yyyyMMdd}.xlsx"
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Erreur lors de la génération du template Excel", 
                    error = ex.Message 
                });
            }
        }

        // POST: api/Paiement/bulk-excel
        /// <summary>
        /// Upload et traitement d'un fichier Excel pour créer plusieurs paiements en lot (Version 2 - Format simplifié)
        /// Le fichier doit contenir les colonnes: DatePaiement, Montant, Devise, ModePaiement, NomCompletEleve, LibelleFrais
        /// L'ID de l'utilisateur et l'ID de l'école sont automatiquement extraits du token JWT
        /// </summary>
        [HttpPost("bulk-excel")]
        [Authorize(Roles = "Admin,Super-Admin,Financier,Caissier")]
        [Permission("Paiement.Create")]
        [ProducesResponseType(typeof(BulkPaiementResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<BulkPaiementResult>> BulkPaiementFromExcel(
            [FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Le fichier Excel est requis" });
            }

            // Extraire l'ID de l'utilisateur depuis le token JWT
            var idUtilisateur = this.GetCurrentUserId();
            if (idUtilisateur <= 0)
            {
                return Unauthorized(new { message = "Token JWT invalide ou utilisateur non identifié" });
            }

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
                var excelService = HttpContext.RequestServices.GetRequiredService<ExcelPaiementServiceV2>();
                var result = await excelService.ProcessExcelFileAsync(file, idEcole.Value, idUtilisateur);

                if (result.Success || result.LignesReussies > 0)
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
                return StatusCode(500, new { 
                    message = "Erreur lors du traitement du fichier Excel", 
                    error = ex.Message 
                });
            }
        }

        // PUT: api/Paiement/5
        /// <summary>
        /// Modifier les informations d'un paiement (Admin et Super-Admin uniquement)
        /// </summary>
        /// <remarks>
        /// Permet de mettre à jour certains champs d'un paiement (commentaire, justificatif, etc.)
        /// Les champs critiques (montant, élève, type frais) sont immuables pour des raisons d'audit.
        /// </remarks>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Super-Admin")]
        [Permission("Paiement.Update")]
        [ProducesResponseType(typeof(Paiement), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Paiement>> UpdatePaiement(int id, [FromBody] UpdatePaiementDto dto)
        {
            if (id != dto.IdPaiement)
            {
                return BadRequest(new { message = "L'ID dans l'URL ne correspond pas à l'ID dans le corps" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingPaiement = await _paiementRepository.GetByIdAsync(id);
            if (existingPaiement == null)
            {
                return NotFound(new { message = "Paiement non trouvé" });
            }

            // 📸 AUDIT: Capturer l'état AVANT modification
            var oldPaiement = new Paiement
            {
                IdPaiement = existingPaiement.IdPaiement,
                Montant = existingPaiement.Montant,
                Commentaire = existingPaiement.Commentaire,
                JustificatifUrl = existingPaiement.JustificatifUrl,
                ReferenceTransaction = existingPaiement.ReferenceTransaction,
                ModePaiement = existingPaiement.ModePaiement,
                StatutPaiement = existingPaiement.StatutPaiement
            };

            // Mettre à jour seulement les champs autorisés
            existingPaiement.Montant = dto.Montant;
            existingPaiement.Commentaire = dto.Commentaire;
            existingPaiement.JustificatifUrl = dto.JustificatifUrl;
            existingPaiement.ReferenceTransaction = dto.ReferenceTransaction;
            existingPaiement.ModePaiement = dto.ModePaiement;
            existingPaiement.StatutPaiement = dto.StatutPaiement;
            
            // Champs protégés (JAMAIS modifiés ici)
            // ❌ IdEleve, IdTypeFrais, IdAnneeScolaire → Immuables
            // ❌ NumeroBordereau → Auto-généré
            // ❌ DatePaiement, HeurePaiement → Immuables

            var updatedPaiement = await _paiementRepository.UpdateAsync(existingPaiement);
            if (updatedPaiement == null)
            {
                return StatusCode(500, new { message = "Erreur lors de la mise à jour" });
            }

            // 📝 AUDIT: Enregistrer la modification
            var auditContext = this.GetAuditContext();
            await _auditService.LogUpdateAsync(
                oldPaiement,
                updatedPaiement,
                auditContext.UserId,
                auditContext.UserName,
                auditContext.UserRole,
                auditContext.IdEcole,
                auditContext.IpAddress,
                auditContext.UserAgent,
                dto.Commentaire // Utiliser le commentaire du DTO comme raison
            );

            return Ok(updatedPaiement);
        }

        // DELETE: api/Paiement/5
        /// <summary>
        /// Supprimer un paiement (Admin et Super-Admin uniquement)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Super-Admin")]
        [Permission("Paiement.Delete")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> DeletePaiement(int id)
        {
            var success = await _paiementRepository.DeleteAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        // PUT: api/Paiement/toggle-statut/{id}
        /// <summary>
        /// Modifier le statut d'un paiement (Admin et Super-Admin uniquement)
        /// </summary>
        [HttpPut("toggle-statut/{id}")]
        [Authorize(Roles = "Admin,Super-Admin")]
        [Permission("Paiement.Update")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _paiementRepository.ToggleStatutAsync(id);
                if (!success)
                    return NotFound(new { message = "Paiement non trouvé" });

                var paiement = await _paiementRepository.GetByIdAsync(id);
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = paiement?.Statut ?? false,
                    paiement = paiement
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur", error = ex.Message });
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // SECTION REPORTING PAIEMENT
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// ✨ Obtient le dashboard de paiement pour une école
        /// </summary>
        /// <param name="idEcole">ID de l'école</param>
        /// <param name="date">Date spécifique (optionnel)</param>
        /// <param name="dateDebut">Date de début (optionnel)</param>
        /// <param name="dateFin">Date de fin (optionnel)</param>
        /// <param name="periode">Période prédéfinie: semaine, mois, trimestre, annee (optionnel)</param>
        [HttpGet("dashboard/ecole/{idEcole}")]
        public async Task<ActionResult> GetDashboardEcole(
            int idEcole,
            [FromQuery] DateTime? date,
            [FromQuery] DateTime? dateDebut,
            [FromQuery] DateTime? dateFin,
            [FromQuery] string? periode,
            [FromQuery] int? idAnneeScolaire = null)
        {
            try
            {
                var paiementService = _paiementRepository as PaiementService;
                if (paiementService == null)
                {
                    return StatusCode(500, new { message = "Service non disponible" });
                }

                var result = await paiementService.GetDashboardEcoleAsync(
                    idEcole, date, dateDebut, dateFin, periode, idAnneeScolaire);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la récupération du dashboard", error = ex.Message });
            }
        }

        /// <summary>
        /// ✨ Obtient le taux de paiement d'un élève sur une période
        /// </summary>
        [HttpGet("eleves/{idEleve}/taux")]
        public async Task<ActionResult> GetEleveTaux(
            int idEleve,
            [FromQuery] DateTime? dateDebut,
            [FromQuery] DateTime? dateFin,
            [FromQuery] string? periode,
            [FromQuery] int? idAnneeScolaire = null)
        {
            try
            {
                var paiementService = _paiementRepository as PaiementService;
                if (paiementService == null)
                {
                    return StatusCode(500, new { message = "Service non disponible" });
                }

                var result = await paiementService.GetTauxPaiementEleveAsync(
                    idEleve, dateDebut, dateFin, periode, idAnneeScolaire);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors du calcul du taux de paiement", error = ex.Message });
            }
        }

        /// <summary>
        /// ✨ Obtient le taux de paiement d'une classe sur une période
        /// </summary>
        [HttpGet("classe/{idClasse}/taux")]
        public async Task<ActionResult> GetClasseTaux(
            int idClasse,
            [FromQuery] DateTime? date,
            [FromQuery] DateTime? dateDebut,
            [FromQuery] DateTime? dateFin,
            [FromQuery] string? periode,
            [FromQuery] bool includeDetails = false,
            [FromQuery] int? idAnneeScolaire = null)
        {
            try
            {
                var paiementService = _paiementRepository as PaiementService;
                if (paiementService == null)
                {
                    return StatusCode(500, new { message = "Service non disponible" });
                }

                var result = await paiementService.GetTauxPaiementClasseAsync(
                    idClasse, date, dateDebut, dateFin, periode, includeDetails, idAnneeScolaire);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors du calcul du taux de recouvrement", error = ex.Message });
            }
        }
    }
}
