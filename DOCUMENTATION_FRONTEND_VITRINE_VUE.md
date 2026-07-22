# Intégration Vue.js — Site vitrine KelasiNaBiso

Guide pour consommer les endpoints publics vitrine depuis **Vue 3** (Composition API).

**API** : `https://dev-knb.asdc-rdc.org` (prod/dev selon environnement)  
**Auth** : aucune (endpoints publics)  
**CORS** : origine vitrine autorisée (`https://dev-knb.kansaconsulting.com`, etc.)

---

## 1. Endpoints

| Endpoint | Usage UI |
|----------|----------|
| `GET /api/vitrine/statistiques` | Bloc chiffres clés (120+ écoles, 35 000+ élèves…) |
| `GET /api/vitrine/partenaires?limit=20` | Carrousel logos partenaires |
| `POST /api/vitrine/contact` | Formulaire contact → `contact@kelasinabiso.com` |

### Réponses

**Statistiques**
```json
{
  "nombreEcoles": 120,
  "nombreEleves": 35000,
  "nombrePersonnel": 4500,
  "nombreParentsConnectes": 28000,
  "dateMiseAJour": "2026-07-10T13:00:00Z"
}
```

**Partenaires** (logo stocké en **base64** en base)
```json
[
  {
    "idEcole": 13,
    "nom": "Complexe Scolaire ABC",
    "logoBase64": "iVBORw0KGgoAAAANS...",
    "logoSrc": "data:image/png;base64,iVBORw0KGgoAAAANS...",
    "type": "Privee",
    "ville": "Kinshasa"
  }
]
```

> Utiliser **`logoSrc`** directement dans `<img :src="logoSrc">`.  
> Ne pas traiter `logoBase64` comme une URL HTTP.

### Contact (`POST /api/vitrine/contact`)

Envoie le formulaire de la page `/contact` vers **contact@kelasinabiso.com** (SMTP `no-reply@kelasinabiso.com`, Reply-To = email du visiteur).

**Corps JSON** (camelCase) :

```json
{
  "nomComplet": "Jean Dupont",
  "etablissement": "École ABC",
  "telephone": "+243 812 345 678",
  "email": "jean@exemple.cd",
  "effectif": "450 élèves",
  "objet": "Demande de démo",
  "message": "Nous souhaitons une démonstration à Kinshasa.",
  "website": ""
}
```

| Champ | Obligatoire | Notes |
|-------|-------------|-------|
| `nomComplet` | oui | max 150 |
| `email` | oui | format email valide |
| `message` | oui | min 10, max 5000 caractères |
| `etablissement`, `telephone`, `effectif`, `objet` | non | optionnels |
| `website` | non | **honeypot** — laisser vide ou omettre |

**Succès (200)** :
```json
{
  "success": true,
  "message": "Votre message a bien été envoyé. Notre équipe vous répondra sous 24 à 48 h ouvrées."
}
```

**Validation (400)** : erreurs par champ (`errors` ASP.NET).

**Échec envoi SMTP (503)** :
```json
{
  "success": false,
  "message": "Impossible d'envoyer votre message pour le moment..."
}
```

**Rate limit** : 3 requêtes / 5 min et 10 / h par IP (HTTP 429).

**Exemple fetch** :
```javascript
async function envoyerContact(form) {
  const res = await fetch(`${API_BASE}/api/vitrine/contact`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
    body: JSON.stringify({
      nomComplet: form.nomComplet,
      etablissement: form.etablissement || null,
      telephone: form.telephone || null,
      email: form.email,
      effectif: form.effectif || null,
      objet: form.objet || null,
      message: form.message,
      website: '' // honeypot — ne pas afficher à l'utilisateur
    })
  });

  const data = await res.json();
  if (!res.ok) throw new Error(data.message || data.title || `Erreur ${res.status}`);
  return data;
}
```

---

## 2. Configuration environnement

`.env.development`
```env
VITE_API_BASE_URL=https://dev-knb.asdc-rdc.org
```

`.env.production`
```env
VITE_API_BASE_URL=https://dev-knb.asdc-rdc.org
```

---

## 3. Client API

`src/api/vitrine.js`

