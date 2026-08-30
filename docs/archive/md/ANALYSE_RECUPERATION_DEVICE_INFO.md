# 📱 ANALYSE : Récupération des Informations du Device lors de l'Authentification

## 📋 Vue d'ensemble

Dans **AkademiaAPI**, lors de l'authentification, les informations du device de l'utilisateur sont automatiquement collectées et stockées pour permettre l'envoi de **notifications push** via Firebase Cloud Messaging (FCM).

---

## 🔍 Comment ça fonctionne

### 1️⃣ **Structure du DTO `AuthentificationRequest`**

**Fichier** : `AkademiaAPI/Models/AuthentificationRequest.cs`

```csharp
public class AuthentificationRequest
{
    [Required(ErrorMessage = "L'email ou le téléphone est requis")]
    public string? EmailOuTelephone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est requis")]
    public string? MotDePasse { get; set; } = string.Empty;

    // ✨ Informations du device pour les notifications push (optionnelles)
    [MaxLength(500)]
    public string? FcmToken { get; set; } // Token Firebase Cloud Messaging

    [MaxLength(50)]
    public string? DeviceType { get; set; } // Android, iOS, Web

    [MaxLength(100)]
    public string? DeviceModel { get; set; } // Ex: iPhone 12, Samsung Galaxy S21

    [MaxLength(50)]
    public string? OsVersion { get; set; } // Ex: Android 12, iOS 15.2
}
```

#### 📊 Champs Device (Optionnels)

| Champ | Type | Description | Exemple |
|-------|------|-------------|---------|
| `FcmToken` | string? | Token unique FCM du device | `"fKx7Y...Jz9M"` |
| `DeviceType` | string? | Type de plateforme | `"Android"`, `"iOS"`, `"Web"` |
| `DeviceModel` | string? | Modèle du device | `"iPhone 12"`, `"Samsung Galaxy S21"` |
| `OsVersion` | string? | Version du système d'exploitation | `"Android 12"`, `"iOS 15.2"` |

---

### 2️⃣ **Logique d'enregistrement dans le Controller**

**Fichier** : `AkademiaAPI/Controllers/UtilisateurController.cs` (lignes 281-315)

```csharp
// POST: api/Utilisateur/authentifier
[HttpPost("authentifier")]
[AllowAnonymous]
public async Task<ActionResult<AuthentificationResponse>> Authentifier(AuthentificationRequest request)
{
    // ... (Authentification standard) ...

    // ✨ Enregistrer le token FCM si fourni (pour les notifications push)
    if (!string.IsNullOrEmpty(request.FcmToken) && 
        request.FcmToken != "string" && 
        request.FcmToken != "null" &&
        !string.IsNullOrEmpty(request.DeviceType) && 
        request.DeviceType != "string" && 
        request.DeviceType != "null")
    {
        try
        {
            await _userDeviceRepository.CreateOrUpdateAsync(
                utilisateur.IdUtilisateur,
                request.FcmToken,
                request.DeviceType,
                request.DeviceModel,
                request.OsVersion
            );
            
            _logger.LogInformation($"✅ Token FCM enregistré pour l'utilisateur {utilisateur.IdUtilisateur} - Device: {request.DeviceType} {request.DeviceModel}");
        }
        catch (ArgumentException argEx)
        {
            // Erreur de validation des données (valeurs "string" par défaut)
            _logger.LogWarning($"⚠️ Données device invalides pour l'utilisateur {utilisateur.IdUtilisateur}: {argEx.Message}");
        }
        catch (Exception ex)
        {
            // Autres erreurs lors de l'enregistrement du token
            _logger.LogError(ex, $"❌ Erreur lors de l'enregistrement du token FCM pour l'utilisateur {utilisateur.IdUtilisateur}");
        }
    }
    else
    {
        _logger.LogWarning($"⚠️ Token FCM ou DeviceType manquant/invalide pour l'utilisateur {utilisateur.IdUtilisateur} - Token: '{request.FcmToken}', Device: '{request.DeviceType}'");
    }

    // ... (Génération du token JWT et réponse) ...
}
```

#### 🔑 Points clés

1. **Validation stricte** :
   - Vérifie que `FcmToken` n'est pas vide, `"string"` ou `"null"`
   - Vérifie que `DeviceType` n'est pas vide, `"string"` ou `"null"`

