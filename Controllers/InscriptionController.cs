using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.DTOs.Pagination;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Services;
using KelasiNaBiso.Attributes;
using KelasiNaBiso.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace KelasiNaBiso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔒 Inscriptions - Token JWT requis
    public class InscriptionController : ControllerBase
    {
        private readonly IInscriptionRepository _inscriptionRepository;
        private readonly IAuditService _auditService;

        public InscriptionController(IInscriptionRepository inscriptionRepository, IAuditService auditService)
        {
            _inscriptionRepository = inscriptionRepository;
            _auditService = auditService;
        }

        // ✅ GET: api/Inscription/paged (NOUVELLE VERSION PAGINÉE - RECOMMANDÉE)
        /// <summary>
        /// Récupère toutes les inscriptions avec pagination offset-based
        /// </summary>
        [HttpGet("paged")]
        [ProducesResponseType(typeof(PagedResult<Inscription>), 200)]
        public async Task<ActionResult<PagedResult<Inscription>>> GetInscriptionsPaged([FromQuery] PagedRequest request)
        {
            var result = await _inscriptionRepository.GetAllPagedAsync(request);
            return Ok(result);
        }

        // GET: api/Inscription (VERSION NON PAGINÉE - DEPRECATED)
        [HttpGet]
        [Obsolete("Utiliser /api/Inscription/paged pour la pagination")]
        public async Task<ActionResult<IEnumerable<Inscription>>> GetInscriptions()
        {
            var inscriptions = await _inscriptionRepository.GetAllAsync();
            return Ok(inscriptions);
        }

        // GET: api/Inscription/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Inscription>> GetInscription(int id)
        {
            var inscription = await _inscriptionRepository.GetByIdAsync(id);
            if (inscription == null)
            {
                return NotFound();
            }
            return Ok(inscription);
        }

        // ✅ GET: api/Inscription/eleve/{idEleve}/paged (NOUVELLE VERSION PAGINÉE)
        /// <summary>
        /// Récupère les inscriptions d'un élève avec pagination
        /// </summary>
        [HttpGet("eleve/{idEleve}/paged")]
        [ProducesResponseType(typeof(PagedResult<Inscription>), 200)]
        public async Task<ActionResult<PagedResult<Inscription>>> GetInscriptionsByElevePaged(int idEleve, [FromQuery] PagedRequest request)
        {
            var result = await _inscriptionRepository.GetByElevePagedAsync(idEleve, request);
            return Ok(result);
        }

        // GET: api/Inscription/eleve/5 (VERSION NON PAGINÉE - DEPRECATED)
        [HttpGet("eleve/{idEleve}")]
        [Obsolete("Utiliser /api/Inscription/eleve/{idEleve}/paged pour la pagination")]
        public async Task<ActionResult<IEnumerable<Inscription>>> GetInscriptionsByEleve(int idEleve)
        {
            var inscriptions = await _inscriptionRepository.GetByEleveAsync(idEleve);
            return Ok(inscriptions);
        }

        // ✅ GET: api/Inscription/classe/{idClasse}/paged (NOUVELLE VERSION PAGINÉE)
        /// <summary>
        /// Récupère les inscriptions d'une classe avec pagination
        /// </summary>
        [HttpGet("classe/{idClasse}/paged")]
        [ProducesResponseType(typeof(PagedResult<Inscription>), 200)]
        public async Task<ActionResult<PagedResult<Inscription>>> GetInscriptionsByClassePaged(int idClasse, [FromQuery] PagedRequest request)
        {
            var result = await _inscriptionRepository.GetByClassePagedAsync(idClasse, request);
            return Ok(result);
        }

        // GET: api/Inscription/classe/5 (VERSION NON PAGINÉE - DEPRECATED)
        [HttpGet("classe/{idClasse}")]
        [Obsolete("Utiliser /api/Inscription/classe/{idClasse}/paged pour la pagination")]
        public async Task<ActionResult<IEnumerable<Inscription>>> GetInscriptionsByClasse(int idClasse)
        {
            var inscriptions = await _inscriptionRepository.GetByClasseAsync(idClasse);
            return Ok(inscriptions);
        }

        // ✅ GET: api/Inscription/ecoles/{idecole}/paged (NOUVELLE VERSION PAGINÉE)
        /// <summary>
        /// Récupère les inscriptions d'une école avec pagination
        /// </summary>
        [HttpGet("ecoles/{idecole}/paged")]
        [ProducesResponseType(typeof(PagedResult<Inscription>), 200)]
        public async Task<ActionResult<PagedResult<Inscription>>> GetInscriptionsByEcolePaged(int idecole, [FromQuery] PagedRequest request)
        {
            var result = await _inscriptionRepository.GetByEcolePagedAsync(idecole, request);
            return Ok(result);
        }

        // GET: api/Inscription/ecoles/5 (VERSION NON PAGINÉE - DEPRECATED)
        [HttpGet("ecoles/{idecole}")]
        [Obsolete("Utiliser /api/Inscription/ecoles/{idecole}/paged pour la pagination")]
        public async Task<ActionResult<IEnumerable<Inscription>>> GetInscriptionsByEcole(int idecole)
        {
            var inscriptions = await _inscriptionRepository.GetByEcoleAsync(idecole);
            return Ok(inscriptions);
        }

        // GET: api/Inscription/annee/2024
        [HttpGet("annee/{anneeScolaire}")]
        //public async Task<ActionResult<IEnumerable<Inscription>>> GetInscriptionsByAnnee(int anneeScolaire)
        //{
        //    var inscriptions = await _inscriptionRepository.GetByAnneeAsync(anneeScolaire);
        //    return Ok(inscriptions);
        //}

        // ✅ GET: api/Inscription/statut/{statut}/paged (NOUVELLE VERSION PAGINÉE)
        /// <summary>
        /// Récupère les inscriptions par statut avec pagination
        /// </summary>
        [HttpGet("statut/{statut}/paged")]
        [ProducesResponseType(typeof(PagedResult<Inscription>), 200)]
        public async Task<ActionResult<PagedResult<Inscription>>> GetInscriptionsByStatutPaged(bool statut, [FromQuery] PagedRequest request)
        {
            var result = await _inscriptionRepository.GetByStatutPagedAsync(statut, request);
            return Ok(result);
        }

        // GET: api/Inscription/statut/true (VERSION NON PAGINÉE - DEPRECATED)
        [HttpGet("statut/{statut}")]
        [Obsolete("Utiliser /api/Inscription/statut/{statut}/paged pour la pagination")]
        public async Task<ActionResult<IEnumerable<Inscription>>> GetInscriptionsByStatut(bool statut)
        {
            var inscriptions = await _inscriptionRepository.GetByStatutAsync(statut);
            return Ok(inscriptions);
        }

        // GET: api/Inscription/exists/5
        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> InscriptionExists(int id)
        {
            var exists = await _inscriptionRepository.ExistsAsync(id);
            return Ok(exists);
        }




        /*
        // POST: api/Inscription
        [HttpPost("{nomEcole}")]
        public async Task<ActionResult<InscriptionResult>> CreateInscription(string nomEcole,  CreateInscriptionDto inscriptionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // r�cup�ration  du matricule �l�ve
            inscriptionDto.MatriculeEleve = _inscriptionRepository.GenerateMatriculeEleve(nomEcole, inscriptionDto);


            var result = await _inscriptionRepository.CreateInscriptionWithStoredProcedureAsync(inscriptionDto);

            if (result.Success)
            {
                return CreatedAtAction(nameof(GetInscription), new { id = result.IdInscription }, result);
            }
            else
            {
                return BadRequest(new { error = result.Message });
            }
        } */

        // POST: api/Inscription/new (nouvelle méthode sans procédure stockée)
        [HttpPost("new/{nomEcole}")]
        public async Task<ActionResult<InscriptionResult>> CreateInscriptionNew(string nomEcole, CreateInscriptionDto inscriptionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // récupération du matricule élève
            inscriptionDto.MatriculeEleve = _inscriptionRepository.GenerateMatriculeEleve(nomEcole, inscriptionDto);

            var result = await _inscriptionRepository.CreateInscriptionAsync(inscriptionDto);

            if (result.Success)
            {
                return CreatedAtAction(nameof(GetInscription), new { id = result.IdInscription }, result);
            }
            else
            {
                return BadRequest(new { error = result.Message });
            }
        }

        // GET: api/Inscription/template-excel
        /// <summary>
        /// Télécharge un template Excel pour les inscriptions en lot (Version 2 - Format simplifié)
        /// Colonnes: Type, DateInscription, NomEleve, PostnomEleve, PrenomEleve, GenreEleve, DateNaissanceEleve,
        /// LieuNaissanceEleve, NationaliteEleve, NomCompletTuteur, GenreTuteur, NomClasse, LibelleAnneeScolaire
        /// </summary>
        [HttpGet("template-excel")]
        [Authorize(Roles = "Admin,Super-Admin,Directeur")]
        [ProducesResponseType(typeof(FileResult), 200)]
        public IActionResult DownloadExcelTemplate()
        {
            try
            {
                var excelService = HttpContext.RequestServices.GetRequiredService<ExcelInscriptionServiceV2>();
                var fileBytes = excelService.GenerateExcelTemplate();
                
                return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Template_Inscriptions_{DateTime.Now:yyyyMMdd}.xlsx"
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

        // POST: api/Inscription/bulk-excel
        /// <summary>
        /// Upload et traitement d'un fichier Excel pour créer plusieurs inscriptions en lot (Version 2 - Format simplifié)
        /// Le fichier doit contenir les colonnes: DateInscription, NomEleve, PostnomEleve, PrenomEleve, GenreEleve,
        /// DateNaissanceEleve, LieuNaissanceEleve, NationaliteEleve, NomCompletTuteur, GenreTuteur
        /// L'ID de l'utilisateur et l'ID de l'école sont automatiquement extraits du token JWT
        /// </summary>
        [HttpPost("bulk-excel")]
        [Authorize(Roles = "Admin,Super-Admin,Directeur")]
        [ProducesResponseType(typeof(BulkInscriptionResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<BulkInscriptionResult>> BulkInscriptionFromExcel(
            [FromForm(Name = "file")] IFormFile file,
            [FromQuery] int idClasse,
            [FromQuery] int idAnneeScolaire,
            [FromQuery] string typeInscription = "Inscription")
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Le fichier Excel est requis" });
            }

            if (idClasse <= 0)
            {
                return BadRequest(new { message = "L'ID de la classe est requis" });
            }

            if (idAnneeScolaire <= 0)
            {
                return BadRequest(new { message = "L'ID de l'année scolaire est requis" });
            }

            if (string.IsNullOrWhiteSpace(typeInscription))
            {
                return BadRequest(new { message = "Le type d'inscription est requis" });
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
                var excelService = HttpContext.RequestServices.GetRequiredService<ExcelInscriptionServiceV2>();
                var result = await excelService.ProcessExcelFileAsync(file, idEcole.Value, idUtilisateur, idClasse, idAnneeScolaire, typeInscription);

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

        // PUT: api/Inscription/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Super-Admin")]
        [ProducesResponseType(typeof(Inscription), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Inscription>> UpdateInscription(int id, [FromBody] UpdateInscriptionDto dto)
        {
            if (id != dto.IdInscription)
            {
                return BadRequest(new { message = "L'ID dans l'URL ne correspond pas à l'ID dans le corps" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingInscription = await _inscriptionRepository.GetByIdAsync(id);
            if (existingInscription == null)
            {
                return NotFound(new { message = "Inscription non trouvée" });
            }

            // 📸 AUDIT: Capturer l'état AVANT modification
            var oldInscription = new Inscription
            {
                IdInscription = existingInscription.IdInscription,
                Type = existingInscription.Type,
                StatutInscription = existingInscription.StatutInscription,
                IdClasse = existingInscription.IdClasse
            };

            // Mettre à jour seulement les champs autorisés
            existingInscription.Type = dto.Type;
            existingInscription.StatutInscription = dto.StatutInscription;
            existingInscription.IdClasse = dto.IdClasse;

            var updatedInscription = await _inscriptionRepository.UpdateAsync(existingInscription);
            if (updatedInscription == null)
            {
                return StatusCode(500, new { message = "Erreur lors de la mise à jour" });
            }

            // 📝 AUDIT: Enregistrer la modification
            var auditContext = this.GetAuditContext();
            await _auditService.LogUpdateAsync(
                oldInscription, updatedInscription,
                auditContext.UserId, auditContext.UserName,
                auditContext.UserRole, auditContext.IdEcole,
                auditContext.IpAddress, auditContext.UserAgent,
                "Modification d'inscription"
            );

            return Ok(updatedInscription);
        }

            // DELETE: api/Inscription/5
            [HttpDelete("{id}")]
            public async Task<IActionResult> DeleteInscription(int id)
            {
                var success = await _inscriptionRepository.DeleteAsync(id);
                if (!success)
                {
                    return NotFound();
                }

                return NoContent();
            }

        // PUT: api/Inscription/toggle-statut/{id}
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _inscriptionRepository.ToggleStatutAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Inscription non trouvée" });
                }

                var inscription = await _inscriptionRepository.GetByIdAsync(id);
                var estActif = inscription != null;
                
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = estActif,
                    inscription = inscription
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors du changement de statut", error = ex.Message });
            }
        }
     }
}
