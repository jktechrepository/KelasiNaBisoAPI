# Intégration portable du paiement électronique MokoAfrika

Ce document décrit une intégration **réutilisable** de MokoAfrika (GoFreshPay /
PayDRC) dans une API ou une application qui peut avoir des règles métier
différentes. Il sépare :

- le contrat technique MokoAfrika, qui doit rester stable ;
- l'orchestration de paiement, commune à la plupart des projets ;
- les règles métier de l'application hôte, à brancher via un adaptateur.

Les exemples utilisent des valeurs fictives. Les clés, numéros et URL de
production doivent être fournis par MokoAfrika et stockés dans un gestionnaire
de secrets.

## 1. Périmètre et vocabulaire

| Terme | Signification |
|---|---|
| **PayIn** | Encaissement : le client paie le marchand (`debit`) |
| **PayOut** | Reversement : le marchand paie un bénéficiaire (`credit`) |
| **Gateway** | API HTTP MokoAfrika |
| **Référence** | Identifiant unique généré par l'application pour une transaction |
| **Callback** | Webhook envoyé par MokoAfrika après une évolution de statut |
| **Transaction locale** | Trace immuable des appels, réponses et changements de statut |
| **Règle métier** | Validation propre au projet : commande, facture, abonnement, frais, etc. |

Une transaction MokoAfrika ne doit pas être confondue avec l'objet métier payé.
Par exemple, une commande peut avoir plusieurs tentatives de paiement, et une
tentative ne doit être considérée comme payée qu'après confirmation définitive.

## 2. Architecture recommandée

```text
Client
  │
  ▼
API de paiement ──► Validateur métier / adaptateur du projet
  │                         │
  │                         └── autorise montant, devise, objet et payeur
  ▼
Orchestrateur de paiement
  ├── Transaction locale (audit + idempotence)
  ├── Client HTTP MokoAfrika
  ├── Traitement callback HMAC
  └── Worker de vérification / PayOut
             │
             ▼
       MokoAfrika Gateway
```

La couche gateway ne doit pas connaître les règles du domaine. Elle reçoit un
payload normalisé et retourne une réponse normalisée. L'orchestrateur décide
ensuite si la commande, la facture ou tout autre objet peut passer à l'état
`paid`.

### 2.1 Composants à implémenter

1. **Configuration** : identifiants marchand, URLs, secrets et callback.
2. **Client gateway** : POST JSON, timeout, journalisation sans secrets.
3. **Calculateur de frais** : montant net, frais de collecte et montant envoyé.
4. **Transaction repository** : persistance avant l'appel externe.
5. **Orchestrateur PayIn** : validation, création, appel, confirmation.
6. **Handler webhook** : signature, parsing, idempotence et routage.
7. **Job de réconciliation** : interrogation périodique des transactions
   toujours en attente.
8. **PayOut worker** (facultatif) : reversement après règlement disponible.

## 3. Configuration

Exemple adaptable à `appsettings.json`, `.env` ou un secret manager :

```json
{
  "MokoAfrika": {
    "MerchantId": "VOTRE_MERCHANT_ID",
    "MerchantCode": "VOTRE_MERCHANT_CODE",
    "SecretKey": "VOTRE_SECRET_KEY",
    "HmacKey": "VOTRE_CLE_HMAC",
    "TestUrl": "https://api.gofreshpay.com/api/v1/gateway",
    "ProductionUrl": "https://paydrc.gofreshbakery.net/api/v5/",
    "IsProduction": false,
    "CallbackUrl": "https://api.example.com/api/payments/moko/callback",
    "StaticCustomerFirstName": "BUSINESS",
    "StaticCustomerLastName": "MERCHANT",
    "StaticCustomerEmail": "payments@example.com",
    "TimeoutSeconds": 120
  }
}
```

