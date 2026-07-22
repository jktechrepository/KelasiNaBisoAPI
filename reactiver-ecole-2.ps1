$apiUrl = "https://localhost:7102"
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

Write-Host "`n=== REACTIVATION ECOLE ID 2 ===" -ForegroundColor Cyan

# Utiliser le Super-Admin de l'école Ekelasi School (ID 1)
Write-Host "`n1. Connexion avec Super-Admin..." -ForegroundColor Yellow
$loginBody = '{"emailOuTelephone":"superadmin@kelasinabiso.cd","motDePasse":"Super-Admin"}'
try {
    $response = Invoke-RestMethod -Uri "$apiUrl/api/Utilisateur/authentifier" -Method POST -Body $loginBody -ContentType "application/json" -ErrorAction Stop
    $token = $response.AccessToken
    $idEcole = $response.Utilisateur.IdEcole
    Write-Host "OK - Token recu, IdEcole: $idEcole" -ForegroundColor Green
} catch {
    Write-Host "ERREUR connexion: $_" -ForegroundColor Red
    exit 1
}

# Headers
$headers = @{}
$headers["Authorization"] = "Bearer $token"
$headers["Content-Type"] = "application/json"

# Réactiver l'école ID 2
Write-Host "`n2. Réactivation de l'école ID 2..." -ForegroundColor Yellow
try {
    $result = Invoke-RestMethod -Uri "$apiUrl/api/Ecole/set-statut/2?statut=true" -Method PUT -Headers $headers -ErrorAction Stop
    Write-Host "✅ SUCCESS - Ecole réactivée !" -ForegroundColor Green
    Write-Host "Message: $($result.message)" -ForegroundColor Gray
    Write-Host "Statut: $($result.statut)" -ForegroundColor Gray
} catch {
    Write-Host "ERREUR: $_" -ForegroundColor Red
    Write-Host "Error details: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $reader.BaseStream.Position = 0
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody" -ForegroundColor Red
    }
    exit 1
}

Write-Host "`n=== TEST TERMINE ===" -ForegroundColor Green

