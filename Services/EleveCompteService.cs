using KelasiNaBiso.Data;
using KelasiNaBiso.Helpers;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KelasiNaBiso.Services
{
    public class EleveCompteService : IEleveCompteService
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly ISmsNotificationService _smsService;
        private readonly ILogger<EleveCompteService> _logger;

        public EleveCompteService(
            KelasiNaBisoDbContext context,
            ISmsNotificationService smsService,
            ILogger<EleveCompteService> logger)
        {
            _context = context;
            _smsService = smsService;
            _logger = logger;
        }

        public async Task<EleveCompteCreateResult> CreateDefaultEleveUserAsync(
            Eleve eleve,
            int idEcole,
            bool sendSmsToTuteur = true)
        {
            if (eleve == null || eleve.IdEleve <= 0)
            {
                return new EleveCompteCreateResult
                {
                    Outcome = EleveCompteCreateOutcome.InvalidEleve,
                    Message = "Élève invalide"
                };
            }

            try
            {
                var matricule = eleve.Matricule?.Trim();
                if (string.IsNullOrWhiteSpace(matricule))
                {
                    _logger.LogWarning(
                        "Compte Élève non créé : matricule absent pour IdEleve={IdEleve}",
                        eleve.IdEleve);
                    return new EleveCompteCreateResult
                    {
                        Outcome = EleveCompteCreateOutcome.NoMatricule,
                        IdEleve = eleve.IdEleve,
                        Message = "Matricule absent"
                    };
                }

                var existingByEleve = await _context.Utilisateurs
                    .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.IdEleve == eleve.IdEleve);

                if (existingByEleve != null)
                {
                    _logger.LogInformation(
                        "Compte Élève déjà existant pour IdEleve={IdEleve} (IdUtilisateur={UserId})",
                        eleve.IdEleve, existingByEleve.IdUtilisateur);
                    return new EleveCompteCreateResult
                    {
                        Outcome = EleveCompteCreateOutcome.AlreadyLinked,
                        IdEleve = eleve.IdEleve,
                        Matricule = matricule,
                        Info = new UtilisateurInfo
                        {
                            IdUtilisateur = existingByEleve.IdUtilisateur,
                            IdEleve = eleve.IdEleve,
                            DefaultUsername = existingByEleve.DefaultUsername ?? matricule,
                            NomComplet = existingByEleve.NomUtilisateur ?? eleve.NomComplet ?? matricule,
                            Role = UserRoles.ELEVE,
                            MotDePasseParDefaut = ""
                        }
                    };
                }

                var existingByUsername = await _context.Utilisateurs
                    .FirstOrDefaultAsync(u => u.DefaultUsername == matricule);

                if (existingByUsername != null)
                {
                    if (existingByUsername.IdEleve == null)
                    {
                        existingByUsername.IdEleve = eleve.IdEleve;
                        await _context.SaveChangesAsync();
                        _logger.LogInformation(
                            "IdEleve lié au compte existant username={Username} IdUtilisateur={UserId}",
                            matricule, existingByUsername.IdUtilisateur);

                        return new EleveCompteCreateResult
                        {
                            Outcome = EleveCompteCreateOutcome.LinkedExistingUsername,
                            IdEleve = eleve.IdEleve,
                            Matricule = matricule,
                            Info = new UtilisateurInfo
                            {
                                IdUtilisateur = existingByUsername.IdUtilisateur,
                                IdEleve = eleve.IdEleve,
                                DefaultUsername = matricule,
                                NomComplet = existingByUsername.NomUtilisateur ?? eleve.NomComplet ?? matricule,
                                Role = UserRoles.ELEVE,
                                MotDePasseParDefaut = ""
                            }
                        };
                    }

                    if (existingByUsername.IdEleve != eleve.IdEleve)
                    {
                        _logger.LogError(
                            "Username (matricule) {Matricule} déjà utilisé par IdEleve={OtherId}, impossible de créer le compte pour IdEleve={IdEleve}",
                            matricule, existingByUsername.IdEleve, eleve.IdEleve);
                        return new EleveCompteCreateResult
                        {
                            Outcome = EleveCompteCreateOutcome.ConflictUsername,
                            IdEleve = eleve.IdEleve,
                            Matricule = matricule,
                            Message = $"Username '{matricule}' déjà utilisé par IdEleve={existingByUsername.IdEleve}"
                        };
                    }

                    return new EleveCompteCreateResult
                    {
                        Outcome = EleveCompteCreateOutcome.AlreadyLinked,
                        IdEleve = eleve.IdEleve,
                        Matricule = matricule,
                        Info = new UtilisateurInfo
                        {
                            IdUtilisateur = existingByUsername.IdUtilisateur,
                            IdEleve = eleve.IdEleve,
                            DefaultUsername = matricule,
                            NomComplet = existingByUsername.NomUtilisateur ?? eleve.NomComplet ?? matricule,
                            Role = UserRoles.ELEVE,
                            MotDePasseParDefaut = ""
                        }
                    };
                }

                var eleveRole = await _context.Roles.FirstOrDefaultAsync(r => r.Nom == UserRoles.ELEVE);
                if (eleveRole == null)
                {
                    eleveRole = new Role
                    {
                        Nom = UserRoles.ELEVE,
                        Description = "Élève",
                        Niveau = 6,
                        DateCreation = DateTime.Now,
                        Statut = true
                    };
                    _context.Roles.Add(eleveRole);
                    await _context.SaveChangesAsync();
                }

                const string motDePasseParDefaut = "123456";
                var nomComplet = string.IsNullOrWhiteSpace(eleve.NomComplet) ? matricule : eleve.NomComplet;

                var eleveUser = new Utilisateur
                {
                    IdEleve = eleve.IdEleve,
                    ReferenceUtilisateur = Guid.NewGuid(),
                    NomUtilisateur = nomComplet,
                    PostNomUtilisateur = eleve.Postnom ?? "",
                    PrenomUtilisateur = eleve.Prenom ?? "",
                    Email = null,
                    DefaultUsername = matricule,
                    Telephone = "",
                    Genre = eleve.Genre,
                    DateNaissance = eleve.DateNaissance,
                    MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(motDePasseParDefaut),
                    Statut = true,
                    DateCreation = DateTime.Now,
                    IsConnecte = false,
                    DoitChangerMotDePasse = true,
                    IdRole = eleveRole.IdRole,
                    IdEcole = idEcole
                };

                _context.Utilisateurs.Add(eleveUser);
                await _context.SaveChangesAsync();

                _context.UserRoles.Add(new UserRole
                {
                    IdUtilisateur = eleveUser.IdUtilisateur,
                    IdRole = eleveRole.IdRole,
                    IsPrimary = true,
                    Statut = true,
                    DateAttribution = DateTime.Now
                });
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Compte Élève créé: IdUtilisateur={UserId}, Username={Username}, IdEleve={IdEleve}",
                    eleveUser.IdUtilisateur, matricule, eleve.IdEleve);

                var created = new EleveCompteCreateResult
                {
                    Outcome = EleveCompteCreateOutcome.Created,
                    IdEleve = eleve.IdEleve,
                    Matricule = matricule,
                    Info = new UtilisateurInfo
                    {
                        IdUtilisateur = eleveUser.IdUtilisateur,
                        IdEleve = eleve.IdEleve,
                        DefaultUsername = matricule,
                        NomComplet = nomComplet,
                        Role = UserRoles.ELEVE,
                        MotDePasseParDefaut = motDePasseParDefaut
                    }
                };

                if (sendSmsToTuteur)
                    await TrySendCredentialsSmsToTuteurAsync(eleve, idEcole, matricule, motDePasseParDefaut);

                return created;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erreur création compte Élève pour IdEleve={IdEleve}: {Message}",
                    eleve.IdEleve, ex.Message);
                return new EleveCompteCreateResult
                {
                    Outcome = EleveCompteCreateOutcome.Error,
                    IdEleve = eleve.IdEleve,
                    Matricule = eleve.Matricule?.Trim(),
                    Message = ex.Message
                };
            }
        }

        public async Task<EleveCompteBackfillResult> BackfillByEcoleAsync(
            int idEcole,
            bool dryRun = false,
            bool sendSms = false)
        {
            var result = new EleveCompteBackfillResult
            {
                IdEcole = idEcole,
                DryRun = dryRun
            };

            // Élèves distincts ayant au moins une inscription confirmée/active dans l'école
            var eleveIds = await _context.Inscriptions
                .AsNoTracking()
                .Where(i => i.IdEcole == idEcole
                    && (i.Statut == true || i.Statut == null)
                    && i.StatutInscription != null
                    && (i.StatutInscription == InscriptionActiveRules.StatutConfirme
                        || i.StatutInscription == "Confirme"
                        || i.StatutInscription.StartsWith("Confirm")))
                .Select(i => i.IdEleve)
                .Distinct()
                .ToListAsync();

            var eleves = await _context.Eleves
                .AsNoTracking()
                .Where(e => eleveIds.Contains(e.IdEleve) && e.Statut == true)
                .ToListAsync();

            result.Candidates = eleves.Count;

            var linkedIds = await _context.Utilisateurs
                .AsNoTracking()
                .Where(u => u.IdEleve != null)
                .Select(u => u.IdEleve!.Value)
                .ToListAsync();
            var linkedSet = linkedIds.ToHashSet();

            foreach (var eleve in eleves)
            {
                if (linkedSet.Contains(eleve.IdEleve))
                {
                    result.SkippedAlreadyLinked++;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(eleve.Matricule))
                {
                    result.SkippedNoMatricule++;
                    continue;
                }

                if (dryRun)
                {
                    // Anticiper conflit username sans écrire
                    var usernameTaken = await _context.Utilisateurs
                        .AsNoTracking()
                        .AnyAsync(u => u.DefaultUsername == eleve.Matricule.Trim()
                            && u.IdEleve != null
                            && u.IdEleve != eleve.IdEleve);
                    if (usernameTaken)
                    {
                        result.ConflictsUsername++;
                        result.Details.Add(new EleveCompteCreateResult
                        {
                            Outcome = EleveCompteCreateOutcome.ConflictUsername,
                            IdEleve = eleve.IdEleve,
                            Matricule = eleve.Matricule.Trim(),
                            Message = "Conflit username (dry-run)"
                        });
                    }
                    else
                    {
                        result.Created++; // candidats qui seraient créés
                        result.Details.Add(new EleveCompteCreateResult
                        {
                            Outcome = EleveCompteCreateOutcome.Created,
                            IdEleve = eleve.IdEleve,
                            Matricule = eleve.Matricule.Trim(),
                            Message = "Serait créé (dry-run)"
                        });
                    }
                    continue;
                }

                var createResult = await CreateDefaultEleveUserAsync(eleve, idEcole, sendSmsToTuteur: sendSms);
                result.Details.Add(createResult);

                switch (createResult.Outcome)
                {
                    case EleveCompteCreateOutcome.Created:
                    case EleveCompteCreateOutcome.LinkedExistingUsername:
                        result.Created++;
                        linkedSet.Add(eleve.IdEleve);
                        break;
                    case EleveCompteCreateOutcome.AlreadyLinked:
                        result.SkippedAlreadyLinked++;
                        break;
                    case EleveCompteCreateOutcome.NoMatricule:
                        result.SkippedNoMatricule++;
                        break;
                    case EleveCompteCreateOutcome.ConflictUsername:
                        result.ConflictsUsername++;
                        break;
                    default:
                        result.Errors++;
                        if (!string.IsNullOrWhiteSpace(createResult.Message))
                            result.ErrorDetails.Add($"IdEleve={eleve.IdEleve}: {createResult.Message}");
                        break;
                }
            }

            return result;
        }

        private async Task TrySendCredentialsSmsToTuteurAsync(
            Eleve eleve,
            int idEcole,
            string matricule,
            string motDePasse)
        {
            try
            {
                var ecole = await _context.Ecoles.AsNoTracking()
                    .FirstOrDefaultAsync(e => e.IdEcole == idEcole);
                if (ecole == null || ecole.AcceptNotification != true)
                    return;

                string? telephone = null;
                if (eleve.IdTuteur.HasValue)
                {
                    telephone = await _context.Tuteurs.AsNoTracking()
                        .Where(t => t.IdTuteur == eleve.IdTuteur.Value)
                        .Select(t => t.Telephone)
                        .FirstOrDefaultAsync();
                }

                if (string.IsNullOrWhiteSpace(telephone))
                {
                    _logger.LogInformation(
                        "SMS compte Élève non envoyé : téléphone tuteur absent (IdEleve={IdEleve})",
                        eleve.IdEleve);
                    return;
                }

                var nomEcole = ecole.Nom ?? "KelasiNaBiso";
                var nomEleve = string.IsNullOrWhiteSpace(eleve.NomComplet) ? matricule : eleve.NomComplet;
                var message =
                    $"📋 {nomEcole}\n" +
                    $"🎓 Compte élève\n" +
                    $"{nomEleve} — User: {matricule}, MDP: {motDePasse}";

                await _smsService.EnvoyerSmsAsync(telephone, message, "COMPTE_ELEVE");
                _logger.LogInformation(
                    "SMS identifiants Élève envoyé au tuteur pour IdEleve={IdEleve}",
                    eleve.IdEleve);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Échec SMS identifiants Élève pour IdEleve={IdEleve}",
                    eleve.IdEleve);
            }
        }
    }
}
