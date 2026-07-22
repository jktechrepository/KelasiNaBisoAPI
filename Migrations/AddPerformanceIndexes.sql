-- ══════════════════════════════════════════════════════════
-- MIGRATION PERFORMANCE - INDEX CRITIQUES
-- Date : 1er novembre 2025
-- Objectif : Améliorer les performances de 200-400%
-- ══════════════════════════════════════════════════════════

-- ══════════════════════════════════════════════════════════
-- 1. INDEX SUR CLÉS ÉTRANGÈRES (CRITIQUE)
-- ══════════════════════════════════════════════════════════

-- Paiements (table très sollicitée)
CREATE INDEX IF NOT EXISTS IX_Paiements_IdEleve 
ON Paiements(IdEleve);

CREATE INDEX IF NOT EXISTS IX_Paiements_IdFrais 
ON Paiements(IdFrais);

CREATE INDEX IF NOT EXISTS IX_Paiements_IdUtilisateur 
ON Paiements(IdUtilisateur);

-- Inscriptions
CREATE INDEX IF NOT EXISTS IX_Inscriptions_IdEleve 
ON Inscriptions(IdEleve);

CREATE INDEX IF NOT EXISTS IX_Inscriptions_IdClasse 
ON Inscriptions(IdClasse);

CREATE INDEX IF NOT EXISTS IX_Inscriptions_IdAnneeScolaire 
ON Inscriptions(IdAnneeScolaire);

CREATE INDEX IF NOT EXISTS IX_Inscriptions_IdEcole 
ON Inscriptions(IdEcole);

-- Notes (très sollicitées)
CREATE INDEX IF NOT EXISTS IX_Notes_IdEleve 
ON Notes(IdEleve);

CREATE INDEX IF NOT EXISTS IX_Notes_IdCours 
ON Notes(IdCours);

CREATE INDEX IF NOT EXISTS IX_Notes_IdAnneeScolaire 
ON Notes(IdAnneeScolaire);

CREATE INDEX IF NOT EXISTS IX_Notes_IdProfesseur 
ON Notes(IdProfesseur);

-- Présences (pointages quotidiens)
CREATE INDEX IF NOT EXISTS IX_Presences_IdEleve 
ON Presences(IdEleve);

CREATE INDEX IF NOT EXISTS IX_Presences_IdAgent 
ON Presences(IdAgent);

CREATE INDEX IF NOT EXISTS IX_Presences_IdVacation 
ON Presences(IdVacation);

-- Eleves (table centrale)
CREATE INDEX IF NOT EXISTS IX_Eleves_IdTuteur 
ON Eleves(IdTuteur);

CREATE INDEX IF NOT EXISTS IX_Eleves_IdClasse 
ON Eleves(IdClasse);

CREATE INDEX IF NOT EXISTS IX_Eleves_IdEcole 
ON Eleves(IdEcole);

-- Classes
CREATE INDEX IF NOT EXISTS IX_Classes_IdDirection 
ON Classes(IdDirection);

CREATE INDEX IF NOT EXISTS IX_Classes_IdSection 
ON Classes(IdSection);

CREATE INDEX IF NOT EXISTS IX_Classes_IdOption 
ON Classes(IdOption);

-- Affectations Cours
CREATE INDEX IF NOT EXISTS IX_AffectationsCours_IdAgent 
ON AffectationsCours(IdAgent);

CREATE INDEX IF NOT EXISTS IX_AffectationsCours_IdCours 
ON AffectationsCours(IdCours);

CREATE INDEX IF NOT EXISTS IX_AffectationsCours_IdAnneeScolaire 
ON AffectationsCours(IdAnneeScolaire);

-- Evaluations
CREATE INDEX IF NOT EXISTS IX_Evaluations_IdCours 
ON Evaluations(IdCours);

CREATE INDEX IF NOT EXISTS IX_Evaluations_IdClasse 
ON Evaluations(IdClasse);

-- Cours
CREATE INDEX IF NOT EXISTS IX_Cours_IdClasse 
ON Cours(IdClasse);

