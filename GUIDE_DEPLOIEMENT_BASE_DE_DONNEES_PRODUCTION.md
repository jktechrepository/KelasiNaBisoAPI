# 🚀 GUIDE DE DÉPLOIEMENT - BASE DE DONNÉES PRODUCTION

## 📋 Vue d'ensemble

Ce guide explique comment créer et configurer la base de données **KnbV2_db** en production en utilisant le script SQL généré automatiquement.

---

## 📦 Fichiers nécessaires

| Fichier | Description | Taille |
|---------|-------------|--------|
| `PRODUCTION_DATABASE_SCRIPT.sql` | Script SQL complet (1842 lignes) | ~75 KB |
| Ce document | Guide d'installation | - |

---

## 🎯 Étape 1 : Préparation du serveur MySQL/MariaDB

### A. Vérifier la version du serveur

```bash
mysql --version
# ou
mariadb --version
```

**Version minimale requise :**
- MySQL 8.0+ ✅
- MariaDB 10.6+ ✅

### B. Se connecter au serveur MySQL

```bash
mysql -u root -p
# Entrer le mot de passe root
```

---

## 🗄️ Étape 2 : Création de la base de données

### Option A : Base de données vide (RECOMMANDÉ)

```sql
-- Créer la base de données avec le bon charset
CREATE DATABASE IF NOT EXISTS KnbV2_db 
    CHARACTER SET utf8mb4 
    COLLATE utf8mb4_unicode_ci;

-- Vérifier la création
SHOW DATABASES LIKE 'KnbV2_db';
```

### Option B : Réinitialisation complète (si base existe)

```sql
-- ⚠️ ATTENTION : Cette commande SUPPRIME TOUTES LES DONNÉES !
DROP DATABASE IF EXISTS KnbV2_db;

-- Recréer la base
CREATE DATABASE KnbV2_db 
    CHARACTER SET utf8mb4 
    COLLATE utf8mb4_unicode_ci;
```

---

## 👤 Étape 3 : Création de l'utilisateur de l'application

```sql
-- Créer l'utilisateur 'kansa' avec mot de passe
CREATE USER IF NOT EXISTS 'kansa'@'localhost' 
    IDENTIFIED BY 'kansa@2025';

-- Accorder tous les privilèges sur la base KnbV2_db
GRANT ALL PRIVILEGES ON KnbV2_db.* TO 'kansa'@'localhost';

-- Si l'API est sur un serveur différent, autoriser l'accès distant
CREATE USER IF NOT EXISTS 'kansa'@'%' 
    IDENTIFIED BY 'kansa@2025';
GRANT ALL PRIVILEGES ON KnbV2_db.* TO 'kansa'@'%';

-- Appliquer les changements
FLUSH PRIVILEGES;

-- Vérifier les privilèges
SHOW GRANTS FOR 'kansa'@'localhost';
```

---

## 📜 Étape 4 : Exécution du script SQL

### Méthode 1 : Depuis le terminal (RECOMMANDÉ)

```bash
# Se positionner dans le dossier du projet
cd G:\KelasiNaBiso\KelasiNaBisoAPI

# Exécuter le script SQL
mysql -u kansa -p KnbV2_db < PRODUCTION_DATABASE_SCRIPT.sql

# Entrer le mot de passe : kansa@2025
```

### Méthode 2 : Depuis MySQL CLI

```sql
-- Se connecter à MySQL
mysql -u kansa -p

-- Sélectionner la base de données
USE KnbV2_db;

-- Exécuter le script
SOURCE G:/KelasiNaBiso/KelasiNaBisoAPI/PRODUCTION_DATABASE_SCRIPT.sql;
```

### Méthode 3 : Avec MySQL Workbench (GUI)

1. Ouvrir MySQL Workbench
2. Se connecter au serveur
3. Menu : **File** → **Open SQL Script**
4. Sélectionner `PRODUCTION_DATABASE_SCRIPT.sql`
5. Cliquer sur l'icône ⚡ **Execute**

---

## ✅ Étape 5 : Vérification de l'installation

### A. Vérifier les tables créées

