# 🔔 Guide de Test Automatisé des Notifications Push

## 📋 Vue d'ensemble

Ce guide explique comment utiliser le script `test-notifications-push-automatise.ps1` pour tester automatiquement les notifications push Firebase pour les événements de **présence** et **paiement**.

---

## 🚀 Prérequis

### 1. API démarrée
- L'API doit être en cours d'exécution sur `https://localhost:7102`
- Firebase Admin SDK doit être initialisé (vérifier les logs au démarrage)
- Vous devriez voir : `✅ Firebase Admin SDK initialisé avec succès`

### 2. Données de test dans la base de données
- Au moins **un élève** avec `Statut = true`
- L'élève doit avoir un **tuteur actif** (`Statut = true`)
- Le tuteur doit avoir un **utilisateur associé** avec `Statut = true`
- **Recommandé** : L'utilisateur doit avoir au moins un **device enregistré** dans `UserDevices` avec `IsActive = true`

### 3. Configuration Firebase
- Le fichier `kelasinabiso-de502-firebase-adminsdk-fbsvc-954ac3e526.json` doit être présent à la racine du projet
- Le chemin dans `appsettings.Development.json` doit être correct

---

## 📝 Utilisation du Script

### Exécution de base

```powershell
.\test-notifications-push-automatise.ps1
```

### Paramètres optionnels

```powershell
.\test-notifications-push-automatise.ps1 `
    -ApiUrl "https://localhost:7102" `
    -Email "superadmin@kelasinabiso.cd" `
    -Password "Super-Admin" `
    -IdEcole 18 `
    -SkipSslCheck
```

**Paramètres disponibles :**
- `-ApiUrl` : URL de l'API (défaut: `https://localhost:7102`)
- `-Email` : Email de l'utilisateur pour l'authentification (défaut: `superadmin@kelasinabiso.cd`)
- `-Password` : Mot de passe de l'utilisateur (défaut: `Super-Admin`)
- `-IdEcole` : ID de l'école à utiliser pour les tests (défaut: `18`)
- `-SkipSslCheck` : Ignorer les erreurs de certificat SSL (défaut: `true`)

---

## 🔍 Ce que fait le script

### Étape 1 : Authentification
- Se connecte à l'API avec les credentials Super-Admin
- Récupère un token JWT pour les requêtes suivantes

### Étape 2 : Recherche d'un élève de test
- Récupère les élèves de l'école spécifiée
- Cherche un élève avec :
  - Tuteur actif
  - Utilisateur associé au tuteur
  - Devices actifs (optionnel mais recommandé)

### Étape 3 : Test notification Présence
- Crée une présence (pointage) pour l'élève
- Déclenche automatiquement :
  - 📲 Notification push Firebase
  - 🔔 Notification SignalR
  - 📱 SMS au tuteur (si configuré)

### Étape 4 : Test notification Paiement
- Crée un paiement de frais pour l'élève
- Déclenche automatiquement :
  - 📲 Notification push Firebase
  - 🔔 Notification SignalR
  - 📱 SMS au tuteur (si configuré)

---

## ✅ Vérification des résultats

### 1. Logs de l'API

Dans la console de l'API, recherchez ces messages :

**✅ Succès :**
```
✅ Notification PUSH Firebase envoyée au tuteur {nomTuteur} (User ID: {idUtilisateur}) pour présence élève {nomEleve}
✅ Notification PUSH Firebase paiement envoyée au tuteur {nomTuteur} (User ID: {idUtilisateur}) pour paiement élève {nomEleve} (Montant: {montant})
```

**⚠️ Échec (aucun device actif) :**
```
⚠️ Échec notification PUSH Firebase pour tuteur {nomTuteur} (aucun device actif ou erreur Firebase)
```

**❌ Erreur Firebase :**
```
❌ Erreur lors de l'envoi notification PUSH Firebase pour présence {idPresence}
```

### 2. Device Mobile (Android/iOS)

Si un device mobile est enregistré et actif :
- Vérifiez que la notification push apparaît sur le mobile
- Le titre et le message doivent correspondre à l'événement (présence ou paiement)

### 3. SignalR (Web)

Si une application web est connectée via SignalR :
- Vérifiez que la notification apparaît en temps réel dans l'interface
- La notification doit être reçue immédiatement après la création de l'événement

### 4. SMS

Si Twilio est configuré et activé :
- Vérifiez que le SMS a été envoyé au numéro de téléphone du tuteur
- Le SMS doit contenir les informations du paiement ou de la présence

---

## 🐛 Dépannage

### Problème : "Aucun élève avec tuteur et utilisateur actif trouvé"

**Solution :**
1. Vérifiez qu'au moins un élève existe dans la base de données
2. Vérifiez que l'élève a un tuteur associé (`IdTuteur` non null)
3. Vérifiez que le tuteur a un utilisateur associé
4. Vérifiez que l'utilisateur et le tuteur ont `Statut = true`

**Requête SQL pour vérifier :**
```sql
SELECT 
    e.IdEleve,
    e.NomComplet AS NomEleve,
    t.IdTuteur,
    t.NomComplet AS NomTuteur,
    u.IdUtilisateur,
    u.NomUtilisateur,
    u.Statut AS StatutUtilisateur,
    t.Statut AS StatutTuteur
FROM Eleves e
INNER JOIN Tuteurs t ON e.IdTuteur = t.IdTuteur
INNER JOIN Utilisateurs u ON u.IdTuteur = t.IdTuteur
WHERE e.Statut = 1 
  AND t.Statut = 1 
  AND u.Statut = 1
LIMIT 1;
```

