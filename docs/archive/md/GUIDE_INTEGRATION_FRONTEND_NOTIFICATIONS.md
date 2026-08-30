# 📱 GUIDE D'INTÉGRATION - NOTIFICATIONS PUSH FRONTEND

**Date** : 1 novembre 2025  
**API** : KelasiNaBisoAPI  
**Endpoints** : Authentification + SignalR Hub

---

## 🎯 VUE D'ENSEMBLE

Ce guide explique comment intégrer les notifications push dans vos applications frontend (Mobile et Web).

---

## 📱 PARTIE 1 : FRONTEND MOBILE (Flutter/React Native)

### 🔐 1. Authentification avec FCM Token

**Endpoint** : `POST /api/Utilisateur/authentifier`  
**URL** : `https://localhost:7105/api/Utilisateur/authentifier`

#### 📝 Payload JSON

```json
{
  "emailOuTelephone": "user@example.com",
  "motDePasse": "Password123!",
  "fcmToken": "fKx7Y...Jz9M",              // 🔑 Token Firebase (obligatoire pour notifications)
  "deviceType": "Android",                  // Android, iOS, ou Web
  "deviceModel": "Samsung Galaxy S21",      // Modèle du device (optionnel)
  "osVersion": "Android 12"                 // Version OS (optionnel)
}
```

#### ✅ Réponse Succès

```json
{
  "success": true,
  "message": "Authentification réussie",
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "tokenType": "Bearer",
  "expiresIn": 864000,
  "utilisateur": {
    "idUtilisateur": 123,
    "nomUtilisateur": "Doe",
    "email": "user@example.com",
    "defaultUsername": "DOE_JOHN_2025",
    ...
  }
}
```

### 📲 2. Obtenir le FCM Token

#### Flutter (firebase_messaging)

```dart
import 'package:firebase_messaging/firebase_messaging.dart';

// Obtenir le token FCM
Future<String?> getFcmToken() async {
  FirebaseMessaging messaging = FirebaseMessaging.instance;
  
  // Demander la permission (iOS)
  await messaging.requestPermission();
  
  // Obtenir le token
  String? token = await messaging.getToken();
  print('FCM Token: $token');
  
  return token;
}

// Écouter les nouveaux tokens (rafraîchissement)
FirebaseMessaging.instance.onTokenRefresh.listen((newToken) {
  print('Nouveau token FCM: $newToken');
  // Envoyer le nouveau token au backend
  updateFcmToken(newToken);
});

// Écouter les messages en avant-plan
FirebaseMessaging.onMessage.listen((RemoteMessage message) {
  print('Message reçu en avant-plan: ${message.notification?.title}');
  
  // Afficher une notification locale
  showLocalNotification(
    title: message.notification?.title ?? 'Notification',
    body: message.notification?.body ?? '',
  );
});

// Écouter les clics sur notifications
FirebaseMessaging.onMessageOpenedApp.listen((RemoteMessage message) {
  print('Notification cliquée: ${message.data}');
  
  // Navigation vers l'écran approprié
  navigateToScreen(message.data);
});
```

#### React Native (react-native-firebase)

```javascript
import messaging from '@react-native-firebase/messaging';

// Obtenir le token FCM
async function getFcmToken() {
  try {
    // Demander la permission
    await messaging().requestPermission();
    
    // Obtenir le token
    const token = await messaging().getToken();
    console.log('FCM Token:', token);
    
    return token;
  } catch (error) {
    console.error('Erreur FCM:', error);
  }
}

// Écouter les nouveaux tokens
messaging().onTokenRefresh(token => {
  console.log('Nouveau token FCM:', token);
  updateFcmToken(token);
});

// Écouter les messages en avant-plan
messaging().onMessage(async remoteMessage => {
  console.log('Message reçu:', remoteMessage);
  
  // Afficher une notification locale
  showLocalNotification(remoteMessage);
});

// Écouter les clics sur notifications
messaging().onNotificationOpenedApp(remoteMessage => {
  console.log('Notification cliquée:', remoteMessage.data);
  navigateToScreen(remoteMessage.data);
});
```

### 🔄 3. Envoyer le Token lors de la Connexion

