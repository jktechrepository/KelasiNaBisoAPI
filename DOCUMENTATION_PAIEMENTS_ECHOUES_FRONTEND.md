# 📚 Documentation Frontend - Gestion des Paiements Échoués

## 🌟 Vue d'ensemble

Le système de gestion des paiements échoués permet de :
- **Sauvegarder automatiquement** tous les paiements qui échouent lors de l'import Excel
- **Consulter** la liste des paiements échoués avec leurs erreurs détaillées
- **Corriger** individuellement ou en masse les paiements échoués
- **Réinjecter** les paiements corrigés dans le système après validation

### 🔗 Informations de base
- **URL de base**: `https://localhost:7102` (HTTPS) ou `http://localhost:5002` (HTTP)
- **Format des données**: JSON
- **Authentification**: Token JWT (Bearer Token)
- **Rôles requis**: `Admin`, `Super-Admin`, `Directeur`, `Financier`

---

## 📋 Table des matières

1. [Configuration](#configuration)
2. [Structures de données](#structures-de-données)
3. [Endpoints API](#endpoints-api)
4. [Exemples d'utilisation](#exemples-dutilisation)
5. [Cas d'usage typiques](#cas-dusage-typiques)
6. [Gestion des erreurs](#gestion-des-erreurs)
7. [Bonnes pratiques](#bonnes-pratiques)

---

## ⚙️ Configuration

### 1. Configuration Axios (JavaScript/Vue.js)

```javascript
import axios from 'axios';

const api = axios.create({
  baseURL: process.env.VUE_APP_API_BASE_URL || 'https://localhost:7102',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Intercepteur pour ajouter le token JWT
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Intercepteur pour gérer les erreurs
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Rediriger vers la page de connexion
      localStorage.removeItem('accessToken');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default api;
```

### 2. Service API dédié (Vue.js)

```javascript
// services/paiementCrashed.service.js
import api from './api';

export const paiementCrashedService = {
  // Récupérer tous les paiements échoués
  async getAll(estResolu = null) {
    const params = estResolu !== null ? { estResolu } : {};
    const response = await api.get('/api/PaiementCrashed/ecole', { params });
    return response.data;
  },

  // Récupérer un paiement échoué par ID
  async getById(id) {
    const response = await api.get(`/api/PaiementCrashed/${id}`);
    return response.data;
  },

  // Modifier un paiement échoué
  async update(id, data) {
    const response = await api.put(`/api/PaiementCrashed/${id}`, data);
    return response.data;
  },

  // Modifier plusieurs paiements en masse
  async bulkUpdate(ids, data) {
    const response = await api.put('/api/PaiementCrashed/bulk-update', {
      ids,
      ...data,
    });
    return response.data;
  },

  // Réinjecter des paiements échoués
  async reinject(ids, forcerReinjection = false) {
    const response = await api.post('/api/PaiementCrashed/reinject', {
      ids,
      forcerReinjection,
    });
    return response.data;
  },

  // Supprimer un paiement échoué
  async delete(id) {
    await api.delete(`/api/PaiementCrashed/${id}`);
  },

  // Supprimer tous les paiements résolus
  async deleteResolved() {
    const response = await api.delete('/api/PaiementCrashed/ecole/resolved');
    return response.data;
  },
};
```

---

## 📊 Structures de données

### PaiementCrashedDto (Réponse)

```typescript
interface PaiementCrashedDto {
  idPaiementCrashed: number;
  datePaiement?: string; // ISO 8601 format
  montant?: number;
  devise?: string;
  modePaiement?: string;
  statutPaiement?: string;
  referenceTransaction?: string;
  commentaire?: string;
  idEleve?: number;
  idFrais?: number;
  nomCompletEleve?: string; // Nom original du fichier Excel
  libelleFrais?: string; // Libellé original du fichier Excel
  erreurs: string[]; // Liste des erreurs
  numeroLigne: number; // Numéro de ligne dans le fichier Excel
  nomFichierOriginal?: string;
  dateEchec: string; // ISO 8601 format
  dateCorrection?: string; // ISO 8601 format
  dateReinjection?: string; // ISO 8601 format
  idPaiementCree?: number; // ID du paiement créé après réinjection
  estResolu: boolean;
  nomEleve?: string; // Nom de l'élève si trouvé
  nomFrais?: string; // Nom du frais si trouvé
}
```

### UpdatePaiementCrashedDto (Modification individuelle)

```typescript
interface UpdatePaiementCrashedDto {
  idEleve?: number; // Doit être > 0 si fourni
  idFrais?: number; // Doit être > 0 si fourni
  datePaiement?: string;
  montant?: number; // Doit être > 0.01 si fourni
  devise?: string; // Max 10 caractères
  modePaiement?: string; // Max 50 caractères
  statutPaiement?: string; // Max 50 caractères
  referenceTransaction?: string; // Max 200 caractères
  commentaire?: string; // Max 1000 caractères
}
```

### BulkUpdatePaiementCrashedDto (Modification en masse)

```typescript
interface BulkUpdatePaiementCrashedDto {
  ids: number[]; // Liste des IDs à modifier (obligatoire)
  idEleve?: number;
  idFrais?: number;
  datePaiement?: string;
  montant?: number;
  devise?: string;
  modePaiement?: string;
  statutPaiement?: string;
}
```

### ReinjectPaiementCrashedDto (Réinjection)

```typescript
interface ReinjectPaiementCrashedDto {
  ids: number[]; // Liste des IDs à réinjecter (obligatoire)
  forcerReinjection?: boolean; // Par défaut: false
}
```

### ReinjectPaiementCrashedResult (Résultat de réinjection)

```typescript
interface ReinjectPaiementCrashedResult {
  totalTentes: number;
  reussis: number;
  echoues: number;
  idsReussis: number[];
  paiementsEchoues: PaiementCrashedDto[];
  message: string;
}
```

### BulkUpdateResult (Résultat de modification en masse)

```typescript
interface BulkUpdateResult {
  total: number;
  reussis: number;
  echoues: number;
  message: string;
}
```

---

## 🔌 Endpoints API

### 1. GET `/api/PaiementCrashed/ecole`

Récupère tous les paiements échoués de l'école de l'utilisateur connecté.

**Rôles requis**: `Admin`, `Super-Admin`, `Directeur`, `Financier`

**Paramètres de requête**:
- `estResolu` (optionnel, boolean): 
  - `true` = uniquement les paiements résolus
  - `false` = uniquement les paiements non résolus
  - `null` ou absent = tous les paiements

**Réponse 200**:
```json
[
  {
    "idPaiementCrashed": 1,
    "datePaiement": "2025-12-04T19:33:38",
    "montant": 50,
    "devise": "USD",
    "modePaiement": "Cash",
    "statutPaiement": "Confirmé",
    "referenceTransaction": null,
    "commentaire": null,
    "idEleve": null,
    "idFrais": null,
    "nomCompletEleve": "ELEVE_INEXISTANT_12345",
    "libelleFrais": "Minerval",
    "erreurs": [
      "L'ID de l'élève est obligatoire",
      "L'ID des frais est obligatoire"
    ],
    "numeroLigne": 3,
    "nomFichierOriginal": "test_paiements_avec_erreurs.xlsx",
    "dateEchec": "2025-12-04T19:33:38",
    "dateCorrection": null,
    "dateReinjection": null,
    "idPaiementCree": null,
    "estResolu": false,
    "nomEleve": null,
    "nomFrais": null
  }
]
```

**Exemple d'utilisation**:
```javascript
// Récupérer tous les paiements échoués
const tousLesPaiements = await paiementCrashedService.getAll();

// Récupérer uniquement les paiements non résolus
const paiementsNonResolus = await paiementCrashedService.getAll(false);

// Récupérer uniquement les paiements résolus
const paiementsResolus = await paiementCrashedService.getAll(true);
```

---

### 2. GET `/api/PaiementCrashed/{id}`

Récupère un paiement échoué par son ID.

**Rôles requis**: `Admin`, `Super-Admin`, `Directeur`, `Financier`

**Réponse 200**:
```json
{
  "idPaiementCrashed": 1,
  "datePaiement": "2025-12-04T19:33:38",
  "montant": 50,
  "devise": "USD",
  "modePaiement": "Cash",
  "statutPaiement": "Confirmé",
  "referenceTransaction": null,
  "commentaire": null,
  "idEleve": null,
  "idFrais": null,
  "nomCompletEleve": "ELEVE_INEXISTANT_12345",
  "libelleFrais": "Minerval",
  "erreurs": [
    "L'ID de l'élève est obligatoire",
    "L'ID des frais est obligatoire"
  ],
  "numeroLigne": 3,
  "nomFichierOriginal": "test_paiements_avec_erreurs.xlsx",
  "dateEchec": "2025-12-04T19:33:38",
  "dateCorrection": null,
  "dateReinjection": null,
  "idPaiementCree": null,
  "estResolu": false,
  "nomEleve": null,
  "nomFrais": null
}
```

**Réponse 404**:
```json
{
  "message": "Paiement échoué avec l'ID 999 introuvable"
}
```

**Exemple d'utilisation**:
```javascript
const paiement = await paiementCrashedService.getById(1);
console.log('Erreurs:', paiement.erreurs);
```

---

### 3. PUT `/api/PaiementCrashed/{id}`

Modifie un paiement échoué.

**Rôles requis**: `Admin`, `Super-Admin`, `Directeur`, `Financier`

**Corps de la requête**:
```json
{
  "idEleve": 488,
  "idFrais": 83,
  "montant": 100,
  "devise": "USD",
  "modePaiement": "Cash",
  "statutPaiement": "Confirmé",
  "referenceTransaction": "REF-12345",
  "commentaire": "Paiement corrigé manuellement"
}
```

**Réponse 200**: Retourne le `PaiementCrashedDto` mis à jour

**Réponse 400**: Erreurs de validation
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "dto.montant": [
      "Le montant doit être supérieur à 0"
    ]
  }
}
```

**Exemple d'utilisation**:
```javascript
const paiementCorrige = await paiementCrashedService.update(1, {
  idEleve: 488,
  idFrais: 83,
  montant: 100,
  devise: 'USD',
  modePaiement: 'Cash',
});
```

---

### 4. PUT `/api/PaiementCrashed/bulk-update`

Modifie plusieurs paiements échoués en masse.

**Rôles requis**: `Admin`, `Super-Admin`, `Directeur`, `Financier`

**Corps de la requête**:
```json
{
  "ids": [1, 2, 3],
  "idEleve": 488,
  "idFrais": 83,
  "montant": 100,
  "devise": "USD",
  "modePaiement": "Cash"
}
```

**Réponse 200**:
```json
{
  "total": 3,
  "reussis": 3,
  "echoues": 0,
  "message": "Mise à jour terminée : 3 réussi(s), 0 échoué(s) sur 3"
}
```

**Exemple d'utilisation**:
```javascript
const result = await paiementCrashedService.bulkUpdate([1, 2, 3], {
  idEleve: 488,
  idFrais: 83,
  montant: 100,
  devise: 'USD',
  modePaiement: 'Cash',
});

console.log(`${result.reussis} paiements mis à jour avec succès`);
```

---

### 5. POST `/api/PaiementCrashed/reinject`

Tente de réinjecter un ou plusieurs paiements échoués après validation.

**Rôles requis**: `Admin`, `Super-Admin`, `Directeur`, `Financier`

**Corps de la requête**:
```json
{
  "ids": [1, 2, 3],
  "forcerReinjection": false
}
```

**Réponse 200**:
```json
{
  "totalTentes": 3,
  "reussis": 3,
  "echoues": 0,
  "idsReussis": [1, 2, 3],
  "paiementsEchoues": [],
  "message": "Réinjection terminée : 3 réussi(s), 0 échoué(s) sur 3"
}
```

**Réponse 200 (avec échecs)**:
```json
{
  "totalTentes": 3,
  "reussis": 1,
  "echoues": 2,
  "idsReussis": [1],
  "paiementsEchoues": [
    {
      "idPaiementCrashed": 2,
      "erreurs": [
        "L'ID de l'élève est obligatoire"
      ],
      ...
    },
    {
      "idPaiementCrashed": 3,
      "erreurs": [
        "Le montant doit être supérieur à 0"
      ],
      ...
    }
  ],
  "message": "Réinjection terminée : 1 réussi(s), 2 échoué(s) sur 3"
}
```

**Exemple d'utilisation**:
```javascript
const result = await paiementCrashedService.reinject([1, 2, 3]);

if (result.reussis > 0) {
  console.log(`${result.reussis} paiements réinjectés avec succès`);
  console.log('IDs des paiements créés:', result.idsReussis);
}

if (result.echoues > 0) {
  console.error(`${result.echoues} paiements ont échoué lors de la réinjection`);
  result.paiementsEchoues.forEach((paiement) => {
    console.error(`ID ${paiement.idPaiementCrashed}:`, paiement.erreurs);
  });
}
```

---

### 6. DELETE `/api/PaiementCrashed/{id}`

Supprime un paiement échoué (après réinjection réussie).

**Rôles requis**: `Admin`, `Super-Admin`

**Réponse 204**: Pas de contenu (succès)

**Réponse 404**:
```json
{
  "message": "Paiement échoué avec l'ID 999 introuvable"
}
```

**Exemple d'utilisation**:
```javascript
await paiementCrashedService.delete(1);
```

---

### 7. DELETE `/api/PaiementCrashed/ecole/resolved`

Supprime tous les paiements échoués résolus de l'école.

**Rôles requis**: `Admin`, `Super-Admin`

**Réponse 200**:
```json
{
  "message": "4 paiement(s) échoué(s) résolu(s) supprimé(s)",
  "count": 4
}
```

**Exemple d'utilisation**:
```javascript
const result = await paiementCrashedService.deleteResolved();
console.log(result.message); // "4 paiement(s) échoué(s) résolu(s) supprimé(s)"
```

---

## 💡 Exemples d'utilisation

### Exemple 1 : Composant Vue.js - Liste des paiements échoués

```vue
<template>
  <div class="paiements-echoues">
    <h2>Paiements Échoués</h2>
    
    <!-- Filtres -->
    <div class="filters">
      <button 
        @click="loadPaiements(null)"
        :class="{ active: filter === null }"
      >
        Tous
      </button>
      <button 
        @click="loadPaiements(false)"
        :class="{ active: filter === false }"
      >
        Non résolus
      </button>
      <button 
        @click="loadPaiements(true)"
        :class="{ active: filter === true }"
      >
        Résolus
      </button>
    </div>

    <!-- Tableau -->
    <table v-if="paiements.length > 0">
      <thead>
        <tr>
          <th>Ligne</th>
          <th>Élève</th>
          <th>Frais</th>
          <th>Montant</th>
          <th>Erreurs</th>
          <th>Statut</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr 
          v-for="paiement in paiements" 
          :key="paiement.idPaiementCrashed"
          :class="{ 'resolu': paiement.estResolu }"
        >
          <td>{{ paiement.numeroLigne }}</td>
          <td>
            {{ paiement.nomCompletEleve || paiement.nomEleve || 'N/A' }}
          </td>
          <td>
            {{ paiement.libelleFrais || paiement.nomFrais || 'N/A' }}
          </td>
          <td>{{ paiement.montant }} {{ paiement.devise }}</td>
          <td>
            <ul class="erreurs">
              <li v-for="(erreur, index) in paiement.erreurs" :key="index">
                {{ erreur }}
              </li>
            </ul>
          </td>
          <td>
            <span :class="paiement.estResolu ? 'badge-success' : 'badge-error'">
              {{ paiement.estResolu ? 'Résolu' : 'Non résolu' }}
            </span>
          </td>
          <td>
            <button 
              @click="editPaiement(paiement)"
              v-if="!paiement.estResolu"
            >
              Modifier
            </button>
            <button 
              @click="reinjectPaiement(paiement.idPaiementCrashed)"
              v-if="!paiement.estResolu"
            >
              Réinjecter
            </button>
          </td>
        </tr>
      </tbody>
    </table>

    <p v-else>Aucun paiement échoué trouvé.</p>
  </div>
</template>

<script>
import { paiementCrashedService } from '@/services/paiementCrashed.service';

export default {
  name: 'PaiementsEchoues',
  data() {
    return {
      paiements: [],
      filter: false, // Par défaut, afficher uniquement les non résolus
      loading: false,
    };
  },
  mounted() {
    this.loadPaiements(false);
  },
  methods: {
    async loadPaiements(estResolu) {
      this.filter = estResolu;
      this.loading = true;
      try {
        this.paiements = await paiementCrashedService.getAll(estResolu);
      } catch (error) {
        console.error('Erreur lors du chargement:', error);
        this.$toast.error('Impossible de charger les paiements échoués');
      } finally {
        this.loading = false;
      }
    },
    editPaiement(paiement) {
      // Ouvrir un modal de modification
      this.$router.push({
        name: 'EditPaiementCrashed',
        params: { id: paiement.idPaiementCrashed },
      });
    },
    async reinjectPaiement(id) {
      try {
        const result = await paiementCrashedService.reinject([id]);
        if (result.reussis > 0) {
          this.$toast.success('Paiement réinjecté avec succès');
          this.loadPaiements(this.filter); // Recharger la liste
        } else {
          this.$toast.error('La réinjection a échoué');
        }
      } catch (error) {
        console.error('Erreur lors de la réinjection:', error);
        this.$toast.error('Erreur lors de la réinjection');
      }
    },
  },
};
</script>
```

---

### Exemple 2 : Composant Vue.js - Modification en masse

```vue
<template>
  <div class="bulk-update">
    <h3>Modification en masse</h3>
    
    <div v-if="selectedIds.length === 0" class="alert">
      Veuillez sélectionner au moins un paiement échoué.
    </div>

    <form v-else @submit.prevent="handleBulkUpdate">
      <div class="form-group">
        <label>ID Élève</label>
        <input 
          v-model="formData.idEleve" 
          type="number" 
          placeholder="ID de l'élève"
        />
      </div>

      <div class="form-group">
        <label>ID Frais</label>
        <input 
          v-model="formData.idFrais" 
          type="number" 
          placeholder="ID des frais"
        />
      </div>

      <div class="form-group">
        <label>Montant</label>
        <input 
          v-model="formData.montant" 
          type="number" 
          step="0.01"
          placeholder="Montant"
        />
      </div>

      <div class="form-group">
        <label>Devise</label>
        <select v-model="formData.devise">
          <option value="USD">USD</option>
          <option value="CDF">CDF</option>
          <option value="EUR">EUR</option>
        </select>
      </div>

      <div class="form-group">
        <label>Mode de paiement</label>
        <select v-model="formData.modePaiement">
          <option value="Cash">Cash</option>
          <option value="Carte">Carte</option>
          <option value="Mobile Money">Mobile Money</option>
        </select>
      </div>

      <button type="submit" :disabled="loading">
        {{ loading ? 'Mise à jour...' : `Mettre à jour ${selectedIds.length} paiement(s)` }}
      </button>
    </form>
  </div>
</template>

<script>
import { paiementCrashedService } from '@/services/paiementCrashed.service';

export default {
  name: 'BulkUpdatePaiements',
  props: {
    selectedIds: {
      type: Array,
      required: true,
    },
  },
  data() {
    return {
      formData: {
        idEleve: null,
        idFrais: null,
        montant: null,
        devise: 'USD',
        modePaiement: 'Cash',
      },
      loading: false,
    };
  },
  methods: {
    async handleBulkUpdate() {
      this.loading = true;
      try {
        const result = await paiementCrashedService.bulkUpdate(
          this.selectedIds,
          this.formData
        );

        if (result.reussis > 0) {
          this.$toast.success(
            `${result.reussis} paiement(s) mis à jour avec succès`
          );
          this.$emit('updated');
        }

        if (result.echoues > 0) {
          this.$toast.warning(
            `${result.echoues} paiement(s) n'ont pas pu être mis à jour`
          );
        }
      } catch (error) {
        console.error('Erreur lors de la mise à jour:', error);
        this.$toast.error('Erreur lors de la mise à jour en masse');
      } finally {
        this.loading = false;
      }
    },
  },
};
</script>
```

---

### Exemple 3 : Composant Vue.js - Réinjection multiple

```vue
<template>
  <div class="reinject-paiements">
    <h3>Réinjection de paiements</h3>
    
    <div v-if="selectedIds.length === 0" class="alert">
      Veuillez sélectionner au moins un paiement échoué à réinjecter.
    </div>

    <div v-else>
      <p>
        Vous êtes sur le point de réinjecter 
        <strong>{{ selectedIds.length }}</strong> paiement(s).
      </p>
      
      <div class="checkbox-group">
        <label>
          <input 
            type="checkbox" 
            v-model="forcerReinjection"
          />
          Forcer la réinjection (ignorer les erreurs de validation)
        </label>
      </div>

      <button 
        @click="handleReinject" 
        :disabled="loading"
        class="btn-primary"
      >
        {{ loading ? 'Réinjection...' : 'Réinjecter' }}
      </button>
    </div>

    <!-- Résultats -->
    <div v-if="result" class="results">
      <div class="success" v-if="result.reussis > 0">
        ✅ {{ result.reussis }} paiement(s) réinjecté(s) avec succès
      </div>
      <div class="error" v-if="result.echoues > 0">
        ❌ {{ result.echoues }} paiement(s) ont échoué
        <ul>
          <li 
            v-for="paiement in result.paiementsEchoues" 
            :key="paiement.idPaiementCrashed"
          >
            ID {{ paiement.idPaiementCrashed }}: 
            {{ paiement.erreurs.join(', ') }}
          </li>
        </ul>
      </div>
    </div>
  </div>
</template>

<script>
import { paiementCrashedService } from '@/services/paiementCrashed.service';

export default {
  name: 'ReinjectPaiements',
  props: {
    selectedIds: {
      type: Array,
      required: true,
    },
  },
  data() {
    return {
      forcerReinjection: false,
      loading: false,
      result: null,
    };
  },
  methods: {
    async handleReinject() {
      if (!confirm(`Êtes-vous sûr de vouloir réinjecter ${this.selectedIds.length} paiement(s) ?`)) {
        return;
      }

      this.loading = true;
      this.result = null;

      try {
        this.result = await paiementCrashedService.reinject(
          this.selectedIds,
          this.forcerReinjection
        );

        if (this.result.reussis > 0) {
          this.$toast.success(
            `${this.result.reussis} paiement(s) réinjecté(s) avec succès`
          );
          this.$emit('reinjected', this.result);
        }

        if (this.result.echoues > 0) {
          this.$toast.warning(
            `${this.result.echoues} paiement(s) ont échoué lors de la réinjection`
          );
        }
      } catch (error) {
        console.error('Erreur lors de la réinjection:', error);
        this.$toast.error('Erreur lors de la réinjection');
      } finally {
        this.loading = false;
      }
    },
  },
};
</script>
```

---

## 🎯 Cas d'usage typiques

### Cas 1 : Import Excel avec erreurs

**Scénario**: Un utilisateur importe un fichier Excel avec des paiements. Certains échouent.

**Flux**:
1. L'utilisateur importe le fichier via `/api/Paiement/bulk-insert-excel`
2. La réponse contient `BulkPaiementResult` avec `lignesAvecErreurs`
3. Le frontend affiche les erreurs et propose de consulter les paiements échoués
4. L'utilisateur clique sur "Voir les paiements échoués"
5. Le frontend charge la liste via `GET /api/PaiementCrashed/ecole?estResolu=false`

**Code**:
```javascript
// Après l'import Excel
const result = await paiementService.bulkInsertExcel(formData);

if (result.lignesAvecErreurs.length > 0) {
  // Afficher un message avec un lien vers les paiements échoués
  this.$toast.warning(
    `${result.lignesAvecErreurs.length} paiement(s) ont échoué. ` +
    `Cliquez ici pour les corriger.`,
    {
      onClick: () => {
        this.$router.push('/paiements-echoues');
      },
    }
  );
}
```

---

### Cas 2 : Correction et réinjection

**Scénario**: Un utilisateur corrige plusieurs paiements échoués et les réinjecte.

**Flux**:
1. L'utilisateur sélectionne plusieurs paiements échoués
2. Il clique sur "Modifier en masse"
3. Il remplit le formulaire (ID Élève, ID Frais, Montant, etc.)
4. Il clique sur "Mettre à jour"
5. Il sélectionne les paiements corrigés
6. Il clique sur "Réinjecter"
7. Le système valide et crée les paiements
8. Les paiements échoués sont marqués comme résolus

**Code**:
```javascript
// 1. Correction en masse
const updateResult = await paiementCrashedService.bulkUpdate(
  [1, 2, 3],
  {
    idEleve: 488,
    idFrais: 83,
    montant: 100,
    devise: 'USD',
    modePaiement: 'Cash',
  }
);

// 2. Réinjection
const reinjectResult = await paiementCrashedService.reinject([1, 2, 3]);

if (reinjectResult.reussis === 3) {
  // Tous les paiements ont été réinjectés avec succès
  console.log('Paiements créés:', reinjectResult.idsReussis);
}
```

---

### Cas 3 : Nettoyage des paiements résolus

**Scénario**: Un administrateur nettoie les anciens paiements échoués résolus.

**Flux**:
1. L'administrateur consulte les paiements résolus
2. Il clique sur "Supprimer tous les paiements résolus"
3. Le système supprime tous les paiements échoués résolus de l'école

**Code**:
```javascript
if (confirm('Êtes-vous sûr de vouloir supprimer tous les paiements résolus ?')) {
  const result = await paiementCrashedService.deleteResolved();
  this.$toast.success(result.message);
  this.loadPaiements(false); // Recharger la liste
}
```

---

## ⚠️ Gestion des erreurs

### Erreurs HTTP courantes

#### 401 Unauthorized
```javascript
// Token JWT invalide ou expiré
if (error.response?.status === 401) {
  localStorage.removeItem('accessToken');
  this.$router.push('/login');
}
```

#### 400 Bad Request
```javascript
// Erreurs de validation
if (error.response?.status === 400) {
  const errors = error.response.data.errors;
  Object.keys(errors).forEach((field) => {
    errors[field].forEach((message) => {
      this.$toast.error(`${field}: ${message}`);
    });
  });
}
```

#### 404 Not Found
```javascript
// Paiement échoué introuvable
if (error.response?.status === 404) {
  this.$toast.error(error.response.data.message);
}
```

#### 500 Internal Server Error
```javascript
// Erreur serveur
if (error.response?.status === 500) {
  this.$toast.error('Une erreur serveur est survenue. Veuillez réessayer plus tard.');
  console.error('Erreur serveur:', error.response.data);
}
```

### Gestion globale des erreurs

```javascript
// Dans votre service API
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response) {
      // Erreur avec réponse du serveur
      const { status, data } = error.response;
      
      switch (status) {
        case 401:
          // Rediriger vers la page de connexion
          localStorage.removeItem('accessToken');
          window.location.href = '/login';
          break;
        case 400:
          // Afficher les erreurs de validation
          if (data.errors) {
            Object.values(data.errors).flat().forEach((message) => {
              toast.error(message);
            });
          } else {
            toast.error(data.message || 'Erreur de validation');
          }
          break;
        case 404:
          toast.error(data.message || 'Ressource introuvable');
          break;
        case 500:
          toast.error('Erreur serveur. Veuillez réessayer plus tard.');
          console.error('Erreur serveur:', data);
          break;
        default:
          toast.error('Une erreur est survenue');
      }
    } else if (error.request) {
      // Pas de réponse du serveur (réseau)
      toast.error('Impossible de contacter le serveur. Vérifiez votre connexion.');
    } else {
      // Erreur lors de la configuration de la requête
      toast.error('Une erreur est survenue lors de la préparation de la requête.');
    }
    
    return Promise.reject(error);
  }
);
```

---

## ✅ Bonnes pratiques

### 1. Gestion de l'état

Utilisez un store (Pinia, Vuex, etc.) pour gérer l'état des paiements échoués :

```javascript
// stores/paiementCrashed.js (Pinia)
import { defineStore } from 'pinia';
import { paiementCrashedService } from '@/services/paiementCrashed.service';

export const usePaiementCrashedStore = defineStore('paiementCrashed', {
  state: () => ({
    paiements: [],
    loading: false,
    error: null,
  }),
  actions: {
    async fetchPaiements(estResolu = null) {
      this.loading = true;
      this.error = null;
      try {
        this.paiements = await paiementCrashedService.getAll(estResolu);
      } catch (error) {
        this.error = error;
        throw error;
      } finally {
        this.loading = false;
      }
    },
    async updatePaiement(id, data) {
      const updated = await paiementCrashedService.update(id, data);
      const index = this.paiements.findIndex((p) => p.idPaiementCrashed === id);
      if (index !== -1) {
        this.paiements[index] = updated;
      }
      return updated;
    },
  },
});
```

### 2. Validation côté client

Validez les données avant d'envoyer la requête :

```javascript
function validateUpdatePaiement(data) {
  const errors = [];
  
  if (data.idEleve !== undefined && data.idEleve <= 0) {
    errors.push("L'ID de l'élève doit être supérieur à 0");
  }
  
  if (data.idFrais !== undefined && data.idFrais <= 0) {
    errors.push("L'ID des frais doit être supérieur à 0");
  }
  
  if (data.montant !== undefined && data.montant <= 0) {
    errors.push('Le montant doit être supérieur à 0');
  }
  
  return errors;
}
```

### 3. Feedback utilisateur

Donnez un feedback clair à l'utilisateur :

```javascript
// Avant la réinjection
this.loading = true;
this.$toast.info('Réinjection en cours...');

try {
  const result = await paiementCrashedService.reinject(ids);
  
  if (result.reussis > 0) {
    this.$toast.success(
      `✅ ${result.reussis} paiement(s) réinjecté(s) avec succès`
    );
  }
  
  if (result.echoues > 0) {
    this.$toast.warning(
      `⚠️ ${result.echoues} paiement(s) ont échoué`
    );
  }
} catch (error) {
  this.$toast.error('❌ Erreur lors de la réinjection');
} finally {
  this.loading = false;
}
```

### 4. Pagination (si nécessaire)

Si vous avez beaucoup de paiements échoués, implémentez la pagination :

```javascript
async function getPaiementsPaged(page = 1, pageSize = 20, estResolu = null) {
  const params = {
    page,
    pageSize,
    estResolu,
  };
  const response = await api.get('/api/PaiementCrashed/ecole', { params });
  return response.data;
}
```

### 5. Cache et rafraîchissement

Mettez en cache les paiements échoués et rafraîchissez après les modifications :

```javascript
// Après une modification ou réinjection réussie
async function refreshPaiements() {
  await this.$store.dispatch('paiementCrashed/fetchPaiements', false);
}
```

---

## 📞 Support

Pour toute question ou problème, contactez l'équipe de développement.

---

## 📝 Changelog

### Version 1.0.0 (2025-12-04)
- ✅ Sauvegarde automatique des paiements échoués
- ✅ Consultation des paiements échoués
- ✅ Modification individuelle et en masse
- ✅ Réinjection avec validation
- ✅ Suppression des paiements résolus

---

**Documentation générée le**: 2025-12-04  
**Version de l'API**: 1.0.0





