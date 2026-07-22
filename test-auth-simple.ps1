# Test authentification simple
$apiUrl = "https://localhost:7105"

Write-Host "Test authentification..." -ForegroundColor Yellow

$body = @{
    emailOuTelephone = "kangudjaobed66@gmail.com"
    motDePasse = "123456"
    fcmToken = "dulYj4WMSOmtoMcX-QPmdO:APA91bHMPm-ssK_SyDjUuLbbtVqtTO1Bn1OyOcEqxy7CO0YgcAuzZ4p39EHTmjgjU7mQsvGSEqj8uDb6sKiSsJ5C42t_WT-vqarjcyfWQ0cPr91nH9SF_9o"
    deviceType = "Android"
    deviceModel = "alps V510B"
    osVersion = "Android 12"
} | ConvertTo-Json

Write-Host "Body JSON:" -ForegroundColor Cyan
Write-Host $body -ForegroundColor Gray

try {
    # Ignorer les erreurs SSL
    add-type @"
using System.Net;
using System.Security.Cryptography.X509Certificates;
public class TrustAllCertsPolicy : ICertificatePolicy {
    public bool CheckValidationResult(
        ServicePoint srvPoint, X509Certificate certificate,
        WebRequest request, int certificateProblem) {
        return true;
    }
}
"@
    [System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy
    
    Write-Host "`nEnvoi de la requete a $apiUrl/api/Utilisateur/authentifier..." -ForegroundColor Yellow
    
    $response = Invoke-RestMethod -Uri "$apiUrl/api/Utilisateur/authentifier" `
        -Method Post `
        -Body $body `
        -ContentType "application/json"
    
    Write-Host "`nSucces!" -ForegroundColor Green
    Write-Host "User ID: $($response.utilisateur.idUtilisateur)" -ForegroundColor White
    Write-Host "Nom: $($response.utilisateur.nomUtilisateur)" -ForegroundColor White
    Write-Host "Token: $($response.accessToken.Substring(0, 50))..." -ForegroundColor White
    
    Write-Host "`nReponse complete:" -ForegroundColor Cyan
    $response | ConvertTo-Json -Depth 3
}
catch {
    Write-Host "`nERREUR:" -ForegroundColor Red
    Write-Host "Message: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Details: $($_.ErrorDetails.Message)" -ForegroundColor Yellow
    
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $reader.BaseStream.Position = 0
        $reader.DiscardBufferedData()
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response Body: $responseBody" -ForegroundColor Yellow
    }
}

