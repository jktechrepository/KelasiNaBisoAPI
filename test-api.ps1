# Test API KelasiNaBiso - Diagnostic CORS
# Exécutez ce script depuis PowerShell

Write-Host "🔍 Test API KelasiNaBiso - Diagnostic CORS" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

$API_BASE_URL = "http://192.168.100.17:5001"

# Test 1: Vérifier si l'API est accessible
Write-Host "`n1️⃣ Test de connectivité..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$API_BASE_URL/api/Utilisateur" -Method GET -TimeoutSec 10
    Write-Host "✅ API accessible - Status: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "❌ API non accessible: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "💡 Vérifiez que l'API est démarrée avec 'dotnet run'" -ForegroundColor Yellow
    exit 1
}

# Test 2: Test de la requête preflight (OPTIONS)
Write-Host "`n2️⃣ Test de la requête preflight (OPTIONS)..." -ForegroundColor Yellow
try {
    $headers = @{
        "Access-Control-Request-Method" = "POST"
        "Access-Control-Request-Headers" = "Content-Type"
        "Origin" = "http://192.168.100.19:5501"
    }
    
    $response = Invoke-WebRequest -Uri "$API_BASE_URL/api/Utilisateur/authentifier" -Method OPTIONS -Headers $headers -TimeoutSec 10
    
    Write-Host "✅ Preflight réussi - Status: $($response.StatusCode)" -ForegroundColor Green
    
    # Vérifier les headers CORS
    Write-Host "📋 Headers CORS reçus:" -ForegroundColor Cyan
    $corsHeaders = @("Access-Control-Allow-Origin", "Access-Control-Allow-Methods", "Access-Control-Allow-Headers", "Access-Control-Allow-Credentials")
    
    foreach ($header in $corsHeaders) {
        $value = $response.Headers[$header]
        if ($value) {
            Write-Host "   $header`: $value" -ForegroundColor White
        }
    }
} catch {
    Write-Host "❌ Erreur preflight: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Test d'authentification
Write-Host "`n3️⃣ Test d'authentification..." -ForegroundColor Yellow
try {
    $body = @{
        emailOuTelephone = "admin@example.com"
        motDePasse = "password123"
    } | ConvertTo-Json
    
    $headers = @{
        "Content-Type" = "application/json"
    }
    
    $response = Invoke-WebRequest -Uri "$API_BASE_URL/api/Utilisateur/authentifier" -Method POST -Body $body -Headers $headers -TimeoutSec 10
    
    Write-Host "✅ Authentification réussie - Status: $($response.StatusCode)" -ForegroundColor Green
    
    $userData = $response.Content | ConvertFrom-Json
    Write-Host "👤 Utilisateur: $($userData.prenomUtilisateur) $($userData.nomUtilisateur)" -ForegroundColor White
    Write-Host "🏫 École: $($userData.nomEcole)" -ForegroundColor White
    
} catch {
    Write-Host "❌ Erreur authentification: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $errorContent = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($errorContent)
        $errorText = $reader.ReadToEnd()
        Write-Host "📄 Détails de l'erreur: $errorText" -ForegroundColor Red
    }
}

# Test 4: Test avec curl (si disponible)
Write-Host "`n4️⃣ Test avec curl..." -ForegroundColor Yellow
try {
    $curlCommand = "curl -X POST `"$API_BASE_URL/api/Utilisateur/authentifier`" -H `"Content-Type: application/json`" -d `"{`"emailOuTelephone`":`"admin@example.com`",`"motDePasse`":`"password123`"}`""
    Write-Host "🔧 Commande curl: $curlCommand" -ForegroundColor Gray
    
    $curlResult = Invoke-Expression $curlCommand 2>$null
    if ($curlResult) {
        Write-Host "✅ Test curl réussi" -ForegroundColor Green
    }
} catch {
    Write-Host "⚠️ Curl non disponible ou erreur" -ForegroundColor Yellow
}

# Test 5: Vérifier les logs de l'API
Write-Host "`n5️⃣ Vérification des logs..." -ForegroundColor Yellow
Write-Host "📝 Vérifiez la console où l'API tourne pour voir les logs" -ForegroundColor Cyan
Write-Host "🔍 Recherchez les erreurs CORS ou de configuration" -ForegroundColor Cyan

# Test 6: Test de Swagger
Write-Host "`n6️⃣ Test de Swagger..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$API_BASE_URL/swagger" -Method GET -TimeoutSec 10
    Write-Host "✅ Swagger accessible - Status: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "🌐 Ouvrez http://192.168.100.17:5001/swagger dans votre navigateur" -ForegroundColor Cyan
} catch {
    Write-Host "❌ Swagger non accessible: $($_.Exception.Message)" -ForegroundColor Red
}

# Résumé et recommandations
Write-Host "`n📊 Résumé et recommandations:" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

Write-Host "`n🔧 Actions à effectuer:" -ForegroundColor Yellow
Write-Host "1. Redémarrez l'API avec 'dotnet run'" -ForegroundColor White
Write-Host "2. Ouvrez test-cors.html dans votre navigateur" -ForegroundColor White
Write-Host "3. Testez avec Postman en utilisant la collection fournie" -ForegroundColor White
Write-Host "4. Vérifiez que l'IP 192.168.100.17 est correcte" -ForegroundColor White
Write-Host "5. Vérifiez que le port 5001 n'est pas bloqué par le firewall" -ForegroundColor White

Write-Host "`n🌐 URLs de test:" -ForegroundColor Yellow
Write-Host "• API: $API_BASE_URL" -ForegroundColor White
Write-Host "• Swagger: $API_BASE_URL/swagger" -ForegroundColor White
Write-Host "• Test CORS: Ouvrez test-cors.html" -ForegroundColor White

Write-Host "`n✅ Test terminé!" -ForegroundColor Green
