-- Application manuelle de la migration EF :
-- 20260925080000_AddHoraireDeCours
--
-- Prérequis :
--   * se connecter à la base de données de l'application ;
--   * sauvegarder la base avant exécution ;
--   * exécuter les migrations précédentes avant ce script.
--
-- Le script est conçu pour être rejouable. Les DDL MySQL peuvent provoquer
-- un commit implicite : la transaction protège les écritures RBAC et l'historique
-- EF, mais ne transforme pas CREATE TABLE en opération transactionnelle.

DELIMITER $$

DROP PROCEDURE IF EXISTS ApplyHoraireDeCoursMigration $$

CREATE PROCEDURE ApplyHoraireDeCoursMigration()
BEGIN
    DECLARE missing_dependencies INT DEFAULT 0;

    SELECT COUNT(*)
      INTO missing_dependencies
      FROM information_schema.tables
     WHERE table_schema = DATABASE()
       AND table_name IN (
           'Agents',
           'AnneeScolaires',
           'Classes',
           'Cours',
           'Permissions',
           'Roles',
           'RolePermissions',
           '__EFMigrationsHistory'
       );

    IF missing_dependencies <> 8 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Des tables prérequises sont absentes. Appliquer les migrations précédentes avant ce script.';
    END IF;

    CREATE TABLE IF NOT EXISTS `HorairesDeCours` (
        `IdHoraireDeCours` int NOT NULL AUTO_INCREMENT,
        `Jour` int NOT NULL,
        `HeureDebut` time(6) NOT NULL,
        `HeureFin` time(6) NOT NULL,
        `IdCours` int NOT NULL,
        `IdClasse` int NOT NULL,
        `IdAgent` int NOT NULL,
        `IdAnneeScolaire` int NOT NULL,
        `Salle` varchar(150) CHARACTER SET utf8mb4 NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        CONSTRAINT `PK_HorairesDeCours`
            PRIMARY KEY (`IdHoraireDeCours`),
        CONSTRAINT `FK_HorairesDeCours_Agents_IdAgent`
            FOREIGN KEY (`IdAgent`) REFERENCES `Agents` (`IdAgent`)
            ON DELETE RESTRICT,
        CONSTRAINT `FK_HorairesDeCours_AnneeScolaires_IdAnneeScolaire`
            FOREIGN KEY (`IdAnneeScolaire`) REFERENCES `AnneeScolaires` (`IdAnneeScolaire`)
            ON DELETE RESTRICT,
        CONSTRAINT `FK_HorairesDeCours_Classes_IdClasse`
            FOREIGN KEY (`IdClasse`) REFERENCES `Classes` (`IdClasse`)
            ON DELETE RESTRICT,
        CONSTRAINT `FK_HorairesDeCours_Cours_IdCours`
            FOREIGN KEY (`IdCours`) REFERENCES `Cours` (`IdCours`)
            ON DELETE RESTRICT,
        KEY `IX_HorairesDeCours_IdAgent_IdAnneeScolaire_Jour_HeureDebut`
            (`IdAgent`, `IdAnneeScolaire`, `Jour`, `HeureDebut`),
        KEY `IX_HorairesDeCours_IdClasse_IdAnneeScolaire_Jour_HeureDebut`
            (`IdClasse`, `IdAnneeScolaire`, `Jour`, `HeureDebut`),
        KEY `IX_HorairesDeCours_IdCours` (`IdCours`)
    ) CHARACTER SET utf8mb4;

    START TRANSACTION;

    INSERT INTO `Permissions`
        (`Nom`, `Description`, `Categorie`, `Action`, `Statut`, `DateCreation`)
    SELECT `v`.`Nom`, `v`.`Description`, 'HoraireDeCours', `v`.`Action`, 1, NOW(6)
      FROM (
          SELECT 'HoraireDeCours.Read' AS `Nom`,
                 'Consulter les horaires de cours.' AS `Description`,
                 'Read' AS `Action`
          UNION ALL
          SELECT 'HoraireDeCours.Create',
                 'Créer un horaire de cours.',
                 'Create'
          UNION ALL
          SELECT 'HoraireDeCours.Update',
                 'Modifier un horaire de cours.',
                 'Update'
          UNION ALL
          SELECT 'HoraireDeCours.Delete',
                 'Supprimer un horaire de cours.',
                 'Delete'
      ) AS `v`
     WHERE NOT EXISTS (
         SELECT 1
           FROM `Permissions` AS `p`
          WHERE `p`.`Nom` = `v`.`Nom`
     );

    INSERT INTO `RolePermissions`
        (`IdRole`, `IdPermission`, `DateAttribution`, `IdUtilisateurAttribution`)
    SELECT `r`.`IdRole`, `p`.`IdPermission`, NOW(6), NULL
      FROM `Roles` AS `r`
      CROSS JOIN `Permissions` AS `p`
     WHERE p.`Categorie` = 'HoraireDeCours'
       AND (
           r.`Nom` IN ('Super-Admin', 'Admin', 'Directeur')
           OR (
               r.`Nom` IN ('Enseignant', 'Eleve', 'Tuteur')
               AND p.`Action` = 'Read'
           )
       )
       AND NOT EXISTS (
           SELECT 1
             FROM `RolePermissions` AS `rp`
            WHERE `rp`.`IdRole` = `r`.`IdRole`
              AND `rp`.`IdPermission` = `p`.`IdPermission`
       );

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    SELECT '20260925080000_AddHoraireDeCours', '6.0.36'
     WHERE NOT EXISTS (
         SELECT 1
           FROM `__EFMigrationsHistory`
          WHERE `MigrationId` = '20260925080000_AddHoraireDeCours'
     );

    COMMIT;
END $$

CALL ApplyHoraireDeCoursMigration() $$

DROP PROCEDURE ApplyHoraireDeCoursMigration $$

DELIMITER ;

-- Contrôles post-exécution.
SELECT `MigrationId`, `ProductVersion`
  FROM `__EFMigrationsHistory`
 WHERE `MigrationId` = '20260925080000_AddHoraireDeCours';

SELECT `Nom`, `Categorie`, `Action`
  FROM `Permissions`
 WHERE `Categorie` = 'HoraireDeCours'
 ORDER BY `Action`;

SELECT `r`.`Nom` AS `Role`, `p`.`Nom` AS `Permission`
  FROM `RolePermissions` AS `rp`
  INNER JOIN `Roles` AS `r` ON `r`.`IdRole` = `rp`.`IdRole`
  INNER JOIN `Permissions` AS `p` ON `p`.`IdPermission` = `rp`.`IdPermission`
 WHERE `p`.`Categorie` = 'HoraireDeCours'
 ORDER BY `r`.`Nom`, `p`.`Action`;
