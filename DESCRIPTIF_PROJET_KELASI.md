# Document descriptif du projet — Kelasi Na Biso API

**Version** : 1.0  
**Date** : janvier 2026  
**Type** : API REST — gestion scolaire multi-établissements  
**Nom du produit** : Kelasi Na Biso (plateforme de gestion scolaire)

---

## 1. Présentation générale

**Kelasi Na Biso API** est le backend d’une plateforme de **gestion scolaire** destinée aux établissements d’enseignement (écoles primaires, secondaires, instituts, etc.). Elle centralise les données pédagogiques, administratives et financières, et expose des services REST consommables par des applications web ou mobiles (tableaux de bord, portails parents, applications enseignants, etc.).

Le projet répond à un besoin concret : **digitaliser et sécuriser** la vie d’un établissement — inscriptions, suivi des élèves, notes, présences, paiements, communication avec les familles — tout en permettant à **plusieurs écoles** de coexister dans la même instance logicielle (**multi-établissements**), avec des droits d’accès différenciés selon le rôle de chaque utilisateur.

---

## 2. Contexte et objectifs

### 2.1 Contexte

Dans de nombreux contextes (notamment en République Démocratique du Congo et en Afrique centrale), la gestion scolaire repose encore sur des processus manuels ou fragmentés : fichiers Excel, registres papier, communication limitée avec les parents. Kelasi Na Biso vise à offrir un **système unifié**, accessible en ligne, traçable et évolutif.

### 2.2 Objectifs principaux

| Objectif | Description |
|----------|-------------|
| **Centralisation** | Une seule base de données et une API pour toutes les entités scolaires (élèves, classes, agents, paiements, etc.). |
| **Multi-établissements** | Chaque utilisateur est rattaché à une école ; les données sont isolées logiquement par établissement. |
| **Sécurisation** | Authentification JWT, rôles et permissions granulaires (RBAC), journal d’audit, limitation du débit des requêtes. |
| **Productivité** | Import en masse (inscriptions, paiements), tableaux de bord, notifications multi-canal. |
| **Traçabilité** | Soft delete sur les entités sensibles, historique des actions, cohérence élève / inscription / statut. |

### 2.3 Périmètre fonctionnel (résumé)

- Gestion **administrative** : écoles, directions, sections, options, classes, années scolaires, agents.
- Gestion **élèves et familles** : élèves, tuteurs, inscriptions, critères d’unicité, réinscriptions.
- Gestion **pédagogique** : cours, affectations, notes, évaluations, devoirs à domicile, présences, vacations.
- Gestion **financière** : frais, paiements, import Excel, suivi des échecs d’import, tableaux de bord financiers.
- **Communication** : messagerie interne, campagnes (push, email, SMS, in-app), notifications temps réel.
- **Pilotage** : dashboards globaux, par école, KPI, synthèse multi-écoles (Super-Admin), métriques système.

---

## 3. Public cible et acteurs

### 3.1 Utilisateurs métier

| Acteur | Rôle typique | Usage principal |
|--------|--------------|-----------------|
| **Super-Admin** | Exploitant de la plateforme | Toutes les écoles, synthèse globale, configuration système. |
| **Admin** | Responsable IT / direction d’une école | Gestion utilisateurs, école, paramètres, imports. |
| **Directeur** | Direction d’établissement | Pilotage, validation, statistiques, communication. |
| **Enseignant / Agent** | Personnel enseignant ou administratif | Notes, présences, classes, devoirs. |
| **Comptable / Financier** | Gestion des frais et paiements | Frais, paiements, rapports financiers. |
| **Parent / Tuteur** | Responsable légal de l’élève | Consultation inscriptions, paiements, messages. |
| **Élève** | (selon déploiement) | Consultation limitée de ses données. |

### 3.2 Utilisateurs techniques

- **Développeurs frontend** : consommation de l’API via JWT, documentation Swagger, collections Postman.
- **Administrateurs système** : déploiement, base MariaDB/MySQL, logs Serilog, configuration des services externes (Firebase, SMTP, Twilio, AWS S3).
- **Équipe qualité** : tests unitaires et d’intégration, scripts SQL de diagnostic et correction des données.

---

## 4. Fonctionnalités détaillées par domaine

### 4.1 Authentification et comptes utilisateurs

- Connexion par **email ou téléphone** et mot de passe (hash **BCrypt**).
- Émission de **tokens JWT** et **refresh tokens** (durée configurable).
- Création de comptes par les administrateurs ; comptes **automatiques** à la création d’un agent ou lors d’une inscription (parent).
- Changement de mot de passe, mot de passe oublié (lien par email), réinitialisation en masse ou individuelle.
- Flag **DoitChangerMotDePasse** pour forcer la mise à jour à la première connexion (comptes créés automatiquement).
- Politique mot de passe actuelle : **minimum 5 caractères** (sans exigence de complexité majuscule / spéciale).
- **Multi-rôles** : un utilisateur peut cumuler plusieurs rôles via la table `UserRole` (rôle principal désigné).

