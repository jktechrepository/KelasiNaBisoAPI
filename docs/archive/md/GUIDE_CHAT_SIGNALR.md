# 💬 GUIDE COMPLET : CHAT EN TEMPS RÉEL AVEC SIGNALR

**Date** : 1 novembre 2025  
**Contexte** : KelasiNaBisoAPI  
**Objectif** : Implémenter un système de chat comme WhatsApp

---

## 🎯 QU'EST-CE QU'UN CHAT SIGNALR ?

Un système de **messagerie instantanée** en temps réel où :
- Les parents peuvent poser des questions à l'administration
- Les administrateurs peuvent répondre immédiatement
- Les messages s'affichent instantanément sans rafraîchir
- Historique des conversations sauvegardé en base de données
- Notifications de "nouveau message" en temps réel

### Analogie : WhatsApp pour votre école

```
Parent → "Bonjour, je voudrais changer la classe de mon enfant"
         [Message envoyé instantanément via SignalR]
         
Admin  → Voit le message apparaître en temps réel (< 100ms)
         "Bonjour ! Bien sûr, de quelle classe vers quelle classe ?"
         [Réponse instantanée]
         
Parent → Voit la réponse immédiatement
         "De 5ème A vers 5ème B"
```

---

## 🏗️ ARCHITECTURE DU SYSTÈME

### Vue d'ensemble

```
┌─────────────────────────────────────────────────────────────┐
│                    BASE DE DONNÉES                           │
│  ┌────────────────────────────────────────────────────┐     │
│  │  Table: Messages                                   │     │
│  │  - IdMessage                                       │     │
│  │  - IdConversation                                  │     │
│  │  - IdExpediteur (qui envoie)                       │     │
│  │  - Contenu                                         │     │
│  │  - DateEnvoi                                       │     │
│  │  - Statut (Envoye, Lu, etc.)                       │     │
│  └────────────────────────────────────────────────────┘     │
│  ┌────────────────────────────────────────────────────┐     │
│  │  Table: Conversations                              │     │
│  │  - IdConversation                                  │     │
│  │  - IdParent                                        │     │
│  │  - IdAdmin                                         │     │
│  │  - Sujet                                           │     │
│  │  - DateCreation                                    │     │
│  │  - Statut (Ouverte, Fermee)                        │     │
│  └────────────────────────────────────────────────────┘     │
└─────────────────────────────────────────────────────────────┘
                          ↕
┌─────────────────────────────────────────────────────────────┐
│                    BACKEND (ASP.NET Core)                    │
│  ┌────────────────────────────────────────────────────┐     │
│  │           ChatHub.cs                               │     │
│  │  - SendMessage(conversationId, message)            │     │
│  │  - JoinConversation(conversationId)                │     │
│  │  - MarkAsRead(messageId)                           │     │
│  │  - UserTyping(conversationId)                      │     │
│  └────────────────────────────────────────────────────┘     │
│  ┌────────────────────────────────────────────────────┐     │
│  │      Controllers & Services                        │     │
│  │  - ConversationController                          │     │
│  │  - MessageService                                  │     │
│  └────────────────────────────────────────────────────┘     │
└─────────────────────────────────────────────────────────────┘
                          ↕ WebSocket (SignalR)
┌─────────────────────────────────────────────────────────────┐
│                    FRONTEND (Vue.js/React)                   │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐         │
│  │   Parent A  │  │   Parent B  │  │   Admin     │         │
│  │  (Chat)     │  │  (Chat)     │  │  (Dashboard)│         │
│  │  Connecté   │  │  Connecté   │  │  Connecté   │         │
│  └─────────────┘  └─────────────┘  └─────────────┘         │
│       ↓                 ↓                 ↓                  │
│  Envoie msg       Envoie msg       Reçoit tous msg          │
│  Reçoit réponse   Reçoit réponse   Répond                   │
└─────────────────────────────────────────────────────────────┘
```

---

