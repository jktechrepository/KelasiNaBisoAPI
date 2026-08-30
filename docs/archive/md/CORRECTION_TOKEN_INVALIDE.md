# 🔧 CORRECTION : Token JWT invalide

## 📋 Problème identifié

### ❌ Symptôme
Après authentification réussie, l'accès aux endpoints protégés retournait :
```
401 Unauthorized
www-authenticate: Bearer error="invalid_token"
```

### 🔍 Cause racine

Le problème venait de **deux incompatibilités** :

1. **Format de token incompatible** :
   - `SimpleJwtService` générait un token **manuellement** (format personnalisé)
   - Le middleware JWT d'ASP.NET Core attend un token **standard** généré par `JwtSecurityTokenHandler`
   
2. **Clés secrètes différentes** (RÉSOLU) :
   - `SimpleJwtService` utilisait : `Jwt:SecretKey`
   - `Program.cs` cherchait : `JwtSettings:SecretKey` ❌
   - Maintenant les deux utilisent : `Jwt:SecretKey` ✅

---

## ✅ Solution appliquée

### 1️⃣ Modification de SimpleJwtService.cs

#### Avant (❌ Token manuel)
```csharp
public string GenerateToken(Utilisateur utilisateur)
{
    // Créer le header JWT manuellement
    var header = new { alg = "HS256", typ = "JWT" };
    var payload = new { sub = utilisateur.IdUtilisateur.ToString(), ... };
    
    // Encoder en Base64URL manuellement
    var headerEncoded = Base64UrlEncode(...);
    var payloadEncoded = Base64UrlEncode(...);
    
    // Créer la signature manuellement avec HMACSHA256
    var signature = CreateHmacSha256Signature(...);
    
    return $"{headerEncoded}.{payloadEncoded}.{signature}";
}
```

#### Après (✅ Token standard)
```csharp
public string GenerateToken(Utilisateur utilisateur)
{
    // ✅ Utilisation de JwtSecurityTokenHandler (bibliothèque standard)
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    // Créer les claims
    var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, utilisateur.IdUtilisateur.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, utilisateur.Email ?? ""),
        new Claim(JwtRegisteredClaimNames.Name, utilisateur.NomUtilisateur ?? ""),
        new Claim("idEcole", utilisateur.IdEcole?.ToString() ?? ""),
        new Claim("idRole", utilisateur.IdRole.ToString()),
        new Claim(ClaimTypes.Role, utilisateur.Role?.Nom ?? ""),
        new Claim("ecole", utilisateur.Ecole?.Nom ?? ""),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    // Créer le token JWT avec JwtSecurityToken
    var token = new JwtSecurityToken(
        issuer: _issuer,
        audience: _audience,
        claims: claims,
        notBefore: DateTime.UtcNow,
        expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
        signingCredentials: credentials
    );

    // Retourner le token encodé
    var tokenHandler = new JwtSecurityTokenHandler();
    return tokenHandler.WriteToken(token);
}
```

### 2️⃣ Modification de ValidateToken

#### Avant (❌ Validation manuelle)
```csharp
public bool ValidateToken(string token)
{
    var parts = token.Split('.');
    if (parts.Length != 3) return false;
    
    // Vérification manuelle de la signature avec HMAC
    var expectedSignature = CreateHmacSha256Signature(...);
    if (signature != expectedSignature) return false;
    
    // Vérification manuelle de l'expiration
    var payload = JsonSerializer.Deserialize<JsonElement>(payloadJson);
    // ...
}
```

#### Après (✅ Validation standard)
```csharp
public bool ValidateToken(string token)
{
    try
    {
        // ✅ Utilisation de JwtSecurityTokenHandler
        var tokenHandler = new JwtSecurityTokenHandler();
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = securityKey,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
        return true;
    }
    catch
    {
        return false;
    }
}
```

### 3️⃣ Synchronisation de la clé secrète dans Program.cs

#### Avant (❌ Clé différente)
```csharp
IssuerSigningKey = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"] ?? "...")
)
```

#### Après (✅ Même clé)
```csharp
IssuerSigningKey = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? "...")
)
```

---

## 🔑 Clé secrète unifiée

### Configuration dans appsettings.json

```json
{
  "Jwt": {
    "SecretKey": "KelasiNaBiso-SecretKey-2025-V1-Ultra-Secure-Key-For-JWT-Token-Generation",
    "Issuer": "KelasiNaBisoAPI",
    "Audience": "KelasiNaBisoClient",
    "ExpirationMinutes": 1440
  }
}
```

