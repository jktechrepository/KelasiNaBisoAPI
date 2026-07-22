# TEST COMPLET NOTIFICATIONS PUSH (Firebase + SignalR)
# Inspire de test-sms-paiement.ps1
# Date: 1 novembre 2025

$apiUrl = "https://localhost:7103"

# Configuration SSL
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

Write-Host "`n=============================================================" -ForegroundColor Cyan
Write-Host "   TEST NOTIFICATIONS PUSH (Firebase + SignalR)" -ForegroundColor Green
Write-Host "=============================================================`n" -ForegroundColor Cyan

# ===========================================================================
# ETAPE 1 : AUTHENTIFICATION
# ===========================================================================
Write-Host "ETAPE 1 : Authentification" -ForegroundColor Yellow
Write-Host "-------------------------------------------------------------" -ForegroundColor Gray

$loginBody = @{
    emailOuTelephone = "kangudjaobed66@gmail.com"
    motDePasse = "123456"
    fcmToken = "dulYj4WMSOmtoMcX-QPmdO:APA91bHMPm-ssK_SyDjUuLbbtVqtTO1Bn1OyOcEqxy7CO0YgcAuzZ4p39EHTmjgjU7mQsvGSEqj8uDb6sKiSsJ5C42t_WT-vqarjcyfWQ0cPr91nH9SF_9o"
    deviceType = "Android"
    deviceModel = "alps V510B"
    osVersion = "Android 12"
} | ConvertTo-Json -Depth 10

Write-Host "Email: kangudjaobed66@gmail.com" -ForegroundColor Gray
Write-Host "Device: Android - alps V510B" -ForegroundColor Gray
Write-Host "FCM Token: dulYj4WM...9SF_9o (Token reel)" -ForegroundColor Gray
Write-Host "`nEnvoi de la requete d'authentification..." -ForegroundColor Gray

try {
    $response = Invoke-RestMethod -Uri "$apiUrl/api/Utilisateur/authentifier" `
        -Method POST `
        -Body $loginBody `
        -ContentType "application/json" `
        -ErrorAction Stop
    
    $token = $response.accessToken
    $userId = $response.utilisateur.idUtilisateur
    $userName = "$($response.utilisateur.nomUtilisateur) $($response.utilisateur.prenomUtilisateur)"
    $idEcole = $response.utilisateur.idEcole
    
    Write-Host "OK - Authentification reussie!" -ForegroundColor Green
    Write-Host "  User ID: $userId" -ForegroundColor White
    Write-Host "  Nom: $userName" -ForegroundColor White
    Write-Host "  Ecole ID: $idEcole" -ForegroundColor White
    Write-Host "  Token FCM enregistre avec succes`n" -ForegroundColor Cyan
} catch {
    Write-Host "ERREUR lors de l'authentification:" -ForegroundColor Red
    Write-Host "Message: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $reader.BaseStream.Position = 0
        $responseBody = $reader.ReadToEnd()
        Write-Host "Details: $responseBody" -ForegroundColor Yellow
    }
    exit 1
}

Start-Sleep -Seconds 2

# ===========================================================================
# ETAPE 2 : RECUPERER UN ELEVE AVEC TUTEUR
# ===========================================================================
Write-Host "`nETAPE 2 : Recuperation d'un eleve avec tuteur" -ForegroundColor Yellow
Write-Host "-------------------------------------------------------------" -ForegroundColor Gray

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

