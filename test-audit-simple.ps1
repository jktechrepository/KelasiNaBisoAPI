# TEST SIMPLIFIÉ - AUDIT TRAIL
$apiUrl = "https://localhost:7105/api"

# Ignorer SSL
add-type @"
using System.Net;
using System.Security.Cryptography.X509Certificates;
public class TrustAllCertsPolicy : ICertificatePolicy {
    public bool CheckValidationResult(ServicePoint svcPt, X509Certificate cert, WebRequest req, int problem) {
        return true;
    }
}
"@
[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host " TEST AUDIT TRAIL" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Cyan

# 1. AUTHENTIFICATION
Write-Host "1. Authentification..." -ForegroundColor Yellow

$loginBody = @{
    email = "superadmin@kelasinabiso.cd"
    motDePasse = "Super-Admin"
} | ConvertTo-Json

try {
    $authResponse = Invoke-RestMethod -Uri "$apiUrl/Utilisateur/authentification" -Method Post -Body $loginBody -ContentType "application/json"
    $token = $authResponse.token
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }
    Write-Host "   OK - Token recu`n" -ForegroundColor Green
}
catch {
    Write-Host "   ERREUR: $($_.Exception.Message)`n" -ForegroundColor Red
    exit 1
}

# 2. CRÉER UN PAIEMENT
Write-Host "2. Creation paiement..." -ForegroundColor Yellow

$paiementBody = @{
    datePaiement = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")
    montant = 100.00
    devise = "USD"
    modePaiement = "Cash"
    statutPaiement = "Confirme"
    referenceTransaction = "TEST-$(Get-Random)"
    justificatifUrl = "https://test.com/justif.pdf"
    commentaire = "Test audit"
    idFrais = 1
    idEleve = 1
    idUtilisateur = 1
} | ConvertTo-Json

try {
    $createResponse = Invoke-RestMethod -Uri "$apiUrl/Paiement" -Method Post -Headers $headers -Body $paiementBody
    $paiementId = $createResponse.idPaiement
    Write-Host "   OK - Paiement cree: ID $paiementId`n" -ForegroundColor Green
}
catch {
    Write-Host "   Erreur: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   Utilisation d'un ID existant pour les tests...`n" -ForegroundColor Yellow
    $paiementId = 1
}

Start-Sleep -Seconds 1

# 3. MODIFIER LE PAIEMENT
Write-Host "3. Modification paiement..." -ForegroundColor Yellow

$updateBody = @{
    idPaiement = $paiementId
    montant = 50.00
    commentaire = "Montant ajuste via audit test"
    justificatifUrl = "https://test.com/justif2.pdf"
    referenceTransaction = "TEST-UPDATE-$(Get-Random)"
    modePaiement = "Mobile Money"
    statutPaiement = "En attente"
} | ConvertTo-Json

try {
    $updateResponse = Invoke-RestMethod -Uri "$apiUrl/Paiement/$paiementId" -Method Put -Headers $headers -Body $updateBody
    Write-Host "   OK - Paiement modifie`n" -ForegroundColor Green
}
catch {
    Write-Host "   ERREUR: $($_.Exception.Message)`n" -ForegroundColor Red
}

Start-Sleep -Seconds 1

# 4. CONSULTER L'HISTORIQUE
Write-Host "4. Consultation historique..." -ForegroundColor Yellow

try {
    $historyResponse = Invoke-RestMethod -Uri "$apiUrl/Audit/history/Paiement/$paiementId" -Method Get -Headers $headers
    Write-Host "   OK - $($historyResponse.totalChanges) modification(s) trouvee(s)`n" -ForegroundColor Green
    
    foreach ($audit in $historyResponse.history) {
        Write-Host "   - $($audit.dateAction) | $($audit.action) | $($audit.userName)" -ForegroundColor Cyan
        if ($audit.changedFields) {
            Write-Host "     Champs: $($audit.changedFields)" -ForegroundColor Gray
        }
    }
    Write-Host ""
}
catch {
    Write-Host "   ERREUR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 5. ACTIVITÉS RÉCENTES
Write-Host "5. Activites recentes..." -ForegroundColor Yellow

try {
    $recentResponse = Invoke-RestMethod -Uri "$apiUrl/Audit/recent?limit=5" -Method Get -Headers $headers
    Write-Host "   OK - $($recentResponse.totalResults) activite(s)`n" -ForegroundColor Green
}
catch {
    Write-Host "   ERREUR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 6. MES ACTIONS
Write-Host "6. Mes actions..." -ForegroundColor Yellow

try {
    $myActionsResponse = Invoke-RestMethod -Uri "$apiUrl/Audit/me?page=1" -Method Get -Headers $headers
    Write-Host "   OK - $($myActionsResponse.totalResults) action(s) pour $($myActionsResponse.userName)`n" -ForegroundColor Green
}
catch {
    Write-Host "   ERREUR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 7. STATISTIQUES
Write-Host "7. Statistiques..." -ForegroundColor Yellow

try {
    $statsResponse = Invoke-RestMethod -Uri "$apiUrl/Audit/statistics" -Method Get -Headers $headers
    Write-Host "   OK - Total: $($statsResponse.totalActions) actions" -ForegroundColor Green
    Write-Host "   Creates: $($statsResponse.creates) | Updates: $($statsResponse.updates) | Deletes: $($statsResponse.deletes)`n" -ForegroundColor Gray
}
catch {
    Write-Host "   ERREUR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# RÉSUMÉ
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host " TESTS TERMINES - AUDIT TRAIL OK !" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Cyan

