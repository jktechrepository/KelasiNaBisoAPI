# Publication KelasiNaBisoAPI pour dev-knb.asdc-rdc.org
# Usage : .\publish-dev-knb.ps1

$ErrorActionPreference = "Stop"
$ProjectRoot = $PSScriptRoot
$OutputDir   = Join-Path $ProjectRoot "publish\dev-knb"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Publication dev-knb.asdc-rdc.org" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 1. Nettoyer l'ancien dossier
if (Test-Path $OutputDir) {
    Write-Host "Suppression de $OutputDir ..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force $OutputDir
}

# 2. dotnet publish
Write-Host "dotnet publish (Release) ..." -ForegroundColor Green
Push-Location $ProjectRoot
dotnet publish KelasiNaBiso.csproj -c Release -o $OutputDir --no-self-contained
if ($LASTEXITCODE -ne 0) {
    Pop-Location
    Write-Host "Echec publish." -ForegroundColor Red
    exit 1
}
Pop-Location

# 3. Dossier logs
$logsDir = Join-Path $OutputDir "logs"
New-Item -ItemType Directory -Force -Path $logsDir | Out-Null
New-Item -ItemType File -Force -Path (Join-Path $logsDir ".gitkeep") | Out-Null

# 4. Fichiers complementaires
$extras = @(
    @{ Src = "scripts\migration-moko-afrika-manual.sql"; Dst = "scripts\migration-moko-afrika-manual.sql" },
    @{ Src = "scripts\verification-moko-migration.sql"; Dst = "scripts\verification-moko-migration.sql" },
    @{ Src = "DOCUMENTATION_FRONTEND_PAIEMENT_MOKO.md"; Dst = "docs\DOCUMENTATION_FRONTEND_PAIEMENT_MOKO.md" },
    @{ Src = "DOCUMENTATION_CARTES_IMPRESSION.md"; Dst = "docs\DOCUMENTATION_CARTES_IMPRESSION.md" },
    @{ Src = "DOCUMENTATION_FRONTEND_CARTES_IMPRESSION.md"; Dst = "docs\DOCUMENTATION_FRONTEND_CARTES_IMPRESSION.md" },
    @{ Src = "KelasiNaBiso_API.postman_collection.json"; Dst = "docs\KelasiNaBiso_API.postman_collection.json" }
)
foreach ($f in $extras) {
    $srcPath = Join-Path $ProjectRoot $f.Src
    $dstPath = Join-Path $OutputDir $f.Dst
    if (Test-Path $srcPath) {
        $dstFolder = Split-Path $dstPath -Parent
        New-Item -ItemType Directory -Force -Path $dstFolder | Out-Null
        Copy-Item $srcPath $dstPath -Force
        Write-Host "Copie : $($f.Dst)" -ForegroundColor Gray
    }
}

# Reports/Templates (FastReport cartes)
$reportsSrc = Join-Path $ProjectRoot "Reports"
$reportsDst = Join-Path $OutputDir "Reports"
if (Test-Path $reportsSrc) {
    Copy-Item $reportsSrc $reportsDst -Recurse -Force
    Write-Host "Copie : Reports/" -ForegroundColor Gray
}

# 5. README deploiement + notes de version
$readme = Join-Path $OutputDir "README-DEPLOIEMENT.md"
Copy-Item (Join-Path $ProjectRoot "DEPLOIEMENT_DEV_KNB.md") $readme -Force -ErrorAction SilentlyContinue
if (-not (Test-Path $readme)) {
    Write-Host "README DEPLOIEMENT_DEV_KNB.md absent a la racine" -ForegroundColor Yellow
}

$releaseNotes = Join-Path $OutputDir "RELEASE-NOTES.md"
$buildDate = Get-Date -Format "yyyy-MM-dd HH:mm"
@"
# Release dev-knb — $buildDate

## Changements depuis le publish precedent (04/07/2026 ~16:05)

### API (code)
- Gestion erreur **503** ``MOKO_MIGRATION_REQUIRED`` si tables MOKO absentes (au lieu de 500)
- ``GET /api/Ecole/{id}/paiement-mobile`` retourne **200** avec ``estConfigure: false`` si non configure
- Controleurs / services MOKO Afrika (PayIn, config ecole, wallet)

### Scripts SQL (dossier ``scripts/``)
- ``migration-moko-afrika-manual.sql`` : ``USE ``dev-knb_db`` `` (backticks obligatoires)
- ``verification-moko-migration.sql`` : script de controle post-migration

### Documentation
- ``docs/DOCUMENTATION_FRONTEND_PAIEMENT_MOKO.md`` mise a jour
- **FastReport cartes** : ``docs/DOCUMENTATION_CARTES_IMPRESSION.md``, endpoints ``/api/Carte/*``

## Actions serveur

1. **Migration BDD** (si pas encore fait sur ``dev-knb_db``) :
   ``mysql -u USER -p dev-knb_db < scripts/migration-moko-afrika-manual.sql``
   Ou import phpMyAdmin sur base ``dev-knb_db`` (pas ``knb_db``).
2. Verifier : ``scripts/verification-moko-migration.sql``
3. Uploader ``publish/dev-knb/`` sur LWS
4. Recycler le pool IIS
5. Tests : Swagger, ``GET /api/MokoAfrika/fees/estimate``, ``GET /api/Ecole/13/paiement-mobile``, ``GET /api/Carte/eleve/{id}/pdf``

"@ | Set-Content -Path $releaseNotes -Encoding UTF8
Write-Host "Genere : RELEASE-NOTES.md" -ForegroundColor Gray

# 6. Archive ZIP (optionnel, pour upload Plesk)
$zipPath = Join-Path $OutputDir "dev-knb.zip"
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path (Join-Path $OutputDir "*") -DestinationPath $zipPath -Force
Write-Host "Archive : dev-knb.zip" -ForegroundColor Gray

# 7. Resume
$fileCount = (Get-ChildItem $OutputDir -Recurse -File).Count
$sizeMb    = [math]::Round(((Get-ChildItem $OutputDir -Recurse -File | Measure-Object Length -Sum).Sum / 1MB), 2)

Write-Host ""
Write-Host "Publication terminee." -ForegroundColor Green
Write-Host "  Dossier : $OutputDir" -ForegroundColor White
Write-Host "  Fichiers : $fileCount | Taille : ${sizeMb} Mo" -ForegroundColor White
Write-Host ""
Write-Host "Swagger apres deploiement :" -ForegroundColor Cyan
Write-Host "  https://dev-knb.asdc-rdc.org/swagger/index.html" -ForegroundColor White
Write-Host ""
