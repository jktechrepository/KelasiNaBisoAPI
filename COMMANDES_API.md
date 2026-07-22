# 🛠️ Commandes Utiles - KelasiNaBiso API

## 🎯 Gestion de l'application

### Démarrer l'application

```powershell
# Naviguer vers le projet
cd G:\KelasiNaBiso\KelasiNaBisoAPI

# Démarrer l'API
dotnet run
```

### Arrêter l'application

```powershell
# Méthode 1 : Si vous voyez la console
Ctrl + C

# Méthode 2 : Forcer l'arrêt (recommandé si en arrière-plan)
Stop-Process -Name KelasiNaBiso -Force

# Méthode 3 : Trouver et tuer le processus
Get-Process -Name KelasiNaBiso | Stop-Process -Force

# Méthode 4 : Tuer tous les processus dotnet (attention !)
Stop-Process -Name dotnet -Force
```

### Redémarrer l'application

```powershell
# Arrêter puis redémarrer
Stop-Process -Name KelasiNaBiso -Force
dotnet run
```

---

## 🔧 Compilation et Build

### Compiler le projet

```powershell
# Build simple
dotnet build

# Build sans restore
dotnet build --no-restore

# Build en mode Release
dotnet build --configuration Release

# Nettoyer + Build
dotnet clean
dotnet build
```

### Restaurer les packages

```powershell
# Restaurer les packages NuGet
dotnet restore

# Restaurer et compiler
dotnet restore
dotnet build
```

---

## 🗄️ Base de données (MariaDB)

### Gestion du service MariaDB

```powershell
# Démarrer MariaDB
net start mariadb

# Arrêter MariaDB
net stop mariadb

# Redémarrer MariaDB
net stop mariadb
net start mariadb

# Vérifier le statut
Get-Service -Name *maria*,*mysql*
```

### Connexion à MariaDB

```powershell
# Se connecter
mysql -u kansa -pkansa2025

# Se connecter et utiliser la base
mysql -u kansa -pkansa2025 KelasiNaBisoDb

# Exécuter une commande directe
mysql -u kansa -pkansa2025 -e "SELECT VERSION();"

# Lister les bases de données
mysql -u kansa -pkansa2025 -e "SHOW DATABASES;"

# Lister les tables
mysql -u kansa -pkansa2025 -e "USE KelasiNaBisoDb; SHOW TABLES;"
```

### Sauvegarder/Restaurer la base

```powershell
# Sauvegarder
mysqldump -u kansa -pkansa2025 `
  --databases KelasiNaBisoDb `
  --routines --triggers --events `
  > backup_$(Get-Date -Format 'yyyyMMdd_HHmmss').sql

# Restaurer
mysql -u kansa -pkansa2025 < backup_20251023_153000.sql

# Sauvegarder juste les données (sans structure)
mysqldump -u kansa -pkansa2025 `
  --no-create-info `
  KelasiNaBisoDb > data_only_backup.sql
```

---

## 🔄 Migrations Entity Framework

### Créer une migration

```powershell
# Créer une nouvelle migration
dotnet ef migrations add NomDeLaMigration

# Exemple
dotnet ef migrations add AjoutChampDateNaissanceEleve
```

### Appliquer les migrations

```powershell
# Appliquer toutes les migrations en attente
dotnet ef database update

# Appliquer jusqu'à une migration spécifique
dotnet ef database update NomDeLaMigration

# Revenir à une migration précédente
dotnet ef database update MigrationPrecedente
```

### Gérer les migrations

```powershell
# Lister les migrations
dotnet ef migrations list

# Supprimer la dernière migration (NON appliquée)
dotnet ef migrations remove

# Générer un script SQL
dotnet ef migrations script > migration.sql

# Script depuis une migration spécifique
dotnet ef migrations script MigrationSource MigrationCible
```

---

## 🧪 Tests de l'API

### Tests via PowerShell

```powershell
# Test simple GET
Invoke-RestMethod -Uri "http://localhost:5002/api/Ecole"

# Test POST (Authentification)
$body = @{
    emailOuTelephone = "superadmin@kelasinabiso.cd"
    motDePasse = "Super-Admin"
} | ConvertTo-Json

$response = Invoke-RestMethod `
    -Uri "http://localhost:5002/api/Utilisateur/authentifier" `
    -Method Post `
    -Body $body `
    -ContentType "application/json"

$response

