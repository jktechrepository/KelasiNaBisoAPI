# Intégration Vue 3 & Flutter — Rôle Controleur (contrôle à l'entrée)

Guide d'implémentation pour l'**écran contrôle à l'entrée** côté **Vue 3** (web) et **Flutter** (tablette recommandée).

**Références API :**
- [DOCUMENTATION_FRONTEND_ROLE_CONTROLEUR.md](DOCUMENTATION_FRONTEND_ROLE_CONTROLEUR.md) — endpoints et parcours métier
- [DOCUMENTATION_FRONTEND_VITRINE_VUE.md](DOCUMENTATION_FRONTEND_VITRINE_VUE.md) — conventions client API Vue
- [Scripts/README_ROLE_CONTROLEUR.md](Scripts/README_ROLE_CONTROLEUR.md) — migration prod + reconnexion JWT

**Base URL dev :** `https://dev-knb.asdc-rdc.org`  
**Auth :** `Authorization: Bearer {jwt_token}`

**Hors périmètre :** guichet, `POST /api/Paiement`, Moko PayIn, wallet, correction présence (`PUT /api/Presence`).

---

## Table des matières

1. [Prérequis et routing](#1-prérequis-et-routing)
2. [Architecture des écrans](#2-architecture-des-écrans)
3. [Vue 3 — configuration](#3-vue-3--configuration)
4. [Vue 3 — client API et types](#4-vue-3--client-api-et-types)
5. [Vue 3 — composables](#5-vue-3--composables)
6. [Vue 3 — écrans exemples](#6-vue-3--écrans-exemples)
7. [Flutter — configuration](#7-flutter--configuration)
8. [Flutter — modèles et service API](#8-flutter--modèles-et-service-api)
9. [Flutter — state et écrans](#9-flutter--state-et-écrans)
10. [Flux pointage](#10-flux-pointage)
11. [Dashboard présence](#11-dashboard-présence)
12. [Feuille d'appel (optionnel v1)](#12-feuille-dappel-optionnel-v1)
13. [Gestion des erreurs](#13-gestion-des-erreurs)
14. [Checklist d'intégration](#14-checklist-dintégration)

---

## 1. Prérequis et routing

### JWT — claims utiles

| Claim | Usage front |
|-------|-------------|
| `sub` | Id utilisateur (`idUtilisateur`) |
| `idEcole` | Id école — **obligatoire** sur tous les appels |
| `role` (plusieurs) | `Controleur`, `Directeur`, `Admin`, etc. |

**Rôle principal :** `Controleur` — pointage + lecture frais.

**Rôles lecture seule (dashboard / feuille d'appel) :** `Directeur`, `Admin`, `Prefet`, `Super-Admin` (sans `Presence.Create` → pas de bouton pointage).

**Reconnexion obligatoire** après migration prod du rôle Controleur (nouveau claim JWT).

### Vue Router — garde de route

```javascript
// router/index.js
const CONTROLE_ENTREE_ROLES = ['Controleur', 'Directeur', 'Admin', 'Prefet', 'Super-Admin'];

const POINTAGE_ROLES = ['Controleur'];

router.beforeEach((to, from, next) => {
  if (!to.meta.requiresControleEntree) return next();

  const auth = useAuthStore();
  if (!auth.token) return next({ name: 'login', query: { redirect: to.fullPath } });

  const hasRole = auth.roles.some(r => CONTROLE_ENTREE_ROLES.includes(r));
  if (!hasRole) return next({ name: 'forbidden' });

  if (to.meta.requiresPointage) {
    const canPoint = auth.roles.some(r => POINTAGE_ROLES.includes(r));
    if (!canPoint) return next({ name: 'forbidden' });
  }

  next();
});

// routes
{
  path: '/controle-entree',
  name: 'controle-entree',
  component: () => import('@/views/controleEntree/ControleEntreeHome.vue'),
  meta: { requiresControleEntree: true }
},
{
  path: '/controle-entree/scan',
  name: 'controle-entree-scan',
  component: () => import('@/views/controleEntree/ControleEntreeScan.vue'),
  meta: { requiresControleEntree: true, requiresPointage: true }
},
{
  path: '/controle-entree/dashboard',
  name: 'controle-entree-dashboard',
  component: () => import('@/views/controleEntree/ControleEntreeDashboard.vue'),
  meta: { requiresControleEntree: true }
},
{
  path: '/controle-entree/feuille-appel',
  name: 'controle-entree-feuille',
  component: () => import('@/views/controleEntree/ControleEntreeFeuilleAppel.vue'),
  meta: { requiresControleEntree: true }
}
```

**Redirection post-login :** si rôle principal = `Controleur` → `/controle-entree/scan`.

**Menu global :** masquer les entrées Guichet / Moko / CRUD frais pour le rôle `Controleur` seul.

### Flutter — GoRouter

```dart
// lib/core/router/app_router.dart
const controleEntreeRoles = {'Controleur', 'Directeur', 'Admin', 'Prefet', 'Super-Admin'};
const pointageRoles = {'Controleur'};

GoRoute(
  path: '/controle-entree',
  redirect: (context, state) {
    final auth = context.read<AuthProvider>();
    if (!auth.isAuthenticated) return '/login';
    if (!auth.roles.any(controleEntreeRoles.contains)) return '/forbidden';
    return null;
  },
  routes: [
    GoRoute(path: '', builder: (_, __) => const ControleEntreeHomeScreen()),
    GoRoute(
      path: 'scan',
      redirect: (context, state) {
        final auth = context.read<AuthProvider>();
        if (!auth.roles.any(pointageRoles.contains)) return '/forbidden';
        return null;
      },
      builder: (_, __) => const ControleEntreeScanScreen(),
    ),
    GoRoute(path: 'dashboard', builder: (_, __) => const ControleEntreeDashboardScreen()),
    GoRoute(path: 'feuille-appel', builder: (_, __) => const ControleEntreeFeuilleAppelScreen()),
  ],
),
```

---

## 2. Architecture des écrans

| Écran | Route | Endpoints |
|-------|-------|-----------|
| Accueil | `/controle-entree` | Liens vers scan + dashboard |
| Scan / pointage | `/controle-entree/scan` | `GET /api/Eleve/reinscription`, `GET /api/Eleve/serial-number/{serial}`, `GET /api/VuePaiementsFraisParEcole/eleve-matricule/{matricule}`, `POST /api/Presence` |
| Dashboard jour | `/controle-entree/dashboard` | `GET /api/Presence/dashboard/ecole/{idEcole}` |
| Feuille d'appel | `/controle-entree/feuille-appel` | `GET /api/Presence/eleves/classe/{idClasse}/feuille-appel`, export xlsx |

**Règle :** toujours passer `idEcole` depuis le JWT, jamais saisi librement par l'utilisateur.

**UX tablette (recommandé) :**
- Champ matricule large, autofocus, compatible scan code-barres
- Lookup badge : `GET /api/Eleve/serial-number/{serial}` (autorisé pour Controleur)
- Carte élève + bandeau vert (frais OK) / rouge (impayés)
- Bouton « Marquer présent » bien visible
- Pas de bouton encaissement

---

## 3. Vue 3 — configuration

### Variables d'environnement

Identiques au guichet — voir [DOCUMENTATION_INTEGRATION_FRONTEND_CAISSIER_VUE_FLUTTER.md §3](DOCUMENTATION_INTEGRATION_FRONTEND_CAISSIER_VUE_FLUTTER.md).

`.env.development`

```env
VITE_API_BASE_URL=https://dev-knb.asdc-rdc.org
```

### Store auth (Pinia)

Réutiliser le store `auth` du guichet (`sub`, `idEcole`, `roles`). Aucune modification requise.

Ajouter un helper permissions (optionnel) :

```javascript
// stores/auth.js — extension
getters: {
  isControleur: (state) => state.roles.includes('Controleur'),
  canPointage: (state) => state.roles.includes('Controleur'),
  canViewControleEntree: (state) =>
    state.roles.some(r =>
      ['Controleur', 'Directeur', 'Admin', 'Prefet', 'Super-Admin'].includes(r)
    )
}
```

### Client HTTP

Réutiliser `src/api/http.js` du guichet (`apiFetch`, `parseApiError`).

---

## 4. Vue 3 — client API et types

### Service contrôle entrée

`src/api/controleEntree.js`

```javascript
import { apiFetch } from './http';

function buildQuery(params) {
  const q = new URLSearchParams();
  Object.entries(params).forEach(([k, v]) => {
    if (v != null && v !== '') q.set(k, String(v));
  });
  return q.toString();
}

/** @returns {Promise<import('../types/controleEntree').EleveReinscriptionPrefillDto>} */
export function searchEleveByMatricule(idEcole, matricule) {
  const qs = buildQuery({ idEcole, matricule });
  return apiFetch(`/api/Eleve/reinscription?${qs}`);
}

/** @returns {Promise<import('../types/controleEntree').VuePaiementsFraisParEcoleDto[]>} */
export function fetchFraisByMatricule(matricule) {
  return apiFetch(
    `/api/VuePaiementsFraisParEcole/eleve-matricule/${encodeURIComponent(matricule)}`
  );
}

/** @returns {Promise<import('../types/controleEntree').VuePaiementsFraisParEcoleDto[]>} */
export function fetchFraisByEleve(idEleve) {
  return apiFetch(`/api/VuePaiementsFraisParEcole/eleve/${idEleve}`);
}

/** @returns {Promise<object>} Présence créée */
export function createPresence({ idEleve, isPresent = true, dateDuJour, heureArrivee, observation }) {
  return apiFetch('/api/Presence', {
    method: 'POST',
    body: JSON.stringify({
      idEleve,
      isPresent,
      dateDuJour,
      heureArrivee,
      observation: observation ?? null
    })
  });
}

/** @returns {Promise<import('../types/controleEntree').DashboardPresenceDto>} */
export function fetchPresenceDashboard(idEcole, { date, dateDebut, dateFin, idAnneeScolaire } = {}) {
  const qs = buildQuery({ date, dateDebut, dateFin, idAnneeScolaire });
  const suffix = qs ? `?${qs}` : '';
  return apiFetch(`/api/Presence/dashboard/ecole/${idEcole}${suffix}`);
}

/** @returns {Promise<import('../types/controleEntree').FeuilleAppelClasseDto>} */
export function fetchFeuilleAppel(idClasse, { date, idAnneeScolaire } = {}) {
  const qs = buildQuery({ date, idAnneeScolaire });
  const suffix = qs ? `?${qs}` : '';
  return apiFetch(`/api/Presence/eleves/classe/${idClasse}/feuille-appel${suffix}`);
}

/** Téléchargement export Excel */
export async function downloadFeuilleAppelExport(idClasse, { date, idAnneeScolaire, format = 'xlsx' } = {}) {
  const qs = buildQuery({ date, idAnneeScolaire, format });
  const auth = (await import('@/stores/auth')).useAuthStore();
  const base = import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, '') || '';
  const res = await fetch(
    `${base}/api/Presence/eleves/classe/${idClasse}/feuille-appel/export?${qs}`,
    { headers: { Authorization: `Bearer ${auth.token}` } }
  );
  if (!res.ok) throw new Error(`Export échoué (${res.status})`);
  return res.blob();
}
```

### Helpers date / heure

`src/utils/controleEntree.js`

```javascript
/** Date du jour au format yyyy-MM-dd */
export function formatDateDuJour(date = new Date()) {
  return date.toISOString().slice(0, 10);
}

/** Heure d'arrivée obligatoire — format HH:mm accepté par l'API */
export function formatHeureArrivee(date = new Date()) {
  return date.toLocaleTimeString('fr-FR', {
    hour: '2-digit',
    minute: '2-digit',
    hour12: false
  });
}

/**
 * Agrège les lignes VuePaiementsFraisParEcole par idFrais.
 * @returns {{ idFrais, libelleFrais, montantFrais, totalPaye, reste, deviseFrais }[]}
 */
export function computeFraisSolde(lignes) {
  const byFrais = new Map();

  for (const row of lignes) {
    const idFrais = row.idFrais;
    if (idFrais == null) continue;

    if (!byFrais.has(idFrais)) {
      byFrais.set(idFrais, {
        idFrais,
        libelleFrais: row.libelleFrais,
        montantFrais: row.montantFrais ?? 0,
        totalPaye: 0,
        deviseFrais: row.deviseFrais ?? 'CDF'
      });
    }

    const entry = byFrais.get(idFrais);
    const statut = (row.statutPaiement ?? '').toLowerCase();
    if (statut === 'confirme' || statut === 'confirmé') {
      entry.totalPaye += row.montant ?? 0;
    }
    if (row.montantFrais != null) entry.montantFrais = row.montantFrais;
  }

  return [...byFrais.values()].map(f => ({
    ...f,
    reste: Math.max(0, f.montantFrais - f.totalPaye)
  }));
}

export function hasImpayes(fraisSolde) {
  return fraisSolde.some(f => f.reste > 0);
}
```

### Types TypeScript

`src/types/controleEntree.ts`

```typescript
export interface EleveReinscriptionPrefillDto {
  type?: string;
  idEleveExistant?: number;
  idEcole: number;
  nomEleve?: string;
  prenomEleve?: string;
  postnomEleve?: string;
  matriculeEleve?: string;
  photoEleveUrl?: string;
  nomClassePrecedente?: string;
  idClassePrecedente?: number;
  dejaInscritAnneeCourante: boolean;
  idInscriptionAnneeCourante?: number;
}

export interface VuePaiementsFraisParEcoleDto {
  idEleve: number;
  matricule?: string;
  nomCompletFormate?: string;
  idFrais?: number;
  libelleFrais?: string;
  montantFrais?: number;
  deviseFrais?: string;
  montant?: number;
  statutPaiement?: string;
  nomClasse?: string;
}

export interface FraisSoldeDto {
  idFrais: number;
  libelleFrais?: string;
  montantFrais: number;
  totalPaye: number;
  reste: number;
  deviseFrais: string;
}

export interface DashboardPresenceDto {
  ecole: { idEcole: number; nomEcole: string; logo?: string };
  periode: { type: string; date?: string; libelle: string };
  resumeEleves: ResumePresenceDto;
  resumeAgents: ResumePresenceDto;
  alertes?: AlerteDto[];
  classesProblematiques?: ClasseProblematiqueDto[];
}

export interface ResumePresenceDto {
  effectifTotal: number;
  presents: number;
  absents: number;
  retards: number;
  tauxPresence: number;
  tauxAbsence: number;
  tauxRetard: number;
}

export interface AlerteDto {
  type: string;
  message: string;
  action?: string;
}

export interface ClasseProblematiqueDto {
  idClasse: number;
  nomClasse: string;
  presents: number;
  absents: number;
  tauxPresence: number;
  status: string;
}

export interface FeuilleAppelClasseDto {
  idClasse: number;
  nomClasse: string;
  idEcole: number;
  date: string;
  effectif: number;
  nbPresents: number;
  nbAbsents: number;
  nbRetards: number;
  lignes: FeuilleAppelLigneDto[];
}

export interface FeuilleAppelLigneDto {
  idEleve: number;
  matricule?: string;
  nomComplet: string;
  statutJour: 'Present' | 'Absent' | 'Retard';
  idPresence?: number;
  isPresent?: boolean;
  heureArrivee?: string;
}
```

---

## 5. Vue 3 — composables

### Recherche élève + frais

`src/composables/useControleEntreeSearch.js`

```javascript
import { ref, computed } from 'vue';
import { useAuthStore } from '@/stores/auth';
import { searchEleveByMatricule, fetchFraisByMatricule } from '@/api/controleEntree';
import { computeFraisSolde, hasImpayes } from '@/utils/controleEntree';

export function useControleEntreeSearch() {
  const auth = useAuthStore();
  const loading = ref(false);
  const error = ref(null);
  const eleve = ref(null);
  const fraisLignes = ref([]);
  const fraisSolde = ref([]);
  const step = ref('idle'); // idle | eleve | frais | done

  const impayes = computed(() => hasImpayes(fraisSolde.value));
  const totalReste = computed(() =>
    fraisSolde.value.reduce((sum, f) => sum + f.reste, 0)
  );

  async function searchByMatricule(matricule) {
    loading.value = true;
    error.value = null;
    eleve.value = null;
    fraisLignes.value = [];
    fraisSolde.value = [];
    step.value = 'idle';

    try {
      const trimmed = matricule.trim();
      if (!trimmed) throw new Error('Matricule requis');

      eleve.value = await searchEleveByMatricule(auth.idEcole, trimmed);
      step.value = 'eleve';

      fraisLignes.value = await fetchFraisByMatricule(trimmed);
      fraisSolde.value = computeFraisSolde(fraisLignes.value);
      step.value = 'frais';

      return { eleve: eleve.value, fraisSolde: fraisSolde.value, impayes: impayes.value };
    } catch (e) {
      error.value = e.status === 404 ? 'Élève introuvable' : e.message;
      throw e;
    } finally {
      loading.value = false;
    }
  }

  function reset() {
    eleve.value = null;
    fraisLignes.value = [];
    fraisSolde.value = [];
    error.value = null;
    step.value = 'idle';
  }

  return {
    loading,
    error,
    eleve,
    fraisSolde,
    impayes,
    totalReste,
    step,
    searchByMatricule,
    reset
  };
}
```

### Pointage présence

`src/composables/usePointagePresence.js`

```javascript
import { ref } from 'vue';
import { createPresence } from '@/api/controleEntree';
import { formatDateDuJour, formatHeureArrivee } from '@/utils/controleEntree';

export function usePointagePresence() {
  const submitting = ref(false);
  const message = ref(null);
  const lastPresence = ref(null);

  async function pointageEleve(idEleve, { observation, blockIfImpayes = false, hasImpayes = false } = {}) {
    if (blockIfImpayes && hasImpayes) {
      throw new Error('Frais impayés — pointage bloqué par la politique de l\'école.');
    }

    submitting.value = true;
    message.value = null;

    try {
      lastPresence.value = await createPresence({
        idEleve,
        isPresent: true,
        dateDuJour: formatDateDuJour(),
        heureArrivee: formatHeureArrivee(),
        observation
      });
      message.value = 'Présence enregistrée.';
      return lastPresence.value;
    } finally {
      submitting.value = false;
    }
  }

  return { submitting, message, lastPresence, pointageEleve };
}
```

### Dashboard présence

`src/composables/usePresenceDashboard.js`

```javascript
import { ref, onMounted } from 'vue';
import { useAuthStore } from '@/stores/auth';
import { fetchPresenceDashboard } from '@/api/controleEntree';
import { formatDateDuJour } from '@/utils/controleEntree';

export function usePresenceDashboard(options = {}) {
  const auth = useAuthStore();
  const dashboard = ref(null);
  const loading = ref(false);
  const error = ref(null);

  async function load(date = options.date ?? formatDateDuJour()) {
    loading.value = true;
    error.value = null;
    try {
      dashboard.value = await fetchPresenceDashboard(auth.idEcole, {
        date,
        idAnneeScolaire: options.idAnneeScolaire
      });
    } catch (e) {
      error.value = e.message;
      throw e;
    } finally {
      loading.value = false;
    }
  }

  onMounted(() => load());

  return { dashboard, loading, error, refresh: load };
}
```

---

## 6. Vue 3 — écrans exemples

### ControleEntreeHome.vue

```vue
<script setup>
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth';

const router = useRouter();
const auth = useAuthStore();
</script>

<template>
  <div class="controle-entree-home">
    <header>
      <h1>Contrôle à l'entrée</h1>
      <p>École #{{ auth.idEcole }}</p>
    </header>

    <nav class="actions-grid">
      <button
        v-if="auth.canPointage"
        type="button"
        class="action-primary"
        @click="router.push('/controle-entree/scan')"
      >
        Scanner / Saisir matricule
      </button>
      <button type="button" @click="router.push('/controle-entree/dashboard')">
        Dashboard présence du jour
      </button>
      <button type="button" @click="router.push('/controle-entree/feuille-appel')">
        Feuille d'appel
      </button>
    </nav>
  </div>
</template>
```

### ControleEntreeScan.vue (flux principal)

```vue
<script setup>
import { ref, computed } from 'vue';
import { useControleEntreeSearch } from '@/composables/useControleEntreeSearch';
import { usePointagePresence } from '@/composables/usePointagePresence';

// Politique école : false = afficher alerte rouge mais autoriser le pointage
const BLOCK_POINTAGE_IF_IMPAYES = false;

const matricule = ref('');
const inputRef = ref(null);

const {
  loading,
  error,
  eleve,
  fraisSolde,
  impayes,
  totalReste,
  searchByMatricule,
  reset
} = useControleEntreeSearch();

const { submitting, message, pointageEleve } = usePointagePresence();

const canSubmitPointage = computed(() => {
  if (!eleve.value?.idEleveExistant) return false;
  if (BLOCK_POINTAGE_IF_IMPAYES && impayes.value) return false;
  return !submitting.value;
});

async function onSearch() {
  try {
    await searchByMatricule(matricule.value);
  } catch {
    /* error affiché via composable */
  }
}

async function onPointage() {
  try {
    await pointageEleve(eleve.value.idEleveExistant, {
      hasImpayes: impayes.value,
      blockIfImpayes: BLOCK_POINTAGE_IF_IMPAYES
    });
    reset();
    matricule.value = '';
    inputRef.value?.focus();
  } catch (e) {
    error.value = e.message;
  }
}

function onNewSearch() {
  reset();
  matricule.value = '';
  inputRef.value?.focus();
}
</script>

<template>
  <div class="controle-scan">
    <section class="search-bar">
      <label for="matricule">Matricule élève</label>
      <input
        id="matricule"
        ref="inputRef"
        v-model="matricule"
        autocomplete="off"
        autofocus
        placeholder="Scan ou saisie"
        @keyup.enter="onSearch"
      />
      <button type="button" :disabled="loading" @click="onSearch">
        {{ loading ? 'Recherche…' : 'Rechercher' }}
      </button>
    </section>

    <p v-if="error" class="banner banner-error">{{ error }}</p>
    <p v-if="message" class="banner banner-success">{{ message }}</p>

    <template v-if="eleve">
      <article class="eleve-card">
        <img v-if="eleve.photoEleveUrl" :src="eleve.photoEleveUrl" alt="" class="photo" />
        <div>
          <h2>{{ eleve.prenomEleve }} {{ eleve.nomEleve }} {{ eleve.postnomEleve }}</h2>
          <p>Matricule : <strong>{{ eleve.matriculeEleve }}</strong></p>
          <p v-if="eleve.nomClassePrecedente">Classe : {{ eleve.nomClassePrecedente }}</p>
        </div>
      </article>

      <div
        class="banner"
        :class="impayes ? 'banner-warning' : 'banner-success'"
      >
        <template v-if="impayes">
          Frais impayés — reste total : {{ totalReste.toLocaleString() }} CDF
        </template>
        <template v-else>
          Frais à jour — accès autorisé
        </template>
      </div>

      <table v-if="fraisSolde.length" class="frais-table">
        <thead>
          <tr>
            <th>Frais</th><th>Dû</th><th>Payé</th><th>Reste</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="f in fraisSolde" :key="f.idFrais" :class="{ 'row-impaye': f.reste > 0 }">
            <td>{{ f.libelleFrais }}</td>
            <td>{{ f.montantFrais }} {{ f.deviseFrais }}</td>
            <td>{{ f.totalPaye }}</td>
            <td>{{ f.reste }}</td>
          </tr>
        </tbody>
      </table>

      <footer class="scan-actions">
        <button type="button" class="btn-secondary" @click="onNewSearch">Nouveau matricule</button>
        <button
          type="button"
          class="btn-primary"
          :disabled="!canSubmitPointage"
          @click="onPointage"
        >
          {{ submitting ? 'Enregistrement…' : 'Marquer présent' }}
        </button>
      </footer>
    </template>
  </div>
</template>

<style scoped>
.controle-scan { max-width: 720px; margin: 0 auto; padding: 1rem; }
.search-bar { display: flex; gap: 0.5rem; align-items: end; }
.search-bar input { flex: 1; font-size: 1.25rem; padding: 0.75rem; }
.banner-success { background: #e8f5e9; color: #2e7d32; padding: 0.75rem; border-radius: 8px; }
.banner-warning { background: #fff3e0; color: #e65100; padding: 0.75rem; border-radius: 8px; }
.banner-error { background: #ffebee; color: #c62828; }
.row-impaye { background: #fff8e1; }
.btn-primary { font-size: 1.1rem; padding: 0.75rem 1.5rem; }
</style>
```

### ControleEntreeDashboard.vue

```vue
<script setup>
import { usePresenceDashboard } from '@/composables/usePresenceDashboard';

const { dashboard, loading, error, refresh } = usePresenceDashboard();
</script>

<template>
  <div class="controle-dashboard">
    <header>
      <h1>Présence du jour</h1>
      <button type="button" @click="refresh()">Actualiser</button>
    </header>

    <div v-if="loading">Chargement…</div>
    <div v-else-if="error">{{ error }}</div>

    <template v-else-if="dashboard">
      <h2>{{ dashboard.ecole.nomEcole }}</h2>
      <p>{{ dashboard.periode.libelle }}</p>

      <section class="kpi-grid">
        <article class="kpi">
          <span>Effectif</span>
          <strong>{{ dashboard.resumeEleves.effectifTotal }}</strong>
        </article>
        <article class="kpi">
          <span>Présents</span>
          <strong>{{ dashboard.resumeEleves.presents }}</strong>
        </article>
        <article class="kpi">
          <span>Absents</span>
          <strong>{{ dashboard.resumeEleves.absents }}</strong>
        </article>
        <article class="kpi">
          <span>Taux présence</span>
          <strong>{{ dashboard.resumeEleves.tauxPresence }}%</strong>
        </article>
      </section>

      <section v-if="dashboard.classesProblematiques?.length">
        <h3>Classes à surveiller</h3>
        <ul>
          <li v-for="c in dashboard.classesProblematiques" :key="c.idClasse">
            {{ c.nomClasse }} — {{ c.tauxPresence }}% ({{ c.status }})
          </li>
        </ul>
      </section>
    </template>
  </div>
</template>
```

---

## 7. Flutter — configuration

Structure recommandée :

```
lib/
  core/
    api/api_client.dart       # réutiliser du guichet
    config/app_config.dart
  features/
    controle_entree/
      api/controle_entree_api.dart
      models/
      providers/controle_entree_provider.dart
      screens/
      utils/frais_solde.dart
```

Lancer :

```bash
flutter run --dart-define=API_BASE_URL=https://dev-knb.asdc-rdc.org
```

Réutiliser `ApiClient` et `AuthProvider` du module guichet — voir [DOCUMENTATION_INTEGRATION_FRONTEND_CAISSIER_VUE_FLUTTER.md §7–8](DOCUMENTATION_INTEGRATION_FRONTEND_CAISSIER_VUE_FLUTTER.md).

---

## 8. Flutter — modèles et service API

### Utils solde frais

`lib/features/controle_entree/utils/frais_solde.dart`

```dart
class FraisSolde {
  FraisSolde({
    required this.idFrais,
    required this.libelleFrais,
    required this.montantFrais,
    required this.totalPaye,
    required this.reste,
    required this.deviseFrais,
  });

  final int idFrais;
  final String? libelleFrais;
  final double montantFrais;
  final double totalPaye;
  final double reste;
  final String deviseFrais;
}

List<FraisSolde> computeFraisSolde(List<Map<String, dynamic>> lignes) {
  final map = <int, FraisSolde>{};

  for (final row in lignes) {
    final idFrais = row['idFrais'] as int?;
    if (idFrais == null) continue;

    map.putIfAbsent(
      idFrais,
      () => FraisSolde(
        idFrais: idFrais,
        libelleFrais: row['libelleFrais'] as String?,
        montantFrais: (row['montantFrais'] as num?)?.toDouble() ?? 0,
        totalPaye: 0,
        reste: 0,
        deviseFrais: row['deviseFrais'] as String? ?? 'CDF',
      ),
    );

    final statut = (row['statutPaiement'] as String? ?? '').toLowerCase();
    if (statut == 'confirme' || statut == 'confirmé') {
      final entry = map[idFrais]!;
      final paye = (row['montant'] as num?)?.toDouble() ?? 0;
      map[idFrais] = FraisSolde(
        idFrais: entry.idFrais,
        libelleFrais: entry.libelleFrais,
        montantFrais: (row['montantFrais'] as num?)?.toDouble() ?? entry.montantFrais,
        totalPaye: entry.totalPaye + paye,
        reste: 0,
        deviseFrais: entry.deviseFrais,
      );
    }
  }

  return map.values
      .map((f) => FraisSolde(
            idFrais: f.idFrais,
            libelleFrais: f.libelleFrais,
            montantFrais: f.montantFrais,
            totalPaye: f.totalPaye,
            reste: (f.montantFrais - f.totalPaye).clamp(0, double.infinity),
            deviseFrais: f.deviseFrais,
          ))
      .toList();
}

String formatDateDuJour([DateTime? date]) {
  final d = date ?? DateTime.now();
  return '${d.year.toString().padLeft(4, '0')}-'
      '${d.month.toString().padLeft(2, '0')}-'
      '${d.day.toString().padLeft(2, '0')}';
}

String formatHeureArrivee([DateTime? date]) {
  final d = date ?? DateTime.now();
  return '${d.hour.toString().padLeft(2, '0')}:'
      '${d.minute.toString().padLeft(2, '0')}';
}
```

### ControleEntreeApi

`lib/features/controle_entree/api/controle_entree_api.dart`

```dart
import '../../../core/api/api_client.dart';

class ControleEntreeApi {
  ControleEntreeApi(this._client);
  final ApiClient _client;

  Future<Map<String, dynamic>> searchEleveByMatricule(int idEcole, String matricule) async {
    final res = await _client.dio.get('/api/Eleve/reinscription', queryParameters: {
      'idEcole': idEcole,
      'matricule': matricule,
    });
    return Map<String, dynamic>.from(res.data as Map);
  }

  Future<List<Map<String, dynamic>>> fetchFraisByMatricule(String matricule) async {
    final res = await _client.dio.get(
      '/api/VuePaiementsFraisParEcole/eleve-matricule/${Uri.encodeComponent(matricule)}',
    );
    return (res.data as List).map((e) => Map<String, dynamic>.from(e as Map)).toList();
  }

  Future<Map<String, dynamic>> createPresence({
    required int idEleve,
    bool isPresent = true,
    String? dateDuJour,
    String? heureArrivee,
    String? observation,
  }) async {
    final res = await _client.dio.post('/api/Presence', data: {
      'idEleve': idEleve,
      'isPresent': isPresent,
      'dateDuJour': dateDuJour ?? formatDateDuJour(),
      'heureArrivee': heureArrivee ?? formatHeureArrivee(),
      if (observation != null) 'observation': observation,
    });
    return Map<String, dynamic>.from(res.data as Map);
  }

  Future<Map<String, dynamic>> fetchPresenceDashboard(
    int idEcole, {
    String? date,
    int? idAnneeScolaire,
  }) async {
    final res = await _client.dio.get(
      '/api/Presence/dashboard/ecole/$idEcole',
      queryParameters: {
        if (date != null) 'date': date,
        if (idAnneeScolaire != null) 'idAnneeScolaire': idAnneeScolaire,
      },
    );
    return Map<String, dynamic>.from(res.data as Map);
  }

  Future<Map<String, dynamic>> fetchFeuilleAppel(
    int idClasse, {
    String? date,
    int? idAnneeScolaire,
  }) async {
    final res = await _client.dio.get(
      '/api/Presence/eleves/classe/$idClasse/feuille-appel',
      queryParameters: {
        if (date != null) 'date': date,
        if (idAnneeScolaire != null) 'idAnneeScolaire': idAnneeScolaire,
      },
    );
    return Map<String, dynamic>.from(res.data as Map);
  }
}
```

---

## 9. Flutter — state et écrans

### Provider scan + pointage

`lib/features/controle_entree/providers/controle_entree_provider.dart`

```dart
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../api/controle_entree_api.dart';
import '../utils/frais_solde.dart';

enum ControleStep { idle, eleve, frais, done }

class ControleEntreeState {
  const ControleEntreeState({
    this.loading = false,
    this.error,
    this.eleve,
    this.fraisSolde = const [],
    this.step = ControleStep.idle,
  });

  final bool loading;
  final String? error;
  final Map<String, dynamic>? eleve;
  final List<FraisSolde> fraisSolde;
  final ControleStep step;

  bool get impayes => fraisSolde.any((f) => f.reste > 0);

  ControleEntreeState copyWith({
    bool? loading,
    String? error,
    Map<String, dynamic>? eleve,
    List<FraisSolde>? fraisSolde,
    ControleStep? step,
  }) =>
      ControleEntreeState(
        loading: loading ?? this.loading,
        error: error,
        eleve: eleve ?? this.eleve,
        fraisSolde: fraisSolde ?? this.fraisSolde,
        step: step ?? this.step,
      );
}

class ControleEntreeNotifier extends Notifier<ControleEntreeState> {
  @override
  ControleEntreeState build() => const ControleEntreeState();

  Future<void> search(String matricule) async {
    state = state.copyWith(loading: true, error: null);
    try {
      final auth = ref.read(authProvider);
      final api = ref.read(controleEntreeApiProvider);
      final eleve = await api.searchEleveByMatricule(auth.idEcole, matricule.trim());
      final lignes = await api.fetchFraisByMatricule(matricule.trim());
      state = state.copyWith(
        loading: false,
        eleve: eleve,
        fraisSolde: computeFraisSolde(lignes),
        step: ControleStep.frais,
      );
    } catch (e) {
      state = state.copyWith(loading: false, error: e.toString());
    }
  }

  Future<bool> pointage({bool blockIfImpayes = false}) async {
    final idEleve = state.eleve?['idEleveExistant'] as int?;
    if (idEleve == null) return false;
    if (blockIfImpayes && state.impayes) {
      state = state.copyWith(error: 'Frais impayés — pointage bloqué.');
      return false;
    }

    state = state.copyWith(loading: true, error: null);
    try {
      final api = ref.read(controleEntreeApiProvider);
      await api.createPresence(idEleve: idEleve);
      state = const ControleEntreeState(step: ControleStep.done);
      return true;
    } catch (e) {
      state = state.copyWith(loading: false, error: e.toString());
      return false;
    }
  }

  void reset() => state = const ControleEntreeState();
}

final controleEntreeProvider =
    NotifierProvider<ControleEntreeNotifier, ControleEntreeState>(
  ControleEntreeNotifier.new,
);
```

### ControleEntreeScanScreen

`lib/features/controle_entree/screens/controle_entree_scan_screen.dart`

```dart
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../providers/controle_entree_provider.dart';

class ControleEntreeScanScreen extends ConsumerStatefulWidget {
  const ControleEntreeScanScreen({super.key});

  @override
  ConsumerState<ControleEntreeScanScreen> createState() => _ControleEntreeScanScreenState();
}

class _ControleEntreeScanScreenState extends ConsumerState<ControleEntreeScanScreen> {
  final _controller = TextEditingController();
  static const blockIfImpayes = false;

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(controleEntreeProvider);

    return Scaffold(
      appBar: AppBar(title: const Text('Contrôle entrée')),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            TextField(
              controller: _controller,
              autofocus: true,
              decoration: const InputDecoration(
                labelText: 'Matricule',
                hintText: 'Scan ou saisie',
              ),
              onSubmitted: (v) => ref.read(controleEntreeProvider.notifier).search(v),
            ),
            const SizedBox(height: 8),
            FilledButton(
              onPressed: state.loading
                  ? null
                  : () => ref.read(controleEntreeProvider.notifier).search(_controller.text),
              child: Text(state.loading ? 'Recherche…' : 'Rechercher'),
            ),
            if (state.error != null)
              Padding(
                padding: const EdgeInsets.only(top: 12),
                child: Text(state.error!, style: TextStyle(color: Theme.of(context).colorScheme.error)),
              ),
            if (state.eleve != null) ...[
              const SizedBox(height: 16),
              ListTile(
                title: Text(
                  '${state.eleve!['prenomEleve']} ${state.eleve!['nomEleve']}',
                  style: Theme.of(context).textTheme.titleLarge,
                ),
                subtitle: Text('Matricule : ${state.eleve!['matriculeEleve']}'),
              ),
              Card(
                color: state.impayes ? Colors.orange.shade50 : Colors.green.shade50,
                child: ListTile(
                  title: Text(state.impayes ? 'Frais impayés' : 'Frais à jour'),
                ),
              ),
              Expanded(
                child: ListView.builder(
                  itemCount: state.fraisSolde.length,
                  itemBuilder: (_, i) {
                    final f = state.fraisSolde[i];
                    return ListTile(
                      title: Text(f.libelleFrais ?? 'Frais'),
                      trailing: Text('Reste : ${f.reste} ${f.deviseFrais}'),
                    );
                  },
                ),
              ),
              Row(
                children: [
                  OutlinedButton(
                    onPressed: () {
                      ref.read(controleEntreeProvider.notifier).reset();
                      _controller.clear();
                    },
                    child: const Text('Nouveau'),
                  ),
                  const SizedBox(width: 8),
                  Expanded(
                    child: FilledButton(
                      onPressed: state.loading
                          ? null
                          : () async {
                              final ok = await ref
                                  .read(controleEntreeProvider.notifier)
                                  .pointage(blockIfImpayes: blockIfImpayes);
                              if (ok && mounted) {
                                ScaffoldMessenger.of(context).showSnackBar(
                                  const SnackBar(content: Text('Présence enregistrée')),
                                );
                                _controller.clear();
                              }
                            },
                      child: const Text('Marquer présent'),
                    ),
                  ),
                ],
              ),
            ],
          ],
        ),
      ),
    );
  }
}
```

---

## 10. Flux pointage

Machine d'états recommandée :

```
idle → (matricule) → eleve → frais → (Marquer présent) → done → idle
```

| Étape | Action UI | API |
|-------|-----------|-----|
| `idle` | Champ matricule vide, focus | — |
| `eleve` | Affiche fiche élève | `GET /api/Eleve/reinscription` |
| `frais` | Bandeau + tableau reste | `GET /api/VuePaiementsFraisParEcole/eleve-matricule/…` |
| pointage | Bouton « Marquer présent » | `POST /api/Presence` |
| `done` | Toast succès, reset | — |

**Corps POST Presence (champs obligatoires) :**

```json
{
  "idEleve": 123,
  "isPresent": true,
  "dateDuJour": "2026-08-31",
  "heureArrivee": "07:45"
}
```

**Politique impayés (configurable côté front) :**

| Mode | Comportement |
|------|--------------|
| Informatif (défaut) | Bandeau rouge + pointage autorisé |
| Strict | Désactiver « Marquer présent » si `reste > 0` |

Ne jamais proposer de redirection vers le guichet depuis cet écran.

---

## 11. Dashboard présence

Endpoint :

```
GET /api/Presence/dashboard/ecole/{idEcole}?date={yyyy-MM-dd}
```

Afficher :
- KPI élèves : effectif, présents, absents, retards, taux
- Alertes (`alertes[]`)
- Classes problématiques (`classesProblematiques[]`)

Rafraîchissement manuel ou auto toutes les 60 s (optionnel).

---

## 12. Feuille d'appel (optionnel v1)

### Consultation

```
GET /api/Presence/eleves/classe/{idClasse}/feuille-appel?date={yyyy-MM-dd}
```

Afficher `lignes[]` avec `statutJour` : `Present`, `Absent`, `Retard`.

### Export Excel

```
GET /api/Presence/eleves/classe/{idClasse}/feuille-appel/export?date=&format=xlsx
```

Vue : `downloadFeuilleAppelExport()` → blob → lien téléchargement.

Flutter : `dio.download` ou ouvrir URL signée si vous ajoutez un proxy.

**Note :** le Controleur peut consulter et exporter ; il ne peut pas `PUT` pour corriger une ligne.

---

## 13. Gestion des erreurs

| Code | Contexte | Action UX |
|------|----------|-----------|
| **401** | Token expiré | Déconnexion → login |
| **403** | Permission manquante | Message « Action non autorisée » ; masquer bouton pointage si rôle sans `Presence.Create` |
| **404** | Matricule inconnu | « Aucun élève trouvé » — focus sur champ matricule |
| **400** | Validation (doublon présence, heure invalide) | Afficher `message` API |
| **500** | Erreur serveur | Toast + retry |

**403 spécifiques à ne pas confondre :**

| Endpoint | Signification pour Controleur |
|----------|------------------------------|
| `POST /api/Paiement` | Normal — pas caissier |
| `PUT /api/Presence/{id}` | Normal — pas enseignant |
| `GET …/wallet/mouvements` | Normal — pas financier |

Intercepteur HTTP global (comme guichet) :

```javascript
if (res.status === 401) {
  auth.logout();
  router.push('/login');
}
```

---

## 14. Checklist d'intégration

### Vue 3

- [ ] Routes `/controle-entree/*` avec garde rôle
- [ ] Redirection post-login `Controleur` → `/controle-entree/scan`
- [ ] Service `controleEntree.js` + types TS
- [ ] Composables recherche / pointage / dashboard
- [ ] Écran scan tablette (matricule + frais + pointage)
- [ ] Bandeau vert/rouge selon solde
- [ ] `heureArrivee` envoyé à chaque POST Presence
- [ ] Menu sans guichet / Moko / CRUD frais
- [ ] Gestion 401 / 403 / 404

### Flutter

- [ ] Feature `controle_entree/` + GoRouter
- [ ] `ControleEntreeApi` + utils solde
- [ ] Provider scan / pointage
- [ ] Écran scan tablette
- [ ] Dashboard présence (écran secondaire)
- [ ] Feuille d'appel (optionnel v1)

### Déploiement

- [ ] API déployée avec rôle Controleur (seeder)
- [ ] Migration SQL agents `contrôleur` si prod ([README_ROLE_CONTROLEUR.md](Scripts/README_ROLE_CONTROLEUR.md))
- [ ] Reconnexion utilisateurs contrôleurs (JWT `Controleur`)
- [ ] Test manuel : matricule → frais → pointage → dashboard

---

**Documentation API complémentaire :** [DOCUMENTATION_FRONTEND_ROLE_CONTROLEUR.md](DOCUMENTATION_FRONTEND_ROLE_CONTROLEUR.md)
