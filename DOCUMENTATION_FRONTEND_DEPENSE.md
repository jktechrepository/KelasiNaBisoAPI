# Documentation Frontend — Module Dépenses (KelasiNaBiso V1)

Guide d’intégration Flutter / Vue pour les sorties d’argent (dépenses scolaires).

**Spec portable (origine) :** [DOCUMENTATION_PORTABLE_INTEGRATION_DEPENSE_FROM_Kenergie.md](DOCUMENTATION_PORTABLE_INTEGRATION_DEPENSE_FROM_Kenergie.md)  
**SQL prod :** [docs/sql/20260916_AddDepenses.sql](docs/sql/20260916_AddDepenses.sql)

**Base URL dev :** `https://dev-knb.asdc-rdc.org` (local : `https://localhost:7102`)  
**Auth :** `Authorization: Bearer {accessToken}`  
**JSON :** camelCase

---

## Table des matières

1. [Vue d’ensemble et écarts vs Kenergie](#1-vue-densemble-et-écarts-vs-kenergie)
2. [Workflow métier](#2-workflow-métier)
3. [Permissions et gating UI](#3-permissions-et-gating-ui)
4. [Catégories de dépense](#4-catégories-de-dépense)
5. [Dépenses — CRUD et actions](#5-dépenses--crud-et-actions)
6. [Rapport du mois](#6-rapport-du-mois)
7. [Multi-devise](#7-multi-devise)
8. [Écrans suggérés](#8-écrans-suggérés)
9. [Erreurs fréquentes](#9-erreurs-fréquentes)
10. [Clients HTTP minimaux](#10-clients-http-minimaux)
11. [Checklist d’intégration](#11-checklist-dintégration)
12. [Fichiers API de référence](#12-fichiers-api-de-référence)

---

## 1. Vue d’ensemble et écarts vs Kenergie

| Concept | Champ API Kelasi |
|---------|------------------|
| Organisation / tenant | **`idEcole`** (pas `idSociete`) |
| Dépense | ressource `/api/Depense` |
| Catégorie | ressource `/api/CategorieDepense` |
| Statuts workflow | `Validee` \| `Annulee` (`EnAttente` réservé, non produit en V1) |

| Kenergie (portable) | Kelasi V1 |
|---------------------|-----------|
| Create → `EnAttente` → Validate | **Create → `Validee` immédiatement** |
| Snapshot devise à la validation | **Snapshot à la création** |
| `/valider`, `/refuser`, `Depense.Validate` | **Absents** |
| Caissier sans accès | **Caissier : Create, Read, ReadAll** |
| Pagination `totalCount` | **`PagedResult`** : `totalRecords`, `hasPrevious`, `hasNext`, … |

---

## 2. Workflow métier

```
[*] --> Validee : POST /api/Depense
Validee --> Annulee : POST /api/Depense/{id}/annuler
```

| Règle | Conséquence UI |
|-------|----------------|
| Création toujours `Validee` | Pas d’écran « file d’attente validation » |
| Seules les `Validee` dans les totaux par défaut | Rapport mois défaut `statut=Validee` |
| **Montant immuable** après création | Correction = annuler + recréer (pas de champ montant en PUT) |
| Soft delete | `DELETE` masque la ligne (`actif=false`) — réservé Admin / Directeur |

---

## 3. Permissions et gating UI

Afficher un bouton seulement si la permission est dans le JWT / profil (`permissions[]`).

| Permission | Capacité |
|------------|----------|
| `Depense.Read` | Détail |
| `Depense.ReadAll` | Liste paginée, rapport mois |
| `Depense.Create` | Création |
| `Depense.Update` | Modifier métadonnées (si `Validee`) + **annuler** |
| `Depense.Delete` | Soft delete |
| `CategorieDepense.Read` / `ReadAll` | Détail / liste catégories |
| `CategorieDepense.Create` / `Update` / `Delete` | CRUD catégories |

### Matrice rôles

| Rôle | Depense | CategorieDepense |
|------|---------|------------------|
| Financier | Create, Update, Read, ReadAll | Read, ReadAll, Create, Update |
| Caissier | Create, Read, ReadAll | Read, ReadAll |
| Directeur | Read, ReadAll, Update, Delete | toutes |
| Admin / Super-Admin | toutes | toutes |
| Sous-Directeur | Read, ReadAll, Update | Read, ReadAll |
| Eleve / Parent / Enseignant | **aucun** | **aucun** |

**Boutons typiques :**

| Action UI | Permission | Qui |
|-----------|------------|-----|
| Créer dépense | `Depense.Create` | Financier, Caissier, Admin |
| Annuler | `Depense.Update` | Financier, Directeur, Admin (pas Caissier) |
| Soft delete | `Depense.Delete` | Directeur, Admin |
| Gérer catégories | `CategorieDepense.Create/Update` | Financier, Directeur, Admin |

Scoping école : `idEcole` query/body doit correspondre à l’école JWT (sinon **403**).

---

## 4. Catégories de dépense

| Méthode | Route | Permission |
|---------|-------|------------|
| `GET` | `/api/CategorieDepense/ecole/{idEcole}?includeInactive=` | `CategorieDepense.ReadAll` |
| `GET` | `/api/CategorieDepense/{id}` | `CategorieDepense.Read` |
| `POST` | `/api/CategorieDepense` | `CategorieDepense.Create` |
| `PUT` | `/api/CategorieDepense/{id}` | `CategorieDepense.Update` |
| `DELETE` | `/api/CategorieDepense/{id}` | `CategorieDepense.Delete` → soft (`statut=false`) |

### Création

```http
POST /api/CategorieDepense
Content-Type: application/json
```

```json
{
  "idEcole": 1,
  "nomCategorie": "Loyer",
  "description": "Loyer locaux"
}
```

### Réponse

```json
{
  "idCategorieDepense": 2,
  "idEcole": 1,
  "nomCategorie": "Loyer",
  "description": "Loyer locaux",
  "statut": true,
  "dateCreation": "2026-09-17T10:00:00Z"
}
```

### Mise à jour

```json
{
  "nomCategorie": "Loyer & charges",
  "description": null,
  "statut": true
}
```

Pour le formulaire de création de dépense : charger `GET .../ecole/{idEcole}` (**sans** `includeInactive`, défaut = catégories actives seulement).

---

## 5. Dépenses — CRUD et actions

| Méthode | Route | Permission |
|---------|-------|------------|
| `GET` | `/api/Depense` | `Depense.ReadAll` |
| `GET` | `/api/Depense/mois` | `Depense.ReadAll` |
| `GET` | `/api/Depense/{id}` | `Depense.Read` |
| `POST` | `/api/Depense` | `Depense.Create` → **`statut: Validee`** + snapshot devise |
| `PUT` | `/api/Depense/{id}` | `Depense.Update` (métadonnées si `Validee`, **pas de montant**) |
| `POST` | `/api/Depense/{id}/annuler` | `Depense.Update` |
| `DELETE` | `/api/Depense/{id}` | `Depense.Delete` → **204** |

### 5.1 Liste paginée

```http
GET /api/Depense?idEcole=1&dateDebut=&dateFin=&idCategorieDepense=&statut=&pageNumber=1&pageSize=20&searchTerm=&sortDescending=true
```

| Query | Description |
|-------|-------------|
| `idEcole` | Requis (ou déduit du JWT école) |
| `dateDebut` / `dateFin` | Filtre `dateDepense` |
| `idCategorieDepense` | Optionnel |
| `statut` | `Validee` \| `Annulee` \| `EnAttente` \| `Tous` |
| `pageNumber` / `pageSize` | Pagination (max 100) |
| `searchTerm` | Libellé, bénéficiaire, référence, description |
| `sortDescending` | Tri par `dateDepense` |

Réponse `PagedResult` :

```json
{
  "data": [ /* DepenseDto[] */ ],
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 3,
  "totalRecords": 42,
  "hasPrevious": false,
  "hasNext": true
}
```

### 5.2 Création

```json
POST /api/Depense
{
  "idEcole": 1,
  "idCategorieDepense": 2,
  "libelle": "Achat fournitures",
  "description": null,
  "beneficiaire": "Fournisseur X",
  "referencePiece": "FAC-2026-014",
  "montant": 150000,
  "codeDeviseMontant": "CDF",
  "modePaiement": "Espèces",
  "dateDepense": "2026-09-16T10:00:00Z"
}
```

Réponse typique (`DepenseDto`) :

```json
{
  "idDepense": 12,
  "idEcole": 1,
  "idCategorieDepense": 2,
  "nomCategorie": "Fournitures",
  "libelle": "Achat fournitures",
  "montant": 150000,
  "codeDeviseMontant": "CDF",
  "codeDevisePrincipale": "USD",
  "tauxVersDevisePrincipale": 0.0004,
  "montantDevisePrincipale": 60,
  "modePaiement": "Espèces",
  "dateDepense": "2026-09-16T10:00:00Z",
  "statut": "Validee",
  "idUtilisateurCreateur": 5,
  "nomCreateur": "Jean Dupont",
  "dateCreation": "2026-09-16T10:01:00Z"
}
```

### 5.3 Mise à jour (métadonnées uniquement)

```json
PUT /api/Depense/12
{
  "libelle": "Achat fournitures (corrigé)",
  "beneficiaire": "Fournisseur Y",
  "referencePiece": "FAC-2026-014-B",
  "modePaiement": "Mobile Money",
  "idCategorieDepense": 2,
  "dateDepense": "2026-09-16T10:00:00Z"
}
```

**Ne pas envoyer `montant`.** Statut doit être `Validee` sinon **400**.

### 5.4 Annuler

```json
POST /api/Depense/12/annuler
{ "motifAnnulation": "Doublon de saisie" }
```

Passe `statut` à `Annulee` ; hors totaux du rapport mois (défaut).

---

## 6. Rapport du mois

```http
GET /api/Depense/mois?mois=9&annee=2026&idEcole=1&statut=Validee
```

| Query | Défaut |
|-------|--------|
| `mois` | mois UTC courant (1–12) |
| `annee` | année UTC courante |
| `idEcole` | école JWT si omis |
| `statut` | **`Validee`** |

Autres `statut` : `Annulee`, `EnAttente`, `Tous`.

Liste **non paginée**. `syntheseDepense` porte sur **les lignes affichées**.

```json
{
  "mois": 9,
  "annee": 2026,
  "dateDebut": "2026-09-01T00:00:00Z",
  "dateFin": "2026-09-30T23:59:59.9999999Z",
  "depenses": [],
  "syntheseDepense": {
    "montantTotal": 150000,
    "nombreDepenses": 8,
    "nombreValidees": 8,
    "nombreEnAttente": 0,
    "nombreAnnulees": 0
  }
}
```

`montantTotal` = somme des `montantDevisePrincipale` (fallback `montant`) des lignes retournées.

Carte KPI recommandée : `syntheseDepense.montantTotal` + devise principale de l’école.

---

## 7. Multi-devise

Prérequis école :

- `Ecole.codeDevisePrincipale` (ex. `USD`)
- Devise saisie active dans `DeviseMonetaire` pour l’école
- Taux `TauxChange` si `codeDeviseMontant` ≠ devise principale (sinon **400**)

Affichage :

- Toujours : `montant` + `codeDeviseMontant`
- Si `statut === "Validee"` et `montantDevisePrincipale` renseigné avec devise différente : afficher aussi l’équivalent principal (`tauxVersDevisePrincipale`)

Catalogues devises : endpoints existants `GET /api/Devise` (selon doc devises du projet).

---

## 8. Écrans suggérés

1. **Liste** — `GET /api/Depense` + filtres période / catégorie / statut + recherche  
2. **Rapport mois** — `GET /mois` + carte `syntheseDepense.montantTotal`  
3. **Création** — catégories actives + devises école → `POST /api/Depense`  
4. **Détail** — `GET /{id}` ; actions Annuler / Modifier selon permissions et `statut`  
5. **Catégories** — CRUD si `CategorieDepense.*`  
6. **Annulées** — liste ou mois avec `statut=Annulee` (audit)

Pas d’écran « à valider » en V1.

---

## 9. Erreurs fréquentes

Corps typique : `{ "message": "..." }`

| HTTP | Cas | UI |
|------|-----|-----|
| 403 | Mauvaise école / permission | Masquer l’action |
| 400 | Catégorie inactive, devise/taux manquant, PUT sur non-`Validee`, `statut` query invalide | Message + recharger |
| 400 | Tentative implicite de changer le montant | Expliquer : annuler + recréer |
| 404 | Dépense / catégorie introuvable | Retour liste |
| 204 | Soft delete OK | Rafraîchir la liste |

---

## 10. Clients HTTP minimaux

### TypeScript

```ts
export const depenseApi = {
  list: (params?: Record<string, unknown>) =>
    api.get('/api/Depense', { params }),
  get: (id: number) => api.get(`/api/Depense/${id}`),
  mois: (params?: { mois?: number; annee?: number; idEcole?: number; statut?: string }) =>
    api.get('/api/Depense/mois', { params }),
  create: (body: Record<string, unknown>) => api.post('/api/Depense', body),
  update: (id: number, body: Record<string, unknown>) =>
    api.put(`/api/Depense/${id}`, body),
  annuler: (id: number, motifAnnulation?: string) =>
    api.post(`/api/Depense/${id}/annuler`, { motifAnnulation }),
  remove: (id: number) => api.delete(`/api/Depense/${id}`),
}

export const categorieDepenseApi = {
  byEcole: (idEcole: number, includeInactive = false) =>
    api.get(`/api/CategorieDepense/ecole/${idEcole}`, {
      params: { includeInactive },
    }),
  get: (id: number) => api.get(`/api/CategorieDepense/${id}`),
  create: (body: Record<string, unknown>) =>
    api.post('/api/CategorieDepense', body),
  update: (id: number, body: Record<string, unknown>) =>
    api.put(`/api/CategorieDepense/${id}`, body),
  remove: (id: number) => api.delete(`/api/CategorieDepense/${id}`),
}
```

### Dart (Dio)

```dart
class DepenseApi {
  DepenseApi(this._dio);
  final Dio _dio;

  Future<Map<String, dynamic>> list({Map<String, dynamic>? params}) async {
    final r = await _dio.get('/api/Depense', queryParameters: params);
    return r.data as Map<String, dynamic>;
  }

  Future<Map<String, dynamic>> mois({
    int? mois,
    int? annee,
    int? idEcole,
    String? statut,
  }) async {
    final r = await _dio.get('/api/Depense/mois', queryParameters: {
      if (mois != null) 'mois': mois,
      if (annee != null) 'annee': annee,
      if (idEcole != null) 'idEcole': idEcole,
      if (statut != null) 'statut': statut,
    });
    return r.data as Map<String, dynamic>;
  }

  Future<Map<String, dynamic>> create(Map<String, dynamic> body) async {
    final r = await _dio.post('/api/Depense', data: body);
    return r.data as Map<String, dynamic>;
  }

  Future<Map<String, dynamic>> update(int id, Map<String, dynamic> body) async {
    final r = await _dio.put('/api/Depense/$id', data: body);
    return r.data as Map<String, dynamic>;
  }

  Future<Map<String, dynamic>> annuler(int id, {String? motif}) async {
    final r = await _dio.post('/api/Depense/$id/annuler', data: {
      'motifAnnulation': motif,
    });
    return r.data as Map<String, dynamic>;
  }

  Future<void> remove(int id) async {
    await _dio.delete('/api/Depense/$id');
  }
}

class CategorieDepenseApi {
  CategorieDepenseApi(this._dio);
  final Dio _dio;

  Future<List<dynamic>> byEcole(int idEcole, {bool includeInactive = false}) async {
    final r = await _dio.get(
      '/api/CategorieDepense/ecole/$idEcole',
      queryParameters: {'includeInactive': includeInactive},
    );
    return r.data as List<dynamic>;
  }
}
```

---

## 11. Checklist d’intégration

- [ ] JWT transmis ; UI pilotée par `Depense.*` / `CategorieDepense.*`
- [ ] Création → `statut === "Validee"` ; apparaît dans `GET /mois` (défaut)
- [ ] Annulation → `Annulee` ; hors totaux par défaut
- [ ] Totaux / KPI : uniquement lignes `Validee` (sauf filtre explicite)
- [ ] Pas de modification du montant après création
- [ ] `idEcole` partout (pas `idSociete`)
- [ ] Pagination : lire `totalRecords` / `hasNext` (pas `totalCount`)
- [ ] Devises école actives + taux avant create multi-devise
- [ ] États loading / empty / error

---

## 12. Fichiers API de référence

| Zone | Fichiers |
|------|----------|
| Controllers | `Controllers/DepenseController.cs`, `Controllers/CategorieDepenseController.cs` |
| Services | `Services/DepenseService.cs`, `Services/CategorieDepenseService.cs` |
| DTOs | `Models/DTOs/Depense/DepenseDtos.cs` |
| SQL | `docs/sql/20260916_AddDepenses.sql` |
| Spec portable | `DOCUMENTATION_PORTABLE_INTEGRATION_DEPENSE_FROM_Kenergie.md` |
