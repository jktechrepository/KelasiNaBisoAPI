using System;
using System.Collections.Generic;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBisoAPI.Services.Repositories;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using KelasiNaBiso.Models.DTOs.Pagination;
using KelasiNaBiso.Helpers;
using KelasiNaBiso.Models.DTOs.Vitrine;
using System.Linq;

namespace KelasiNaBiso.Services
{
    public class EcoleService : IEcoleRepository
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IInscriptionActiveResolver _inscriptionResolver;

        public EcoleService(KelasiNaBisoDbContext context, IEmailService emailService, IInscriptionActiveResolver inscriptionResolver)
        {
            _context = context;
            _emailService = emailService;
            _inscriptionResolver = inscriptionResolver;
        }

        public async Task<IEnumerable<Ecole>> GetAllAsync()
        {
            return await _context.Ecoles
               // .Include(e => e.Classes)
              //  .Include(e => e.Utilisateurs)
                //.Include(e => e.Tuteurs)
               // .Include(e => e.Agents)
               // .Include(e => e.Sections)
               // .Include(e => e.AnneeScolaires)
               // .Include(e => e.Inscriptions)
               // .Include(e => e.GroupesMessages)
                .Where(e => e.Statut == true) // ? Filtrer uniquement les �coles actives
                .ToListAsync();
        }

        public async Task<Ecole> GetByIdAsync(int id)
        {
            return await _context.Ecoles
               // .Include(e => e.Classes)
               // .Include(e => e.Utilisateurs)
               // .Include(e => e.Tuteurs)
               // .Include(e => e.Agents)
               // .Include(e => e.Sections)
               // .Include(e => e.AnneeScolaires)
               // .Include(e => e.Inscriptions)
               // .Include(e => e.GroupesMessages)
                .Where(e => e.Statut == true) // ? Filtrer uniquement les �coles actives
                .FirstOrDefaultAsync(e => e.IdEcole == id);
        }

        public async Task<Ecole> GetByNomAsync(string nom)
        {
            return await _context.Ecoles
                //.Include(e => e.Classes)
               // .Include(e => e.Utilisateurs)
               // .Include(e => e.Tuteurs)
               // .Include(e => e.Agents)
               // .Include(e => e.Sections)
               // .Include(e => e.AnneeScolaires)
               // .Include(e => e.Inscriptions)
               // .Include(e => e.GroupesMessages)
                .Where(e => e.Statut == true) // ? Filtrer uniquement les �coles actives
                .FirstOrDefaultAsync(e => e.Nom == nom);
        }

        //public async Task<Ecole> GetByCodeAsync(string code)
        //{
        //    return await _context.Ecoles
        //        .Include(e => e.Classes)
        //        .Include(e => e.Utilisateurs)
        //        .Include(e => e.Tuteurs)
        //        .Include(e => e.Enseignants)
        //        .Include(e => e.Sections)
        //        .Include(e => e.AnneeScolaires)
        //        .Include(e => e.Inscriptions)
        //        .Include(e => e.GroupesMessages)
        //        .FirstOrDefaultAsync(e => e.Code == code);
        //}

        //public async Task<IEnumerable<Ecole>> GetByStatutAsync(bool statut)
        //{
        //    return await _context.Ecoles
        //        .Include(e => e.Classes)
        //        .Include(e => e.Utilisateurs)
        //        .Include(e => e.Tuteurs)
        //        .Include(e => e.Enseignants)
        //        .Include(e => e.Sections)
        //        .Include(e => e.AnneeScolaires)
        //        .Include(e => e.Inscriptions)
        //        .Include(e => e.GroupesMessages)
        //        .Where(e => e.Statut == statut)
        //        .ToListAsync();
        //}

        public async Task<Ecole> CreateAsync(Ecole ecole)
        {
            ecole.DateCreation = DateTime.Now;
            
            _context.Ecoles.Add(ecole);
            await _context.SaveChangesAsync();
            
            // ? LOGIQUE CORRIG�E : Cr�er d'abord un Agent (directeur), puis un Utilisateur li� � cet Agent
            await CreateDefaultDirecteurAgentAsync(ecole);
            
            return ecole;
        }

