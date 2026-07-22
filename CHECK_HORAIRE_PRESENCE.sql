-- 🔍 Vérifier la colonne HoraireIdHoraire dans la table Presences

-- 1️⃣ Voir la structure de la table
DESCRIBE Presences;

-- 2️⃣ Vérifier s'il y a des valeurs NULL
SELECT 
    COUNT(*) AS TotalPresences,
    SUM(CASE WHEN HoraireIdHoraire IS NULL THEN 1 ELSE 0 END) AS HoraireNull,
    SUM(CASE WHEN IdVacation IS NULL THEN 1 ELSE 0 END) AS VacationNull,
    SUM(CASE WHEN IdEleve IS NULL THEN 1 ELSE 0 END) AS EleveNull,
    SUM(CASE WHEN IdAgent IS NULL THEN 1 ELSE 0 END) AS AgentNull
FROM Presences;

-- 3️⃣ Voir quelques exemples
SELECT 
    IdPresence,
    HoraireIdHoraire,
    IdVacation,
    IdEleve,
    IdAgent,
    TypePresence
FROM Presences
LIMIT 10;

