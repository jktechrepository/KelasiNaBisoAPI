-- Exonération / allègement de frais par catégorie d'élève
-- Fichier : docs/sql/20260922_AddEleveTarifExoneration.sql

CREATE TABLE IF NOT EXISTS `CategoriesEleveTarif` (
  `IdCategorieEleveTarif` int NOT NULL AUTO_INCREMENT,
  `IdEcole` int NOT NULL,
  `Code` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
  `Libelle` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
  `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
  `Statut` tinyint(1) NOT NULL,
  `DateCreation` datetime(6) NOT NULL,
  PRIMARY KEY (`IdCategorieEleveTarif`),
  UNIQUE KEY `UX_CategoriesEleveTarif_IdEcole_Code` (`IdEcole`, `Code`)
) CHARACTER SET utf8mb4;

CREATE TABLE IF NOT EXISTS `AffectationsEleveCategorieTarif` (
  `IdAffectationEleveCategorieTarif` int NOT NULL AUTO_INCREMENT,
  `IdEleve` int NOT NULL,
  `IdCategorieEleveTarif` int NOT NULL,
  `IdAnneeScolaire` int NOT NULL,
  `DateDebut` datetime(6) NOT NULL,
  `DateFin` datetime(6) NULL,
  `Motif` varchar(500) CHARACTER SET utf8mb4 NULL,
  `IdAuteur` int NULL,
  `DateCreation` datetime(6) NOT NULL,
  PRIMARY KEY (`IdAffectationEleveCategorieTarif`),
  KEY `IX_AffectationsEleveCategorieTarif_Eleve_Annee` (`IdEleve`, `IdAnneeScolaire`)
) CHARACTER SET utf8mb4;

CREATE TABLE IF NOT EXISTS `ReglesExonerationFrais` (
  `IdRegleExonerationFrais` int NOT NULL AUTO_INCREMENT,
  `IdEcole` int NOT NULL,
  `IdAnneeScolaire` int NOT NULL,
  `IdCategorieEleveTarif` int NOT NULL,
  `IdFrais` int NOT NULL,
  `TypeRegle` varchar(30) CHARACTER SET utf8mb4 NOT NULL,
  `Valeur` decimal(18,2) NOT NULL,
  `Statut` tinyint(1) NOT NULL,
  `IdAuteur` int NULL,
  `DateCreation` datetime(6) NOT NULL,
  `DateModification` datetime(6) NULL,
  PRIMARY KEY (`IdRegleExonerationFrais`),
  UNIQUE KEY `UX_ReglesExonerationFrais_Annee_Categorie_Frais` (`IdAnneeScolaire`, `IdCategorieEleveTarif`, `IdFrais`)
) CHARACTER SET utf8mb4;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
SELECT '20260922160000_AddEleveTarifExoneration', '6.0.36'
FROM DUAL
WHERE NOT EXISTS (
  SELECT 1 FROM `__EFMigrationsHistory`
  WHERE `MigrationId` = '20260922160000_AddEleveTarifExoneration'
);
