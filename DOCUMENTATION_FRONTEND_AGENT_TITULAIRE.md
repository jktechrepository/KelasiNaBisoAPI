# Documentation Frontend — Classes titulaires sur le détail agent

Guide d’intégration **Vue 3** (web) et **Flutter** (fiche / carte agent).

**Changement :** rétrocompatible sur les champs agent (racine). Champ **`titulairesActifs`** (liste). Remplace l’objet unique `titulaireActif` (ne plus l’utiliser).

**Base URL dev :** `https://localhost:7102` ou `https://dev-knb.asdc-rdc.org`  
**Swagger :** `/swagger/index.html` — tag **Agent**  
**Auth :** `Authorization: Bearer {accessToken}`

Références :
- [DOCUMENTATION_TITULAIRE_CLASSE.md](DOCUMENTATION_TITULAIRE_CLASSE.md) — 1 titulaire / classe / année (un agent peut déjà avoir plusieurs classes en base)
- Swagger : `GET /api/Agent/{id}`
- Historique : `GET /api/TitulaireClasse/agent/{idAgent}`

---

## 1. Vue d’ensemble

L’écran **détail agent** (une carte, un round-trip) lit `GET /api/Agent/{id}`.

| Besoin UI | Endpoint |
|-----------|----------|
| Fiche agent + classe(s) titulaire(s) **en cours** | `GET /api/Agent/{id}` → `titulairesActifs` |
| Historique des titulariats | `GET /api/TitulaireClasse/agent/{idAgent}` (inchangé) |
| Liste agents d’une école | `GET /api/Agent/ecole/{id}` — **pas** de `titulairesActifs` |

`titulairesActifs` est **`[]`** si l’agent n’est pas titulaire ouvert (secrétaire, directeur, enseignant secondaire sans ligne `TitulairesClasses`). Ce n’est **pas** un champ de `RoleAgent`.

Un enseignant peut être titulaire de **plusieurs classes** (données existantes). La création reste limitée à 1 classe / année côté API (`AgentEstDisponible`).

---

## 2. Endpoint

```http
GET /api/Agent/10
Authorization: Bearer {accessToken}
```

Filtre côté API (ne pas refiltrer `statut` / `dateFin` / année côté client pour cette carte) :

- `IdAgent` = `{id}`
- `Statut == true`
- `DateFin == null`
- s’il y a une **année scolaire en cours** pour l’école de l’agent : **toutes** les lignes ouvertes de cette année
- sinon : toutes les lignes ouvertes, tri `dateDebut` desc

---

## 3. Réponse JSON

Champs agent habituels (`idAgent`, `nom`, `matricule`, `idEcole`, …) **plus** :

```json
{
  "idAgent": 10,
  "nom": "Kalala",
  "roleAgent": "Enseignant",
  "idEcole": 1,
  "titulairesActifs": [
    {
      "idTitulaireClasse": 42,
      "idClasse": 7,
      "nomClasse": "1ère Primaire A",
      "idAnneeScolaire": 101,
      "libelleAnneeScolaire": "2025-2026",
      "dateDebut": "2025-09-01T00:00:00"
    },
    {
      "idTitulaireClasse": 43,
      "idClasse": 8,
      "nomClasse": "2ème Primaire B",
      "idAnneeScolaire": 101,
      "libelleAnneeScolaire": "2025-2026",
      "dateDebut": "2025-09-15T00:00:00"
    }
  ]
}
```

Sans titulariat ouvert :

```json
{
  "idAgent": 10,
  "titulairesActifs": []
}
```

---

## 4. Vue 3 / Flutter

- Désérialiser `titulairesActifs` en **liste** (jamais null). Un `fromJson` Agent existant ignore le champ inconnu.
- `length == 0` : masquer le bloc.
- `length == 1` : « Titulaire de {nomClasse} ({libelleAnneeScolaire}) ».
- `length > 1` : chips / lignes, une par classe.
- Ne pas appeler TitulaireClasse uniquement pour peupler la fiche ; garder cet endpoint pour un onglet historique.

---

## 5. Checklist

- [ ] Fiche agent : un seul `GET /api/Agent/{id}`
- [ ] Chips / lignes si `titulairesActifs.length > 0`
- [ ] Secondaire / non-enseignant : liste vide, pas d’erreur
- [ ] Liste école : inchangée
- [ ] Historique : `GET /api/TitulaireClasse/agent/{idAgent}`
- [ ] Ne plus lire `titulaireActif`
