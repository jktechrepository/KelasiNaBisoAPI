# 🔒 GUIDE RATE LIMITING - Protection Brute-Force

## 📋 Vue d'ensemble

**Objectif** : Protéger l'API contre les abus et attaques  
**Priorité** : ⭐⭐⭐ CRITIQUE pour la production  
**Temps d'implémentation** : 30 minutes (critique) à 2h (complet)  
**Date** : 1er novembre 2025

---

## 🎯 OBJECTIFS DE SÉCURITÉ

### Menaces à protéger

| Menace | Sans Rate Limit | Avec Rate Limit |
|--------|----------------|-----------------|
| **Brute-force login** | 1000 tentatives/min ❌ | 5 tentatives/min ✅ |
| **DDoS API** | 10000 req/min = crash ❌ | 100 req/min max ✅ |
| **Abus SMS** | 1000 SMS = 500$ 💸 | 10 SMS/h max ✅ |
| **Scraping données** | Base complète volée ❌ | Détecté et bloqué ✅ |

---

## 🚀 OPTION 1 : AspNetCoreRateLimit (RECOMMANDÉ ⭐⭐⭐)

### Installation

```bash
dotnet add package AspNetCoreRateLimit
```

### Configuration dans `Program.cs`

```csharp
using AspNetCoreRateLimit;

var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════════════════════
// 🔒 RATE LIMITING - Protection contre abus et brute-force
// ═══════════════════════════════════════════════════════════════════

// 1. Configuration du stockage en mémoire
builder.Services.AddMemoryCache();

// 2. Configuration du Rate Limiting par IP
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    // ✅ Règles GLOBALES (pour toute l'API)
    options.EnableEndpointRateLimiting = true;
    options.StackBlockedRequests = false; // Ne pas compter les requêtes bloquées
    options.HttpStatusCode = 429; // Too Many Requests
    options.RealIpHeader = "X-Real-IP"; // Pour Nginx/Load Balancer
    options.ClientIdHeader = "X-ClientId";

    // ✅ Règles générales par IP
    options.GeneralRules = new List<RateLimitRule>
    {
        // Limite globale : 100 requêtes par minute
        new RateLimitRule
        {
            Endpoint = "*",
            Period = "1m",
            Limit = 100
        },
        // Limite globale : 1000 requêtes par heure
        new RateLimitRule
        {
            Endpoint = "*",
            Period = "1h",
            Limit = 1000
        }
    };

    // ✅ Règles SPÉCIFIQUES par endpoint (endpoints critiques)
    options.EndpointWhitelist = new List<string>(); // Pas de whitelist par défaut
    options.ClientWhitelist = new List<string>(); // IPs en whitelist (admin, etc.)

    // Message personnalisé
    options.QuotaExceededMessage = "Limite de requêtes dépassée. Réessayez dans {0}.";
});

// 3. Configuration du Rate Limiting par Utilisateur (authentifié)
builder.Services.Configure<ClientRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = true;
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*",
            Period = "1m",
            Limit = 200 // Utilisateurs authentifiés = limite plus haute
        }
    };
});

// 4. Enregistrement des services
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

// 5. Inject HttpContextAccessor (requis par AspNetCoreRateLimit)
builder.Services.AddHttpContextAccessor();

Log.Information("✅ Rate Limiting configuré (AspNetCoreRateLimit)");

// ... (reste de la configuration) ...

var app = builder.Build();

// ✅ ACTIVATION du Rate Limiting (à mettre TÔT dans le pipeline)
app.UseIpRateLimiting();
Log.Information("✅ Rate Limiting activé");

// ... (reste du pipeline) ...
```

### Configuration dans `appsettings.json`

```json
{
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": true,
    "StackBlockedRequests": false,
    "RealIpHeader": "X-Real-IP",
    "ClientIdHeader": "X-ClientId",
    "HttpStatusCode": 429,
    "QuotaExceededMessage": "Limite de requêtes dépassée. Réessayez dans {0}.",
    
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "1m",
        "Limit": 100
      },
      {
        "Endpoint": "*",
        "Period": "1h",
        "Limit": 1000
      }
    ],

    "EndpointRules": [
      {
        "Endpoint": "POST:/api/Utilisateur/authentifier",
        "Period": "1m",
        "Limit": 5
      },
      {
        "Endpoint": "POST:/api/Utilisateur/authentifier",
        "Period": "1h",
        "Limit": 20
      },
      {
        "Endpoint": "POST:/api/Utilisateur/reinitialiser-mot-de-passe",
        "Period": "1h",
        "Limit": 3
      },
      {
        "Endpoint": "POST:/api/Agent/batch",
        "Period": "1h",
        "Limit": 10
      },
      {
        "Endpoint": "POST:/api/Eleve/batch",
        "Period": "1h",
        "Limit": 10
      },
      {
        "Endpoint": "POST:/api/Paiement/batch",
        "Period": "1h",
        "Limit": 20
      }
    ],

    "ClientWhitelist": [
      "127.0.0.1",
      "::1"
    ]
  }
}
```

