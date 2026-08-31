-- =============================================================================
-- KelasiNaBiso — Vérification post-déploiement rôle Controleur (knb_db)
-- Usage : exécuter après déploiement API + migration utilisateurs
-- =============================================================================

-- 1. Rôle Controleur existe
SELECT IdRole, Nom, Description, Niveau, Statut
FROM Roles
WHERE Nom = 'Controleur';

-- 2. Permissions assignées au Controleur (attendu : présence create/read, frais/paiement read)
SELECT p.Nom, p.Categorie, p.Action
FROM RolePermissions rp
INNER JOIN Roles r ON r.IdRole = rp.IdRole
INNER JOIN Permissions p ON p.IdPermission = rp.IdPermission
WHERE r.Nom = 'Controleur'
ORDER BY p.Categorie, p.Nom;

-- 3. Permissions interdites (ne doivent PAS apparaître)
SELECT p.Nom
FROM RolePermissions rp
INNER JOIN Roles r ON r.IdRole = rp.IdRole
INNER JOIN Permissions p ON p.IdPermission = rp.IdPermission
WHERE r.Nom = 'Controleur'
  AND p.Nom IN (
    'Paiement.Create', 'Frais.Create', 'Frais.Update', 'Frais.Delete',
    'Presence.Update', 'Presence.Delete'
  );

-- 4. Utilisateurs avec rôle Controleur actif
SELECT u.IdUtilisateur, u.Email, u.NomUtilisateur, a.Fonction, r.Nom AS Role
FROM UserRoles ur
INNER JOIN Utilisateurs u ON u.IdUtilisateur = ur.IdUtilisateur
INNER JOIN Roles r ON r.IdRole = ur.IdRole
LEFT JOIN Agents a ON a.IdAgent = u.IdAgent
WHERE ur.Statut = 1 AND r.Nom = 'Controleur'
ORDER BY u.IdUtilisateur;

-- 5. Agents contrôleur sans rôle Controleur (à migrer)
SELECT u.IdUtilisateur, u.Email, a.Fonction, r.Nom AS RoleActuel
FROM Utilisateurs u
INNER JOIN Agents a ON a.IdAgent = u.IdAgent
INNER JOIN UserRoles ur ON ur.IdUtilisateur = u.IdUtilisateur AND ur.Statut = 1
INNER JOIN Roles r ON r.IdRole = ur.IdRole
WHERE u.Statut = 1
  AND LOWER(TRIM(a.Fonction)) IN (
    'controlleur', 'contrôleur', 'controleur',
    'contrôleur des frais', 'controleur des frais',
    'contrôleur des entrées', 'controleur des entrees'
  )
  AND NOT EXISTS (
    SELECT 1 FROM UserRoles ur2
    INNER JOIN Roles r2 ON r2.IdRole = ur2.IdRole
    WHERE ur2.IdUtilisateur = u.IdUtilisateur
      AND ur2.Statut = 1
      AND r2.Nom = 'Controleur'
  );
