# 📋 Guide d'Exécution : Correction des Incohérences Inscriptions/Élèves

**Date** : 2025-01-16  
**Version** : 1.0  
**Phase** : Phase 3 - Correction des données existantes

---

## ⚠️ AVERTISSEMENTS IMPORTANTS

1. **BACKUP OBLIGATOIRE** : Faire un backup complet de la base de données avant toute exécution
2. **TEST EN PREMIER** : Exécuter d'abord sur une base de test si possible
3. **VALIDATION** : Valider les résultats de chaque étape avant de continuer
4. **TRANSACTION** : Le script utilise des transactions pour permettre l'annulation

---

## 📋 Checklist Pré-Exécution

Avant de commencer, vérifiez que vous avez :

- [ ] Fait un **backup complet** de la base de données
- [ ] Exécuté le script de diagnostic (`identifier_incohérences_inscriptions_eleves_inactifs.sql`)
- [ ] Analysé les résultats du diagnostic
- [ ] Validé que la correction est nécessaire
- [ ] Accès en écriture à la base de données
- [ ] Outil pour exécuter les scripts SQL (MySQL Workbench, phpMyAdmin, etc.)

---

## 🔄 Étapes d'Exécution

### **Étape 0 : Backup** ⚠️ OBLIGATOIRE

**Méthode 1 : Via ligne de commande**
```bash
mysqldump -u [USER] -p [DATABASE_NAME] > backup_avant_correction_inscriptions_$(date +%Y%m%d_%H%M%S).sql
```

**Méthode 2 : Via MySQL Workbench**
1. Ouvrir MySQL Workbench
2. Server → Data Export
3. Sélectionner la base de données
4. Exporter vers un fichier SQL

**Méthode 3 : Via phpMyAdmin**
1. Sélectionner la base de données
2. Cliquer sur "Exporter"
3. Choisir "SQL" comme format
4. Cliquer sur "Exécuter"

---

### **Étape 1 : Vérification Avant Correction** ✅

**Objectif** : Voir exactement ce qui sera modifié

**Action** : Exécuter la section "ÉTAPE 1" du script SQL

**Résultat attendu** : Liste des inscriptions qui seront désactivées

**Vérifications** :
- [ ] Le nombre d'inscriptions à désactiver est raisonnable
- [ ] Les inscriptions listées correspondent bien à des élèves inactifs
- [ ] Aucune inscription importante n'est concernée par erreur

**Si tout est correct** → Passer à l'Étape 2  
**Si problème** → Analyser et corriger avant de continuer

---

### **Étape 1B : Résumé Avant Correction** ✅

**Objectif** : Avoir une vue d'ensemble

**Action** : Exécuter la section "ÉTAPE 1B" du script SQL

**Résultat attendu** : Statistiques globales

**Exemple de résultat** :
```
Etape                          | NombreInscriptionsADesactiver | NombreElevesInactifsConcernes | NombreEcolesConcernées
AVANT CORRECTION              | 25                            | 15                            | 3
```

---

### **Étape 2 : Correction** 🔄

**Objectif** : Désactiver les inscriptions d'élèves inactifs

**Action** : 
1. **Décommenter** la section "ÉTAPE 2" du script SQL
2. **Exécuter** la requête UPDATE

**Code à décommenter** :
```sql
START TRANSACTION;

UPDATE Inscriptions i
INNER JOIN Eleves e ON i.IdEleve = e.IdEleve
SET i.Statut = 0
WHERE 
    (i.Statut = 1 OR i.Statut IS NULL)
    AND 
    (e.Statut = 0 OR e.Statut IS NULL);

SELECT ROW_COUNT() AS NombreInscriptionsDesactivees;

COMMIT;
```

**Vérifications** :
- [ ] Le nombre de lignes affectées correspond au nombre attendu
- [ ] Aucune erreur SQL
- [ ] La transaction est validée (COMMIT)

**Si problème** → Utiliser `ROLLBACK;` pour annuler

---

### **Étape 3 : Vérification Après Correction** ✅

**Objectif** : Confirmer qu'il n'y a plus d'incohérences

**Action** : Exécuter la section "ÉTAPE 3" du script SQL

**Résultat attendu** :
```
Etape              | NombreInscriptionsActivesAvecElevesInactifs | NombreElevesInactifsConcernes | NombreEcolesConcernées
APRÈS CORRECTION  | 0                                           | 0                             | 0
```

**Si le résultat est 0** → ✅ Correction réussie  
**Si le résultat > 0** → ❌ Il reste des incohérences, analyser pourquoi

---

### **Étape 4 : Statistiques par École** ✅

