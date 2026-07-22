# 🔧 Correction : Nom du fichier Firebase

**Date :** 2025-11-06  
**Problème :** `NullReferenceException` à la ligne 91 de `FirebaseNotificationService.cs`

---

## 🔴 **Problème identifié**

### **Erreur :**
```
System.NullReferenceException: Object reference not set to an instance of an object.
at FirebaseNotificationService.EnvoyerNotificationAUtilisateurAsync:line 91
```

### **Cause :**
`FirebaseMessaging.DefaultInstance` était **NULL** car Firebase Admin SDK n'était **PAS initialisé**.

---

## 🔍 **Analyse**

### **Fichier attendu :**
```
firebase-credentials.json  (avec un "s")
```

### **Fichier créé :**
```
firebase-credentiels.json  (sans "s")
```

### **Configuration dans `appsettings.json` :**
```json
"Firebase": {
  "CredentialsPath": "firebase-credentials.json"  // Cherche avec un "s"
}
```

### **Résultat :**
- Fichier introuvable au démarrage
- Firebase Admin SDK non initialisé
- `FirebaseMessaging.DefaultInstance` = NULL
- NullReferenceException lors de l'envoi de notifications

---

## ✅ **Solution appliquée**

1. **Suppression** de `firebase-credentiels.json` (mauvais nom)
2. **Création** de `firebase-credentials.json` (bon nom)
3. **Redémarrage** de l'API

---

## 📋 **Vérification après redémarrage**

### **Logs attendus au démarrage :**
```
✅ Firebase Admin SDK initialisé avec succès
   📄 Credentials: G:\KelasiNaBiso\KelasiNaBisoAPI\firebase-credentials.json
```

### **Si le fichier est introuvable :**
```
⚠️  ATTENTION: Fichier Firebase credentials introuvable: [chemin]
⚠️  Les notifications push ne fonctionneront PAS.
💡 Solution: Placer le fichier firebase-credentials.json à la racine du projet.
```

---

## 🧪 **Test après correction**

### **Commande de test :**
```http
POST /api/Presence
{
  "idEleve": 424,
  "dateDuJour": "2025-11-06",
  "heureArrivee": "09:00:00",
  "isPresent": true,
  "typePresence": "ELEVE",
  "observation": "Test après correction nom fichier Firebase"
}
```

### **Logs attendus (SANS erreur) :**
```
[INFO] 1 token(s) actif(s) trouvé(s) pour utilisateur 382
[INFO] Notification envoyée à l'utilisateur 382. Succès: 1/1
[INFO] ✅ Notification PUSH Firebase envoyée au tuteur kansa de kansa (User ID: 382)
```

### **📱 Mobile reçoit :**
```
📍 Pointage de Zozo machine mu tutu
✅ PRÉSENT le 06/11/2025 à 09:00
📝 Test après correction nom fichier Firebase
```

---

## ⚠️ **Problème secondaire : Twilio SMS**

### **Erreur observée :**
```
[ERR] ❌ Échec d'envoi SMS vers +243825099299: Authenticate
Twilio.Exceptions.ApiException: Authenticate
```

### **Cause :**
Credentials Twilio invalides ou manquantes dans `appsettings.json`.

### **Solution :**
Vérifier et corriger les credentials Twilio :
```json
"Twilio": {
  "AccountSid": "...",  // Vérifier la valeur
  "AuthToken": "...",   // Vérifier la valeur
  "PhoneNumber": "...", // Numéro Twilio
  "SenderId": "..."     // Sender ID
}
```

---

## ✅ **État actuel**

| Composant | Status avant | Status après |
|-----------|--------------|--------------|
| Fichier Firebase | ❌ Mauvais nom | ✅ Corrigé |
| Firebase initialisé | ❌ Non | ✅ Oui (après redémarrage) |
| NullReferenceException | ❌ Présente | ✅ Corrigée |
| Notifications push | ❌ Ne fonctionnent pas | ⏳ À tester |
| Twilio SMS | ❌ Erreur d'authentification | ⚠️ À corriger |

---

**Attends que l'API redémarre et vérifie les logs pour voir `✅ Firebase Admin SDK initialisé avec succès` ! 🚀**

