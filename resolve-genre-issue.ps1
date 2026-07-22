# Script pour résoudre le problème du champ Genre
Write-Host "=== RÉSOLUTION DU PROBLÈME DU CHAMP GENRE ===" -ForegroundColor Cyan
Write-Host ""

Write-Host "Le problème est que la vue V_Utilisateur essaie d'accéder au champ Genre" -ForegroundColor Yellow
Write-Host "qui n'existe pas encore dans la table Utilisateurs de la base de données." -ForegroundColor Yellow
Write-Host ""

Write-Host "ÉTAPES À SUIVRE :" -ForegroundColor Green
Write-Host ""

Write-Host "1. Ouvrez SQL Server Management Studio (SSMS)" -ForegroundColor White
Write-Host "2. Connectez-vous à votre base de données" -ForegroundColor White
Write-Host "3. Ouvrez le fichier 'resolve-genre-issue.sql'" -ForegroundColor White
Write-Host "4. Exécutez le script SQL" -ForegroundColor White
Write-Host ""

Write-Host "Le script va :" -ForegroundColor Yellow
Write-Host "   - Supprimer la vue V_Utilisateur existante" -ForegroundColor White
Write-Host "   - Ajouter la colonne Genre à la table Utilisateurs" -ForegroundColor White
Write-Host "   - Recréer la vue V_Utilisateur avec le champ Genre" -ForegroundColor White
Write-Host ""

Write-Host "5. Après avoir exécuté le script SQL, testez l'API avec :" -ForegroundColor Green
Write-Host "   - Le fichier 'test-genre-utilisateur.http'" -ForegroundColor White
Write-Host ""

Write-Host "6. Si tout fonctionne, vous pouvez supprimer les migrations inutiles :" -ForegroundColor Yellow
Write-Host "   - 20250822150619_AddGenreToUtilisateur.cs" -ForegroundColor White
Write-Host "   - 20250822155510_AddGenreColumnToUtilisateurs.cs" -ForegroundColor White
Write-Host ""

Write-Host "Appuyez sur une touche pour continuer..." -ForegroundColor Cyan
Read-Host
