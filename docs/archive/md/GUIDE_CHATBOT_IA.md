# 🤖 GUIDE COMPLET : CHATBOT IA AVEC SIGNALR

**Date** : 1 novembre 2025  
**Contexte** : KelasiNaBisoAPI  
**Objectif** : Intégrer un chatbot intelligent pour répondre automatiquement

---

## 🎯 QU'EST-CE QU'UN CHATBOT ?

Un **assistant virtuel intelligent** qui répond automatiquement aux questions fréquentes des parents, **24h/24, 7j/7**, avant de transférer vers un humain si nécessaire.

### Analogie : Votre Réceptionniste Virtuel

```
Parent (23h) → "Quels sont les frais de scolarité pour la 5ème ?"
Chatbot     → "Les frais de scolarité pour la 5ème sont de 150 000 CDF
               par trimestre. Voulez-vous voir le détail ?"
Parent      → "Oui"
Chatbot     → "Voici le détail :
               - Minerval : 100 000 CDF
               - Fournitures : 30 000 CDF
               - Activités : 20 000 CDF
               Puis-je vous aider avec autre chose ?"
```

**Avantage** : Réponse **instantanée**, même la nuit ! Sans mobiliser un humain.

---

## 🏗️ ARCHITECTURE DU SYSTÈME

### Vue d'ensemble

```
┌─────────────────────────────────────────────────────────────┐
│                    UTILISATEUR (Parent)                      │
│  "Quels sont les horaires de l'école ?"                     │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│                    FRONTEND (Chat UI)                        │
│  - Interface de chat                                         │
│  - Connexion SignalR                                         │
└─────────────────────────────────────────────────────────────┘
                          ↓ SignalR
┌─────────────────────────────────────────────────────────────┐
│                    BACKEND (ChatHub)                         │
│  - Reçoit le message                                         │
│  - Détecte si c'est une question bot ou humain              │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│                  CHATBOT SERVICE                             │
│  ┌────────────────────────────────────────────────────┐     │
│  │  1. Analyse de l'intention (NLP)                   │     │
│  │     "horaires" → Intent: DEMANDE_HORAIRES          │     │
│  └────────────────────────────────────────────────────┘     │
│  ┌────────────────────────────────────────────────────┐     │
│  │  2. Base de connaissances                          │     │
│  │     - FAQ pré-enregistrées                         │     │
│  │     - Données dynamiques (DB)                      │     │
│  └────────────────────────────────────────────────────┘     │
│  ┌────────────────────────────────────────────────────┐     │
│  │  3. Génération de réponse                          │     │
│  │     - Template ou IA générative (GPT)              │     │
│  └────────────────────────────────────────────────────┘     │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│                RÉPONSE AU PARENT                             │
│  "L'école est ouverte de 7h30 à 15h00, du lundi au vendredi"│
└─────────────────────────────────────────────────────────────┘
```

---

## 🎨 TYPES DE CHATBOTS

### 1. **Chatbot Simple (Règles)**

**Fonctionnement** : Mots-clés → Réponses pré-écrites

```csharp
// Exemple simple avec mots-clés
if (message.Contains("frais") || message.Contains("scolarité"))
{
    return "Les frais de scolarité varient selon la classe. " +
           "Pour quelle classe voulez-vous des informations ?";
}
```

✅ **Avantages** : Simple, rapide, gratuit  
❌ **Inconvénients** : Limité, rigide

---

### 2. **Chatbot Intelligent (NLP + Base de Connaissances)**

**Fonctionnement** : Analyse sémantique → Recherche dans FAQ → Réponse contextuelle

```csharp
// Exemple avec analyse d'intention
var intent = await _nlpService.DetectIntent(message);

switch (intent)
{
    case Intent.FRAIS_SCOLARITE:
        return await GenerateFraisResponse(context);
    case Intent.HORAIRES:
        return await GenerateHorairesResponse();
    case Intent.INSCRIPTION:
        return await GenerateInscriptionResponse();
    default:
        return "Je n'ai pas compris. Voulez-vous parler à un conseiller ?";
}
```

✅ **Avantages** : Comprend les variations, contextuel  
❌ **Inconvénients** : Plus complexe

