-- =============================================================================
-- KelasiNaBiso — Vérification après correction VuePaiementsFraisParEcole
-- =============================================================================

USE knb_db;

-- 1. Colonnes critiques (attendu : 3 lignes)
SHOW COLUMNS FROM `VuePaiementsFraisParEcole`
WHERE Field IN ('NomCompletFormate', 'ReferenceTransaction', 'NomCompletOriginal');

-- 2. Migration EF enregistrée (attendu : 1 ligne)
SELECT MigrationId, ProductVersion
FROM `__EFMigrationsHistory`
WHERE MigrationId = '20260831153000_RecreateVuePaiementsFraisParEcole';

-- 3. Lecture échantillon
SELECT IdPaiement, Matricule, NomCompletFormate, ReferenceTransaction, LibelleFrais, Montant
FROM `VuePaiementsFraisParEcole`
WHERE Matricule IS NOT NULL AND Matricule <> ''
LIMIT 5;

-- 4. Comptage global
SELECT COUNT(*) AS TotalLignes FROM `VuePaiementsFraisParEcole`;
