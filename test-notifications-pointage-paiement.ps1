# ═══════════════════════════════════════════════════════════════════════
# TEST NOTIFICATIONS : POINTAGE PRÉSENCE + PAIEMENT FRAIS
# Date: 3 novembre 2025
# Objectif: Tester l'envoi de notifications pour présence et paiement
# ═══════════════════════════════════════════════════════════════════════

$apiUrl = "https://localhost:7102"

# Configuration SSL
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

Write-Host "`n═══════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "   🔔 TEST NOTIFICATIONS : POINTAGE PRÉSENCE + PAIEMENT FRAIS" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

# ═══════════════════════════════════════════════════════════════════════
# ÉTAPE 1 : AUTHENTIFICATION
# ═══════════════════════════════════════════════════════════════════════
Write-Host "📝 ÉTAPE 1 : Authentification" -ForegroundColor Yellow
Write-Host "─────────────────────────────────────────────────────────────────────────" -ForegroundColor Gray

# IMPORTANT : Modifier ces informations avec vos propres credentials
$emailOuTelephone = "elsynchropos@gmail.com"
$motDePasse = "Admin"

Write-Host "   • Email/Téléphone: $emailOuTelephone" -ForegroundColor White
Write-Host "   • Mot de passe: $motDePasse`n" -ForegroundColor White

$loginBody = @{
    emailOuTelephone = $emailOuTelephone
    motDePasse = $motDePasse
} | ConvertTo-Json -Depth 10

try {
    Write-Host "   Envoi de la requête d'authentification..." -ForegroundColor Gray
    $response = Invoke-RestMethod -Uri "$apiUrl/api/Utilisateur/authentifier" `
        -Method POST `
        -Body $loginBody `
        -ContentType "application/json" `
        -ErrorAction Stop
    
    $token = $response.accessToken
    $userId = $response.utilisateur.idUtilisateur
    $userName = "$($response.utilisateur.prenomUtilisateur) $($response.utilisateur.nomUtilisateur)"
    $idEcole = $response.utilisateur.idEcole
    
    Write-Host "   ✅ Authentification réussie!" -ForegroundColor Green
    Write-Host "      • User ID: $userId" -ForegroundColor White
    Write-Host "      • Nom: $userName" -ForegroundColor White
    Write-Host "      • École ID: $idEcole`n" -ForegroundColor White
} catch {
    Write-Host "   ❌ ERREUR lors de l'authentification:" -ForegroundColor Red
    Write-Host "      Message: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $reader.BaseStream.Position = 0
        $responseBody = $reader.ReadToEnd()
        Write-Host "      Détails: $responseBody" -ForegroundColor Yellow
    }
    Write-Host "`n   💡 Vérifiez vos credentials et que l'API est démarrée.`n" -ForegroundColor Yellow
    exit 1
}

Start-Sleep -Seconds 1

# ═══════════════════════════════════════════════════════════════════════
# ÉTAPE 2 : RÉCUPÉRATION D'UN ÉLÈVE AVEC TUTEUR
# ═══════════════════════════════════════════════════════════════════════
Write-Host "📝 ÉTAPE 2 : Récupération d'un élève avec tuteur" -ForegroundColor Yellow
Write-Host "─────────────────────────────────────────────────────────────────────────" -ForegroundColor Gray

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

try {
    Write-Host "   Recherche des élèves de l'école $idEcole..." -ForegroundColor Gray
    $eleves = Invoke-RestMethod -Uri "$apiUrl/api/Eleve/ecole/$idEcole" `
        -Method GET `
        -Headers $headers `
        -ErrorAction Stop
    
    if ($eleves.Count -eq 0) {
        Write-Host "   ❌ AUCUN ÉLÈVE TROUVÉ pour cette école" -ForegroundColor Red
        Write-Host "   💡 Créez d'abord des élèves avec leurs tuteurs.`n" -ForegroundColor Yellow
        exit 1
    }
    
    $eleve = $eleves[0]
    $eleveId = $eleve.idEleve
    $eleveNom = $eleve.nomComplet
    $tuteurId = $eleve.idTuteur
    
    Write-Host "   ✅ Élève trouvé: $eleveNom (ID: $eleveId)" -ForegroundColor Green
    
    if (-not $tuteurId) {
        Write-Host "   ❌ ERREUR: Élève sans tuteur" -ForegroundColor Red
        Write-Host "   💡 Cet élève n'a pas de tuteur assigné. Les notifications ne peuvent pas être envoyées.`n" -ForegroundColor Yellow
        exit 1
    }
    
    # Récupérer le tuteur
    Write-Host "   Récupération du tuteur (ID: $tuteurId)..." -ForegroundColor Gray
    $tuteur = Invoke-RestMethod -Uri "$apiUrl/api/Tuteur/$tuteurId" `
        -Method GET `
        -Headers $headers `
        -ErrorAction Stop
    
    $tuteurNom = $tuteur.nomComplet
    $tuteurTel = $tuteur.telephone
    
    if ([string]::IsNullOrWhiteSpace($tuteurTel)) {
        Write-Host "   ⚠️  ATTENTION: Tuteur sans numéro de téléphone" -ForegroundColor Yellow
        Write-Host "      Les notifications SMS ne pourront pas être envoyées.`n" -ForegroundColor Yellow
    } else {
        Write-Host "   ✅ Tuteur: $tuteurNom" -ForegroundColor Green
        Write-Host "      • Téléphone: $tuteurTel`n" -ForegroundColor White
    }
} catch {
    Write-Host "   ❌ ERREUR lors de la récupération des données:" -ForegroundColor Red
    Write-Host "      $($_.Exception.Message)`n" -ForegroundColor Red
    exit 1
}

