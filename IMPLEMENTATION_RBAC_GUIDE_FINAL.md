# 🚀 GUIDE D'IMPLÉMENTATION FINALE - RBAC AVEC PERMISSIONS

**Date** : 27 octobre 2025  
**Statut** : 70% complété - Étapes finales à effectuer

---

## ✅ CE QUI A ÉTÉ CRÉÉ

### Phase 1 : Modèles de données ✅ COMPLÉTÉE

- ✅ `Models/Permission.cs` - Modèle Permission avec Nom, Catégorie, Action, Description
- ✅ `Models/RolePermission.cs` - Table de liaison N-N entre Roles et Permissions
- ✅ `Models/Role.cs` - Mis à jour avec Niveau hiérarchique et relation RolePermissions
- ✅ `Models/Enums/UserRoles.cs` - Énumération de tous les rôles avec méthodes utilitaires

### Phase 2 : Services et logique métier ✅ COMPLÉTÉE

- ✅ `Services/Repositories/IPermissionService.cs` - Interface du service de permissions
- ✅ `Services/PermissionService.cs` - Implémentation complète (400+ lignes)
- ✅ `Services/Repositories/ICurrentUserService.cs` - Interface d'accès aux claims utilisateur
- ✅ `Services/CurrentUserService.cs` - Implémentation d'accès aux claims JWT
- ✅ `Attributes/PermissionAttribute.cs` - Attribut personnalisé `[Permission("...")]`

### Phase 3 : Base de données ✅ PARTIELLEMENT COMPLÉTÉE

- ✅ `Data/KelasiNaBisoDbContext.cs` - Ajout de `DbSet<Permission>` et `DbSet<RolePermission>`
- ⏳ Migration EF Core - **À CRÉER**
- ⏳ Script d'initialisation permissions - **À CRÉER**

### Phase 4 : Configuration ⏳ EN ATTENTE

- ⏳ Enregistrement services dans `Program.cs` - **À FAIRE**
- ⏳ `PermissionController.cs` - **À CRÉER**
- ⏳ Tests de validation - **À FAIRE**

---

## 🔨 ÉTAPES FINALES À EFFECTUER

### ⏳ Étape 8 : Créer la migration EF Core

**Commande à exécuter** :

```powershell
# Dans le terminal PowerShell de Visual Studio
dotnet ef migrations add AddRBACPermissionsSystem --context KelasiNaBisoDbContext
```

Cette commande va créer une migration qui :
- Ajoutera la table `Permissions`
- Ajoutera la table `RolePermissions`
- Mettra à jour la table `Roles` avec les nouvelles colonnes (`Description`, `Niveau`)

**Appliquer la migration** :

```powershell
dotnet ef database update --context KelasiNaBisoDbContext
```

---

### ⏳ Étape 9 : Créer le script d'initialisation des permissions

Créer le fichier `Data/PermissionSeeder.cs` :

