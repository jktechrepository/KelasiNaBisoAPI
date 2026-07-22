// ============================================================================
// CLIENT DE TEST SIGNALR POUR DEVOIRS
// ============================================================================
// Client Node.js pour tester les notifications SignalR en temps réel
// Installation : npm install @microsoft/signalr
// ============================================================================

const signalR = require("@microsoft/signalr");

// Configuration
const BASE_URL = process.env.BASE_URL || "https://localhost:7102";
const HUB_URL = `${BASE_URL}/hubs/devoirs-adomicile`;
const TOKEN = process.env.TOKEN || "";

if (!TOKEN) {
  console.error("❌ Token JWT requis. Utilisez : TOKEN=<votre_token> node test-signalr-client.js");
  process.exit(1);
}

console.log("🔌 Connexion au Hub SignalR...");
console.log(`   URL : ${HUB_URL}`);
console.log("");

// Créer la connexion SignalR
const connection = new signalR.HubConnectionBuilder()
  .withUrl(HUB_URL, {
    accessTokenFactory: () => TOKEN,
    skipNegotiation: false,
    transport: signalR.HttpTransportType.WebSockets
  })
  .withAutomaticReconnect()
  .configureLogging(signalR.LogLevel.Information)
  .build();

// Gérer les événements de connexion
connection.onclose((error) => {
  console.log("❌ Connexion fermée");
  if (error) {
    console.error("   Erreur :", error);
  }
});

connection.onreconnecting((error) => {
  console.log("🔄 Reconnexion en cours...");
  if (error) {
    console.error("   Erreur :", error);
  }
});

connection.onreconnected((connectionId) => {
  console.log(`✅ Reconnecté. ConnectionId : ${connectionId}`);
});

// Écouter l'événement NouveauDevoir
connection.on("NouveauDevoir", (devoir) => {
  console.log("");
  console.log("📚 ========================================");
  console.log("📚 NOUVEAU DEVOIR REÇU (SignalR)");
  console.log("📚 ========================================");
  console.log("");
  console.log("📝 Titre :", devoir.titre);
  console.log("📅 Date limite :", devoir.dateLimite || "Non spécifiée");
  console.log("👨‍🏫 Enseignant :", devoir.nomAgent);
  console.log("📚 Classe :", devoir.nomClasse);
  console.log("📎 Type :", devoir.typeDevoir);
  if (devoir.description) {
    console.log("📄 Description :", devoir.description);
  }
  if (devoir.contenu) {
    console.log("📝 Contenu :", devoir.contenu.substring(0, 100) + "...");
  }
  console.log("");
  console.log("✅ Notification SignalR reçue avec succès !");
  console.log("");
});

// Écouter l'événement NouveauDevoirParent (personnalisé)
connection.on("NouveauDevoirParent", (data) => {
  console.log("");
  console.log("👨‍👩‍👧‍👦 ========================================");
  console.log("👨‍👩‍👧‍👦 NOUVEAU DEVOIR POUR VOS ENFANTS (SignalR)");
  console.log("👨‍👩‍👧‍👦 ========================================");
  console.log("");
  console.log("💬 Message :", data.messagePersonnalise);
  console.log("👶 Enfants concernés :", data.enfants.join(", "));
  console.log("📝 Titre du devoir :", data.devoir.titre);
  console.log("📅 Date limite :", data.devoir.dateLimite || "Non spécifiée");
  console.log("");
  console.log("✅ Notification personnalisée reçue avec succès !");
  console.log("");
});

// Écouter l'événement de confirmation de groupe
connection.on("JoinedClasseGroup", (idClasse) => {
  console.log(`✅ Ajouté au groupe classe_${idClasse}`);
});

connection.on("LeftClasseGroup", (idClasse) => {
  console.log(`✅ Retiré du groupe classe_${idClasse}`);
});

connection.on("ConnectionStatus", (status) => {
  console.log("📊 Statut de connexion :", JSON.stringify(status, null, 2));
});

// Démarrer la connexion
async function start() {
  try {
    await connection.start();
    console.log("✅ Connecté au Hub SignalR");
    console.log(`   ConnectionId : ${connection.connectionId}`);
    console.log("");

    // Demander le statut de connexion
    await connection.invoke("GetConnectionStatus");

    // Rejoindre le groupe d'une classe (optionnel)
    // await connection.invoke("JoinClasseGroup", 80);

    console.log("👂 En attente de notifications...");
    console.log("   (Appuyez sur Ctrl+C pour quitter)");
    console.log("");

    // Garder le processus actif
    process.on("SIGINT", async () => {
      console.log("\n🛑 Arrêt du client SignalR...");
      await connection.stop();
      process.exit(0);
    });

  } catch (err) {
    console.error("❌ Erreur de connexion :", err);
    process.exit(1);
  }
}

start();

