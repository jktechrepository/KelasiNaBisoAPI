# Documentation Frontend — Notifications (Push, In-App, SignalR)

Guide d’intégration **Vue 3** (web) et **Flutter** (mobile parent / élève / enseignant).

**Base URL dev :** `https://localhost:7102` ou `https://dev-knb.asdc-rdc.org`  
**Swagger :** `/swagger/index.html`  
**Auth :** `Authorization: Bearer {jwt_token}` (`sub` = `IdUtilisateur`)

Références backend : [`DOCUMENTATION_ETAT_NOTIFICATIONS_PUSH.md`](DOCUMENTATION_ETAT_NOTIFICATIONS_PUSH.md).  
Ne pas suivre [`docs/archive/md/GUIDE_INTEGRATION_FRONTEND_NOTIFICATIONS.md`](docs/archive/md/GUIDE_INTEGRATION_FRONTEND_NOTIFICATIONS.md) (port et hub devoirs incorrects).

---

## Table des matières

1. [Les trois canaux](#1-les-trois-canaux)
2. [Matrice des processus](#2-matrice-des-processus)
3. [Enregistrement FCM](#3-enregistrement-fcm)
4. [Centre In-App (REST)](#4-centre-in-app-rest)
5. [SignalR](#5-signalr)
6. [Payloads FCM `data`](#6-payloads-fcm-data)
7. [Navigation UI](#7-navigation-ui)
8. [Vue 3](#8-vue-3)
9. [Flutter](#9-flutter)
10. [Checklist](#10-checklist)

---

## 1. Les trois canaux

| Canal | Quand | Contrat |
|-------|--------|---------|
| **Push FCM** | App tuée ou en arrière-plan | `notification.title` / `body` + map `data` (strings) |
| **In-App** | Centre de notifs, badge, historique | Table `Notifications` via REST |
| **SignalR** | App ouverte, JWT sur le hub | Groupe auto `user_{idUtilisateur}` |

Un même événement peut utiliser **plusieurs** canaux. L’app doit : enregistrer le token FCM, charger le centre au login, et se connecter à SignalR tant qu’elle est au premier plan.

---

## 2. Matrice des processus

| Événement | Push FCM | In-App REST | SignalR |
|-----------|----------|-------------|---------|
| Pointage **élève** | Oui (tuteur + token) | Oui | `/hubs/notifications` — payload **custom** `type: PRESENCE_ELEVE` |
| Pointage **agent** | Non | Non | Non (dashboard présence seulement) |
| Paiement élève **confirmé** | Oui | Oui | `/hubs/notifications` — payload **custom** `type: PAIEMENT_ELEVE` |
| Paiement Moko en attente | Après confirmation | Après confirmation | Après confirmation |
| **Nouveau devoir** | Oui (parents) | **Non** | **Autre hub** `/hubs/devoirs-adomicile` (`NouveauDevoir` / `NouveauDevoirParent`) |
| **Communiqué** | Si canal Push (défaut oui) | Si canal InApp (défaut oui) | `/hubs/notifications` si InApp — payload **entité** `type: COMMUNICATION` |

Destinataire parent : **compte `Utilisateur` lié au tuteur**, pas `IdTuteur` dans l’URL des notifs.

---

## 3. Enregistrement FCM

### Login

`POST /api/Utilisateur/authentifier`

```json
{
  "emailOuTelephone": "parent@ecole.cd",
  "motDePasse": "…",
  "fcmToken": "dXyz…",
  "deviceType": "Android",
  "deviceModel": "Pixel 8",
  "osVersion": "14",
  "appVersion": "1.2.3"
}
```

`deviceType` : `Android` | `iOS` | `Web`. Ignorer les placeholders `"string"` / `"null"`.

### Refresh token Firebase

`POST /api/UserDevice/register` (JWT)

```json
{
  "idUtilisateur": 42,
  "fcmToken": "nouveau-token",
  "deviceType": "Android",
  "appVersion": "1.2.3"
}
```

À appeler sur `onTokenRefresh` **et** après login si le token a changé.

### Logout

`POST /api/Utilisateur/deconnecter` — désactive le device. Sans ça, le compte continue de recevoir les push.

Sans token actif, l’API logue « Aucun token FCM actif » : ce n’est pas une panne Firebase.

---

## 4. Centre In-App (REST)

`idDestinataire` = **`IdUtilisateur` du JWT**, pas l’id élève.

| Méthode | Route | Permission |
|---------|-------|------------|
| GET | `/api/Notification/destinataire/{id}` | `Notification.ReadOwn` |
| GET | `/api/Notification/destinataire/{id}/non-lues` | `Notification.ReadOwn` |
| PUT | `/api/Notification/{id}/marquer-lue` | `Notification.UpdateOwn` |
| PUT | `/api/Notification/destinataire/{id}/marquer-toutes-lues` | `Notification.UpdateOwn` |

Autre destinataire → **403**.

Champs utiles : `idNotification`, `titre`, `contenu`, `typeNotification`, `estLue`, `dateCreation`, `idCampaign`, `idEleve`, `priorite`.

**Présents dans le centre :** présence, paiement, communiqué (et convocations / campagnes).  
**Absents :** devoirs (utiliser FCM + hub devoirs, puis `GET /api/DevoirADomicile/...`).

Au login : charger `non-lues` pour le badge. Au tap SignalR custom (sans `id`) : recharger la liste.

---

## 5. SignalR

### Hub centre de notifs (obligatoire parent)

```
wss://{host}/hubs/notifications?access_token={jwt}
```

Connexion JWT. Le serveur ajoute le client à `user_{id}` (claim `NameIdentifier` = `sub` = `IdUtilisateur`).

Événement : **`ReceiveNotification`**. **Deux formes** — tester `id` / `isCustom` :

**A — Custom (présence, paiement)** — pas d’`id` fiche :

```json
{
  "title": "Pointage de Présences",
  "message": "L'élève … a bien pointé…",
  "type": "PRESENCE_ELEVE",
  "timestamp": "2026-10-06T14:00:00Z",
  "isCustom": true
}
```

Paiement : `"type": "PAIEMENT_ELEVE"`.

**B — Entité (communiqué)** — a un `id` :

```json
{
  "id": 1842,
  "title": "Réunion parents",
  "message": "…",
  "type": "COMMUNICATION",
  "isRead": false,
  "dateCreation": "2026-10-06T14:00:00Z",
  "metadata": { "ecoleId": 1, "eleveId": null }
}
```

Clic B : `GET /api/Notification/{id}` puis `PUT .../marquer-lue`.  
Clic A : naviguer via `type` + recharger `destinataire/{id}/non-lues`.

Reconnect auto recommandé. User hors ligne : pas d’erreur campagne ; le GET rattrape.

### Hub devoirs (optionnel parent / élève)

```
/hubs/devoirs-adomicile
```

Événements : `NouveauDevoir`, `NouveauDevoirParent` (pas `ReceiveNotification`).  
Groupes : `user_{id}`, `classe_{idClasse}`, `parents_classe_{idClasse}`.

### Hors scope centre parent

`/hubs/dashboard` — écrans admin (présence / paiement agrégés).

---

## 6. Payloads FCM `data`

Toutes les valeurs sont des **strings**.

**Présence** (`type` = `PRESENCE_ELEVE`) : `idPresence`, `idEleve`, `nomEleve`, `isPresent`, `datePointage`, `heureArrivee`, `nomEcole`, `nomClasse`, `section`.

**Paiement** (`type` = `PAIEMENT_ELEVE`) : ids paiement / élève, montant, type de frais (voir DTO builder).

**Devoir** (clés PascalCase) :

```
Type=DevoirADomicile
IdDevoir=…
IdClasse=…
Titre=…
DateLimite=yyyy-MM-ddTHH:mm:ss
```

**Communiqué :** `campaignId`, `importance`.

Navigation FCM : même table que §7. iOS : demander la permission avant `getToken()`.

---

## 7. Navigation UI

| `type` / FCM | Écran |
|--------------|--------|
| `PRESENCE_ELEVE` | Présence de l’enfant (`idEleve`) |
| `PAIEMENT_ELEVE` | Frais / historique paiements |
| `COMMUNICATION` | Détail notif `id` ou campagne `campaignId` |
| `DevoirADomicile` | `GET /api/DevoirADomicile/{IdDevoir}` (liste filtrable : [`DOCUMENTATION_FRONTEND_FILTRE_DATE_DEVOIRS.md`](DOCUMENTATION_FRONTEND_FILTRE_DATE_DEVOIRS.md)) |

---

## 8. Vue 3

```ts
import * as signalR from '@microsoft/signalr'

export function connectNotificationHub(baseUrl: string, getToken: () => string) {
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${baseUrl}/hubs/notifications`, { accessTokenFactory: getToken })
    .withAutomaticReconnect()
    .build()

  connection.on('ReceiveNotification', (n: Record<string, unknown>) => {
    if (typeof n.id === 'number') {
      // Forme B — communiqué : badge + éventuellement drawer
      return
    }
    const type = String(n.type ?? '')
    // Forme A — PRESENCE_ELEVE | PAIEMENT_ELEVE : toast + GET non-lues
  })

  return connection.start().then(() => connection)
}
```

Hub devoirs (parent) :

```ts
.withUrl(`${baseUrl}/hubs/devoirs-adomicile`, { accessTokenFactory: getToken })
connection.on('NouveauDevoirParent', (payload) => { /* ouvrir devoir */ })
```

Refresh FCM (web) : `getToken` Firebase + `POST /api/UserDevice/register`.

---

## 9. Flutter

**FCM**

```dart
final messaging = FirebaseMessaging.instance;
await messaging.requestPermission();
final token = await messaging.getToken();
// POST authentifier ou /api/UserDevice/register

FirebaseMessaging.instance.onTokenRefresh.listen((t) { /* register */ });

FirebaseMessaging.onMessage.listen((m) {
  final data = m.data; // Type, type, IdDevoir, campaignId…
});
FirebaseMessaging.onMessageOpenedApp.listen((m) { /* navigation §7 */ });
```

**SignalR** (`signalr_netcore` ou équivalent)

```dart
final hub = HubConnectionBuilder()
    .withUrl('$baseUrl/hubs/notifications',
        options: HttpConnectionOptions(accessTokenFactory: () async => jwt))
    .withAutomaticReconnect()
    .build();
hub.on('ReceiveNotification', (args) {
  final n = args?.first as Map?;
  final hasId = n?['id'] != null;
  // hasId → COMMUNICATION ; sinon type PRESENCE_ELEVE / PAIEMENT_ELEVE
});
await hub.start();
```

Seconde connexion pour les devoirs : `/hubs/devoirs-adomicile`, `NouveauDevoirParent`.

---

## 10. Checklist

- [ ] Login envoie `fcmToken` + `deviceType`
- [ ] `onTokenRefresh` → `POST /api/UserDevice/register`
- [ ] Logout → `deconnecter`
- [ ] GET `destinataire/{idUtilisateur}/non-lues` au démarrage
- [ ] Hub `/hubs/notifications` + handler **deux formes** `ReceiveNotification`
- [ ] Parent : hub `/hubs/devoirs-adomicile` pour les devoirs
- [ ] FCM `data` : `type` **et** `Type` (devoir)
- [ ] Tap communiqué : `id` → marquer lue
- [ ] Tap présence / paiement custom : navigation + refresh liste
- [ ] Ne pas attendre une fiche In-App pour un devoir

**Swagger :** tag `Notification`, `UserDevice`, `NotificationPush` (admin). Hubs : `/hubs/notifications`, `/hubs/devoirs-adomicile`.
