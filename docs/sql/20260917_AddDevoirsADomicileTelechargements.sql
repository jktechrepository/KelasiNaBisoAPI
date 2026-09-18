-- =============================================================================
-- KelasiNaBiso — Suivi téléchargements devoirs (par utilisateur)
-- Fichier : docs/sql/20260917_AddDevoirsADomicileTelechargements.sql
-- SGBD    : MySQL 8 / MariaDB 10.11+
-- =============================================================================
-- Unique (IdDevoirADomicile, IdUtilisateur) = premier téléchargement.
-- nombreTelechargements (colonne DevoirsADomicile) reste le compteur GLOBAL.
-- =============================================================================

SET NAMES utf8mb4;

CREATE TABLE IF NOT EXISTS `DevoirsADomicileTelechargements` (
  `IdDevoirADomicileTelechargement` int NOT NULL AUTO_INCREMENT,
  `IdDevoirADomicile` int NOT NULL,
  `IdUtilisateur` int NOT NULL,
  `DateTelechargement` datetime(6) NOT NULL,
  PRIMARY KEY (`IdDevoirADomicileTelechargement`),
  UNIQUE KEY `IX_DevoirADomicileTelechargement_Devoir_Utilisateur`
    (`IdDevoirADomicile`, `IdUtilisateur`),
  KEY `IX_DevoirADomicileTelechargement_IdDevoir` (`IdDevoirADomicile`),
  KEY `IX_DevoirsADomicileTelechargements_IdUtilisateur` (`IdUtilisateur`),
  CONSTRAINT `FK_DevoirTelechargements_Devoir`
    FOREIGN KEY (`IdDevoirADomicile`) REFERENCES `DevoirsADomicile` (`IdDevoirADomicile`)
    ON DELETE CASCADE,
  CONSTRAINT `FK_DevoirTelechargements_Utilisateur`
    FOREIGN KEY (`IdUtilisateur`) REFERENCES `Utilisateurs` (`IdUtilisateur`)
    ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Si la table existait déjà sans UNIQUE (ancienne migration), tenter l'ajout :
-- (ignorer l'erreur si l'index existe déjà)
-- ALTER TABLE `DevoirsADomicileTelechargements`
--   ADD UNIQUE KEY `IX_DevoirADomicileTelechargement_Devoir_Utilisateur`
--   (`IdDevoirADomicile`, `IdUtilisateur`);

SELECT COUNT(*) AS NbLignesSuivi FROM `DevoirsADomicileTelechargements`;
