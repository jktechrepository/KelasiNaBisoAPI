-- ============================================================================
-- SCRIPT SQL SIMPLIFIÉ : CRÉATION INDEX UNIQUE COMPOSITE POUR ÉLÈVES
-- ============================================================================
-- Date : 1er décembre 2025
-- Version : Simplifiée (sans vérifications détaillées)
-- ============================================================================
-- 
-- ⚠️ UTILISER CE SCRIPT si vous avez déjà vérifié et nettoyé les doublons
-- 
-- ============================================================================

-- Vérifier si l'index existe déjà
SELECT 
    CASE 
        WHEN COUNT(*) > 0 THEN 'L''index existe déjà'
        ELSE 'L''index n''existe pas - Création en cours...'
    END as Statut
FROM INFORMATION_SCHEMA.STATISTICS
WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'Eleves'
    AND INDEX_NAME = 'IX_Eleves_Unique_Nom_Prenom_DateNaissance_Tuteur_Classe';

-- Créer l'index unique composite
-- ⚠️ Cette commande échouera si des doublons existent
CREATE UNIQUE INDEX IX_Eleves_Unique_Nom_Prenom_DateNaissance_Tuteur_Classe
ON Eleves(Nom, Postnom, Prenom, DateNaissance, IdTuteur, IdClasse);

-- Vérifier que l'index a été créé
SELECT 
    INDEX_NAME as NomIndex,
    GROUP_CONCAT(COLUMN_NAME ORDER BY SEQ_IN_INDEX SEPARATOR ', ') as Colonnes,
    CASE 
        WHEN NON_UNIQUE = 0 THEN 'UNIQUE ✅'
        ELSE 'NON-UNIQUE'
    END as TypeIndex
FROM INFORMATION_SCHEMA.STATISTICS
WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'Eleves'
    AND INDEX_NAME = 'IX_Eleves_Unique_Nom_Prenom_DateNaissance_Tuteur_Classe'
GROUP BY INDEX_NAME, NON_UNIQUE;

-- ============================================================================
-- ROLLBACK (Si nécessaire)
-- ============================================================================
-- DROP INDEX IX_Eleves_Unique_Nom_Prenom_DateNaissance_Tuteur_Classe ON Eleves;

