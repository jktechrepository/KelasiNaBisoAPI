-- ═══════════════════════════════════════════════════════════════════════════════
-- 📋 SCRIPT DE MIGRATION REFRESH TOKEN POUR PRODUCTION
-- ═══════════════════════════════════════════════════════════════════════════════
-- 
-- Description : Migration pour ajouter le système de refresh token JWT
-- Date : 2025-11-17
-- Base de données : MariaDB 10.11+
-- 
-- ⚠️ IMPORTANT : 
--   1. Faire une sauvegarde complète de la base de données avant d'exécuter ce script
--   2. Tester d'abord sur une base de données de test
--   3. Exécuter ce script pendant une période de faible activité
--   4. Vérifier les résultats après l'exécution
--   5. Cette migration est non-destructive (ajout de table uniquement)
--
-- ═══════════════════════════════════════════════════════════════════════════════

-- ═══════════════════════════════════════════════════════════════════════════════
-- ÉTAPE 1 : VÉRIFICATIONS PRÉLIMINAIRES
-- ═══════════════════════════════════════════════════════════════════════════════

-- Vérifier que la table RefreshTokens n'existe pas déjà
SELECT COUNT(*) as TableExists 
FROM information_schema.tables 
WHERE table_schema = DATABASE() 
  AND table_name = 'RefreshTokens';

-- Vérifier que la table Utilisateurs existe (table parente)
SELECT COUNT(*) as UtilisateursTableExists 
FROM information_schema.tables 
WHERE table_schema = DATABASE() 
  AND table_name = 'Utilisateurs';

-- ═══════════════════════════════════════════════════════════════════════════════
-- ÉTAPE 2 : CRÉATION DE LA TABLE RefreshTokens
-- ═══════════════════════════════════════════════════════════════════════════════

CREATE TABLE IF NOT EXISTS `RefreshTokens` (
    `IdRefreshToken` int NOT NULL AUTO_INCREMENT,
    `IdUtilisateur` int NOT NULL COMMENT 'ID de l''utilisateur propriétaire du token',
    `TokenHash` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'Token hashé (SHA256) pour sécurité',
    `DateCreation` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'Date de création du token',
    `DateExpiration` datetime(6) NOT NULL COMMENT 'Date d''expiration du token (30 jours par défaut)',
    `DateRevocation` datetime(6) NULL DEFAULT NULL COMMENT 'Date de révocation si le token a été révoqué',
    `DeviceInfo` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL DEFAULT NULL COMMENT 'Informations sur l''appareil (ex: Android iPhone, Web)',
    `IpAddress` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL DEFAULT NULL COMMENT 'Adresse IP qui a créé le token (pour audit)',
    CONSTRAINT `PK_RefreshTokens` PRIMARY KEY (`IdRefreshToken`),
    CONSTRAINT `FK_RefreshTokens_Utilisateurs_IdUtilisateur` 
        FOREIGN KEY (`IdUtilisateur`) 
        REFERENCES `Utilisateurs` (`IdUtilisateur`) 
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ═══════════════════════════════════════════════════════════════════════════════
-- ÉTAPE 3 : CRÉATION DES INDEX POUR PERFORMANCE
-- ═══════════════════════════════════════════════════════════════════════════════

-- Index sur IdUtilisateur pour les requêtes de recherche par utilisateur
CREATE INDEX IF NOT EXISTS `IX_RefreshTokens_IdUtilisateur` 
    ON `RefreshTokens` (`IdUtilisateur`);

-- Index sur TokenHash pour les recherches rapides lors de la validation
CREATE INDEX IF NOT EXISTS `IX_RefreshTokens_TokenHash` 
    ON `RefreshTokens` (`TokenHash`);

-- Index composite sur DateExpiration et DateRevocation pour le nettoyage automatique
CREATE INDEX IF NOT EXISTS `IX_RefreshTokens_Expiration_Revocation` 
    ON `RefreshTokens` (`DateExpiration`, `DateRevocation`);

-- ═══════════════════════════════════════════════════════════════════════════════
-- ÉTAPE 4 : VÉRIFICATIONS POST-MIGRATION
-- ═══════════════════════════════════════════════════════════════════════════════

-- Vérifier que la table a été créée correctement
SELECT 
    TABLE_NAME,
    TABLE_ROWS,
    CREATE_TIME,
    TABLE_COLLATION
FROM information_schema.tables 
WHERE table_schema = DATABASE() 
  AND table_name = 'RefreshTokens';

-- Vérifier que les index ont été créés
SELECT 
    INDEX_NAME,
    COLUMN_NAME,
    SEQ_IN_INDEX,
    NON_UNIQUE
FROM information_schema.statistics 
WHERE table_schema = DATABASE() 
  AND table_name = 'RefreshTokens'
ORDER BY INDEX_NAME, SEQ_IN_INDEX;

-- Vérifier que la clé étrangère a été créée
SELECT 
    CONSTRAINT_NAME,
    TABLE_NAME,
    COLUMN_NAME,
    REFERENCED_TABLE_NAME,
    REFERENCED_COLUMN_NAME,
    DELETE_RULE
FROM information_schema.KEY_COLUMN_USAGE 
WHERE table_schema = DATABASE() 
  AND table_name = 'RefreshTokens'
  AND REFERENCED_TABLE_NAME IS NOT NULL;

-- ═══════════════════════════════════════════════════════════════════════════════
-- ÉTAPE 5 : COMMENTAIRES ET DOCUMENTATION
-- ═══════════════════════════════════════════════════════════════════════════════

-- La table RefreshTokens permet de :
-- 1. Stocker les refresh tokens JWT de manière sécurisée (hashés avec SHA256)
-- 2. Gérer l'expiration des tokens (30 jours par défaut)
-- 3. Permettre la révocation des tokens (déconnexion d'un appareil)
-- 4. Tracer l'origine des tokens (device info, IP address) pour audit
-- 5. Nettoyer automatiquement les tokens expirés

-- ═══════════════════════════════════════════════════════════════════════════════
-- ✅ MIGRATION TERMINÉE
-- ═══════════════════════════════════════════════════════════════════════════════
-- 
-- Prochaines étapes :
-- 1. Vérifier que l'application démarre correctement
-- 2. Tester l'authentification et la génération de refresh tokens
-- 3. Tester le rafraîchissement de tokens via l'endpoint /api/Utilisateur/refresh-token
-- 4. Configurer un job de nettoyage automatique des tokens expirés (optionnel)
--
-- ═══════════════════════════════════════════════════════════════════════════════

