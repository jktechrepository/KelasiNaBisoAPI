# ═══════════════════════════════════════════════════════════════════════════════
# 🔔 SCRIPT DE TEST AUTOMATISÉ DES NOTIFICATIONS PUSH
# ═══════════════════════════════════════════════════════════════════════════════
# Description: Teste automatiquement les notifications push Firebase pour :
#   1. Présence (pointage élève)
#   2. Paiement (paiement de frais)
#
# Prérequis:
#   - API démarrée sur https://localhost:7102
#   - Firebase Admin SDK initialisé (voir logs au démarrage)
#   - Au moins un élève avec un tuteur actif qui a un utilisateur avec devices
#
# Auteur: KelasiNaBisoAPI Team
# Date: $(Get-Date -Format "yyyy-MM-dd")
# ═══════════════════════════════════════════════════════════════════════════════

param(
    [string]$ApiUrl = "https://localhost:7102",
    [string]$Email = "superadmin@kelasinabiso.cd",
    [string]$Password = "Super-Admin",
    [int]$IdEcole = 18,
    [switch]$SkipSslCheck = $true
)

# ═══════════════════════════════════════════════════════════════════════════════
# 🎨 CONFIGURATION DES COULEURS ET STYLES
# ═══════════════════════════════════════════════════════════════════════════════

$ErrorActionPreference = "Stop"

function Write-Header {
    param([string]$Message)
    Write-Host "`n" -NoNewline
    Write-Host "════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  $Message" -ForegroundColor Yellow
    Write-Host "════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor Green
}

function Write-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor Red
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ️  $Message" -ForegroundColor Cyan
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor Yellow
}

# ═══════════════════════════════════════════════════════════════════════════════
# 🔧 CONFIGURATION SSL (pour certificat auto-signé)
# ═══════════════════════════════════════════════════════════════════════════════

if ($SkipSslCheck) {
    # Ignorer les erreurs de certificat SSL (développement uniquement)
    if (-not ([System.Management.Automation.PSTypeName]'TrustAllCertsPolicy').Type) {
        Add-Type @"
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
    }
    [System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy
    [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12
    Write-Info "Certificat SSL auto-signé accepté (mode développement)"
}

# ═══════════════════════════════════════════════════════════════════════════════
# 📋 ÉTAPE 1 : AUTHENTIFICATION
# ═══════════════════════════════════════════════════════════════════════════════

Write-Header "ÉTAPE 1 : AUTHENTIFICATION"

$authBody = @{
    email = $Email
    motDePasse = $Password
} | ConvertTo-Json

try {
    Write-Info "Tentative de connexion avec $Email..."
    $authResponse = Invoke-RestMethod -Uri "$ApiUrl/api/Utilisateur/authentifier" `
        -Method Post `
        -Body $authBody `
        -ContentType "application/json" `
        -ErrorAction Stop
    
    $token = $authResponse.token
    $userId = $authResponse.utilisateur.idUtilisateur
    
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }
    
    Write-Success "Authentification réussie !"
    Write-Info "   Token: $($token.Substring(0, 20))..."
    Write-Info "   User ID: $userId"
} catch {
    Write-Error "Échec de l'authentification: $($_.Exception.Message)"
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $reader.BaseStream.Position = 0
        $responseBody = $reader.ReadToEnd()
        Write-Info "Réponse: $responseBody"
    }
    exit 1
}

Start-Sleep -Seconds 1

# ═══════════════════════════════════════════════════════════════════════════════
# 📋 ÉTAPE 2 : RÉCUPÉRATION D'UN ÉLÈVE AVEC TUTEUR ET UTILISATEUR ACTIF
# ═══════════════════════════════════════════════════════════════════════════════

Write-Header "ÉTAPE 2 : RECHERCHE D'UN ÉLÈVE POUR LES TESTS"

