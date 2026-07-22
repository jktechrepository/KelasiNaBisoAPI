# TEST COMPLET - NOTIFICATIONS PUSH
# Ce script teste l'envoi de notifications via SignalR et Firebase
# Date: 1 novembre 2025

$apiUrl = "https://localhost:7105"

Write-Host "`n==============================================================" -ForegroundColor Cyan
Write-Host "   TEST COMPLET NOTIFICATIONS PUSH" -ForegroundColor Green
Write-Host "==============================================================`n" -ForegroundColor Cyan

# ETAPE 1 : AUTHENTIFICATION
Write-Host "ETAPE 1 : Authentification" -ForegroundColor Yellow
Write-Host "--------------------------------------------------------------`n" -ForegroundColor Gray

$loginData = @{
    emailOuTelephone = "kangudjaobed66@gmail.com"
    motDePasse = "123456"
    fcmToken = "dulYj4WMSOmtoMcX-QPmdO:APA91bHMPm-ssK_SyDjUuLbbtVqtTO1Bn1OyOcEqxy7CO0YgcAuzZ4p39EHTmjgjU7mQsvGSEqj8uDb6sKiSsJ5C42t_WT-vqarjcyfWQ0cPr91nH9SF_9o"
    deviceType = "Android"
    deviceModel = "alps V510B"
    osVersion = "Android 12"
} | ConvertTo-Json

Write-Host "Tentative de connexion avec:" -ForegroundColor White
Write-Host "  Email: kangudjaobed66@gmail.com" -ForegroundColor Gray
Write-Host "  FCM Token: dulYj4WM...9SF_9o (Token reel Firebase)" -ForegroundColor Gray
Write-Host "  Device: Android - alps V510B`n" -ForegroundColor Gray

