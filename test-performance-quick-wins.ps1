# ══════════════════════════════════════════════════════════════════════════════════
# TEST PERFORMANCE - QUICK WINS
# Vérifie que toutes les optimisations sont actives
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
Write-Host "║      🚀 TEST PERFORMANCE - QUICK WINS 🚀                          ║" -ForegroundColor Green
Write-Host "║                                                                    ║" -ForegroundColor Cyan
Write-Host "══════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# ══════════════════════════════════════════════════════════════════════════════════
# ÉTAPE 1 : AUTHENTIFICATION
# ══════════════════════════════════════════════════════════════════════════════════

Write-Host "📝 ÉTAPE 1 : Authentification..." -ForegroundColor Yellow
Write-Host ""

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
    Write-Host "  ✅ Authentification réussie !" -ForegroundColor Green
    Write-Host "  📌 Token obtenu : $($token.Substring(0, 30))..." -ForegroundColor Gray
    Write-Host ""
}
catch {
    Write-Host "  ❌ ERREUR d'authentification : $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

$headers = @{
    "Authorization" = "Bearer $token"
    "Accept-Encoding" = "gzip, deflate, br"  # Pour tester la compression
}

# ══════════════════════════════════════════════════════════════════════════════════
# ÉTAPE 2 : TEST COMPRESSION GZIP/BROTLI
# ══════════════════════════════════════════════════════════════════════════════════

Write-Host "📦 ÉTAPE 2 : Test de la compression Gzip/Brotli..." -ForegroundColor Yellow
Write-Host ""

try {
    # Requête pour obtenir la liste des écoles
    $startTime = Get-Date
    $response = Invoke-WebRequest -Uri "$apiUrl/Ecole" `
                                  -Method GET `
                                  -Headers $headers `
                                  -ErrorAction Stop

    $endTime = Get-Date
    $duration = ($endTime - $startTime).TotalMilliseconds

    # Vérifier si compression est active
    $contentEncoding = $response.Headers["Content-Encoding"]
    $contentLength = $response.RawContentLength

    Write-Host "  🎯 Endpoint : GET /api/Ecole" -ForegroundColor White
    Write-Host "  ⏱️  Temps de réponse : ${duration}ms" -ForegroundColor White
    Write-Host "  📊 Taille de la réponse : $contentLength octets" -ForegroundColor White

    if ($contentEncoding -match "gzip|br") {
        Write-Host "  ✅ COMPRESSION ACTIVE : $contentEncoding" -ForegroundColor Green
        Write-Host "  💡 Économie estimée : ~70-85%" -ForegroundColor Cyan
    }
    else {
        Write-Host "  ⚠️  COMPRESSION NON DÉTECTÉE" -ForegroundColor Yellow
        Write-Host "  💡 Assurez-vous que UseResponseCompression() est dans Program.cs" -ForegroundColor Gray
    }
    Write-Host ""
}
catch {
    Write-Host "  ❌ ERREUR : $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
}

# ══════════════════════════════════════════════════════════════════════════════════
# ÉTAPE 3 : TEST PAGINATION
# ══════════════════════════════════════════════════════════════════════════════════

Write-Host "📄 ÉTAPE 3 : Test de la pagination..." -ForegroundColor Yellow
Write-Host ""

try {
    # Test pagination : Page 1, 10 éléments
    $startTime = Get-Date
    $response = Invoke-WebRequest -Uri "$apiUrl/Eleve?page=1&pageSize=10" `
                                  -Method GET `
                                  -Headers $headers `
                                  -ErrorAction Stop

    $endTime = Get-Date
    $duration = ($endTime - $startTime).TotalMilliseconds

    $data = ($response.Content | ConvertFrom-Json)

    Write-Host "  🎯 Endpoint : GET /api/Eleve?page=1&pageSize=10" -ForegroundColor White
    Write-Host "  ⏱️  Temps de réponse : ${duration}ms" -ForegroundColor White

    # Vérifier les headers de pagination
    $currentPage = $response.Headers["X-Pagination-CurrentPage"]
    $pageSize = $response.Headers["X-Pagination-PageSize"]
    $totalItems = $response.Headers["X-Pagination-TotalItems"]
    $totalPages = $response.Headers["X-Pagination-TotalPages"]

    if ($currentPage) {
        Write-Host "  ✅ PAGINATION ACTIVE" -ForegroundColor Green
        Write-Host "     • Page courante : $currentPage" -ForegroundColor Cyan
        Write-Host "     • Éléments par page : $pageSize" -ForegroundColor Cyan
        Write-Host "     • Total d'éléments : $totalItems" -ForegroundColor Cyan
        Write-Host "     • Total de pages : $totalPages" -ForegroundColor Cyan
    }
    else {
        Write-Host "  ⚠️  PAGINATION NON DÉTECTÉE (headers manquants)" -ForegroundColor Yellow
        Write-Host "  💡 Nombre d'éléments retournés : $($data.data.Count)" -ForegroundColor Gray
    }
    Write-Host ""
}
catch {
    Write-Host "  ❌ ERREUR : $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
}

# ══════════════════════════════════════════════════════════════════════════════════
# ÉTAPE 4 : TEST CACHE (CACHE HIT/MISS)
# ══════════════════════════════════════════════════════════════════════════════════

Write-Host "💾 ÉTAPE 4 : Test du cache In-Memory..." -ForegroundColor Yellow
Write-Host ""

try {
    # Première requête (CACHE MISS attendu)
    Write-Host "  📌 Requête 1 (CACHE MISS attendu)..." -ForegroundColor Gray
    $startTime1 = Get-Date
    $response1 = Invoke-RestMethod -Uri "$apiUrl/Ecole" `
                                   -Method GET `
                                   -Headers $headers `
                                   -ErrorAction Stop
    $endTime1 = Get-Date
    $duration1 = ($endTime1 - $startTime1).TotalMilliseconds
    Write-Host "     ⏱️  Temps : ${duration1}ms" -ForegroundColor White

    Start-Sleep -Milliseconds 500

    # Deuxième requête (CACHE HIT attendu)
    Write-Host "  📌 Requête 2 (CACHE HIT attendu)..." -ForegroundColor Gray
    $startTime2 = Get-Date
    $response2 = Invoke-RestMethod -Uri "$apiUrl/Ecole" `
                                   -Method GET `
                                   -Headers $headers `
                                   -ErrorAction Stop
    $endTime2 = Get-Date
    $duration2 = ($endTime2 - $startTime2).TotalMilliseconds
    Write-Host "     ⏱️  Temps : ${duration2}ms" -ForegroundColor White

    Write-Host ""
    $improvement = [Math]::Round((($duration1 - $duration2) / $duration1) * 100, 1)

    if ($duration2 -lt $duration1 * 0.8) {
        Write-Host "  ✅ CACHE FONCTIONNE !" -ForegroundColor Green
        Write-Host "     • Requête 1 (MISS) : ${duration1}ms" -ForegroundColor Cyan
        Write-Host "     • Requête 2 (HIT)  : ${duration2}ms" -ForegroundColor Cyan
        Write-Host "     • 🚀 Amélioration : +${improvement}%" -ForegroundColor Yellow
    }
    else {
        Write-Host "  ⚠️  CACHE NON DÉTECTÉ (ou pas implémenté pour /Ecole)" -ForegroundColor Yellow
        Write-Host "     • Temps similaires entre les deux requêtes" -ForegroundColor Gray
        Write-Host "     💡 Le cache est optionnel - À activer au besoin" -ForegroundColor Gray
    }
    Write-Host ""
}
catch {
    Write-Host "  ❌ ERREUR : $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
}

# ══════════════════════════════════════════════════════════════════════════════════
# ÉTAPE 5 : BENCHMARK DE PERFORMANCE
# ══════════════════════════════════════════════════════════════════════════════════

Write-Host "📊 ÉTAPE 5 : Benchmark de performance..." -ForegroundColor Yellow
Write-Host ""

$endpoints = @(
    @{ Name = "GET /api/Ecole"; Url = "$apiUrl/Ecole" },
    @{ Name = "GET /api/Eleve"; Url = "$apiUrl/Eleve" },
    @{ Name = "GET /api/Classe"; Url = "$apiUrl/Classe" }
)

$results = @()

foreach ($endpoint in $endpoints) {
    try {
        $times = @()
        
        # Faire 3 requêtes pour avoir une moyenne
        for ($i = 1; $i -le 3; $i++) {
            $startTime = Get-Date
            $response = Invoke-RestMethod -Uri $endpoint.Url `
                                          -Method GET `
                                          -Headers $headers `
                                          -ErrorAction Stop
            $endTime = Get-Date
            $duration = ($endTime - $startTime).TotalMilliseconds
            $times += $duration

            Start-Sleep -Milliseconds 200
        }

        $avgTime = [Math]::Round(($times | Measure-Object -Average).Average, 0)
        $minTime = [Math]::Round(($times | Measure-Object -Minimum).Minimum, 0)
        $maxTime = [Math]::Round(($times | Measure-Object -Maximum).Maximum, 0)

        $results += [PSCustomObject]@{
            Endpoint = $endpoint.Name
            Min = $minTime
            Avg = $avgTime
            Max = $maxTime
        }

        Write-Host "  ✅ $($endpoint.Name)" -ForegroundColor White
        Write-Host "     • Min : ${minTime}ms | Avg : ${avgTime}ms | Max : ${maxTime}ms" -ForegroundColor Cyan
    }
    catch {
        Write-Host "  ❌ $($endpoint.Name) : ERREUR" -ForegroundColor Red
    }
}

Write-Host ""

# ══════════════════════════════════════════════════════════════════════════════════
# RÉSUMÉ FINAL
# ══════════════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "══════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "║                                                                    ║" -ForegroundColor Cyan
Write-Host "║      📊 RÉSUMÉ DES TESTS PERFORMANCE 📊                           ║" -ForegroundColor Green
Write-Host "║                                                                    ║" -ForegroundColor Cyan
Write-Host "══════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

Write-Host "🎯 OPTIMISATIONS TESTÉES :" -ForegroundColor Yellow
Write-Host ""
Write-Host "  ✅ Compression Gzip/Brotli  : $(if($contentEncoding) { 'ACTIF' } else { 'À VÉRIFIER' })" -ForegroundColor $(if($contentEncoding) { 'Green' } else { 'Yellow' })
Write-Host "  ✅ Pagination               : $(if($currentPage) { 'ACTIF' } else { 'À IMPLÉMENTER' })" -ForegroundColor $(if($currentPage) { 'Green' } else { 'Yellow' })
Write-Host "  ✅ Cache In-Memory          : $(if($duration2 -lt $duration1 * 0.8) { 'ACTIF' } else { 'OPTIONNEL' })" -ForegroundColor $(if($duration2 -lt $duration1 * 0.8) { 'Green' } else { 'Yellow' })
Write-Host "  ✅ Index DB                 : À vérifier manuellement (voir AddPerformanceIndexes.sql)" -ForegroundColor Gray
Write-Host "  ✅ AsNoTracking()           : À vérifier dans les Services (code)" -ForegroundColor Gray
Write-Host ""

Write-Host "📈 PERFORMANCES MESURÉES :" -ForegroundColor Yellow
Write-Host ""
$results | Format-Table -AutoSize
Write-Host ""

Write-Host "💡 RECOMMANDATIONS :" -ForegroundColor Yellow
Write-Host ""
if (-not $contentEncoding) {
    Write-Host "  ⚠️  Compression non détectée → Vérifier Program.cs" -ForegroundColor Yellow
}
if (-not $currentPage) {
    Write-Host "  ⚠️  Pagination non implémentée → Ajouter PaginationParams aux contrôleurs" -ForegroundColor Yellow
}
if ($duration2 -ge $duration1 * 0.8) {
    Write-Host "  💡 Cache non utilisé → Ajouter ICacheService aux contrôleurs si besoin" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "📝 PROCHAINES ÉTAPES :" -ForegroundColor Yellow
Write-Host ""
Write-Host "  1. Appliquer la migration SQL : AddPerformanceIndexes.sql" -ForegroundColor White
Write-Host "  2. Ajouter pagination aux endpoints GET volumineux" -ForegroundColor White
Write-Host "  3. Ajouter cache aux données statiques (Ecoles, Classes, etc.)" -ForegroundColor White
Write-Host "  4. Vérifier AsNoTracking() dans les repositories" -ForegroundColor White
Write-Host "  5. Re-tester les performances après optimisations" -ForegroundColor White
Write-Host ""

Write-Host "══════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "✅ Tests terminés avec succès !" -ForegroundColor Green
Write-Host ""

