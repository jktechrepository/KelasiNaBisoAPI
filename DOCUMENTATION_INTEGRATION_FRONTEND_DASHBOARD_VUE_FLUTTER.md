# Integration Vue 3 & Flutter - Dashboards financier et caissier

Guide d'integration des ecrans de synthese financiere et de caisse dans les
clients **Vue 3** (web) et **Flutter** (mobile/tablette).

## Sommaire

1. [Perimetre fonctionnel](#1-perimetre-fonctionnel)
2. [Authentification et permissions](#2-authentification-et-permissions)
3. [Endpoints](#3-endpoints)
4. [Parametres et scopes](#4-parametres-et-scopes)
5. [Contrat de reponse](#5-contrat-de-reponse)
6. [Regle multidevise](#6-regle-multidevise)
7. [Integration Vue 3](#7-integration-vue-3)
8. [Integration Flutter](#8-integration-flutter)
9. [Erreurs, rafraichissement et performance](#9-erreurs-rafraichissement-et-performance)
10. [Checklist](#10-checklist)

## 1. Perimetre fonctionnel

| Dashboard | Usage | Resume |
|---|---|---|
| Caissier | Encaissements de la journee | Nombre, total, PayIns Moko et paiements recents |
| Financier | Suivi financier de la journee | Totaux par statut, solde net, repartition par mode |
| Cloture caissier | Impression/export de caisse | Dashboard + liste complete `tousLesPaiements` |
| Cloture financier | Impression/export financier | Dashboard + liste complete `tousLesPaiements` |

Les montants agreges sont consolides dans la devise principale de l'ecole.
Les paiements individuels conservent leur devise source et leur equivalent
converti.

## 2. Authentification et permissions

Tous les endpoints dashboard exigent un JWT :

```http
Authorization: Bearer <access-token>
Accept: application/json
```

L'application cliente doit utiliser l'`idEcole` correspondant au tenant du
token. Ne pas laisser l'utilisateur saisir librement une autre ecole.

### Roles

| Endpoint | Roles habituels |
|---|---|
| Dashboard caissier | `Caissier`, `Directeur`, `Financier`, `Admin`, `Super-Admin` selon le token |
| Dashboard financier | Role financier ou role d'administration autorise par la configuration |
| Scope `ecole` | `Directeur`, `Financier`, `Admin`, `Super-Admin` |

Un caissier non autorise au scope `ecole` reste automatiquement au scope
`moi`, meme si `scope=ecole` est transmis.

## 3. Endpoints

Les routes en minuscules sont recommandees. Les routes `Dashbord...` (avec
l'orthographe historique) restent disponibles pour compatibilite.

### 3.1 Dashboards courants

```http
GET /api/Dashboard/caissier?idEcole={idEcole}
GET /api/Dashboard/financier?idEcole={idEcole}
```

Alias compatibles :

```http
GET /api/Dashboard/DashbordCaissier?idEcole={idEcole}
GET /api/Dashboard/DashbordFinancier?idEcole={idEcole}
```

### 3.2 Clotures

```http
GET /api/Dashboard/caissier/cloture?idEcole={idEcole}
GET /api/Dashboard/financier/cloture?idEcole={idEcole}
```

Alias compatibles :

```http
GET /api/Dashboard/DashbordCaissier/cloture?idEcole={idEcole}
GET /api/Dashboard/DashbordFinancier/cloture?idEcole={idEcole}
```

Une cloture reprend les memes blocs que le dashboard courant et ajoute :

- `genereLe` : date de generation du rapport ;
- `tousLesPaiements` : tous les paiements du jour et du scope selectionne.

## 4. Parametres et scopes

| Parametre | Type | Requis | Description |
|---|---|---:|---|
| `idEcole` | `int` | oui | Ecole cible, normalement issue du JWT |
| `idAnneeScolaire` | `int` | non | Annee scolaire cible ; l'annee courante est utilisee par defaut |
| `date` | ISO 8601 | non | Jour a afficher ; aujourd'hui par defaut |
| `scope` | `moi` ou `ecole` | non | Paiements de l'utilisateur ou de toute l'ecole |

Exemple :

```http
GET /api/Dashboard/financier?idEcole=1&idAnneeScolaire=101&date=2026-09-24&scope=ecole
```

La periode est journaliere : `dateDebut` et `dateFin` correspondent au jour
demande. Les dates doivent etre envoyees au format ISO (`yyyy-MM-dd` ou
timestamp ISO selon le client).

## 5. Contrat de reponse

Les noms JSON sont en camelCase.

### 5.1 Reponse caissier

```json
{
  "ecole": {
    "idEcole": 1,
    "nomEcole": "Ecole Exemple",
    "logo": null,
    "devisePrincipale": "CDF"
  },
  "idAnneeScolaire": 101,
  "libelleAnneeScolaire": "2026-2027",
  "periode": {
    "type": "jour",
    "date": "2026-09-24T00:00:00",
    "dateDebut": "2026-09-24T00:00:00",
    "dateFin": "2026-09-24T00:00:00",
    "joursOuvrables": 1,
    "libelle": "jeudi 24 septembre 2026"
  },
  "resume": {
    "nombrePaiements": 2,
    "montantTotal": 210000,
    "payInsReussis": 1,
    "payInsEnAttente": 0,
    "payInsEchoues": 0
  },
  "repartitionParMode": {
    "modes": {
      "Cash": { "nombre": 1, "montant": 70000, "pourcentage": 33.33 },
      "Moko": { "nombre": 1, "montant": 140000, "pourcentage": 66.67 }
    }
  },
  "derniersPaiements": [],
  "moko": {
    "estConfigure": true,
    "mesPayInsEnAttente": 0,
    "payInsEnAttente": []
  },
  "scope": "moi"
}
```

### 5.2 Reponse financier

Le dashboard financier partage les blocs `ecole`, `periode`,
`repartitionParMode`, `derniersPaiements`, `moko` et `scope`. Son resume est :

```json
{
  "nombrePaiements": 3,
  "montantTotal": 350000,
  "montantConfirme": 250000,
  "montantEnAttente": 100000,
  "montantEchoue": 0,
  "soldeNet": 350000
}
```

Tous ces montants sont dans `ecole.devisePrincipale`.

### 5.3 Paiement recent

```json
{
  "idPaiement": 45,
  "datePaiement": "2026-09-24T10:15:00",
  "montant": 50,
  "devise": "USD",
  "codeDevisePaiement": "USD",
  "codeDevisePrincipale": "CDF",
  "montantDevisePrincipale": 140000,
  "modePaiement": "Cash",
  "statut": "Confirme",
  "nomEleve": "Amina K.",
  "libelleFrais": "Minerval",
  "referenceMoko": null
}
```

`derniersPaiements` contient au maximum 15 elements. Une liste vide est valide
et ne doit pas etre interpretee comme une erreur.

### 5.4 PayIns Moko en attente

```json
{
  "reference": "MKO-20260924-001",
  "idPaiement": 46,
  "montant": 50,
  "codeDevisePaiement": "USD",
  "nomEleve": "Amina K.",
  "libelleFrais": "Minerval",
  "dateCreation": "2026-09-24T10:20:00"
}
```

## 6. Regle multidevise

Le frontend doit distinguer les deux usages suivants :

| Champ | Usage |
|---|---|
| `ecole.devisePrincipale` | Devise a afficher pour les cartes et totaux |
| `resume.*` | Montants deja consolides dans la devise principale |
| `montant` | Montant original du paiement |
| `devise` | Code lisible de la devise source, par exemple `USD` |
| `codeDevisePaiement` | Code technique de la devise source |
| `codeDevisePrincipale` | Devise de consolidation de ce paiement |
| `montantDevisePrincipale` | Equivalent converti et arrondi |

Ne jamais additionner `montant` lorsque plusieurs devises sont presentes.
Pour un total local, additionner `montantDevisePrincipale` ou utiliser
directement `resume.montantTotal`.

Exemple d'affichage :

```text
50 USD (140 000 CDF)
Total de la journee : 350 000 CDF
```

## 7. Integration Vue 3

### 7.1 Types TypeScript

```ts
export interface DashboardPaiementRecent {
  idPaiement: number
  datePaiement: string
  montant: number
  devise?: string
  codeDevisePaiement?: string
  codeDevisePrincipale?: string
  montantDevisePrincipale: number
  modePaiement: string
  statut: string
  nomEleve?: string
  libelleFrais?: string
  referenceMoko?: string
}

export interface DashboardResponse {
  ecole: {
    idEcole: number
    nomEcole: string
    logo?: string
    devisePrincipale: string
  }
  idAnneeScolaire: number
  libelleAnneeScolaire?: string
  periode: {
    type: string
    date?: string
    dateDebut: string
    dateFin: string
    joursOuvrables: number
    libelle: string
  }
  resume: Record<string, number>
  repartitionParMode: {
    modes: Record<string, {
      nombre: number
      montant: number
      pourcentage: number
    }>
  }
  derniersPaiements: DashboardPaiementRecent[]
  moko: {
    estConfigure: boolean
    mesPayInsEnAttente: number
    payInsEnAttente: unknown[]
  }
  scope: 'moi' | 'ecole'
}
```

### 7.2 Service API et composable

```ts
const apiBaseUrl = import.meta.env.VITE_API_BASE_URL

export async function getDashboard(
  token: string,
  role: 'caissier' | 'financier',
  params: {
    idEcole: number
    idAnneeScolaire?: number
    date?: string
    scope?: 'moi' | 'ecole'
  }
): Promise<DashboardResponse> {
  const query = new URLSearchParams({ idEcole: String(params.idEcole) })
  if (params.idAnneeScolaire) query.set('idAnneeScolaire', String(params.idAnneeScolaire))
  if (params.date) query.set('date', params.date)
  if (params.scope) query.set('scope', params.scope)

  const response = await fetch(`${apiBaseUrl}/api/Dashboard/${role}?${query}`, {
    headers: { Authorization: `Bearer ${token}`, Accept: 'application/json' }
  })
  if (!response.ok) {
    const body = await response.json().catch(() => ({}))
    throw new Error(body.message ?? `Erreur dashboard (${response.status})`)
  }
  return response.json() as Promise<DashboardResponse>
}
```

Dans un composable, conserver distinctement `loading`, `error` et `data` :

```ts
const data = ref<DashboardResponse | null>(null)
const loading = ref(false)
const error = ref<string | null>(null)

async function refresh() {
  loading.value = true
  error.value = null
  try {
    data.value = await getDashboard(token.value, 'financier', {
      idEcole: auth.idEcole,
      date: selectedDate.value
    })
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Erreur inconnue'
  } finally {
    loading.value = false
  }
}
```

### 7.3 Formatage des montants

```ts
export function formatAmount(value: number, currency: string): string {
  return new Intl.NumberFormat('fr-FR', {
    style: 'currency',
    currency,
    maximumFractionDigits: 2
  }).format(value)
}

// formatAmount(data.resume.montantTotal, data.ecole.devisePrincipale)
```

Utiliser `montant` + `devise` pour la ligne source et
`montantDevisePrincipale` + `codeDevisePrincipale` pour l'equivalent.

## 8. Integration Flutter

### 8.1 Modeles Dart

```dart
class DashboardPaiementRecent {
  const DashboardPaiementRecent({
    required this.idPaiement,
    required this.montant,
    required this.montantDevisePrincipale,
    this.devise,
    this.codeDevisePaiement,
    this.codeDevisePrincipale,
  });

  final int idPaiement;
  final double montant;
  final double montantDevisePrincipale;
  final String? devise;
  final String? codeDevisePaiement;
  final String? codeDevisePrincipale;

  factory DashboardPaiementRecent.fromJson(Map<String, dynamic> json) {
    return DashboardPaiementRecent(
      idPaiement: json['idPaiement'] as int,
      montant: (json['montant'] as num).toDouble(),
      montantDevisePrincipale:
          (json['montantDevisePrincipale'] as num).toDouble(),
      devise: json['devise'] as String?,
      codeDevisePaiement: json['codeDevisePaiement'] as String?,
      codeDevisePrincipale: json['codeDevisePrincipale'] as String?,
    );
  }
}
```

Le modele de dashboard doit conserver au minimum `ecole`, `periode`, `resume`,
`repartitionParMode`, `derniersPaiements`, `moko` et `scope`. Les nombres JSON
se decodent en `num`, puis se convertissent en `double`.

### 8.2 Service HTTP

```dart
Future<Map<String, dynamic>> getDashboard({
  required String token,
  required String role,
  required int idEcole,
  int? idAnneeScolaire,
  DateTime? date,
  String? scope,
}) async {
  final query = <String, String>{
    'idEcole': '$idEcole',
    if (idAnneeScolaire != null)
      'idAnneeScolaire': '$idAnneeScolaire',
    if (date != null)
      'date': date.toIso8601String().split('T').first,
    if (scope != null) 'scope': scope,
  };
  final uri = Uri.parse('$apiBaseUrl/api/Dashboard/$role')
      .replace(queryParameters: query);
  final response = await http.get(uri, headers: {
    'Authorization': 'Bearer $token',
    'Accept': 'application/json',
  });
  final body = jsonDecode(response.body) as Map<String, dynamic>;
  if (response.statusCode < 200 || response.statusCode >= 300) {
    throw DashboardApiException(
      response.statusCode,
      body['message'] as String? ?? 'Erreur dashboard',
    );
  }
  return body;
}
```

### 8.3 Etat de l'ecran

L'ecran doit prevoir les etats `initial`, `loading`, `success`, `empty` et
`error`. Une reponse `200` avec `derniersPaiements: []` est un etat success
sans paiement recent, pas une erreur reseau.

Pour Riverpod, Provider ou Bloc, conserver le meme principe :

```dart
final devise = dashboard.ecole.devisePrincipale;
final total = formatAmount(dashboard.resume.montantTotal, devise);
final source = formatAmount(paiement.montant, paiement.devise ?? devise);
final converti = formatAmount(
  paiement.montantDevisePrincipale,
  paiement.codeDevisePrincipale ?? devise,
);
```

## 9. Erreurs, rafraichissement et performance

### Codes HTTP

| Code | Signification | Action frontend |
|---:|---|---|
| `200` | Dashboard disponible | Afficher les donnees |
| `400` | Utilisateur absent, parametre invalide ou scope invalide | Afficher un message corrigeable |
| `401` | JWT absent ou expire | Rafraichir le token ou rediriger vers login |
| `403` | Ecole ou role non autorise | Afficher acces refuse |
| `404` | Ecole ou annee introuvable | Recharger le contexte de selection |
| `500` | Erreur serveur | Conserver l'ecran precedent et proposer un retry |

### Bonnes pratiques

- Debouncer le changement de date et annuler la requete precedente si
  possible.
- Rafraichir apres un encaissement confirme, un retour Moko ou un changement
  de scope.
- Ne pas recalculer les totaux a partir des lignes si `resume` est fourni.
- Utiliser `genereLe` et `tousLesPaiements` uniquement pour les ecrans de
  cloture/impression.
- Afficher la devise principale dans le titre des cartes de total.
- Ne pas afficher un total converti sans son code devise.
- Journaliser le code HTTP et le contexte (role, ecole, date), sans journaliser
  le JWT.

## 10. Checklist

- [ ] Le token JWT est ajoute dans `Authorization: Bearer`.
- [ ] `idEcole` provient du contexte authentifie.
- [ ] La route canonique est utilisee (`caissier` ou `financier`).
- [ ] `idAnneeScolaire`, `date` et `scope` sont transmis si necessaire.
- [ ] Les cartes utilisent `ecole.devisePrincipale`.
- [ ] Les aggregats ne sont jamais calcules en additionnant `montant` de
  devises differentes.
- [ ] Une ligne recente affiche `montant` avec `devise`.
- [ ] L'equivalent affiche `montantDevisePrincipale` avec
  `codeDevisePrincipale`.
- [ ] Les listes vides, erreurs 401/403 et erreurs serveur sont gerees.
- [ ] Le bouton retry ne duplique pas les appels en cours.
- [ ] L'ecran de cloture utilise `tousLesPaiements` et `genereLe`.
- [ ] Les alias historiques ne sont utilises qu'en compatibilite.