```csharp
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Data
{
    /// <summary>
    /// Initialise les permissions par défaut du système
    /// </summary>
    public static class PermissionSeeder
    {
        /// <summary>
        /// Crée toutes les permissions par défaut et les assigne aux rôles appropriés
        /// </summary>
        public static async Task SeedPermissionsAsync(KelasiNaBisoDbContext context)
        {
            // Vérifier si des permissions existent déjà
            if (await context.Permissions.AnyAsync())
            {
                Console.WriteLine("✅ Permissions déjà initialisées");
                return;
            }

            Console.WriteLine("🔨 Initialisation des permissions par défaut...");

            var permissions = GetDefaultPermissions();

            // Ajouter toutes les permissions
            await context.Permissions.AddRangeAsync(permissions);
            await context.SaveChangesAsync();

            Console.WriteLine($"✅ {permissions.Count} permissions créées");

            // Assigner les permissions aux rôles
            await AssignPermissionsToRolesAsync(context);

            Console.WriteLine("✅ Permissions assignées aux rôles avec succès");
        }

        /// <summary>
        /// Retourne la liste de toutes les permissions par défaut
        /// </summary>
        private static List<Permission> GetDefaultPermissions()
        {
            return new List<Permission>
            {
                // ═══════════════════════════════════════════════════════════════════
                // ÉCOLE
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "Ecole.Create", Categorie = "Ecole", Action = "Create", Description = "Créer une école" },
                new Permission { Nom = "Ecole.Read", Categorie = "Ecole", Action = "Read", Description = "Voir les informations d'une école" },
                new Permission { Nom = "Ecole.ReadAll", Categorie = "Ecole", Action = "ReadAll", Description = "Voir toutes les écoles" },
                new Permission { Nom = "Ecole.Update", Categorie = "Ecole", Action = "Update", Description = "Modifier une école" },
                new Permission { Nom = "Ecole.Delete", Categorie = "Ecole", Action = "Delete", Description = "Supprimer une école" },

                // ═══════════════════════════════════════════════════════════════════
                // UTILISATEUR
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "Utilisateur.Create", Categorie = "Utilisateur", Action = "Create", Description = "Créer un utilisateur" },
                new Permission { Nom = "Utilisateur.Read", Categorie = "Utilisateur", Action = "Read", Description = "Voir un utilisateur" },
                new Permission { Nom = "Utilisateur.ReadAll", Categorie = "Utilisateur", Action = "ReadAll", Description = "Voir tous les utilisateurs" },
                new Permission { Nom = "Utilisateur.Update", Categorie = "Utilisateur", Action = "Update", Description = "Modifier un utilisateur" },
                new Permission { Nom = "Utilisateur.Delete", Categorie = "Utilisateur", Action = "Delete", Description = "Supprimer un utilisateur" },
                new Permission { Nom = "Utilisateur.ChangePassword", Categorie = "Utilisateur", Action = "ChangePassword", Description = "Changer le mot de passe d'un utilisateur" },

                // ═══════════════════════════════════════════════════════════════════
                // ÉLÈVE
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "Eleve.Create", Categorie = "Eleve", Action = "Create", Description = "Créer un élève" },
                new Permission { Nom = "Eleve.Read", Categorie = "Eleve", Action = "Read", Description = "Voir un élève" },
                new Permission { Nom = "Eleve.ReadAll", Categorie = "Eleve", Action = "ReadAll", Description = "Voir tous les élèves" },
                new Permission { Nom = "Eleve.ReadOwn", Categorie = "Eleve", Action = "ReadOwn", Description = "Voir ses propres informations (élève)" },
                new Permission { Nom = "Eleve.ReadChildren", Categorie = "Eleve", Action = "ReadChildren", Description = "Voir ses enfants (parent)" },
                new Permission { Nom = "Eleve.Update", Categorie = "Eleve", Action = "Update", Description = "Modifier un élève" },
                new Permission { Nom = "Eleve.Delete", Categorie = "Eleve", Action = "Delete", Description = "Supprimer un élève" },

                // ═══════════════════════════════════════════════════════════════════
                // AGENT
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "Agent.Create", Categorie = "Agent", Action = "Create", Description = "Créer un agent" },
                new Permission { Nom = "Agent.Read", Categorie = "Agent", Action = "Read", Description = "Voir un agent" },
                new Permission { Nom = "Agent.ReadAll", Categorie = "Agent", Action = "ReadAll", Description = "Voir tous les agents" },
                new Permission { Nom = "Agent.Update", Categorie = "Agent", Action = "Update", Description = "Modifier un agent" },
                new Permission { Nom = "Agent.Delete", Categorie = "Agent", Action = "Delete", Description = "Supprimer un agent" },

                // ═══════════════════════════════════════════════════════════════════
                // PAIEMENT
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "Paiement.Create", Categorie = "Paiement", Action = "Create", Description = "Créer un paiement" },
                new Permission { Nom = "Paiement.Read", Categorie = "Paiement", Action = "Read", Description = "Voir un paiement" },
                new Permission { Nom = "Paiement.ReadAll", Categorie = "Paiement", Action = "ReadAll", Description = "Voir tous les paiements" },
                new Permission { Nom = "Paiement.ReadOwn", Categorie = "Paiement", Action = "ReadOwn", Description = "Voir ses propres paiements (parent)" },
                new Permission { Nom = "Paiement.Update", Categorie = "Paiement", Action = "Update", Description = "Modifier un paiement" },
                new Permission { Nom = "Paiement.Delete", Categorie = "Paiement", Action = "Delete", Description = "Supprimer un paiement" },
                new Permission { Nom = "Paiement.Validate", Categorie = "Paiement", Action = "Validate", Description = "Valider un paiement" },

                // ═══════════════════════════════════════════════════════════════════
                // NOTE
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "Note.Create", Categorie = "Note", Action = "Create", Description = "Créer une note" },
                new Permission { Nom = "Note.Read", Categorie = "Note", Action = "Read", Description = "Voir une note" },
                new Permission { Nom = "Note.ReadAll", Categorie = "Note", Action = "ReadAll", Description = "Voir toutes les notes" },
                new Permission { Nom = "Note.ReadOwn", Categorie = "Note", Action = "ReadOwn", Description = "Voir ses propres notes (élève)" },
                new Permission { Nom = "Note.ReadChildren", Categorie = "Note", Action = "ReadChildren", Description = "Voir les notes de ses enfants (parent)" },
                new Permission { Nom = "Note.Update", Categorie = "Note", Action = "Update", Description = "Modifier une note" },
                new Permission { Nom = "Note.Delete", Categorie = "Note", Action = "Delete", Description = "Supprimer une note" },

                // ═══════════════════════════════════════════════════════════════════
                // TUTEUR
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "Tuteur.Create", Categorie = "Tuteur", Action = "Create", Description = "Créer un tuteur" },
                new Permission { Nom = "Tuteur.Read", Categorie = "Tuteur", Action = "Read", Description = "Voir un tuteur" },
                new Permission { Nom = "Tuteur.ReadAll", Categorie = "Tuteur", Action = "ReadAll", Description = "Voir tous les tuteurs" },
                new Permission { Nom = "Tuteur.Update", Categorie = "Tuteur", Action = "Update", Description = "Modifier un tuteur" },
                new Permission { Nom = "Tuteur.Delete", Categorie = "Tuteur", Action = "Delete", Description = "Supprimer un tuteur" },

                // ═══════════════════════════════════════════════════════════════════
                // RÔLE
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "Role.Create", Categorie = "Role", Action = "Create", Description = "Créer un rôle" },
                new Permission { Nom = "Role.Read", Categorie = "Role", Action = "Read", Description = "Voir un rôle" },
                new Permission { Nom = "Role.ReadAll", Categorie = "Role", Action = "ReadAll", Description = "Voir tous les rôles" },
                new Permission { Nom = "Role.Update", Categorie = "Role", Action = "Update", Description = "Modifier un rôle" },
                new Permission { Nom = "Role.Delete", Categorie = "Role", Action = "Delete", Description = "Supprimer un rôle" },

                // ═══════════════════════════════════════════════════════════════════
                // PERMISSION
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "Permission.Create", Categorie = "Permission", Action = "Create", Description = "Créer une permission" },
                new Permission { Nom = "Permission.Read", Categorie = "Permission", Action = "Read", Description = "Voir une permission" },
                new Permission { Nom = "Permission.ReadAll", Categorie = "Permission", Action = "ReadAll", Description = "Voir toutes les permissions" },
                new Permission { Nom = "Permission.Update", Categorie = "Permission", Action = "Update", Description = "Modifier une permission" },
                new Permission { Nom = "Permission.Delete", Categorie = "Permission", Action = "Delete", Description = "Supprimer une permission" },
                new Permission { Nom = "Permission.Assign", Categorie = "Permission", Action = "Assign", Description = "Assigner une permission à un rôle" },
                new Permission { Nom = "Permission.Revoke", Categorie = "Permission", Action = "Revoke", Description = "Retirer une permission d'un rôle" },

                // ═══════════════════════════════════════════════════════════════════
                // CLASSE
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "Classe.Create", Categorie = "Classe", Action = "Create", Description = "Créer une classe" },
                new Permission { Nom = "Classe.Read", Categorie = "Classe", Action = "Read", Description = "Voir une classe" },
                new Permission { Nom = "Classe.ReadAll", Categorie = "Classe", Action = "ReadAll", Description = "Voir toutes les classes" },
                new Permission { Nom = "Classe.Update", Categorie = "Classe", Action = "Update", Description = "Modifier une classe" },
                new Permission { Nom = "Classe.Delete", Categorie = "Classe", Action = "Delete", Description = "Supprimer une classe" },

                // ═══════════════════════════════════════════════════════════════════
                // FRAIS
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "Frais.Create", Categorie = "Frais", Action = "Create", Description = "Créer un frais" },
                new Permission { Nom = "Frais.Read", Categorie = "Frais", Action = "Read", Description = "Voir un frais" },
                new Permission { Nom = "Frais.ReadAll", Categorie = "Frais", Action = "ReadAll", Description = "Voir tous les frais" },
                new Permission { Nom = "Frais.Update", Categorie = "Frais", Action = "Update", Description = "Modifier un frais" },
                new Permission { Nom = "Frais.Delete", Categorie = "Frais", Action = "Delete", Description = "Supprimer un frais" },

                // ═══════════════════════════════════════════════════════════════════
                // INSCRIPTION
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "Inscription.Create", Categorie = "Inscription", Action = "Create", Description = "Créer une inscription" },
                new Permission { Nom = "Inscription.Read", Categorie = "Inscription", Action = "Read", Description = "Voir une inscription" },
                new Permission { Nom = "Inscription.ReadAll", Categorie = "Inscription", Action = "ReadAll", Description = "Voir toutes les inscriptions" },
                new Permission { Nom = "Inscription.Update", Categorie = "Inscription", Action = "Update", Description = "Modifier une inscription" },
                new Permission { Nom = "Inscription.Delete", Categorie = "Inscription", Action = "Delete", Description = "Supprimer une inscription" },

                // ═══════════════════════════════════════════════════════════════════
                // PRÉSENCE
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "Presence.Create", Categorie = "Presence", Action = "Create", Description = "Créer une présence" },
                new Permission { Nom = "Presence.Read", Categorie = "Presence", Action = "Read", Description = "Voir une présence" },
                new Permission { Nom = "Presence.ReadAll", Categorie = "Presence", Action = "ReadAll", Description = "Voir toutes les présences" },
                new Permission { Nom = "Presence.Update", Categorie = "Presence", Action = "Update", Description = "Modifier une présence" },
                new Permission { Nom = "Presence.Delete", Categorie = "Presence", Action = "Delete", Description = "Supprimer une présence" },

                // ═══════════════════════════════════════════════════════════════════
                // COURS
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "Cours.Create", Categorie = "Cours", Action = "Create", Description = "Créer un cours" },
                new Permission { Nom = "Cours.Read", Categorie = "Cours", Action = "Read", Description = "Voir un cours" },
                new Permission { Nom = "Cours.ReadAll", Categorie = "Cours", Action = "ReadAll", Description = "Voir tous les cours" },
                new Permission { Nom = "Cours.Update", Categorie = "Cours", Action = "Update", Description = "Modifier un cours" },
                new Permission { Nom = "Cours.Delete", Categorie = "Cours", Action = "Delete", Description = "Supprimer un cours" },

                // ... Ajouter d'autres catégories si nécessaire (Document, Message, etc.)
            };
        }

        /// <summary>
        /// Assigne les permissions aux rôles appropriés
        /// </summary>
        private static async Task AssignPermissionsToRolesAsync(KelasiNaBisoDbContext context)
        {
            // Récupérer les rôles
            var superAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == UserRoles.SUPER_ADMIN);
            var directeurRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == UserRoles.DIRECTEUR);
            var comptableRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == UserRoles.COMPTABLE);
            var enseignantRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == UserRoles.ENSEIGNANT);
            var secretaireRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == UserRoles.SECRETAIRE);
            var parentRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == UserRoles.PARENT);
            var eleveRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == UserRoles.ELEVE);

            // Si les rôles n'existent pas encore, les créer
            if (superAdminRole == null)
            {
                Console.WriteLine("⚠️ Rôles non trouvés. Assurez-vous que les rôles ont été créés d'abord.");
                return;
            }

            // Récupérer toutes les permissions
            var allPermissions = await context.Permissions.ToListAsync();

            // ═══════════════════════════════════════════════════════════════════
            // SUPER-ADMIN : TOUTES LES PERMISSIONS
            // ═══════════════════════════════════════════════════════════════════
            if (superAdminRole != null)
            {
                foreach (var permission in allPermissions)
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        IdRole = superAdminRole.IdRole,
                        IdPermission = permission.IdPermission
                    });
                }
            }

            // ═══════════════════════════════════════════════════════════════════
            // DIRECTEUR : Gestion complète de son école
            // ═══════════════════════════════════════════════════════════════════
            if (directeurRole != null)
            {
                var directeurPermissions = allPermissions.Where(p =>
                    p.Categorie == "Ecole" && p.Action != "Create" && p.Action != "Delete" ||
                    p.Categorie == "Utilisateur" ||
                    p.Categorie == "Eleve" ||
                    p.Categorie == "Agent" ||
                    p.Categorie == "Paiement" ||
                    p.Categorie == "Note" ||
                    p.Categorie == "Classe" ||
                    p.Categorie == "Frais" ||
                    p.Categorie == "Inscription" ||
                    p.Categorie == "Presence" ||
                    p.Categorie == "Cours"
                ).ToList();

                foreach (var permission in directeurPermissions)
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        IdRole = directeurRole.IdRole,
                        IdPermission = permission.IdPermission
                    });
                }
            }

            // ═══════════════════════════════════════════════════════════════════
            // COMPTABLE : Gestion financière
            // ═══════════════════════════════════════════════════════════════════
            if (comptableRole != null)
            {
                var comptablePermissions = allPermissions.Where(p =>
                    p.Categorie == "Paiement" ||
                    p.Categorie == "Frais" ||
                    (p.Categorie == "Eleve" && (p.Action == "Read" || p.Action == "ReadAll"))
                ).ToList();

                foreach (var permission in comptablePermissions)
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        IdRole = comptableRole.IdRole,
                        IdPermission = permission.IdPermission
                    });
                }
            }

            // ═══════════════════════════════════════════════════════════════════
            // SECRÉTAIRE : Gestion élèves et inscriptions
            // ═══════════════════════════════════════════════════════════════════
            if (secretaireRole != null)
            {
                var secretairePermissions = allPermissions.Where(p =>
                    p.Categorie == "Eleve" ||
                    p.Categorie == "Tuteur" ||
                    p.Categorie == "Inscription" ||
                    (p.Categorie == "Classe" && (p.Action == "Read" || p.Action == "ReadAll")) ||
                    (p.Categorie == "Paiement" && p.Action == "Create")
                ).ToList();

                foreach (var permission in secretairePermissions)
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        IdRole = secretaireRole.IdRole,
                        IdPermission = permission.IdPermission
                    });
                }
            }

            // ═══════════════════════════════════════════════════════════════════
            // ENSEIGNANT : Notes, présences, cours
            // ═══════════════════════════════════════════════════════════════════
            if (enseignantRole != null)
            {
                var enseignantPermissions = allPermissions.Where(p =>
                    p.Categorie == "Note" && p.Action != "Delete" ||
                    p.Categorie == "Presence" ||
                    (p.Categorie == "Eleve" && (p.Action == "Read" || p.Action == "ReadAll")) ||
                    (p.Categorie == "Cours" && (p.Action == "Read" || p.Action == "ReadAll"))
                ).ToList();

                foreach (var permission in enseignantPermissions)
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        IdRole = enseignantRole.IdRole,
                        IdPermission = permission.IdPermission
                    });
                }
            }

            // ═══════════════════════════════════════════════════════════════════
            // PARENT : Consultation données de ses enfants
            // ═══════════════════════════════════════════════════════════════════
            if (parentRole != null)
            {
                var parentPermissions = allPermissions.Where(p =>
                    p.Action == "ReadOwn" || p.Action == "ReadChildren"
                ).ToList();

                foreach (var permission in parentPermissions)
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        IdRole = parentRole.IdRole,
                        IdPermission = permission.IdPermission
                    });
                }
            }

            // ═══════════════════════════════════════════════════════════════════
            // ÉLÈVE : Consultation de ses propres données
            // ═══════════════════════════════════════════════════════════════════
            if (eleveRole != null)
            {
                var elevePermissions = allPermissions.Where(p =>
                    p.Action == "ReadOwn"
                ).ToList();

                foreach (var permission in elevePermissions)
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        IdRole = eleveRole.IdRole,
                        IdPermission = permission.IdPermission
                    });
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
```

