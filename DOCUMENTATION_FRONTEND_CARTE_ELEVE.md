# Intégration Frontend — Carte d'élève (recto-verso)

Guide pour brancher l'UI web sur l'impression / aperçu des **cartes élèves** KelasiNaBiso.

**Template serveur** : `Reports/Templates/RectoEleve.frx` (recto + verso, logo école au verso)  
**Auth** : `Authorization: Bearer {token}`  
**Rôles** : `Admin`, `Directeur`, `Super-Admin`, `IT-Support`  
**Contenu** : textes en **MAJUSCULES** ; PDF = **2 pages** par élève (recto puis verso)

---

## 1. Endpoints carte élève

Base API (dev) : `https://dev-knb.asdc-rdc.org`  
Base locale : `https://localhost:7102`

| Méthode | Route | Description | Réponse |
|---------|-------|-------------|---------|
| `GET` | `/api/Carte/eleve/{idEleve}/pdf` | PDF 1 élève (recto + verso) | `application/pdf` |
| `GET` | `/api/Carte/eleve/{idEleve}/apercu` | Aperçu HTML FastReport | `text/html` |
| `GET` | `/api/Carte/eleve/{idEleve}/viewer` | Redirige vers page Aperçu serveur | `302` |
| `POST` | `/api/Carte/eleves/pdf` | PDF lot (sélection) | `application/pdf` |
| `GET` | `/api/Carte/eleves/ecole/{idEcole}/pdf?idClasse={id}` | PDF toute l'école (optionnellement filtrée par classe) | `application/pdf` |

### Body lot — `POST /api/Carte/eleves/pdf`

```json
{
  "idEcole": 13,
  "ids": [42, 57, 88]
}
```

| Champ | Type | Obligatoire |
|-------|------|-------------|
| `idEcole` | number | oui |
| `ids` | number[] | oui (min. 1) |

Chaque élève produit **2 pages** dans le PDF (recto + verso).

---

## 2. Listes pour alimenter l'UI

| Besoin | Endpoint |
|--------|----------|
| Écoles | `GET /api/Ecole` |
| Élèves par école | `GET /api/V_Eleve/ecole/{idEcole}` |
| Classes (filtre) | `GET /api/Classe/ecole/{idEcole}` |

Champs utiles côté liste (`V_Eleve`) pour les vignettes UI :

| Champ | Usage |
|-------|--------|
| `idEleve` | ID pour PDF / aperçu |
| `nomComplet` | Colonne nom |
| `matricule` | Colonne matricule |
| `nomClasse` | Colonne classe |
| `photoUrl` | Photo vignette (voir helper image) |
| `logoUrlEcole` | Logo école vignette |
| `nomEcole` | Libellé école |

> Il n'existe **pas** encore de `GET /api/Eleve/.../carte-data` (contrairement aux agents). Utiliser `V_Eleve` + helper `toImageSrc` pour l'UI, et FastReport pour l'impression PDF.

---

## 3. Contenu généré (recto / verso)

### Recto
- École, photo élève, logo école (header)
- Nom complet
- `MATRICULE : …`
- `CLASSE : …`
- `ADRESSE : …`
- Année scolaire

### Verso
- Fond officiel + **logo de l'école** (`logo1`)

---

## 4. Helper image (vignettes liste)

```javascript
export function toImageSrc(value) {
  if (!value) return null;
  const v = String(value).trim();
  if (v.startsWith('http://') || v.startsWith('https://') || v.startsWith('data:')) return v;
  // base64 brut
  if (/^[A-Za-z0-9+/=]+$/.test(v.slice(0, 80))) {
    const mime = v.startsWith('/9j/') ? 'image/jpeg' : 'image/png';
    return `data:${mime};base64,${v}`;
  }
  return v;
}
```

```html
<img :src="toImageSrc(eleve.photoUrl)" alt="photo" />
<img :src="toImageSrc(eleve.logoUrlEcole)" alt="logo" />
```

---

## 5. Client API (exemple)

