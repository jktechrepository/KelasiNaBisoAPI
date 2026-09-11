using KelasiNaBiso.Helpers;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.DTOs.DevoirADomicile;
using KelasiNaBiso.Models.DTOs.Pagination;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBisoAPI.Hubs;
using KelasiNaBisoAPI.Services.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace KelasiNaBiso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔒 Authentification requise
    public class DevoirADomicileController : ControllerBase
    {
        private readonly IDevoirADomicileRepository _devoirRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IAntivirusService _antivirusService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IHubContext<DevoirADomicileHub> _hubContext;
        private readonly IFirebaseNotificationService _firebaseNotificationService;
        private readonly IEleveRepository _eleveRepository;
        private readonly KelasiNaBiso.Data.KelasiNaBisoDbContext _context;
        private readonly ILogger<DevoirADomicileController> _logger;
        private readonly ISmsNotificationService _smsService;
        private readonly IEmailService _emailService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IInscriptionActiveResolver _inscriptionResolver;
        private readonly EleveAnneeScopeHelper _scope;

        public DevoirADomicileController(
            IDevoirADomicileRepository devoirRepository,
            IFileStorageService fileStorageService,
            IAntivirusService antivirusService,
            ICurrentUserService currentUserService,
            IHubContext<DevoirADomicileHub> hubContext,
            IFirebaseNotificationService firebaseNotificationService,
            IEleveRepository eleveRepository,
            KelasiNaBiso.Data.KelasiNaBisoDbContext context,
            ILogger<DevoirADomicileController> logger,
            ISmsNotificationService smsService,
            IEmailService emailService,
            IServiceScopeFactory serviceScopeFactory,
            IInscriptionActiveResolver inscriptionResolver,
            EleveAnneeScopeHelper scope)
        {
            _devoirRepository = devoirRepository;
            _fileStorageService = fileStorageService;
            _antivirusService = antivirusService;
            _currentUserService = currentUserService;
            _hubContext = hubContext;
            _firebaseNotificationService = firebaseNotificationService;
            _eleveRepository = eleveRepository;
            _context = context;
            _logger = logger;
            _smsService = smsService;
            _emailService = emailService;
            _serviceScopeFactory = serviceScopeFactory;
            _inscriptionResolver = inscriptionResolver;
            _scope = scope;
        }

        /// <summary>
        /// Publier un devoir à domicile (Enseignant, Directeur, Admin ou Super-Admin)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = $"{UserRoles.ENSEIGNANT},{UserRoles.DIRECTEUR},{UserRoles.ADMIN},{UserRoles.SUPER_ADMIN}")]
        [RequestSizeLimit(5 * 1024 * 1024)] // 5 MB max
        [ProducesResponseType(typeof(DevoirADomicileDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<DevoirADomicileDto>> PublierDevoir(
            [FromForm] CreateDevoirADomicileDto dto)
        {
            try
            {
                // Récupérer le fichier manuellement depuis Request.Form.Files (optionnel)
                IFormFile? fichier = null;
                if (Request.Form.Files != null && Request.Form.Files.Count > 0)
                {
                    // Chercher le fichier avec le nom "fichier" ou "file", sinon prendre le premier
                    foreach (var file in Request.Form.Files)
                    {
                        if (file.Name == "fichier" || file.Name == "file")
                        {
                            fichier = file;
                            break;
                        }
                    }
                    // Si aucun fichier trouvé avec ces noms, prendre le premier fichier
                    if (fichier == null && Request.Form.Files.Count > 0)
                    {
                        fichier = Request.Form.Files[0];
                    }
                }

                // 1. Valider qu'au moins un fichier OU un contenu est fourni (ou les deux peuvent être vides)
                bool hasFile = fichier != null && fichier.Length > 0;
                bool hasContent = !string.IsNullOrWhiteSpace(dto.Contenu);
                
                // Le fichier et le contenu sont tous les deux optionnels
                // Un devoir peut être créé avec seulement un titre et une description

                // 2. Valider le fichier si fourni
                if (hasFile)
                {
                    if (!_fileStorageService.IsValidFileType(fichier.FileName))
                    {
                        return BadRequest(new { message = "Type de fichier non autorisé. Formats acceptés : PDF, JPG, PNG." });
                    }

                    if (!_fileStorageService.IsValidFileSize(fichier.Length))
                    {
                        return BadRequest(new { message = "Fichier trop volumineux. Taille maximum : 5 MB" });
                    }
                }

                // 3. Récupérer l'ID de l'agent depuis le token JWT
                var role = _currentUserService.UserRole;
                var idAgent = _currentUserService.AgentId;
                
                // Pour Super-Admin, Admin, Directeur et Enseignant, ils doivent avoir un IdAgent
                // (selon la cohérence métier, tous ces rôles sont liés à un Agent)
                if (!idAgent.HasValue)
                {
                    return Forbid("Vous devez être un agent pour publier un devoir");
                }

                // 4. Vérifier que l'agent peut publier pour cette classe
                // Cette méthode gère maintenant Super-Admin, Admin, Directeur et Enseignant
                var peutPublier = await _devoirRepository.AgentPeutPublierPourClasseAsync(idAgent.Value, dto.IdClasse);
                if (!peutPublier)
                {
                    return Forbid("Vous n'êtes pas autorisé à publier pour cette classe");
                }

                // 5. Récupérer les informations de la classe pour IdEcole et IdDirection
                var classe = await _context.Classes
                    .Include(c => c.Direction)
                    .FirstOrDefaultAsync(c => c.IdClasse == dto.IdClasse);

                if (classe == null)
                {
                    return NotFound(new { message = "Classe introuvable" });
                }

                if (classe.Direction == null)
                {
                    return BadRequest(new { message = "La classe n'a pas de direction associée" });
                }

                var idEcole = classe.Direction.IdEcole!.Value;
                var idAnnee = await _scope.ResolveIdAnneeScolaireAsync(idEcole, dto.IdAnneeScolaire);

                // 6. Uploader le fichier si fourni
                string? nomFichier = null;
                string? cheminFichier = null;
                long? tailleFichier = null;
                string? typeMIME = null;

                if (hasFile)
                {
                    var uploadResult = await _fileStorageService.UploadFileAsync(fichier, "devoirs");

                    // Scanner le fichier avec antivirus
                    var isSafe = await _antivirusService.ScanFileAsync(uploadResult.FilePath);
                    if (!isSafe)
                    {
                        // Supprimer le fichier si infecté
                        await _fileStorageService.DeleteFileAsync(uploadResult.FilePath);
                        return BadRequest(new { message = "Le fichier a été rejeté par le scanner antivirus" });
                    }

                    nomFichier = uploadResult.OriginalFileName;
                    cheminFichier = uploadResult.FilePath;
                    tailleFichier = uploadResult.FileSize;
                    typeMIME = uploadResult.TypeMIME;
                }

                // 7. Créer le devoir en base
                var devoir = new DevoirADomicile
                {
                    Titre = dto.Titre,
                    Description = dto.Description,
                    Contenu = dto.Contenu,
                    NomFichier = nomFichier,
                    CheminFichier = cheminFichier,
                    TailleFichier = tailleFichier,
                    TypeMIME = typeMIME,
                    IdEcole = idEcole,
                    IdDirection = classe.Direction.IdDirection,
                    IdAgent = idAgent.Value,
                    IdClasse = dto.IdClasse,
                    IdAnneeScolaire = idAnnee,
                    IdCours = dto.IdCours,
                    CoefficientDevoir = dto.CoefficientDevoir,
                    DateLimite = dto.DateLimite
                };

                var devoirCree = await _devoirRepository.CreateAsync(devoir);

                // 7.1. Créer automatiquement une Evaluation associée au devoir
                // L'évaluation n'est créée que si IdCours est fourni (car IdCours est obligatoire dans Evaluation)
                if (dto.IdCours.HasValue && dto.IdCours.Value > 0)
                {
                    try
                    {
                        var evaluation = new Evaluation
                        {
                            TypeEvaluation = "Devoir",
                            TitreEvaluation = dto.Titre,
                            Coefficient = dto.CoefficientDevoir,
                            IdCours = dto.IdCours.Value,
                            IdClasse = dto.IdClasse,
                            Statut = true,
                            DateCreation = DateTime.Now
                        };

                        _context.Evaluations.Add(evaluation);
                        await _context.SaveChangesAsync();

                        _logger.LogInformation($"✅ Évaluation créée automatiquement pour le devoir {devoirCree.IdDevoirADomicile} : ID {evaluation.IdEvaluation}");
                    }
                    catch (Exception ex)
                    {
                        // Log l'erreur mais ne bloque pas la création du devoir
                        _logger.LogError(ex, $"❌ Erreur lors de la création automatique de l'évaluation pour le devoir {devoirCree.IdDevoirADomicile}");
                        // On continue même si l'évaluation n'a pas pu être créée
                    }
                }
                else
                {
                    _logger.LogWarning($"⚠️ Aucune évaluation créée pour le devoir {devoirCree.IdDevoirADomicile} : IdCours non fourni");
                }

                // 8. Notifier via SignalR (temps réel) - Multi-groupes avec données enrichies
                // Utiliser un nouveau scope pour éviter ObjectDisposedException
                _ = Task.Run(async () =>
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        try
                        {
                            // Récupérer les services depuis le nouveau scope
                            var scopedContext = scope.ServiceProvider.GetRequiredService<KelasiNaBiso.Data.KelasiNaBisoDbContext>();
                            var scopedHubContext = scope.ServiceProvider.GetRequiredService<IHubContext<DevoirADomicileHub>>();
                            var scopedResolver = scope.ServiceProvider.GetRequiredService<IInscriptionActiveResolver>();
                            
                            await EnvoyerNotificationSignalRAsync(
                                devoirCree, 
                                classe, 
                                scopedContext, 
                                scopedHubContext,
                                scopedResolver);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Erreur lors de l'envoi de la notification SignalR pour devoir {devoirCree.IdDevoirADomicile}");
                        }
                    }
                });

                // 9. Notifier les parents via multi-canal (Push + SMS + Email) en parallèle
                // Utiliser un nouveau scope pour éviter ObjectDisposedException
                _ = Task.Run(async () =>
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        try
                        {
                            _logger.LogInformation($"📤 Démarrage de l'envoi des notifications pour devoir {devoirCree.IdDevoirADomicile}");
                            
                            // Récupérer les services depuis le nouveau scope
                            var scopedContext = scope.ServiceProvider.GetRequiredService<KelasiNaBiso.Data.KelasiNaBisoDbContext>();
                            var scopedFirebaseService = scope.ServiceProvider.GetRequiredService<IFirebaseNotificationService>();
                            var scopedSmsService = scope.ServiceProvider.GetRequiredService<ISmsNotificationService>();
                            var scopedEmailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                            var scopedResolver = scope.ServiceProvider.GetRequiredService<IInscriptionActiveResolver>();
                            
                            await EnvoyerNotificationsDevoirAuxParentsAsync(
                                devoirCree, 
                                classe, 
                                scopedContext, 
                                scopedFirebaseService, 
                                scopedSmsService, 
                                scopedEmailService,
                                scopedResolver);
                            
                            _logger.LogInformation($"✅ Envoi des notifications terminé pour devoir {devoirCree.IdDevoirADomicile}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"❌ Erreur lors de l'envoi des notifications pour devoir {devoirCree.IdDevoirADomicile}");
                        }
                    }
                });

                // 10. Retourner le devoir créé
                var devoirDto = await MapToDtoAsync(devoirCree);
                return CreatedAtAction(nameof(GetDevoir), new { id = devoirCree.IdDevoirADomicile }, devoirDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la publication du devoir");
                return StatusCode(500, new { message = "Erreur interne du serveur", error = ex.Message });
            }
        }

        /// <summary>
        /// Voir mes devoirs publiés (Enseignant) ou tous les devoirs de l'école (Admin/Directeur/Super-Admin)
        /// Retourne un résultat paginé scopé à l'année scolaire (défaut : année courante).
        /// Filtres optionnels : idEcole (Super-Admin uniquement), idClasse, idAnneeScolaire
        /// </summary>
        [HttpGet("mes-devoirs")]
        [Authorize(Roles = $"{UserRoles.ENSEIGNANT},{UserRoles.DIRECTEUR},{UserRoles.ADMIN},{UserRoles.SUPER_ADMIN}")]
        [ProducesResponseType(typeof(ElevesAnneeScopedResult<PagedResult<DevoirADomicileDto>>), 200)]
        public async Task<ActionResult<ElevesAnneeScopedResult<PagedResult<DevoirADomicileDto>>>> MesDevoirs(
            [FromQuery] PagedRequest? request,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idClasse = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            try
            {
                var role = _currentUserService.UserRole;
                var idEcoleUtilisateur = _currentUserService.EcoleId;
                var pagedRequest = request ?? new PagedRequest { PageNumber = 1, PageSize = 15 };
                var (resolvedEcole, resolvedAnnee) = await ResolveMesDevoirsScopeAsync(
                    role, idEcole, idEcoleUtilisateur, idAnneeScolaire);

                PagedResult<DevoirADomicile> pagedResult;

                if (role == UserRoles.SUPER_ADMIN)
                {
                    pagedResult = await _devoirRepository.GetAllPagedAsync(
                        pagedRequest, resolvedAnnee, resolvedEcole > 0 ? resolvedEcole : idEcole, idClasse);
                }
                else if (role == UserRoles.ADMIN || role == UserRoles.DIRECTEUR)
                {
                    if (idEcoleUtilisateur == 0)
                        return Forbid("Vous devez être associé à une école pour voir les devoirs");

                    pagedResult = await _devoirRepository.GetByEcolePagedAsync(
                        idEcoleUtilisateur, pagedRequest, resolvedAnnee, idClasse);
                    resolvedEcole = idEcoleUtilisateur;
                }
                else
                {
                    var idAgent = _currentUserService.AgentId;
                    if (!idAgent.HasValue)
                        return Forbid("Vous devez être un agent pour voir vos devoirs");

                    pagedResult = await _devoirRepository.GetByAgentPagedAsync(
                        idAgent.Value, pagedRequest, resolvedAnnee, idClasse);
                    resolvedEcole = idEcoleUtilisateur;
                }

                var devoirsDto = new List<DevoirADomicileDto>();
                foreach (var devoir in pagedResult.Data)
                    devoirsDto.Add(await MapToDtoAsync(devoir));

                var result = new PagedResult<DevoirADomicileDto>(
                    devoirsDto,
                    pagedResult.TotalRecords,
                    pagedResult.PageNumber,
                    pagedResult.PageSize);

                return Ok(EleveAnneeScopeHelper.Wrap(result, resolvedEcole, resolvedAnnee));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des devoirs");
                return StatusCode(500, new { message = "Erreur interne du serveur" });
            }
        }

        /// <summary>
        /// Voir les devoirs d'une classe (Parent/Élève/Enseignant/Admin/Directeur/Super-Admin)
        /// </summary>
        [HttpGet("classe/{idClasse}")]
        [Authorize(Roles = $"{UserRoles.PARENT},{UserRoles.ELEVE},{UserRoles.ENSEIGNANT},{UserRoles.DIRECTEUR},{UserRoles.ADMIN},{UserRoles.SUPER_ADMIN}")]
        [ProducesResponseType(typeof(ElevesAnneeScopedResult<IEnumerable<DevoirADomicileDto>>), 200)]
        public async Task<ActionResult<ElevesAnneeScopedResult<IEnumerable<DevoirADomicileDto>>>> GetDevoirsByClasse(
            int idClasse,
            [FromQuery] PagedRequest? request,
            [FromQuery] int? idAnneeScolaire = null)
        {
            try
            {
                var idUtilisateur = _currentUserService.UserId;
                var aAcces = await _devoirRepository.UserPeutAccederAClasseAsync(idUtilisateur, idClasse);
                if (!aAcces)
                    return Forbid("Vous n'avez pas accès à cette classe");

                var (idEcole, idAnnee) = await _scope.ResolveClasseAnneeAsync(idClasse, idAnneeScolaire);

                IEnumerable<DevoirADomicile> devoirs;
                if (request != null)
                {
                    var pagedResult = await _devoirRepository.GetByClassePagedAsync(idClasse, request, idAnnee);
                    devoirs = pagedResult.Data;
                }
                else
                {
                    devoirs = await _devoirRepository.GetByClasseAsync(idClasse, idAnnee);
                }

                var devoirsDto = new List<DevoirADomicileDto>();
                foreach (var devoir in devoirs)
                    devoirsDto.Add(await MapToDtoAsync(devoir));

                return Ok(EleveAnneeScopeHelper.Wrap(devoirsDto, idEcole, idAnnee));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des devoirs de la classe");
                return StatusCode(500, new { message = "Erreur interne du serveur" });
            }
        }

        /// <summary>
        /// Devoirs des classes des enfants d'un tuteur (inscriptions confirmées, école + année).
        /// Parent/Élève : uniquement leur propre IdTuteur.
        /// </summary>
        [HttpGet("tuteur/{idTuteur}")]
        [Authorize(Roles = $"{UserRoles.PARENT},{UserRoles.ELEVE},{UserRoles.ENSEIGNANT},{UserRoles.DIRECTEUR},{UserRoles.ADMIN},{UserRoles.SUPER_ADMIN}")]
        [ProducesResponseType(typeof(PagedResult<DevoirADomicilePourTuteurDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetDevoirsByTuteur(
            int idTuteur,
            [FromQuery] PagedRequest? request,
            [FromQuery] string? libelleAnneeScolaire = null)
        {
            var role = _currentUserService.UserRole;
            if (role == UserRoles.PARENT || role == UserRoles.ELEVE)
            {
                if (!_currentUserService.TuteurId.HasValue || _currentUserService.TuteurId.Value != idTuteur)
                    return Forbid("Vous ne pouvez consulter que les devoirs de vos enfants");
            }

            try
            {
                var pagedRequest = request ?? new PagedRequest { PageNumber = 1, PageSize = 15 };
                var result = await _devoirRepository.GetByTuteurPagedAsync(
                    idTuteur, pagedRequest, libelleAnneeScolaire);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des devoirs du tuteur {IdTuteur}", idTuteur);
                return StatusCode(500, new { message = "Erreur interne du serveur" });
            }
        }

        /// <summary>
        /// Télécharger un devoir (Parent/Élève/Enseignant/Admin/Directeur/Super-Admin)
        /// </summary>
        [HttpGet("{id}/telecharger")]
        [Authorize(Roles = $"{UserRoles.PARENT},{UserRoles.ELEVE},{UserRoles.ENSEIGNANT},{UserRoles.DIRECTEUR},{UserRoles.ADMIN},{UserRoles.SUPER_ADMIN}")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> TelechargerDevoir(int id)
        {
            try
            {
                var idUtilisateur = _currentUserService.UserId;

                // 1. Vérifier l'accès
                var aAcces = await _devoirRepository.UserPeutAccederAuDevoirAsync(idUtilisateur, id);
                if (!aAcces)
                {
                    return Forbid("Vous n'avez pas accès à ce devoir");
                }

                // 2. Récupérer le devoir
                var devoir = await _devoirRepository.GetByIdAsync(id);

                // 3. Vérifier si le devoir a un fichier
                if (string.IsNullOrWhiteSpace(devoir.CheminFichier))
                {
                    return BadRequest(new { message = "Ce devoir n'a pas de fichier associé. Il contient uniquement du texte." });
                }

                // 4. Récupérer le fichier
                var fileStream = await _fileStorageService.GetFileStreamAsync(devoir.CheminFichier);

                // 5. Incrémenter le nombre de téléchargements
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _devoirRepository.IncrementerTelechargementsAsync(id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Erreur lors de l'incrémentation des téléchargements pour devoir {id}");
                    }
                });

                // 6. Retourner le fichier
                var contentType = devoir.TypeMIME ?? "application/octet-stream";
                var fileName = devoir.NomFichier ?? "devoir.pdf";
                return File(fileStream, contentType, fileName);
            }
            catch (FileNotFoundException)
            {
                return NotFound(new { message = "Fichier introuvable" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors du téléchargement du devoir {id}");
                return StatusCode(500, new { message = "Erreur interne du serveur" });
            }
        }

        /// <summary>
        /// Supprimer un devoir (Enseignant propriétaire ou Directeur)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = $"{UserRoles.ENSEIGNANT},{UserRoles.DIRECTEUR},{UserRoles.ADMIN},{UserRoles.SUPER_ADMIN}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> SupprimerDevoir(int id)
        {
            try
            {
                var devoir = await _devoirRepository.GetByIdAsync(id);
                var idAgent = _currentUserService.AgentId;

                // Vérifier que l'agent est propriétaire ou Directeur
                var estDirecteur = idAgent.HasValue && await _devoirRepository.EstDirecteurAsync(idAgent.Value);
                var estProprietaire = idAgent.HasValue && devoir.IdAgent == idAgent.Value;

                if (!estProprietaire && !estDirecteur)
                {
                    return Forbid("Vous n'êtes pas autorisé à supprimer ce devoir");
                }

                // Supprimer le fichier du disque s'il existe
                if (!string.IsNullOrWhiteSpace(devoir.CheminFichier))
                {
                    await _fileStorageService.DeleteFileAsync(devoir.CheminFichier);
                }

                // Supprimer de la base
                await _devoirRepository.DeleteAsync(id);

                _logger.LogInformation($"Devoir {id} supprimé par agent {idAgent}");

                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Devoir introuvable" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la suppression du devoir {id}");
                return StatusCode(500, new { message = "Erreur interne du serveur" });
            }
        }

        /// <summary>
        /// Mettre à jour un devoir (Enseignant propriétaire ou Directeur)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = $"{UserRoles.ENSEIGNANT},{UserRoles.DIRECTEUR},{UserRoles.ADMIN},{UserRoles.SUPER_ADMIN}")]
        [ProducesResponseType(typeof(DevoirADomicileDto), 200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<DevoirADomicileDto>> UpdateDevoir(int id, UpdateDevoirADomicileDto dto)
        {
            try
            {
                var devoir = await _devoirRepository.GetByIdAsync(id);
                var idAgent = _currentUserService.AgentId;

                // Vérifier que l'agent est propriétaire ou Directeur
                var estDirecteur = idAgent.HasValue && await _devoirRepository.EstDirecteurAsync(idAgent.Value);
                var estProprietaire = idAgent.HasValue && devoir.IdAgent == idAgent.Value;

                if (!estProprietaire && !estDirecteur)
                {
                    return Forbid("Vous n'êtes pas autorisé à modifier ce devoir");
                }

                // Mettre à jour les métadonnées
                devoir.Titre = dto.Titre;
                devoir.Description = dto.Description;
                devoir.DateLimite = dto.DateLimite;
                devoir.Statut = dto.Statut;

                var devoirMisAJour = await _devoirRepository.UpdateAsync(devoir);

                var devoirDto = await MapToDtoAsync(devoirMisAJour);
                return Ok(devoirDto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Devoir introuvable" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la mise à jour du devoir {id}");
                return StatusCode(500, new { message = "Erreur interne du serveur" });
            }
        }

        /// <summary>
        /// Récupérer un devoir par ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DevoirADomicileDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<DevoirADomicileDto>> GetDevoir(int id)
        {
            try
            {
                var idUtilisateur = _currentUserService.UserId;
                var devoir = await _devoirRepository.GetByIdAsync(id);

                // Vérifier l'accès
                var aAcces = await _devoirRepository.UserPeutAccederAuDevoirAsync(idUtilisateur, id);
                if (!aAcces)
                {
                    return Forbid("Vous n'avez pas accès à ce devoir");
                }

                var devoirDto = await MapToDtoAsync(devoir);
                return Ok(devoirDto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Devoir introuvable" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la récupération du devoir {id}");
                return StatusCode(500, new { message = "Erreur interne du serveur" });
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // MÉTHODES PRIVÉES
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Envoie les notifications SignalR en temps réel aux différents groupes
        /// avec données enrichies et multi-groupes
        /// </summary>
        private async Task EnvoyerNotificationSignalRAsync(
            DevoirADomicile devoir,
            Classe classe,
            KelasiNaBiso.Data.KelasiNaBisoDbContext? context = null,
            IHubContext<DevoirADomicileHub>? hubContext = null,
            IInscriptionActiveResolver? inscriptionResolver = null)
        {
            try
            {
                // Utiliser le contexte et hub fournis ou ceux du contrôleur
                var dbContext = context ?? _context;
                var hubContextToUse = hubContext ?? _hubContext;
                var resolver = inscriptionResolver ?? _inscriptionResolver;
                
                // 1. Récupérer les informations complémentaires
                var agent = await dbContext.Agents.FindAsync(devoir.IdAgent);
                var nomAgent = GetAgentNomComplet(agent);
                var nomClasse = classe.NomClasse ?? "Classe inconnue";
                var cours = devoir.IdCours.HasValue 
                    ? await _context.Cours.FindAsync(devoir.IdCours.Value) 
                    : null;
                var nomCours = cours?.NomCours;

                // 2. Déterminer le type de devoir
                var typeMime = devoir.TypeMIME ?? "";
                var extensionFichier = !string.IsNullOrWhiteSpace(typeMime) && typeMime.Contains('/')
                    ? typeMime.Split(new[] { '/' }, StringSplitOptions.None).LastOrDefault() ?? "PDF"
                    : "PDF";
                
                var typeDevoir = !string.IsNullOrWhiteSpace(devoir.NomFichier)
                    ? $"Fichier ({extensionFichier.ToUpper()})"
                    : !string.IsNullOrWhiteSpace(devoir.Contenu)
                        ? "Contenu textuel"
                        : "Devoir";

                // 3. Préparer les données enrichies du devoir
                var devoirData = new
                {
                    IdDevoirADomicile = devoir.IdDevoirADomicile,
                    Titre = devoir.Titre,
                    Description = devoir.Description,
                    Contenu = devoir.Contenu,
                    NomFichier = devoir.NomFichier,
                    TailleFichier = devoir.TailleFichier,
                    TypeMIME = devoir.TypeMIME,
                    DatePublication = devoir.DatePublication,
                    DateLimite = devoir.DateLimite,
                    IdClasse = devoir.IdClasse,
                    NomClasse = nomClasse,
                    IdAgent = devoir.IdAgent,
                    NomAgent = nomAgent,
                    IdCours = devoir.IdCours,
                    NomCours = nomCours,
                    TypeDevoir = typeDevoir,
                    IdEcole = devoir.IdEcole,
                    NombreTelechargements = devoir.NombreTelechargements,
                    Timestamp = DateTime.UtcNow
                };

                // 4. Envoyer aux différents groupes en parallèle
                var tasks = new List<Task>();

                // Groupe classe (élèves et enseignants de la classe)
                tasks.Add(hubContextToUse.Clients
                    .Group($"classe_{devoir.IdClasse}")
                    .SendAsync("NouveauDevoir", devoirData));

                // Groupe parents de la classe
                tasks.Add(hubContextToUse.Clients
                    .Group($"parents_classe_{devoir.IdClasse}")
                    .SendAsync("NouveauDevoir", devoirData));

                // Groupe école (administrateurs et directeurs)
                tasks.Add(hubContextToUse.Clients
                    .Group($"ecole_{devoir.IdEcole}")
                    .SendAsync("NouveauDevoir", devoirData));

                // Groupe général (tous les utilisateurs connectés)
                tasks.Add(hubContextToUse.Clients
                    .Group("all_users")
                    .SendAsync("NouveauDevoir", devoirData));

                // 5. Notifications personnalisées aux parents (si disponibles)
                var parents = await resolver
                    .FilterUtilisateursParentsInClasse(dbContext.Utilisateurs, devoir.IdClasse)
                    .Select(u => new
                    {
                        u.IdUtilisateur,
                        Enfants = resolver
                            .FilterElevesInClasse(dbContext.Eleves, devoir.IdClasse, null)
                            .Where(e => e.IdTuteur == u.IdTuteur)
                            .Select(e => e.NomComplet)
                            .ToList()
                    })
                    .ToListAsync();

                foreach (var parent in parents)
                {
                    var nomsEnfants = string.Join(", ", parent.Enfants);
                    tasks.Add(hubContextToUse.Clients
                        .Group($"user_{parent.IdUtilisateur}")
                        .SendAsync("NouveauDevoirParent", new
                        {
                            Devoir = devoirData,
                            Enfants = parent.Enfants,
                            MessagePersonnalise = $"Votre enfant {(parent.Enfants.Count == 1 ? nomsEnfants : "vos enfants")} a un nouveau devoir"
                        }));
                }

                // 6. Exécuter toutes les notifications en parallèle
                await Task.WhenAll(tasks);

                _logger.LogInformation($"✅ Notifications SignalR envoyées pour devoir {devoir.IdDevoirADomicile} à {tasks.Count} groupe(s)");
                _logger.LogInformation($"📡 Groupes SignalR notifiés : classe_{devoir.IdClasse}, parents_classe_{devoir.IdClasse}, ecole_{devoir.IdEcole}, all_users, et {parents.Count} parent(s) individuel(s)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de l'envoi des notifications SignalR pour devoir {devoir.IdDevoirADomicile}");
            }
        }

        /// <summary>
        /// Envoie les notifications multi-canal (Push + SMS + Email) aux parents de la classe
        /// avec gestion des doublons et traitement en parallèle
        /// </summary>
        private async Task EnvoyerNotificationsDevoirAuxParentsAsync(
            DevoirADomicile devoir,
            Classe classe,
            KelasiNaBiso.Data.KelasiNaBisoDbContext? context = null,
            IFirebaseNotificationService? firebaseService = null,
            ISmsNotificationService? smsService = null,
            IEmailService? emailService = null,
            IInscriptionActiveResolver? inscriptionResolver = null)
        {
            try
            {
                // Utiliser le contexte fourni ou celui du contrôleur
                var dbContext = context ?? _context;
                var firebaseServiceToUse = firebaseService ?? _firebaseNotificationService;
                var smsServiceToUse = smsService ?? _smsService;
                var emailServiceToUse = emailService ?? _emailService;
                var resolver = inscriptionResolver ?? _inscriptionResolver;
                
                _logger.LogInformation($"🔍 Recherche des parents pour devoir {devoir.IdDevoirADomicile}, classe {classe.IdClasse}");
                
                // 1. Récupérer tous les parents de la classe (sans doublons)
                // Note: On ne peut pas utiliser Distinct() sur une projection avec collection
                // On récupère d'abord les IDs uniques, puis on charge les données
                var parentIds = await resolver
                    .FilterUtilisateursParentsInClasse(dbContext.Utilisateurs, classe.IdClasse)
                    .Select(u => u.IdUtilisateur)
                    .Distinct()
                    .ToListAsync();

                // 2. Charger les données complètes pour chaque parent
                var parents = new List<dynamic>();
                foreach (var parentId in parentIds)
                {
                    var utilisateur = await dbContext.Utilisateurs
                        .FirstOrDefaultAsync(u => u.IdUtilisateur == parentId);
                    
                    if (utilisateur != null)
                    {
                        var enfants = await resolver
                            .FilterElevesInClasse(dbContext.Eleves, classe.IdClasse, null)
                            .Where(e => e.IdTuteur == utilisateur.IdTuteur)
                            .Select(e => e.NomComplet)
                            .ToListAsync();

                        parents.Add(new
                        {
                            IdUtilisateur = utilisateur.IdUtilisateur,
                            Email = utilisateur.Email,
                            Telephone = utilisateur.Telephone,
                            NomUtilisateur = utilisateur.NomUtilisateur,
                            PrenomUtilisateur = utilisateur.PrenomUtilisateur,
                            PostNomUtilisateur = utilisateur.PostNomUtilisateur,
                            Enfants = enfants
                        });
                    }
                }

                _logger.LogInformation($"👥 {parents.Count} parent(s) trouvé(s) pour la classe {classe.IdClasse}");

                if (parents.Count == 0)
                {
                    _logger.LogWarning($"⚠️ Aucun parent trouvé pour la classe {classe.IdClasse}");
                    return;
                }

                // 3. Récupérer les informations complémentaires
                var agent = await dbContext.Agents
                    .FirstOrDefaultAsync(a => a.IdAgent == devoir.IdAgent);
                
                var nomAgent = GetAgentNomComplet(agent);
                var nomClasse = classe.NomClasse ?? "Classe inconnue";
                var cours = devoir.IdCours.HasValue 
                    ? await _context.Cours.FindAsync(devoir.IdCours.Value) 
                    : null;
                var nomCours = cours?.NomCours;

                // 4. Préparer le message enrichi
                var dateLimiteStr = devoir.DateLimite.HasValue
                    ? devoir.DateLimite.Value.ToString("dd/MM/yyyy à HH:mm")
                    : "Non spécifiée";

                var typeMime = devoir.TypeMIME ?? "";
                var extensionFichier = !string.IsNullOrWhiteSpace(typeMime) && typeMime.Contains('/')
                    ? typeMime.Split(new[] { '/' }, StringSplitOptions.None).LastOrDefault() ?? "PDF"
                    : "PDF";
                
                var typeDevoir = !string.IsNullOrWhiteSpace(devoir.NomFichier)
                    ? $"📎 Fichier ({extensionFichier.ToUpper()})"
                    : !string.IsNullOrWhiteSpace(devoir.Contenu)
                        ? "📝 Contenu textuel"
                        : "📄 Devoir";

                // 5. Envoyer les notifications en parallèle pour chaque parent
                var notificationTasks = parents.Select(async parent =>
                {
                    try
                    {
                        // Construire le nom complet du parent
                        var nomCompletParent = string.Join(" ", new[] { parent.PrenomUtilisateur, parent.NomUtilisateur, parent.PostNomUtilisateur }
                            .Where(s => !string.IsNullOrWhiteSpace(s)))
                            .Trim();
                        if (string.IsNullOrWhiteSpace(nomCompletParent))
                            nomCompletParent = "Parent";

                        // Préparer les messages personnalisés
                        var nomsEnfants = string.Join(", ", parent.Enfants);
                        var titre = $"📚 Nouveau devoir - {nomClasse}";
                        
                        var corpsPush = $"Votre enfant {(parent.Enfants.Count == 1 ? nomsEnfants : "vos enfants")} a un nouveau devoir :\n" +
                                      $"📝 {devoir.Titre}\n" +
                                      $"👨‍🏫 Enseignant : {nomAgent}\n" +
                                      $"📅 Date limite : {dateLimiteStr}\n" +
                                      $"📎 Type : {typeDevoir}";

                        var corpsSms = $"Nouveau devoir pour {nomsEnfants}:\n" +
                                      $"{devoir.Titre}\n" +
                                      $"Enseignant: {nomAgent}\n" +
                                      $"Date limite: {dateLimiteStr}";

                        var corpsEmail = $@"
Bonjour {nomCompletParent},

Votre enfant {(parent.Enfants.Count == 1 ? nomsEnfants : "vos enfants")} a un nouveau devoir à domicile.

<b>Détails du devoir :</b>
<ul>
  <li><strong>Titre :</strong> {devoir.Titre}</li>
  <li><strong>Classe :</strong> {nomClasse}</li>
  {(string.IsNullOrWhiteSpace(nomCours) ? "" : $"<li><strong>Cours :</strong> {nomCours}</li>")}
  <li><strong>Enseignant :</strong> {nomAgent}</li>
  <li><strong>Date limite :</strong> {dateLimiteStr}</li>
  <li><strong>Type :</strong> {typeDevoir}</li>
</ul>

{(string.IsNullOrWhiteSpace(devoir.Description) ? "" : $"<p><strong>Description :</strong><br/>{devoir.Description}</p>")}

<p>Vous pouvez consulter le devoir dans l'application KelasiNaBiso.</p>

<p>Cordialement,<br/>L'équipe KelasiNaBiso</p>";

                        var donneesPush = new Dictionary<string, string>
                        {
                            { "Type", "DevoirADomicile" },
                            { "IdDevoir", devoir.IdDevoirADomicile.ToString() },
                            { "IdClasse", devoir.IdClasse.ToString() },
                            { "Titre", devoir.Titre },
                            { "DateLimite", devoir.DateLimite?.ToString("yyyy-MM-ddTHH:mm:ss") ?? "" }
                        };

                        // Envoyer les notifications en parallèle (Push + SMS + Email)
                        var tasks = new List<Task>();

                        // Push Firebase (notification in-app)
                        tasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                _logger.LogInformation($"📲 Tentative d'envoi Push au parent {parent.IdUtilisateur} pour devoir {devoir.IdDevoirADomicile}");
                                var result = await firebaseServiceToUse.EnvoyerNotificationAUtilisateurAsync(
                                    parent.IdUtilisateur,
                                    titre,
                                    corpsPush,
                                    donneesPush
                                );
                                if (result)
                                {
                                    _logger.LogInformation($"✅ Push envoyé avec succès au parent {parent.IdUtilisateur} pour devoir {devoir.IdDevoirADomicile}");
                                }
                                else
                                {
                                    _logger.LogWarning($"⚠️ Push non envoyé au parent {parent.IdUtilisateur} (probablement aucun token FCM actif)");
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"❌ Échec Push pour parent {parent.IdUtilisateur} - Détails: {ex.Message}");
                            }
                        }));

                        // SMS (si numéro disponible)
                        if (!string.IsNullOrWhiteSpace(parent.Telephone))
                        {
                            tasks.Add(Task.Run(async () =>
                            {
                                try
                                {
                                    await smsServiceToUse.EnvoyerSmsAUtilisateurAsync(
                                        parent.IdUtilisateur,
                                        corpsSms,
                                        "DEVOIR_ADOMICILE"
                                    );
                                    _logger.LogInformation($"✅ SMS envoyé au parent {parent.IdUtilisateur} pour devoir {devoir.IdDevoirADomicile}");
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning(ex, $"⚠️ Échec SMS pour parent {parent.IdUtilisateur}");
                                }
                            }));
                        }

                        // Email (si email disponible)
                        if (!string.IsNullOrWhiteSpace(parent.Email))
                        {
                            tasks.Add(Task.Run(async () =>
                            {
                                try
                                {
                                    await emailServiceToUse.SendGenericEmailAsync(
                                        parent.Email,
                                        nomCompletParent,
                                        titre,
                                        corpsPush, // Version texte pour email
                                        corpsEmail // Version HTML
                                    );
                                    _logger.LogInformation($"✅ Email envoyé au parent {parent.IdUtilisateur} pour devoir {devoir.IdDevoirADomicile}");
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning(ex, $"⚠️ Échec Email pour parent {parent.IdUtilisateur}");
                                }
                            }));
                        }

                        // Attendre que toutes les notifications soient envoyées pour ce parent
                        await Task.WhenAll(tasks);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Erreur lors de l'envoi des notifications pour parent {parent.IdUtilisateur}");
                    }
                });

                // 6. Traiter tous les parents en parallèle
                _logger.LogInformation($"⏳ Envoi des notifications en cours pour {notificationTasks.Count()} parent(s)...");
                await Task.WhenAll(notificationTasks);

                _logger.LogInformation($"✅ Notifications multi-canal envoyées à {parents.Count} parent(s) pour devoir {devoir.IdDevoirADomicile}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de l'envoi des notifications aux parents pour devoir {devoir.IdDevoirADomicile}");
            }
        }

        private string GetAgentNomComplet(Agent? agent)
        {
            if (agent == null)
                return "Enseignant";
            
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(agent.Prenom))
                parts.Add(agent.Prenom);
            if (!string.IsNullOrWhiteSpace(agent.Nom))
                parts.Add(agent.Nom);
            if (!string.IsNullOrWhiteSpace(agent.Postnom))
                parts.Add(agent.Postnom);
            
            return parts.Count > 0 ? string.Join(" ", parts) : "Enseignant";
        }

        private async Task<DevoirADomicileDto> MapToDtoAsync(DevoirADomicile devoir)
        {
            var ecole = await _context.Ecoles.FindAsync(devoir.IdEcole);
            var direction = await _context.Directions.FindAsync(devoir.IdDirection);
            var agent = await _context.Agents.FindAsync(devoir.IdAgent);
            var classe = await _context.Classes.FindAsync(devoir.IdClasse);
            var cours = devoir.IdCours.HasValue ? await _context.Cours.FindAsync(devoir.IdCours.Value) : null;

            return new DevoirADomicileDto
            {
                IdDevoirADomicile = devoir.IdDevoirADomicile,
                Titre = devoir.Titre,
                Description = devoir.Description,
                Contenu = devoir.Contenu,
                NomFichier = devoir.NomFichier,
                TailleFichier = devoir.TailleFichier,
                TypeMIME = devoir.TypeMIME,
                IdEcole = devoir.IdEcole,
                NomEcole = ecole?.Nom,
                IdDirection = devoir.IdDirection,
                NomDirection = direction?.NomDirection,
                IdAgent = devoir.IdAgent,
                NomAgent = agent != null ? $"{agent.Nom} {agent.Postnom} {agent.Prenom}".Trim() : null,
                IdClasse = devoir.IdClasse,
                NomClasse = classe?.NomClasse,
                IdAnneeScolaire = devoir.IdAnneeScolaire,
                LibelleAnneeScolaire = (await _context.AnneeScolaires.FindAsync(devoir.IdAnneeScolaire))?.LibelleAnneeScolaire,
                IdCours = devoir.IdCours,
                NomCours = cours?.NomCours,
                DatePublication = devoir.DatePublication,
                DateLimite = devoir.DateLimite,
                NombreTelechargements = devoir.NombreTelechargements,
                Statut = devoir.Statut
            };
        }

        private async Task<(int IdEcole, int IdAnneeScolaire)> ResolveMesDevoirsScopeAsync(
            string role,
            int? idEcole,
            int idEcoleUtilisateur,
            int? idAnneeScolaire)
        {
            if (role == UserRoles.SUPER_ADMIN)
            {
                if (idEcole.HasValue && idEcole.Value > 0)
                {
                    var annee = await _scope.ResolveIdAnneeScolaireAsync(idEcole.Value, idAnneeScolaire);
                    return (idEcole.Value, annee);
                }

                if (idAnneeScolaire.HasValue && idAnneeScolaire.Value > 0)
                    return (0, idAnneeScolaire.Value);

                throw new InvalidOperationException(
                    "Précisez idEcole ou idAnneeScolaire pour filtrer les devoirs.");
            }

            if (idEcoleUtilisateur <= 0)
            {
                throw new InvalidOperationException(
                    "Aucune école associée à l'utilisateur pour résoudre l'année scolaire.");
            }

            var resolvedAnnee = await _scope.ResolveIdAnneeScolaireAsync(idEcoleUtilisateur, idAnneeScolaire);
            return (idEcoleUtilisateur, resolvedAnnee);
        }
    }
}

