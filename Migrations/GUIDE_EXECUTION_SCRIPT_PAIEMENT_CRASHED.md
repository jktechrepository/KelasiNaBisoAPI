# 📋 Guide d'Exécution du Script SQL - PaiementCrashed

**Script :** `MAKE_PAIEMENT_CRASHED_DATES_NULLABLE_PRODUCTION.sql`  
**Objectif :** Corriger l'erreur `InvalidCastException` en rendant `DateEchec` et `DateCreation` nullable  
**Durée estimée :** 2-3 minutes  
**Impact :** ✅ Résout l'erreur 500 sur `/api/PaiementCrashed/ecole`

---

## ⚠️ **AVANT DE COMMENCER**

1. **Faire une sauvegarde de la base de données** :
   ```bash
   mysqldump -u kansa -p dev-knb_db > backup_avant_migration_$(date +%Y%m%d_%H%M%S).sql
   ```

2. **Vérifier que vous êtes en période de maintenance** (si production)

3. **Noter l'heure de début** pour pouvoir rollback si nécessaire

---

## 🚀 **MÉTHODE 1 : Via MySQL en ligne de commande (Recommandé)**

### Étape 1 : Se connecter à MySQL
```bash
mysql -u kansa -p dev-knb_db
```
Entrez le mot de passe : `kansa@2025`

### Étape 2 : Exécuter le script
```bash
source /Users/mac/Desktop/KelasiNaBisoAPI/Migrations/MAKE_PAIEMENT_CRASHED_DATES_NULLABLE_PRODUCTION.sql
```

**OU** directement :
```bash
mysql -u kansa -p dev-knb_db < /Users/mac/Desktop/KelasiNaBisoAPI/Migrations/MAKE_PAIEMENT_CRASHED_DATES_NULLABLE_PRODUCTION.sql
```

---

## 🚀 **MÉTHODE 2 : Via phpMyAdmin (Interface Web)**

1. **Se connecter à phpMyAdmin** : `https://votre-serveur/phpmyadmin`
2. **Sélectionner la base de données** : `dev-knb_db`
3. **Onglet "SQL"** (en haut)
4. **Copier-coller le contenu** du fichier `MAKE_PAIEMENT_CRASHED_DATES_NULLABLE_PRODUCTION.sql`
5. **Cliquer sur "Exécuter"**

---

## 🚀 **MÉTHODE 3 : Via MySQL Workbench**

1. **Ouvrir MySQL Workbench**
2. **Se connecter** au serveur MySQL
3. **Sélectionner la base** `dev-knb_db` dans le panneau de gauche
4. **Onglet "File" → "Open SQL Script"**
5. **Sélectionner** `MAKE_PAIEMENT_CRASHED_DATES_NULLABLE_PRODUCTION.sql`
6. **Cliquer sur l'icône "Execute"** (⚡) ou `Ctrl+Shift+Enter`

---

## 🚀 **MÉTHODE 4 : Via SSH sur le serveur de production**

Si vous êtes sur le serveur de production :

```bash
# 1. Se connecter en SSH
ssh utilisateur@dev-knb.asdc-rdc.org

# 2. Naviguer vers le répertoire du projet
cd /chemin/vers/KelasiNaBisoAPI

# 3. Exécuter le script
mysql -u kansa -p dev-knb_db < Migrations/MAKE_PAIEMENT_CRASHED_DATES_NULLABLE_PRODUCTION.sql
```

---

## ✅ **VÉRIFICATION APRÈS EXÉCUTION**

### 1. Vérifier que les colonnes sont maintenant NULLABLE

Exécutez cette requête :
```sql
SELECT 
    COLUMN_NAME, 
    IS_NULLABLE, 
    COLUMN_TYPE
FROM 
    INFORMATION_SCHEMA.COLUMNS 
WHERE 
    TABLE_SCHEMA = 'dev-knb_db' AND 
    TABLE_NAME = 'PaiementsCrashed' AND 
    COLUMN_NAME IN ('DateEchec', 'DateCreation');
```

**Résultat attendu :**
```
+--------------+-------------+-------------+
| COLUMN_NAME  | IS_NULLABLE | COLUMN_TYPE |
+--------------+-------------+-------------+
| DateEchec    | YES         | datetime    |
| DateCreation | YES         | datetime    |
+--------------+-------------+-------------+
```

### 2. Tester l'API

```bash
curl -X GET "https://dev-knb.asdc-rdc.org/api/PaiementCrashed/ecole" \
  -H "Authorization: Bearer VOTRE_TOKEN_JWT"
```

**Résultat attendu :** Status 200 avec la liste des paiements échoués (ou liste vide)

---

## 🔄 **ROLLBACK (Si nécessaire)**

Si quelque chose ne va pas, restaurez la sauvegarde :

```bash
mysql -u kansa -p dev-knb_db < backup_avant_migration_YYYYMMDD_HHMMSS.sql
```

---

## 📊 **CE QUE LE SCRIPT FAIT**

1. **Vérifie l'état actuel** des colonnes `DateEchec` et `DateCreation`
2. **Compte les enregistrements** avec des valeurs NULL
3. **Corrige les données** : assigne une valeur par défaut aux NULL existants
4. **Modifie le schéma** : rend les colonnes nullable
5. **Vérifie le résultat** final

---

## ⚠️ **PROBLÈMES POSSIBLES**

### Erreur : "Access denied"
- Vérifiez les identifiants dans `appsettings.json`
- Vérifiez les permissions de l'utilisateur MySQL

### Erreur : "Table doesn't exist"
- Vérifiez que la table `PaiementsCrashed` existe
- Vérifiez le nom de la base de données (`dev-knb_db`)

### Erreur : "Lock wait timeout"
- Attendez quelques secondes et réessayez
- Vérifiez qu'aucune autre transaction n'est en cours

---

## 📞 **SUPPORT**

Si vous rencontrez des problèmes :
1. Vérifiez les logs MySQL : `/var/log/mysql/error.log`
2. Vérifiez les permissions de l'utilisateur
3. Contactez l'administrateur de la base de données

---

**✅ Une fois le script exécuté avec succès, redéployez l'application pour que les modifications du code prennent effet.**