| Paramètre | Règle |
|---|---|
| `MerchantId`, `MerchantCode`, `SecretKey` | Identiques pour PayIn et PayOut |
| `HmacKey` | Secret utilisé pour vérifier le callback |
| `IsProduction` | Sélectionne l'URL de test ou de production |
| `CallbackUrl` | HTTPS public, accessible par MokoAfrika |
| `StaticCustomer*` | Profil marchand envoyé dans les requêtes |
| `TimeoutSeconds` | Ne pas confondre timeout HTTP et statut métier |

Ne jamais exposer `SecretKey` ou `HmacKey` au frontend, dans les logs, dans
Swagger public ou dans le contrôle de version. Le callback doit être configuré
avec la même URL pour les PayIn et les PayOut, sauf indication contraire de
MokoAfrika.

## 4. Contrat HTTP MokoAfrika

### 4.1 Requête commune

Toutes les opérations sont des `POST` avec `Content-Type: application/json`.

```http
POST https://api.gofreshpay.com/api/v1/gateway
Content-Type: application/json
```

Le champ `merchant_secrete` est volontairement écrit ainsi dans le contrat
gateway ; ne pas le renommer en `merchant_secret`.

```json
{
  "merchant_id": "merchant-id",
  "merchant_secrete": "secret",
  "merchant_code": "merchant-code",
  "amount": "1050.00",
  "currency": "CDF",
  "action": "debit",
  "customer_number": "0812345678",
  "firstname": "BUSINESS",
  "lastname": "MERCHANT",
  "e-mail": "payments@example.com",
  "reference": "MKO_20260925_000001",
  "method": "airtel",
  "callback_url": "https://api.example.com/api/payments/moko/callback"
}
```

Le nom du champ email est **`e-mail`**, avec un tiret.

### 4.2 Actions

| Action | Usage | `customer_number` |
|---|---|---|
| `debit` | PayIn client → marchand | Téléphone du payeur |
| `credit` | PayOut marchand → bénéficiaire | Téléphone du bénéficiaire |
| `verify` | Vérification du statut | Référence de la transaction |

Le même marchand peut donc collecter un paiement sur le numéro du client puis
effectuer un reversement sur un autre numéro, sans changer les identifiants
marchand.

### 4.3 Méthodes et devises

Les méthodes généralement prises en charge par l'intégration sont :
`airtel`, `orange`, `mpesa`, `africell` et `card`. La devise doit être
explicitement transmise, par exemple `CDF`. La liste réellement autorisée,
les plafonds et les frais doivent être confirmés avec MokoAfrika.

## 5. Références et modèle de données portable

Générer une référence unique, non réutilisable et indexée en base :

```text
MKO_{UTC_yyyyMMddHHmmss}_{identifiant-aléatoire}
```

Pour un PayOut, générer une nouvelle référence et conserver le lien vers le
PayIn parent (`parentReference`). Ne jamais utiliser la référence métier seule
si elle peut avoir plusieurs tentatives.

### 5.1 Transaction gateway minimale

```text
PaymentTransaction
  id
  reference                unique
  parentReference          nullable (PayOut)
  businessObjectType       ex. Order, Invoice, Subscription
  businessObjectId         identifiant dans l'application hôte
  action                   debit | credit | verify
  amount                   montant envoyé à MokoAfrika
  amountNet                montant métier avant frais
  collectionFee            frais PayIn
  payoutFee                frais PayOut estimés
  currency
  method
  customerPhone
  status                   pending | success | error | timeout
  gatewayTransactionId
  statusDescription
  rawRequest / rawResponse / rawCallback
  createdAt / updatedAt
```

Conserver les corps bruts pour le diagnostic, avec une politique de rétention
et une protection des données adaptée. Masquer les numéros et ne jamais
persister les secrets marchand dans ces champs.

### 5.2 États de l'objet métier

L'objet payé doit avoir ses propres états, par exemple :

```text
unpaid → payment_pending → paid
                     └──► payment_failed
```