### 4.2 Gestion des établissements et structure scolaire

Hiérarchie typique :

```
École
 └── Direction (ex. Primaire, Secondaire)
      └── Classe (liée à Section / Option)
           └── Élève
```

- CRUD sur écoles, directions, sections, options, classes, années scolaires, vacations (créneaux horaires).
- Création d’école avec **administrateur par défaut** (Manager Général) et attribution automatique des permissions du rôle Admin.
- Soft delete (`Statut`) sur la majorité des entités pour préserver l’historique.

### 4.3 Élèves, tuteurs et inscriptions

- Fiche élève : identité, adresse, classe, tuteur, matricule, photo, statut actif/inactif.
- **Inscription** : création manuelle ou en masse (Excel), lien élève–classe–école–année scolaire.
- **Unicité** : détection des doublons sur base de nom complet normalisé, date de naissance et tuteur.
- Règle métier : **un élève actif = une inscription active** (corrections SQL et cascade à la désactivation d’un élève).
- Pagination, recherche, filtres par école, classe, statut.
- Vues enrichies (`V_Eleve`) pour listes et reporting.

### 4.4 Personnel (agents)

- Modèle unifié **Agent** (enseignants, direction, comptables, etc.) avec fonction et rôle applicatif.
- Création d’un compte utilisateur lié automatiquement, attribution du rôle selon la fonction.
- Gestion multi-rôles sur un agent existant (ajout / remplacement de rôles).
- Titulaires de classe (Maternelle / Primaire).

### 4.5 Vie scolaire et pédagogie

- **Notes** et **évaluations** par élève, cours, période.
- **Présences** : pointage par élève, vacation, date ; reporting par école.
- **Cours** et **affectations** enseignant–classe–cours.
- **Devoirs à domicile** : contenu, fichiers (stockage local ou **AWS S3**), téléchargements, antivirus sur upload.
- **Ressources pédagogiques** et **documents** liés aux élèves ou à l’école.

### 4.6 Finances

- Définition des **frais** par direction / type.
- Enregistrement et validation des **paiements**.
- **Import Excel** des paiements (recherche élève et libellé de frais par normalisation avancée des noms).
- Table **PaiementCrashed** pour les lignes en échec à corriger ou réinjecter.
- Tableaux de bord paiements (montants, modes, retards, répartition).

### 4.7 Communication et notifications

- **Messagerie** privée et par groupes.
- **Campagnes** ciblées (segments) : Push (Firebase), Email (SMTP), SMS (Twilio, activable), notifications in-app.
- **SignalR** : hubs temps réel (`/hubs/notifications`, `/hubs/dashboard`).
- Notifications liées aux inscriptions, paiements, présences, devoirs.

### 4.8 Tableaux de bord et pilotage

Endpoints dédiés sous `/api/Dashboard` :

| Endpoint | Description | Accès |
|----------|-------------|--------|
| `global` | Statistiques générales d’une école | Admin / direction |
| `presence` | Synthèse présences | Authentifié |
| `paiement` | Synthèse financière | Authentifié |
| `comparaison` | Comparaisons temporelles | Authentifié |
| `kpi` | Indicateurs clés | Authentifié |
| `super-admin` | Vue plateforme | Super-Admin |
| `synthese-ecole` | Liste des écoles avec statistiques élèves | Super-Admin |

Comptages élèves : distinction **total** vs **actifs** ; répartitions par direction, section, option, genre, province, ville.

### 4.9 Monitoring et audit

- Route **Metrics** : santé système, base de données, métriques académiques et financières agrégées.
- **Audit** : traçage des créations / modifications / suppressions sur les entités sensibles.
- **Logs** Serilog : console, fichiers rotatifs, option MySQL.

### 4.4 Imports et traitements de masse

| Module | Format | Fonction |
|--------|--------|----------|
| Inscriptions | Excel | Création élèves, tuteurs, inscriptions en lot |
| Paiements | Excel | Enregistrement paiements avec matching intelligent |
| Scripts SQL | SQL | Diagnostic, correction cohérence données (prod) |

---

## 5. Architecture technique

### 5.1 Stack technologique

