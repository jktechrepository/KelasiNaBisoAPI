-- =============================================================================
-- KelasiNaBiso — Diagnostic vue VuePaiementsFraisParEcole (production)
-- Sans information_schema (compatible phpMyAdmin / compte restreint)
-- =============================================================================
-- Remplacer knb_db par le nom de votre base production si différent.

USE knb_db;

-- 1. Vue existe ?
SHOW TABLES LIKE 'VuePaiementsFraisParEcole';

-- 2. Colonnes attendues par VuePaiementsFraisParEcoleDTO (0 ligne = correction requise)
SHOW COLUMNS FROM `VuePaiementsFraisParEcole`
WHERE Field IN ('NomCompletFormate', 'ReferenceTransaction', 'NomCompletOriginal');

-- 3. Prérequis CREATE VIEW : Eleves.IdClasse (requis par le script actuel)
SHOW COLUMNS FROM `Eleves`
WHERE Field = 'IdClasse';

-- 4. Migration EF déjà enregistrée ?
SELECT MigrationId, ProductVersion
FROM `__EFMigrationsHistory`
WHERE MigrationId = '20260831153000_RecreateVuePaiementsFraisParEcole';

-- 5. Échantillon (si la vue est lisible malgré colonnes manquantes côté EF)
-- SELECT Matricule, NomCompletFormate FROM VuePaiementsFraisParEcole LIMIT 1;