```javascript
const API_BASE = import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, '') || '';

function authHeaders(accept = '*/*') {
  return {
    Authorization: `Bearer ${localStorage.getItem('token')}`,
    Accept: accept
  };
}

async function downloadBlob(path, filename) {
  const res = await fetch(`${API_BASE}${path}`, { headers: authHeaders() });
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

/** PDF 1 élève */
export function downloadCarteElevePdf(idEleve) {
  return downloadBlob(
    `/api/Carte/eleve/${idEleve}/pdf`,
    `carte-eleve-${idEleve}.pdf`
  );
}

/** PDF sélection */
export async function downloadCartesElevesLot(idEcole, ids) {
  const res = await fetch(`${API_BASE}/api/Carte/eleves/pdf`, {
    method: 'POST',
    headers: { ...authHeaders(), 'Content-Type': 'application/json' },
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

/** PDF toute l'école (optionnellement une classe) */
export function downloadCartesElevesEcole(idEcole, idClasse = null) {
  const qs = idClasse ? `?idClasse=${idClasse}` : '';
  return downloadBlob(
    `/api/Carte/eleves/ecole/${idEcole}/pdf${qs}`,
    `cartes-eleves-ecole-${idEcole}.pdf`
  );
}

/** Aperçu HTML dans un iframe / nouvel onglet.
 * Ne pas forcer Accept: application/json (axios/interceptors) — l’API renvoie text/html.
 * Si Accept: application/json était bloqué (406), l’API l’accepte désormais aussi. */
export async function getApercuCarteEleveHtml(idEleve) {
  const res = await fetch(`${API_BASE}/api/Carte/eleve/${idEleve}/apercu`, {
    headers: authHeaders('text/html')
  });
  if (!res.ok) throw new Error(`Aperçu impossible (${res.status})`);
  return await res.text();
}
```

---

## 6. Exemple Vue 3 (extrait)

```vue
<script setup>
import { ref } from 'vue';
import {
  downloadCarteElevePdf,
  downloadCartesElevesLot,
  downloadCartesElevesEcole,
  getApercuCarteEleveHtml,
  toImageSrc
} from '@/api/carteEleve';

const eleves = ref([]);
const selected = ref([]); // idEleve[]
const idEcole = ref(null);
const apercuHtml = ref('');

async function imprimerUn(id) {
  await downloadCarteElevePdf(id);
}

async function imprimerSelection() {
  if (!idEcole.value || selected.value.length === 0) return;
  await downloadCartesElevesLot(idEcole.value, selected.value);
}

async function imprimerEcole() {
  await downloadCartesElevesEcole(idEcole.value);
}

async function apercu(id) {
  apercuHtml.value = await getApercuCarteEleveHtml(id);
}
</script>

<template>
  <table>
    <tr v-for="e in eleves" :key="e.idEleve">
      <td><input type="checkbox" :value="e.idEleve" v-model="selected" /></td>
      <td><img :src="toImageSrc(e.photoUrl)" width="40" height="40" /></td>
      <td>{{ e.nomComplet }}</td>
      <td>{{ e.matricule }}</td>
      <td>{{ e.nomClasse }}</td>
      <td>
        <button @click="apercu(e.idEleve)">Aperçu</button>
        <button @click="imprimerUn(e.idEleve)">PDF</button>
      </td>
    </tr>
  </table>

  <button @click="imprimerSelection">Imprimer la sélection</button>
  <button @click="imprimerEcole">Imprimer toute l'école</button>

  <!-- Aperçu : 2 pages (recto + verso) dans le HTML FastReport -->
  <div v-if="apercuHtml" v-html="apercuHtml" />
</template>
```

---

## 7. Points d'attention

1. **Ne pas** ouvrir le PDF via `<a href="/api/Carte/...">` : le JWT n'est pas envoyé → **401**. Toujours `fetch` + `Authorization`.
2. **Multi-écoles (Super-Admin)** : passer `idEcole` cohérent avec les `ids` du lot.
3. **Accès école** : Admin / Directeur / **IT-Support** ne voient que leur école ; Super-Admin : toutes.
4. Aperçu HTML peut être lourd (fonds images) — préférer un modal / onglet dédié.
5. Après déploiement template `.frx`, **redémarrer l'API**.

---

## 8. Codes HTTP

| Code | Signification |
|------|----------------|
| 200 | PDF / HTML OK |
| 400 | Body lot invalide |
| 401 | Token manquant / expiré |
| 403 | École non autorisée |
| 404 | Élève introuvable |
| 500 | Erreur génération FastReport |

---

## 9. Checklist intégration

- [ ] Liste élèves via `GET /api/V_Eleve/ecole/{idEcole}`
- [ ] Vignettes photo/logo via `toImageSrc`
- [ ] Bouton PDF unitaire → `GET /api/Carte/eleve/{id}/pdf` avec Bearer
- [ ] Sélection multiple → `POST /api/Carte/eleves/pdf`
- [ ] Toute l'école / classe → `GET /api/Carte/eleves/ecole/{id}/pdf`
- [ ] Aperçu → `GET /api/Carte/eleve/{id}/apercu` (recto + verso)
- [ ] Vérifier PDF : **2 pages** par élève, logo école au verso
