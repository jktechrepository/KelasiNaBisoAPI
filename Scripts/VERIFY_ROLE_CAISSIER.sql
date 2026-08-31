-- =============================================================================
-- KelasiNaBiso — Vérification post-déploiement rôle Caissier (knb_db)
-- Usage : exécuter après déploiement API + migration utilisateurs
-- =============================================================================

-- 1. Rôle Caissier existe
SELECT IdRole, Nom, Description, Niveau, Statut
FROM Roles
WHERE Nom = 'Caissier';

-- 2. Permissions assignées au Caissier (attendu : ~13)
SELECT p.Nom, p.Categorie, p.Action
FROM RolePermissions rp
INNER JOIN Roles r ON r.IdRole = rp.IdRole
INNER JOIN Permissions p ON p.IdPermission = rp.IdPermission
WHERE r.Nom = 'Caissier'
ORDER BY p.Categorie, p.Nom;

-- 3. Utilisateurs avec rôle Caissier actif
SELECT u.IdUtilisateur, u.Email, u.NomUtilisateur, a.Fonction, r.Nom AS Role
FROM UserRoles ur
INNER JOIN Utilisateurs u ON u.IdUtilisateur = ur.IdUtilisateur
INNER JOIN Roles r ON r.IdRole = ur.IdRole
LEFT JOIN Agents a ON a.IdAgent = u.IdAgent
WHERE ur.Statut = 1 AND r.Nom = 'Caissier'
ORDER BY u.IdUtilisateur;

-- 4. Agents caissier encore mappés sur Financier uniquement (à migrer)
SELECT u.IdUtilisateur, u.Email, a.Fonction, r.Nom AS RoleActuel
FROM Utilisateurs u
INNER JOIN Agents a ON a.IdAgent = u.IdAgent
INNER JOIN UserRoles ur ON ur.IdUtilisateur = u.IdUtilisateur AND ur.Statut = 1
INNER JOIN Roles r ON r.IdRole = ur.IdRole
WHERE u.Statut = 1
  AND LOWER(TRIM(a.Fonction)) IN ('caissier', 'caissière', 'caissiere')
  AND r.Nom = 'Financier'
  AND NOT EXISTS (
    SELECT 1 FROM UserRoles ur2
    INNER JOIN Roles r2 ON r2.IdRole = ur2.IdRole
    WHERE ur2.IdUtilisateur = u.IdUtilisateur
      AND ur2.Statut = 1
      AND r2.Nom = 'Caissier'
  );
