# 📋 Scripts SQL pour l'assignation des permissions et rôles

Ce dossier contient des scripts SQL pour assigner les permissions et rôles aux utilisateurs en production.

## 📁 Fichiers disponibles

### 1. `assigner_permissions_admin_utilisateur.sql` (Script complet)
**Description** : Script détaillé avec toutes les étapes et vérifications  
**Usage** : Pour une assignation complète avec toutes les vérifications

### 2. `assigner_admin_rapide.sql` (Script rapide)
**Description** : Version simplifiée pour un usage rapide  
**Usage** : Pour assigner rapidement le rôle Admin à un utilisateur

### 3. `verifier_permissions_utilisateur.sql` (Script de vérification)
**Description** : Vérifie l'état actuel des permissions et rôles d'un utilisateur  
**Usage** : Pour diagnostiquer les problèmes de permissions

---

## 🚀 Utilisation

### Étape 1 : Modifier les variables

Avant d'exécuter un script, modifiez la variable `@EmailUtilisateur` ou `@IdUtilisateur` :

```sql
-- Option 1 : Utiliser l'email
SET @EmailUtilisateur = 'votre.email@exemple.com';

-- Option 2 : Utiliser directement l'ID
SET @IdUtilisateur = 1234;
```

### Étape 2 : Exécuter le script

#### Pour assigner le rôle Admin (script rapide) :
```bash
mysql -u votre_user -p votre_database < assigner_admin_rapide.sql
```

#### Pour une assignation complète :
```bash
mysql -u votre_user -p votre_database < assigner_permissions_admin_utilisateur.sql
```

#### Pour vérifier les permissions :
```bash
mysql -u votre_user -p votre_database < verifier_permissions_utilisateur.sql
```

---

## 📊 Ce que font les scripts

### Script d'assignation

1. ✅ **Récupère l'ID de l'utilisateur** (par email ou ID)
2. ✅ **Récupère l'ID du rôle Admin**
3. ✅ **Assigne toutes les permissions au rôle Admin** (si pas déjà fait)
   - Permissions Ecole : Read, ReadAll, Update
   - Toutes les permissions pour : Utilisateur, Eleve, Agent, Paiement, Note, Tuteur, Classe, Frais, Inscription, Presence, Cours
4. ✅ **Assigne le rôle Admin à l'utilisateur** dans la table `UserRoles`
5. ✅ **Définit le rôle Admin comme rôle principal** (`IsPrimary = TRUE`)
6. ✅ **Met à jour `IdRole` dans `Utilisateurs`** (pour rétrocompatibilité)
7. ✅ **Affiche un résumé** avec toutes les informations

### Script de vérification

Affiche :
- 📋 Informations générales de l'utilisateur
- 👤 Rôles assignés
- ⭐ Rôle principal actuel
- 🔐 Liste complète des permissions
- 📊 Statistiques (nombre de rôles, permissions, etc.)
- 📈 Permissions par catégorie
- ✅ Vérification spécifique des permissions Admin

---

## ⚠️ Important

### Avant d'exécuter en production :

1. **Faites une sauvegarde** de votre base de données
2. **Testez d'abord** sur une base de données de test
3. **Vérifiez** que l'utilisateur existe bien
4. **Vérifiez** que le rôle Admin existe

### Permissions Admin assignées :

Le rôle Admin reçoit les permissions suivantes :

| Catégorie | Actions |
|-----------|---------|
| **Ecole** | Read, ReadAll, Update (PAS Create ni Delete) |
| **Utilisateur** | Toutes les actions |
| **Eleve** | Toutes les actions |
| **Agent** | Toutes les actions |
| **Paiement** | Toutes les actions |
| **Note** | Toutes les actions |
| **Tuteur** | Toutes les actions |
| **Classe** | Toutes les actions |
| **Frais** | Toutes les actions |
| **Inscription** | Toutes les actions |
| **Presence** | Toutes les actions |
| **Cours** | Toutes les actions |

---

## 🔍 Exemple d'utilisation

### Cas 1 : Assigner le rôle Admin à un nouvel utilisateur