---

### 3. **Chatbot IA Générative (GPT/Claude)**

**Fonctionnement** : Contexte + Base de données → IA génère réponse personnalisée

```csharp
// Exemple avec OpenAI GPT
var prompt = $@"
Tu es un assistant pour l'école '{schoolName}'.
Base de connaissances : {knowledgeBase}
Historique de conversation : {conversationHistory}

Question du parent : {userMessage}

Réponds de manière claire et professionnelle.
";

var response = await _openAiService.GetCompletion(prompt);
return response;
```

✅ **Avantages** : Très intelligent, naturel, flexible  
❌ **Inconvénients** : Coût API, nécessite monitoring

---

## 🔧 IMPLÉMENTATION - CHATBOT SIMPLE (MVP)

### 1. Service ChatbotService.cs

```csharp
using System.Text.RegularExpressions;

namespace KelasiNaBisoAPI.Services
{
    public interface IChatbotService
    {
        Task<ChatbotResponse> ProcessMessageAsync(string message, int userId, int? ecoleId);
        Task<bool> ShouldBotRespondAsync(string message);
    }

    public class ChatbotService : IChatbotService
    {
        private readonly ILogger<ChatbotService> _logger;
        private readonly KelasiNaBisoDbContext _context;
        private readonly Dictionary<string, List<string>> _intentions;
        private readonly Dictionary<string, string> _responses;

        public ChatbotService(
            ILogger<ChatbotService> logger,
            KelasiNaBisoDbContext context)
        {
            _logger = logger;
            _context = context;
            
            // Initialiser les intentions (mots-clés)
            _intentions = new Dictionary<string, List<string>>
            {
                { "SALUTATION", new List<string> { "bonjour", "salut", "hello", "bonsoir" } },
                { "FRAIS", new List<string> { "frais", "scolarité", "payer", "paiement", "coût", "prix" } },
                { "HORAIRES", new List<string> { "horaire", "heure", "ouverture", "fermeture" } },
                { "INSCRIPTION", new List<string> { "inscription", "inscrire", "admission", "nouveau élève" } },
                { "PROGRAMME", new List<string> { "programme", "cours", "matières", "enseignement" } },
                { "TRANSPORT", new List<string> { "transport", "bus", "ramassage" } },
                { "CANTINE", new List<string> { "cantine", "repas", "déjeuner", "restaurant" } },
                { "ABSENCE", new List<string> { "absence", "absent", "malade", "justificatif" } },
                { "BULLETIN", new List<string> { "bulletin", "notes", "résultats", "moyenne" } },
                { "AIDE", new List<string> { "aide", "aider", "help", "assistance" } }
            };
            
            // Réponses par défaut
            _responses = new Dictionary<string, string>
            {
                { "SALUTATION", "Bonjour ! 👋 Je suis l'assistant virtuel de l'école. Comment puis-je vous aider aujourd'hui ?" },
                { "HORAIRES", "🕐 L'école est ouverte de **7h30 à 15h00**, du lundi au vendredi.\n\nSouhaitez-vous des informations sur autre chose ?" },
                { "AIDE", "Je peux vous aider avec :\n• 💰 Frais de scolarité\n• 🕐 Horaires\n• 📝 Inscriptions\n• 📚 Programme scolaire\n• 🚌 Transport\n• 🍽️ Cantine\n\nQue voulez-vous savoir ?" }
            };
        }

        /// <summary>
        /// Détecter si le bot doit répondre (question simple vs complexe)
        /// </summary>
        public async Task<bool> ShouldBotRespondAsync(string message)
        {
            var normalizedMessage = message.ToLower().Trim();
            
            // Le bot répond si :
            // 1. C'est une salutation
            if (_intentions["SALUTATION"].Any(k => normalizedMessage.Contains(k)))
                return true;
            
            // 2. C'est une demande d'aide
            if (_intentions["AIDE"].Any(k => normalizedMessage.Contains(k)))
                return true;
            
            // 3. C'est une question sur les sujets connus
            foreach (var intent in _intentions)
            {
                if (intent.Value.Any(keyword => normalizedMessage.Contains(keyword)))
                    return true;
            }
            
            // 4. Message court (< 10 mots) = probablement simple
            if (normalizedMessage.Split(' ').Length < 10)
                return true;
            
            // Sinon, transférer vers un humain
            return false;
        }

        /// <summary>
        /// Traiter le message et générer une réponse
        /// </summary>
        public async Task<ChatbotResponse> ProcessMessageAsync(string message, int userId, int? ecoleId)
        {
            var normalizedMessage = message.ToLower().Trim();
            
            // Détecter l'intention
            var intent = DetectIntent(normalizedMessage);
            
            _logger.LogInformation($"🤖 Chatbot - Intent détecté : {intent} pour message : '{message}'");
            
            // Générer la réponse selon l'intention
            var response = await GenerateResponseAsync(intent, userId, ecoleId);
            
            return new ChatbotResponse
            {
                Message = response,
                Intent = intent,
                ShouldTransferToHuman = intent == "UNKNOWN" || intent == "COMPLEX",
                SuggestedActions = GetSuggestedActions(intent)
            };
        }

        /// <summary>
        /// Détecter l'intention du message
        /// </summary>
        private string DetectIntent(string message)
        {
            // Vérifier chaque intention
            foreach (var intent in _intentions)
            {
                if (intent.Value.Any(keyword => message.Contains(keyword)))
                {
                    return intent.Key;
                }
            }
            
            // Si pas trouvé
            return "UNKNOWN";
        }

        /// <summary>
        /// Générer la réponse selon l'intention
        /// </summary>
        private async Task<string> GenerateResponseAsync(string intent, int userId, int? ecoleId)
        {
            switch (intent)
            {
                case "SALUTATION":
                    return _responses["SALUTATION"];
                
                case "HORAIRES":
                    return _responses["HORAIRES"];
                
                case "AIDE":
                    return _responses["AIDE"];
                
                case "FRAIS":
                    return await GenerateFraisResponseAsync(userId, ecoleId);
                
                case "INSCRIPTION":
                    return await GenerateInscriptionResponseAsync(ecoleId);
                
                case "PROGRAMME":
                    return await GenerateProgrammeResponseAsync(ecoleId);
                
                case "TRANSPORT":
                    return "🚌 Nous proposons un service de transport scolaire.\n\n" +
                           "Pour plus d'informations (itinéraires, tarifs), veuillez contacter le secrétariat au 📞 +243 XXX XXX XXX.\n\n" +
                           "Puis-je vous aider avec autre chose ?";
                
                case "CANTINE":
                    return "🍽️ La cantine scolaire est ouverte tous les jours de 12h00 à 13h00.\n\n" +
                           "Menu équilibré et varié. Tarif : 5 000 CDF/jour ou 80 000 CDF/mois.\n\n" +
                           "Souhaitez-vous des informations supplémentaires ?";
                
                case "ABSENCE":
                    return "📋 Pour signaler une absence :\n" +
                           "1. Prévenez-nous avant 8h00\n" +
                           "2. Fournissez un justificatif (certificat médical si maladie)\n" +
                           "3. Remettez le justificatif au retour de l'élève\n\n" +
                           "Voulez-vous signaler une absence maintenant ?";
                
                case "BULLETIN":
                    return await GenerateBulletinResponseAsync(userId);
                
                default:
                    return "🤔 Je n'ai pas bien compris votre question.\n\n" +
                           "Voulez-vous parler à un conseiller humain ? " +
                           "Tapez 'Oui' ou choisissez un sujet :\n" +
                           "• Frais\n• Horaires\n• Inscription\n• Programme";
            }
        }

        /// <summary>
        /// Générer réponse sur les frais (dynamique depuis DB)
        /// </summary>
        private async Task<string> GenerateFraisResponseAsync(int userId, int? ecoleId)
        {
            if (!ecoleId.HasValue)
            {
                return "💰 Pour connaître les frais de scolarité, veuillez d'abord vous connecter ou me préciser la classe qui vous intéresse.";
            }
            
            // Récupérer les types de frais de l'école
            var typesFrais = await _context.TypeFrais
                .Where(tf => tf.IdEcole == ecoleId.Value && tf.Statut == true)
                .ToListAsync();
            
            if (!typesFrais.Any())
            {
                return "💰 Les informations sur les frais ne sont pas encore disponibles. " +
                       "Veuillez contacter le secrétariat pour plus de détails.";
            }
            
            var response = "💰 **Frais de scolarité** :\n\n";
            
            foreach (var typeFrais in typesFrais.OrderBy(tf => tf.NomTypeFrais))
            {
                response += $"• **{typeFrais.NomTypeFrais}** : {typeFrais.MontantBase:N0} {typeFrais.Devise ?? "CDF"}\n";
            }
            
            response += "\n📌 Ces montants peuvent varier selon la classe.\n";
            response += "Pour une information personnalisée, voulez-vous parler à un conseiller ?";
            
            return response;
        }

        /// <summary>
        /// Générer réponse sur l'inscription
        /// </summary>
        private async Task<string> GenerateInscriptionResponseAsync(int? ecoleId)
        {
            return "📝 **Inscription d'un nouvel élève** :\n\n" +
                   "**Documents requis** :\n" +
                   "• Acte de naissance\n" +
                   "• Bulletin de l'année précédente\n" +
                   "• Certificat médical\n" +
                   "• 2 photos d'identité\n" +
                   "• Photocopie carte d'identité du parent\n\n" +
                   "**Procédure** :\n" +
                   "1. Remplir le formulaire d'inscription (disponible au secrétariat)\n" +
                   "2. Déposer les documents\n" +
                   "3. Payer les frais d'inscription\n\n" +
                   "Souhaitez-vous prendre rendez-vous avec le secrétariat ?";
        }

        /// <summary>
        /// Générer réponse sur le programme
        /// </summary>
        private async Task<string> GenerateProgrammeResponseAsync(int? ecoleId)
        {
            if (!ecoleId.HasValue)
            {
                return "📚 Pour connaître le programme scolaire, veuillez me préciser la classe qui vous intéresse.";
            }
            
            // Récupérer les cours disponibles
            var cours = await _context.Cours
                .Include(c => c.Classe)
                .Where(c => c.Classe.IdEcole == ecoleId.Value)
                .Select(c => c.NomCours)
                .Distinct()
                .ToListAsync();
            
            if (!cours.Any())
            {
                return "📚 Les informations sur le programme ne sont pas encore disponibles. " +
                       "Veuillez contacter le secrétariat.";
            }
            
            var response = "📚 **Programme scolaire** :\n\n";
            response += "Nos cours incluent :\n";
            
            foreach (var nomCours in cours.OrderBy(c => c).Take(10))
            {
                response += $"• {nomCours}\n";
            }
            
            response += "\nPour le programme détaillé d'une classe spécifique, précisez la classe.";
            
            return response;
        }

        /// <summary>
        /// Générer réponse sur le bulletin
        /// </summary>
        private async Task<string> GenerateBulletinResponseAsync(int userId)
        {
            // Vérifier si l'utilisateur a des enfants
            var utilisateur = await _context.Utilisateurs
                .Include(u => u.Tuteur)
                    .ThenInclude(t => t.Eleves)
                .FirstOrDefaultAsync(u => u.IdUtilisateur == userId);
            
            if (utilisateur?.Tuteur == null || !utilisateur.Tuteur.Eleves.Any())
            {
                return "📊 Pour consulter les bulletins, vous devez être connecté en tant que parent.\n\n" +
                       "Souhaitez-vous vous connecter ?";
            }
            
            var nbEleves = utilisateur.Tuteur.Eleves.Count;
            var elevesNoms = string.Join(", ", utilisateur.Tuteur.Eleves.Take(3).Select(e => e.Prenom));
            
            return $"📊 **Bulletins scolaires** :\n\n" +
                   $"Vous avez {nbEleves} enfant(s) inscrit(s) ({elevesNoms}{(nbEleves > 3 ? "..." : "")}).\n\n" +
                   "Les bulletins sont consultables dans votre espace parent, section 'Résultats scolaires'.\n\n" +
                   "Voulez-vous que je vous explique comment y accéder ?";
        }

        /// <summary>
        /// Obtenir les actions suggérées (boutons rapides)
        /// </summary>
        private List<string> GetSuggestedActions(string intent)
        {
            switch (intent)
            {
                case "FRAIS":
                    return new List<string> { "Voir le détail", "Modes de paiement", "Parler à un conseiller" };
                
                case "INSCRIPTION":
                    return new List<string> { "Prendre rendez-vous", "Télécharger le formulaire", "Parler à un conseiller" };
                
                case "ABSENCE":
                    return new List<string> { "Oui, signaler une absence", "Non, merci", "Parler à un conseiller" };
                
                case "UNKNOWN":
                    return new List<string> { "Parler à un conseiller", "Voir les sujets d'aide" };
                
                default:
                    return new List<string> { "Frais", "Horaires", "Inscription", "Programme" };
            }
        }
    }

    /// <summary>
    /// Réponse du chatbot
    /// </summary>
    public class ChatbotResponse
    {
        public string Message { get; set; }
        public string Intent { get; set; }
        public bool ShouldTransferToHuman { get; set; }
        public List<string> SuggestedActions { get; set; }
    }
}
```