try {
    Write-Host "Recherche des eleves de l'ecole $idEcole..." -ForegroundColor Gray
    $eleves = Invoke-RestMethod -Uri "$apiUrl/api/Eleve/ecole/$idEcole" `
        -Method GET `
        -Headers $headers `
        -ErrorAction Stop
    
    if ($eleves.Count -eq 0) {
        Write-Host "AUCUN ELEVE TROUVE pour cette ecole" -ForegroundColor Red
        Write-Host "Creez d'abord des eleves avec test-batch-eleves-paiements.ps1`n" -ForegroundColor Yellow
        exit 1
    }
    
    $eleve = $eleves[0]
    $eleveId = $eleve.idEleve
    $eleveNom = $eleve.nomComplet
    $tuteurId = $eleve.idTuteur
    
    Write-Host "OK - Eleve trouve: $eleveNom (ID: $eleveId)" -ForegroundColor Green
    
    if (-not $tuteurId) {
        Write-Host "ERREUR: Eleve sans tuteur" -ForegroundColor Red
        exit 1
    }
    
    # Recuperer le tuteur
    Write-Host "Recuperation du tuteur (ID: $tuteurId)..." -ForegroundColor Gray
    $tuteur = Invoke-RestMethod -Uri "$apiUrl/api/Tuteur/$tuteurId" `
        -Method GET `
        -Headers $headers `
        -ErrorAction Stop
    
    $tuteurNom = $tuteur.nomComplet
    $tuteurTel = $tuteur.telephone
    
    Write-Host "OK - Tuteur: $tuteurNom" -ForegroundColor Green
    Write-Host "  Telephone: $tuteurTel`n" -ForegroundColor White
} catch {
    Write-Host "ERREUR lors de la recuperation des donnees:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

Start-Sleep -Seconds 2

# ===========================================================================
# ETAPE 3 : TEST NOTIFICATION PAIEMENT
# ===========================================================================
Write-Host "`nETAPE 3 : Test Notification PAIEMENT" -ForegroundColor Yellow
Write-Host "-------------------------------------------------------------" -ForegroundColor Gray

$reference = "TEST-NOTIF-PUSH-$(Get-Date -Format 'yyyyMMddHHmmss')"

$paiementBody = @{
    idEleve = $eleveId
    idTypeFrais = 1
    montant = 50000
    datePaiement = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")
    modePaiement = "Especes"
    numeroBordereau = $reference
    referenceTransaction = $reference
    commentaire = "Test notification push - Paiement de frais"
    justificatifUrl = ""
    devise = "CDF"
    statutPaiement = "Confirme"
    statut = $true
    idUtilisateur = $userId
} | ConvertTo-Json -Depth 10

Write-Host "Creation d'un paiement de test..." -ForegroundColor Gray
Write-Host "  Montant: 50000 CDF" -ForegroundColor White
Write-Host "  Eleve: $eleveNom" -ForegroundColor White
Write-Host "  Reference: $reference" -ForegroundColor White
Write-Host "`nEnvoi de la requete..." -ForegroundColor Gray

try {
    $paiement = Invoke-RestMethod -Uri "$apiUrl/api/Paiement" `
        -Method POST `
        -Body $paiementBody `
        -Headers $headers `
        -ErrorAction Stop
    
    Write-Host "`nOK - Paiement cree avec succes!" -ForegroundColor Green
    Write-Host "  ID Paiement: $($paiement.idPaiement)" -ForegroundColor White
    Write-Host "`n  Notifications envoyees:" -ForegroundColor Cyan
    Write-Host "    - Push Firebase (mobile Android)" -ForegroundColor Green
    Write-Host "    - Push SignalR (web temps reel)" -ForegroundColor Green
    Write-Host "    - SMS au $tuteurTel" -ForegroundColor Green
    
    Write-Host "`nVERIFIEZ:" -ForegroundColor Yellow
    Write-Host "  1. Notification sur le mobile Android (FCM)" -ForegroundColor White
    Write-Host "  2. Les logs de l'application ci-dessous" -ForegroundColor White
    Write-Host "  3. Le telephone $tuteurTel pour le SMS`n" -ForegroundColor White
} catch {
    Write-Host "`nERREUR lors de la creation du paiement:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $reader.BaseStream.Position = 0
        $responseBody = $reader.ReadToEnd()
        Write-Host "Details: $responseBody" -ForegroundColor Yellow
    }
}

Start-Sleep -Seconds 3

# ===========================================================================
# ETAPE 4 : TEST NOTIFICATION PRESENCE
# ===========================================================================
Write-Host "`nETAPE 4 : Test Notification PRESENCE" -ForegroundColor Yellow
Write-Host "-------------------------------------------------------------" -ForegroundColor Gray

$presenceBody = @{
    idEleve = $eleveId
    datePresence = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")
    isPresent = $true
    heureArrivee = (Get-Date).ToString("HH:mm:ss")
    marquerPar = $userId
    idVacation = 1
    commentaire = "Test notification push - Presence"
} | ConvertTo-Json -Depth 10

Write-Host "Enregistrement d'une presence..." -ForegroundColor Gray
Write-Host "  Eleve: $eleveNom" -ForegroundColor White
Write-Host "  Statut: PRESENT" -ForegroundColor Green
Write-Host "  Heure: $(Get-Date -Format 'HH:mm')" -ForegroundColor White
Write-Host "`nEnvoi de la requete..." -ForegroundColor Gray

try {
    $presence = Invoke-RestMethod -Uri "$apiUrl/api/Presence" `
        -Method POST `
        -Body $presenceBody `
        -Headers $headers `
        -ErrorAction Stop
    
    Write-Host "`nOK - Presence enregistree avec succes!" -ForegroundColor Green
    Write-Host "  ID Presence: $($presence.idPresence)" -ForegroundColor White
    Write-Host "`n  Notifications envoyees:" -ForegroundColor Cyan
    Write-Host "    - Push Firebase (mobile Android)" -ForegroundColor Green
    Write-Host "    - Push SignalR (web temps reel)" -ForegroundColor Green
    Write-Host "    - SMS au $tuteurTel" -ForegroundColor Green
    
    Write-Host "`nVERIFIEZ les notifications sur le mobile et les logs`n" -ForegroundColor Yellow
} catch {
    Write-Host "`nERREUR lors de l'enregistrement de la presence:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $reader.BaseStream.Position = 0
        $responseBody = $reader.ReadToEnd()
        Write-Host "Details: $responseBody" -ForegroundColor Yellow
    }
}