```dart
// Flutter - Exemple d'authentification
Future<void> login(String email, String password) async {
  // 1. Obtenir le token FCM
  String? fcmToken = await getFcmToken();
  
  // 2. Obtenir les informations du device
  DeviceInfoPlugin deviceInfo = DeviceInfoPlugin();
  String deviceType = Platform.isAndroid ? 'Android' : 'iOS';
  String deviceModel = '';
  String osVersion = '';
  
  if (Platform.isAndroid) {
    AndroidDeviceInfo androidInfo = await deviceInfo.androidInfo;
    deviceModel = '${androidInfo.manufacturer} ${androidInfo.model}';
    osVersion = 'Android ${androidInfo.version.release}';
  } else if (Platform.isIOS) {
    IosDeviceInfo iosInfo = await deviceInfo.iosInfo;
    deviceModel = iosInfo.model;
    osVersion = 'iOS ${iosInfo.systemVersion}';
  }
  
  // 3. Appeler l'API d'authentification
  final response = await http.post(
    Uri.parse('https://your-api.com/api/Utilisateur/authentifier'),
    headers: {'Content-Type': 'application/json'},
    body: jsonEncode({
      'emailOuTelephone': email,
      'motDePasse': password,
      'fcmToken': fcmToken,
      'deviceType': deviceType,
      'deviceModel': deviceModel,
      'osVersion': osVersion,
    }),
  );
  
  if (response.statusCode == 200) {
    final data = jsonDecode(response.body);
    // Sauvegarder le token JWT
    await saveAccessToken(data['accessToken']);
    print('✅ Authentification réussie');
  }
}
```

### 📨 4. Types de Notifications Reçues

Le backend envoie automatiquement des notifications Firebase pour :

#### 🎓 **Inscriptions**
- **Trigger** : Élève inscrit dans une classe
- **Destinataire** : Tuteur de l'élève
- **Contenu** :
  ```json
  {
    "notification": {
      "title": "🎓 Inscription de Jean Doe",
      "body": "Jean Doe a été inscrit dans la classe 6ème A. Nom d'utilisateur: DOE_JEAN_2025, Mot de passe: Kelasi2025!"
    },
    "data": {
      "type": "INSCRIPTION",
      "eleveId": "123",
      "classeId": "45",
      "userId": "67"
    }
  }
  ```

#### 💵 **Paiements**
- **Trigger** : Paiement de frais enregistré
- **Destinataire** : Tuteur de l'élève
- **Contenu** :
  ```json
  {
    "notification": {
      "title": "💵 Paiement de frais",
      "body": "Paiement de 50000 CDF pour Jean Doe (Minerval) - Merci !"
    },
    "data": {
      "type": "PAIEMENT",
      "paiementId": "89",
      "eleveId": "123",
      "montant": "50000",
      "typeFrais": "Minerval"
    }
  }
  ```

#### ✅ **Présences**
- **Trigger** : Pointage de présence enregistré
- **Destinataire** : Tuteur de l'élève
- **Contenu** :
  ```json
  {
    "notification": {
      "title": "✓ Présence de Jean Doe",
      "body": "Jean Doe est PRÉSENT le 01/11/2025 à 08:30"
    },
    "data": {
      "type": "PRESENCE",
      "presenceId": "234",
      "eleveId": "123",
      "isPresent": "true",
      "datePresence": "2025-11-01T08:30:00Z"
    }
  }
  ```

---

## 🌐 PARTIE 2 : FRONTEND WEB (Vue.js/React/Angular)

### 🔌 1. Installation de SignalR

#### Vue.js / React / Angular

```bash
npm install @microsoft/signalr
```

### 🔐 2. Authentification

Même endpoint que mobile, mais **sans** FCM Token :

```javascript
// JavaScript/TypeScript
async function login(email, password) {
  const response = await fetch('https://localhost:7105/api/Utilisateur/authentifier', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      emailOuTelephone: email,
      motDePasse: password,
      // Pas de fcmToken pour le web
    })
  });
  
  const data = await response.json();
  
  if (data.success) {
    // Sauvegarder le token JWT
    localStorage.setItem('accessToken', data.accessToken);
    
    // Connecter SignalR
    await connectSignalR(data.accessToken);
  }
}
```