### Utilisée par :
- ✅ `SimpleJwtService.cs` (génération du token)
- ✅ `Program.cs` (validation du token par le middleware)

---

## 🧪 Comment tester la correction

### Étape 1 : Générer un NOUVEAU token

```http
POST https://localhost:7105/api/Utilisateur/authentifier
Content-Type: application/json

{
  "emailOuTelephone": "superadmin@kelasinabiso.cd",
  "motDePasse": "Super-Admin"
}
```

**Résultat attendu :**
```json
{
  "success": true,
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiw...",
  "tokenType": "Bearer",
  "expiresIn": 86400
}
```

### Étape 2 : Tester avec le nouveau token

```http
GET https://localhost:7105/api/AffectationCours
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiw...
```

**Résultat attendu :**
```
200 OK
[données des affectations de cours]
```

### Étape 3 : Tester sans token (doit échouer)

```http
GET https://localhost:7105/api/AffectationCours
```

**Résultat attendu :**
```
401 Unauthorized
```

---

## 📊 Comparaison avant/après

| Aspect | Avant | Après |
|--------|-------|-------|
| **Génération token** | ❌ Manuel (Base64, HMAC custom) | ✅ JwtSecurityTokenHandler |
| **Format token** | ❌ Personnalisé | ✅ Standard JWT |
| **Validation** | ❌ Manuelle | ✅ JwtSecurityTokenHandler |
| **Clé secrète** | ❌ Incohérente | ✅ Synchronisée (`Jwt:SecretKey`) |
| **Compatibilité** | ❌ Incompatible avec middleware | ✅ Compatible |
| **Résultat** | ❌ 401 Unauthorized | ✅ 200 OK |

---

## 🎯 Avantages de la solution

### ✅ Utilisation de bibliothèques standard
- `JwtSecurityTokenHandler` : Bibliothèque officielle Microsoft
- Gestion automatique du format JWT
- Validation robuste et éprouvée

### ✅ Compatibilité totale
- Token généré compatible avec le middleware ASP.NET Core
- Pas de conversion ou adaptation nécessaire
- Support natif dans Swagger

### ✅ Sécurité renforcée
- Même clé pour génération et validation
- Validation stricte de la signature
- Vérification de l'expiration

### ✅ Maintenabilité
- Code plus simple et lisible
- Moins de code personnalisé à maintenir
- Utilisation des standards de l'industrie

---

## ⚠️ Points importants

### 1️⃣ Ancien token invalide

Les tokens générés **avant** cette correction ne fonctionneront **plus** car :
- Format différent (manuel vs standard)
- Signature différente

**Solution** : Générer un nouveau token avec `/api/Utilisateur/authentifier`

### 2️⃣ Clé secrète identique

La même clé `Jwt:SecretKey` est maintenant utilisée pour :
- Générer le token (SimpleJwtService)
- Valider le token (Program.cs middleware)

### 3️⃣ Format de token standard

Le nouveau format est :
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1h...
```

Décodé, il contient :
- **Header** : `{"alg":"HS256","typ":"JWT"}`
- **Payload** : `{"sub":"1","email":"admin@...","name":"...","exp":...}`
- **Signature** : Calculée avec HMAC-SHA256

---

## 📁 Fichiers modifiés

| Fichier | Modification | Ligne |
|---------|--------------|-------|
| `Services/SimpleJwtService.cs` | Utilisation de `JwtSecurityTokenHandler` | 32-72 |
| `Services/SimpleJwtService.cs` | Validation avec `JwtSecurityTokenHandler` | 74-99 |
| `Services/SimpleJwtService.cs` | Suppression méthodes manuelles | - |
| `Program.cs` | Synchronisation clé `Jwt:SecretKey` | 30 |

---

## ✅ Statut : CORRIGÉ

**Date :** 25 octobre 2025  
**Problème :** Token JWT invalide après authentification  
**Solution :** Utilisation de `JwtSecurityTokenHandler` + Synchronisation des clés  
**Test :** ✅ À effectuer par l'utilisateur

---

## 🧪 Checklist de test

- [ ] Générer un nouveau token via `/api/Utilisateur/authentifier`
- [ ] Copier le `accessToken` retourné
- [ ] Tester un endpoint protégé (ex: `/api/AffectationCours`) avec le token
- [ ] Vérifier que le résultat est `200 OK` (au lieu de `401 Unauthorized`)
- [ ] Tester sans token → Doit retourner `401 Unauthorized`
- [ ] Tester avec un token invalide → Doit retourner `401 Unauthorized`

---

**🎯 L'authentification JWT devrait maintenant fonctionner parfaitement !**

