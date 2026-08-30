-- ============================================================================
-- SCRIPT SQL : Vérifier les permissions et rôles d'un utilisateur
-- ============================================================================
-- Usage : Remplacez l'email ou l'ID de l'utilisateur ci-dessous
-- ============================================================================

-- ⚠️ MODIFIEZ CETTE LIGNE : Remplacez par l'email de votre utilisateur
SET @EmailUtilisateur = 'ecamlimete2025@gmail.com';

-- OU utilisez directement l'ID :
-- SET @IdUtilisateur = 2039;

-- Récupérer l'ID utilisateur (avec gestion de la collation)
SELECT IdUtilisateur INTO @IdUtilisateur 
FROM Utilisateurs 
WHERE Email COLLATE utf8mb4_unicode_ci = @EmailUtilisateur COLLATE utf8mb4_unicode_ci;

-- ============================================================================
-- 1. Informations générales de l'utilisateur
-- ============================================================================
SELECT 
    '=== INFORMATIONS UTILISATEUR ===' AS Section;

SELECT 
    u.IdUtilisateur,
    u.Email,
    CONCAT(u.PrenomUtilisateur, ' ', u.NomUtilisateur, ' ', COALESCE(u.PostNomUtilisateur, '')) AS NomComplet,
    u.IdEcole,
    u.IdRole AS IdRoleLegacy,  -- Rôle legacy (rétrocompatibilité)
    u.Statut AS StatutUtilisateur,
    u.DateCreation
FROM Utilisateurs u
WHERE u.IdUtilisateur = @IdUtilisateur;

-- ============================================================================
-- 2. Rôles assignés à l'utilisateur
-- ============================================================================
SELECT 
    '=== RÔLES ASSIGNÉS ===' AS Section;

SELECT 
    ur.IdUserRole,
    r.Nom AS RoleNom,
    r.IdRole,
    ur.IsPrimary AS RolePrincipal,
    ur.Statut AS RoleStatut,
    ur.DateAttribution
FROM UserRoles ur
INNER JOIN Roles r ON ur.IdRole = r.IdRole
WHERE ur.IdUtilisateur = @IdUtilisateur
ORDER BY ur.IsPrimary DESC, ur.DateAttribution DESC;

-- ============================================================================
-- 3. Rôle principal actuel
-- ============================================================================
SELECT 
    '=== RÔLE PRINCIPAL ===' AS Section;

SELECT 
    r.Nom AS RolePrincipal,
    r.IdRole,
    ur.DateAttribution,
    ur.Statut
FROM UserRoles ur
INNER JOIN Roles r ON ur.IdRole = r.IdRole
WHERE ur.IdUtilisateur = @IdUtilisateur
AND ur.IsPrimary = TRUE
AND ur.Statut = TRUE;

-- ============================================================================
-- 4. Permissions de l'utilisateur (via ses rôles)
-- ============================================================================
SELECT 
    '=== PERMISSIONS (via rôles) ===' AS Section;

SELECT 
    p.Nom AS PermissionNom,
    p.Categorie,
    p.Action,
    p.Description,
    r.Nom AS RoleSource,
    rp.DateAttribution
FROM Permissions p
INNER JOIN RolePermissions rp ON p.IdPermission = rp.IdPermission
INNER JOIN UserRoles ur ON rp.IdRole = ur.IdRole
INNER JOIN Roles r ON ur.IdRole = r.IdRole
WHERE ur.IdUtilisateur = @IdUtilisateur
AND ur.Statut = TRUE
ORDER BY p.Categorie, p.Action;

-- ============================================================================
-- 5. Statistiques
-- ============================================================================
SELECT 
    '=== STATISTIQUES ===' AS Section;

SELECT 
    (SELECT COUNT(*) FROM UserRoles WHERE IdUtilisateur = @IdUtilisateur) AS 'Nombre total de rôles',
    (SELECT COUNT(*) FROM UserRoles WHERE IdUtilisateur = @IdUtilisateur AND IsPrimary = TRUE) AS 'Nombre de rôles principaux',
    (SELECT COUNT(*) FROM UserRoles WHERE IdUtilisateur = @IdUtilisateur AND Statut = TRUE) AS 'Nombre de rôles actifs',
    (
        SELECT COUNT(DISTINCT p.IdPermission)
        FROM Permissions p
        INNER JOIN RolePermissions rp ON p.IdPermission = rp.IdPermission
        INNER JOIN UserRoles ur ON rp.IdRole = ur.IdRole
        WHERE ur.IdUtilisateur = @IdUtilisateur
        AND ur.Statut = TRUE
    ) AS 'Nombre total de permissions';

-- ============================================================================
-- 6. Permissions par catégorie
-- ============================================================================
SELECT 
    '=== PERMISSIONS PAR CATÉGORIE ===' AS Section;

SELECT 
    p.Categorie,
    COUNT(DISTINCT p.IdPermission) AS NombrePermissions,
    GROUP_CONCAT(DISTINCT p.Action ORDER BY p.Action SEPARATOR ', ') AS Actions
FROM Permissions p
INNER JOIN RolePermissions rp ON p.IdPermission = rp.IdPermission
INNER JOIN UserRoles ur ON rp.IdRole = ur.IdRole
WHERE ur.IdUtilisateur = @IdUtilisateur
AND ur.Statut = TRUE
GROUP BY p.Categorie
ORDER BY p.Categorie;

-- ============================================================================
-- 7. Vérification des permissions Admin
-- ============================================================================
SELECT 
    '=== VÉRIFICATION PERMISSIONS ADMIN ===' AS Section;

SELECT 
    CASE 
        WHEN EXISTS (
            SELECT 1 
            FROM UserRoles ur
            INNER JOIN Roles r ON ur.IdRole = r.IdRole
            WHERE ur.IdUtilisateur = @IdUtilisateur
            AND r.Nom = 'Admin'
            AND ur.IsPrimary = TRUE
            AND ur.Statut = TRUE
        ) THEN '✅ Rôle Admin assigné comme rôle principal'
        ELSE '❌ Rôle Admin non assigné ou non principal'
    END AS StatutRoleAdmin,
    
    CASE 
        WHEN (
            SELECT COUNT(*)
            FROM RolePermissions rp
            INNER JOIN UserRoles ur ON rp.IdRole = ur.IdRole
            INNER JOIN Roles r ON ur.IdRole = r.IdRole
            WHERE ur.IdUtilisateur = @IdUtilisateur
            AND r.Nom = 'Admin'
            AND ur.Statut = TRUE
        ) > 50 THEN '✅ Permissions Admin correctement assignées'
        ELSE '⚠️ Nombre de permissions Admin suspect (attendu: > 50)'
    END AS StatutPermissionsAdmin;
