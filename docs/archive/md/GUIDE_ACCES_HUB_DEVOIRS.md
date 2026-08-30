# 📚 Guide d'accès au Hub SignalR - Devoirs à Domicile

## 🔗 URL du Hub

**Route du Hub :** `/hubs/devoirs-adomicile`

**URLs complètes :**
- **Développement local :** `https://localhost:7102/hubs/devoirs-adomicile`
- **Production :** `https://mombongo.asdc-rdc.org/hubs/devoirs-adomicile`
- **WebSocket Secure :** `wss://localhost:7102/hubs/devoirs-adomicile`

---

## 🔐 Authentification

Le hub nécessite une **authentification JWT**. Vous devez :
1. Vous authentifier via `/api/Utilisateur/Authentifier`
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
const API_BASE_URL = 'https://localhost:7102'; // ou votre URL de production
const HUB_URL = `${API_BASE_URL}/hubs/devoirs-adomicile`;

// Variable globale pour la connexion
let devoirConnection: signalR.HubConnection | null = null;

/**
 * Se connecter au hub Devoirs
 */
async function connectToDevoirHub(accessToken: string) {
  try {
    // Créer la connexion
    devoirConnection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, {
        accessTokenFactory: () => accessToken,
        skipNegotiation: false,
        transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: retryContext => {
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

    // Événement : Nouveau devoir publié (pour tous)
    devoirConnection.on("NouveauDevoir", (devoirData) => {
      console.log("📚 Nouveau devoir reçu :", devoirData);
      
      // Structure de devoirData :
      // {
      //   idDevoirADomicile: number,
      //   titre: string,
      //   description: string,
      //   contenu: string,
      //   nomFichier: string,
      //   tailleFichier: number,
      //   typeMIME: string,
      //   datePublication: Date,
      //   dateLimite: Date,
      //   idClasse: number,
      //   nomClasse: string,
      //   idAgent: number,
      //   nomAgent: string,
      //   idCours: number,
      //   nomCours: string,
      //   typeDevoir: string,
      //   idEcole: number,
      //   nombreTelechargements: number,
      //   timestamp: Date
      // }
      
      // Afficher une notification à l'utilisateur
      showNotification(`Nouveau devoir : ${devoirData.titre}`, devoirData.description);
      
      // Mettre à jour la liste des devoirs
      refreshDevoirsList();
    });

    // Événement : Nouveau devoir pour parent (personnalisé)
    devoirConnection.on("NouveauDevoirParent", (data) => {
      console.log("👨‍👩‍👧‍👦 Nouveau devoir pour parent :", data);
      
      // Structure de data :
      // {
      //   devoir: { ... }, // Même structure que NouveauDevoir
      //   enfants: string[], // Noms des enfants concernés
      //   messagePersonnalise: string // Ex: "Votre enfant Jean a un nouveau devoir"
      // }
      
      // Afficher une notification personnalisée
      showNotification(
        data.messagePersonnalise,
        `Classe : ${data.devoir.nomClasse} - ${data.devoir.titre}`
      );
      
      // Mettre à jour la liste des devoirs
      refreshDevoirsList();
    });

    // Événement : Confirmation de connexion à un groupe classe
    devoirConnection.on("JoinedClasseGroup", (idClasse) => {
      console.log(`✅ Connecté au groupe classe_${idClasse}`);
    });

    // Événement : Confirmation de déconnexion d'un groupe classe
    devoirConnection.on("LeftClasseGroup", (idClasse) => {
      console.log(`❌ Déconnecté du groupe classe_${idClasse}`);
    });

    // Événement : Statut de connexion
    devoirConnection.on("ConnectionStatus", (status) => {
      console.log("📊 Statut de connexion :", status);
      // {
      //   isConnected: boolean,
      //   userId: string,
      //   userName: string,
      //   idEcole: string,
      //   idClasse: string,
      //   connectionId: string,
      //   timestamp: Date
      // }
    });

    // ═══════════════════════════════════════════════════════════════════
    // 🔌 GESTION DES ÉVÉNEMENTS DE CONNEXION
    // ═══════════════════════════════════════════════════════════════════

    devoirConnection.onclose((error) => {
      console.log("❌ Connexion fermée", error);
      // La reconnexion automatique se fera grâce à withAutomaticReconnect
    });

    devoirConnection.onreconnecting((error) => {
      console.log("🔄 Reconnexion en cours...", error);
    });

    devoirConnection.onreconnected((connectionId) => {
      console.log("✅ Reconnexion réussie", connectionId);
    });

    // Démarrer la connexion
    await devoirConnection.start();
    console.log("✅ Connecté au hub Devoirs à Domicile");

    // Demander le statut de connexion
    await devoirConnection.invoke("GetConnectionStatus");

    return devoirConnection;
  } catch (error) {
    console.error("❌ Erreur de connexion au hub :", error);
    throw error;
  }
}

/**
 * Se déconnecter du hub
 */
async function disconnectFromDevoirHub() {
  if (devoirConnection) {
    await devoirConnection.stop();
    devoirConnection = null;
    console.log("❌ Déconnecté du hub Devoirs");
  }
}

/**
 * Rejoindre un groupe classe spécifique
 */
async function joinClasseGroup(idClasse: number) {
  if (devoirConnection && devoirConnection.state === signalR.HubConnectionState.Connected) {
    await devoirConnection.invoke("JoinClasseGroup", idClasse);
  }
}

/**
 * Quitter un groupe classe
 */
async function leaveClasseGroup(idClasse: number) {
  if (devoirConnection && devoirConnection.state === signalR.HubConnectionState.Connected) {
    await devoirConnection.invoke("LeaveClasseGroup", idClasse);
  }
}
```

---

## 📱 Exemple Vue.js 3 (Composition API)

```vue
<template>
  <div>
    <button @click="connect" :disabled="isConnected">Se connecter</button>
    <button @click="disconnect" :disabled="!isConnected">Se déconnecter</button>
    <div v-if="isConnected">✅ Connecté</div>
    <div v-else>❌ Déconnecté</div>
    
    <div v-for="devoir in nouveauxDevoirs" :key="devoir.idDevoirADomicile">
      <h3>{{ devoir.titre }}</h3>
      <p>{{ devoir.description }}</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';
import * as signalR from '@microsoft/signalr';
import { useAuth } from '@/composables/useAuth'; // Votre composable d'authentification

const { token } = useAuth();
const isConnected = ref(false);
const nouveauxDevoirs = ref([]);
let connection: signalR.HubConnection | null = null;

const API_BASE_URL = 'https://localhost:7102';
const HUB_URL = `${API_BASE_URL}/hubs/devoirs-adomicile`;

async function connect() {
  if (!token.value) {
    console.error('Token manquant');
    return;
  }

  connection = new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL, {
      accessTokenFactory: () => token.value,
      transport: signalR.HttpTransportType.WebSockets
    })
    .withAutomaticReconnect()
    .build();

  // Écouter les nouveaux devoirs
  connection.on("NouveauDevoir", (devoir) => {
    nouveauxDevoirs.value.unshift(devoir);
    console.log('Nouveau devoir reçu :', devoir);
  });

  // Écouter les devoirs personnalisés pour parents
  connection.on("NouveauDevoirParent", (data) => {
    nouveauxDevoirs.value.unshift(data.devoir);
    console.log('Nouveau devoir parent :', data);
  });

  try {
    await connection.start();
    isConnected.value = true;
    console.log('✅ Connecté au hub Devoirs');
  } catch (error) {
    console.error('❌ Erreur de connexion :', error);
  }
}

async function disconnect() {
  if (connection) {
    await connection.stop();
    connection = null;
    isConnected.value = false;
  }
}

onMounted(() => {
  if (token.value) {
    connect();
  }
});

onUnmounted(() => {
  disconnect();
});
</script>
```

---

## 🧪 Test avec un client SignalR (Outil de test)

### Option 1 : Utiliser un client Web (HTML/JavaScript)

Créez un fichier `test-devoir-hub.html` :

```html
<!DOCTYPE html>
<html>
<head>
    <title>Test Hub Devoirs</title>
    <script src="https://cdn.jsdelivr.net/npm/@microsoft/signalr@7.0.0/dist/browser/signalr.min.js"></script>
</head>
<body>
    <h1>Test Hub SignalR - Devoirs à Domicile</h1>
    <div>
        <input type="text" id="token" placeholder="Token JWT" style="width: 500px;">
        <button onclick="connect()">Se connecter</button>
        <button onclick="disconnect()">Se déconnecter</button>
    </div>
    <div id="status"></div>
    <div id="messages"></div>

    <script>
        let connection = null;
        const HUB_URL = 'https://localhost:7102/hubs/devoirs-adomicile';

        async function connect() {
            const token = document.getElementById('token').value;
            
            connection = new signalR.HubConnectionBuilder()
                .withUrl(HUB_URL, {
                    accessTokenFactory: () => token,
                    transport: signalR.HttpTransportType.WebSockets
                })
                .withAutomaticReconnect()
                .build();

            connection.on("NouveauDevoir", (devoir) => {
                addMessage('📚 Nouveau devoir', JSON.stringify(devoir, null, 2));
            });

            connection.on("NouveauDevoirParent", (data) => {
                addMessage('👨‍👩‍👧‍👦 Devoir parent', JSON.stringify(data, null, 2));
            });

            connection.onclose(() => {
                updateStatus('❌ Déconnecté');
            });

            try {
                await connection.start();
                updateStatus('✅ Connecté');
            } catch (error) {
                updateStatus('❌ Erreur : ' + error.message);
            }
        }

        async function disconnect() {
            if (connection) {
                await connection.stop();
                connection = null;
                updateStatus('❌ Déconnecté');
            }
        }

        function updateStatus(message) {
            document.getElementById('status').textContent = message;
        }

        function addMessage(title, content) {
            const div = document.createElement('div');
            div.innerHTML = `<h3>${title}</h3><pre>${content}</pre>`;
            document.getElementById('messages').appendChild(div);
        }
    </script>
</body>
</html>
```

### Option 2 : Utiliser Postman ou un client WebSocket

1. **Obtenez un token JWT** via l'API :
   ```bash
   curl -X POST https://localhost:7102/api/Utilisateur/Authentifier \
     -H "Content-Type: application/json" \
     -d '{"emailOuTelephone":"jk2@kelasinabiso.cd","motDePasse":"12345678"}'
   ```

2. **Connectez-vous au WebSocket** :
   - URL : `wss://localhost:7102/hubs/devoirs-adomicile`
   - Headers : `Authorization: Bearer <VOTRE_TOKEN>`

---

## 📋 Groupes SignalR automatiques

Lors de la connexion, vous êtes automatiquement ajouté aux groupes suivants selon votre rôle :

### Pour tous les utilisateurs :
- `user_{idUtilisateur}` - Groupe personnel
- `all_users` - Tous les utilisateurs connectés

### Pour les parents (tuteurs) :
- `classe_{idClasse}` - Pour chaque classe de leurs enfants
- `parents_classe_{idClasse}` - Groupe parents de chaque classe

### Pour les enseignants (agents) :
- `classe_{idClasse}` - Pour chaque classe où ils enseignent
- `ecole_{idEcole}` - Groupe de l'école

---

## 🔍 Vérification de la connexion

### Vérifier les logs côté serveur

```bash
# Voir les connexions au hub
tail -f logs/log-$(date +%Y%m%d).txt | grep -i "DevoirADomicileHub.*connected"

# Voir les notifications envoyées
tail -f logs/log-$(date +%Y%m%d).txt | grep -i "SignalR.*devoir"
```

### Utiliser le script de vérification

```bash
./verifier-signalr-devoir.sh [ID_DEVOIR]
```

---

## 🐛 Dépannage

### Problème : Connexion refusée

**Solution :**
- Vérifiez que le token JWT est valide
- Vérifiez que l'URL du hub est correcte
- Vérifiez que CORS est configuré correctement

### Problème : WebSocket non supporté

**Solution :**
- SignalR basculera automatiquement vers Server-Sent Events (SSE) ou Long Polling
- Vérifiez que le transport est configuré : `transport: signalR.HttpTransportType.WebSockets`

### Problème : Reconnexion automatique ne fonctionne pas

**Solution :**
- Vérifiez que `withAutomaticReconnect()` est appelé
- Vérifiez la configuration du timeout dans `Program.cs`

---

## 📚 Ressources supplémentaires

- [Documentation officielle SignalR](https://docs.microsoft.com/en-us/aspnet/core/signalr/introduction)
- [Guide complet SignalR de l'API](./GUIDE_COMPLET_SIGNALR.md)
- [Configuration SignalR](./CONFIGURATION_SIGNALR.md)