Un `pending` gateway ne doit jamais produire `paid`. Le mapping entre statut
gateway et statut métier doit être centralisé dans l'orchestrateur.

## 6. Flux PayIn recommandé

1. Recevoir `businessObjectId`, montant net, devise, méthode et téléphone.
2. Charger l'objet métier avec verrou ou contrôle de concurrence.
3. Vérifier qu'il est payable et que le montant demandé est autorisé.
4. Vérifier l'absence d'une tentative `pending` équivalente.
5. Calculer les frais et le montant total à collecter.
6. Créer la transaction locale en `pending` **avant** l'appel gateway.
7. Appeler MokoAfrika avec `action: "debit"`.
8. Retourner la référence et `pending` au client.
9. Confirmer uniquement par callback ou par `verify`.
10. Dans une transaction DB idempotente, marquer l'objet payé et déclencher
    les effets annexes (reçu, notification, stock, accès, etc.).

L'appel initial peut répondre `resultCode: "0"` alors que le client doit encore
valider un USSD. Ce code signifie que la demande est acceptée, pas que l'argent
est définitivement reçu.

### Exemple de réponse API applicative

```json
{
  "reference": "MKO_20260925_000001",
  "status": "pending",
  "amountNet": 1000,
  "amountToCollect": 1050,
  "currency": "CDF",
  "method": "airtel",
  "requiresCustomerConfirmation": true
}
```

L'API hôte peut appeler ce endpoint `POST /api/payments/moko/payin`, mais le
nom et les paramètres doivent être adaptés au domaine cible. Il est préférable
de recevoir un `businessObjectType` + `businessObjectId` génériques ou un DTO
spécifique au projet, plutôt que de réutiliser des champs scolaires.

## 7. Frais et montant collecté

Conserver séparément :

```text
montantNet       = montant dû par le métier
fraisCollecte   = frais liés à l'encaissement
fraisPayout     = frais prévus pour le reversement
montantCollecte = montantNet + fraisCollecte + fraisPayout
```

Arrondir selon la règle contractuelle, généralement à deux décimales avec un
arrondi explicite. Ne pas recalculer les frais lors d'une confirmation :
persister le devis appliqué au moment de l'initiation.

Les taux varient selon l'opérateur et peuvent évoluer. Ils doivent être
configurables et validés côté serveur, jamais fournis uniquement par le
frontend. Exemple de contrat de calcul :

```json
{
  "amountNet": 1000,
  "collectionFee": 30,
  "payoutFee": 25,
  "amountToCollect": 1055,
  "currency": "CDF",
  "method": "airtel"
}
```

## 8. Callback (webhook)

Exposer une route publique dédiée, par exemple :

```http
POST /api/payments/moko/callback
X-Signature: sha256=<signature>
Content-Type: application/json
```

Le corps exact reçu doit être lu **sans le re-sérialiser** avant le calcul HMAC.
Calculer :

```text
signature = HMAC-SHA256(HmacKey, rawBody)
```

Accepter selon la convention contractuelle les formats hexadécimal ou Base64
et, si prévu par le fournisseur, le préfixe `sha256=`. Comparer en temps
constant. En cas de clé HMAC configurée et de signature absente ou invalide,
répondre `401` sans traiter le callback.

### Traitement idempotent

Le callback peut être livré plusieurs fois ou arriver après un `verify` :

- retrouver la transaction par `reference` ;
- ignorer sans erreur une référence inconnue après journalisation ;
- si déjà `success` et effets métier appliqués, répondre positivement sans
  rejouer ces effets ;
- si `success` mais confirmation locale incomplète, reprendre la confirmation ;
- enregistrer le dernier callback et sa date.

Répondre rapidement en `200` après avoir persisté le résultat. Si le traitement
long doit être asynchrone, placer un événement signé dans une file durable.

## 9. Statuts et interprétation

### 9.1 Statuts normalisés