```javascript
const API_BASE = import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, '') || '';

async function getJson(path) {
  const res = await fetch(`${API_BASE}${path}`, {
    method: 'GET',
    headers: { Accept: 'application/json' }
  });

  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error(err.message || `Erreur API ${res.status}`);
  }

  return res.json();
}

/** @returns {Promise<import('../types/vitrine').VitrineStatistiques>} */
export function fetchVitrineStatistiques() {
  return getJson('/api/vitrine/statistiques');
}

/** @returns {Promise<import('../types/vitrine').EcolePartenaire[]>} */
export function fetchVitrinePartenaires(limit = 20) {
  return getJson(`/api/vitrine/partenaires?limit=${limit}`);
}
```

---

## 4. Types (optionnel — TypeScript)

`src/types/vitrine.ts`

```typescript
export interface VitrineStatistiques {
  nombreEcoles: number;
  nombreEleves: number;
  nombrePersonnel: number;
  nombreParentsConnectes: number;
  dateMiseAJour: string;
}

export interface EcolePartenaire {
  idEcole: number;
  nom: string;
  logoBase64?: string | null;
  logoSrc?: string | null;
  type?: string | null;
  ville?: string | null;
}
```

---

## 5. Composables

`src/composables/useVitrineStatistiques.js`

```javascript
import { ref, onMounted } from 'vue';
import { fetchVitrineStatistiques } from '@/api/vitrine';

export function useVitrineStatistiques() {
  const stats = ref(null);
  const loading = ref(true);
  const error = ref(null);

  async function load() {
    loading.value = true;
    error.value = null;
    try {
      stats.value = await fetchVitrineStatistiques();
    } catch (e) {
      error.value = e.message || 'Impossible de charger les statistiques';
    } finally {
      loading.value = false;
    }
  }

  onMounted(load);

  return { stats, loading, error, reload: load };
}
```

`src/composables/useVitrinePartenaires.js`

```javascript
import { ref, onMounted } from 'vue';
import { fetchVitrinePartenaires } from '@/api/vitrine';

export function useVitrinePartenaires(limit = 20) {
  const partenaires = ref([]);
  const loading = ref(true);
  const error = ref(null);

  async function load() {
    loading.value = true;
    error.value = null;
    try {
      partenaires.value = await fetchVitrinePartenaires(limit);
    } catch (e) {
      error.value = e.message || 'Impossible de charger les partenaires';
    } finally {
      loading.value = false;
    }
  }

  onMounted(load);

  return { partenaires, loading, error, reload: load };
}
```

---

## 6. Utilitaire affichage chiffres

`src/utils/formatStat.js`

```javascript
/** Affiche 120 → "120+", 35000 → "35 000+" */
export function formatStatPlus(value) {
  if (value == null || Number.isNaN(Number(value))) return '0+';
  return `${Number(value).toLocaleString('fr-FR')}+`;
}
```

---

## 7. Composant — Statistiques (4 cartes)

`src/components/vitrine/VitrineStatsSection.vue`

```vue
<script setup>
import { computed } from 'vue';
import { useVitrineStatistiques } from '@/composables/useVitrineStatistiques';
import { formatStatPlus } from '@/utils/formatStat';

const { stats, loading, error } = useVitrineStatistiques();

const cards = computed(() => {
  if (!stats.value) return [];
  return [
    { icon: 'school', label: 'Écoles', value: formatStatPlus(stats.value.nombreEcoles) },
    { icon: 'graduation-cap', label: 'Élèves', value: formatStatPlus(stats.value.nombreEleves) },
    { icon: 'id-card', label: 'Personnel', value: formatStatPlus(stats.value.nombrePersonnel) },
    { icon: 'users', label: 'Parents connectés', value: formatStatPlus(stats.value.nombreParentsConnectes) }
  ];
});
</script>

<template>
  <section class="vitrine-stats" aria-label="Statistiques KelasiNaBiso">
    <p v-if="loading" class="muted">Chargement des statistiques…</p>
    <p v-else-if="error" class="error">{{ error }}</p>

    <div v-else class="stats-grid">
      <article v-for="card in cards" :key="card.label" class="stat-card">
        <div class="stat-icon" :data-icon="card.icon" />
        <p class="stat-value">{{ card.value }}</p>
        <p class="stat-label">{{ card.label }}</p>
      </article>
    </div>
  </section>
</template>

<style scoped>
.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 1rem;
}
.stat-card {
  background: #fff;
  border: 1px solid #e8edf5;
  border-radius: 12px;
  padding: 1.5rem;
  text-align: center;
}
.stat-value {
  font-size: 1.75rem;
  font-weight: 700;
  color: #0b1f3a;
  margin: 0.5rem 0 0.25rem;
}
.stat-label {
  color: #5b6b82;
  font-size: 0.95rem;
}
.muted { color: #6b7280; }
.error { color: #b91c1c; }
</style>
```

