-- ═══════════════════════════════════════════════════════════════════════════════
-- 📋 SCRIPT DE MIGRATION POUR METTRE À JOUR LES PERMISSIONS EN PRODUCTION
-- ═══════════════════════════════════════════════════════════════════════════════
--
-- Description : Ce script met à jour les permissions pour les rôles Directeur et Financier
--               selon les nouvelles règles :
--               
--               CHANGEMENT 1 - DIRECTEUR :
--               - Avoir les mêmes permissions que Admin
--               - SAUF : Paiement.Update et Paiement.Delete (exclus)
--               - Peut créer des utilisateurs sauf Admin et Super-Admin (vérifié au niveau métier)
--               
--               CHANGEMENT 2 - FINANCIER :
--               - Ne peut plus modifier ni supprimer les paiements
--               - Exclure : Paiement.Update et Paiement.Delete
--
-- Date : 2025-01-19
-- Base de données : MariaDB 10.11+
--
-- ⚠️ IMPORTANT :
--   1. Faire une sauvegarde complète de la base de données avant d'exécuter ce script
--   2. Tester d'abord sur une base de données de test
--   3. Exécuter ce script pendant une période de faible activité
--   4. Vérifier les résultats après l'exécution
--   5. Ce script est idempotent (peut être exécuté plusieurs fois sans problème)
--
-- ═══════════════════════════════════════════════════════════════════════════════

-- ═══════════════════════════════════════════════════════════════════════════════
-- ÉTAPE 1 : VÉRIFICATIONS PRÉLIMINAIRES
-- ═══════════════════════════════════════════════════════════════════════════════

SELECT 'Vérification de l''existence des tables...' AS Info;
SELECT COUNT(*) AS TableRolesExists FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'Roles';
SELECT COUNT(*) AS TablePermissionsExists FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'Permissions';
SELECT COUNT(*) AS TableRolePermissionsExists FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'RolePermissions';

-- Vérifier que les rôles existent
SELECT 'Vérification de l''existence des rôles...' AS Info;
SELECT IdRole, Nom FROM Roles WHERE Nom IN ('Directeur', 'Financier', 'Admin') ORDER BY Nom;

-- Vérifier que les permissions Paiement existent
SELECT 'Vérification de l''existence des permissions Paiement...' AS Info;
SELECT IdPermission, Nom, Action FROM Permissions WHERE Categorie = 'Paiement' ORDER BY Nom;

-- ═══════════════════════════════════════════════════════════════════════════════
-- ÉTAPE 2 : SUPPRIMER LES PERMISSIONS Paiement.Update ET Paiement.Delete
--           DU RÔLE DIRECTEUR
-- ═══════════════════════════════════════════════════════════════════════════════

SELECT 'Suppression des permissions Paiement.Update et Paiement.Delete du rôle Directeur...' AS Info;

-- Supprimer Paiement.Update du rôle Directeur
DELETE rp FROM RolePermissions rp
INNER JOIN Roles r ON rp.IdRole = r.IdRole
INNER JOIN Permissions p ON rp.IdPermission = p.IdPermission
WHERE r.Nom = 'Directeur'
  AND p.Nom = 'Paiement.Update';

SELECT ROW_COUNT() AS PaiementUpdateDeletedFromDirecteur;

-- Supprimer Paiement.Delete du rôle Directeur
DELETE rp FROM RolePermissions rp
INNER JOIN Roles r ON rp.IdRole = r.IdRole
INNER JOIN Permissions p ON rp.IdPermission = p.IdPermission
WHERE r.Nom = 'Directeur'
  AND p.Nom = 'Paiement.Delete';

SELECT ROW_COUNT() AS PaiementDeleteDeletedFromDirecteur;

-- ═══════════════════════════════════════════════════════════════════════════════
-- ÉTAPE 3 : SUPPRIMER LES PERMISSIONS Paiement.Update ET Paiement.Delete
--           DU RÔLE FINANCIER
-- ═══════════════════════════════════════════════════════════════════════════════

SELECT 'Suppression des permissions Paiement.Update et Paiement.Delete du rôle Financier...' AS Info;

