# ════════════════════════════════════════════════════════════════════════════════
# 📨 TEST AUTOMATISÉ COMMUNICATIONS MULTI-CANAUX
# ════════════════════════════════════════════════════════════════════════════════
# Objectif : Valider le nouveau workflow simplifié :
#   1. Authentification (Super-Admin & Admin)
#   2. Création d'une campagne + segments
#   3. Regénération des destinataires
#   4. Envoi direct par un rôle autorisé (Admin)
#   5. Contrôles post-envoi + annulation
#
# Prérequis :
#   - Script SQL de Phase 1 exécuté (tables Communication*)
#   - API lancée (https://localhost:7102 par défaut)
#   - Comptes avec rôles Super-Admin et Admin disponibles
#   - Données de parents/tuteurs actifs (classe/direction) pour segments
#
# Auteur : KelasiNaBiso Squad Notifications
# Date   : $(Get-Date -Format 'yyyy-MM-dd')
# ════════════════════════════════════════════════════════════════════════════════

param(
    [string]$ApiUrl = "https://localhost:7102",
    [string]$SuperAdminEmail = "superadmin@kelasinabiso.cd",
    [string]$SuperAdminPassword = "Super-Admin",
    [string]$AdminEmail = "admin@kelasinabiso.cd",
    [string]$AdminPassword = "Admin",
    [int]$IdEcole = 18,
    [int]$IdClasseTest = 42,
    [int]$IdDirectionTest = 7,
    [switch]$SkipSslCheck = $true
)

$ErrorActionPreference = "Stop"

# ───── Fonctions utilitaires affichage ──────────────────────────────────────────
function Write-Header([string]$Message) {
    Write-Host "`n════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ("  {0}" -f $Message) -ForegroundColor Yellow
    Write-Host "════════════════════════════════════════════════════════════════════`n" -ForegroundColor Cyan
}
function Write-Step([string]$Message) { Write-Host "➡️  $Message" -ForegroundColor Cyan }
function Write-Ok([string]$Message) { Write-Host "✅ $Message" -ForegroundColor Green }
function Write-Warn([string]$Message) { Write-Host "⚠️  $Message" -ForegroundColor Yellow }
function Write-Ko([string]$Message) { Write-Host "❌ $Message" -ForegroundColor Red }