```sql
-- 1. Modifier l'email dans le script
SET @EmailUtilisateur = 'nouvel.admin@ecole.com';

-- 2. Exécuter le script rapide
-- Le script va :
--   - Trouver l'utilisateur par email
--   - Assigner toutes les permissions Admin au rôle Admin
--   - Assigner le rôle Admin à l'utilisateur
--   - Définir le rôle Admin comme rôle principal
```

### Cas 2 : Vérifier pourquoi un utilisateur n'a pas de permissions

```sql
-- 1. Modifier l'email dans le script de vérification
SET @EmailUtilisateur = 'utilisateur@ecole.com';

-- 2. Exécuter le script de vérification
-- Le script va afficher :
--   - Si l'utilisateur a un rôle assigné
--   - Si le rôle a des permissions
--   - Liste complète des permissions
```

---

## 🐛 Dépannage

### Erreur : "Illegal mix of collations (utf8mb4_general_ci, IMPLICIT) and (utf8mb4_unicode_ci, IMPLICIT)"

**Problème** : Conflit de collation entre la colonne `Email` et la variable `@EmailUtilisateur`.

**Solutions** :

1. **Utiliser le script par ID** : Utilisez `assigner_admin_par_id.sql` avec l'ID utilisateur directement
   ```sql
   SET @IdUtilisateur = 2039;  -- Remplacez par l'ID réel
   ```

2. **Forcer la collation** : Les scripts ont été corrigés pour utiliser `COLLATE utf8mb4_unicode_ci`
   - Si l'erreur persiste, vérifiez la collation de votre table :
     ```sql
     SHOW FULL COLUMNS FROM Utilisateurs WHERE Field = 'Email';
     ```
   - Ajustez le script selon la collation réelle de votre table

3. **Alternative** : Utiliser une sous-requête pour éviter les variables
   ```sql
   -- Au lieu de :
   SELECT IdUtilisateur INTO @IdUtilisateur FROM Utilisateurs WHERE Email = @EmailUtilisateur;
   
   -- Utilisez directement :
   SET @IdUtilisateur = (SELECT IdUtilisateur FROM Utilisateurs WHERE Email COLLATE utf8mb4_unicode_ci = 'votre@email.com' COLLATE utf8mb4_unicode_ci);
   ```

### Erreur : "Utilisateur non trouvé"
- Vérifiez que l'email est correct
- Vérifiez que l'utilisateur existe dans la table `Utilisateurs`
- Utilisez le script `assigner_admin_par_id.sql` avec l'ID directement

### Erreur : "Rôle Admin non trouvé"
- Vérifiez que le rôle Admin existe : `SELECT * FROM Roles WHERE Nom = 'Admin';`
- Si le rôle n'existe pas, créez-le d'abord

### L'utilisateur n'a toujours pas de permissions après exécution
1. Exécutez le script de vérification pour voir l'état actuel
2. Vérifiez que le rôle Admin a bien des permissions :
   ```sql
   SELECT COUNT(*) FROM RolePermissions WHERE IdRole = (SELECT IdRole FROM Roles WHERE Nom = 'Admin');
   ```
3. Si le nombre est 0, les permissions n'ont pas été initialisées. Exécutez d'abord `PermissionSeeder.SeedPermissionsAsync()` via l'API

---

## 📝 Notes

- Les scripts utilisent `NOW()` pour la date d'attribution (MySQL/MariaDB)
- Pour PostgreSQL, remplacez `NOW()` par `CURRENT_TIMESTAMP`
- Les scripts sont idempotents : vous pouvez les exécuter plusieurs fois sans problème
- Le script vérifie automatiquement si les permissions/rôles existent déjà avant de les créer

---

## 🔗 Structure des tables utilisées

- `Utilisateurs` : Table principale des utilisateurs
- `Roles` : Table des rôles (Admin, Directeur, etc.)
- `Permissions` : Table des permissions (Ecole.Create, Paiement.Read, etc.)
- `RolePermissions` : Table de liaison entre rôles et permissions (N-N)
- `UserRoles` : Table de liaison entre utilisateurs et rôles (N-N)

---

**Créé le** : 2025-01-16  
**Auteur** : Script généré automatiquement pour KelasiNaBisoAPI