-- Supprimer Paiement.Update du rôle Financier
DELETE rp FROM RolePermissions rp
INNER JOIN Roles r ON rp.IdRole = r.IdRole
INNER JOIN Permissions p ON rp.IdPermission = p.IdPermission
WHERE r.Nom = 'Financier'
  AND p.Nom = 'Paiement.Update';

SELECT ROW_COUNT() AS PaiementUpdateDeletedFromFinancier;

-- Supprimer Paiement.Delete du rôle Financier
DELETE rp FROM RolePermissions rp
INNER JOIN Roles r ON rp.IdRole = r.IdRole
INNER JOIN Permissions p ON rp.IdPermission = p.IdPermission
WHERE r.Nom = 'Financier'
  AND p.Nom = 'Paiement.Delete';

SELECT ROW_COUNT() AS PaiementDeleteDeletedFromFinancier;

-- ═══════════════════════════════════════════════════════════════════════════════
-- ÉTAPE 4 : AJOUTER TOUTES LES PERMISSIONS ADMIN AU RÔLE DIRECTEUR
--           (SAUF CELLES QU'IL A DÉJÀ ET SAUF Paiement.Update et Paiement.Delete)
-- ═══════════════════════════════════════════════════════════════════════════════

SELECT 'Ajout des permissions Admin au rôle Directeur (sauf celles déjà présentes et Paiement.Update/Delete)...' AS Info;

-- Récupérer l'ID du rôle Directeur
SET @directeurRoleId = (SELECT IdRole FROM Roles WHERE Nom = 'Directeur' LIMIT 1);
SET @adminRoleId = (SELECT IdRole FROM Roles WHERE Nom = 'Admin' LIMIT 1);

-- Vérifier que les rôles existent
SELECT 
    CASE 
        WHEN @directeurRoleId IS NULL THEN 'ERREUR: Rôle Directeur non trouvé'
        WHEN @adminRoleId IS NULL THEN 'ERREUR: Rôle Admin non trouvé'
        ELSE 'Rôles trouvés avec succès'
    END AS Verification;

-- Insérer toutes les permissions Admin dans Directeur (sauf celles déjà présentes et Paiement.Update/Delete)
INSERT INTO RolePermissions (IdRole, IdPermission, DateAttribution, IdUtilisateurAttribution)
SELECT 
    @directeurRoleId AS IdRole,
    rp_admin.IdPermission,
    NOW() AS DateAttribution,
    NULL AS IdUtilisateurAttribution
FROM RolePermissions rp_admin
INNER JOIN Permissions p ON rp_admin.IdPermission = p.IdPermission
WHERE rp_admin.IdRole = @adminRoleId
  -- Exclure Paiement.Update et Paiement.Delete
  AND p.Nom NOT IN ('Paiement.Update', 'Paiement.Delete')
  -- Exclure les permissions déjà présentes pour Directeur
  AND NOT EXISTS (
      SELECT 1 
      FROM RolePermissions rp_directeur
      WHERE rp_directeur.IdRole = @directeurRoleId
        AND rp_directeur.IdPermission = rp_admin.IdPermission
  );

SELECT ROW_COUNT() AS PermissionsAddedToDirecteur;

-- ═══════════════════════════════════════════════════════════════════════════════
-- ÉTAPE 5 : VÉRIFICATIONS POST-MIGRATION
-- ═══════════════════════════════════════════════════════════════════════════════

SELECT 'Vérification des permissions du rôle Directeur après migration...' AS Info;

-- Vérifier que Directeur n'a plus Paiement.Update ni Paiement.Delete
SELECT 
    CASE 
        WHEN COUNT(*) = 0 THEN '✅ OK: Directeur n''a plus Paiement.Update ni Paiement.Delete'
        ELSE CONCAT('❌ ERREUR: Directeur a encore ', COUNT(*), ' permission(s) Paiement.Update/Delete')
    END AS VerificationPaiementDirecteur
FROM RolePermissions rp
INNER JOIN Roles r ON rp.IdRole = r.IdRole
INNER JOIN Permissions p ON rp.IdPermission = p.IdPermission
WHERE r.Nom = 'Directeur'
  AND p.Nom IN ('Paiement.Update', 'Paiement.Delete');

-- Vérifier que Financier n'a plus Paiement.Update ni Paiement.Delete
SELECT 
    CASE 
        WHEN COUNT(*) = 0 THEN '✅ OK: Financier n''a plus Paiement.Update ni Paiement.Delete'
        ELSE CONCAT('❌ ERREUR: Financier a encore ', COUNT(*), ' permission(s) Paiement.Update/Delete')
    END AS VerificationPaiementFinancier
FROM RolePermissions rp
INNER JOIN Roles r ON rp.IdRole = r.IdRole
INNER JOIN Permissions p ON rp.IdPermission = p.IdPermission
WHERE r.Nom = 'Financier'
  AND p.Nom IN ('Paiement.Update', 'Paiement.Delete');

-- Compter les permissions de Directeur
SELECT 
    'Directeur' AS Role,
    COUNT(*) AS NombrePermissions
FROM RolePermissions rp
INNER JOIN Roles r ON rp.IdRole = r.IdRole
WHERE r.Nom = 'Directeur';

-- Compter les permissions de Admin (pour comparaison)
SELECT 
    'Admin' AS Role,
    COUNT(*) AS NombrePermissions
FROM RolePermissions rp
INNER JOIN Roles r ON rp.IdRole = r.IdRole
WHERE r.Nom = 'Admin';

-- Compter les permissions de Financier
SELECT 
    'Financier' AS Role,
    COUNT(*) AS NombrePermissions
FROM RolePermissions rp
INNER JOIN Roles r ON rp.IdRole = r.IdRole
WHERE r.Nom = 'Financier';

-- Lister les permissions Paiement pour Directeur
SELECT 
    'Permissions Paiement du Directeur' AS Info,
    p.Nom AS Permission,
    p.Action AS Action
FROM RolePermissions rp
INNER JOIN Roles r ON rp.IdRole = r.IdRole
INNER JOIN Permissions p ON rp.IdPermission = p.IdPermission
WHERE r.Nom = 'Directeur'
  AND p.Categorie = 'Paiement'
ORDER BY p.Nom;

-- Lister les permissions Paiement pour Financier
SELECT 
    'Permissions Paiement du Financier' AS Info,
    p.Nom AS Permission,
    p.Action AS Action
FROM RolePermissions rp
INNER JOIN Roles r ON rp.IdRole = r.IdRole
INNER JOIN Permissions p ON rp.IdPermission = p.IdPermission
WHERE r.Nom = 'Financier'
  AND p.Categorie = 'Paiement'
ORDER BY p.Nom;

-- Vérifier que Directeur a Utilisateur.Create (pour créer des utilisateurs)
SELECT 
    CASE 
        WHEN COUNT(*) > 0 THEN '✅ OK: Directeur a la permission Utilisateur.Create'
        ELSE '❌ ERREUR: Directeur n''a pas la permission Utilisateur.Create'
    END AS VerificationUtilisateurCreateDirecteur
FROM RolePermissions rp
INNER JOIN Roles r ON rp.IdRole = r.IdRole
INNER JOIN Permissions p ON rp.IdPermission = p.IdPermission
WHERE r.Nom = 'Directeur'
  AND p.Nom = 'Utilisateur.Create';

-- ═══════════════════════════════════════════════════════════════════════════════
-- RÉSUMÉ FINAL
-- ═══════════════════════════════════════════════════════════════════════════════

SELECT '═══════════════════════════════════════════════════════════════════════════════' AS Separator;
SELECT 'MIGRATION TERMINÉE' AS Status;
SELECT '═══════════════════════════════════════════════════════════════════════════════' AS Separator;
SELECT 
    'Résumé des modifications:' AS Info,
    '1. Directeur: Permissions Admin ajoutées (sauf Paiement.Update et Paiement.Delete)' AS Modification1,
    '2. Financier: Paiement.Update et Paiement.Delete supprimées' AS Modification2,
    '3. Directeur peut maintenant créer des utilisateurs (sauf Admin et Super-Admin)' AS Modification3;

