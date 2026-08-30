-- ============================================================================
-- SCRIPT SQL RAPIDE : Assigner le rôle Admin à un utilisateur (PAR ID)
-- ============================================================================
-- Usage : Utilisez cette version si vous avez des problèmes de collation
--         ou si vous préférez utiliser directement l'ID utilisateur
-- ============================================================================

-- ⚠️ MODIFIEZ CETTE LIGNE : Remplacez par l'ID de votre utilisateur
SET @IdUtilisateur = 2039;

-- Récupérer l'ID du rôle Admin
SELECT IdRole INTO @IdRoleAdmin FROM Roles WHERE Nom = 'Admin';

-- Vérification que l'utilisateur existe
SELECT 
    CASE 
        WHEN @IdUtilisateur IS NULL THEN '❌ ERREUR : ID utilisateur non défini !'
        WHEN NOT EXISTS (SELECT 1 FROM Utilisateurs WHERE IdUtilisateur = @IdUtilisateur) 
        THEN CONCAT('❌ ERREUR : Utilisateur ID ', @IdUtilisateur, ' non trouvé !')
        ELSE CONCAT('✅ Utilisateur trouvé : ID = ', @IdUtilisateur)
    END AS Verification;

-- Vérification que le rôle Admin existe
SELECT 
    CASE 
        WHEN @IdRoleAdmin IS NULL THEN '❌ ERREUR : Rôle Admin non trouvé !'
        ELSE CONCAT('✅ Rôle Admin trouvé : ID = ', @IdRoleAdmin)
    END AS Verification;

-- 1. Assigner toutes les permissions au rôle Admin (si pas déjà fait)
INSERT INTO RolePermissions (IdRole, IdPermission, DateAttribution)
SELECT @IdRoleAdmin, p.IdPermission, NOW()
FROM Permissions p
WHERE (
    (p.Categorie = 'Ecole' AND p.Action IN ('Read', 'ReadAll', 'Update'))
    OR p.Categorie IN ('Utilisateur', 'Eleve', 'Agent', 'Paiement', 'Note', 'Tuteur', 'Classe', 'Frais', 'Inscription', 'Presence', 'Cours')
)
AND NOT EXISTS (
    SELECT 1 FROM RolePermissions rp 
    WHERE rp.IdRole = @IdRoleAdmin AND rp.IdPermission = p.IdPermission
);

-- 2. Désactiver les autres rôles principaux
UPDATE UserRoles SET IsPrimary = FALSE WHERE IdUtilisateur = @IdUtilisateur;

-- 3. Assigner le rôle Admin comme rôle principal
INSERT INTO UserRoles (IdUtilisateur, IdRole, IsPrimary, DateAttribution, Statut)
VALUES (@IdUtilisateur, @IdRoleAdmin, TRUE, NOW(), TRUE)
ON DUPLICATE KEY UPDATE 
    IsPrimary = TRUE, 
    Statut = TRUE, 
    DateAttribution = NOW();

-- 4. Mettre à jour IdRole pour rétrocompatibilité
UPDATE Utilisateurs SET IdRole = @IdRoleAdmin WHERE IdUtilisateur = @IdUtilisateur;

-- Vérification finale
SELECT 
    u.IdUtilisateur,
    u.Email,
    CONCAT(u.PrenomUtilisateur, ' ', u.NomUtilisateur) AS Nom,
    r.Nom AS Role,
    ur.IsPrimary AS RolePrincipal,
    (SELECT COUNT(*) FROM RolePermissions WHERE IdRole = @IdRoleAdmin) AS PermissionsTotal
FROM Utilisateurs u
LEFT JOIN UserRoles ur ON u.IdUtilisateur = ur.IdUtilisateur AND ur.IsPrimary = TRUE
LEFT JOIN Roles r ON ur.IdRole = r.IdRole
WHERE u.IdUtilisateur = @IdUtilisateur;