## 📊 MODÈLES DE DONNÉES

### 1. Table `Conversations`

```csharp
public class Conversation
{
    public int IdConversation { get; set; }
    
    // Participants
    public int IdParent { get; set; }
    public int? IdAdmin { get; set; } // Null si pas encore assigné
    
    // Métadonnées
    public string Sujet { get; set; } // Ex: "Changement de classe"
    public DateTime DateCreation { get; set; }
    public DateTime? DateDerniereActivite { get; set; }
    
    // Statut
    public string Statut { get; set; } // "Ouverte", "EnCours", "Fermee"
    public bool ParentVuDernier { get; set; } // Pour les notifications
    public bool AdminVuDernier { get; set; }
    
    // Relations
    public Utilisateur Parent { get; set; }
    public Utilisateur? Admin { get; set; }
    public ICollection<Message> Messages { get; set; }
}
```

### 2. Table `Messages`

```csharp
public class Message
{
    public int IdMessage { get; set; }
    
    // Appartenance
    public int IdConversation { get; set; }
    public int IdExpediteur { get; set; }
    
    // Contenu
    public string Contenu { get; set; }
    public string? TypeMessage { get; set; } // "Texte", "Image", "Document"
    public string? FichierUrl { get; set; } // Si c'est une image/document
    
    // Métadonnées
    public DateTime DateEnvoi { get; set; }
    public DateTime? DateLu { get; set; }
    public bool EstLu { get; set; }
    
    // Relations
    public Conversation Conversation { get; set; }
    public Utilisateur Expediteur { get; set; }
}
```

---

## 🔧 IMPLÉMENTATION BACKEND