# Test avec headers personnalisés
Invoke-RestMethod `
    -Uri "http://localhost:5002/api/Ecole" `
    -Method Get `
    -Headers @{"Accept"="application/json"}

# Tester la disponibilité du port
Test-NetConnection -ComputerName localhost -Port 5002
```

### Tests via curl (si installé)

```powershell
# Test GET
curl http://localhost:5002/api/Ecole

# Test POST
curl -X POST "http://localhost:5002/api/Utilisateur/authentifier" `
  -H "Content-Type: application/json" `
  -d '{\"emailOuTelephone\":\"superadmin@kelasinabiso.cd\",\"motDePasse\":\"Super-Admin\"}'

# Télécharger curl si pas installé
# Via Chocolatey: choco install curl
```

---

## 📊 Monitoring et Logs

### Voir les processus

```powershell
# Voir tous les processus dotnet
Get-Process -Name dotnet | Select-Object Id, ProcessName, StartTime

# Voir le processus KelasiNaBiso
Get-Process -Name KelasiNaBiso | Select-Object Id, CPU, WorkingSet64

# Voir les ports utilisés
netstat -ano | findstr :5002
netstat -ano | findstr :7102
```

### Logs et debugging

```powershell
# Lancer avec logs détaillés
dotnet run --verbosity detailed

# Lancer en mode Debug
dotnet run --configuration Debug

# Lancer en mode Release
dotnet run --configuration Release

# Voir les logs Entity Framework
# Dans appsettings.json, modifier:
"Microsoft.EntityFrameworkCore": "Information"  # ou "Debug"
```

---

## 🔍 Diagnostic

### Vérifier l'état de l'API

```powershell
# Test de connectivité
Test-NetConnection -ComputerName localhost -Port 5002 -InformationLevel Detailed

# Tester Swagger
Start-Process "http://localhost:5002/swagger"

# Vérifier les processus actifs
Get-Process | Where-Object {$_.ProcessName -like "*dotnet*" -or $_.ProcessName -like "*KelasiNaBiso*"}
```

### Résoudre les problèmes

```powershell
# Port déjà utilisé ?
netstat -ano | findstr :5002
# Si occupé, tuez le processus avec son PID

# MariaDB pas démarré ?
Get-Service -Name *maria*,*mysql*
net start mariadb

# Logs d'erreur MariaDB
Get-Content "C:\Program Files\MariaDB 10.11\data\*.err" -Tail 50

# Nettoyer et rebuild
dotnet clean
dotnet restore
dotnet build
```

---

## 🚀 Développement

### Hot reload (rechargement auto)

```powershell
# Lancer avec watch (redémarre automatiquement lors des changements)
dotnet watch run

# Watch avec logs détaillés
dotnet watch run --verbosity detailed
```

### Nettoyer le projet

```powershell
# Nettoyer les fichiers de build
dotnet clean

# Supprimer obj et bin manuellement
Remove-Item -Recurse -Force obj,bin

# Tout nettoyer et reconstruire
dotnet clean
Remove-Item -Recurse -Force obj,bin
dotnet restore
dotnet build
```

---

## 📦 Publication

### Publier l'application

```powershell
# Publier en mode Release
dotnet publish --configuration Release

# Publier vers un dossier spécifique
dotnet publish -c Release -o ./publish

# Publier avec runtime spécifique (Windows)
dotnet publish -c Release -r win-x64 --self-contained

# Publier avec runtime Linux
dotnet publish -c Release -r linux-x64 --self-contained
```

---

## 🔐 Sécurité

### Gestion des secrets

```powershell
# Initialiser les secrets utilisateur
dotnet user-secrets init

# Ajouter un secret
dotnet user-secrets set "ConnectionStrings:KelasiConnection" "Server=...;Password=..."

# Lister les secrets
dotnet user-secrets list

# Supprimer un secret
dotnet user-secrets remove "ConnectionStrings:KelasiConnection"

# Nettoyer tous les secrets
dotnet user-secrets clear
```

---

## 🧹 Maintenance

### Mettre à jour les packages

```powershell
# Lister les packages obsolètes
dotnet list package --outdated

# Mettre à jour un package spécifique
dotnet add package Pomelo.EntityFrameworkCore.MySql --version 6.0.3

# Mettre à jour tous les packages (attention !)
# Faites-le manuellement dans le .csproj
```

### Vérifier les dépendances