### Réponse API quand limite dépassée

```http
HTTP/1.1 429 Too Many Requests
Content-Type: application/json
X-Rate-Limit-Limit: 5
X-Rate-Limit-Remaining: 0
X-Rate-Limit-Reset: 2025-11-01T14:32:00Z
Retry-After: 42

{
  "message": "Limite de requêtes dépassée. Réessayez dans 42 secondes."
}
```

---

## 🚀 OPTION 2 : Rate Limiting Natif .NET 7+ (SIMPLE ⭐⭐)

### Avantages
- ✅ **Natif** (pas de package externe)
- ✅ **Performant** (intégré dans ASP.NET Core)
- ✅ **Simple** pour cas basiques

### Inconvénients
- ⚠️ Moins flexible qu'AspNetCoreRateLimit
- ⚠️ Pas de règles par endpoint facilement

### Configuration dans `Program.cs`

```csharp
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════════════════════
// 🔒 RATE LIMITING NATIF .NET 7+
// ═══════════════════════════════════════════════════════════════════

builder.Services.AddRateLimiter(options =>
{
    // 1. Politique globale (Fixed Window)
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));

    // 2. Politique stricte pour login (5 tentatives/minute)
    options.AddFixedWindowLimiter("login", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0; // Pas de queue
    });

    // 3. Politique pour endpoints batch (10/heure)
    options.AddFixedWindowLimiter("batch", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromHours(1);
        limiterOptions.QueueLimit = 0;
    });

    // Message quand limite dépassée
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            message = "Trop de requêtes. Réessayez plus tard.",
            retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter) 
                ? retryAfter.TotalSeconds 
                : null
        }, cancellationToken: token);
    };
});

var app = builder.Build();

// ✅ ACTIVATION du Rate Limiting
app.UseRateLimiter();
```

### Application sur endpoints spécifiques

```csharp
[ApiController]
[Route("api/[controller]")]
public class UtilisateurController : ControllerBase
{
    // ✅ Appliquer la politique "login" (5 req/min)
    [HttpPost("authentifier")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Authentifier([FromBody] LoginRequest request)
    {
        // ...
    }

    // ✅ Pas de rate limiting spécifique (global s'applique)
    [HttpGet]
    public async Task<IActionResult> GetUtilisateurs()
    {
        // ...
    }
}

[ApiController]
[Route("api/[controller]")]
public class AgentController : ControllerBase
{
    // ✅ Appliquer la politique "batch" (10 req/h)
    [HttpPost("batch")]
    [EnableRateLimiting("batch")]
    public async Task<IActionResult> CreateBatch([FromBody] List<Agent> agents)
    {
        // ...
    }
}
```

---

## 🚀 OPTION 3 : Rate Limiting Custom (AVANCÉ ⭐)

Pour un contrôle TOTAL, créer un middleware custom.

### Middleware personnalisé

