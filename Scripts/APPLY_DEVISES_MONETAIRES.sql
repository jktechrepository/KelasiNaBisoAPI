-- ==========================================================
-- Catalogue DevisesMonetaires + seed USD/CDF par école
-- MySQL / MariaDB — script idempotent
-- ==========================================================

START TRANSACTION;

CREATE TABLE IF NOT EXISTS DevisesMonetaires (
    IdDeviseMonetaire INT NOT NULL AUTO_INCREMENT,
    IdEcole INT NOT NULL,
    CodeDevise VARCHAR(10) NOT NULL,
    Libelle VARCHAR(120) NOT NULL,
    Symbole VARCHAR(10) NULL,
    Statut TINYINT(1) NOT NULL DEFAULT 1,
    DateCreation DATETIME(6) NOT NULL,
    PRIMARY KEY (IdDeviseMonetaire),
    UNIQUE KEY IX_DevisesMonetaires_IdEcole_CodeDevise (IdEcole, CodeDevise),
    KEY IX_DevisesMonetaires_IdEcole (IdEcole)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Seed USD pour chaque école
INSERT INTO DevisesMonetaires (IdEcole, CodeDevise, Libelle, Symbole, Statut, DateCreation)
SELECT e.IdEcole, 'USD', 'Dollar américain', '$', 1, UTC_TIMESTAMP(6)
FROM Ecoles e
WHERE NOT EXISTS (
    SELECT 1 FROM DevisesMonetaires d
    WHERE d.IdEcole = e.IdEcole AND d.CodeDevise = 'USD'
);

-- Seed CDF pour chaque école
INSERT INTO DevisesMonetaires (IdEcole, CodeDevise, Libelle, Symbole, Statut, DateCreation)
SELECT e.IdEcole, 'CDF', 'Franc congolais', 'FC', 1, UTC_TIMESTAMP(6)
FROM Ecoles e
WHERE NOT EXISTS (
    SELECT 1 FROM DevisesMonetaires d
    WHERE d.IdEcole = e.IdEcole AND d.CodeDevise = 'CDF'
);

-- Couvrir CodeDevisePrincipale hors USD/CDF
INSERT INTO DevisesMonetaires (IdEcole, CodeDevise, Libelle, Symbole, Statut, DateCreation)
SELECT e.IdEcole,
       UPPER(TRIM(e.CodeDevisePrincipale)),
       UPPER(TRIM(e.CodeDevisePrincipale)),
       NULL,
       1,
       UTC_TIMESTAMP(6)
FROM Ecoles e
WHERE e.CodeDevisePrincipale IS NOT NULL
  AND TRIM(e.CodeDevisePrincipale) <> ''
  AND UPPER(TRIM(e.CodeDevisePrincipale)) NOT IN ('USD', 'CDF')
  AND NOT EXISTS (
      SELECT 1 FROM DevisesMonetaires d
      WHERE d.IdEcole = e.IdEcole
        AND d.CodeDevise = UPPER(TRIM(e.CodeDevisePrincipale))
  );

COMMIT;

-- Vérifications :
-- SHOW TABLES LIKE 'DevisesMonetaires';
-- SELECT IdEcole, CodeDevise, Libelle, Statut FROM DevisesMonetaires ORDER BY IdEcole, CodeDevise LIMIT 50;
