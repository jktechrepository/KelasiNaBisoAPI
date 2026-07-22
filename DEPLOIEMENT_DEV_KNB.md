# Déploiement KelasiNaBiso API — dev-knb.asdc-rdc.org

**URL Swagger :** [https://dev-knb.asdc-rdc.org/swagger/index.html](https://dev-knb.asdc-rdc.org/swagger/index.html)

---

## 1. Générer le dossier de publication (local)

```powershell
cd C:\Developpements\kelasi\KelasiNaBisoAPI\KelasiNaBisoAPI
.\publish-dev-knb.ps1
```

Le dossier **`publish/dev-knb/`** contient tous les fichiers à uploader sur le serveur LWS.

---

## 2. Prérequis serveur (LWS / IIS)

| Composant | Version |
|-----------|---------|
| .NET Runtime | **6.0** ou supérieur ([ASP.NET Core Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/6.0)) |
| MySQL / MariaDB | 10.x — base **`dev-knb_db`** |
| IIS | ASP.NET Core Module V2 (inclus dans Hosting Bundle) |

---

## 3. Upload sur le serveur

1. Copier **tout le contenu** de `publish/dev-knb/` vers le répertoire du site (ex. `wwwroot/dev-knb/` ou racine du sous-domaine).
2. Vérifier la présence de :
   - `KelasiNaBiso.dll`
   - `web.config`
   - `appsettings.json` + `appsettings.Production.json`
   - `kelasinabiso-de502-firebase-adminsdk-fbsvc-954ac3e526.json`
   - dossier `logs/` (écriture autorisée pour IIS)

---

## 4. Configuration à adapter sur le serveur

Éditer **`appsettings.Production.json`** (ou variables d'environnement IIS) :

### Base de données

```json
"ConnectionStrings": {
  "KelasiConnection": "Server=localhost;Port=3306;Database=dev-knb_db;User=VOTRE_USER;Password=VOTRE_MDP;CharSet=utf8mb4;"
}
```

Mettre à jour aussi la connection string Serilog MySQL dans la même section.

### CORS (frontends autorisés)

Ajouter l'URL du frontend dans `Cors:AllowedOrigins` si différente.

### MOKO Afrika (paiement)

```json
"MokoSettings": {
  "IsProduction": false,
  "CallbackUrl": "https://dev-knb.asdc-rdc.org/api/MokoAfrika/callback"
}
```

Renseigner `MerchantId`, `SecretKey`, etc. dans `appsettings.json` (hérité) ou via secrets serveur.

---

## 5. Migration base de données MOKO

Si les tables MOKO ne existent pas encore sur **`dev-knb_db`** :

```bash
mysql -u USER -p dev-knb_db < scripts/migration-moko-afrika-manual.sql
```

**phpMyAdmin :** sélectionner `dev-knb_db` à gauche, puis Importer le script.

> Le nom de base contient un tiret : utiliser `` USE `dev-knb_db`; `` (backticks obligatoires).

Vérification après migration :

```bash
mysql -u USER -p dev-knb_db < scripts/verification-moko-migration.sql
```

Scripts inclus dans le dossier publié :
- `scripts/migration-moko-afrika-manual.sql`
- `scripts/verification-moko-migration.sql`

**Note :** si la migration a déjà été appliquée (6 tables MOKO présentes), cette étape est ignorée — déployer uniquement l'API.

---

## 6. Pool d'applications IIS

1. Créer / recycler le **Application Pool** :
   - .NET CLR version : **No Managed Code**
   - Identity : compte avec droits lecture/écriture sur `logs/`
2. Site lié au sous-domaine `dev-knb.asdc-rdc.org`
3. Certificat SSL actif (HTTPS obligatoire pour JWT / MOKO callback)

---

## 7. Vérifications post-déploiement

| Test | URL / action |
|------|----------------|
| Swagger UI | https://dev-knb.asdc-rdc.org/swagger/index.html |
| Auth | `POST /api/Utilisateur/authentifier` |
| MOKO fees | `GET /api/MokoAfrika/fees/estimate?amount=500&method=airtel` |
| Carte élève PDF | `GET /api/Carte/eleve/{id}/pdf` (token Admin) |
| Logs stdout | `logs/stdout_*.log` en cas d'erreur démarrage |

---

## 8. Dépannage

| Symptôme | Cause probable | Action |
|----------|----------------|--------|
| 502.5 / app ne démarre pas | .NET 6 Hosting Bundle manquant | Installer Hosting Bundle + `iisreset` |
| Erreur MySQL au démarrage | Connection string incorrecte | Corriger `appsettings.Production.json` |
| Swagger 404 | Mauvais chemin IIS / reverse proxy | Vérifier que toutes les routes passent vers l'app |
| MOKO 503 `MOKO_MIGRATION_REQUIRED` | Tables MOKO absentes | Exécuter `scripts/migration-moko-afrika-manual.sql` sur `dev-knb_db` |
| MOKO 500 sur paiement-mobile | Ancienne version API | Redéployer + migration BDD |
| Firebase notifications KO | Fichier JSON absent | Vérifier `kelasinabiso-de502-firebase-adminsdk-fbsvc-954ac3e526.json` |
| Emails non envoyés | SMTP port 465 | Passer `EmailSettings:Port` à **587** si échec |

---

## 9. Structure du dossier publié

```
publish/dev-knb/
├── KelasiNaBiso.dll          # Application principale
├── web.config                # IIS + ASPNETCORE_ENVIRONMENT=Production
├── appsettings.json
├── appsettings.Production.json
├── kelasinabiso-de502-firebase-adminsdk-fbsvc-954ac3e526.json
├── logs/
├── scripts/
│   ├── migration-moko-afrika-manual.sql
│   └── verification-moko-migration.sql
├── RELEASE-NOTES.md
├── dev-knb.zip                 # Archive pour upload Plesk (optionnel)
├── docs/
│   ├── DOCUMENTATION_FRONTEND_PAIEMENT_MOKO.md
│   └── KelasiNaBiso_API.postman_collection.json
└── README-DEPLOIEMENT.md
```

---

*KelasiNaBiso API — environnement dev-knb — ASDC RDC*
