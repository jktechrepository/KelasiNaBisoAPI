-- ============================================================================
-- SCRIPT SQL RAPIDE : Assigner le rôle Admin à un utilisateur
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

-- Récupérer l'ID du rôle Admin
SELECT IdRole INTO @IdRoleAdmin FROM Roles WHERE Nom = 'Admin';

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

-- Vérification
SELECT 
    u.Email,
    CONCAT(u.PrenomUtilisateur, ' ', u.NomUtilisateur) AS Nom,
    r.Nom AS Role,
    (SELECT COUNT(*) FROM RolePermissions WHERE IdRole = @IdRoleAdmin) AS PermissionsTotal
FROM Utilisateurs u
LEFT JOIN UserRoles ur ON u.IdUtilisateur = ur.IdUtilisateur AND ur.IsPrimary = TRUE
LEFT JOIN Roles r ON ur.IdRole = r.IdRole
WHERE u.IdUtilisateur = @IdUtilisateur;
