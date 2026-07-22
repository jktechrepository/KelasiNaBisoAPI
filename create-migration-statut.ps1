# Script PowerShell pour créer la migration "AjoutChampStatutSoftDelete"
# Date : 16 octobre 2025

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  CRÉATION MIGRATION - CHAMP STATUT" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Vérifier si nous sommes dans le bon répertoire
if (-Not (Test-Path "KelasiNaBiso.csproj")) {
    Write-Host "❌ ERREUR : Fichier KelasiNaBiso.csproj non trouvé" -ForegroundColor Red
    Write-Host "Assurez-vous d'être dans le répertoire KelasiNaBisoAPI" -ForegroundColor Yellow
    exit 1
}

Write-Host "📁 Répertoire actuel : $(Get-Location)" -ForegroundColor Green
Write-Host ""

# Nom de la migration
$migrationName = "AjoutChampStatutSoftDelete"

Write-Host "🔨 Création de la migration '$migrationName'..." -ForegroundColor Yellow
Write-Host ""

# Créer la migration
try {
    dotnet ef migrations add $migrationName

    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "  ✅ MIGRATION CRÉÉE AVEC SUCCÈS !" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
        Write-Host ""
        Write-Host "📋 Prochaines étapes :" -ForegroundColor Cyan
        Write-Host "  1. Vérifier les fichiers de migration dans /Migrations/" -ForegroundColor White
        Write-Host "  2. Exécuter : dotnet ef database update" -ForegroundColor White
        Write-Host "  3. Ou utiliser le script : ./apply-migration-statut.ps1" -ForegroundColor White
        Write-Host ""
    } else {
        Write-Host ""
        Write-Host "❌ ERREUR lors de la création de la migration" -ForegroundColor Red
        Write-Host "Vérifiez les erreurs ci-dessus" -ForegroundColor Yellow
        exit 1
    }
} catch {
    Write-Host ""
    Write-Host "❌ EXCEPTION : $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "Appuyez sur une touche pour continuer..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

