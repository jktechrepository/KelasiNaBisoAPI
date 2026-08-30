# 🚨 URGENT : Migration base de données requise

## ❌ **Problème actuel**

```
System.InvalidCastException: Unable to cast object of type 'System.String' to type 'System.Boolean'.
```

**Cause :** Le champ `Statut` dans la base de données est toujours en **`VARCHAR`** avec des valeurs comme `"True"`, `"False"` (chaînes de caractères) au lieu de **`TINYINT(1)`** avec `0` ou `1`.

---

## ✅ **Solution : Exécuter le script SQL**

### **Étape 1 : Ouvrir HeidiSQL**

1. Lance **HeidiSQL**
2. Connecte-toi à ta base de données MariaDB

---

### **Étape 2 : Ouvrir le script**

1. Dans HeidiSQL, va dans **Fichier > Charger fichier SQL**
2. Sélectionne : `G:\KelasiNaBiso\KelasiNaBisoAPI\APPLIQUER_MIGRATION_STATUT_NULLABLE.sql`

---

### **Étape 3 : Exécuter le script**

1. Clique sur **Exécuter** (ou F9)
2. Attends la fin de l'exécution (~30 secondes)

---

### **Étape 4 : Vérifier**

Exécute cette requête pour vérifier que `Statut` est bien en `TINYINT` :

```sql
SHOW COLUMNS FROM Presences WHERE Field = 'Statut';
```

**Résultat attendu :**
```
Field   | Type          | Null | Key | Default | Extra
--------|---------------|------|-----|---------|------
Statut  | tinyint(1)    | YES  |     | NULL    |
```

---

## 📋 **Ce que fait le script**

Le script `APPLIQUER_MIGRATION_STATUT_NULLABLE.sql` :

1. ✅ **Convertit tous les champs `Statut`** de `VARCHAR` → `TINYINT(1) NULL`
2. ✅ **Convertit les données** : `"True"` → `1`, `"False"` → `0`, `NULL` → `NULL`
3. ✅ **Corrige les GUID invalides** dans `ReferenceEleve` et `ReferenceUtilisateur`
4. ✅ **Ajoute `ReferenceTransaction`** dans la table `Paiements` si manquante
5. ✅ **Enregistre les migrations** dans `__EFMigrationsHistory`

---

## ⚠️ **Pourquoi c'est bloquant ?**

Sans cette migration, **TOUS les endpoints qui lisent le champ `Statut`** vont échouer avec :

```
System.InvalidCastException: Unable to cast object of type 'System.String' to type 'System.Boolean'
```

Cela inclut :
- ❌ `/api/Dashboard/global`
- ❌ `/api/Presence/dashboard/ecole/{id}`
- ❌ `/api/Paiement/dashboard/ecole/{id}`
- ❌ `/api/Eleve/ecole/{id}`
- ❌ `/api/Agent`
- ❌ Et pratiquement **tous les autres endpoints** !

---

## 🚀 **Après avoir exécuté le script**

1. **Redémarre l'application** :
   ```powershell
   Stop-Process -Name "KelasiNaBiso" -Force
   dotnet run
   ```

2. **Reteste l'endpoint** :
   ```http
   GET /api/Dashboard/global?idEcole=13
   ```

3. **Ça devrait fonctionner** ! ✅

---

## 📊 **Tables concernées par la migration**

Le script modifie le champ `Statut` dans ces tables :
- `Ecoles`
- `Directions`
- `Classes`
- `Options`
- `Sections`
- `Eleves`
- `Tuteurs`
- `Agents`
- `Utilisateurs`
- `Presences`
- `Paiements`
- `Frais`
- `Cours`
- `AffectationsCours`
- `Roles`
- `Permissions`
- `GroupesMessages`
- `Messages`
- `Inscriptions`
- `AnneeScolaires`
- `Documents`
- `Evaluations`
- `Notes`
- `RessourcesPedagogiques`
- `Notifications`
- `Vacations`

---

## ❓ **Si tu as peur d'exécuter le script**

### **Option 1 : Backup avant**
```sql
-- Créer un backup de la base de données
mysqldump -u root -p kelasinabiso > backup_avant_migration.sql
```

### **Option 2 : Exécuter seulement pour `Presences`**
```sql
-- Test sur une seule table
ALTER TABLE Presences MODIFY COLUMN Statut tinyint(1) NULL;
```

Puis teste l'endpoint. Si ça marche, continue avec le script complet.

---

## 🎯 **Résumé**

1. **Ouvre HeidiSQL**
2. **Charge le script** : `APPLIQUER_MIGRATION_STATUT_NULLABLE.sql`
3. **Exécute** (F9)
4. **Redémarre l'API**
5. **Reteste** `/api/Dashboard/global?idEcole=13`

---

**💡 C'est la seule façon de résoudre le problème `InvalidCastException` !**

**Une fois fait, tous les dashboards fonctionneront parfaitement.** 😊

