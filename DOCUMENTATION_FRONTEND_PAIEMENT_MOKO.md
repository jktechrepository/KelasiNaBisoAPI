# Documentation Frontend — Paiement électronique MOKO Afrika (intégration complète)

Guide pour **Parent**, **personnel école** (Admin, Directeur, Financier,Caissier) et **Super-Admin**.

**Base URL dev :** `https://dev-knb.asdc-rdc.org`  
**Swagger :** [https://dev-knb.asdc-rdc.org/swagger/index.html](https://dev-knb.asdc-rdc.org/swagger/index.html)  
**Auth :** `Authorization: Bearer {jwt_token}` (sauf endpoints publics)

---

## Table des matières

1. [Vue d'ensemble métier](#1-vue-densemble-métier)
2. [Prérequis serveur — cause des erreurs 500](#2-prérequis-serveur--cause-des-erreurs-500)
3. [Acteurs et rôles](#3-acteurs-et-rôles)
4. [Architecture du flux](#4-architecture-du-flux)
5. [Statuts et opérateurs](#5-statuts-et-opérateurs)
6. [Endpoints — paiement (Parent + École)](#6-endpoints--paiement-parent--école)
7. [Endpoints — configuration école / wallet](#7-endpoints--configuration-école--wallet)
8. [Endpoints — Super-Admin PayOut retry](#8-endpoints--super-admin-payout-retry)
9. [Modèles TypeScript](#9-modèles-typescript)
10. [Écrans frontend à implémenter](#10-écrans-frontend-à-implémenter)
11. [Intégration Vue.js (exemples)](#11-intégration-vuejs-exemples)
12. [Polling USSD](#12-polling-ussd)
13. [Gestion des erreurs](#13-gestion-des-erreurs)
14. [Checklist](#14-checklist)

---

## 1. Vue d'ensemble métier

### Qui peut payer électroniquement ?

| Acteur | Rôle JWT | Peut lancer un PayIn ? | Contexte |
|--------|----------|------------------------|----------|
| **Parent** | `Parent` | Oui | Paie les frais de **son** enfant depuis l'app mobile |
| **Personnel école (guichet)** | `Admin`, `Financier`, **`Caissier`** | Oui | Encaisse au guichet : le parent est présent, saisie du **téléphone du payeur** (Mobile Money) |
| **Directeur** | `Directeur` | Non | Supervision guichet (lecture dashboard, journal) — pas d'encaissement |
| **Super-Admin** | `Super-Admin` | Oui | Idem personnel école + relance PayOut échoué |

**Règle clé :** Parent **et** école utilisent le **même endpoint** `POST /api/MokoAfrika/payin/frais-scolaire`. Seul le rôle JWT change.

### Wallet virtuel par école

Chaque école possède **1 site MOKO** = **1 wallet virtuel** :

| Solde | Signification |
|-------|---------------|
| `soldeEnAttente` | PayIn confirmés, délai MOKO pas encore écoulé |
| `soldeDisponible` | Montant prêt pour reversement (PayOut) |
| `totalRecu` | Cumul PayIn nets reçus |
| `totalReverse` | Cumul PayOut versés au bénéficiaire école |

### PayOut automatique vs relance Super-Admin

1. PayIn réussi → crédit wallet école → **PayOut automatique** planifié (si `payoutAutomatique = true`)
2. PayOut échoue → montant **recrédité** sur `soldeDisponible`
3. **Seul le Super-Admin** relance manuellement : `POST /api/MokoAfrika/payout/retry?payInReference=...`

### Ce qu'il ne faut plus faire

Ne plus appeler `POST /api/Paiement` pour **Mobile Money** ou **Carte**. Réserver ce endpoint à **Cash**, **Virement**, **Chèque** (saisie manuelle).

---

## 2. Prérequis serveur — cause des erreurs 500

### Symptômes observés

```
GET /api/Ecole/13/paiement-mobile          → 500
GET /api/Dashboard/global?idEcole=13       → 500
```

### Cause principale

La **migration MOKO** n'a pas été appliquée sur la base `dev-knb_db`. L'API tente d'accéder à :

- Tables : `EcolesInfoPaiementMobile`, `EcolesWallets`, `TransactionsMoko`, `FilePayoutsMoko`…
- Colonnes sur `Paiements` : `MontantNet`, `MontantCollecte`, `OperateurMobileMoney`

Sans ces objets SQL → **Internal Server Error 500** sur tout endpoint touchant paiements ou dashboard.

### Correction (obligatoire avant intégration frontend)

Sur le serveur MySQL :

```bash
mysql -u kansa -p dev-knb_db < scripts/migration-moko-afrika-manual.sql
```

Puis redémarrer l'API (recycler le pool IIS).

### Réponse API si migration toujours absente

Après mise à jour API, `GET /api/Ecole/{id}/paiement-mobile` retourne **503** avec :

```json
{
  "code": "MOKO_MIGRATION_REQUIRED",
  "message": "Les tables MOKO Afrika ne sont pas encore créées en base...",
  "detail": "Table 'dev-knb_db.EcolesInfoPaiementMobile' doesn't exist"
}
```

**Action frontend :** afficher un bandeau « Migration base requise — contacter l'administrateur » au lieu d'une erreur générique.

### École non configurée (migration OK)

`GET /api/Ecole/{id}/paiement-mobile` retourne **200** :

```json
{
  "idEcole": 13,
  "estConfigure": false,
  "configuration": null,
  "stats": {
    "soldeEnAttente": 0,
    "soldeDisponible": 0,
    "payInsReussis": 0,
    "payInsEnAttente": 0,
    "payInsEchoues": 0,
    "payOutsReussis": 0,
    "payOutsEnAttente": 0,
    "payOutsEchoues": 0,
    "devise": "CDF"
  }
}
```

→ Afficher l'écran **« Initialiser la configuration »** (wizard), pas une page d'erreur.

---

## 3. Acteurs et rôles

| Endpoint | Parent | Admin | Directeur | Financier | **Caissier** | Super-Admin |
|----------|--------|-------|-----------|-----------|--------------|-------------|
| `GET /api/Ecole/{id}/paiement-mobile` | — | ✅ | ✅ | ✅ | **✅** | ✅ |
| `POST /api/Ecole/{id}/paiement-mobile` (config) | — | ✅ | ✅ | — | — | ✅ |
| `POST .../beneficiaires` | — | ✅ | ✅ | — | — | ✅ |
| `GET .../transactions` | — | ✅ | ✅ | ✅ | — | ✅ |
| `GET .../wallet/mouvements` | — | ✅ | ✅ | ✅ | — | ✅ |
| `GET .../payouts` | — | ✅ | ✅ | ✅ | — | ✅ |
| `POST /api/MokoAfrika/payin/frais-scolaire` | ✅ | ✅ | — | ✅ | **✅** | ✅ |
| `GET /api/Dashboard/caissier` | — | ✅ | ✅ | ✅ | **✅** | ✅ |
| `GET /api/Dashboard/caissier/cloture` | — | ✅ | ✅ | ✅ | **✅** | ✅ |
| `GET /api/MokoAfrika/fees/estimate` | ✅ (public) | ✅ | ✅ | ✅ | ✅ | ✅ |
| `POST /api/MokoAfrika/payout/retry` | — | — | — | — | **✅ seul** |

**Contrôle d'accès école :** le frontend doit utiliser `idEcole` de l'utilisateur connecté (JWT / profil), pas un ID arbitraire.

### Dashboard guichet Caissier

**Route :** `GET /api/Dashboard/caissier?idEcole={idEcole}&idAnneeScolaire={optionnel}&date={optionnel}&scope=moi|ecole`

- Réservé aux rôles guichet : `Caissier`, `Financier`, `Directeur`, `Admin`, `Super-Admin`
- **`scope=moi`** (défaut) : encaissements du caissier connecté (`IdUtilisateur` JWT)
- **`scope=ecole`** : toute la caisse du jour — **Directeur / Financier / Admin / Super-Admin uniquement** ; ignoré pour un Caissier seul
- **Clôture :** `GET /api/Dashboard/caissier/cloture?...` — même paramètres + liste complète `tousLesPaiements`
- **Ne pas utiliser** `GET /api/Dashboard/global` pour l'écran Caissier (présence, KPI direction, wallet)
- **Doc guichet complète :** [DOCUMENTATION_FRONTEND_ROLE_CAISSIER.md](DOCUMENTATION_FRONTEND_ROLE_CAISSIER.md)

**Payload principal :**

| Bloc | Usage front |
|------|-------------|
| `resume` | Compteurs du jour (nombre, montant, PayIn Moko réussis/en attente/échoués) |
| `repartitionParMode` | Camembert espèces / chèque / Mobile Money |
| `derniersPaiements` | Journal du jour (15 max) |
| `moko.estConfigure` | Afficher wizard si `false` |
| `moko.payInsEnAttente` | Liste des refs à poller via `POST .../status/{ref}/check` |

**Exemple TypeScript :**

```typescript
interface DashboardCaissierDto {
  ecole: { idEcole: number; nomEcole: string; logo?: string };
  idAnneeScolaire: number;
  libelleAnneeScolaire?: string;
  periode: { type: string; date?: string; libelle: string };
  resume: {
    nombrePaiements: number;
    montantTotal: number;
    payInsReussis: number;
    payInsEnAttente: number;
    payInsEchoues: number;
  };
  repartitionParMode: { modes: Record<string, { nombre: number; montant: number; pourcentage: number }> };
  derniersPaiements: Array<{
    idPaiement: number;
    datePaiement: string;
    montant: number;
    modePaiement: string;
    statut: string;
    nomEleve?: string;
    libelleFrais?: string;
    referenceMoko?: string;
  }>;
  moko: {
    estConfigure: boolean;
    mesPayInsEnAttente: number;
    payInsEnAttente: Array<{
      reference: string;
      idPaiement?: number;
      montant: number;
      nomEleve?: string;
      libelleFrais?: string;
      dateCreation: string;
    }>;
  };
}
```

---

## 4. Architecture du flux

```
┌─────────────┐     ┌─────────────┐
│   Parent    │     │  École      │
│  (mobile)   │     │ (guichet)   │
└──────┬──────┘     └──────┬──────┘
       │                   │
       └─────────┬─────────┘
                 ▼
    GET /api/MokoAfrika/fees/estimate
                 ▼
    POST /api/MokoAfrika/payin/frais-scolaire
                 ▼
         Paiement "En attente"
         Transaction MOKO "pending"
                 ▼
    USSD sur téléphone payeur (243...)
                 ▼
    POST /api/MokoAfrika/status/{ref}/check  (polling)
    ou webhook POST /api/MokoAfrika/callback
                 ▼
         PayIn "success"
         Paiement "Confirme"
         Wallet école crédité
         Notification push tuteur
                 ▼
    PayOut auto (délai configurable, ex. 3 min)
                 ▼
    Reversement vers numéro bénéficiaire école
```

---

## 5. Statuts et opérateurs

### Statuts paiement (`statutPaiement`)

| Valeur | UI |
|--------|-----|
| `En attente` | ⏳ En cours — validez sur votre téléphone |
| `Confirme` | ✅ Payé |
| `Echoue` | ❌ Échec |

**Sans accent** — cohérence notifications push existantes.

### Statuts transaction MOKO (`status`)

| Valeur | `isDefinitive` | Signification |
|--------|----------------|---------------|
| `pending` | `false` | Gateway / USSD en cours — **continuer** le poll |
| `success` | `true` | Succès — arrêter |
| `error` | `true` | Échec définitif — arrêter ; lire `statusDescription` |
| `timeout` | `true` | Timeout USSD — arrêter |

Champs utiles sur `POST .../status/{ref}/check` : `statusDescription`, `isDefinitive`.

### Actions transaction (`action`)

| Valeur | Type |
|--------|------|
| `debit` | PayIn (collecte parent → plateforme) |
| `credit` | PayOut (reversement → école) |
| `check` | Vérification statut |

### Opérateurs (`method`)

| Code | Libellé |
|------|---------|
| `airtel` | Airtel Money |
| `orange` | Orange Money |
| `mpesa` | M-Pesa |
| `africell` | Africell |
| `card` | Carte Visa/Mastercard |

Téléphone : `243XXXXXXXXX` ou `0XXXXXXXXX`.

---

## 6. Endpoints — paiement (Parent + École)

### 6.1 Estimer les frais (public)

```http
GET /api/MokoAfrika/fees/estimate?amount=50000&method=airtel&currency=CDF
```

**Réponse 200 :**

```json
{
  "montantNet": 50000,
  "fraisCollecte": 1250,
  "fraisDecaissement": 500,
  "montantCollecte": 51250,
  "devise": "CDF",
  "method": "airtel"
}
```

Afficher clairement : **« Vous allez payer 51 250 CDF (dont 50 000 frais scolaires) »**.

### 6.2 Lancer un PayIn (Parent OU École)

```http
POST /api/MokoAfrika/payin/frais-scolaire
Authorization: Bearer {token}
Content-Type: application/json
```

**Body :**

```json
{
  "idEleve": 42,
  "idFrais": 7,
  "montantNet": 50000,
  "method": "airtel",
  "telephonePayeur": "243970000000",
  "commentaire": "Paiement frais inscription — guichet"
}
```

| Champ | Obligatoire | Notes |
|-------|-------------|-------|
| `idEleve` | Oui | Élève concerné |
| `idFrais` | Oui | Doit appartenir à l'école de l'élève |
| `montantNet` | Non | Défaut = montant du frais |
| `method` | Oui | Opérateur ou `card` |
| `telephonePayeur` | Oui | **Téléphone qui recevra l'USSD** (parent ou payeur présent au guichet) |
| `commentaire` | Non | Recommandé pour paiements guichet |

**Réponse 200 (en attente USSD) :**

```json
{
  "idPaiement": 891,
  "idEcole": 13,
  "reference": "MOKO_20260704143022_7841",
  "statutPaiement": "En attente",
  "statutGateway": "pending",
  "montantNet": 50000,
  "montantCollecte": 51250,
  "requiresUssdConfirmation": true,
  "message": "Validez le paiement sur votre téléphone."
}
```

**Erreurs fréquentes 400 :**

- « Paiement mobile non configuré pour l'école X » → configurer l'école d'abord
- « Un paiement est déjà en attente pour ce frais et cet élève »
- « Paiement Mobile Money désactivé pour cette école »

### 6.3 Vérifier le statut (polling)

```http
POST /api/MokoAfrika/status/{reference}/check
Authorization: Bearer {token}
```

```http
GET /api/MokoAfrika/status/{reference}
Authorization: Bearer {token}
```

---

## 7. Endpoints — configuration école / wallet

Base : `/api/Ecole/{idEcole}/paiement-mobile`

### 7.1 Vue d'ensemble (écran principal Manage.vue)

```http
GET /api/Ecole/13/paiement-mobile
```

**Réponse configurée :**

```json
{
  "idEcole": 13,
  "estConfigure": true,
  "configuration": {
    "idEcoleInfoPaiementMobile": 1,
    "idEcole": 13,
    "mobileMoneyActif": true,
    "carteActif": false,
    "devise": "CDF",
    "delaiReglementMinutes": 3,
    "payoutAutomatique": true,
    "statut": true,
    "wallet": {
      "idEcoleWallet": 1,
      "idEcole": 13,
      "devise": "CDF",
      "soldeEnAttente": 150000,
      "soldeDisponible": 320000,
      "totalRecu": 2500000,
      "totalReverse": 2030000
    },
    "beneficiaires": [
      {
        "idEcoleBeneficiaireMomo": 1,
        "idEcole": 13,
        "methode": "airtel",
        "numero": "243970000000",
        "nomTitulaire": "Ecole ABC",
        "estPrincipal": true,
        "statut": "ACTIF"
      }
    ]
  },
  "stats": {
    "soldeEnAttente": 150000,
    "soldeDisponible": 320000,
    "totalRecu": 2500000,
    "totalReverse": 2030000,
    "devise": "CDF",
    "payInsReussis": 45,
    "payInsEnAttente": 2,
    "payInsEchoues": 3,
    "payOutsReussis": 42,
    "payOutsEnAttente": 1,
    "payOutsEchoues": 2
  }
}
```

### 7.2 Initialiser / mettre à jour la configuration

**Première configuration** (quand `estConfigure === false`) :

```http
POST /api/Ecole/13/paiement-mobile
Authorization: Bearer {token}
Content-Type: application/json

{
  "mobileMoneyActif": true,
  "carteActif": false,
  "devise": "CDF",
  "delaiReglementMinutes": 3,
  "payoutAutomatique": true
}
```

Crée automatiquement le **wallet** associé.

**Mise à jour partielle :**

```http
PUT /api/Ecole/13/paiement-mobile

{
  "mobileMoneyActif": true,
  "payoutAutomatique": true,
  "statut": true
}
```

### 7.3 Bénéficiaires Mobile Money (numéros de reversement)

```http
POST /api/Ecole/13/paiement-mobile/beneficiaires

{
  "methode": "airtel",
  "numero": "243970000000",
  "nomTitulaire": "Complexe Scolaire XYZ",
  "estPrincipal": true
}
```

Un bénéficiaire **principal** par opérateur. PayOut utilise ce numéro.

```http
PUT /api/Ecole/13/paiement-mobile/beneficiaires/{id}
DELETE /api/Ecole/13/paiement-mobile/beneficiaires/{id}
```

### 7.4 Liste des transactions MOKO

```http
GET /api/Ecole/13/paiement-mobile/transactions?status=success&action=debit&limit=50&offset=0
```

**Filtres query :**

| Param | Valeurs | Usage |
|-------|---------|-------|
| `status` | `pending`, `success`, `error`, `timeout` | Onglets Réussies / En attente / Échouées |
| `action` | `debit` (PayIn), `credit` (PayOut) | Filtrer collectes vs reversements |
| `limit` | 1–200 (défaut 50) | Pagination |
| `offset` | 0+ | Pagination |

**Réponse (extrait) :**

```json
[
  {
    "idTransactionMoko": 12,
    "reference": "MOKO_20260704143022_7841",
    "idPaiement": 891,
    "idEcole": 13,
    "action": "debit",
    "amount": 51250,
    "amountNet": 50000,
    "devise": "CDF",
    "method": "airtel",
    "status": "success",
    "customerPhone": "243970000000",
    "statutPaiement": "Confirme",
    "nomEleve": "KABONGO Jean",
    "libelleFrais": "Frais inscription",
    "dateCreation": "2026-07-04T14:30:22"
  }
]
```

### 7.5 Mouvements wallet (journal comptable)

```http
GET /api/Ecole/13/paiement-mobile/wallet/mouvements?limit=50
```

Types de mouvements :

| `typeMouvement` | Description |
|-----------------|-------------|
| `PAYIN_CREDIT_PENDING` | Crédit en attente après PayIn |
| `PAYIN_RELEASE_AVAILABLE` | Libération vers solde disponible |
| `PAYOUT_DEBIT` | Débit PayOut vers bénéficiaire |
| `PAYOUT_REVERSAL` | Recrédit si PayOut échoué |

### 7.6 File PayOut (reversements)

```http
GET /api/Ecole/13/paiement-mobile/payouts?status=failed
```

Statuts file : `pending`, `processing`, `success`, `failed`, `cancelled`.

Afficher les PayOut échoués à l'école en **lecture seule**. Bouton « Relancer » visible **uniquement Super-Admin**.

---

## 8. Endpoints — Super-Admin PayOut retry

```http
POST /api/MokoAfrika/payout/retry?payInReference=MOKO_20260704143022_7841
Authorization: Bearer {token_super_admin}
```

**Réponse :**

```json
{
  "payInReference": "MOKO_20260704143022_7841",
  "payOutReference": "MOKO_20260704150000_9922",
  "status": "pending",
  "message": "PayOut relancé."
}
```

---

## 9. Modèles TypeScript

```typescript
export interface EcolePaiementMobileOverview {
  idEcole: number;
  estConfigure: boolean;
  configuration: EcoleInfoPaiementMobile | null;
  stats: EcoleWalletStats;
}

export interface EcoleInfoPaiementMobile {
  idEcoleInfoPaiementMobile: number;
  idEcole: number;
  mobileMoneyActif: boolean;
  carteActif: boolean;
  devise: string;
  delaiReglementMinutes: number;
  payoutAutomatique: boolean;
  statut: boolean;
  wallet: EcoleWallet | null;
  beneficiaires: EcoleBeneficiaireMomo[];
}

export interface EcoleWallet {
  soldeEnAttente: number;
  soldeDisponible: number;
  totalRecu: number;
  totalReverse: number;
  devise: string;
}

export interface EcoleWalletStats extends EcoleWallet {
  payInsReussis: number;
  payInsEnAttente: number;
  payInsEchoues: number;
  payOutsReussis: number;
  payOutsEnAttente: number;
  payOutsEchoues: number;
}

export interface TransactionMokoListItem {
  idTransactionMoko: number;
  reference: string;
  action: 'debit' | 'credit' | 'check';
  amount: number;
  amountNet?: number;
  method?: string;
  status: 'pending' | 'success' | 'error' | 'timeout';
  statusDescription?: string;
  isDefinitive?: boolean;
  customerPhone?: string;
  statutPaiement?: string;
  nomEleve?: string;
  libelleFrais?: string;
  dateCreation: string;
}

export interface PayInFraisScolaireRequest {
  idEleve: number;
  idFrais: number;
  montantNet?: number;
  method: string;
  telephonePayeur: string;
  commentaire?: string;
}

export interface PayInFraisScolaireResult {
  idPaiement: number;
  idEcole: number;
  reference: string;
  statutPaiement: string;
  statutGateway: string;
  montantNet: number;
  montantCollecte: number;
  requiresUssdConfirmation: boolean;
  message?: string;
}
```

---

## 10. Écrans frontend à implémenter

### A. Parent (app mobile)

1. **Liste frais impayés** — exclure montants « En attente » du solde dû affiché comme payé
2. **Choix opérateur + téléphone**
3. **Récapitulatif frais MOKO** (`fees/estimate`)
4. **Confirmation USSD** + polling
5. **Historique paiements** — badge statut `En attente` / `Confirme` / `Echoue`

### B. École — Paiement mobile MOKO (`Manage.vue`)

1. **Bandeau migration** si HTTP 503 + `code === 'MOKO_MIGRATION_REQUIRED'`
2. **Wizard initialisation** si `estConfigure === false` :
   - Activer Mobile Money / Carte
   - Ajouter au moins 1 bénéficiaire par opérateur utilisé
3. **Dashboard wallet** :
   - Cartes : Solde en attente | Solde disponible | Total reçu | Total reversé
   - Compteurs : PayIns réussis / en attente / échoués
4. **Onglets transactions** :
   - Toutes | Réussies (`success`) | En attente (`pending`) | Échouées (`error`)
5. **Mouvements wallet** (journal)
6. **PayOuts échoués** (liste lecture seule pour école)

### C. École — Encaissement guichet

1. Recherche élève → sélection frais
2. Saisie téléphone payeur (Mobile Money du parent présent)
3. Même flux PayIn + polling que le parent
4. Commentaire « Guichet — {agent} »

### D. Super-Admin

1. Tout ce que voit l'école (par école)
2. **Bouton « Relancer PayOut »** sur PayOut `failed`
3. Configuration globale MOKO (`appsettings` — hors scope frontend)

---

## 11. Intégration Vue.js (exemples)

### Charger l'écran Manage.vue (corrige l'erreur 500 actuelle)

```javascript
// services/mokoPaiement.js
export async function loadPaiementMobileOverview(idEcole, token) {
  const res = await fetch(`/api/Ecole/${idEcole}/paiement-mobile`, {
    headers: { Authorization: `Bearer ${token}` }
  });

  if (res.status === 503) {
    const err = await res.json();
    if (err.code === 'MOKO_MIGRATION_REQUIRED') {
      return { migrationRequired: true, message: err.message };
    }
  }

  if (!res.ok) {
    throw new Error((await res.json()).message || 'Erreur serveur');
  }

  return await res.json();
}
```

```javascript
// Manage.vue — onMounted
const overview = await loadPaiementMobileOverview(ecoleId, token);

if (overview.migrationRequired) {
  showMigrationBanner.value = true;
  return;
}

if (!overview.estConfigure) {
  showSetupWizard.value = true;
} else {
  wallet.value = overview.configuration.wallet;
  stats.value = overview.stats;
}
```

### Initialiser une école

```javascript
async function initialiserConfig(idEcole, token) {
  await fetch(`/api/Ecole/${idEcole}/paiement-mobile`, {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      mobileMoneyActif: true,
      carteActif: false,
      devise: 'CDF',
      delaiReglementMinutes: 3,
      payoutAutomatique: true
    })
  });
}
```

### Paiement guichet (personnel école)

```javascript
async function payerAuGuichet({ idEleve, idFrais, method, telephonePayeur }, token) {
  const res = await fetch('/api/MokoAfrika/payin/frais-scolaire', {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      idEleve,
      idFrais,
      method,
      telephonePayeur,
      commentaire: 'Paiement guichet école'
    })
  });
  return res.json();
}
```

### Dashboard global (erreur séparée)

```javascript
// GET /api/Dashboard/global?idEcole=13
// Échoue aussi si migration Paiements non appliquée.
// Après migration SQL, recharger la page.
// Utiliser Promise.allSettled pour ne pas bloquer tout l'écran si dashboard échoue :
const [overviewResult, dashboardResult] = await Promise.allSettled([
  loadPaiementMobileOverview(ecoleId, token),
  fetch(`/api/Dashboard/global?idEcole=${ecoleId}`, { headers: { Authorization: `Bearer ${token}` } })
]);
```

---

## 12. SignalR (guichet / dashboard école)

Hub : `wss://{api}/hubs/dashboard` (JWT requis). À la connexion, le client rejoint le groupe `ecole_{idEcole}`.

| Événement | Quand | `eventType` |
|-----------|-------|-------------|
| `DashboardUpdateNotification` | PayIn initié ou confirmé | `payin_pending` / `payin_confirmed` |
| `PayInStatusUpdated` | Idem (payload enrichi) | `payin_pending` / `payin_confirmed` |

Payload commun :

```json
{
  "ecoleId": 1,
  "dashboardType": "paiement",
  "eventType": "payin_pending",
  "reference": "MOKO_...",
  "idPaiement": 42,
  "idEleve": 10,
  "montantNet": 50000,
  "statutPaiement": "En attente",
  "statutGateway": "pending",
  "timestamp": "2026-09-02T11:00:00Z"
}
```

**Recommandation front :** écouter `PayInStatusUpdated` pour rafraîchir le guichet sans polling agressif ; conserver le polling `status/check` comme filet de sécurité (timeout 120 s).

```javascript
connection.on('PayInStatusUpdated', (payload) => {
  if (payload.eventType === 'payin_pending') {
    ajouterPayInEnAttente(payload);
  }
  if (payload.eventType === 'payin_confirmed') {
    retirerPayInEnAttente(payload.reference);
    refreshDashboardCaissier();
  }
});
```

---

## 13. Polling USSD

### Règles obligatoires (Flutter / Vue)

1. **Ne pas** utiliser `GET /api/MokoAfrika/status/{ref}` pour le polling USSD — lecture **DB seule** (ne rafraîchit pas la gateway).
2. Poller uniquement : **`POST /api/MokoAfrika/status/{reference}/check`**
3. **Délai avant le 1er check** : **3–5 secondes** (jamais immédiat après l’initiation)
4. Intervalle : **5–10 secondes** ; durée max : **120 secondes**
5. Arrêter seulement si `isDefinitive === true` **et** (`success` / `error` / `timeout`) **après** un `POST .../check`, ou via SignalR `PayInStatusUpdated`
6. **Devise** : le backend utilise la devise configurée de l’école (souvent `CDF`). Ne pas poster `USD` si l’école est en `CDF` — sinon `montantNet: 10` est traité comme **10 CDF**, pas 10 USD.

Succès quand `status === 'success'` (et `isDefinitive === true`).  
Préférer SignalR `PayInStatusUpdated` ; le polling reste un filet de sécurité.

**Statuts check :**

| `status` | `isDefinitive` | Action front |
|----------|----------------|--------------|
| `pending` | `false` | Continuer le poll (USSD en cours — y compris si la gateway a renvoyé un `Status: Error` soft) |
| `success` | `true` | Succès — arrêter |
| `error` | `true` | Échec **définitif** — arrêter ; afficher `statusDescription` |
| `timeout` | `true` | Timeout — arrêter |

Le backend **ne passe plus** un PayIn `pending` en `error` sur un **check** ou un **callback** trop tôt avec seulement `Status: "Error"` (sans `resultCodeError`). Seuls les échecs durs (`resultCodeError`, `cancelled`, `rejected`, `declined`, `timeout`) marquent `Echoue`.

**Anti-pattern observé (04 Sep 2026) :** initiation `pending` + USSD → front appelle immédiatement `GET .../status/{ref}` → lit `error` déjà en base (faux soft Error) → stop. Corriger : `POST .../check` + délai 3–5 s.

```javascript
async function attendreConfirmation(reference, token, maxMs = 120000) {
  await new Promise(r => setTimeout(r, 4000)); // délai initial USSD
  const debut = Date.now();
  while (Date.now() - debut < maxMs) {
    const res = await fetch(`/api/MokoAfrika/status/${reference}/check`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}` }
    });
    const tx = await res.json();
    if (tx.status === 'success')
      return { ok: true, tx };
    if (tx.isDefinitive && (tx.status === 'error' || tx.status === 'timeout'))
      return { ok: false, tx, message: tx.statusDescription };
    await new Promise(r => setTimeout(r, 8000));
  }
  return { ok: false, timeout: true };
}
```

---

## 14. Gestion des erreurs

| HTTP | Code / message | Action UI |
|------|----------------|-----------|
| **503** | `MOKO_MIGRATION_REQUIRED` | Bandeau admin : exécuter migration SQL |
| **200** | `estConfigure: false` | Wizard configuration, pas d'erreur |
| **400** | Paiement déjà en attente | Afficher paiement en cours |
| **400** | MM désactivé | Rediriger vers config école |
| **404** | Élève/frais introuvable | Vérifier sélection |
| **401** | Token expiré | Reconnexion |
| **500** | Erreur interne | Vérifier logs serveur + migration DB |

### Notifications push

| Moment | Notification |
|--------|--------------|
| PayIn `En attente` | Aucune |
| PayIn `Confirme` | Push tuteur « Confirmation Paiement Frais » |
| PayIn `Echoue` | Aucune (message in-app uniquement) |

---

## 14. Checklist

### Infra / Backend
- [ ] Exécuter `scripts/migration-moko-afrika-manual.sql` sur `dev-knb_db`
- [ ] Vérifier `MokoSettings.CallbackUrl` = `https://dev-knb.asdc-rdc.org/api/MokoAfrika/callback`
- [ ] Redéployer API avec dernière version

### Frontend — Parent
- [ ] Flux MOKO remplace `POST /api/Paiement` pour MM/Carte
- [ ] Estimation frais avant validation
- [ ] Polling USSD
- [ ] Ne pas compter « En attente » comme payé

### Frontend — École / Caissier
- [ ] **Écran accueil guichet** : `GET /api/Dashboard/caissier?idEcole={jwt.idEcole}` au login
- [ ] Gérer `estConfigure === false` (wizard, pas erreur 500)
- [ ] Gérer HTTP 503 migration
- [ ] Dashboard wallet + stats
- [ ] Onglets transactions (réussies / en attente / échouées)
- [ ] Mouvements wallet + PayOuts échoués (lecture seule)
- [ ] Écran encaissement guichet (PayIn avec rôle Admin/Financier/**Caissier** — pas Directeur)

### Frontend — Super-Admin
- [ ] Bouton retry PayOut (`POST payout/retry`) **uniquement Super-Admin**
- [ ] Config école identique au personnel école

---

*KelasiNaBiso — Paiement électronique MOKO Afrika — Juillet 2026*
