# Script pour lancer l'API KelasiNaBiso
Write-Host "🚀 Démarrage de l'API KelasiNaBiso..." -ForegroundColor Green

# Vérifier le certificat HTTPS
Write-Host "🔐 Vérification du certificat HTTPS..." -ForegroundColor Yellow
dotnet dev-certs https --check

# Lancer l'API
Write-Host "🌐 Lancement de l'API..." -ForegroundColor Cyan
Write-Host "📖 Swagger sera disponible sur: https://192.168.43.139:7102/swagger" -ForegroundColor Magenta
Write-Host "🔗 API HTTP: http://192.168.43.139:5002" -ForegroundColor Magenta
Write-Host "🔒 API HTTPS: https://192.168.43.139:7102" -ForegroundColor Magenta
Write-Host ""

dotnet run