### 2. Intégration dans ChatHub.cs

```csharp
public class ChatHub : Hub
{
    private readonly IChatbotService _chatbotService;
    private readonly IMessageService _messageService;
    
    public ChatHub(
        IChatbotService chatbotService,
        IMessageService messageService)
    {
        _chatbotService = chatbotService;
        _messageService = messageService;
    }

    /// <summary>
    /// Envoyer un message (avec support chatbot)
    /// </summary>
    public async Task SendMessage(int conversationId, string contenu)
    {
        var userId = int.Parse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        
        try {
            // Sauvegarder le message de l'utilisateur
            var userMessage = await _messageService.CreateAsync(new Message
            {
                IdConversation = conversationId,
                IdExpediteur = userId,
                Contenu = contenu,
                DateEnvoi = DateTime.UtcNow
            });
            
            // Envoyer à tous les participants
            await Clients.Group($"conversation_{conversationId}")
                .SendAsync("ReceiveMessage", MapToDto(userMessage, userName));
            
            // ✨ NOUVEAU : Vérifier si le bot doit répondre
            var shouldBotRespond = await _chatbotService.ShouldBotRespondAsync(contenu);
            
            if (shouldBotRespond)
            {
                // Récupérer l'école de l'utilisateur
                var user = await _context.Utilisateurs.FindAsync(userId);
                
                // Générer la réponse du bot
                var botResponse = await _chatbotService.ProcessMessageAsync(contenu, userId, user?.IdEcole);
                
                // Attendre un peu (simuler "en train d'écrire")
                await Task.Delay(500);
                
                // Sauvegarder la réponse du bot
                var botMessage = await _messageService.CreateAsync(new Message
                {
                    IdConversation = conversationId,
                    IdExpediteur = 0, // 0 = Bot
                    Contenu = botResponse.Message,
                    DateEnvoi = DateTime.UtcNow
                });
                
                // Envoyer la réponse du bot
                await Clients.Group($"conversation_{conversationId}")
                    .SendAsync("ReceiveBotMessage", new
                    {
                        idMessage = botMessage.IdMessage,
                        idConversation = conversationId,
                        contenu = botResponse.Message,
                        dateEnvoi = botMessage.DateEnvoi,
                        suggestedActions = botResponse.SuggestedActions,
                        shouldTransferToHuman = botResponse.ShouldTransferToHuman
                    });
                
                // Si le bot ne peut pas répondre, notifier un admin
                if (botResponse.ShouldTransferToHuman)
                {
                    await NotifyAdminsForHelp(conversationId, userId, userName, contenu);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SendMessage");
            await Clients.Caller.SendAsync("MessageError", new { message = "Erreur lors de l'envoi" });
        }
    }
    
    /// <summary>
    /// Notifier les admins qu'un parent a besoin d'aide
    /// </summary>
    private async Task NotifyAdminsForHelp(int conversationId, int userId, string userName, string message)
    {
        // Récupérer les admins de l'école
        var user = await _context.Utilisateurs.FindAsync(userId);
        var admins = await _context.Utilisateurs
            .Where(u => u.IdEcole == user.IdEcole && (u.Role.Nom == "Admin" || u.Role.Nom == "Super-Admin"))
            .ToListAsync();
        
        // Notifier chaque admin
        foreach (var admin in admins)
        {
            await Clients.Group($"user_{admin.IdUtilisateur}")
                .SendAsync("NewHelpRequest", new
                {
                    conversationId = conversationId,
                    userName = userName,
                    messagePreview = message.Length > 50 ? message.Substring(0, 50) + "..." : message,
                    timestamp = DateTime.UtcNow
                });
        }
    }
}
```

