-- Backfill IdAnneeScolaire / IdClasse sur Frais (si migration EF non appliquée).
-- IdClasse reste NULL (frais direction-wide).
-- Exécuter manuellement sur prod/dev après déploiement.

-- 1) Colonnes
SET @col_annee = (
    SELECT COUNT(1) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Frais' AND COLUMN_NAME = 'IdAnneeScolaire'
);
SET @sql = IF(@col_annee = 0,
    'ALTER TABLE `Frais` ADD COLUMN `IdAnneeScolaire` int NULL',
    'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @col_classe = (
    SELECT COUNT(1) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Frais' AND COLUMN_NAME = 'IdClasse'
);
SET @sql = IF(@col_classe = 0,
    'ALTER TABLE `Frais` ADD COLUMN `IdClasse` int NULL',
    'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- 2) Backfill année courante de l'école
UPDATE Frais f
INNER JOIN Directions d ON d.IdDirection = f.IdDirection
SET f.IdAnneeScolaire = (
    SELECT a.IdAnneeScolaire
    FROM AnneeScolaires a
    WHERE a.IdEcole = d.IdEcole
      AND a.Statut = 1
      AND a.DateDebut <= NOW()
      AND a.DateFin >= NOW()
    ORDER BY a.DateDebut DESC
    LIMIT 1
)
WHERE f.IdAnneeScolaire IS NULL;

-- 3) Fallback année la plus récente active
UPDATE Frais f
INNER JOIN Directions d ON d.IdDirection = f.IdDirection
SET f.IdAnneeScolaire = (
    SELECT a.IdAnneeScolaire
    FROM AnneeScolaires a
    WHERE a.IdEcole = d.IdEcole
      AND a.Statut = 1
    ORDER BY a.DateDebut DESC
    LIMIT 1
)
WHERE f.IdAnneeScolaire IS NULL;

-- Prévisualisation des lignes encore NULL (à corriger manuellement avant NOT NULL) :
-- SELECT f.IdFrais, f.LibelleFrais, f.IdDirection, d.IdEcole
-- FROM Frais f
-- JOIN Directions d ON d.IdDirection = f.IdDirection
-- WHERE f.IdAnneeScolaire IS NULL;

ALTER TABLE `Frais`
    MODIFY COLUMN `IdAnneeScolaire` int NOT NULL;
