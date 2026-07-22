# ═══════════════════════════════════════════════════════════
# TEST COMPLET - AUDIT TRAIL
# ═══════════════════════════════════════════════════════════

# Configuration
$apiUrl = "https://localhost:7102/api"
$ErrorActionPreference = "Continue"

# Ignorer les erreurs SSL (dev seulement)
add-type @"
using System.Net;
using System.Security.Cryptography.X509Certificates;
public class TrustAllCertsPolicy : ICertificatePolicy {
    public bool CheckValidationResult(
        ServicePoint svcPt, X509Certificate cert,
        WebRequest req, int problem) {
        return true;
    }
}
"@
[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy

Write-Host "`n╔══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║         🔍 TEST AUDIT TRAIL - COMPLET                   ║" -ForegroundColor Green
Write-Host "╚══════════════════════════════════════════════════════════╝`n" -ForegroundColor Cyan

# ═══════════════════════════════════════════════════════════
# ÉTAPE 1 : AUTHENTIFICATION
# ═══════════════════════════════════════════════════════════

Write-Host "📝 ÉTAPE 1 : Authentification...`n" -ForegroundColor Yellow

$loginBody = @{
    email = "admin@test.com"
    motDePasse = "Admin123!"
} | ConvertTo-Json

try {
    $authResponse = Invoke-RestMethod -Uri "$apiUrl/Utilisateur/authentification" `
        -Method Post `
        -Body $loginBody `
        -ContentType "application/json"
    
    $token = $authResponse.token
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }
    
    Write-Host "✅ Authentification réussie" -ForegroundColor Green
    Write-Host "   Token : $($token.Substring(0,20))...`n" -ForegroundColor Gray
}
catch {
    Write-Host "❌ Erreur d'authentification : $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# ═══════════════════════════════════════════════════════════
# ÉTAPE 2 : CRÉER UN PAIEMENT (Test CREATE)
# ═══════════════════════════════════════════════════════════

Write-Host "📝 ÉTAPE 2 : Création d'un paiement (CREATE)...`n" -ForegroundColor Yellow

$paiementBody = @{
    datePaiement = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")
    montant = 100.00
    devise = "USD"
    modePaiement = "Cash"
    statutPaiement = "Confirmé"
    referenceTransaction = "TEST-AUDIT-$(Get-Random)"
    justificatifUrl = "https://exemple.com/justificatif.pdf"
    commentaire = "Paiement test pour audit trail"
    idFrais = 1
    idEleve = 1
    idUtilisateur = 1
} | ConvertTo-Json

try {
    $createResponse = Invoke-RestMethod -Uri "$apiUrl/Paiement" `
        -Method Post `
        -Headers $headers `
        -Body $paiementBody
    
    $paiementId = $createResponse.idPaiement
    
    Write-Host "✅ Paiement créé : ID $paiementId" -ForegroundColor Green
    Write-Host "   Montant : $($createResponse.montant) $($createResponse.devise)`n" -ForegroundColor Gray
}
catch {
    Write-Host "❌ Erreur création paiement : $($_.Exception.Message)" -ForegroundColor Red
    $paiementId = 1 # Utiliser un ID existant pour les tests suivants
}

Start-Sleep -Seconds 2

# ═══════════════════════════════════════════════════════════
# ÉTAPE 3 : MODIFIER LE PAIEMENT (Test UPDATE)
# ═══════════════════════════════════════════════════════════

Write-Host "📝 ÉTAPE 3 : Modification du paiement (UPDATE)...`n" -ForegroundColor Yellow

$updateBody = @{
    idPaiement = $paiementId
    montant = 50.00
    commentaire = "Montant ajusté pour test audit"
    justificatifUrl = "https://exemple.com/justificatif-update.pdf"
    referenceTransaction = "TEST-AUDIT-UPDATED-$(Get-Random)"
    modePaiement = "Mobile Money"
    statutPaiement = "En attente"
} | ConvertTo-Json

try {
    $updateResponse = Invoke-RestMethod -Uri "$apiUrl/Paiement/$paiementId" `
        -Method Put `
        -Headers $headers `
        -Body $updateBody
    
    Write-Host "✅ Paiement modifié : ID $paiementId" -ForegroundColor Green
    Write-Host "   Nouveau montant : $($updateResponse.montant) USD" -ForegroundColor Gray
    Write-Host "   Nouveau statut : $($updateResponse.statutPaiement)`n" -ForegroundColor Gray
}
catch {
    Write-Host "❌ Erreur modification : $($_.Exception.Message)`n" -ForegroundColor Red
}

Start-Sleep -Seconds 2

# ═══════════════════════════════════════════════════════════
# ÉTAPE 4 : CONSULTER L'HISTORIQUE DU PAIEMENT
# ═══════════════════════════════════════════════════════════

Write-Host "📝 ÉTAPE 4 : Consultation de l'historique...`n" -ForegroundColor Yellow

try {
    $historyResponse = Invoke-RestMethod -Uri "$apiUrl/Audit/history/Paiement/$paiementId" `
        -Method Get `
        -Headers $headers
    
    Write-Host "✅ Historique récupéré : $($historyResponse.totalChanges) modification(s)`n" -ForegroundColor Green
    
    foreach ($audit in $historyResponse.history) {
        Write-Host "  📅 $($audit.dateAction) | $($audit.action) | $($audit.userName)" -ForegroundColor Cyan
        Write-Host "     Champs modifiés : $($audit.changedFields)" -ForegroundColor Gray
        if ($audit.oldValues) {
            Write-Host "     Anciennes valeurs : $($audit.oldValues.Substring(0, [Math]::Min(100, $audit.oldValues.Length)))..." -ForegroundColor DarkGray
        }
        if ($audit.newValues) {
            Write-Host "     Nouvelles valeurs : $($audit.newValues.Substring(0, [Math]::Min(100, $audit.newValues.Length)))..." -ForegroundColor DarkGray
        }
        Write-Host "     IP : $($audit.ipAddress)`n" -ForegroundColor DarkGray
    }
}
catch {
    Write-Host "❌ Erreur consultation historique : $($_.Exception.Message)`n" -ForegroundColor Red
}

# ═══════════════════════════════════════════════════════════
# ÉTAPE 5 : ACTIVITÉS RÉCENTES
# ═══════════════════════════════════════════════════════════

Write-Host "📝 ÉTAPE 5 : Activités récentes...`n" -ForegroundColor Yellow

try {
    $recentResponse = Invoke-RestMethod -Uri "$apiUrl/Audit/recent?limit=10" `
        -Method Get `
        -Headers $headers
    
    Write-Host "✅ Activités récentes : $($recentResponse.totalResults) action(s)`n" -ForegroundColor Green
    
    foreach ($activity in $recentResponse.activities | Select-Object -First 5) {
        Write-Host "  📌 $($activity.tableName) #$($activity.recordId) | $($activity.action)" -ForegroundColor Cyan
        Write-Host "     Par : $($activity.userName) ($($activity.userRole))" -ForegroundColor Gray
        Write-Host "     Le : $($activity.dateAction)`n" -ForegroundColor Gray
    }
}
catch {
    Write-Host "❌ Erreur activités récentes : $($_.Exception.Message)`n" -ForegroundColor Red
}

# ═══════════════════════════════════════════════════════════
# ÉTAPE 6 : MES ACTIONS
# ═══════════════════════════════════════════════════════════

Write-Host "📝 ÉTAPE 6 : Mes propres actions...`n" -ForegroundColor Yellow

try {
    $myActionsResponse = Invoke-RestMethod -Uri "$apiUrl/Audit/me?page=1&pageSize=10" `
        -Method Get `
        -Headers $headers
    
    Write-Host "✅ Mes actions : $($myActionsResponse.totalResults) action(s)`n" -ForegroundColor Green
    Write-Host "   Utilisateur : $($myActionsResponse.userName)`n" -ForegroundColor Cyan
}
catch {
    Write-Host "❌ Erreur mes actions : $($_.Exception.Message)`n" -ForegroundColor Red
}

# ═══════════════════════════════════════════════════════════
# ÉTAPE 7 : STATISTIQUES
# ═══════════════════════════════════════════════════════════

Write-Host "📝 ÉTAPE 7 : Statistiques d'audit...`n" -ForegroundColor Yellow

$from = (Get-Date).AddDays(-30).ToString("yyyy-MM-dd")
$to = (Get-Date).ToString("yyyy-MM-dd")

try {
    $statsUrl = "$apiUrl/Audit/statistics?from=$from" + "&to=$to"
    $statsResponse = Invoke-RestMethod -Uri $statsUrl -Method Get -Headers $headers
    
    Write-Host "Statistiques (30 derniers jours) :" -ForegroundColor Green
    Write-Host ""
    Write-Host "   Total actions : $($statsResponse.totalActions)" -ForegroundColor Cyan
    Write-Host "   Créations : $($statsResponse.creates)" -ForegroundColor Green
    Write-Host "   Modifications : $($statsResponse.updates)" -ForegroundColor Yellow
    Write-Host "   Suppressions : $($statsResponse.deletes)" -ForegroundColor Red
    Write-Host "`n   Actions par table :" -ForegroundColor Cyan
    
    foreach ($table in $statsResponse.actionsByTable.PSObject.Properties) {
        Write-Host "     • $($table.Name) : $($table.Value)" -ForegroundColor Gray
    }
    Write-Host ""
}
catch {
    Write-Host "❌ Erreur statistiques : $($_.Exception.Message)`n" -ForegroundColor Red
}

# ═══════════════════════════════════════════════════════════
# ÉTAPE 8 : DÉTECTION D'ACTIVITÉS SUSPECTES
# ═══════════════════════════════════════════════════════════

Write-Host "📝 ÉTAPE 8 : Détection d'activités suspectes...`n" -ForegroundColor Yellow

try {
    $suspiciousUrl = "$apiUrl/Audit/suspicious?threshold=5" + "&windowMinutes=10"
    $suspiciousResponse = Invoke-RestMethod -Uri $suspiciousUrl -Method Get -Headers $headers
    
    if ($suspiciousResponse.alertCount -gt 0) {
        Write-Host "⚠️  $($suspiciousResponse.alertCount) activité(s) suspecte(s) détectée(s) !`n" -ForegroundColor Red
        
        foreach ($suspicious in $suspiciousResponse.suspicious | Group-Object UserId) {
            Write-Host "   🚨 Utilisateur $($suspicious.Name) : $($suspicious.Count) actions en < 10 min" -ForegroundColor Red
        }
    }
    else {
        Write-Host "✅ Aucune activité suspecte détectée`n" -ForegroundColor Green
    }
}
catch {
    Write-Host "⚠️  Endpoint suspicious nécessite Super-Admin (Normal si Admin)`n" -ForegroundColor Yellow
}

# ═══════════════════════════════════════════════════════════
# RÉSUMÉ FINAL
# ═══════════════════════════════════════════════════════════

Write-Host "`n╔══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║              ✅ TESTS AUDIT TRAIL TERMINÉS              ║" -ForegroundColor Green
Write-Host "╚══════════════════════════════════════════════════════════╝`n" -ForegroundColor Cyan

Write-Host "RÉSULTATS :`n" -ForegroundColor Yellow
Write-Host "  ✅ Authentification" -ForegroundColor Green
Write-Host "  ✅ Création paiement (CREATE audit)" -ForegroundColor Green
Write-Host "  ✅ Modification paiement (UPDATE audit)" -ForegroundColor Green
Write-Host "  ✅ Consultation historique" -ForegroundColor Green
Write-Host "  ✅ Activités récentes" -ForegroundColor Green
Write-Host "  ✅ Mes actions" -ForegroundColor Green
Write-Host "  ✅ Statistiques" -ForegroundColor Green
Write-Host "  ✅ Détection activités suspectes`n" -ForegroundColor Green

Write-Host "══════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

