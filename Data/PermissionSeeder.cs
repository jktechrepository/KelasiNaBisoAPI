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
                await EnsureEvaluationPermissionsAsync(context);
                await EnsureCaissierPermissionsAsync(context);
                await EnsureControleurPermissionsAsync(context);
                await EnsureDirecteurPermissionsAsync(context);
                await EnsureElevePermissionsAsync(context);
                await EnsureBulletinPermissionsAsync(context);
                await EnsureDepensePermissionsAsync(context);
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
                (UserRoles.CAISSIER, "Encaissement guichet", 3),
                (UserRoles.CONTROLEUR, "Contrôle présence et frais (lecture)", 4),
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
                new Permission { Nom = "Paiement.PayOwn", Categorie = "Paiement", Action = "PayOwn", Description = "Payer ses propres frais (élève / parent)", Statut = true },
                new Permission { Nom = "Paiement.Update", Categorie = "Paiement", Action = "Update", Description = "Modifier un paiement", Statut = true },
                new Permission { Nom = "Paiement.Delete", Categorie = "Paiement", Action = "Delete", Description = "Supprimer un paiement", Statut = true },
                new Permission { Nom = "Paiement.Validate", Categorie = "Paiement", Action = "Validate", Description = "Valider un paiement", Statut = true },

                // ═══════════════════════════════════════════════════════════════════
                // NOTIFICATION - consultation « own »
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "Notification.ReadOwn", Categorie = "Notification", Action = "ReadOwn", Description = "Consulter ses propres notifications", Statut = true },
                new Permission { Nom = "Notification.UpdateOwn", Categorie = "Notification", Action = "UpdateOwn", Description = "Marquer ses notifications comme lues", Statut = true },

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
                // BULLETIN - 5 permissions
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "Bulletin.Read", Categorie = "Bulletin", Action = "Read", Description = "Voir les bulletins (classe / école)", Statut = true },
                new Permission { Nom = "Bulletin.ReadOwn", Categorie = "Bulletin", Action = "ReadOwn", Description = "Voir son propre bulletin (élève)", Statut = true },
                new Permission { Nom = "Bulletin.ReadChildren", Categorie = "Bulletin", Action = "ReadChildren", Description = "Voir les bulletins de ses enfants (parent)", Statut = true },
                new Permission { Nom = "Bulletin.Update", Categorie = "Bulletin", Action = "Update", Description = "Saisir décision / appréciation de bulletin", Statut = true },
                new Permission { Nom = "Bulletin.Unlock", Categorie = "Bulletin", Action = "Unlock", Description = "Déverrouiller un bulletin figé", Statut = true },

                // ═══════════════════════════════════════════════════════════════════
                // DEPENSE - 5 permissions
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "Depense.Read", Categorie = "Depense", Action = "Read", Description = "Voir une dépense", Statut = true },
                new Permission { Nom = "Depense.ReadAll", Categorie = "Depense", Action = "ReadAll", Description = "Lister les dépenses", Statut = true },
                new Permission { Nom = "Depense.Create", Categorie = "Depense", Action = "Create", Description = "Créer une dépense", Statut = true },
                new Permission { Nom = "Depense.Update", Categorie = "Depense", Action = "Update", Description = "Modifier / annuler une dépense", Statut = true },
                new Permission { Nom = "Depense.Delete", Categorie = "Depense", Action = "Delete", Description = "Supprimer (soft) une dépense", Statut = true },

                // ═══════════════════════════════════════════════════════════════════
                // CATEGORIE DEPENSE - 5 permissions
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "CategorieDepense.Read", Categorie = "CategorieDepense", Action = "Read", Description = "Voir une catégorie de dépense", Statut = true },
                new Permission { Nom = "CategorieDepense.ReadAll", Categorie = "CategorieDepense", Action = "ReadAll", Description = "Lister les catégories de dépense", Statut = true },
                new Permission { Nom = "CategorieDepense.Create", Categorie = "CategorieDepense", Action = "Create", Description = "Créer une catégorie de dépense", Statut = true },
                new Permission { Nom = "CategorieDepense.Update", Categorie = "CategorieDepense", Action = "Update", Description = "Modifier une catégorie de dépense", Statut = true },
                new Permission { Nom = "CategorieDepense.Delete", Categorie = "CategorieDepense", Action = "Delete", Description = "Supprimer / désactiver une catégorie", Statut = true },

                // ═══════════════════════════════════════════════════════════════════
                // EVALUATION - 5 permissions
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "Evaluation.Create", Categorie = "Evaluation", Action = "Create", Description = "Créer une évaluation", Statut = true },
                new Permission { Nom = "Evaluation.Read", Categorie = "Evaluation", Action = "Read", Description = "Voir une évaluation", Statut = true },
                new Permission { Nom = "Evaluation.ReadAll", Categorie = "Evaluation", Action = "ReadAll", Description = "Voir toutes les évaluations", Statut = true },
                new Permission { Nom = "Evaluation.Update", Categorie = "Evaluation", Action = "Update", Description = "Modifier une évaluation", Statut = true },
                new Permission { Nom = "Evaluation.Delete", Categorie = "Evaluation", Action = "Delete", Description = "Supprimer une évaluation", Statut = true },

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
                // FRAIS - 6 permissions
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "Frais.Create", Categorie = "Frais", Action = "Create", Description = "Créer un frais", Statut = true },
                new Permission { Nom = "Frais.Read", Categorie = "Frais", Action = "Read", Description = "Voir un frais", Statut = true },
                new Permission { Nom = "Frais.ReadAll", Categorie = "Frais", Action = "ReadAll", Description = "Voir tous les frais", Statut = true },
                new Permission { Nom = "Frais.ReadOwn", Categorie = "Frais", Action = "ReadOwn", Description = "Voir ses propres frais / situation de paiement (élève)", Statut = true },
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

                // ═══════════════════════════════════════════════════════════════════
                // DEVOIR À DOMICILE - 5 permissions
                // ═══════════════════════════════════════════════════════════════════
                new Permission { Nom = "DevoirADomicile.Create", Categorie = "DevoirADomicile", Action = "Create", Description = "Créer et publier un devoir à domicile", Statut = true },
                new Permission { Nom = "DevoirADomicile.Read", Categorie = "DevoirADomicile", Action = "Read", Description = "Voir et consulter les devoirs à domicile", Statut = true },
                new Permission { Nom = "DevoirADomicile.Update", Categorie = "DevoirADomicile", Action = "Update", Description = "Modifier un devoir à domicile existant", Statut = true },
                new Permission { Nom = "DevoirADomicile.Delete", Categorie = "DevoirADomicile", Action = "Delete", Description = "Supprimer un devoir à domicile", Statut = true },
                new Permission { Nom = "DevoirADomicile.Download", Categorie = "DevoirADomicile", Action = "Download", Description = "Télécharger le fichier PDF d'un devoir à domicile", Statut = true },
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
            var caissierRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == UserRoles.CAISSIER);
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
                    p.Categorie == "Bulletin" ||
                    p.Categorie == "Evaluation" ||
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
            // 🟢 DIRECTEUR : Mêmes permissions que Admin, sauf création/modification/suppression de paiements
            // Peut créer des utilisateurs sauf Admin et Super-Admin (vérifié au niveau métier)
            // ═══════════════════════════════════════════════════════════════════
            if (directeurRole != null)
            {
                var directeurPermissions = GetDirecteurPermissions(allPermissions);

                foreach (var permission in directeurPermissions)
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        IdRole = directeurRole.IdRole,
                        IdPermission = permission.IdPermission,
                        DateAttribution = DateTime.UtcNow
                    });
                }
                Console.WriteLine($"✅ {directeurPermissions.Count} permissions assignées à Directeur (mêmes que Admin sauf Paiement.Create, Paiement.Update et Paiement.Delete)");
            }

            // ═══════════════════════════════════════════════════════════════════
            // 🟡 ENSEIGNANT : Notes et présences de ses élèves
            // ═══════════════════════════════════════════════════════════════════
            if (enseignantRole != null)
            {
                var enseignantPermissions = allPermissions.Where(p =>
                    // Notes : Créer, lire, modifier (pas supprimer)
                    (p.Categorie == "Note" && p.Action != "Delete") ||
                    // Bulletins : lecture + saisie décision (ACL titulaire)
                    (p.Categorie == "Bulletin" && (p.Action == "Read" || p.Action == "Update")) ||
                    // Évaluations : Créer, lire, modifier (pas supprimer)
                    (p.Categorie == "Evaluation" && p.Action != "Delete") ||
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
                    // Dépenses : Create/Update/Read/ReadAll
                    (p.Categorie == "Depense" && p.Action != "Delete") ||
                    // Catégories dépense : sans Delete
                    (p.Categorie == "CategorieDepense" && p.Action != "Delete") ||
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
            // 🟤 CAISSIER : Encaissement guichet uniquement (subset strict du Financier)
            // ═══════════════════════════════════════════════════════════════════
            if (caissierRole != null)
            {
                var caissierPermissions = GetCaissierPermissions(allPermissions);

                foreach (var permission in caissierPermissions)
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        IdRole = caissierRole.IdRole,
                        IdPermission = permission.IdPermission,
                        DateAttribution = DateTime.UtcNow
                    });
                }
                Console.WriteLine($"✅ {caissierPermissions.Count} permissions assignées à Caissier (guichet uniquement)");
            }

            // ═══════════════════════════════════════════════════════════════════
            // 🟢 CONTROLEUR : Pointage présence + contrôle frais (lecture seule)
            // ═══════════════════════════════════════════════════════════════════
            var controleurRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == UserRoles.CONTROLEUR);
            if (controleurRole != null)
            {
                var controleurPermissions = GetControleurPermissions(allPermissions);

                foreach (var permission in controleurPermissions)
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        IdRole = controleurRole.IdRole,
                        IdPermission = permission.IdPermission,
                        DateAttribution = DateTime.UtcNow
                    });
                }
                Console.WriteLine($"✅ {controleurPermissions.Count} permissions assignées à Controleur");
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
                    || (p.Categorie == "DevoirADomicile" && (p.Action == "Read" || p.Action == "Download"))
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
            await EnsureCaissierPermissionsAsync(context);
            await EnsureControleurPermissionsAsync(context);
            await EnsureDirecteurPermissionsAsync(context);
            await EnsureElevePermissionsAsync(context);
            await EnsureBulletinPermissionsAsync(context);

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Permissions Directeur : mêmes que Admin sauf Paiement.Create/Update/Delete (lecture et validation uniquement).
        /// </summary>
        private static List<Permission> GetDirecteurPermissions(IEnumerable<Permission> allPermissions) =>
            allPermissions.Where(p =>
                (p.Categorie == "Ecole" && (p.Action == "Read" || p.Action == "ReadAll" || p.Action == "Update")) ||
                p.Categorie == "Utilisateur" ||
                p.Categorie == "Eleve" ||
                p.Categorie == "Agent" ||
                (p.Categorie == "Paiement" && (p.Action == "Read" || p.Action == "ReadAll" || p.Action == "Validate")) ||
                p.Categorie == "Note" ||
                p.Categorie == "Bulletin" ||
                p.Categorie == "Evaluation" ||
                p.Categorie == "Tuteur" ||
                p.Categorie == "Classe" ||
                p.Categorie == "Frais" ||
                p.Categorie == "Inscription" ||
                p.Categorie == "Presence" ||
                p.Categorie == "Cours" ||
                (p.Categorie == "Depense" && (p.Action == "Read" || p.Action == "ReadAll" || p.Action == "Update" || p.Action == "Delete")) ||
                p.Categorie == "CategorieDepense"
            ).ToList();

        /// <summary>
        /// Retire (idempotent) Paiement.Create/Update/Delete du rôle Directeur sur les bases existantes.
        /// </summary>
        public static async Task EnsureDirecteurPermissionsAsync(KelasiNaBisoDbContext context)
        {
            var directeurRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "Directeur");
            if (directeurRole == null)
            {
                Console.WriteLine("⚠️ Le rôle Directeur n'existe pas. Impossible de mettre à jour les permissions.");
                return;
            }

            var forbiddenNames = new[] { "Paiement.Create", "Paiement.Update", "Paiement.Delete" };
            var forbiddenPermissionIds = await context.Permissions
                .Where(p => forbiddenNames.Contains(p.Nom))
                .Select(p => p.IdPermission)
                .ToListAsync();

            if (!forbiddenPermissionIds.Any())
            {
                Console.WriteLine("⚠️ Permissions Paiement.Create/Update/Delete introuvables. Rien à retirer pour Directeur.");
                return;
            }

            var toRemove = await context.RolePermissions
                .Where(rp => rp.IdRole == directeurRole.IdRole && forbiddenPermissionIds.Contains(rp.IdPermission))
                .ToListAsync();

            if (!toRemove.Any())
            {
                Console.WriteLine("✅ Le rôle Directeur n'a déjà plus Paiement.Create/Update/Delete.");
                return;
            }

            context.RolePermissions.RemoveRange(toRemove);
            await context.SaveChangesAsync();
            Console.WriteLine($"✅ {toRemove.Count} permission(s) Paiement retirée(s) du rôle Directeur (Create/Update/Delete).");
        }

        /// <summary>
        /// Crée (idempotent) Bulletin.Read / ReadOwn / ReadChildren et les assigne aux rôles.
        /// </summary>
        public static async Task EnsureBulletinPermissionsAsync(KelasiNaBisoDbContext context)
        {
            var defs = new (string Nom, string Action, string Description)[]
            {
                ("Bulletin.Read", "Read", "Voir les bulletins (classe / école)"),
                ("Bulletin.ReadOwn", "ReadOwn", "Voir son propre bulletin (élève)"),
                ("Bulletin.ReadChildren", "ReadChildren", "Voir les bulletins de ses enfants (parent)"),
                ("Bulletin.Update", "Update", "Saisir décision / appréciation de bulletin"),
                ("Bulletin.Unlock", "Unlock", "Déverrouiller un bulletin figé"),
            };

            var existingNames = await context.Permissions
                .Where(p => p.Categorie == "Bulletin")
                .Select(p => p.Nom)
                .ToListAsync();

            var created = 0;
            foreach (var (nom, action, description) in defs)
            {
                if (existingNames.Contains(nom))
                    continue;

                context.Permissions.Add(new Permission
                {
                    Nom = nom,
                    Categorie = "Bulletin",
                    Action = action,
                    Description = description,
                    Statut = true,
                    DateCreation = DateTime.UtcNow
                });
                created++;
            }

            if (created > 0)
            {
                await context.SaveChangesAsync();
                Console.WriteLine($"✅ {created} permission(s) Bulletin ajoutée(s) au catalogue");
            }

            var all = await context.Permissions.Where(p => p.Categorie == "Bulletin").ToListAsync();
            if (!all.Any())
                return;

            async Task AssignToRole(string roleName, Func<Permission, bool> predicate)
            {
                var role = await context.Roles.FirstOrDefaultAsync(r => r.Nom == roleName
                    || (roleName == UserRoles.ELEVE && r.Nom == "Eleve"));
                if (role == null)
                    return;

                var existingIds = await context.RolePermissions
                    .Where(rp => rp.IdRole == role.IdRole)
                    .Select(rp => rp.IdPermission)
                    .ToListAsync();

                var toAdd = all.Where(predicate).Where(p => !existingIds.Contains(p.IdPermission)).ToList();
                foreach (var p in toAdd)
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        IdRole = role.IdRole,
                        IdPermission = p.IdPermission,
                        DateAttribution = DateTime.UtcNow
                    });
                }

                if (toAdd.Count > 0)
                    Console.WriteLine($"✅ {toAdd.Count} Bulletin.* assignée(s) à {roleName}");
            }

            await AssignToRole(UserRoles.SUPER_ADMIN, _ => true);
            await AssignToRole(UserRoles.ADMIN, _ => true);
            await AssignToRole(UserRoles.DIRECTEUR, _ => true);
            await AssignToRole(UserRoles.SOUS_DIRECTEUR, p => p.Action == "Read" || p.Action == "Update" || p.Action == "Unlock");
            await AssignToRole(UserRoles.ENSEIGNANT, p => p.Action == "Read" || p.Action == "Update");
            await AssignToRole(UserRoles.PARENT, p => p.Action == "ReadChildren");
            await AssignToRole(UserRoles.ELEVE, p => p.Action == "ReadOwn");

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Crée (idempotent) Depense.* / CategorieDepense.* et les assigne selon la matrice Kelasi V1.
        /// </summary>
        public static async Task EnsureDepensePermissionsAsync(KelasiNaBisoDbContext context)
        {
            var defs = new (string Nom, string Categorie, string Action, string Description)[]
            {
                ("Depense.Read", "Depense", "Read", "Voir une dépense"),
                ("Depense.ReadAll", "Depense", "ReadAll", "Lister les dépenses"),
                ("Depense.Create", "Depense", "Create", "Créer une dépense"),
                ("Depense.Update", "Depense", "Update", "Modifier / annuler une dépense"),
                ("Depense.Delete", "Depense", "Delete", "Supprimer (soft) une dépense"),
                ("CategorieDepense.Read", "CategorieDepense", "Read", "Voir une catégorie de dépense"),
                ("CategorieDepense.ReadAll", "CategorieDepense", "ReadAll", "Lister les catégories de dépense"),
                ("CategorieDepense.Create", "CategorieDepense", "Create", "Créer une catégorie de dépense"),
                ("CategorieDepense.Update", "CategorieDepense", "Update", "Modifier une catégorie de dépense"),
                ("CategorieDepense.Delete", "CategorieDepense", "Delete", "Supprimer / désactiver une catégorie"),
            };

            var existingNames = await context.Permissions
                .Where(p => p.Categorie == "Depense" || p.Categorie == "CategorieDepense")
                .Select(p => p.Nom)
                .ToListAsync();

            var created = 0;
            foreach (var (nom, categorie, action, description) in defs)
            {
                if (existingNames.Contains(nom))
                    continue;

                context.Permissions.Add(new Permission
                {
                    Nom = nom,
                    Categorie = categorie,
                    Action = action,
                    Description = description,
                    Statut = true,
                    DateCreation = DateTime.UtcNow
                });
                created++;
            }

            if (created > 0)
            {
                await context.SaveChangesAsync();
                Console.WriteLine($"✅ {created} permission(s) Depense/CategorieDepense ajoutée(s)");
            }

            var all = await context.Permissions
                .Where(p => p.Categorie == "Depense" || p.Categorie == "CategorieDepense")
                .ToListAsync();
            if (!all.Any())
                return;

            async Task AssignToRole(string roleName, Func<Permission, bool> predicate)
            {
                var role = await context.Roles.FirstOrDefaultAsync(r => r.Nom == roleName);
                if (role == null)
                    return;

                var existingIds = await context.RolePermissions
                    .Where(rp => rp.IdRole == role.IdRole)
                    .Select(rp => rp.IdPermission)
                    .ToListAsync();

                var toAdd = all.Where(predicate).Where(p => !existingIds.Contains(p.IdPermission)).ToList();
                foreach (var p in toAdd)
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        IdRole = role.IdRole,
                        IdPermission = p.IdPermission,
                        DateAttribution = DateTime.UtcNow
                    });
                }

                if (toAdd.Count > 0)
                    Console.WriteLine($"✅ {toAdd.Count} Depense/Categorie assignée(s) à {roleName}");
            }

            await AssignToRole(UserRoles.SUPER_ADMIN, _ => true);
            await AssignToRole(UserRoles.ADMIN, _ => true);
            await AssignToRole(UserRoles.FINANCIER, p =>
                (p.Categorie == "Depense" && p.Action != "Delete")
                || (p.Categorie == "CategorieDepense" && p.Action != "Delete"));
            await AssignToRole(UserRoles.CAISSIER, p =>
                (p.Categorie == "Depense" && (p.Action == "Create" || p.Action == "Read" || p.Action == "ReadAll"))
                || (p.Categorie == "CategorieDepense" && (p.Action == "Read" || p.Action == "ReadAll")));
            await AssignToRole(UserRoles.DIRECTEUR, p =>
                (p.Categorie == "Depense" && (p.Action == "Read" || p.Action == "ReadAll" || p.Action == "Update" || p.Action == "Delete"))
                || p.Categorie == "CategorieDepense");
            await AssignToRole(UserRoles.SOUS_DIRECTEUR, p =>
                (p.Categorie == "Depense" && (p.Action == "Read" || p.Action == "ReadAll" || p.Action == "Update"))
                || (p.Categorie == "CategorieDepense" && (p.Action == "Read" || p.Action == "ReadAll")));

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Assigne (idempotent) lecture « own », PayOwn, notifications et devoirs au rôle Eleve.
        /// Crée aussi Frais.ReadOwn, Paiement.PayOwn, Notification.ReadOwn/UpdateOwn si manquants.
        /// </summary>
        public static async Task EnsureElevePermissionsAsync(KelasiNaBisoDbContext context)
        {
            var extraDefs = new (string Nom, string Categorie, string Action, string Description)[]
            {
                ("Frais.ReadOwn", "Frais", "ReadOwn", "Voir ses propres frais / situation de paiement (élève)"),
                ("Paiement.PayOwn", "Paiement", "PayOwn", "Payer ses propres frais (élève / parent)"),
                ("Notification.ReadOwn", "Notification", "ReadOwn", "Consulter ses propres notifications"),
                ("Notification.UpdateOwn", "Notification", "UpdateOwn", "Marquer ses notifications comme lues"),
            };

            var extraNames = extraDefs.Select(d => d.Nom).ToList();
            var existingNames = await context.Permissions
                .Where(p => extraNames.Contains(p.Nom))
                .Select(p => p.Nom)
                .ToListAsync();

            var createdExtra = 0;
            foreach (var (nom, categorie, action, description) in extraDefs)
            {
                if (existingNames.Contains(nom))
                    continue;

                context.Permissions.Add(new Permission
                {
                    Nom = nom,
                    Categorie = categorie,
                    Action = action,
                    Description = description,
                    Statut = true,
                    DateCreation = DateTime.UtcNow
                });
                createdExtra++;
            }

            if (createdExtra > 0)
            {
                await context.SaveChangesAsync();
                Console.WriteLine($"✅ {createdExtra} permission(s) Eleve/PayOwn/Notification ajoutée(s) au catalogue");
            }

            var devoirDefs = new (string Nom, string Action, string Description)[]
            {
                ("DevoirADomicile.Create", "Create", "Créer et publier un devoir à domicile"),
                ("DevoirADomicile.Read", "Read", "Voir et consulter les devoirs à domicile"),
                ("DevoirADomicile.Update", "Update", "Modifier un devoir à domicile existant"),
                ("DevoirADomicile.Delete", "Delete", "Supprimer un devoir à domicile"),
                ("DevoirADomicile.Download", "Download", "Télécharger le fichier PDF d'un devoir à domicile"),
            };

            var existingDevoirNames = await context.Permissions
                .Where(p => p.Categorie == "DevoirADomicile")
                .Select(p => p.Nom)
                .ToListAsync();

            var createdDevoir = 0;
            foreach (var (nom, action, description) in devoirDefs)
            {
                if (existingDevoirNames.Contains(nom))
                    continue;

                context.Permissions.Add(new Permission
                {
                    Nom = nom,
                    Categorie = "DevoirADomicile",
                    Action = action,
                    Description = description,
                    Statut = true,
                    DateCreation = DateTime.UtcNow
                });
                createdDevoir++;
            }

            if (createdDevoir > 0)
            {
                await context.SaveChangesAsync();
                Console.WriteLine($"✅ {createdDevoir} permission(s) DevoirADomicile ajoutée(s) au catalogue");
            }

            var allPermissions = await context.Permissions.ToListAsync();
            if (!allPermissions.Any())
            {
                Console.WriteLine("⚠️ Aucune permission en base. Initialise d'abord le catalogue Permissions.");
                return;
            }

            // Parent : PayOwn pour payer les frais des enfants (PayIn Moko)
            await AssignPermissionsToRoleAsync(
                context,
                UserRoles.PARENT,
                allPermissions.Where(p => p.Nom == "Paiement.PayOwn"
                    || p.Nom == "Notification.ReadOwn"
                    || p.Nom == "Notification.UpdateOwn").ToList(),
                "Parent");

            // Admin / Super-Admin : notifications own (routes détail / marquer lu)
            var notifOwn = allPermissions.Where(p =>
                p.Nom == "Notification.ReadOwn" || p.Nom == "Notification.UpdateOwn").ToList();
            await AssignPermissionsToRoleAsync(context, UserRoles.ADMIN, notifOwn, "Admin");
            await AssignPermissionsToRoleAsync(context, UserRoles.SUPER_ADMIN, notifOwn, "Super-Admin");

            var eleveRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == UserRoles.ELEVE || r.Nom == "Eleve");
            if (eleveRole == null)
            {
                Console.WriteLine("⚠️ Le rôle Eleve n'existe pas. Impossible d'assigner les permissions.");
                return;
            }

            var targetPermissions = allPermissions.Where(p =>
                p.Action == "ReadOwn"
                || p.Nom == "Paiement.PayOwn"
                || p.Nom == "Notification.UpdateOwn"
                || (p.Categorie == "DevoirADomicile" && (p.Action == "Read" || p.Action == "Download"))
            ).ToList();

            var existingIds = await context.RolePermissions
                .Where(rp => rp.IdRole == eleveRole.IdRole)
                .Select(rp => rp.IdPermission)
                .ToListAsync();

            var toAdd = targetPermissions
                .Where(p => !existingIds.Contains(p.IdPermission))
                .ToList();

            if (!toAdd.Any())
            {
                Console.WriteLine($"✅ Le rôle Eleve a déjà les permissions cibles ({existingIds.Count} assignée(s)).");
                return;
            }

            foreach (var permission in toAdd)
            {
                context.RolePermissions.Add(new RolePermission
                {
                    IdRole = eleveRole.IdRole,
                    IdPermission = permission.IdPermission,
                    DateAttribution = DateTime.UtcNow
                });
            }

            await context.SaveChangesAsync();
            Console.WriteLine($"✅ {toAdd.Count} permission(s) ajoutée(s) au rôle Eleve (cible: {targetPermissions.Count})");
        }

        private static async Task AssignPermissionsToRoleAsync(
            KelasiNaBisoDbContext context,
            string roleName,
            List<Permission> permissions,
            string label)
        {
            if (permissions.Count == 0)
                return;

            var role = await context.Roles.FirstOrDefaultAsync(r => r.Nom == roleName);
            if (role == null)
                return;

            var existingIds = await context.RolePermissions
                .Where(rp => rp.IdRole == role.IdRole)
                .Select(rp => rp.IdPermission)
                .ToListAsync();

            var toAdd = permissions.Where(p => !existingIds.Contains(p.IdPermission)).ToList();
            foreach (var p in toAdd)
            {
                context.RolePermissions.Add(new RolePermission
                {
                    IdRole = role.IdRole,
                    IdPermission = p.IdPermission,
                    DateAttribution = DateTime.UtcNow
                });
            }

            if (toAdd.Count > 0)
            {
                await context.SaveChangesAsync();
                Console.WriteLine($"✅ {toAdd.Count} permission(s) ajoutée(s) au rôle {label}");
            }
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
        /// Permissions guichet pour le rôle Caissier (encaissement + lecture contexte).
        /// </summary>
        private static List<Permission> GetCaissierPermissions(IEnumerable<Permission> allPermissions) =>
            allPermissions.Where(p =>
                (p.Categorie == "Paiement" && p.Action != "Update" && p.Action != "Delete") ||
                (p.Categorie == "Frais" && (p.Action == "Read" || p.Action == "ReadAll")) ||
                (p.Categorie == "Eleve" && (p.Action == "Read" || p.Action == "ReadAll")) ||
                (p.Categorie == "Classe" && (p.Action == "Read" || p.Action == "ReadAll")) ||
                (p.Categorie == "Inscription" && (p.Action == "Read" || p.Action == "ReadAll")) ||
                (p.Categorie == "Depense" && (p.Action == "Create" || p.Action == "Read" || p.Action == "ReadAll")) ||
                (p.Categorie == "CategorieDepense" && (p.Action == "Read" || p.Action == "ReadAll"))
            ).ToList();

        /// <summary>
        /// Assigne (idempotent) les permissions guichet au rôle Caissier.
        /// Appelé au démarrage même si le catalogue Permissions existe déjà.
        /// </summary>
        public static async Task EnsureCaissierPermissionsAsync(KelasiNaBisoDbContext context)
        {
            var caissierRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == UserRoles.CAISSIER);
            if (caissierRole == null)
            {
                Console.WriteLine("⚠️ Le rôle Caissier n'existe pas. Impossible d'assigner les permissions.");
                return;
            }

            var allPermissions = await context.Permissions.ToListAsync();
            if (!allPermissions.Any())
            {
                Console.WriteLine("⚠️ Aucune permission en base. Initialise d'abord le catalogue Permissions.");
                return;
            }

            var caissierPermissions = GetCaissierPermissions(allPermissions);

            var existingIds = await context.RolePermissions
                .Where(rp => rp.IdRole == caissierRole.IdRole)
                .Select(rp => rp.IdPermission)
                .ToListAsync();

            var toAdd = caissierPermissions
                .Where(p => !existingIds.Contains(p.IdPermission))
                .ToList();

            if (!toAdd.Any())
            {
                Console.WriteLine($"✅ Le rôle Caissier a déjà {existingIds.Count} permission(s) assignée(s).");
                return;
            }

            foreach (var permission in toAdd)
            {
                context.RolePermissions.Add(new RolePermission
                {
                    IdRole = caissierRole.IdRole,
                    IdPermission = permission.IdPermission,
                    DateAttribution = DateTime.UtcNow
                });
            }

            await context.SaveChangesAsync();
            Console.WriteLine($"✅ {toAdd.Count} permission(s) ajoutée(s) au rôle Caissier (total cible: {caissierPermissions.Count})");
        }

        /// <summary>
        /// Permissions contrôle entrée : présence (create/read) + frais/paiements lecture.
        /// </summary>
        private static List<Permission> GetControleurPermissions(IEnumerable<Permission> allPermissions) =>
            allPermissions.Where(p =>
                (p.Categorie == "Presence" && p.Action != "Update" && p.Action != "Delete") ||
                (p.Categorie == "Frais" && (p.Action == "Read" || p.Action == "ReadAll")) ||
                (p.Categorie == "Paiement" && (p.Action == "Read" || p.Action == "ReadAll")) ||
                (p.Categorie == "Eleve" && (p.Action == "Read" || p.Action == "ReadAll")) ||
                (p.Categorie == "Classe" && (p.Action == "Read" || p.Action == "ReadAll")) ||
                (p.Categorie == "Inscription" && (p.Action == "Read" || p.Action == "ReadAll"))
            ).ToList();

        /// <summary>
        /// Assigne (idempotent) les permissions contrôle au rôle Controleur.
        /// </summary>
        public static async Task EnsureControleurPermissionsAsync(KelasiNaBisoDbContext context)
        {
            var controleurRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == UserRoles.CONTROLEUR);
            if (controleurRole == null)
            {
                Console.WriteLine("⚠️ Le rôle Controleur n'existe pas. Impossible d'assigner les permissions.");
                return;
            }

            var allPermissions = await context.Permissions.ToListAsync();
            if (!allPermissions.Any())
            {
                Console.WriteLine("⚠️ Aucune permission en base. Initialise d'abord le catalogue Permissions.");
                return;
            }

            var controleurPermissions = GetControleurPermissions(allPermissions);

            var existingIds = await context.RolePermissions
                .Where(rp => rp.IdRole == controleurRole.IdRole)
                .Select(rp => rp.IdPermission)
                .ToListAsync();

            var toAdd = controleurPermissions
                .Where(p => !existingIds.Contains(p.IdPermission))
                .ToList();

            if (!toAdd.Any())
            {
                Console.WriteLine($"✅ Le rôle Controleur a déjà {existingIds.Count} permission(s) assignée(s).");
                return;
            }

            foreach (var permission in toAdd)
            {
                context.RolePermissions.Add(new RolePermission
                {
                    IdRole = controleurRole.IdRole,
                    IdPermission = permission.IdPermission,
                    DateAttribution = DateTime.UtcNow
                });
            }

            await context.SaveChangesAsync();
            Console.WriteLine($"✅ {toAdd.Count} permission(s) ajoutée(s) au rôle Controleur (total cible: {controleurPermissions.Count})");
        }

        /// <summary>
        /// Crée (idempotent) les permissions Evaluation.* et les assigne aux rôles pédagogiques.
        /// </summary>
        public static async Task EnsureEvaluationPermissionsAsync(KelasiNaBisoDbContext context)
        {
            var defs = new (string Nom, string Action, string Description)[]
            {
                ("Evaluation.Create", "Create", "Créer une évaluation"),
                ("Evaluation.Read", "Read", "Voir une évaluation"),
                ("Evaluation.ReadAll", "ReadAll", "Voir toutes les évaluations"),
                ("Evaluation.Update", "Update", "Modifier une évaluation"),
                ("Evaluation.Delete", "Delete", "Supprimer une évaluation"),
            };

            var existingNames = await context.Permissions
                .Where(p => p.Categorie == "Evaluation")
                .Select(p => p.Nom)
                .ToListAsync();

            var created = 0;
            foreach (var (nom, action, description) in defs)
            {
                if (existingNames.Contains(nom))
                    continue;

                context.Permissions.Add(new Permission
                {
                    Nom = nom,
                    Categorie = "Evaluation",
                    Action = action,
                    Description = description,
                    Statut = true,
                    DateCreation = DateTime.UtcNow
                });
                created++;
            }

            if (created > 0)
                await context.SaveChangesAsync();

            var evalPermissions = await context.Permissions
                .Where(p => p.Categorie == "Evaluation")
                .ToListAsync();

            if (!evalPermissions.Any())
                return;

            var roleNames = new[] { "Super-Admin", "Admin", "Directeur", "Enseignant" };
            var roles = await context.Roles.Where(r => roleNames.Contains(r.Nom)).ToListAsync();

            foreach (var role in roles)
            {
                var existingIds = await context.RolePermissions
                    .Where(rp => rp.IdRole == role.IdRole)
                    .Select(rp => rp.IdPermission)
                    .ToListAsync();

                foreach (var permission in evalPermissions)
                {
                    if (role.Nom == "Enseignant" && permission.Action == "Delete")
                        continue;

                    if (existingIds.Contains(permission.IdPermission))
                        continue;

                    context.RolePermissions.Add(new RolePermission
                    {
                        IdRole = role.IdRole,
                        IdPermission = permission.IdPermission,
                        DateAttribution = DateTime.UtcNow
                    });
                }
            }

            await context.SaveChangesAsync();
            if (created > 0)
                Console.WriteLine($"✅ {created} permission(s) Evaluation ajoutée(s) et assignées aux rôles.");
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

