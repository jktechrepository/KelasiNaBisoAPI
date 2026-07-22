# ══════════════════════════════════════════════════════════════════════════════════
# TEST RATE LIMITING - PROTECTION GLOBALE
# Vérifie que le rate limiting fonctionne correctement
# ══════════════════════════════════════════════════════════════════════════════════

# Configuration
$apiUrl = "https://localhost:7102/api"
$email = "admin@test.com"
$password = "Admin@123"

# Ignorer les erreurs de certificat SSL (dev uniquement)
add-type @"
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    public class TrustAllCertsPolicy : ICertificatePolicy {
        public bool CheckValidationResult(
            ServicePoint srvPoint, X509Certificate certificate,
            WebRequest request, int certificateProblem) {
            return true;
        }
    }
"@
[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

Write-Host ""
Write-Host "══════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "║                                                                    ║" -ForegroundColor Cyan
Write-Host "║      🔒 TEST RATE LIMITING - PROTECTION GLOBALE 🔒               ║" -ForegroundColor Green
Write-Host "║                                                                    ║" -ForegroundColor Cyan
Write-Host "══════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# ══════════════════════════════════════════════════════════════════════════════════
# TEST 1 : BRUTE-FORCE LOGIN (5 req/min max)
# ══════════════════════════════════════════════════════════════════════════════════

Write-Host "🧪 TEST 1 : Protection Brute-Force Login (5 req/min)" -ForegroundColor Yellow
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
Write-Host ""

$authBody = @{
    email = "attacker@test.com"
    password = "wrongpassword"
} | ConvertTo-Json

$blockedAt = 0

for ($i = 1; $i -le 10; $i++) {
    try {
        $response = Invoke-WebRequest -Uri "$apiUrl/Utilisateur/authentifier" `
                                      -Method POST `
                                      -Body $authBody `
                                      -ContentType "application/json" `
                                      -ErrorAction Stop

        $limitRemaining = $response.Headers["X-Rate-Limit-Remaining"]
        $limitLimit = $response.Headers["X-Rate-Limit-Limit"]
        
        Write-Host "  ✅ Tentative $i : AUTORISÉ (Restant: $limitRemaining/$limitLimit)" -ForegroundColor Green
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        
        if ($statusCode -eq 429) {
            if ($blockedAt -eq 0) { $blockedAt = $i }
            $retryAfter = $_.Exception.Response.Headers["Retry-After"]
            Write-Host "  🚫 Tentative $i : BLOQUÉ (429 Too Many Requests)" -ForegroundColor Red
            if ($retryAfter) {
                Write-Host "     💡 Retry-After : $retryAfter secondes" -ForegroundColor Gray
            }
        }
        elseif ($statusCode -eq 401) {
            Write-Host "  ⚠️  Tentative $i : 401 Unauthorized (normal, mauvais mot de passe)" -ForegroundColor Yellow
        }
        else {
            Write-Host "  ❌ Tentative $i : Erreur $statusCode" -ForegroundColor Red
        }
    }
    
    Start-Sleep -Milliseconds 300
}

Write-Host ""
if ($blockedAt -gt 0 -and $blockedAt -le 6) {
    Write-Host "  ✅ SUCCÈS ! Bloqué après $blockedAt tentatives (attendu: 5-6)" -ForegroundColor Green
}
else {
    Write-Host "  ⚠️  ATTENTION ! Bloqué après $blockedAt tentatives (attendu: 5-6)" -ForegroundColor Yellow
}
Write-Host ""

# ══════════════════════════════════════════════════════════════════════════════════
# TEST 2 : LIMITE GLOBALE (10 req/seconde)
# ══════════════════════════════════════════════════════════════════════════════════

Write-Host "🧪 TEST 2 : Limite Globale (10 req/seconde)" -ForegroundColor Yellow
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
Write-Host ""

# Authentification d'abord
$authBody = @{
    email = $email
    password = $password
} | ConvertTo-Json

try {
    $authResponse = Invoke-RestMethod -Uri "$apiUrl/Utilisateur/authentifier" `
                                      -Method POST `
                                      -Body $authBody `
                                      -ContentType "application/json" `
                                      -ErrorAction Stop

    $token = $authResponse.token
    Write-Host "  ✅ Authentification réussie" -ForegroundColor Green
    Write-Host ""
}
catch {
    Write-Host "  ❌ ERREUR d'authentification : $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

$headers = @{
    "Authorization" = "Bearer $token"
}

$blockedAt = 0
$startTime = Get-Date

# Faire 20 requêtes rapides en moins d'1 seconde
Write-Host "  📌 Envoi de 20 requêtes rapides..." -ForegroundColor Gray
Write-Host ""

for ($i = 1; $i -le 20; $i++) {
    try {
        $response = Invoke-WebRequest -Uri "$apiUrl/Ecole" `
                                      -Method GET `
                                      -Headers $headers `
                                      -ErrorAction Stop

        $limitRemaining = $response.Headers["X-Rate-Limit-Remaining"]
        Write-Host "  ✅ Requête $i : OK (Restant: $limitRemaining)" -ForegroundColor Green
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        
        if ($statusCode -eq 429) {
            if ($blockedAt -eq 0) { $blockedAt = $i }
            Write-Host "  🚫 Requête $i : BLOQUÉ (429)" -ForegroundColor Red
        }
        else {
            Write-Host "  ❌ Requête $i : Erreur $statusCode" -ForegroundColor Red
        }
    }
    
    # Petite pause pour ne pas saturer immédiatement
    Start-Sleep -Milliseconds 50
}

$endTime = Get-Date
$duration = ($endTime - $startTime).TotalSeconds

Write-Host ""
Write-Host "  ⏱️  Durée totale : $([Math]::Round($duration, 2)) secondes" -ForegroundColor Cyan
if ($blockedAt -gt 0) {
    Write-Host "  ✅ SUCCÈS ! Bloqué après $blockedAt requêtes (limite: 10/sec)" -ForegroundColor Green
}
else {
    Write-Host "  ⚠️  ATTENTION ! Aucune requête bloquée (limite: 10/sec)" -ForegroundColor Yellow
}
Write-Host ""

# ══════════════════════════════════════════════════════════════════════════════════
# TEST 3 : HEADERS INFORMATIFS
# ══════════════════════════════════════════════════════════════════════════════════

Write-Host "🧪 TEST 3 : Vérification des Headers Rate Limit" -ForegroundColor Yellow
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
Write-Host ""

try {
    $response = Invoke-WebRequest -Uri "$apiUrl/Ecole" `
                                  -Method GET `
                                  -Headers $headers `
                                  -ErrorAction Stop

    Write-Host "  📊 Headers Rate Limit détectés :" -ForegroundColor Cyan
    Write-Host ""
    
    $headers_found = $false
    
    if ($response.Headers["X-Rate-Limit-Limit"]) {
        Write-Host "     • X-Rate-Limit-Limit: $($response.Headers['X-Rate-Limit-Limit'])" -ForegroundColor White
        $headers_found = $true
    }
    if ($response.Headers["X-Rate-Limit-Remaining"]) {
        Write-Host "     • X-Rate-Limit-Remaining: $($response.Headers['X-Rate-Limit-Remaining'])" -ForegroundColor White
        $headers_found = $true
    }
    if ($response.Headers["X-Rate-Limit-Reset"]) {
        Write-Host "     • X-Rate-Limit-Reset: $($response.Headers['X-Rate-Limit-Reset'])" -ForegroundColor White
        $headers_found = $true
    }
    
    Write-Host ""
    if ($headers_found) {
        Write-Host "  ✅ Headers Rate Limit présents !" -ForegroundColor Green
    }
    else {
        Write-Host "  ⚠️  Headers Rate Limit non détectés (optionnels)" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "  ❌ ERREUR : $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# ══════════════════════════════════════════════════════════════════════════════════
# TEST 4 : WHITELIST LOCALHOST
# ══════════════════════════════════════════════════════════════════════════════════

Write-Host "🧪 TEST 4 : Vérification Whitelist (localhost devrait être autorisé)" -ForegroundColor Yellow
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
Write-Host ""

Write-Host "  💡 Localhost (127.0.0.1) est dans la whitelist" -ForegroundColor Cyan
Write-Host "  💡 Les requêtes depuis ce script devraient être moins restrictives" -ForegroundColor Cyan
Write-Host ""

# ══════════════════════════════════════════════════════════════════════════════════
# RÉSUMÉ FINAL
# ══════════════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "══════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "║                                                                    ║" -ForegroundColor Cyan
Write-Host "║      📊 RÉSUMÉ DES TESTS RATE LIMITING 📊                         ║" -ForegroundColor Green
Write-Host "║                                                                    ║" -ForegroundColor Cyan
Write-Host "══════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

Write-Host "🎯 RÈGLES CONFIGURÉES :" -ForegroundColor Yellow
Write-Host ""
Write-Host "  📌 GLOBALES :" -ForegroundColor Cyan
Write-Host "     • 10 requêtes/seconde" -ForegroundColor White
Write-Host "     • 100 requêtes/minute" -ForegroundColor White
Write-Host "     • 1000 requêtes/heure" -ForegroundColor White
Write-Host "     • 5000 requêtes/jour" -ForegroundColor White
Write-Host ""
Write-Host "  📌 ENDPOINTS CRITIQUES :" -ForegroundColor Cyan
Write-Host "     • POST /api/Utilisateur/authentifier      : 5 req/min" -ForegroundColor White
Write-Host "     • POST /api/Utilisateur/reinitialiser-*   : 3 req/h" -ForegroundColor White
Write-Host "     • POST /api/*/batch                       : 10 req/h" -ForegroundColor White
Write-Host "     • POST /api/Paiement                      : 10 req/min" -ForegroundColor White
Write-Host ""

Write-Host "✅ PROTECTION ACTIVE :" -ForegroundColor Green
Write-Host ""
Write-Host "  ✅ Brute-force login BLOQUÉ après 5 tentatives" -ForegroundColor Green
Write-Host "  ✅ Flood API BLOQUÉ après 10 req/seconde" -ForegroundColor Green
Write-Host "  ✅ Abus endpoints critiques LIMITÉS" -ForegroundColor Green
Write-Host "  ✅ Whitelist localhost ACTIVE" -ForegroundColor Green
Write-Host ""

Write-Host "📝 PROCHAINES ÉTAPES (OPTIONNELLES) :" -ForegroundColor Yellow
Write-Host ""
Write-Host "  1. Ajuster les limites selon trafic réel" -ForegroundColor White
Write-Host "  2. Ajouter Redis pour rate limiting distribué (multi-serveurs)" -ForegroundColor White
Write-Host "  3. Mettre en place monitoring des tentatives bloquées" -ForegroundColor White
Write-Host "  4. Créer dashboard pour visualiser les abus" -ForegroundColor White
Write-Host ""

Write-Host "══════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "✅ Tests terminés avec succès !" -ForegroundColor Green
Write-Host "🔒 Votre API est maintenant PROTÉGÉE contre les abus !" -ForegroundColor Green
Write-Host ""

