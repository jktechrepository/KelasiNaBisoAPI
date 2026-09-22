# Intégration frontend — Exonération / allègement de frais élèves

Documentation pour Vue / Flutter : catégories tarifaires, affectations, règles d’exonération et affichage du dû effectif vs catalogue.

## Concepts

| Notion | Rôle |
|--------|------|
| **Catalogue** | `Frais.Montant` — référence inchangée |
| **Catégorie** | Référentiel école (`BOURSIER`, `ENFANT_ENSEIGNANT`, …) |
| **Affectation** | Lien élève ↔ catégorie pour une année (`DateFin` null = en cours) |
| **Règle** | Exonération **par frais** et catégorie (`Totale`, `Pourcentage`, `MontantReduction`, `MontantDuFixe`) |
| **Dû effectif** | Résultat de la règle sur le catalogue |
| **Reste** | `max(0, dûEffectif − totalPayé confirmé)` |

Les paiements déjà enregistrés **ne sont jamais recalculés** lors d’un changement de catégorie.

## Rôles

| Action | Admin | Financier | Caissier | Directeur |
|--------|:-----:|:---------:|:--------:|:---------:|
| CRUD catégories | ✓ | ✓ | ✓ | — |
| Affecter / clôturer | ✓ | ✓ | ✓ | — |
| Lecture règles | ✓ | ✓ | ✓ | ✓ |
| Écriture règles | ✓ | ✓ | — | — |
| Lecture frais-dus enrichis | ✓ | ✓ | ✓ | ✓ |

Scope école : claim JWT `idEcole` ; Super-Admin / IT-Support passent `?idEcole=`.

## Endpoints

Base : `api/EleveTarif`

### Catégories

```
GET    /categories?idEcole=&includeInactive=
GET    /categories/{id}
POST   /categories?idEcole=
PUT    /categories/{id}
DELETE /categories/{id}          # soft (Statut=false)
```

**POST body**

```json
{ "code": "BOURSIER", "libelle": "Élève boursier", "description": null, "statut": true }
```

### Affectations

```
GET  /eleves/{idEleve}/affectations?idAnneeScolaire=
GET  /eleves/{idEleve}/affectation-active?idAnneeScolaire=
POST /affectations
POST /affectations/{id}/cloturer?dateFin=
```

**POST `/affectations`** — clôture automatiquement l’affectation ouverte précédente pour le même élève / année.

```json
{
  "idEleve": 12,
  "idCategorieEleveTarif": 3,
  "idAnneeScolaire": 5,
  "dateDebut": null,
  "motif": "Enfant d'enseignant"
}
```

### Règles

```
GET    /regles?idEcole=&idAnneeScolaire=&idCategorie=&includeInactive=
GET    /regles/{id}
PUT    /regles?idEcole=          # upsert (clé année+catégorie+frais)
DELETE /regles/{id}              # soft
```

**PUT body**

```json
{
  "idAnneeScolaire": 5,
  "idCategorieEleveTarif": 3,
  "idFrais": 40,
  "typeRegle": "Pourcentage",
  "valeur": 50,
  "statut": true
}
```

| `typeRegle` | `valeur` | Dû effectif |
|-------------|----------|-------------|
| `Totale` | ignorée (0) | `0` |
| `Pourcentage` | 0–100 | `catalogue × (1 − v/100)` |
| `MontantReduction` | montant | `max(0, catalogue − v)` |
| `MontantDuFixe` | montant | `max(0, v)` |

### Lecture dus enrichis

```
GET /eleves/{idEleve}/frais-dus?idAnneeScolaire=
GET /eleves/{idEleve}/frais-dus/{idFrais}
```

**Réponse (extrait)**

```json
{
  "idEleve": 12,
  "idFrais": 40,
  "libelleFrais": "Minerval T1",
  "codeDevise": "USD",
  "montantCatalogue": 200,
  "montantDuEffectif": 100,
  "montantReduction": 100,
  "montantPaye": 40,
  "resteAPayer": 60,
  "codeCategorie": "BOURSIER",
  "typeRegle": "Pourcentage",
  "valeurRegle": 50
}
```

## Sync offline

Sur `FraisDuSyncDto` / pull frais dus :

- `montantDu` = reste (dû effectif − payé) — **rétrocompat**
- `montantTotal` = dû effectif
- `montantCatalogue`, `montantReduction`, `codeCategorie` (nouveaux, optionnels)

## UI recommandée

1. **Paramétrage école** : écran catégories + règles par année / frais (Admin / Financier).
2. **Fiche élève** : badge catégorie active + historique d’affectations ; bouton Affecter / Clôturer (Caissier OK).
3. **Encaissement / solde** : afficher catalogue barré si réduction, puis dû effectif et reste.
4. Ne pas proposer de « recalculer les paiements passés ».

## Script SQL

`docs/sql/20260922_AddEleveTarifExoneration.sql` — tables `CategoriesEleveTarif`, `AffectationsEleveCategorieTarif`, `ReglesExonerationFrais`.
