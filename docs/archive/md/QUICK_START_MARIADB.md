# 🚀 Guide de Démarrage Rapide - Migration MariaDB

## ⏱️ Migration en 5 minutes

### Prérequis
- ✅ MariaDB 10.x installé et démarré
- ✅ MySQL actuel avec la base `KelasiNaBisoDb`
- ✅ Droits administrateur

---

## 🎯 Option 1 : Migration Automatique (Recommandée)

### Windows (PowerShell)

```powershell
# 1. Ouvrir PowerShell en Administrateur
# 2. Naviguer vers le projet
cd G:\KelasiNaBiso\KelasiNaBisoAPI

# 3. Exécuter le script
.\migration-mysql-to-mariadb.ps1

# 4. Suivre les instructions
# Le script fait tout automatiquement!
```

### Linux/macOS (Bash)

```bash
# 1. Donner les droits d'exécution
chmod +x migration-mysql-to-mariadb.sh

# 2. Exécuter le script
./migration-mysql-to-mariadb.sh

# 3. Suivre les instructions
```

### Après la migration

```bash
# Restaurer les packages
dotnet restore

# Démarrer l'API
dotnet run

# Tester
# Ouvrez: http://localhost:5002/swagger
```

---

## 🛠️ Option 2 : Migration Manuelle (5 commandes)

### Étape 1 : Sauvegarder MySQL

```bash
mysqldump --host=localhost \
          --user=kansa \
          --password=kansa2025 \
          --databases KelasiNaBisoDb \
          --routines --triggers --events \
          > backup_kelasinabiso.sql
```

### Étape 2 : Arrêter MySQL (optionnel)

```bash
# Windows
net stop MySQL80

# Linux
sudo systemctl stop mysql
```

### Étape 3 : Démarrer MariaDB

```bash
# Windows
net start mariadb

# Linux
sudo systemctl start mariadb
```

### Étape 4 : Importer dans MariaDB

```bash
mysql --host=localhost \
      --user=kansa \
      --password=kansa2025 \
      < backup_kelasinabiso.sql
```

### Étape 5 : Lancer l'API

```bash
dotnet restore
dotnet run
```

---

## ✅ Vérification rapide

### Test 1 : Version de MariaDB
```sql
SELECT VERSION();
-- Résultat attendu: 10.x.x-MariaDB
```

### Test 2 : Compter les tables
```sql
USE KelasiNaBisoDb;
SELECT COUNT(*) FROM information_schema.tables 
WHERE table_schema = 'KelasiNaBisoDb';
-- Résultat attendu: ~30 tables
```

### Test 3 : Test API
```bash
curl http://localhost:5002/api/Ecole
# Devrait retourner la liste des écoles
```

---

## 🔧 Modifications effectuées dans le code

### 1. KelasiNaBiso.csproj
- ❌ Suppression de `MySql.Data` (non nécessaire)
- ✅ Conservation de `Pomelo.EntityFrameworkCore.MySql` (compatible MariaDB)

### 2. Program.cs
```csharp
// AVANT (MySQL)
new MySqlServerVersion(new Version(8, 0, 21))

// APRÈS (MariaDB)
new MariaDbServerVersion(new Version(10, 11, 0))
```

### 3. appsettings.json
- ✅ Aucun changement nécessaire
- ✅ Chaîne de connexion identique

---

## ❗ Troubleshooting Rapide

### Problème : Erreur de connexion
```bash
# Vérifier que MariaDB est démarré
net start mariadb  # Windows
sudo systemctl status mariadb  # Linux
```

### Problème : Erreur d'authentification
```sql
-- Se connecter en root
mysql -u root -p

-- Recréer l'utilisateur
CREATE USER IF NOT EXISTS 'kansa'@'localhost' IDENTIFIED BY 'kansa2025';
GRANT ALL PRIVILEGES ON KelasiNaBisoDb.* TO 'kansa'@'localhost';
FLUSH PRIVILEGES;
```

### Problème : Base manquante
```bash
# Réimporter le backup
mysql -u kansa -pkansa2025 < backup_kelasinabiso.sql
```

---

## 📞 Support

### Documentation complète
Consultez `MIGRATION_MYSQL_TO_MARIADB.md` pour le guide détaillé.

### Liens utiles
- **MariaDB Download**: https://mariadb.org/download/
- **Documentation**: https://mariadb.com/kb/en/
- **Migration Guide**: https://mariadb.com/kb/en/migrating-from-mysql-to-mariadb/

---

## 🎉 C'est terminé !

Votre API **KelasiNaBiso** fonctionne maintenant avec **MariaDB 10** ! 🚀

### Avantages obtenus
- ✅ Meilleures performances
- ✅ 100% Open Source
- ✅ Support communautaire actif
- ✅ Versions LTS avec support long terme

---

*Guide créé le: $(date)*

