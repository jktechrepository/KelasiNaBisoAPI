-- =============================================================================
-- KelasiNaBiso — Vérification migration MOKO (à exécuter APRÈS migration)
-- ▼▼▼ Même base que migration-moko-afrika-manual.sql ▼▼▼
-- =============================================================================

SET @db = 'dev-knb_db';
-- SET @db = 'knb_db';

-- Tiret dans le nom → backticks obligatoires pour USE
USE `dev-knb_db`;
-- USE knb_db;

-- 0. Contexte : les deux colonnes doivent afficher dev-knb_db (ou knb_db si prod)
SELECT @db AS BaseCible, DATABASE() AS BaseCourantePhpMyAdmin;

SELECT TABLE_NAME, TABLE_ROWS
FROM information_schema.TABLES
WHERE TABLE_SCHEMA = @db
  AND TABLE_NAME IN (
    'EcolesInfoPaiementMobile',
    'EcolesBeneficiairesMomo',
    'EcolesWallets',
    'TransactionsMoko',
    'FilePayoutsMoko',
    'EcolesWalletMouvements'
  )
ORDER BY TABLE_NAME;

-- 1. Colonnes MOKO sur Paiements ? (attendu : 3 lignes)
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = @db
  AND TABLE_NAME = 'Paiements'
  AND COLUMN_NAME IN ('MontantNet', 'MontantCollecte', 'OperateurMobileMoney');

-- 2. Compteurs (attendu : 6 lignes — backticks sur le nom de base)
SELECT 'EcolesInfoPaiementMobile' AS TableName, COUNT(*) AS Nb FROM `dev-knb_db`.`EcolesInfoPaiementMobile`
UNION ALL SELECT 'EcolesBeneficiairesMomo', COUNT(*) FROM `dev-knb_db`.`EcolesBeneficiairesMomo`
UNION ALL SELECT 'EcolesWallets', COUNT(*) FROM `dev-knb_db`.`EcolesWallets`
UNION ALL SELECT 'TransactionsMoko', COUNT(*) FROM `dev-knb_db`.`TransactionsMoko`
UNION ALL SELECT 'FilePayoutsMoko', COUNT(*) FROM `dev-knb_db`.`FilePayoutsMoko`
UNION ALL SELECT 'EcolesWalletMouvements', COUNT(*) FROM `dev-knb_db`.`EcolesWalletMouvements`;

-- 3. École 13 configurée ? (NULL = normal si pas encore initialisée via API)
SELECT e.IdEcole, e.NomEcole, i.IdEcoleInfoPaiementMobile, w.IdEcoleWallet
FROM `dev-knb_db`.`Ecoles` e
LEFT JOIN `dev-knb_db`.`EcolesInfoPaiementMobile` i ON i.IdEcole = e.IdEcole
LEFT JOIN `dev-knb_db`.`EcolesWallets` w ON w.IdEcole = e.IdEcole
WHERE e.IdEcole = 13;

-- Attendu si migration OK :
--   • Requête 0 : 6 tables listées dans dev-knb_db
--   • Requête 1 : 3 colonnes sur Paiements
--   • Requête 2 : 6 lignes de compteurs (souvent Nb = 0)
--   • Requête 3 : école 13 existe ; config/wallet NULL tant que POST /paiement-mobile non fait
--
-- Erreur #1064 sur USE : utilisez USE `dev-knb_db`; (backticks autour du nom avec tiret).