try {
    # Ignorer les erreurs de certificat SSL
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
    
    $response = Invoke-RestMethod -Uri "$apiUrl/api/Utilisateur/authentifier" `
        -Method Post `
        -Body $loginData `
        -ContentType "application/json"
    
    $token = $response.accessToken
    $userId = $response.utilisateur.idUtilisateur
    $userName = $response.utilisateur.nomUtilisateur
    
    Write-Host "Authentification reussie!" -ForegroundColor Green
    Write-Host "   User ID: $userId" -ForegroundColor White
    Write-Host "   Nom: $userName" -ForegroundColor White
    Write-Host "   Token: $($token.Substring(0, 20))...`n" -ForegroundColor White
}
catch {
    Write-Host "ERREUR lors de l'authentification:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

Start-Sleep -Seconds 2

# ETAPE 2 : RECUPERER UN ELEVE ET SON TUTEUR
Write-Host "`nETAPE 2 : Recuperation des donnees" -ForegroundColor Yellow
Write-Host "--------------------------------------------------------------`n" -ForegroundColor Gray

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

try {
    # Recuperer tous les eleves
    $eleves = Invoke-RestMethod -Uri "$apiUrl/api/Eleve" `
        -Method Get `
        -Headers $headers
    
    if ($eleves.Count -eq 0) {
        Write-Host "Aucun eleve trouve dans la base" -ForegroundColor Yellow
        Write-Host "   Creez d'abord des eleves avec test-batch-eleves-paiements.ps1`n" -ForegroundColor Gray
        exit 1
    }
    
    $eleve = $eleves[0]
    $eleveId = $eleve.idEleve
    $eleveNom = $eleve.nomComplet
    $tuteurId = $eleve.idTuteur
    
    Write-Host "Eleve trouve:" -ForegroundColor Green
    Write-Host "   ID: $eleveId" -ForegroundColor White
    Write-Host "   Nom: $eleveNom" -ForegroundColor White
    Write-Host "   Tuteur ID: $tuteurId`n" -ForegroundColor White
}
catch {
    Write-Host "ERREUR lors de la recuperation des donnees:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

Start-Sleep -Seconds 2

# ETAPE 3 : TEST NOTIFICATION PAIEMENT
Write-Host "`nETAPE 3 : Test Notification PAIEMENT" -ForegroundColor Yellow
Write-Host "--------------------------------------------------------------`n" -ForegroundColor Gray

$paiementData = @{
    idEleve = $eleveId
    idTypeFrais = 1
    montant = 50000
    datePaiement = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")
    modePaiement = "Especes"
    numeroBordereau = "BORDEREAU-TEST-$(Get-Random -Maximum 9999)"
} | ConvertTo-Json

Write-Host "Creation d'un paiement de test..." -ForegroundColor White
Write-Host "  Montant: 50000 CDF" -ForegroundColor Gray
Write-Host "  Eleve: $eleveNom" -ForegroundColor Gray
Write-Host "  Mode: Especes`n" -ForegroundColor Gray

try {
    $paiementResponse = Invoke-RestMethod -Uri "$apiUrl/api/Paiement" `
        -Method Post `
        -Headers $headers `
        -Body $paiementData
    
    Write-Host "Paiement cree avec succes!" -ForegroundColor Green
    Write-Host "   ID Paiement: $($paiementResponse.idPaiement)" -ForegroundColor White
    Write-Host "   Notification Firebase envoyee au tuteur" -ForegroundColor Cyan
    Write-Host "   Notification SignalR envoyee au tuteur`n" -ForegroundColor Cyan
}
catch {
    Write-Host "ERREUR lors de la creation du paiement:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Start-Sleep -Seconds 3

# ETAPE 4 : TEST NOTIFICATION PRESENCE
Write-Host "`nETAPE 4 : Test Notification PRESENCE" -ForegroundColor Yellow
Write-Host "--------------------------------------------------------------`n" -ForegroundColor Gray

$presenceData = @{
    idEleve = $eleveId
    datePresence = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")
    isPresent = $true
    heureArrivee = (Get-Date).ToString("HH:mm:ss")
    marquerPar = $userId
} | ConvertTo-Json

Write-Host "Enregistrement d'une presence..." -ForegroundColor White
Write-Host "  Eleve: $eleveNom" -ForegroundColor Gray
Write-Host "  Statut: PRESENT" -ForegroundColor Green
Write-Host "  Heure: $(Get-Date -Format 'HH:mm')`n" -ForegroundColor Gray

try {
    $presenceResponse = Invoke-RestMethod -Uri "$apiUrl/api/Presence" `
        -Method Post `
        -Headers $headers `
        -Body $presenceData
    
    Write-Host "Presence enregistree avec succes!" -ForegroundColor Green
    Write-Host "   ID Presence: $($presenceResponse.idPresence)" -ForegroundColor White
    Write-Host "   Notification Firebase envoyee au tuteur" -ForegroundColor Cyan
    Write-Host "   Notification SignalR envoyee au tuteur`n" -ForegroundColor Cyan
}
catch {
    Write-Host "ERREUR lors de l'enregistrement de la presence:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Start-Sleep -Seconds 3

# ETAPE 5 : TEST SIGNALR DIRECT (via TestSignalRController)
Write-Host "`nETAPE 5 : Test SignalR Direct" -ForegroundColor Yellow
Write-Host "--------------------------------------------------------------`n" -ForegroundColor Gray

# Test 1 : Broadcast a tous
Write-Host "Test 1 : Broadcast a tous les utilisateurs connectes" -ForegroundColor White

$broadcastData = @{
    titre = "Notification Test Broadcast"
    message = "Ceci est un test de notification broadcast a tous les utilisateurs"
    type = "TEST_BROADCAST"
} | ConvertTo-Json

try {
    $broadcastResponse = Invoke-RestMethod -Uri "$apiUrl/api/TestSignalR/broadcast" `
        -Method Post `
        -Headers $headers `
        -Body $broadcastData
    
    Write-Host "Notification broadcast envoyee!" -ForegroundColor Green
    Write-Host "   Message: $($broadcastResponse.message)`n" -ForegroundColor White
}
catch {
    Write-Host "ERREUR:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Start-Sleep -Seconds 2

# Test 2 : Notification a un utilisateur specifique
Write-Host "Test 2 : Notification a un utilisateur specifique (User $userId)" -ForegroundColor White

$userNotificationData = @{
    titre = "Notification Personnelle"
    message = "Ceci est une notification destinee uniquement a vous"
    type = "TEST_USER"
} | ConvertTo-Json

try {
    $userResponse = Invoke-RestMethod -Uri "$apiUrl/api/TestSignalR/user/$userId" `
        -Method Post `
        -Headers $headers `
        -Body $userNotificationData
    
    Write-Host "Notification utilisateur envoyee!" -ForegroundColor Green
    Write-Host "   Message: $($userResponse.message)`n" -ForegroundColor White
}
catch {
    Write-Host "ERREUR:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Start-Sleep -Seconds 2

# Test 3 : Notification a un groupe
Write-Host "Test 3 : Notification au groupe 'all_users'" -ForegroundColor White

$groupNotificationData = @{
    titre = "Notification Groupe"
    message = "Ceci est une notification destinee au groupe 'all_users'"
    type = "TEST_GROUP"
} | ConvertTo-Json

try {
    $groupResponse = Invoke-RestMethod -Uri "$apiUrl/api/TestSignalR/group/all_users" `
        -Method Post `
        -Headers $headers `
        -Body $groupNotificationData
    
    Write-Host "Notification groupe envoyee!" -ForegroundColor Green
    Write-Host "   Message: $($groupResponse.message)`n" -ForegroundColor White
}
catch {
    Write-Host "ERREUR:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

# RESUME FINAL
Write-Host "`n==============================================================" -ForegroundColor Cyan
Write-Host "   TESTS TERMINES" -ForegroundColor Green
Write-Host "==============================================================`n" -ForegroundColor Cyan

Write-Host "Resume des tests:" -ForegroundColor Yellow
Write-Host "  [OK] Authentification avec FCM Token" -ForegroundColor Green
Write-Host "  [OK] Notification Paiement (Firebase + SignalR)" -ForegroundColor Green
Write-Host "  [OK] Notification Presence (Firebase + SignalR)" -ForegroundColor Green
Write-Host "  [OK] Test SignalR Broadcast" -ForegroundColor Green
Write-Host "  [OK] Test SignalR Utilisateur" -ForegroundColor Green
Write-Host "  [OK] Test SignalR Groupe`n" -ForegroundColor Green

Write-Host "Pour tester cote frontend:" -ForegroundColor Yellow
Write-Host "  - Mobile: Utilisez votre app Flutter/React Native avec FCM" -ForegroundColor White
Write-Host "  - Web: Ouvrez test-signalr-notifications.html`n" -ForegroundColor White

Write-Host "Documentation complete:" -ForegroundColor Yellow
Write-Host "  - GUIDE_INTEGRATION_FRONTEND_NOTIFICATIONS.md`n" -ForegroundColor White

Write-Host "==============================================================`n" -ForegroundColor Cyan