```sql
USE KnbV2_db;

-- Lister toutes les tables
SHOW TABLES;

-- Devrait afficher environ 35+ tables dont :
-- ✅ Ecoles, Utilisateurs, Roles, Permissions
-- ✅ Eleves, Tuteurs, Agents
-- ✅ Classes, Cours, Notes, Inscriptions
-- ✅ Paiements, Frais, Presences
-- ✅ AuditLogs (Audit Trail)
-- ✅ __EFMigrationsHistory (historique EF Core)
```

### B. Vérifier les index de performance

```sql
-- Vérifier les index sur la table Utilisateurs
SHOW INDEX FROM Utilisateurs;

-- Vérifier les index sur la table AuditLogs
SHOW INDEX FROM AuditLogs;

-- Vérifier les index sur la table Eleves
SHOW INDEX FROM Eleves;
```

### C. Vérifier les contraintes et relations

```sql
-- Voir la structure de la table Utilisateurs
DESCRIBE Utilisateurs;

-- Voir les clés étrangères
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    CONSTRAINT_NAME,
    REFERENCED_TABLE_NAME,
    REFERENCED_COLUMN_NAME
FROM
    INFORMATION_SCHEMA.KEY_COLUMN_USAGE
WHERE
    REFERENCED_TABLE_SCHEMA = 'KnbV2_db'
    AND TABLE_NAME = 'Utilisateurs';
```

---

## 📊 Étape 6 : Initialisation des données de base

### A. Vérifier les rôles par défaut

```sql
SELECT * FROM Roles;

-- Si vide, l'application créera automatiquement les rôles
-- au premier démarrage via PermissionSeeder.
```

### B. Créer le premier utilisateur Admin (OPTIONNEL)

```sql
-- Insérer une école de test
INSERT INTO Ecoles (Nom, Type, Telephone, EmailContact, Statut, AcceptNotification, DateCreation)
VALUES ('École Test', 'Primaire', '+243000000000', 'test@ecole.cd', 1, 1, NOW());

-- Récupérer l'IdEcole
SET @IdEcole = LAST_INSERT_ID();

-- Créer un utilisateur Admin (à ajuster selon vos besoins)
-- Note : Le mot de passe doit être haché. Utilisez l'API pour créer le premier utilisateur.
```

---

## 🔒 Étape 7 : Sécurisation (PRODUCTION)

### A. Modifier le mot de passe de l'utilisateur 'kansa'

```sql
-- Générer un mot de passe fort (exemple)
ALTER USER 'kansa'@'localhost' 
    IDENTIFIED BY 'VotreMdpFort@2025!xyz';

-- Pour l'accès distant
ALTER USER 'kansa'@'%' 
    IDENTIFIED BY 'VotreMdpFort@2025!xyz';

FLUSH PRIVILEGES;
```

### B. Mettre à jour `appsettings.json`

```json
{
  "ConnectionStrings": {
    "KelasiConnection": "Server=localhost;Port=3306;Database=KnbV2_db;User=kansa;Password=VotreMdpFort@2025!xyz;CharSet=utf8mb4;"
  }
}
```

### C. Restreindre l'accès distant (si nécessaire)

```sql
-- Supprimer l'accès depuis n'importe où
DROP USER IF EXISTS 'kansa'@'%';

-- Autoriser uniquement une IP spécifique
CREATE USER 'kansa'@'192.168.1.100' 
    IDENTIFIED BY 'VotreMdpFort@2025!xyz';
GRANT ALL PRIVILEGES ON KnbV2_db.* TO 'kansa'@'192.168.1.100';
FLUSH PRIVILEGES;
```

---

## 🧪 Étape 8 : Test de connexion avec l'API

### A. Démarrer l'application

```bash
cd G:\KelasiNaBiso\KelasiNaBisoAPI
dotnet run
```

### B. Vérifier les logs de démarrage

Vous devriez voir :

```
✅ KelasiNaBisoAPI démarré et prêt à recevoir des requêtes
📊 Environnement : Production
🔗 Swagger UI : https://localhost:7102/swagger
```

### C. Tester la connexion à la base de données

```bash
# Appel API de test (si endpoint de health check existe)
curl https://localhost:7102/api/health
```

---

## 📋 Contenu du script SQL

Le script `PRODUCTION_DATABASE_SCRIPT.sql` contient :

### ✅ Tables principales (35+)

