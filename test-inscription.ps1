$apiUrl = "https://localhost:7102"
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

Write-Host "=== TEST SMS INSCRIPTION ===" -ForegroundColor Cyan

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

# 2. Recuperer nom ecole et autres infos necessaires
Write-Host "`n2. Recherche des informations necessaires..." -ForegroundColor Yellow
try {
    $ecoles = Invoke-RestMethod -Uri "$apiUrl/api/Ecole" -Method GET -Headers $headers
    $ecole = ($ecoles | Where-Object { $_.idEcole -eq $idEcole })[0]
    if (-not $ecole) {
        Write-Host "ECOLE NON TROUVEE" -ForegroundColor Red
        exit 1
    }
    $nomEcole = $ecole.nom
    Write-Host "OK - Ecole: $nomEcole" -ForegroundColor Green

    # Recuperer une classe
    $classes = Invoke-RestMethod -Uri "$apiUrl/api/Classe/ecole/$idEcole" -Method GET -Headers $headers
    if ($classes.Count -eq 0) {
        Write-Host "AUCUNE CLASSE TROUVEE" -ForegroundColor Red
        exit 1
    }
    $classe = $classes[0]
    Write-Host "OK - Classe: $($classe.nomClasse) (ID: $($classe.idClasse))" -ForegroundColor Green

    # Recuperer une annee scolaire
    $annees = Invoke-RestMethod -Uri "$apiUrl/api/AnneeScolaire" -Method GET -Headers $headers
    if ($annees.Count -eq 0) {
        Write-Host "AUCUNE ANNEE SCOLAIRE TROUVEE" -ForegroundColor Red
        exit 1
    }
    $annee = $annees[0]
    Write-Host "OK - Annee: $($annee.libelleAnneeScolaire) (ID: $($annee.idAnneeScolaire))" -ForegroundColor Green
} catch {
    Write-Host "ERREUR: $_" -ForegroundColor Red
    exit 1
}

# 3. Creer une inscription
Write-Host "`n3. Creation inscription..." -ForegroundColor Yellow
$dateNaissance = (Get-Date).AddYears(-12).ToString("yyyy-MM-dd")
$dateInscription = (Get-Date).ToString("yyyy-MM-dd")

# Generer un nom unique pour l'eleve de test
$timestamp = (Get-Date).ToString("HHmmss")
$inscriptionBody = @{
    type = "Inscription"
    idEcole = $idEcole
    idClasse = $classe.idClasse
    idAnneeScolaire = $annee.idAnneeScolaire
    dateInscription = $dateInscription
    statutInscription = "Confirmé"
    
    # Donnees eleve
    nomEleve = "TEST"
    postnomEleve = "SMS"
    prenomEleve = "Inscription_$timestamp"
    genreEleve = "M"
    dateNaissanceEleve = $dateNaissance
    lieuNaissanceEleve = "Kinshasa"
    nationaliteEleve = "Congolaise"
    provinceEleve = "Kinshasa"
    villeEleve = "Kinshasa"
    communeEleve = "Kalamu"
    quartierEleve = "Matonge"
    
    # Donnees tuteur
    nomCompletTuteur = "Papa Obed"
    genreTuteur = "M"
    emailTuteur = "papa.obed.test@email.com"
    telephoneTuteur = "+243812726582"
} | ConvertTo-Json -Depth 10

Write-Host "Payload: $inscriptionBody" -ForegroundColor Gray

try {
    $inscription = Invoke-RestMethod -Uri "$apiUrl/api/Inscription/new/$nomEcole" -Method POST -Body $inscriptionBody -Headers $headers
    Write-Host "OK - Inscription creee ID: $($inscription.idInscription)" -ForegroundColor Green
    Write-Host "Eleve cree ID: $($inscription.idEleve)" -ForegroundColor Green
    Write-Host "Tuteur cree ID: $($inscription.idTuteur)" -ForegroundColor Green
    
    Write-Host "`nAttente 5 secondes pour SMS..." -ForegroundColor Yellow
    Start-Sleep -Seconds 5
    Write-Host "`nVERIFIEZ LES LOGS ET LE TELEPHONE +243812726582 !" -ForegroundColor Cyan
} catch {
    Write-Host "ERREUR creation inscription: $_" -ForegroundColor Red
    Write-Host "Details: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "`n=== TEST TERMINE ===" -ForegroundColor Green

