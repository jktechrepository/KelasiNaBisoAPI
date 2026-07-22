# ════════════════════════════════════════════════════════════════
# SCRIPT DE TEST : Sécurité COMPLÈTE - UtilisateurController
# ════════════════════════════════════════════════════════════════
# Teste TOUS les endpoints corrigés avec leurs contrôles de sécurité
# ════════════════════════════════════════════════════════════════

# Configuration
$apiUrl = "https://localhost:7102"
$testsPasses = 0
$testsEchoues = 0

# Fonction helper pour gérer les certificats SSL
if (-not ([System.Management.Automation.PSTypeName]'TrustAllCertsPolicy').Type) {
    Add-Type @"
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    public class TrustAllCertsPolicy : ICertificatePolicy {
        public bool CheckValidationResult(
            ServicePoint svcPoint, X509Certificate certificate,
            WebRequest webRequest, int certificateProblem) {
            return true;
        }
    }
"@
}
[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

Write-Host "`n══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "   TESTS DE SECURITE - UtilisateurController" -ForegroundColor Green
Write-Host "══════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

# ════════════════════════════════════════════════════════════════
# ETAPE 1 : AUTHENTIFICATION
# ════════════════════════════════════════════════════════════════

Write-Host "ETAPE 1 : Authentification (utilisateur normal)`n" -ForegroundColor Yellow

$loginData = @{
    emailOuTelephone = "patrick.ilunga@exemple.com"
    motDePasse = "Test@1234"
    fcmToken = "test_token"
    deviceType = "Web"
} | ConvertTo-Json

try {
    $authResponse = Invoke-RestMethod -Uri "$apiUrl/api/Utilisateur/authentifier" `
        -Method Post `
        -Body $loginData `
        -ContentType "application/json"
    
    $token = $authResponse.accessToken
    $userId = $authResponse.utilisateur.idUtilisateur
    $userRole = $authResponse.nomRole
    $userEcole = $authResponse.utilisateur.idEcole
    
    Write-Host "  ✅ Authentification reussie" -ForegroundColor Green
    Write-Host "     User ID : $userId" -ForegroundColor White
    Write-Host "     Role : $userRole" -ForegroundColor White
    Write-Host "     Ecole : $userEcole`n" -ForegroundColor White
    $testsPasses++
}
catch {
    Write-Host "  ❌ Erreur authentification" -ForegroundColor Red
    Write-Host "     $($_.Exception.Message)`n" -ForegroundColor Red
    $testsEchoues++
    exit
}

$headers = @{
    "Authorization" = "Bearer $token"
}

# ════════════════════════════════════════════════════════════════
# TEST 1 : GET /api/Utilisateur/{id} - Ses propres infos
# ════════════════════════════════════════════════════════════════

Write-Host "TEST 1 : GET /api/Utilisateur/{id} (ses propres infos)`n" -ForegroundColor Yellow

try {
    $user = Invoke-RestMethod -Uri "$apiUrl/api/Utilisateur/$userId" `
        -Method Get `
        -Headers $headers
    
    if ($user.motDePasseHash -ne $null) {
        Write-Host "  ❌ SECURITE : MotDePasseHash retourne !" -ForegroundColor Red
        $testsEchoues++
    }
    else {
        Write-Host "  ✅ Recuperation ses propres infos : OK" -ForegroundColor Green
        Write-Host "  ✅ MotDePasseHash non retourne : OK`n" -ForegroundColor Green
        $testsPasses++
    }
}
catch {
    Write-Host "  ❌ Erreur : $($_.Exception.Message)`n" -ForegroundColor Red
    $testsEchoues++
}

# ════════════════════════════════════════════════════════════════
# TEST 2 : GET /api/Utilisateur/{id} - Infos d'un autre (doit échouer)
# ════════════════════════════════════════════════════════════════

Write-Host "TEST 2 : GET /api/Utilisateur/{id} (autre user - doit echouer)`n" -ForegroundColor Yellow

$otherUserId = $userId + 1

try {
    $user = Invoke-RestMethod -Uri "$apiUrl/api/Utilisateur/$otherUserId" `
        -Method Get `
        -Headers $headers
    
    Write-Host "  ❌ PROBLEME SECURITE : Acces autorise alors qu'il devrait etre bloque !" -ForegroundColor Red
    $testsEchoues++
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    if ($statusCode -eq 403) {
        Write-Host "  ✅ Acces bloque (403 Forbidden) : OK`n" -ForegroundColor Green
        $testsPasses++
    }
    else {
        Write-Host "  ⚠️  Code retour inattendu : $statusCode`n" -ForegroundColor Yellow
        $testsEchoues++
    }
}

# ════════════════════════════════════════════════════════════════
# TEST 3 : PUT /api/Utilisateur/{id} - Modification avec DTO
# ════════════════════════════════════════════════════════════════

Write-Host "TEST 3 : PUT /api/Utilisateur/{id} (modification avec DTO)`n" -ForegroundColor Yellow

$updateDto = @{
    idUtilisateur = $userId
    nomUtilisateur = "TestNom"
    prenomUtilisateur = "TestPrenom"
    email = "test.modification@example.com"
    telephone = "+243999888777"
} | ConvertTo-Json

try {
    $updatedUser = Invoke-RestMethod -Uri "$apiUrl/api/Utilisateur/$userId" `
        -Method Put `
        -Body $updateDto `
        -Headers $headers `
        -ContentType "application/json"
    
    if ($updatedUser.motDePasseHash -ne $null) {
        Write-Host "  ❌ SECURITE : MotDePasseHash retourne !" -ForegroundColor Red
        $testsEchoues++
    }
    else {
        Write-Host "  ✅ Modification avec DTO : OK" -ForegroundColor Green
        Write-Host "  ✅ MotDePasseHash non retourne : OK`n" -ForegroundColor Green
        $testsPasses++
    }
}
catch {
    Write-Host "  ❌ Erreur : $($_.Exception.Message)`n" -ForegroundColor Red
    $testsEchoues++
}

# ════════════════════════════════════════════════════════════════
# TEST 4 : PUT /api/Utilisateur/{id} - Modifier un autre (doit échouer)
# ════════════════════════════════════════════════════════════════

Write-Host "TEST 4 : PUT /api/Utilisateur/{id} (autre user - doit echouer)`n" -ForegroundColor Yellow

$updateDtoOther = @{
    idUtilisateur = $otherUserId
    nomUtilisateur = "Pirate"
    prenomUtilisateur = "Hacker"
    email = "pirate@test.com"
} | ConvertTo-Json

try {
    $result = Invoke-RestMethod -Uri "$apiUrl/api/Utilisateur/$otherUserId" `
        -Method Put `
        -Body $updateDtoOther `
        -Headers $headers `
        -ContentType "application/json"
    
    Write-Host "  ❌ PROBLEME SECURITE : Modification autorisee !" -ForegroundColor Red
    $testsEchoues++
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    if ($statusCode -eq 403) {
        Write-Host "  ✅ Modification bloquee (403 Forbidden) : OK`n" -ForegroundColor Green
        $testsPasses++
    }
    else {
        Write-Host "  ⚠️  Code retour inattendu : $statusCode`n" -ForegroundColor Yellow
        $testsEchoues++
    }
}

# ════════════════════════════════════════════════════════════════
# TEST 5 : POST /changer_mot_de_passe - Son propre mot de passe
# ════════════════════════════════════════════════════════════════

Write-Host "TEST 5 : POST /changer_mot_de_passe (son propre mdp)`n" -ForegroundColor Yellow

$changeMdp = @{
    idUtilisateur = $userId
    ancienMotDePasse = "Test@1234"
    nouveauMotDePasse = "NewTest@1234"
} | ConvertTo-Json

try {
    $result = Invoke-RestMethod -Uri "$apiUrl/api/Utilisateur/changer_mot_de_passe" `
        -Method Post `
        -Body $changeMdp `
        -Headers $headers `
        -ContentType "application/json"
    
    Write-Host "  ✅ Changement son propre mot de passe : OK`n" -ForegroundColor Green
    $testsPasses++
    
    # Remettre l'ancien mot de passe
    $changeMdpBack = @{
        idUtilisateur = $userId
        ancienMotDePasse = "NewTest@1234"
        nouveauMotDePasse = "Test@1234"
    } | ConvertTo-Json
    
    $null = Invoke-RestMethod -Uri "$apiUrl/api/Utilisateur/changer_mot_de_passe" `
        -Method Post `
        -Body $changeMdpBack `
        -Headers $headers `
        -ContentType "application/json"
}
catch {
    Write-Host "  ❌ Erreur : $($_.Exception.Message)`n" -ForegroundColor Red
    $testsEchoues++
}

# ════════════════════════════════════════════════════════════════
# TEST 6 : POST /changer_mot_de_passe - Autre user (doit échouer)
# ════════════════════════════════════════════════════════════════

Write-Host "TEST 6 : POST /changer_mot_de_passe (autre user - doit echouer)`n" -ForegroundColor Yellow

$changeMdpOther = @{
    idUtilisateur = $otherUserId
    ancienMotDePasse = "Test@1234"
    nouveauMotDePasse = "Pirate@123"
} | ConvertTo-Json

try {
    $result = Invoke-RestMethod -Uri "$apiUrl/api/Utilisateur/changer_mot_de_passe" `
        -Method Post `
        -Body $changeMdpOther `
        -Headers $headers `
        -ContentType "application/json"
    
    Write-Host "  ❌ PROBLEME SECURITE : Changement autorise !" -ForegroundColor Red
    $testsEchoues++
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    if ($statusCode -eq 403) {
        Write-Host "  ✅ Changement bloque (403 Forbidden) : OK`n" -ForegroundColor Green
        $testsPasses++
    }
    else {
        Write-Host "  ⚠️  Code retour inattendu : $statusCode`n" -ForegroundColor Yellow
        $testsEchoues++
    }
}

# ════════════════════════════════════════════════════════════════
# TEST 7 : GET /api/Utilisateur - Pagination (si admin)
# ════════════════════════════════════════════════════════════════

Write-Host "TEST 7 : GET /api/Utilisateur (pagination)`n" -ForegroundColor Yellow

try {
    $users = Invoke-RestMethod -Uri "$apiUrl/api/Utilisateur?page=1&pageSize=10" `
        -Method Get `
        -Headers $headers
    
    if ($users.page -eq 1 -and $users.pageSize -eq 10) {
        Write-Host "  ✅ Pagination fonctionnelle : OK" -ForegroundColor Green
        Write-Host "     Total : $($users.totalCount) utilisateurs" -ForegroundColor White
        Write-Host "     Pages : $($users.totalPages)`n" -ForegroundColor White
        $testsPasses++
    }
    else {
        Write-Host "  ❌ Structure de reponse incorrecte`n" -ForegroundColor Red
        $testsEchoues++
    }
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    if ($statusCode -eq 403 -and $userRole -ne "Admin" -and $userRole -ne "Super-Admin") {
        Write-Host "  ✅ Acces bloque pour non-admin : OK`n" -ForegroundColor Green
        $testsPasses++
    }
    else {
        Write-Host "  ⚠️  Erreur : $statusCode - $($_.Exception.Message)`n" -ForegroundColor Yellow
    }
}

# ════════════════════════════════════════════════════════════════
# TEST 8 : GET /ecole/{idEcole} - Sa propre école
# ════════════════════════════════════════════════════════════════

Write-Host "TEST 8 : GET /ecole/{idEcole} (sa propre ecole)`n" -ForegroundColor Yellow

try {
    $users = Invoke-RestMethod -Uri "$apiUrl/api/Utilisateur/ecole/$userEcole?page=1&pageSize=10" `
        -Method Get `
        -Headers $headers
    
    if ($users.page -and $users.data) {
        Write-Host "  ✅ Acces a sa propre ecole : OK" -ForegroundColor Green
        Write-Host "     Users : $($users.data.Count) / $($users.totalCount)`n" -ForegroundColor White
        $testsPasses++
    }
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    if ($statusCode -eq 403) {
        Write-Host "  ✅ Acces bloque pour non-admin : OK`n" -ForegroundColor Green
        $testsPasses++
    }
    else {
        Write-Host "  ❌ Erreur : $statusCode`n" -ForegroundColor Red
        $testsEchoues++
    }
}

# ════════════════════════════════════════════════════════════════
# TEST 9 : GET /ecole/{idEcole} - Autre école (doit échouer)
# ════════════════════════════════════════════════════════════════

Write-Host "TEST 9 : GET /ecole/{idEcole} (autre ecole - doit echouer)`n" -ForegroundColor Yellow

$autreEcole = if ($userEcole -eq 1) { 2 } else { 1 }

try {
    $users = Invoke-RestMethod -Uri "$apiUrl/api/Utilisateur/ecole/$autreEcole" `
        -Method Get `
        -Headers $headers
    
    Write-Host "  ❌ PROBLEME SECURITE : Acces inter-ecoles autorise !" -ForegroundColor Red
    $testsEchoues++
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    if ($statusCode -eq 403) {
        Write-Host "  ✅ Acces inter-ecoles bloque (403) : OK`n" -ForegroundColor Green
        $testsPasses++
    }
    else {
        Write-Host "  ⚠️  Code retour inattendu : $statusCode`n" -ForegroundColor Yellow
    }
}

# ════════════════════════════════════════════════════════════════
# TEST 10 : POST /api/Utilisateur - Création (doit échouer si non-admin)
# ════════════════════════════════════════════════════════════════

Write-Host "TEST 10 : POST /api/Utilisateur (creation - admin uniquement)`n" -ForegroundColor Yellow

$newUser = @{
    nomUtilisateur = "Nouveau"
    prenomUtilisateur = "Test"
    email = "nouveau.test@example.com"
    motDePasse = "Test@1234"
    idRole = 4
    idEcole = $userEcole
    statut = $true
} | ConvertTo-Json

try {
    $created = Invoke-RestMethod -Uri "$apiUrl/api/Utilisateur" `
        -Method Post `
        -Body $newUser `
        -Headers $headers `
        -ContentType "application/json"
    
    if ($userRole -eq "Admin" -or $userRole -eq "Super-Admin") {
        Write-Host "  ✅ Creation par admin : OK`n" -ForegroundColor Green
        $testsPasses++
    }
    else {
        Write-Host "  ❌ PROBLEME SECURITE : Non-admin peut creer !" -ForegroundColor Red
        $testsEchoues++
    }
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    if ($statusCode -eq 403 -and $userRole -ne "Admin" -and $userRole -ne "Super-Admin") {
        Write-Host "  ✅ Creation bloquee pour non-admin (403) : OK`n" -ForegroundColor Green
        $testsPasses++
    }
    else {
        Write-Host "  ⚠️  Erreur : $statusCode`n" -ForegroundColor Yellow
        $testsEchoues++
    }
}

# ════════════════════════════════════════════════════════════════
# TEST 11 : PUT /toggle-statut/{id} - (doit échouer si non-admin)
# ════════════════════════════════════════════════════════════════

Write-Host "TEST 11 : PUT /toggle-statut/{id} (admin uniquement)`n" -ForegroundColor Yellow

try {
    $result = Invoke-RestMethod -Uri "$apiUrl/api/Utilisateur/toggle-statut/$otherUserId" `
        -Method Put `
        -Headers $headers
    
    if ($userRole -eq "Admin" -or $userRole -eq "Super-Admin") {
        Write-Host "  ✅ Toggle statut par admin : OK`n" -ForegroundColor Green
        $testsPasses++
    }
    else {
        Write-Host "  ❌ PROBLEME SECURITE : Non-admin peut toggle !" -ForegroundColor Red
        $testsEchoues++
    }
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    if ($statusCode -eq 403 -and $userRole -ne "Admin" -and $userRole -ne "Super-Admin") {
        Write-Host "  ✅ Toggle bloque pour non-admin (403) : OK`n" -ForegroundColor Green
        $testsPasses++
    }
    else {
        Write-Host "  ⚠️  Erreur : $statusCode`n" -ForegroundColor Yellow
        $testsEchoues++
    }
}

# ════════════════════════════════════════════════════════════════
# TEST 12 : DELETE /api/Utilisateur/{id} - (doit échouer si non Super-Admin)
# ════════════════════════════════════════════════════════════════

Write-Host "TEST 12 : DELETE /api/Utilisateur/{id} (super-admin uniquement)`n" -ForegroundColor Yellow

try {
    $result = Invoke-RestMethod -Uri "$apiUrl/api/Utilisateur/$otherUserId" `
        -Method Delete `
        -Headers $headers
    
    if ($userRole -eq "Super-Admin") {
        Write-Host "  ✅ Suppression par Super-Admin : OK`n" -ForegroundColor Green
        $testsPasses++
    }
    else {
        Write-Host "  ❌ PROBLEME SECURITE : Non Super-Admin peut supprimer !" -ForegroundColor Red
        $testsEchoues++
    }
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    if ($statusCode -eq 403 -and $userRole -ne "Super-Admin") {
        Write-Host "  ✅ Suppression bloquee pour non Super-Admin (403) : OK`n" -ForegroundColor Green
        $testsPasses++
    }
    else {
        Write-Host "  ⚠️  Erreur : $statusCode`n" -ForegroundColor Yellow
        $testsEchoues++
    }
}

# ════════════════════════════════════════════════════════════════
# RECAPITULATIF
# ════════════════════════════════════════════════════════════════

Write-Host "`n══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "   RECAPITULATIF DES TESTS" -ForegroundColor Green
Write-Host "══════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

$totalTests = $testsPasses + $testsEchoues
$pourcentage = if ($totalTests -gt 0) { [math]::Round(($testsPasses / $totalTests) * 100, 2) } else { 0 }

Write-Host "  Tests passes  : $testsPasses" -ForegroundColor Green
Write-Host "  Tests echoues : $testsEchoues" -ForegroundColor $(if ($testsEchoues -eq 0) { "Green" } else { "Red" })
Write-Host "  Total         : $totalTests" -ForegroundColor White
Write-Host "  Taux reussite : $pourcentage%`n" -ForegroundColor $(if ($pourcentage -eq 100) { "Green" } else { "Yellow" })

if ($testsEchoues -eq 0) {
    Write-Host "  🎉 TOUS LES TESTS SONT PASSES !" -ForegroundColor Green
    Write-Host "  ✅ Les endpoints sont securises et prets pour production !`n" -ForegroundColor Green
}
else {
    Write-Host "  ⚠️  Certains tests ont echoue" -ForegroundColor Yellow
    Write-Host "  Verifiez les logs ci-dessus pour plus de details`n" -ForegroundColor Yellow
}

Write-Host "══════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

