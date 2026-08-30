-- =============================================================================
-- KelasiNaBiso — Insertion idempotente du rôle IT-Support
-- Usage : impression cartes + attribution SerialNumber (périmètre école)
-- =============================================================================

INSERT INTO Roles (Nom, Description, Niveau, Statut, DateCreation)
SELECT
    'IT-Support',
    'Impression cartes scolaires et attribution SerialNumber',
    4,
    1,
    UTC_TIMESTAMP(6)
WHERE NOT EXISTS (
    SELECT 1 FROM Roles WHERE Nom = 'IT-Support'
);

-- Exemple d'affectation (à adapter) :
-- SET @id_role := (SELECT IdRole FROM Roles WHERE Nom = 'IT-Support' LIMIT 1);
-- SET @id_user := (SELECT IdUtilisateur FROM Utilisateurs WHERE Email = 'itsupport@ecole.cd' LIMIT 1);
-- INSERT INTO UserRoles (IdUtilisateur, IdRole, IsPrimary, Statut, DateAttribution)
-- SELECT @id_user, @id_role, 1, 1, UTC_TIMESTAMP(6)
-- WHERE @id_user IS NOT NULL AND @id_role IS NOT NULL
--   AND NOT EXISTS (
--     SELECT 1 FROM UserRoles
--     WHERE IdUtilisateur = @id_user AND IdRole = @id_role AND Statut = 1
--   );
