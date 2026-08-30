-- =============================================================================
-- KelasiNaBiso — Vérification post-correction Dashboard (knb_db)
-- Sans information_schema (compatible kelasiUser / phpMyAdmin restreint)
-- =============================================================================

USE knb_db;

SELECT DATABASE() AS BaseCourante;

-- 1. Colonnes MOKO sur Paiements (attendu : 3 lignes)
SHOW COLUMNS FROM `Paiements`
WHERE Field IN ('MontantNet', 'MontantCollecte', 'OperateurMobileMoney');

-- 2. Tables MOKO (attendu : 6 lignes)
SHOW TABLES LIKE 'EcolesInfoPaiementMobile';
SHOW TABLES LIKE 'EcolesBeneficiairesMomo';
SHOW TABLES LIKE 'EcolesWallets';
SHOW TABLES LIKE 'TransactionsMoko';
SHOW TABLES LIKE 'FilePayoutsMoko';
SHOW TABLES LIKE 'EcolesWalletMouvements';

-- 3. Colonnes Frais
SHOW COLUMNS FROM `Frais`
WHERE Field IN ('IdAnneeScolaire', 'IdClasse');

-- 4. Frais sans année (attendu : 0)
SELECT COUNT(*) AS FraisSansAnnee FROM `Frais` WHERE IdAnneeScolaire IS NULL;

-- 5. Migrations enregistrées
SELECT MigrationId FROM `__EFMigrationsHistory`
WHERE MigrationId IN (
    '20260704123906_AddMokoAfrikaPaymentEntities',
    '20260806120000_AddFraisAnneeScolaireAndClasse'
)
ORDER BY MigrationId;

-- 6. Compteurs tables MOKO
SELECT 'EcolesInfoPaiementMobile' AS TableName, COUNT(*) AS Nb FROM `EcolesInfoPaiementMobile`
UNION ALL SELECT 'EcolesBeneficiairesMomo', COUNT(*) FROM `EcolesBeneficiairesMomo`
UNION ALL SELECT 'EcolesWallets', COUNT(*) FROM `EcolesWallets`
UNION ALL SELECT 'TransactionsMoko', COUNT(*) FROM `TransactionsMoko`
UNION ALL SELECT 'FilePayoutsMoko', COUNT(*) FROM `FilePayoutsMoko`
UNION ALL SELECT 'EcolesWalletMouvements', COUNT(*) FROM `EcolesWalletMouvements`;
