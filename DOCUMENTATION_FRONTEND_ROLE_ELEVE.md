# Documentation Frontend — Rôle Eleve (Flutter / Vue)

Guide d’intégration de l’espace **élève** (app mobile Flutter ou web Vue) pour le rôle JWT **`Eleve`**.

**PayIn Moko (détail) :** [DOCUMENTATION_FRONTEND_PAIEMENT_MOKO.md](DOCUMENTATION_FRONTEND_PAIEMENT_MOKO.md)  
**Barèmes / solde frais :** [DOCUMENTATION_INTEGRATION_FRONTEND_FRAIS_VUE_FLUTTER.md](DOCUMENTATION_INTEGRATION_FRONTEND_FRAIS_VUE_FLUTTER.md) (scoping année/classe)

**Base URL dev :** `https://dev-knb.asdc-rdc.org`  
**Auth :** `Authorization: Bearer {accessToken}`

---

## Table des matières

1. [Vue d’ensemble](#1-vue-densemble)
2. [Parcours utilisateur](#2-parcours-utilisateur)
3. [Authentification](#3-authentification)
4. [JWT et identité](#4-jwt-et-identité)
5. [Permissions](#5-permissions)
6. [Dashboard](#6-dashboard)
7. [Devoirs à domicile](#7-devoirs-à-domicile)
8. [Notes et bulletins](#8-notes-et-bulletins)
9. [Frais, solde et paiements](#9-frais-solde-et-paiements)
10. [Payer un frais (PayIn Moko)](#10-payer-un-frais-payin-moko)
11. [Profil](#11-profil)
12. [Erreurs et pièges](#12-erreurs-et-pièges)
13. [Checklist Flutter / Vue](#13-checklist-flutter--vue)

---

## 1. Vue d’ensemble

| Capacité | Eleve | Parent | Caissier |
|----------|-------|--------|----------|
| Login (matricule / MDP) | ✅ | email / téléphone | email / téléphone |
| Dashboard soi | ✅ | enfants | guichet |
| Devoirs de sa classe | ✅ | enfants | ❌ |
| Notes / bulletin (soi) | ✅ | enfants | ❌ |
| Voir solde frais (soi) | ✅ | enfants | école |
| PayIn Moko | ✅ **soi uniquement** | enfants | guichet |
| Créer paiement manuel | ❌ | ❌ | ✅ |
| Config wallet école | ❌ | ❌ | ❌ (Admin/Financier) |

**Règle d’or :** dès qu’un `idEleve` est dans l’URL ou le body, il **doit** être égal au claim JWT `EleveId`. Sinon → **403**.

---

## 2. Parcours utilisateur

```
Login (matricule + MDP)
    │
    ├─ doitChangerMotDePasse == true
    │       └─► POST /api/Utilisateur/changer_mot_de_passe
    │
    ▼
Home ──► GET /api/Dashboard/eleve
    │
    ├── Devoirs ──► GET /api/DevoirADomicile/eleve/moi
    │                 └─ télécharger ──► GET /api/DevoirADomicile/{id}/telecharger
    │
    ├── Notes ──► GET /api/Note/eleve/{idEleve}
    │   Bulletin ──► GET /api/Bulletin/eleve/{idEleve}?periode=...
    │                 └─ PDF ──► GET /api/Bulletin/eleve/{idEleve}/pdf
    │
    └── Frais / payer
            ├── Solde ──► GET /api/VuePaiementsFraisParEcole/eleve/{idEleve}
            ├── Historique ──► GET /api/Paiement/eleve/{idEleve}
            └── PayIn ──► POST /api/MokoAfrika/payin/frais-scolaire
                            └─ polling ──► POST /api/MokoAfrika/status/{ref}/check
```

---

## 3. Authentification

### Login

```
POST /api/Utilisateur/authentifier
Content-Type: application/json
```

```json
{
  "emailOuTelephone": "EKSMB25MAK",
  "motDePasse": "123456",
  "deviceType": "mobile",
  "deviceModel": "Flutter"
}
```

| Champ | Élève |
|-------|--------|
| `emailOuTelephone` | **Matricule** (= `defaultUsername`) |
| `motDePasse` | Initialement `123456` (comptes créés à l’inscription / backfill) |

### Réponse (champs utiles)

| Champ JSON | Usage front |
|------------|-------------|
| `accessToken` | Bearer |
| `refreshToken` | Renouvellement |
| `expiresIn` / `expiresAt` | TTL |
| `doitChangerMotDePasse` | Forcer l’écran changement MDP |
| `nomRole` / `primaryRole` / `roles` | Doit contenir `Eleve` |
| `permissions` | Liste string pour UI gating |
| `utilisateur.idEleve` | ID fiche élève (**obligatoire** pour le reste) |
| `utilisateur.idEcole` | École du compte |
| `utilisateur.defaultUsername` | Matricule |
| `nomEcole` | Affichage |

Stocker dès le login : `accessToken`, `refreshToken`, `idEleve`, `idEcole`, `doitChangerMotDePasse`, `permissions`.

### Changement de mot de passe

```
POST /api/Utilisateur/changer_mot_de_passe
Authorization: Bearer {token}
```

Si `doitChangerMotDePasse == true`, **bloquer** la navigation métier jusqu’au succès de cet appel (le login n’est pas refusé côté API, c’est au front de forcer le flux).

---

## 4. JWT et identité

| Claim | Clé | Usage |
|-------|-----|--------|
| Id élève | **`EleveId`** | Source de vérité pour tous les appels « soi » |
| École | **`idEcole`** | Contexte école |
| Rôle | `role` / ClaimTypes.Role = **`Eleve`** | `[Authorize(Roles = "Eleve")]` |
| Rôles multi | `roles`, `primaryRole`, `idRole` | Affichage / debug |

**Flutter (exemple) :** décoder le JWT et exposer `eleveId` dans un `Provider` / service auth.

**Vue :** même principe (store Pinia / composable `useAuth`).

> Ne jamais laisser l’utilisateur saisir un autre `idEleve`. Toujours prendre la valeur du JWT (ou `utilisateur.idEleve` du login).

---

## 5. Permissions

Permissions typiques après seed / scripts
[`docs/sql/20260915_AlignPermissionsRoleEleve.sql`](docs/sql/20260915_AlignPermissionsRoleEleve.sql) et
[`docs/sql/20260917_AlignPermissionsRoleEleve_PayOwn_Notification.sql`](docs/sql/20260917_AlignPermissionsRoleEleve_PayOwn_Notification.sql) :

| Permission | Usage UI |
|------------|----------|
| `Eleve.ReadOwn` | Profil / soi |
| `Note.ReadOwn` | Notes (soi) |
| `Bulletin.ReadOwn` | Bulletins JSON / PDF (soi) |
| `Paiement.ReadOwn` | Historique paiements |
| `Paiement.PayOwn` | Bouton payer (Moko PayIn frais scolaire) |
| `Frais.ReadOwn` | Situation frais / solde |
| `Notification.ReadOwn` | Inbox / détail notifications |
| `Notification.UpdateOwn` | Marquer comme lue(s) |
| `DevoirADomicile.Read` | Liste devoirs |
| `DevoirADomicile.Download` | Téléchargement PDF |

**Absent (ne pas exposer dans l’UI) :** `Note.Read` (classe), `Paiement.Create` (cash guichet), CRUD frais, config Moko école, bulletins de classe, `Notification` broadcast (`GetAll` / école / create).

Les routes dashboard / devoirs `eleve/moi` s’appuient surtout sur **Roles = Eleve**, pas sur `[Permission]`. Les permissions servent au **gating UI**, notes/bulletins, PayIn et inbox notifications.

---

## 6. Dashboard

```
GET /api/Dashboard/eleve?idEcole={optionnel}&libelleAnneeScolaire={optionnel}
Authorization: Bearer {token}
```

| Auth | Roles = `Eleve` uniquement |
|------|----------------------------|
| Scope | `EleveId` lu **depuis le JWT** (pas de `idEleve` en query) |

### Réponse (`DashboardEleveDto`)

```json
{
  "ecole": { "idEcole": 1, "nom": "..." },
  "idAnneeScolaire": 3,
  "libelleAnneeScolaire": "2025-2026",
  "periode": { },
  "profil": {
    "idEleve": 1,
    "matricule": "EKSMB25MAK",
    "nomComplet": "...",
    "idEcole": 1,
    "nomEcole": "...",
    "idClasse": 12,
    "nomClasse": "6ème A",
    "statutInscription": "Confirmé"
  },
  "resume": {
    "nombrePaiements": 2,
    "montantPaye": 150000,
    "alertePaiement": false,
    "messagePaiement": "Paiements enregistrés"
  },
  "alertes": []
}
```

Écran d’accueil recommandé : carte profil + résumé paiements + liens Devoirs / Notes / Frais.

---

## 7. Devoirs à domicile

### Liste (recommandé)

```
GET /api/DevoirADomicile/eleve/moi?pageNumber=1&pageSize=15&idAnneeScolaire=
```

| Auth | Roles = `Eleve` |
|------|-----------------|
| Scope | Classe déduite de l’inscription active du `EleveId` JWT |

Réponse paginée encapsulée (`ElevesAnneeScopedResult`) avec `data`, `idEcole`, `idAnneeScolaire`.

Chaque élément de `data` expose notamment :

| Champ | Sens |
|-------|------|
| `nombreTelechargements` | **Total global** de téléchargements du devoir (tous utilisateurs) |
| `estTelechargeParMoi` | `true` si **le compte JWT** a déjà téléchargé ce devoir au moins une fois |

### Téléchargement

```
GET /api/DevoirADomicile/{id}/telecharger
```

Contrôle métier d’accès au devoir (classe de l’élève). Afficher un bouton seulement si `permissions` contient `DevoirADomicile.Download` (signal UI).

Après un téléchargement réussi : `estTelechargeParMoi` passe à `true` au prochain refresh de liste ; `nombreTelechargements` est incrémenté à chaque download (même re-téléchargement).

**SQL suivi (prod)** : [`docs/sql/20260917_AddDevoirsADomicileTelechargements.sql`](docs/sql/20260917_AddDevoirsADomicileTelechargements.sql)

**À éviter :** `GET /api/DevoirADomicile/tuteur/{id}` (parcours Parent).

---

## 8. Notes et bulletins

Toujours passer **`idEleve` = JWT `EleveId`** (Eleve) ou **un enfant du tuteur** (Parent, JWT `IdTuteur`).

| Méthode | Route | Permission API |
|---------|-------|----------------|
| `GET` | `/api/Note/eleve/{idEleve}` | `Note.Read` **ou** `Note.ReadOwn` **ou** `Note.ReadChildren` |
| `GET` | `/api/PeriodeCotation` | Catalogue T1/T2/T3 (picker UI) |
| `GET` | `/api/Bulletin/eleve/{idEleve}?idPeriode=` **ou** `?periode=` | `Note.Read*` **ou** `Bulletin.Read*` |
| `GET` | `/api/Bulletin/eleve/{idEleve}/pdf?idPeriode=` **ou** `?periode=` | Idem (PDF) |

| Acteur | Scope |
|--------|--------|
| **Eleve** | `idEleve` = claim `EleveId` uniquement |
| **Parent** | `idEleve` doit être un enfant lié (`Eleves.IdTuteur` = JWT `IdTuteur`) ; multi-écoles OK via `ForbidIfWrongSchoolAsync` |
| **École** (`Note.Read`) | Accès lecture selon école JWT |

| Paramètre | Requis | Exemple |
|-----------|--------|---------|
| `idPeriode` | Recommandé | `1` (via `GET /api/PeriodeCotation`) |
| `periode` | Alternatif | `T1`, `Trimestre 1`, `1er Trimestre` (alias acceptés) |

Fournir **au moins** `idPeriode` **ou** `periode`. Préférer `idPeriode` pour éviter les ambiguïtés de libellé.

**Inaccessible pour Eleve / Parent :** `GET /api/Bulletin/classe/{idClasse}`, listes notes par évaluation / classe (`Note.Read` seul pour la classe).

---

## 9. Frais, solde et paiements

### Solde / situation (recommandé)

```
GET /api/VuePaiementsFraisParEcole/eleve/{idEleve}?idAnneeScolaire=
```

Alternatives (même scope own) :

```
GET /api/VuePaiementsFraisParEcole/eleve-matricule/{matricule}
GET /api/VuePaiementsFraisParEcole/eleve-reference/{referenceEleve}
```

### Historique paiements

```
GET /api/Paiement/eleve/{idEleve}
GET /api/Paiement/eleve/{idEleve}/paged?pageNumber=1&pageSize=20
GET /api/Paiement/eleves/{idEleve}/taux
```

### Catalogue barèmes (optionnel)

Les GET `/api/Frais/ecole/{idEcole}` sont publics / anonymes côté API ; pour l’élève, préférer la **vue solde** (frais applicables + reste à payer).

---

## 10. Payer un frais (PayIn Moko)

Même endpoint que Parent / guichet. Détail complet : [DOCUMENTATION_FRONTEND_PAIEMENT_MOKO.md](DOCUMENTATION_FRONTEND_PAIEMENT_MOKO.md).

```
POST /api/MokoAfrika/payin/frais-scolaire
Authorization: Bearer {token}
```

```json
{
  "idEleve": 1,
  "idFrais": 42,
  "montantNet": 50000,
  "method": "airtel",
  "telephonePayeur": "2438XXXXXXXX",
  "devise": "CDF"
}
```

| Champ | Règle Eleve |
|-------|-------------|
| `idEleve` | **Obligatoire = JWT `EleveId`** sinon 403 |
| `idFrais` | Frais éligible à sa classe / année |
| `method` | `airtel`, `orange`, `mpesa`, `card`, … |
| `telephonePayeur` | Numéro Mobile Money du payeur (souvent le tuteur / l’élève) |

### Estimation frais Moko (optionnel)

```
GET /api/MokoAfrika/fees/estimate?amount=50000&method=airtel&currency=CDF
```

(public)

### Suivi USSD

```
POST /api/MokoAfrika/status/{reference}/check
```

Polling recommandé jusqu’à succès / échec / timeout (~120 s), en complément des events SignalR si branchés.

---

## 11. Profil

| Source | Route | Remarque |
|--------|-------|----------|
| **Recommandé** | `GET /api/Dashboard/eleve` → `profil` | Scope JWT natif |
| Alternatif | `GET /api/Eleve/{id}` | JWT seul — **forcer** `id === EleveId` côté front (pas de `ForbidIfWrongEleve` aujourd’hui sur ce GET) |

Pas d’endpoint `/api/Eleve/moi` dédié.

---

## 12. Erreurs et pièges

| Situation | HTTP | Message / cause |
|-----------|------|-----------------|
| Autre `idEleve` que le JWT | **403** | `ForbidIfWrongEleve` |
| Token sans `EleveId` | **400** / **403** | Dashboard / devoirs « moi » |
| `Note.Read` seul (routes classe) | **403** | Eleve n’a que `Note.ReadOwn` ; Parent n’a que `Note.ReadChildren` |
| Parent sans `IdTuteur` / autre enfant | **403** | `ForbidIfWrongChildAsync` |
| PayIn autre élève | **403** | Idem scope |
| Compte sans inscription confirmée | **400** | Devoirs / frais liés à l’inscription |
| MDP initial | — | `123456` + `doitChangerMotDePasse` |

### Flutter / Vue — bonnes pratiques

1. Intercepteur HTTP : attacher le Bearer ; sur 401 → refresh puis retry.
2. Sur 403 « propres données » → toast clair, ne pas retry avec un autre id.
3. Feature flags UI : masquer les écrans selon `permissions` + `nomRole === "Eleve"`.
4. Ne jamais hardcoder un `idEleve` de test en prod.

---

## 13. Checklist Flutter / Vue

- [ ] Login par **matricule** + stockage `idEleve` / `EleveId` JWT
- [ ] Flux **changement MDP** si `doitChangerMotDePasse`
- [ ] Home : `GET /api/Dashboard/eleve`
- [ ] Devoirs : `GET .../DevoirADomicile/eleve/moi` + téléchargement
- [ ] Notes : `GET /api/Note/eleve/{idEleve}` avec id JWT
- [ ] Bulletin : JSON + PDF avec `periode`
- [ ] Solde : `VuePaiementsFraisParEcole/eleve/{idEleve}`
- [ ] PayIn : body `idEleve` = JWT + polling statut
- [ ] Aucun écran admin / guichet / wallet école
- [ ] Tests manuels : autre `idEleve` → 403 ; sans `EleveId` → erreur explicite

---

## Références code API

| Zone | Fichiers |
|------|----------|
| Auth / JWT | `Controllers/UtilisateurController.cs`, `Services/SimpleJwtService.cs` |
| Dashboard | `Controllers/DashboardController.cs`, `Services/DashboardEleveService.cs` |
| Devoirs | `Controllers/DevoirADomicileController.cs` |
| Notes / bulletins | `Controllers/NoteController.cs`, `Controllers/BulletinController.cs` |
| Solde / paiements | `Controllers/VuePaiementsFraisParEcoleController.cs`, `Controllers/PaiementController.cs` |
| PayIn | `Controllers/MokoAfrikaController.cs`, `UserRoles.CashierPayInRoles` |
| Scope own | `Helpers/AuditHelpers.cs` (`ForbidIfWrongEleve*`) |
| Permissions | `Data/PermissionSeeder.cs` (`EnsureElevePermissionsAsync`) |
