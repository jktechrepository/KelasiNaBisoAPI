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
    $idUtilisateur = $response.Utilisateur.IdUtilisateur
    Write-Host "OK - Token recu, IdEcole: $idEcole, IdUser: $idUtilisateur" -ForegroundColor Green
} catch {
    Write-Host "ERREUR connexion: $_" -ForegroundColor Red
    Write-Host "Error details: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $reader.BaseStream.Position = 0
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody" -ForegroundColor Red
    }
    exit 1
}

# 2. Headers
$headers = @{}
$headers["Authorization"] = "Bearer $token"
$headers["Content-Type"] = "application/json"

# 3. Recuperer eleves
Write-Host "`n2. Recherche eleve pour ecole $idEcole..." -ForegroundColor Yellow
try {
    $eleves = Invoke-RestMethod -Uri "$apiUrl/api/Eleve/ecole/$idEcole" -Method GET -Headers $headers
    if ($eleves.Count -eq 0) {
        Write-Host "AUCUN ELEVE TROUVE" -ForegroundColor Red
        exit 1
    }
    $eleve = $eleves[0]
    Write-Host "OK - Eleve: $($eleve.nomComplet) (ID: $($eleve.idEleve))" -ForegroundColor Green
    
    if (-not $eleve.idTuteur) {
        Write-Host "ELEVE SANS TUTEUR" -ForegroundColor Red
        exit 1
    }
    
    $tuteur = Invoke-RestMethod -Uri "$apiUrl/api/Tuteur/$($eleve.idTuteur)" -Method GET -Headers $headers
    if ([string]::IsNullOrWhiteSpace($tuteur.telephone)) {
        Write-Host "TUTEUR SANS TELEPHONE" -ForegroundColor Red
        exit 1
    }
    Write-Host "OK - Tuteur: $($tuteur.nomComplet) - Tel: $($tuteur.telephone)" -ForegroundColor Green
} catch {
    Write-Host "ERREUR: $_" -ForegroundColor Red
    exit 1
}

# 4. Creer paiement
Write-Host "`n3. Creation paiement..." -ForegroundColor Yellow
$ref = "TEST-SMS-$(Get-Date -Format 'yyyyMMddHHmmss')"
$paiementBody = @{
    datePaiement = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")
    montant = 50
    devise = "USD"
    modePaiement = "Cash"
    statutPaiement = "Confirme"
    statut = $true
    referenceTransaction = $ref
    justificatifUrl = ""
    commentaire = "Test SMS"
    idEleve = $eleve.idEleve
    idUtilisateur = $idUtilisateur
} | ConvertTo-Json -Depth 10

try {
    $paiement = Invoke-RestMethod -Uri "$apiUrl/api/Paiement" -Method POST -Body $paiementBody -Headers $headers
    Write-Host "OK - Paiement cree ID: $($paiement.idPaiement)" -ForegroundColor Green
    Write-Host "`nAttente 3 secondes pour SMS..." -ForegroundColor Yellow
    Start-Sleep -Seconds 3
    Write-Host "`nVERIFIEZ LES LOGS DE L'APPLICATION POUR VOIR L'ENVOI SMS !" -ForegroundColor Cyan
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