### 3. Configuration dans Program.cs

```csharp
// Enregistrer le service chatbot
builder.Services.AddScoped<IChatbotService, ChatbotService>();
```

---

## 🎨 INTERFACE UTILISATEUR FRONTEND

### Affichage des Messages Bot

```vue
<!-- components/ChatMessage.vue -->
<template>
  <div :class="['message', { mine: message.isMine, bot: message.isBot }]">
    <div v-if="message.isBot" class="bot-avatar">
      🤖
    </div>
    <div v-else-if="!message.isMine" class="message-avatar">
      {{ message.senderName?.[0] || '?' }}
    </div>
    
    <div class="message-bubble">
      <div class="message-content" v-html="formatMarkdown(message.content)"></div>
      
      <!-- Actions suggérées (boutons rapides) -->
      <div v-if="message.suggestedActions && message.suggestedActions.length" class="suggested-actions">
        <button 
          v-for="action in message.suggestedActions"
          :key="action"
          @click="handleSuggestedAction(action)"
          class="btn-action"
        >
          {{ action }}
        </button>
      </div>
      
      <!-- Badge "Transférer vers humain" -->
      <div v-if="message.shouldTransferToHuman" class="transfer-notice">
        ℹ️ Un conseiller va bientôt vous répondre...
      </div>
      
      <div class="message-meta">
        <span class="message-time">{{ formatTime(message.timestamp) }}</span>
      </div>
    </div>
  </div>
</template>

<script setup>
import { defineProps, defineEmits } from 'vue';

const props = defineProps({
  message: Object
});

const emit = defineEmits(['suggestedAction']);

function handleSuggestedAction(action) {
  emit('suggestedAction', action);
}

function formatMarkdown(text) {
  // Convertir markdown simple en HTML
  return text
    .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>') // **gras**
    .replace(/\n/g, '<br>'); // Sauts de ligne
}

function formatTime(date) {
  // ... format time
}
</script>

<style scoped>
.message.bot .message-bubble {
  background: #f0f0f0;
  border-left: 3px solid #007bff;
}

.bot-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 24px;
}

.suggested-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-top: 12px;
}

.btn-action {
  padding: 8px 16px;
  background: white;
  border: 1px solid #007bff;
  color: #007bff;
  border-radius: 20px;
  cursor: pointer;
  font-size: 14px;
  transition: all 0.2s;
}

.btn-action:hover {
  background: #007bff;
  color: white;
}

.transfer-notice {
  margin-top: 8px;
  padding: 8px 12px;
  background: #fff3cd;
  border-radius: 8px;
  font-size: 13px;
  color: #856404;
}
</style>
```

