# 🔥 GUIDE : MISE À JOUR FIREBASE CREDENTIALS

**Date** : 27 janvier 2025  
**Projet** : KelasiNaBisoAPI  
**Nouveau Project ID** : `kelasinabiso-de502`

---

## ✅ ÉTAPE 1 : CREDENTIALS MISES À JOUR

Les fichiers `appsettings.json` et `appsettings.Development.json` ont été mis à jour avec vos nouvelles credentials :

```json
{
  "Firebase": {
    "ProjectId": "kelasinabiso-de502",
    "SenderId": "1018914946705",
    "ApiKey": "AIzaSyBLDtIcIOlCzxDUgz_gy9Aa_JGR8kAbLcg",
    "ServiceAccountEmail": "firebase-adminsdk-kelasinabiso@kelasinabiso-de502.iam.gserviceaccount.com",
    "CredentialsPath": "firebase-credentials.json"
  }
}
```

### ✅ Récapitulatif des changements

| Paramètre | Ancienne valeur | Nouvelle valeur |
|-----------|-----------------|-----------------|
| **ProjectId** | kelasinabiso-app-2025 | **kelasinabiso-de502** ✅ |
| **SenderId** | 123456789012 | **1018914946705** ✅ |
| **ApiKey** | AIzaSyDYPRtz0ZIdTFogbYjItfYDhmL_aOaX7SE | **AIzaSyBLDtIcIOlCzxDUgz_gy9Aa_JGR8kAbLcg** ✅ |
| **ServiceAccountEmail** | ...@kelasinabiso-app-2025... | ...@**kelasinabiso-de502**... ✅ |

---

## ⚠️ ÉTAPE 2 : TÉLÉCHARGER firebase-credentials.json

Pour que les **push notifications** fonctionnent, vous devez télécharger le fichier `firebase-credentials.json` depuis la console Firebase.

### 📥 Procédure détaillée

#### 1. Accéder à la console Firebase

```
🌐 https://console.firebase.google.com
```

#### 2. Sélectionner votre projet

- Cliquez sur **"kelasinabiso-de502"**

#### 3. Aller dans Paramètres du projet

```
⚙️ Icône d'engrenage en haut à gauche → Paramètres du projet
```

#### 4. Onglet "Comptes de service"

```
📋 Onglets en haut : Vue d'ensemble | Utilisateurs et autorisations | Intégrations | Comptes de service
```

#### 5. Générer une nouvelle clé privée

```
🔑 Section "SDK Admin Firebase"
   → Bouton "Générer une nouvelle clé privée"
   → Confirmer : "Générer la clé"
```

⚠️ **ATTENTION** : Ce fichier contient des **informations sensibles**. Ne le partagez jamais publiquement !

#### 6. Télécharger le fichier JSON

Le navigateur téléchargera un fichier avec un nom comme :
```
kelasinabiso-de502-firebase-adminsdk-xxxxx-xxxxxxxxxx.json
```

#### 7. Renommer le fichier

```
kelasinabiso-de502-firebase-adminsdk-xxxxx-xxxxxxxxxx.json
↓
firebase-credentials.json
```

#### 8. Placer le fichier dans le projet

```
G:\KelasiNaBiso\KelasiNaBisoAPI\
├── Program.cs
├── appsettings.json
├── firebase-credentials.json  ← ICI
└── ...
```

---

## 🔒 ÉTAPE 3 : SÉCURITÉ

### ✅ Vérifier le .gitignore

Le fichier `firebase-credentials.json` **NE DOIT PAS** être commité sur Git !

Vérifiez que votre `.gitignore` contient :

```gitignore
# Firebase credentials
firebase-credentials.json
**/firebase-credentials.json
```

### 🔐 Structure attendue du fichier

Votre `firebase-credentials.json` devrait ressembler à ceci :

```json
{
  "type": "service_account",
  "project_id": "kelasinabiso-de502",
  "private_key_id": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "private_key": "-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----\n",
  "client_email": "firebase-adminsdk-xxxxx@kelasinabiso-de502.iam.gserviceaccount.com",
  "client_id": "xxxxxxxxxxxxxxxxxxxx",
  "auth_uri": "https://accounts.google.com/o/oauth2/auth",
  "token_uri": "https://oauth2.googleapis.com/token",
  "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
  "client_x509_cert_url": "https://www.googleapis.com/robot/v1/metadata/x509/firebase-adminsdk-xxxxx%40kelasinabiso-de502.iam.gserviceaccount.com"
}
```

