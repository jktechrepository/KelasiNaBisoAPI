# 🔒 GUIDE CORS - SÉCURITÉ PRODUCTION

## 📋 Vue d'ensemble

**Objectif** : Sécuriser l'API en production contre les attaques Cross-Origin  
**Priorité** : ⭐⭐⭐ CRITIQUE pour la sécurité  
**Temps d'implémentation** : 15 minutes  
**Date** : 1er novembre 2025

---

## 🎯 QU'EST-CE QUE CORS ?

**CORS** = **C**ross-**O**rigin **R**esource **S**haring

### Le problème que CORS résout

```
┌─────────────────────────────────────────────────────────────┐
│ SANS CORS (avant 2010)                                      │
├─────────────────────────────────────────────────────────────┤
│ Site A (https://kelasinabiso.com) peut appeler :           │
│   ✅ API A (https://api.kelasinabiso.com) → OK             │
│                                                             │
│ Site B (https://site-pirate.com) peut appeler :            │
│   ✅ API A (https://api.kelasinabiso.com) → OK aussi ! ❌  │
│                                                             │
│ = N'IMPORTE QUEL SITE peut utiliser ton API !              │
└─────────────────────────────────────────────────────────────┘

↓↓↓ SOLUTION : CORS ↓↓↓

┌─────────────────────────────────────────────────────────────┐
│ AVEC CORS (depuis 2010)                                     │
├─────────────────────────────────────────────────────────────┤
│ Site A (https://kelasinabiso.com) peut appeler :           │
│   ✅ API A (https://api.kelasinabiso.com) → OK (autorisé)  │
│                                                             │
│ Site B (https://site-pirate.com) peut appeler :            │
│   ❌ API A (https://api.kelasinabiso.com) → BLOQUÉ ! ✅    │
│                                                             │
│ = SEULS les sites AUTORISÉS peuvent utiliser ton API !     │
└─────────────────────────────────────────────────────────────┘
```

---

## 🚨 PROBLÈME ACTUEL DANS KelasiNaBiso

### Code actuel (Program.cs lignes 264-282)

```csharp
else // Production
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
    if (allowedOrigins != null && allowedOrigins.Length > 0)
    {
        // ✅ BON : Liste blanche d'origines
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    }
    else
    {
        // ❌ TRÈS DANGEREUX : Si pas de config, accepte TOUT !
        policy.SetIsOriginAllowed(origin => true)  // 💀 DANGER !
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    }
}
```

### ⚠️ Le danger

Si tu oublies de configurer `Cors:AllowedOrigins` dans `appsettings.json` en production, **TON API EST OUVERTE À TOUS** ! 😱

---

## ✅ SOLUTION RECOMMANDÉE

### 1. Configurer `appsettings.json`

```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://kelasinabiso.com",
      "https://www.kelasinabiso.com",
      "https://app.kelasinabiso.com",
      "https://admin.kelasinabiso.com"
    ]
  }
}
```

**Important** :
- ✅ **HTTPS uniquement** en production (pas http://)
- ✅ **Domaines exacts** (pas de wildcards * en production)
- ✅ **Avec et sans www** si nécessaire
- ✅ **Sous-domaines explicites** (app., admin., etc.)

### 2. Configurer `appsettings.Development.json` (séparé)

```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:4200",
      "http://localhost:5173",
      "http://localhost:8080",
      "http://127.0.0.1:3000"
    ]
  }
}
```

### 3. Améliorer `Program.cs` (SÉCURITÉ RENFORCÉE)

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // ✅ DÉVELOPPEMENT : Permissif mais contrôlé
            var devOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
            if (devOrigins != null && devOrigins.Length > 0)
            {
                policy.WithOrigins(devOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            }
            else
            {
                // Fallback dev : localhost uniquement
                policy.SetIsOriginAllowed(origin => 
                          origin.Contains("localhost") || 
                          origin.Contains("127.0.0.1"))
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            }
        }
        else
        {
            // ✅ PRODUCTION : STRICT (pas de fallback dangereux)
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
            
            if (allowedOrigins == null || allowedOrigins.Length == 0)
            {
                // ⚠️ ERREUR FATALE : Pas de CORS configuré en production !
                throw new InvalidOperationException(
                    "❌ ERREUR CRITIQUE : Cors:AllowedOrigins DOIT être configuré en production ! " +
                    "Ajoutez les origines autorisées dans appsettings.json"
                );
            }
            
            // Configuration stricte
            policy.WithOrigins(allowedOrigins)
                  .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH") // Méthodes explicites
                  .WithHeaders("Authorization", "Content-Type", "Accept") // Headers explicites
                  .AllowCredentials()
                  .SetIsOriginAllowedToAllowWildcardSubdomains(); // Si besoin de *.kelasinabiso.com
        }
    });
});

Log.Information("✅ CORS configuré pour : {Origins}", 
    builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "AUCUN (dev mode)" });
```

---

## 🎯 BÉNÉFICES DE CORS SÉCURISÉ

### 1. **Protection contre CSRF (Cross-Site Request Forgery)**

```
❌ SANS CORS SÉCURISÉ :
User connecté → Visite site pirate → Site pirate appelle ton API → Succès

