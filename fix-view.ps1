# Script PowerShell simple pour corriger la vue
Write-Host "Correction de la vue VuePointagePresenceParEcole..." -ForegroundColor Green

try {
    # Lire le contenu du fichier SQL
    $sqlContent = Get-Content "recreate-view-simple.sql" -Raw
    
    # Exécuter le script via dotnet run avec un endpoint temporaire
    Write-Host "Exécution du script SQL..." -ForegroundColor Yellow
    
    # Créer un script C# temporaire pour exécuter le SQL
    $tempScript = @"
using Microsoft.EntityFrameworkCore;
using GestionTicketAPI.Data;

var optionsBuilder = new DbContextOptionsBuilder<KelasiNaBisoDbContext>();
optionsBuilder.UseSqlServer("Server=localhost;Database=KelasiNaBisoDB;Trusted_Connection=true;TrustServerCertificate=true;");

using var context = new KelasiNaBisoDbContext(optionsBuilder.Options);

// Exécuter le script SQL
var sql = @"$sqlContent";
context.Database.ExecuteSqlRaw(sql);

Console.WriteLine("Vue recréée avec succès!");
"@

    $tempScript | Out-File -FilePath "temp-fix.cs" -Encoding UTF8
    
    # Compiler et exécuter
    dotnet run --project . --no-build -- --fix-view
    
    Write-Host "Correction terminée avec succès!" -ForegroundColor Green
}
catch {
    Write-Host "Erreur: $($_.Exception.Message)" -ForegroundColor Red
}
finally {
    # Nettoyer
    if (Test-Path "temp-fix.cs") {
        Remove-Item "temp-fix.cs"
    }
}
