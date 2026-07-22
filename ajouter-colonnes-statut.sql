-- =============================================
-- Script pour ajouter les colonnes Statut manquantes
-- =============================================

-- Vérifier et ajouter la colonne Statut à chaque table si elle n'existe pas

-- 1. Ecoles
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Ecoles') AND name = 'Statut')
BEGIN
    ALTER TABLE Ecoles ADD Statut bit NOT NULL DEFAULT 1;
    PRINT 'Colonne Statut ajoutée à la table Ecoles';
END
ELSE
BEGIN
    PRINT 'Colonne Statut existe déjà dans la table Ecoles';
END

-- 2. Directions
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Directions') AND name = 'Statut')
BEGIN
    ALTER TABLE Directions ADD Statut bit NOT NULL DEFAULT 1;
    PRINT 'Colonne Statut ajoutée à la table Directions';
END
ELSE
BEGIN
    PRINT 'Colonne Statut existe déjà dans la table Directions';
END

-- 3. Classes
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Classes') AND name = 'Statut')
BEGIN
    ALTER TABLE Classes ADD Statut bit NOT NULL DEFAULT 1;
    PRINT 'Colonne Statut ajoutée à la table Classes';
END
ELSE
BEGIN
    PRINT 'Colonne Statut existe déjà dans la table Classes';
END

-- 4. Sections
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Sections') AND name = 'Statut')
BEGIN
    ALTER TABLE Sections ADD Statut bit NOT NULL DEFAULT 1;
    PRINT 'Colonne Statut ajoutée à la table Sections';
END
ELSE
BEGIN
    PRINT 'Colonne Statut existe déjà dans la table Sections';
END

-- 5. Options
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Options') AND name = 'Statut')
BEGIN
    ALTER TABLE Options ADD Statut bit NOT NULL DEFAULT 1;
    PRINT 'Colonne Statut ajoutée à la table Options';
END
ELSE
BEGIN
    PRINT 'Colonne Statut existe déjà dans la table Options';
END

-- 6. AnneeScolaires
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AnneeScolaires') AND name = 'Statut')
BEGIN
    ALTER TABLE AnneeScolaires ADD Statut bit NOT NULL DEFAULT 1;
    PRINT 'Colonne Statut ajoutée à la table AnneeScolaires';
END
ELSE
BEGIN
    PRINT 'Colonne Statut existe déjà dans la table AnneeScolaires';
END

-- 7. Roles
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Roles') AND name = 'Statut')
BEGIN
    ALTER TABLE Roles ADD Statut bit NOT NULL DEFAULT 1;
    PRINT 'Colonne Statut ajoutée à la table Roles';
END
ELSE
BEGIN
    PRINT 'Colonne Statut existe déjà dans la table Roles';
END

-- 8. Vacations
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Vacations') AND name = 'Statut')
BEGIN
    ALTER TABLE Vacations ADD Statut bit NOT NULL DEFAULT 1;
    PRINT 'Colonne Statut ajoutée à la table Vacations';
END
ELSE
BEGIN
    PRINT 'Colonne Statut existe déjà dans la table Vacations';
END

-- 9. Cours
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Cours') AND name = 'Statut')
BEGIN
    ALTER TABLE Cours ADD Statut bit NOT NULL DEFAULT 1;
    PRINT 'Colonne Statut ajoutée à la table Cours';
END
ELSE
BEGIN
    PRINT 'Colonne Statut existe déjà dans la table Cours';
END

-- 10. Notes
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Notes') AND name = 'Statut')
BEGIN
    ALTER TABLE Notes ADD Statut bit NOT NULL DEFAULT 1;
    PRINT 'Colonne Statut ajoutée à la table Notes';
END
ELSE
BEGIN
    PRINT 'Colonne Statut existe déjà dans la table Notes';
END

-- 11. Evaluations
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Evaluations') AND name = 'Statut')
BEGIN
    ALTER TABLE Evaluations ADD Statut bit NOT NULL DEFAULT 1;
    PRINT 'Colonne Statut ajoutée à la table Evaluations';
END
ELSE
BEGIN
    PRINT 'Colonne Statut existe déjà dans la table Evaluations';
END

-- 12. Horaires
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Horaires') AND name = 'Statut')
BEGIN
    ALTER TABLE Horaires ADD Statut bit NOT NULL DEFAULT 1;
    PRINT 'Colonne Statut ajoutée à la table Horaires';
END
ELSE
BEGIN
    PRINT 'Colonne Statut existe déjà dans la table Horaires';
END

-- 13. Messages
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Messages') AND name = 'Statut')
BEGIN
    ALTER TABLE Messages ADD Statut bit NOT NULL DEFAULT 1;
    PRINT 'Colonne Statut ajoutée à la table Messages';
END
ELSE
BEGIN
    PRINT 'Colonne Statut existe déjà dans la table Messages';
END

-- 14. GroupeMessages
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('GroupeMessages') AND name = 'Statut')
BEGIN
    ALTER TABLE GroupeMessages ADD Statut bit NOT NULL DEFAULT 1;
    PRINT 'Colonne Statut ajoutée à la table GroupeMessages';
END
ELSE
BEGIN
    PRINT 'Colonne Statut existe déjà dans la table GroupeMessages';
END

-- 15. Documents
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Documents') AND name = 'Statut')
BEGIN
    ALTER TABLE Documents ADD Statut bit NOT NULL DEFAULT 1;
    PRINT 'Colonne Statut ajoutée à la table Documents';
END
ELSE
BEGIN
    PRINT 'Colonne Statut existe déjà dans la table Documents';
END

-- 16. RessourcePedagogiques
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RessourcePedagogiques') AND name = 'Statut')
BEGIN
    ALTER TABLE RessourcePedagogiques ADD Statut bit NOT NULL DEFAULT 1;
    PRINT 'Colonne Statut ajoutée à la table RessourcePedagogiques';
END
ELSE
BEGIN
    PRINT 'Colonne Statut existe déjà dans la table RessourcePedagogiques';
END

-- 17. Inscriptions (ajouter le champ Statut en plus de StatutInscription existant)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Inscriptions') AND name = 'Statut')
BEGIN
    ALTER TABLE Inscriptions ADD Statut bit NOT NULL DEFAULT 1;
    PRINT 'Colonne Statut ajoutée à la table Inscriptions';
END
ELSE
BEGIN
    PRINT 'Colonne Statut existe déjà dans la table Inscriptions';
END

-- 18. AffectationsCours
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AffectationsCours') AND name = 'Statut')
BEGIN
    ALTER TABLE AffectationsCours ADD Statut bit NOT NULL DEFAULT 1;
    PRINT 'Colonne Statut ajoutée à la table AffectationsCours';
END
ELSE
BEGIN
    PRINT 'Colonne Statut existe déjà dans la table AffectationsCours';
END

PRINT 'Script terminé avec succès !';
