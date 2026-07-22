# 📋 Guide d'utilisation du script de mise à jour des permissions

## 📄 Fichier
`UPDATE_PERMISSIONS_DIRECTEUR_FINANCIER_PRODUCTION.sql`

## 🎯 Objectif
Ce script met à jour les permissions pour les rôles **Directeur** et **Financier** selon les nouvelles règles de sécurité.

## 🔄 Changements appliqués

### Changement 1 : Rôle Directeur
- ✅ **Ajoute** toutes les permissions du rôle Admin
- ❌ **Supprime** les permissions `Paiement.Update` et `Paiement.Delete`
- ✅ Le Directeur peut maintenant créer des utilisateurs (sauf Admin et Super-Admin, vérifié au niveau métier)

### Changement 2 : Rôle Financier
- ❌ **Supprime** les permissions `Paiement.Update` et `Paiement.Delete`
- ✅ Le Financier peut toujours créer et lire les paiements, mais ne peut plus les modifier ni les supprimer

## ⚠️ AVANT D'EXÉCUTER LE SCRIPT

### 1. Sauvegarde obligatoire
```bash
# Exemple de commande de sauvegarde MariaDB
mysqldump -u root -p knb_db > backup_before_permissions_update_$(date +%Y%m%d_%H%M%S).sql
```

### 2. Vérifications préalables
- ✅ Vérifier que la base de données est accessible
- ✅ Vérifier que les tables `Roles`, `Permissions` et `RolePermissions` existent
- ✅ Vérifier que les rôles `Directeur`, `Financier` et `Admin` existent
- ✅ Tester le script sur une base de données de test d'abord

### 3. Période d'exécution
- ⏰ Exécuter pendant une période de faible activité
- ⏰ Informer les utilisateurs si nécessaire
- ⏰ Prévoir un temps d'arrêt si nécessaire (le script est rapide, < 1 seconde)

## 🚀 Exécution du script

### Méthode 1 : Via MySQL/MariaDB CLI
```bash
mysql -u root -p knb_db < UPDATE_PERMISSIONS_DIRECTEUR_FINANCIER_PRODUCTION.sql
```

### Méthode 2 : Via MySQL Workbench ou phpMyAdmin
1. Ouvrir le fichier `UPDATE_PERMISSIONS_DIRECTEUR_FINANCIER_PRODUCTION.sql`
2. Sélectionner la base de données `knb_db`
3. Exécuter le script

### Méthode 3 : Via ligne de commande avec sortie
```bash
mysql -u root -p knb_db < UPDATE_PERMISSIONS_DIRECTEUR_FINANCIER_PRODUCTION.sql > migration_output.log 2>&1
```

## ✅ Vérifications après exécution

Le script inclut des vérifications automatiques, mais vous pouvez aussi vérifier manuellement :

### 1. Vérifier que Directeur n'a plus Paiement.Update/Delete
```sql
SELECT COUNT(*) 
FROM RolePermissions rp
INNER JOIN Roles r ON rp.IdRole = r.IdRole
INNER JOIN Permissions p ON rp.IdPermission = p.IdPermission
WHERE r.Nom = 'Directeur'
  AND p.Nom IN ('Paiement.Update', 'Paiement.Delete');
-- Résultat attendu : 0
```

### 2. Vérifier que Financier n'a plus Paiement.Update/Delete
```sql
SELECT COUNT(*) 
FROM RolePermissions rp
INNER JOIN Roles r ON rp.IdRole = r.IdRole
INNER JOIN Permissions p ON rp.IdPermission = p.IdPermission
WHERE r.Nom = 'Financier'
  AND p.Nom IN ('Paiement.Update', 'Paiement.Delete');
-- Résultat attendu : 0
```

### 3. Vérifier que Directeur a Utilisateur.Create
```sql
SELECT COUNT(*) 
FROM RolePermissions rp
INNER JOIN Roles r ON rp.IdRole = r.IdRole
INNER JOIN Permissions p ON rp.IdPermission = p.IdPermission
WHERE r.Nom = 'Directeur'
  AND p.Nom = 'Utilisateur.Create';
-- Résultat attendu : 1
```

### 4. Compter les permissions de Directeur
```sql
SELECT COUNT(*) AS NombrePermissions
FROM RolePermissions rp
INNER JOIN Roles r ON rp.IdRole = r.IdRole
WHERE r.Nom = 'Directeur';
-- Le nombre devrait être proche de celui d'Admin (moins 2 pour Paiement.Update/Delete)
```

## 🔄 Rollback (en cas de problème)

Si vous devez annuler les changements :

```sql
-- 1. Restaurer la sauvegarde
mysql -u root -p knb_db < backup_before_permissions_update_YYYYMMDD_HHMMSS.sql

-- OU

-- 2. Réajouter manuellement les permissions supprimées (si nécessaire)
-- Note: Cette méthode nécessite de connaître l'état précédent exact
```

## 📊 Résumé des permissions après migration

| Rôle | Paiement.Create | Paiement.Read | Paiement.Update | Paiement.Delete | Utilisateur.Create |
|------|----------------|---------------|-----------------|-----------------|-------------------|
| Super-Admin | ✅ | ✅ | ✅ | ✅ | ✅ (tous rôles) |
| Admin | ✅ | ✅ | ✅ | ✅ | ✅ (sauf Super-Admin) |
| **Directeur** | ✅ | ✅ | ❌ | ❌ | ✅ (sauf Admin/Super-Admin) |
| **Financier** | ✅ | ✅ | ❌ | ❌ | ❌ |

## 🐛 Dépannage

### Erreur : "Rôle Directeur non trouvé"
- Vérifier que le rôle existe : `SELECT * FROM Roles WHERE Nom = 'Directeur';`
- Vérifier l'orthographe (sensible à la casse)

### Erreur : "Permission Paiement.Update non trouvée"
- Vérifier que la permission existe : `SELECT * FROM Permissions WHERE Nom = 'Paiement.Update';`
- Si elle n'existe pas, elle sera créée lors du prochain seed des permissions

### Le script ne modifie rien
- Vérifier que les permissions existent déjà dans RolePermissions
- Le script est idempotent : il peut être exécuté plusieurs fois sans problème

## 📝 Notes importantes

1. **Idempotence** : Le script peut être exécuté plusieurs fois sans problème
2. **Transactions** : Le script n'utilise pas de transactions explicites (chaque opération est immédiate)
3. **Performance** : Le script est optimisé et s'exécute rapidement (< 1 seconde)
4. **Sécurité** : Les permissions sont supprimées de manière définitive (pas de soft delete)

## 📞 Support

En cas de problème, vérifier :
1. Les logs de la base de données
2. La sauvegarde avant migration
3. Les résultats des vérifications incluses dans le script

