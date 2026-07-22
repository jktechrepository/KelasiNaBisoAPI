# 📚 Documentation Complète - Module de Communication

## 🌟 Vue d'ensemble

Le module de communication de KelasiNaBiso offre un système complet de communication multi-canal permettant :
- **Messagerie instantanée** entre utilisateurs (messages privés et groupes)
- **Campagnes de communication** pour envoyer des notifications en masse
- **Notifications en temps réel** via SignalR
- **Multi-canal** : Push Firebase, Email, SMS, In-App

### 🔗 Informations de base
- **URL de base**: `https://localhost:7102` (HTTPS) ou `http://localhost:5002` (HTTP)
- **Format des données**: JSON
- **Authentification**: Token JWT (Bearer Token)
- **SignalR Hub**: `/hubs/notifications` et `/hubs/dashboard`

---

## 📋 Table des matières

1. [Architecture du module](#architecture-du-module)
2. [Messagerie (Messages privés et groupes)](#messagerie)
3. [Campagnes de communication](#campagnes-de-communication)
4. [SignalR - Communication en temps réel](#signalr)
5. [Endpoints API](#endpoints-api)
6. [Exemples d'utilisation frontend](#exemples-dutilisation-frontend)
7. [Cas d'usage typiques](#cas-dusage-typiques)
8. [Gestion des erreurs](#gestion-des-erreurs)

---

## 🏗️ Architecture du module

### Composants principaux

```
┌─────────────────────────────────────────────────────────────┐
│                    MODULE DE COMMUNICATION                   │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────────────┐      ┌──────────────────────────┐     │
│  │   MESSAGERIE     │      │  CAMPAGNES COMMUNICATION │     │
│  ├──────────────────┤      ├──────────────────────────┤     │
│  │ • Message        │      │ • CommunicationCampaign  │     │
│  │ • GroupeMessage  │      │ • CommunicationSegment  │     │
│  │ • Notifications  │      │ • Multi-canal           │     │
│  │   Push/SMS       │      │ • Planification         │     │
│  └──────────────────┘      └──────────────────────────┘     │
│                                                               │
│  ┌──────────────────────────────────────────────────────┐   │
│  │          SIGNALR - TEMPS RÉEL                        │   │
│  ├──────────────────────────────────────────────────────┤   │
│  │ • NotificationHub (notifications)                    │   │
│  │ • DashboardHub (mises à jour dashboard)              │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

### Flux de communication

```
Utilisateur A                    Serveur                    Utilisateur B
     │                              │                              │
     │─── Envoie Message ───────────>│                              │
     │                              │─── Notification Push ───────>│
     │                              │─── SMS Fallback (si échec) ─>│
     │                              │─── SignalR (temps réel) ────>│
     │                              │                              │
```

---

## 💬 Messagerie

### Vue d'ensemble

Le système de messagerie permet :
- **Messages privés** entre deux utilisateurs
- **Messages de groupe** dans des groupes de discussion
- **Notifications automatiques** (Push Firebase + SMS fallback)
- **Historique complet** des conversations

### Modèles de données

#### Message

```typescript
interface Message {
  idMessage: number;
  idExpediteur?: number;        // ID de l'expéditeur
  idDestinateur?: number;      // ID du destinataire (pour message privé)
  idGroupe?: number;           // ID du groupe (pour message de groupe)
  contenuMessage: string;      // Contenu du message (max 1000 caractères)
  fichierUrl?: string;          // URL du fichier joint (max 500 caractères)
  statut?: boolean;            // Statut actif/inactif (soft delete)
  dateEnvoi: string;            // Date d'envoi (ISO 8601)
  
  // Navigation (optionnel dans les réponses)
  expediteur?: Utilisateur;
  destinateur?: Utilisateur;
  groupeMessage?: GroupeMessage;
}
```

#### GroupeMessage

```typescript
interface GroupeMessage {
  idGroupe: number;
  nomGroupe: string;            // Nom du groupe (max 100 caractères)
  creePar?: number;            // ID du créateur
  idEcole: number;             // ID de l'école
  statut?: boolean;            // Statut actif/inactif
  dateCreation: string;        // Date de création (ISO 8601)
  
  // Navigation (optionnel dans les réponses)
  utilisateur?: Utilisateur;
  ecole?: Ecole;
  messages?: Message[];         // Collection des messages du groupe
}
```

### Notifications automatiques

Lorsqu'un message privé est envoyé, le système envoie automatiquement :
1. **Notification Push Firebase** (prioritaire)
2. **SMS Fallback** (si la notification push échoue)

**Contenu de la notification** :
- **Titre** : `💬 Message de [Nom Expéditeur]`
- **Corps** : Contenu du message (tronqué à 200 caractères)
- **Données additionnelles** :
  ```json
  {
    "type": "MESSAGE_INDIVIDUEL",
    "idMessage": 123,
    "idExpediteur": 45,
    "idDestinataire": 67,
    "dateEnvoi": "2025-12-04 19:33:38"
  }
  ```

---

## 📢 Campagnes de communication

### Vue d'ensemble

Les campagnes de communication permettent d'envoyer des messages en masse à des groupes ciblés d'utilisateurs via plusieurs canaux simultanément.

### Fonctionnalités

- ✅ **Multi-canal** : Push, Email, SMS, In-App
- ✅ **Segmentation** : Cibler des groupes spécifiques (classes, parents, enseignants, etc.)
- ✅ **Planification** : Envoyer immédiatement ou planifier pour plus tard
- ✅ **Rappels automatiques** : Programmer des rappels
- ✅ **Validation** : Workflow d'approbation pour les campagnes importantes
- ✅ **Suivi** : Historique complet et statistiques d'envoi

### Modèles de données

#### CommunicationCampaign

```typescript
interface CommunicationCampaign {
  idCampaign: number;
  idEcole: number;
  idAuteur: number;
  titre: string;                    // Titre de la campagne (max 200 caractères)
  contenuMarkdown: string;           // Contenu en Markdown
  statut: string;                    // "Brouillon", "En attente", "Envoyée", "Annulée"
  importance: string;                // "Info", "Important", "Urgent"
  channelsJson: string;              // JSON des canaux activés
  dateCreation: string;
  planifiedAt?: string;             // Date de planification
  expirationAt?: string;             // Date d'expiration
  rappelAuto: boolean;              // Activer les rappels automatiques
  validationAt?: string;            // Date de validation
  validatedBy?: number;             // ID du validateur
}
```

#### CommunicationChannelsDto

```typescript
interface CommunicationChannelsDto {
  push: boolean;      // Notifications push Firebase
  email: boolean;     // Emails SMTP
  sms: boolean;       // SMS Twilio
  inApp: boolean;     // Notifications in-app
}
```

#### CommunicationSegmentDto

```typescript
interface CommunicationSegmentDto {
  idSegment: number;
  idCampaign: number;
  typeSegment: string;        // "Classe", "Parent", "Enseignant", "Tous"
  criteresJson: string;       // Critères de segmentation (JSON)
}
```

### Statuts de campagne

| Statut | Description |
|--------|------------|
| `Brouillon` | Campagne en cours de création |
| `En attente` | Campagne validée, en attente d'envoi |
| `Envoyée` | Campagne envoyée avec succès |
| `Annulée` | Campagne annulée |
| `Expirée` | Campagne expirée |

### Niveaux d'importance

| Importance | Description |
|------------|-------------|
| `Info` | Information générale |
| `Important` | Information importante |
| `Urgent` | Information urgente |

---

## 📡 SignalR - Communication en temps réel

### Vue d'ensemble

SignalR permet une communication bidirectionnelle en temps réel entre le serveur et les clients. Deux hubs sont disponibles :

1. **NotificationHub** (`/hubs/notifications`) : Notifications en temps réel
2. **DashboardHub** (`/hubs/dashboard`) : Mises à jour de dashboard

### Configuration

#### URL des Hubs

- **HTTP** : `http://localhost:5002/hubs/notifications`
- **HTTPS** : `https://localhost:7102/hubs/notifications`
- **WebSocket** : `ws://localhost:5002/hubs/notifications`
- **WebSocket Secure** : `wss://localhost:7102/hubs/notifications`

#### Groupes SignalR

Les utilisateurs sont automatiquement ajoutés aux groupes suivants :

- `user_{userId}` : Groupe personnel de l'utilisateur
- `all_users` : Groupe général (tous les utilisateurs)
- `ecole_{ecoleId}` : Groupe de l'école (DashboardHub uniquement)

### Événements disponibles

#### NotificationHub

| Événement | Direction | Description |
|-----------|-----------|------------|
| `ReceiveNotification` | Serveur → Client | Notification reçue |
| `UserConnected` | Serveur → Client | Utilisateur connecté |
| `UserDisconnected` | Serveur → Client | Utilisateur déconnecté |

#### DashboardHub

| Événement | Direction | Description |
|-----------|-----------|------------|
| `DashboardUpdated` | Serveur → Client | Dashboard mis à jour |
| `StatsUpdated` | Serveur → Client | Statistiques mises à jour |

---

## 🔌 Endpoints API

### Messagerie

#### 1. GET `/api/Message`

Récupère tous les messages actifs.

**Rôles requis**: Tous les utilisateurs authentifiés

**Réponse 200**:
```json
[
  {
    "idMessage": 1,
    "idExpediteur": 45,
    "idDestinateur": 67,
    "idGroupe": null,
    "contenuMessage": "Bonjour, comment allez-vous ?",
    "fichierUrl": null,
    "statut": true,
    "dateEnvoi": "2025-12-04T19:33:38"
  }
]
```

---

#### 2. GET `/api/Message/{id}`

Récupère un message par son ID.

**Réponse 200**: Objet `Message`  
**Réponse 404**: Message introuvable

---

#### 3. GET `/api/Message/expediteur/{idExpediteur}`

Récupère tous les messages envoyés par un utilisateur.

**Exemple**:
```javascript
const messages = await api.get(`/api/Message/expediteur/45`);
```

---

#### 4. GET `/api/Message/groupe/{idGroupe}`

Récupère tous les messages d'un groupe.

**Exemple**:
```javascript
const messages = await api.get(`/api/Message/groupe/12`);
```

---

#### 5. POST `/api/Message`

Crée un nouveau message.

**Corps de la requête**:
```json
{
  "idExpediteur": 45,
  "idDestinateur": 67,
  "idGroupe": null,
  "contenuMessage": "Bonjour, comment allez-vous ?",
  "fichierUrl": null
}
```

**Réponse 201**: Message créé avec succès

**Note** : Une notification push/SMS est automatiquement envoyée au destinataire si `idDestinateur` est renseigné.

---

#### 6. PUT `/api/Message/{id}`

Met à jour un message.

**Corps de la requête**:
```json
{
  "idMessage": 1,
  "contenuMessage": "Message modifié",
  "fichierUrl": "https://example.com/file.pdf"
}
```

---

#### 7. DELETE `/api/Message/{id}`

Supprime un message (suppression physique).

---

#### 8. PUT `/api/Message/toggle-statut/{id}`

Active/désactive un message (soft delete).

**Réponse 200**:
```json
{
  "message": "Statut modifié avec succès",
  "nouveauStatut": false,
  "messageData": { ... }
}
```

---

### Groupes de messages

#### 1. GET `/api/GroupeMessage`

Récupère tous les groupes de messages.

---

#### 2. GET `/api/GroupeMessage/{id}`

Récupère un groupe par son ID.

---

#### 3. GET `/api/GroupeMessage/ecole/{idEcole}`

Récupère tous les groupes d'une école.

---

#### 4. GET `/api/GroupeMessage/nom/{nom}`

Récupère un groupe par son nom.

---

#### 5. POST `/api/GroupeMessage`

Crée un nouveau groupe.

**Corps de la requête**:
```json
{
  "nomGroupe": "Classe 5ème A",
  "creePar": 45,
  "idEcole": 1
}
```

---

#### 6. PUT `/api/GroupeMessage/{id}`

Met à jour un groupe.

**Corps de la requête**:
```json
{
  "idGroupe": 12,
  "nomGroupe": "Classe 5ème A - Modifié"
}
```

---

#### 7. DELETE `/api/GroupeMessage/{id}`

Supprime un groupe.

---

#### 8. PUT `/api/GroupeMessage/toggle-statut/{id}`

Active/désactive un groupe (soft delete).

---

### Campagnes de communication

#### 1. GET `/api/Communication`

Récupère la liste paginée des campagnes.

**Paramètres de requête**:
- `pageNumber` (int, défaut: 1)
- `pageSize` (int, défaut: 10)
- `searchTerm` (string, optionnel)
- `sortBy` (string, optionnel: "Titre", "Importance", "Statut")
- `sortDescending` (bool, défaut: false)

**Rôles requis**: `Super-Admin`, `Admin`, `Directeur`, `Sous-Directeur`

**Réponse 200**:
```json
{
  "items": [
    {
      "idCampaign": 1,
      "idEcole": 1,
      "nomEcole": "École Primaire",
      "titre": "Réunion parents-professeurs",
      "importance": "Important",
      "statut": "Envoyée",
      "dateCreation": "2025-12-04T19:33:38",
      "planifiedAt": null,
      "expirationAt": null,
      "rappelAuto": false,
      "auteur": "Jean Dupont",
      "validateur": "Marie Martin",
      "totalDestinataires": 150,
      "envoyes": 145,
      "echecs": 5
    }
  ],
  "total": 1,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

---

#### 2. GET `/api/Communication/{id}`

Récupère les détails d'une campagne.

**Réponse 200**: Objet `CommunicationCampaignDetailDto`  
**Réponse 404**: Campagne introuvable

---

#### 3. GET `/api/Communication/ecole/{ecoleId}`

Récupère les campagnes d'une école spécifique.

---

#### 4. POST `/api/Communication`

Crée une nouvelle campagne.

**Corps de la requête**:
```json
{
  "idEcole": 1,
  "titre": "Réunion parents-professeurs",
  "contenuMarkdown": "Bonjour,\n\nNous vous invitons à une réunion...",
  "importance": "Important",
  "canaux": {
    "push": true,
    "email": true,
    "sms": false,
    "inApp": true
  },
  "rappelAuto": true,
  "sendImmediately": false,
  "planifiedAt": "2025-12-10T10:00:00",
  "expirationAt": "2025-12-15T23:59:59",
  "segments": [
    {
      "typeSegment": "Classe",
      "criteresJson": "{\"idClasse\": 5}"
    }
  ]
}
```

**Réponse 201**: Campagne créée avec succès

---

#### 5. PUT `/api/Communication/{id}`

Met à jour une campagne.

**Corps de la requête**: `UpdateCommunicationCampaignDto`

---

#### 6. POST `/api/Communication/{id}/annuler`

Annule une campagne.

**Corps de la requête** (optionnel):
```json
{
  "raison": "Campagne annulée par erreur"
}
```

---

#### 7. POST `/api/Communication/{id}/destinataires/recharger`

Recharge la liste des destinataires basée sur les segments.

**Réponse 200**:
```json
{
  "destinataires": 150
}
```

---

#### 8. GET `/api/Communication/{id}/destinataires`

Récupère la liste paginée des destinataires d'une campagne.

**Paramètres de requête**: `PagedRequest`

**Réponse 200**:
```json
{
  "items": [
    {
      "idRecipient": 1,
      "idCampaign": 1,
      "idUtilisateur": 45,
      "nomUtilisateur": "Jean Dupont",
      "email": "jean@example.com",
      "telephone": "+243900000000",
      "status": "Envoye",
      "dateEnvoi": "2025-12-04T19:33:38",
      "canaux": {
        "push": true,
        "email": true,
        "sms": false,
        "inApp": true
      }
    }
  ],
  "total": 150,
  "pageNumber": 1,
  "pageSize": 10
}
```

---

#### 9. GET `/api/Communication/{id}/historique`

Récupère l'historique d'une campagne.

**Paramètres de requête**: `PagedRequest`

**Réponse 200**:
```json
{
  "items": [
    {
      "idHistory": 1,
      "idCampaign": 1,
      "action": "Créée",
      "auteur": "Jean Dupont",
      "dateAction": "2025-12-04T19:33:38",
      "details": "Campagne créée avec 150 destinataires"
    }
  ],
  "total": 5,
  "pageNumber": 1,
  "pageSize": 10
}
```

---

#### 10. POST `/api/Communication/{id}/envoyer`

Envoie une campagne (passe de "En attente" à "Envoyée").

**Réponse 202**: Campagne en cours d'envoi

**Note** : L'envoi est asynchrone. Utilisez l'endpoint `/historique` pour suivre le progrès.

---

## 💻 Exemples d'utilisation frontend

### Configuration Axios

```javascript
import axios from 'axios';

const api = axios.create({
  baseURL: process.env.VUE_APP_API_BASE_URL || 'https://localhost:7102',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Intercepteur pour ajouter le token JWT
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default api;
```

### Service de messagerie

```javascript
// services/message.service.js
import api from './api';

export const messageService = {
  // Récupérer tous les messages
  async getAll() {
    const response = await api.get('/api/Message');
    return response.data;
  },

  // Récupérer un message par ID
  async getById(id) {
    const response = await api.get(`/api/Message/${id}`);
    return response.data;
  },

  // Récupérer les messages d'un expéditeur
  async getByExpediteur(idExpediteur) {
    const response = await api.get(`/api/Message/expediteur/${idExpediteur}`);
    return response.data;
  },

  // Récupérer les messages d'un groupe
  async getByGroupe(idGroupe) {
    const response = await api.get(`/api/Message/groupe/${idGroupe}`);
    return response.data;
  },

  // Envoyer un message
  async sendMessage(message) {
    const response = await api.post('/api/Message', message);
    return response.data;
  },

  // Modifier un message
  async updateMessage(id, dto) {
    const response = await api.put(`/api/Message/${id}`, dto);
    return response.data;
  },

  // Supprimer un message
  async deleteMessage(id) {
    await api.delete(`/api/Message/${id}`);
  },

  // Toggle statut (soft delete)
  async toggleStatut(id) {
    const response = await api.put(`/api/Message/toggle-statut/${id}`);
    return response.data;
  },
};
```

### Service de campagnes

```javascript
// services/communication.service.js
import api from './api';

export const communicationService = {
  // Récupérer les campagnes
  async getCampaigns(params = {}) {
    const response = await api.get('/api/Communication', { params });
    return response.data;
  },

  // Récupérer une campagne par ID
  async getCampaign(id) {
    const response = await api.get(`/api/Communication/${id}`);
    return response.data;
  },

  // Créer une campagne
  async createCampaign(dto) {
    const response = await api.post('/api/Communication', dto);
    return response.data;
  },

  // Mettre à jour une campagne
  async updateCampaign(id, dto) {
    const response = await api.put(`/api/Communication/${id}`, dto);
    return response.data;
  },

  // Annuler une campagne
  async cancelCampaign(id, raison = null) {
    const response = await api.post(`/api/Communication/${id}/annuler`, {
      raison,
    });
    return response.data;
  },

  // Recharger les destinataires
  async refreshRecipients(id) {
    const response = await api.post(`/api/Communication/${id}/destinataires/recharger`);
    return response.data;
  },

  // Récupérer les destinataires
  async getRecipients(id, params = {}) {
    const response = await api.get(`/api/Communication/${id}/destinataires`, { params });
    return response.data;
  },

  // Récupérer l'historique
  async getHistory(id, params = {}) {
    const response = await api.get(`/api/Communication/${id}/historique`, { params });
    return response.data;
  },

  // Envoyer une campagne
  async dispatchCampaign(id) {
    const response = await api.post(`/api/Communication/${id}/envoyer`);
    return response.data;
  },
};
```

### Configuration SignalR

```javascript
// services/signalr.service.js
import * as signalR from '@microsoft/signalr';

class SignalRService {
  constructor() {
    this.connection = null;
    this.notificationHandlers = [];
  }

  // Se connecter au hub
  async connect(token) {
    const baseUrl = process.env.VUE_APP_API_BASE_URL || 'https://localhost:7102';
    
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${baseUrl}/hubs/notifications`, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .build();

    // Écouter les notifications
    this.connection.on('ReceiveNotification', (notification) => {
      this.notificationHandlers.forEach((handler) => handler(notification));
    });

    // Écouter les connexions/déconnexions
    this.connection.on('UserConnected', (data) => {
      console.log('Utilisateur connecté:', data);
    });

    this.connection.on('UserDisconnected', (data) => {
      console.log('Utilisateur déconnecté:', data);
    });

    // Gérer les erreurs
    this.connection.onclose((error) => {
      console.error('Connexion SignalR fermée:', error);
    });

    // Démarrer la connexion
    await this.connection.start();
    console.log('✅ Connecté à SignalR');
  }

  // S'abonner aux notifications
  onNotification(handler) {
    this.notificationHandlers.push(handler);
  }

  // Se déconnecter
  async disconnect() {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }
  }
}

export default new SignalRService();
```

### Composant Vue.js - Chat

```vue
<template>
  <div class="chat-container">
    <div class="messages" ref="messagesContainer">
      <div
        v-for="message in messages"
        :key="message.idMessage"
        :class="['message', { 'own': message.idExpediteur === currentUserId }]"
      >
        <div class="message-header">
          <strong>{{ getExpediteurName(message) }}</strong>
          <span class="date">{{ formatDate(message.dateEnvoi) }}</span>
        </div>
        <div class="message-content">{{ message.contenuMessage }}</div>
        <div v-if="message.fichierUrl" class="message-file">
          <a :href="message.fichierUrl" target="_blank">📎 Fichier joint</a>
        </div>
      </div>
    </div>

    <div class="message-input">
      <input
        v-model="newMessage"
        @keyup.enter="sendMessage"
        placeholder="Tapez votre message..."
      />
      <button @click="sendMessage" :disabled="!newMessage.trim()">
        Envoyer
      </button>
    </div>
  </div>
</template>

<script>
import { messageService } from '@/services/message.service';
import signalRService from '@/services/signalr.service';

export default {
  name: 'ChatComponent',
  props: {
    idDestinateur: {
      type: Number,
      required: true,
    },
    idGroupe: {
      type: Number,
      default: null,
    },
  },
  data() {
    return {
      messages: [],
      newMessage: '',
      currentUserId: null,
      loading: false,
    };
  },
  async mounted() {
    // Récupérer l'ID de l'utilisateur actuel
    this.currentUserId = this.$store.state.auth.userId;

    // Charger les messages existants
    await this.loadMessages();

    // Se connecter à SignalR
    const token = localStorage.getItem('accessToken');
    await signalRService.connect(token);

    // Écouter les nouveaux messages en temps réel
    signalRService.onNotification((notification) => {
      if (notification.type === 'MESSAGE_INDIVIDUEL') {
        this.loadMessages(); // Recharger les messages
      }
    });
  },
  beforeUnmount() {
    signalRService.disconnect();
  },
  methods: {
    async loadMessages() {
      this.loading = true;
      try {
        if (this.idGroupe) {
          this.messages = await messageService.getByGroupe(this.idGroupe);
        } else {
          // Pour les messages privés, on peut créer un endpoint de conversation
          // Pour l'instant, on récupère les messages de l'expéditeur
          this.messages = await messageService.getByExpediteur(this.currentUserId);
        }
        this.$nextTick(() => {
          this.scrollToBottom();
        });
      } catch (error) {
        console.error('Erreur lors du chargement des messages:', error);
        this.$toast.error('Impossible de charger les messages');
      } finally {
        this.loading = false;
      }
    },
    async sendMessage() {
      if (!this.newMessage.trim()) return;

      try {
        await messageService.sendMessage({
          idExpediteur: this.currentUserId,
          idDestinateur: this.idDestinateur,
          idGroupe: this.idGroupe,
          contenuMessage: this.newMessage,
        });

        this.newMessage = '';
        await this.loadMessages();
      } catch (error) {
        console.error('Erreur lors de l\'envoi:', error);
        this.$toast.error('Impossible d\'envoyer le message');
      }
    },
    getExpediteurName(message) {
      if (message.expediteur) {
        return `${message.expediteur.prenomUtilisateur} ${message.expediteur.nomUtilisateur}`;
      }
      return 'Utilisateur';
    },
    formatDate(dateString) {
      const date = new Date(dateString);
      return date.toLocaleString('fr-FR');
    },
    scrollToBottom() {
      const container = this.$refs.messagesContainer;
      if (container) {
        container.scrollTop = container.scrollHeight;
      }
    },
  },
};
</script>
```

### Composant Vue.js - Campagnes

```vue
<template>
  <div class="campaigns-container">
    <div class="campaigns-header">
      <h2>Campagnes de communication</h2>
      <button @click="showCreateModal = true">Créer une campagne</button>
    </div>

    <!-- Liste des campagnes -->
    <div class="campaigns-list">
      <div
        v-for="campaign in campaigns.items"
        :key="campaign.idCampaign"
        class="campaign-card"
      >
        <div class="campaign-header">
          <h3>{{ campaign.titre }}</h3>
          <span :class="['badge', campaign.statut.toLowerCase()]">
            {{ campaign.statut }}
          </span>
        </div>
        <div class="campaign-info">
          <p><strong>Importance:</strong> {{ campaign.importance }}</p>
          <p><strong>Destinataires:</strong> {{ campaign.totalDestinataires }}</p>
          <p><strong>Envoyés:</strong> {{ campaign.envoyes }}</p>
          <p><strong>Échecs:</strong> {{ campaign.echecs }}</p>
          <p><strong>Date:</strong> {{ formatDate(campaign.dateCreation) }}</p>
        </div>
        <div class="campaign-actions">
          <button @click="viewCampaign(campaign.idCampaign)">Voir détails</button>
          <button
            v-if="campaign.statut === 'En attente'"
            @click="dispatchCampaign(campaign.idCampaign)"
          >
            Envoyer
          </button>
          <button
            v-if="campaign.statut !== 'Envoyée' && campaign.statut !== 'Annulée'"
            @click="cancelCampaign(campaign.idCampaign)"
          >
            Annuler
          </button>
        </div>
      </div>
    </div>

    <!-- Pagination -->
    <div class="pagination">
      <button
        @click="loadCampaigns(campaigns.pageNumber - 1)"
        :disabled="campaigns.pageNumber === 1"
      >
        Précédent
      </button>
      <span>Page {{ campaigns.pageNumber }} sur {{ campaigns.totalPages }}</span>
      <button
        @click="loadCampaigns(campaigns.pageNumber + 1)"
        :disabled="campaigns.pageNumber === campaigns.totalPages"
      >
        Suivant
      </button>
    </div>

    <!-- Modal de création -->
    <CampaignCreateModal
      v-if="showCreateModal"
      @close="showCreateModal = false"
      @created="handleCampaignCreated"
    />
  </div>
</template>

<script>
import { communicationService } from '@/services/communication.service';

export default {
  name: 'CampaignsComponent',
  data() {
    return {
      campaigns: {
        items: [],
        total: 0,
        pageNumber: 1,
        pageSize: 10,
        totalPages: 0,
      },
      showCreateModal: false,
      loading: false,
    };
  },
  mounted() {
    this.loadCampaigns(1);
  },
  methods: {
    async loadCampaigns(page = 1) {
      this.loading = true;
      try {
        this.campaigns = await communicationService.getCampaigns({
          pageNumber: page,
          pageSize: 10,
        });
      } catch (error) {
        console.error('Erreur lors du chargement:', error);
        this.$toast.error('Impossible de charger les campagnes');
      } finally {
        this.loading = false;
      }
    },
    async viewCampaign(id) {
      this.$router.push(`/campaigns/${id}`);
    },
    async dispatchCampaign(id) {
      if (!confirm('Êtes-vous sûr de vouloir envoyer cette campagne ?')) {
        return;
      }

      try {
        await communicationService.dispatchCampaign(id);
        this.$toast.success('Campagne en cours d\'envoi');
        await this.loadCampaigns(this.campaigns.pageNumber);
      } catch (error) {
        console.error('Erreur lors de l\'envoi:', error);
        this.$toast.error('Impossible d\'envoyer la campagne');
      }
    },
    async cancelCampaign(id) {
      if (!confirm('Êtes-vous sûr de vouloir annuler cette campagne ?')) {
        return;
      }

      try {
        await communicationService.cancelCampaign(id);
        this.$toast.success('Campagne annulée');
        await this.loadCampaigns(this.campaigns.pageNumber);
      } catch (error) {
        console.error('Erreur lors de l\'annulation:', error);
        this.$toast.error('Impossible d\'annuler la campagne');
      }
    },
    handleCampaignCreated() {
      this.showCreateModal = false;
      this.loadCampaigns(this.campaigns.pageNumber);
    },
    formatDate(dateString) {
      const date = new Date(dateString);
      return date.toLocaleString('fr-FR');
    },
  },
};
</script>
```

---

## 🎯 Cas d'usage typiques

### Cas 1 : Envoyer un message privé

**Scénario**: Un parent veut contacter l'administration de l'école.

**Flux**:
1. Le parent ouvre la page de messagerie
2. Il sélectionne le destinataire (administration)
3. Il tape son message et clique sur "Envoyer"
4. Le message est sauvegardé en base de données
5. Une notification push est envoyée à l'administration
6. Si la push échoue, un SMS est envoyé en fallback
7. Le message apparaît en temps réel via SignalR

**Code**:
```javascript
// Envoyer le message
const message = await messageService.sendMessage({
  idExpediteur: currentUserId,
  idDestinateur: adminUserId,
  contenuMessage: 'Bonjour, je voudrais prendre rendez-vous.',
});

// Le système envoie automatiquement une notification
// Pas besoin de code supplémentaire côté frontend
```

---

### Cas 2 : Créer et envoyer une campagne

**Scénario**: Le directeur veut informer tous les parents d'une réunion.

**Flux**:
1. Le directeur crée une campagne avec :
   - Titre : "Réunion parents-professeurs"
   - Contenu : Message en Markdown
   - Canaux : Push + Email + In-App
   - Segments : Tous les parents
2. Il valide la campagne
3. Il clique sur "Envoyer"
4. Le système envoie la campagne à tous les destinataires via les canaux sélectionnés
5. Les statistiques sont mises à jour en temps réel

**Code**:
```javascript
// Créer la campagne
const campaign = await communicationService.createCampaign({
  idEcole: 1,
  titre: 'Réunion parents-professeurs',
  contenuMarkdown: 'Bonjour,\n\nNous vous invitons à une réunion...',
  importance: 'Important',
  canaux: {
    push: true,
    email: true,
    sms: false,
    inApp: true,
  },
  rappelAuto: true,
  sendImmediately: false,
  planifiedAt: '2025-12-10T10:00:00',
  segments: [
    {
      typeSegment: 'Parent',
      criteresJson: JSON.stringify({}),
    },
  ],
});

// Envoyer la campagne
await communicationService.dispatchCampaign(campaign.idCampaign);
```

---

### Cas 3 : Recevoir des notifications en temps réel

**Scénario**: Un utilisateur veut recevoir des notifications instantanées.

**Flux**:
1. L'utilisateur se connecte à l'application
2. Le frontend se connecte automatiquement à SignalR
3. Les notifications arrivent en temps réel sans rafraîchissement
4. L'utilisateur voit les notifications dans l'interface

**Code**:
```javascript
// Dans le composant principal (App.vue)
import signalRService from '@/services/signalr.service';

export default {
  async mounted() {
    const token = localStorage.getItem('accessToken');
    await signalRService.connect(token);

    // Écouter les notifications
    signalRService.onNotification((notification) => {
      this.$toast.info(notification.titre, {
        body: notification.message,
        onClick: () => {
          // Rediriger vers la page appropriée
          this.handleNotificationClick(notification);
        },
      });
    });
  },
  methods: {
    handleNotificationClick(notification) {
      switch (notification.type) {
        case 'MESSAGE_INDIVIDUEL':
          this.$router.push(`/messages/${notification.idMessage}`);
          break;
        case 'CAMPAGNE':
          this.$router.push(`/campaigns/${notification.idCampaign}`);
          break;
        default:
          break;
      }
    },
  },
};
```

---

## ⚠️ Gestion des erreurs

### Erreurs HTTP courantes

#### 401 Unauthorized
```javascript
// Token JWT invalide ou expiré
if (error.response?.status === 401) {
  localStorage.removeItem('accessToken');
  this.$router.push('/login');
}
```

#### 400 Bad Request
```javascript
// Erreurs de validation
if (error.response?.status === 400) {
  const errors = error.response.data.errors;
  Object.keys(errors).forEach((field) => {
    errors[field].forEach((message) => {
      this.$toast.error(`${field}: ${message}`);
    });
  });
}
```

#### 403 Forbidden
```javascript
// Permissions insuffisantes
if (error.response?.status === 403) {
  this.$toast.error('Vous n\'avez pas les permissions nécessaires');
}
```

#### 404 Not Found
```javascript
// Ressource introuvable
if (error.response?.status === 404) {
  this.$toast.error(error.response.data.message || 'Ressource introuvable');
}
```

#### 500 Internal Server Error
```javascript
// Erreur serveur
if (error.response?.status === 500) {
  this.$toast.error('Une erreur serveur est survenue. Veuillez réessayer plus tard.');
  console.error('Erreur serveur:', error.response.data);
}
```

### Gestion des erreurs SignalR

```javascript
// Dans signalr.service.js
this.connection.onclose((error) => {
  if (error) {
    console.error('Connexion SignalR fermée avec erreur:', error);
    // Tentative de reconnexion automatique
    setTimeout(() => {
      this.connect(token);
    }, 5000);
  } else {
    console.log('Connexion SignalR fermée normalement');
  }
});

this.connection.onreconnecting((error) => {
  console.log('Reconnexion SignalR en cours...');
});

this.connection.onreconnected((connectionId) => {
  console.log('✅ Reconnecté à SignalR:', connectionId);
});
```

---

## ✅ Bonnes pratiques

### 1. Gestion de l'état

Utilisez un store (Pinia, Vuex) pour gérer l'état des messages et campagnes :

```javascript
// stores/communication.js (Pinia)
import { defineStore } from 'pinia';
import { messageService, communicationService } from '@/services';

export const useCommunicationStore = defineStore('communication', {
  state: () => ({
    messages: [],
    campaigns: [],
    currentConversation: null,
    loading: false,
  }),
  actions: {
    async fetchMessages() {
      this.loading = true;
      try {
        this.messages = await messageService.getAll();
      } finally {
        this.loading = false;
      }
    },
    async sendMessage(message) {
      const sent = await messageService.sendMessage(message);
      this.messages.push(sent);
      return sent;
    },
  },
});
```

### 2. Optimisation des performances

- **Pagination** : Utilisez toujours la pagination pour les listes longues
- **Lazy loading** : Chargez les messages uniquement quand nécessaire
- **Cache** : Mettez en cache les conversations fréquentes
- **Debounce** : Utilisez debounce pour les recherches

### 3. Sécurité

- **Validation côté client** : Validez toujours les données avant envoi
- **Sanitization** : Nettoyez le contenu Markdown pour éviter les XSS
- **Rate limiting** : Respectez les limites d'envoi (messages par minute)

### 4. UX

- **Feedback visuel** : Affichez un indicateur de chargement
- **Notifications** : Utilisez les notifications du navigateur
- **Son** : Jouez un son pour les nouveaux messages (optionnel)
- **Marquage lu/non lu** : Indiquez visuellement les messages non lus

---

## 📞 Support

Pour toute question ou problème, contactez l'équipe de développement.

---

## 📝 Changelog

### Version 1.0.0 (2025-12-04)
- ✅ Système de messagerie (messages privés et groupes)
- ✅ Notifications automatiques (Push + SMS fallback)
- ✅ Campagnes de communication multi-canal
- ✅ SignalR pour le temps réel
- ✅ Segmentation des destinataires
- ✅ Planification et rappels automatiques
- ✅ Historique et statistiques

---

**Documentation générée le**: 2025-12-04  
**Version de l'API**: 1.0.0

