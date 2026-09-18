-- =============================================================================
-- KelasiNaBiso — Vérification après correction VuePaiementsFraisParEcole
-- =============================================================================

USE knb_db;

-- 1. Colonnes critiques (attendu : 3 lignes)
SHOW COLUMNS FROM `VuePaiementsFraisParEcole`
WHERE Field IN ('NomCompletFormate', 'ReferenceTransaction', 'NomCompletOriginal');

-- 2. Migration EF enregistrée (attendu : 1 ligne si FIX avec history exécuté)
SELECT MigrationId, ProductVersion
FROM `__EFMigrationsHistory`
WHERE MigrationId = '20260831153000_RecreateVuePaiementsFraisParEcole';

-- 3. Lecture échantillon (classe peut être NULL si pas d'inscription confirmée)
SELECT IdPaiement, Matricule, NomCompletFormate, ReferenceTransaction,
       IdClasse, NomClasse, NomEcole, LibelleFrais, Montant
FROM `VuePaiementsFraisParEcole`
WHERE Matricule IS NOT NULL AND Matricule <> ''
LIMIT 5;

-- 4. Comptage global
SELECT COUNT(*) AS TotalLignes FROM `VuePaiementsFraisParEcole`;

-- 5. Répartition avec / sans classe (inscription)
SELECT
    SUM(CASE WHEN IdClasse IS NOT NULL THEN 1 ELSE 0 END) AS AvecClasse,
    SUM(CASE WHEN IdClasse IS NULL THEN 1 ELSE 0 END) AS SansClasse
FROM `VuePaiementsFraisParEcole`;