### 🔔 3. Connexion SignalR

#### Vue.js (Composition API)

```typescript
import { ref } from 'vue';
import * as signalR from '@microsoft/signalr';

const connection = ref<signalR.HubConnection | null>(null);
const isConnected = ref(false);

async function connectSignalR(accessToken: string) {
  try {
    // Créer la connexion
    connection.value = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7105/hubs/notifications', {
        accessTokenFactory: () => accessToken, // 🔑 Authentification JWT
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect() // Reconnexion automatique
      .configureLogging(signalR.LogLevel.Information)
      .build();
    
    // Écouter les événements
    setupSignalRListeners();
    
    // Démarrer la connexion
    await connection.value.start();
    isConnected.value = true;
    console.log('✅ SignalR connecté');
  } catch (error) {
    console.error('❌ Erreur connexion SignalR:', error);
  }
}

function setupSignalRListeners() {
  if (!connection.value) return;
  
  // Écouter les notifications générales
  connection.value.on('ReceiveNotification', (notification) => {
    console.log('📩 Notification reçue:', notification);
    
    // Afficher une notification toast
    showToast(notification.titre, notification.message, notification.type);
  });
  
  // Écouter les notifications de paiement
  connection.value.on('ReceivePaiementNotification', (notification) => {
    console.log('💵 Paiement:', notification);
    
    // Mettre à jour l'interface
    updatePaiementsList();
    showToast('Paiement', notification.message, 'PAIEMENT');
  });
  
  // Écouter les notifications de présence
  connection.value.on('ReceivePresenceNotification', (notification) => {
    console.log('✓ Présence:', notification);
    
    updatePresencesList();
    showToast('Présence', notification.message, 'PRESENCE');
  });
  
  // Écouter les notifications d'inscription
  connection.value.on('ReceiveInscriptionNotification', (notification) => {
    console.log('🎓 Inscription:', notification);
    
    updateInscriptionsList();
    showToast('Inscription', notification.message, 'INSCRIPTION');
  });
  
  // Écouter les changements de statut
  connection.value.on('ReceiveStatusChange', (notification) => {
    console.log('🔄 Changement statut:', notification);
    
    if (notification.statut === false) {
      // L'école a été désactivée
      showWarning('Votre école a été désactivée. Veuillez contacter l\'administrateur.');
      logout();
    }
  });
  
  // Reconnexion
  connection.value.onreconnecting((error) => {
    console.warn('⚠️ Reconnexion SignalR...', error);
    isConnected.value = false;
  });
  
  connection.value.onreconnected((connectionId) => {
    console.log('✅ SignalR reconnecté:', connectionId);
    isConnected.value = true;
  });
  
  // Déconnexion
  connection.value.onclose((error) => {
    console.error('❌ SignalR déconnecté:', error);
    isConnected.value = false;
  });
}

// Déconnexion
async function disconnectSignalR() {
  if (connection.value) {
    await connection.value.stop();
    isConnected.value = false;
    console.log('🔌 SignalR déconnecté');
  }
}
```

#### React (avec hooks)

```typescript
import { useEffect, useState } from 'react';
import * as signalR from '@microsoft/signalr';

export function useSignalR(accessToken: string | null) {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
  const [isConnected, setIsConnected] = useState(false);
  
  useEffect(() => {
    if (!accessToken) return;
    
    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7105/hubs/notifications', {
        accessTokenFactory: () => accessToken,
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect()
      .build();
    
    setConnection(newConnection);
    
    return () => {
      newConnection.stop();
    };
  }, [accessToken]);
  
  useEffect(() => {
    if (!connection) return;
    
    connection.on('ReceiveNotification', (notification) => {
      console.log('📩 Notification:', notification);
      // Votre logique ici
    });
    
    connection.start()
      .then(() => {
        console.log('✅ SignalR connecté');
        setIsConnected(true);
      })
      .catch(err => console.error('❌ Erreur SignalR:', err));
    
    return () => {
      connection.off('ReceiveNotification');
    };
  }, [connection]);
  
  return { connection, isConnected };
}
```

