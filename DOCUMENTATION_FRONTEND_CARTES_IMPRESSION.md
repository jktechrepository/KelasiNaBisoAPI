# Documentation Frontend Web — Impression des cartes scolaires

Guide d'intégration **frontend web** (Vue.js) pour l'impression des cartes d'identité **élèves** et **personnel (agents)** via FastReport, et l'attribution des **SerialNumber**.

> **Périmètre UI recommandé :** menus impression / serial pour **`Super-Admin`** et **`IT-Support`**.  
> L'API accepte aussi `Admin` et `Directeur` (périmètre école).  
> Les autres rôles (Enseignant, Parent…) ne doivent **pas** voir ces menus.  
> **Accès app :** `IT-Support` doit être autorisé sur le **layout authentifié** et **`/change-password`** (sinon écran « Accès restreint » après un login API OK). Ne jamais appeler `GET /api/Role/nomRole/Super-Admin` pour un utilisateur IT-Support — utiliser son `NomRole` réel.

**Frontend web dev :** `https://dev-knb.kansaconsulting.com`  
**API dev :** `https://dev-knb.asdc-rdc.org`  
**Swagger :** [https://dev-knb.asdc-rdc.org/swagger/index.html](https://dev-knb.asdc-rdc.org/swagger/index.html)  
**Auth :** `Authorization: Bearer {jwt_token}`  
**CORS :** origine `https://dev-knb.kansaconsulting.com` autorisée sur l'API

**Référence API backend :** [DOCUMENTATION_CARTES_IMPRESSION.md](./DOCUMENTATION_CARTES_IMPRESSION.md)

---

## Table des matières

1. [Vue d'ensemble](#1-vue-densemble)
2. [Contrôle d'accès frontend](#2-contrôle-daccès-frontend)
3. [Architecture écrans](#3-architecture-écrans)
4. [Endpoints API cartes (PDF / aperçu)](#4-endpoints-api-cartes-pdf--aperçu)
5. [Endpoints complémentaires (listes et prévisualisation)](#5-endpoints-complémentaires-listes-et-prévisualisation)
6. [Images : logos et photos (base64)](#6-images--logos-et-photos-base64)
7. [Modèles TypeScript](#7-modèles-typescript)
8. [Service API (JavaScript)](#8-service-api-javascript)
9. [Intégration Vue.js](#9-intégration-vuejs)
10. [Téléchargement PDF avec JWT](#10-téléchargement-pdf-avec-jwt)
11. [Aperçu navigateur](#11-aperçu-navigateur)
12. [Gestion des erreurs](#12-gestion-des-erreurs)
13. [Checklist Super-Admin](#13-checklist-super-admin)

---

## 1. Vue d'ensemble

### Objectif métier

Le **Super-Admin** peut, depuis le portail web :

- imprimer la carte d'un **élève** ou d'un **agent** (PDF unitaire) ;
- imprimer un **lot** d'élèves ou d'agents sélectionnés ;
- imprimer **toute une école** (élèves uniquement, optionnellement filtrée par classe) ;
- prévisualiser une carte avant impression (PDF ou vignettes dans la liste).

### Format de sortie

| Sortie | Usage |
|--------|--------|
| **PDF** | Impression physique (format carte CR80, 85,6 × 54 mm) |
| **HTML / WebReport** | Aperçu à l'écran avant téléchargement |
| **Données JSON `carte-data`** | Vignettes photo/logo dans les tableaux Super-Admin |

### Contenu affiché sur la carte

| Élément | Élève | Personnel |
|---------|-------|-----------|
| Photo | `PhotoUrl` (base64 ou URL) | `PhotoUrl` (base64 ou URL) |
| Nom complet | oui | oui |
| Matricule | oui + code-barres | oui + code-barres |
| Classe / Fonction | Classe | Fonction + rôle |
| École | Nom, slogan, logo | Nom, slogan, logo |
| Année scolaire | année active en base | année active en base |

### Deux modes d'intégration

| Mode | Quand l'utiliser |
|------|------------------|
| **`/api/Carte/.../pdf`** | Impression finale (fichier PDF généré par FastReport côté serveur) |
| **`/api/Agent/.../carte-data`** | Affichage des vignettes et prévisualisation légère dans l'UI (agents) |
| **`GET /api/V_Eleve/ecole/{id}`** | Liste élèves avec `photoUrl`, `logoUrlEcole` pour vignettes |

> La génération PDF est **toujours** côté API (`CarteController`). Le frontend ne reconstruit pas la mise en page carte : il déclenche le téléchargement ou l'aperçu.

---

## 2. Contrôle d'accès frontend

### Règle obligatoire

```javascript
const CARD_PRINT_ROLES = ['Super-Admin', 'IT-Support'];

function canAccessCarteImpression(user) {
  const roles = Array.isArray(user?.roles) ? user.roles : [user?.role];
  return roles.some((r) => CARD_PRINT_ROLES.includes(r));
}
```

### Garde de route (Vue Router exemple)

```javascript
{
  path: '/cartes-impression',
  name: 'CartesImpression',
  component: () => import('@/views/CartesImpression.vue'),
  meta: { requiresAuth: true, roles: ['Super-Admin', 'IT-Support'] }
}
```

```javascript
// router.beforeEach
if (to.meta.roles && !to.meta.roles.some((r) => store.userRoles.includes(r))) {
  return next('/acces-refuse');
}
```

### « Accès restreint » après login IT-Support

L’API **ne bloque pas** le login IT-Support. Si l’écran 403 apparaît :

1. Ajouter `IT-Support` aux rôles du **layout** et laisser `/change-password` ouvert à tout utilisateur authentifié (`meta.requiresAuth` seul).
2. Utiliser `NomRole` de la réponse login pour `GET /api/Role/nomRole/{NomRole}` (pas hardcoder `Super-Admin`).
3. Rediriger IT-Support vers l’écran **cartes**, pas `/agents/create`.

### Masquage UI

Afficher les menus « Impression cartes » / « SerialNumber » pour **Super-Admin** et **IT-Support**.  
Ne pas les afficher pour Enseignant, Parent, Élève, etc.

- **Super-Admin** : sélecteur d'école (multi-écoles).
- **IT-Support** : **toutes les écoles** (sélecteur d'école comme Super-Admin pour listes / cartes / SerialNumber).
- **Admin / Directeur** : école du JWT uniquement.

### Sélection d'école (Super-Admin / IT-Support)

Le JWT Super-Admin (et souvent IT-Support) n'est pas toujours lié utilement à une seule école.  
Le frontend doit **demander l'école cible** (sélecteur) avant de lister élèves/agents ou lancer un lot.  
`GET /api/Ecole` fournit la liste des écoles de la plateforme.

---

## 3. Architecture écrans

```
Super-Admin
└── Cartes scolaires
    ├── [Onglet Élèves]
    │   ├── Sélecteur école
    │   ├── Filtre classe (optionnel)
    │   ├── Table élèves (checkbox multi-sélection, vignette photo)
    │   └── Actions : Aperçu PDF | PDF unitaire | PDF sélection | PDF toute l'école
    └── [Onglet Personnel]
        ├── Sélecteur école
        ├── Table agents (checkbox, vignette photo + logo école)
        └── Actions : Aperçu PDF | PDF unitaire | PDF sélection | PDF tous les agents
```

### Écran recommandé : `SuperAdminCartesImpression.vue`

| Zone | Composant | Description |
|------|-----------|-------------|
| En-tête | `EcoleSelect` | Liste des écoles (`GET /api/Ecole`) |
| Filtre | `ClasseSelect` | Classes de l'école (`GET /api/Classe/ecole/{idEcole}`) — onglet élèves |
| Liste | `DataTable` | Élèves ou agents avec cases à cocher et vignettes |
| Actions ligne | Boutons | Aperçu PDF, Télécharger PDF |
| Actions lot | Boutons | « Imprimer la sélection », « Imprimer toute l'école / tous les agents » |

---

## 4. Endpoints API cartes (PDF / aperçu)

Base : `/api/Carte` — header `Authorization: Bearer {token}` requis.  
Rôles API autorisés : `Admin`, `Directeur`, `Super-Admin`, `IT-Support` (UI recommandée : Super-Admin + IT-Support).

### SerialNumber (attribution)

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/api/Eleve/serial-number/{serial}` | Lookup élève |
| PUT | `/api/Eleve/{id}/serial-number` | Attribuer serial élève |
| PUT | `/api/Eleve/matricule/{matricule}/serial-number` | Attribuer serial élève |
| GET | `/api/Agent/serial-number/{serial}` | Lookup agent |
| PUT | `/api/Agent/{id}/serial-number` | Attribuer serial agent |
| PUT | `/api/Agent/matricule/{matricule}/serial-number` | Attribuer serial agent |

Mêmes rôles ; Admin / Directeur limités à leur école ; **Super-Admin et IT-Support** : toutes les écoles. Body : `{ "serialNumber": "..." }`.

### Élèves

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/api/Carte/eleve/{idEleve}/pdf` | PDF une carte élève |
| GET | `/api/Carte/eleve/{idEleve}/apercu` | HTML aperçu (inline) |
| GET | `/api/Carte/eleve/{idEleve}/viewer` | Redirige vers `/Carte/Apercu?type=eleve&id={id}` |
| POST | `/api/Carte/eleves/pdf` | PDF lot (body ci-dessous) |
| GET | `/api/Carte/eleves/ecole/{idEcole}/pdf?idClasse={id}` | PDF tous les élèves de l'école |

### Personnel (agents)

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/api/Carte/agent/{idAgent}/pdf` | PDF une carte agent |
| GET | `/api/Carte/agent/{idAgent}/apercu` | HTML aperçu |
| GET | `/api/Carte/agent/{idAgent}/viewer` | Redirige vers `/Carte/Apercu?type=agent&id={id}` |
| POST | `/api/Carte/agents/pdf` | PDF lot agents |

> **Note :** il n'existe pas de route `GET .../agents/ecole/{id}/pdf`. Pour imprimer **tous** les agents d'une école, récupérer leurs IDs via `carte-data` puis appeler `POST /api/Carte/agents/pdf`.

### Body POST lot

```json
{
  "idEcole": 13,
  "ids": [42, 57, 88]
}
```

| Champ | Type | Obligatoire |
|-------|------|-------------|
| `idEcole` | number | oui |
| `ids` | number[] | oui, min. 1 id |

**Réponse lot :** un seul fichier PDF multi-pages (une carte par page).

---

## 5. Endpoints complémentaires (listes et prévisualisation)

### Listes pour alimenter les tableaux

| Besoin | Endpoint | Remarque |
|--------|----------|----------|
| Liste écoles | `GET /api/Ecole` | Sélecteur Super-Admin |
| Élèves par école | `GET /api/V_Eleve/ecole/{idEcole}` | Inclut `photoUrl`, `logoUrlEcole`, `nomClasse` |
| Élèves (paginé) | `GET /api/Eleve/ecole/{idEcole}/all` | Alternative si volume important |
| Classes par école | `GET /api/Classe/ecole/{idEcole}` | Filtre impression élèves |
| Agents (fiche brute) | `GET /api/Agent/ecole/{idEcole}` | **Sans** logo école ni photo décodée |
| **Agents (cartes UI)** | `GET /api/Agent/ecole/{idEcole}/carte-data` | **Recommandé** — logo + photo prêts pour `<img>` |
| Agent unitaire (carte UI) | `GET /api/Agent/{idAgent}/carte-data` | Détail / fiche agent |

### Réponse `GET /api/Agent/ecole/{idEcole}/carte-data`

```json
[
  {
    "idAgent": 42,
    "idEcole": 13,
    "matricule": "AGT-2024-042",
    "nomComplet": "KABONGO Jean Paul",
    "fonction": "Enseignant",
    "roleAgent": "Admin",
    "nomEcole": "Complexe Scolaire ABC",
    "sloganEcole": "Excellence et discipline",
    "logoBase64": "iVBORw0KGgoAAAANS...",
    "photoBase64": "/9j/4AAQSkZJRg...",
    "logoSrc": "data:image/png;base64,iVBORw0KGgoAAAANS...",
    "photoSrc": "data:image/jpeg;base64,/9j/4AAQSkZJRg...",
    "logoBytesBase64": "iVBORw0KGgoAAAANS...",
    "photoBytesBase64": "/9j/4AAQSkZJRg..."
  }
]
```

| Champ | Usage frontend |
|-------|----------------|
| `photoSrc` / `logoSrc` | **Utiliser dans `<img :src="...">`** (data URI prête) |
| `photoBase64` / `logoBase64` | Payload brut si vous construisez vous-même le `data:` URI |
| `nomComplet`, `matricule`, `fonction` | Colonnes du tableau |

### Champs utiles — élèves (`V_Eleve`)

`idEleve`, `nomComplet`, `matricule`, `nomClasse`, `photoUrl`, `logoUrlEcole`, `statut`

### Pourquoi `carte-data` pour les agents ?

`GET /api/Agent/ecole/{id}` retourne l'entité `Agent` sans navigation `Ecole` (logo absent).  
`carte-data` joint l'école, résout logo/photo (base64 **ou** URL HTTP) et renvoie des champs prêts pour l'affichage.

---

## 6. Images : logos et photos (base64)

### Stockage en base

Les logos d'école (`Ecole.Logo`) sont souvent stockés en **base64 brut** (pas une URL HTTP).  
Les photos (`Agent.PhotoUrl`, `Eleve.PhotoUrl`) peuvent être base64, data URI ou URL distante.

### Règle d'affichage frontend

```javascript
/** Convertit base64 brut, data URI ou URL http(s) en src utilisable */
export function toImageSrc(value, defaultMime = 'image/png') {
  if (!value?.trim()) return null;
  const v = value.trim();
  if (v.startsWith('data:') || v.startsWith('http://') || v.startsWith('https://')) {
    return v;
  }
  return `data:${defaultMime};base64,${v}`;
}
```

| Source | Champ recommandé |
|--------|------------------|
| Agents (`carte-data`) | `photoSrc`, `logoSrc` directement |
| Élèves (`V_Eleve`) | `toImageSrc(eleve.photoUrl)` et `toImageSrc(eleve.logoUrlEcole)` |

### PDF côté serveur

La génération PDF (`CarteReportService` + `ReportImageResolver`) décode automatiquement :

- base64 brut ou data URI ;
- URLs HTTP publiques accessibles depuis le serveur API.

Si l'image est absente ou illisible, la carte est générée **sans** photo/logo (pas d'erreur bloquante).

> **Ne pas** traiter `logoBase64` comme une URL : toujours passer par `logoSrc` ou `toImageSrc()`.

---

## 7. Modèles TypeScript

```typescript
/** Body POST /api/Carte/eleves/pdf ou /agents/pdf */
export interface CarteBatchRequest {
  idEcole: number;
  ids: number[];
}

export type CarteType = 'eleve' | 'agent';

/** GET /api/Agent/ecole/{id}/carte-data */
export interface CarteAgentData {
  idAgent: number;
  idEcole: number | null;
  matricule: string;
  nomComplet: string;
  fonction: string;
  roleAgent: string;
  nomEcole: string;
  sloganEcole: string;
  logoBase64?: string | null;
  photoBase64?: string | null;
  logoSrc?: string | null;
  photoSrc?: string | null;
  logoBytesBase64?: string | null;
  photoBytesBase64?: string | null;
}

export interface EleveCarteRow {
  idEleve: number;
  nomComplet: string;
  matricule?: string;
  nomClasse?: string;
  photoUrl?: string;
  logoUrlEcole?: string;
  photoSrc?: string | null;
  logoSrc?: string | null;
  selected?: boolean;
}

export interface AgentCarteRow extends CarteAgentData {
  selected?: boolean;
}

export interface CarteApiError {
  message: string;
  error?: string;
}
```

---

## 8. Service API (JavaScript)

Fichier suggéré : `src/services/carteImpressionService.js`

```javascript
const API_BASE = (import.meta.env.VITE_API_BASE_URL ?? '').replace(/\/$/, '');

function authHeaders(token) {
  return { Authorization: `Bearer ${token}` };
}

export function toImageSrc(value, defaultMime = 'image/png') {
  if (!value?.trim()) return null;
  const v = value.trim();
  if (v.startsWith('data:') || v.startsWith('http://') || v.startsWith('https://')) {
    return v;
  }
  return `data:${defaultMime};base64,${v}`;
}

/** Télécharge un PDF et déclenche le save-as */
export async function downloadCartePdf(path, token, filename) {
  const res = await fetch(`${API_BASE}${path}`, {
    headers: authHeaders(token)
  });

  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error(err.message || `Erreur ${res.status}`);
  }

  const blob = await res.blob();
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = filename;
  a.click();
  URL.revokeObjectURL(url);
}

export function downloadCarteEleve(idEleve, token) {
  return downloadCartePdf(
    `/api/Carte/eleve/${idEleve}/pdf`,
    token,
    `carte-eleve-${idEleve}.pdf`
  );
}

export function downloadCarteAgent(idAgent, token) {
  return downloadCartePdf(
    `/api/Carte/agent/${idAgent}/pdf`,
    token,
    `carte-agent-${idAgent}.pdf`
  );
}

export async function downloadCarteElevesLot(idEcole, ids, token) {
  const res = await fetch(`${API_BASE}/api/Carte/eleves/pdf`, {
    method: 'POST',
    headers: {
      ...authHeaders(token),
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ idEcole, ids })
  });

  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error(err.message || `Erreur ${res.status}`);
  }

  const blob = await res.blob();
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = `cartes-eleves-ecole-${idEcole}.pdf`;
  a.click();
  URL.revokeObjectURL(url);
}

export async function downloadCarteElevesEcole(idEcole, idClasse, token) {
  const qs = idClasse ? `?idClasse=${idClasse}` : '';
  return downloadCartePdf(
    `/api/Carte/eleves/ecole/${idEcole}/pdf${qs}`,
    token,
    `cartes-eleves-ecole-${idEcole}.pdf`
  );
}

export async function downloadCarteAgentsLot(idEcole, ids, token) {
  const res = await fetch(`${API_BASE}/api/Carte/agents/pdf`, {
    method: 'POST',
    headers: {
      ...authHeaders(token),
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ idEcole, ids })
  });

  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error(err.message || `Erreur ${res.status}`);
  }

  const blob = await res.blob();
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = `cartes-agents-ecole-${idEcole}.pdf`;
  a.click();
  URL.revokeObjectURL(url);
}

/** Liste agents avec logo/photo pour l'UI Super-Admin */
export async function fetchAgentsCarteData(idEcole, token) {
  const res = await fetch(`${API_BASE}/api/Agent/ecole/${idEcole}/carte-data`, {
    headers: authHeaders(token)
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error(err.message || `Erreur ${res.status}`);
  }
  return res.json();
}

/** Imprimer tous les agents actifs d'une école */
export async function downloadCarteAgentsEcole(idEcole, token) {
  const agents = await fetchAgentsCarteData(idEcole, token);
  const ids = agents.map((a) => a.idAgent);
  if (!ids.length) throw new Error('Aucun agent actif pour cette école.');
  return downloadCarteAgentsLot(idEcole, ids, token);
}
```

### Configuration `.env`

```env
VITE_API_BASE_URL=https://dev-knb.asdc-rdc.org
```

---

## 9. Intégration Vue.js

### Onglet Élèves (extrait)

```vue
<script setup>
import { ref, computed, watch } from 'vue';
import { useAuthStore } from '@/stores/auth';
import {
  downloadCarteEleve,
  downloadCarteElevesLot,
  downloadCarteElevesEcole,
  toImageSrc
} from '@/services/carteImpressionService';

const auth = useAuthStore();
const idEcole = ref(null);
const eleves = ref([]);
const selectedIds = ref([]);
const loading = ref(false);
const error = ref(null);

if (auth.role !== 'Super-Admin') {
  throw new Error('Accès réservé Super-Admin');
}

async function loadEleves() {
  if (!idEcole.value) return;
  loading.value = true;
  error.value = null;
  try {
    const API_BASE = import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, '') ?? '';
    const res = await fetch(`${API_BASE}/api/V_Eleve/ecole/${idEcole.value}`, {
      headers: { Authorization: `Bearer ${auth.token}` }
    });
    if (!res.ok) throw new Error('Impossible de charger les élèves');
    const rows = await res.json();
    eleves.value = rows.map((e) => ({
      ...e,
      photoSrc: toImageSrc(e.photoUrl, 'image/jpeg'),
      logoSrc: toImageSrc(e.logoUrlEcole)
    }));
  } catch (e) {
    error.value = e.message;
  } finally {
    loading.value = false;
  }
}

watch(idEcole, loadEleves);

const hasSelection = computed(() => selectedIds.value.length > 0);
</script>

<template>
  <tr v-for="e in eleves" :key="e.idEleve">
    <td><input type="checkbox" :value="e.idEleve" v-model="selectedIds" /></td>
    <td>
      <img v-if="e.photoSrc" :src="e.photoSrc" alt="" class="avatar" width="40" height="40" />
    </td>
    <td>{{ e.nomComplet }}</td>
    <td>{{ e.matricule }}</td>
    <td>{{ e.nomClasse }}</td>
    <td>
      <button @click="downloadCarteEleve(e.idEleve, auth.token)">PDF</button>
    </td>
  </tr>
</template>
```

### Onglet Personnel (extrait — `carte-data`)

```vue
<script setup>
import { ref, watch } from 'vue';
import { useAuthStore } from '@/stores/auth';
import {
  fetchAgentsCarteData,
  downloadCarteAgent,
  downloadCarteAgentsLot,
  downloadCarteAgentsEcole
} from '@/services/carteImpressionService';

const auth = useAuthStore();
const idEcole = ref(null);
const agents = ref([]);
const selectedIds = ref([]);

async function loadAgents() {
  if (!idEcole.value) return;
  agents.value = await fetchAgentsCarteData(idEcole.value, auth.token);
}

watch(idEcole, loadAgents);

async function onPdfLot() {
  await downloadCarteAgentsLot(idEcole.value, selectedIds.value, auth.token);
}

async function onPdfTousAgents() {
  await downloadCarteAgentsEcole(idEcole.value, auth.token);
}
</script>

<template>
  <tr v-for="a in agents" :key="a.idAgent">
    <td><input type="checkbox" :value="a.idAgent" v-model="selectedIds" /></td>
    <td>
      <img v-if="a.photoSrc" :src="a.photoSrc" alt="" class="avatar" width="40" height="40" />
    </td>
    <td>{{ a.nomComplet }}</td>
    <td>{{ a.matricule }}</td>
    <td>{{ a.fonction }}</td>
    <td>
      <button @click="downloadCarteAgent(a.idAgent, auth.token)">PDF</button>
    </td>
  </tr>
  <button :disabled="!selectedIds.length" @click="onPdfLot">Imprimer la sélection</button>
  <button :disabled="!idEcole" @click="onPdfTousAgents">Imprimer tous les agents</button>
</template>
```

---

## 10. Téléchargement PDF avec JWT

Les liens `<a href="/api/Carte/.../pdf">` **ne transmettent pas** le header `Authorization`.

**Toujours utiliser `fetch` + `blob`** (voir `downloadCartePdf` ci-dessus).

```javascript
// Anti-pattern — ne pas faire
window.location.href = '/api/Carte/eleve/42/pdf'; // → 401

// Correct
await downloadCarteEleve(42, token);
```

Le frontend (`dev-knb.kansaconsulting.com`) et l'API (`dev-knb.asdc-rdc.org`) sont sur **des domaines différents** : les cookies de session API ne sont pas partagés. L'authentification passe **uniquement** par le JWT dans `fetch`.

---

## 11. Aperçu navigateur

### Option A — PDF dans un nouvel onglet (recommandé)

```javascript
export async function previewCarteElevePdf(idEleve, token) {
  const API_BASE = import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, '') ?? '';
  const res = await fetch(`${API_BASE}/api/Carte/eleve/${idEleve}/pdf`, {
    headers: { Authorization: `Bearer ${token}` }
  });
  if (!res.ok) throw new Error('Aperçu impossible');
  const blob = await res.blob();
  const url = URL.createObjectURL(blob);
  window.open(url, '_blank', 'noopener');
}
```

### Option B — HTML inline

```javascript
const res = await fetch(`${API_BASE}/api/Carte/eleve/${idEleve}/apercu`, {
  headers: { Authorization: `Bearer ${token}` }
});
const html = await res.text();
// Afficher dans iframe ou modal
```

### Option C — Page WebReport API (`/viewer`)

```
GET /api/Carte/eleve/{id}/viewer
```

Fonctionne si l'utilisateur est authentifié sur le **même domaine** que l'API (ex. ouverture directe sur `dev-knb.asdc-rdc.org`).  
Depuis le frontend Vue sur `kansaconsulting.com`, **préférer l'option A** (fetch PDF + blob URL).

---

## 12. Gestion des erreurs

| HTTP | Cause probable | Action UI |
|------|----------------|-----------|
| **401** | Token absent / expiré | Rediriger vers login |
| **403** | Rôle insuffisant ou école non autorisée | Message « Réservé Super-Admin » |
| **404** | Élève/agent/école introuvable | Toast « Fiche introuvable » |
| **500** | Erreur génération (template, données) | Toast + détail `error` si présent |

```javascript
try {
  await downloadCarteEleve(idEleve, token);
} catch (e) {
  if (e.message.includes('401')) router.push('/login');
  else toast.error(e.message || 'Échec impression carte');
}
```

### Images manquantes

Si photo ou logo est vide, la carte PDF est générée **sans** visuel correspondant.  
Vérifier en Super-Admin que les photos URLs distantes sont accessibles **depuis le serveur API** (pas `localhost` côté client).

---

## 13. Checklist Super-Admin

### Routing & sécurité
- [ ] Route `/super-admin/cartes` protégée `meta.roles: ['Super-Admin']`
- [ ] Aucun bouton carte visible pour Admin / Directeur / Parent
- [ ] Menu « Cartes scolaires » uniquement dans layout Super-Admin
- [ ] `VITE_API_BASE_URL` pointe vers l'API (`dev-knb.asdc-rdc.org`)

### Écran élèves
- [ ] Sélecteur école obligatoire
- [ ] Liste via `GET /api/V_Eleve/ecole/{idEcole}`
- [ ] Vignettes avec `toImageSrc(photoUrl)`
- [ ] PDF unitaire (fetch + blob)
- [ ] PDF lot (sélection multiple)
- [ ] PDF toute l'école (+ filtre classe optionnel via `idClasse`)
- [ ] Aperçu PDF (blob URL)

### Écran personnel
- [ ] Liste via `GET /api/Agent/ecole/{id}/carte-data` (pas `Agent/ecole` seul)
- [ ] Vignettes avec `photoSrc` / `logoSrc`
- [ ] PDF unitaire + lot
- [ ] « Imprimer tous les agents » → collecte IDs + `POST /api/Carte/agents/pdf`

### UX
- [ ] Indicateur chargement pendant génération PDF
- [ ] Désactiver boutons si aucune sélection (lot)
- [ ] Messages d'erreur explicites (401, 403, 404, 500)

### Tests manuels
- [ ] Super-Admin : `GET /api/Carte/eleve/{id}/pdf` → PDF non vide
- [ ] Super-Admin : `GET /api/Agent/ecole/{id}/carte-data` → `logoSrc` et `photoSrc` valides
- [ ] Admin connecté : module cartes **absent** de l'UI
- [ ] Lot 3 élèves → 1 PDF 3 pages
- [ ] Élève sans photo → PDF généré quand même
- [ ] Logo école en base64 → visible sur carte agent PDF

---

*KelasiNaBiso — Frontend Web — Module cartes scolaires (Super-Admin uniquement) — mise à jour juillet 2026*
