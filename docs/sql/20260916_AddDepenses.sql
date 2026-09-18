-- Module Dépenses V1 (CategoriesDepense + Depenses)
-- Exécuter si EF migrate échoue.

CREATE TABLE IF NOT EXISTS `CategoriesDepense` (
  `IdCategorieDepense` int NOT NULL AUTO_INCREMENT,
  `IdEcole` int NOT NULL,
  `NomCategorie` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
  `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
  `Statut` tinyint(1) NOT NULL DEFAULT 1,
  `DateCreation` datetime(6) NOT NULL,
  PRIMARY KEY (`IdCategorieDepense`),
  KEY `IX_CategoriesDepense_IdEcole` (`IdEcole`),
  CONSTRAINT `FK_CategoriesDepense_Ecoles_IdEcole` FOREIGN KEY (`IdEcole`) REFERENCES `Ecoles` (`IdEcole`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `Depenses` (
  `IdDepense` int NOT NULL AUTO_INCREMENT,
  `IdEcole` int NOT NULL,
  `IdCategorieDepense` int NOT NULL,
  `Libelle` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
  `Description` varchar(1000) CHARACTER SET utf8mb4 NULL,
  `Beneficiaire` varchar(200) CHARACTER SET utf8mb4 NULL,
  `ReferencePiece` varchar(100) CHARACTER SET utf8mb4 NULL,
  `Montant` decimal(18,2) NOT NULL,
  `CodeDeviseMontant` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
  `CodeDevisePrincipale` varchar(10) CHARACTER SET utf8mb4 NULL,
  `TauxVersDevisePrincipale` decimal(18,8) NULL,
  `MontantDevisePrincipale` decimal(18,2) NULL,
  `ModePaiement` varchar(50) CHARACTER SET utf8mb4 NULL,
  `DateDepense` datetime(6) NOT NULL,
  `StatutWorkflow` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
  `Actif` tinyint(1) NOT NULL DEFAULT 1,
  `IdUtilisateurCreateur` int NULL,
  `DateCreation` datetime(6) NOT NULL,
  `MotifAnnulation` varchar(500) CHARACTER SET utf8mb4 NULL,
  `IdUtilisateurAnnulation` int NULL,
  `DateAnnulation` datetime(6) NULL,
  PRIMARY KEY (`IdDepense`),
  KEY `IX_Depenses_IdEcole_DateDepense` (`IdEcole`, `DateDepense`),
  KEY `IX_Depenses_IdCategorieDepense` (`IdCategorieDepense`),
  KEY `IX_Depenses_StatutWorkflow` (`StatutWorkflow`),
  CONSTRAINT `FK_Depenses_Ecoles_IdEcole` FOREIGN KEY (`IdEcole`) REFERENCES `Ecoles` (`IdEcole`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Depenses_CategoriesDepense_IdCategorieDepense` FOREIGN KEY (`IdCategorieDepense`) REFERENCES `CategoriesDepense` (`IdCategorieDepense`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Depenses_Utilisateurs_IdUtilisateurCreateur` FOREIGN KEY (`IdUtilisateurCreateur`) REFERENCES `Utilisateurs` (`IdUtilisateur`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Permissions Depense.*
INSERT INTO Permissions (Nom, Categorie, Action, Description, Statut, DateCreation)
SELECT 'Depense.Read', 'Depense', 'Read', 'Voir une dépense', 1, UTC_TIMESTAMP() FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'Depense.Read');

INSERT INTO Permissions (Nom, Categorie, Action, Description, Statut, DateCreation)
SELECT 'Depense.ReadAll', 'Depense', 'ReadAll', 'Lister les dépenses', 1, UTC_TIMESTAMP() FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'Depense.ReadAll');

INSERT INTO Permissions (Nom, Categorie, Action, Description, Statut, DateCreation)
SELECT 'Depense.Create', 'Depense', 'Create', 'Créer une dépense', 1, UTC_TIMESTAMP() FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'Depense.Create');

INSERT INTO Permissions (Nom, Categorie, Action, Description, Statut, DateCreation)
SELECT 'Depense.Update', 'Depense', 'Update', 'Modifier / annuler une dépense', 1, UTC_TIMESTAMP() FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'Depense.Update');

INSERT INTO Permissions (Nom, Categorie, Action, Description, Statut, DateCreation)
SELECT 'Depense.Delete', 'Depense', 'Delete', 'Supprimer (soft) une dépense', 1, UTC_TIMESTAMP() FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'Depense.Delete');

-- Permissions CategorieDepense.*
INSERT INTO Permissions (Nom, Categorie, Action, Description, Statut, DateCreation)
SELECT 'CategorieDepense.Read', 'CategorieDepense', 'Read', 'Voir une catégorie de dépense', 1, UTC_TIMESTAMP() FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'CategorieDepense.Read');

INSERT INTO Permissions (Nom, Categorie, Action, Description, Statut, DateCreation)
SELECT 'CategorieDepense.ReadAll', 'CategorieDepense', 'ReadAll', 'Lister les catégories de dépense', 1, UTC_TIMESTAMP() FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'CategorieDepense.ReadAll');

INSERT INTO Permissions (Nom, Categorie, Action, Description, Statut, DateCreation)
SELECT 'CategorieDepense.Create', 'CategorieDepense', 'Create', 'Créer une catégorie de dépense', 1, UTC_TIMESTAMP() FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'CategorieDepense.Create');

INSERT INTO Permissions (Nom, Categorie, Action, Description, Statut, DateCreation)
SELECT 'CategorieDepense.Update', 'CategorieDepense', 'Update', 'Modifier une catégorie de dépense', 1, UTC_TIMESTAMP() FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'CategorieDepense.Update');

INSERT INTO Permissions (Nom, Categorie, Action, Description, Statut, DateCreation)
SELECT 'CategorieDepense.Delete', 'CategorieDepense', 'Delete', 'Supprimer / désactiver une catégorie', 1, UTC_TIMESTAMP() FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'CategorieDepense.Delete');

-- Alignement rôles
-- Admin / Super-Admin : toutes
INSERT INTO RolePermissions (IdRole, IdPermission, DateAttribution)
SELECT r.IdRole, p.IdPermission, UTC_TIMESTAMP()
FROM Roles r
CROSS JOIN Permissions p
WHERE r.Nom IN ('Admin', 'Super-Admin')
  AND p.Categorie IN ('Depense', 'CategorieDepense')
  AND NOT EXISTS (
    SELECT 1 FROM RolePermissions rp
    WHERE rp.IdRole = r.IdRole AND rp.IdPermission = p.IdPermission
  );

-- Financier : Depense Create/Update/Read/ReadAll ; Categorie Read/ReadAll/Create/Update
INSERT INTO RolePermissions (IdRole, IdPermission, DateAttribution)
SELECT r.IdRole, p.IdPermission, UTC_TIMESTAMP()
FROM Roles r
CROSS JOIN Permissions p
WHERE r.Nom = 'Financier'
  AND (
    (p.Categorie = 'Depense' AND p.Action IN ('Create', 'Update', 'Read', 'ReadAll'))
    OR (p.Categorie = 'CategorieDepense' AND p.Action IN ('Read', 'ReadAll', 'Create', 'Update'))
  )
  AND NOT EXISTS (
    SELECT 1 FROM RolePermissions rp
    WHERE rp.IdRole = r.IdRole AND rp.IdPermission = p.IdPermission
  );

-- Caissier : Depense Create/Read/ReadAll ; Categorie Read/ReadAll
INSERT INTO RolePermissions (IdRole, IdPermission, DateAttribution)
SELECT r.IdRole, p.IdPermission, UTC_TIMESTAMP()
FROM Roles r
CROSS JOIN Permissions p
WHERE r.Nom = 'Caissier'
  AND (
    (p.Categorie = 'Depense' AND p.Action IN ('Create', 'Read', 'ReadAll'))
    OR (p.Categorie = 'CategorieDepense' AND p.Action IN ('Read', 'ReadAll'))
  )
  AND NOT EXISTS (
    SELECT 1 FROM RolePermissions rp
    WHERE rp.IdRole = r.IdRole AND rp.IdPermission = p.IdPermission
  );

-- Directeur : Depense Read/ReadAll/Update/Delete ; Categorie toutes
INSERT INTO RolePermissions (IdRole, IdPermission, DateAttribution)
SELECT r.IdRole, p.IdPermission, UTC_TIMESTAMP()
FROM Roles r
CROSS JOIN Permissions p
WHERE r.Nom = 'Directeur'
  AND (
    (p.Categorie = 'Depense' AND p.Action IN ('Read', 'ReadAll', 'Update', 'Delete'))
    OR (p.Categorie = 'CategorieDepense')
  )
  AND NOT EXISTS (
    SELECT 1 FROM RolePermissions rp
    WHERE rp.IdRole = r.IdRole AND rp.IdPermission = p.IdPermission
  );

-- Sous-Directeur : Depense Read/ReadAll/Update ; Categorie Read/ReadAll
INSERT INTO RolePermissions (IdRole, IdPermission, DateAttribution)
SELECT r.IdRole, p.IdPermission, UTC_TIMESTAMP()
FROM Roles r
CROSS JOIN Permissions p
WHERE r.Nom IN ('Sous-Directeur', 'SousDirecteur')
  AND (
    (p.Categorie = 'Depense' AND p.Action IN ('Read', 'ReadAll', 'Update'))
    OR (p.Categorie = 'CategorieDepense' AND p.Action IN ('Read', 'ReadAll'))
  )
  AND NOT EXISTS (
    SELECT 1 FROM RolePermissions rp
    WHERE rp.IdRole = r.IdRole AND rp.IdPermission = p.IdPermission
  );
