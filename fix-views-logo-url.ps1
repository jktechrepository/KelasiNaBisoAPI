# =============================================
# Script PowerShell pour corriger les vues LogoUrl
# =============================================

Write-Host "Début de la correction des vues LogoUrl..." -ForegroundColor Green

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
    Write-Host "Exécution du script de correction des vues..." -ForegroundColor Yellow
    
    $sqlScript = Get-Content "fix-views-logo-url.sql" -Raw
    
    # Utiliser sqlcmd pour exécuter le script
    $result = sqlcmd -S $server -d $database -E -Q $sqlScript
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Correction des vues terminée avec succès!" -ForegroundColor Green
        Write-Host "Les vues suivantes ont été corrigées:" -ForegroundColor Cyan
        Write-Host "  - V_Eleve (LogoUrlEcole)" -ForegroundColor White
        Write-Host "  - EleveParEcole (LogoUrl)" -ForegroundColor White
        Write-Host "  - VuePaiementsFraisParEcole (LogoUrl)" -ForegroundColor White
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
