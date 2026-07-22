# Vérification rapide CORS - API KelasiNaBiso
Write-Host "🔍 Vérification rapide CORS..." -ForegroundColor Cyan

$API_URL = "http://192.168.100.17:5001"

# Test 1: Connectivité de base
Write-Host "`n1️⃣ Test de connectivité..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$API_URL/api/Utilisateur" -Method GET -TimeoutSec 5
    Write-Host "✅ API accessible" -ForegroundColor Green
} catch {
    Write-Host "❌ API non accessible - Vérifiez que l'API est démarrée" -ForegroundColor Red
    exit 1
}

# Test 2: Headers CORS
Write-Host "`n2️⃣ Test des headers CORS..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$API_URL/api/Utilisateur/authentifier" -Method OPTIONS -Headers @{
        "Access-Control-Request-Method" = "POST"
        "Access-Control-Request-Headers" = "Content-Type"
        "Origin" = "http://192.168.100.19:5501"
    } -TimeoutSec 5
    
    $corsOrigin = $response.Headers["Access-Control-Allow-Origin"]
    $corsMethods = $response.Headers["Access-Control-Allow-Methods"]
    $corsCredentials = $response.Headers["Access-Control-Allow-Credentials"]
    
    Write-Host "📋 Headers CORS reçus:" -ForegroundColor Cyan
    Write-Host "   Origin: $corsOrigin" -ForegroundColor White
    Write-Host "   Methods: $corsMethods" -ForegroundColor White
    Write-Host "   Credentials: $corsCredentials" -ForegroundColor White
    
    if ($corsOrigin -and $corsMethods -and $corsCredentials) {
        Write-Host "✅ Headers CORS corrects" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Headers CORS manquants" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Erreur lors du test CORS: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Authentification
Write-Host "`n3️⃣ Test d'authentification..." -ForegroundColor Yellow
try {
    $body = @{
        emailOuTelephone = "admin@example.com"
        motDePasse = "password123"
    } | ConvertTo-Json
    
    $response = Invoke-WebRequest -Uri "$API_URL/api/Utilisateur/authentifier" -Method POST -Body $body -Headers @{
        "Content-Type" = "application/json"
    } -TimeoutSec 5
    
    Write-Host "✅ Authentification réussie" -ForegroundColor Green
} catch {
    Write-Host "❌ Erreur d'authentification: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n✅ Vérification terminée!" -ForegroundColor Green
