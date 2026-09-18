-- =============================================================================
-- KelasiNaBiso — Backfill comptes Élève (toutes écoles) — phpMyAdmin-safe
-- Fichier : docs/sql/20260915_BackfillComptesEleve.sql
-- Date    : 2026-09-15
-- SGBD    : MySQL 8 / MariaDB 10.11+
-- =============================================================================
--
-- Prérequis :
--   1) Script schéma déjà appliqué : 20260915_AddIdEleve_And_RoleEleve.sql
--      (colonne utilisateurs.IdEleve + rôle 'Eleve')
--   2) Backup complet avant exécution
--
-- Comportement (aligné EleveCompteService) :
--   - Candidats : élèves actifs (Statut=1), matricule non vide,
--     avec au moins une inscription confirmée/active
--   - Login = matricule, MDP = 123456 (hash BCrypt fixe ci-dessous)
--   - DoitChangerMotDePasse = 1
--   - IdEcole = inscription confirmée la plus récente
--   - Si un utilisateur existe déjà avec DefaultUsername = matricule et IdEleve NULL
--     → liaison IdEleve uniquement (pas de reset MDP)
--   - Conflits username (matricule déjà lié à un autre IdEleve) → exclus
--
-- Hors scope : SMS tuteurs (utiliser l'API si besoin)
--
-- Tables : minuscules (convention serveur / MySQL macOS).
-- Si votre prod utilise Utilisateurs/Eleves/..., adapter les noms.
-- =============================================================================

SET NAMES utf8mb4;

-- Hash BCrypt.Net-Next de "123456" (cost 11) — vérifié Verify=true
SET @pwd_hash = '$2a$11$KtbR3z.jf7H724o9KaZfjuhm50aRDU6FhScJ.sQpxKPFlmbcUjGri';

SET @id_role_eleve = (
    SELECT `IdRole` FROM `Roles` WHERE `Nom` = 'Eleve' LIMIT 1
);

-- -----------------------------------------------------------------------------
-- 1. Diagnostic (lecture seule) — exécuter avant les écritures
-- -----------------------------------------------------------------------------

-- 1.a Déjà liés
SELECT COUNT(*) AS DejaLies
FROM `utilisateurs` u
WHERE u.`IdEleve` IS NOT NULL;

-- 1.b Candidats à créer (pas de compte IdEleve, pas de username conflictuel)
SELECT
    e.`IdEleve`,
    TRIM(e.`Matricule`) AS Matricule,
    e.`NomComplet`,
    (
        SELECT i.`IdEcole`
        FROM `inscriptions` i
        WHERE i.`IdEleve` = e.`IdEleve`
          AND (i.`Statut` = 1 OR i.`Statut` IS NULL)
          AND i.`StatutInscription` IS NOT NULL
          AND (
                i.`StatutInscription` = 'Confirmé'
             OR i.`StatutInscription` = 'Confirme'
             OR i.`StatutInscription` LIKE 'Confirm%'
          )
        ORDER BY i.`DateInscription` DESC, i.`IdInscription` DESC
        LIMIT 1
    ) AS IdEcoleProposee
FROM `eleves` e
WHERE e.`Statut` = 1
  AND TRIM(COALESCE(e.`Matricule`, '')) <> ''
  AND NOT EXISTS (
      SELECT 1 FROM `utilisateurs` u WHERE u.`IdEleve` = e.`IdEleve`
  )
  AND NOT EXISTS (
      SELECT 1 FROM `utilisateurs` u
      WHERE u.`DefaultUsername` = TRIM(e.`Matricule`)
  )
  AND EXISTS (
      SELECT 1 FROM `inscriptions` i
      WHERE i.`IdEleve` = e.`IdEleve`
        AND (i.`Statut` = 1 OR i.`Statut` IS NULL)
        AND i.`StatutInscription` IS NOT NULL
        AND (
              i.`StatutInscription` = 'Confirmé'
           OR i.`StatutInscription` = 'Confirme'
           OR i.`StatutInscription` LIKE 'Confirm%'
        )
  );

SELECT COUNT(*) AS NbACreer
FROM `eleves` e
WHERE e.`Statut` = 1
  AND TRIM(COALESCE(e.`Matricule`, '')) <> ''
  AND NOT EXISTS (SELECT 1 FROM `utilisateurs` u WHERE u.`IdEleve` = e.`IdEleve`)
  AND NOT EXISTS (
      SELECT 1 FROM `utilisateurs` u
      WHERE u.`DefaultUsername` = TRIM(e.`Matricule`)
  )
  AND EXISTS (
      SELECT 1 FROM `inscriptions` i
      WHERE i.`IdEleve` = e.`IdEleve`
        AND (i.`Statut` = 1 OR i.`Statut` IS NULL)
        AND i.`StatutInscription` IS NOT NULL
        AND (
              i.`StatutInscription` = 'Confirmé'
           OR i.`StatutInscription` = 'Confirme'
           OR i.`StatutInscription` LIKE 'Confirm%'
        )
  );

-- 1.c À lier (username = matricule, IdEleve NULL)
SELECT
    u.`IdUtilisateur`,
    u.`DefaultUsername`,
    e.`IdEleve`,
    e.`NomComplet`
FROM `utilisateurs` u
INNER JOIN `eleves` e
    ON TRIM(e.`Matricule`) = u.`DefaultUsername`
LEFT JOIN `utilisateurs` u2
    ON u2.`IdEleve` = e.`IdEleve`
WHERE u.`IdEleve` IS NULL
  AND u2.`IdUtilisateur` IS NULL
  AND e.`Statut` = 1
  AND TRIM(COALESCE(e.`Matricule`, '')) <> ''
  AND EXISTS (
      SELECT 1 FROM `inscriptions` i
      WHERE i.`IdEleve` = e.`IdEleve`
        AND (i.`Statut` = 1 OR i.`Statut` IS NULL)
        AND i.`StatutInscription` IS NOT NULL
        AND (
              i.`StatutInscription` = 'Confirmé'
           OR i.`StatutInscription` = 'Confirme'
           OR i.`StatutInscription` LIKE 'Confirm%'
        )
  );

-- 1.d Conflits username (à traiter manuellement)
SELECT
    e.`IdEleve` AS IdEleveCandidat,
    TRIM(e.`Matricule`) AS Matricule,
    u.`IdUtilisateur` AS IdUtilisateurExistant,
    u.`IdEleve` AS IdEleveDejaLie
FROM `eleves` e
INNER JOIN `utilisateurs` u
    ON u.`DefaultUsername` = TRIM(e.`Matricule`)
   AND u.`IdEleve` IS NOT NULL
   AND u.`IdEleve` <> e.`IdEleve`
WHERE e.`Statut` = 1
  AND TRIM(COALESCE(e.`Matricule`, '')) <> ''
  AND NOT EXISTS (SELECT 1 FROM `utilisateurs` ux WHERE ux.`IdEleve` = e.`IdEleve`);

-- -----------------------------------------------------------------------------
-- 2. Liaison comptes existants (DefaultUsername = matricule, IdEleve NULL)
-- -----------------------------------------------------------------------------

UPDATE `utilisateurs` u
INNER JOIN `eleves` e
    ON TRIM(e.`Matricule`) = u.`DefaultUsername`
LEFT JOIN `utilisateurs` u2
    ON u2.`IdEleve` = e.`IdEleve`
SET u.`IdEleve` = e.`IdEleve`,
    u.`IdRole` = COALESCE(u.`IdRole`, @id_role_eleve)
WHERE u.`IdEleve` IS NULL
  AND u2.`IdUtilisateur` IS NULL
  AND e.`Statut` = 1
  AND TRIM(COALESCE(e.`Matricule`, '')) <> ''
  AND @id_role_eleve IS NOT NULL
  AND EXISTS (
      SELECT 1 FROM `inscriptions` i
      WHERE i.`IdEleve` = e.`IdEleve`
        AND (i.`Statut` = 1 OR i.`Statut` IS NULL)
        AND i.`StatutInscription` IS NOT NULL
        AND (
              i.`StatutInscription` = 'Confirmé'
           OR i.`StatutInscription` = 'Confirme'
           OR i.`StatutInscription` LIKE 'Confirm%'
        )
  );

-- -----------------------------------------------------------------------------
-- 3. Création des comptes manquants
-- -----------------------------------------------------------------------------

INSERT INTO `utilisateurs` (
    `IdEleve`,
    `ReferenceUtilisateur`,
    `NomUtilisateur`,
    `PostNomUtilisateur`,
    `PrenomUtilisateur`,
    `Email`,
    `DefaultUsername`,
    `Telephone`,
    `Genre`,
    `DateNaissance`,
    `MotDePasseHash`,
    `Statut`,
    `DateCreation`,
    `IsConnecte`,
    `DoitChangerMotDePasse`,
    `IdRole`,
    `IdEcole`
)
SELECT
    e.`IdEleve`,
    UUID(),
    LEFT(COALESCE(NULLIF(TRIM(e.`NomComplet`), ''), TRIM(e.`Matricule`)), 100),
    COALESCE(e.`Postnom`, ''),
    COALESCE(e.`Prenom`, ''),
    NULL,
    TRIM(e.`Matricule`),
    '',
    e.`Genre`,
    e.`DateNaissance`,
    @pwd_hash,
    1,
    UTC_TIMESTAMP(6),
    0,
    1,
    @id_role_eleve,
    (
        SELECT i.`IdEcole`
        FROM `inscriptions` i
        WHERE i.`IdEleve` = e.`IdEleve`
          AND (i.`Statut` = 1 OR i.`Statut` IS NULL)
          AND i.`StatutInscription` IS NOT NULL
          AND (
                i.`StatutInscription` = 'Confirmé'
             OR i.`StatutInscription` = 'Confirme'
             OR i.`StatutInscription` LIKE 'Confirm%'
          )
        ORDER BY i.`DateInscription` DESC, i.`IdInscription` DESC
        LIMIT 1
    )
FROM `eleves` e
WHERE e.`Statut` = 1
  AND TRIM(COALESCE(e.`Matricule`, '')) <> ''
  AND @id_role_eleve IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM `utilisateurs` u WHERE u.`IdEleve` = e.`IdEleve`
  )
  AND NOT EXISTS (
      SELECT 1 FROM `utilisateurs` u
      WHERE u.`DefaultUsername` = TRIM(e.`Matricule`)
  )
  AND EXISTS (
      SELECT 1 FROM `inscriptions` i
      WHERE i.`IdEleve` = e.`IdEleve`
        AND (i.`Statut` = 1 OR i.`Statut` IS NULL)
        AND i.`StatutInscription` IS NOT NULL
        AND (
              i.`StatutInscription` = 'Confirmé'
           OR i.`StatutInscription` = 'Confirme'
           OR i.`StatutInscription` LIKE 'Confirm%'
        )
  );

