$apiUrl = "https://localhost:7105"
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

Write-Host "`n════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "   🔔 TEST NOTIFICATIONS SIGNALR - ACTIONS RÉELLES" -ForegroundColor White
Write-Host "════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

Write-Host "📋 Ce script va tester les notifications SignalR pour:" -ForegroundColor Yellow
Write-Host "  💰 Paiement de frais" -ForegroundColor Green
Write-Host "  ✓ Pointage de présence" -ForegroundColor Cyan
Write-Host "  📝 Inscription d'élève`n" -ForegroundColor Magenta

# 1. Connexion
Write-Host "1. Authentification..." -ForegroundColor Yellow
$loginBody = '{"emailOuTelephone":"elsynchropos@gmail.com","motDePasse":"Admin"}'
try {
    $response = Invoke-RestMethod -Uri "$apiUrl/api/Utilisateur/authentifier" -Method POST -Body $loginBody -ContentType "application/json" -ErrorAction Stop
    $token = $response.AccessToken
    $idEcole = $response.Utilisateur.IdEcole
    Write-Host "   ✅ Token recu, IdEcole: $idEcole`n" -ForegroundColor Green
} catch {
    Write-Host "   ❌ ERREUR connexion: $_" -ForegroundColor Red
    exit 1
}

$headers = @{}
$headers["Authorization"] = "Bearer $token"
$headers["Content-Type"] = "application/json"

# 2. Récupération des données nécessaires
Write-Host "2. Récupération des données..." -ForegroundColor Yellow
try {
    $eleves = Invoke-RestMethod -Uri "$apiUrl/api/Eleve" -Method GET -Headers $headers -ErrorAction Stop
    if ($eleves.Count -eq 0) {
        Write-Host "   ❌ Aucun élève trouvé" -ForegroundColor Red
        exit 1
    }
    $eleve = $eleves[0]
    Write-Host "   ✅ Élève: $($eleve.nomComplet)" -ForegroundColor Green
    
    $vacations = Invoke-RestMethod -Uri "$apiUrl/api/Vacation" -Method GET -Headers $headers -ErrorAction Stop
    if ($vacations.Count -eq 0) {
        Write-Host "   ❌ Aucune vacation trouvée" -ForegroundColor Red
        exit 1
    }
    $vacation = $vacations[0]
    Write-Host "   ✅ Vacation: $($vacation.nomVacation)`n" -ForegroundColor Green
} catch {
    Write-Host "   ❌ ERREUR: $_" -ForegroundColor Red
    exit 1
}

Write-Host "════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "   📍 INSTRUCTIONS" -ForegroundColor Yellow
Write-Host "════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "Avant de continuer, veuillez:" -ForegroundColor White
Write-Host "  1️⃣  Ouvrir test-signalr-notifications.html dans votre navigateur" -ForegroundColor Cyan
Write-Host "  2️⃣  Cliquer sur le bouton '▶️ Connecter'" -ForegroundColor Cyan
Write-Host "  3️⃣  Vérifier que le statut indique '● Connecté' (VERT)" -ForegroundColor Cyan
Write-Host ""
Write-Host "Une fois connecté, appuyez sur ENTRÉE pour continuer..." -ForegroundColor Yellow
Read-Host

Write-Host "`n════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "   🧪 DÉBUT DES TESTS" -ForegroundColor White
Write-Host "════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

# 3. TEST PAIEMENT
Write-Host "3. Test PAIEMENT..." -ForegroundColor Yellow
$timestamp = Get-Date -Format "yyyyMMddHHmmss"
$reference = "SIGNALR_TEST_$timestamp"

$paiementBody = @{
    datePaiement = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")
    montant = 75.0
    devise = "USD"
    modePaiement = "Cash"
    statutPaiement = "Confirme"
    statut = $true
    referenceTransaction = $reference
    justificatifUrl = ""
    commentaire = "Test SignalR Paiement"
    idEleve = $eleve.idEleve
    idUtilisateur = 1
} | ConvertTo-Json -Depth 10

try {
    $paiement = Invoke-RestMethod -Uri "$apiUrl/api/Paiement" -Method POST -Body $paiementBody -Headers $headers -ErrorAction Stop
    Write-Host "   ✅ Paiement créé (ID: $($paiement.idPaiement))" -ForegroundColor Green
    Write-Host "   💰 Montant: $($paiement.montant) $($paiement.devise)" -ForegroundColor Gray
    Write-Host "   🔔 Notification SignalR envoyée au parent !" -ForegroundColor Cyan
} catch {
    Write-Host "   ❌ ERREUR: $_" -ForegroundColor Red
}

