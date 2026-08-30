-- ============================================================================
-- SCRIPT SQL : AJOUT DES PERMISSIONS POUR LES DEVOIRS À DOMICILE
-- ============================================================================
-- Date : 2025-01-30
-- Objectif : Ajouter les permissions pour les devoirs à domicile dans la base de production
-- 
-- ⚠️ IMPORTANT : Exécuter ce script sur la base de données de PRODUCTION
-- ============================================================================

-- Désactiver temporairement les vérifications de clés étrangères pour éviter les erreurs
SET FOREIGN_KEY_CHECKS = 0;
SET SQL_SAFE_UPDATES = 0;

-- ============================================================================
-- ÉTAPE 1 : VÉRIFIER SI LES PERMISSIONS EXISTENT DÉJÀ
-- ============================================================================

SELECT '🔍 Vérification des permissions existantes...' AS Status;

-- Vérifier si les permissions DevoirADomicile existent déjà
SELECT 
    COUNT(*) AS PermissionsExistantes,
    GROUP_CONCAT(Nom SEPARATOR ', ') AS PermissionsTrouvees
FROM Permissions
WHERE Categorie = 'DevoirADomicile';

-- ============================================================================
-- ÉTAPE 2 : INSÉRER LES PERMISSIONS (si elles n'existent pas)
-- ============================================================================

SELECT '📝 Insertion des permissions DevoirADomicile...' AS Status;

-- Permission 1 : DevoirADomicile.Create
INSERT INTO Permissions (Nom, Categorie, Action, Description, Statut, DateCreation)
SELECT 
    'DevoirADomicile.Create',
    'DevoirADomicile',
    'Create',
    'Créer et publier un devoir à domicile',
    TRUE,
    NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM Permissions WHERE Nom = 'DevoirADomicile.Create'
);

-- Permission 2 : DevoirADomicile.Read
INSERT INTO Permissions (Nom, Categorie, Action, Description, Statut, DateCreation)
SELECT 
    'DevoirADomicile.Read',
    'DevoirADomicile',
    'Read',
    'Voir et consulter les devoirs à domicile',
    TRUE,
    NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM Permissions WHERE Nom = 'DevoirADomicile.Read'
);

-- Permission 3 : DevoirADomicile.Update
INSERT INTO Permissions (Nom, Categorie, Action, Description, Statut, DateCreation)
SELECT 
    'DevoirADomicile.Update',
    'DevoirADomicile',
    'Update',
    'Modifier un devoir à domicile existant',
    TRUE,
    NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM Permissions WHERE Nom = 'DevoirADomicile.Update'
);

-- Permission 4 : DevoirADomicile.Delete
INSERT INTO Permissions (Nom, Categorie, Action, Description, Statut, DateCreation)
SELECT 
    'DevoirADomicile.Delete',
    'DevoirADomicile',
    'Delete',
    'Supprimer un devoir à domicile',
    TRUE,
    NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM Permissions WHERE Nom = 'DevoirADomicile.Delete'
);

-- Permission 5 : DevoirADomicile.Download
INSERT INTO Permissions (Nom, Categorie, Action, Description, Statut, DateCreation)
SELECT 
    'DevoirADomicile.Download',
    'DevoirADomicile',
    'Download',
    'Télécharger le fichier PDF d''un devoir à domicile',
    TRUE,
    NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM Permissions WHERE Nom = 'DevoirADomicile.Download'
);

-- Afficher les permissions créées
SELECT '✅ Permissions créées :' AS Status;
SELECT IdPermission, Nom, Categorie, Action, Description
FROM Permissions
WHERE Categorie = 'DevoirADomicile'
ORDER BY Action;

-- ============================================================================
-- ÉTAPE 3 : ASSOCIER LES PERMISSIONS AUX RÔLES
-- ============================================================================

SELECT '🔗 Association des permissions aux rôles...' AS Status;

-- ════════════════════════════════════════════════════════════════════════════
-- SUPER-ADMIN : Toutes les permissions
-- ════════════════════════════════════════════════════════════════════════════

INSERT INTO RolePermissions (IdRole, IdPermission, DateAttribution, IdUtilisateurAttribution)
SELECT 
    r.IdRole,
    p.IdPermission,
    NOW(),
    NULL
FROM Roles r
CROSS JOIN Permissions p
WHERE r.Nom = 'Super-Admin'
  AND p.Categorie = 'DevoirADomicile'
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions rp 
      WHERE rp.IdRole = r.IdRole 
        AND rp.IdPermission = p.IdPermission
  );

-- ════════════════════════════════════════════════════════════════════════════
-- ADMIN : Create, Read, Update, Delete, Download (pour son école)
-- ════════════════════════════════════════════════════════════════════════════

INSERT INTO RolePermissions (IdRole, IdPermission, DateAttribution, IdUtilisateurAttribution)
SELECT 
    r.IdRole,
    p.IdPermission,
    NOW(),
    NULL
FROM Roles r
CROSS JOIN Permissions p
WHERE r.Nom = 'Admin'
  AND p.Categorie = 'DevoirADomicile'
  AND p.Action IN ('Create', 'Read', 'Update', 'Delete', 'Download')
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions rp 
      WHERE rp.IdRole = r.IdRole 
        AND rp.IdPermission = p.IdPermission
  );