-- -----------------------------------------------------------------------------
-- 4. Attribution rôle Eleve (UserRoles) — idempotent
-- -----------------------------------------------------------------------------

INSERT INTO `UserRoles` (
    `IdUtilisateur`,
    `IdRole`,
    `IsPrimary`,
    `Statut`,
    `DateAttribution`
)
SELECT
    u.`IdUtilisateur`,
    @id_role_eleve,
    1,
    1,
    UTC_TIMESTAMP(6)
FROM `utilisateurs` u
WHERE u.`IdEleve` IS NOT NULL
  AND @id_role_eleve IS NOT NULL
  AND NOT EXISTS (
      SELECT 1
      FROM `UserRoles` ur
      WHERE ur.`IdUtilisateur` = u.`IdUtilisateur`
        AND ur.`IdRole` = @id_role_eleve
  );

-- -----------------------------------------------------------------------------
-- 5. Vérifications
-- -----------------------------------------------------------------------------

SELECT COUNT(*) AS ComptesAvecIdEleve
FROM `utilisateurs`
WHERE `IdEleve` IS NOT NULL;

SELECT COUNT(*) AS ElevesSansCompteRestants
FROM `eleves` e
WHERE e.`Statut` = 1
  AND TRIM(COALESCE(e.`Matricule`, '')) <> ''
  AND NOT EXISTS (SELECT 1 FROM `utilisateurs` u WHERE u.`IdEleve` = e.`IdEleve`)
  AND EXISTS (
      SELECT 1 FROM `inscriptions` i
      WHERE i.`IdEleve` = e.`IdEleve`
        AND (i.`Statut` = 1 OR i.`Statut` IS NULL)
        AND i.`StatutInscription` IS NOT NULL
        AND (
              i.`StatutInscription` = 'Confirmé'
           OR i.`StatutInscription` = 'Confirme'
           OR i.`StatutInscription` LIKE 'Confirm%'
        )
  );

SELECT COUNT(*) AS ConflitsUsernameRestants
FROM `eleves` e
INNER JOIN `utilisateurs` u
    ON u.`DefaultUsername` = TRIM(e.`Matricule`)
   AND u.`IdEleve` IS NOT NULL
   AND u.`IdEleve` <> e.`IdEleve`
WHERE e.`Statut` = 1
  AND TRIM(COALESCE(e.`Matricule`, '')) <> ''
  AND NOT EXISTS (SELECT 1 FROM `utilisateurs` ux WHERE ux.`IdEleve` = e.`IdEleve`);

SELECT
    u.`IdUtilisateur`,
    u.`DefaultUsername`,
    u.`IdEleve`,
    u.`IdEcole`,
    u.`DoitChangerMotDePasse`,
    r.`Nom` AS Role
FROM `utilisateurs` u
LEFT JOIN `Roles` r ON r.`IdRole` = u.`IdRole`
WHERE u.`IdEleve` IS NOT NULL
ORDER BY u.`IdUtilisateur` DESC
LIMIT 20;

SELECT 'Backfill comptes Élève terminé — vérifier les comptes ci-dessus (login=matricule, MDP=123456)' AS Status;
