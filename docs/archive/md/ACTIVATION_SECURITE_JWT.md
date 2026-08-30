# 🔐 ACTIVATION DE LA SÉCURITÉ JWT

## 📋 Problème identifié

Les endpoints de l'API étaient **accessibles sans authentification**, malgré l'existence d'un système d'authentification JWT. C'est un **problème de sécurité majeur**.

---

## ✅ Corrections appliquées

### 1️⃣ Installation du package JWT

**Fichier modifié :** `KelasiNaBiso.csproj`

```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="6.0.25" />
```

### 2️⃣ Configuration de l'authentification JWT

**Fichier modifié :** `Program.cs`

#### Avant (❌ Authentification désactivée)
```csharp
// Configuration JWT avec middleware personnalisé
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
```

#### Après (✅ Authentification JWT activée)
```csharp
// ✅ Configuration JWT avec authentification Bearer
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.RequireHttpsMetadata = false; // Pour le développement
        options.SaveToken = true;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"] ?? "VotreCleSecreteSuperLongueEtComplexeIci123!")
            ),
            ValidateIssuer = false, // Pas de validation d'issuer pour simplifier
            ValidateAudience = false, // Pas de validation d'audience pour simplifier
            ValidateLifetime = true, // Valider l'expiration du token
            ClockSkew = TimeSpan.Zero // Pas de tolérance sur l'expiration
        };
    });

builder.Services.AddAuthorization();
```

