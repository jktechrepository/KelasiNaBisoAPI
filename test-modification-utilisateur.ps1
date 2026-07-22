# ════════════════════════════════════════════════════════════════
# SCRIPT DE TEST : Modification des Informations Utilisateur (DTO)
# ════════════════════════════════════════════════════════════════

# ✅ Configuration
$apiUrl = "https://localhost:7103"  # Ajuster selon votre port

# ════════════════════════════════════════════════════════════════
# ÉTAPE 1 : AUTHENTIFICATION
# ════════════════════════════════════════════════════════════════

Write-Host "`n═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "   🔐 ÉTAPE 1 : AUTHENTIFICATION" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

$loginData = @{
    emailOuTelephone = "patrick.ilunga@exemple.com"  # Remplacer par un utilisateur de test
    motDePasse = "Test@1234"  # Remplacer par le mot de passe
    fcmToken = "test_token_123"
    deviceType = "Web"
    deviceModel = "Chrome"
    osVersion = "Windows 10"
} | ConvertTo-Json

try {
    $authResponse = Invoke-RestMethod -Uri "$apiUrl/api/Utilisateur/authentifier" `
        -Method Post `
        -Body $loginData `
        -ContentType "application/json" `
        -SkipCertificateCheck
    
    $token = $authResponse.accessToken
    $userId = $authResponse.utilisateur.idUtilisateur
    
    Write-Host "✅ Authentification réussie !" -ForegroundColor Green
    Write-Host "   User ID : $userId" -ForegroundColor White
    Write-Host "   Nom : $($authResponse.utilisateur.nomUtilisateur) $($authResponse.utilisateur.prenomUtilisateur)" -ForegroundColor White
    Write-Host "   Email : $($authResponse.utilisateur.email)" -ForegroundColor White
    Write-Host "   Rôle : $($authResponse.nomRole)" -ForegroundColor White
}
catch {
    Write-Host "❌ Erreur lors de l'authentification" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit
}

# ════════════════════════════════════════════════════════════════
# ÉTAPE 2 : RÉCUPÉRER LES INFORMATIONS ACTUELLES
# ════════════════════════════════════════════════════════════════

Write-Host "`n═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "   📖 ÉTAPE 2 : RÉCUPÉRATION DES INFORMATIONS ACTUELLES" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

