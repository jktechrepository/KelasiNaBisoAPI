# Script pour nettoyer les migrations inutiles après résolution du problème Genre
Write-Host "=== NETTOYAGE DES MIGRATIONS INUTILES ===" -ForegroundColor Cyan
Write-Host ""

Write-Host "Ce script supprime les migrations inutiles créées pour le champ Genre" -ForegroundColor Yellow
Write-Host ""

Write-Host "Migrations à supprimer :" -ForegroundColor Green
Write-Host "   - 20250822150619_AddGenreToUtilisateur" -ForegroundColor White
Write-Host "   - 20250822155510_AddGenreColumnToUtilisateurs" -ForegroundColor White
Write-Host ""

$confirm = Read-Host "Voulez-vous continuer ? (O/N)"

if ($confirm -eq "O" -or $confirm -eq "o" -or $confirm -eq "Y" -or $confirm -eq "y") {
    Write-Host ""
    Write-Host "Suppression des migrations..." -ForegroundColor Yellow
    
    # Supprimer la première migration
    dotnet ef migrations remove
    Write-Host "Migration 20250822155510_AddGenreColumnToUtilisateurs supprimée" -ForegroundColor Green
    
    # Supprimer la deuxième migration
    dotnet ef migrations remove
    Write-Host "Migration 20250822150619_AddGenreToUtilisateur supprimée" -ForegroundColor Green
    
    Write-Host ""
    Write-Host "Nettoyage terminé avec succès !" -ForegroundColor Green
    Write-Host "Les migrations inutiles ont été supprimées." -ForegroundColor Yellow
} else {
    Write-Host ""
    Write-Host "Nettoyage annulé." -ForegroundColor Yellow
}
