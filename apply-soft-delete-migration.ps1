# Script pour créer et appliquer la migration Soft Delete pour Presence
# Date: 16 Octobre 2025

Write-Host "=======================================" -ForegroundColor Cyan
Write-Host "  MIGRATION SOFT DELETE - PRESENCE" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host ""

# 1. Vérifier qu'on est dans le bon répertoire
$currentPath = Get-Location
Write-Host "📁 Répertoire actuel: $currentPath" -ForegroundColor Yellow
Write-Host ""

# 2. Créer la migration
Write-Host "🔨 Étape 1/3: Création de la migration..." -ForegroundColor Green
Write-Host ""

$migrationName = "AjoutSoftDeletePresence"
dotnet ef migrations add $migrationName

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Erreur lors de la création de la migration!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "✅ Migration créée avec succès: $migrationName" -ForegroundColor Green
Write-Host ""

# 3. Afficher un résumé de ce qui va être appliqué
Write-Host "📋 Modifications qui seront appliquées:" -ForegroundColor Cyan
Write-Host "   - Ajout colonne 'Statut' (BIT, NOT NULL, DEFAULT 1)" -ForegroundColor White
Write-Host "   - Renommage 'Statut' → 'StatutPresence' (si nécessaire)" -ForegroundColor White
Write-Host ""

# 4. Demander confirmation
Write-Host "⚠️  ATTENTION: Cette opération va modifier la base de données!" -ForegroundColor Yellow
$confirmation = Read-Host "Voulez-vous appliquer la migration? (O/N)"

if ($confirmation -ne "O" -and $confirmation -ne "o") {
    Write-Host "❌ Migration annulée par l'utilisateur" -ForegroundColor Red
    exit 0
}

Write-Host ""
Write-Host "🔨 Étape 2/3: Application de la migration..." -ForegroundColor Green
Write-Host ""

# 5. Appliquer la migration
dotnet ef database update

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Erreur lors de l'application de la migration!" -ForegroundColor Red
    Write-Host ""
    Write-Host "💡 Solutions possibles:" -ForegroundColor Yellow
    Write-Host "   1. Vérifier que la base de données est accessible" -ForegroundColor White
    Write-Host "   2. Vérifier la chaîne de connexion dans appsettings.json" -ForegroundColor White
    Write-Host "   3. Vérifier que SQL Server est démarré" -ForegroundColor White
    exit 1
}

Write-Host ""
Write-Host "✅ Migration appliquée avec succès!" -ForegroundColor Green
Write-Host ""

# 6. Vérifier la migration
Write-Host "🔨 Étape 3/3: Vérification..." -ForegroundColor Green
Write-Host ""

# Lister les migrations
Write-Host "📋 Migrations appliquées:" -ForegroundColor Cyan
dotnet ef migrations list

Write-Host ""
Write-Host "=======================================" -ForegroundColor Green
Write-Host "  ✅ MIGRATION TERMINÉE AVEC SUCCÈS" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Green
Write-Host ""

Write-Host "🎯 Prochaines étapes:" -ForegroundColor Cyan
Write-Host "   1. Tester avec: test-soft-delete-presence.http" -ForegroundColor White
Write-Host "   2. Lancer l'API: dotnet run" -ForegroundColor White
Write-Host "   3. Vérifier dans Swagger: http://localhost:5002/swagger" -ForegroundColor White
Write-Host ""

Write-Host "💡 Endpoints ajoutés:" -ForegroundColor Cyan
Write-Host "   PUT /api/Presence/toggle-statut/{id}" -ForegroundColor White
Write-Host ""

Write-Host "📊 Comportement:" -ForegroundColor Cyan
Write-Host "   - Toutes les présences existantes: Statut = true (actif)" -ForegroundColor White
Write-Host "   - Toutes les requêtes GET: Filtrent sur Statut = true" -ForegroundColor White
Write-Host "   - Toggle statut: Désactive/Réactive sans supprimer" -ForegroundColor White
Write-Host ""

