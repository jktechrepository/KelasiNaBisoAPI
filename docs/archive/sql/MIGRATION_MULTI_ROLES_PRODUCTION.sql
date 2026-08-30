-- ═══════════════════════════════════════════════════════════════════════════════
-- 📋 SCRIPT DE MIGRATION MULTI-RÔLES POUR PRODUCTION
-- ═══════════════════════════════════════════════════════════════════════════════
-- 
-- Description : Migration pour permettre à un utilisateur d'avoir plusieurs rôles
-- Date : 2025
-- Base de données : MariaDB 10.11+
-- 
-- ⚠️ IMPORTANT : 
--   1. Faire une sauvegarde complète de la base de données avant d'exécuter ce script
--   2. Tester d'abord sur une base de données de test
--   3. Exécuter ce script pendant une période de faible activité
--   4. Vérifier les résultats après l'exécution
--
-- ═══════════════════════════════════════════════════════════════════════════════

-- ═══════════════════════════════════════════════════════════════════════════════
-- ÉTAPE 1 : VÉRIFICATIONS PRÉLIMINAIRES
-- ═══════════════════════════════════════════════════════════════════════════════

-- Vérifier que la table UserRoles n'existe pas déjà
SELECT COUNT(*) as TableExists 
FROM information_schema.tables 
WHERE table_schema = DATABASE() 
  AND table_name = 'UserRoles';

-- Compter le nombre d'utilisateurs à migrer
SELECT COUNT(*) as TotalUtilisateurs 
FROM Utilisateurs 
WHERE IdRole IS NOT NULL;

-- ═══════════════════════════════════════════════════════════════════════════════
-- ÉTAPE 2 : CRÉATION DE LA TABLE UserRoles
-- ═══════════════════════════════════════════════════════════════════════════════