---

## 🚀 ÉVOLUTION FUTURE : CHATBOT IA (GPT)

### Intégration OpenAI GPT

```csharp
public class OpenAIChatbotService : IChatbotService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly KelasiNaBisoDbContext _context;
    
    public async Task<ChatbotResponse> ProcessMessageAsync(string message, int userId, int? ecoleId)
    {
        // 1. Récupérer le contexte de l'école
        var knowledgeBase = await BuildKnowledgeBaseAsync(ecoleId);
        
        // 2. Récupérer l'historique de conversation
        var conversationHistory = await GetConversationHistoryAsync(userId);
        
        // 3. Construire le prompt
        var prompt = $@"
Tu es un assistant virtuel pour l'école '{knowledgeBase.SchoolName}'.

INFORMATIONS SUR L'ÉCOLE :
{knowledgeBase.SchoolInfo}

FRAIS DE SCOLARITÉ :
{knowledgeBase.Fees}

HORAIRES :
{knowledgeBase.Schedule}

HISTORIQUE DE CONVERSATION :
{conversationHistory}

QUESTION DU PARENT : {message}

INSTRUCTIONS :
- Réponds de manière claire, professionnelle et amicale
- Utilise des emojis appropriés
- Si tu ne sais pas, propose de transférer vers un humain
- Reste dans le contexte de l'école
";
        
        // 4. Appeler l'API OpenAI
        var response = await CallOpenAIAsync(prompt);
        
        return new ChatbotResponse
        {
            Message = response,
            Intent = "AI_GENERATED",
            ShouldTransferToHuman = response.Contains("[TRANSFER_TO_HUMAN]"),
            SuggestedActions = ExtractSuggestedActions(response)
        };
    }
    
    private async Task<string> CallOpenAIAsync(string prompt)
    {
        var requestBody = new
        {
            model = "gpt-4",
            messages = new[]
            {
                new { role = "system", content = "Tu es un assistant virtuel pour une école." },
                new { role = "user", content = prompt }
            },
            temperature = 0.7,
            max_tokens = 500
        };
        
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
        {
            Headers = { { "Authorization", $"Bearer {_apiKey}" } },
            Content = JsonContent.Create(requestBody)
        };
        
        var response = await _httpClient.SendAsync(request);
        var result = await response.Content.ReadFromJsonAsync<OpenAIResponse>();
        
        return result.Choices[0].Message.Content;
    }
}
```

