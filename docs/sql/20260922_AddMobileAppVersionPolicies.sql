-- Policy versions app mobile + AppVersion sur UserDevices
-- Fichier : docs/sql/20260922_AddMobileAppVersionPolicies.sql

CREATE TABLE IF NOT EXISTS `MobileAppVersionPolicies` (
  `IdMobileAppVersionPolicy` int NOT NULL AUTO_INCREMENT,
  `Platform` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
  `MinSupportedVersion` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
  `LatestVersion` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
  `RecommendFromVersion` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
  `Message` varchar(500) CHARACTER SET utf8mb4 NULL,
  `StoreUrl` varchar(500) CHARACTER SET utf8mb4 NULL,
  `IsActive` tinyint(1) NOT NULL,
  `DateCreation` datetime(6) NOT NULL,
  `DateModification` datetime(6) NULL,
  `IdAuteur` int NULL,
  PRIMARY KEY (`IdMobileAppVersionPolicy`),
  UNIQUE KEY `UX_MobileAppVersionPolicies_Platform` (`Platform`)
) CHARACTER SET utf8mb4;

INSERT INTO `MobileAppVersionPolicies`
(`Platform`, `MinSupportedVersion`, `LatestVersion`, `RecommendFromVersion`, `Message`, `StoreUrl`, `IsActive`, `DateCreation`, `DateModification`, `IdAuteur`)
SELECT 'Android', '1.0.0', '1.0.0', '1.0.0', 'Une nouvelle version est disponible.', NULL, 0, UTC_TIMESTAMP(6), NULL, NULL
FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM `MobileAppVersionPolicies` WHERE `Platform` = 'Android');

INSERT INTO `MobileAppVersionPolicies`
(`Platform`, `MinSupportedVersion`, `LatestVersion`, `RecommendFromVersion`, `Message`, `StoreUrl`, `IsActive`, `DateCreation`, `DateModification`, `IdAuteur`)
SELECT 'iOS', '1.0.0', '1.0.0', '1.0.0', 'Une nouvelle version est disponible.', NULL, 0, UTC_TIMESTAMP(6), NULL, NULL
FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM `MobileAppVersionPolicies` WHERE `Platform` = 'iOS');

ALTER TABLE `UserDevices`
  ADD COLUMN `AppVersion` varchar(20) CHARACTER SET utf8mb4 NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
SELECT '20260922150000_AddMobileAppVersionPolicies', '6.0.36'
FROM DUAL
WHERE NOT EXISTS (
  SELECT 1 FROM `__EFMigrationsHistory`
  WHERE `MigrationId` = '20260922150000_AddMobileAppVersionPolicies'
);