### 1. ChatHub.cs (Hub SignalR)

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace KelasiNaBisoAPI.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IMessageService _messageService;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(
            IMessageService messageService,
            ILogger<ChatHub> logger)
        {
            _messageService = messageService;
            _logger = logger;
        }

        /// <summary>
        /// Rejoindre une conversation (room)
        /// </summary>
        public async Task JoinConversation(int conversationId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
            
            // Ajouter l'utilisateur au groupe de la conversation
            await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
            
            _logger.LogInformation($"User {userName} (ID: {userId}) joined conversation {conversationId}");
            
            // Notifier les autres participants
            await Clients.GroupExcept($"conversation_{conversationId}", Context.ConnectionId)
                .SendAsync("UserJoined", new { userId, userName, conversationId });
        }

        /// <summary>
        /// Quitter une conversation
        /// </summary>
        public async Task LeaveConversation(int conversationId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
            
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
            
            _logger.LogInformation($"User {userName} (ID: {userId}) left conversation {conversationId}");
            
            // Notifier les autres participants
            await Clients.Group($"conversation_{conversationId}")
                .SendAsync("UserLeft", new { userId, userName, conversationId });
        }

        /// <summary>
        /// Envoyer un message dans une conversation
        /// </summary>
        public async Task SendMessage(int conversationId, string contenu)
        {
            var userId = int.Parse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
            
            try
            {
                // Sauvegarder le message en base de données
                var message = await _messageService.CreateAsync(new Message
                {
                    IdConversation = conversationId,
                    IdExpediteur = userId,
                    Contenu = contenu,
                    DateEnvoi = DateTime.UtcNow,
                    EstLu = false
                });
                
                _logger.LogInformation($"Message sent by {userName} in conversation {conversationId}");
                
                // Envoyer le message à tous les participants de la conversation
                await Clients.Group($"conversation_{conversationId}")
                    .SendAsync("ReceiveMessage", new
                    {
                        idMessage = message.IdMessage,
                        idConversation = conversationId,
                        idExpediteur = userId,
                        nomExpediteur = userName,
                        contenu = contenu,
                        dateEnvoi = message.DateEnvoi,
                        estLu = false
                    });
                
                // Envoyer une notification aux participants qui ne sont pas dans la conversation
                await NotifyParticipantsNotInConversation(conversationId, userId, userName, contenu);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                await Clients.Caller.SendAsync("MessageError", new { message = "Erreur lors de l'envoi du message" });
            }
        }

        /// <summary>
        /// Marquer un message comme lu
        /// </summary>
        public async Task MarkAsRead(int messageId)
        {
            var userId = int.Parse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            try
            {
                await _messageService.MarkAsReadAsync(messageId, userId);
                
                var message = await _messageService.GetByIdAsync(messageId);
                
                // Notifier l'expéditeur que son message a été lu
                await Clients.Group($"conversation_{message.IdConversation}")
                    .SendAsync("MessageRead", new { messageId, readBy = userId, readAt = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking message as read");
            }
        }

        /// <summary>
        /// Indiquer que l'utilisateur est en train de taper
        /// </summary>
        public async Task UserTyping(int conversationId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
            
            // Notifier les autres participants (sauf l'expéditeur)
            await Clients.GroupExcept($"conversation_{conversationId}", Context.ConnectionId)
                .SendAsync("UserIsTyping", new { userId, userName, conversationId });
        }

        /// <summary>
        /// Indiquer que l'utilisateur a arrêté de taper
        /// </summary>
        public async Task UserStoppedTyping(int conversationId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
            
            await Clients.GroupExcept($"conversation_{conversationId}", Context.ConnectionId)
                .SendAsync("UserStoppedTyping", new { userId, userName, conversationId });
        }

        /// <summary>
        /// Notifier les participants qui ne sont pas actuellement dans la conversation
        /// </summary>
        private async Task NotifyParticipantsNotInConversation(
            int conversationId, 
            int senderId, 
            string senderName, 
            string messagePreview)
        {
            // Récupérer les IDs des participants de la conversation
            var conversation = await _messageService.GetConversationByIdAsync(conversationId);
            
            var participantIds = new List<int>();
            if (conversation.IdParent != senderId)
                participantIds.Add(conversation.IdParent);
            if (conversation.IdAdmin.HasValue && conversation.IdAdmin.Value != senderId)
                participantIds.Add(conversation.IdAdmin.Value);
            
            // Envoyer une notification push à chaque participant
            foreach (var participantId in participantIds)
            {
                await Clients.Group($"user_{participantId}")
                    .SendAsync("NewChatMessage", new
                    {
                        conversationId = conversationId,
                        senderName = senderName,
                        messagePreview = messagePreview.Length > 50 
                            ? messagePreview.Substring(0, 50) + "..." 
                            : messagePreview,
                        timestamp = DateTime.UtcNow
                    });
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation($"User {userId} disconnected from ChatHub");
            
            await base.OnDisconnectedAsync(exception);
        }
    }
}
```

### 2. ConversationController.cs

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConversationController : ControllerBase
{
    private readonly IConversationService _conversationService;
    private readonly ILogger<ConversationController> _logger;

    public ConversationController(
        IConversationService conversationService,
        ILogger<ConversationController> logger)
    {
        _conversationService = conversationService;
        _logger = logger;
    }

    // GET: api/Conversation
    // Récupérer toutes les conversations de l'utilisateur connecté
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Conversation>>> GetMyConversations()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var conversations = await _conversationService.GetUserConversationsAsync(userId);
        return Ok(conversations);
    }

    // GET: api/Conversation/{id}
    // Récupérer une conversation spécifique avec tous ses messages
    [HttpGet("{id}")]
    public async Task<ActionResult<Conversation>> GetConversation(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var conversation = await _conversationService.GetByIdAsync(id);
        
        if (conversation == null)
            return NotFound();
        
        // Vérifier que l'utilisateur fait partie de la conversation
        if (conversation.IdParent != userId && conversation.IdAdmin != userId)
            return Forbid();
        
        return Ok(conversation);
    }

    // GET: api/Conversation/{id}/messages
    // Récupérer les messages d'une conversation
    [HttpGet("{id}/messages")]
    public async Task<ActionResult<IEnumerable<Message>>> GetMessages(int id, [FromQuery] int? limit = 50)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var conversation = await _conversationService.GetByIdAsync(id);
        
        if (conversation == null)
            return NotFound();
        
        if (conversation.IdParent != userId && conversation.IdAdmin != userId)
            return Forbid();
        
        var messages = await _conversationService.GetMessagesAsync(id, limit.Value);
        return Ok(messages);
    }

    // POST: api/Conversation
    // Créer une nouvelle conversation
    [HttpPost]
    public async Task<ActionResult<Conversation>> CreateConversation(CreateConversationDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        var conversation = await _conversationService.CreateAsync(new Conversation
        {
            IdParent = userId,
            Sujet = dto.Sujet,
            DateCreation = DateTime.UtcNow,
            Statut = "Ouverte"
        });
        
        return CreatedAtAction(nameof(GetConversation), new { id = conversation.IdConversation }, conversation);
    }

    // PUT: api/Conversation/{id}/assign
    // Assigner un admin à une conversation
    [HttpPut("{id}/assign")]
    [Authorize(Roles = "Admin,Super-Admin")]
    public async Task<ActionResult> AssignAdmin(int id, [FromBody] int adminId)
    {
        var success = await _conversationService.AssignAdminAsync(id, adminId);
        
        if (!success)
            return NotFound();
        
        return Ok(new { message = "Admin assigné avec succès" });
    }

    // PUT: api/Conversation/{id}/close
    // Fermer une conversation
    [HttpPut("{id}/close")]
    public async Task<ActionResult> CloseConversation(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var success = await _conversationService.CloseConversationAsync(id, userId);
        
        if (!success)
            return NotFound();
        
        return Ok(new { message = "Conversation fermée" });
    }
}
```

### 3. Configuration dans Program.cs

```csharp
// Enregistrer les services
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IMessageService, MessageService>();

// Mapper le ChatHub
app.MapHub<ChatHub>("/hubs/chat");
```

---

## 🌐 IMPLÉMENTATION FRONTEND

### 1. Service Chat (Vue.js Composition API)

```javascript
// composables/useChat.js
import { ref, onMounted, onUnmounted } from 'vue';
import * as signalR from '@microsoft/signalr';

export function useChat() {
  const connection = ref(null);
  const isConnected = ref(false);
  const currentConversation = ref(null);
  const messages = ref([]);
  const conversations = ref([]);
  const isTyping = ref(false);
  const typingUsers = ref([]);

  // Connexion au ChatHub
  async function connect() {
    connection.value = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7103/hubs/chat', {
        accessTokenFactory: () => localStorage.getItem('accessToken'),
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    setupEventListeners();

    try {
      await connection.value.start();
      isConnected.value = true;
      console.log('✅ Connecté au ChatHub');
    } catch (error) {
      console.error('❌ Erreur connexion ChatHub:', error);
    }
  }

  // Configurer les écouteurs d'événements
  function setupEventListeners() {
    // Recevoir un message
    connection.value.on('ReceiveMessage', (message) => {
      console.log('📩 Nouveau message:', message);
      
      // Ajouter le message à la liste
      messages.value.push({
        id: message.idMessage,
        conversationId: message.idConversation,
        senderId: message.idExpediteur,
        senderName: message.nomExpediteur,
        content: message.contenu,
        timestamp: new Date(message.dateEnvoi),
        isRead: false,
        isMine: message.idExpediteur === getCurrentUserId()
      });
      
      // Jouer un son
      playNotificationSound();
      
      // Marquer comme lu si on est dans la conversation
      if (currentConversation.value?.id === message.idConversation) {
        markAsRead(message.idMessage);
      }
      
      // Scroll vers le bas
      scrollToBottom();
    });

    // Utilisateur rejoint
    connection.value.on('UserJoined', (data) => {
      console.log('👋 Utilisateur rejoint:', data.userName);
    });

    // Utilisateur quitte
    connection.value.on('UserLeft', (data) => {
      console.log('👋 Utilisateur quitte:', data.userName);
    });

    // Message lu
    connection.value.on('MessageRead', (data) => {
      console.log('✓✓ Message lu:', data.messageId);
      
      const message = messages.value.find(m => m.id === data.messageId);
      if (message) {
        message.isRead = true;
        message.readAt = new Date(data.readAt);
      }
    });

    // Utilisateur en train de taper
    connection.value.on('UserIsTyping', (data) => {
      console.log('✍️ Utilisateur tape:', data.userName);
      
      if (!typingUsers.value.includes(data.userName)) {
        typingUsers.value.push(data.userName);
      }
    });

    // Utilisateur arrête de taper
    connection.value.on('UserStoppedTyping', (data) => {
      typingUsers.value = typingUsers.value.filter(u => u !== data.userName);
    });

    // Nouveau message (notification quand pas dans la conversation)
    connection.value.on('NewChatMessage', (data) => {
      console.log('🔔 Nouveau message de chat:', data);
      
      // Afficher une notification toast
      showToast(
        `Nouveau message de ${data.senderName}`,
        data.messagePreview,
        () => openConversation(data.conversationId)
      );
      
      // Incrémenter le compteur de messages non lus
      incrementUnreadCount(data.conversationId);
    });

    // Erreur
    connection.value.on('MessageError', (data) => {
      console.error('❌ Erreur message:', data.message);
      showError(data.message);
    });
  }

  // Rejoindre une conversation
  async function joinConversation(conversationId) {
    if (!connection.value) return;
    
    try {
      await connection.value.invoke('JoinConversation', conversationId);
      console.log(`✅ Conversation ${conversationId} rejointe`);
      
      // Charger les messages
      await loadMessages(conversationId);
      
      currentConversation.value = { id: conversationId };
    } catch (error) {
      console.error('❌ Erreur rejoindre conversation:', error);
    }
  }

  // Quitter une conversation
  async function leaveConversation(conversationId) {
    if (!connection.value) return;
    
    try {
      await connection.value.invoke('LeaveConversation', conversationId);
      console.log(`👋 Conversation ${conversationId} quittée`);
      
      currentConversation.value = null;
      messages.value = [];
    } catch (error) {
      console.error('❌ Erreur quitter conversation:', error);
    }
  }

  // Envoyer un message
  async function sendMessage(conversationId, content) {
    if (!connection.value || !content.trim()) return;
    
    try {
      await connection.value.invoke('SendMessage', conversationId, content);
      console.log('✅ Message envoyé');
    } catch (error) {
      console.error('❌ Erreur envoi message:', error);
      showError('Erreur lors de l\'envoi du message');
    }
  }

  // Marquer comme lu
  async function markAsRead(messageId) {
    if (!connection.value) return;
    
    try {
      await connection.value.invoke('MarkAsRead', messageId);
    } catch (error) {
      console.error('❌ Erreur marquer comme lu:', error);
    }
  }

  // Indiquer qu'on tape
  let typingTimeout = null;
  async function startTyping(conversationId) {
    if (!connection.value) return;
    
    try {
      await connection.value.invoke('UserTyping', conversationId);
      
      // Arrêter automatiquement après 3 secondes
      clearTimeout(typingTimeout);
      typingTimeout = setTimeout(() => stopTyping(conversationId), 3000);
    } catch (error) {
      console.error('❌ Erreur typing:', error);
    }
  }

  async function stopTyping(conversationId) {
    if (!connection.value) return;
    
    try {
      await connection.value.invoke('UserStoppedTyping', conversationId);
    } catch (error) {
      console.error('❌ Erreur stop typing:', error);
    }
  }

  // Charger les messages via API REST
  async function loadMessages(conversationId, limit = 50) {
    try {
      const response = await fetch(
        `https://localhost:7103/api/Conversation/${conversationId}/messages?limit=${limit}`,
        {
          headers: {
            'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
          }
        }
      );
      
      const data = await response.json();
      messages.value = data.map(msg => ({
        id: msg.idMessage,
        conversationId: msg.idConversation,
        senderId: msg.idExpediteur,
        content: msg.contenu,
        timestamp: new Date(msg.dateEnvoi),
        isRead: msg.estLu,
        isMine: msg.idExpediteur === getCurrentUserId()
      }));
    } catch (error) {
      console.error('❌ Erreur chargement messages:', error);
    }
  }

  // Charger les conversations via API REST
  async function loadConversations() {
    try {
      const response = await fetch(
        'https://localhost:7103/api/Conversation',
        {
          headers: {
            'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
          }
        }
      );
      
      conversations.value = await response.json();
    } catch (error) {
      console.error('❌ Erreur chargement conversations:', error);
    }
  }

  // Déconnexion
  async function disconnect() {
    if (connection.value) {
      await connection.value.stop();
      isConnected.value = false;
      console.log('🔌 Déconnecté du ChatHub');
    }
  }

  // Lifecycle hooks
  onMounted(() => {
    connect();
  });

  onUnmounted(() => {
    disconnect();
  });

  return {
    // State
    isConnected,
    currentConversation,
    messages,
    conversations,
    typingUsers,
    
    // Methods
    connect,
    disconnect,
    joinConversation,
    leaveConversation,
    sendMessage,
    markAsRead,
    startTyping,
    stopTyping,
    loadMessages,
    loadConversations
  };
}
```

### 2. Composant Chat (Vue.js)

```vue
<!-- components/Chat.vue -->
<template>
  <div class="chat-container">
    <!-- Liste des conversations (sidebar) -->
    <div class="conversations-list">
      <div class="conversations-header">
        <h3>💬 Mes Conversations</h3>
        <button @click="newConversation" class="btn-new">
          + Nouveau
        </button>
      </div>
      
      <div class="conversations">
        <div 
          v-for="conv in conversations" 
          :key="conv.idConversation"
          :class="['conversation-item', { active: currentConversation?.id === conv.idConversation }]"
          @click="selectConversation(conv)"
        >
          <div class="conversation-avatar">
            {{ conv.admin?.nomUtilisateur?.[0] || '?' }}
          </div>
          <div class="conversation-info">
            <div class="conversation-title">{{ conv.sujet }}</div>
            <div class="conversation-preview">{{ conv.dernierMessage }}</div>
          </div>
          <div class="conversation-meta">
            <span class="conversation-time">{{ formatTime(conv.dateDerniereActivite) }}</span>
            <span v-if="conv.unreadCount" class="unread-badge">{{ conv.unreadCount }}</span>
          </div>
        </div>
      </div>
    </div>

    <!-- Zone de chat -->
    <div class="chat-area" v-if="currentConversation">
      <!-- Header -->
      <div class="chat-header">
        <div class="chat-user-info">
          <div class="chat-avatar">
            {{ currentConversation.otherUser?.nomUtilisateur?.[0] || '?' }}
          </div>
          <div>
            <div class="chat-username">{{ currentConversation.otherUser?.nomUtilisateur }}</div>
            <div class="chat-status">
              <span v-if="typingUsers.length > 0" class="typing">
                ✍️ {{ typingUsers.join(', ') }} en train d'écrire...
              </span>
              <span v-else class="online">En ligne</span>
            </div>
          </div>
        </div>
        <button @click="closeConversation" class="btn-close">✖</button>
      </div>

      <!-- Messages -->
      <div class="chat-messages" ref="messagesContainer">
        <div 
          v-for="message in messages" 
          :key="message.id"
          :class="['message', { mine: message.isMine }]"
        >
          <div class="message-avatar" v-if="!message.isMine">
            {{ message.senderName?.[0] || '?' }}
          </div>
          <div class="message-bubble">
            <div class="message-content">{{ message.content }}</div>
            <div class="message-meta">
              <span class="message-time">{{ formatTime(message.timestamp) }}</span>
              <span v-if="message.isMine" class="message-status">
                <span v-if="message.isRead">✓✓</span>
                <span v-else>✓</span>
              </span>
            </div>
          </div>
        </div>
      </div>

      <!-- Input -->
      <div class="chat-input-area">
        <input 
          v-model="messageInput"
          @keyup.enter="handleSendMessage"
          @input="handleTyping"
          placeholder="Écrivez votre message..."
          class="chat-input"
        />
        <button @click="handleSendMessage" class="btn-send">
          Envoyer
        </button>
      </div>
    </div>

    <!-- Placeholder si aucune conversation sélectionnée -->
    <div v-else class="chat-placeholder">
      <div class="placeholder-content">
        <div class="placeholder-icon">💬</div>
        <h3>Sélectionnez une conversation</h3>
        <p>Choisissez une conversation dans la liste ou créez-en une nouvelle</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, nextTick } from 'vue';
