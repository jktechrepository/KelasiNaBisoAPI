# Documentation Frontend — Filtre date des devoirs à domicile

Guide d’intégration **Vue 3** (web admin) et **Flutter** (mobile élève / parent / enseignant).

**Changement :** rétrocompatible. Les listes existantes restent valides **sans** query date.

**Base URL dev :** `https://localhost:7102` ou `https://dev-knb.asdc-rdc.org`  
**Swagger :** `/swagger/index.html`  
**Auth :** `Authorization: Bearer {jwt_token}`

Références :
- [DOCUMENTATION_FRONTEND_ROLE_ELEVE.md](DOCUMENTATION_FRONTEND_ROLE_ELEVE.md) — parcours élève
- Swagger tag **DevoirADomicile**

---

## Table des matières

1. [Vue d’ensemble](#1-vue-densemble)
2. [Endpoints concernés](#2-endpoints-concernés)
3. [Paramètres `dateDebut` / `dateFin`](#3-paramètres-datedebut--datefin)
4. [Exemples d’URLs](#4-exemples-durls)
5. [Réponses JSON](#5-réponses-json)
6. [Erreurs](#6-erreurs)
7. [Recommandations UI](#7-recommandations-ui)
8. [Vue 3 — types et service](#8-vue-3--types-et-service)
9. [Flutter — modèles et service](#9-flutter--modèles-et-service)
10. [Checklist](#10-checklist)

---

## 1. Vue d’ensemble

Les GET de **liste** des devoirs peuvent filtrer sur **`datePublication`** (jour où le devoir apparaît dans l’app), pas sur `dateLimite` (date de rendu).

| Besoin UI | Query API |
|-----------|-----------|
| « Publiés aujourd’hui / cette semaine / ce mois » | `dateDebut` + `dateFin` sur **DatePublication** |
| Calendrier « à rendre le … » | **Non disponible** — `dateLimite` n’est pas filtrée |

Sans `dateDebut` ni `dateFin` : même résultat qu’avant (année scolaire déjà scopée).

Un seul des deux paramètres suffit (borne ouverte). Les deux jours sont **inclusifs**.

`GET /api/DevoirADomicile/{id}/telecharger` : **inchangé**, pas de filtre date.

---

## 2. Endpoints concernés

| Méthode | Route | Rôles JWT | Usage front |
|---------|-------|-----------|-------------|
| GET | `/api/DevoirADomicile/mes-devoirs` | Enseignant, Directeur, Admin, Super-Admin | Liste « mes publications » / école |
| GET | `/api/DevoirADomicile/eleve/moi` | Eleve | Liste élève connecté |
| GET | `/api/DevoirADomicile/classe/{idClasse}` | Parent, Eleve, Enseignant, Directeur, Admin, Super-Admin | Liste d’une classe |
| GET | `/api/DevoirADomicile/tuteur/{idTuteur}` | Parent, Enseignant, Directeur, Admin, Super-Admin | Parent : `idTuteur` = claim JWT uniquement |

Tous acceptent en plus les query déjà existants (`pageNumber`, `pageSize`, `searchTerm`, `idAnneeScolaire`, etc.).

---

## 3. Paramètres `dateDebut` / `dateFin`

| Query | Type | Obligatoire | Format |
|-------|------|-------------|--------|
| `dateDebut` | date | Non | `yyyy-MM-dd` |
| `dateFin` | date | Non | `yyyy-MM-dd` |

### Règles

- Filtre sur **`datePublication`**, jour calendaire inclus.
- Un devoir publié le **10/03/2026 à 08:30** est inclus si `dateDebut=2026-03-10` et/ou `dateFin=2026-03-10`.
- `dateDebut` seul : publications **à partir de** ce jour (00:00).
- `dateFin` seul : publications **jusqu’à** ce jour (fin de journée).
- `dateDebut` **et** `dateFin` : intervalle fermé.
- Si `dateDebut` > `dateFin` → **400** `{ "message": "dateDebut ne peut pas être postérieure à dateFin." }`
- Valider la plage **côté UI** avant l’appel (évite un round-trip).

### Encodage

```
dateDebut=2026-03-10&dateFin=2026-03-17
```

Ne pas envoyer d’heure (`T08:00:00`) : le backend tronque au jour, mais le contrat front est **date seule**.

Ne pas envoyer `dateDebut=` vide : omettre le paramètre.

---

## 4. Exemples d’URLs

Aujourd’hui (jour J) :

```
GET /api/DevoirADomicile/eleve/moi?pageNumber=1&pageSize=15&dateDebut=2026-03-10&dateFin=2026-03-10
```

Semaine (lundi → dimanche) :

```
GET /api/DevoirADomicile/mes-devoirs?pageNumber=1&pageSize=15&dateDebut=2026-03-09&dateFin=2026-03-15
```

Depuis une date (borne ouverte) :

```
GET /api/DevoirADomicile/classe/10?idAnneeScolaire=101&dateDebut=2026-03-01
```

Parent :

```
GET /api/DevoirADomicile/tuteur/42?pageNumber=1&pageSize=15&dateDebut=2026-03-10&dateFin=2026-03-10
```

Sans filtre date (comportement historique) :

```
GET /api/DevoirADomicile/mes-devoirs?pageNumber=1&pageSize=15
```

---

## 5. Réponses JSON

Les enveloppes **ne changent pas**. Le filtre réduit seulement `data`.

### `mes-devoirs` et `eleve/moi`

`ElevesAnneeScopedResult<PagedResult<DevoirADomicileDto>>` :

```json
{
  "data": {
    "data": [
      {
        "idDevoirADomicile": 1,
        "titre": "Exercices maths",
        "datePublication": "2026-03-10T08:30:00",
        "dateLimite": "2026-03-17T23:59:59",
        "estTelechargeParMoi": false
      }
    ],
    "pageNumber": 1,
    "pageSize": 15,
    "totalRecords": 1,
    "totalPages": 1
  },
  "idEcole": 1,
  "idAnneeScolaire": 101,
  "libelleAnneeScolaire": "2025-2026"
}
```

Afficher **`datePublication`** comme date du filtre. Afficher **`dateLimite`** comme « À rendre le » (informatif seulement).

### `classe/{idClasse}`

Toujours un wrapper `{ data, idEcole, idAnneeScolaire }` :

- **Sans** `pageNumber` / `pageSize` / `searchTerm` / `sortBy` : `data` = **tableau** de devoirs.
- **Avec** pagination : `data` = **page courante** (tableau), **sans** `totalRecords` au premier niveau. Pour une UI paginée, préférer `eleve/moi` ou `mes-devoirs`.

### `tuteur/{idTuteur}`

`PagedResult` **sans** wrapper année :

```json
{
  "data": [ { "idDevoirADomicile": 1, "titre": "…", "datePublication": "2026-03-10T08:30:00" } ],
  "pageNumber": 1,
  "pageSize": 15,
  "totalRecords": 1
}
```

---

## 6. Erreurs

| HTTP | Cas | Action UI |
|------|-----|-----------|
| 200 | Liste vide | Empty state « Aucun devoir sur cette période » |
| 400 | `dateDebut` > `dateFin` | Message du champ `message` |
| 400 | Super-Admin sans `idEcole` ni `idAnneeScolaire` (`mes-devoirs`) | Existant : préciser l’école ou l’année |
| 403 | Parent avec un autre `idTuteur` | Ne pas exposer le sélecteur d’id |
| 401 | JWT manquant / expiré | Relogin |

---

## 7. Recommandations UI

1. **Date range picker** (ou deux champs date) + bouton « Appliquer ».
2. Raccourcis : Aujourd’hui / 7 derniers jours / Ce mois — calculer `yyyy-MM-dd` en **local** (fuseau utilisateur).
3. Chip « Toutes les dates » = **omettre** les deux query.
4. Réinitialiser `pageNumber` à **1** quand la plage change.
5. Ne pas filtrer `dateLimite` en local en croyant que l’API a déjà filtré la deadline.
6. Téléchargement : toujours `GET .../{id}/telecharger` sans dates.

---

## 8. Vue 3 — types et service

```ts
export type DevoirDateFilter = {
  dateDebut?: string | null // 'YYYY-MM-DD'
  dateFin?: string | null
}

function toQueryDate(d: Date): string {
  const y = d.getFullYear()
  const m = String(d.getMonth() + 1).padStart(2, '0')
  const day = String(d.getDate()).padStart(2, '0')
  return `${y}-${m}-${day}`
}

export function assertPlageValide(f: DevoirDateFilter): void {
  if (f.dateDebut && f.dateFin && f.dateDebut > f.dateFin) {
    throw new Error('La date de début ne peut pas être postérieure à la date de fin.')
  }
}

function appendDateQuery(params: URLSearchParams, f?: DevoirDateFilter) {
  if (f?.dateDebut) params.set('dateDebut', f.dateDebut)
  if (f?.dateFin) params.set('dateFin', f.dateFin)
}

export async function fetchMesDevoirsEleve(
  token: string,
  opts: { pageNumber?: number; pageSize?: number; idAnneeScolaire?: number } & DevoirDateFilter
) {
  assertPlageValide(opts)
  const params = new URLSearchParams()
  params.set('pageNumber', String(opts.pageNumber ?? 1))
  params.set('pageSize', String(opts.pageSize ?? 15))
  if (opts.idAnneeScolaire) params.set('idAnneeScolaire', String(opts.idAnneeScolaire))
  appendDateQuery(params, opts)

  const res = await fetch(
    `${import.meta.env.VITE_API_BASE}/api/DevoirADomicile/eleve/moi?${params}`,
    { headers: { Authorization: `Bearer ${token}`, Accept: 'application/json' } }
  )
  if (!res.ok) {
    const body = await res.json().catch(() => ({}))
    throw new Error(body.message ?? `HTTP ${res.status}`)
  }
  return res.json()
}
```

Raccourci « aujourd’hui » :

```ts
const j = toQueryDate(new Date())
await fetchMesDevoirsEleve(token, { dateDebut: j, dateFin: j })
```

Même `appendDateQuery` pour `mes-devoirs`, `classe/{id}` et `tuteur/{id}`.

---

## 9. Flutter — modèles et service

```dart
String toQueryDate(DateTime d) =>
    '${d.year.toString().padLeft(4, '0')}-'
    '${d.month.toString().padLeft(2, '0')}-'
    '${d.day.toString().padLeft(2, '0')}';

class DevoirDateFilter {
  final DateTime? dateDebut;
  final DateTime? dateFin;
  const DevoirDateFilter({this.dateDebut, this.dateFin});

  void validate() {
    if (dateDebut != null &&
        dateFin != null &&
        dateDebut!.isAfter(DateTime(dateFin!.year, dateFin!.month, dateFin!.day))) {
      throw ArgumentError('dateDebut ne peut pas être postérieure à dateFin.');
    }
  }

  void applyTo(Map<String, String> q) {
    if (dateDebut != null) q['dateDebut'] = toQueryDate(dateDebut!);
    if (dateFin != null) q['dateFin'] = toQueryDate(dateFin!);
  }
}
```

```dart
Future<Map<String, dynamic>> fetchDevoirsEleveMoi({
  required String token,
  required Uri base,
  int pageNumber = 1,
  int pageSize = 15,
  DevoirDateFilter? dates,
}) async {
  dates?.validate();
  final q = <String, String>{
    'pageNumber': '$pageNumber',
    'pageSize': '$pageSize',
  };
  dates?.applyTo(q);
  final uri = base.replace(
    path: '/api/DevoirADomicile/eleve/moi',
    queryParameters: q,
  );
  final res = await http.get(uri, headers: {
    'Authorization': 'Bearer $token',
    'Accept': 'application/json',
  });
  if (res.statusCode != 200) {
    throw Exception(res.body);
  }
  return jsonDecode(res.body) as Map<String, dynamic>;
}
```

Parser `datePublication` pour l’affichage liste ; ne pas confondre avec `dateLimite`.

---

## 10. Checklist

- [ ] Date range optionnel ; sans dates = liste actuelle
- [ ] Query `yyyy-MM-dd` uniquement
- [ ] Blocage UI si début > fin
- [ ] Empty state période vs empty state « aucun devoir cette année »
- [ ] Élève : `eleve/moi` (pas `tuteur/{id}`)
- [ ] Parent : `tuteur/{idTuteurJwt}`
- [ ] Enseignant / admin : `mes-devoirs`
- [ ] Télécharger : pas de dates
- [ ] Pagination : reset page 1 au changement de plage

**Swagger :** `https://localhost:7102/swagger/index.html` — tag `DevoirADomicile`.