| Statut local | Signification | Effet |
|---|---|---|
| `pending` | USSD ou traitement en cours | Aucun effet final |
| `success` | Confirmation définitive | Appliquer les effets métier une fois |
| `error` | Refus ou échec définitif | Marquer la tentative échouée |
| `timeout` | Aucune confirmation après politique de délai | Réconciliation ou échec selon le métier |

Normaliser les variantes MokoAfrika (`Success`, `successful`, `approved`,
`paid`, `completed`) vers `success`. Normaliser `pending`, `initiated`,
`processing` et `in_progress` vers `pending`.

Un champ `Status: Error` reçu immédiatement peut être ambigu pendant l'USSD.
Ne le transformer en échec définitif que si la réponse contient un signal
terminal explicite ou si la fenêtre de grâce est dépassée. Cette fenêtre doit
être configurable (par exemple 120 secondes).

### 9.2 Endpoint de vérification

Prévoir une route protégée telle que :

```http
POST /api/payments/moko/{reference}/check
```

Elle envoie à MokoAfrika :

```json
{
  "merchant_id": "merchant-id",
  "merchant_secrete": "secret",
  "merchant_code": "merchant-code",
  "action": "verify",
  "reference": "MKO_20260925_000001"
}
```

Le polling frontend doit être limité (intervalle, durée maximale et arrêt sur
`success`, `error` ou `timeout`). Un worker backend doit réconcilier les
transactions abandonnées, car le frontend peut être fermé.

## 10. PayOut (reversement facultatif)

Le PayOut est une nouvelle transaction :

1. vérifier qu'un solde ou une autorisation de reversement est disponible ;
2. réserver le montant de façon atomique ;
3. créer un `credit` avec une nouvelle référence ;
4. appeler MokoAfrika avec le téléphone bénéficiaire ;
5. attendre callback ou `verify` ;
6. libérer le règlement sur succès ;
7. recréditer la réserve sur échec définitif ;
8. limiter les retries et appliquer un backoff.

Un ledger (`pending`, `available`, mouvements débit/reversal) est recommandé
plutôt qu'un simple champ de solde. Le PayOut peut être désactivé si le projet
ne fait que collecter.

## 11. Sécurité, fiabilité et observabilité

- Utiliser HTTPS pour le gateway et le callback.
- Ne pas faire confiance au montant, à la devise ou à l'identité envoyés par le
  frontend : recharger et valider l'objet métier côté serveur.
- Ajouter un index unique sur `reference`.
- Utiliser une clé d'idempotence applicative par tentative de paiement.
- Protéger les endpoints internes par authentification et autorisation.
- Ne jamais considérer un retour HTTP `200` seul comme un succès financier.
- Définir des timeouts, une politique de retry uniquement pour les erreurs
  réseau et une limite de retries.
- Journaliser référence, action, code HTTP, statut et durée, mais pas les clés.
- Mesurer les transactions `pending` anciennes, callbacks rejetés et écarts de
  réconciliation.
- Ajouter des alertes sur les erreurs de signature et les réponses gateway
  vides ou non JSON.

## 12. Contrat applicatif minimal à exposer

Les routes ci-dessous sont un exemple, pas une obligation :

| Route | Rôle |
|---|---|
| `GET /api/payments/moko/fees` | Estimer les frais |
| `POST /api/payments/moko/payin` | Créer une collecte |
| `GET /api/payments/moko/{reference}` | Lire le statut local |
| `POST /api/payments/moko/{reference}/check` | Réconcilier avec MokoAfrika |
| `POST /api/payments/moko/callback` | Recevoir le webhook |
| `POST /api/payments/moko/payout` | Créer un reversement, si nécessaire |

Réponses HTTP conseillées :

| Statut | Usage |
|---:|---|
| `200` | Lecture, estimation ou confirmation traitée |
| `201` | Transaction locale créée |
| `400` | Validation ou méthode non supportée |
| `401` | Token ou signature invalide |
| `403` | Autorisation insuffisante |
| `404` | Objet ou référence introuvable |
| `409` | Tentative concurrente ou paiement déjà en cours |
| `502` / `503` | Gateway indisponible ou intégration non configurée |

