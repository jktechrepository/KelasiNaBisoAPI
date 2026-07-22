$apiUrl = "https://localhost:7102"
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

Write-Host "=== TEST SMS PRESENCE ===" -ForegroundColor Cyan

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

# Headers
$headers = @{}
$headers["Authorization"] = "Bearer $token"
$headers["Content-Type"] = "application/json"

# 2. Recuperer eleves
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

# 3. Recuperer les vacations pour avoir IdVacation
Write-Host "`n3. Recherche d'une vacation..." -ForegroundColor Yellow
try {
    $vacations = Invoke-RestMethod -Uri "$apiUrl/api/Vacation" -Method GET -Headers $headers
    if ($vacations.Count -eq 0) {
        Write-Host "AUCUNE VACATION TROUVEE" -ForegroundColor Red
        exit 1
    }
    $vacation = $vacations[0]
    Write-Host "OK - Vacation: $($vacation.nomVacation) (ID: $($vacation.idVacation))" -ForegroundColor Green
} catch {
    Write-Host "ERREUR: $_" -ForegroundColor Red
    exit 1
}

# 4. Creer presence
Write-Host "`n4. Creation pointage de presence..." -ForegroundColor Yellow
$heure = Get-Date -Format "HH:mm:ss"
# Use tomorrow's date to avoid duplicate error
$date = (Get-Date).AddDays(1).ToString("yyyy-MM-dd")
$presenceBody = @{
    idEleve = $eleve.idEleve
    idVacation = $vacation.idVacation
    isPresent = $true
    heureArrivee = $heure
    dateDuJour = $date
    observation = "Pointage Présence"
    longitute = "15.0000"
    latitude = "0.0000"
} | ConvertTo-Json -Depth 10

Write-Host "Payload: $presenceBody" -ForegroundColor Gray

try {
    $presence = Invoke-RestMethod -Uri "$apiUrl/api/Presence" -Method POST -Body $presenceBody -Headers $headers
    Write-Host "OK - Presence creee ID: $($presence.idPresence)" -ForegroundColor Green
    Write-Host "`nAttente 3 secondes pour SMS..." -ForegroundColor Yellow
    Start-Sleep -Seconds 3
    Write-Host "`nVERIFIEZ LES LOGS ET LE TELEPHONE POUR VOIR L'ENVOI SMS !" -ForegroundColor Cyan
} catch {
    Write-Host "ERREUR creation presence: $_" -ForegroundColor Red
    Write-Host "Details: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "`n=== TEST TERMINE ===" -ForegroundColor Green

