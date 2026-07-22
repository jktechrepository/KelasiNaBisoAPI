# 🚨 Guide complet de correction des erreurs base de données

**Date :** 2025-11-05  
**Statut :** 🔴 2 erreurs critiques bloquantes

---

## 📋 **Résumé des 2 erreurs**

| # | Erreur | Impact | Solution |
|---|--------|--------|----------|
| **1** | `InvalidCastException: String → Boolean` | ❌ Tous les endpoints avec `Statut` | Script SQL #1 |
| **2** | `Data too long for column 'NewValues'` | ❌ Mise à jour Agent/Eleve/etc. | Script SQL #2 |

---

## 🔴 **Erreur #1 : Champ Statut (String au lieu de Boolean)**

### **Symptôme :**
```
System.InvalidCastException: Unable to cast object of type 'System.String' to type 'System.Boolean'
```

### **Cause :**
Le champ `Statut` est en `VARCHAR("True"/"False")` au lieu de `TINYINT(1)` dans la base de données.

### **Endpoints affectés :**
- ❌ `/api/Dashboard/global`
- ❌ `/api/Presence/paged`
- ❌ `/api/Eleve/ecole/{id}`
- ❌ `/api/Agent`
- ❌ Pratiquement TOUS les endpoints !

---

## 🔴 **Erreur #2 : Colonne AuditLogs.NewValues trop petite**

### **Symptôme :**
```
MySqlConnector.MySqlException: Data too long for column 'NewValues' at row 1
```

### **Cause :**
La colonne `NewValues` dans la table `AuditLogs` est probablement en `TEXT` (max 65KB) au lieu de `LONGTEXT` (max 4GB).

Quand tu modifies un Agent avec beaucoup de données (photo, relations, etc.), le JSON d'audit dépasse 65KB.

### **Endpoints affectés :**
- ❌ `PUT /api/Agent/{id}` (mise à jour agent)
- ❌ `PUT /api/Eleve/{id}` (mise à jour élève)
- ❌ Tous les endpoints UPDATE avec beaucoup de données

---

## ✅ **SOLUTION : Exécuter 2 scripts SQL**

### **📂 Fichiers à exécuter dans HeidiSQL :**

1. **Script principal** : `APPLIQUER_MIGRATION_STATUT_NULLABLE.sql`
   - Corrige le problème #1 (Statut String → Boolean)
   - Corrige aussi les GUID invalides
   - Ajoute ReferenceTransaction si manquante

2. **Script audit** : `FIX_AUDIT_NEWVALUES_COLUMN.sql`
   - Corrige le problème #2 (NewValues trop petit)
   - Convertit TEXT → LONGTEXT

---

## 🛠️ **Procédure d'exécution dans HeidiSQL**

### **Étape 1 : Ouvrir HeidiSQL**
1. Lance **HeidiSQL**
2. Connecte-toi à ta base de données MariaDB
3. Sélectionne la base : `kelasinabiso`

---

### **Étape 2 : Exécuter le script #1 (Statut)**

1. **Fichier > Charger fichier SQL**
2. **Sélectionne** : `G:\KelasiNaBiso\KelasiNaBisoAPI\APPLIQUER_MIGRATION_STATUT_NULLABLE.sql`
3. **Exécute** (F9 ou bouton ▶️)
4. **Attends** ~30-60 secondes
5. **Vérifie** : Tu devrais voir des messages de succès

**Vérification :**
```sql
SHOW COLUMNS FROM Presences WHERE Field = 'Statut';
```

**Résultat attendu :**
```
Field   | Type          | Null
--------|---------------|------
Statut  | tinyint(1)    | YES
```

✅ Si tu vois `tinyint(1)`, c'est bon !

---

### **Étape 3 : Exécuter le script #2 (AuditLogs)**

1. **Nouvel onglet de requête** (Ctrl+T)
2. **Fichier > Charger fichier SQL**
3. **Sélectionne** : `G:\KelasiNaBiso\KelasiNaBisoAPI\FIX_AUDIT_NEWVALUES_COLUMN.sql`
4. **Exécute** (F9)
5. **Vérifie** les résultats

**Vérification :**
```sql
SHOW COLUMNS FROM AuditLogs WHERE Field = 'NewValues';
```

**Résultat attendu :**
```
Field      | Type       | Null
-----------|------------|------
NewValues  | longtext   | YES
```

✅ Si tu vois `longtext`, c'est bon !

---

## 🚀 **Après avoir exécuté les 2 scripts**

### **Relance l'application :**
```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run
```

### **Teste les endpoints dans Swagger :**

1. **Dashboard global :**
   ```http
   GET /api/Dashboard/global?idEcole=13
   ```
   **Résultat attendu :** ✅ Code 200 avec JSON complet

2. **Présences paginées :**
   ```http
   GET /api/Presence/paged?PageNumber=1&PageSize=15
   ```
   **Résultat attendu :** ✅ Code 200 avec liste de présences

3. **Mise à jour Agent :**
   ```http
   PUT /api/Agent/3
   ```
   **Résultat attendu :** ✅ Code 200 sans erreur d'audit

---

## 📊 **Tailles des colonnes**

### **Avant (problématique) :**
```
TEXT          = 65,535 octets (65 KB)  ❌ Trop petit pour audit complexe
VARCHAR(10)   = 10 caractères          ❌ Trop petit pour boolean JSON
```

### **Après (corrigé) :**
```
LONGTEXT      = 4,294,967,295 octets (4 GB)  ✅ Suffisant pour audit
TINYINT(1)    = 1 ou 0 ou NULL               ✅ Format boolean correct
```

---

## 🎯 **Checklist complète**

### **À faire MAINTENANT :**
- [ ] Ouvrir HeidiSQL
- [ ] Exécuter `APPLIQUER_MIGRATION_STATUT_NULLABLE.sql`
- [ ] Exécuter `FIX_AUDIT_NEWVALUES_COLUMN.sql`
- [ ] Vérifier avec `SHOW COLUMNS`
- [ ] Relancer l'application
- [ ] Tester `/api/Dashboard/global?idEcole=13`
- [ ] Tester `/api/Presence/paged`
- [ ] Tester `PUT /api/Agent/3`

### **Résultats attendus après migration :**
- ✅ Aucune erreur `InvalidCastException`
- ✅ Aucune erreur `Data too long`
- ✅ Dashboard global fonctionne
- ✅ Mise à jour Agent fonctionne
- ✅ Audit enregistré correctement

---

**Es-tu prêt à exécuter les 2 scripts SQL dans HeidiSQL maintenant ?** 💾

Une fois fait, je relancerai l'application et on testera tout ensemble ! 😊