# ───── SSL dev ──────────────────────────────────────────────────────────────────
if ($SkipSslCheck) {
    if (-not ([System.Management.Automation.PSTypeName]'TrustAllCertsPolicy').Type) {
        Add-Type @"
            using System.Net;
            using System.Security.Cryptography.X509Certificates;
            public class TrustAllCertsPolicy : ICertificatePolicy {
                public bool CheckValidationResult(ServicePoint srvPoint, X509Certificate certificate, WebRequest request, int certificateProblem) {
                    return true;
                }
            }
"@
    }
    [System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy
    [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12
}

# ───── Helper REST ───────────────────────────────────────────────────────────────
function Invoke-Api {
    param(
        [string]$Method,
        [string]$Endpoint,
        [hashtable]$Headers,
        $Body,
        [int]$ExpectedStatus = 200
    )
    $uri = "$ApiUrl$Endpoint"
    $params = @{
        Uri = $uri
        Method = $Method
        Headers = $Headers
        ErrorAction = 'Stop'
    }
    if ($Body -ne $null) {
        $params.ContentType = "application/json"
        $params.Body = ($Body | ConvertTo-Json -Depth 10)
    }

    try {
        return Invoke-RestMethod @params
    } catch {
        $resp = $_.Exception.Response
        if ($resp) {
            $reader = New-Object System.IO.StreamReader($resp.GetResponseStream())
            $reader.BaseStream.Position = 0
            $payload = $reader.ReadToEnd()
            Write-Ko ("API {0} {1} -> {2}`n{3}" -f $Method, $uri, $resp.StatusCode.value__, $payload)
        }
        throw
    }
}

# ───── Authentification helper ──────────────────────────────────────────────────
function Get-JwtToken {
    param(
        [string]$Email,
        [string]$Password
    )
    Write-Step "Authentification pour $Email"
    $response = Invoke-Api -Method Post -Endpoint "/api/Utilisateur/authentifier" -Headers @{} -Body @{
        email = $Email
        motDePasse = $Password
    }
    return @{
        token = $response.token
        headers = @{
            "Authorization" = "Bearer $($response.token)"
            "Content-Type" = "application/json"
        }
        user = $response.utilisateur
    }
}

# ───── Étape 1 : Super-admin crée campagne ───────────────────────────────────────
Write-Header "Étape 1 – Création campagne (Super-Admin)"
$superAdmin = Get-JwtToken -Email $SuperAdminEmail -Password $SuperAdminPassword
$saHeaders = $superAdmin.headers
Write-Ok ("Super-Admin connecté (Utilisateur ID: {0})" -f $superAdmin.user.idUtilisateur)

$campagnePayload = @{
    idEcole = $IdEcole
    titre = "Test auto communication – $(Get-Date -Format 'HH:mm:ss')"
    contenuMarkdown = @"
# Communication automatisée test

- Segment Classe ID: $IdClasseTest
- Segment Direction ID: $IdDirectionTest
- Date : $(Get-Date -Format 'dd/MM/yyyy HH:mm')
"@
    importance = "Info"
    canaux = @{
        push = $true
        email = $false
        sms = $false
        inApp = $true
    }
    rappelAuto = $false
    segments = @(
        @{
            nomSegment = "Classe-$IdClasseTest"
            typeSegment = "Classe"
            isReusable = $false
            classeIds = @($IdClasseTest)
        },
        @{
            nomSegment = "Direction-$IdDirectionTest"
            typeSegment = "Direction"
            isReusable = $false
            directionIds = @($IdDirectionTest)
        }
    )
}

$campagne = Invoke-Api -Method Post -Endpoint "/api/Communication" -Headers $saHeaders -Body $campagnePayload
$campagneId = $campagne.idCampaign
Write-Ok ("Campagne créée ID={0}, statut={1}" -f $campagneId, $campagne.statut)

# Rafraîchir destinataires
Write-Step "Rafraîchissement des destinataires"
$destResp = Invoke-Api -Method Post -Endpoint "/api/Communication/$campagneId/destinataires/recharger" -Headers $saHeaders -Body @{}
$totalDest = $destResp.destinataires
Write-Ok ("Destinataires générés: {0}" -f $totalDest)

if ($totalDest -le 0) {
    Write-Warn "Aucun destinataire détecté. Vérifier les segments ou les données."
}

# ───── Étape 2 : Envoi direct par l'Admin ───────────────────────────────────────
Write-Header "Étape 2 – Envoi direct par l'Admin"
$admin = Get-JwtToken -Email $AdminEmail -Password $AdminPassword
$adminHeaders = $admin.headers
Write-Ok ("Admin connecté (Utilisateur ID: {0})" -f $admin.user.idUtilisateur)

Write-Step "Déclenchement de l'envoi (Admin)"
Invoke-Api -Method Post -Endpoint "/api/Communication/$campagneId/envoyer" -Headers $adminHeaders -Body @{} | Out-Null
Write-Ok "Envoi lancé (Accepted, Admin)"

# Attendre exécution job (Task.Run)
Start-Sleep -Seconds 5

# ───── Étape 3 : Vérifications ──────────────────────────────────────────────────
Write-Header "Étape 3 – Vérifications"

Write-Step "Consultation destinataires (Admin)"
$destPage = Invoke-Api -Method Get -Endpoint "/api/Communication/$campagneId/destinataires?pageNumber=1&pageSize=50" -Headers $adminHeaders -Body $null
$destEnvoyes = @($destPage.data | Where-Object { $_.status -eq "Envoye" }).Count
$destEchecs = @($destPage.data | Where-Object { $_.status -eq "Echec" }).Count
Write-Ok ("Destinataires envoyés={0}, échecs={1}" -f $destEnvoyes, $destEchecs)

Write-Step "Consultation historique"
$history = Invoke-Api -Method Get -Endpoint "/api/Communication/$campagneId/historique?pageNumber=1&pageSize=20" -Headers $adminHeaders -Body $null
$timeline = $history.data | ForEach-Object { "{0:u} - {1}" -f $_.dateAction, $_.action }
$timeline | ForEach-Object { Write-Host "   • $_" -ForegroundColor Gray }

try {
    Write-Step "Consultation notification in-app (si endpoint dispo)"
    $notifications = Invoke-Api -Method Get -Endpoint "/api/Notifications/campagne/$campagneId" -Headers $saHeaders -Body $null -ExpectedStatus 200
    if ($notifications -and $notifications.Count -gt 0) {
        Write-Ok ("{0} notification(s) in-app enregistrée(s)" -f $notifications.Count)
    } else {
        Write-Warn "Aucune notification in-app trouvée pour cette campagne."
    }
} catch {
    Write-Warn "Endpoint /api/Notifications/campagne non disponible (étape ignorée)."
}

# ───── Étape 4 : Nettoyage (optionnel) ───────────────────────────────────────────
Write-Header "Étape 4 – Nettoyage (Annulation)"
Invoke-Api -Method Post -Endpoint "/api/Communication/$campagneId/annuler" -Headers $saHeaders -Body @{ message = "Test automatique terminé – annulation" } | Out-Null
Write-Ok "Campagne annulée (statut marqué)"

# ───── Résumé ────────────────────────────────────────────────────────────────────
Write-Header "Résumé"
Write-Ok ("Campagne ID={0}" -f $campagneId)
Write-Ok ("Destinataires initialisés : {0}" -f $totalDest)
Write-Ok ("Envoyés : {0} | Échecs : {1}" -f $destEnvoyes, $destEchecs)
Write-Host ""
Write-Warn "Actions manuelles recommandées :"
Write-Host "  1. Vérifier la base `Notifications` (table) pour confirmer les entrées créées." -ForegroundColor Yellow
Write-Host "  2. Contrôler les logs API pour les envois push/email/sms." -ForegroundColor Yellow
Write-Host "  3. Inspecter `CampaignRecipients` pour les finalChannel et erreurs éventuelles." -ForegroundColor Yellow
Write-Host ""
Write-Host "Script terminé." -ForegroundColor Cyan