| Composant | Technologie |
|-----------|-------------|
| Framework | **ASP.NET Core** (.NET 6) |
| ORM | **Entity Framework Core 6** |
| Base de données | **MariaDB 10.11** / MySQL (provider Pomelo) |
| API | REST + **Swagger/OpenAPI** |
| Temps réel | **SignalR** |
| Auth | **JWT Bearer** + refresh tokens |
| Mots de passe | **BCrypt** |
| Logs | **Serilog** |
| Cache / perf | Compression Gzip/Brotli, cache mémoire |
| Sécurité requêtes | **AspNetCoreRateLimit** (limitation par IP) |
| Fichiers | Stockage local ou **Amazon S3** |
| Tests | xUnit (unitaires + intégration) |

### 5.2 Architecture logicielle

Le projet suit une **architecture en couches** avec le pattern **Repository** :

```
┌─────────────────────────────────────────┐
│           Controllers (API)              │  ← HTTP, validation, autorisation
├─────────────────────────────────────────┤
│           Services (métier)              │  ← Règles métier, orchestration
├─────────────────────────────────────────┤
│     Repositories (interfaces + impl.)    │  ← Accès données, requêtes EF
├─────────────────────────────────────────┤
│   Models / DTOs + KelasiNaBisoDbContext  │  ← Entités, transferts, EF Core
└─────────────────────────────────────────┘
                    │
                    ▼
              MariaDB / MySQL
```

**Principes appliqués** :

- Injection de dépendances (DI) dans `Program.cs`.
- DTOs dédiés pour création / mise à jour (séparation API ↔ entités).
- Pagination standardisée (`PagedRequest`, `PagedResult`, pagination par curseur).
- Vues SQL / entités read-only (`V_Eleve`, `V_Utilisateur`) pour les listes complexes.

### 5.3 Organisation du dépôt (synthèse)

| Dossier | Rôle |
|---------|------|
| `Controllers/` | ~40 contrôleurs API |
| `Services/` | Logique métier et implémentations repositories |
| `Models/` | Entités, DTOs, enums |
| `Data/` | DbContext, seeders (permissions, rôles) |
| `Migrations/` | Migrations EF et scripts SQL |
| `SCRIPTS_SQL/` | Scripts maintenance et correction production |
| `EmailTemplates/` | Modèles d’emails transactionnels |
| `KelasiNaBiso.Tests.*` | Tests automatisés |

---

## 6. Sécurité et gouvernance des accès

### 6.1 Authentification

- Toutes les routes sensibles sont protégées par **`[Authorize]`** (JWT).
- Authentification : `POST /api/Utilisateur/authentifier`.
- Claims JWT : identifiant utilisateur, rôle, école, etc.

### 6.2 Autorisation (RBAC)

- Modèle **Role – Permission – UserRole – RolePermission**.
- Attribut personnalisé **`[Permission("Categorie.Action")]`** sur les endpoints (ex. `Eleve.ReadAll`, `Paiement.Validate`).
- Seeder de permissions au démarrage ; méthode `EnsureAdminPermissionsAsync` pour les écoles créées après coup.
- Matrice documentée : `MATRICE_ENDPOINTS_SECURITE.md`.

### 6.3 Rôles principaux

| Rôle | Niveau | Périmètre |
|------|--------|-----------|
| Super-Admin | 1 | Plateforme entière |
| Admin | 2 | Une école |
| Directeur | 3 | Pédagogie + admin limité |
| Enseignant | 4 | Classes assignées |
| Comptable | 5 | Finances |
| Parent | 6 | Enfants liés |
| Élève | 7 | Données personnelles |

### 6.4 Bonnes pratiques sécurité

- Mots de passe jamais retournés en clair dans les réponses API.
- Filtrage des entités inactives (`Statut = true`) sur les listes par défaut.
- Audit des modifications sensibles.
- Rate limiting configurable contre abus et brute-force.

---

## 7. Intégrations externes

| Service | Usage |
|---------|--------|
| **Firebase Cloud Messaging** | Notifications push mobiles |
| **SMTP (ex. Gmail)** | Emails de bienvenue, reset mot de passe, campagnes |
| **Twilio** | SMS (optionnel, `Enabled` dans configuration) |
| **Amazon S3** | Stockage de fichiers (devoirs, documents) si credentials configurés |
| **MariaDB / MySQL** | Persistance principale |

La configuration se fait via `appsettings.json` / `appsettings.Development.json` (chaînes de connexion, JWT, clés externes). **Les secrets ne doivent pas être versionnés en production** ; utiliser des variables d’environnement ou un coffre de secrets.

---

## 8. Modèle de données (vue conceptuelle)

Entités centrales et relations :

