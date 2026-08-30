# 🧪 Guide de Test : Validation des Corrections du Calcul du Nombre d'Élèves

**Date** : 2025-01-16  
**Version** : 1.0  
**Statut** : ✅ Corrections appliquées - Prêt pour les tests

---

## 🎯 Objectif des Tests

Valider que les corrections apportées au calcul du nombre d'élèves fonctionnent correctement et que :
- `nombreEleves` compte **tous les élèves** (actifs + inactifs)
- `nombreElevesActifs` compte **uniquement les actifs**
- Les répartitions utilisent les bons totaux pour les pourcentages

---

## 📋 Prérequis

1. ✅ API en cours d'exécution
2. ✅ Token JWT valide (obtenu via authentification)
3. ✅ Base de données avec des élèves actifs et inactifs pour tester

---

## 🚀 Méthode de Test

### **Option 1 : Via Fichier HTTP (Recommandé)**

1. **Ouvrir** : `test-dashboard-nombre-eleves.http` dans VS Code
2. **Authentifier** : Exécuter l'ÉTAPE 0 pour obtenir le token
3. **Configurer** : Remplacer `YOUR_JWT_TOKEN_HERE` par votre token
4. **Tester** : Exécuter les tests dans l'ordre

### **Option 2 : Via Swagger**

1. Ouvrir : `https://votre-api.com/swagger`
2. Authentifier via `POST /api/Utilisateur/authentifier`
3. Tester les endpoints Dashboard

---

## 📊 Tests à Effectuer

### **Test 1 : Dashboard Global**

**Endpoint** :
```http
GET /api/Dashboard/global?idEcole=1
Authorization: Bearer {token}
```

**Vérifications** :
- ✅ `statistiques.nombreEleves` = Tous les élèves (actifs + inactifs)
- ✅ `statistiques.nombreElevesActifs` = Uniquement les actifs
- ✅ `nombreEleves >= nombreElevesActifs` (toujours vrai)
- ✅ `repartitionEleves.totalEleves` = `statistiques.nombreEleves`
- ✅ `repartitionEleves.totalElevesActifs` = `statistiques.nombreElevesActifs`

**Exemple de Résultat Attendu** :
```json
{
  "statistiques": {
    "nombreEleves": 120,        // ✅ Tous les élèves
    "nombreElevesActifs": 100   // ✅ Uniquement actifs
  },
  "repartitionEleves": {
    "totalEleves": 120,         // ✅ Doit correspondre
    "totalElevesActifs": 100    // ✅ Doit correspondre
  }
}
```

---

### **Test 2 : Dashboard Super-Admin**

**Endpoint** :
```http
GET /api/Dashboard/super-admin
Authorization: Bearer {token}
```

**Vérifications** :
- ✅ `statistiquesGlobales.totalEleves` = Tous les élèves (actifs + inactifs)
- ✅ `statistiquesGlobales.totalElevesActifs` = Uniquement les actifs
- ✅ `totalEleves >= totalElevesActifs`
- ✅ `parEcole[].nombreEleves` = Tous les élèves par école
- ✅ `parEcole[].nombreElevesActifs` = Uniquement les actifs par école

---

### **Test 3 : Vérification des Répartitions**

**Endpoint** :
```http
GET /api/Dashboard/global?idEcole=1
Authorization: Bearer {token}
```

**Vérifications** :
- ✅ `repartitionEleves.parDirection[].nombreEleves` = Total (actifs + inactifs)
- ✅ `repartitionEleves.parDirection[].nombreElevesActifs` = Uniquement actifs
- ✅ `repartitionEleves.parSection[].nombreEleves` = Total (actifs + inactifs)
- ✅ `repartitionEleves.parSection[].nombreElevesActifs` = Uniquement actifs
- ✅ `repartitionEleves.parOption[].nombreEleves` = Total (actifs + inactifs)
- ✅ `repartitionEleves.parOption[].nombreElevesActifs` = Uniquement actifs
- ✅ La somme des répartitions = `totalEleves` (approximativement, car certains élèves peuvent ne pas avoir de direction/section/option)

---

### **Test 4 : Vérification des Pourcentages**

**Endpoint** :
```http
GET /api/Dashboard/global?idEcole=1
Authorization: Bearer {token}
```

**Vérifications** :
- ✅ `repartitionEleves.parDirection[].pourcentage` = Calculé sur `totalEleves`
- ✅ `repartitionEleves.parSection[].pourcentage` = Calculé sur `totalEleves`
- ✅ `repartitionEleves.parOption[].pourcentage` = Calculé sur `totalEleves`
- ✅ La somme des pourcentages ≈ 100% (arrondi)

**Exemple** :
```json
{
  "repartitionEleves": {
    "totalEleves": 120,
    "parDirection": [
      {
        "nombreEleves": 50,
        "pourcentage": 41.67  // ✅ 50/120 = 41.67%
      }
    ]
  }
}
```

---

### **Test 5 : Comparaison avec Metrics**

**Endpoint** :
```http
GET /api/Metrics/general
Authorization: Bearer {token}
```

**Vérifications** :
- ✅ `eleves.total` = Tous les élèves (sans filtre par école)
- ✅ `eleves.actifs` = Uniquement les actifs
- ✅ Comparer avec Dashboard Super-Admin pour cohérence