**Appeler cette méthode dans `Program.cs`** après les migrations :

```csharp
// Après context.Database.Migrate();
await PermissionSeeder.SeedPermissionsAsync(context);
```

---

### ⏳ Étape 10 : Enregistrer les services dans Program.cs

Ajouter dans `Program.cs` (section services) :

```csharp
// ✨ NOUVEAU : Services RBAC avec permissions
builder.Services.AddHttpContextAccessor(); // Nécessaire pour ICurrentUserService
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
```

---

### ⏳ Étape 11 : Créer PermissionController

Créer le fichier `Controllers/PermissionController.cs` :

```csharp
using KelasiNaBiso.Attributes;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KelasiNaBiso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Tous les endpoints nécessitent authentification
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        // ═══════════════════════════════════════════════════════════════════
        // CONSULTATION DES PERMISSIONS
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Récupère toutes les permissions du système
        /// </summary>
        [HttpGet]
        [Permission("Permission.ReadAll")]
        public async Task<ActionResult<IEnumerable<Permission>>> GetAllPermissions()
        {
            var permissions = await _permissionService.GetAllPermissionsAsync();
            return Ok(permissions);
        }

        /// <summary>
        /// Récupère une permission par son ID
        /// </summary>
        [HttpGet("{id}")]
        [Permission("Permission.Read")]
        public async Task<ActionResult<Permission>> GetPermission(int id)
        {
            var permission = await _permissionService.GetPermissionByIdAsync(id);
            if (permission == null)
            {
                return NotFound();
            }
            return Ok(permission);
        }

        /// <summary>
        /// Récupère les permissions d'une catégorie
        /// </summary>
        [HttpGet("category/{category}")]
        [Permission("Permission.ReadAll")]
        public async Task<ActionResult<IEnumerable<Permission>>> GetPermissionsByCategory(string category)
        {
            var permissions = await _permissionService.GetPermissionsByCategoryAsync(category);
            return Ok(permissions);
        }

        /// <summary>
        /// Récupère toutes les permissions d'un utilisateur
        /// </summary>
        [HttpGet("user/{userId}")]
        [Permission("Permission.ReadAll")]
        public async Task<ActionResult<IEnumerable<string>>> GetUserPermissions(int userId)
        {
            var permissions = await _permissionService.GetUserPermissionsAsync(userId);
            return Ok(permissions);
        }

        /// <summary>
        /// Récupère les permissions d'un rôle
        /// </summary>
        [HttpGet("role/{roleId}")]
        [Permission("Permission.ReadAll")]
        public async Task<ActionResult<IEnumerable<Permission>>> GetRolePermissions(int roleId)
        {
            var permissions = await _permissionService.GetRolePermissionsAsync(roleId);
            return Ok(permissions);
        }

        // ═══════════════════════════════════════════════════════════════════
        // GESTION DES PERMISSIONS (SUPER-ADMIN UNIQUEMENT)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Crée une nouvelle permission
        /// </summary>
        [HttpPost]
        [Permission("Permission.Create")]
        public async Task<ActionResult<Permission>> CreatePermission([FromBody] Permission permission)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdPermission = await _permissionService.CreatePermissionAsync(permission);
                return CreatedAtAction(nameof(GetPermission), new { id = createdPermission.IdPermission }, createdPermission);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Met à jour une permission
        /// </summary>
        [HttpPut("{id}")]
        [Permission("Permission.Update")]
        public async Task<IActionResult> UpdatePermission(int id, [FromBody] Permission permission)
        {
            if (id != permission.IdPermission)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedPermission = await _permissionService.UpdatePermissionAsync(permission);
            if (updatedPermission == null)
            {
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Supprime une permission
        /// </summary>
        [HttpDelete("{id}")]
        [Permission("Permission.Delete")]
        public async Task<IActionResult> DeletePermission(int id)
        {
            try
            {
                var success = await _permissionService.DeletePermissionAsync(id);
                if (!success)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // ASSIGNATION DES PERMISSIONS AUX RÔLES
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Assigne une permission à un rôle
        /// </summary>
        [HttpPost("assign")]
        [Permission("Permission.Assign")]
        public async Task<IActionResult> AssignPermissionToRole([FromBody] AssignPermissionRequest request)
        {
            var success = await _permissionService.AssignPermissionToRoleAsync(
                request.RoleId,
                request.PermissionId,
                request.UserId
            );

            if (!success)
            {
                return BadRequest(new { message = "Échec de l'assignation de la permission" });
            }

            return Ok(new { message = "Permission assignée avec succès" });
        }

        /// <summary>
        /// Retire une permission d'un rôle
        /// </summary>
        [HttpPost("revoke")]
        [Permission("Permission.Revoke")]
        public async Task<IActionResult> RevokePermissionFromRole([FromBody] RevokePermissionRequest request)
        {
            var success = await _permissionService.RevokePermissionFromRoleAsync(
                request.RoleId,
                request.PermissionId
            );

            if (!success)
            {
                return BadRequest(new { message = "Échec du retrait de la permission" });
            }

            return Ok(new { message = "Permission retirée avec succès" });
        }

        /// <summary>
        /// Assigne plusieurs permissions à un rôle
        /// </summary>
        [HttpPost("assign-multiple")]
        [Permission("Permission.Assign")]
        public async Task<IActionResult> AssignMultiplePermissions([FromBody] AssignMultiplePermissionsRequest request)
        {
            var count = await _permissionService.AssignMultiplePermissionsToRoleAsync(
                request.RoleId,
                request.PermissionIds,
                request.UserId
            );

            return Ok(new { message = $"{count} permission(s) assignée(s) avec succès" });
        }

        /// <summary>
        /// Remplace toutes les permissions d'un rôle
        /// </summary>
        [HttpPut("role/{roleId}/permissions")]
        [Permission("Permission.Assign")]
        public async Task<IActionResult> ReplaceRolePermissions(int roleId, [FromBody] ReplacePermissionsRequest request)
        {
            var success = await _permissionService.ReplaceRolePermissionsAsync(
                roleId,
                request.PermissionIds,
                request.UserId
            );

            if (!success)
            {
                return BadRequest(new { message = "Échec du remplacement des permissions" });
            }

            return Ok(new { message = "Permissions remplacées avec succès" });
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // DTOs pour les requêtes
    // ═══════════════════════════════════════════════════════════════════

    public class AssignPermissionRequest
    {
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
        public int? UserId { get; set; }
    }

    public class RevokePermissionRequest
    {
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
    }

    public class AssignMultiplePermissionsRequest
    {
        public int RoleId { get; set; }
        public List<int> PermissionIds { get; set; } = new();
        public int? UserId { get; set; }
    }

    public class ReplacePermissionsRequest
    {
        public List<int> PermissionIds { get; set; } = new();
        public int? UserId { get; set; }
    }
}
```