---

## 📊 AVANTAGES DU CHATBOT

| Aspect | Sans Chatbot | Avec Chatbot |
|--------|--------------|--------------|
| **Disponibilité** | 8h-17h (bureau) | 24h/24, 7j/7 |
| **Temps de réponse** | Minutes/Heures | < 1 seconde |
| **Questions FAQ** | Mobilise un humain | Automatique |
| **Coût** | Salaire agent | Faible (gratuit pour version simple) |
| **Scalabilité** | Limité | Illimité |

### Exemple d'économies

```
École avec 500 parents :
- 50% posent des questions simples (FAQ)
- 250 questions/semaine
- Temps humain : 5 min/question

Sans bot : 250 × 5 = 1250 min = 20h/semaine
Avec bot : 80% résolues auto = 4h/semaine seulement

ÉCONOMIE : 16 heures/semaine = 2 jours de travail !
```

---

## 🎯 PLAN D'IMPLÉMENTATION

### Phase 1 : Chatbot Simple (MVP) - 1 semaine
✅ Règles basées sur mots-clés  
✅ Réponses pré-écrites  
✅ FAQ dynamique depuis DB  
✅ Transfert vers humain

### Phase 2 : Chatbot Intelligent - 2 semaines
⚠️ Analyse d'intention (NLP)  
⚠️ Base de connaissances enrichie  
⚠️ Contexte de conversation  
⚠️ Suggestions d'actions

