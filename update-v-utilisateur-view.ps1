# Script pour mettre à jour la vue V_Utilisateur avec le champ Genre
Write-Host "Mise à jour de la vue V_Utilisateur avec le champ Genre..." -ForegroundColor Green

# Lire le contenu du script SQL
$sqlScript = Get-Content -Path "update-v-utilisateur-view.sql" -Raw

# Exécuter le script SQL via Entity Framework
dotnet ef database update

Write-Host "Vue V_Utilisateur mise à jour avec succès!" -ForegroundColor Green
Write-Host "Le champ Genre est maintenant disponible dans la vue V_Utilisateur" -ForegroundColor Yellow
