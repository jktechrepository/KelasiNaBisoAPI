-- Script SQL pour réactiver l'école ID 1 (Ekelasi School)
-- À exécuter dans votre client SQL (HeidiSQL, MySQL Workbench, etc.)

UPDATE Ecoles 
SET Statut = 1 
WHERE IdEcole = 1;

-- Vérifier le résultat
SELECT IdEcole, Nom, Statut 
FROM Ecoles 
WHERE IdEcole = 1;

