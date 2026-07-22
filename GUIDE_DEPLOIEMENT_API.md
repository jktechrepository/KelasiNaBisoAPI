# 📦 Guide de déploiement - KelasiNaBisoAPI

**Date :** 2025-11-06  
**Version :** Release  
**Dossier de publication :** `./publish`

---

## ✅ **Publication réussie**

L'API a été publiée avec succès dans le dossier `./publish`.

---

## 📂 **Contenu du dossier publish**

Le dossier `./publish` contient tous les fichiers nécessaires pour déployer l'API :

```
publish/
├── KelasiNaBiso.dll
├── KelasiNaBiso.exe
├── KelasiNaBiso.deps.json
├── KelasiNaBiso.runtimeconfig.json
├── appsettings.json
├── appsettings.Production.json
├── kelasinabiso-de502-firebase-adminsdk-fbsvc-954ac3e526.json  ← ⚠️ À copier manuellement
├── web.config
└── [autres DLLs et dépendances]
```

---

## ⚠️ **IMPORTANT : Fichier Firebase**

Le fichier Firebase **N'EST PAS** copié automatiquement dans le dossier `publish`.

### **Action requise :**

Copie manuellement le fichier Firebase dans le dossier `publish` :

```powershell
Copy-Item "kelasinabiso-de502-firebase-adminsdk-fbsvc-954ac3e526.json" -Destination "./publish/"
```

---

## 🔧 **Configuration pour la production**

### **1. Modifier `appsettings.Production.json`**

Dans le dossier `./publish`, édite `appsettings.Production.json` pour configurer :

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=PRODUCTION_SERVER;Database=kelasinabiso;User=USERNAME;Password=PASSWORD;Port=3306;..."
  },
  "Jwt": {
    "SecretKey": "NOUVELLE_CLE_SECRETE_ULTRA_SECURE_PRODUCTION"
  },
  "Firebase": {
    "CredentialsPath": "kelasinabiso-de502-firebase-adminsdk-fbsvc-954ac3e526.json"
  },
  "Twilio": {
    "AccountSid": "PRODUCTION_ACCOUNT_SID",
    "AuthToken": "PRODUCTION_AUTH_TOKEN",
    "SenderId": "PRODUCTION_SENDER_ID"
  },
  "Cors": {
    "AllowedOrigins": [
      "https://knb.kansaconsulting.com",
      "https://dev-knb.kansaconsulting.com"
    ]
  }
}
```

---

## 🚀 **Déploiement sur le serveur**

### **Option 1 : IIS (Windows Server)**

1. Copier le dossier `publish` sur le serveur
2. Créer un site IIS pointant vers ce dossier
3. Installer le module ASP.NET Core Hosting Bundle
4. Configurer le pool d'applications (.NET CLR Version: No Managed Code)
5. Démarrer le site

### **Option 2 : Linux avec systemd**

1. Copier le dossier `publish` vers `/var/www/kelasinabiso/`
2. Créer un service systemd :

```bash
sudo nano /etc/systemd/system/kelasinabiso.service
```

```ini
[Unit]
Description=KelasiNaBiso API

[Service]
WorkingDirectory=/var/www/kelasinabiso
ExecStart=/usr/bin/dotnet /var/www/kelasinabiso/KelasiNaBiso.dll
Restart=always
RestartSec=10
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

3. Activer et démarrer :

```bash
sudo systemctl enable kelasinabiso
sudo systemctl start kelasinabiso
sudo systemctl status kelasinabiso
```

### **Option 3 : Docker**

1. Créer un `Dockerfile` à la racine du projet
2. Build l'image Docker
3. Déployer avec Docker Compose

---

## 📋 **Checklist de déploiement**

### **Avant le déploiement :**
- [ ] ✅ API publiée dans `./publish`
- [ ] Fichier Firebase copié dans `./publish`
- [ ] `appsettings.Production.json` configuré
- [ ] Connection string de production configurée
- [ ] Clé JWT de production configurée
- [ ] Credentials Twilio de production configurés
- [ ] CORS origins de production configurés

### **Après le déploiement :**
- [ ] API accessible via URL de production
- [ ] Swagger UI accessible (si activé en production)
- [ ] Base de données accessible
- [ ] Firebase notifications fonctionnelles
- [ ] SMS Twilio fonctionnels
- [ ] SignalR fonctionnel
- [ ] CORS configuré correctement

---

## ⚠️ **Actions critiques AVANT mise en production**

### **1. Exécuter le script SQL**

Dans HeidiSQL sur la DB de production :

```sql
-- Exécuter APPLIQUER_MIGRATION_STATUT_NULLABLE.sql
```

Ce script corrige :
- Colonnes `Statut` (VARCHAR → TINYINT(1) NULL)
- Colonnes `HoraireIdHoraire`, `IdVacation` (permettre NULL)
- GUID invalides

### **2. Copier le fichier Firebase**

```powershell
Copy-Item "kelasinabiso-de502-firebase-adminsdk-fbsvc-954ac3e526.json" -Destination "./publish/"
```

### **3. Vérifier appsettings.Production.json**

S'assurer que toutes les configurations pointent vers les ressources de production.

---

## 🧪 **Tests après déploiement**

1. **Test de santé** : `GET /api/health` (si implémenté)
2. **Authentification** : `POST /api/Utilisateur/authentifier`
3. **Dashboard Global** : `GET /api/Dashboard/global?idEcole=18`
4. **Notifications** : Créer une présence et vérifier les notifications
5. **CORS** : Tester depuis le frontend web

---

## 📊 **Monitoring**

Après déploiement, surveiller :
- Logs de l'application
- Utilisation CPU/Mémoire
- Temps de réponse API
- Erreurs HTTP 500
- Connexions base de données

---

## 🎯 **Nouveautés de cette version**

1. ✅ **Dashboard Global amélioré** :
   - Période mensuelle (au lieu d'un jour)
   - Statistiques générales (Classes, Élèves, Enseignants, Directions)

2. ✅ **Corrections notifications** :
   - ObjectDisposedException corrigé (IServiceScopeFactory)
   - TwilioSmsService DbContext handling
   - HoraireIdHoraire ajouté au modèle Presence

3. ✅ **CORS amélioré** :
   - Headers Cache-Control, Pragma, Expires autorisés
   - Origins de production configurés

4. ⚠️ **Firebase** :
   - Configuration mise à jour
   - Fichier credentials téléchargé
   - Nécessite vérification d'initialisation

---

**L'API est prête à être déployée ! 🚀**

