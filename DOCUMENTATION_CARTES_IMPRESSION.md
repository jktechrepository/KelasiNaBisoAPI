# Documentation — Impression cartes élèves et personnel (FastReport)

Guide **API backend** pour la génération de cartes scolaires via **FastReport Open Source**.

**Documentation frontend web (Super-Admin uniquement) :** [DOCUMENTATION_FRONTEND_CARTES_IMPRESSION.md](./DOCUMENTATION_FRONTEND_CARTES_IMPRESSION.md)

**Auth :** `Authorization: Bearer {jwt_token}`  
**Rôles API :** `Admin`, `Directeur`, `Super-Admin`  
**Périmètre frontend web :** réservé à `Super-Admin` (voir doc frontend)

---

## Endpoints

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/api/Carte/eleve/{idEleve}/apercu` | Aperçu HTML (navigateur) |
| GET | `/api/Carte/eleve/{idEleve}/pdf` | PDF carte élève |
| GET | `/api/Carte/eleve/{idEleve}/viewer` | Redirige vers page WebReport (`/Carte/Apercu`) |
| POST | `/api/Carte/eleves/pdf` | PDF lot élèves (body ci-dessous) |
| GET | `/api/Carte/eleves/ecole/{idEcole}/pdf?idClasse=` | PDF tous les élèves (classe optionnelle) |
| GET | `/api/Carte/agent/{idAgent}/apercu` | Aperçu HTML personnel |
| GET | `/api/Carte/agent/{idAgent}/pdf` | PDF carte personnel |
| GET | `/api/Carte/agent/{idAgent}/viewer` | Page WebReport personnel |
| POST | `/api/Carte/agents/pdf` | PDF lot agents |

### Body lot (élèves ou agents)

```json
{
  "idEcole": 13,
  "ids": [42, 57, 88]
}
```

---

## Exemples frontend

### Télécharger PDF élève

```http
GET /api/Carte/eleve/42/pdf
Authorization: Bearer {token}
```

Réponse : `application/pdf` — fichier `carte-eleve-42.pdf`

### Aperçu dans un nouvel onglet

```javascript
const url = `${API_BASE}/api/Carte/eleve/${idEleve}/apercu`;
window.open(url, '_blank', 'noopener');
// Envoyer le token via un fetch puis blob URL, ou utiliser /Carte/Apercu?type=eleve&id=42
```

### Viewer WebReport (toolbar FastReport)

```
GET /api/Carte/eleve/42/viewer
→ redirige vers /Carte/Apercu?type=eleve&id=42
```

> La page Razor nécessite une session/cookie JWT valide (même origine ou auth configurée).

### Impression lot (sélection multiple)

```http
POST /api/Carte/eleves/pdf
Content-Type: application/json
Authorization: Bearer {token}

{
  "idEcole": 13,
  "ids": [1, 2, 3, 4]
}
```

---

## Données affichées sur la carte

| Champ | Élève | Personnel |
|-------|-------|-----------|
| Photo | `PhotoUrl` (base64, data URI ou URL) | `PhotoUrl` |
| Nom complet | oui | oui |
| Matricule | oui + code-barres | oui + code-barres |
| Classe / Fonction | Classe | Fonction + rôle |
| École | Nom, slogan, logo | Nom, slogan, logo |
| Année scolaire | active en base | active en base |

**Format carte :** CR80 (85,6 × 54 mm), une carte par page en PDF lot.

---

## Endpoints données carte (UI frontend)

Pour l'affichage des vignettes côté frontend (sans générer le PDF) :

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/api/Agent/ecole/{idEcole}/carte-data` | Liste agents actifs + logo école + photos (`logoSrc`, `photoSrc`) |
| GET | `/api/Agent/{idAgent}/carte-data` | Un agent avec logo/photo prêts pour `<img>` |

DTO : `Models/DTOs/Reporting/CarteAgentDataDto.cs`  
Helper images : `Helpers/ImageSourceHelper.cs`  
Résolution images PDF : `Services/Reporting/ReportImageResolver.cs` (base64 + HTTP).

---

## Prérequis

1. Images (photo, logo) : base64 brut, data URI **ou** URL HTTP accessible **depuis le serveur API** (`ReportImageResolver`).
2. Rôle Admin/Directeur avec `IdEcole` correct dans le JWT (sauf Super-Admin).
3. Dossier `Reports/Templates/` déployé avec l'API (`CarteEleve.frx`, `CarteAgent.frx`, `RectoAgent.frx`, etc.).

---

## Personnalisation des modèles

Éditer les fichiers avec **FastReport Designer Community** (Windows) :

- `Reports/Templates/CarteEleve.frx`
- `Reports/Templates/CarteAgent.frx`

Voir `Reports/Templates/README.md` pour la liste des champs datasource `Carte`.

---

## Tests post-déploiement

| Test | Attendu |
|------|---------|
| `GET /api/Carte/eleve/1/pdf` sans token | 401 |
| `GET /api/Carte/eleve/1/pdf` avec token Admin | 200, PDF non vide |
| `GET /api/MokoAfrika/fees/estimate?...` | inchangé (autre module) |

---

*KelasiNaBiso API — module cartes scolaires*