⚠️ **Vérifications importantes** :
- ✅ `project_id` = `"kelasinabiso-de502"`
- ✅ `client_email` contient `@kelasinabiso-de502.iam.gserviceaccount.com`

---

## 🚀 ÉTAPE 4 : REDÉMARRER L'API

Une fois le fichier `firebase-credentials.json` en place :

```bash
cd G:\KelasiNaBiso\KelasiNaBisoAPI
dotnet run
```

### ✅ Vérifications au démarrage

Vous devriez voir dans les logs :

```
✅ Firebase Admin SDK initialisé avec succès
   📄 Credentials: G:\KelasiNaBiso\KelasiNaBisoAPI\firebase-credentials.json
```

### ❌ Si vous voyez une erreur

```
❌ Erreur lors de l'initialisation de Firebase: ...
⚠️  Les notifications push ne fonctionneront pas.
```

**Solutions possibles** :
1. Vérifier que le fichier existe bien
2. Vérifier le nom du fichier (exactement `firebase-credentials.json`)
3. Vérifier que le `project_id` dans le JSON = `kelasinabiso-de502`
4. Régénérer une nouvelle clé depuis Firebase Console

---

## 🧪 ÉTAPE 5 : TESTER LES NOTIFICATIONS

### Test 1 : Enregistrer un device (FCM Token)

```http
POST http://localhost:5000/api/UserDevice/register-device
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json

{
  "fcmToken": "YOUR_FCM_TOKEN_FROM_MOBILE_APP",
  "deviceType": "Android",
  "deviceModel": "Samsung Galaxy S21",
  "osVersion": "Android 12"
}
```

### Test 2 : Envoyer une notification test

```http
POST http://localhost:5000/api/NotificationPush/send-to-user
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json

{
  "userId": 1,
  "title": "Test notification",
  "body": "Ceci est un test de notification push",
  "data": {
    "type": "TEST",
    "timestamp": "2025-01-27T10:00:00Z"
  }
}
```

### ✅ Résultat attendu

```json
{
  "success": true,
  "message": "Notification envoyée avec succès",
  "tokensReached": 1
}
```

---

## 📋 CHECKLIST FINALE

- [x] ✅ `appsettings.json` mis à jour
- [x] ✅ `appsettings.Development.json` mis à jour
- [ ] ⏳ `firebase-credentials.json` téléchargé depuis Firebase Console
- [ ] ⏳ `firebase-credentials.json` placé à la racine du projet
- [ ] ⏳ `.gitignore` vérifié (ne pas commit firebase-credentials.json)
- [ ] ⏳ API redémarrée
- [ ] ⏳ Logs de démarrage vérifiés (Firebase initialisé ✅)
- [ ] ⏳ Test d'envoi de notification effectué

---

## 🆘 DÉPANNAGE

### Problème 1 : "Fichier firebase-credentials.json introuvable"

**Cause** : Le fichier n'est pas au bon endroit ou mal nommé

**Solution** :
```bash
# Vérifier l'emplacement
dir firebase-credentials.json

# Le fichier DOIT être à la racine du projet :
G:\KelasiNaBiso\KelasiNaBisoAPI\firebase-credentials.json
```

### Problème 2 : "Erreur lors de l'initialisation de Firebase"

**Cause** : Le fichier JSON est invalide ou les credentials sont incorrects

**Solution** :
1. Ouvrir `firebase-credentials.json` et vérifier que c'est un JSON valide
2. Vérifier `project_id` = `"kelasinabiso-de502"`
3. Régénérer une nouvelle clé depuis Firebase Console

### Problème 3 : "Notification non reçue sur le mobile"

**Causes possibles** :
1. FCM Token invalide ou expiré
2. Device pas enregistré dans UserDevices
3. Firebase credentials incorrect
4. Application mobile pas configurée avec le bon `google-services.json`

**Solution** :
1. Vérifier que le FCM Token est valide
2. Réenregistrer le device via `/api/UserDevice/register-device`
3. Vérifier les logs de l'API

---

## 📞 SUPPORT

Si vous rencontrez des problèmes :

1. **Vérifier les logs** : `dotnet run` affiche les erreurs Firebase
2. **Tester avec Postman** : Utiliser les endpoints de test
3. **Consulter la documentation** : `ACTIVATION_FIREBASE_COMPLETE.md`

---

**Dernière mise à jour** : 27 janvier 2025  
**Auteur** : Équipe KelasiNaBiso  
**Version** : 1.0.0

