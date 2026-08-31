-- =============================================================================
-- KelasiNaBiso — Rôle Controleur (contrôle à l'entrée)
-- Usage : créer le rôle et réassigner les agents « controlleur / contrôleur »
-- Les RolePermissions sont assignées automatiquement au redémarrage de l'API
-- (PermissionSeeder.EnsureControleurPermissionsAsync).
-- =============================================================================

-- 1. Créer le rôle Controleur (idempotent)
INSERT INTO Roles (Nom, Description, Niveau, Statut, DateCreation)
SELECT
    'Controleur',
    'Contrôle présence et frais (lecture)',
    4,
    1,
    UTC_TIMESTAMP(6)
WHERE NOT EXISTS (
    SELECT 1 FROM Roles WHERE Nom = 'Controleur'
);

-- 2. PRÉVISUALISATION — agents contrôleur liés à un utilisateur avec rôle Enseignant ou Personnel
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
--   AND LOWER(TRIM(a.Fonction)) IN (
--     'controlleur', 'contrôleur', 'controleur',
--     'contrôleur des frais', 'controleur des frais',
--     'contrôleur des entrées', 'controleur des entrees'
--   );

-- 3. Attribuer le rôle Controleur (idempotent par utilisateur)
-- SET @id_role_controleur := (SELECT IdRole FROM Roles WHERE Nom = 'Controleur' LIMIT 1);

-- INSERT INTO UserRoles (IdUtilisateur, IdRole, IsPrimary, Statut, DateAttribution)
-- SELECT u.IdUtilisateur, @id_role_controleur, 1, 1, UTC_TIMESTAMP(6)
-- FROM Utilisateurs u
-- INNER JOIN Agents a ON a.IdAgent = u.IdAgent
-- WHERE u.Statut = 1
--   AND LOWER(TRIM(a.Fonction)) IN (
--     'controlleur', 'contrôleur', 'controleur',
--     'contrôleur des frais', 'controleur des frais',
--     'contrôleur des entrées', 'controleur des entrees'
--   )
--   AND @id_role_controleur IS NOT NULL
--   AND NOT EXISTS (
--     SELECT 1 FROM UserRoles ur
--     WHERE ur.IdUtilisateur = u.IdUtilisateur
--       AND ur.IdRole = @id_role_controleur
--       AND ur.Statut = 1
--   );

-- 4. OPTIONNEL — retirer le rôle Enseignant pour les contrôleurs (uniquement si Enseignant
--    était le seul rôle métier). Décommenter après validation de l'étape 2.
-- SET @id_role_enseignant := (SELECT IdRole FROM Roles WHERE Nom = 'Enseignant' LIMIT 1);

-- UPDATE UserRoles ur
-- INNER JOIN Utilisateurs u ON u.IdUtilisateur = ur.IdUtilisateur
-- INNER JOIN Agents a ON a.IdAgent = u.IdAgent
-- SET ur.Statut = 0
-- WHERE ur.IdRole = @id_role_enseignant
--   AND ur.Statut = 1
--   AND LOWER(TRIM(a.Fonction)) IN (
--     'controlleur', 'contrôleur', 'controleur',
--     'contrôleur des frais', 'controleur des frais',
--     'contrôleur des entrées', 'controleur des entrees'
--   );

-- 5. Vérification
-- SELECT Nom, Description, Niveau FROM Roles WHERE Nom = 'Controleur';
-- SELECT COUNT(*) AS PermissionsControleur
-- FROM RolePermissions rp
-- INNER JOIN Roles r ON r.IdRole = rp.IdRole
-- WHERE r.Nom = 'Controleur';