**Points clés :**
- `ValidateIssuerSigningKey = true` : Vérifie la signature du token
- `ValidateLifetime = true` : Vérifie que le token n'est pas expiré
- `ClockSkew = TimeSpan.Zero` : Pas de tolérance (le token expire exactement à l'heure prévue)

### 3️⃣ Activation des middlewares

**Fichier modifié :** `Program.cs`

#### Avant (❌ Middlewares commentés)
```csharp
app.UseRouting();

// Middleware d'authentification simple - DÉSACTIVÉ TEMPORAIREMENT
// app.UseMiddleware<KelasiNaBiso.Middleware.SimpleAuthMiddleware>();

// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();
```

#### Après (✅ Middlewares activés)
```csharp
app.UseRouting();

// ✅ Activation de l'authentification et de l'autorisation JWT
app.UseAuthentication(); // DOIT être avant UseAuthorization
app.UseAuthorization();

app.MapControllers();
```

**⚠️ IMPORTANT :** L'ordre est crucial !
1. `UseRouting()`
2. `UseAuthentication()` ← Vérifie le token
3. `UseAuthorization()` ← Vérifie les permissions
4. `MapControllers()`

### 4️⃣ Configuration de Swagger pour JWT

**Fichier modifié :** `Program.cs`

```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "KelasiNaBisoAPI",
        Version = "v1",
        Description = "Kelasi Na Biso - API sécurisée avec JWT"
    });
    
    // ✅ Configuration de l'authentification JWT dans Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Entrez le token JWT comme : Bearer {votre_token}"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
```

---

## 🛡️ Prochaine étape : Protéger les contrôleurs

### Méthode 1 : Protection globale (Recommandé)

Ajoutez `[Authorize]` sur **chaque contrôleur** :

```csharp
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
[Authorize] // ✅ Toutes les méthodes du contrôleur sont protégées
public class EleveController : ControllerBase
{
    // ...
}
```

### Méthode 2 : Protection sélective

Si certains endpoints doivent rester publics :

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Protection par défaut
public class EcoleController : ControllerBase
{
    [AllowAnonymous] // ✅ Cet endpoint reste public
    [HttpGet("publiques")]
    public async Task<ActionResult<IEnumerable<Ecole>>> GetEcolesPubliques()
    {
        // ...
    }
    
    [HttpPost] // ✅ Cet endpoint nécessite un token
    public async Task<ActionResult<Ecole>> CreateEcole(Ecole ecole)
    {
        // ...
    }
}
```

### Méthode 3 : Endpoints publics spécifiques

Le contrôleur `UtilisateurController` doit avoir des endpoints publics :

```csharp
[ApiController]
[Route("api/[controller]")]
public class UtilisateurController : ControllerBase
{
    [AllowAnonymous] // ✅ Login public
    [HttpPost("authentifier")]
    public async Task<ActionResult<object>> Authentifier(LoginRequest request)
    {
        // ...
    }
    
    [Authorize] // ✅ Changement de mot de passe protégé
    [HttpPost("changer-mot-de-passe")]
    public async Task<ActionResult<object>> ChangerMotDePasse(ChangerMotDePasseRequest request)
    {
        // ...
    }
}
```

---

## 🧪 Comment tester

### 1️⃣ Tester sans token (doit échouer)

```http
GET https://localhost:7102/api/Eleve
```

**Résultat attendu :** `401 Unauthorized`

### 2️⃣ S'authentifier pour obtenir un token

```http
POST https://localhost:7102/api/Utilisateur/authentifier
Content-Type: application/json

{
  "email0uTelephone": "superadmin@kelasinabiso.cd",
  "motDePasse": "Super-Admin"
}
```

**Résultat attendu :**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 720
}
```

### 3️⃣ Utiliser le token pour accéder aux endpoints protégés

```http
GET https://localhost:7102/api/Eleve
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Résultat attendu :** `200 OK` avec les données

### 4️⃣ Tester avec Swagger

1. Ouvrez `https://localhost:7102/swagger`
2. Cliquez sur le bouton **"Authorize"** (🔓) en haut à droite
3. Entrez le token comme : `Bearer {votre_token}`
4. Cliquez sur **"Authorize"**
5. Tous les appels Swagger incluront automatiquement le token

---

## 📊 Contrôleurs à protéger

### ✅ Contrôleurs qui DOIVENT être protégés

- `EleveController` ← Données sensibles (élèves)
- `EcoleController` ← Gestion des écoles
- `InscriptionController` ← Inscriptions
- `NoteController` ← Notes des élèves
- `PaiementController` ← Paiements
- `AgentController` ← Gestion des enseignants/agents
- `TuteurController` ← Données des tuteurs
- `MessageController` ← Messages privés
- `PresenceController` ← Pointages
- `NotificationController` ← Notifications
- `DocumentController` ← Documents
- `EvaluationController` ← Évaluations

### ⚠️ Contrôleurs avec endpoints publics possibles

- `UtilisateurController` :
  - ✅ Public : `authentifier`, `reset-password`
  - 🔒 Protégé : `changer-mot-de-passe`, `profil`, `liste`

---

## 🔑 Endpoints qui doivent rester publics

### Liste des endpoints publics recommandés :

1. **Authentification**
   ```csharp
   [AllowAnonymous]
   [HttpPost("authentifier")]
   ```

2. **Réinitialisation de mot de passe**
   ```csharp
   [AllowAnonymous]
   [HttpPost("demander-reset-password")]
   ```
   
3. **Vérification du code de réinitialisation**
   ```csharp
   [AllowAnonymous]
   [HttpPost("verifier-code-reset")]
   ```

4. **Nouveau mot de passe (avec code)**
   ```csharp
   [AllowAnonymous]
   [HttpPost("nouveau-mot-de-passe")]
   ```

**Tous les autres endpoints** doivent être protégés avec `[Authorize]`.

---

## 📋 Checklist d'implémentation

### Phase 1 : Configuration (✅ TERMINÉ)
- [x] Installer le package `Microsoft.AspNetCore.Authentication.JwtBearer`
- [x] Configurer l'authentification JWT dans `Program.cs`
- [x] Activer les middlewares `UseAuthentication()` et `UseAuthorization()`
- [x] Configurer Swagger pour JWT

### Phase 2 : Protection des contrôleurs (⏳ À FAIRE)
- [ ] Ajouter `[Authorize]` sur tous les contrôleurs critiques
- [ ] Identifier les endpoints qui doivent rester publics
- [ ] Ajouter `[AllowAnonymous]` sur les endpoints publics
- [ ] Tester chaque contrôleur avec et sans token

### Phase 3 : Tests (⏳ À FAIRE)
- [ ] Tester l'authentification
- [ ] Tester l'accès aux endpoints protégés sans token (doit échouer)
- [ ] Tester l'accès aux endpoints protégés avec token (doit réussir)
- [ ] Tester l'expiration des tokens
- [ ] Tester Swagger avec authentification

---

## ⚠️ AVERTISSEMENT IMPORTANT

**Avant de déployer en production :**

1. **Changez la clé secrète** dans `appsettings.json` :
   ```json
   {
     "JwtSettings": {
       "SecretKey": "VOTRE_CLE_SECRETE_SUPER_LONGUE_ET_COMPLEXE_ICI_123456789!",
       "ExpirationMinutes": 720
     }
   }
   ```

2. **Activez HTTPS** en production :
   ```csharp
   options.RequireHttpsMetadata = true; // En production
   ```

3. **Validez l'Issuer et l'Audience** pour plus de sécurité

4. **Utilisez une durée d'expiration plus courte** (ex: 60 minutes au lieu de 720)

---

## ✅ Résumé des modifications

| Fichier | Modification | Statut |
|---------|--------------|--------|
| `KelasiNaBiso.csproj` | Ajout du package JWT | ✅ Fait |
| `Program.cs` | Configuration JWT | ✅ Fait |
| `Program.cs` | Activation des middlewares | ✅ Fait |
| `Program.cs` | Configuration Swagger JWT | ✅ Fait |
| `*Controller.cs` | Ajout `[Authorize]` | ⏳ À faire |

---

**Date :** 25 octobre 2025  
**Statut :** Configuration de base complétée - Protection des contrôleurs à implémenter

