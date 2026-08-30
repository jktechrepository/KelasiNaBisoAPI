-- =============================================================================
-- KelasiNaBiso — Diagnostic Dashboard production (knb_db)
-- Sans information_schema (compatible kelasiUser / phpMyAdmin restreint)
-- =============================================================================

USE knb_db;

-- 1. Colonnes MOKO sur Paiements (attendu si erreur 500 : 0 ligne)
SHOW COLUMNS FROM `Paiements`
WHERE Field IN ('MontantNet', 'MontantCollecte', 'OperateurMobileMoney');

-- 2. Migrations EF liées
SELECT MigrationId, ProductVersion
FROM `__EFMigrationsHistory`
WHERE MigrationId IN (
    '20260704123906_AddMokoAfrikaPaymentEntities',
    '20260806120000_AddFraisAnneeScolaireAndClasse'
)
ORDER BY MigrationId;

-- 3. Colonnes Frais pour le dashboard paiement
SHOW COLUMNS FROM `Frais`
WHERE Field IN ('IdAnneeScolaire', 'IdClasse');

-- 4. Tables MOKO présentes ?
SHOW TABLES LIKE 'EcolesInfoPaiementMobile';
SHOW TABLES LIKE 'EcolesBeneficiairesMomo';
SHOW TABLES LIKE 'EcolesWallets';
SHOW TABLES LIKE 'TransactionsMoko';
SHOW TABLES LIKE 'FilePayoutsMoko';
SHOW TABLES LIKE 'EcolesWalletMouvements';