-- ════════════════════════════════════════════════════════════════════════════
-- DIRECTEUR : Create, Read, Update, Delete, Download (pour son école)
-- ════════════════════════════════════════════════════════════════════════════

INSERT INTO RolePermissions (IdRole, IdPermission, DateAttribution, IdUtilisateurAttribution)
SELECT 
    r.IdRole,
    p.IdPermission,
    NOW(),
    NULL
FROM Roles r
CROSS JOIN Permissions p
WHERE r.Nom = 'Directeur'
  AND p.Categorie = 'DevoirADomicile'
  AND p.Action IN ('Create', 'Read', 'Update', 'Delete', 'Download')
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions rp 
      WHERE rp.IdRole = r.IdRole 
        AND rp.IdPermission = p.IdPermission
  );

-- ════════════════════════════════════════════════════════════════════════════
-- ENSEIGNANT : Create, Read, Update, Delete, Download (pour sa classe)
-- ════════════════════════════════════════════════════════════════════════════

INSERT INTO RolePermissions (IdRole, IdPermission, DateAttribution, IdUtilisateurAttribution)
SELECT 
    r.IdRole,
    p.IdPermission,
    NOW(),
    NULL
FROM Roles r
CROSS JOIN Permissions p
WHERE r.Nom = 'Enseignant'
  AND p.Categorie = 'DevoirADomicile'
  AND p.Action IN ('Create', 'Read', 'Update', 'Delete', 'Download')
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions rp 
      WHERE rp.IdRole = r.IdRole 
        AND rp.IdPermission = p.IdPermission
  );

-- ════════════════════════════════════════════════════════════════════════════
-- PARENT : Read, Download (pour la classe de son enfant)
-- ════════════════════════════════════════════════════════════════════════════

INSERT INTO RolePermissions (IdRole, IdPermission, DateAttribution, IdUtilisateurAttribution)
SELECT 
    r.IdRole,
    p.IdPermission,
    NOW(),
    NULL
FROM Roles r
CROSS JOIN Permissions p
WHERE r.Nom = 'Parent'
  AND p.Categorie = 'DevoirADomicile'
  AND p.Action IN ('Read', 'Download')
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions rp 
      WHERE rp.IdRole = r.IdRole 
        AND rp.IdPermission = p.IdPermission
  );

-- ════════════════════════════════════════════════════════════════════════════
-- ÉLÈVE : Read, Download (pour sa classe)
-- ════════════════════════════════════════════════════════════════════════════

INSERT INTO RolePermissions (IdRole, IdPermission, DateAttribution, IdUtilisateurAttribution)
SELECT 
    r.IdRole,
    p.IdPermission,
    NOW(),
    NULL
FROM Roles r
CROSS JOIN Permissions p
WHERE r.Nom = 'Eleve'
  AND p.Categorie = 'DevoirADomicile'
  AND p.Action IN ('Read', 'Download')
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions rp 
      WHERE rp.IdRole = r.IdRole 
        AND rp.IdPermission = p.IdPermission
  );

-- ============================================================================
-- ÉTAPE 4 : VÉRIFICATION ET RAPPORT
-- ============================================================================

SELECT '✅ Vérification des associations...' AS Status;

-- Afficher le résumé des permissions par rôle
SELECT 
    r.Nom AS Role,
    p.Action AS Permission,
    p.Description,
    rp.DateAttribution AS DateAttribution
FROM RolePermissions rp
INNER JOIN Roles r ON rp.IdRole = r.IdRole
INNER JOIN Permissions p ON rp.IdPermission = p.IdPermission
WHERE p.Categorie = 'DevoirADomicile'
ORDER BY r.Nom, p.Action;

-- Compter les permissions par rôle
SELECT 
    r.Nom AS Role,
    COUNT(*) AS NombrePermissions
FROM RolePermissions rp
INNER JOIN Roles r ON rp.IdRole = r.IdRole
INNER JOIN Permissions p ON rp.IdPermission = p.IdPermission
WHERE p.Categorie = 'DevoirADomicile'
GROUP BY r.Nom
ORDER BY r.Nom;

-- ============================================================================
-- ÉTAPE 5 : RÉACTIVER LES VÉRIFICATIONS
-- ============================================================================

SET FOREIGN_KEY_CHECKS = 1;
SET SQL_SAFE_UPDATES = 1;

SELECT '✅ Script terminé avec succès !' AS Status;

-- ============================================================================
-- RÉSUMÉ DES PERMISSIONS AJOUTÉES
-- ============================================================================
-- 
-- Permissions créées :
--   1. DevoirADomicile.Create
--   2. DevoirADomicile.Read
--   3. DevoirADomicile.Update
--   4. DevoirADomicile.Delete
--   5. DevoirADomicile.Download
--
-- Rôles et permissions associées :
--   - Super-Admin : Toutes les permissions (5/5)
--   - Admin : Create, Read, Update, Delete, Download (5/5)
--   - Directeur : Create, Read, Update, Delete, Download (5/5)
--   - Enseignant : Create, Read, Update, Delete, Download (5/5)
--   - Parent : Read, Download (2/5)
--   - Élève : Read, Download (2/5)
--
-- ============================================================================

