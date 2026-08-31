-- =============================================================================
-- KelasiNaBiso — Rôle Caissier (guichet)
-- Usage : créer le rôle et réassigner les agents « caissier » depuis Financier
-- Les RolePermissions sont assignées automatiquement au redémarrage de l'API
-- (PermissionSeeder.EnsureCaissierPermissionsAsync).
-- =============================================================================

-- 1. Créer le rôle Caissier (idempotent)
INSERT INTO Roles (Nom, Description, Niveau, Statut, DateCreation)
SELECT
    'Caissier',
    'Encaissement guichet',
    3,
    1,
    UTC_TIMESTAMP(6)
WHERE NOT EXISTS (
    SELECT 1 FROM Roles WHERE Nom = 'Caissier'
);

-- 2. PRÉVISUALISATION — agents caissier liés à un utilisateur avec rôle Financier
-- SELECT
--     u.IdUtilisateur,
--     u.Email,
--     a.Fonction,
--     r.Nom AS RoleActuel
-- FROM Utilisateurs u
-- INNER JOIN Agents a ON a.IdAgent = u.IdAgent
-- INNER JOIN UserRoles ur ON ur.IdUtilisateur = u.IdUtilisateur AND ur.Statut = 1
-- INNER JOIN Roles r ON r.IdRole = ur.IdRole
-- WHERE u.Statut = 1
--   AND LOWER(TRIM(a.Fonction)) IN ('caissier', 'caissière', 'caissiere')
--   AND r.Nom = 'Financier';

-- 3. Attribuer le rôle Caissier (idempotent par utilisateur)
-- SET @id_role_caissier := (SELECT IdRole FROM Roles WHERE Nom = 'Caissier' LIMIT 1);

-- INSERT INTO UserRoles (IdUtilisateur, IdRole, IsPrimary, Statut, DateAttribution)
-- SELECT u.IdUtilisateur, @id_role_caissier, 1, 1, UTC_TIMESTAMP(6)
-- FROM Utilisateurs u
-- INNER JOIN Agents a ON a.IdAgent = u.IdAgent
-- WHERE u.Statut = 1
--   AND LOWER(TRIM(a.Fonction)) IN ('caissier', 'caissière', 'caissiere')
--   AND @id_role_caissier IS NOT NULL
--   AND NOT EXISTS (
--     SELECT 1 FROM UserRoles ur
--     WHERE ur.IdUtilisateur = u.IdUtilisateur
--       AND ur.IdRole = @id_role_caissier
--       AND ur.Statut = 1
--   );

-- 4. OPTIONNEL — retirer le rôle Financier pour les caissiers (uniquement si Financier
--    était le seul rôle métier finance). Décommenter après validation de l'étape 2.
-- SET @id_role_financier := (SELECT IdRole FROM Roles WHERE Nom = 'Financier' LIMIT 1);

-- UPDATE UserRoles ur
-- INNER JOIN Utilisateurs u ON u.IdUtilisateur = ur.IdUtilisateur
-- INNER JOIN Agents a ON a.IdAgent = u.IdAgent
-- SET ur.Statut = 0
-- WHERE ur.IdRole = @id_role_financier
--   AND ur.Statut = 1
--   AND LOWER(TRIM(a.Fonction)) IN ('caissier', 'caissière', 'caissiere');

-- 5. Vérification
-- SELECT Nom, Description, Niveau FROM Roles WHERE Nom = 'Caissier';
-- SELECT COUNT(*) AS PermissionsCaissier
-- FROM RolePermissions rp
-- INNER JOIN Roles r ON r.IdRole = rp.IdRole
-- WHERE r.Nom = 'Caissier';
