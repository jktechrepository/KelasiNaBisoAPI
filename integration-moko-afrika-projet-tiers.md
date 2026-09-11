# Intégration MOKO Afrika — Guide projet tiers (PayIn + PayOut, même marchand)

Guide **autonome** pour intégrer le gateway **MOKO Afrika** (GoFreshPay / PayDRC) dans **n'importe quel backend ou application**, avec :

- **PayIn** (`debit`) — collecte Mobile Money / carte auprès du **client payeur**
- **PayOut** (`credit`) — reversement vers le **bénéficiaire** (mobile money du site / boutique)
- **Le même compte marchand** pour les deux opérations

> Référence d'implémentation production : [MombongoAPI](https://bitbucket.org/kansabusiness/mombongo-api) — voir aussi `docs/integration-moko-afrika.md` pour le wallet virtuel et l'orchestration complète.

---

## Table des matières

1. [Concepts clés](#1-concepts-clés)
2. [Même marchand, deux numéros différents](#2-même-marchand-deux-numéros-différents)
3. [Configuration](#3-configuration)
4. [Gateway HTTP — spécification](#4-gateway-http--spécification)
5. [PayIn (collecte client)](#5-payin-collecte-client)
6. [PayOut (reversement bénéficiaire)](#6-payout-reversement-bénéficiaire)
7. [Vérification de statut](#7-vérification-de-statut)
8. [Callbacks webhook](#8-callbacks-webhook)
9. [Grille des frais](#9-grille-des-frais)
10. [Flux métier recommandé](#10-flux-métier-recommandé)
11. [Exemples de code](#11-exemples-de-code)
12. [Erreurs fréquentes](#12-erreurs-fréquentes)
13. [Checklist d'intégration](#13-checklist-dintégration)
14. [Annexe — intégration via MombongoAPI](#14-annexe--intégration-via-mombongoapi)

---

## 1. Concepts clés

MOKO Afrika est un agrégateur de paiements en RDC (Mobile Money + carte).

| Action | Champ `action` | Sens | Qui est `customer_number` ? |
|--------|----------------|------|-----------------------------|
| **PayIn** | `debit` | Client → Marchand (C2B) | **Téléphone du client** qui paie |
| **PayOut** | `credit` | Marchand → Bénéficiaire (B2C) | **Téléphone du bénéficiaire** (ex. mobile du site) |
| **Check** | `check` | Consultation statut | — |

### URLs du gateway

| Environnement | URL (POST JSON) |
|---------------|-----------------|
| **Test (sandbox)** | `https://api.gofreshpay.com/api/v1/gateway` |
| **Production** | `https://paydrc.gofreshbakery.net/api/v5/` |

Toutes les opérations sont des **`POST`** vers l'URL de base, corps **`application/json`**, timeout recommandé **120 secondes**.

### Identifiants marchand (identiques PayIn et PayOut)

| Paramètre config | Champ gateway | Description |
|------------------|---------------|-------------|
| `MerchantId` | `merchant_id` | Identifiant marchand MOKO |
| `SecretKey` | `merchant_secrete` | Clé secrète marchand |
| `MerchantCode` | `merchant_code` | Code marchand (souvent requis) |

---

## 2. Même marchand, deux numéros différents

Règle fondamentale validée en production :

```
┌─────────────────────────────────────────────────────────────┐
│  MARCHAND MOKO (identique PayIn et PayOut)                  │
│  merchant_id + merchant_secrete + merchant_code             │
└─────────────────────────────────────────────────────────────┘
         │                                    │
         │ action: debit                      │ action: credit
         ▼                                    ▼
┌─────────────────┐                 ┌─────────────────┐
│ customer_number │                 │ customer_number │
│ = CLIENT        │                 │ = BÉNÉFICIAIRE  │
│ (payeur)        │                 │ (site / boutique)│
│ ex: 0970940927  │                 │ ex: 0990990337  │
└─────────────────┘                 └─────────────────┘
```

| Élément | PayIn | PayOut |
|---------|-------|--------|
| `merchant_id` | Identique | Identique |
| `merchant_secrete` | Identique | Identique |
| `merchant_code` | Identique | Identique |
| `callback_url` | Identique | Identique |
| `firstname` / `lastname` / `e-mail` | Profil marchand fixe (ex. KANSA / BUSINESS) | **Même profil fixe** |
| `customer_number` | **Client payeur** | **Bénéficiaire du reversement** |
| `amount` | Montant **collecté** (net + frais) | Montant **net** reversé |
| `action` | `debit` | `credit` |

> **Erreur fréquente** : envoyer le nom du détenteur du compte bénéficiaire dans `firstname`/`lastname` au PayOut. MOKO attend le **même profil statique** qu'au PayIn ; seul `customer_number` change.

---

## 3. Configuration

Exemple de configuration (adaptable à `.env`, `appsettings.json`, secrets cloud, etc.) :

```json
{
  "MokoSettings": {
    "MerchantId": "VOTRE_MERCHANT_ID",
    "MerchantCode": "VOTRE_MERCHANT_CODE",
    "SecretKey": "VOTRE_SECRET_KEY",
    "HmacKey": "VOTRE_HMAC_KEY",
    "TestUrl": "https://api.gofreshpay.com/api/v1/gateway",
    "ProductionUrl": "https://paydrc.gofreshbakery.net/api/v5/",
    "IsProduction": false,
    "CallbackUrl": "https://votre-api.com/api/moko/callback",
    "StaticCustomerFirstName": "KANSA",
    "StaticCustomerLastName": "BUSINESS",
    "StaticCustomerEmail": "contact@votre-domaine.com"
  }
}
```

| Clé | Usage |
|-----|--------|
| `IsProduction` | `false` → TestUrl, `true` → ProductionUrl |
| `CallbackUrl` | Webhook HTTPS public (même URL PayIn et PayOut) |
| `StaticCustomer*` | Profil envoyé au gateway pour **PayIn et PayOut** |
| `HmacKey` | Vérification signature callback `X-Signature` |

**Sécurité** : ne jamais committer les clés ; utiliser variables d'environnement en production.

---

## 4. Gateway HTTP — spécification

### En-têtes

```http
POST {GatewayUrl}
Content-Type: application/json
```

### Schéma JSON (snake_case obligatoire)

```json
{
  "merchant_id": "string",
  "merchant_secrete": "string",
  "merchant_code": "string",
  "amount": "string",
  "currency": "CDF",
  "action": "debit | credit | check",
  "customer_number": "string",
  "firstname": "string",
  "lastname": "string",
  "e-mail": "string",
  "reference": "string",
  "method": "airtel | orange | mpesa | africell | card",
  "callback_url": "https://..."
}
```

### Champs par action

| Champ | PayIn | PayOut | Check |
|-------|:-----:|:------:|:-----:|
| `merchant_id` | ✓ | ✓ | ✓ |
| `merchant_secrete` | ✓ | ✓ | ✓ |
| `merchant_code` | ✓ | ✓ | ✓ |
| `action` | `debit` | `credit` | `check` |
| `amount` | ✓ | ✓ | — |
| `currency` | ✓ | ✓ | — |
| `customer_number` | ✓ | ✓ | — |
| `firstname` | ✓ | ✓ | — |
| `lastname` | ✓ | ✓ | — |
| `e-mail` | ✓ | ✓ | — |
| `reference` | ✓ | ✓ | ✓ |
| `method` | ✓ | ✓ | — |
| `callback_url` | ✓ | ✓ | — |

> Le champ email gateway est **`e-mail`** (avec tiret), pas `email`.

### Référence unique

Format recommandé : `MOKO_{yyyyMMddHHmmss}_{4 chiffres}`

Exemple : `MOKO_20260630151414_5569`

- **PayIn** : une référence par collecte
- **PayOut** : une **nouvelle** référence ; lier au PayIn via votre base (`parentReference`)

### Interprétation succès

**À l'initiation PayIn Mobile Money**, `resultCode == "0"` signifie que la requête a été **acceptée** (push USSD envoyé) — ce n'est **pas** une confirmation de paiement.

Considérer la transaction comme **définitivement réussie** (crédit wallet, confirmation paiement) uniquement si :

- `Status` / `trans_status` ∈ `success`, `successful`, `approved`, `paid`, `SUCCESS`, **et**
- `resultCodeError` absent ou égal à `"0"`

Cas **en attente** (ne pas confirmer) :

- `Status` / `trans_status` ∈ `pending`, `initiated`, `processing`
- `resultCode == "0"` sans status de succès explicite

Exemple PayIn réussi :

```json
{
  "Amount": 527,
  "Comment": "Transaction Received Successfully",
  "Currency": "CDF",
  "Customer_Number": "970940927",
  "Reference": "MOKO_20260630155307_8897",
  "Status": "Success",
  "Transaction_id": "PD20260630952DBC85D9J43"
}
```

Exemple PayOut échoué :

```json
{
  "Comment": "your merchant identifier not recognized in the system",
  "Status": "Error",
  "resultCode": 1,
  "resultCodeError": 404,
  "resultCodeErrorDescription": "your merchant identifier not recognized in the system..."
}
```

---

## 5. PayIn (collecte client)

### Quand l'utiliser

Le client paie une commande, une facture, un abonnement via Mobile Money ou carte.

### Montant

Le client paie le **montant collecté** :

```
montantCollecte = montantNet + fraisCollecte + fraisDecaissement
```

Les frais de décaissement sont inclus dans le PayIn pour couvrir le futur PayOut.

### Payload gateway complet (exemple Airtel, net 500 CDF)

Montant collecté calculé : **527,50 CDF** (voir [grille des frais](#9-grille-des-frais)).

```json
{
  "merchant_id": "VOTRE_MERCHANT_ID",
  "merchant_secrete": "VOTRE_SECRET_KEY",
  "merchant_code": "VOTRE_MERCHANT_CODE",
  "amount": "527.5",
  "currency": "CDF",
  "action": "debit",
  "customer_number": "0970940927",
  "firstname": "KANSA",
  "lastname": "BUSINESS",
  "e-mail": "billy-paul.kalambayi@kansaconsulting.com",
  "reference": "MOKO_20260630155307_8897",
  "method": "airtel",
  "callback_url": "https://votre-api.com/api/moko/callback"
}
```

### cURL

```bash
curl -X POST "https://paydrc.gofreshbakery.net/api/v5/" \
  -H "Content-Type: application/json" \
  -d '{
    "merchant_id": "VOTRE_MERCHANT_ID",
    "merchant_secrete": "VOTRE_SECRET_KEY",
    "merchant_code": "VOTRE_MERCHANT_CODE",
    "amount": "527.5",
    "currency": "CDF",
    "action": "debit",
    "customer_number": "0970940927",
    "firstname": "KANSA",
    "lastname": "BUSINESS",
    "e-mail": "billy-paul.kalambayi@kansaconsulting.com",
    "reference": "MOKO_20260630155307_8897",
    "method": "airtel",
    "callback_url": "https://votre-api.com/api/moko/callback"
  }'
```

### Après succès PayIn

1. **Confirmer la commande / vente** côté métier (le client a payé).
2. Persister : référence, montant net, montant collecté, téléphone client, statut.
3. Planifier le **PayOut** vers le bénéficiaire (immédiat ou différé).

---

## 6. PayOut (reversement bénéficiaire)

### Quand l'utiliser

Reverser le **montant net** au mobile money du site, de la boutique ou du partenaire après une vente réussie.

### Montant

Le PayOut envoie le **montant net** (hors frais Moko déjà prélevés au PayIn) :

```
montantPayOut = montantNet
```

### Payload gateway complet (exemple validé production)

```json
{
  "merchant_id": "VOTRE_MERCHANT_ID",
  "merchant_secrete": "VOTRE_SECRET_KEY",
  "merchant_code": "VOTRE_MERCHANT_CODE",
  "amount": "1900",
  "currency": "CDF",
  "action": "credit",
  "customer_number": "0970940927",
  "firstname": "KANSA",
  "lastname": "BUSINESS",
  "e-mail": "billy-paul.kalambayi@kansaconsulting.com",
  "reference": "MOKO_20260630151414_5569",
  "method": "airtel",
  "callback_url": "https://votre-api.com/api/moko/callback"
}
```

### Différences PayIn vs PayOut (même marchand)

| | PayIn | PayOut |
|---|-------|--------|
| `action` | `debit` | `credit` |
| `amount` | `527.5` (collecté) | `500` (net) |
| `customer_number` | Client `0970940927` | Bénéficiaire (peut être un autre numéro) |
| `reference` | `MOKO_...8897` | **Nouvelle** ref `MOKO_...5569` |
| Marchand | Identique | Identique |
| Profil nom/email | KANSA / BUSINESS | KANSA / BUSINESS |

### cURL

```bash
curl -X POST "https://paydrc.gofreshbakery.net/api/v5/" \
  -H "Content-Type: application/json" \
  -d '{
    "merchant_id": "VOTRE_MERCHANT_ID",
    "merchant_secrete": "VOTRE_SECRET_KEY",
    "merchant_code": "VOTRE_MERCHANT_CODE",
    "amount": "1900",
    "currency": "CDF",
    "action": "credit",
    "customer_number": "0970940927",
    "firstname": "KANSA",
    "lastname": "BUSINESS",
    "e-mail": "billy-paul.kalambayi@kansaconsulting.com",
    "reference": "MOKO_20260630151414_5569",
    "method": "airtel",
    "callback_url": "https://votre-api.com/api/moko/callback"
  }'
```

### Gestion des échecs PayOut

- La **vente reste valide** si le PayIn a réussi.
- Implémenter : file d'attente, retry, alertes email équipe technique.
- Relancer avec une **nouvelle référence** PayOut (ne pas réutiliser la ref échouée).

---

## 7. Vérification de statut

```json
{
  "merchant_id": "VOTRE_MERCHANT_ID",
  "merchant_secrete": "VOTRE_SECRET_KEY",
  "merchant_code": "VOTRE_MERCHANT_CODE",
  "action": "check",
  "reference": "MOKO_20260630155307_8897"
}
```

Utile après timeout HTTP ou en polling jusqu'à confirmation callback.

---

## 8. Callbacks webhook

### Réception

```http
POST /api/moko/callback
Content-Type: application/json
X-Signature: {hmac_sha256_hex_du_corps_brut}
```

### Corps type

```json
{
  "reference": "MOKO_20260630155307_8897",
  "trans_status": "SUCCESS",
  "trans_status_description": "Transaction finalisée avec succès"
}
```

### Vérification HMAC

1. Lire le corps **brut** (raw body).
2. Calculer `HMAC-SHA256(rawBody, HmacKey)`.
3. Comparer en hexadécimal (insensible à la casse) avec l'en-tête `X-Signature`.

### Bonnes pratiques

- Endpoint **public** (sans JWT utilisateur).
- Répondre `200` rapidement ; traiter en async si nécessaire.
- Idempotence : ne pas créditer deux fois la même `reference`.

---

## 9. Grille des frais

Grille par défaut (RDC, montant net en entrée) :

| Opérateur | Code | Collecte % | Décaissement % | TVA |
|-----------|------|------------|----------------|-----|
| Orange Money | `orange` | 3,00 | 2,00 | Incluse |
| M-Pesa | `mpesa` | 2,50 | 2,00 | Incluse |
| Airtel Money | `airtel` | 3,00 | 2,50 | Incluse |
| Africell | `africell` | 3,00 | 2,50 | Incluse |
| Carte | `card` | 3,50 | 3,00 | **16 % en sus** sur frais collecte |

### Formules

```
fraisCollecteBase = montantNet × (tauxCollecte / 100)
fraisTva          = fraisCollecteBase × (tauxTva / 100)   // carte uniquement
fraisCollecte     = fraisCollecteBase + fraisTva
fraisDecaissement = montantNet × (tauxDecaissement / 100)
montantCollecte   = montantNet + fraisCollecte + fraisDecaissement
```

Arrondi : 2 décimales, arrondi commercial (`AwayFromZero`).

### Exemple Airtel — net 500 CDF

| Élément | Montant |
|---------|---------|
| Frais collecte (3 %) | 15 CDF |
| Frais décaissement (2,5 %) | 12,50 CDF |
| **Client paie (PayIn)** | **527,50 CDF** |
| **Bénéficiaire reçoit (PayOut)** | **500 CDF** |

### Méthodes de paiement

| Code | Opérateur |
|------|-----------|
| `orange` | Orange Money |
| `mpesa` | Vodacom / M-Pesa |
| `airtel` | Airtel Money |
| `africell` | Africell |
| `card` | Visa / Mastercard |

Numéros acceptés : `243XXXXXXXXX` ou `0XXXXXXXXX`.

---

## 10. Flux métier recommandé

### Flux minimal (sans wallet intermédiaire)

```
1. Calculer montantCollecte à partir du montantNet
2. PayIn (debit) → customer_number = client
3. Si succès → confirmer commande
4. PayOut (credit) → customer_number = bénéficiaire site
5. Si succès → marquer reversement OK
6. Si PayOut échoue → retry + alerte (vente déjà confirmée)
```

### Flux avec délai de règlement (recommandé Mombongo / K-Mombongo)

```
1. PayIn réussi → crédit solde "en attente"
2. Attendre N minutes (ex. 3 min — délai Moko)
3. Libérer solde "disponible"
4. PayOut automatique vers bénéficiaire
5. Débiter solde disponible
```

### Modèle de données minimal

| Table / entité | Champs clés |
|----------------|-------------|
| `transactions_moko` | `reference`, `parent_reference`, `action`, `amount`, `customer_phone`, `method`, `status`, `raw_request`, `raw_response` |
| `beneficiaires` | `id_site`, `methode`, `numero`, `statut` (ACTIF) |
| `file_payout` | `pay_in_reference`, `montant_net`, `scheduled_at`, `status`, `pay_out_reference`, `error_message` |

---

## 11. Exemples de code

### JavaScript (Node.js / fetch)

```javascript
const GATEWAY_URL = process.env.MOKO_GATEWAY_URL;
const MERCHANT = {
  id: process.env.MOKO_MERCHANT_ID,
  secret: process.env.MOKO_SECRET_KEY,
  code: process.env.MOKO_MERCHANT_CODE,
};
const PROFILE = {
  firstname: "KANSA",
  lastname: "BUSINESS",
  email: "contact@votre-domaine.com",
};
const CALLBACK = "https://votre-api.com/api/moko/callback";

function ref() {
  const ts = new Date().toISOString().replace(/\D/g, "").slice(0, 14);
  return `MOKO_${ts}_${Math.floor(1000 + Math.random() * 9000)}`;
}

async function mokoGateway(payload) {
  const res = await fetch(GATEWAY_URL, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
    signal: AbortSignal.timeout(120_000),
  });
  const body = await res.text();
  return { status: res.status, body: JSON.parse(body) };
}

export async function payIn(customerNumber, amountCollecte, method = "airtel") {
  return mokoGateway({
    merchant_id: MERCHANT.id,
    merchant_secrete: MERCHANT.secret,
    merchant_code: MERCHANT.code,
    amount: String(amountCollecte),
    currency: "CDF",
    action: "debit",
    customer_number: customerNumber,
    firstname: PROFILE.firstname,
    lastname: PROFILE.lastname,
    "e-mail": PROFILE.email,
    reference: ref(),
    method,
    callback_url: CALLBACK,
  });
}

export async function payOut(beneficiaryNumber, montantNet, method = "airtel") {
  return mokoGateway({
    merchant_id: MERCHANT.id,
    merchant_secrete: MERCHANT.secret,
    merchant_code: MERCHANT.code,
    amount: String(montantNet),
    currency: "CDF",
    action: "credit",
    customer_number: beneficiaryNumber,
    firstname: PROFILE.firstname,
    lastname: PROFILE.lastname,
    "e-mail": PROFILE.email,
    reference: ref(),
    method,
    callback_url: CALLBACK,
  });
}
```

### Python

```python
import os, json, random, requests
from datetime import datetime

GATEWAY_URL = os.environ["MOKO_GATEWAY_URL"]
MERCHANT_ID = os.environ["MOKO_MERCHANT_ID"]
MERCHANT_SECRET = os.environ["MOKO_SECRET_KEY"]
MERCHANT_CODE = os.environ["MOKO_MERCHANT_CODE"]
CALLBACK = "https://votre-api.com/api/moko/callback"
PROFILE = {"firstname": "KANSA", "lastname": "BUSINESS", "e-mail": "contact@votre-domaine.com"}

def new_ref():
    ts = datetime.utcnow().strftime("%Y%m%d%H%M%S")
    return f"MOKO_{ts}_{random.randint(1000, 9999)}"

def moko_call(payload: dict) -> dict:
    r = requests.post(GATEWAY_URL, json=payload, timeout=120)
    return {"status": r.status_code, "body": r.json()}

def pay_in(customer_number: str, amount_collecte: float, method: str = "airtel") -> dict:
    return moko_call({
        "merchant_id": MERCHANT_ID,
        "merchant_secrete": MERCHANT_SECRET,
        "merchant_code": MERCHANT_CODE,
        "amount": str(amount_collecte),
        "currency": "CDF",
        "action": "debit",
        "customer_number": customer_number,
        **PROFILE,
        "reference": new_ref(),
        "method": method,
        "callback_url": CALLBACK,
    })

def pay_out(beneficiary_number: str, montant_net: float, method: str = "airtel") -> dict:
    return moko_call({
        "merchant_id": MERCHANT_ID,
        "merchant_secrete": MERCHANT_SECRET,
        "merchant_code": MERCHANT_CODE,
        "amount": str(montant_net),
        "currency": "CDF",
        "action": "credit",
        "customer_number": beneficiary_number,
        **PROFILE,
        "reference": new_ref(),
        "method": method,
        "callback_url": CALLBACK,
    })
```

### C# (.NET) — extrait

Voir implémentation complète dans MombongoAPI : `Services/MokoAfrikaService.cs`.

Points clés :

```csharp
// PayIn et PayOut : même MerchantId, SecretKey, MerchantCode
// PayIn  : Action = "debit",  CustomerNumber = client
// PayOut : Action = "credit", CustomerNumber = beneficiaire
// PayOut : FirstName/LastName/Email = profil statique (pas le nom du bénéficiaire)
```

---

## 12. Erreurs fréquentes

| Symptôme | Cause probable | Action |
|----------|----------------|--------|
| PayIn OK, PayOut « merchant not recognized » | Compte marchand sans droit B2C / mauvais env | Contacter MOKO ; vérifier `IsProduction` vs credentials |
| PayOut échoue, PayIn OK | `firstname`/`lastname` du bénéficiaire au lieu du profil fixe | Utiliser KANSA / BUSINESS pour PayIn **et** PayOut |
| `customer_number` incorrect | Confusion client vs bénéficiaire | PayIn = payeur ; PayOut = mobile du site |
| Montant PayOut rejeté | Envoi du montant collecté au lieu du net | PayOut = `montantNet` uniquement |
| Callback non reçu | URL incorrecte ou non HTTPS | `callback_url` → `/api/MokoAfrika/callback` exposé |
| Timeout | Push USSD client lent | Timeout 120 s + `action: check` |
| Référence dupliquée | Réutilisation d'une ref PayOut échouée | Générer une nouvelle ref à chaque tentative |

---

## 13. Checklist d'intégration

### Configuration
- [ ] Identifiants marchand test puis production
- [ ] `IsProduction` cohérent avec l'URL gateway
- [ ] `CallbackUrl` HTTPS public et testé
- [ ] Profil statique `KANSA` / `BUSINESS` / email configuré

### PayIn
- [ ] `action: debit`, montant = **montantCollecte**
- [ ] `customer_number` = téléphone **client**
- [ ] Référence unique par transaction
- [ ] Confirmation métier uniquement après succès gateway

### PayOut
- [ ] `action: credit`, montant = **montantNet**
- [ ] `customer_number` = mobile **bénéficiaire**
- [ ] Même marchand que PayIn
- [ ] Même profil nom/email que PayIn
- [ ] Nouvelle référence à chaque PayOut / retry

### Opérationnel
- [ ] Persistance audit (`raw_request`, `raw_response`)
- [ ] Webhook HMAC vérifié
- [ ] Retry PayOut + alertes si échec
- [ ] Tests sandbox avant production

---

## 14. Annexe — intégration via MombongoAPI

Si vous consommez **MombongoAPI** plutôt que le gateway directement :

| Besoin | Endpoint |
|--------|----------|
| Estimation frais | `GET /api/MokoAfrika/fees/estimate?amount=&method=&currency=` |
| PayIn manuel | `POST /api/MokoAfrika/payin` |
| PayOut manuel | `POST /api/MokoAfrika/payout` |
| Statut | `GET /api/MokoAfrika/status/{reference}` |
| Vente + PayIn auto | `POST /api/Vente/enregistrer` (`modePaiement: "Mobile Money"`) |
| Retry reversement | `POST /api/MokoAfrika/payout/retry?payInReference=` |
| Solde wallet site | `GET /api/Wallets/site/{siteId}/balance` |

Documentation complète MombongoAPI : `docs/integration-moko-afrika.md`

---

*Guide MOKO Afrika — projet tiers — PayIn + PayOut même marchand. Kansa Consulting / K-Mombongo — juillet 2026.*
