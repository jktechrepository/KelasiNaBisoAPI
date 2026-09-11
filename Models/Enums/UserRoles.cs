namespace KelasiNaBiso.Models.Enums
{
    /// <summary>
    /// Définit tous les rôles utilisateur du système avec leurs niveaux hiérarchiques
    /// Facilite l'utilisation des rôles avec IntelliSense et évite les erreurs de frappe
    /// </summary>
    public static class UserRoles
    {
        // ═══════════════════════════════════════════════════════════════════
        // 🔴 NIVEAU 1 : SYSTÈME (Accès complet à tout)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Super-Administrateur système - Accès complet à toutes les écoles et fonctionnalités
        /// </summary>
        public const string SUPER_ADMIN = "Super-Admin";

        // ═══════════════════════════════════════════════════════════════════
        // 🟠 NIVEAU 2 : DIRECTION ÉCOLE (Gestion complète d'une école)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Directeur d'école - Gestion complète de son école
        /// </summary>
        public const string DIRECTEUR = "Directeur";

        /// <summary>
        /// Sous-Directeur - Assiste le directeur avec permissions réduites
        /// </summary>
        public const string SOUS_DIRECTEUR = "Sous-Directeur";

        // ═══════════════════════════════════════════════════════════════════
        // 🟡 NIVEAU 3 : ADMINISTRATION (Gestion administrative)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Secrétaire - Gestion des inscriptions, élèves, documents
        /// </summary>
        public const string SECRETAIRE = "Secrétaire";

        /// <summary>
        /// Administrateur d'école - Gestion opérationnelle globale
        /// </summary>
        public const string ADMIN = "Admin";

        /// <summary>
        /// Financier - Gestion financière, paiements, frais
        /// </summary>
        public const string FINANCIER = "Financier";

        /// <summary>
        /// Caissier - Encaissement guichet (espèces, chèque, PayIn Moko)
        /// </summary>
        public const string CAISSIER = "Caissier";

        /// <summary>
        /// Controleur - Contrôle présence à l'entrée et vérification des frais (lecture seule)
        /// </summary>
        public const string CONTROLEUR = "Controleur";

        // ═══════════════════════════════════════════════════════════════════
        // 🟢 NIVEAU 4 : PÉDAGOGIE (Enseignement et éducation)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Enseignant - Gestion des notes, présences, cours
        /// </summary>
        public const string ENSEIGNANT = "Enseignant";

        /// <summary>
        /// Préfet - Discipline, gestion des présences
        /// </summary>
        public const string PREFET = "Préfet";

        // ═══════════════════════════════════════════════════════════════════
        // 🔵 NIVEAU 5 : UTILISATEURS EXTERNES (Consultation)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Parent/Tuteur - Consultation des données de ses enfants
        /// </summary>
        public const string PARENT = "Parent";

        /// <summary>
        /// Élève - Consultation de ses propres données
        /// </summary>
        public const string ELEVE = "Élève";

        /// <summary>
        /// Bailleur/Sponsor - Consultation des rapports financiers
        /// </summary>
        public const string BAILLEUR = "Bailleur";

        // ═══════════════════════════════════════════════════════════════════
        // 🟣 NIVEAU 6 : SUPPORT (Maintenance et support)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Agent de support - Assistance technique
        /// </summary>
        public const string AGENT_SUPPORT = "Agent Support";

        /// <summary>
        /// IT-Support — impression des cartes scolaires et attribution SerialNumber (périmètre école)
        /// </summary>
        public const string IT_SUPPORT = "IT-Support";

        /// <summary>
        /// Autre personnel - Rôle générique pour personnel non catégorisé
        /// </summary>
        public const string AUTRE_PERSONNEL = "Autre Personnel";

        // ═══════════════════════════════════════════════════════════════════
        // GROUPES DE RÔLES (pour faciliter les vérifications)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Tous les rôles ayant des droits d'administration
        /// </summary>
        public static string[] AdminRoles => new[]
        {
            SUPER_ADMIN,
            ADMIN,
            DIRECTEUR,
            SOUS_DIRECTEUR
        };

        /// <summary>
        /// Impression cartes + attribution SerialNumber
        /// </summary>
        public static string[] CardPrintRoles => new[]
        {
            SUPER_ADMIN,
            ADMIN,
            DIRECTEUR,
            IT_SUPPORT
        };

        /// <summary>
        /// Accès multi-écoles (toutes les écoles de la plateforme)
        /// </summary>
        public static string[] AllSchoolsAccessRoles => new[]
        {
            SUPER_ADMIN,
            IT_SUPPORT
        };

        /// <summary>
        /// Tous les rôles du personnel de l'école
        /// </summary>
        public static string[] StaffRoles => new[]
        {
            SUPER_ADMIN,
            DIRECTEUR,
            SOUS_DIRECTEUR,
            SECRETAIRE,
            FINANCIER,
            CAISSIER,
            CONTROLEUR,
            ENSEIGNANT,
            PREFET,
            IT_SUPPORT
        };

        /// <summary>
        /// Rôles autorisés au pointage / feuille d'appel (create + read, sans update/delete).
        /// </summary>
        public const string ControleurPresenceRoles =
            $"{SUPER_ADMIN},{ADMIN},{DIRECTEUR},{ENSEIGNANT},{PREFET},{CONTROLEUR}";

        /// <summary>
        /// Rôles autorisés à encaisser au guichet (PayIn Moko, création paiement).
        /// </summary>
        public const string CashierPayInRoles =
            $"{PARENT},{SUPER_ADMIN},{ADMIN},{FINANCIER},{CAISSIER}";

        /// <summary>
        /// Rôles autorisés à consulter l'overview paiement mobile (sans trésorerie).
        /// </summary>
        public const string CashierGuichetRoles =
            $"{SUPER_ADMIN},{ADMIN},{DIRECTEUR},{FINANCIER},{CAISSIER}";

        /// <summary>
        /// Rôles ayant accès à la trésorerie Moko (wallet, payouts, transactions).
        /// </summary>
        public const string FinanceTreasuryRoles =
            $"{SUPER_ADMIN},{ADMIN},{DIRECTEUR},{FINANCIER}";

        /// <summary>
        /// Rôles ayant accès à la gestion financière
        /// </summary>
        public const string FinanceRoles =
            $"{SUPER_ADMIN},{DIRECTEUR},{FINANCIER}";

        public static string[] FinanceRoleArray => new[]
        {
            SUPER_ADMIN,
            DIRECTEUR,
            FINANCIER
        };

        /// <summary>
        /// Rôles ayant accès à la gestion pédagogique
        /// </summary>
        public static string[] PedagogieRoles => new[]
        {
            SUPER_ADMIN,
            DIRECTEUR,
            SOUS_DIRECTEUR,
            ENSEIGNANT,
            PREFET
        };

        /// <summary>
        /// Rôles ayant accès à la gestion des élèves
        /// </summary>
        public static string[] GestionElevesRoles => new[]
        {
            SUPER_ADMIN,
            DIRECTEUR,
            SOUS_DIRECTEUR,
            SECRETAIRE
        };

        /// <summary>
        /// Rôles externes (non-personnel)
        /// </summary>
        public static string[] ExternalRoles => new[]
        {
            PARENT,
            ELEVE,
            BAILLEUR
        };

        // ═══════════════════════════════════════════════════════════════════
        // MÉTHODES UTILITAIRES
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Vérifie si un rôle est un rôle d'administration
        /// </summary>
        public static bool IsAdminRole(string role)
        {
            return AdminRoles.Contains(role);
        }

        /// <summary>
        /// Vérifie si un rôle peut imprimer les cartes / attribuer un SerialNumber
        /// </summary>
        public static bool CanPrintCards(string? role)
        {
            return !string.IsNullOrWhiteSpace(role) && CardPrintRoles.Contains(role);
        }

        /// <summary>
        /// Super-Admin et IT-Support : accès élèves/agents/cartes sur toutes les écoles.
        /// </summary>
        public static bool CanAccessAllSchools(string? role)
        {
            return !string.IsNullOrWhiteSpace(role)
                   && AllSchoolsAccessRoles.Any(r =>
                       string.Equals(r, role, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Vérifie si un rôle est un rôle de personnel
        /// </summary>
        public static bool IsStaffRole(string role)
        {
            return StaffRoles.Contains(role);
        }

        /// <summary>
        /// Vérifie si un rôle a accès à la gestion financière
        /// </summary>
        public static bool HasFinanceAccess(string role)
        {
            return FinanceRoleArray.Contains(role);
        }

        /// <summary>
        /// Vérifie si un rôle a accès à la gestion pédagogique
        /// </summary>
        public static bool HasPedagogieAccess(string role)
        {
            return PedagogieRoles.Contains(role);
        }

        /// <summary>
        /// Vérifie si un rôle peut gérer les élèves
        /// </summary>
        public static bool CanManageEleves(string role)
        {
            return GestionElevesRoles.Contains(role);
        }

        /// <summary>
        /// Vérifie si un rôle est un rôle externe (non-personnel)
        /// </summary>
        public static bool IsExternalRole(string role)
        {
            return ExternalRoles.Contains(role);
        }

        /// <summary>
        /// Retourne tous les rôles disponibles
        /// </summary>
        public static string[] GetAllRoles()
        {
            return new[]
            {
                SUPER_ADMIN,
                DIRECTEUR,
                SOUS_DIRECTEUR,
                SECRETAIRE,
                FINANCIER,
                CAISSIER,
                CONTROLEUR,
                ENSEIGNANT,
                PREFET,
                PARENT,
                ELEVE,
                BAILLEUR,
                AGENT_SUPPORT,
                IT_SUPPORT,
                AUTRE_PERSONNEL,
                ADMIN
            };
        }

        /// <summary>
        /// Retourne le niveau hiérarchique d'un rôle (1 = plus haut niveau)
        /// </summary>
        public static int GetRoleLevel(string role)
        {
            return role switch
            {
                SUPER_ADMIN => 1,
                DIRECTEUR => 2,
                SOUS_DIRECTEUR => 2,
                SECRETAIRE => 3,
                ADMIN => 3,
                FINANCIER => 3,
                CAISSIER => 3,
                CONTROLEUR => 4,
                ENSEIGNANT => 4,
                PREFET => 4,
                IT_SUPPORT => 4,
                PARENT => 5,
                ELEVE => 6,
                BAILLEUR => 5,
                AGENT_SUPPORT => 4,
                AUTRE_PERSONNEL => 5,
                _ => 10 // Rôle inconnu = niveau le plus bas
            };
        }

        /// <summary>
        /// Vérifie si le premier rôle a un niveau supérieur au second
        /// </summary>
        public static bool IsHigherLevel(string role1, string role2)
        {
            return GetRoleLevel(role1) < GetRoleLevel(role2);
        }
    }
}