---

## 8. Composant — Partenaires (logos base64)

`src/components/vitrine/VitrinePartenairesSection.vue`

```vue
<script setup>
import { useVitrinePartenaires } from '@/composables/useVitrinePartenaires';

const props = defineProps({
  limit: { type: Number, default: 20 }
});

const { partenaires, loading, error } = useVitrinePartenaires(props.limit);

/** Initiales de secours si pas de logo */
function initiales(nom) {
  return (nom || '')
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map(w => w[0]?.toUpperCase() ?? '')
    .join('');
}
</script>

<template>
  <section class="partenaires" aria-label="Nos partenaires">
    <header class="partenaires-header">
      <div>
        <p class="eyebrow">RÉSEAU</p>
        <h2>Nos partenaires</h2>
        <p class="subtitle">
          Écoles, institutions et acteurs éducatifs qui font grandir l'écosystème KelasiNaBiso.
        </p>
      </div>
    </header>

    <p v-if="loading" class="muted">Chargement des partenaires…</p>
    <p v-else-if="error" class="error">{{ error }}</p>

    <div v-else class="logos-row">
      <div
        v-for="p in partenaires"
        :key="p.idEcole"
        class="logo-card"
        :title="p.nom"
      >
        <img
          v-if="p.logoSrc"
          :src="p.logoSrc"
          :alt="`Logo ${p.nom}`"
          loading="lazy"
          class="logo-img"
        />
        <span v-else class="logo-fallback">{{ initiales(p.nom) }}</span>
      </div>
    </div>
  </section>
</template>

<style scoped>
.partenaires-header { margin-bottom: 1.5rem; }
.eyebrow {
  color: #16a34a;
  font-size: 0.75rem;
  font-weight: 700;
  letter-spacing: 0.08em;
}
.subtitle { color: #5b6b82; max-width: 42rem; }
.logos-row {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
}
.logo-card {
  width: 88px;
  height: 88px;
  border-radius: 12px;
  border: 1px solid #e8edf5;
  background: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
}
.logo-img {
  max-width: 72px;
  max-height: 72px;
  object-fit: contain;
}
.logo-fallback {
  font-weight: 700;
  color: #0b1f3a;
  font-size: 1.1rem;
}
</style>
```

---

## 9. Page d'accueil

`src/views/HomeView.vue` (extrait)

```vue
<script setup>
import VitrineStatsSection from '@/components/vitrine/VitrineStatsSection.vue';
import VitrinePartenairesSection from '@/components/vitrine/VitrinePartenairesSection.vue';
</script>

<template>
  <main>
    <!-- Hero existant -->
    <section class="hero">…</section>

    <!-- Chiffres clés -->
    <VitrineStatsSection />

    <!-- Partenaires -->
    <VitrinePartenairesSection :limit="20" />
  </main>
</template>
```

---

## 10. Test rapide (console navigateur)

Sur `https://dev-knb.kansaconsulting.com` :

```javascript
fetch('https://dev-knb.asdc-rdc.org/api/vitrine/statistiques')
  .then(r => r.json())
  .then(console.log);

fetch('https://dev-knb.asdc-rdc.org/api/vitrine/partenaires?limit=5')
  .then(r => r.json())
  .then(console.log);
```

Si erreur CORS → vérifier que l’origine du site vitrine est dans `Cors:AllowedOrigins` côté API.

---

## 11. Checklist intégration

- [ ] `VITE_API_BASE_URL` configuré
- [ ] Pas de token JWT sur ces appels
- [ ] `<img :src="partenaire.logoSrc">` (pas `logoBase64` brut seul)
- [ ] Gestion `loading` / `error` sur les deux sections
- [ ] Format chiffres avec `formatStatPlus` pour l’UI « 120+ »
- [ ] Fallback initiales si école sans logo

---

*KelasiNaBiso — Site vitrine Vue.js — Juillet 2026*