### Phase 3 : Chatbot IA (GPT) - 3 semaines
🔮 Intégration OpenAI/Claude  
🔮 Réponses personnalisées  
🔮 Apprentissage continu  
🔮 Support multilingue

---

## ✅ RÉSUMÉ

### Ce qu'apporte un Chatbot :

1. **Disponibilité 24/7** → Parents peuvent poser questions la nuit
2. **Réponses instantanées** → Pas d'attente
3. **Automatisation FAQ** → 80% questions simples résolues auto
4. **Économie de temps** → Humains se concentrent sur cas complexes
5. **Meilleure expérience** → Parents satisfaits

### Exemple concret :

```
23h00 - Parent : "Combien coûte l'inscription en 5ème ?"
23h00 - Bot : "Les frais d'inscription en 5ème sont de 50 000 CDF. 
               Voulez-vous voir le détail ?"
23h00 - Parent : "Oui"
23h00 - Bot : "Voici le détail :
               - Inscription : 30 000 CDF
               - Fournitures : 15 000 CDF
               - Assurance : 5 000 CDF"
```

**Sans bot** : Parent aurait dû attendre le lendemain matin !

---

**Le chatbot est l'évolution naturelle de votre système de chat.** 🤖✨  
**Commencez simple (Phase 1), puis évoluez vers l'IA !**

Des questions sur l'implémentation ? 😊

