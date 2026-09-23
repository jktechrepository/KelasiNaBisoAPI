using KelasiNaBiso.Data;
using KelasiNaBiso.Helpers;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBisoAPI.Services.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KelasiNaBiso.Services
{
    /// <summary>
    /// Création / liaison du compte Parent pour un tuteur
    /// (partagé entre inscription et POST /api/Tuteur).
    /// </summary>
    public class TuteurCompteService : ITuteurCompteService
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly IUtilisateurRepository _utilisateurRepository;
        private readonly KelasiNaBisoAPI.Services.Repositories.IEmailService _emailService;
        private readonly IFirebaseNotificationService _notificationService;
        private readonly ISmsNotificationService _smsService;
        private readonly ISignalRNotificationService _signalRNotificationService;
        private readonly IInscriptionActiveResolver _inscriptionResolver;
        private readonly ILogger<TuteurCompteService> _logger;

        public TuteurCompteService(
            KelasiNaBisoDbContext context,
            IUtilisateurRepository utilisateurRepository,
            KelasiNaBisoAPI.Services.Repositories.IEmailService emailService,
            IFirebaseNotificationService notificationService,
            ISmsNotificationService smsService,
            ISignalRNotificationService signalRNotificationService,
            IInscriptionActiveResolver inscriptionResolver,
            ILogger<TuteurCompteService> logger)
        {
            _context = context;
            _utilisateurRepository = utilisateurRepository;
            _emailService = emailService;
            _notificationService = notificationService;
            _smsService = smsService;
            _signalRNotificationService = signalRNotificationService;
            _inscriptionResolver = inscriptionResolver;
            _logger = logger;
        }

        /// <summary>
        /// Crée automatiquement un utilisateur Parent/Tuteur par défaut lors de l'inscription d'un nouvel élève
        /// Adapté d'AkademiaAPI - Applique la même logique que la création d'Etudiant
        /// </summary>
        public async Task<UtilisateurInfo?> CreateDefaultTuteurUserAsync(
            Tuteur tuteur,
            int idEcole,
            Eleve? eleve = null,
            bool sendInscriptionNotifications = true,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("🔍 CreateDefaultTuteurUserAsync appelé pour tuteur {TuteurId} (Email: {Email}, Telephone: {Telephone})", 
                    tuteur.IdTuteur, tuteur.Email, tuteur.Telephone);
                
                // ═══════════════════════════════════════════════════════════════════
                // ✅ ÉTAPE 1 : Récupérer le rôle Parent (OBLIGATOIRE avant création utilisateur)
                // ═══════════════════════════════════════════════════════════════════
                var parentRole = await _context.Roles.FirstOrDefaultAsync(r => r.Nom == "Parent");
                if (parentRole == null)
                {
                    _logger.LogWarning("⚠️ Rôle 'Parent' non trouvé, création du rôle...");
                    // Créer le rôle Parent s'il n'existe pas
                    parentRole = new Role
                    {
                        Nom = "Parent",
                        DateCreation = DateTime.Now,
                        Statut = true
                    };
                    _context.Roles.Add(parentRole);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("✅ Rôle 'Parent' créé avec succès (ID: {RoleId})", parentRole.IdRole);
                }
                else
                {
                    _logger.LogInformation("✅ Rôle 'Parent' trouvé (ID: {RoleId})", parentRole.IdRole);
                }
                
                // ✅ Validation : S'assurer que le rôle Parent a un ID valide
                if (parentRole == null || parentRole.IdRole <= 0)
                {
                    _logger.LogError("❌ ERREUR CRITIQUE : Impossible de récupérer ou créer le rôle Parent. parentRole est null ou IdRole invalide.");
                    throw new InvalidOperationException("Le rôle 'Parent' est requis mais n'a pas pu être récupéré ou créé. Vérifiez la base de données.");
                }
                
                _logger.LogInformation("✅ Validation réussie : Rôle Parent disponible (ID: {RoleId}, Nom: {RoleNom})", 
                    parentRole.IdRole, parentRole.Nom);

                // Récupérer l'école
                var ecole = await _context.Ecoles
                    .FirstOrDefaultAsync(e => e.IdEcole == idEcole);
                
                if (ecole == null)
                {
                    _logger.LogError("❌ École non trouvée pour IdEcole {EcoleId}", idEcole);
                    return null;
                }
                
                _logger.LogInformation("✅ École trouvée: {EcoleNom} (ID: {EcoleId})", ecole.Nom, ecole.IdEcole);

                // Utiliser l'email du tuteur s'il est fourni, sinon vide
                string email = tuteur.Email ?? "";
                string telephone = TelephoneNormalizer.Normalize(tuteur.Telephone) ?? "";
                
                // Construire le nom complet du tuteur en premier (avec valeur par défaut si NULL)
                string nomComplet = tuteur.NomComplet ?? "";
                
                // ✅ FIX: S'assurer que NomUtilisateur n'est jamais NULL ou vide (champ [Required])
                if (string.IsNullOrWhiteSpace(nomComplet))
                {
                    nomComplet = "Parent"; // Valeur par défaut si NULL
                    _logger.LogWarning("⚠️ Le nom complet du tuteur est NULL ou vide, utilisation de la valeur par défaut 'Parent'");
                }
                
                // ✨ NOUVEAU : Générer le DefaultUsername basé sur le nom complet + nombre aléatoire
                // Format: NomComplet (sans espaces) + nombre aléatoire (1-999)
                // Exemple: "Marie Dupont" → "MarieDupont456"
                string baseUsername = nomComplet.Replace(" ", "").Replace("-", "").Replace("'", "");
                if (string.IsNullOrWhiteSpace(baseUsername))
                {
                    baseUsername = "Parent"; // Valeur par défaut si le nom complet est vide
                }
                if (baseUsername.Length > 20)
                {
                    baseUsername = baseUsername.Substring(0, 20);
                }
                Random random = new Random();
                int randomNumber = random.Next(1, 1000);
                string defaultUsername = $"{baseUsername}{randomNumber}";
                
                // Le mot de passe par défaut est simple : 123456
                // L'utilisateur DOIT le changer à la première connexion
                string motDePasseParDefaut = "123456";
                
                _logger.LogInformation("🔍 Nom complet: {NomComplet}, Username généré: {Username}", nomComplet, defaultUsername);
                
                // ═══════════════════════════════════════════════════════════════════
                // ✅ MULTI-RÔLES : Vérifier si un utilisateur existe déjà par email/téléphone
                // ═══════════════════════════════════════════════════════════════════
                
                Utilisateur? existingUser = null;
                
                // 1. Vérifier si un utilisateur existe déjà pour ce tuteur (par IdTuteur)
                existingUser = await _context.Utilisateurs
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.IdTuteur == tuteur.IdTuteur);
                
                // 2. Si pas trouvé, chercher par email ou téléphone (pour le multi-rôles)
                if (existingUser == null && (!string.IsNullOrWhiteSpace(email) || !string.IsNullOrWhiteSpace(telephone)))
                {
                    existingUser = await _context.Utilisateurs
                        .Include(u => u.UserRoles)
                            .ThenInclude(ur => ur.Role)
                        .FirstOrDefaultAsync(u => 
                            (!string.IsNullOrWhiteSpace(email) && u.Email == email) ||
                            (!string.IsNullOrWhiteSpace(telephone) && u.Telephone == telephone)
                        );
                }
                
                // 3. Si utilisateur existe, ajouter le rôle Parent (multi-rôles)
                if (existingUser != null)
                {
                    _logger.LogInformation("✅ Utilisateur existant trouvé pour le tuteur '{NomComplet}' (ID: {UserId}, Email: {Email})", 
                        tuteur.NomComplet, existingUser.IdUtilisateur, existingUser.Email);
                    
                    // Recharger les UserRoles pour s'assurer qu'on a les données à jour
                    await _context.Entry(existingUser)
                        .Collection(u => u.UserRoles)
                        .Query()
                        .Include(ur => ur.Role)
                        .LoadAsync();
                    
                    // Vérifier si l'utilisateur a déjà le rôle Parent
                    var hasParentRole = existingUser.UserRoles
                        .Any(ur => ur.Role.Nom == "Parent" && ur.Statut == true);
                    
                    _logger.LogInformation("🔍 Vérification rôle Parent pour utilisateur {UserId}: hasParentRole = {HasRole}", 
                        existingUser.IdUtilisateur, hasParentRole);
                    
                    if (!hasParentRole)
                    {
                        // Ajouter le rôle Parent à l'utilisateur existant
                        _logger.LogInformation("➕ Ajout du rôle Parent (ID: {RoleId}) à l'utilisateur existant (ID: {UserId})", 
                            parentRole.IdRole, existingUser.IdUtilisateur);
                        
                        var roleAdded = await _utilisateurRepository.AddRoleToUserAsync(
                            existingUser.IdUtilisateur,
                            parentRole.IdRole,
                            assignedByUserId: null,
                            isPrimary: false // Ne pas changer le rôle principal
                        );
                        
                        if (roleAdded)
                        {
                            _logger.LogInformation("✅ Rôle Parent ajouté avec succès à l'utilisateur {UserId}", 
                                existingUser.IdUtilisateur);
                            
                            // Recharger les UserRoles après l'ajout pour vérification
                            await _context.Entry(existingUser)
                                .Collection(u => u.UserRoles)
                                .Query()
                                .Include(ur => ur.Role)
                                .LoadAsync();
                            
                            // Vérifier que le rôle a bien été ajouté
                            var verifyParentRole = existingUser.UserRoles
                                .Any(ur => ur.Role.Nom == "Parent" && ur.Statut == true);
                            
                            if (verifyParentRole)
                            {
                                _logger.LogInformation("✅ Vérification réussie : Le rôle Parent est bien présent dans UserRoles pour l'utilisateur {UserId}", 
                                    existingUser.IdUtilisateur);
                            }
                            else
                            {
                                _logger.LogWarning("⚠️ ATTENTION : Le rôle Parent n'a pas été trouvé après l'ajout pour l'utilisateur {UserId}", 
                                    existingUser.IdUtilisateur);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("⚠️ Échec de l'ajout du rôle Parent à l'utilisateur {UserId}", 
                                existingUser.IdUtilisateur);
                        }
                    }
                    else
                    {
                        _logger.LogInformation("ℹ️ L'utilisateur {UserId} a déjà le rôle Parent", 
                            existingUser.IdUtilisateur);
                    }
                    
                    // Mettre à jour IdTuteur si nécessaire
                    if (existingUser.IdTuteur != tuteur.IdTuteur)
                    {
                        existingUser.IdTuteur = tuteur.IdTuteur;
                        await _context.SaveChangesAsync();
                    }
                    
                    // Retourner les infos de l'utilisateur existant et envoyer les notifications
                    var utilisateurInfo = new UtilisateurInfo
                    {
                        IdUtilisateur = existingUser.IdUtilisateur,
                        IdTuteur = existingUser.IdTuteur ?? tuteur.IdTuteur,
                        Email = existingUser.Email ?? email,
                        DefaultUsername = existingUser.DefaultUsername ?? defaultUsername,
                        Telephone = existingUser.Telephone ?? telephone,
                        MotDePasseParDefaut = "", // Ne pas révéler le mot de passe
                        NomComplet = existingUser.NomUtilisateur ?? nomComplet,
                        Role = "Parent"
                    };
                    
                    if (sendInscriptionNotifications)
                    {
                        await SendNotificationForExistingUserAsync(existingUser, ecole, eleve);
                    }
                    
                    return utilisateurInfo;
                }
                
                // ═══════════════════════════════════════════════════════════════════
                // ✅ MULTI-RÔLES : Créer un nouvel utilisateur avec UserRole
                // ═══════════════════════════════════════════════════════════════════
                
                // Créer l'utilisateur Parent/Tuteur par défaut (sans IdRole)
                var tuteurUser = new Utilisateur
                {
                    IdTuteur = tuteur.IdTuteur,
                    ReferenceUtilisateur = Guid.NewGuid(),
                    NomUtilisateur = nomComplet, // ✅ Utiliser la variable 'nomComplet' qui est garantie non-null/non-vide
                    PostNomUtilisateur = "",
                    PrenomUtilisateur = "",
                    Email = email,
                    DefaultUsername = defaultUsername,
                    Telephone = telephone,
                    PhotoUrl = tuteur.PhotoTuteurUrl,
                    Genre = tuteur.Genre,
                    MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(motDePasseParDefaut),
                    Statut = true,
                    DateCreation = DateTime.Now,
                    IsConnecte = false,
                    DoitChangerMotDePasse = true, // ✨ FORCER le changement de mot de passe à la première connexion
                    // ⚠️ TEMPORAIRE : Définir IdRole pour compatibilité avec la base de données (si IdRole n'est pas encore nullable)
                    // TODO : Retirer cette ligne après avoir exécuté le script MAKE_IDROLE_NULLABLE_PRODUCTION.sql en production
                    IdRole = parentRole.IdRole, // ✅ Définir temporairement pour éviter l'erreur "Column 'IdRole' cannot be null"
                    IdEcole = idEcole
                };

                _logger.LogInformation("🔍 Création de l'utilisateur avec les valeurs: NomUtilisateur={Nom}, Email={Email}, IdEcole={EcoleId}, IdTuteur={TuteurId}", 
                    tuteurUser.NomUtilisateur, tuteurUser.Email, tuteurUser.IdEcole, tuteurUser.IdTuteur);

                // ✅ Validation avant ajout
                try
                {
                _context.Utilisateurs.Add(tuteurUser);
                    _logger.LogInformation("✅ Utilisateur ajouté au contexte. Validation en cours...");
                    
                    // Valider que les champs requis ne sont pas NULL
                    if (string.IsNullOrWhiteSpace(tuteurUser.NomUtilisateur))
                    {
                        throw new InvalidOperationException("NomUtilisateur est requis mais est NULL ou vide");
                    }
                    if (string.IsNullOrWhiteSpace(tuteurUser.MotDePasseHash))
                    {
                        throw new InvalidOperationException("MotDePasseHash est requis mais est NULL ou vide");
                    }
                    
                    _logger.LogInformation("✅ Validation réussie. Sauvegarde en cours...");
                await _context.SaveChangesAsync();
                    _logger.LogInformation("✅ Utilisateur sauvegardé avec succès. IdUtilisateur={UserId}", tuteurUser.IdUtilisateur);
                }
                catch (Exception saveEx)
                {
                    _logger.LogError(saveEx, "❌ ERREUR lors de la sauvegarde de l'utilisateur: {ErrorMessage}", saveEx.Message);
                    if (saveEx.InnerException != null)
                    {
                        _logger.LogError(saveEx.InnerException, "❌ Exception interne: {InnerMessage}", saveEx.InnerException.Message);
                    }
                    throw; // Re-lancer l'exception pour qu'elle soit capturée par le catch principal
                }
                
                // ✅ Créer le UserRole pour le système multi-rôles
                _logger.LogInformation("🔍 Création du UserRole pour IdUtilisateur={UserId}, IdRole={RoleId}", 
                    tuteurUser.IdUtilisateur, parentRole.IdRole);
                
                UserRole userRole;
                try
                {
                    userRole = new UserRole
                    {
                        IdUtilisateur = tuteurUser.IdUtilisateur,
                        IdRole = parentRole.IdRole,
                        IsPrimary = true, // Premier rôle = principal
                        Statut = true,
                        DateAttribution = DateTime.Now,
                        IdUtilisateurAttribution = null
                    };
                    
                    _context.UserRoles.Add(userRole);
                    _logger.LogInformation("✅ UserRole ajouté au contexte. Sauvegarde en cours...");
                    
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("✅ UserRole sauvegardé avec succès. IdUserRole={UserRoleId}", userRole.IdUserRole);
                }
                catch (Exception roleEx)
                {
                    _logger.LogError(roleEx, "❌ ERREUR lors de la sauvegarde du UserRole: {ErrorMessage}", roleEx.Message);
                    if (roleEx.InnerException != null)
                    {
                        _logger.LogError(roleEx.InnerException, "❌ Exception interne: {InnerMessage}", roleEx.InnerException.Message);
                    }
                    throw; // Re-lancer l'exception pour qu'elle soit capturée par le catch principal
                }
                
                _logger.LogInformation("✅ UserRole créé avec succès : IdUtilisateur={UserId}, IdRole={RoleId} (Role: {RoleName}), IsPrimary={IsPrimary}", 
                    userRole.IdUtilisateur, userRole.IdRole, parentRole.Nom, userRole.IsPrimary);
                
                // Vérifier que le UserRole a bien été créé
                var verifyUserRole = await _context.UserRoles
                    .Include(ur => ur.Role)
                    .FirstOrDefaultAsync(ur => ur.IdUtilisateur == tuteurUser.IdUtilisateur && ur.IdRole == parentRole.IdRole);
                
                if (verifyUserRole != null)
                {
                    _logger.LogInformation("✅ Vérification réussie : UserRole trouvé dans la base de données (ID: {UserRoleId})", 
                        verifyUserRole.IdUserRole);
                }
                else
                {
                    _logger.LogError("❌ ERREUR : UserRole non trouvé dans la base de données après création pour utilisateur {UserId}", 
                        tuteurUser.IdUtilisateur);
                }
                
                _logger.LogInformation("✅ Utilisateur Parent créé pour '{NomComplet}' - Email: {Email}, Username: {Username}", 
                    nomComplet, tuteurUser.Email, defaultUsername);
                
                if (sendInscriptionNotifications)
                {
                    // Récupérer le nom de l'école et vérifier AcceptNotification
                    string nomEcole = ecole.Nom ?? "KelasiNaBiso";
                    bool acceptNotification = ecole.AcceptNotification == true;
                
                    // ⚠️ Vérifier si l'école accepte les notifications SMS
                    if (!acceptNotification)
                    {
                        _logger.LogInformation($"📵 École {nomEcole} n'accepte pas les notifications SMS - SMS non envoyé pour l'inscription de {eleve?.NomComplet}");
                        // On continue quand même pour les autres notifications (email, push)
                    }
                
                    // Envoyer l'email de bienvenue (si email fourni)
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                    
                        // ✨ Récupérer les informations de l'enfant pour l'email
                        string nomEnfant = eleve?.NomComplet ?? "";
                        string classeEnfant = "";
                        if (eleve != null)
                        {
                            var insActive = await _inscriptionResolver.GetInscriptionActiveAsync(eleve.IdEleve);
                            classeEnfant = insActive?.Classe?.NomClasse ?? "";
                        }
                        string matriculeEnfant = eleve?.Matricule ?? "";
                    
                        // ✨ Envoyer les notifications EN PARALLÈLE (Email + Push + SMS)
                    
                        // 📧 Notification Email (en parallèle)
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await _emailService.SendWelcomeEmailAsync(
                                    email,
                                    nomComplet,
                                    defaultUsername,
                                    telephone,
                                    motDePasseParDefaut,
                                    "Parent",
                                    nomEcole,
                                    tuteur.Genre ?? "Masculin",  // ✨ Passer le genre du tuteur
                                    null,                         // Pas de fonction pour parent
                                    null,                         // Pas de matricule pour parent
                                    nomEnfant,                    // ✨ Nom de l'enfant
                                    classeEnfant,                 // ✨ Classe de l'enfant
                                    matriculeEnfant               // ✨ Matricule de l'enfant
                                );
                                _logger.LogInformation($"✅ Email de bienvenue envoyé à {email}");
                            }
                            catch (Exception emailEx)
                            {
                                _logger.LogError(emailEx, $"❌ Échec de l'envoi de l'email à {email}");
                            }
                        });
                    
                        // 📲 Notification Push (en parallèle)
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                string titre = $"🎓 Inscription de {nomEnfant}";
                                string corps = $"Bienvenue sur KelasiNaBiso ! {nomEnfant} a été inscrit dans la classe {classeEnfant}.";
                            
                                var donnees = new Dictionary<string, string>
                                {
                                    { "type", "INSCRIPTION_ENFANT" },
                                    { "idEleve", eleve?.IdEleve.ToString() ?? "N/A" },
                                    { "nomEleve", nomEnfant },
                                    { "classe", classeEnfant },
                                    { "matricule", matriculeEnfant }
                                };
                            
                                var pushEnvoye = await _notificationService.EnvoyerNotificationAUtilisateurAsync(
                                    tuteurUser.IdUtilisateur,
                                    titre,
                                    corps,
                                    donnees
                                );
                            
                                if (pushEnvoye)
                                {
                                    _logger.LogInformation($"✅ Notification PUSH Firebase inscription envoyée à {nomComplet}");
                                }
                                else
                                {
                                    _logger.LogWarning($"⚠️ Échec notification PUSH Firebase inscription pour {nomComplet}");
                                }
                            }
                            catch (Exception pushEx)
                            {
                                _logger.LogError(pushEx, $"❌ Erreur lors de l'envoi notification PUSH Firebase inscription pour {nomComplet}");
                            }
                        });
                    
                        // 🔔 Notification SignalR (en parallèle)
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await _signalRNotificationService.SendCustomNotificationAsync(
                                    tuteurUser.IdUtilisateur,
                                    $"🎓 Inscription de {nomEnfant}",
                                    $"{nomEnfant} a été inscrit dans la classe {classeEnfant}. Nom d'utilisateur: {defaultUsername}, Mot de passe: {motDePasseParDefaut}",
                                    "INSCRIPTION"
                                );
                                _logger.LogInformation($"✅ Notification SignalR inscription envoyée à {nomComplet}");
                            }
                            catch (Exception signalREx)
                            {
                                _logger.LogError(signalREx, $"❌ Erreur lors de l'envoi notification SignalR inscription pour {nomComplet}");
                            }
                        });
                    
                        // 📱 Notification SMS (en parallèle)
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                if (!string.IsNullOrWhiteSpace(telephone) && acceptNotification)
                                {
                                    // Construire le message personnalisé
                                    string messageSms = "";
                                
                                    // Si on a le nom de l'école, l'ajouter avec titre
                                    if (!string.IsNullOrWhiteSpace(nomEcole))
                                    {
                                        messageSms = $"📋 {nomEcole}\n";
                                        messageSms += $"🎓 Confirmation d'inscription\n";
                                        messageSms += $"{nomEnfant} est inscrit en {classeEnfant}.\n";
                                        messageSms += $"User: {defaultUsername}, MDP: {motDePasseParDefaut}";
                                    
                                        // Ajouter email si disponible
                                        if (!string.IsNullOrWhiteSpace(email))
                                        {
                                            messageSms += $", Email: {email}";
                                        }
                                    }
                                    else
                                    {
                                        // Format simplifié sans école
                                        messageSms = $"🎓 Confirmation d'inscription\n";
                                        messageSms += $"{nomEnfant} est inscrit en {classeEnfant}.\n";
                                        messageSms += $"User: {defaultUsername}, MDP: {motDePasseParDefaut}";
                                    
                                        // Ajouter email si disponible
                                        if (!string.IsNullOrWhiteSpace(email))
                                        {
                                            messageSms += $", Email: {email}";
                                        }
                                    }
                                
                                    var smsLog = await _smsService.EnvoyerSmsAsync(
                                        telephone,
                                        messageSms,
                                        "INSCRIPTION_ENFANT"
                                    );
                                
                                    if (smsLog != null && smsLog.Statut != "failed")
                                    {
                                        _logger.LogInformation($"✅ SMS inscription envoyé à {nomComplet} (Coût: {smsLog.CoutUsd} USD)");
                                    }
                                    else
                                    {
                                        _logger.LogWarning($"⚠️ Échec SMS inscription pour {nomComplet}");
                                    }
                                }
                                else
                                {
                                    _logger.LogInformation($"ℹ️ Aucun numéro de téléphone pour {nomComplet}, SMS non envoyé");
                                }
                            }
                            catch (Exception smsEx)
                            {
                                _logger.LogError(smsEx, $"❌ Erreur lors de l'envoi SMS inscription pour {nomComplet}");
                            }
                        });
                    
                        _logger.LogInformation("📧 Notifications (Email + Push + SMS) programmées pour {Email}", email);
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ Aucun email fourni pour le tuteur '{NomComplet}'. Seules les notifications Push et SMS seront envoyées.", 
                            nomComplet);
                    
                        string nomEnfantSansEmail = eleve?.NomComplet ?? "";
                        string classeEnfantSansEmail = "";
                        if (eleve != null)
                        {
                            var insActiveSansEmail = await _inscriptionResolver.GetInscriptionActiveAsync(eleve.IdEleve);
                            classeEnfantSansEmail = insActiveSansEmail?.Classe?.NomClasse ?? "";
                        }

                        // 📲 Envoyer quand même Push et SMS même sans email
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                string nomEnfant = nomEnfantSansEmail;
                                string classeEnfant = classeEnfantSansEmail;
                            
                                string titre = $"🎓 Inscription de {nomEnfant}";
                                string corps = $"Bienvenue sur KelasiNaBiso ! {nomEnfant} a été inscrit dans la classe {classeEnfant}.";
                            
                                var donnees = new Dictionary<string, string>
                                {
                                    { "type", "INSCRIPTION_ENFANT" },
                                    { "idEleve", eleve?.IdEleve.ToString() ?? "N/A" },
                                    { "nomEleve", nomEnfant },
                                    { "classe", classeEnfant }
                                };
                            
                                await _notificationService.EnvoyerNotificationAUtilisateurAsync(
                                    tuteurUser.IdUtilisateur,
                                    titre,
                                    corps,
                                    donnees
                                );
                            
                                // 🔔 Notification SignalR
                                try
                                {
                                    await _signalRNotificationService.SendCustomNotificationAsync(
                                        tuteurUser.IdUtilisateur,
                                        $"🎓 Inscription de {nomEnfant}",
                                        $"{nomEnfant} a été inscrit dans la classe {classeEnfant}. Nom d'utilisateur: {defaultUsername}, Mot de passe: {motDePasseParDefaut}",
                                        "INSCRIPTION"
                                    );
                                    _logger.LogInformation($"✅ Notification SignalR inscription envoyée (utilisateur existant)");
                                }
                                catch (Exception signalREx)
                                {
                                    _logger.LogError(signalREx, $"❌ Erreur SignalR inscription");
                                }
                            
                                // SMS si téléphone disponible et accepté par l'école
                                if (!string.IsNullOrWhiteSpace(telephone) && acceptNotification)
                                {
                                    // Construire le message personnalisé
                                    string messageSms = "";
                                
                                    // Si on a le nom de l'école, l'ajouter avec titre
                                    if (!string.IsNullOrWhiteSpace(nomEcole))
                                    {
                                        messageSms = $"📋 {nomEcole}\n";
                                        messageSms += $"🎓 Confirmation d'inscription\n";
                                        messageSms += $"{nomEnfant} est inscrit en {classeEnfant}.\n";
                                        messageSms += $"User: {defaultUsername}, MDP: {motDePasseParDefaut}";
                                    }
                                    else
                                    {
                                        // Format simplifié sans école
                                        messageSms = $"🎓 Confirmation d'inscription\n";
                                        messageSms += $"{nomEnfant} est inscrit en {classeEnfant}.\n";
                                        messageSms += $"User: {defaultUsername}, MDP: {motDePasseParDefaut}";
                                    }
                                
                                    await _smsService.EnvoyerSmsAsync(telephone, messageSms, "INSCRIPTION_ENFANT");
                                }
                            }
                            catch (Exception notifEx)
                            {
                                _logger.LogError(notifEx, "⚠️ Erreur notifications sans email: {ErrorMessage}", notifEx.Message);
                            }
                        });
                    }
                
                    // Retourner les informations du compte créé
                }

                return new UtilisateurInfo
                {
                    IdUtilisateur = tuteurUser.IdUtilisateur,
                    IdTuteur = tuteurUser.IdTuteur,
                    Email = tuteurUser.Email ?? "",
                    DefaultUsername = tuteurUser.DefaultUsername ?? "",
                    Telephone = tuteurUser.Telephone ?? "",
                    MotDePasseParDefaut = motDePasseParDefaut,
                    NomComplet = nomComplet,
                    Role = "Parent"
                };
            }
            catch (Exception ex)
            {
                // Log l'erreur complète avec toutes les exceptions internes
                _logger.LogError(ex, "❌ ERREUR lors de la création de l'utilisateur Parent par défaut pour tuteur {TuteurId}: {ErrorMessage}", 
                    tuteur.IdTuteur, ex.Message);
                
                // Log l'exception interne si elle existe
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, "❌ Exception interne: {InnerMessage}", ex.InnerException.Message);
                    
                    // Si c'est une DbUpdateException, log les détails supplémentaires
                    if (ex.InnerException is Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
                    {
                        _logger.LogError("❌ DbUpdateException détectée. Entrées: {Entries}", 
                            string.Join(", ", dbEx.Entries.Select(e => $"{e.Entity.GetType().Name} - {string.Join(", ", e.Properties.Select(p => $"{p.Metadata.Name}={p.CurrentValue}"))}")));
                    }
                }
                
                // Log le stack trace complet
                _logger.LogError("❌ StackTrace: {StackTrace}", ex.StackTrace);
                
                return null;
            }
        }

        /// <summary>
        /// Envoie les notifications pour un utilisateur existant (cas réinscription ou deuxième enfant)
        /// </summary>
        private async Task SendNotificationForExistingUserAsync(Utilisateur utilisateur, Ecole ecole, Eleve eleve)
        {
            try
            {
                string nomEcole = ecole.Nom ?? "KelasiNaBiso";
                bool acceptNotification = ecole.AcceptNotification == true;
                string nomEnfant = eleve?.NomComplet ?? "";
                string classeEnfant = "";
                if (eleve != null)
                {
                    var insActive = await _inscriptionResolver.GetInscriptionActiveAsync(eleve.IdEleve);
                    classeEnfant = insActive?.Classe?.NomClasse ?? "";
                }
                string defaultUsername = utilisateur.DefaultUsername ?? "";
                
                // ⚠️ Vérifier si l'école accepte les notifications SMS
                if (!acceptNotification)
                {
                    _logger.LogInformation($"📵 École {nomEcole} n'accepte pas les notifications SMS - SMS non envoyé pour l'inscription de {nomEnfant}");
                    // On continue quand même pour les autres notifications (email, push)
                }
                
                // Email si disponible
                if (!string.IsNullOrWhiteSpace(utilisateur.Email))
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _emailService.SendWelcomeEmailAsync(
                                utilisateur.Email,
                                utilisateur.NomUtilisateur ?? "",
                                defaultUsername,
                                utilisateur.Telephone ?? "",
                                "", // Pas de mot de passe
                                "Parent",
                                nomEcole,
                                utilisateur.Genre ?? "Masculin",
                                null,
                                null,
                                nomEnfant,
                                classeEnfant,
                                eleve?.Matricule ?? ""
                            );
                            _logger.LogInformation($"✅ Email de bienvenue envoyé à {utilisateur.Email}");
                        }
                        catch (Exception emailEx)
                        {
                            _logger.LogError(emailEx, $"❌ Échec de l'envoi de l'email à {utilisateur.Email}");
                        }
                    });
                }

                // Push notification
                _ = Task.Run(async () =>
                {
                    try
                    {
                        string titre = $"🎓 Inscription de {nomEnfant}";
                        string corps = $"Bienvenue sur KelasiNaBiso ! {nomEnfant} a été inscrit dans la classe {classeEnfant}.";

                        var donnees = new Dictionary<string, string>
                        {
                            { "type", "INSCRIPTION_ENFANT" },
                            { "idEleve", eleve?.IdEleve.ToString() ?? "N/A" },
                            { "nomEleve", nomEnfant },
                            { "classe", classeEnfant }
                        };

                        var pushEnvoye = await _notificationService.EnvoyerNotificationAUtilisateurAsync(
                            utilisateur.IdUtilisateur,
                            titre,
                            corps,
                            donnees
                        );

                        if (pushEnvoye)
                        {
                            _logger.LogInformation($"✅ Notification PUSH Firebase inscription envoyée à {utilisateur.NomUtilisateur}");
                        }
                        else
                        {
                            _logger.LogWarning($"⚠️ Échec notification PUSH Firebase inscription pour {utilisateur.NomUtilisateur}");
                        }
                    }
                    catch (Exception pushEx)
                    {
                        _logger.LogError(pushEx, $"❌ Erreur lors de l'envoi notification PUSH Firebase inscription");
                    }
                });

                // 🔔 Notification SignalR (en parallèle)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _signalRNotificationService.SendCustomNotificationAsync(
                            utilisateur.IdUtilisateur,
                            $"🎓 Inscription de {nomEnfant}",
                            $"{nomEnfant} a été inscrit dans la classe {classeEnfant}. Nom d'utilisateur: {defaultUsername}",
                            "INSCRIPTION"
                        );
                        _logger.LogInformation($"✅ Notification SignalR inscription envoyée à {utilisateur.NomUtilisateur} (utilisateur existant)");
                    }
                    catch (Exception signalREx)
                    {
                        _logger.LogError(signalREx, $"❌ Erreur SignalR inscription (utilisateur existant)");
                    }
                });

                // SMS si téléphone disponible et accepté par l'école
                if (!string.IsNullOrWhiteSpace(utilisateur.Telephone) && acceptNotification)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            // Construire le message personnalisé
                            string messageSms = "";
                            
                            // Si on a le nom de l'école, l'ajouter avec titre
                            if (!string.IsNullOrWhiteSpace(nomEcole))
                            {
                                messageSms = $"📋 {nomEcole}\n";
                                messageSms += $"🎓 Confirmation d'inscription\n";
                                messageSms += $"{nomEnfant} est inscrit en {classeEnfant}.\n";
                                messageSms += $"User: {defaultUsername}";
                                
                                // Ajouter email si disponible
                                if (!string.IsNullOrWhiteSpace(utilisateur.Email))
                                {
                                    messageSms += $", Email: {utilisateur.Email}";
                                }
                            }
                            else
                            {
                                // Format simplifié sans école
                                messageSms = $"🎓 Confirmation d'inscription\n";
                                messageSms += $"{nomEnfant} est inscrit en {classeEnfant}.\n";
                                messageSms += $"User: {defaultUsername}";
                                
                                // Ajouter email si disponible
                                if (!string.IsNullOrWhiteSpace(utilisateur.Email))
                                {
                                    messageSms += $", Email: {utilisateur.Email}";
                                }
                            }

                            var smsLog = await _smsService.EnvoyerSmsAsync(
                                utilisateur.Telephone,
                                messageSms,
                                "INSCRIPTION_ENFANT"
                            );
                            
                            if (smsLog != null && smsLog.Statut != "failed")
                            {
                                _logger.LogInformation($"✅ SMS inscription envoyé (Coût: {smsLog.CoutUsd} USD)");
                            }
                            else
                            {
                                _logger.LogWarning($"⚠️ Échec SMS inscription");
                            }
                        }
                        catch (Exception smsEx)
                        {
                            _logger.LogError(smsEx, $"❌ Erreur lors de l'envoi SMS inscription");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Erreur lors de l'envoi des notifications pour utilisateur existant");
            }
        }
    }
}