### Problème : "Échec notification PUSH Firebase (aucun device actif)"

**Solution :**
1. Vérifiez qu'au moins un device est enregistré pour l'utilisateur du tuteur
2. Vérifiez que le device a `IsActive = true`

**Requête SQL pour vérifier :**
```sql
SELECT 
    ud.IdDevice,
    ud.DeviceToken,
    ud.Platform,
    ud.IsActive,
    u.IdUtilisateur,
    u.NomUtilisateur
FROM UserDevices ud
INNER JOIN Utilisateurs u ON ud.IdUtilisateur = u.IdUtilisateur
WHERE u.IdTuteur = {idTuteur}
  AND ud.IsActive = 1;
```

### Problème : "Firebase Admin SDK non initialisé"

**Solution :**
1. Vérifiez que le fichier `kelasinabiso-de502-firebase-adminsdk-fbsvc-954ac3e526.json` existe à la racine du projet
2. Vérifiez que le chemin dans `appsettings.Development.json` est correct :
   ```json
   "Firebase": {
     "CredentialsPath": "kelasinabiso-de502-firebase-adminsdk-fbsvc-954ac3e526.json"
   }
   ```
3. Redémarrez l'API et vérifiez les logs au démarrage

### Problème : "Erreur SSL"

**Solution :**
Le script utilise `-SkipSslCheck` par défaut, mais si vous rencontrez des erreurs SSL :
1. Vérifiez que l'API est bien démarrée
2. Vérifiez que l'URL est correcte (`https://localhost:7102`)
3. Si nécessaire, acceptez le certificat SSL auto-signé dans votre navigateur d'abord

---

## 📊 Exemple de sortie du script

```
════════════════════════════════════════════════════════════════════
  ÉTAPE 1 : AUTHENTIFICATION
════════════════════════════════════════════════════════════════════

ℹ️  Tentative de connexion avec superadmin@kelasinabiso.cd...
✅ Authentification réussie !
ℹ️     Token: eyJhbGciOiJIUzI1NiIs...
ℹ️     User ID: 296

════════════════════════════════════════════════════════════════════
  ÉTAPE 2 : RECHERCHE D'UN ÉLÈVE POUR LES TESTS
════════════════════════════════════════════════════════════════════

ℹ️  Récupération des élèves de l'école 18...
ℹ️     10 élève(s) trouvé(s)
ℹ️     Vérification de l'élève: KABAMBA Patrick Junior (ID: 82)
ℹ️        ✅ Tuteur actif: MUKENDI Grace
ℹ️        ✅ Utilisateur trouvé: parent1 (ID: 123)
✅        ✅ 2 device(s) actif(s) trouvé(s) !
✅ Élève de test sélectionné :
ℹ️     • Élève: KABAMBA Patrick Junior (ID: 82)
ℹ️     • Tuteur: MUKENDI Grace (ID: 45)
ℹ️     • Téléphone tuteur: +243900000000
ℹ️     • Utilisateur ID: 123

════════════════════════════════════════════════════════════════════
  ÉTAPE 3 : TEST NOTIFICATION PRÉSENCE (POINTAGE ÉLÈVE)
════════════════════════════════════════════════════════════════════

ℹ️  Création d'une présence...
ℹ️     • Élève: KABAMBA Patrick Junior
ℹ️     • Date: 2025-11-06
ℹ️     • Heure d'arrivée: 10:30:00
ℹ️     • Statut: PRÉSENT
✅ Présence créée avec succès !
ℹ️     • ID Présence: 1250
ℹ️     • Date: 2025-11-06 10:30:00

✅ Notifications envoyées :
ℹ️     📲 Push Firebase (mobile Android/iOS)
ℹ️     🔔 SignalR (web temps réel)
ℹ️     📱 SMS au tuteur (si configuré)

⚠️  VÉRIFICATIONS À FAIRE :
ℹ️     1. Vérifier les logs de l'API ci-dessous :
ℹ️        → Rechercher: '✅ Notification PUSH Firebase envoyée au tuteur'
ℹ️        → Ou: '⚠️ Échec notification PUSH Firebase'
...

════════════════════════════════════════════════════════════════════
  ÉTAPE 4 : TEST NOTIFICATION PAIEMENT (FRAIS SCOLAIRES)
════════════════════════════════════════════════════════════════════

...

════════════════════════════════════════════════════════════════════
  RÉSUMÉ DES TESTS
════════════════════════════════════════════════════════════════════

✅ Tests terminés avec succès !
...
```

---

## 🔄 Relancer les tests

Pour relancer les tests rapidement :

```powershell
.\test-notifications-push-automatise.ps1
```

Le script créera automatiquement de nouvelles présences et paiements à chaque exécution.

---

## 📚 Ressources complémentaires

- **Firebase Console** : https://console.firebase.google.com/project/kelasinabiso-de502
- **Logs de l'API** : Consulter la console où l'API est démarrée
- **Documentation Firebase** : https://firebase.google.com/docs/cloud-messaging

---

## ✅ Checklist de vérification

Avant d'exécuter le script, vérifiez :

- [ ] API démarrée et accessible sur `https://localhost:7102`
- [ ] Firebase Admin SDK initialisé (voir logs)
- [ ] Fichier Firebase credentials présent à la racine
- [ ] Au moins un élève avec tuteur et utilisateur actif dans la DB
- [ ] (Recommandé) Au moins un device actif pour l'utilisateur du tuteur
- [ ] Twilio configuré (si vous voulez tester les SMS)

---

**Date de création :** 2025-11-06  
**Dernière mise à jour :** 2025-11-06

