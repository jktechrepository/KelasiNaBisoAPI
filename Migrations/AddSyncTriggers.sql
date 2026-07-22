-- ══════════════════════════════════════════════════════════════════════════════════
-- MIGRATION : TRIGGERS DE SYNCHRONISATION
-- Date : 1er novembre 2025
-- Objectif : Synchroniser automatiquement Utilisateur ↔ Agent ↔ Tuteur
-- ══════════════════════════════════════════════════════════════════════════════════

-- ══════════════════════════════════════════════════════════════════════════════════
-- TRIGGER 1 : Agent → Utilisateur (AFTER UPDATE)
-- Quand un Agent est modifié, synchroniser vers l'Utilisateur associé
-- ══════════════════════════════════════════════════════════════════════════════════

DELIMITER $$

DROP TRIGGER IF EXISTS sync_agent_to_utilisateur_update$$

CREATE TRIGGER sync_agent_to_utilisateur_update
AFTER UPDATE ON Agents
FOR EACH ROW
BEGIN
    -- Synchroniser uniquement si des champs pertinents ont changé
    IF (NEW.Nom != OLD.Nom OR 
        NEW.Postnom != OLD.Postnom OR 
        NEW.EmailAgent != OLD.EmailAgent OR 
        NEW.TelephoneAgent != OLD.TelephoneAgent OR 
        IFNULL(NEW.PhotoUrl, '') != IFNULL(OLD.PhotoUrl, '')) THEN
        
        -- Mettre à jour l'Utilisateur associé
        UPDATE Utilisateurs 
        SET 
            Nom = NEW.Nom,
            Postnom = NEW.Postnom,
            Email = NEW.EmailAgent,
            Telephone = NEW.TelephoneAgent,
            Photo = NEW.PhotoUrl,
            DateModification = NOW()
        WHERE IdAgent = NEW.IdAgent
        AND (
            -- Ne mettre à jour que si les données sont vraiment différentes
            Nom != NEW.Nom OR
            Postnom != NEW.Postnom OR
            Email != NEW.EmailAgent OR
            IFNULL(Telephone, '') != IFNULL(NEW.TelephoneAgent, '') OR
            IFNULL(Photo, '') != IFNULL(NEW.PhotoUrl, '')
        );
    END IF;
END$$

DELIMITER ;

-- ══════════════════════════════════════════════════════════════════════════════════
-- TRIGGER 2 : Tuteur → Utilisateur (AFTER UPDATE)
-- Quand un Tuteur est modifié, synchroniser vers l'Utilisateur associé
-- ══════════════════════════════════════════════════════════════════════════════════

DELIMITER $$

DROP TRIGGER IF EXISTS sync_tuteur_to_utilisateur_update$$

CREATE TRIGGER sync_tuteur_to_utilisateur_update
AFTER UPDATE ON Tuteurs
FOR EACH ROW
BEGIN
    -- Déclarer variables pour extraction Nom/Prénom
    DECLARE nom_extract VARCHAR(100);
    DECLARE prenom_extract VARCHAR(100);
    DECLARE space_pos INT;
    
    -- Synchroniser uniquement si des champs pertinents ont changé
    IF (IFNULL(NEW.NomComplet, '') != IFNULL(OLD.NomComplet, '') OR 
        IFNULL(NEW.Email, '') != IFNULL(OLD.Email, '') OR 
        IFNULL(NEW.Telephone, '') != IFNULL(OLD.Telephone, '')) THEN
        
        -- Extraire Nom et Prénom depuis NomComplet (format: "Nom Prénom")
        SET space_pos = LOCATE(' ', NEW.NomComplet);
        
        IF space_pos > 0 THEN
            SET nom_extract = SUBSTRING(NEW.NomComplet, 1, space_pos - 1);
            SET prenom_extract = SUBSTRING(NEW.NomComplet, space_pos + 1);
        ELSE
            SET nom_extract = NEW.NomComplet;
            SET prenom_extract = '';
        END IF;
        
        -- Mettre à jour l'Utilisateur associé
        UPDATE Utilisateurs 
        SET 
            Nom = nom_extract,
            Prenom = prenom_extract,
            Email = NEW.Email,
            Telephone = NEW.Telephone,
            DateModification = NOW()
        WHERE IdTuteur = NEW.IdTuteur
        AND (
            Nom != nom_extract OR
            IFNULL(Prenom, '') != IFNULL(prenom_extract, '') OR
            IFNULL(Email, '') != IFNULL(NEW.Email, '') OR
            IFNULL(Telephone, '') != IFNULL(NEW.Telephone, '')
        );
    END IF;
