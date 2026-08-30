# 📊 Guide d'accès au Hub SignalR Dashboard - Temps Réel

## 🔗 URL du Hub

**Route du Hub :** `/hubs/dashboard`

**URLs complètes :**
- **Développement local :** `http://localhost:5000/hubs/dashboard` ou `https://localhost:7102/hubs/dashboard`
- **Production :** `https://prod-knb.asdc-rdc.org/hubs/dashboard`

---

## 🔐 Authentification

Le hub nécessite une **authentification JWT**. Vous devez :
1. Vous authentifier via `/api/Utilisateur/authentifier`
2. Récupérer le `accessToken`
3. L'utiliser pour vous connecter au hub

---

## 📦 Installation (Frontend)

### JavaScript/TypeScript

```bash
npm install @microsoft/signalr
```

### Vue.js / React / Angular

```bash
npm install @microsoft/signalr
```

---

## 🚀 Connexion au Hub

### Exemple complet (JavaScript/TypeScript)

```typescript
import * as signalR from '@microsoft/signalr';

// Configuration
const API_BASE_URL = 'https://prod-knb.asdc-rdc.org'; // ou votre URL
const HUB_URL = `${API_BASE_URL}/hubs/dashboard`;

// Variable globale pour la connexion
let dashboardConnection: signalR.HubConnection | null = null;

/**
 * Se connecter au hub Dashboard
 */
async function connectToDashboardHub(accessToken: string) {
  try {
    // Créer la connexion
    dashboardConnection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, {
        accessTokenFactory: () => accessToken, // 🔑 Token JWT
        skipNegotiation: false, // Laisser SignalR négocier le transport
        transport: signalR.HttpTransportType.WebSockets // Préférer WebSocket
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: retryContext => {
          // Reconnexion progressive : 0s, 2s, 10s, 30s, puis toutes les 30s
          if (retryContext.previousRetryCount === 0) return 0;
          if (retryContext.previousRetryCount === 1) return 2000;
          if (retryContext.previousRetryCount === 2) return 10000;
          return 30000;
        }
      })
      .configureLogging(signalR.LogLevel.Information)
      .build();

    // ═══════════════════════════════════════════════════════════════════
    // 📡 ÉCOUTER LES ÉVÉNEMENTS
    // ═══════════════════════════════════════════════════════════════════

    // 1. Notification de mise à jour (événement générique)
    dashboardConnection.on("DashboardUpdateNotification", (notification) => {
      console.log("📢 Notification de mise à jour dashboard:", notification);
      /*
      notification = {
        ecoleId: 13,
        dashboardType: "presence" | "paiement" | "global",
        eventType: "presence_created" | "paiement_created" | etc.,
        timestamp: "2025-01-27T10:30:00Z"
      }
      */
      
      // Recharger le dashboard via API
      if (notification.dashboardType === "presence") {
        refreshDashboardPresence(notification.ecoleId);
      } else if (notification.dashboardType === "paiement") {
        refreshDashboardPaiement(notification.ecoleId);
      } else if (notification.dashboardType === "global") {
        refreshDashboardGlobal(notification.ecoleId);
      }
    });

    // 2. Dashboard global mis à jour (données complètes)
    dashboardConnection.on("DashboardGlobalUpdated", (dashboard) => {
      console.log("📊 Dashboard global mis à jour:", dashboard);
      // Mettre à jour directement l'UI avec les nouvelles données
      updateDashboardUI(dashboard);
    });

    // 3. Dashboard présence mis à jour (données complètes)
    dashboardConnection.on("DashboardPresenceUpdated", (dashboard) => {
      console.log("📊 Dashboard présence mis à jour:", dashboard);
      updatePresenceDashboardUI(dashboard);
    });

    // 4. Dashboard paiement mis à jour (données complètes)
    dashboardConnection.on("DashboardPaiementUpdated", (dashboard) => {
      console.log("📊 Dashboard paiement mis à jour:", dashboard);
      updatePaiementDashboardUI(dashboard);
    });

    // 5. Confirmation d'ajout à un groupe d'école
    dashboardConnection.on("JoinedEcoleGroup", (idEcole) => {
      console.log(`✅ Ajouté au groupe de l'école ${idEcole}`);
    });

    // 6. Confirmation de sortie d'un groupe d'école
    dashboardConnection.on("LeftEcoleGroup", (idEcole) => {
      console.log(`👋 Quitté le groupe de l'école ${idEcole}`);
    });

    // 7. Statut de connexion
    dashboardConnection.on("ConnectionStatus", (status) => {
      console.log("📡 Statut de connexion:", status);
      /*
      status = {
        IsConnected: true,
        UserId: "123",
        UserName: "John Doe",
        IdEcole: "13",
        ConnectionId: "abc123",
        Timestamp: "2025-01-27T10:30:00Z"
      }
      */
    });

    // ═══════════════════════════════════════════════════════════════════
    // 🔄 GESTION DES ÉTATS DE CONNEXION
    // ═══════════════════════════════════════════════════════════════════

    dashboardConnection.onclose((error) => {
      console.log("❌ Connexion fermée", error);
      // La reconnexion automatique se fera si configurée
    });

    dashboardConnection.onreconnecting((error) => {
      console.log("🔄 Reconnexion en cours...", error);
    });

    dashboardConnection.onreconnected((connectionId) => {
      console.log("✅ Reconnexion réussie. ConnectionId:", connectionId);
    });

    // Démarrer la connexion
    await dashboardConnection.start();
    console.log("✅ Connecté au DashboardHub SignalR");

    // Obtenir le statut de connexion
    await dashboardConnection.invoke("GetConnectionStatus");

    return dashboardConnection;
  } catch (error) {
    console.error("❌ Erreur lors de la connexion au DashboardHub:", error);
    throw error;
  }
}