## 13. Adaptation à un autre projet

Avant de copier l'intégration, définir ces décisions métier :

| Question | Exemple de réponse |
|---|---|
| Quel objet est payé ? | commande, facture, abonnement |
| Le montant est-il fixe ou partiel ? | fixe, acompte, solde |
| Qui peut payer ? | client connecté, guichetier, invité |
| Une tentative pending bloque-t-elle l'objet ? | oui pendant 10 minutes |
| Quand l'objet devient-il payé ? | uniquement après `success` |
| Faut-il un PayOut ? | oui après délai, ou non |
| Quelle devise et quelle conversion ? | CDF, USD, taux figé |
| Que faire après timeout ? | réconciliation manuelle ou nouvelle tentative |
| Quelles notifications produire ? | reçu, email, push, webhook interne |

Créer ensuite un adaptateur du type :

```text
PaymentBusinessAdapter
  ValidatePayable(objectId, payer, amount, currency)
  CreatePendingPayment(objectId, transaction)
  ConfirmPayment(transaction)
  FailPayment(transaction, reason)
```

L'adaptateur peut être spécifique à chaque projet, tandis que le client
MokoAfrika, la vérification HMAC, le parser de statuts et le repository de
transactions restent partagés.

## 14. Tests indispensables

Tester au minimum :

- validation des méthodes, devises, montants et téléphones ;
- génération et unicité des références ;
- création locale avant panne gateway ;
- `resultCode = 0` sans confirmation : reste `pending` ;
- succès via callback et via `verify` ;
- callback dupliqué : aucun double effet métier ;
- signature hexadécimale, Base64, absente et invalide ;
- callback JSON invalide ou référence inconnue ;
- échec terminal et expiration de la fenêtre USSD ;
- concurrence entre deux demandes pour le même objet ;
- PayOut réussi, échoué, recrédit et retry limité ;
- absence de secrets dans les logs et les réponses API.

## 15. Checklist de mise en production

- [ ] Les identifiants marchand sont fournis par un secret manager.
- [ ] L'URL de production et la callback HTTPS ont été validées avec MokoAfrika.
- [ ] La signature HMAC est obligatoire en production.
- [ ] Les références sont uniques et indexées.
- [ ] Le montant et l'objet payable sont recalculés côté serveur.
- [ ] `pending` n'est jamais traité comme `success`.
- [ ] Le callback et `verify` partagent la même logique idempotente.
- [ ] Un worker de réconciliation existe, même si le frontend fait du polling.
- [ ] Les timeouts, retries et fenêtres USSD sont configurables.
- [ ] Les données sensibles sont masquées dans les logs.
- [ ] Les tests de panne, doublon et concurrence passent.
- [ ] Une procédure de rapprochement manuel est documentée.

## 16. Particularités de l'implémentation de référence

Dans KelasiNaBiso, l'objet métier est un frais scolaire et les routes concrètes
sont notamment `POST /api/MokoAfrika/payin/frais-scolaire`,
`POST /api/MokoAfrika/callback` et
`POST /api/MokoAfrika/status/{reference}/check`. Ces noms, les rôles, le wallet
par école et les règles d'éligibilité élève/frais sont **spécifiques à ce
projet** et ne doivent pas être copiés dans un autre domaine.

Pour une nouvelle application, conserver le protocole et les garanties décrits
ci-dessus, mais remplacer :

- `IdEleve` / `IdFrais` par l'identifiant de l'objet facturable ;
- les règles d'école par le validateur métier du nouveau projet ;
- le wallet par le ledger ou le compte de règlement approprié ;
- les permissions par les rôles et politiques de la nouvelle application ;
- les notifications scolaires par les effets métier nécessaires.