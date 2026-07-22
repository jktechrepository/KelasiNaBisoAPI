# 📥 Guide d'Installation MariaDB 10 sur Windows

## 🎯 Objectif
Installer **MariaDB 10.11 (LTS)** sur Windows pour remplacer MySQL

---

## 📋 Option 1 : Installation avec l'installeur MSI (Recommandée)

### Étape 1 : Téléchargement

1. **Ouvrez votre navigateur** et allez sur :
   ```
   https://mariadb.org/download/?t=mariadb&p=mariadb&r=10.11.10
   ```

2. **Sélectionnez** :
   - Version : **10.11.10** (LTS - Long Term Support)
   - OS : **Windows**
   - Package Type : **MSI Package**

3. **Téléchargez** : `mariadb-10.11.10-winx64.msi` (~120 MB)

### Étape 2 : Installation

1. **Double-cliquez** sur le fichier `.msi` téléchargé

2. **Suivez l'assistant d'installation** :

   **Écran 1 : Welcome**
   - Cliquez sur **Next**

   **Écran 2 : License Agreement**
   - Acceptez la licence ✅
   - Cliquez sur **Next**

   **Écran 3 : Custom Setup**
   - Laissez les options par défaut
   - ✅ **HeidiSQL** (optionnel mais recommandé)
   - Cliquez sur **Next**

   **Écran 4 : Database instance properties**
   ```
   ✅ Enable networking
   ✅ TCP Port: 3306
      (ou 3307 si MySQL utilise déjà le port 3306)
   
   ✅ UTF8 as default server's character set
   
   ✅ Root password: [Choisissez un mot de passe sécurisé]
   ✅ Confirm password: [Confirmez]
   
   ✅ Create user account:
      - Username: kansa
      - Password: kansa2025
   ```

   **Écran 5 : Windows Service**
   ```
   ✅ Install as service
   ✅ Service Name: MySQL (ou MariaDB)
   ✅ Enable networking
   ✅ Run as: Local System Account
   ```

   **Écran 6 : Ready to Install**
   - Cliquez sur **Install**
   - Attendez la fin de l'installation (~2-3 minutes)

3. **Terminé !** ✅
   - Cliquez sur **Finish**

### Étape 3 : Vérification de l'installation

#### 3.1 Vérifier le service

1. **Ouvrez PowerShell en Administrateur**
2. **Exécutez** :
   ```powershell
   Get-Service -Name *maria*,*mysql*
   ```

3. **Résultat attendu** :
   ```
   Status   Name               DisplayName
   ------   ----               -----------
   Running  MySQL              MySQL
   ```

#### 3.2 Ajouter MariaDB au PATH

1. **Ouvrez PowerShell en Administrateur**
2. **Exécutez** :
   ```powershell
   # Trouver l'emplacement de MariaDB
   $mariadbPath = "C:\Program Files\MariaDB 10.11\bin"
   
   # Vérifier que le dossier existe
   if (Test-Path $mariadbPath) {
       # Ajouter au PATH système
       [Environment]::SetEnvironmentVariable(
           "Path",
           [Environment]::GetEnvironmentVariable("Path", "Machine") + ";$mariadbPath",
           "Machine"
       )
       Write-Host "✅ MariaDB ajouté au PATH" -ForegroundColor Green
   } else {
       Write-Host "❌ Dossier MariaDB non trouvé" -ForegroundColor Red
       Write-Host "Recherche en cours..." -ForegroundColor Yellow
       
       # Recherche automatique
       $found = Get-ChildItem "C:\Program Files" -Directory -Filter "MariaDB*" -ErrorAction SilentlyContinue
       if ($found) {
           Write-Host "MariaDB trouvé dans: $($found.FullName)\bin" -ForegroundColor Cyan
       }
   }
   ```

3. **Fermez et rouvrez PowerShell** pour que les changements prennent effet

4. **Testez** :
   ```powershell
   mysql --version
   ```
   
   **Résultat attendu** :
   ```
   mysql  Ver 15.1 Distrib 10.11.10-MariaDB, for Win64 (AMD64)
   ```

#### 3.3 Tester la connexion

```powershell
# Se connecter en tant que root
mysql -u root -p

# Entrez votre mot de passe root
```