/**
 * Rejoindre le groupe d'une école spécifique
 * Utile pour les Super-Admins qui veulent surveiller plusieurs écoles
 */
async function joinEcoleGroup(idEcole: number) {
  if (!dashboardConnection || dashboardConnection.state !== signalR.HubConnectionState.Connected) {
    console.error("❌ Pas connecté au hub");
    return;
  }

  try {
    await dashboardConnection.invoke("JoinEcoleGroup", idEcole);
    console.log(`✅ Rejoint le groupe de l'école ${idEcole}`);
  } catch (error) {
    console.error("❌ Erreur lors de l'ajout au groupe:", error);
  }
}

/**
 * Quitter le groupe d'une école
 */
async function leaveEcoleGroup(idEcole: number) {
  if (!dashboardConnection || dashboardConnection.state !== signalR.HubConnectionState.Connected) {
    console.error("❌ Pas connecté au hub");
    return;
  }

  try {
    await dashboardConnection.invoke("LeaveEcoleGroup", idEcole);
    console.log(`👋 Quitté le groupe de l'école ${idEcole}`);
  } catch (error) {
    console.error("❌ Erreur lors de la sortie du groupe:", error);
  }
}

/**
 * Déconnecter du hub
 */
async function disconnectDashboardHub() {
  if (dashboardConnection) {
    await dashboardConnection.stop();
    dashboardConnection = null;
    console.log("👋 Déconnecté du DashboardHub");
  }
}

// ═══════════════════════════════════════════════════════════════════
// 🔄 FONCTIONS DE RECHARGEMENT DES DASHBOARDS
// ═══════════════════════════════════════════════════════════════════

async function refreshDashboardGlobal(idEcole: number) {
  try {
    const response = await fetch(`${API_BASE_URL}/api/Dashboard/global?idEcole=${idEcole}`, {
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
      }
    });
    const dashboard = await response.json();
    updateDashboardUI(dashboard);
  } catch (error) {
    console.error("❌ Erreur lors du rechargement du dashboard global:", error);
  }
}