import { useChat } from '@/composables/useChat';

const {
  isConnected,
  currentConversation,
  messages,
  conversations,
  typingUsers,
  joinConversation,
  leaveConversation,
  sendMessage,
  startTyping,
  stopTyping,
  loadConversations
} = useChat();

const messageInput = ref('');
const messagesContainer = ref(null);

// Sélectionner une conversation
async function selectConversation(conv) {
  if (currentConversation.value?.id) {
    await leaveConversation(currentConversation.value.id);
  }
  
  await joinConversation(conv.idConversation);
  
  // Scroll vers le bas
  await nextTick();
  scrollToBottom();
}

// Envoyer un message
async function handleSendMessage() {
  if (!messageInput.value.trim() || !currentConversation.value) return;
  
  await sendMessage(currentConversation.value.id, messageInput.value);
  messageInput.value = '';
  
  await stopTyping(currentConversation.value.id);
}

// Gérer le typing
let typingTimeoutId = null;
function handleTyping() {
  if (!currentConversation.value) return;
  
  startTyping(currentConversation.value.id);
  
  // Arrêter après 1 seconde d'inactivité
  clearTimeout(typingTimeoutId);
  typingTimeoutId = setTimeout(() => {
    stopTyping(currentConversation.value.id);
  }, 1000);
}