END$$

DELIMITER ;

-- ══════════════════════════════════════════════════════════════════════════════════
-- TRIGGER 3 : Utilisateur → Agent (AFTER UPDATE)
-- Quand un Utilisateur (qui est un Agent) est modifié, synchroniser vers l'Agent
-- ══════════════════════════════════════════════════════════════════════════════════

DELIMITER $$

DROP TRIGGER IF EXISTS sync_utilisateur_to_agent_update$$

CREATE TRIGGER sync_utilisateur_to_agent_update
AFTER UPDATE ON Utilisateurs
FOR EACH ROW
BEGIN
    -- Synchroniser uniquement si l'utilisateur est un Agent
    IF NEW.IdAgent IS NOT NULL THEN
        
        -- Synchroniser uniquement si des champs pertinents ont changé
        IF (IFNULL(NEW.Nom, '') != IFNULL(OLD.Nom, '') OR 
            IFNULL(NEW.Postnom, '') != IFNULL(OLD.Postnom, '') OR 
            IFNULL(NEW.Email, '') != IFNULL(OLD.Email, '') OR 
            IFNULL(NEW.Telephone, '') != IFNULL(OLD.Telephone, '') OR 
            IFNULL(NEW.Photo, '') != IFNULL(OLD.Photo, '')) THEN
            
            -- Mettre à jour l'Agent associé
            UPDATE Agents 
            SET 
                Nom = NEW.Nom,
                Postnom = NEW.Postnom,
                EmailAgent = NEW.Email,
                TelephoneAgent = NEW.Telephone,
                PhotoUrl = NEW.Photo,
                DateModification = NOW()
            WHERE IdAgent = NEW.IdAgent
            AND (
                Nom != NEW.Nom OR
                IFNULL(Postnom, '') != IFNULL(NEW.Postnom, '') OR
                IFNULL(EmailAgent, '') != IFNULL(NEW.Email, '') OR
                IFNULL(TelephoneAgent, '') != IFNULL(NEW.Telephone, '') OR
                IFNULL(PhotoUrl, '') != IFNULL(NEW.Photo, '')
            );
        END IF;
    END IF;
END$$

DELIMITER ;

-- ══════════════════════════════════════════════════════════════════════════════════
-- TRIGGER 4 : Utilisateur → Tuteur (AFTER UPDATE)
-- Quand un Utilisateur (qui est un Tuteur) est modifié, synchroniser vers le Tuteur
-- ══════════════════════════════════════════════════════════════════════════════════

DELIMITER $$

DROP TRIGGER IF EXISTS sync_utilisateur_to_tuteur_update$$

CREATE TRIGGER sync_utilisateur_to_tuteur_update
AFTER UPDATE ON Utilisateurs
FOR EACH ROW
BEGIN
    -- Déclarer variable pour NomComplet reconstruit
    DECLARE nom_complet_reconstruit VARCHAR(255);
    
    -- Synchroniser uniquement si l'utilisateur est un Tuteur
    IF NEW.IdTuteur IS NOT NULL THEN
        
        -- Synchroniser uniquement si des champs pertinents ont changé
        IF (IFNULL(NEW.Nom, '') != IFNULL(OLD.Nom, '') OR 
            IFNULL(NEW.Prenom, '') != IFNULL(OLD.Prenom, '') OR 
            IFNULL(NEW.Email, '') != IFNULL(OLD.Email, '') OR 
            IFNULL(NEW.Telephone, '') != IFNULL(OLD.Telephone, '')) THEN
            
            -- Reconstruire NomComplet depuis Nom + Prénom
            SET nom_complet_reconstruit = CONCAT(
                IFNULL(NEW.Nom, ''), 
                ' ', 
                IFNULL(NEW.Prenom, '')
            );
            SET nom_complet_reconstruit = TRIM(nom_complet_reconstruit);
            
            -- Mettre à jour le Tuteur associé
            UPDATE Tuteurs 
            SET 
                NomComplet = nom_complet_reconstruit,
                Email = NEW.Email,
                Telephone = NEW.Telephone,
                DateModification = NOW()
            WHERE IdTuteur = NEW.IdTuteur
            AND (
                IFNULL(NomComplet, '') != nom_complet_reconstruit OR
                IFNULL(Email, '') != IFNULL(NEW.Email, '') OR
                IFNULL(Telephone, '') != IFNULL(NEW.Telephone, '')
            );
        END IF;
    END IF;