---

### ⏳ Étape 12 : Utiliser les permissions dans vos contrôleurs

**Exemple : Sécuriser EcoleController**

```csharp
using KelasiNaBiso.Attributes; // Ajouter ce using

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EcoleController : ControllerBase
{
    // ✅ AVANT : [HttpPost]
    // ✅ APRÈS :
    [HttpPost]
    [Permission("Ecole.Create")] // 🔒 Seuls les utilisateurs avec cette permission peuvent créer
    public async Task<ActionResult<object>> CreateEcole(Ecole ecole)
    {
        // ...
    }

    // ✅ AVANT : [HttpDelete("{id}")]
    // ✅ APRÈS :
    [HttpDelete("{id}")]
    [Permission("Ecole.Delete")] // 🔒 Permission spécifique pour supprimer
    public async Task<IActionResult> DeleteEcole(int id)
    {
        // ...
    }

    [HttpGet]
    [Permission("Ecole.ReadAll")]
    public async Task<ActionResult<IEnumerable<Ecole>>> GetEcoles()
    {
        // ...
    }
}
```

---

## 🧪 TESTS À EFFECTUER

### Test 1 : Vérifier la migration

```powershell
dotnet ef migrations list
# Vous devriez voir : AddRBACPermissionsSystem

dotnet ef database update
# La migration doit s'appliquer sans erreur
```