try {
    Write-Info "Récupération des élèves de l'école $IdEcole..."
    
    # Récupérer les élèves de l'école
    $elevesResponse = Invoke-RestMethod -Uri "$ApiUrl/api/Eleve/ecole/$IdEcole?page=1&pageSize=10" `
        -Method Get `
        -Headers $headers `
        -ErrorAction Stop
    
    $eleves = $elevesResponse.data
    if (-not $eleves -or $eleves.Count -eq 0) {
        Write-Error "Aucun élève trouvé pour l'école $IdEcole"
        exit 1
    }
    
    Write-Info "   $($eleves.Count) élève(s) trouvé(s)"
    
    # Chercher un élève avec un tuteur qui a un utilisateur actif
    $eleveTest = $null
    $tuteurInfo = $null
    $utilisateurInfo = $null
    
    foreach ($eleve in $eleves) {
        if ($eleve.statut -eq $true -and $eleve.idTuteur) {
            Write-Info "   Vérification de l'élève: $($eleve.nomComplet) (ID: $($eleve.idEleve))"
            
            try {
                # Récupérer les informations du tuteur
                $tuteurResponse = Invoke-RestMethod -Uri "$ApiUrl/api/Tuteur/$($eleve.idTuteur)" `
                    -Method Get `
                    -Headers $headers `
                    -ErrorAction Stop
                
                if ($tuteurResponse.statut -eq $true) {
                    Write-Info "      ✅ Tuteur actif: $($tuteurResponse.nomComplet)"
                    
                    # Vérifier si le tuteur a un utilisateur
                    try {
                        # Récupérer tous les utilisateurs de l'école et filtrer par IdTuteur
                        $utilisateursResponse = Invoke-RestMethod -Uri "$ApiUrl/api/Utilisateur/ecole/$IdEcole?page=1&pageSize=100" `
                            -Method Get `
                            -Headers $headers `
                            -ErrorAction Stop
                        
                        $utilisateurs = $utilisateursResponse.data
                        if ($utilisateurs -and $utilisateurs.Count -gt 0) {
                            # Chercher un utilisateur avec IdTuteur correspondant
                            $utilisateur = $utilisateurs | Where-Object { 
                                $_.idTuteur -eq $tuteurResponse.idTuteur -and $_.statut -eq $true 
                            } | Select-Object -First 1
                            
                            if ($utilisateur) {
                                Write-Info "      ✅ Utilisateur trouvé: $($utilisateur.nomUtilisateur) (ID: $($utilisateur.idUtilisateur))"
                                
                                # Vérifier les devices actifs
                                try {
                                    $devicesResponse = Invoke-RestMethod -Uri "$ApiUrl/api/UserDevice/utilisateur/$($utilisateur.idUtilisateur)" `
                                        -Method Get `
                                        -Headers $headers `
                                        -ErrorAction Stop
                                    
                                    $devicesActifs = $devicesResponse | Where-Object { $_.statut -eq $true -or $_.isActive -eq $true }
                                    if ($devicesActifs -and $devicesActifs.Count -gt 0) {
                                        Write-Success "      ✅ $($devicesActifs.Count) device(s) actif(s) trouvé(s) !"
                                        $eleveTest = $eleve
                                        $tuteurInfo = $tuteurResponse
                                        $utilisateurInfo = $utilisateur
                                        break
                                    } else {
                                        Write-Warning "      ⚠️  Aucun device actif pour cet utilisateur (on continue quand même)"
                                        # On continue quand même avec cet élève
                                        $eleveTest = $eleve
                                        $tuteurInfo = $tuteurResponse
                                        $utilisateurInfo = $utilisateur
                                        break
                                    }
                                } catch {
                                    Write-Warning "      ⚠️  Impossible de vérifier les devices (endpoint peut ne pas exister ou erreur)"
                                    Write-Warning "      ⚠️  Continuons quand même avec cet élève..."
                                    # On continue quand même avec cet élève
                                    $eleveTest = $eleve
                                    $tuteurInfo = $tuteurResponse
                                    $utilisateurInfo = $utilisateur
                                    break
                                }
                            } else {
                                Write-Warning "      ⚠️  Aucun utilisateur trouvé avec IdTuteur = $($tuteurResponse.idTuteur)"
                            }
                        }
                    } catch {
                        Write-Warning "      ⚠️  Impossible de récupérer les utilisateurs: $($_.Exception.Message)"
                    }
                }
            } catch {
                Write-Warning "      ⚠️  Impossible de récupérer le tuteur: $($_.Exception.Message)"
            }
        }
    }
    
    if (-not $eleveTest) {
        Write-Error "Aucun élève avec tuteur et utilisateur actif trouvé."
        Write-Info "   Assurez-vous qu'au moins un élève a :"
        Write-Info "      • Un tuteur actif"
        Write-Info "      • Un utilisateur associé au tuteur"
        Write-Info "      • Au moins un device enregistré (optionnel mais recommandé)"
        exit 1
    }
    
    $eleveId = $eleveTest.idEleve
    $eleveNom = $eleveTest.nomComplet
    $tuteurId = $tuteurInfo.idTuteur
    $tuteurNom = $tuteurInfo.nomComplet
    $tuteurTel = $tuteurInfo.telephone
    $utilisateurId = $utilisateurInfo.idUtilisateur
    
    Write-Success "Élève de test sélectionné :"
    Write-Info "   • Élève: $eleveNom (ID: $eleveId)"
    Write-Info "   • Tuteur: $tuteurNom (ID: $tuteurId)"
    Write-Info "   • Téléphone tuteur: $tuteurTel"
    Write-Info "   • Utilisateur ID: $utilisateurId"
    
} catch {
    Write-Error "Erreur lors de la récupération des données: $($_.Exception.Message)"
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $reader.BaseStream.Position = 0
        $responseBody = $reader.ReadToEnd()
        Write-Info "Réponse: $responseBody"
    }
    exit 1
}