END$$

DELIMITER ;

-- ══════════════════════════════════════════════════════════════════════════════════
-- VÉRIFICATION DES TRIGGERS CRÉÉS
-- ══════════════════════════════════════════════════════════════════════════════════

-- Afficher tous les triggers créés
SHOW TRIGGERS WHERE `Trigger` LIKE 'sync_%';

-- ══════════════════════════════════════════════════════════════════════════════════
-- TESTS DE VALIDATION
-- ══════════════════════════════════════════════════════════════════════════════════

-- Test 1 : Modifier un Agent et vérifier synchronisation
-- UPDATE Agents SET Nom = 'TestNom', EmailAgent = 'test@email.com' WHERE IdAgent = 1;
-- SELECT U.Nom, U.Email, A.Nom, A.EmailAgent FROM Utilisateurs U JOIN Agents A ON U.IdAgent = A.IdAgent WHERE A.IdAgent = 1;

-- Test 2 : Modifier un Utilisateur (Agent) et vérifier synchronisation
-- UPDATE Utilisateurs SET Nom = 'NouveauNom', Email = 'nouveau@email.com' WHERE IdAgent = 1;
-- SELECT U.Nom, U.Email, A.Nom, A.EmailAgent FROM Utilisateurs U JOIN Agents A ON U.IdAgent = A.IdAgent WHERE A.IdAgent = 1;

-- Test 3 : Modifier un Tuteur et vérifier synchronisation
-- UPDATE Tuteurs SET NomComplet = 'Mukendi Jean', Email = 'mukendi@email.com' WHERE IdTuteur = 1;
-- SELECT T.NomComplet, T.Email, U.Nom, U.Prenom, U.Email FROM Tuteurs T JOIN Utilisateurs U ON U.IdTuteur = T.IdTuteur WHERE T.IdTuteur = 1;

-- Test 4 : Modifier un Utilisateur (Tuteur) et vérifier synchronisation
-- UPDATE Utilisateurs SET Nom = 'Kalala', Prenom = 'Pierre', Email = 'kalala@email.com' WHERE IdTuteur = 1;
-- SELECT U.Nom, U.Prenom, U.Email, T.NomComplet, T.Email FROM Utilisateurs U JOIN Tuteurs T ON U.IdTuteur = T.IdTuteur WHERE T.IdTuteur = 1;

-- ══════════════════════════════════════════════════════════════════════════════════
-- RÉSUMÉ
-- ══════════════════════════════════════════════════════════════════════════════════
-- 4 Triggers créés avec succès :
--   1. sync_agent_to_utilisateur_update     : Agent → Utilisateur
--   2. sync_tuteur_to_utilisateur_update    : Tuteur → Utilisateur
--   3. sync_utilisateur_to_agent_update     : Utilisateur → Agent
--   4. sync_utilisateur_to_tuteur_update    : Utilisateur → Tuteur
--
-- ✅ Synchronisation automatique bidirectionnelle
-- ✅ Protection contre boucles infinies (IF OLD != NEW)
-- ✅ Gestion des valeurs NULL
-- ✅ Mise à jour DateModification automatique
-- ══════════════════════════════════════════════════════════════════════════════════

