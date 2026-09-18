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

-- 3. Prérequis CREATE VIEW : Inscriptions (classe via inscription, plus Eleves.IdClasse)
SHOW COLUMNS FROM `Inscriptions`
WHERE Field IN ('IdEleve', 'IdClasse', 'DateInscription', 'Statut', 'StatutInscription');

-- 3b. Frais.IdEcole (école de la vue)
SHOW COLUMNS FROM `Frais`
WHERE Field = 'IdEcole';

-- 4. Migration EF déjà enregistrée ?
SELECT MigrationId, ProductVersion
FROM `__EFMigrationsHistory`
WHERE MigrationId = '20260831153000_RecreateVuePaiementsFraisParEcole';

-- 5. La vue est-elle lisible ? (échoue si définition obsolète avec Eleves.IdClasse)
SELECT COUNT(*) AS NbLignesVue FROM `VuePaiementsFraisParEcole`;