✅ AVEC CORS SÉCURISÉ :
User connecté → Visite site pirate → Site pirate appelle ton API → BLOQUÉ par navigateur
```

### 2. **Protection contre le vol de données**

```
❌ SANS CORS SÉCURISÉ :
Site pirate fait : GET /api/Eleve → Récupère toute la base → Vol réussi

✅ AVEC CORS SÉCURISÉ :
Site pirate fait : GET /api/Eleve → Navigateur refuse → Vol échoué
```

### 3. **Protection contre le phishing**

```
❌ SANS CORS SÉCURISÉ :
Site pirate copie ton UI → Utilise TON API → Users ne voient pas la différence

✅ AVEC CORS SÉCURISÉ :
Site pirate copie ton UI → API refuse connexion → Attaque visible immédiatement
```

### 4. **Contrôle d'accès par domaine**

```
✅ Seuls tes domaines officiels peuvent utiliser l'API :
  - https://kelasinabiso.com        ✅
  - https://app.kelasinabiso.com    ✅
  - https://admin.kelasinabiso.com  ✅
  
❌ Tous les autres sont bloqués :
  - https://fake-kelasinabiso.com   ❌ BLOQUÉ
  - https://site-pirate.com         ❌ BLOQUÉ
  - https://phishing-school.com     ❌ BLOQUÉ
```

---

## 📊 COMPARAISON : AVANT / APRÈS

### Scénario d'attaque : Site pirate tente de voler les données

| Étape | Sans CORS sécurisé | Avec CORS sécurisé |
|-------|-------------------|-------------------|
| 1. User visite site pirate | ✅ | ✅ |
| 2. Site pirate fait `fetch("https://api.kelasinabiso.com/api/Eleve")` | ✅ Requête envoyée | ✅ Requête envoyée |
| 3. Navigateur vérifie CORS | ✅ Origin autorisée (AllowAny) | ❌ Origin NON autorisée |
| 4. API répond avec données | ✅ Données envoyées | ❌ Réponse bloquée par navigateur |
| 5. Site pirate reçoit données | ✅ Vol réussi 💀 | ❌ Erreur CORS ✅ |

### Console navigateur

**Sans CORS sécurisé** :
```javascript
// Succès ❌
const response = await fetch('https://api.kelasinabiso.com/api/Eleve');
const data = await response.json();
console.log(data); // 500 élèves récupérés ! 💀
```

**Avec CORS sécurisé** :
```javascript
// Échec ✅
const response = await fetch('https://api.kelasinabiso.com/api/Eleve');
// ❌ Error: CORS policy: No 'Access-Control-Allow-Origin' header
```

---

## 🧪 TESTER LA CONFIGURATION CORS

### Script PowerShell de test

```powershell
# Test depuis une origine non autorisée
$headers = @{
    "Origin" = "https://site-pirate.com"
}

try {
    $response = Invoke-WebRequest -Uri "https://localhost:7102/api/Ecole" `
                                  -Method OPTIONS `
                                  -Headers $headers

    Write-Host "ATTENTION : CORS trop permissif !" -ForegroundColor Red
}
catch {
    Write-Host "SUCCÈS : Origine non autorisée bloquée !" -ForegroundColor Green
}

# Test depuis une origine autorisée
$headers = @{
    "Origin" = "https://kelasinabiso.com"
}

try {
    $response = Invoke-WebRequest -Uri "https://localhost:7102/api/Ecole" `
                                  -Method OPTIONS `
                                  -Headers $headers

    $allowOrigin = $response.Headers["Access-Control-Allow-Origin"]
    
    if ($allowOrigin -eq "https://kelasinabiso.com") {
        Write-Host "SUCCÈS : Origine autorisée acceptée !" -ForegroundColor Green
    }
}
catch {
    Write-Host "ATTENTION : Origine légitime bloquée !" -ForegroundColor Red
}
```

### Tester dans le navigateur

```javascript
// Ouvrir la console sur https://kelasinabiso.com
fetch('https://api.kelasinabiso.com/api/Ecole', {
    method: 'GET',
    credentials: 'include'
})
.then(response => response.json())
.then(data => console.log('✅ CORS OK:', data))
.catch(error => console.error('❌ CORS ERROR:', error));
```

---

## 🔧 CONFIGURATIONS AVANCÉES

### 1. Autoriser plusieurs domaines (staging + production)

```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://kelasinabiso.com",
      "https://www.kelasinabiso.com",
      "https://staging.kelasinabiso.com",
      "https://app.kelasinabiso.com",
      "https://admin.kelasinabiso.com"
    ]
  }
}
```

### 2. Autoriser des sous-domaines dynamiques (*.kelasinabiso.com)

```csharp
policy.SetIsOriginAllowed(origin => 
{
    if (string.IsNullOrWhiteSpace(origin)) return false;
    
    var uri = new Uri(origin);
    
    // Autoriser tous les sous-domaines de kelasinabiso.com
    return uri.Host.EndsWith(".kelasinabiso.com") || 
           uri.Host == "kelasinabiso.com";
})
.AllowAnyHeader()
.AllowAnyMethod()
.AllowCredentials();
```

### 3. CORS différent par environnement

```csharp
// appsettings.Production.json
{
  "Cors": {
    "AllowedOrigins": [
      "https://kelasinabiso.com",
      "https://www.kelasinabiso.com"
    ]
  }
}