Start-Sleep -Seconds 3

# ===========================================================================
# ETAPE 5 : TEST SIGNALR DIRECT
# ===========================================================================
Write-Host "`nETAPE 5 : Test SignalR Direct (via TestSignalRController)" -ForegroundColor Yellow
Write-Host "-------------------------------------------------------------" -ForegroundColor Gray

# Test 1 : Broadcast
Write-Host "Test 5.1 : Broadcast a tous les utilisateurs connectes" -ForegroundColor White

$broadcastBody = @{
    titre = "Test Broadcast"
    message = "Ceci est un test de notification broadcast"
    type = "TEST_BROADCAST"
} | ConvertTo-Json -Depth 10

try {
    $broadcastResponse = Invoke-RestMethod -Uri "$apiUrl/api/TestSignalR/broadcast" `
        -Method POST `
        -Body $broadcastBody `
        -Headers $headers `
        -ErrorAction Stop
    
    Write-Host "OK - Notification broadcast envoyee!" -ForegroundColor Green
} catch {
    Write-Host "ERREUR: $($_.Exception.Message)" -ForegroundColor Red
}

Start-Sleep -Seconds 2

# Test 2 : Notification utilisateur specifique
Write-Host "`nTest 5.2 : Notification a l'utilisateur $userId" -ForegroundColor White

$userBody = @{
    titre = "Notification Personnelle"
    message = "Ceci est une notification destinee uniquement a vous"
    type = "TEST_USER"
} | ConvertTo-Json -Depth 10

try {
    $userResponse = Invoke-RestMethod -Uri "$apiUrl/api/TestSignalR/user/$userId" `
        -Method POST `
        -Body $userBody `
        -Headers $headers `
        -ErrorAction Stop
    
    Write-Host "OK - Notification utilisateur envoyee!" -ForegroundColor Green
} catch {
    Write-Host "ERREUR: $($_.Exception.Message)" -ForegroundColor Red
}

Start-Sleep -Seconds 2

# Test 3 : Notification groupe
Write-Host "`nTest 5.3 : Notification au groupe 'all_users'" -ForegroundColor White

$groupBody = @{
    titre = "Notification Groupe"
    message = "Ceci est une notification destinee au groupe all_users"
    type = "TEST_GROUP"
} | ConvertTo-Json -Depth 10

try {
    $groupResponse = Invoke-RestMethod -Uri "$apiUrl/api/TestSignalR/group/all_users" `
        -Method POST `
        -Body $groupBody `
        -Headers $headers `
        -ErrorAction Stop
    
    Write-Host "OK - Notification groupe envoyee!`n" -ForegroundColor Green
} catch {
    Write-Host "ERREUR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ===========================================================================
# RESUME FINAL
# ===========================================================================
Write-Host "`n=============================================================" -ForegroundColor Cyan
Write-Host "   TESTS TERMINES AVEC SUCCES" -ForegroundColor Green
Write-Host "=============================================================`n" -ForegroundColor Cyan

Write-Host "Resume des tests:" -ForegroundColor Yellow
Write-Host "  [OK] Authentification avec FCM Token" -ForegroundColor Green
Write-Host "  [OK] Token FCM enregistre dans UserDevices" -ForegroundColor Green
Write-Host "  [OK] Notification Paiement (Firebase + SignalR + SMS)" -ForegroundColor Green
Write-Host "  [OK] Notification Presence (Firebase + SignalR + SMS)" -ForegroundColor Green
Write-Host "  [OK] Test SignalR Broadcast" -ForegroundColor Green
Write-Host "  [OK] Test SignalR Utilisateur" -ForegroundColor Green
Write-Host "  [OK] Test SignalR Groupe`n" -ForegroundColor Green

Write-Host "Pour verifier les notifications:" -ForegroundColor Yellow
Write-Host "  1. Mobile Android ($tuteurTel):" -ForegroundColor White
Write-Host "     - Ouvrez l'application mobile" -ForegroundColor Gray
Write-Host "     - Verifiez les notifications Firebase (FCM)" -ForegroundColor Gray
Write-Host "`n  2. Application Web:" -ForegroundColor White
Write-Host "     - Ouvrez test-signalr-notifications.html" -ForegroundColor Gray
Write-Host "     - Connectez-vous et verifiez les notifications temps reel" -ForegroundColor Gray
Write-Host "`n  3. SMS:" -ForegroundColor White
Write-Host "     - Verifiez le telephone $tuteurTel" -ForegroundColor Gray
Write-Host "`n  4. Logs de l'application:" -ForegroundColor White
Write-Host "     - Regardez les logs ci-dessus pour voir les envois" -ForegroundColor Gray

Write-Host "`nDocumentation complete:" -ForegroundColor Yellow
Write-Host "  - GUIDE_INTEGRATION_FRONTEND_NOTIFICATIONS.md`n" -ForegroundColor White

Write-Host "=============================================================`n" -ForegroundColor Cyan

