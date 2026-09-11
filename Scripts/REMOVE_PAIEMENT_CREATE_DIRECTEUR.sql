-- ═══════════════════════════════════════════════════════════════════════════════
-- Retirer Paiement.Create (et cohérence Update/Delete) du rôle Directeur
-- ═══════════════════════════════════════════════════════════════════════════════
--
-- Objectif : le Directeur conserve la lecture et la validation des paiements,
--            mais ne peut plus créer, modifier ni supprimer de paiements.
--
-- Idempotent : peut être exécuté plusieurs fois sans effet secondaire.
-- Alternative : redémarrer l'API (PermissionSeeder.EnsureDirecteurPermissionsAsync).
--
-- ═══════════════════════════════════════════════════════════════════════════════

SELECT 'Vérification des rôles et permissions Paiement...' AS Info;

SELECT IdRole, Nom FROM Roles WHERE Nom = 'Directeur';

SELECT IdPermission, Nom FROM Permissions
WHERE Nom IN ('Paiement.Create', 'Paiement.Update', 'Paiement.Delete')
ORDER BY Nom;

-- ─────────────────────────────────────────────────────────────────────────────
-- Suppression des permissions interdites pour Directeur
-- ─────────────────────────────────────────────────────────────────────────────

DELETE rp FROM RolePermissions rp
INNER JOIN Roles r ON rp.IdRole = r.IdRole
INNER JOIN Permissions p ON rp.IdPermission = p.IdPermission
WHERE r.Nom = 'Directeur'
  AND p.Nom IN ('Paiement.Create', 'Paiement.Update', 'Paiement.Delete');

SELECT ROW_COUNT() AS PermissionsPaiementRetireesDuDirecteur;

-- ─────────────────────────────────────────────────────────────────────────────
-- Vérifications post-migration
-- ─────────────────────────────────────────────────────────────────────────────

SELECT
    CASE
        WHEN COUNT(*) = 0 THEN 'OK: Directeur n''a plus Paiement.Create/Update/Delete'
        ELSE CONCAT('ERREUR: Directeur a encore ', COUNT(*), ' permission(s) interdite(s)')
    END AS VerificationPaiementInterdites
FROM RolePermissions rp
INNER JOIN Roles r ON rp.IdRole = r.IdRole
INNER JOIN Permissions p ON rp.IdPermission = p.IdPermission
WHERE r.Nom = 'Directeur'
  AND p.Nom IN ('Paiement.Create', 'Paiement.Update', 'Paiement.Delete');

SELECT
    'Permissions Paiement du Directeur' AS Info,
    p.Nom AS Permission,
    p.Action AS Action
FROM RolePermissions rp
INNER JOIN Roles r ON rp.IdRole = r.IdRole
INNER JOIN Permissions p ON rp.IdPermission = p.IdPermission
WHERE r.Nom = 'Directeur'
  AND p.Categorie = 'Paiement'
ORDER BY p.Nom;