**Objectif** : Vérifier la cohérence par école

**Action** : Exécuter la section "ÉTAPE 4" du script SQL

**Résultat attendu** : 
- Aucune ligne retournée (ou toutes les différences à 0)
- Toutes les écoles ont `DifferenceIncoherence = 0`

---

### **Étape 5 : Vérification Finale** ✅

**Objectif** : Confirmation globale

**Action** : Exécuter la section "ÉTAPE 5" du script SQL

**Résultat attendu** :
```
Type                  | TotalElevesActifs | TotalInscriptionsActivesAvecElevesActifs | TotalInscriptionsActivesAvecElevesInactifs | StatutCorrection
VÉRIFICATION FINALE   | 500               | 450                                     | 0                                          | ✅ CORRECTION RÉUSSIE
```

**Vérifications** :
- [ ] `TotalInscriptionsActivesAvecElevesInactifs = 0`
- [ ] `StatutCorrection = '✅ CORRECTION RÉUSSIE'`

---

## 🔍 Exemple d'Exécution Complète

### **Avant Correction**

```sql
-- Étape 1B : Résumé
Etape              | NombreInscriptionsADesactiver | NombreElevesInactifsConcernes | NombreEcolesConcernées
AVANT CORRECTION   | 25                            | 15                            | 3
```

### **Correction**

```sql
-- Étape 2 : UPDATE
NombreInscriptionsDesactivees
25
```

### **Après Correction**

```sql
-- Étape 3 : Vérification
Etape              | NombreInscriptionsActivesAvecElevesInactifs | NombreElevesInactifsConcernes | NombreEcolesConcernées
APRÈS CORRECTION   | 0                                           | 0                             | 0

-- Étape 5 : Vérification finale
Type                  | TotalElevesActifs | TotalInscriptionsActivesAvecElevesActifs | TotalInscriptionsActivesAvecElevesInactifs | StatutCorrection
VÉRIFICATION FINALE   | 500               | 450                                     | 0                                          | ✅ CORRECTION RÉUSSIE
```

---

## ⚠️ Gestion des Erreurs

### **Erreur : "Lock wait timeout exceeded"**

**Cause** : Transaction en cours ou verrous sur les tables

**Solution** :
1. Vérifier qu'aucune autre transaction n'est en cours
2. Attendre quelques secondes et réessayer
3. Vérifier les processus MySQL actifs

---

### **Erreur : "Cannot update Inscriptions table"**

**Cause** : Permissions insuffisantes

**Solution** :
1. Vérifier que l'utilisateur a les droits UPDATE
2. Utiliser un utilisateur avec les permissions appropriées

---

### **Résultat inattendu après correction**

**Si des incohérences persistent** :
1. Vérifier la logique des conditions WHERE
2. Vérifier les valeurs NULL dans votre base
3. Exécuter à nouveau le script de diagnostic
4. Analyser les cas spécifiques

---

## 📊 Validation Post-Correction

Après la correction, validez que :

- [ ] Le script de diagnostic ne retourne plus d'incohérences
- [ ] Les rapports Dashboard affichent des données cohérentes
- [ ] Le nombre d'inscriptions actives ≤ nombre d'élèves actifs
- [ ] Les endpoints d'inscriptions fonctionnent correctement

---

## 🔄 Rollback (Annulation)

Si vous devez annuler la correction :

**Option 1 : Via Transaction (si pas encore COMMIT)**
```sql
ROLLBACK;
```

**Option 2 : Via Backup**
```bash
mysql -u [USER] -p [DATABASE_NAME] < backup_avant_correction_inscriptions_[DATE].sql
```

---

## 📝 Notes Finales

1. **Logique Métier** : Ce script désactive les inscriptions d'élèves inactifs. Si vous réactivez un élève plus tard, ses inscriptions resteront inactives. Vous devrez peut-être les réactiver manuellement.

2. **Performance** : Pour de grandes bases de données, l'UPDATE peut prendre quelques minutes. Soyez patient.

3. **Index** : Assurez-vous qu'il y a des index sur `Inscriptions.IdEleve` et `Eleves.Statut` pour optimiser les performances.

4. **Suivi** : Documentez le nombre d'inscriptions désactivées pour référence future.

---

## ✅ Checklist Post-Exécution

- [ ] Backup sauvegardé en lieu sûr
- [ ] Correction exécutée avec succès
- [ ] Vérifications post-correction passées
- [ ] Aucune incohérence restante
- [ ] Rapports Dashboard cohérents
- [ ] Documentation mise à jour

---

**Version** : 1.0  
**Date** : 2025-01-16  
**Statut** : ✅ Script prêt pour exécution