async function refreshDashboardPresence(idEcole: number) {
  try {
    const response = await fetch(`${API_BASE_URL}/api/Dashboard/presence?idEcole=${idEcole}`, {
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
      }
    });
    const dashboard = await response.json();
    updatePresenceDashboardUI(dashboard);
  } catch (error) {
    console.error("❌ Erreur lors du rechargement du dashboard présence:", error);
  }
}

async function refreshDashboardPaiement(idEcole: number) {
  try {
    const response = await fetch(`${API_BASE_URL}/api/Dashboard/paiement?idEcole=${idEcole}`, {
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
      }
    });
    const dashboard = await response.json();
    updatePaiementDashboardUI(dashboard);
  } catch (error) {
    console.error("❌ Erreur lors du rechargement du dashboard paiement:", error);
  }
}

// ═══════════════════════════════════════════════════════════════════
// 🎨 FONCTIONS DE MISE À JOUR DE L'UI (À ADAPTER SELON VOTRE FRAMEWORK)
// ═══════════════════════════════════════════════════════════════════

function updateDashboardUI(dashboard: any) {
  // Mettre à jour votre interface utilisateur
  // Exemple avec Vue.js :
  // dashboardData.value = dashboard;
}

function updatePresenceDashboardUI(dashboard: any) {
  // Mettre à jour le dashboard présence
}

function updatePaiementDashboardUI(dashboard: any) {
  // Mettre à jour le dashboard paiement
}
```

---

## 📱 Exemple Vue.js (Composition API)

```vue
<template>
  <div>
    <div v-if="!isConnected" class="status disconnected">
      ⚠️ Non connecté au dashboard temps réel
    </div>
    <div v-else class="status connected">
      ✅ Connecté au dashboard temps réel
    </div>
    
    <!-- Votre dashboard ici -->
    <DashboardComponent :data="dashboardData" />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';
import * as signalR from '@microsoft/signalr';
import { useAuthStore } from '@/stores/auth';

const authStore = useAuthStore();
const isConnected = ref(false);
const dashboardData = ref(null);
let connection: signalR.HubConnection | null = null;

onMounted(async () => {
  if (authStore.token) {
    await connectToDashboard();
  }
});

onUnmounted(async () => {
  if (connection) {
    await connection.stop();
  }
});

async function connectToDashboard() {
  try {
    connection = new signalR.HubConnectionBuilder()
      .withUrl('https://prod-knb.asdc-rdc.org/hubs/dashboard', {
        accessTokenFactory: () => authStore.token
      })
      .withAutomaticReconnect()
      .build();

    // Écouter les mises à jour
    connection.on("DashboardUpdateNotification", (notification) => {
      console.log("📢 Mise à jour dashboard:", notification);
      // Recharger les données
      loadDashboard();
    });

    connection.on("DashboardGlobalUpdated", (dashboard) => {
      dashboardData.value = dashboard;
    });

    await connection.start();
    isConnected.value = true;
    console.log("✅ Connecté au DashboardHub");
  } catch (error) {
    console.error("❌ Erreur connexion:", error);
  }
}

