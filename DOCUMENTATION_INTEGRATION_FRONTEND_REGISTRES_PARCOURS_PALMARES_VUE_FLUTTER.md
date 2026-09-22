# Intégration Vue 3 & Flutter — Registres publics, Parcours scolaire & Palmarès

Guide d’intégration frontend pour les endpoints livrés récemment :

- Registres **anonymes** (vérification d’identité minimale)
- **ParcoursScolaire** (dossier complet authentifié)
- **PalmaresEcole** (classement officiel, bulletins figés)

**Base URL dev :** `https://localhost:7102` (local) / `https://dev-knb.asdc-rdc.org`  
**JSON :** camelCase  
**Auth :** `Authorization: Bearer {jwt}` **uniquement** pour ParcoursScolaire et PalmaresEcole

---

## Table des matières

1. [Périmètre](#1-périmètre)
2. [Endpoints](#2-endpoints)
3. [Règles d’accès](#3-règles-daccès)
4. [Contrats JSON](#4-contrats-json)
5. [Erreurs HTTP](#5-erreurs-http)
6. [Vue 3 — client API](#6-vue-3--client-api)
7. [Flutter — Dio](#7-flutter--dio)
8. [UX recommandée](#8-ux-recommandée)
9. [Checklist](#9-checklist)

---

## 1. Périmètre

| Inclus | Exclu |
|--------|--------|
| Recherche publique enseignants / élèves / écoles (champs minimaux) | Contacts, matricule élève en registre, idEcole en registre école |
| Dossier scolaire multi-écoles par matricule | Modification des données |
| Palmarès école (bulletins **figés** uniquement) | Calcul live / notes non figées |

**Ne pas confondre** avec `GET /api/Vitrine/partenaires` (logos marketing).

---

## 2. Endpoints

| Méthode | Route | Auth | Query principales |
|---------|-------|------|-------------------|
| `GET` | `/api/Agent/RegistreEnseignant` | **Anonyme** | `province?`, `ville?`, `limit` (défaut 50, max 100) |
| `GET` | `/api/Eleve/RegistreEleve` | **Anonyme** | **`nomComplet`** (min 3), `limit` (défaut 10, max 20) |
| `GET` | `/api/Ecole/RegistreEcole` | **Anonyme** | **`nom`** (min 3), `province?`, `ville?`, `limit` (défaut 10, max 20) |
| `GET` | `/api/Eleve/ParcoursScolaire` | JWT | **`matricule`** (obligatoire) |
| `GET` | `/api/Ecole/PalmaresEcole` | JWT | `idEcole?`, `idAnneeScolaire?`, `idPeriode?` / `periode?`, `idClasse?`, `idDirection?`, `limit` (défaut 20, max 100) |

### Exemples

```http
GET /api/Agent/RegistreEnseignant?province=Kinshasa&ville=Kinshasa&limit=20
GET /api/Eleve/RegistreEleve?nomComplet=Mukendi&limit=10
GET /api/Ecole/RegistreEcole?nom=Collège&ville=Kinshasa
GET /api/Eleve/ParcoursScolaire?matricule=MAT-001
Authorization: Bearer {jwt}

GET /api/Ecole/PalmaresEcole?idEcole=13&periode=T2&limit=20
Authorization: Bearer {jwt}
```

**Palmares — résolution :**
- `idEcole` : query **ou** claim JWT (`idEcole` / `EcoleId`). Super-Admin doit passer `?idEcole=`.
- `idAnneeScolaire` omis → année courante de l’école.
- Période omise → **dernière période figée** (Ordre max) pour le périmètre.
- `idClasse` prioritaire sur `idDirection`.

---

## 3. Règles d’accès

### Registres (anonymes)

Aucune authentification. Rate-limit côté API éventuel : respecter `limit` et min. 3 caractères.

### ParcoursScolaire

| Rôle | Règle |
|------|--------|
| `Super-Admin` | Accès total |
| `Admin`, `Directeur`, `Sous-Directeur` | Autorisé si l’élève a **≥ 1 inscription confirmée dans leur école** ; réponse = **parcours multi-écoles** |
| `Eleve` | Uniquement son propre `EleveId` |
| `Parent` | Uniquement enfants liés (`IdTuteur`) |
| Autres | **403** |

### PalmaresEcole

Rôles : `Super-Admin`, `Admin`, `Directeur`, `Sous-Directeur` uniquement.  
Scope école : `ForbidIfWrongSchool` (claim JWT vs `idEcole` query).

---

## 4. Contrats JSON

### 4.1 `RegistreEnseignantDto[]`

```json
[
  {
    "nom": "Mukendi",
    "postnom": "Kabongo",
    "prenom": "Jean",
    "province": "Kinshasa",
    "ville": "Kinshasa"
  }
]
```

Pas d’`idAgent`, email, téléphone, fonction.

### 4.2 `RegistreEleveDto[]`

```json
[
  {
    "nom": "Mukendi",
    "postnom": "Test",
    "prenom": "Jean",
    "nomClasse": "6e A",
    "nomEcole": "Ecole Alpha",
    "libelleAnneeScolaire": "2025-2026"
  }
]
```

Dernière inscription **confirmée**. Pas de matricule / `idEleve`.

### 4.3 `RegistreEcoleDto[]`

```json
[
  {
    "nom": "Collège Saint Joseph",
    "type": "Privée",
    "province": "Kinshasa",
    "ville": "Kinshasa",
    "commune": "Gombe",
    "provinceEducationnel": "Kinshasa"
  }
]
```

Pas d’`idEcole`, contacts, logo.

### 4.4 `ParcoursScolaireDto`

```json
{
  "eleve": {
    "idEleve": 1,
    "matricule": "MAT-001",
    "nom": "Mukendi",
    "postnom": "Test",
    "prenom": "Jean",
    "genre": "M",
    "dateNaissance": "2015-05-01T00:00:00",
    "nationalite": "RDC",
    "idTuteur": 1
  },
  "etapes": [
    {
      "idInscription": 2,
      "type": "Réinscription",
      "dateInscription": "2025-09-01T00:00:00",
      "statutInscription": "Confirmé",
      "idEcole": 13,
      "nomEcole": "Ecole Alpha",
      "idClasse": 10,
      "nomClasse": "6e A",
      "idAnneeScolaire": 100,
      "libelleAnneeScolaire": "2025-2026",
      "dateDebutAnnee": "2025-09-01T00:00:00",
      "dateFinAnnee": "2026-07-15T00:00:00",
      "bulletins": [],
      "notes": [],
      "paiements": [],
      "presences": []
    }
  ]
}
```

**Étapes :** inscriptions confirmées, tri année (`DateDebut`) puis `dateInscription`.  
Chaque étape embarque bulletins (`source`: `Fige` \| `Decision`), notes, paiements, présences (détail).

### 4.5 `PalmaresEcoleDto`

```json
{
  "idEcole": 13,
  "nomEcole": "Ecole Alpha",
  "idAnneeScolaire": 100,
  "libelleAnneeScolaire": "2025-2026",
  "idPeriode": 2,
  "codePeriode": "T2",
  "libellePeriode": "Trimestre 2",
  "idClasse": null,
  "nomClasse": null,
  "idDirection": null,
  "nomDirection": null,
  "effectifPrisEnCompte": 120,
  "limit": 20,
  "lignes": [
    {
      "rang": 1,
      "idEleve": 42,
      "matricule": "M1",
      "nom": "Alpha",
      "postnom": "Test",
      "prenom": "Jean",
      "nomComplet": "Alpha Test Jean",
      "idClasse": 10,
      "nomClasse": "6e A",
      "moyenneGenerale": 18.5,
      "decision": "Passe",
      "appreciationGenerale": null
    }
  ]
}
```

**Ex æquo :** même moyenne → même rang (ex. 1, 2, 2, 4).  
`effectifPrisEnCompte` = total classé ; `lignes` = top `limit`.

---

## 5. Erreurs HTTP

| Code | Cas typiques |
|------|----------------|
| `400` | `nomComplet` / `nom` &lt; 3 car. ; `matricule` vide ; période / classe invalide (Palmarès) |
| `401` | JWT manquant (Parcours / Palmarès) |
| `403` | Rôle non autorisé ; Parent/Élève hors périmètre ; mauvaise école |
| `404` | Matricule inconnu (ParcoursScolaire) |

Exemple :

```json
{ "message": "Le paramètre nomComplet est obligatoire (minimum 3 caractères)." }
```

---

## 6. Vue 3 — client API

```ts
// types/registre.ts
export interface RegistreEnseignant {
  nom?: string
  postnom?: string
  prenom?: string
  province?: string
  ville?: string
}

export interface RegistreEleve {
  nom?: string
  postnom?: string
  prenom?: string
  nomClasse?: string
  nomEcole?: string
  libelleAnneeScolaire?: string
}

export interface RegistreEcole {
  nom?: string
  type?: string
  province?: string
  ville?: string
  commune?: string
  provinceEducationnel?: string
}

export interface ParcoursScolaire {
  eleve: {
    idEleve: number
    matricule?: string
    nom?: string
    postnom?: string
    prenom?: string
    genre?: string
    dateNaissance: string
    nationalite?: string
    idTuteur?: number
  }
  etapes: ParcoursEtape[]
}

export interface ParcoursEtape {
  idInscription: number
  type?: string
  dateInscription: string
  statutInscription?: string
  idEcole: number
  nomEcole?: string
  idClasse: number
  nomClasse?: string
  idAnneeScolaire: number
  libelleAnneeScolaire?: string
  bulletins: unknown[]
  notes: unknown[]
  paiements: unknown[]
  presences: unknown[]
}

export interface PalmaresEcole {
  idEcole: number
  nomEcole?: string
  idAnneeScolaire: number
  libelleAnneeScolaire?: string
  idPeriode?: number
  codePeriode?: string
  libellePeriode?: string
  effectifPrisEnCompte: number
  limit: number
  lignes: PalmaresLigne[]
}

export interface PalmaresLigne {
  rang?: number
  idEleve: number
  matricule?: string
  nomComplet?: string
  nomClasse?: string
  moyenneGenerale?: number
  decision?: string
}
```

```ts
// api/registreApi.ts
import axios from 'axios'

const api = axios.create({ baseURL: import.meta.env.VITE_API_URL })

/** Anonyme — pas de Bearer */
export const fetchRegistreEnseignant = (params: {
  province?: string
  ville?: string
  limit?: number
}) => api.get<RegistreEnseignant[]>('/api/Agent/RegistreEnseignant', { params })

export const fetchRegistreEleve = (nomComplet: string, limit = 10) =>
  api.get<RegistreEleve[]>('/api/Eleve/RegistreEleve', {
    params: { nomComplet, limit },
  })

export const fetchRegistreEcole = (params: {
  nom: string
  province?: string
  ville?: string
  limit?: number
}) => api.get<RegistreEcole[]>('/api/Ecole/RegistreEcole', { params })

/** Authentifié */
export const fetchParcoursScolaire = (matricule: string, token: string) =>
  api.get<ParcoursScolaire>('/api/Eleve/ParcoursScolaire', {
    params: { matricule },
    headers: { Authorization: `Bearer ${token}` },
  })

export const fetchPalmaresEcole = (
  token: string,
  params: {
    idEcole?: number
    idAnneeScolaire?: number
    idPeriode?: number
    periode?: string
    idClasse?: number
    idDirection?: number
    limit?: number
  } = {},
) =>
  api.get<PalmaresEcole>('/api/Ecole/PalmaresEcole', {
    params,
    headers: { Authorization: `Bearer ${token}` },
  })
```

**Debounce** recommandé sur les champs `nom` / `nomComplet` (≥ 300 ms, déclencher seulement si longueur ≥ 3).

---

## 7. Flutter — Dio

```dart
// models/registre_models.dart
class RegistreEleve {
  final String? nom;
  final String? postnom;
  final String? prenom;
  final String? nomClasse;
  final String? nomEcole;
  final String? libelleAnneeScolaire;

  RegistreEleve.fromJson(Map<String, dynamic> j)
      : nom = j['nom'] as String?,
        postnom = j['postnom'] as String?,
        prenom = j['prenom'] as String?,
        nomClasse = j['nomClasse'] as String?,
        nomEcole = j['nomEcole'] as String?,
        libelleAnneeScolaire = j['libelleAnneeScolaire'] as String?;
}

class PalmaresLigne {
  final int? rang;
  final int idEleve;
  final String? matricule;
  final String? nomComplet;
  final double? moyenneGenerale;

  PalmaresLigne.fromJson(Map<String, dynamic> j)
      : rang = j['rang'] as int?,
        idEleve = j['idEleve'] as int,
        matricule = j['matricule'] as String?,
        nomComplet = j['nomComplet'] as String?,
        moyenneGenerale = (j['moyenneGenerale'] as num?)?.toDouble();
}
```

```dart
// services/registre_api_service.dart
class RegistreApiService {
  RegistreApiService(this._dio);
  final Dio _dio;

  Future<List<dynamic>> registreEnseignant({
    String? province,
    String? ville,
    int limit = 50,
  }) async {
    final res = await _dio.get(
      '/api/Agent/RegistreEnseignant',
      queryParameters: {
        if (province != null) 'province': province,
        if (ville != null) 'ville': ville,
        'limit': limit,
      },
      options: Options(extra: {'skipAuth': true}), // selon votre intercepteur
    );
    return res.data as List<dynamic>;
  }

  Future<List<RegistreEleve>> registreEleve(String nomComplet, {int limit = 10}) async {
    final res = await _dio.get(
      '/api/Eleve/RegistreEleve',
      queryParameters: {'nomComplet': nomComplet, 'limit': limit},
      options: Options(extra: {'skipAuth': true}),
    );
    return (res.data as List)
        .map((e) => RegistreEleve.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  Future<Map<String, dynamic>> parcoursScolaire(String matricule) async {
    final res = await _dio.get(
      '/api/Eleve/ParcoursScolaire',
      queryParameters: {'matricule': matricule},
    );
    return res.data as Map<String, dynamic>;
  }

  Future<Map<String, dynamic>> palmaresEcole({
    int? idEcole,
    int? idAnneeScolaire,
    String? periode,
    int? idClasse,
    int? idDirection,
    int limit = 20,
  }) async {
    final res = await _dio.get(
      '/api/Ecole/PalmaresEcole',
      queryParameters: {
        if (idEcole != null) 'idEcole': idEcole,
        if (idAnneeScolaire != null) 'idAnneeScolaire': idAnneeScolaire,
        if (periode != null) 'periode': periode,
        if (idClasse != null) 'idClasse': idClasse,
        if (idDirection != null) 'idDirection': idDirection,
        'limit': limit,
      },
    );
    return res.data as Map<String, dynamic>;
  }
}
```

Pour les appels anonymes : configurer l’intercepteur Dio pour **ne pas** envoyer de Bearer (ou ignorer 401) lorsque `skipAuth: true`.

---

## 8. UX recommandée

| Écran | Comportement |
|-------|----------------|
| Registre enseignant / élève / école | Champ recherche + debounce ; message si &lt; 3 car. ; liste compacte sans IDs sensibles |
| Parcours scolaire | Saisie matricule (staff) ou auto (élève connecté) ; timeline d’étapes ; onglets Notes / Paiements / Présences / Bulletins |
| Palmarès | Sélecteurs année / période / classe ; tableau rang · nom · classe · moyenne ; badge ex æquo |

**Routes suggérées**

| Écran | Vue Router | GoRouter |
|-------|------------|----------|
| Registre enseignant | `/public/registre-enseignant` | `public/registre-enseignant` |
| Registre élève | `/public/registre-eleve` | `public/registre-eleve` |
| Registre école | `/public/registre-ecole` | `public/registre-ecole` |
| Parcours | `/eleves/parcours` | `eleves/parcours` |
| Palmarès | `/ecole/palmares` | `ecole/palmares` |

---

## 9. Checklist

- [ ] Registres anonymes : pas de token ; validation min. 3 caractères côté UI
- [ ] Registre élève / école : ne pas afficher ni stocker de matricule / idEcole issus de ces endpoints
- [ ] Parcours : gérer `404` (inconnu) vs `403` (pas le droit)
- [ ] Parent : ne proposer que les matricules des enfants liés
- [ ] Élève : appeler avec son matricule uniquement (ou masquer le champ)
- [ ] Palmarès : Super-Admin envoie toujours `idEcole` ; afficher `codePeriode` résolu (P1)
- [ ] Palmarès : `idClasse` prioritaire si les deux filtres sont remplis
- [ ] Debounce + `limit` pour limiter la charge réseau
- [ ] Messages d’erreur API (`message`) affichés à l’utilisateur

---

*Document aligné sur l’API KelasiNaBiso — septembre 2026.*