CREATE TABLE IF NOT EXISTS `UserRoles` (
    `IdUserRole` int NOT NULL AUTO_INCREMENT,
    `IdUtilisateur` int NOT NULL,
    `IdRole` int NOT NULL,
    `IsPrimary` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Indique si ce rôle est le rôle principal',
    `DateAttribution` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `IdUtilisateurAttribution` int NULL COMMENT 'ID de l''utilisateur qui a attribué ce rôle (pour audit)',
    `Statut` tinyint(1) NOT NULL DEFAULT 1 COMMENT 'Statut actif/inactif (soft delete)',
    CONSTRAINT `PK_UserRoles` PRIMARY KEY (`IdUserRole`),
    CONSTRAINT `FK_UserRoles_Utilisateurs_IdUtilisateur` 
        FOREIGN KEY (`IdUtilisateur`) 
        REFERENCES `Utilisateurs` (`IdUtilisateur`) 
        ON DELETE CASCADE,
    CONSTRAINT `FK_UserRoles_Roles_IdRole` 
        FOREIGN KEY (`IdRole`) 
        REFERENCES `Roles` (`IdRole`) 
        ON DELETE RESTRICT,
    CONSTRAINT `IX_UserRole_Utilisateur_Role_Unique` 
        UNIQUE KEY (`IdUtilisateur`, `IdRole`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Table de liaison N-N entre Utilisateurs et Roles pour permettre les multi-rôles';

-- ═══════════════════════════════════════════════════════════════════════════════
-- ÉTAPE 3 : CRÉATION DES INDEX POUR PERFORMANCE
-- ═══════════════════════════════════════════════════════════════════════════════

-- Index sur IdUtilisateur (pour les requêtes fréquentes)
CREATE INDEX IF NOT EXISTS `IX_UserRole_IdUtilisateur` 
    ON `UserRoles` (`IdUtilisateur`);

-- Index sur IdRole (pour les requêtes par rôle)
CREATE INDEX IF NOT EXISTS `IX_UserRole_IdRole` 
    ON `UserRoles` (`IdRole`);

-- Index composite sur IdUtilisateur et Statut (pour les requêtes actives)
CREATE INDEX IF NOT EXISTS `IX_UserRole_Utilisateur_Statut` 
    ON `UserRoles` (`IdUtilisateur`, `Statut`);

-- ═══════════════════════════════════════════════════════════════════════════════
-- ÉTAPE 4 : MIGRATION DES DONNÉES EXISTANTES
-- ═══════════════════════════════════════════════════════════════════════════════
-- 
-- Cette étape migre tous les utilisateurs existants vers la nouvelle table UserRoles
-- Chaque utilisateur aura son rôle actuel comme rôle principal (IsPrimary = 1)
--

INSERT INTO `UserRoles` (
    `IdUtilisateur`, 
    `IdRole`, 
    `IsPrimary`, 
    `DateAttribution`, 
    `Statut`
)
SELECT 
    `IdUtilisateur`,
    `IdRole`,
    1 AS `IsPrimary`,  -- Tous les rôles existants deviennent le rôle principal
    `DateCreation` AS `DateAttribution`,  -- Utiliser la date de création de l'utilisateur
    1 AS `Statut`  -- Tous actifs
FROM `Utilisateurs`
WHERE `IdRole` IS NOT NULL
  AND NOT EXISTS (
      -- Éviter les doublons si le script est réexécuté
      SELECT 1 
      FROM `UserRoles` ur 
      WHERE ur.`IdUtilisateur` = `Utilisateurs`.`IdUtilisateur` 
        AND ur.`IdRole` = `Utilisateurs`.`IdRole`
  );

-- ═══════════════════════════════════════════════════════════════════════════════
-- ÉTAPE 5 : VÉRIFICATIONS POST-MIGRATION
-- ═══════════════════════════════════════════════════════════════════════════════

-- Vérifier que tous les utilisateurs ont été migrés
SELECT 
    (SELECT COUNT(*) FROM `Utilisateurs` WHERE `IdRole` IS NOT NULL) AS TotalUtilisateurs,
    (SELECT COUNT(*) FROM `UserRoles` WHERE `Statut` = 1) AS TotalUserRoles,
    CASE 
        WHEN (SELECT COUNT(*) FROM `Utilisateurs` WHERE `IdRole` IS NOT NULL) = 
             (SELECT COUNT(*) FROM `UserRoles` WHERE `Statut` = 1)
        THEN '✅ Migration réussie : Tous les utilisateurs ont été migrés'
        ELSE '❌ ERREUR : Nombre d''utilisateurs et de UserRoles ne correspondent pas'
    END AS StatutMigration;

-- Vérifier que chaque utilisateur a exactement un rôle principal
SELECT 
    `IdUtilisateur`,
    COUNT(*) AS NombreRolesPrincipaux
FROM `UserRoles`
WHERE `IsPrimary` = 1 AND `Statut` = 1
GROUP BY `IdUtilisateur`
HAVING COUNT(*) > 1;

-- Si cette requête retourne des lignes, il y a un problème (un utilisateur ne peut avoir qu'un seul rôle principal)

-- Vérifier les utilisateurs sans rôle
SELECT 
    u.`IdUtilisateur`,
    u.`NomUtilisateur`,
    u.`Email`
FROM `Utilisateurs` u
LEFT JOIN `UserRoles` ur ON u.`IdUtilisateur` = ur.`IdUtilisateur` AND ur.`Statut` = 1
WHERE u.`IdRole` IS NOT NULL
  AND ur.`IdUserRole` IS NULL;

-- Si cette requête retourne des lignes, certains utilisateurs n'ont pas été migrés

-- ═══════════════════════════════════════════════════════════════════════════════
-- ÉTAPE 6 : STATISTIQUES POST-MIGRATION
-- ═══════════════════════════════════════════════════════════════════════════════

-- Afficher la répartition des rôles
SELECT 
    r.`Nom` AS NomRole,
    COUNT(ur.`IdUserRole`) AS NombreUtilisateurs
FROM `Roles` r
LEFT JOIN `UserRoles` ur ON r.`IdRole` = ur.`IdRole` AND ur.`Statut` = 1
GROUP BY r.`IdRole`, r.`Nom`
ORDER BY COUNT(ur.`IdUserRole`) DESC;

-- Afficher les utilisateurs avec leur(s) rôle(s)
SELECT 
    u.`IdUtilisateur`,
    u.`NomUtilisateur`,
    u.`Email`,
    GROUP_CONCAT(
        CONCAT(
            r.`Nom`, 
            IF(ur.`IsPrimary` = 1, ' (Principal)', '')
        ) 
        ORDER BY ur.`IsPrimary` DESC 
        SEPARATOR ', '
    ) AS Roles
FROM `Utilisateurs` u
INNER JOIN `UserRoles` ur ON u.`IdUtilisateur` = ur.`IdUtilisateur` AND ur.`Statut` = 1
INNER JOIN `Roles` r ON ur.`IdRole` = r.`IdRole`
GROUP BY u.`IdUtilisateur`, u.`NomUtilisateur`, u.`Email`
ORDER BY u.`IdUtilisateur`
LIMIT 20;  -- Limiter à 20 pour l'affichage

-- ═══════════════════════════════════════════════════════════════════════════════
-- ✅ MIGRATION TERMINÉE
-- ═══════════════════════════════════════════════════════════════════════════════
--
-- Prochaines étapes :
--   1. Vérifier que toutes les vérifications sont passées
--   2. Tester l'application avec la nouvelle structure
--   3. Mettre à jour le code de l'application pour utiliser UserRoles
--   4. (Optionnel) Supprimer la colonne IdRole de la table Utilisateurs plus tard
--      (après avoir vérifié que tout fonctionne correctement)
--
-- ═══════════════════════════════════════════════════════════════════════════════

