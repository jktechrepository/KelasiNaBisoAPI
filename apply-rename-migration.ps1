# Script PowerShell pour appliquer le renommage Enseignant -> Agent
# Date: 17 Octobre 2025

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "RENOMMAGE: ENSEIGNANT -> AGENT" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Execution du script SQL..." -ForegroundColor Yellow

# Lire le contenu du script SQL et l'exécuter
$sqlScript = Get-Content ".\rename-enseignant-to-agent.sql" -Raw

# Exécuter le SQL via mysql
$sqlScript | mysql -h localhost -P 3306 -u kansa -pkansa2025 KelasiNaBisoDb

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "SUCCESS - MIGRATION APPLIQUEE !" -ForegroundColor Green
    Write-Host ""
    Write-Host "Modifications effectuees:" -ForegroundColor Cyan
    Write-Host "   - Table 'Enseignants' -> 'Agents'" -ForegroundColor Green
    Write-Host "   - Colonne 'IdEnseignant' -> 'IdAgent'" -ForegroundColor Green
    Write-Host "   - Colonne 'TelephoneEnseignant' -> 'TelephoneAgent'" -ForegroundColor Green
    Write-Host "   - Colonne 'EmailEnseignant' -> 'EmailAgent'" -ForegroundColor Green
    Write-Host "   - Vue 'Vue_RepertoireEnseignantsParParent' -> 'Vue_RepertoireAgentsParParent'" -ForegroundColor Green
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "ERREUR lors de l'application !" -ForegroundColor Red
    Write-Host ""
}

Write-Host "========================================" -ForegroundColor Cyan
