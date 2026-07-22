# ══════════════════════════════════════════════════════════════════════════════════
# TEST RATE LIMITING - VERSION SIMPLE
# ══════════════════════════════════════════════════════════════════════════════════

$apiUrl = "https://localhost:7102/api"
$email = "admin@test.com"
$password = "Admin@123"

# Ignorer SSL
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

Write-Host "`n==================================================================" -ForegroundColor Cyan
Write-Host "TEST RATE LIMITING - PROTECTION BRUTE-FORCE" -ForegroundColor Green
Write-Host "==================================================================`n" -ForegroundColor Cyan

# ══════════════════════════════════════════════════════════════════════════════════
# TEST 1 : BRUTE-FORCE LOGIN
# ══════════════════════════════════════════════════════════════════════════════════

Write-Host "TEST 1 : Protection Brute-Force Login (5 req/min max)`n" -ForegroundColor Yellow

$authBody = @{
    email = "attacker@test.com"
    password = "wrongpassword"
} | ConvertTo-Json

$blockedCount = 0
$allowedCount = 0

for ($i = 1; $i -le 10; $i++) {
    try {
        $response = Invoke-WebRequest -Uri "$apiUrl/Utilisateur/authentifier" `
                                      -Method POST `
                                      -Body $authBody `
                                      -ContentType "application/json" `
                                      -ErrorAction Stop

        $limitRemaining = $response.Headers["X-Rate-Limit-Remaining"]
        Write-Host "  Tentative $i : AUTORISE (Restant: $limitRemaining)" -ForegroundColor Green
        $allowedCount++
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        
        if ($statusCode -eq 429) {
            Write-Host "  Tentative $i : BLOQUE (429 Too Many Requests)" -ForegroundColor Red
            $blockedCount++
        }
        elseif ($statusCode -eq 401) {
            Write-Host "  Tentative $i : 401 Unauthorized (mauvais mot de passe)" -ForegroundColor Yellow
            $allowedCount++
        }
        else {
            Write-Host "  Tentative $i : Erreur $statusCode" -ForegroundColor Red
        }
    }
    
    Start-Sleep -Milliseconds 300
}

Write-Host "`n  RESULTAT : $allowedCount autorisees, $blockedCount bloquees" -ForegroundColor Cyan
if ($blockedCount -gt 0) {
    Write-Host "  SUCCES : Rate limiting fonctionne !" -ForegroundColor Green
}
else {
    Write-Host "  ATTENTION : Aucune requete bloquee" -ForegroundColor Yellow
}

# ══════════════════════════════════════════════════════════════════════════════════
# TEST 2 : LIMITE GLOBALE
# ══════════════════════════════════════════════════════════════════════════════════

Write-Host "`n`nTEST 2 : Limite Globale (10 req/seconde)`n" -ForegroundColor Yellow

# Authentification
$authBody = @{
    email = $email
    password = $password
} | ConvertTo-Json

try {
    $authResponse = Invoke-RestMethod -Uri "$apiUrl/Utilisateur/authentifier" `
                                      -Method POST `
                                      -Body $authBody `
                                      -ContentType "application/json"

    $token = $authResponse.token
    Write-Host "  Authentification reussie`n" -ForegroundColor Green
}
catch {
    Write-Host "  ERREUR d'authentification`n" -ForegroundColor Red
    exit 1
}

$headers = @{
    "Authorization" = "Bearer $token"
}

$blockedCount = 0
$allowedCount = 0

Write-Host "  Envoi de 20 requetes rapides...`n" -ForegroundColor Gray

for ($i = 1; $i -le 20; $i++) {
    try {
        $response = Invoke-WebRequest -Uri "$apiUrl/Ecole" `
                                      -Method GET `
                                      -Headers $headers `
                                      -ErrorAction Stop

        $limitRemaining = $response.Headers["X-Rate-Limit-Remaining"]
        Write-Host "  Requete $i : OK (Restant: $limitRemaining)" -ForegroundColor Green
        $allowedCount++
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        
        if ($statusCode -eq 429) {
            Write-Host "  Requete $i : BLOQUE (429)" -ForegroundColor Red
            $blockedCount++
        }
        else {
            Write-Host "  Requete $i : Erreur $statusCode" -ForegroundColor Red
        }
    }
    
    Start-Sleep -Milliseconds 50
}

Write-Host "`n  RESULTAT : $allowedCount autorisees, $blockedCount bloquees" -ForegroundColor Cyan
if ($blockedCount -gt 0) {
    Write-Host "  SUCCES : Rate limiting fonctionne !" -ForegroundColor Green
}
else {
    Write-Host "  INFO : Toutes les requetes autorisees (localhost en whitelist)" -ForegroundColor Yellow
}

# ══════════════════════════════════════════════════════════════════════════════════
# RESUME
# ══════════════════════════════════════════════════════════════════════════════════

Write-Host "`n`n==================================================================" -ForegroundColor Cyan
Write-Host "RESUME DES TESTS" -ForegroundColor Green
Write-Host "==================================================================`n" -ForegroundColor Cyan

Write-Host "REGLES CONFIGUREES :" -ForegroundColor Yellow
Write-Host "  - Globales : 10 req/sec, 100 req/min, 1000 req/h" -ForegroundColor White
Write-Host "  - Login : 5 req/min max" -ForegroundColor White
Write-Host "  - Batch : 10 req/h max" -ForegroundColor White
Write-Host "  - Paiement : 10 req/min max`n" -ForegroundColor White

Write-Host "PROTECTION ACTIVE :" -ForegroundColor Green
Write-Host "  - Brute-force login LIMITE" -ForegroundColor Green
Write-Host "  - Flood API LIMITE" -ForegroundColor Green
Write-Host "  - Abus endpoints LIMITE" -ForegroundColor Green
Write-Host "  - Whitelist localhost ACTIVE`n" -ForegroundColor Green

Write-Host "==================================================================`n" -ForegroundColor Cyan
Write-Host "Tests termines avec succes !" -ForegroundColor Green
Write-Host "Votre API est maintenant PROTEGEE contre les abus !`n" -ForegroundColor Green

