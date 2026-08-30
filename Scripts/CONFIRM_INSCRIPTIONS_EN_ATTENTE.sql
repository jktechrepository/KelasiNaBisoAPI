-- Confirme les inscriptions encore en attente (En attente / EN_ATTENTE),
-- pour qu'elles apparaissent dans les listes élèves filtrées année courante.
-- À exécuter manuellement après déploiement API (dev / prod).
--
-- Prévisualisation :
-- SELECT IdInscription, IdEleve, IdEcole, IdAnneeScolaire, StatutInscription
-- FROM Inscriptions
-- WHERE Statut = 1
--   AND (
--     StatutInscription IS NULL
--     OR TRIM(StatutInscription) = ''
--     OR UPPER(REPLACE(StatutInscription, ' ', '_')) IN ('EN_ATTENTE', 'ENATTENTE')
--     OR StatutInscription LIKE 'En attente%'
--   );

UPDATE Inscriptions
SET StatutInscription = 'Confirmé'
WHERE Statut = 1
  AND (
    StatutInscription IS NULL
    OR TRIM(StatutInscription) = ''
    OR UPPER(REPLACE(StatutInscription, ' ', '_')) IN ('EN_ATTENTE', 'ENATTENTE')
    OR StatutInscription LIKE 'En attente%'
  );