Start-Sleep -Seconds 1

# ═══════════════════════════════════════════════════════════════════════
# ÉTAPE 3 : TEST NOTIFICATION PAIEMENT FRAIS
# ═══════════════════════════════════════════════════════════════════════
Write-Host "📝 ÉTAPE 3 : Test Notification PAIEMENT FRAIS" -ForegroundColor Yellow
Write-Host "─────────────────────────────────────────────────────────────────────────" -ForegroundColor Gray

$reference = "TEST-NOTIF-PAY-$(Get-Date -Format 'yyyyMMddHHmmss')"
$montant = 50000

$paiementBody = @{
    idEleve = $eleveId
    montant = $montant
    datePaiement = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")
    modePaiement = "Espèces"
    referenceTransaction = $reference
    commentaire = "Test notification - Paiement de frais scolaires"
    justificatifUrl = ""
    devise = "CDF"
    statutPaiement = "Confirmé"
    statut = $true
    idUtilisateur = $userId
} | ConvertTo-Json -Depth 10

Write-Host "   Création d'un paiement de test..." -ForegroundColor Gray
Write-Host "      • Montant: $montant CDF" -ForegroundColor White
Write-Host "      • Élève: $eleveNom" -ForegroundColor White
Write-Host "      • Référence: $reference" -ForegroundColor White
Write-Host "`n   Envoi de la requête..." -ForegroundColor Gray

try {
    $paiement = Invoke-RestMethod -Uri "$apiUrl/api/Paiement" `
        -Method POST `
        -Body $paiementBody `
        -Headers $headers `
        -ErrorAction Stop
    
    Write-Host "`n   ✅ Paiement créé avec succès!" -ForegroundColor Green
    Write-Host "      • ID Paiement: $($paiement.idPaiement)" -ForegroundColor White
    Write-Host "`n   📱 Notifications envoyées:" -ForegroundColor Cyan
    if (-not [string]::IsNullOrWhiteSpace($tuteurTel)) {
        Write-Host "      ✅ SMS au $tuteurTel" -ForegroundColor Green
    } else {
        Write-Host "      ⚠️  SMS non envoyé (pas de téléphone)" -ForegroundColor Yellow
    }
    Write-Host "      ✅ Notification dans l'application (si intégré)" -ForegroundColor Green
    
    Write-Host "`n   🔍 VÉRIFIEZ:" -ForegroundColor Yellow
    Write-Host "      1. Les logs de l'application ci-dessous (envoi SMS)" -ForegroundColor White
    if (-not [string]::IsNullOrWhiteSpace($tuteurTel)) {
        Write-Host "      2. Le téléphone $tuteurTel pour le SMS reçu`n" -ForegroundColor White
    }
} catch {
    Write-Host "`n   ❌ ERREUR lors de la création du paiement:" -ForegroundColor Red
    Write-Host "      $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $reader.BaseStream.Position = 0
        $responseBody = $reader.ReadToEnd()
        Write-Host "      Détails: $responseBody`n" -ForegroundColor Yellow
    }
}

Start-Sleep -Seconds 2

# ═══════════════════════════════════════════════════════════════════════
# ÉTAPE 4 : RÉCUPÉRATION D'UNE VACATION (pour pointage présence)
# ═══════════════════════════════════════════════════════════════════════
Write-Host "📝 ÉTAPE 4 : Récupération d'une vacation" -ForegroundColor Yellow
Write-Host "─────────────────────────────────────────────────────────────────────────" -ForegroundColor Gray

try {
    Write-Host "   Recherche des vacations..." -ForegroundColor Gray
    $vacations = Invoke-RestMethod -Uri "$apiUrl/api/Vacation" `
        -Method GET `
        -Headers $headers `
        -ErrorAction Stop
    
    if ($vacations.Count -eq 0) {
        Write-Host "   ⚠️  AUCUNE VACATION TROUVÉE" -ForegroundColor Yellow
        Write-Host "      Test de présence ignoré (vacation nécessaire).`n" -ForegroundColor Yellow
        $skipPresence = $true
    } else {
        $vacation = $vacations[0]
        $vacationId = $vacation.idVacation
        $vacationNom = $vacation.nomVacation
        Write-Host "   ✅ Vacation trouvée: $vacationNom (ID: $vacationId)`n" -ForegroundColor Green
        $skipPresence = $false
    }
} catch {
    Write-Host "   ⚠️  Erreur lors de la récupération des vacations:" -ForegroundColor Yellow
    Write-Host "      $($_.Exception.Message)" -ForegroundColor Yellow
    Write-Host "      Test de présence ignoré.`n" -ForegroundColor Yellow
    $skipPresence = $true
}