### 📨 4. Afficher les Notifications (Toast)

```typescript
function showToast(titre: string, message: string, type: string) {
  // Utiliser une bibliothèque de toast (ex: vue-toastification)
  const icon = type === 'PAIEMENT' ? '💵' : 
               type === 'PRESENCE' ? '✓' : 
               type === 'INSCRIPTION' ? '🎓' : '🔔';
  
  toast.success(`${icon} ${titre}\n${message}`, {
    timeout: 5000,
    position: 'top-right',
    closeButton: true
  });
  
  // Ou utiliser l'API Notification native du navigateur
  if ('Notification' in window && Notification.permission === 'granted') {
    new Notification(titre, {
      body: message,
      icon: '/logo.png',
      tag: type
    });
  }
}
```

---

## 🧪 TESTS

### 📱 Test Mobile (Flutter/React Native)

1. **Lancer l'application mobile**
2. **Se connecter** avec un compte tuteur
3. **Vérifier les logs** :
   ```
   ✅ Token FCM enregistré pour l'utilisateur 123 - Device: Android Samsung Galaxy S21
   ```
4. **Déclencher une action** (inscription, paiement, présence) depuis le backend
5. **Vérifier la réception** de la notification Firebase

### 🌐 Test Web (Vue.js/React)

1. **Ouvrir** `test-signalr-notifications.html` dans un navigateur
2. **Se connecter** (bouton "Connecter")
3. **Vérifier le statut** : `✅ Connecté au hub SignalR`
4. **Cliquer sur les boutons** de test :
   - Test Paiement
   - Test Présence
   - Test Inscription
5. **Vérifier la réception** des notifications en temps réel

---

## 📊 RÉSUMÉ DES ENDPOINTS

| Type | Méthode | Endpoint | Description |
|------|---------|----------|-------------|
| **Auth** | POST | `/api/Utilisateur/authentifier` | Authentification + enregistrement FCM token |
| **SignalR** | WebSocket | `/hubs/notifications` | Hub temps réel pour notifications web |
| **Test** | POST | `/api/TestSignalR/broadcast` | Test notification broadcast |
| **Test** | POST | `/api/TestSignalR/user/{userId}` | Test notification utilisateur |
| **Test** | POST | `/api/TestSignalR/group/{groupName}` | Test notification groupe |

---

## ✅ CHECKLIST D'INTÉGRATION

### Mobile (Flutter/React Native)
- [ ] Firebase configuré (`google-services.json` / `GoogleService-Info.plist`)
- [ ] Package `firebase_messaging` / `@react-native-firebase/messaging` installé
- [ ] Permissions notifications demandées
- [ ] FCM Token obtenu
- [ ] FCM Token envoyé lors de l'authentification
- [ ] Listeners configurés (`onMessage`, `onMessageOpenedApp`)
- [ ] Notifications locales configurées

### Web (Vue.js/React/Angular)
- [ ] Package `@microsoft/signalr` installé
- [ ] Connexion SignalR établie après authentification
- [ ] JWT Token passé dans `accessTokenFactory`
- [ ] Listeners SignalR configurés (`ReceiveNotification`, etc.)
- [ ] Reconnexion automatique activée
- [ ] Toast/Notifications affichées
- [ ] Gestion de la déconnexion (logout)

---

## 🔒 SÉCURITÉ

### Mobile
- ✅ **FCM Token** stocké côté serveur de manière sécurisée
- ✅ **Authentification JWT** requise pour tous les appels API
- ✅ **Token expiré** → Déconnexion automatique

### Web
- ✅ **SignalR** protégé par **[Authorize]** → JWT requis
- ✅ **Reconnexion automatique** avec le même token
- ✅ **Déconnexion côté serveur** → `OnDisconnectedAsync` nettoyage

---

## 📞 SUPPORT

- **Documentation** : Ce fichier
- **Tests** : `test-signalr-notifications.html`, `test-signalr-real-actions.ps1`
- **Logs** : Vérifier les logs de l'API (`dotnet run`)

---

**✅ GUIDE COMPLET - Prêt pour l'intégration !**