2. **Gestion des erreurs** :
   - Capture les `ArgumentException` (données invalides)
   - Capture les autres exceptions (erreurs système)
   - **Ne bloque pas l'authentification** si l'enregistrement échoue

3. **Logging détaillé** :
   - ✅ Succès : Token enregistré
   - ⚠️ Warning : Données invalides ou manquantes
   - ❌ Erreur : Problème technique

---

### 3️⃣ **Service `CreateOrUpdateAsync`**

**Fichier** : `AkademiaAPI/Services/UserDeviceService.cs` (lignes 66-115)

```csharp
/// <summary>
/// Crée ou met à jour un device pour un utilisateur
/// ✅ CORRIGÉ: Recherche par UserId + DeviceType pour permettre plusieurs devices par utilisateur
/// ✅ CORRIGÉ: Validation des données pour éviter les valeurs "string" par défaut
/// </summary>
public async Task<UserDevice> CreateOrUpdateAsync(
    int idUtilisateur, 
    string fcmToken, 
    string? deviceType, 
    string? deviceModel, 
    string? osVersion)
{
    // 🚨 VALIDATION: Rejeter les valeurs par défaut "string"
    if (string.IsNullOrWhiteSpace(fcmToken) || fcmToken == "string" || fcmToken == "null")
    {
        throw new ArgumentException("FCM Token invalide", nameof(fcmToken));
    }

    if (string.IsNullOrWhiteSpace(deviceType) || deviceType == "string" || deviceType == "null")
    {
        throw new ArgumentException("Device Type invalide", nameof(deviceType));
    }

    // ✅ CORRIGÉ: Chercher par UserId + DeviceType (au lieu de FCM Token uniquement)
    // Cela permet à un utilisateur d'avoir plusieurs devices (ex: Android + iOS)
    var existingDevice = await _context.UserDevices
        .FirstOrDefaultAsync(ud => ud.IdUtilisateur == idUtilisateur && ud.DeviceType == deviceType);

    if (existingDevice != null)
    {
        // Mettre à jour le device existant pour ce type
        existingDevice.FcmToken = fcmToken; // Le token peut changer
        existingDevice.DeviceModel = deviceModel ?? existingDevice.DeviceModel;
        existingDevice.OsVersion = osVersion ?? existingDevice.OsVersion;
        existingDevice.DateDerniereUtilisation = DateTime.Now;
        existingDevice.Statut = true;

        await _context.SaveChangesAsync();
        return existingDevice;
    }
    else
    {
        // Créer un nouveau device pour ce type
        var newDevice = new UserDevice
        {
            IdUtilisateur = idUtilisateur,
            FcmToken = fcmToken,
            DeviceType = deviceType,
            DeviceModel = deviceModel ?? "Unknown",
            OsVersion = osVersion ?? "Unknown",
            DateEnregistrement = DateTime.Now,
            DateDerniereUtilisation = DateTime.Now,
            Statut = true
        };

        _context.UserDevices.Add(newDevice);
        await _context.SaveChangesAsync();
        return newDevice;
    }
}
```

#### 🎯 Logique intelligente

**Recherche par `IdUtilisateur` + `DeviceType`** :
- Permet à un utilisateur d'avoir **plusieurs devices** (Android + iOS + Web)
- Si l'utilisateur se connecte depuis le **même type de device**, le token FCM est **mis à jour**
- Si l'utilisateur se connecte depuis un **nouveau type de device**, un **nouvel enregistrement** est créé

**Mise à jour vs Création** :

| Scénario | Action | Résultat |
|----------|--------|----------|
| Device existant (même type) | **UPDATE** | Token FCM mis à jour, `DateDerniereUtilisation` actualisée |
| Nouveau type de device | **CREATE** | Nouvel enregistrement avec nouveau token |
| Token FCM change | **UPDATE** | Le nouveau token remplace l'ancien |

---

## 📊 Modèle de données `UserDevice`

**Fichier** : `AkademiaAPI/Models/UserDevice.cs`