        public async Task<Ecole> UpdateAsync(Ecole ecole)
        {
            var existingEcole = await _context.Ecoles.FindAsync(ecole.IdEcole);
            if (existingEcole == null)
                return null;

            _context.Entry(existingEcole).CurrentValues.SetValues(ecole);
            await _context.SaveChangesAsync();
            return existingEcole;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var ecole = await _context.Ecoles.FindAsync(id);
            if (ecole == null)
                return false;

            _context.Ecoles.Remove(ecole);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Ecoles.AnyAsync(e => e.IdEcole == id);
        }

        public async Task<bool> ExistsByNomAsync(string nom)
        {
            return await _context.Ecoles.AnyAsync(e => e.Nom == nom);
        }

        //public async Task<bool> ExistsByCodeAsync(string code)
        //{
        //    return await _context.Ecoles.AnyAsync(e => e.Code == code);
        //}

        public async Task<IEnumerable<Classe>> GetClassesAsync(int idEcole)
        {
            return await _context.Classes
               // .Include(c => c.Section)
               // .Include(c => c.Option)
               // .Include(c => c.Eleves)
               // .Include(c => c.Cours)
               // .Include(c => c.Inscriptions)
               // .Include(c => c.Evaluations)
                .Where(c => c.Direction.IdEcole == idEcole)
                .ToListAsync();
        }

        public async Task<IEnumerable<Utilisateur>> GetUtilisateursAsync(int idEcole)
        {
            return await _context.Utilisateurs
                .Include(u => u.Role)
                .Where(u => u.IdEcole == idEcole)
                .ToListAsync();
        }

        public async Task<IEnumerable<Tuteur>> GetTuteursAsync(int idEcole)
        {
            return await _inscriptionResolver
                .FilterTuteursInEcole(_context.Tuteurs, idEcole)
                .ToListAsync();
        }

        public async Task<IEnumerable<Agent>> GetAgentsAsync(int idEcole)
        {
            return await _context.Agents
                .Where(e => e.IdEcole == idEcole)
                .ToListAsync();
        }

        public async Task<PagedResult<Agent>> GetAgentsByRoleAsync(int idEcole, string roleNom, PagedRequest request)
        {
            request ??= new PagedRequest();

            if (string.IsNullOrWhiteSpace(roleNom))
            {
                return new PagedResult<Agent>(new List<Agent>(), 0, request.PageNumber, request.PageSize);
            }

            var normalizedRole = roleNom.Trim().ToLower();

            var query = _context.Agents
                .Where(a => a.IdEcole == idEcole &&
                            a.Statut == true &&
                            !string.IsNullOrEmpty(a.RoleAgent) &&
                            a.RoleAgent.ToLower() == normalizedRole);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();
                query = query.Where(a =>
                    (a.Nom ?? string.Empty).ToLower().Contains(term) ||
                    (a.Postnom ?? string.Empty).ToLower().Contains(term) ||
                    (a.Prenom ?? string.Empty).ToLower().Contains(term) ||
                    (a.EmailAgent ?? string.Empty).ToLower().Contains(term) ||
                    (a.TelephoneAgent ?? string.Empty).ToLower().Contains(term) ||
                    (a.Fonction ?? string.Empty).ToLower().Contains(term));
            }

            query = request.SortBy switch
            {
                "Nom" => request.SortDescending ? query.OrderByDescending(a => a.Nom) : query.OrderBy(a => a.Nom),
                "Prenom" => request.SortDescending ? query.OrderByDescending(a => a.Prenom) : query.OrderBy(a => a.Prenom),
                "DateCreation" => request.SortDescending ? query.OrderByDescending(a => a.DateCreation) : query.OrderBy(a => a.DateCreation),
                _ => request.SortDescending ? query.OrderByDescending(a => a.IdAgent) : query.OrderBy(a => a.IdAgent)
            };

