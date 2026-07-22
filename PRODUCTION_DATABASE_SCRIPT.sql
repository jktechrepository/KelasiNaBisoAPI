CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    ALTER DATABASE CHARACTER SET utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE TABLE `Ecoles` (
        `IdEcole` int NOT NULL AUTO_INCREMENT,
        `Nom` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
        `Slogan` longtext CHARACTER SET utf8mb4 NULL,
        `Longitute` longtext CHARACTER SET utf8mb4 NULL,
        `Latitude` longtext CHARACTER SET utf8mb4 NULL,
        `Type` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Logo` longtext CHARACTER SET utf8mb4 NULL,
        `Telephone` longtext CHARACTER SET utf8mb4 NULL,
        `EmailContact` longtext CHARACTER SET utf8mb4 NULL,
        `SiteWeb` longtext CHARACTER SET utf8mb4 NULL,
        `ProvinceEducationnel` longtext CHARACTER SET utf8mb4 NULL,
        `NomCompletResponsable` longtext CHARACTER SET utf8mb4 NULL,
        `GenreResponsable` varchar(10) CHARACTER SET utf8mb4 NULL,
        `Description` longtext CHARACTER SET utf8mb4 NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        `Province` longtext CHARACTER SET utf8mb4 NULL,
        `Ville` longtext CHARACTER SET utf8mb4 NULL,
        `Commune` longtext CHARACTER SET utf8mb4 NULL,
        `Quartier` longtext CHARACTER SET utf8mb4 NULL,
        `Avenue` longtext CHARACTER SET utf8mb4 NULL,
        `Numero` longtext CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_Ecoles` PRIMARY KEY (`IdEcole`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE TABLE `Permissions` (
        `IdPermission` int NOT NULL AUTO_INCREMENT,
        `Nom` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Description` varchar(255) CHARACTER SET utf8mb4 NULL,
        `Categorie` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Action` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        CONSTRAINT `PK_Permissions` PRIMARY KEY (`IdPermission`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE TABLE `Roles` (
        `IdRole` int NOT NULL AUTO_INCREMENT,
        `Nom` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Description` varchar(255) CHARACTER SET utf8mb4 NULL,
        `Niveau` int NOT NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        CONSTRAINT `PK_Roles` PRIMARY KEY (`IdRole`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE TABLE `Agents` (
        `IdAgent` int NOT NULL AUTO_INCREMENT,
        `Matricule` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Nom` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Postnom` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Prenom` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Genre` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
        `DateNaissance` datetime(6) NOT NULL,
        `TelephoneAgent` longtext CHARACTER SET utf8mb4 NULL,
        `EmailAgent` varchar(255) CHARACTER SET utf8mb4 NULL,
        `Statut` tinyint(1) NOT NULL,
        `EtatCivil` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `SerialNumber` varchar(255) CHARACTER SET utf8mb4 NULL,
        `Fonction` longtext CHARACTER SET utf8mb4 NULL,
        `RoleAgent` longtext CHARACTER SET utf8mb4 NULL,
        `PhotoUrl` longtext CHARACTER SET utf8mb4 NULL,
        `IdEcole` int NULL,
        `DateCreation` datetime(6) NOT NULL,
        `Province` longtext CHARACTER SET utf8mb4 NULL,
        `Ville` longtext CHARACTER SET utf8mb4 NULL,
        `Commune` longtext CHARACTER SET utf8mb4 NULL,
        `Quartier` longtext CHARACTER SET utf8mb4 NULL,
        `Avenue` longtext CHARACTER SET utf8mb4 NULL,
        `Numero` longtext CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_Agents` PRIMARY KEY (`IdAgent`),
        CONSTRAINT `FK_Agents_Ecoles_IdEcole` FOREIGN KEY (`IdEcole`) REFERENCES `Ecoles` (`IdEcole`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE TABLE `AnneeScolaires` (
        `IdAnneeScolaire` int NOT NULL AUTO_INCREMENT,
        `LibelleAnneeScolaire` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `DateDebut` datetime(6) NOT NULL,
        `DateFin` datetime(6) NOT NULL,
        `IdEcole` int NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        CONSTRAINT `PK_AnneeScolaires` PRIMARY KEY (`IdAnneeScolaire`),
        CONSTRAINT `FK_AnneeScolaires_Ecoles_IdEcole` FOREIGN KEY (`IdEcole`) REFERENCES `Ecoles` (`IdEcole`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE TABLE `Directions` (
        `IdDirection` int NOT NULL AUTO_INCREMENT,
        `NomDirection` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `IdEcole` int NULL,
        `NiveauEnseignement` varchar(20) CHARACTER SET utf8mb4 NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NULL,
        CONSTRAINT `PK_Directions` PRIMARY KEY (`IdDirection`),
        CONSTRAINT `FK_Directions_Ecoles_IdEcole` FOREIGN KEY (`IdEcole`) REFERENCES `Ecoles` (`IdEcole`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE TABLE `Sections` (
        `IdSection` int NOT NULL AUTO_INCREMENT,
        `NomSection` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `IdEcole` int NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        CONSTRAINT `PK_Sections` PRIMARY KEY (`IdSection`),
        CONSTRAINT `FK_Sections_Ecoles_IdEcole` FOREIGN KEY (`IdEcole`) REFERENCES `Ecoles` (`IdEcole`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE TABLE `Tuteurs` (
        `IdTuteur` int NOT NULL AUTO_INCREMENT,
        `NomComplet` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
        `Genre` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
        `Email` varchar(255) CHARACTER SET utf8mb4 NULL,
        `Telephone` longtext CHARACTER SET utf8mb4 NULL,
        `NomCompletRepresentant` longtext CHARACTER SET utf8mb4 NULL,
        `TelephoneRepresentant` longtext CHARACTER SET utf8mb4 NULL,
        `PhotoTuteurUrl` longtext CHARACTER SET utf8mb4 NULL,
        `PieceIdentiteTuteur` longtext CHARACTER SET utf8mb4 NULL,
        `IdEcole` int NULL,
        `Statut` tinyint(1) NOT NULL,
        `SerialNumber` longtext CHARACTER SET utf8mb4 NULL,
        `DateCreation` datetime(6) NULL,
        CONSTRAINT `PK_Tuteurs` PRIMARY KEY (`IdTuteur`),
        CONSTRAINT `FK_Tuteurs_Ecoles_IdEcole` FOREIGN KEY (`IdEcole`) REFERENCES `Ecoles` (`IdEcole`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE TABLE `Vacations` (
        `IdVacation` int NOT NULL AUTO_INCREMENT,
        `NomVacation` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `HeureDebut` time(6) NOT NULL,
        `HeureFin` time(6) NOT NULL,
        `HeureDebutPause` time(6) NULL,
        `HeureFinPause` time(6) NULL,
        `NombreJoursParSemaine` int NOT NULL,
        `IdEcole` int NOT NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        CONSTRAINT `PK_Vacations` PRIMARY KEY (`IdVacation`),
        CONSTRAINT `FK_Vacations_Ecoles_IdEcole` FOREIGN KEY (`IdEcole`) REFERENCES `Ecoles` (`IdEcole`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE TABLE `RolePermissions` (
        `IdRolePermission` int NOT NULL AUTO_INCREMENT,
        `IdRole` int NOT NULL,
        `IdPermission` int NOT NULL,
        `DateAttribution` datetime(6) NOT NULL,
        `IdUtilisateurAttribution` int NULL,
        CONSTRAINT `PK_RolePermissions` PRIMARY KEY (`IdRolePermission`),
        CONSTRAINT `FK_RolePermissions_Permissions_IdPermission` FOREIGN KEY (`IdPermission`) REFERENCES `Permissions` (`IdPermission`) ON DELETE CASCADE,
        CONSTRAINT `FK_RolePermissions_Roles_IdRole` FOREIGN KEY (`IdRole`) REFERENCES `Roles` (`IdRole`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE TABLE `Frais` (
        `IdFrais` int NOT NULL AUTO_INCREMENT,
        `LibelleFrais` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Montant` double NOT NULL,
        `Devise` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
        `TypeFrais` longtext CHARACTER SET utf8mb4 NULL,
        `Periodicite` longtext CHARACTER SET utf8mb4 NULL,
        `Description` longtext CHARACTER SET utf8mb4 NULL,
        `Statut` tinyint(1) NOT NULL,
        `IdDirection` int NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        CONSTRAINT `PK_Frais` PRIMARY KEY (`IdFrais`),
        CONSTRAINT `FK_Frais_Directions_IdDirection` FOREIGN KEY (`IdDirection`) REFERENCES `Directions` (`IdDirection`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE TABLE `Options` (
        `IdOption` int NOT NULL AUTO_INCREMENT,
        `NomOption` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `IdSection` int NOT NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        CONSTRAINT `PK_Options` PRIMARY KEY (`IdOption`),
        CONSTRAINT `FK_Options_Sections_IdSection` FOREIGN KEY (`IdSection`) REFERENCES `Sections` (`IdSection`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE TABLE `Utilisateurs` (
        `IdUtilisateur` int NOT NULL AUTO_INCREMENT,
        `ReferenceUtilisateur` char(36) COLLATE ascii_general_ci NOT NULL,
        `NomUtilisateur` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `PostNomUtilisateur` longtext CHARACTER SET utf8mb4 NULL,
        `PrenomUtilisateur` longtext CHARACTER SET utf8mb4 NULL,
        `Email` varchar(255) CHARACTER SET utf8mb4 NULL,
        `Telephone` longtext CHARACTER SET utf8mb4 NULL,
        `PhotoUrl` longtext CHARACTER SET utf8mb4 NULL,
        `LieuNaissance` longtext CHARACTER SET utf8mb4 NULL,
        `DateNaissance` datetime(6) NULL,
        `Genre` longtext CHARACTER SET utf8mb4 NULL,
        `MotDePasseHash` longtext CHARACTER SET utf8mb4 NOT NULL,
        `DefaultUsername` longtext CHARACTER SET utf8mb4 NULL,
        `DoitChangerMotDePasse` tinyint(1) NOT NULL,
        `Statut` tinyint(1) NOT NULL,
        `IdRole` int NOT NULL,
        `IdEcole` int NULL,
        `DateCreation` datetime(6) NOT NULL,
        `IsConnecte` tinyint(1) NOT NULL,
        `IdAgent` int NULL,
        `IdTuteur` int NULL,
        `Province` longtext CHARACTER SET utf8mb4 NULL,
        `Ville` longtext CHARACTER SET utf8mb4 NULL,
        `Commune` longtext CHARACTER SET utf8mb4 NULL,
        `Quartier` longtext CHARACTER SET utf8mb4 NULL,
        `Avenue` longtext CHARACTER SET utf8mb4 NULL,
        `Numero` longtext CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_Utilisateurs` PRIMARY KEY (`IdUtilisateur`),
        CONSTRAINT `FK_Utilisateurs_Agents_IdAgent` FOREIGN KEY (`IdAgent`) REFERENCES `Agents` (`IdAgent`),
        CONSTRAINT `FK_Utilisateurs_Ecoles_IdEcole` FOREIGN KEY (`IdEcole`) REFERENCES `Ecoles` (`IdEcole`) ON DELETE CASCADE,
        CONSTRAINT `FK_Utilisateurs_Roles_IdRole` FOREIGN KEY (`IdRole`) REFERENCES `Roles` (`IdRole`) ON DELETE CASCADE,
        CONSTRAINT `FK_Utilisateurs_Tuteurs_IdTuteur` FOREIGN KEY (`IdTuteur`) REFERENCES `Tuteurs` (`IdTuteur`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE TABLE `Classes` (
        `IdClasse` int NOT NULL AUTO_INCREMENT,
        `NomClasse` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `IdDirection` int NULL,
        `IdSection` int NULL,
        `IdOption` int NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NULL,
        CONSTRAINT `PK_Classes` PRIMARY KEY (`IdClasse`),
        CONSTRAINT `FK_Classes_Directions_IdDirection` FOREIGN KEY (`IdDirection`) REFERENCES `Directions` (`IdDirection`) ON DELETE RESTRICT,
        CONSTRAINT `FK_Classes_Options_IdOption` FOREIGN KEY (`IdOption`) REFERENCES `Options` (`IdOption`) ON DELETE RESTRICT,
        CONSTRAINT `FK_Classes_Sections_IdSection` FOREIGN KEY (`IdSection`) REFERENCES `Sections` (`IdSection`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE TABLE `GroupeMessages` (
        `IdGroupe` int NOT NULL AUTO_INCREMENT,
        `NomGroupe` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `CreePar` int NULL,
        `IdEcole` int NOT NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        CONSTRAINT `PK_GroupeMessages` PRIMARY KEY (`IdGroupe`),
        CONSTRAINT `FK_GroupeMessages_Ecoles_IdEcole` FOREIGN KEY (`IdEcole`) REFERENCES `Ecoles` (`IdEcole`) ON DELETE CASCADE,
        CONSTRAINT `FK_GroupeMessages_Utilisateurs_CreePar` FOREIGN KEY (`CreePar`) REFERENCES `Utilisateurs` (`IdUtilisateur`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE TABLE `SmsLogs` (
        `IdSmsLog` int NOT NULL AUTO_INCREMENT,
        `NumeroDestinataire` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `IdUtilisateur` int NULL,
        `Message` varchar(1600) CHARACTER SET utf8mb4 NOT NULL,
        `TypeNotification` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Statut` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `MessageSid` varchar(100) CHARACTER SET utf8mb4 NULL,
        `MessageErreur` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CodeErreur` int NULL,
        `CoutUsd` double NOT NULL,
        `CoutFc` double NOT NULL,
        `DateEnvoi` datetime(6) NOT NULL,
        `DateLivraison` datetime(6) NULL,
        `DateEchec` datetime(6) NULL,
        `NombreSegments` int NOT NULL,
        `Direction` varchar(10) CHARACTER SET utf8mb4 NULL,
        `NumeroExpediteur` varchar(20) CHARACTER SET utf8mb4 NULL,
        `UtilisateurIdUtilisateur` int NULL,
        CONSTRAINT `PK_SmsLogs` PRIMARY KEY (`IdSmsLog`),
        CONSTRAINT `FK_SmsLogs_Utilisateurs_UtilisateurIdUtilisateur` FOREIGN KEY (`UtilisateurIdUtilisateur`) REFERENCES `Utilisateurs` (`IdUtilisateur`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE TABLE `UserDevices` (
        `IdUserDevice` int NOT NULL AUTO_INCREMENT,
        `IdUtilisateur` int NOT NULL,
        `FcmToken` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `DeviceType` varchar(100) CHARACTER SET utf8mb4 NULL,
        `DeviceModel` varchar(100) CHARACTER SET utf8mb4 NULL,
        `OsVersion` varchar(50) CHARACTER SET utf8mb4 NULL,
        `DefaultDevice` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateEnregistrement` datetime(6) NOT NULL,
        `DateDerniereUtilisation` datetime(6) NULL,
        CONSTRAINT `PK_UserDevices` PRIMARY KEY (`IdUserDevice`),
        CONSTRAINT `FK_UserDevices_Utilisateurs_IdUtilisateur` FOREIGN KEY (`IdUtilisateur`) REFERENCES `Utilisateurs` (`IdUtilisateur`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE TABLE `UserPermissions` (
        `IdUserPermission` int NOT NULL AUTO_INCREMENT,
        `IdUtilisateur` int NOT NULL,
        `IdPermission` int NOT NULL,
        `IsGranted` tinyint(1) NOT NULL,
        `DateAttribution` datetime(6) NOT NULL,
        `DateExpiration` datetime(6) NULL,
        `Commentaire` varchar(500) CHARACTER SET utf8mb4 NULL,
        `AttribueParIdUtilisateur` int NULL,
        CONSTRAINT `PK_UserPermissions` PRIMARY KEY (`IdUserPermission`),
        CONSTRAINT `FK_UserPermissions_Permissions_IdPermission` FOREIGN KEY (`IdPermission`) REFERENCES `Permissions` (`IdPermission`) ON DELETE CASCADE,
        CONSTRAINT `FK_UserPermissions_Utilisateurs_AttribueParIdUtilisateur` FOREIGN KEY (`AttribueParIdUtilisateur`) REFERENCES `Utilisateurs` (`IdUtilisateur`),
        CONSTRAINT `FK_UserPermissions_Utilisateurs_IdUtilisateur` FOREIGN KEY (`IdUtilisateur`) REFERENCES `Utilisateurs` (`IdUtilisateur`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE TABLE `Cours` (
        `IdCours` int NOT NULL AUTO_INCREMENT,
        `NomCours` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Description` longtext CHARACTER SET utf8mb4 NULL,
        `Ponderation` int NULL,
        `IdClasse` int NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NULL,
        CONSTRAINT `PK_Cours` PRIMARY KEY (`IdCours`),
        CONSTRAINT `FK_Cours_Classes_IdClasse` FOREIGN KEY (`IdClasse`) REFERENCES `Classes` (`IdClasse`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE TABLE `Eleves` (
        `IdEleve` int NOT NULL AUTO_INCREMENT,
        `ReferenceEleve` char(36) COLLATE ascii_general_ci NOT NULL,
        `Matricule` varchar(255) CHARACTER SET utf8mb4 NULL,
        `Nom` longtext CHARACTER SET utf8mb4 NULL,
        `Postnom` longtext CHARACTER SET utf8mb4 NULL,
        `Prenom` longtext CHARACTER SET utf8mb4 NULL,
        `NomComplet` longtext CHARACTER SET utf8mb4 NULL,
        `Genre` longtext CHARACTER SET utf8mb4 NULL,
        `DateNaissance` datetime(6) NOT NULL,
        `LieuNaissance` longtext CHARACTER SET utf8mb4 NULL,
        `PhotoUrl` longtext CHARACTER SET utf8mb4 NULL,
        `Nationalite` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Commentaire` longtext CHARACTER SET utf8mb4 NULL,
        `IdClasse` int NULL,
        `IdTuteur` int NULL,
        `Statut` tinyint(1) NOT NULL,
        `SerialNumber` varchar(255) CHARACTER SET utf8mb4 NULL,
        `DateCreation` datetime(6) NOT NULL,
        `Province` longtext CHARACTER SET utf8mb4 NULL,
        `Ville` longtext CHARACTER SET utf8mb4 NULL,
        `Commune` longtext CHARACTER SET utf8mb4 NULL,
        `Quartier` longtext CHARACTER SET utf8mb4 NULL,
        `Avenue` longtext CHARACTER SET utf8mb4 NULL,
        `Numero` longtext CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_Eleves` PRIMARY KEY (`IdEleve`),
        CONSTRAINT `FK_Eleves_Classes_IdClasse` FOREIGN KEY (`IdClasse`) REFERENCES `Classes` (`IdClasse`),
        CONSTRAINT `FK_Eleves_Tuteurs_IdTuteur` FOREIGN KEY (`IdTuteur`) REFERENCES `Tuteurs` (`IdTuteur`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE TABLE `Horaires` (
        `IdHoraire` int NOT NULL AUTO_INCREMENT,
        `Vacation` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `HeureDebut` time(6) NOT NULL,
        `HeureFin` time(6) NOT NULL,
        `HeureDebutPause` time(6) NULL,
        `HeureFinPause` time(6) NULL,
        `IdClasse` int NOT NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        CONSTRAINT `PK_Horaires` PRIMARY KEY (`IdHoraire`),
        CONSTRAINT `FK_Horaires_Classes_IdClasse` FOREIGN KEY (`IdClasse`) REFERENCES `Classes` (`IdClasse`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE TABLE `TitulairesClasses` (
        `IdTitulaireClasse` int NOT NULL AUTO_INCREMENT,
        `IdAgent` int NOT NULL,
        `IdClasse` int NOT NULL,
        `IdAnneeScolaire` int NOT NULL,
        `DateDebut` datetime(6) NOT NULL,
        `DateFin` datetime(6) NULL,
        `Statut` tinyint(1) NOT NULL,
        `Commentaire` varchar(500) CHARACTER SET utf8mb4 NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateModification` datetime(6) NULL,
        CONSTRAINT `PK_TitulairesClasses` PRIMARY KEY (`IdTitulaireClasse`),
        CONSTRAINT `FK_TitulairesClasses_Agents_IdAgent` FOREIGN KEY (`IdAgent`) REFERENCES `Agents` (`IdAgent`),
        CONSTRAINT `FK_TitulairesClasses_AnneeScolaires_IdAnneeScolaire` FOREIGN KEY (`IdAnneeScolaire`) REFERENCES `AnneeScolaires` (`IdAnneeScolaire`),
        CONSTRAINT `FK_TitulairesClasses_Classes_IdClasse` FOREIGN KEY (`IdClasse`) REFERENCES `Classes` (`IdClasse`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE TABLE `Messages` (
        `IdMessage` int NOT NULL AUTO_INCREMENT,
        `IdExpediteur` int NULL,
        `IdDestinateur` int NULL,
        `IdGroupe` int NULL,
        `ContenuMessage` varchar(1000) CHARACTER SET utf8mb4 NOT NULL,
        `FichierUrl` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateEnvoi` datetime(6) NOT NULL,
        CONSTRAINT `PK_Messages` PRIMARY KEY (`IdMessage`),
        CONSTRAINT `FK_Messages_GroupeMessages_IdGroupe` FOREIGN KEY (`IdGroupe`) REFERENCES `GroupeMessages` (`IdGroupe`) ON DELETE CASCADE,
        CONSTRAINT `FK_Messages_Utilisateurs_IdDestinateur` FOREIGN KEY (`IdDestinateur`) REFERENCES `Utilisateurs` (`IdUtilisateur`),
        CONSTRAINT `FK_Messages_Utilisateurs_IdExpediteur` FOREIGN KEY (`IdExpediteur`) REFERENCES `Utilisateurs` (`IdUtilisateur`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE TABLE `AffectationsCours` (
        `IdAffectationCours` int NOT NULL AUTO_INCREMENT,
        `IdAgent` int NOT NULL,
        `IdCours` int NOT NULL,
        `IdAnneeScolaire` int NOT NULL,
        `DateAffectation` datetime(6) NOT NULL,
        `DateFinAffectation` datetime(6) NULL,
        `Statut` tinyint(1) NOT NULL,
        `Commentaire` varchar(500) CHARACTER SET utf8mb4 NULL,
        `DateCreation` datetime(6) NOT NULL,
        CONSTRAINT `PK_AffectationsCours` PRIMARY KEY (`IdAffectationCours`),
        CONSTRAINT `FK_AffectationsCours_Agents_IdAgent` FOREIGN KEY (`IdAgent`) REFERENCES `Agents` (`IdAgent`),
        CONSTRAINT `FK_AffectationsCours_AnneeScolaires_IdAnneeScolaire` FOREIGN KEY (`IdAnneeScolaire`) REFERENCES `AnneeScolaires` (`IdAnneeScolaire`),
        CONSTRAINT `FK_AffectationsCours_Cours_IdCours` FOREIGN KEY (`IdCours`) REFERENCES `Cours` (`IdCours`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE TABLE `Evaluations` (
        `IdEvaluation` int NOT NULL AUTO_INCREMENT,
        `TypeEvaluation` longtext CHARACTER SET utf8mb4 NULL,
        `Coefficient` double NOT NULL,
        `IdCours` int NOT NULL,
        `IdClasse` int NOT NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NULL,
        CONSTRAINT `PK_Evaluations` PRIMARY KEY (`IdEvaluation`),
        CONSTRAINT `FK_Evaluations_Classes_IdClasse` FOREIGN KEY (`IdClasse`) REFERENCES `Classes` (`IdClasse`),
        CONSTRAINT `FK_Evaluations_Cours_IdCours` FOREIGN KEY (`IdCours`) REFERENCES `Cours` (`IdCours`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE TABLE `RessourcePedagogiques` (
        `IdRessourcePedagogique` int NOT NULL AUTO_INCREMENT,
        `TitreRessourcePedagogique` longtext CHARACTER SET utf8mb4 NOT NULL,
        `FormatRessourcePedagogique` longtext CHARACTER SET utf8mb4 NOT NULL,
        `UrlRessourcePedagogique` longtext CHARACTER SET utf8mb4 NOT NULL,
        `IdCours` int NOT NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        CONSTRAINT `PK_RessourcePedagogiques` PRIMARY KEY (`IdRessourcePedagogique`),
        CONSTRAINT `FK_RessourcePedagogiques_Cours_IdCours` FOREIGN KEY (`IdCours`) REFERENCES `Cours` (`IdCours`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE TABLE `Documents` (
        `IdDocument` int NOT NULL AUTO_INCREMENT,
        `IdEleve` int NOT NULL,
        `TypeDocument` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `IdUtilisateur` int NOT NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        CONSTRAINT `PK_Documents` PRIMARY KEY (`IdDocument`),
        CONSTRAINT `FK_Documents_Eleves_IdEleve` FOREIGN KEY (`IdEleve`) REFERENCES `Eleves` (`IdEleve`),
        CONSTRAINT `FK_Documents_Utilisateurs_IdUtilisateur` FOREIGN KEY (`IdUtilisateur`) REFERENCES `Utilisateurs` (`IdUtilisateur`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE TABLE `Inscriptions` (
        `IdInscription` int NOT NULL AUTO_INCREMENT,
        `Type` longtext CHARACTER SET utf8mb4 NOT NULL,
        `IdEleve` int NOT NULL,
        `IdEcole` int NOT NULL,
        `IdClasse` int NOT NULL,
        `IdAnneeScolaire` int NOT NULL,
        `DateInscription` datetime(6) NOT NULL,
        `StatutInscription` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        CONSTRAINT `PK_Inscriptions` PRIMARY KEY (`IdInscription`),
        CONSTRAINT `FK_Inscriptions_AnneeScolaires_IdAnneeScolaire` FOREIGN KEY (`IdAnneeScolaire`) REFERENCES `AnneeScolaires` (`IdAnneeScolaire`),
        CONSTRAINT `FK_Inscriptions_Classes_IdClasse` FOREIGN KEY (`IdClasse`) REFERENCES `Classes` (`IdClasse`),
        CONSTRAINT `FK_Inscriptions_Ecoles_IdEcole` FOREIGN KEY (`IdEcole`) REFERENCES `Ecoles` (`IdEcole`),
        CONSTRAINT `FK_Inscriptions_Eleves_IdEleve` FOREIGN KEY (`IdEleve`) REFERENCES `Eleves` (`IdEleve`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE TABLE `Notes` (
        `IdNote` int NOT NULL AUTO_INCREMENT,
        `Session` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `NoteObtenue` double NOT NULL,
        `Appreciation` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `DateEvaluation` datetime(6) NOT NULL,
        `IdProfesseur` int NOT NULL,
        `IdEleve` int NOT NULL,
        `IdCours` int NOT NULL,
        `IdAnneeScolaire` int NOT NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NULL,
        CONSTRAINT `PK_Notes` PRIMARY KEY (`IdNote`),
        CONSTRAINT `FK_Notes_AnneeScolaires_IdAnneeScolaire` FOREIGN KEY (`IdAnneeScolaire`) REFERENCES `AnneeScolaires` (`IdAnneeScolaire`),
        CONSTRAINT `FK_Notes_Cours_IdCours` FOREIGN KEY (`IdCours`) REFERENCES `Cours` (`IdCours`),
        CONSTRAINT `FK_Notes_Eleves_IdEleve` FOREIGN KEY (`IdEleve`) REFERENCES `Eleves` (`IdEleve`),
        CONSTRAINT `FK_Notes_Utilisateurs_IdProfesseur` FOREIGN KEY (`IdProfesseur`) REFERENCES `Utilisateurs` (`IdUtilisateur`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE TABLE `Notifications` (
        `IdNotification` int NOT NULL AUTO_INCREMENT,
        `Titre` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Statut` tinyint(1) NOT NULL,
        `Contenu` varchar(1000) CHARACTER SET utf8mb4 NOT NULL,
        `TypeNotification` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `EstLue` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateLecture` datetime(6) NULL,
        `LienAction` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Icone` varchar(50) CHARACTER SET utf8mb4 NULL,
        `EstActive` tinyint(1) NOT NULL,
        `IdExpediteur` int NULL,
        `IdDestinataire` int NULL,
        `IdEcole` int NULL,
        `IdClasse` int NULL,
        `IdEleve` int NULL,
        `IdAgent` int NULL,
        `IdCours` int NULL,
        `IdAnneeScolaire` int NULL,
        `AgentIdAgent` int NULL,
        CONSTRAINT `PK_Notifications` PRIMARY KEY (`IdNotification`),
        CONSTRAINT `FK_Notifications_Agents_AgentIdAgent` FOREIGN KEY (`AgentIdAgent`) REFERENCES `Agents` (`IdAgent`),
        CONSTRAINT `FK_Notifications_AnneeScolaires_IdAnneeScolaire` FOREIGN KEY (`IdAnneeScolaire`) REFERENCES `AnneeScolaires` (`IdAnneeScolaire`),
        CONSTRAINT `FK_Notifications_Classes_IdClasse` FOREIGN KEY (`IdClasse`) REFERENCES `Classes` (`IdClasse`),
        CONSTRAINT `FK_Notifications_Cours_IdCours` FOREIGN KEY (`IdCours`) REFERENCES `Cours` (`IdCours`),
        CONSTRAINT `FK_Notifications_Ecoles_IdEcole` FOREIGN KEY (`IdEcole`) REFERENCES `Ecoles` (`IdEcole`),
        CONSTRAINT `FK_Notifications_Eleves_IdEleve` FOREIGN KEY (`IdEleve`) REFERENCES `Eleves` (`IdEleve`),
        CONSTRAINT `FK_Notifications_Utilisateurs_IdDestinataire` FOREIGN KEY (`IdDestinataire`) REFERENCES `Utilisateurs` (`IdUtilisateur`),
        CONSTRAINT `FK_Notifications_Utilisateurs_IdExpediteur` FOREIGN KEY (`IdExpediteur`) REFERENCES `Utilisateurs` (`IdUtilisateur`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE TABLE `Paiements` (
        `IdPaiement` int NOT NULL AUTO_INCREMENT,
        `DatePaiement` datetime(6) NOT NULL,
        `Montant` double NOT NULL,
        `Devise` longtext CHARACTER SET utf8mb4 NULL,
        `ModePaiement` longtext CHARACTER SET utf8mb4 NULL,
        `Statut` tinyint(1) NOT NULL,
        `StatutPaiement` longtext CHARACTER SET utf8mb4 NOT NULL,
        `ReferenceTransaction` longtext CHARACTER SET utf8mb4 NOT NULL,
        `JustificatifUrl` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Commentaire` longtext CHARACTER SET utf8mb4 NOT NULL,
        `DateEnregistrement` datetime(6) NOT NULL,
        `ReferencePaiemenet` longtext CHARACTER SET utf8mb4 NOT NULL,
        `IdFrais` int NULL,
        `IdEleve` int NULL,
        `IdUtilisateur` int NULL,
        `DateCreation` datetime(6) NOT NULL,
        CONSTRAINT `PK_Paiements` PRIMARY KEY (`IdPaiement`),
        CONSTRAINT `FK_Paiements_Eleves_IdEleve` FOREIGN KEY (`IdEleve`) REFERENCES `Eleves` (`IdEleve`),
        CONSTRAINT `FK_Paiements_Frais_IdFrais` FOREIGN KEY (`IdFrais`) REFERENCES `Frais` (`IdFrais`),
        CONSTRAINT `FK_Paiements_Utilisateurs_IdUtilisateur` FOREIGN KEY (`IdUtilisateur`) REFERENCES `Utilisateurs` (`IdUtilisateur`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE TABLE `Presences` (
        `IdPresence` int NOT NULL AUTO_INCREMENT,
        `IdEleve` int NULL,
        `IdAgent` int NULL,
        `Statut` tinyint(1) NOT NULL,
        `IsPresent` tinyint(1) NULL,
        `TypePresence` varchar(10) CHARACTER SET utf8mb4 NULL,
        `HeureArrivee` time(6) NOT NULL,
        `HeureDepart` time(6) NULL,
        `DateDuJour` datetime(6) NOT NULL,
        `Observation` varchar(500) CHARACTER SET utf8mb4 NULL,
        `Longitute` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Latitude` longtext CHARACTER SET utf8mb4 NOT NULL,
        `IdVacation` int NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        `HoraireIdHoraire` int NULL,
        CONSTRAINT `PK_Presences` PRIMARY KEY (`IdPresence`),
        CONSTRAINT `FK_Presences_Agents_IdAgent` FOREIGN KEY (`IdAgent`) REFERENCES `Agents` (`IdAgent`),
        CONSTRAINT `FK_Presences_Eleves_IdEleve` FOREIGN KEY (`IdEleve`) REFERENCES `Eleves` (`IdEleve`),
        CONSTRAINT `FK_Presences_Horaires_HoraireIdHoraire` FOREIGN KEY (`HoraireIdHoraire`) REFERENCES `Horaires` (`IdHoraire`),
        CONSTRAINT `FK_Presences_Vacations_IdVacation` FOREIGN KEY (`IdVacation`) REFERENCES `Vacations` (`IdVacation`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_AffectationsCours_IdAgent` ON `AffectationsCours` (`IdAgent`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_AffectationsCours_IdAnneeScolaire` ON `AffectationsCours` (`IdAnneeScolaire`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_AffectationsCours_IdCours` ON `AffectationsCours` (`IdCours`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Agents_Email_Unique` ON `Agents` (`EmailAgent`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Agents_IdEcole` ON `Agents` (`IdEcole`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Agents_Matricule_Unique` ON `Agents` (`Matricule`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Agents_SerialNumber_Unique` ON `Agents` (`SerialNumber`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_AnneeScolaires_IdEcole` ON `AnneeScolaires` (`IdEcole`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Classes_IdDirection` ON `Classes` (`IdDirection`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Classes_IdOption` ON `Classes` (`IdOption`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Classes_IdSection` ON `Classes` (`IdSection`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Cours_IdClasse` ON `Cours` (`IdClasse`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Directions_IdEcole` ON `Directions` (`IdEcole`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Directions_NomDirection_IdEcole` ON `Directions` (`NomDirection`, `IdEcole`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Documents_IdEleve` ON `Documents` (`IdEleve`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Documents_IdUtilisateur` ON `Documents` (`IdUtilisateur`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Eleves_IdClasse` ON `Eleves` (`IdClasse`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Eleves_IdTuteur` ON `Eleves` (`IdTuteur`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Eleves_Matricule_Unique` ON `Eleves` (`Matricule`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Eleves_SerialNumber_Unique` ON `Eleves` (`SerialNumber`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Evaluations_IdClasse` ON `Evaluations` (`IdClasse`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Evaluations_IdCours` ON `Evaluations` (`IdCours`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Frais_IdDirection` ON `Frais` (`IdDirection`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_GroupeMessages_CreePar` ON `GroupeMessages` (`CreePar`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_GroupeMessages_IdEcole` ON `GroupeMessages` (`IdEcole`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Horaires_IdClasse` ON `Horaires` (`IdClasse`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Inscriptions_IdAnneeScolaire` ON `Inscriptions` (`IdAnneeScolaire`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Inscriptions_IdClasse` ON `Inscriptions` (`IdClasse`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Inscriptions_IdEcole` ON `Inscriptions` (`IdEcole`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Inscriptions_IdEleve` ON `Inscriptions` (`IdEleve`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Messages_IdDestinateur` ON `Messages` (`IdDestinateur`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Messages_IdExpediteur` ON `Messages` (`IdExpediteur`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Messages_IdGroupe` ON `Messages` (`IdGroupe`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Notes_IdAnneeScolaire` ON `Notes` (`IdAnneeScolaire`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Notes_IdCours` ON `Notes` (`IdCours`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Notes_IdEleve` ON `Notes` (`IdEleve`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Notes_IdProfesseur` ON `Notes` (`IdProfesseur`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Notifications_AgentIdAgent` ON `Notifications` (`AgentIdAgent`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Notifications_IdAnneeScolaire` ON `Notifications` (`IdAnneeScolaire`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Notifications_IdClasse` ON `Notifications` (`IdClasse`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Notifications_IdCours` ON `Notifications` (`IdCours`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Notifications_IdDestinataire` ON `Notifications` (`IdDestinataire`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Notifications_IdEcole` ON `Notifications` (`IdEcole`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Notifications_IdEleve` ON `Notifications` (`IdEleve`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Notifications_IdExpediteur` ON `Notifications` (`IdExpediteur`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Options_IdSection` ON `Options` (`IdSection`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Paiements_IdEleve` ON `Paiements` (`IdEleve`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Paiements_IdFrais` ON `Paiements` (`IdFrais`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Paiements_IdUtilisateur` ON `Paiements` (`IdUtilisateur`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Presences_HoraireIdHoraire` ON `Presences` (`HoraireIdHoraire`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Presences_IdAgent` ON `Presences` (`IdAgent`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Presences_IdEleve` ON `Presences` (`IdEleve`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Presences_IdVacation` ON `Presences` (`IdVacation`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_RessourcePedagogiques_IdCours` ON `RessourcePedagogiques` (`IdCours`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_RolePermissions_IdPermission` ON `RolePermissions` (`IdPermission`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_RolePermissions_IdRole` ON `RolePermissions` (`IdRole`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Roles_Nom` ON `Roles` (`Nom`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Sections_IdEcole` ON `Sections` (`IdEcole`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_SmsLogs_UtilisateurIdUtilisateur` ON `SmsLogs` (`UtilisateurIdUtilisateur`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_TitulairesClasses_IdAgent` ON `TitulairesClasses` (`IdAgent`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_TitulairesClasses_IdAnneeScolaire` ON `TitulairesClasses` (`IdAnneeScolaire`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_TitulairesClasses_IdClasse_IdAnneeScolaire_Statut` ON `TitulairesClasses` (`IdClasse`, `IdAnneeScolaire`, `Statut`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Tuteurs_Email_Unique` ON `Tuteurs` (`Email`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Tuteurs_IdEcole` ON `Tuteurs` (`IdEcole`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_UserDevices_IdUtilisateur` ON `UserDevices` (`IdUtilisateur`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_UserPermissions_AttribueParIdUtilisateur` ON `UserPermissions` (`AttribueParIdUtilisateur`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_UserPermissions_IdPermission` ON `UserPermissions` (`IdPermission`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_UserPermissions_IdUtilisateur` ON `UserPermissions` (`IdUtilisateur`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Utilisateurs_Email_Unique` ON `Utilisateurs` (`Email`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Utilisateurs_IdAgent` ON `Utilisateurs` (`IdAgent`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Utilisateurs_IdEcole` ON `Utilisateurs` (`IdEcole`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Utilisateurs_IdRole` ON `Utilisateurs` (`IdRole`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Utilisateurs_IdTuteur` ON `Utilisateurs` (`IdTuteur`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    CREATE INDEX `IX_Vacations_IdEcole` ON `Vacations` (`IdEcole`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251028103016_InitialCreate') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251028103016_InitialCreate', '6.0.25');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251031052322_AddAcceptNotificationToEcole') THEN

    ALTER TABLE `Ecoles` ADD `AcceptNotification` tinyint(1) NOT NULL DEFAULT TRUE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251031052322_AddAcceptNotificationToEcole') THEN

    UPDATE Ecoles SET AcceptNotification = 1

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251031052322_AddAcceptNotificationToEcole') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251031052322_AddAcceptNotificationToEcole', '6.0.25');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251031054943_UpdateAcceptNotificationToTrue') THEN

    UPDATE Ecoles SET AcceptNotification = 1 WHERE AcceptNotification = 0

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251031054943_UpdateAcceptNotificationToTrue') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251031054943_UpdateAcceptNotificationToTrue', '6.0.25');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251101191812_AddAuditLogTable') THEN

    CREATE TABLE `AuditLogs` (
        `IdAudit` bigint NOT NULL AUTO_INCREMENT,
        `TableName` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `RecordId` int NOT NULL,
        `Action` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `UserId` int NOT NULL,
        `UserName` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `UserRole` varchar(50) CHARACTER SET utf8mb4 NULL,
        `IdEcole` int NULL,
        `DateAction` datetime(6) NOT NULL,
        `OldValues` TEXT CHARACTER SET utf8mb4 NULL,
        `NewValues` TEXT CHARACTER SET utf8mb4 NULL,
        `ChangedFields` varchar(500) CHARACTER SET utf8mb4 NULL,
        `IpAddress` varchar(50) CHARACTER SET utf8mb4 NULL,
        `UserAgent` varchar(500) CHARACTER SET utf8mb4 NULL,
        `Commentaire` TEXT CHARACTER SET utf8mb4 NULL,
        `HttpMethod` varchar(10) CHARACTER SET utf8mb4 NULL,
        `Endpoint` varchar(500) CHARACTER SET utf8mb4 NULL,
        `DurationMs` int NULL,
        `Success` tinyint(1) NOT NULL,
        `ErrorMessage` varchar(1000) CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_AuditLogs` PRIMARY KEY (`IdAudit`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251101191812_AddAuditLogTable') THEN

    CREATE INDEX `IX_AuditLog_Action` ON `AuditLogs` (`Action`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251101191812_AddAuditLogTable') THEN

    CREATE INDEX `IX_AuditLog_DateAction` ON `AuditLogs` (`DateAction`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251101191812_AddAuditLogTable') THEN

    CREATE INDEX `IX_AuditLog_IdEcole` ON `AuditLogs` (`IdEcole`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251101191812_AddAuditLogTable') THEN

    CREATE INDEX `IX_AuditLog_Table_Record` ON `AuditLogs` (`TableName`, `RecordId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251101191812_AddAuditLogTable') THEN

    CREATE INDEX `IX_AuditLog_UserId` ON `AuditLogs` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251101191812_AddAuditLogTable') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251101191812_AddAuditLogTable', '6.0.25');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

