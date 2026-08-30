# 🛠️ Instructions - Exécution du script SQL

**Fichier :** `APPLIQUER_MIGRATION_STATUT_NULLABLE.sql`  
**Durée estimée :** 1-2 minutes  
**Impact :** ✅ Résout tous les `InvalidCastException`

---

## 🎯 **Pourquoi exécuter ce script ?**

Ce script corrige les différences entre le code C# (qui attend des colonnes `NULL`) et la base de données (qui contient des colonnes `NOT NULL` ou des valeurs invalides).

**Sans ce script, tu auras :**
- ❌ `InvalidCastException: Unable to cast DBNull to Int32`
- ❌ `InvalidCastException: Unable to cast DBNull to Boolean`
- ❌ `FormatException` sur les GUID invalides
- ❌ Notifications push qui échouent
- ❌ Certaines requêtes qui plantent

---

## 📋 **Ce que le script fait**

### **1. Colonnes `Statut` (VARCHAR → TINYINT(1) NULL)**

Convertit toutes les colonnes `Statut` de `VARCHAR` ou `TINYINT(1) NOT NULL` vers `TINYINT(1) NULL`.

**Tables concernées :**
- `Vacations`, `Utilisateurs`, `UserDevices`, `Tuteurs`, `TitulaireClasses`
- `Sections`, `RolesPermissions`, `Roles`, `Presences`, `Permissions`
- `Options`, `Notes`, `Messages`, `InscriptionEleve`, `Horaires`
- `GroupeMessages`, `Frais`, `Evaluations`, `Documents`, `Directions`
- `Cours`, `Classes`, `AnneeScolaires`, `Agents`, `Ecoles`, `Eleves`
- `AffectationsCours`, `Paiements`, `Inscriptions`, `Notifications`

### **2. Colonne `Niveau` dans `Roles` (INT → INT NULL)**

Permet `NULL` dans la colonne `Niveau` de la table `Roles`.

### **3. Colonnes GUID (nettoyage des valeurs invalides)**

Convertit les GUID invalides (`''`, `'0000...'`, longueur incorrecte) en `NULL` :
- `ReferenceEleve` dans `Eleves`
- `ReferenceUtilisateur` dans `Utilisateurs`

### **4. Colonne `ReferenceTransaction` dans `Paiements`**

Ajoute la colonne si elle n'existe pas :
```sql
ALTER TABLE Paiements ADD COLUMN ReferenceTransaction VARCHAR(255) NULL;
```

### **5. Colonne `NewValues` dans `AuditLogs`**

Agrandit la colonne pour éviter les erreurs "Data too long" :
```sql
ALTER TABLE AuditLogs MODIFY COLUMN NewValues LONGTEXT NULL;
```

---

## 🚀 **Comment exécuter**

### **Étape 1 : Ouvrir HeidiSQL**

1. Lance **HeidiSQL**
2. Connecte-toi à ta base de données **kelasinabiso**

### **Étape 2 : Ouvrir le script**

1. Clique sur **Fichier** → **Ouvrir**
2. Navigue vers `G:\KelasiNaBiso\KelasiNaBisoAPI\`
3. Sélectionne **`APPLIQUER_MIGRATION_STATUT_NULLABLE.sql`**
4. Clique sur **Ouvrir**

### **Étape 3 : Exécuter le script**

1. Le script s'ouvre dans l'onglet de requête
2. Clique sur **▶️ Exécuter** (F9)
3. Attends la fin de l'exécution (1-2 minutes)

### **Étape 4 : Vérifier le résultat**

Tu devrais voir :
```
✅ Query OK, 0 rows affected
✅ Transaction committed
```

---

## ⚠️ **IMPORTANT**

### **Avant l'exécution :**
- 🔴 **ARRÊTE l'API** si elle tourne
- 💾 **Fais un backup de la DB** (recommandé)

### **Après l'exécution :**
- ✅ Redémarre l'API
- ✅ Teste les fonctionnalités (présence, paiement, etc.)

---

## 🧪 **Vérification après exécution**

### **Test rapide dans HeidiSQL :**

```sql
-- Vérifier que Statut est bien TINYINT(1) NULL
DESCRIBE Utilisateurs;
DESCRIBE Eleves;
DESCRIBE Presences;

-- Vérifier que les GUID invalides sont nettoyés
SELECT COUNT(*) AS GuidInvalides
FROM Eleves
WHERE ReferenceEleve IS NULL;

SELECT COUNT(*) AS GuidInvalides
FROM Utilisateurs
WHERE ReferenceUtilisateur IS NULL;
```

---

## ✅ **Résultats attendus**

Après l'exécution du script :

1. ✅ Toutes les colonnes `Statut` sont `TINYINT(1) NULL`
2. ✅ `Roles.Niveau` est `INT NULL`
3. ✅ GUID invalides sont `NULL`
4. ✅ `Paiements.ReferenceTransaction` existe
5. ✅ `AuditLogs.NewValues` est `LONGTEXT`
6. ✅ Plus d'`InvalidCastException` !
7. ✅ API fonctionne normalement
8. ✅ Notifications push fonctionnelles (après correction Firebase)

---

## 🔥 **Firebase ensuite**

Une fois le script SQL exécuté, on pourra se concentrer sur :
- Comprendre pourquoi le code d'initialisation Firebase ne s'exécute pas
- Voir les logs détaillés qu'on a ajoutés
- Tester les notifications push

---

**Exécute le script maintenant et dis-moi quand c'est fait ! 🚀**