            var total = await query.CountAsync();

            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PagedResult<Agent>(data, total, request.PageNumber, request.PageSize);
        }

        public async Task<IEnumerable<Section>> GetSectionsAsync(int idEcole)
        {
            return await _context.Sections
              //  .Include(s => s.Options)
              //  .Include(s => s.Classes)
                .Where(s => s.IdEcole == idEcole)
                .ToListAsync();
        }

        public async Task<IEnumerable<AnneeScolaire>> GetAnneeScolairesAsync(int idEcole)
        {
            return await _context.AnneeScolaires
              //  .Include(a => a.Inscriptions)
              //  .Include(a => a.Notes)
                .Where(a => a.IdEcole == idEcole)
                .ToListAsync();
        }

        public async Task<IEnumerable<Inscription>> GetInscriptionsAsync(int idEcole)
        {
            return await _context.Inscriptions
             //   .Include(i => i.Eleve)
              //  .Include(i => i.Classe)
              //  .Include(i => i.AnneeScolaire)
                .Where(i => i.IdEcole == idEcole)
                .ToListAsync();
        }

        public async Task<IEnumerable<GroupeMessage>> GetGroupesMessagesAsync(int idEcole)
        {
            return await _context.GroupeMessages
               // .Include(g => g.Utilisateur)
               // .Include(g => g.Messages)
                .Where(g => g.IdEcole == idEcole)
                .ToListAsync();
        }

        /// <summary>
        /// ? NOUVELLE LOGIQUE : Cr�e automatiquement un Agent (Manager G�n�ral) et son compte Utilisateur
        /// lors de la cr�ation d'une �cole
        /// 
        /// PROCESSUS :
        /// 1. Cr�er un Agent avec la fonction "Manager G�n�ral"
        /// 2. Cr�er un Utilisateur li� � cet Agent avec le r�le "Admin"
        /// 
        /// Cette approche respecte la logique m�tier :
        /// - Un Utilisateur est soit un Agent, soit un Parent (Tuteur)
        /// - Le Manager G�n�ral est un Agent avec des droits Admin sur toute l'�cole
        /// </summary>
        private async Task CreateDefaultDirecteurAgentAsync(Ecole ecole)
        {
            try
            {
                // ? V�RIFICATION UNICIT� EMAIL : V�rifier si l'email existe d�j�
                string emailDirecteur = ecole.EmailContact?.Trim() ?? "";
                
                if (!string.IsNullOrEmpty(emailDirecteur))
                {
                    var emailExists = await _context.Utilisateurs.AnyAsync(u => u.Email == emailDirecteur);
                    if (emailExists)
                    {
                        Console.WriteLine($"?? Un utilisateur avec l'email '{emailDirecteur}' existe d�j�. " +
                                        $"Agent directeur non cr�� pour l'�cole '{ecole.Nom}'.");
                        return;
                    }
                }

                // 1?? CR�ER L'AGENT MANAGER G�N�RAL
                string nomCompletResponsable = ecole.NomCompletResponsable?.Trim() ?? "Manager General";
                
                // Parser le nom complet (format: "Prenom Nom Postnom" ou "Nom Postnom Prenom")
                string[] partiesNom = nomCompletResponsable.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string prenom = partiesNom.Length > 0 ? partiesNom[0] : "Manager";
                string nom = partiesNom.Length > 1 ? partiesNom[1] : "General";
                string postnom = partiesNom.Length > 2 ? string.Join(" ", partiesNom.Skip(2)) : "";

                var managerAgent = new Agent
                {
                    Nom = nom,
                    Postnom = postnom,
                    Prenom = prenom,
                    Genre = ecole.GenreResponsable ?? "Masculin",
                    DateNaissance = DateTime.Now.AddYears(-35), // Age par d�faut : 35 ans
                    TelephoneAgent = ecole.Telephone,
                    EmailAgent = emailDirecteur,
                    Statut = true,
                    EtatCivil = "Mari�",
                    Fonction = "Manager G�n�ral", // ? Fonction Manager G�n�ral de l'�cole
                    RoleAgent = "Administrateur",
                    IdEcole = ecole.IdEcole,
                    Province = ecole.Province,
                    Ville = ecole.Ville,
                    Commune = ecole.Commune,
                    Quartier = ecole.Quartier,
                    Avenue = ecole.Avenue,
                    Numero = ecole.Numero,
                    DateCreation = DateTime.Now
                };

                // G�n�rer le matricule pour l'agent manager
                string matricule = await GenerateMatriculeManagerGeneral(ecole);
                managerAgent.Matricule = matricule;

                _context.Agents.Add(managerAgent);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"? Agent Manager G�n�ral cr�� : {nomCompletResponsable} - Matricule: {matricule}");

                // 2?? CR�ER L'UTILISATEUR LI� � CET AGENT
                await CreateDefaultAdminUserForAgentAsync(managerAgent, ecole);
            }
            catch (Exception ex)
            {
                // Log l'erreur mais ne pas faire �chouer la cr�ation de l'�cole
                Console.WriteLine($"? Erreur lors de la cr�ation du directeur/admin par d�faut: {ex.Message}");
                Console.WriteLine($"   StackTrace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// G�n�re un matricule unique pour le Manager G�n�ral (Agent)
        /// Format: [NAT][Ann�e(2)]-[GUID(6)]
        /// </summary>
        private async Task<string> GenerateMatriculeManagerGeneral(Ecole ecole)
        {
            string matricule;
            
            do
            {
                // Pr�fixe national pour tous les agents
                string annee = DateTime.Now.Year.ToString().Substring(2);
                string guid = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
                matricule = $"NAT{annee}-{guid}";
                
            } while (await _context.Agents.AnyAsync(a => a.Matricule == matricule));
            
            return matricule;
        }

        /// <summary>
        /// Cr�e un compte Utilisateur Admin li� � l'Agent Manager G�n�ral
        /// </summary>
        private async Task CreateDefaultAdminUserForAgentAsync(Agent managerAgent, Ecole ecole)
        {
            try
            {
                // R�cup�rer le r�le Admin
                var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Nom == "Admin");
                if (adminRole == null)
                {
                    // Cr�er le r�le Admin s'il n'existe pas
                    adminRole = new Role
                    {
                        Nom = "Admin",
                        DateCreation = DateTime.Now,
                        Statut = true
                    };
                    _context.Roles.Add(adminRole);
                    await _context.SaveChangesAsync();
                    
                    // ✨ NOUVEAU : S'assurer que les permissions sont assignées au rôle Admin
                    // Ceci est nécessaire car le rôle peut être créé après l'initialisation des permissions
                    await PermissionSeeder.EnsureAdminPermissionsAsync(_context);
                }
                else
                {
                    // ✨ Vérifier aussi si le rôle Admin existant a des permissions assignées
                    // (au cas où il aurait été créé avant l'initialisation des permissions)
                    var hasPermissions = await _context.RolePermissions
                        .AnyAsync(rp => rp.IdRole == adminRole.IdRole);
                    
                    if (!hasPermissions)
                    {
                        Console.WriteLine("⚠️ Le rôle Admin existe mais n'a pas de permissions. Assignation en cours...");
                        await PermissionSeeder.EnsureAdminPermissionsAsync(_context);
                    }
                }

                string emailAdmin = managerAgent.EmailAgent ?? "";
                string nomComplet = $"{managerAgent.Prenom} {managerAgent.Nom} {managerAgent.Postnom}".Trim();
                
                // ? V�rification finale de l'email (double s�curit�)
                if (!string.IsNullOrEmpty(emailAdmin))
                {
                    var emailExists = await _context.Utilisateurs.AnyAsync(u => u.Email == emailAdmin);
                    if (emailExists)
                    {
                        Console.WriteLine($"?? Email '{emailAdmin}' d�j� utilis�. Utilisateur admin non cr��.");
                        return;
                    }
                }
                
                // ? G�n�rer un username unique
                string defaultUsername = await GenerateUniqueUsernameAsync(nomComplet);
                
                // Mot de passe par d�faut
                string motDePasseParDefaut = "Admin";
                
                // Cr�er l'utilisateur Admin li� � l'agent Manager G�n�ral
                var adminUser = new Utilisateur
                {
                    IdAgent = managerAgent.IdAgent, // ? LIEN AVEC L'AGENT MANAGER G�N�RAL
                    ReferenceUtilisateur = Guid.NewGuid(),
                    NomUtilisateur = managerAgent.Nom,
                    PostNomUtilisateur = managerAgent.Postnom,
                    PrenomUtilisateur = managerAgent.Prenom,
                    Email = emailAdmin,
                    DefaultUsername = defaultUsername,
                    Telephone = managerAgent.TelephoneAgent,
                    PhotoUrl = managerAgent.PhotoUrl,
                    DateNaissance = managerAgent.DateNaissance,
                    Genre = managerAgent.Genre,
                    MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(motDePasseParDefaut),
                    Statut = true,
                    DateCreation = DateTime.Now,
                    IsConnecte = false,
                    DoitChangerMotDePasse = true, // ? Doit changer le mot de passe � la premi�re connexion
                    IdRole = adminRole.IdRole,
                    IdEcole = ecole.IdEcole,
                    Province = managerAgent.Province,
                    Ville = managerAgent.Ville,
                    Commune = managerAgent.Commune,
                    Quartier = managerAgent.Quartier,
                    Avenue = managerAgent.Avenue,
                    Numero = managerAgent.Numero
                };

                _context.Utilisateurs.Add(adminUser);
                await _context.SaveChangesAsync();
                
                // ✨ NOUVEAU : Créer le UserRole pour le système multi-rôles
                // Ceci est essentiel pour que GetUserRolesAsync, GetUserPrimaryRoleAsync et GetUserPermissionsAsync fonctionnent
                try
                {
                    var userRole = new UserRole
                    {
                        IdUtilisateur = adminUser.IdUtilisateur,
                        IdRole = adminRole.IdRole,
                        IsPrimary = true, // Rôle principal (premier rôle = principal)
                        Statut = true,
                        DateAttribution = DateTime.Now,
                        IdUtilisateurAttribution = null
                    };
                    
                    _context.UserRoles.Add(userRole);
                    await _context.SaveChangesAsync();
                    
                    Console.WriteLine($"✅ UserRole créé avec succès pour l'utilisateur Admin (ID: {adminUser.IdUtilisateur}, Rôle: {adminRole.Nom}, IsPrimary: true)");
                }
                catch (Exception roleEx)
                {
                    // Log l'erreur mais ne pas faire échouer la création de l'utilisateur
                    Console.WriteLine($"❌ ERREUR lors de la création du UserRole pour l'utilisateur Admin: {roleEx.Message}");
                    if (roleEx.InnerException != null)
                    {
                        Console.WriteLine($"   Exception interne: {roleEx.InnerException.Message}");
                    }
                    // Ne pas throw - l'utilisateur est déjà créé, on peut continuer
                }
                
                Console.WriteLine($"✅ Utilisateur Admin créé pour le Manager Général '{nomComplet}' - Email: {emailAdmin}");
                
                // Envoyer l'email de bienvenue (si email fourni)
                if (!string.IsNullOrWhiteSpace(emailAdmin))
                {
                    string nomEcole = ecole.Nom ?? "KelasiNaBiso";
                    
                    // Envoi asynchrone (ne bloque pas si �chec)
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _emailService.SendWelcomeEmailAsync(
                                emailAdmin,
                                nomComplet,
                                defaultUsername,
                                managerAgent.TelephoneAgent ?? "",
                                motDePasseParDefaut,
                                "Manager G�n�ral/Administrateur",
                                nomEcole,
                                managerAgent.Genre,
                                "Manager G�n�ral", // Fonction
                                managerAgent.Matricule // Matricule
                            );
                            
                            Console.WriteLine($"?? Email de bienvenue envoy� au Manager G�n�ral : {emailAdmin}");
                        }
                        catch (Exception emailEx)
                        {
                            Console.WriteLine($"?? �chec de l'envoi de l'email � {emailAdmin}: {emailEx.Message}");
                        }
                    });
                }
                else
                {
                    Console.WriteLine($"?? Aucun email fourni pour le Manager G�n�ral '{nomComplet}'.");
                }
            }
            catch (Exception ex)
            {
                // Log l'erreur mais ne pas faire �chouer la cr�ation de l'�cole
                Console.WriteLine($"? Erreur lors de la cr�ation de l'utilisateur Admin pour l'agent Manager G�n�ral: {ex.Message}");
            }
        }

        /// <summary>
        /// ? AM�LIORATION : G�n�re un nom d'utilisateur UNIQUE avec v�rification en boucle
        /// Format: [NomResponsable][NombreAleatoire]
        /// Exemple: "Peter Tendayo" ? "PeterTendayo123"
        /// Garantit l'unicit� en v�rifiant dans la base de donn�es
        /// </summary>
        private async Task<string> GenerateUniqueUsernameAsync(string nomComplet)
        {
            if (string.IsNullOrWhiteSpace(nomComplet))
            {
                nomComplet = "Admin";
            }
            
            // Supprimer les espaces et les caract�res sp�ciaux
            string baseUsername = nomComplet.Replace(" ", "").Replace("-", "").Replace("'", "");
            
            // Limiter � 20 caract�res pour le nom de base
            if (baseUsername.Length > 20)
            {
                baseUsername = baseUsername.Substring(0, 20);
            }
            
            string username;
            int attempts = 0;
            int maxAttempts = 100; // Limite de s�curit� pour �viter une boucle infinie
            
            do
            {
                // G�n�rer un nombre al�atoire entre 1 et 9999 (plus large pour r�duire les collisions)
                Random random = new Random(Guid.NewGuid().GetHashCode()); // Seed unique pour meilleure randomisation
                int randomNumber = random.Next(1, 10000);
                
                // Combiner le nom de base avec le nombre al�atoire
                username = $"{baseUsername}{randomNumber}";
                
                attempts++;
                
                // V�rifier l'unicit� dans la base de donn�es
                var usernameExists = await _context.Utilisateurs.AnyAsync(u => u.DefaultUsername == username);
                
                if (!usernameExists)
                {
                    Console.WriteLine($"? Username unique g�n�r�: {username} (tentative {attempts})");
                    break; // Username unique trouv� !
                }
                
                if (attempts >= maxAttempts)
                {
                    // Si on a d�pass� le nombre max de tentatives, ajouter un GUID partiel
                    string guidSuffix = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
                    username = $"{baseUsername}{guidSuffix}";
                    Console.WriteLine($"?? Max tentatives atteint. Username avec GUID g�n�r�: {username}");
                    break;
                }
                
            } while (true);
            
            return username;
        }

        /// <summary>
        /// [DEPRECATED] Ancienne m�thode sans v�rification d'unicit� - conserv�e pour r�f�rence
        /// Utilisez GenerateUniqueUsernameAsync() � la place
        /// </summary>
        [Obsolete("Utilisez GenerateUniqueUsernameAsync() pour garantir l'unicit�")]
        private string GenerateUsernameFromName(string nomComplet)
        {
            if (string.IsNullOrWhiteSpace(nomComplet))
            {
                nomComplet = "Admin";
            }
            
            // Supprimer les espaces et les caract�res sp�ciaux
            string baseUsername = nomComplet.Replace(" ", "").Replace("-", "").Replace("'", "");
            
            // Limiter � 20 caract�res pour le nom de base
            if (baseUsername.Length > 20)
            {
                baseUsername = baseUsername.Substring(0, 20);
            }
            
            // G�n�rer un nombre al�atoire entre 1 et 999
            Random random = new Random();
            int randomNumber = random.Next(1, 1000);
            
            // Combiner le nom de base avec le nombre al�atoire
            string username = $"{baseUsername}{randomNumber}";
            
            return username;
        }

        // ? SOFT DELETE: Toggle le statut d'une �cole (actif <-> inactif)
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var ecole = await _context.Ecoles.FindAsync(id);
            if (ecole == null)
                return false;

            ecole.Statut = ecole.Statut != true;
            await _context.SaveChangesAsync();
            return true;
        }
        
        // ? SOFT DELETE: D�finir une valeur sp�cifique pour le statut d'une �cole
        public async Task<bool> SetStatutAsync(int id, bool statut)
        {
            var ecole = await _context.Ecoles.FindAsync(id);
            if (ecole == null)
                return false;

            ecole.Statut = statut;
            await _context.SaveChangesAsync();
            return true;
        }
        
        // ? GESTION ACCEPT NOTIFICATION: Toggle la valeur AcceptNotification (true <-> false)
        public async Task<bool> ToggleAcceptNotificationAsync(int id)
        {
            var ecole = await _context.Ecoles.FindAsync(id);
            if (ecole == null)
                return false;

            ecole.AcceptNotification = !ecole.AcceptNotification;
            await _context.SaveChangesAsync();
            return true;
        }
        
        // ? GESTION ACCEPT NOTIFICATION: D�finir une valeur sp�cifique pour AcceptNotification
        public async Task<bool> SetAcceptNotificationAsync(int id, bool acceptNotification)
        {
            var ecole = await _context.Ecoles.FindAsync(id);
            if (ecole == null)
                return false;

            ecole.AcceptNotification = acceptNotification;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<EcolePartenaireDto>> GetPartenairesLogosAsync(int limit = 50)
        {
            var take = Math.Clamp(limit, 1, 100);

            var rows = await _context.Ecoles
                .AsNoTracking()
                .Where(e => e.Statut == true && e.Logo != null && e.Logo.Trim() != string.Empty)
                .OrderBy(e => e.Nom)
                .Take(take)
                .Select(e => new { e.IdEcole, e.Nom, e.Logo, e.Type, e.Ville })
                .ToListAsync();

            return rows.Select(e => new EcolePartenaireDto
            {
                IdEcole = e.IdEcole,
                Nom = e.Nom ?? string.Empty,
                LogoBase64 = ImageSourceHelper.ToRawBase64(e.Logo),
                LogoSrc = ImageSourceHelper.ToImageSrc(e.Logo),
                Type = e.Type,
                Ville = e.Ville
            }).ToList();
        }
    }
}