- **Ecole** ↔ Direction ↔ Classe ↔ Eleve
- **Eleve** ↔ Tuteur, Inscription, Note, Presence, Paiement
- **Utilisateur** ↔ Ecole, Role(s), Agent ou Tuteur
- **Agent** ↔ Ecole, AffectationCours
- **Frais** ↔ Paiement ↔ Eleve
- **Communication** : Message, GroupeMessage, CommunicationCampaign, segments, destinataires

Pattern **soft delete** : champ booléen `Statut` sur la plupart des tables (actif / inactif plutôt que suppression physique).

---

## 9. Déploiement et exploitation

### 9.1 Prérequis

- .NET 6 SDK (ou version cible du `.csproj`)
- MariaDB 10.11+ ou MySQL 8+
- (Optionnel) Compte Firebase, SMTP, Twilio, AWS pour les intégrations

### 9.2 Démarrage local

```bash
dotnet restore
dotnet build
dotnet run
```

- API HTTPS : port **7102** (configurable dans `Program.cs`)
- Documentation interactive : **`/swagger`**

### 9.3 Base de données

- Chaîne de connexion : `ConnectionStrings:KelasiConnection`
- Migrations Entity Framework + scripts SQL complémentaires dans `Migrations/` et `SCRIPTS_SQL/`

### 9.4 Observabilité

- Logs applicatifs : dossier `logs/`
- Niveau configurable via Serilog
- Endpoint metrics pour supervision

---

## 10. Qualité et maintenance

- **Tests unitaires** : `KelasiNaBiso.Tests.Unit`
- **Tests d’intégration** : `KelasiNaBiso.Tests.Integration` (ex. contrôleurs, multi-rôles)
- **Documentation technique** : plus de 100 fichiers Markdown (guides RBAC, bulk insert, dashboard, corrections SQL, etc.)
- **Index documentation** : `README_DOCUMENTATION.md`
- Scripts de **diagnostic et correction** des incohérences élève/inscription documentés et versionnés

---

## 11. Documentation associée (références internes)

| Document | Contenu |
|----------|---------|
| `README.md` | Vue d’ensemble et démarrage rapide |
| `README_DOCUMENTATION.md` | Index de toute la documentation |
| `STRUCTURE_PROJET.md` | Architecture détaillée du code |
| `ARCHITECTURE.md` | Couches et patterns |
| `MATRICE_ENDPOINTS_SECURITE.md` | Sécurité par endpoint |
| `DOCUMENTATION_BULK_INSERT_INSCRIPTIONS.md` | Import inscriptions |
| `DOCUMENTATION_BULK_INSERT_PAIEMENTS.md` | Import paiements |
| `DOCUMENTATION_ENDPOINT_SYNTHESE_ECOLE.md` | Synthèse multi-écoles |
| `DOCUMENTATION_METRICS_API.md` | Monitoring |
| `README_COMMUNICATION.md` | Module communication |
| `SCRIPTS_SQL/README.md` | Scripts SQL maintenance |

Collections **Postman** : `KelasiNaBiso_API.postman_collection.json`

---

## 12. Évolutions récentes et axes d’amélioration

### Réalisé récemment (indicatif)

- Système **multi-rôles** (UserRole) et permissions Admin à la création d’école.
- **Dashboard** harmonisé (élèves actifs vs totaux, synthèse Super-Admin).
- Recherche **normalisée** pour imports Excel (élèves, frais).
- **Cascade** désactivation inscriptions lors du soft delete élève.
- Contraintes mot de passe **assouplies** (min. 5 caractères).
- Endpoint **Metrics** et documentation associée.

### Axes possibles

- Harmonisation complète du filtrage `Eleve.Statut` sur toutes les requêtes d’inscriptions.
- Centralisation de la politique mot de passe (helper unique).
- Correction du double hash éventuel à la création utilisateur via contrôleur.
- Renforcement des tests E2E sur imports et dashboards.
- Internationalisation (FR/EN) des messages API.

---

## 13. Synthèse

**Kelasi Na Biso API** est une solution backend **complète et modulaire** pour la gestion scolaire multi-établissements. Elle combine :

- une **API REST** riche et documentée ;
- une **sécurité** par rôles et permissions ;
- des **imports** et **tableaux de bord** orientés terrain ;
- une **communication** multi-canal avec les familles ;
- une base technique **maintenable** (EF Core, MariaDB, .NET).

Le projet est adapté à un déploiement par **plusieurs écoles clientes** sur une même infrastructure, avec un niveau de personnalisation par établissement et un pilotage central pour l’exploitant (Super-Admin).

---

## 14. Contacts et licence

*À compléter selon votre organisation :*

- **Porteur du projet** : …
- **Équipe technique** : …
- **Support** : …
- **Licence** : …

---

*Document généré à partir de l’analyse du dépôt KelasiNaBisoAPI — janvier 2026.*