**Dans la console MySQL** :
```sql
-- Vérifier la version
SELECT VERSION();

-- Résultat attendu: 10.11.10-MariaDB

-- Créer la base de données (si pas encore faite)
CREATE DATABASE IF NOT EXISTS KelasiNaBisoDb 
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;

-- Vérifier les privilèges de l'utilisateur kansa
SHOW GRANTS FOR 'kansa'@'localhost';

-- Si l'utilisateur n'existe pas, le créer
CREATE USER IF NOT EXISTS 'kansa'@'localhost' IDENTIFIED BY 'kansa2025';
GRANT ALL PRIVILEGES ON KelasiNaBisoDb.* TO 'kansa'@'localhost';
FLUSH PRIVILEGES;

-- Quitter
EXIT;
```

---

## 📋 Option 2 : Installation avec Chocolatey

Si vous préférez utiliser Chocolatey (gestionnaire de paquets Windows) :

### Installer Chocolatey (si pas déjà installé)

```powershell
# Ouvrir PowerShell en Administrateur
Set-ExecutionPolicy Bypass -Scope Process -Force
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
```

### Installer MariaDB

```powershell
choco install mariadb -y
```

---

## 🔧 Configuration Post-Installation

### 1. Arrêter MySQL (si existant)

Si vous avez MySQL qui tourne actuellement :

```powershell
# Arrêter le service MySQL
net stop MySQL80

# Ou via les services
Stop-Service -Name MySQL80
```

### 2. Démarrer MariaDB

```powershell
# Démarrer le service
net start MySQL

# Ou
Start-Service -Name MySQL
```

### 3. Configurer le pare-feu (si nécessaire)

```powershell
# Autoriser MariaDB dans le pare-feu
New-NetFirewallRule -DisplayName "MariaDB" -Direction Inbound -Protocol TCP -LocalPort 3306 -Action Allow
```

---

## 🧪 Tests de vérification

### Test 1 : Service démarré
```powershell
Get-Service -Name MySQL
# Status doit être "Running"
```

### Test 2 : Connexion
```powershell
mysql -u kansa -pkansa2025 -e "SELECT VERSION();"
```

### Test 3 : Base de données
```powershell
mysql -u kansa -pkansa2025 -e "SHOW DATABASES;"
# Vous devriez voir "KelasiNaBisoDb" (après migration)
```

---

## ❗ Troubleshooting

### Problème 1 : Port 3306 déjà utilisé

**Solution** : Installer MariaDB sur un autre port

Pendant l'installation, choisissez le port **3307** au lieu de **3306**.

Puis modifiez votre `appsettings.json` :
```json
"KelasiConnection": "Server=localhost;Database=KelasiNaBisoDb;User=kansa;Password=kansa2025;Port=3307;SslMode=none;CharSet=utf8mb4;"
```

### Problème 2 : Service ne démarre pas

```powershell
# Vérifier les logs
Get-EventLog -LogName Application -Source MySQL -Newest 10

# Ou consulter les logs MariaDB
type "C:\Program Files\MariaDB 10.11\data\*.err"
```

### Problème 3 : mysql commande non reconnue

**Solution** : Ajouter MariaDB au PATH (voir Étape 3.2 ci-dessus)

Ou utiliser le chemin complet :
```powershell
& "C:\Program Files\MariaDB 10.11\bin\mysql.exe" --version
```

### Problème 4 : Erreur d'authentification

```sql
-- Se connecter en root
mysql -u root -p

-- Recréer l'utilisateur
DROP USER IF EXISTS 'kansa'@'localhost';
CREATE USER 'kansa'@'localhost' IDENTIFIED BY 'kansa2025';
GRANT ALL PRIVILEGES ON *.* TO 'kansa'@'localhost' WITH GRANT OPTION;
FLUSH PRIVILEGES;
```

---

## 📚 Outils recommandés

### HeidiSQL (inclus avec l'installeur)
- Interface graphique pour gérer MariaDB
- Emplacement : `C:\Program Files\HeidiSQL\heidisql.exe`

### MySQL Workbench
- Compatible avec MariaDB
- Téléchargement : https://dev.mysql.com/downloads/workbench/

### DBeaver
- Client universel multi-bases
- Téléchargement : https://dbeaver.io/download/

---

## ✅ Prochaines étapes

Une fois MariaDB installé et configuré :

1. ✅ Vérifier que MariaDB fonctionne
2. 📥 Exécuter le script de migration : `.\migration-mysql-to-mariadb.ps1`
3. 🧪 Tester l'API avec MariaDB

---

## 🎉 C'est terminé !

MariaDB est maintenant installé et prêt à recevoir vos données !

**Prochaine étape** : Exécutez le script de migration
```powershell
.\migration-mysql-to-mariadb.ps1
```

---

*Guide créé pour KelasiNaBiso API*  
*Version MariaDB : 10.11 LTS*  
*Support jusqu'en : 2028*

