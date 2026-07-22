$apiUrl = "https://localhost:7102"
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

Write-Host "=== TEST SMS PAIEMENT ===" -ForegroundColor Cyan

# 1. Connexion
Write-Host "`n1. Connexion..." -ForegroundColor Yellow
$loginBody = '{"emailOuTelephone":"elsynchropos@gmail.com","motDePasse":"Admin"}'
try {
    Write-Host "Envoi requete..." -ForegroundColor Gray
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

# 2. Récupérer un élève avec tuteur
Write-Host "`n2. Recherche d'un élève avec tuteur..." -ForegroundColor Yellow
try {
    $eleves = Invoke-RestMethod -Uri "$apiUrl/api/Eleve" -Method GET -Headers $headers -ErrorAction Stop
    if ($eleves.Count -eq 0) {
        Write-Host "AUCUN ELEVE TROUVE" -ForegroundColor Red
        exit 1
    }
    $eleve = $eleves[0]
    Write-Host "OK - Eleve: $($eleve.nomComplet) (ID: $($eleve.idEleve))" -ForegroundColor Green
    
    # Récupérer le tuteur
    $tuteur = Invoke-RestMethod -Uri "$apiUrl/api/Tuteur/$($eleve.idTuteur)" -Method GET -Headers $headers -ErrorAction Stop
    Write-Host "OK - Tuteur: $($tuteur.nomComplet) - Tel: $($tuteur.telephone)" -ForegroundColor Green
} catch {
    Write-Host "ERREUR: $_" -ForegroundColor Red
    exit 1
}

# 3. Créer un paiement
Write-Host "`n3. Création paiement..." -ForegroundColor Yellow
$timestamp = Get-Date -Format "yyyy-MM-ddTHH:mm:ss"
$reference = "TEST-SMS-$(Get-Date -Format 'yyyyMMddHHmmss')"

$paiementBody = @{
    datePaiement = $timestamp
    montant = 50.0
    devise = "USD"
    modePaiement = "Cash"
    statutPaiement = "Confirme"
    statut = $true
    referenceTransaction = $reference
    justificatifUrl = ""
    commentaire = "Test SMS paiement"
    idEleve = $eleve.idEleve
    idUtilisateur = 1
} | ConvertTo-Json -Depth 10

Write-Host "Payload: $paiementBody" -ForegroundColor Gray

try {
    $paiement = Invoke-RestMethod -Uri "$apiUrl/api/Paiement" -Method POST -Body $paiementBody -Headers $headers -ErrorAction Stop
    Write-Host "OK - Paiement créé ID: $($paiement.idPaiement)" -ForegroundColor Green
    Write-Host "Montant: $($paiement.montant) $($paiement.devise)" -ForegroundColor Green
    
    Write-Host "`nAttente 3 secondes pour SMS..." -ForegroundColor Yellow
    Start-Sleep -Seconds 3
    Write-Host "`nVERIFIEZ LES LOGS ET LE TELEPHONE $($tuteur.telephone) !" -ForegroundColor Cyan
} catch {
    Write-Host "ERREUR creation paiement: $_" -ForegroundColor Red
    Write-Host "Details: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $reader.BaseStream.Position = 0
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody" -ForegroundColor Red
    }
    exit 1
}

Write-Host "`n=== TEST TERMINE ===" -ForegroundColor Green
