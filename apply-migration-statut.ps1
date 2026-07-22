# Script PowerShell pour appliquer la migration "AjoutChampStatutSoftDelete"
# Date : 16 octobre 2025

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  APPLICATION MIGRATION - CHAMP STATUT" -ForegroundColor Cyan
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

Write-Host "⚠️  ATTENTION : Cette opération va modifier la base de données !" -ForegroundColor Yellow
Write-Host ""
Write-Host "Voulez-vous continuer ? (O/N)" -ForegroundColor Yellow
$confirmation = Read-Host

if ($confirmation -ne 'O' -and $confirmation -ne 'o') {
    Write-Host "❌ Opération annulée" -ForegroundColor Red
    exit 0
}

Write-Host ""
Write-Host "🔨 Application de la migration à la base de données..." -ForegroundColor Yellow
Write-Host ""

# Appliquer la migration
try {
    dotnet ef database update

    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "  ✅ MIGRATION APPLIQUÉE AVEC SUCCÈS !" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
        Write-Host ""
        Write-Host "📋 Modifications appliquées :" -ForegroundColor Cyan
        Write-Host "  ✅ Colonne 'Statut' ajoutée à 17 tables" -ForegroundColor Green
        Write-Host "  ✅ Valeur par défaut : true (1)" -ForegroundColor Green
        Write-Host "  ✅ Données existantes : Statut = true" -ForegroundColor Green
        Write-Host ""
        Write-Host "📋 Prochaines étapes :" -ForegroundColor Cyan
        Write-Host "  1. Créer les endpoints d'activation/désactivation" -ForegroundColor White
        Write-Host "  2. Mettre à jour les services pour filtrer par Statut" -ForegroundColor White
        Write-Host "  3. Tester les endpoints" -ForegroundColor White
        Write-Host ""
    } else {
        Write-Host ""
        Write-Host "❌ ERREUR lors de l'application de la migration" -ForegroundColor Red
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

