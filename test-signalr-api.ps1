$apiUrl = "https://localhost:7103"
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

Write-Host "`n=== TEST SIGNALR API ===" -ForegroundColor Cyan

# 1. Connexion
Write-Host "`n1. Authentification..." -ForegroundColor Yellow
$loginBody = '{"emailOuTelephone":"elsynchropos@gmail.com","motDePasse":"Admin"}'
try {
    $response = Invoke-RestMethod -Uri "$apiUrl/api/Utilisateur/authentifier" -Method POST -Body $loginBody -ContentType "application/json" -ErrorAction Stop
    $token = $response.AccessToken
    $userId = $response.Utilisateur.IdUtilisateur
    Write-Host "OK - Token recu, UserId: $userId" -ForegroundColor Green
} catch {
    Write-Host "ERREUR connexion: $_" -ForegroundColor Red
    exit 1
}

# Headers
$headers = @{}
$headers["Authorization"] = "Bearer $token"
$headers["Content-Type"] = "application/json"

# 2. Verifier le statut du hub
Write-Host "`n2. Verification du statut du hub SignalR..." -ForegroundColor Yellow
try {
    $status = Invoke-RestMethod -Uri "$apiUrl/api/TestSignalR/status" -Method GET -Headers $headers -ErrorAction Stop
    Write-Host "OK - Hub SignalR actif" -ForegroundColor Green
    Write-Host "   Endpoint: $($status.hubEndpoint)" -ForegroundColor Gray
} catch {
    Write-Host "ERREUR: $_" -ForegroundColor Red
}

# 3. Envoyer une notification broadcast
Write-Host "`n3. Envoi notification BROADCAST..." -ForegroundColor Yellow
$broadcastBody = @{
    type = "INFO"
    titre = "Test Broadcast"
    message = "Ceci est une notification de test envoyée à tous les utilisateurs connectés !"
    data = @{
        testId = 1
        source = "PowerShell Script"
    }
} | ConvertTo-Json

try {
    $result = Invoke-RestMethod -Uri "$apiUrl/api/TestSignalR/broadcast" -Method POST -Body $broadcastBody -Headers $headers -ErrorAction Stop
    Write-Host "OK - Notification broadcast envoyée" -ForegroundColor Green
    Start-Sleep -Seconds 2
} catch {
    Write-Host "ERREUR: $_" -ForegroundColor Red
}

# 4. Envoyer une notification à un groupe
Write-Host "`n4. Envoi notification au GROUPE 'ecole_2'..." -ForegroundColor Yellow
$groupBody = @{
    type = "ECOLE"
    titre = "Message pour l'école"
    message = "Ceci est un message pour tous les membres de l'école 2"
    data = @{
        ecoleId = 2
    }
} | ConvertTo-Json

try {
    $result = Invoke-RestMethod -Uri "$apiUrl/api/TestSignalR/group/ecole_2" -Method POST -Body $groupBody -Headers $headers -ErrorAction Stop
    Write-Host "OK - Notification groupe envoyée" -ForegroundColor Green
    Start-Sleep -Seconds 2
} catch {
    Write-Host "ERREUR: $_" -ForegroundColor Red
}

# 5. Envoyer une notification à un utilisateur spécifique
Write-Host "`n5. Envoi notification à l'utilisateur $userId..." -ForegroundColor Yellow
$userBody = @{
    type = "PERSONNEL"
    titre = "Message personnel"
    message = "Ceci est un message personnel pour vous !"
    data = @{
        userId = $userId
        priority = "HIGH"
    }
} | ConvertTo-Json

try {
    $result = Invoke-RestMethod -Uri "$apiUrl/api/TestSignalR/user/$userId" -Method POST -Body $userBody -Headers $headers -ErrorAction Stop
    Write-Host "OK - Notification utilisateur envoyée" -ForegroundColor Green
    Start-Sleep -Seconds 2
} catch {
    Write-Host "ERREUR: $_" -ForegroundColor Red
}

# 6. Notification de paiement simulée
Write-Host "`n6. Simulation notification PAIEMENT..." -ForegroundColor Yellow
$paiementBody = @{
    type = "PAIEMENT"
    titre = "Confirmation de Paiement"
    message = "Un paiement de 150 USD a été reçu pour l'élève Jean MUKOKO"
    data = @{
        montant = 150.0
        devise = "USD"
        eleve = "Jean MUKOKO"
        date = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
    }
} | ConvertTo-Json

try {
    $result = Invoke-RestMethod -Uri "$apiUrl/api/TestSignalR/broadcast" -Method POST -Body $paiementBody -Headers $headers -ErrorAction Stop
    Write-Host "OK - Notification paiement envoyée" -ForegroundColor Green
    Start-Sleep -Seconds 2
} catch {
    Write-Host "ERREUR: $_" -ForegroundColor Red
}

# 7. Notification de présence simulée
Write-Host "`n7. Simulation notification PRESENCE..." -ForegroundColor Yellow
$presenceBody = @{
    type = "PRESENCE"
    titre = "Confirmation de présence"
    message = "Marie KABONGO est PRÉSENTE aujourd'hui à 07:30"
    data = @{
        eleve = "Marie KABONGO"
        statut = "PRÉSENT"
        heure = "07:30"
        date = (Get-Date).ToString("yyyy-MM-dd")
    }
} | ConvertTo-Json

try {
    $result = Invoke-RestMethod -Uri "$apiUrl/api/TestSignalR/broadcast" -Method POST -Body $presenceBody -Headers $headers -ErrorAction Stop
    Write-Host "OK - Notification présence envoyée" -ForegroundColor Green
} catch {
    Write-Host "ERREUR: $_" -ForegroundColor Red
}

Write-Host "`n=== TEST TERMINE ===" -ForegroundColor Green
Write-Host "`n📊 INSTRUCTIONS:" -ForegroundColor Cyan
Write-Host "1. Ouvrez test-signalr.html dans votre navigateur" -ForegroundColor Yellow
Write-Host "2. Cliquez sur 'Se connecter et récupérer le token'" -ForegroundColor Yellow
Write-Host "3. Cliquez sur 'Connecter' pour établir la connexion SignalR" -ForegroundColor Yellow
Write-Host "4. Rejoignez le groupe 'ecole_2' si vous voulez" -ForegroundColor Yellow
Write-Host "5. Relancez ce script pour envoyer des notifications" -ForegroundColor Yellow
Write-Host "`nVous devriez voir les notifications apparaître en temps réel !" -ForegroundColor Green

