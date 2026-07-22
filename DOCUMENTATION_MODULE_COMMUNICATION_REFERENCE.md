# 📚 Documentation Complète - Module de Communication
## Guide de Référence pour Implémentation

**Version** : 1.0  
**Date** : Décembre 2024  
**Projet Source** : KelasiNaBisoAPI  
**Framework** : ASP.NET Core 6.0

---

## 📋 Table des matières

1. [Vue d'ensemble](#vue-densemble)
2. [Architecture du module](#architecture-du-module)
3. [Modèles de données](#modèles-de-données)
4. [Services et interfaces](#services-et-interfaces)
5. [Contrôleurs et endpoints API](#contrôleurs-et-endpoints-api)
6. [SignalR - Communication temps réel](#signalr---communication-temps-réel)
7. [Notifications multi-canal](#notifications-multi-canal)
8. [Campagnes de communication](#campagnes-de-communication)
9. [Messagerie](#messagerie)
10. [Configuration et dépendances](#configuration-et-dépendances)
11. [Guide d'implémentation](#guide-dimplémentation)
12. [Exemples de code](#exemples-de-code)
13. [Bonnes pratiques](#bonnes-pratiques)

---

## 🌟 Vue d'ensemble

Le module de communication de KelasiNaBiso offre un système complet de communication multi-canal permettant :

- **Messagerie instantanée** : Messages privés entre utilisateurs et messages de groupe
- **Campagnes de communication** : Envoi en masse avec segmentation avancée
- **Notifications en temps réel** : Via SignalR pour une expérience utilisateur fluide
- **Multi-canal** : Push Firebase, Email, SMS (Twilio), In-App
- **Planification** : Envoi différé et rappels automatiques
- **Tracking** : Suivi des envois, statistiques et historique complet

### Fonctionnalités principales

| Fonctionnalité | Description |
|----------------|-------------|
| **Messagerie** | Messages privés et groupes avec notifications automatiques |
| **Campagnes** | Création, validation, envoi et suivi de campagnes |
| **Segmentation** | Ciblage par classe, direction, utilisateur, tuteur, niveau |
| **Multi-canal** | Push, Email, SMS, In-App avec fallback automatique |
| **Temps réel** | SignalR pour notifications instantanées |
| **Planification** | Envoi différé avec rappels automatiques |
| **Historique** | Traçabilité complète des actions et envois |

---

## 🏗️ Architecture du module

### Diagramme d'architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    MODULE DE COMMUNICATION                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌──────────────────┐      ┌──────────────────────────┐         │
│  │   MESSAGERIE     │      │  CAMPAGNES COMMUNICATION │         │
│  ├──────────────────┤      ├──────────────────────────┤         │
│  │ • Message        │      │ • CommunicationCampaign  │         │
│  │ • GroupeMessage  │      │ • CommunicationSegment    │         │
│  │ • Notifications  │      │ • CampaignRecipient      │         │
│  │   Push/SMS       │      │ • CommunicationHistory   │         │
│  └──────────────────┘      └──────────────────────────┘         │
│                                                                   │
│  ┌──────────────────────────────────────────────────────┐       │
│  │          SIGNALR - TEMPS RÉEL                        │       │
│  ├──────────────────────────────────────────────────────┤       │
│  │ • NotificationHub (notifications)                    │       │
│  │ • DashboardHub (mises à jour dashboard)             │       │
│  └──────────────────────────────────────────────────────┘       │
│                                                                   │
│  ┌──────────────────────────────────────────────────────┐       │
│  │       SERVICES DE NOTIFICATION MULTI-CANAL           │       │
│  ├──────────────────────────────────────────────────────┤       │
│  │ • FirebaseNotificationService (Push)                 │       │
│  │ • SmsNotificationService (Twilio)                    │       │
│  │ • EmailService (SMTP)                                 │       │
│  │ • SignalRNotificationService (In-App)                 │       │
│  └──────────────────────────────────────────────────────┘       │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

### Flux de communication

```
Utilisateur A                    Serveur                    Utilisateur B
     
     │─── Envoie Message ───────────>│                              │
     │                              │─── Notification Push ───────>│
     │                              │─── SMS Fallback (si échec) ─>│
     │                              │─── SignalR (temps réel) ────>│
     │                              │─── Email (si configuré) ────>│
     │                              │                              │
```

### Composants principaux

1. **Controllers** : Points d'entrée API REST
   - `MessageController` : Gestion des messages
   - `GroupeMessageController` : Gestion des groupes
   - `CommunicationController` : Gestion des campagnes

2. **Services** : Logique métier
   - `MessageService` : Gestion des messages avec notifications
   - `CommunicationCampaignService` : Gestion des campagnes
   - `CommunicationDispatchWorker` : Envoi asynchrone des campagnes

3. **Hubs SignalR** : Communication temps réel
   - `NotificationHub` : Notifications utilisateur
   - `DashboardHub` : Mises à jour dashboard

4. **Services de notification** : Multi-canal
   - `FirebaseNotificationService` : Push notifications
   - `SmsNotificationService` : SMS via Twilio
   - `EmailService` : Emails SMTP
   - `SignalRNotificationService` : Notifications in-app

---

## 📊 Modèles de données

### 1. Message

**Fichier** : `Models/Message.cs`

```csharp
public class Message
{
    [Key]
    public int IdMessage { get; set; }
    
    public int? IdExpediteur { get; set; }
    public int? IdDestinateur { get; set; }
    public int? IdGroupe { get; set; }
    
    [Required]
    [MaxLength(1000)]
    public string ContenuMessage { get; set; }
    
    [MaxLength(500)]
    public string FichierUrl { get; set; }
    
    public bool? Statut { get; set; } = true;
    
    [JsonIgnore]
    public DateTime DateEnvoi { get; set; }
    
    // Navigation
    public Utilisateur Expediteur { get; set; }
    public Utilisateur Destinateur { get; set; }
    public GroupeMessage GroupeMessage { get; set; }
}
```

**Relations** :
- Un message peut être privé (`IdDestinateur`) ou de groupe (`IdGroupe`)
- Un message appartient à un expéditeur (`IdExpediteur`)
- Soft delete via `Statut` (true = actif, false = supprimé)

### 2. GroupeMessage

**Fichier** : `Models/GroupeMessage.cs`

```csharp
public class GroupeMessage
{
    [Key]
    public int IdGroupe { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string NomGroupe { get; set; }
    
    public int? CreePar { get; set; }
    [Required]
    public int IdEcole { get; set; }
    
    public bool? Statut { get; set; } = true;
    
    [JsonIgnore]
    public DateTime DateCreation { get; set; }
    
    // Navigation
    public Utilisateur Utilisateur { get; set; }
    public Ecole Ecole { get; set; }
    public ICollection<Message> Messages { get; set; }
}
```

### 3. CommunicationCampaign

**Fichier** : `Models/CommunicationCampaign.cs`

```csharp
public class CommunicationCampaign
{
    [Key]
    public int IdCampaign { get; set; }
    
    [Required]
    public int IdEcole { get; set; }
    [Required]
    public int IdAuteur { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Titre { get; set; }
    
    [Required]
    public string ContenuMarkdown { get; set; }
    
    [Required]
    [MaxLength(30)]
    public string Statut { get; set; } = "Brouillon"; // Brouillon, Valide, EnCours, Envoye, Annule, Partiel
    
    [Required]
    [MaxLength(20)]
    public string Importance { get; set; } = "Info"; // Info, Important, Urgent
    
    [Required]
    public string ChannelsJson { get; set; } = "{\"push\":true,\"email\":true,\"sms\":false,\"inApp\":true}";
    
    [Required]
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    
    public DateTime? PlanifiedAt { get; set; }
    public DateTime? ExpirationAt { get; set; }
    public bool RappelAuto { get; set; }
    
    public DateTime? ValidationAt { get; set; }
    public int? ValidatedBy { get; set; }
    
    // Navigation
    public Ecole Ecole { get; set; }
    public Utilisateur Auteur { get; set; }
    public Utilisateur Validateur { get; set; }
    public ICollection<CommunicationSegment> Segments { get; set; }
    public ICollection<CampaignRecipient> Destinataires { get; set; }
    public ICollection<CommunicationHistory> Historique { get; set; }
    public ICollection<Notification> Notifications { get; set; }
}
```

**Statuts de campagne** :
- `Brouillon` : En cours de création
- `Valide` : Validée, prête à être envoyée
- `EnCours` : En cours d'envoi
- `Envoye` : Envoi terminé avec succès
- `Partiel` : Envoi partiel (certains échecs)
- `Annule` : Annulée avant envoi

### 4. CommunicationSegment

**Fichier** : `Models/CommunicationSegment.cs`

```csharp
public class CommunicationSegment
{
    [Key]
    public int IdSegment { get; set; }
    
    public int? IdCampaign { get; set; }
    public int? IdEcole { get; set; }
    
    [Required]
    [MaxLength(150)]
    public string NomSegment { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string TypeSegment { get; set; } // "Parent", "Enseignant", "Eleve", etc.
    
    [Required]
    public string CriteriaJson { get; set; } = "{}"; // Critères de segmentation
    
    [Required]
    public bool IsReusable { get; set; }
    
    [Required]
    public int CreatedBy { get; set; }
    
    [Required]
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public CommunicationCampaign Campaign { get; set; }
    public Ecole Ecole { get; set; }
    public Utilisateur Createur { get; set; }
}
```

**Critères de segmentation** (`CriteriaJson`) :
```json
{
  "ClasseIds": [1, 2, 3],
  "DirectionIds": [5],
  "UtilisateurIds": [10, 20],
  "TuteurIds": [30, 40],
  "Niveau": "Primaire",
  "Tags": ["urgent", "important"]
}
```

### 5. CampaignRecipient

**Fichier** : `Models/CampaignRecipient.cs`

```csharp
public class CampaignRecipient
{
    [Key]
    public long IdRecipient { get; set; }
    
    [Required]
    public int IdCampaign { get; set; }
    
    [Required]
    public int IdUtilisateur { get; set; }
    
    public int? IdTuteur { get; set; }
    public int? IdEleve { get; set; }
    public int? IdNotification { get; set; }
    
    [MaxLength(20)]
    public string? PreferredChannel { get; set; } // Push, Email, Sms, InApp
    
    [MaxLength(20)]
    public string? FinalChannel { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Planifie"; // Planifie, Envoye, Echec
    
    [Required]
    public DateTime DatePlanifie { get; set; } = DateTime.UtcNow;
    
    public DateTime? DateEnvoi { get; set; }
    
    [MaxLength(500)]
    public string? ErrorMessage { get; set; }
    
    // Navigation
    public CommunicationCampaign Campaign { get; set; }
    public Utilisateur Utilisateur { get; set; }
    public Tuteur Tuteur { get; set; }
    public Eleve Eleve { get; set; }
    public Notification Notification { get; set; }
}
```

### 6. CommunicationHistory

**Fichier** : `Models/CommunicationHistory.cs`

```csharp
public class CommunicationHistory
{
    [Key]
    public long IdHistory { get; set; }
    
    [Required]
    public int IdCampaign { get; set; }
    
    public int? UserId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Action { get; set; } // "Creation", "MiseAJour", "EnvoiDemarre", "Annulation"
    
    public string? DetailJson { get; set; }
    
    [Required]
    public DateTime DateAction { get; set; } = DateTime.UtcNow;
    
    [MaxLength(45)]
    public string? AdresseIP { get; set; }
    
    // Navigation
    public CommunicationCampaign Campaign { get; set; }
    public Utilisateur Utilisateur { get; set; }
}
```

### 7. Notification

**Fichier** : `Models/Notification.cs`

```csharp
public class Notification
{
    [Key]
    public int IdNotification { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Titre { get; set; }
    
    [Required]
    [MaxLength(1000)]
    public string Contenu { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string TypeNotification { get; set; } // "INFO", "WARNING", "ERROR", "SUCCESS"
    
    [Required]
    public bool EstLue { get; set; } = false;
    
    [Required]
    public DateTime DateCreation { get; set; } = DateTime.Now;
    
    public DateTime? DateLecture { get; set; }
    
    [MaxLength(100)]
    public string? LienAction { get; set; }
    
    [MaxLength(50)]
    public string? Icone { get; set; }
    
    // Relations
    public int? IdExpediteur { get; set; }
    public int? IdDestinataire { get; set; }
    public int? IdEcole { get; set; }
    public int? IdCampaign { get; set; }
    
    [MaxLength(20)]
    public string? CanalUtilise { get; set; } // Push, Email, Sms, InApp
    
    [Required]
    [MaxLength(20)]
    public string Priorite { get; set; } = "INFO";
    
    public string? PayloadJson { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string StatutEnvoi { get; set; } = "Envoye";
    
    [MaxLength(100)]
    public string? TrackingId { get; set; }
    
    // Navigation
    public Utilisateur Expediteur { get; set; }
    public Utilisateur Destinataire { get; set; }
    public Ecole Ecole { get; set; }
    public CommunicationCampaign Campaign { get; set; }
}
```

---

## 🔧 Services et interfaces

### 1. IMessageRepository

**Fichier** : `Services/Repositories/IMessageRepository.cs`

```csharp
public interface IMessageRepository
{
    Task<IEnumerable<Message>> GetAllAsync();
    Task<Message> GetByIdAsync(int id);
    Task<IEnumerable<Message>> GetByExpediteurAsync(int idExpediteur);
    Task<IEnumerable<Message>> GetByDestinateurAsync(int idDestinateur);
    Task<IEnumerable<Message>> GetByGroupeAsync(int idGroupe);
    Task<IEnumerable<Message>> GetConversationAsync(int idExpediteur, int idDestinateur);
    Task<Message> CreateAsync(Message message);
    Task<Message> UpdateAsync(Message message);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ToggleStatutAsync(int id);
}
```

**Implémentation** : `Services/MessageService.cs`

**Fonctionnalités clés** :
- Envoi automatique de notifications Push/SMS lors de la création d'un message privé
- Soft delete via `ToggleStatutAsync`
- Récupération des conversations complètes

### 2. ICommunicationCampaignService

**Fichier** : `Services/Repositories/ICommunicationCampaignService.cs`

```csharp
public interface ICommunicationCampaignService
{
    // CRUD
    Task<PagedResult<CommunicationCampaignSummaryDto>> GetCampaignsAsync(
        int currentUserId, string currentUserRole, int? currentUserEcoleId, 
        PagedRequest request, CancellationToken cancellationToken = default);
    
    Task<CommunicationCampaignDetailDto?> GetCampaignByIdAsync(
        int idCampaign, int currentUserId, string currentUserRole, 
        int? currentUserEcoleId, CancellationToken cancellationToken = default);
    
    Task<CommunicationCampaignDetailDto> CreateCampaignAsync(
        CreateCommunicationCampaignDto dto, int currentUserId, 
        string currentUserRole, int? currentUserEcoleId, 
        CancellationToken cancellationToken = default);
    
    Task<CommunicationCampaignDetailDto> UpdateCampaignAsync(
        int idCampaign, UpdateCommunicationCampaignDto dto, 
        int currentUserId, string currentUserRole, int? currentUserEcoleId, 
        CancellationToken cancellationToken = default);
    
    Task<bool> CancelCampaignAsync(
        int idCampaign, CancelCommunicationRequest? request, 
        int currentUserId, string currentUserRole, int? currentUserEcoleId, 
        CancellationToken cancellationToken = default);
    
    // Destinataires
    Task<int> RefreshRecipientsAsync(
        int idCampaign, int currentUserId, string currentUserRole, 
        int? currentUserEcoleId, CancellationToken cancellationToken = default);
    
    Task<PagedResult<CommunicationRecipientDto>> GetRecipientsAsync(
        int idCampaign, PagedRequest request, int currentUserId, 
        string currentUserRole, int? currentUserEcoleId, 
        CancellationToken cancellationToken = default);
    
    // Historique
    Task<PagedResult<CommunicationHistoryDto>> GetHistoryAsync(
        int idCampaign, PagedRequest request, int currentUserId, 
        string currentUserRole, int? currentUserEcoleId, 
        CancellationToken cancellationToken = default);
    
    // Envoi
    Task<bool> DispatchCampaignAsync(
        int idCampaign, int currentUserId, string currentUserRole, 
        int? currentUserEcoleId, CancellationToken cancellationToken = default);
}
```

**Implémentation** : `Services/CommunicationCampaignService.cs`

**Fonctionnalités clés** :
- Gestion des permissions par rôle (Super-Admin, Admin, Directeur, etc.)
- Segmentation avancée avec critères multiples
- Génération automatique des destinataires
- Respect des préférences de communication des utilisateurs
- Historique complet des actions

### 3. IFirebaseNotificationService

**Fichier** : `Services/Repositories/IFirebaseNotificationService.cs`

```csharp
public interface IFirebaseNotificationService
{
    Task<bool> EnvoyerNotificationAUtilisateurAsync(
        int idUtilisateur, string titre, string corps, 
        Dictionary<string, string>? donnees = null);
    
    Task<int> EnvoyerNotificationParRoleAsync(
        int idRole, string titre, string corps, 
        Dictionary<string, string>? donnees = null);
    
    Task<int> EnvoyerNotificationParEcoleAsync(
        int idEcole, string titre, string corps, 
        Dictionary<string, string>? donnees = null);
    
    Task<int> EnvoyerNotificationParClasseAsync(
        int idClasse, string titre, string corps, 
        Dictionary<string, string>? donnees = null);
    
    Task<bool> EnvoyerNotificationATokenAsync(
        string fcmToken, string titre, string corps, 
        Dictionary<string, string>? donnees = null);
    
    Task<bool> EnvoyerNotificationAvanceeAsync(
        string fcmToken, string titre, string corps, 
        string? imageUrl = null, string? clickAction = null, 
        Dictionary<string, string>? donnees = null, 
        string? sound = null, string? badge = null);
}
```

**Implémentation** : `Services/FirebaseNotificationService.cs`

**Fonctionnalités clés** :
- Envoi multicast (plusieurs devices)
- Gestion automatique des tokens invalides
- Support Android, iOS, Web
- Notifications avancées avec images, sons, badges

### 4. ISmsNotificationService

**Fichier** : `Services/Repositories/ISmsNotificationService.cs`

```csharp
public interface ISmsNotificationService
{
    // Envoi
    Task<SmsLog?> EnvoyerSmsAsync(
        string numeroTelephone, string message, 
        string? typeNotification = null);
    
    Task<SmsLog?> EnvoyerSmsAUtilisateurAsync(
        int idUtilisateur, string message, 
        string? typeNotification = null);
    
    Task<List<SmsLog>> EnvoyerSmsEnMasseAsync(
        List<string> numerosDestination, string message, 
        string? typeNotification = null);
    
    Task<List<SmsLog>> EnvoyerSmsParRoleAsync(
        string role, string message, 
        string? typeNotification = null);
    
    Task<List<SmsLog>> EnvoyerSmsParEcoleAsync(
        int idEcole, string message, 
        string? typeNotification = null);
    
    // Tracking
    Task<SmsLog?> VerifierStatutSmsAsync(string messageSid);
    Task<int> MettreAJourStatutsSmsEnAttenteAsync();
    
    // Rapports
    Task<PagedResult<SmsLog>> GetHistoriqueSmsAsync(
        PagedRequest request, string? statut = null, 
        string? typeNotification = null, int? idUtilisateur = null, 
        DateTime? dateDebut = null, DateTime? dateFin = null);
    
    Task<object> GetRapportCoutsSmsAsync(
        DateTime dateDebut, DateTime dateFin);
    
    Task<object> GetStatistiquesSmsAsync();
    
    // Utilitaires
    bool ValiderNumeroTelephone(string numeroTelephone);
    string FormaterNumeroTelephone(string numeroTelephone);
    int CalculerNombreSegments(string message);
}
```

**Implémentation** : `Services/SmsNotificationService.cs` (Twilio)

**Fonctionnalités clés** :
- Intégration Twilio
- Validation et formatage des numéros
- Calcul des coûts par segment
- Tracking des statuts d'envoi
- Rapports et statistiques

### 5. ICommunicationDispatchScheduler

**Fichier** : `Services/Repositories/ICommunicationDispatchScheduler.cs`

```csharp
public interface ICommunicationDispatchScheduler
{
    Task EnqueueDispatchAsync(
        int idCampaign, int userId, 
        CancellationToken cancellationToken = default);
}
```

**Implémentation** : `Services/CommunicationDispatchScheduler.cs`

**Fonctionnalités clés** :
- Envoi asynchrone des campagnes
- Gestion de la file d'attente
- Traitement en arrière-plan

### 6. ICommunicationDispatchWorker

**Fichier** : `Services/Repositories/ICommunicationDispatchWorker.cs`

```csharp
public interface ICommunicationDispatchWorker
{
    Task ProcessDispatchAsync(
        int idCampaign, int userId, 
        CancellationToken cancellationToken = default);
}
```

**Implémentation** : `Services/CommunicationDispatchWorker.cs`

**Fonctionnalités clés** :
- Traitement des destinataires par batch
- Envoi multi-canal avec fallback
- Mise à jour des statuts en temps réel
- Gestion des erreurs et retry

---

## 🌐 Contrôleurs et endpoints API

### 1. MessageController

**Fichier** : `Controllers/MessageController.cs`  
**Route de base** : `/api/Message`  
**Authentification** : JWT Bearer (requis)

#### Endpoints

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/api/Message` | Liste tous les messages actifs |
| GET | `/api/Message/{id}` | Détails d'un message |
| GET | `/api/Message/expediteur/{idExpediteur}` | Messages envoyés par un utilisateur |
| GET | `/api/Message/groupe/{idGroupe}` | Messages d'un groupe |
| GET | `/api/Message/exists/{id}` | Vérifier l'existence d'un message |
| POST | `/api/Message` | Créer un message (notifications auto) |
| PUT | `/api/Message/{id}` | Modifier un message |
| DELETE | `/api/Message/{id}` | Supprimer un message |
| PUT | `/api/Message/toggle-statut/{id}` | Activer/désactiver un message (soft delete) |

#### Exemple de requête POST

```json
{
  "idExpediteur": 1,
  "idDestinateur": 2,
  "contenuMessage": "Bonjour, comment allez-vous ?",
  "fichierUrl": null
}
```

#### Réponse

```json
{
  "idMessage": 42,
  "idExpediteur": 1,
  "idDestinateur": 2,
  "contenuMessage": "Bonjour, comment allez-vous ?",
  "fichierUrl": null,
  "statut": true,
  "dateEnvoi": "2024-12-15T10:30:00Z"
}
```

**Note** : Lors de la création d'un message privé, une notification Push est automatiquement envoyée au destinataire. Si la Push échoue, un SMS est envoyé en fallback.

### 2. GroupeMessageController

**Fichier** : `Controllers/GroupeMessageController.cs`  
**Route de base** : `/api/GroupeMessage`  
**Authentification** : JWT Bearer (requis)

#### Endpoints

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/api/GroupeMessage` | Liste tous les groupes |
| GET | `/api/GroupeMessage/{id}` | Détails d'un groupe |
| GET | `/api/GroupeMessage/ecole/{idEcole}` | Groupes d'une école |
| POST | `/api/GroupeMessage` | Créer un groupe |
| PUT | `/api/GroupeMessage/{id}` | Modifier un groupe |
| DELETE | `/api/GroupeMessage/{id}` | Supprimer un groupe |
| PUT | `/api/GroupeMessage/toggle-statut/{id}` | Activer/désactiver un groupe |

### 3. CommunicationController

**Fichier** : `Controllers/CommunicationController.cs`  
**Route de base** : `/api/Communication`  
**Authentification** : JWT Bearer  
**Rôles autorisés** : `Super-Admin`, `Admin`, `Directeur`, `Sous-Directeur`

#### Endpoints

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/api/Communication` | Liste paginée des campagnes |
| GET | `/api/Communication/{id}` | Détails d'une campagne |
| GET | `/api/Communication/ecole/{ecoleId}` | Campagnes d'une école |
| POST | `/api/Communication` | Créer une campagne |
| PUT | `/api/Communication/{id}` | Modifier une campagne |
| POST | `/api/Communication/{id}/annuler` | Annuler une campagne |
| POST | `/api/Communication/{id}/destinataires/recharger` | Régénérer les destinataires |
| GET | `/api/Communication/{id}/destinataires` | Liste paginée des destinataires |
| GET | `/api/Communication/{id}/historique` | Historique d'une campagne |
| POST | `/api/Communication/{id}/envoyer` | Envoyer une campagne |

#### Exemple de requête POST (Création de campagne)

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
  "planifiedAt": "2024-12-20T10:00:00Z",
  "segments": [
    {
      "nomSegment": "Tous les parents",
      "typeSegment": "Parent",
      "isReusable": false,
      "classeIds": null,
      "directionIds": null,
      "utilisateurIds": null,
      "tuteurIds": null,
      "niveau": null,
      "tags": null
    }
  ]
}
```

#### Réponse

```json
{
  "idCampaign": 15,
  "idEcole": 1,
  "nomEcole": "École Primaire ABC",
  "titre": "Réunion parents-professeurs",
  "contenuMarkdown": "Bonjour,\n\nNous vous invitons...",
  "importance": "Important",
  "statut": "Brouillon",
  "canaux": {
    "push": true,
    "email": true,
    "sms": false,
    "inApp": true
  },
  "rappelAuto": true,
  "dateCreation": "2024-12-15T10:00:00Z",
  "planifiedAt": "2024-12-20T10:00:00Z",
  "totalDestinataires": 0,
  "envoyes": 0,
  "echecs": 0
}
```

---

## 🔔 SignalR - Communication temps réel

### Configuration

**Fichier** : `Program.cs`

```csharp
// Enregistrer SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
});

// Mapper les hubs
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<DashboardHub>("/hubs/dashboard");
```

### NotificationHub

**Fichier** : `Hubs/NotificationHub.cs`

#### Groupes disponibles

- `user_{userId}` : Groupe personnel d'un utilisateur
- `all_users` : Tous les utilisateurs connectés
- `ecole_{ecoleId}` : Tous les utilisateurs d'une école

#### Méthodes du Hub

| Méthode | Description |
|---------|-------------|
| `OnConnectedAsync()` | Appelé à la connexion (ajout automatique aux groupes) |
| `OnDisconnectedAsync()` | Appelé à la déconnexion |
| `JoinGroup(string groupName)` | Rejoindre un groupe spécifique |
| `LeaveGroup(string groupName)` | Quitter un groupe |
| `MarkNotificationAsRead(int notificationId)` | Marquer une notification comme lue |
| `GetConnectionStatus()` | Obtenir le statut de connexion |

#### Envoi de notifications depuis un service

```csharp
public class MonService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    
    public async Task EnvoyerNotificationAsync(int userId, string titre, string message)
    {
        var notification = new
        {
            type = "INFO",
            titre = titre,
            message = message,
            timestamp = DateTime.UtcNow
        };
        
        // Envoyer à un utilisateur spécifique
        await _hubContext.Clients
            .Group($"user_{userId}")
            .SendAsync("ReceiveNotification", notification);
        
        // Envoyer à tous les utilisateurs
        await _hubContext.Clients
            .Group("all_users")
            .SendAsync("ReceiveNotification", notification);
        
        // Envoyer à une école
        await _hubContext.Clients
            .Group($"ecole_{ecoleId}")
            .SendAsync("ReceiveNotification", notification);
    }
}
```

### Connexion côté client (JavaScript)

```javascript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://api.example.com/hubs/notifications", {
        accessTokenFactory: () => {
            // Retourner le token JWT
            return localStorage.getItem("token");
        }
    })
    .withAutomaticReconnect()
    .build();

// Démarrer la connexion
await connection.start();

// Écouter les notifications
connection.on("ReceiveNotification", (notification) => {
    console.log("Nouvelle notification:", notification);
    // Afficher la notification dans l'UI
    afficherNotification(notification);
});

// Rejoindre un groupe
await connection.invoke("JoinGroup", "ecole_1");

// Marquer comme lue
await connection.invoke("MarkNotificationAsRead", notificationId);
```

---

## 📱 Notifications multi-canal

### Ordre de priorité des canaux

1. **Push Firebase** (prioritaire)
2. **Email** (contenu riche)
3. **SMS Twilio** (fallback)
4. **In-App** (SignalR + stockage DB)

### Firebase Cloud Messaging (FCM)

#### Configuration

**Fichier** : `appsettings.json`

```json
{
  "Firebase": {
    "CredentialsPath": "path/to/firebase-credentials.json",
    "ProjectId": "your-project-id"
  }
}
```

**Initialisation** : `Program.cs`

```csharp
// Initialiser Firebase au démarrage
FirebaseNotificationService.InitializeFirebase(
    builder.Configuration["Firebase:CredentialsPath"]
);
```

#### Structure d'une notification Push

```csharp
var notification = new
{
    Title = "📚 Nouveau devoir",
    Body = "Votre enfant a un nouveau devoir de Mathématiques",
    Data = new Dictionary<string, string>
    {
        { "Type", "DevoirADomicile" },
        { "IdDevoir", "42" },
        { "IdClasse", "15" },
        { "Titre", "Exercices de Mathématiques" },
        { "DateLimite", "2024-12-20T18:00:00" }
    }
};
```

### SMS Twilio

#### Configuration

**Fichier** : `appsettings.json`

```json
{
  "Twilio": {
    "Enabled": true,
    "AccountSid": "your-account-sid",
    "AuthToken": "your-auth-token",
    "FromNumber": "+1234567890"
  }
}
```

#### Utilisation

```csharp
var smsService = serviceProvider.GetService<ISmsNotificationService>();

// Envoyer un SMS
var smsLog = await smsService.EnvoyerSmsAUtilisateurAsync(
    idUtilisateur: 1,
    message: "Votre enfant a un nouveau devoir",
    typeNotification: "DEVOIR"
);

if (smsLog != null && smsLog.Statut == "delivered")
{
    Console.WriteLine($"SMS envoyé avec succès. Coût: {smsLog.CoutUsd} USD");
}
```

**Note** : Le SMS est utilisé comme fallback si la notification Push échoue.

### Email (SMTP)

#### Configuration

**Fichier** : `appsettings.json`

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-password",
    "FromEmail": "noreply@example.com",
    "FromName": "KelasiNaBiso"
  }
}
```

---

## 📢 Campagnes de communication

### Cycle de vie d'une campagne

```
Brouillon → Valide → EnCours → Envoye
                ↓
            Annule
```

### Processus de création

1. **Création** : L'utilisateur crée une campagne avec titre, contenu, segments
2. **Validation** : La campagne est validée (optionnel, selon les rôles)
3. **Génération des destinataires** : Les destinataires sont générés à partir des segments
4. **Envoi** : La campagne est envoyée via les canaux sélectionnés
5. **Suivi** : Les statuts sont mis à jour en temps réel

### Segmentation

Les segments permettent de cibler précisément les destinataires :

- **Par classe** : `ClasseIds: [1, 2, 3]`
- **Par direction** : `DirectionIds: [5]`
- **Par utilisateur** : `UtilisateurIds: [10, 20]`
- **Par tuteur** : `TuteurIds: [30, 40]`
- **Par niveau** : `Niveau: "Primaire"`
- **Par tags** : `Tags: ["urgent", "important"]`

### Exemple de campagne complète

```csharp
// 1. Créer la campagne
var dto = new CreateCommunicationCampaignDto
{
    IdEcole = 1,
    Titre = "Réunion parents-professeurs",
    ContenuMarkdown = "Bonjour,\n\nNous vous invitons...",
    Importance = "Important",
    Canaux = new CommunicationChannelsDto
    {
        Push = true,
        Email = true,
        Sms = false,
        InApp = true
    },
    RappelAuto = true,
    PlanifiedAt = DateTime.UtcNow.AddDays(5),
    Segments = new List<CreateCommunicationSegmentDto>
    {
        new CreateCommunicationSegmentDto
        {
            NomSegment = "Parents de la classe 3A",
            TypeSegment = "Parent",
            ClasseIds = new List<int> { 15 },
            IsReusable = false
        }
    }
};

var campaign = await campaignService.CreateCampaignAsync(
    dto, currentUserId, currentUserRole, currentUserEcoleId
);

// 2. Régénérer les destinataires (si nécessaire)
await campaignService.RefreshRecipientsAsync(
    campaign.IdCampaign, currentUserId, currentUserRole, currentUserEcoleId
);

// 3. Envoyer la campagne
await campaignService.DispatchCampaignAsync(
    campaign.IdCampaign, currentUserId, currentUserRole, currentUserEcoleId
);
```

### Préférences de communication

Les utilisateurs peuvent définir leurs préférences de communication via le modèle `ParentCommunicationPreference` :

```csharp
public class ParentCommunicationPreference
{
    public int IdPreference { get; set; }
    public int IdTuteur { get; set; }
    public string Canal { get; set; } // "Push", "Email", "Sms", "InApp"
    public bool OptIn { get; set; } // true = accepte, false = refuse
}
```

Le système respecte automatiquement ces préférences lors de l'envoi des campagnes.

---

## 💬 Messagerie

### Messages privés

Un message privé est envoyé entre deux utilisateurs (`IdExpediteur` et `IdDestinateur`).

**Flux automatique** :
1. Le message est sauvegardé en base de données
2. Une notification Push est envoyée au destinataire
3. Si la Push échoue, un SMS est envoyé en fallback
4. Une notification SignalR est envoyée en temps réel
5. Le message apparaît dans l'historique de conversation

### Messages de groupe

Un message de groupe est envoyé à tous les membres d'un `GroupeMessage`.

**Flux** :
1. Le message est sauvegardé avec `IdGroupe`
2. Les notifications sont envoyées à tous les membres du groupe
3. Les notifications apparaissent en temps réel via SignalR

### Exemple d'utilisation

```csharp
// Créer un message privé
var message = new Message
{
    IdExpediteur = 1,
    IdDestinateur = 2,
    ContenuMessage = "Bonjour, comment allez-vous ?",
    DateEnvoi = DateTime.Now
};

var createdMessage = await messageService.CreateAsync(message);
// → Notification Push/SMS envoyée automatiquement

// Récupérer une conversation
var conversation = await messageService.GetConversationAsync(
    idExpediteur: 1,
    idDestinateur: 2
);
```

---

## ⚙️ Configuration et dépendances

### Packages NuGet requis

```xml
<ItemGroup>
  <!-- SignalR (inclus dans .NET 6.0) -->
  <!-- Pas de package nécessaire -->
  
  <!-- Firebase Admin SDK -->
  <PackageReference Include="FirebaseAdmin" Version="2.4.0" />
  
  <!-- Twilio SDK -->
  <PackageReference Include="Twilio" Version="6.0.0" />
  
  <!-- Entity Framework Core -->
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="6.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="6.0.0" />
  <PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="6.0.0" />
  
  <!-- JWT Authentication -->
  <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="6.0.0" />
</ItemGroup>
```

### Configuration appsettings.json

```json
{
  "ConnectionStrings": {
    "KelasiConnection": "Server=localhost;Database=kelasi_db;User=root;Password=..."
  },
  "Jwt": {
    "Key": "your-secret-key",
    "Issuer": "KelasiNaBisoAPI",
    "Audience": "KelasiNaBisoClient"
  },
  "Firebase": {
    "CredentialsPath": "firebase-credentials.json",
    "ProjectId": "your-project-id"
  },
  "Twilio": {
    "Enabled": true,
    "AccountSid": "your-account-sid",
    "AuthToken": "your-auth-token",
    "FromNumber": "+1234567890"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-password",
    "FromEmail": "noreply@example.com",
    "FromName": "KelasiNaBiso"
  }
}
```

### Configuration CORS

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(origin => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

---

## 🚀 Guide d'implémentation

### Étape 1 : Créer les modèles

1. Créer les fichiers dans `Models/` :
   - `Message.cs`
   - `GroupeMessage.cs`
   - `CommunicationCampaign.cs`
   - `CommunicationSegment.cs`
   - `CampaignRecipient.cs`
   - `CommunicationHistory.cs`
   - `Notification.cs`

2. Ajouter les DbSet dans le DbContext :

```csharp
public class KelasiNaBisoDbContext : DbContext
{
    public DbSet<Message> Messages { get; set; }
    public DbSet<GroupeMessage> GroupeMessages { get; set; }
    public DbSet<CommunicationCampaign> CommunicationCampaigns { get; set; }
    public DbSet<CommunicationSegment> CommunicationSegments { get; set; }
    public DbSet<CampaignRecipient> CampaignRecipients { get; set; }
    public DbSet<CommunicationHistory> CommunicationHistory { get; set; }
    public DbSet<Notification> Notifications { get; set; }
}
```

3. Créer et appliquer les migrations :

```bash
dotnet ef migrations add AddCommunicationModule
dotnet ef database update
```

### Étape 2 : Créer les interfaces et services

1. Créer les interfaces dans `Services/Repositories/` :
   - `IMessageRepository.cs`
   - `ICommunicationCampaignService.cs`
   - `IFirebaseNotificationService.cs`
   - `ISmsNotificationService.cs`
   - `ICommunicationDispatchScheduler.cs`
   - `ICommunicationDispatchWorker.cs`

2. Implémenter les services dans `Services/` :
   - `MessageService.cs`
   - `CommunicationCampaignService.cs`
   - `FirebaseNotificationService.cs`
   - `SmsNotificationService.cs`
   - `CommunicationDispatchScheduler.cs`
   - `CommunicationDispatchWorker.cs`

### Étape 3 : Créer les contrôleurs

1. Créer les contrôleurs dans `Controllers/` :
   - `MessageController.cs`
   - `GroupeMessageController.cs`
   - `CommunicationController.cs`

2. Enregistrer les services dans `Program.cs` :

```csharp
builder.Services.AddScoped<IMessageRepository, MessageService>();
builder.Services.AddScoped<ICommunicationCampaignService, CommunicationCampaignService>();
builder.Services.AddScoped<IFirebaseNotificationService, FirebaseNotificationService>();
builder.Services.AddScoped<ISmsNotificationService, SmsNotificationService>();
builder.Services.AddScoped<ICommunicationDispatchScheduler, CommunicationDispatchScheduler>();
builder.Services.AddScoped<ICommunicationDispatchWorker, CommunicationDispatchWorker>();
```

### Étape 4 : Configurer SignalR

1. Enregistrer SignalR dans `Program.cs` :

```csharp
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});
```

2. Créer les hubs dans `Hubs/` :
   - `NotificationHub.cs`
   - `DashboardHub.cs`

3. Mapper les hubs dans `Program.cs` :

```csharp
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<DashboardHub>("/hubs/dashboard");
```

### Étape 5 : Configurer Firebase

1. Télécharger le fichier de credentials Firebase depuis la console Firebase
2. Placer le fichier dans le projet (ex: `firebase-credentials.json`)
3. Initialiser Firebase dans `Program.cs` :

```csharp
FirebaseNotificationService.InitializeFirebase(
    builder.Configuration["Firebase:CredentialsPath"]
);
```

### Étape 6 : Configurer Twilio

1. Créer un compte Twilio et obtenir `AccountSid` et `AuthToken`
2. Configurer dans `appsettings.json`
3. Implémenter `SmsNotificationService` avec le SDK Twilio

### Étape 7 : Tester

1. Tester les endpoints API avec Swagger ou Postman
2. Tester les notifications Push avec un device réel
3. Tester SignalR avec un client JavaScript
4. Tester les campagnes de communication

---

## 💻 Exemples de code

### Exemple 1 : Envoyer un message avec notification

```csharp
[HttpPost("api/Message")]
[Authorize]
public async Task<ActionResult<Message>> CreateMessage(Message message)
{
    var createdMessage = await _messageRepository.CreateAsync(message);
    
    // La notification est envoyée automatiquement dans MessageService.CreateAsync
    // Pas besoin de code supplémentaire ici
    
    return CreatedAtAction(nameof(GetMessage), 
        new { id = createdMessage.IdMessage }, 
        createdMessage);
}
```

### Exemple 2 : Créer et envoyer une campagne

```csharp
[HttpPost("api/Communication")]
[Authorize(Roles = "Admin,Directeur")]
public async Task<ActionResult> CreateAndSendCampaign(
    [FromBody] CreateCommunicationCampaignDto dto)
{
    // 1. Créer la campagne
    var campaign = await _campaignService.CreateCampaignAsync(
        dto, _currentUserService.UserId, 
        _currentUserService.UserRole, 
        _currentUserService.EcoleId
    );
    
    // 2. Régénérer les destinataires
    await _campaignService.RefreshRecipientsAsync(
        campaign.IdCampaign, 
        _currentUserService.UserId, 
        _currentUserService.UserRole, 
        _currentUserService.EcoleId
    );
    
    // 3. Envoyer immédiatement (ou planifier)
    if (dto.SendImmediately)
    {
        await _campaignService.DispatchCampaignAsync(
            campaign.IdCampaign, 
            _currentUserService.UserId, 
            _currentUserService.UserRole, 
            _currentUserService.EcoleId
        );
    }
    
    return Ok(campaign);
}
```

### Exemple 3 : Envoyer une notification Push personnalisée

```csharp
public async Task NotifierNouveauDevoir(int idEleve, int idDevoir)
{
    // Récupérer le tuteur de l'élève
    var eleve = await _context.Eleves
        .Include(e => e.Tuteur)
        .ThenInclude(t => t.Utilisateur)
        .FirstOrDefaultAsync(e => e.IdEleve == idEleve);
    
    if (eleve?.Tuteur?.Utilisateur == null) return;
    
    var titre = "📚 Nouveau devoir";
    var corps = $"Votre enfant {eleve.NomComplet} a un nouveau devoir";
    
    var donnees = new Dictionary<string, string>
    {
        { "Type", "DevoirADomicile" },
        { "IdDevoir", idDevoir.ToString() },
        { "IdEleve", idEleve.ToString() }
    };
    
    // Envoyer la notification Push
    await _firebaseService.EnvoyerNotificationAUtilisateurAsync(
        eleve.Tuteur.Utilisateur.IdUtilisateur,
        titre,
        corps,
        donnees
    );
    
    // Envoyer aussi via SignalR
    await _hubContext.Clients
        .Group($"user_{eleve.Tuteur.Utilisateur.IdUtilisateur}")
        .SendAsync("ReceiveNotification", new
        {
            type = "DEVOIR",
            titre = titre,
            message = corps,
            data = donnees,
            timestamp = DateTime.UtcNow
        });
}
```

### Exemple 4 : Client JavaScript pour SignalR

```javascript
// signalr-client.js
import * as signalR from "@microsoft/signalr";

class SignalRClient {
    constructor(baseUrl, getToken) {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(`${baseUrl}/hubs/notifications`, {
                accessTokenFactory: () => getToken()
            })
            .withAutomaticReconnect({
                nextRetryDelayInMilliseconds: retryContext => {
                    if (retryContext.elapsedMilliseconds < 60000) {
                        return 2000; // Reconnecter après 2 secondes
                    } else {
                        return null; // Arrêter après 60 secondes
                    }
                }
            })
            .build();
        
        this.setupHandlers();
    }
    
    async start() {
        try {
            await this.connection.start();
            console.log("✅ Connecté à SignalR");
        } catch (err) {
            console.error("❌ Erreur de connexion SignalR:", err);
            setTimeout(() => this.start(), 5000);
        }
    }
    
    setupHandlers() {
        // Écouter les notifications
        this.connection.on("ReceiveNotification", (notification) => {
            console.log("📬 Nouvelle notification:", notification);
            this.onNotificationReceived(notification);
        });
        
        // Gérer la reconnexion
        this.connection.onreconnecting(() => {
            console.log("🔄 Reconnexion en cours...");
        });
        
        this.connection.onreconnected(() => {
            console.log("✅ Reconnecté à SignalR");
        });
        
        this.connection.onclose(() => {
            console.log("❌ Déconnecté de SignalR");
        });
    }
    
    onNotificationReceived(notification) {
        // Afficher la notification dans l'UI
        this.showNotification(notification);
        
        // Sauvegarder dans la base de données locale
        this.saveNotification(notification);
    }
    
    showNotification(notification) {
        // Implémenter l'affichage (toast, modal, etc.)
        if (Notification.permission === "granted") {
            new Notification(notification.titre, {
                body: notification.message,
                icon: "/icon.png",
                data: notification.data
            });
        }
    }
    
    async markAsRead(notificationId) {
        await this.connection.invoke("MarkNotificationAsRead", notificationId);
    }
}

// Utilisation
const client = new SignalRClient(
    "https://api.example.com",
    () => localStorage.getItem("token")
);

await client.start();
```

---

## ✅ Bonnes pratiques

### 1. Gestion des erreurs

- Toujours logger les erreurs avec `ILogger`
- Gérer les timeouts et retry pour les services externes
- Implémenter des fallbacks (ex: SMS si Push échoue)

### 2. Performance

- Utiliser la pagination pour les listes
- Envoyer les campagnes en arrière-plan (background jobs)
- Utiliser `AsNoTracking()` pour les requêtes en lecture seule
- Mettre en cache les tokens FCM valides

### 3. Sécurité

- Valider tous les inputs avec des Data Annotations
- Vérifier les permissions par rôle avant chaque action
- Utiliser JWT Bearer pour l'authentification
- Sanitizer le contenu Markdown avant l'envoi

### 4. Scalabilité

- Utiliser des files d'attente pour les envois en masse
- Traiter les campagnes par batch (ex: 100 destinataires à la fois)
- Utiliser des workers en arrière-plan pour les tâches longues
- Monitorer les performances avec des métriques

### 5. Maintenance

- Documenter tous les endpoints avec XML comments
- Créer des tests unitaires et d'intégration
- Utiliser des DTOs pour les échanges API
- Versionner les APIs si nécessaire

---

## 📝 Conclusion

Ce module de communication offre une solution complète et scalable pour gérer la communication dans une application éducative. Il combine :

- **Messagerie** : Communication directe entre utilisateurs
- **Campagnes** : Communication en masse avec segmentation
- **Temps réel** : SignalR pour une expérience fluide
- **Multi-canal** : Push, Email, SMS, In-App avec fallback

Pour toute question ou amélioration, référez-vous au code source de KelasiNaBisoAPI.

---

**Documentation créée le** : Décembre 2024  
**Version** : 1.0  
**Auteur** : Documentation automatique basée sur KelasiNaBisoAPI

