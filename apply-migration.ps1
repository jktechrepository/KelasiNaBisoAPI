# Script pour appliquer la migration Entity Framework
Write-Host "Application de la migration à la base de données..." -ForegroundColor Green

# Appliquer la migration
dotnet ef database update

Write-Host "Migration appliquée avec succès!" -ForegroundColor Green
Write-Host "La contrainte unique sur le nom du rôle est maintenant active." -ForegroundColor Yellow