// Scroll vers le bas
function scrollToBottom() {
  if (messagesContainer.value) {
    messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight;
  }
}

// Formater le temps
function formatTime(date) {
  if (!date) return '';
  const d = new Date(date);
  const now = new Date();
  
  const diffMs = now - d;
  const diffMins = Math.floor(diffMs / 60000);
  
  if (diffMins < 1) return 'À l\'instant';
  if (diffMins < 60) return `Il y a ${diffMins} min`;
  if (diffMins < 1440) return `Il y a ${Math.floor(diffMins / 60)} h`;
  
  return d.toLocaleDateString('fr-FR', { day: 'numeric', month: 'short' });
}

// Charger les conversations au démarrage
loadConversations();
</script>

<style scoped>
/* Styles CSS (exemple) */
.chat-container {
  display: flex;
  height: 600px;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  overflow: hidden;
}

.conversations-list {
  width: 320px;
  border-right: 1px solid #e0e0e0;
  display: flex;
  flex-direction: column;
}

.chat-messages {
  flex: 1;
  overflow-y: auto;
  padding: 20px;
}

.message {
  display: flex;
  margin-bottom: 16px;
}

.message.mine {
  flex-direction: row-reverse;
}

.message-bubble {
  max-width: 70%;
  padding: 12px 16px;
  border-radius: 16px;
  background: #f0f0f0;
}

