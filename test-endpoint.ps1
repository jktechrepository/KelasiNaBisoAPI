# Script de test pour l'endpoint VuePointagePresenceParEcole
try {
    Write-Host "Test de l'endpoint VuePointagePresenceParEcole..." -ForegroundColor Green
    
    # Test avec HTTPS
    $uri = "https://192.168.43.233:7155/api/VuePointagePresenceParEcole"
    Write-Host "Tentative de connexion à: $uri" -ForegroundColor Yellow
    
    # Ignorer les erreurs de certificat SSL
    [System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
    
    $response = Invoke-RestMethod -Uri $uri -Method GET -Headers @{"Accept"="application/json"} -TimeoutSec 30
    Write-Host "Succès! Réponse reçue:" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 3
}
catch {
    Write-Host "Erreur lors du test HTTPS: $($_.Exception.Message)" -ForegroundColor Red
    
    # Test avec HTTP
    try {
        Write-Host "Tentative avec HTTP..." -ForegroundColor Yellow
        $uri = "http://192.168.43.233:5000/api/VuePointagePresenceParEcole"
        $response = Invoke-RestMethod -Uri $uri -Method GET -Headers @{"Accept"="application/json"} -TimeoutSec 30
        Write-Host "Succès avec HTTP! Réponse reçue:" -ForegroundColor Green
        $response | ConvertTo-Json -Depth 3
    }
    catch {
        Write-Host "Erreur lors du test HTTP: $($_.Exception.Message)" -ForegroundColor Red
    }
}
