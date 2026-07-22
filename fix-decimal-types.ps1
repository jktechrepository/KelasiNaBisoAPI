# =============================================
# Script PowerShell pour corriger les types decimal
# =============================================

Write-Host "Début de la correction des types decimal..." -ForegroundColor Green

try {
    # Lire le fichier de configuration
    $config = Get-Content "appsettings.json" | ConvertFrom-Json
    $connectionString = $config.ConnectionStrings.DefaultConnection
    
    # Extraire les informations de connexion
    if ($connectionString -match "Server=([^;]+);Database=([^;]+);") {
        $server = $matches[1]
        $database = $matches[2]
        
        Write-Host "Serveur: $server" -ForegroundColor Yellow
        Write-Host "Base de données: $database" -ForegroundColor Yellow
    } else {
        throw "Impossible d'extraire les informations de connexion"
    }
    
    # Exécuter le script SQL
    Write-Host "Exécution du script de correction des types decimal..." -ForegroundColor Yellow
    
    $sqlScript = Get-Content "fix-decimal-types.sql" -Raw
    
    # Utiliser sqlcmd pour exécuter le script
    $result = sqlcmd -S $server -d $database -E -Q $sqlScript
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Correction des types decimal terminée avec succès!" -ForegroundColor Green
        Write-Host "Les colonnes suivantes ont été converties:" -ForegroundColor Cyan
        Write-Host "  - Paiements.Montant (double → decimal)" -ForegroundColor White
        Write-Host "  - Frais.Montant (double → decimal)" -ForegroundColor White
        Write-Host "  - Vue VuePaiementsFraisParEcole recréée" -ForegroundColor White
    } else {
        Write-Host "❌ Erreur lors de l'exécution du script SQL" -ForegroundColor Red
        Write-Host "Code de sortie: $LASTEXITCODE" -ForegroundColor Red
        Write-Host "Sortie: $result" -ForegroundColor Red
    }
    
} catch {
    Write-Host "❌ Erreur: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "`nScript terminé." -ForegroundColor Green