.message.mine .message-bubble {
  background: #007bff;
  color: white;
}

.chat-input-area {
  display: flex;
  padding: 16px;
  border-top: 1px solid #e0e0e0;
}

.chat-input {
  flex: 1;
  padding: 12px;
  border: 1px solid #e0e0e0;
  border-radius: 24px;
  outline: none;
}

.btn-send {
  margin-left: 8px;
  padding: 12px 24px;
  background: #007bff;
  color: white;
  border: none;
  border-radius: 24px;
  cursor: pointer;
}
</style>
```

---

## 🎨 FONCTIONNALITÉS AVANCÉES

### 1. **Indicateur "En train d'écrire..."**
✅ Implémenté via `UserTyping` / `UserStoppedTyping`

### 2. **Double coche (lu/non lu)**
✅ Implémenté via `MarkAsRead` / `MessageRead`

### 3. **Notifications push hors conversation**
✅ Implémenté via `NewChatMessage`

### 4. **Historique des messages**
✅ Chargement via API REST `/api/Conversation/{id}/messages`

### 5. **Support des images/fichiers** (Future extension)
```csharp
// Ajouter dans ChatHub
public async Task SendFile(int conversationId, string fileName, string fileUrl)
{
    var message = await _messageService.CreateAsync(new Message
    {
        IdConversation = conversationId,
        IdExpediteur = GetCurrentUserId(),
        Contenu = $"📎 {fileName}",
        TypeMessage = "Document",
        FichierUrl = fileUrl,
        DateEnvoi = DateTime.UtcNow
    });
    
    await Clients.Group($"conversation_{conversationId}")
        .SendAsync("ReceiveFile", message);
}
```

---

## 📊 AVANTAGES PAR RAPPORT AUX EMAILS

| Aspect | Email | Chat SignalR |
|--------|-------|--------------|
| **Temps de réponse** | Minutes/Heures | Instantané (< 1s) |
| **Interactivité** | Faible | Haute |
| **Notifications** | Email | Push temps réel |
| **Historique** | Dispersé | Centralisé |
| **UX** | Lourd | Fluide (comme WhatsApp) |

---

## 🎯 CAS D'USAGE CONCRETS

### 1. **Parent → Admin**
```
Parent: "Mon enfant sera absent demain"
Admin: "Merci de nous prévenir. Quelle est la raison ?"
Parent: "Rendez-vous médical"
Admin: "D'accord, c'est noté. Bonne journée !"
```

### 2. **Admin → Tous les Parents d'une Classe**
```
Admin crée un broadcast pour "Classe 5ème A"
Message: "Réunion parents-profs samedi 10h"
Tous les parents de 5ème A reçoivent instantanément
```

### 3. **Support Technique**
```
Parent: "Je n'arrive pas à payer en ligne"
Support: "Je regarde ça. Quel message d'erreur avez-vous ?"
Parent: [Envoie capture d'écran]
Support: "Ah je vois, c'est un problème de carte. Essayez avec..."
```

---

## ✅ RÉSUMÉ

### Ce qu'il faut pour implémenter le chat :

**Backend** :
- ✅ Créer les tables `Conversations` et `Messages`
- ✅ Créer le `ChatHub.cs`
- ✅ Créer le `ConversationController.cs`
- ✅ Créer les services `ConversationService` et `MessageService`
- ✅ Mapper le hub : `app.MapHub<ChatHub>("/hubs/chat")`

**Frontend** :
- ✅ Créer le composable `useChat.js`
- ✅ Créer le composant `Chat.vue` 
- ✅ Se connecter au ChatHub
- ✅ Implémenter l'UI (liste conversations + zone messages + input)

**Avantages** :
- 💬 Communication instantanée comme WhatsApp
- 🔔 Notifications en temps réel
- ✓✓ Indicateurs de lecture
- ✍️ Indicateur "en train d'écrire"
- 📝 Historique sauvegardé
- 📱 Support mobile et web

---

**C'est exactement comme WhatsApp, mais intégré dans votre plateforme scolaire !** 🎓💬

Des questions sur l'implémentation ? Besoin d'exemples supplémentaires ? 😊

