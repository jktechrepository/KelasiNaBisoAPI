# Script PowerShell pour corriger la vue VuePointagePresenceParEcole
# Ce script recrée la vue avec les conversions de type correctes

Write-Host "Démarrage de la correction de la vue VuePointagePresenceParEcole..." -ForegroundColor Green

try {
    # Charger l'assembly et la configuration
    Add-Type -Path "bin\Debug\net8.0\GestionTicketAPI.dll"
    
    # Créer une instance du DbContext
    $optionsBuilder = New-Object Microsoft.EntityFrameworkCore.DbContextOptionsBuilder[GestionTicketAPI.Data.KelasiNaBisoDbContext]
    $optionsBuilder.UseSqlServer("Server=localhost;Database=KelasiNaBisoDB;Trusted_Connection=true;TrustServerCertificate=true;")
    
    $context = New-Object GestionTicketAPI.Data.KelasiNaBisoDbContext($optionsBuilder.Options)
    
    Write-Host "Connexion à la base de données établie." -ForegroundColor Yellow
    
    # Recréer la vue avec les corrections
    Write-Host "Recréation de la vue VuePointagePresenceParEcole..." -ForegroundColor Yellow
    $context.CreateViewVuePointagePresenceParEcole()
    
    Write-Host "Vue VuePointagePresenceParEcole recréée avec succès!" -ForegroundColor Green
    
    # Tester la vue
    Write-Host "Test de la vue..." -ForegroundColor Yellow
    $count = $context.VuePointagePresenceParEcole.Count()
    Write-Host "Nombre d'enregistrements dans la vue: $count" -ForegroundColor Green
    
    $context.Dispose()
    Write-Host "Correction terminée avec succès!" -ForegroundColor Green
}
catch {
    Write-Host "Erreur lors de la correction: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Stack trace: $($_.Exception.StackTrace)" -ForegroundColor Red
    exit 1
}
