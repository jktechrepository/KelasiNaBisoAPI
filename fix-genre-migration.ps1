# Script pour résoudre le problème de migration du champ Genre
Write-Host "Résolution du problème de migration du champ Genre..." -ForegroundColor Green

# Lire le contenu du script SQL
$sqlScript = Get-Content -Path "fix-genre-migration.sql" -Raw

Write-Host "Exécution du script SQL de correction..." -ForegroundColor Yellow

# Exécuter le script SQL via Entity Framework
# Note: Ce script doit être exécuté directement dans SQL Server Management Studio
# ou via une connexion directe à la base de données

Write-Host "IMPORTANT: Veuillez exécuter le script 'fix-genre-migration.sql' directement dans SQL Server Management Studio" -ForegroundColor Red
Write-Host "ou via une connexion directe à votre base de données." -ForegroundColor Red
Write-Host ""
Write-Host "Après avoir exécuté le script SQL, vous pouvez tester l'API avec le fichier 'test-genre-utilisateur.http'" -ForegroundColor Yellow
