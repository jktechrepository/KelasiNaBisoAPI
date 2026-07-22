-- ═══════════════════════════════════════════════════════════════════
-- CORRECTION URGENTE : Colonne NewValues trop petite dans AuditLogs
-- ═══════════════════════════════════════════════════════════════════
-- Date : 2025-11-05
-- Problème : Data too long for column 'NewValues' at row 1
-- Solution : Convertir NewValues de TEXT (65KB) en LONGTEXT (4GB)
-- ═══════════════════════════════════════════════════════════════════

USE kelasinabiso;

-- 1️⃣ Vérifier la taille actuelle de la colonne NewValues
SELECT 
    COLUMN_NAME,
    COLUMN_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'kelasinabiso'
  AND TABLE_NAME = 'AuditLogs'
  AND COLUMN_NAME IN ('OldValues', 'NewValues', 'ChangedFields');

-- 2️⃣ Modifier les colonnes pour supporter de grandes données JSON
ALTER TABLE AuditLogs 
    MODIFY COLUMN OldValues LONGTEXT NULL,
    MODIFY COLUMN NewValues LONGTEXT NULL,
    MODIFY COLUMN ChangedFields LONGTEXT NULL;

-- 3️⃣ Vérifier que la modification a été appliquée
SELECT 
    COLUMN_NAME,
    COLUMN_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'kelasinabiso'
  AND TABLE_NAME = 'AuditLogs'
  AND COLUMN_NAME IN ('OldValues', 'NewValues', 'ChangedFields');

-- ✅ Résultat attendu :
-- OldValues      : LONGTEXT (4 294 967 295 caractères max)
-- NewValues      : LONGTEXT (4 294 967 295 caractères max)
-- ChangedFields  : LONGTEXT (4 294 967 295 caractères max)

SELECT '✅ Colonnes AuditLogs mises à jour avec succès !' AS Resultat;