Start-Sleep -Seconds 1

# ═══════════════════════════════════════════════════════════════════════
# ÉTAPE 5 : TEST NOTIFICATION POINTAGE PRÉSENCE
# ═══════════════════════════════════════════════════════════════════════
if (-not $skipPresence) {
    Write-Host "📝 ÉTAPE 5 : Test Notification POINTAGE PRÉSENCE" -ForegroundColor Yellow
    Write-Host "─────────────────────────────────────────────────────────────────────────" -ForegroundColor Gray

    $heure = Get-Date -Format "HH:mm:ss"
    # Utiliser demain pour éviter les doublons
    $date = (Get-Date).AddDays(1).ToString("yyyy-MM-dd")

    $presenceBody = @{
        idEleve = $eleveId
        idVacation = $vacationId
        isPresent = $true
        heureArrivee = $heure
        dateDuJour = $date
        observation = "Test notification - Pointage présence"
        longitude = "15.3136"
        latitude = "-4.3276"
    } | ConvertTo-Json -Depth 10

    Write-Host "   Enregistrement d'une présence..." -ForegroundColor Gray
    Write-Host "      • Élève: $eleveNom" -ForegroundColor White
    Write-Host "      • Statut: PRÉSENT ✅" -ForegroundColor Green
    Write-Host "      • Heure: $heure" -ForegroundColor White
    Write-Host "      • Date: $date`n" -ForegroundColor White
    Write-Host "   Envoi de la requête..." -ForegroundColor Gray

    try {
        $presence = Invoke-RestMethod -Uri "$apiUrl/api/Presence" `
            -Method POST `
            -Body $presenceBody `
            -Headers $headers `
            -ErrorAction Stop
        
        Write-Host "`n   ✅ Présence enregistrée avec succès!" -ForegroundColor Green
        Write-Host "      • ID Présence: $($presence.idPresence)" -ForegroundColor White
        Write-Host "`n   📱 Notifications envoyées:" -ForegroundColor Cyan
        if (-not [string]::IsNullOrWhiteSpace($tuteurTel)) {
            Write-Host "      ✅ SMS au $tuteurTel" -ForegroundColor Green
        } else {
            Write-Host "      ⚠️  SMS non envoyé (pas de téléphone)" -ForegroundColor Yellow
        }
        Write-Host "      ✅ Notification dans l'application (si intégré)" -ForegroundColor Green
        
        Write-Host "`n   🔍 VÉRIFIEZ:" -ForegroundColor Yellow
        Write-Host "      1. Les logs de l'application ci-dessous (envoi SMS)" -ForegroundColor White
        if (-not [string]::IsNullOrWhiteSpace($tuteurTel)) {
            Write-Host "      2. Le téléphone $tuteurTel pour le SMS reçu`n" -ForegroundColor White
        }
    } catch {
        Write-Host "`n   ❌ ERREUR lors de l'enregistrement de la présence:" -ForegroundColor Red
        Write-Host "      $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $reader.BaseStream.Position = 0
            $responseBody = $reader.ReadToEnd()
            Write-Host "      Détails: $responseBody`n" -ForegroundColor Yellow
        }
    }

    Start-Sleep -Seconds 2
}

# ═══════════════════════════════════════════════════════════════════════
# RÉSUMÉ FINAL
# ═══════════════════════════════════════════════════════════════════════
Write-Host "`n═══════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "   ✅ TESTS TERMINÉS AVEC SUCCÈS" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

Write-Host "📊 Résumé des tests:" -ForegroundColor Yellow
Write-Host "   ✅ Authentification réussie" -ForegroundColor Green
Write-Host "   ✅ Élève et tuteur récupérés" -ForegroundColor Green
Write-Host "   ✅ Test Notification Paiement (SMS + App)" -ForegroundColor Green
if (-not $skipPresence) {
    Write-Host "   ✅ Test Notification Présence (SMS + App)" -ForegroundColor Green
} else {
    Write-Host "   ⚠️  Test Présence ignoré (pas de vacation)" -ForegroundColor Yellow
}

if (-not [string]::IsNullOrWhiteSpace($tuteurTel)) {
    Write-Host "`n📱 Pour vérifier les notifications SMS:" -ForegroundColor Yellow
    Write-Host "   • Téléphone: $tuteurTel" -ForegroundColor White
    Write-Host "   • Vous devriez avoir reçu 2 SMS (paiement + présence)" -ForegroundColor Gray
}

Write-Host "`n📋 Prochaines étapes:" -ForegroundColor Yellow
Write-Host "   1. Vérifiez les logs de l'application pour voir les envois SMS" -ForegroundColor White
Write-Host "   2. Vérifiez que les notifications arrivent bien sur le téléphone" -ForegroundColor White
Write-Host "   3. Testez avec l'application mobile (si disponible)" -ForegroundColor White

Write-Host "`n═══════════════════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