### Test 2 : Vérifier l'initialisation des permissions

Démarrer l'application et vérifier les logs :
- ✅ "🔨 Initialisation des permissions par défaut..."
- ✅ "✅ X permissions créées"
- ✅ "✅ Permissions assignées aux rôles avec succès"

### Test 3 : Tester un endpoint protégé

```http
### Tester avec Postman ou Thunder Client

# Se connecter
POST http://localhost:5000/api/Utilisateur/authentifier
Content-Type: application/json

{
  "emailOuTelephone": "superadmin@kelasinabiso.cd",
  "motDePasse": "Super-Admin"
}

### Copier le token JWT retourné

# Essayer de créer une école (devrait marcher avec Super-Admin)
POST http://localhost:5000/api/Ecole
Authorization: Bearer <TOKEN_JWT>
Content-Type: application/json

{
  "nom": "Test École",
  ...
}

### Résultat attendu : ✅ 201 Created


# Essayer avec un compte Parent (devrait être refusé)
POST http://localhost:5000/api/Utilisateur/authentifier
Content-Type: application/json

{
  "emailOuTelephone": "parent@example.com",
  "motDePasse": "password"
}

### Copier le token

POST http://localhost:5000/api/Ecole
Authorization: Bearer <TOKEN_PARENT>
Content-Type: application/json

{
  "nom": "Test École",
  ...
}

### Résultat attendu : ❌ 403 Forbidden
```

