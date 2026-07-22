-- =============================================================================
-- KelasiNaBiso — Permissions du rôle IT-Support (idempotent)
-- Périmètre : lecture Élèves/Agents/Classes/École, change MDP, lecture Role
-- =============================================================================

USE knb_db;
-- USE `dev-knb_db`;

SET @id_role := (SELECT IdRole FROM Roles WHERE Nom = 'IT-Support' LIMIT 1);

-- Créer le rôle s'il manque
INSERT INTO Roles (Nom, Description, Niveau, Statut, DateCreation)
SELECT
    'IT-Support',
    'Impression cartes scolaires et attribution SerialNumber',
    4,
    1,
    UTC_TIMESTAMP(6)
WHERE @id_role IS NULL;

SET @id_role := (SELECT IdRole FROM Roles WHERE Nom = 'IT-Support' LIMIT 1);

INSERT INTO RolePermissions (IdRole, IdPermission, DateAttribution)
SELECT @id_role, p.IdPermission, UTC_TIMESTAMP(6)
FROM Permissions p
WHERE @id_role IS NOT NULL
  AND (
        (p.Categorie = 'Eleve' AND p.Action IN ('Read', 'ReadAll'))
     OR (p.Categorie = 'Agent' AND p.Action IN ('Read', 'ReadAll'))
     OR (p.Categorie = 'Classe' AND p.Action IN ('Read', 'ReadAll'))
     OR (p.Categorie = 'Ecole' AND p.Action IN ('Read', 'ReadAll'))
     OR (p.Categorie = 'Utilisateur' AND p.Action IN ('ChangePassword', 'Read'))
     OR (p.Categorie = 'Role' AND p.Action IN ('Read', 'ReadAll'))
  )
  AND NOT EXISTS (
        SELECT 1 FROM RolePermissions rp
        WHERE rp.IdRole = @id_role AND rp.IdPermission = p.IdPermission
  );

SELECT r.Nom AS Role, COUNT(rp.IdPermission) AS NbPermissions
FROM Roles r
LEFT JOIN RolePermissions rp ON rp.IdRole = r.IdRole
WHERE r.Nom = 'IT-Support'
GROUP BY r.Nom;
