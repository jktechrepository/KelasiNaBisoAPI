-- ============================================================================
-- SCRIPT SQL : Assigner les permissions Admin et le rôle principal à un utilisateur
-- ============================================================================
-- Description : Ce script assigne toutes les permissions du rôle Admin
--               et définit le rôle Admin comme rôle principal pour un utilisateur
--
-- IMPORTANT : Remplacez les valeurs suivantes avant d'exécuter :
--   - @EmailUtilisateur : L'email de l'utilisateur à modifier
--   - OU @IdUtilisateur : L'ID de l'utilisateur (si vous préférez utiliser l'ID)
-- ============================================================================

-- Variables à modifier selon votre utilisateur
SET @EmailUtilisateur = 'ecamlimete2025@gmail.com';  -- ⚠️ MODIFIEZ CETTE VALEUR
-- OU utilisez l'ID directement :
-- SET @IdUtilisateur = 2039;  -- ⚠️ MODIFIEZ CETTE VALEUR si vous utilisez l'ID

-- ============================================================================
-- ÉTAPE 1 : Récupérer l'ID de l'utilisateur (si vous utilisez l'email)
-- ============================================================================
SET @IdUtilisateur = NULL;

-- Si vous utilisez l'email, décommentez cette ligne :
SELECT IdUtilisateur INTO @IdUtilisateur 
FROM Utilisateurs 
WHERE Email COLLATE utf8mb4_unicode_ci = @EmailUtilisateur COLLATE utf8mb4_unicode_ci;

-- Si vous utilisez directement l'ID, décommentez cette ligne :
-- SET @IdUtilisateur = 2039;  -- Remplacez par l'ID réel

-- Vérification que l'utilisateur existe
SELECT 
    CASE 
        WHEN @IdUtilisateur IS NULL THEN '❌ ERREUR : Utilisateur non trouvé !'
        ELSE CONCAT('✅ Utilisateur trouvé : ID = ', @IdUtilisateur)
    END AS Verification;

-- ============================================================================
-- ÉTAPE 2 : Récupérer l'ID du rôle Admin
-- ============================================================================
SET @IdRoleAdmin = NULL;

SELECT IdRole INTO @IdRoleAdmin 
FROM Roles 
WHERE Nom = 'Admin';

-- Vérification que le rôle Admin existe
SELECT 
    CASE 
        WHEN @IdRoleAdmin IS NULL THEN '❌ ERREUR : Rôle Admin non trouvé !'
        ELSE CONCAT('✅ Rôle Admin trouvé : ID = ', @IdRoleAdmin)
    END AS Verification;

-- ============================================================================
-- ÉTAPE 3 : S'assurer que le rôle Admin a toutes ses permissions assignées
-- ============================================================================

-- Récupérer toutes les permissions qui doivent être assignées au rôle Admin
-- Permissions Admin : 
-- - Ecole: Read, ReadAll, Update (PAS Create ni Delete)
-- - Toutes les permissions pour : Utilisateur, Eleve, Agent, Paiement, Note, 
--   Tuteur, Classe, Frais, Inscription, Presence, Cours

-- Insérer les permissions manquantes pour le rôle Admin
INSERT INTO RolePermissions (IdRole, IdPermission, DateAttribution)
SELECT 
    @IdRoleAdmin AS IdRole,
    p.IdPermission,
    NOW() AS DateAttribution
FROM Permissions p
WHERE 
    -- Écoles : Lecture et modification uniquement
    (
        (p.Categorie = 'Ecole' AND p.Action IN ('Read', 'ReadAll', 'Update'))
    )
    -- Gestion complète de son école
    OR p.Categorie = 'Utilisateur'
    OR p.Categorie = 'Eleve'
    OR p.Categorie = 'Agent'
    OR p.Categorie = 'Paiement'
    OR p.Categorie = 'Note'
    OR p.Categorie = 'Tuteur'
    OR p.Categorie = 'Classe'
    OR p.Categorie = 'Frais'
    OR p.Categorie = 'Inscription'
    OR p.Categorie = 'Presence'
    OR p.Categorie = 'Cours'
    -- Exclure les permissions déjà assignées
    AND NOT EXISTS (
        SELECT 1 
        FROM RolePermissions rp 
        WHERE rp.IdRole = @IdRoleAdmin 
        AND rp.IdPermission = p.IdPermission
    );

-- Afficher le nombre de permissions assignées
SELECT 
    COUNT(*) AS 'Nombre de permissions assignées au rôle Admin'
FROM RolePermissions 
WHERE IdRole = @IdRoleAdmin;

-- ============================================================================
-- ÉTAPE 4 : Assigner le rôle Admin à l'utilisateur dans UserRoles
-- ============================================================================

-- Désactiver les autres rôles principaux de l'utilisateur (un seul rôle principal à la fois)
UPDATE UserRoles 
SET IsPrimary = FALSE 
WHERE IdUtilisateur = @IdUtilisateur AND IsPrimary = TRUE;

-- Vérifier si le rôle Admin est déjà assigné à l'utilisateur
SET @UserRoleExists = (
    SELECT COUNT(*) 
    FROM UserRoles 
    WHERE IdUtilisateur = @IdUtilisateur 
    AND IdRole = @IdRoleAdmin
);

-- Si le rôle n'est pas encore assigné, l'ajouter
INSERT INTO UserRoles (
    IdUtilisateur, 
    IdRole, 
    IsPrimary, 
    DateAttribution, 
    Statut
)
SELECT 
    @IdUtilisateur,
    @IdRoleAdmin,
    TRUE AS IsPrimary,  -- Rôle principal
    NOW() AS DateAttribution,
    TRUE AS Statut
WHERE @UserRoleExists = 0;

-- Si le rôle existe déjà, le mettre à jour comme rôle principal
UPDATE UserRoles 
SET 
    IsPrimary = TRUE,
    Statut = TRUE,
    DateAttribution = NOW()
WHERE IdUtilisateur = @IdUtilisateur 
AND IdRole = @IdRoleAdmin;

-- ============================================================================
-- ÉTAPE 5 : Mettre à jour IdRole dans Utilisateurs (pour rétrocompatibilité)
-- ============================================================================
UPDATE Utilisateurs 
SET IdRole = @IdRoleAdmin 
WHERE IdUtilisateur = @IdUtilisateur;

-- ============================================================================
-- ÉTAPE 6 : Vérification finale
-- ============================================================================

-- Afficher les informations de l'utilisateur avec son rôle
SELECT 
    u.IdUtilisateur,
    u.Email,
    CONCAT(u.PrenomUtilisateur, ' ', u.NomUtilisateur, ' ', COALESCE(u.PostNomUtilisateur, '')) AS NomComplet,
    r.Nom AS RoleNom,
    r.IdRole,
    ur.IsPrimary AS RolePrincipal,
    ur.Statut AS RoleStatut,
    ur.DateAttribution AS DateAttributionRole
FROM Utilisateurs u
LEFT JOIN UserRoles ur ON u.IdUtilisateur = ur.IdUtilisateur AND ur.IsPrimary = TRUE
LEFT JOIN Roles r ON ur.IdRole = r.IdRole
WHERE u.IdUtilisateur = @IdUtilisateur;

-- Afficher toutes les permissions de l'utilisateur (via son rôle Admin)
SELECT 
    p.Nom AS PermissionNom,
    p.Categorie,
    p.Action,
    p.Description,
    rp.DateAttribution
FROM Permissions p
INNER JOIN RolePermissions rp ON p.IdPermission = rp.IdPermission
INNER JOIN UserRoles ur ON rp.IdRole = ur.IdRole
WHERE ur.IdUtilisateur = @IdUtilisateur
AND ur.IdRole = @IdRoleAdmin
AND ur.Statut = TRUE
ORDER BY p.Categorie, p.Action;

-- Afficher le résumé
SELECT 
    CONCAT('✅ Utilisateur ID ', @IdUtilisateur, ' : Rôle Admin assigné avec succès') AS Resultat,
    (SELECT COUNT(*) FROM RolePermissions WHERE IdRole = @IdRoleAdmin) AS 'Nombre total de permissions Admin',
    (SELECT COUNT(*) FROM UserRoles WHERE IdUtilisateur = @IdUtilisateur AND IsPrimary = TRUE) AS 'Nombre de rôles principaux';

-- ============================================================================
-- FIN DU SCRIPT
-- ============================================================================
