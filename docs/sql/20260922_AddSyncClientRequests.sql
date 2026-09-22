-- Étape 0 offline sync : journal d'idempotence
-- Fichier : docs/sql/20260922_AddSyncClientRequests.sql

CREATE TABLE IF NOT EXISTS `SyncClientRequests` (
  `IdSyncClientRequest` bigint NOT NULL AUTO_INCREMENT,
  `IdEcole` int NOT NULL,
  `ClientRequestId` varchar(36) CHARACTER SET utf8mb4 NOT NULL,
  `ResourceType` varchar(40) CHARACTER SET utf8mb4 NOT NULL,
  `ResourceId` int NULL,
  `Status` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
  `Message` varchar(500) CHARACTER SET utf8mb4 NULL,
  `ErrorCode` varchar(80) CHARACTER SET utf8mb4 NULL,
  `ResultJson` text CHARACTER SET utf8mb4 NULL,
  `IdUtilisateur` int NULL,
  `DeviceId` varchar(100) CHARACTER SET utf8mb4 NULL,
  `DateCreation` datetime(6) NOT NULL,
  PRIMARY KEY (`IdSyncClientRequest`),
  UNIQUE KEY `UX_SyncClientRequests_IdEcole_ClientRequestId` (`IdEcole`, `ClientRequestId`),
  KEY `IX_SyncClientRequests_DateCreation` (`DateCreation`)
) CHARACTER SET utf8mb4;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
SELECT '20260922090000_AddSyncClientRequests', '6.0.36'
FROM DUAL
WHERE NOT EXISTS (
  SELECT 1 FROM `__EFMigrationsHistory`
  WHERE `MigrationId` = '20260922090000_AddSyncClientRequests'
);