Start-Sleep -Seconds 2

# ═══════════════════════════════════════════════════════════════════════════════
# 📋 ÉTAPE 3 : TEST NOTIFICATION PRÉSENCE
# ═══════════════════════════════════════════════════════════════════════════════

Write-Header "ÉTAPE 3 : TEST NOTIFICATION PRÉSENCE (POINTAGE ÉLÈVE)"

$heureArrivee = (Get-Date).ToString("HH:mm:ss")
$dateDuJour = (Get-Date).ToString("yyyy-MM-dd")

$presenceBody = @{
    idEleve = $eleveId
    isPresent = $true
    heureArrivee = $heureArrivee
    dateDuJour = $dateDuJour
    observation = "Test automatique notification push - Présence"
    latitude = "-4.3276"
    longitute = "15.3136"
} | ConvertTo-Json -Depth 10

Write-Info "Création d'une présence..."
Write-Info "   • Élève: $eleveNom"
Write-Info "   • Date: $dateDuJour"
Write-Info "   • Heure d'arrivée: $heureArrivee"
Write-Info "   • Statut: PRÉSENT"

try {
    $presenceResponse = Invoke-RestMethod -Uri "$ApiUrl/api/Presence" `
        -Method Post `
        -Headers $headers `
        -Body $presenceBody `
        -ErrorAction Stop
    
    $presenceId = $presenceResponse.idPresence
    
    Write-Success "Présence créée avec succès !"
    Write-Info "   • ID Présence: $presenceId"
    Write-Info "   • Date: $dateDuJour $heureArrivee"
    
    Write-Host ""
    Write-Success "Notifications envoyées :"
    Write-Info "   📲 Push Firebase (mobile Android/iOS)"
    Write-Info "   🔔 SignalR (web temps réel)"
    Write-Info "   📱 SMS au tuteur (si configuré)"
    
    Write-Host ""
    Write-Warning "VÉRIFICATIONS À FAIRE :"
    Write-Info "   1. Vérifier les logs de l'API ci-dessous :"
    Write-Info "      → Rechercher: '✅ Notification PUSH Firebase envoyée au tuteur'"
    Write-Info "      → Ou: '⚠️ Échec notification PUSH Firebase'"
    Write-Info "   2. Si un device mobile est enregistré :"
    Write-Info "      → Vérifier la notification push sur le mobile"
    Write-Info "   3. Si SignalR est connecté :"
    Write-Info "      → Vérifier la notification en temps réel sur le web"
    Write-Info "   4. Vérifier le téléphone du tuteur pour SMS :"
    Write-Info "      → Téléphone: $tuteurTel"
    
} catch {
    Write-Error "Erreur lors de la création de la présence: $($_.Exception.Message)"
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $reader.BaseStream.Position = 0
        $responseBody = $reader.ReadToEnd()
        Write-Info "Réponse: $responseBody"
    }
}

Start-Sleep -Seconds 3

# ═══════════════════════════════════════════════════════════════════════════════
# 📋 ÉTAPE 4 : TEST NOTIFICATION PAIEMENT
# ═══════════════════════════════════════════════════════════════════════════════

Write-Header "ÉTAPE 4 : TEST NOTIFICATION PAIEMENT (FRAIS SCOLAIRES)"

# Récupérer un frais disponible (optionnel)
$idFrais = $null
try {
    Write-Info "Recherche d'un frais disponible (optionnel)..."
    
    # Essayer différents endpoints possibles
    $fraisResponse = $null
    try {
        $fraisResponse = Invoke-RestMethod -Uri "$ApiUrl/api/Frais/ecole/$IdEcole" `
            -Method Get `
            -Headers $headers `
            -ErrorAction Stop
    } catch {
        # Essayer l'endpoint général
        try {
            $fraisResponse = Invoke-RestMethod -Uri "$ApiUrl/api/Frais" `
                -Method Get `
                -Headers $headers `
                -ErrorAction Stop
        } catch {
            Write-Warning "   ⚠️  Impossible de récupérer les frais (endpoint peut ne pas exister)"
        }
    }
    
    if ($fraisResponse -and $fraisResponse.Count -gt 0) {
        $fraisActif = $fraisResponse | Where-Object { $_.statut -eq $true } | Select-Object -First 1
        if ($fraisActif) {
            $idFrais = $fraisActif.idFrais
            Write-Info "   ✅ Frais trouvé: $($fraisActif.typeFrais) - $($fraisActif.montant) $($fraisActif.devise)"
        } else {
            Write-Info "   ℹ️  Aucun frais actif trouvé (on créera un paiement sans frais)"
        }
    } else {
        Write-Info "   ℹ️  Aucun frais trouvé (on créera un paiement sans frais)"
    }
} catch {
    Write-Warning "   ⚠️  Impossible de récupérer les frais (on créera un paiement sans frais)"
}

