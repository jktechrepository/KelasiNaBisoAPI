# 📋 Instructions pour appliquer la migration "MakeStatutNullable"

## 🎯 Objectif
Rendre tous les champs `Statut` de la base de données nullables pour accepter `true`, `false`, ou `null`.

## ⚠️ Problème rencontré
Entity Framework essaie de recréer des tables qui existent déjà, car il ne sait pas que les anciennes migrations ont été appliquées.

## ✅ Solution
Exécuter le script SQL `APPLIQUER_MIGRATION_STATUT_NULLABLE.sql` manuellement dans votre outil de gestion de base de données.

---

## ⚠️ IMPORTANT : Migration automatique désactivée

La migration automatique dans `Program.cs` a été **temporairement désactivée** pour éviter les conflits.
Vous devez maintenant appliquer le script SQL **manuellement**.

---

## 📝 Étapes à suivre

### Option 1 : Avec HeidiSQL (Recommandé)

1. **Ouvrir HeidiSQL**
2. **Se connecter à la base de données `kelasinabiso`**
   - Host: `localhost`
   - User: `root`
   - Password: `1234`
   - Database: `kelasinabiso`

3. **Ouvrir l'onglet "Requête"** (ou appuyer sur `Ctrl+T`)

4. **Ouvrir le fichier SQL**
   - Menu : `Fichier` → `Charger le fichier SQL`
   - Sélectionner : `APPLIQUER_MIGRATION_STATUT_NULLABLE.sql`

5. **Exécuter le script**
   - Cliquer sur le bouton "▶ Exécuter" (ou appuyer sur `F9`)

6. **Vérifier le résultat**
   - Vous devriez voir un message de succès
   - La dernière requête affiche toutes les migrations appliquées

---

### Option 2 : Avec phpMyAdmin

1. **Ouvrir phpMyAdmin** dans votre navigateur
   - URL : `http://localhost/phpmyadmin`

2. **Sélectionner la base de données `kelasinabiso`**

3. **Aller dans l'onglet "SQL"**

4. **Copier-coller le contenu du fichier `APPLIQUER_MIGRATION_STATUT_NULLABLE.sql`**

5. **Cliquer sur "Exécuter"**

6. **Vérifier le résultat**
   - Vous devriez voir un message de succès
   - La table `__EFMigrationsHistory` devrait contenir 5 migrations

---

### Option 3 : En ligne de commande (Si MySQL est dans le PATH)

```bash
# Ouvrir un terminal dans le dossier du projet
cd G:\KelasiNaBiso\KelasiNaBisoAPI

# Exécuter le script SQL
mysql -u root -p1234 kelasinabiso < APPLIQUER_MIGRATION_STATUT_NULLABLE.sql
```

---

## 🔍 Vérification après l'exécution

### Dans votre outil SQL, exécutez cette requête :

```sql
-- Vérifier que toutes les migrations sont enregistrées
SELECT * FROM __EFMigrationsHistory ORDER BY MigrationId;
```

**Résultat attendu :**
```
20251028103016_InitialCreate
20251031052322_AddAcceptNotificationToEcole
20251031054943_UpdateAcceptNotificationToTrue
20251101191812_AddAuditLogTable
20251103133630_MakeStatutNullable  ← Nouvelle migration
```

### Vérifier qu'une colonne Statut est bien nullable :

```sql
-- Vérifier la structure de la table Ecoles
DESCRIBE Ecoles;
```

**Résultat attendu :**
- La colonne `Statut` doit avoir `NULL` dans la colonne "Null" (au lieu de "NO")

---

## 🚀 Démarrer l'application

Une fois le script exécuté avec succès :

```bash
# Dans le terminal PowerShell
cd G:\KelasiNaBiso\KelasiNaBisoAPI
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run
```

L'application devrait démarrer sans erreur ! ✅

---

## ❓ En cas de problème

### Erreur : "Table '__EFMigrationsHistory' already exists"
➡️ **Solution :** Le script gère déjà ce cas avec `CREATE TABLE IF NOT EXISTS`. Ignorez cette erreur.

### Erreur : "Duplicate entry for key 'PRIMARY'"
➡️ **Solution :** Le script utilise `INSERT IGNORE`, donc cette erreur ne devrait pas apparaître. Si elle apparaît, c'est que les migrations sont déjà enregistrées. Vous pouvez simplement ignorer.

### L'application affiche toujours "Table 'ecoles' already exists"
➡️ **Solution :** 
1. Vérifiez que le script SQL a bien été exécuté
2. Exécutez `SELECT * FROM __EFMigrationsHistory` pour vérifier
3. Redémarrez l'application

---

## 📊 Récapitulatif des changements

### Avant la migration :
```csharp
public bool Statut { get; set; }  // Ne peut être que true ou false
```

### Après la migration :
```csharp
public bool? Statut { get; set; }  // Peut être true, false, ou null
```

### Dans la base de données :
```sql
-- Avant
Statut TINYINT(1) NOT NULL DEFAULT 1

-- Après
Statut TINYINT(1) NULL DEFAULT 1
```

---

## ✅ Checklist finale

- [ ] Script SQL exécuté sans erreur
- [ ] 5 migrations visibles dans `__EFMigrationsHistory`
- [ ] Colonne `Statut` est nullable (vérifiée avec `DESCRIBE`)
- [ ] Application démarre sans erreur
- [ ] Aucune erreur "Table already exists"

---

## 🎉 Succès !

Votre base de données est maintenant synchronisée avec vos modèles Entity Framework !
Tous les champs `Statut` acceptent désormais les valeurs nullables. 🚀

