# Script pour créer la migration Entity Framework
Write-Host "Création de la migration pour ajouter le champ Genre à l'utilisateur..." -ForegroundColor Green

# Créer la migration
dotnet ef migrations add AddGenreToUtilisateur

Write-Host "Migration créée avec succès!" -ForegroundColor Green
Write-Host "Pour appliquer la migration à la base de données, exécutez: dotnet ef database update" -ForegroundColor Yellow