Write-Host ""
Start-Sleep -Seconds 2

# 4. TEST PRÉSENCE
Write-Host "4. Test PRÉSENCE..." -ForegroundColor Yellow
$heure = Get-Date -Format "HH:mm:ss"
$dateDemain = (Get-Date).AddDays(1).ToString("yyyy-MM-dd")

$presenceBody = @{
    idEleve = $eleve.idEleve
    idVacation = $vacation.idVacation
    isPresent = $true
    heureArrivee = $heure
    dateDuJour = $dateDemain
    observation = "Test SignalR Présence"
    longitute = "15.3136"
    latitude = "-4.3276"
} | ConvertTo-Json -Depth 10

try {
    $presence = Invoke-RestMethod -Uri "$apiUrl/api/Presence" -Method POST -Body $presenceBody -Headers $headers -ErrorAction Stop
    Write-Host "   ✅ Présence créée (ID: $($presence.idPresence))" -ForegroundColor Green
    Write-Host "   ✓ Statut: PRÉSENT à $heure" -ForegroundColor Gray
    Write-Host "   🔔 Notification SignalR envoyée au parent !" -ForegroundColor Cyan
} catch {
    Write-Host "   ❌ ERREUR: $_" -ForegroundColor Red
    Write-Host "   Details: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Start-Sleep -Seconds 2

# 5. TEST INSCRIPTION
Write-Host "5. Test INSCRIPTION..." -ForegroundColor Yellow
$timestampInscr = (Get-Date).ToString("HHmmss")

try {
    $classes = Invoke-RestMethod -Uri "$apiUrl/api/Classe/ecole/$idEcole" -Method GET -Headers $headers -ErrorAction Stop
    $classe = $classes[0]
    
    $annees = Invoke-RestMethod -Uri "$apiUrl/api/AnneeScolaire" -Method GET -Headers $headers -ErrorAction Stop
    $annee = $annees[0]
    
    $inscriptionBody = @{
        type = "Inscription"
        idEcole = $idEcole
        idClasse = $classe.idClasse
        idAnneeScolaire = $annee.idAnneeScolaire
        dateInscription = (Get-Date).ToString("yyyy-MM-dd")
        statutInscription = "Confirmé"
        nomEleve = "SIGNAL"
        postnomEleve = "R"
        prenomEleve = "Test_$timestampInscr"
        genreEleve = "M"
        dateNaissanceEleve = (Get-Date).AddYears(-10).ToString("yyyy-MM-dd")
        lieuNaissanceEleve = "Kinshasa"
        nationaliteEleve = "Congolaise"
        provinceEleve = "Kinshasa"
        villeEleve = "Kinshasa"
        communeEleve = "Kalamu"
        quartierEleve = "Matonge"
        nomCompletTuteur = "Papa SignalR Test"
        genreTuteur = "M"
        emailTuteur = "papa.signalr.test_$timestampInscr@email.com"
        telephoneTuteur = "+243812726582"
    } | ConvertTo-Json -Depth 10
    
    $ecole = Invoke-RestMethod -Uri "$apiUrl/api/Ecole/$idEcole" -Method GET -Headers $headers -ErrorAction Stop
    $inscription = Invoke-RestMethod -Uri "$apiUrl/api/Inscription/new/$($ecole.nom)" -Method POST -Body $inscriptionBody -Headers $headers -ErrorAction Stop
    Write-Host "   ✅ Inscription créée (ID: $($inscription.idInscription))" -ForegroundColor Green
    Write-Host "   📝 Élève: SIGNAL R Test_$timestampInscr" -ForegroundColor Gray
    Write-Host "   🔔 Notification SignalR envoyée au parent !" -ForegroundColor Cyan
} catch {
    Write-Host "   ❌ ERREUR: $_" -ForegroundColor Red
    Write-Host "   Details: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "   ✅ TESTS TERMINÉS" -ForegroundColor Green
Write-Host "════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

Write-Host "Resultat:" -ForegroundColor Yellow
Write-Host "  Verifiez dans votre navigateur" -ForegroundColor White
Write-Host "  Vous devriez voir 3 notifications:" -ForegroundColor White
Write-Host "    - 1 notification de PAIEMENT" -ForegroundColor Green
Write-Host "    - 1 notification de PRESENCE" -ForegroundColor Cyan
Write-Host "    - 1 notification INSCRIPTION" -ForegroundColor Magenta