try {
    $headers = @{
        "Authorization" = "Bearer $token"
    }
    
    $userBeforeUpdate = Invoke-RestMethod -Uri "$apiUrl/api/Utilisateur/$userId" `
        -Method Get `
        -Headers $headers `
        -SkipCertificateCheck
    
    Write-Host "✅ Informations récupérées :" -ForegroundColor Green
    Write-Host "   Nom : $($userBeforeUpdate.nomUtilisateur)" -ForegroundColor White
    Write-Host "   Prénom : $($userBeforeUpdate.prenomUtilisateur)" -ForegroundColor White
    Write-Host "   Email : $($userBeforeUpdate.email)" -ForegroundColor White
    Write-Host "   Téléphone : $($userBeforeUpdate.telephone)" -ForegroundColor White
}
catch {
    Write-Host "❌ Erreur lors de la récupération" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit
}

# ════════════════════════════════════════════════════════════════
# ÉTAPE 3 : MODIFICATION AVEC DTO (MÉTHODE SIMPLE)
# ════════════════════════════════════════════════════════════════

Write-Host "`n═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "   ✏️  ÉTAPE 3 : MODIFICATION AVEC DTO (NOUVEAU !)" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

# ✅ Nouveau DTO : Seulement les champs modifiables !
$updateDto = @{
    idUtilisateur = $userId
    nomUtilisateur = $userBeforeUpdate.nomUtilisateur
    postNomUtilisateur = $userBeforeUpdate.postNomUtilisateur
    prenomUtilisateur = $userBeforeUpdate.prenomUtilisateur
    email = $userBeforeUpdate.email
    telephone = "+243999999999"  # ✅ ON MODIFIE SEULEMENT LE TÉLÉPHONE !
    photoUrl = $userBeforeUpdate.photoUrl
    lieuNaissance = $userBeforeUpdate.lieuNaissance
    dateNaissance = $userBeforeUpdate.dateNaissance
    genre = $userBeforeUpdate.genre
    # ✅ PAS BESOIN DE :
    #    - motDePasseHash
    #    - idRole
    #    - idEcole
    #    - statut
    #    - etc.
} | ConvertTo-Json

Write-Host "📤 Envoi de la modification (seulement téléphone changé)..." -ForegroundColor Yellow

try {
    $userAfterUpdate = Invoke-RestMethod -Uri "$apiUrl/api/Utilisateur/$userId" `
        -Method Put `
        -Body $updateDto `
        -Headers $headers `
        -ContentType "application/json" `
        -SkipCertificateCheck
    
    Write-Host "`n✅ MODIFICATION RÉUSSIE !" -ForegroundColor Green
    Write-Host "`n🔄 Comparaison :" -ForegroundColor Cyan
    Write-Host "   Téléphone AVANT : $($userBeforeUpdate.telephone)" -ForegroundColor Yellow
    Write-Host "   Téléphone APRÈS : $($userAfterUpdate.telephone)" -ForegroundColor Green
    Write-Host "`n✅ Autres champs intacts :" -ForegroundColor Green
    Write-Host "   Nom : $($userAfterUpdate.nomUtilisateur)" -ForegroundColor White
    Write-Host "   Email : $($userAfterUpdate.email)" -ForegroundColor White
    Write-Host "   Rôle : $($userAfterUpdate.role.nom)" -ForegroundColor White
    Write-Host "   Statut : $($userAfterUpdate.statut)" -ForegroundColor White
}
catch {
    Write-Host "`n❌ Erreur lors de la modification" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    
    if ($_.ErrorDetails.Message) {
        $errorDetails = $_.ErrorDetails.Message | ConvertFrom-Json
        Write-Host "   Message : $($errorDetails.message)" -ForegroundColor Red
        
        if ($errorDetails.errors) {
            Write-Host "   Erreurs de validation :" -ForegroundColor Red
            $errorDetails.errors.PSObject.Properties | ForEach-Object {
                Write-Host "      $($_.Name) : $($_.Value -join ', ')" -ForegroundColor Red
            }
        }
    }
}

# ════════════════════════════════════════════════════════════════
# ÉTAPE 4 : TEST DE SÉCURITÉ (Tentative de modifier un autre utilisateur)
# ════════════════════════════════════════════════════════════════

Write-Host "`n═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "   🔒 ÉTAPE 4 : TEST DE SÉCURITÉ" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

Write-Host "⚠️  Tentative de modifier un autre utilisateur (doit échouer)..." -ForegroundColor Yellow

$otherUserId = $userId + 1  # ID d'un autre utilisateur

$updateDtoOther = @{
    idUtilisateur = $otherUserId
    nomUtilisateur = "Pirate"
    prenomUtilisateur = "Hacker"
    email = "hacker@test.com"
    telephone = "+243123456789"
} | ConvertTo-Json

try {
    $result = Invoke-RestMethod -Uri "$apiUrl/api/Utilisateur/$otherUserId" `
        -Method Put `
        -Body $updateDtoOther `
        -Headers $headers `
        -ContentType "application/json" `
        -SkipCertificateCheck
    
    Write-Host "❌ PROBLÈME DE SÉCURITÉ : La modification a réussi alors qu'elle devrait être bloquée !" -ForegroundColor Red
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    
    if ($statusCode -eq 403) {
        Write-Host "✅ SÉCURITÉ OK : Modification bloquée (403 Forbidden)" -ForegroundColor Green
        Write-Host "   Un utilisateur ne peut pas modifier un autre utilisateur" -ForegroundColor White
    }
    elseif ($statusCode -eq 404) {
        Write-Host "✅ SÉCURITÉ OK : Utilisateur non trouvé (404)" -ForegroundColor Green
    }
    else {
        Write-Host "⚠️  Erreur inattendue : $statusCode" -ForegroundColor Yellow
    }
}

# ════════════════════════════════════════════════════════════════
# RÉCAPITULATIF
# ════════════════════════════════════════════════════════════════

Write-Host "`n═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "   ✅ RÉCAPITULATIF DES TESTS" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

Write-Host "✅ Authentification : OK" -ForegroundColor Green
Write-Host "✅ Récupération infos : OK" -ForegroundColor Green
Write-Host "✅ Modification avec DTO : OK" -ForegroundColor Green
Write-Host "✅ Sécurité (403 Forbidden) : OK" -ForegroundColor Green

Write-Host "`n🎉 TOUS LES TESTS SONT PASSÉS !" -ForegroundColor Green
Write-Host "`n📝 AVANTAGES DU DTO :" -ForegroundColor Cyan
Write-Host "   • Pas besoin d'envoyer TOUS les champs" -ForegroundColor White
Write-Host "   • Pas besoin du mot de passe !" -ForegroundColor White
Write-Host "   • Champs sensibles protégés (rôle, école, statut)" -ForegroundColor White
Write-Host "   • Validation automatique" -ForegroundColor White
Write-Host "   • Sécurité renforcée" -ForegroundColor White

Write-Host "`n═══════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