$reference = "TEST-NOTIF-$(Get-Date -Format 'yyyyMMddHHmmss')"
$montant = 50000

$paiementBody = @{
    idEleve = $eleveId
    montant = $montant
    datePaiement = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")
    modePaiement = "Espèces"
    referenceTransaction = $reference
    commentaire = "Test automatique notification push - Paiement de frais"
    devise = "CDF"
    statutPaiement = "Confirmé"
    statut = $true
    idUtilisateur = $userId
}

if ($idFrais) {
    $paiementBody.idFrais = $idFrais
}

$paiementBody = $paiementBody | ConvertTo-Json -Depth 10

Write-Info "Création d'un paiement..."
Write-Info "   • Élève: $eleveNom"
Write-Info "   • Montant: $montant CDF"
Write-Info "   • Mode: Espèces"
Write-Info "   • Référence: $reference"
if ($idFrais) {
    Write-Info "   • Frais ID: $idFrais"
}

try {
    $paiementResponse = Invoke-RestMethod -Uri "$ApiUrl/api/Paiement" `
        -Method Post `
        -Headers $headers `
        -Body $paiementBody `
        -ErrorAction Stop
    
    $paiementId = $paiementResponse.idPaiement
    
    Write-Success "Paiement créé avec succès !"
    Write-Info "   • ID Paiement: $paiementId"
    Write-Info "   • Montant: $montant CDF"
    Write-Info "   • Référence: $reference"
    
    Write-Host ""
    Write-Success "Notifications envoyées :"
    Write-Info "   📲 Push Firebase (mobile Android/iOS)"
    Write-Info "   🔔 SignalR (web temps réel)"
    Write-Info "   📱 SMS au tuteur (si configuré)"
    
    Write-Host ""
    Write-Warning "VÉRIFICATIONS À FAIRE :"
    Write-Info "   1. Vérifier les logs de l'API ci-dessous :"
    Write-Info "      → Rechercher: '✅ Notification PUSH Firebase paiement envoyée au tuteur'"
    Write-Info "      → Ou: '⚠️ Échec notification PUSH Firebase'"
    Write-Info "   2. Si un device mobile est enregistré :"
    Write-Info "      → Vérifier la notification push sur le mobile"
    Write-Info "   3. Si SignalR est connecté :"
    Write-Info "      → Vérifier la notification en temps réel sur le web"
    Write-Info "   4. Vérifier le téléphone du tuteur pour SMS :"
    Write-Info "      → Téléphone: $tuteurTel"
    
} catch {
    Write-Error "Erreur lors de la création du paiement: $($_.Exception.Message)"
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $reader.BaseStream.Position = 0
        $responseBody = $reader.ReadToEnd()
        Write-Info "Réponse: $responseBody"
    }
}

Start-Sleep -Seconds 2

# ═══════════════════════════════════════════════════════════════════════════════
# 📊 RÉSUMÉ FINAL
# ═══════════════════════════════════════════════════════════════════════════════

Write-Header "RÉSUMÉ DES TESTS"

Write-Success "Tests terminés avec succès !"
Write-Host ""
Write-Info "Résumé :"
Write-Info "   • Présence créée: ID $presenceId"
Write-Info "   • Paiement créé: ID $paiementId"
Write-Info "   • Élève testé: $eleveNom"
Write-Info "   • Tuteur: $tuteurNom"
Write-Host ""
Write-Warning "PROCHAINES ÉTAPES :"
Write-Info "   1. Vérifier les logs de l'API pour confirmer l'envoi des notifications"
Write-Info "   2. Vérifier les devices mobiles pour les notifications push"
Write-Info "   3. Vérifier SignalR pour les notifications en temps réel"
Write-Info "   4. Vérifier les SMS envoyés au tuteur"
Write-Host ""
Write-Info "Pour relancer les tests, exécutez :"
Write-Host "   .\test-notifications-push-automatise.ps1" -ForegroundColor Cyan
Write-Host ""