```csharp
public class CustomRateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CustomRateLimitMiddleware> _logger;

    // Configuration
    private const int MAX_REQUESTS_PER_MINUTE = 100;
    private const int LOGIN_MAX_ATTEMPTS = 5;

    public CustomRateLimitMiddleware(
        RequestDelegate next, 
        IMemoryCache cache, 
        ILogger<CustomRateLimitMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var endpoint = $"{context.Request.Method}:{context.Request.Path}";
        var cacheKey = $"rate_limit_{clientIp}_{endpoint}";

        // Endpoints critiques avec limite stricte
        bool isLoginEndpoint = endpoint.Contains("/authentifier", StringComparison.OrdinalIgnoreCase);
        int maxRequests = isLoginEndpoint ? LOGIN_MAX_ATTEMPTS : MAX_REQUESTS_PER_MINUTE;

        // Récupérer le compteur de requêtes
        var requestCount = _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            return 0;
        });

        // Vérifier la limite
        if (requestCount >= maxRequests)
        {
            _logger.LogWarning($"🚨 Rate limit dépassé : {clientIp} → {endpoint} ({requestCount} req)");

            context.Response.StatusCode = 429;
            context.Response.Headers.Append("X-Rate-Limit-Limit", maxRequests.ToString());
            context.Response.Headers.Append("X-Rate-Limit-Remaining", "0");
            context.Response.Headers.Append("Retry-After", "60");

            await context.Response.WriteAsJsonAsync(new
            {
                message = $"Limite de {maxRequests} requêtes par minute dépassée.",
                retryAfter = 60
            });
            return;
        }

        // Incrémenter le compteur
        _cache.Set(cacheKey, requestCount + 1, TimeSpan.FromMinutes(1));

        // Headers informatifs
        context.Response.Headers.Append("X-Rate-Limit-Limit", maxRequests.ToString());
        context.Response.Headers.Append("X-Rate-Limit-Remaining", (maxRequests - requestCount - 1).ToString());

        await _next(context);
    }
}

// Enregistrement
public static class CustomRateLimitMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomRateLimit(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CustomRateLimitMiddleware>();
    }
}

// Dans Program.cs
app.UseCustomRateLimit();
```

---

## 📊 RÈGLES RECOMMANDÉES PAR ENDPOINT

| Endpoint | Limite/minute | Limite/heure | Justification |
|----------|---------------|--------------|---------------|
| `POST /api/Utilisateur/authentifier` | **5** | **20** | 🔒 Anti brute-force |
| `POST /api/Utilisateur/reinitialiser-mot-de-passe` | **2** | **3** | 🔒 Abus email |
| `POST /api/Agent/batch` | **5** | **10** | 💾 Charge DB |
| `POST /api/Eleve/batch` | **5** | **10** | 💾 Charge DB |
| `POST /api/Paiement/batch` | **10** | **20** | 💾 Charge DB |
| `POST /api/Paiement` (SMS) | **10** | **50** | 💸 Coût SMS |
| `GET /api/*` (global) | **100** | **1000** | 📊 Performance |
| `POST /api/*` (global) | **50** | **500** | 📊 Performance |

---

## 🧪 TESTER LE RATE LIMITING

### Script PowerShell de test

```powershell
# Test brute-force login (doit bloquer après 5 tentatives)
$apiUrl = "https://localhost:7102/api"

Write-Host "🧪 Test Rate Limiting - Brute-force login`n" -ForegroundColor Yellow

for ($i = 1; $i -le 10; $i++) {
    try {
        $response = Invoke-WebRequest -Uri "$apiUrl/Utilisateur/authentifier" `
                                      -Method POST `
                                      -Body '{"email":"test@test.com","password":"wrong"}' `
                                      -ContentType "application/json" `
                                      -ErrorAction Stop

        $limitRemaining = $response.Headers["X-Rate-Limit-Remaining"]
        Write-Host "  ✅ Tentative $i : OK (Restant: $limitRemaining)" -ForegroundColor Green
    }
    catch {
        if ($_.Exception.Response.StatusCode -eq 429) {
            Write-Host "  🚫 Tentative $i : BLOQUÉ (Rate limit atteint)" -ForegroundColor Red
        }
        else {
            Write-Host "  ⚠️  Tentative $i : Erreur $($_.Exception.Response.StatusCode)" -ForegroundColor Yellow
        }
    }
    
    Start-Sleep -Milliseconds 200
}

Write-Host "`n✅ Si bloqué après 5 tentatives → Rate limiting fonctionne !" -ForegroundColor Green
```

---

## ✅ RECOMMANDATION FINALE

### Pour KelasiNaBiso, je recommande :

**🥇 OPTION 1 : AspNetCoreRateLimit** 
- ✅ Facile à configurer (30 min)
- ✅ Règles flexibles par endpoint
- ✅ Très mature et stable
- ✅ Parfait pour 95% des cas

### À implémenter maintenant (CRITIQUE) :
1. ✅ Protection login (`/authentifier` → 5 req/min)
2. ✅ Protection reset password (3 req/h)
3. ✅ Protection endpoints batch (10 req/h)
4. ✅ Whitelist localhost (dev)

### À considérer plus tard (selon croissance) :
- Redis pour rate limiting distribué (multi-serveurs)
- Blacklist d'IPs suspectes
- Rate limiting par utilisateur (en plus de par IP)
- Monitoring des tentatives de brute-force

---

**Tu veux que j'implémente AspNetCoreRateLimit maintenant ?** 🚀  
C'est vraiment critique pour la production et ça prend 30 minutes max !