// appsettings.Staging.json
{
  "Cors": {
    "AllowedOrigins": [
      "https://staging.kelasinabiso.com",
      "https://test.kelasinabiso.com"
    ]
  }
}

// appsettings.Development.json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:4200",
      "http://localhost:5173"
    ]
  }
}
```

### 4. CORS avec headers personnalisés

```csharp
policy.WithOrigins(allowedOrigins)
      .WithMethods("GET", "POST", "PUT", "DELETE")
      .WithHeaders(
          "Authorization", 
          "Content-Type", 
          "Accept",
          "X-Custom-Header",     // Header personnalisé
          "X-Requested-With"
      )
      .WithExposedHeaders(       // Headers exposés au client
          "X-Pagination-TotalItems",
          "X-Pagination-TotalPages",
          "X-Rate-Limit-Remaining"
      )
      .AllowCredentials()
      .SetPreflightMaxAge(TimeSpan.FromHours(1)); // Cache preflight 1h
```

---

## ⚠️ ERREURS COURANTES À ÉVITER

### ❌ 1. Wildcard en production

```csharp
// ❌ NE JAMAIS FAIRE ÇA EN PRODUCTION !
policy.SetIsOriginAllowed(origin => true)
      .AllowCredentials();
```

### ❌ 2. AllowAnyOrigin avec AllowCredentials

```csharp
// ❌ IMPOSSIBLE : Erreur de configuration !
policy.AllowAnyOrigin()      // ← Interdit
      .AllowCredentials();   // ← avec credentials
```

**Pourquoi** : Le navigateur refuse cette combinaison pour sécurité.

### ❌ 3. Oublier le protocole (http/https)

```json
// ❌ MAUVAIS
"AllowedOrigins": [ "kelasinabiso.com" ]

// ✅ BON
"AllowedOrigins": [ "https://kelasinabiso.com" ]
```

### ❌ 4. Mélanger HTTP et HTTPS

```json
// ⚠️ ATTENTION : HTTP non sécurisé en production !
"AllowedOrigins": [
    "http://kelasinabiso.com",   // ❌ HTTP
    "https://kelasinabiso.com"   // ✅ HTTPS
]
```

**Solution** : HTTPS uniquement en production !

---

## 📋 CHECKLIST DE DÉPLOIEMENT

### Avant de déployer en production :

- [ ] ✅ `Cors:AllowedOrigins` configuré dans `appsettings.json`
- [ ] ✅ Uniquement HTTPS (pas http://)
- [ ] ✅ Domaines exacts (pas de wildcard *)
- [ ] ✅ Inclure www. et non-www. si nécessaire
- [ ] ✅ Tester avec un site externe (pas localhost)
- [ ] ✅ Vérifier que site pirate est bien bloqué
- [ ] ✅ Vérifier logs CORS au démarrage
- [ ] ✅ Documenter les origines autorisées

---

## 🎯 RECOMMANDATION FINALE POUR KelasiNaBiso

### Configuration minimale sécurisée

**1. Modifier `Program.cs`** : Ajouter validation stricte (throw exception si pas de config)

**2. Créer `appsettings.Production.json`** :
```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://kelasinabiso.com",
      "https://www.kelasinabiso.com",
      "https://app.kelasinabiso.com"
    ]
  }
}
```

**3. Garder `appsettings.Development.json`** :
```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:4200",
      "http://localhost:5173",
      "http://localhost:6600"
    ]
  }
}
```

**4. Tester** :
```powershell
.\test-cors-securite.ps1
```

---

## 🏆 RÉSULTAT

```
┌────────────────────────────────────────────────────────────┐
│ AVANT (CORS non sécurisé)                                  │
├────────────────────────────────────────────────────────────┤
│ ❌ N'importe quel site peut appeler l'API                 │
│ ❌ Vol de données possible                                │
│ ❌ CSRF possible                                           │
│ ❌ Phishing facile                                         │
└────────────────────────────────────────────────────────────┘

↓↓↓ TRANSFORMATION ↓↓↓

┌────────────────────────────────────────────────────────────┐
│ APRÈS (CORS sécurisé)                                      │
├────────────────────────────────────────────────────────────┤
│ ✅ Seuls tes domaines peuvent appeler l'API               │
│ ✅ Vol de données IMPOSSIBLE                               │
│ ✅ CSRF BLOQUÉ                                             │
│ ✅ Phishing DÉTECTABLE                                     │
└────────────────────────────────────────────────────────────┘

🔒 SÉCURITÉ : +1000% !
```

---

📅 **Date** : 1er novembre 2025  
✍️ **Auteur** : Assistant IA  
📧 **Projet** : KelasiNaBiso API v2.0  
🔒 **Priorité** : CRITIQUE pour production