**Note** : Metrics compte tous les élèves de toutes les écoles, tandis que Dashboard compte par école.

---

## 🔍 Points Critiques à Vérifier

### **1. Cohérence des Totaux**

```
nombreEleves >= nombreElevesActifs (toujours vrai)
```

**Si égal** : Tous les élèves sont actifs (cas normal)

**Si différent** : Il y a des élèves inactifs (cas à vérifier)

---

### **2. Correspondance des Totaux**

```
statistiques.nombreEleves == repartitionEleves.totalEleves
statistiques.nombreElevesActifs == repartitionEleves.totalElevesActifs
```

**Si différent** : Problème de cohérence entre les méthodes

---

### **3. Calcul des Pourcentages**

```
pourcentage = (nombreEleves / totalEleves) * 100
```

**Vérifier** : Les pourcentages sont calculés sur `totalEleves` (tous statuts), pas sur `totalElevesActifs`

---

### **4. Dashboard Paiement**

**Note** : Le Dashboard Paiement compte uniquement les élèves actifs (logique métier : seuls les actifs doivent payer)

```
resume.nombreEleves == statistiques.nombreElevesActifs (pour la même école)
```

---

## 📝 Exemple de Test Complet

### **Scénario** : École avec 100 élèves actifs + 20 inactifs = 120 total

**Test 1 : Dashboard Global**
```json
{
  "statistiques": {
    "nombreEleves": 120,        // ✅ Correct (tous)
    "nombreElevesActifs": 100   // ✅ Correct (actifs)
  },
  "repartitionEleves": {
    "totalEleves": 120,         // ✅ Correct (correspond à nombreEleves)
    "totalElevesActifs": 100,   // ✅ Correct (correspond à nombreElevesActifs)
    "parDirection": [
      {
        "nombreEleves": 50,     // ✅ Total (actifs + inactifs)
        "nombreElevesActifs": 45, // ✅ Uniquement actifs
        "pourcentage": 41.67    // ✅ Calculé sur 120 (50/120)
      }
    ]
  }
}
```

**Test 2 : Dashboard Paiement**
```json
{
  "resume": {
    "nombreEleves": 100,        // ✅ Correct (uniquement actifs - logique métier)
    "elevesAyantPaye": 80,
    "elevesEnRetard": 20        // ✅ 100 - 80 = 20
  }
}
```

**Test 3 : Dashboard Super-Admin**
```json
{
  "statistiquesGlobales": {
    "totalEleves": 500,         // ✅ Tous les élèves (toutes écoles)
    "totalElevesActifs": 450    // ✅ Uniquement actifs
  },
  "parEcole": [
    {
      "nombreEleves": 120,      // ✅ Tous les élèves de l'école
      "nombreElevesActifs": 100 // ✅ Uniquement actifs
    }
  ]
}
```

---

## ✅ Checklist de Validation

- [ ] Test 1 : Dashboard Global - `nombreEleves >= nombreElevesActifs`
- [ ] Test 1 : Dashboard Global - `totalEleves == nombreEleves`
- [ ] Test 1 : Dashboard Global - `totalElevesActifs == nombreElevesActifs`
- [ ] Test 2 : Dashboard Super-Admin - `totalEleves >= totalElevesActifs`
- [ ] Test 3 : Répartitions - Utilisent `totalEleves` pour les pourcentages
- [ ] Test 4 : Pourcentages - Calculés sur `totalEleves`
- [ ] Test 5 : Metrics - Cohérence avec Dashboard Super-Admin

---

## 🐛 Dépannage

### **Problème : `nombreEleves == nombreElevesActifs` toujours**

**Cause** : Tous les élèves sont actifs (cas normal) OU la correction n'a pas été appliquée

**Vérification** :
1. Vérifier qu'il y a des élèves inactifs en base de données
2. Vérifier que le code a été recompilé
3. Vérifier que l'API a été redémarrée

---

### **Problème : `totalEleves != nombreEleves`**

**Cause** : Incohérence entre les méthodes

**Vérification** :
1. Vérifier que les deux utilisent la même requête de base
2. Vérifier qu'il n'y a pas de filtres supplémentaires

---

### **Problème : Pourcentages incorrects**

**Cause** : Calcul sur `totalElevesActifs` au lieu de `totalEleves`

**Vérification** :
1. Vérifier que les pourcentages sont calculés sur `totalEleves`
2. Vérifier la formule : `(nombreEleves / totalEleves) * 100`

---

## 📊 Résumé des Corrections Appliquées

1. ✅ **Méthode helper créée** : `GetElevesQueries()` pour standardiser
2. ✅ **CalculerStatistiquesGeneralesAsync** : Corrigé
3. ✅ **CalculerRepartitionElevesAsync** : Corrigé
4. ✅ **CalculerStatistiquesGeneralesGlobalesAsync** : Corrigé
5. ✅ **CalculerRepartitionsGeographiquesAsync** : Corrigé
6. ✅ **PaiementService.GetDashboardEcoleAsync** : Amélioré

---

**Version** : 1.0  
**Date** : 2025-01-16  
**Statut** : ✅ Prêt pour les tests
