using KelasiNaBiso.Models;
using KelasiNaBiso.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Data
{
    /// <summary>
    /// Initialise les permissions par défaut du système RBAC
    /// </summary>
    public static class PermissionSeeder
    {
        /// <summary>
        /// Crée toutes les permissions par défaut et les assigne aux rôles appropriés
        /// </summary>
        public static async Task SeedPermissionsAsync(KelasiNaBisoDbContext context)
        {
            // Toujours s'assurer que les rôles catalogue existent (y compris IT-Support)
            await CreateMissingRolesAsync(context);

            // Vérifier si des permissions existent déjà
            if (await context.Permissions.AnyAsync())
            {
                Console.WriteLine("✅ Permissions déjà initialisées");
                // Même si les permissions existent : s'assurer qu'IT-Support a bien les siennes
                await EnsureItSupportPermissionsAsync(context);
                return;
            }

            Console.WriteLine("🔨 Initialisation des permissions par défaut...");

            var permissions = GetDefaultPermissions();

            // 2. Ajouter toutes les permissions
            await context.Permissions.AddRangeAsync(permissions);
            await context.SaveChangesAsync();

            Console.WriteLine($"✅ {permissions.Count} permissions créées");

            // 3. Assigner les permissions aux rôles
            await AssignPermissionsToRolesAsync(context);

            Console.WriteLine("✅ Permissions assignées aux rôles avec succès");
        }

        /// <summary>
        /// Crée tous les rôles manquants dans le système
        /// </summary>
        private static async Task CreateMissingRolesAsync(KelasiNaBisoDbContext context)
        {
            var rolesToEnsure = new (string Nom, string Description, int Niveau)[]
            {
                ("Super-Admin", "Administrateur plateforme", 1),
                ("Admin", "Administrateur d'école", 3),
                ("Directeur", "Directeur d'école", 2),
                ("Financier", "Gestion financière", 3),
                ("Enseignant", "Enseignant", 4),
                ("Parent", "Parent / tuteur", 5),
                ("Eleve", "Élève", 6),
                (UserRoles.IT_SUPPORT, "Impression cartes scolaires et attribution SerialNumber", 4),
            };

            var existingRoles = await context.Roles.Select(r => r.Nom).ToListAsync();

            foreach (var (nom, description, niveau) in rolesToEnsure)
            {
                if (!existingRoles.Contains(nom))
                {
                    context.Roles.Add(new Role
                    {
                        Nom = nom,
                        Description = description,
                        Niveau = niveau,
                        DateCreation = DateTime.UtcNow,
                        Statut = true
                    });
                    Console.WriteLine($"✅ Rôle '{nom}' créé");
                }
                else
                {
                    Console.WriteLine($"✓ Rôle '{nom}' existe déjà");
                }
            }

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Retourne la liste de toutes les permissions par défaut (80+ permissions)
        /// </summary>
        private static List<Permission> GetDefaultPermissions()
        {
            return new List<Permission>
            {
                // ═══════════════════════════════════════════════════════════════════
                // ÉCOLE - 5 permissions
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "Ecole.Create", Categorie = "Ecole", Action = "Create", Description = "Créer une école", Statut = true },
                new Permission { Nom = "Ecole.Read", Categorie = "Ecole", Action = "Read", Description = "Voir les informations d'une école", Statut = true },
                new Permission { Nom = "Ecole.ReadAll", Categorie = "Ecole", Action = "ReadAll", Description = "Voir toutes les écoles", Statut = true },
                new Permission { Nom = "Ecole.Update", Categorie = "Ecole", Action = "Update", Description = "Modifier une école", Statut = true },
                new Permission { Nom = "Ecole.Delete", Categorie = "Ecole", Action = "Delete", Description = "Supprimer une école", Statut = true },

                // ═══════════════════════════════════════════════════════════════════
                // UTILISATEUR - 6 permissions
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "Utilisateur.Create", Categorie = "Utilisateur", Action = "Create", Description = "Créer un utilisateur", Statut = true },
                new Permission { Nom = "Utilisateur.Read", Categorie = "Utilisateur", Action = "Read", Description = "Voir un utilisateur", Statut = true },
                new Permission { Nom = "Utilisateur.ReadAll", Categorie = "Utilisateur", Action = "ReadAll", Description = "Voir tous les utilisateurs", Statut = true },
                new Permission { Nom = "Utilisateur.Update", Categorie = "Utilisateur", Action = "Update", Description = "Modifier un utilisateur", Statut = true },
                new Permission { Nom = "Utilisateur.Delete", Categorie = "Utilisateur", Action = "Delete", Description = "Supprimer un utilisateur", Statut = true },
                new Permission { Nom = "Utilisateur.ChangePassword", Categorie = "Utilisateur", Action = "ChangePassword", Description = "Changer le mot de passe d'un utilisateur", Statut = true },

                // ═══════════════════════════════════════════════════════════════════
                // ÉLÈVE - 7 permissions
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "Eleve.Create", Categorie = "Eleve", Action = "Create", Description = "Créer un élève", Statut = true },
                new Permission { Nom = "Eleve.Read", Categorie = "Eleve", Action = "Read", Description = "Voir un élève", Statut = true },
                new Permission { Nom = "Eleve.ReadAll", Categorie = "Eleve", Action = "ReadAll", Description = "Voir tous les élèves", Statut = true },
                new Permission { Nom = "Eleve.ReadOwn", Categorie = "Eleve", Action = "ReadOwn", Description = "Voir ses propres informations (élève)", Statut = true },
                new Permission { Nom = "Eleve.ReadChildren", Categorie = "Eleve", Action = "ReadChildren", Description = "Voir ses enfants (parent)", Statut = true },
                new Permission { Nom = "Eleve.Update", Categorie = "Eleve", Action = "Update", Description = "Modifier un élève", Statut = true },
                new Permission { Nom = "Eleve.Delete", Categorie = "Eleve", Action = "Delete", Description = "Supprimer un élève", Statut = true },

                // ═══════════════════════════════════════════════════════════════════
                // AGENT - 5 permissions
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "Agent.Create", Categorie = "Agent", Action = "Create", Description = "Créer un agent", Statut = true },
                new Permission { Nom = "Agent.Read", Categorie = "Agent", Action = "Read", Description = "Voir un agent", Statut = true },
                new Permission { Nom = "Agent.ReadAll", Categorie = "Agent", Action = "ReadAll", Description = "Voir tous les agents", Statut = true },
                new Permission { Nom = "Agent.Update", Categorie = "Agent", Action = "Update", Description = "Modifier un agent", Statut = true },
                new Permission { Nom = "Agent.Delete", Categorie = "Agent", Action = "Delete", Description = "Supprimer un agent", Statut = true },

                // ═══════════════════════════════════════════════════════════════════
                // PAIEMENT - 7 permissions
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "Paiement.Create", Categorie = "Paiement", Action = "Create", Description = "Créer un paiement", Statut = true },
                new Permission { Nom = "Paiement.Read", Categorie = "Paiement", Action = "Read", Description = "Voir un paiement", Statut = true },
                new Permission { Nom = "Paiement.ReadAll", Categorie = "Paiement", Action = "ReadAll", Description = "Voir tous les paiements", Statut = true },
                new Permission { Nom = "Paiement.ReadOwn", Categorie = "Paiement", Action = "ReadOwn", Description = "Voir ses propres paiements (parent)", Statut = true },
                new Permission { Nom = "Paiement.Update", Categorie = "Paiement", Action = "Update", Description = "Modifier un paiement", Statut = true },
                new Permission { Nom = "Paiement.Delete", Categorie = "Paiement", Action = "Delete", Description = "Supprimer un paiement", Statut = true },
                new Permission { Nom = "Paiement.Validate", Categorie = "Paiement", Action = "Validate", Description = "Valider un paiement", Statut = true },

                // ═══════════════════════════════════════════════════════════════════
                // NOTE - 7 permissions
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "Note.Create", Categorie = "Note", Action = "Create", Description = "Créer une note", Statut = true },
                new Permission { Nom = "Note.Read", Categorie = "Note", Action = "Read", Description = "Voir une note", Statut = true },
                new Permission { Nom = "Note.ReadAll", Categorie = "Note", Action = "ReadAll", Description = "Voir toutes les notes", Statut = true },
                new Permission { Nom = "Note.ReadOwn", Categorie = "Note", Action = "ReadOwn", Description = "Voir ses propres notes (élève)", Statut = true },
                new Permission { Nom = "Note.ReadChildren", Categorie = "Note", Action = "ReadChildren", Description = "Voir les notes de ses enfants (parent)", Statut = true },
                new Permission { Nom = "Note.Update", Categorie = "Note", Action = "Update", Description = "Modifier une note", Statut = true },
                new Permission { Nom = "Note.Delete", Categorie = "Note", Action = "Delete", Description = "Supprimer une note", Statut = true },

                // ═══════════════════════════════════════════════════════════════════
                // TUTEUR - 5 permissions
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "Tuteur.Create", Categorie = "Tuteur", Action = "Create", Description = "Créer un tuteur", Statut = true },
                new Permission { Nom = "Tuteur.Read", Categorie = "Tuteur", Action = "Read", Description = "Voir un tuteur", Statut = true },
                new Permission { Nom = "Tuteur.ReadAll", Categorie = "Tuteur", Action = "ReadAll", Description = "Voir tous les tuteurs", Statut = true },
                new Permission { Nom = "Tuteur.Update", Categorie = "Tuteur", Action = "Update", Description = "Modifier un tuteur", Statut = true },
                new Permission { Nom = "Tuteur.Delete", Categorie = "Tuteur", Action = "Delete", Description = "Supprimer un tuteur", Statut = true },

                // ═══════════════════════════════════════════════════════════════════
                // RÔLE - 5 permissions
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "Role.Create", Categorie = "Role", Action = "Create", Description = "Créer un rôle", Statut = true },
                new Permission { Nom = "Role.Read", Categorie = "Role", Action = "Read", Description = "Voir un rôle", Statut = true },
                new Permission { Nom = "Role.ReadAll", Categorie = "Role", Action = "ReadAll", Description = "Voir tous les rôles", Statut = true },
                new Permission { Nom = "Role.Update", Categorie = "Role", Action = "Update", Description = "Modifier un rôle", Statut = true },
                new Permission { Nom = "Role.Delete", Categorie = "Role", Action = "Delete", Description = "Supprimer un rôle", Statut = true },

                // ═══════════════════════════════════════════════════════════════════
                // PERMISSION - 7 permissions
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "Permission.Create", Categorie = "Permission", Action = "Create", Description = "Créer une permission", Statut = true },
                new Permission { Nom = "Permission.Read", Categorie = "Permission", Action = "Read", Description = "Voir une permission", Statut = true },
                new Permission { Nom = "Permission.ReadAll", Categorie = "Permission", Action = "ReadAll", Description = "Voir toutes les permissions", Statut = true },
                new Permission { Nom = "Permission.Update", Categorie = "Permission", Action = "Update", Description = "Modifier une permission", Statut = true },
                new Permission { Nom = "Permission.Delete", Categorie = "Permission", Action = "Delete", Description = "Supprimer une permission", Statut = true },
                new Permission { Nom = "Permission.Assign", Categorie = "Permission", Action = "Assign", Description = "Assigner une permission à un rôle", Statut = true },
                new Permission { Nom = "Permission.Revoke", Categorie = "Permission", Action = "Revoke", Description = "Retirer une permission d'un rôle", Statut = true },

                // ═══════════════════════════════════════════════════════════════════
                // CLASSE - 5 permissions
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "Classe.Create", Categorie = "Classe", Action = "Create", Description = "Créer une classe", Statut = true },
                new Permission { Nom = "Classe.Read", Categorie = "Classe", Action = "Read", Description = "Voir une classe", Statut = true },
                new Permission { Nom = "Classe.ReadAll", Categorie = "Classe", Action = "ReadAll", Description = "Voir toutes les classes", Statut = true },
                new Permission { Nom = "Classe.Update", Categorie = "Classe", Action = "Update", Description = "Modifier une classe", Statut = true },
                new Permission { Nom = "Classe.Delete", Categorie = "Classe", Action = "Delete", Description = "Supprimer une classe", Statut = true },

                // ═══════════════════════════════════════════════════════════════════
                // FRAIS - 5 permissions
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "Frais.Create", Categorie = "Frais", Action = "Create", Description = "Créer un frais", Statut = true },
                new Permission { Nom = "Frais.Read", Categorie = "Frais", Action = "Read", Description = "Voir un frais", Statut = true },
                new Permission { Nom = "Frais.ReadAll", Categorie = "Frais", Action = "ReadAll", Description = "Voir tous les frais", Statut = true },
                new Permission { Nom = "Frais.Update", Categorie = "Frais", Action = "Update", Description = "Modifier un frais", Statut = true },
                new Permission { Nom = "Frais.Delete", Categorie = "Frais", Action = "Delete", Description = "Supprimer un frais", Statut = true },

                // ═══════════════════════════════════════════════════════════════════
                // INSCRIPTION - 5 permissions
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "Inscription.Create", Categorie = "Inscription", Action = "Create", Description = "Créer une inscription", Statut = true },
                new Permission { Nom = "Inscription.Read", Categorie = "Inscription", Action = "Read", Description = "Voir une inscription", Statut = true },
                new Permission { Nom = "Inscription.ReadAll", Categorie = "Inscription", Action = "ReadAll", Description = "Voir toutes les inscriptions", Statut = true },
                new Permission { Nom = "Inscription.Update", Categorie = "Inscription", Action = "Update", Description = "Modifier une inscription", Statut = true },
                new Permission { Nom = "Inscription.Delete", Categorie = "Inscription", Action = "Delete", Description = "Supprimer une inscription", Statut = true },

                // ═══════════════════════════════════════════════════════════════════
                // PRÉSENCE - 5 permissions
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "Presence.Create", Categorie = "Presence", Action = "Create", Description = "Créer une présence", Statut = true },
                new Permission { Nom = "Presence.Read", Categorie = "Presence", Action = "Read", Description = "Voir une présence", Statut = true },
                new Permission { Nom = "Presence.ReadAll", Categorie = "Presence", Action = "ReadAll", Description = "Voir toutes les présences", Statut = true },
                new Permission { Nom = "Presence.Update", Categorie = "Presence", Action = "Update", Description = "Modifier une présence", Statut = true },
                new Permission { Nom = "Presence.Delete", Categorie = "Presence", Action = "Delete", Description = "Supprimer une présence", Statut = true },

                // ═══════════════════════════════════════════════════════════════════
                // COURS - 5 permissions
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "Cours.Create", Categorie = "Cours", Action = "Create", Description = "Créer un cours", Statut = true },
                new Permission { Nom = "Cours.Read", Categorie = "Cours", Action = "Read", Description = "Voir un cours", Statut = true },
                new Permission { Nom = "Cours.ReadAll", Categorie = "Cours", Action = "ReadAll", Description = "Voir tous les cours", Statut = true },
                new Permission { Nom = "Cours.Update", Categorie = "Cours", Action = "Update", Description = "Modifier un cours", Statut = true },
                new Permission { Nom = "Cours.Delete", Categorie = "Cours", Action = "Delete", Description = "Supprimer un cours", Statut = true },
            };
        }

        /// <summary>
        /// Assigne les permissions aux rôles appropriés
        /// </summary>
        private static async Task AssignPermissionsToRolesAsync(KelasiNaBisoDbContext context)
        {
            // Récupérer les rôles existants
            var superAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "Super-Admin");
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "Admin");
            var directeurRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "Directeur");
            var financierRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "Financier"); // ✨ Changé de Comptable à Financier
            var enseignantRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "Enseignant");
            var parentRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "Parent");
            var eleveRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "Eleve");

            if (superAdminRole == null)
            {
                Console.WriteLine("⚠️ Rôles non trouvés. Les permissions seront créées mais non assignées.");
                Console.WriteLine("⚠️ Vous devrez assigner manuellement les permissions aux rôles.");
                return;
            }

            // Récupérer toutes les permissions
            var allPermissions = await context.Permissions.ToListAsync();

            // ═══════════════════════════════════════════════════════════════════
            // 🔴 SUPER-ADMIN : TOUTES LES PERMISSIONS (Root User - Aucune restriction)
            // ═══════════════════════════════════════════════════════════════════
            if (superAdminRole != null)
            {
                foreach (var permission in allPermissions)
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        IdRole = superAdminRole.IdRole,
                        IdPermission = permission.IdPermission,
                        DateAttribution = DateTime.UtcNow
                    });
                }
                Console.WriteLine($"✅ {allPermissions.Count} permissions assignées à Super-Admin (Root - Aucune restriction)");
            }

            // ═══════════════════════════════════════════════════════════════════
            // 🔵 ADMIN : Gestion complète de son école (sauf création/suppression d'écoles)
            // ═══════════════════════════════════════════════════════════════════
            if (adminRole != null)
            {
                var adminPermissions = allPermissions.Where(p =>
                    // Écoles : Lecture et modification uniquement (pas création/suppression)
                    (p.Categorie == "Ecole" && (p.Action == "Read" || p.Action == "ReadAll" || p.Action == "Update")) ||
                    // Gestion complète de son école
                    p.Categorie == "Utilisateur" ||
                    p.Categorie == "Eleve" ||
                    p.Categorie == "Agent" ||
                    p.Categorie == "Paiement" ||
                    p.Categorie == "Note" ||
                    p.Categorie == "Tuteur" ||
                    p.Categorie == "Classe" ||
                    p.Categorie == "Frais" ||
                    p.Categorie == "Inscription" ||
                    p.Categorie == "Presence" ||
                    p.Categorie == "Cours"
                ).ToList();

                foreach (var permission in adminPermissions)
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        IdRole = adminRole.IdRole,
                        IdPermission = permission.IdPermission,
                        DateAttribution = DateTime.UtcNow
                    });
                }
                Console.WriteLine($"✅ {adminPermissions.Count} permissions assignées à Admin");
            }

            // ═══════════════════════════════════════════════════════════════════
            // 🟢 DIRECTEUR : Mêmes permissions que Admin, sauf modification/suppression de paiements
            // Peut créer des utilisateurs sauf Admin et Super-Admin (vérifié au niveau métier)
            // ═══════════════════════════════════════════════════════════════════
            if (directeurRole != null)
            {
                // Prendre toutes les permissions de Admin
                var directeurPermissions = allPermissions.Where(p =>
                    // Écoles : Lecture et modification uniquement (pas création/suppression)
                    (p.Categorie == "Ecole" && (p.Action == "Read" || p.Action == "ReadAll" || p.Action == "Update")) ||
                    // Gestion complète de son école
                    p.Categorie == "Utilisateur" ||
                    p.Categorie == "Eleve" ||
                    p.Categorie == "Agent" ||
                    // Paiements : Création et lecture uniquement (PAS modification ni suppression)
                    (p.Categorie == "Paiement" && p.Action != "Update" && p.Action != "Delete") ||
                    p.Categorie == "Note" ||
                    p.Categorie == "Tuteur" ||
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
                        IdPermission = permission.IdPermission,
                        DateAttribution = DateTime.UtcNow
                    });
                }
                Console.WriteLine($"✅ {directeurPermissions.Count} permissions assignées à Directeur (mêmes que Admin sauf Paiement.Update et Paiement.Delete)");
            }

            // ═══════════════════════════════════════════════════════════════════
            // 🟡 ENSEIGNANT : Notes et présences de ses élèves
            // ═══════════════════════════════════════════════════════════════════
            if (enseignantRole != null)
            {
                var enseignantPermissions = allPermissions.Where(p =>
                    // Notes : Créer, lire, modifier (pas supprimer)
                    (p.Categorie == "Note" && p.Action != "Delete") ||
                    // Présences : Gestion complète
                    p.Categorie == "Presence" ||
                    // Élèves : Lecture seule
                    (p.Categorie == "Eleve" && (p.Action == "Read" || p.Action == "ReadAll")) ||
                    // Cours : Lecture seule
                    (p.Categorie == "Cours" && (p.Action == "Read" || p.Action == "ReadAll")) ||
                    // Classes : Lecture seule
                    (p.Categorie == "Classe" && (p.Action == "Read" || p.Action == "ReadAll"))
                ).ToList();

                foreach (var permission in enseignantPermissions)
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        IdRole = enseignantRole.IdRole,
                        IdPermission = permission.IdPermission,
                        DateAttribution = DateTime.UtcNow
                    });
                }
                Console.WriteLine($"✅ {enseignantPermissions.Count} permissions assignées à Enseignant");
            }

            // ═══════════════════════════════════════════════════════════════════
            // 🟠 FINANCIER : Gestion financière (Paiements et Frais)
            // Peut créer et lire les paiements, mais PAS modifier ni supprimer
            // ═══════════════════════════════════════════════════════════════════
            if (financierRole != null)
            {
                var financierPermissions = allPermissions.Where(p =>
                    // Paiements : Création et lecture uniquement (PAS modification ni suppression)
                    (p.Categorie == "Paiement" && p.Action != "Update" && p.Action != "Delete") ||
                    // Frais : Gestion complète
                    p.Categorie == "Frais" ||
                    // Élèves : Lecture seule (pour vérifier les paiements)
                    (p.Categorie == "Eleve" && (p.Action == "Read" || p.Action == "ReadAll")) ||
                    // Classes : Lecture seule (pour les frais par classe)
                    (p.Categorie == "Classe" && (p.Action == "Read" || p.Action == "ReadAll")) ||
                    // Inscriptions : Lecture seule
                    (p.Categorie == "Inscription" && (p.Action == "Read" || p.Action == "ReadAll"))
                ).ToList();

                foreach (var permission in financierPermissions)
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        IdRole = financierRole.IdRole,
                        IdPermission = permission.IdPermission,
                        DateAttribution = DateTime.UtcNow
                    });
                }
                Console.WriteLine($"✅ {financierPermissions.Count} permissions assignées à Financier (Paiement.Update et Paiement.Delete exclus)");
            }

            // ═══════════════════════════════════════════════════════════════════
            // 🟣 PARENT : Consultation des données de ses enfants uniquement
            // ═══════════════════════════════════════════════════════════════════
            if (parentRole != null)
            {
                var parentPermissions = allPermissions.Where(p =>
                    p.Action == "ReadChildren" || p.Action == "ReadOwn"
                ).ToList();

                foreach (var permission in parentPermissions)
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        IdRole = parentRole.IdRole,
                        IdPermission = permission.IdPermission,
                        DateAttribution = DateTime.UtcNow
                    });
                }
                Console.WriteLine($"✅ {parentPermissions.Count} permissions assignées à Parent");
            }

            // ═══════════════════════════════════════════════════════════════════
            // ⚪ ÉLÈVE : Consultation de ses propres données uniquement
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
                        IdPermission = permission.IdPermission,
                        DateAttribution = DateTime.UtcNow
                    });
                }
                Console.WriteLine($"✅ {elevePermissions.Count} permissions assignées à Élève");
            }

            // ═══════════════════════════════════════════════════════════════════
            // 🖥️ IT-SUPPORT : Cartes scolaires (lecture élèves/agents) + change MDP
            // ═══════════════════════════════════════════════════════════════════
            await EnsureItSupportPermissionsAsync(context);

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Assigne (idempotent) les permissions carte / lecture au rôle IT-Support.
        /// Appelé au démarrage même si le catalogue Permissions existe déjà.
        /// </summary>
        public static async Task EnsureItSupportPermissionsAsync(KelasiNaBisoDbContext context)
        {
            var itSupportRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == UserRoles.IT_SUPPORT);
            if (itSupportRole == null)
            {
                Console.WriteLine("⚠️ Le rôle IT-Support n'existe pas. Impossible d'assigner les permissions.");
                return;
            }

            var allPermissions = await context.Permissions.ToListAsync();
            if (!allPermissions.Any())
            {
                Console.WriteLine("⚠️ Aucune permission en base. Initialise d'abord le catalogue Permissions.");
                return;
            }

            var itSupportPermissions = allPermissions.Where(p =>
                // Élèves / agents : lecture pour listes d'impression
                (p.Categorie == "Eleve" && (p.Action == "Read" || p.Action == "ReadAll")) ||
                (p.Categorie == "Agent" && (p.Action == "Read" || p.Action == "ReadAll")) ||
                // Classes / école : contexte d'impression
                (p.Categorie == "Classe" && (p.Action == "Read" || p.Action == "ReadAll")) ||
                (p.Categorie == "Ecole" && (p.Action == "Read" || p.Action == "ReadAll")) ||
                // Compte : changer son mot de passe + lire son profil
                (p.Categorie == "Utilisateur" && (p.Action == "ChangePassword" || p.Action == "Read")) ||
                // Rôles : lecture (écran profil / filtres)
                (p.Categorie == "Role" && (p.Action == "Read" || p.Action == "ReadAll"))
            ).ToList();

            var existingIds = await context.RolePermissions
                .Where(rp => rp.IdRole == itSupportRole.IdRole)
                .Select(rp => rp.IdPermission)
                .ToListAsync();

            var toAdd = itSupportPermissions
                .Where(p => !existingIds.Contains(p.IdPermission))
                .ToList();

            if (!toAdd.Any())
            {
                Console.WriteLine($"✅ Le rôle IT-Support a déjà {existingIds.Count} permission(s) assignée(s).");
                return;
            }

            foreach (var permission in toAdd)
            {
                context.RolePermissions.Add(new RolePermission
                {
                    IdRole = itSupportRole.IdRole,
                    IdPermission = permission.IdPermission,
                    DateAttribution = DateTime.UtcNow
                });
            }

            await context.SaveChangesAsync();
            Console.WriteLine($"✅ {toAdd.Count} permission(s) ajoutée(s) au rôle IT-Support (total cible: {itSupportPermissions.Count})");
        }

        /// <summary>
        /// Assigne les permissions au rôle Admin si elles n'ont pas encore été assignées
        /// Cette méthode peut être appelée après la création d'un rôle Admin pour s'assurer
        /// que les permissions sont bien assignées même si le rôle a été créé après l'initialisation
        /// </summary>
        public static async Task EnsureAdminPermissionsAsync(KelasiNaBisoDbContext context)
        {
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "Admin");
            if (adminRole == null)
            {
                Console.WriteLine("⚠️ Le rôle Admin n'existe pas. Impossible d'assigner les permissions.");
                return;
            }

            // Vérifier si le rôle Admin a déjà des permissions assignées
            var existingPermissions = await context.RolePermissions
                .Where(rp => rp.IdRole == adminRole.IdRole)
                .Select(rp => rp.IdPermission)
                .ToListAsync();

            if (existingPermissions.Any())
            {
                Console.WriteLine($"✅ Le rôle Admin a déjà {existingPermissions.Count} permissions assignées.");
                return;
            }

            // Récupérer toutes les permissions
            var allPermissions = await context.Permissions.ToListAsync();
            if (!allPermissions.Any())
            {
                Console.WriteLine("⚠️ Aucune permission n'existe dans la base de données. Initialisez d'abord les permissions.");
                return;
            }

            // Définir les permissions pour le rôle Admin (même logique que dans AssignPermissionsToRolesAsync)
            var adminPermissions = allPermissions.Where(p =>
                // Écoles : Lecture et modification uniquement (pas création/suppression)
                (p.Categorie == "Ecole" && (p.Action == "Read" || p.Action == "ReadAll" || p.Action == "Update")) ||
                // Gestion complète de son école
                p.Categorie == "Utilisateur" ||
                p.Categorie == "Eleve" ||
                p.Categorie == "Agent" ||
                p.Categorie == "Paiement" ||
                p.Categorie == "Note" ||
                p.Categorie == "Tuteur" ||
                p.Categorie == "Classe" ||
                p.Categorie == "Frais" ||
                p.Categorie == "Inscription" ||
                p.Categorie == "Presence" ||
                p.Categorie == "Cours"
            ).ToList();

            // Assigner les permissions au rôle Admin
            foreach (var permission in adminPermissions)
            {
                // Vérifier que cette permission n'est pas déjà assignée (double sécurité)
                var alreadyExists = await context.RolePermissions
                    .AnyAsync(rp => rp.IdRole == adminRole.IdRole && rp.IdPermission == permission.IdPermission);

                if (!alreadyExists)
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        IdRole = adminRole.IdRole,
                        IdPermission = permission.IdPermission,
                        DateAttribution = DateTime.UtcNow
                    });
                }
            }

            await context.SaveChangesAsync();
            Console.WriteLine($"✅ {adminPermissions.Count} permissions assignées au rôle Admin");
        }
    }
}