-- ══════════════════════════════════════════════════════════
-- 2. INDEX COMPOSITES POUR REQUÊTES COURANTES
-- ══════════════════════════════════════════════════════════

-- Paiements par école et date (rapports)
CREATE INDEX IF NOT EXISTS IX_Paiements_Ecole_Date 
ON Paiements(IdUtilisateur, DatePaiement DESC);

-- Notes par élève et année (bulletins)
CREATE INDEX IF NOT EXISTS IX_Notes_Eleve_Annee_Cours 
ON Notes(IdEleve, IdAnneeScolaire, IdCours);

-- Présences par date (pointages quotidiens)
CREATE INDEX IF NOT EXISTS IX_Presences_Date_Type 
ON Presences(DatePresence DESC, TypePresence);

-- Inscriptions actives par école
CREATE INDEX IF NOT EXISTS IX_Inscriptions_Ecole_Statut 
ON Inscriptions(IdEcole, StatutInscription, Statut);

-- Elèves actifs par classe
CREATE INDEX IF NOT EXISTS IX_Eleves_Classe_Statut 
ON Eleves(IdClasse, Statut);

-- ══════════════════════════════════════════════════════════
-- 3. INDEX SUR COLONNES DE RECHERCHE
-- ══════════════════════════════════════════════════════════

-- Recherche par nom (Full-Text à considérer plus tard)
CREATE INDEX IF NOT EXISTS IX_Eleves_NomComplet 
ON Eleves(NomComplet(100)); -- Index partiel sur 100 premiers caractères

CREATE INDEX IF NOT EXISTS IX_Agents_Nom 
ON Agents(Nom(50), Postnom(50));

CREATE INDEX IF NOT EXISTS IX_Tuteurs_NomComplet 
ON Tuteurs(NomComplet(100));

-- Recherche par statut (filtres courants)
CREATE INDEX IF NOT EXISTS IX_Paiements_StatutPaiement 
ON Paiements(StatutPaiement, DatePaiement DESC);

CREATE INDEX IF NOT EXISTS IX_Inscriptions_StatutInscription 
ON Inscriptions(StatutInscription, DateInscription DESC);

-- Recherche par référence
CREATE INDEX IF NOT EXISTS IX_Paiements_ReferenceTransaction 
ON Paiements(ReferenceTransaction);

CREATE INDEX IF NOT EXISTS IX_Paiements_ReferencePaiemenet 
ON Paiements(ReferencePaiemenet);

-- ══════════════════════════════════════════════════════════
-- 4. INDEX SUR CHAMPS DE TRI FRÉQUENTS
-- ══════════════════════════════════════════════════════════

CREATE INDEX IF NOT EXISTS IX_Paiements_DatePaiement 
ON Paiements(DatePaiement DESC);

CREATE INDEX IF NOT EXISTS IX_Presences_DatePresence 
ON Presences(DatePresence DESC);

CREATE INDEX IF NOT EXISTS IX_Inscriptions_DateInscription 
ON Inscriptions(DateInscription DESC);

CREATE INDEX IF NOT EXISTS IX_Notes_DateEvaluation 
ON Notes(DateEvaluation DESC);

-- ══════════════════════════════════════════════════════════
-- 5. INDEX SUR COLONNES STATUT (SOFT DELETE)
-- ══════════════════════════════════════════════════════════

CREATE INDEX IF NOT EXISTS IX_Eleves_Statut 
ON Eleves(Statut);

CREATE INDEX IF NOT EXISTS IX_Agents_Statut 
ON Agents(Statut);

CREATE INDEX IF NOT EXISTS IX_Utilisateurs_Statut 
ON Utilisateurs(Statut);

CREATE INDEX IF NOT EXISTS IX_Paiements_Statut 
ON Paiements(Statut);

-- ══════════════════════════════════════════════════════════
-- RÉSUMÉ
-- ══════════════════════════════════════════════════════════
-- Total Index créés : ~45
-- Impact estimé : Requêtes 10-100x plus rapides
-- Espace disque : +5-10% (acceptable)
-- Temps d'exécution : ~2-5 minutes
-- ══════════════════════════════════════════════════════════

