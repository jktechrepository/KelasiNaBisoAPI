$apiUrl = "https://localhost:7102"
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

Write-Host "`n=== TEST BATCH ELEVES ET PAIEMENTS ===" -ForegroundColor Cyan

# 1. Connexion
Write-Host "`n1. Connexion..." -ForegroundColor Yellow
$loginBody = '{"emailOuTelephone":"elsynchropos@gmail.com","motDePasse":"Admin"}'
try {
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

# 2. Récupérer une classe et un tuteur
Write-Host "`n2. Récupération des informations nécessaires..." -ForegroundColor Yellow
try {
    $classes = Invoke-RestMethod -Uri "$apiUrl/api/Classe/ecole/$idEcole" -Method GET -Headers $headers -ErrorAction Stop
    if ($classes.Count -eq 0) {
        Write-Host "AUCUNE CLASSE TROUVEE" -ForegroundColor Red
        exit 1
    }
    $classe = $classes[0]
    Write-Host "OK - Classe: $($classe.nomClasse) (ID: $($classe.idClasse))" -ForegroundColor Green
    
    $tuteurs = Invoke-RestMethod -Uri "$apiUrl/api/Tuteur" -Method GET -Headers $headers -ErrorAction Stop
    if ($tuteurs.Count -eq 0) {
        Write-Host "AUCUN TUTEUR TROUVE" -ForegroundColor Red
        exit 1
    }
    $tuteur = $tuteurs[0]
    Write-Host "OK - Tuteur: $($tuteur.nomComplet) (ID: $($tuteur.idTuteur))" -ForegroundColor Green
} catch {
    Write-Host "ERREUR: $_" -ForegroundColor Red
    exit 1
}

# 3. Créer une liste d'élèves
Write-Host "`n3. Création d'une liste d'élèves..." -ForegroundColor Yellow
$timestamp = (Get-Date).ToString("yyyyMMddHHmmss")
$eleves = @(
    @{
        nom = "MAKONGO"
        postnom = "NTOTO"
        prenom = "Luc_$timestamp"
        genre = "M"
        dateNaissance = "2010-05-15"
        lieuNaissance = "Kinshasa"
        nationalite = "Congolaise"
        statut = $true
        idClasse = $classe.idClasse
        idTuteur = $tuteur.idTuteur
    },
    @{
        nom = "MUKAMBA"
        postnom = "KASENGA"
        prenom = "Sophie_$timestamp"
        genre = "F"
        dateNaissance = "2011-08-20"
        lieuNaissance = "Kinshasa"
        nationalite = "Congolaise"
        statut = $true
        idClasse = $classe.idClasse
        idTuteur = $tuteur.idTuteur
    },
    @{
        nom = "KABONGO"
        postnom = "MBALA"
        prenom = "David_$timestamp"
        genre = "M"
        dateNaissance = "2010-03-10"
        lieuNaissance = "Kinshasa"
        nationalite = "Congolaise"
        statut = $true
        idClasse = $classe.idClasse
        idTuteur = $tuteur.idTuteur
    }
)

$elevesBody = $eleves | ConvertTo-Json -Depth 10
Write-Host "OK - Liste de $($eleves.Count) élèves préparée" -ForegroundColor Green

# 4. Envoyer la requête POST batch pour les élèves
Write-Host "`n4. Envoi de la requête POST batch élèves..." -ForegroundColor Yellow
try {
    $elevesResult = Invoke-RestMethod -Uri "$apiUrl/api/Eleve/batch" -Method POST -Body $elevesBody -Headers $headers -ErrorAction Stop
    Write-Host "✅ SUCCESS - Élèves créés !" -ForegroundColor Green
    Write-Host "   Total: $($elevesResult.total), Créés: $($elevesResult.success)" -ForegroundColor Gray
    
    $createdEleves = $elevesResult.eleves
    if ($createdEleves.Count -gt 0) {
        Write-Host "`n   Élèves créés:" -ForegroundColor Yellow
        foreach ($eleve in $createdEleves) {
            Write-Host "   - $($eleve.nom) $($eleve.prenom) (ID: $($eleve.idEleve), Matricule: $($eleve.matricule))" -ForegroundColor Gray
        }
    }
} catch {
    Write-Host "❌ ERREUR: $_" -ForegroundColor Red
    Write-Host "Details: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $reader.BaseStream.Position = 0
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody" -ForegroundColor Red
    }
    exit 1
}

# 5. Créer une liste de paiements pour ces élèves
Write-Host "`n5. Création d'une liste de paiements..." -ForegroundColor Yellow
$referenceBase = "BATCH_$(Get-Date -Format 'yyyyMMddHHmmss')"
$paiements = @(
    @{
        datePaiement = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")
        montant = 75.0
        devise = "USD"
        modePaiement = "Cash"
        statutPaiement = "Confirme"
        statut = $true
        referenceTransaction = "$referenceBase-001"
        justificatifUrl = ""
        commentaire = "Paiement test batch 1"
        idEleve = $createdEleves[0].idEleve
        idUtilisateur = 1
    },
    @{
        datePaiement = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")
        montant = 100.0
        devise = "USD"
        modePaiement = "Mobile Money"
        statutPaiement = "Confirme"
        statut = $true
        referenceTransaction = "$referenceBase-002"
        justificatifUrl = ""
        commentaire = "Paiement test batch 2"
        idEleve = $createdEleves[1].idEleve
        idUtilisateur = 1
    },
    @{
        datePaiement = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")
        montant = 50.0
        devise = "USD"
        modePaiement = "Cash"
        statutPaiement = "Confirme"
        statut = $true
        referenceTransaction = "$referenceBase-003"
        justificatifUrl = ""
        commentaire = "Paiement test batch 3"
        idEleve = $createdEleves[2].idEleve
        idUtilisateur = 1
    }
)

$paiementsBody = $paiements | ConvertTo-Json -Depth 10
Write-Host "OK - Liste de $($paiements.Count) paiements préparée" -ForegroundColor Green

# 6. Envoyer la requête POST batch pour les paiements
Write-Host "`n6. Envoi de la requête POST batch paiements..." -ForegroundColor Yellow
try {
    $paiementsResult = Invoke-RestMethod -Uri "$apiUrl/api/Paiement/batch" -Method POST -Body $paiementsBody -Headers $headers -ErrorAction Stop
    Write-Host "✅ SUCCESS - Paiements créés !" -ForegroundColor Green
    Write-Host "   Total: $($paiementsResult.total), Créés: $($paiementsResult.success)" -ForegroundColor Gray
    
    if ($paiementsResult.paiements.Count -gt 0) {
        Write-Host "`n   Paiements créés:" -ForegroundColor Yellow
        foreach ($paiement in $paiementsResult.paiements) {
            Write-Host "   - $($paiement.referenceTransaction) : $($paiement.montant) $($paiement.devise)" -ForegroundColor Gray
        }
    }
    
    Write-Host "`n   ⏳ Attente 5 secondes pour les notifications SMS..." -ForegroundColor Yellow
    Start-Sleep -Seconds 5
    Write-Host "   VERIFIEZ LES LOGS ET LES TELEPHONES POUR VOIR LES SMS !" -ForegroundColor Cyan
} catch {
    Write-Host "❌ ERREUR: $_" -ForegroundColor Red
    Write-Host "Details: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== TEST TERMINE ===" -ForegroundColor Green