| Catégorie | Tables |
|-----------|--------|
| **Authentification** | `Utilisateurs`, `Roles`, `Permissions`, `RolePermissions`, `UserPermissions`, `UserDevices` |
| **École** | `Ecoles`, `Directions`, `AnneeScolaires`, `Sections`, `Options`, `Classes` |
| **Personnel** | `Agents`, `TitulairesClasses` |
| **Élèves** | `Eleves`, `Tuteurs`, `Inscriptions` |
| **Pédagogie** | `Cours`, `AffectationsCours`, `Notes`, `Evaluations`, `RessourcePedagogiques` |
| **Présence** | `Presences`, `Vacations`, `Horaires` |
| **Finance** | `Frais`, `Paiements` |
| **Communication** | `Messages`, `GroupeMessages`, `Notifications` |
| **Audit** | `AuditLogs` (Audit Trail complet) |
| **Autres** | `Documents`, `SmsLogs` |

### ✅ Index de performance

- **Unicité** : Email, Matricule, SerialNumber
- **Performance** : Recherches, Tris, Filtres
- **Audit** : Index sur TableName, RecordId, UserId, DateAction, IdEcole

### ✅ Relations et contraintes

- Clés étrangères avec `ON DELETE CASCADE/RESTRICT/NO ACTION`
- Contraintes d'intégrité référentielle
- Indexes pour optimiser les jointures

---

## 🚨 Dépannage

### Problème 1 : Erreur de connexion

```
Error: Access denied for user 'kansa'@'localhost'
```

**Solution :**
```sql
GRANT ALL PRIVILEGES ON KnbV2_db.* TO 'kansa'@'localhost';
FLUSH PRIVILEGES;
```

### Problème 2 : Base de données introuvable

```
Error: Unknown database 'KnbV2_db'
```

**Solution :**
```sql
CREATE DATABASE KnbV2_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

### Problème 3 : Charset incorrect

```
Error: Incorrect string value
```

**Solution :**
```sql
ALTER DATABASE KnbV2_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

### Problème 4 : Script SQL trop volumineux

```
Error: MySQL server has gone away
```

**Solution :**
```sql
-- Augmenter la taille max des packets
SET GLOBAL max_allowed_packet=67108864; -- 64 MB
```

---

## 📝 Notes importantes

### ⚠️ Script idempotent

Le script SQL généré est **idempotent**, ce qui signifie :
- Il peut être exécuté plusieurs fois sans erreur
- Il vérifie si les tables existent avant de les créer
- Il utilise `IF NOT EXISTS` quand c'est possible
- Il gère les migrations EF Core via `__EFMigrationsHistory`

### 🔄 Mises à jour futures

Pour les mises à jour futures de la base de données :

1. **Avec EF Core Migrations (RECOMMANDÉ)**
   ```bash
   dotnet ef migrations add NomDeLaMigration
   dotnet ef database update
   ```

2. **Script SQL incrémental**
   ```bash
   dotnet ef migrations script [MigrationPrécédente] [NouvelleMigration]
   ```

### 📦 Sauvegarde

Avant toute modification en production :

```bash
# Backup complet
mysqldump -u kansa -p KnbV2_db > backup_$(date +%Y%m%d_%H%M%S).sql

# Backup avec compression
mysqldump -u kansa -p KnbV2_db | gzip > backup_$(date +%Y%m%d_%H%M%S).sql.gz
```

---

## ✅ Checklist finale

- [ ] Base de données `KnbV2_db` créée avec charset UTF8MB4
- [ ] Utilisateur `kansa` créé avec privilèges appropriés
- [ ] Script `PRODUCTION_DATABASE_SCRIPT.sql` exécuté sans erreurs
- [ ] Tables vérifiées (35+ tables présentes)
- [ ] Index de performance créés
- [ ] Mot de passe sécurisé en production
- [ ] `appsettings.json` mis à jour avec les bons paramètres
- [ ] Test de connexion API réussi
- [ ] Backup initial créé
- [ ] Documentation conservée pour référence

---

## 📞 Support

En cas de problème :
1. Vérifier les logs de l'application (Serilog)
2. Vérifier les logs MySQL (`/var/log/mysql/error.log`)
3. Consulter la documentation EF Core
4. Contacter l'équipe de développement

---

**Date de génération :** 3 novembre 2025  
**Version de l'API :** 2.0  
**Base de données :** KnbV2_db  
**Auteur :** Assistant IA

