-- =============================================================================
-- KelasiNaBiso — Insertion manuelle d'un Super-Admin (MySQL)
-- =============================================================================
-- Aligné sur la logique applicative :
--   1) Rôle Super-Admin
--   2) École plateforme (Ekelasi School par défaut)
--   3) Agent « Manager Général » lié à l'école
--   4) Utilisateur lié à l'Agent
--   5) Entrée UserRoles (multi-rôles — obligatoire pour le JWT)
--
-- Connexion par défaut après exécution :
--   Identifiant : superadmin@kelasinabiso.cd  (ou DefaultUsername : SuperAdmin)
--   Mot de passe : Super-Admin
--
-- ⚠️ Changer le mot de passe immédiatement en production.
-- =============================================================================

-- Adapter la base selon l'environnement :
USE knb_db;
-- USE dev-knb_db;

-- ===================== PARAMÈTRES =====================
SET @email              := 'superadmin@kelasinabiso.cd';
SET @default_username   := 'SuperAdmin';
SET @telephone          := '+243999999999';
SET @nom_ecole          := 'Ekelasi School';
-- Hash BCrypt du mot de passe « Super-Admin » (cost 11)
SET @mot_de_passe_hash  := '$2a$11$hbjgXqTyFeirjjTLxA/UTOkBCtpQ04pJGUlpgv8K2Er8bo0OY5Dka';
-- ======================================================

START TRANSACTION;

-- 1) Rôle Super-Admin
INSERT INTO Roles (Nom, Description, Niveau, Statut, DateCreation)
SELECT 'Super-Admin', 'Administrateur plateforme KelasiNaBiso', 1, 1, UTC_TIMESTAMP(6)
WHERE NOT EXISTS (SELECT 1 FROM Roles WHERE Nom = 'Super-Admin');

SELECT IdRole INTO @id_role FROM Roles WHERE Nom = 'Super-Admin' LIMIT 1;

-- 2) École plateforme (doit être active pour la connexion)
INSERT INTO Ecoles (
    Nom, Slogan, Type, ProvinceEducationnel, NomCompletResponsable,
    Description, Statut, AcceptNotification, DateCreation
)
SELECT
    @nom_ecole,
    'Excellence et Innovation',
    'Privée',
    '1000',
    '50',
    'École plateforme KelasiNaBiso',
    1,
    1,
    UTC_TIMESTAMP(6)
WHERE NOT EXISTS (SELECT 1 FROM Ecoles WHERE Nom = @nom_ecole);

SELECT IdEcole INTO @id_ecole FROM Ecoles WHERE Nom = @nom_ecole LIMIT 1;

-- S'assurer que l'école est active
UPDATE Ecoles SET Statut = 1 WHERE IdEcole = @id_ecole AND (Statut IS NULL OR Statut = 0);

-- 3) Agent Manager Général
SELECT IdAgent INTO @id_agent
FROM Agents
WHERE IdEcole = @id_ecole AND Fonction = 'Manager Général'
LIMIT 1;

SET @matricule := CONCAT(
    'NAT',
    DATE_FORMAT(UTC_TIMESTAMP(), '%y'),
    '-',
    UPPER(LEFT(REPLACE(UUID(), '-', ''), 6))
);

INSERT INTO Agents (
    Matricule, Nom, Postnom, Prenom, Genre, DateNaissance,
    TelephoneAgent, EmailAgent, Statut, EtatCivil,
    Fonction, RoleAgent, IdEcole, DateCreation
)
SELECT
    @matricule,
    'Super',
    'Admin',
    'Administrateur',
    'Masculin',
    DATE_SUB(UTC_TIMESTAMP(), INTERVAL 40 YEAR),
    @telephone,
    @email,
    1,
    'Marié',
    'Manager Général',
    'Super-Administrateur',
    @id_ecole,
    UTC_TIMESTAMP(6)
WHERE @id_agent IS NULL;

SELECT IdAgent INTO @id_agent
FROM Agents
WHERE IdEcole = @id_ecole AND Fonction = 'Manager Général'
LIMIT 1;

-- 4) Utilisateur Super-Admin
SELECT IdUtilisateur INTO @id_user
FROM Utilisateurs
WHERE Email = @email
   OR DefaultUsername = @default_username
LIMIT 1;

INSERT INTO Utilisateurs (
    ReferenceUtilisateur,
    NomUtilisateur, PostNomUtilisateur, PrenomUtilisateur,
    Email, Telephone, DefaultUsername,
    MotDePasseHash, Genre, DateNaissance,
    Statut, IdRole, IdEcole, IdAgent,
    DoitChangerMotDePasse, IsConnecte, DateCreation
)
SELECT
    UUID(),
    'Super',
    'Admin',
    'Administrateur',
    @email,
    @telephone,
    @default_username,
    @mot_de_passe_hash,
    'Masculin',
    DATE_SUB(UTC_TIMESTAMP(), INTERVAL 40 YEAR),
    1,
    @id_role,
    @id_ecole,
    @id_agent,
    0,
    0,
    UTC_TIMESTAMP(6)
WHERE @id_user IS NULL;

SELECT IdUtilisateur INTO @id_user
FROM Utilisateurs
WHERE Email = @email
LIMIT 1;

-- Mettre à jour un compte existant (rôle + agent + école)
UPDATE Utilisateurs
SET
    IdRole = @id_role,
    IdEcole = @id_ecole,
    IdAgent = @id_agent,
    Statut = 1,
    MotDePasseHash = @mot_de_passe_hash
WHERE IdUtilisateur = @id_user;

-- 5) UserRoles (multi-rôles — requis pour ClaimTypes.Role dans le JWT)
INSERT INTO UserRoles (IdUtilisateur, IdRole, IsPrimary, DateAttribution, Statut)
SELECT @id_user, @id_role, 1, UTC_TIMESTAMP(6), 1
WHERE NOT EXISTS (
    SELECT 1 FROM UserRoles
    WHERE IdUtilisateur = @id_user AND IdRole = @id_role
);

-- Forcer le rôle principal
UPDATE UserRoles
SET IsPrimary = 1, Statut = 1
WHERE IdUtilisateur = @id_user AND IdRole = @id_role;

COMMIT;

-- ===================== VÉRIFICATION =====================
SELECT
    u.IdUtilisateur,
    u.Email,
    u.DefaultUsername,
    r.Nom AS RolePrincipal,
    e.Nom AS Ecole,
    a.IdAgent,
    a.Matricule,
    a.Fonction,
    ur.IsPrimary,
    ur.Statut AS UserRoleActif
FROM Utilisateurs u
JOIN Roles r ON r.IdRole = u.IdRole
LEFT JOIN Ecoles e ON e.IdEcole = u.IdEcole
LEFT JOIN Agents a ON a.IdAgent = u.IdAgent
LEFT JOIN UserRoles ur ON ur.IdUtilisateur = u.IdUtilisateur AND ur.IdRole = u.IdRole
WHERE u.IdUtilisateur = @id_user;