async function loadDashboard() {
  // Charger le dashboard via API
  const response = await fetch('/api/Dashboard/global?idEcole=' + authStore.idEcole, {
    headers: { 'Authorization': `Bearer ${authStore.token}` }
  });
  dashboardData.value = await response.json();
}
</script>
```

---

## 📡 Événements disponibles

### Événements reçus (du serveur vers le client)

| Événement | Description | Données |
|-----------|------------|---------|
| `DashboardUpdateNotification` | Notification qu'un événement a eu lieu | `{ ecoleId, dashboardType, eventType, timestamp }` |
| `DashboardGlobalUpdated` | Dashboard global complet mis à jour | `DashboardGlobalDto` |
| `DashboardPresenceUpdated` | Dashboard présence complet mis à jour | `DashboardPresenceDto` |
| `DashboardPaiementUpdated` | Dashboard paiement complet mis à jour | `DashboardPaiementDto` |
| `JoinedEcoleGroup` | Confirmation d'ajout à un groupe | `idEcole` |
| `LeftEcoleGroup` | Confirmation de sortie d'un groupe | `idEcole` |
| `ConnectionStatus` | Statut de la connexion | `{ IsConnected, UserId, UserName, IdEcole, ConnectionId, Timestamp }` |

### Méthodes invocables (du client vers le serveur)

| Méthode | Description | Paramètres |
|---------|-------------|------------|
| `JoinEcoleGroup` | Rejoindre le groupe d'une école | `idEcole: number` |
| `LeaveEcoleGroup` | Quitter le groupe d'une école | `idEcole: number` |
| `GetConnectionStatus` | Obtenir le statut de connexion | Aucun |

---

## 🔄 Flux de fonctionnement

### 1. Connexion automatique
- L'utilisateur se connecte automatiquement au groupe de son école (`ecole_{idEcole}`)
- L'`idEcole` est extrait du token JWT

### 2. Réception des notifications
- Lorsqu'un pointage est créé → `DashboardUpdateNotification` avec `dashboardType: "presence"`
- Lorsqu'un paiement est créé → `DashboardUpdateNotification` avec `dashboardType: "paiement"`

### 3. Rechargement des données
- Le client reçoit la notification
- Le client fait un appel API pour récupérer les nouvelles données
- L'UI est mise à jour

---

## 🧪 Test de connexion

### Avec curl (WebSocket)

```bash
# Note: curl ne supporte pas directement WebSocket
# Utilisez plutôt un client WebSocket ou le code JavaScript ci-dessus
```

### Avec un client WebSocket (wscat)

```bash
npm install -g wscat

# Connexion (nécessite un token JWT valide)
wscat -c "wss://prod-knb.asdc-rdc.org/hubs/dashboard?access_token=VOTRE_TOKEN_JWT"
```

### Test depuis le navigateur (Console)

```javascript
// 1. Obtenir un token JWT
const loginResponse = await fetch('https://prod-knb.asdc-rdc.org/api/Utilisateur/authentifier', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    emailOuTelephone: 'votre@email.com',
    motDePasse: 'votreMotDePasse'
  })
});
const { accessToken } = await loginResponse.json();

// 2. Se connecter au hub (nécessite @microsoft/signalr)
// Voir l'exemple TypeScript ci-dessus
```

---

## ⚠️ Points importants

1. **Authentification obligatoire** : Le hub nécessite un token JWT valide
2. **Groupes automatiques** : L'utilisateur est automatiquement ajouté au groupe de son école
3. **Reconnexion automatique** : SignalR reconnecte automatiquement en cas de perte de connexion
4. **Notifications légères** : Par défaut, seules les notifications sont envoyées (pas les données complètes)
5. **Rechargement manuel** : Le client doit faire un appel API pour récupérer les nouvelles données après une notification

---

## 🎯 Cas d'usage

### Cas 1 : Dashboard simple (notification + rechargement)

```typescript
// Écouter les notifications
connection.on("DashboardUpdateNotification", async (notification) => {
  // Recharger le dashboard
  const response = await fetch(`/api/Dashboard/global?idEcole=${notification.ecoleId}`);
  const dashboard = await response.json();
  updateUI(dashboard);
});
```

### Cas 2 : Dashboard avec données complètes (si implémenté)

```typescript
// Recevoir directement les données mises à jour
connection.on("DashboardGlobalUpdated", (dashboard) => {
  // Mettre à jour directement l'UI sans appel API
  updateUI(dashboard);
});
```

---

## 📝 Notes

- Le hub est disponible sur `/hubs/dashboard`
- L'authentification JWT est requise
- Les utilisateurs sont automatiquement ajoutés au groupe de leur école
- Les Super-Admins peuvent rejoindre plusieurs groupes d'écoles
- La reconnexion automatique est configurée

