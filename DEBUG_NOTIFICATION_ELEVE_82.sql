-- 🔍 DEBUG : Vérifier le lien Élève 82 → Tuteur → Utilisateur → Device

-- ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
-- ÉTAPE 1 : Vérifier l'élève ID 82
-- ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

SELECT 
    e.IdEleve,
    e.NomComplet AS NomEleve,
    e.IdTuteur,
    e.Statut AS StatutEleve,
    CASE 
        WHEN e.IdTuteur IS NULL THEN '❌ PAS DE TUTEUR'
        ELSE '✅ Tuteur lié'
    END AS VerificationTuteur
FROM Eleves e
WHERE e.IdEleve = 82;

-- ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
-- ÉTAPE 2 : Vérifier le tuteur de l'élève
-- ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

SELECT 
    t.IdTuteur,
    t.NomComplet AS NomTuteur,
    t.Telephone,
    t.Email,
    t.Statut AS StatutTuteur,
    CASE 
        WHEN t.Statut = 1 THEN '✅ Actif'
        ELSE '❌ Inactif'
    END AS VerificationStatut
FROM Tuteurs t
WHERE t.IdTuteur = (SELECT IdTuteur FROM Eleves WHERE IdEleve = 82);

-- ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
-- ÉTAPE 3 : Vérifier l'utilisateur lié au tuteur
-- ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

SELECT 
    u.IdUtilisateur,
    u.NomUtilisateur,
    u.DefaultUsername,
    u.IdTuteur,
    u.Statut AS StatutUtilisateur,
    r.Nom AS Role,
    CASE 
        WHEN u.IdTuteur IS NULL THEN '❌ PAS DE TUTEUR LIÉ'
        WHEN u.Statut = 0 THEN '❌ UTILISATEUR INACTIF'
        ELSE '✅ OK'
    END AS Verification
FROM Utilisateurs u
INNER JOIN Roles r ON u.IdRole = r.IdRole
WHERE u.IdTuteur = (SELECT IdTuteur FROM Eleves WHERE IdEleve = 82);

-- ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
-- ÉTAPE 4 : Vérifier les devices de cet utilisateur
-- ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

SELECT 
    ud.IdUserDevice,
    ud.IdUtilisateur,
    ud.FcmToken,
    ud.DeviceType,
    ud.Statut AS StatutDevice,
    CASE 
        WHEN ud.Statut = 1 THEN '✅ Device ACTIF'
        ELSE '❌ Device INACTIF'
    END AS VerificationDevice
FROM UserDevices ud
WHERE ud.IdUtilisateur = (
    SELECT u.IdUtilisateur 
    FROM Utilisateurs u 
    WHERE u.IdTuteur = (SELECT IdTuteur FROM Eleves WHERE IdEleve = 82)
    LIMIT 1
);

-- ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
-- ÉTAPE 5 : Chaîne complète (diagnostic complet)
-- ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

SELECT 
    e.IdEleve,
    e.NomComplet AS NomEleve,
    e.IdTuteur AS IdTuteurEleve,
    t.NomComplet AS NomTuteur,
    t.Statut AS StatutTuteur,
    u.IdUtilisateur,
    u.NomUtilisateur,
    u.DefaultUsername,
    u.IdTuteur AS IdTuteurUtilisateur,
    u.Statut AS StatutUtilisateur,
    r.Nom AS RoleUtilisateur,
    ud.IdUserDevice,
    ud.FcmToken IS NOT NULL AS AUnToken,
    ud.Statut AS StatutDevice,
    CASE 
        WHEN e.IdTuteur IS NULL THEN '❌ Élève sans tuteur'
        WHEN t.Statut = 0 THEN '❌ Tuteur inactif'
        WHEN u.IdUtilisateur IS NULL THEN '❌ Tuteur sans compte utilisateur'
        WHEN u.Statut = 0 THEN '❌ Utilisateur inactif'
        WHEN ud.IdUserDevice IS NULL THEN '⚠️ Utilisateur sans device'
        WHEN ud.Statut = 0 THEN '❌ Device inactif'
        ELSE '✅ TOUT EST OK - Notification devrait fonctionner'
    END AS Diagnostic
FROM Eleves e
LEFT JOIN Tuteurs t ON e.IdTuteur = t.IdTuteur
LEFT JOIN Utilisateurs u ON t.IdTuteur = u.IdTuteur
LEFT JOIN Roles r ON u.IdRole = r.IdRole
LEFT JOIN UserDevices ud ON u.IdUtilisateur = ud.IdUtilisateur AND ud.Statut = 1
WHERE e.IdEleve = 82;

-- ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
-- RÉSUMÉ : Ce qu'on cherche
-- ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/*
Pour que la notification fonctionne, il faut :

1. ✅ Élève existe (IdEleve = 82)
2. ✅ Élève a un tuteur (IdTuteur NOT NULL)
3. ✅ Tuteur est actif (Statut = 1)
4. ✅ Tuteur a un compte utilisateur (Utilisateurs.IdTuteur = Tuteurs.IdTuteur)
5. ✅ Utilisateur est actif (Statut = 1)
6. ✅ Utilisateur a un device (UserDevices existe)
7. ✅ Device est actif (Statut = 1)

Si UN SEUL de ces critères est faux → Notification échoue
*/