```csharp
public class UserDevice
{
    [Key]
    public int IdUserDevice { get; set; }
    
    [Required]
    public int IdUtilisateur { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string FcmToken { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? DeviceType { get; set; } // Android, iOS, Web
    
    [MaxLength(100)]
    public string? DeviceModel { get; set; }
    
    [MaxLength(50)]
    public string? OsVersion { get; set; }
    
    public DateTime DateEnregistrement { get; set; } = DateTime.Now;
    public DateTime DateDerniereUtilisation { get; set; } = DateTime.Now;
    
    public bool Statut { get; set; } = true;
    
    // Navigation
    [JsonIgnore]
    [ValidateNever]
    public Utilisateur? Utilisateur { get; set; }
}
```

---

## 🎯 Cas d'utilisation

### Scénario 1 : Première connexion depuis Android

**Request** :
```json
{
  "emailOuTelephone": "user@example.com",
  "motDePasse": "Password123",
  "fcmToken": "fKx7Y...Jz9M",
  "deviceType": "Android",
  "deviceModel": "Samsung Galaxy S21",
  "osVersion": "Android 12"
}
```

**Résultat** :
- ✅ Authentification réussie
- ✅ **Nouveau device créé** dans `UserDevices`
- ✅ Token JWT retourné

---

### Scénario 2 : Connexion depuis le même Android (token FCM changé)

**Request** :
```json
{
  "emailOuTelephone": "user@example.com",
  "motDePasse": "Password123",
  "fcmToken": "aB3x9...Pq7N",  // ⬅️ Nouveau token
  "deviceType": "Android",      // ⬅️ Même type
  "deviceModel": "Samsung Galaxy S21",
  "osVersion": "Android 12"
}
```

**Résultat** :
- ✅ Authentification réussie
- ✅ **Device existant mis à jour** (nouveau token FCM)
- ✅ `DateDerniereUtilisation` actualisée
- ✅ Token JWT retourné

---

### Scénario 3 : Connexion depuis un iPhone (nouveau type)

**Request** :
```json
{
  "emailOuTelephone": "user@example.com",
  "motDePasse": "Password123",
  "fcmToken": "cZ5k2...Wm8L",
  "deviceType": "iOS",          // ⬅️ Nouveau type de device
  "deviceModel": "iPhone 13",
  "osVersion": "iOS 15.2"
}
```

**Résultat** :
- ✅ Authentification réussie
- ✅ **Nouveau device créé** (iOS)
- ✅ L'utilisateur a maintenant **2 devices** (Android + iOS)
- ✅ Token JWT retourné

---

### Scénario 4 : Connexion sans données device

**Request** :
```json
{
  "emailOuTelephone": "user@example.com",
  "motDePasse": "Password123"
  // ⬅️ Pas de fcmToken, deviceType, etc.
}
```

**Résultat** :
- ✅ Authentification réussie
- ⚠️ Warning loggé : "Token FCM ou DeviceType manquant"
- ✅ Token JWT retourné
- ❌ Pas d'enregistrement device

---

## ⚠️ Validation des données

Le système rejette les valeurs suivantes :

| Valeur | Raison |
|--------|--------|
| `null` | Valeur nulle |
| `""` | Chaîne vide |
| `"string"` | Valeur par défaut Swagger |
| `"null"` | Chaîne "null" (pas vraiment null) |

**Exemple de requête invalide** :
```json
{
  "emailOuTelephone": "user@example.com",
  "motDePasse": "Password123",
  "fcmToken": "string",      // ❌ Rejeté
  "deviceType": "string",    // ❌ Rejeté
  "deviceModel": "string",
  "osVersion": "string"
}
```

**Résultat** :
- ✅ Authentification réussie
- ⚠️ Warning : "Données device invalides"
- ✅ Token JWT retourné
- ❌ Pas d'enregistrement device

---

## 🔄 Flux complet

