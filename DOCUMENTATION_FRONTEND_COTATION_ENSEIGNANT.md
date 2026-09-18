# Documentation frontend — Cotation enseignant

Guide d’intégration Vue / Flutter pour le module de cotation (évaluations, notes, périodes, bulletins).

## 1. Prérequis auth

| Claim JWT | Usage |
|-----------|--------|
| `IdUtilisateur` | Forcé comme `IdProfesseur` à la création de notes |
| `IdAgent` | Scope pédagogique (titulaire ∪ affectation) |
| `IdEcole` | Tenant école |

Permissions utiles enseignant : `Evaluation.Create/Read/Update`, `Note.Create/Read/Update`, `Bulletin.Read`, `Cours.Read`, `Classe.Read`.

Hors scope (cours non enseigné) → **403**.

## 2. Périodes de cotation

```
GET /api/PeriodeCotation
GET /api/PeriodeCotation/{id}
```

Seed : **T1 / T2 / T3** (`code`, `libelle`, `ordre`).

Préférer `idPeriode` partout ; `periode` (alias `T1`, `Trimestre 1`, `1er Trimestre`…) reste accepté.

## 3. Évaluations

```
GET  /api/Evaluation/cours/{idCours}
GET  /api/Evaluation/classe/{idClasse}
POST /api/Evaluation
PUT  /api/Evaluation/{id}
```

Body create/update :

```json
{
  "typeEvaluation": "Interro",
  "titreEvaluation": "Interro 1",
  "idPeriode": 1,
  "coefficient": 2,
  "idCours": 50,
  "idClasse": 10
}
```

`periode` (string) optionnel si `idPeriode` fourni — miroir du libellé côté API.

## 4. Notes

### Unitaire

```
POST /api/Note
PUT  /api/Note/{id}
GET  /api/Note/evaluation/{idEvaluation}
```

Pour l’enseignant, `idProfesseur` du body est **ignoré** (JWT `IdUtilisateur`).

### Saisie groupée (recommandé)

```
POST /api/Note/bulk
```

```json
{
  "idEvaluation": 12,
  "idAnneeScolaire": 100,
  "dateEvaluation": "2026-09-16",
  "lignes": [
    { "idEleve": 1, "noteObtenue": 14, "appreciation": "Bien" },
    { "idEleve": 2, "noteObtenue": 11 }
  ]
}
```

- Upsert `(élève, évaluation)` active  
- Max **80** lignes, transaction stricte  
- Élèves doivent être inscrits dans la classe de l’évaluation  
- Réponse : `{ created, updated, notes[] }`

## 5. Bulletins

```
GET /api/Bulletin/eleve/{idEleve}?idPeriode=1&idAnneeScolaire=
GET /api/Bulletin/eleve/{idEleve}/pdf?idPeriode=1
GET /api/Bulletin/classe/{idClasse}?idPeriode=1
PUT /api/Bulletin/eleve/{idEleve}/decision
POST /api/Bulletin/eleve/{idEleve}/figer
POST /api/Bulletin/classe/{idClasse}/figer
POST /api/Bulletin/eleve/{idEleve}/deverrouiller
```

Permissions lecture : `Bulletin.Read` **ou** `Note.Read` (rétrocompat).  
Saisie décision / figer : `Bulletin.Update` — **titulaire** ou **direction** (Admin / Directeur / Sous-Directeur).  
Déverrouiller : `Bulletin.Unlock` — **direction uniquement** (Admin / Directeur / Sous-Directeur / Super-Admin), pas le titulaire.

```json
PUT /api/Bulletin/eleve/42/decision
{
  "idPeriode": 1,
  "idAnneeScolaire": 100,
  "decision": "Admis avec félicitations",
  "appreciationGenerale": "Élève sérieux et régulier."
}
```

Texte libre. Réapparaît dans le JSON / PDF via `decision` et `appreciationGenerale`.

### Figé / validé (V2)

Workflow à 2 états : **live** (calcul à la volée) → **figé** (snapshot immuable).

```json
POST /api/Bulletin/eleve/42/figer
{ "idPeriode": 1, "idAnneeScolaire": 100 }

POST /api/Bulletin/classe/10/figer
{ "idPeriode": 1 }

POST /api/Bulletin/eleve/42/deverrouiller
{ "idPeriode": 1 }
```

- Après fige : les GET JSON/PDF renvoient le snapshot (`estFige: true`, `dateValidation`).
- Modifier les notes ou la décision ne change plus le rendu tant que figé.
- `PUT .../decision` sur un bulletin figé → **400**.
- Double fige élève → **409**.
- `POST .../classe/.../figer` ignore les élèves déjà figés (`figes`, `dejaFiges` dans la réponse).
- Déverrouiller = suppression du snapshot → recalcul live.

### Calcul

- **Moyenne cours** = notes × `Evaluation.Coefficient`
- **Moyenne générale** = moyennes cours × `Cours.Ponderation` (défaut **1** si null)
- Rang compétition (1, 2, 2, 4…) — le rang figé reste celui du moment du gel
- Champ `ponderationCours` exposé sur chaque ligne

Configurer `Cours.ponderation` côté admin pour refléter les poids matières (ex. Math 3, Français 1).

## 6. Parcours UI suggéré

1. Charger `GET /api/PeriodeCotation` → picker période  
2. Choisir classe / cours (dashboard enseignant ou listes ACL)  
3. Créer / sélectionner une évaluation (`idPeriode`)  
4. Afficher la feuille : élèves de la classe + notes existantes (`GET /api/Note/evaluation/{id}`)  
5. Sauvegarder via `POST /api/Note/bulk`  
6. Aperçu bulletin classe / élève  
7. Saisie décision (titulaire) puis **Figer** quand la période est close  
8. Direction : **Déverrouiller** seulement en cas de correction exceptionnelle  

## 7. Erreurs fréquentes

| HTTP | Cause |
|------|--------|
| 403 | Cours/classe hors affectation ou mauvaise école |
| 400 | Élève hors classe, note hors 0–100, >80 lignes, doublon élève, décision sur bulletin figé |
| 404 | Évaluation / élève introuvable |
| 409 | Bulletin déjà figé |

## 8. Fichiers API de référence

- `Controllers/EvaluationController.cs`, `NoteController.cs`, `BulletinController.cs`, `PeriodeCotationController.cs`
- `Services/PedagogieAuthorizationService.cs`, `BulletinService.cs`
- SQL prod périodes : `docs/sql/20260916_AddPeriodesCotation_And_IdPeriode.sql`
- SQL permissions bulletin : `docs/sql/20260916_AlignPermissionsBulletin.sql`
- SQL bulletins figés : `docs/sql/20260916_AddBulletinsFiges.sql`
