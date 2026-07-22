# Test simple de l'endpoint
try {
    Write-Host "Test de l'endpoint VuePointagePresenceParEcole..." -ForegroundColor Green
    
    # Ignorer les erreurs de certificat SSL
    [System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
    
    # Test avec HTTPS
    $uri = "https://192.168.43.233:7155/api/VuePointagePresenceParEcole"
    Write-Host "Tentative de connexion à: $uri" -ForegroundColor Yellow
    
    $response = Invoke-WebRequest -Uri $uri -Method GET -Headers @{"Accept"="application/json"} -TimeoutSec 30
    Write-Host "Succès! Code de statut: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "Contenu de la réponse:" -ForegroundColor Green
    $response.Content | ConvertFrom-Json | ConvertTo-Json -Depth 3
}
catch {
    Write-Host "Erreur: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Détails: $($_.Exception)" -ForegroundColor Red
}