---

## 📝 CHECKLIST FINALE

- [ ] Étape 8 : Migration EF Core créée et appliquée
- [ ] Étape 9 : `PermissionSeeder.cs` créé et appelé dans `Program.cs`
- [ ] Étape 10 : Services enregistrés dans `Program.cs`
- [ ] Étape 11 : `PermissionController.cs` créé
- [ ] Étape 12 : Attributs `[Permission]` ajoutés sur les endpoints critiques
- [ ] Tests : Migration appliquée correctement
- [ ] Tests : Permissions initialisées dans la base de données
- [ ] Tests : Endpoints protégés fonctionnent correctement
- [ ] Tests : Accès refusé pour utilisateurs sans permission

---

## 🎯 PROCHAINES ÉTAPES RECOMMANDÉES

1. **Court terme** (après implémentation)
   - Remplacer progressivement `[Authorize]` par `[Permission("...")]` sur tous les endpoints
   - Créer une interface web d'administration des permissions (optionnel)
   - Ajouter des tests unitaires pour `PermissionService`

2. **Moyen terme**
   - Implémenter un système d'audit trail (qui a fait quoi, quand)
   - Créer des rapports de permissions par rôle
   - Ajouter des permissions dynamiques basées sur des règles métier

3. **Long terme**
   - Système de permissions temporaires (accès limité dans le temps)
   - Permissions basées sur les attributs (ABAC - Attribute-Based Access Control)
   - Interface de gestion des permissions en temps réel

---

## 📞 BESOIN D'AIDE ?

Si vous rencontrez des erreurs :

1. **Erreur de migration** : Vérifiez que tous les modèles sont corrects
2. **Erreur de compilation** : Vérifiez les usings manquants
3. **403 Forbidden sur tous les endpoints** : Vérifiez que les services sont enregistrés dans `Program.cs`
4. **Permissions non initialisées** : Vérifiez que `PermissionSeeder.SeedPermissionsAsync()` est appelé

---

## ✅ RÉSUMÉ

Vous avez maintenant un système RBAC complet avec :

- ✅ **Permission** - Modèle de permission avec catégorie et action
- ✅ **RolePermission** - Table de liaison N-N
- ✅ **PermissionService** - Logique métier complète
- ✅ **CurrentUserService** - Accès facile aux claims utilisateur
- ✅ **[Permission]** attribute - Autorisation personnalisée
- ⏳ **Migration** - À créer et appliquer
- ⏳ **PermissionSeeder** - À créer et exécuter
- ⏳ **PermissionController** - À créer

**Il ne reste plus que 4 fichiers à créer et vous aurez un système complet !** 🚀