```
┌─────────────────────────────────────────────────────────────────┐
│ 1. Client envoie POST /api/Utilisateur/authentifier            │
│    avec EmailOuTelephone, MotDePasse, FcmToken, DeviceType     │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│ 2. UtilisateurController.Authentifier()                        │
│    ✅ Valide Email/Telephone                                    │
│    ✅ Vérifie mot de passe (BCrypt)                             │
│    ✅ Marque utilisateur comme connecté                         │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│ 3. Validation des données device                               │
│    ✅ FcmToken != null/empty/"string"/"null" ?                  │
│    ✅ DeviceType != null/empty/"string"/"null" ?                │
└─────────────────────────────────────────────────────────────────┘
                              ↓
                     ┌────────┴────────┐
                     │                 │
                   OUI               NON
                     │                 │
                     ↓                 ↓
┌──────────────────────────────┐  ┌─────────────────────────┐
│ 4a. UserDeviceRepository.    │  │ 4b. Warning loggé       │
│     CreateOrUpdateAsync()    │  │     Pas d'enregistrement│
│     ✅ Validation stricte     │  └─────────────────────────┘
│     ✅ Recherche par UserId + │                │
│        DeviceType            │                │
│     ✅ UPDATE ou CREATE       │                │
└──────────────────────────────┘                │
                     │                           │
                     └───────────┬───────────────┘
                                 ↓
┌─────────────────────────────────────────────────────────────────┐
│ 5. Génération du token JWT                                     │
│    ✅ Inclut IdUtilisateur, Role, Ecole, etc.                   │
│    ✅ Expiration: 1440 minutes (24h)                            │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│ 6. Retour AuthentificationResponse                             │
│    {                                                            │
│      "success": true,                                           │
│      "accessToken": "eyJhbGci...",                              │
│      "tokenType": "Bearer",                                     │
│      "expiresIn": 86400,                                        │
│      "utilisateur": {...}                                       │
│    }                                                            │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🎨 Avantages de cette approche

### ✅ 1. **Multi-device support**
Un utilisateur peut se connecter depuis plusieurs types de devices (Android, iOS, Web) et recevoir des notifications sur tous.

### ✅ 2. **Mise à jour automatique**
Si le token FCM change (réinstallation de l'app, etc.), il est automatiquement mis à jour.

### ✅ 3. **Non bloquant**
L'échec de l'enregistrement du device **ne bloque pas l'authentification**.

### ✅ 4. **Validation robuste**
Rejette les valeurs par défaut Swagger (`"string"`, `"null"`).

### ✅ 5. **Logging détaillé**
Permet de déboguer facilement les problèmes d'enregistrement.

### ✅ 6. **Suivi de l'utilisation**
`DateDerniereUtilisation` permet de savoir quand l'utilisateur s'est connecté depuis chaque device.

---

## 🔧 Points d'amélioration possibles

### 1️⃣ **Nettoyage des tokens inactifs**
Supprimer les devices non utilisés depuis X mois.

### 2️⃣ **Limitation du nombre de devices**
Limiter à 5-10 devices actifs par utilisateur.

### 3️⃣ **Notification de nouveau device**
Envoyer un email/SMS quand un nouveau device est enregistré (sécurité).

### 4️⃣ **Géolocalisation**
Ajouter `Latitude`, `Longitude` pour savoir d'où l'utilisateur se connecte.

### 5️⃣ **User-Agent**
Enregistrer le User-Agent HTTP pour plus d'informations.

---

## 📝 Résumé

| Aspect | Description |
|--------|-------------|
| **Quand ?** | Lors de l'authentification (`POST /api/Utilisateur/authentifier`) |
| **Quoi ?** | FcmToken, DeviceType, DeviceModel, OsVersion |
| **Optionnel ?** | Oui, l'authentification fonctionne sans |
| **Multi-device ?** | Oui, un utilisateur peut avoir plusieurs devices |
| **Mise à jour ?** | Automatique si même type de device |
| **Validation ?** | Stricte, rejette les valeurs "string" / "null" |
| **Erreur ?** | Ne bloque pas l'authentification |
| **Utilité ?** | Notifications push via Firebase Cloud Messaging |

---

## 🎯 Conclusion

Le système de récupération des informations du device lors de l'authentification dans **AkademiaAPI** est :
- ✅ **Robuste** : Validation stricte, gestion d'erreurs complète
- ✅ **Flexible** : Support multi-device, optionnel
- ✅ **Intelligent** : Mise à jour automatique des tokens FCM
- ✅ **Non intrusif** : Ne bloque pas l'authentification
- ✅ **Traçable** : Logging détaillé pour le débogage

**Cette approche peut être directement réutilisée dans KelasiNaBisoAPI !** 🚀