```powershell
# Voir l'arbre des dépendances
dotnet list package --include-transitive

# Voir les versions installées
dotnet list package
```

---

## 📋 COMMANDES LES PLUS UTILISÉES

### Top 10 commandes quotidiennes

```powershell
# 1. Démarrer l'API
dotnet run

# 2. Arrêter l'API
Stop-Process -Name KelasiNaBiso -Force

# 3. Compiler
dotnet build

# 4. Restaurer les packages
dotnet restore

# 5. Ouvrir Swagger
Start-Process "http://localhost:5002/swagger"

# 6. Démarrer MariaDB
net start mariadb

# 7. Se connecter à MariaDB
mysql -u kansa -pkansa2025

# 8. Test API rapide
Invoke-RestMethod -Uri "http://localhost:5002/api/Ecole"

# 9. Appliquer les migrations
dotnet ef database update

# 10. Nettoyer et rebuild
dotnet clean; dotnet build
```

---

## 🎯 SCRIPT DE DÉMARRAGE RAPIDE

Créez un fichier `start-api.ps1` :

```powershell
# start-api.ps1
Write-Host "🚀 Démarrage de KelasiNaBiso API..." -ForegroundColor Cyan

# Vérifier MariaDB
$mariadb = Get-Service -Name *maria*,*mysql* -ErrorAction SilentlyContinue
if ($mariadb -and $mariadb.Status -ne 'Running') {
    Write-Host "📊 Démarrage de MariaDB..." -ForegroundColor Yellow
    net start mariadb
}

# Naviguer vers le projet
cd G:\KelasiNaBiso\KelasiNaBisoAPI

# Démarrer l'API
Write-Host "⚡ Lancement de l'API..." -ForegroundColor Yellow
dotnet run

# Ouvrir Swagger automatiquement
Start-Sleep -Seconds 5
Start-Process "http://localhost:5002/swagger"
```

Utilisez-le :
```powershell
.\start-api.ps1
```

---

## 🎯 SCRIPT D'ARRÊT

Créez un fichier `stop-api.ps1` :

```powershell
# stop-api.ps1
Write-Host "⏹️ Arrêt de KelasiNaBiso API..." -ForegroundColor Cyan

# Arrêter l'application
Stop-Process -Name KelasiNaBiso -Force -ErrorAction SilentlyContinue

Write-Host "✅ Application arrêtée avec succès!" -ForegroundColor Green
```

Utilisez-le :
```powershell
.\stop-api.ps1
```

---

## 🔄 SCRIPT DE REDÉMARRAGE

Créez un fichier `restart-api.ps1` :

```powershell
# restart-api.ps1
Write-Host "🔄 Redémarrage de KelasiNaBiso API..." -ForegroundColor Cyan

# Arrêter
Write-Host "⏹️ Arrêt..." -ForegroundColor Yellow
Stop-Process -Name KelasiNaBiso -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

# Démarrer
Write-Host "▶️ Démarrage..." -ForegroundColor Yellow
cd G:\KelasiNaBiso\KelasiNaBisoAPI
dotnet run
```

Utilisez-le :
```powershell
.\restart-api.ps1
```

---

## 📝 RÉSUMÉ DES COMMANDES

| Action | Commande |
|--------|----------|
| **Démarrer** | `dotnet run` |
| **Arrêter** | `Stop-Process -Name KelasiNaBiso -Force` |
| **Compiler** | `dotnet build` |
| **Nettoyer** | `dotnet clean` |
| **Restaurer** | `dotnet restore` |
| **Migrations** | `dotnet ef database update` |
| **MariaDB start** | `net start mariadb` |
| **MariaDB stop** | `net stop mariadb` |
| **Swagger** | `Start-Process "http://localhost:5002/swagger"` |
| **Test API** | `curl http://localhost:5002/api/Ecole` |

---

## 🎯 COMMANDE RAPIDE - TOUT EN UN

```powershell
# Arrêter, nettoyer, reconstruire et relancer
Stop-Process -Name KelasiNaBiso -Force -ErrorAction SilentlyContinue; `
dotnet clean; `
dotnet restore; `
dotnet build; `
dotnet run
```

---

**📚 Pour plus d'aide, consultez `START_HERE.md` !**

---

*Guide des commandes créé le: 23 octobre 2025*  
*Projet: KelasiNaBiso API*  
*Status: ✅ COMPLET*

