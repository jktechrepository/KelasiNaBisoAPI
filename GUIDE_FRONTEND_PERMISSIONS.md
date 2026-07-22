# 🎨 Guide Frontend : Utilisation des Permissions

## 📌 Vue d'ensemble

Depuis l'implémentation du système RBAC, **les permissions sont maintenant retournées automatiquement lors de l'authentification**. Le frontend peut les utiliser pour afficher/masquer des éléments UI sans faire d'appels API supplémentaires.

---

## 🔐 Réponse d'Authentification

### Exemple de Requête

```http
POST /api/Utilisateur/authentifier
Content-Type: application/json

{
  "emailOuTelephone": "+243999999999",
  "motDePasse": "Super-Admin"
}
```

### Exemple de Réponse (avec permissions)

```json
{
  "success": true,
  "message": "Authentification réussie",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresIn": 86400,
  "expiresAt": "2025-10-29T05:32:14Z",
  "doitChangerMotDePasse": false,
  "utilisateur": {
    "idUtilisateur": 1,
    "referenceUtilisateur": "550003f06f0f4fac0d56697ac505033e",
    "nomUtilisateur": "ADMIN",
    "postnomUtilisateur": null,
    "prenomUtilisateur": "Super",
    "email": "superadmin@ekelasi.com",
    "telephone": "+243999999999",
    "photoUrl": null,
    "genre": "Masculin",
    "statut": true,
    "idEcole": 1,
    "ecole": {
      "idEcole": 1,
      "nom": "Ekelasi School"
    },
    "idRole": 1
  },
  "nomRole": "Super-Admin",
  "nomEcole": "Ekelasi School",
  "permissions": [
    "Ecole.Create",
    "Ecole.Read",
    "Ecole.ReadAll",
    "Ecole.Update",
    "Ecole.Delete",
    "Utilisateur.Create",
    "Utilisateur.Read",
    "Utilisateur.ReadAll",
    "Utilisateur.Update",
    "Utilisateur.Delete",
    "Eleve.Create",
    "Eleve.Read",
    "Eleve.ReadAll",
    "Paiement.Create",
    "Paiement.Read",
    "Paiement.Validate",
    "Note.Create",
    "Note.Update",
    "Permission.ReadAll",
    "..."
  ]
}
```

---

## 💾 Stockage Côté Client

### localStorage (recommandé pour session persistante)

```javascript
// Lors de la connexion
const response = await fetch('/api/Utilisateur/authentifier', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ emailOuTelephone, motDePasse })
});

const data = await response.json();

if (data.success) {
  // Stocker le token
  localStorage.setItem('accessToken', data.accessToken);
  
  // Stocker les infos utilisateur
  localStorage.setItem('user', JSON.stringify(data.utilisateur));
  
  // ✨ Stocker les permissions
  localStorage.setItem('permissions', JSON.stringify(data.permissions));
  
  // Stocker le rôle et l'école
  localStorage.setItem('userRole', data.nomRole);
  localStorage.setItem('userEcole', data.nomEcole);
}
```

### sessionStorage (recommandé pour session temporaire)

```javascript
// Même chose mais avec sessionStorage
sessionStorage.setItem('accessToken', data.accessToken);
sessionStorage.setItem('permissions', JSON.stringify(data.permissions));
```

---

## 🎯 Utilisation dans le Frontend

### 1. Service de Permissions (Vanilla JS)

```javascript
// services/permissionService.js

class PermissionService {
  /**
   * Récupère les permissions de l'utilisateur connecté
   */
  static getPermissions() {
    const permissions = localStorage.getItem('permissions');
    return permissions ? JSON.parse(permissions) : [];
  }

  /**
   * Vérifie si l'utilisateur a une permission spécifique
   */
  static hasPermission(permissionName) {
    const permissions = this.getPermissions();
    return permissions.includes(permissionName);
  }

  /**
   * Vérifie si l'utilisateur a AU MOINS UNE des permissions
   */
  static hasAnyPermission(...permissionNames) {
    const permissions = this.getPermissions();
    return permissionNames.some(p => permissions.includes(p));
  }

  /**
   * Vérifie si l'utilisateur a TOUTES les permissions
   */
  static hasAllPermissions(...permissionNames) {
    const permissions = this.getPermissions();
    return permissionNames.every(p => permissions.includes(p));
  }

  /**
   * Vérifie si l'utilisateur a accès à une catégorie (ex: "Ecole.*")
   */
  static hasCategoryAccess(category) {
    const permissions = this.getPermissions();
    return permissions.some(p => p.startsWith(category + '.'));
  }
}

export default PermissionService;
```

### 2. Vue.js 3 (Composition API)

```javascript
// composables/usePermissions.js

import { computed } from 'vue';

export function usePermissions() {
  const permissions = computed(() => {
    const stored = localStorage.getItem('permissions');
    return stored ? JSON.parse(stored) : [];
  });

  const hasPermission = (permissionName) => {
    return permissions.value.includes(permissionName);
  };

  const hasAnyPermission = (...permissionNames) => {
    return permissionNames.some(p => permissions.value.includes(p));
  };

  const hasAllPermissions = (...permissionNames) => {
    return permissionNames.every(p => permissions.value.includes(p));
  };

  return {
    permissions,
    hasPermission,
    hasAnyPermission,
    hasAllPermissions
  };
}
```

**Utilisation dans un composant** :

```vue
<template>
  <div>
    <!-- Afficher le bouton "Créer" seulement si autorisé -->
    <button v-if="hasPermission('Ecole.Create')" @click="createEcole">
      ➕ Créer une école
    </button>

    <!-- Afficher le bouton "Valider" seulement pour les comptables -->
    <button v-if="hasPermission('Paiement.Validate')" @click="validatePaiement">
      ✅ Valider le paiement
    </button>

    <!-- Section visible si l'utilisateur a accès aux paiements -->
    <div v-if="hasAnyPermission('Paiement.Read', 'Paiement.ReadAll')">
      <h2>Gestion des Paiements</h2>
      <!-- ... -->
    </div>
  </div>
</template>

<script setup>
import { usePermissions } from '@/composables/usePermissions';

const { hasPermission, hasAnyPermission } = usePermissions();

const createEcole = () => {
  // Logique de création
};

const validatePaiement = () => {
  // Logique de validation
};
</script>
```

### 3. Vue.js 3 (Directive Personnalisée)

```javascript
// plugins/permissionDirective.js

export default {
  install(app) {
    app.directive('permission', {
      mounted(el, binding) {
        const requiredPermission = binding.value;
        const permissions = JSON.parse(localStorage.getItem('permissions') || '[]');
        
        if (!permissions.includes(requiredPermission)) {
          // Cacher l'élément ou le désactiver
          el.style.display = 'none';
          // OU
          // el.disabled = true;
          // el.classList.add('disabled');
        }
      }
    });
  }
};
```

**Utilisation** :

```vue
<template>
  <!-- Cacher automatiquement si pas la permission -->
  <button v-permission="'Ecole.Create'">Créer une école</button>
  
  <button v-permission="'Paiement.Validate'">Valider</button>
  
  <div v-permission="'Note.ReadAll'">
    <h2>Toutes les notes</h2>
  </div>
</template>
```

### 4. React (Hooks)

```javascript
// hooks/usePermissions.js

import { useMemo } from 'react';

export function usePermissions() {
  const permissions = useMemo(() => {
    const stored = localStorage.getItem('permissions');
    return stored ? JSON.parse(stored) : [];
  }, []);

  const hasPermission = (permissionName) => {
    return permissions.includes(permissionName);
  };

  const hasAnyPermission = (...permissionNames) => {
    return permissionNames.some(p => permissions.includes(p));
  };

  return { permissions, hasPermission, hasAnyPermission };
}
```

**Utilisation** :

```jsx
import { usePermissions } from './hooks/usePermissions';

function EcoleManager() {
  const { hasPermission } = usePermissions();

  return (
    <div>
      {hasPermission('Ecole.Create') && (
        <button onClick={createEcole}>➕ Créer une école</button>
      )}
      
      {hasPermission('Paiement.Validate') && (
        <button onClick={validatePaiement}>✅ Valider</button>
      )}
    </div>
  );
}
```

### 5. Angular (Service)

```typescript
// services/permission.service.ts

import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class PermissionService {
  private getPermissions(): string[] {
    const permissions = localStorage.getItem('permissions');
    return permissions ? JSON.parse(permissions) : [];
  }

  hasPermission(permissionName: string): boolean {
    return this.getPermissions().includes(permissionName);
  }

  hasAnyPermission(...permissionNames: string[]): boolean {
    const permissions = this.getPermissions();
    return permissionNames.some(p => permissions.includes(p));
  }

  hasAllPermissions(...permissionNames: string[]): boolean {
    const permissions = this.getPermissions();
    return permissionNames.every(p => permissions.includes(p));
  }
}
```

**Utilisation dans un composant** :

```typescript
import { Component } from '@angular/core';
import { PermissionService } from './services/permission.service';

@Component({
  selector: 'app-ecole-manager',
  template: `
    <button *ngIf="canCreate" (click)="createEcole()">
      ➕ Créer une école
    </button>
    
    <button *ngIf="canValidate" (click)="validatePaiement()">
      ✅ Valider le paiement
    </button>
  `
})
export class EcoleManagerComponent {
  canCreate: boolean;
  canValidate: boolean;

  constructor(private permissionService: PermissionService) {
    this.canCreate = this.permissionService.hasPermission('Ecole.Create');
    this.canValidate = this.permissionService.hasPermission('Paiement.Validate');
  }

  createEcole() {
    // Logique
  }

  validatePaiement() {
    // Logique
  }
}
```

---

## 🎨 Exemples d'Utilisation UI

### Afficher/Masquer des Menus

```javascript
// Navigation conditionnelle
const menuItems = [
  {
    label: 'Écoles',
    icon: '🏫',
    permission: 'Ecole.ReadAll',
    route: '/ecoles'
  },
  {
    label: 'Élèves',
    icon: '👨‍🎓',
    permission: 'Eleve.ReadAll',
    route: '/eleves'
  },
  {
    label: 'Paiements',
    icon: '💰',
    permission: 'Paiement.ReadAll',
    route: '/paiements'
  },
  {
    label: 'Permissions',
    icon: '🔐',
    permission: 'Permission.ReadAll',
    route: '/permissions'
  }
];

// Filtrer selon les permissions
const visibleMenuItems = menuItems.filter(item => 
  PermissionService.hasPermission(item.permission)
);
```

### Activer/Désactiver des Boutons

```html
<button 
  :disabled="!hasPermission('Ecole.Delete')"
  @click="deleteEcole">
  🗑️ Supprimer
</button>
```

### Affichage Conditionnel de Colonnes

```javascript
// Tableau avec colonnes conditionnelles
const columns = [
  { field: 'nom', label: 'Nom', visible: true },
  { field: 'email', label: 'Email', visible: true },
  { 
    field: 'actions', 
    label: 'Actions', 
    visible: PermissionService.hasAnyPermission('Ecole.Update', 'Ecole.Delete')
  }
];
```

---

## ⚠️ Sécurité Important

### ❌ Ne JAMAIS faire confiance uniquement au frontend

```javascript
// ❌ MAUVAIS : Le frontend peut être contourné
if (hasPermission('Paiement.Validate')) {
  // L'utilisateur pourrait modifier le localStorage
  // et s'attribuer la permission
}
```

### ✅ Toujours valider côté Backend

```javascript
// ✅ BON : Le frontend masque les boutons
if (hasPermission('Paiement.Validate')) {
  // Afficher le bouton
}

// Mais l'API vérifie TOUJOURS avec [Permission]
// POST /api/Paiement/valider protégé par [Permission("Paiement.Validate")]
```

**Le frontend utilise les permissions pour l'UX, mais le backend les vérifie pour la sécurité !**

---

## 🔄 Rafraîchissement des Permissions

Si les permissions changent (par exemple, un admin modifie les permissions d'un rôle), l'utilisateur doit se reconnecter pour obtenir les nouvelles permissions.

### Option 1 : Forcer la reconnexion

```javascript
// Après modification des permissions
alert('Vos permissions ont été mises à jour. Veuillez vous reconnecter.');
logout();
```

### Option 2 : Endpoint de rafraîchissement (bonus)

```javascript
// Récupérer les permissions à jour sans se reconnecter
async function refreshPermissions() {
  const response = await fetch('/api/Permission/my-permissions', {
    headers: {
      'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
    }
  });
  
  const permissions = await response.json();
  localStorage.setItem('permissions', JSON.stringify(permissions));
}
```

---

## 📊 Exemple Complet : Page de Gestion des Écoles

```vue
<template>
  <div class="ecoles-page">
    <h1>Gestion des Écoles</h1>
    
    <!-- Bouton Créer (visible uniquement si autorisé) -->
    <button 
      v-if="hasPermission('Ecole.Create')"
      @click="showCreateModal = true"
      class="btn btn-primary">
      ➕ Créer une école
    </button>
    
    <!-- Liste des écoles -->
    <table>
      <thead>
        <tr>
          <th>Nom</th>
          <th>Type</th>
          <th>Email</th>
          <th v-if="canModify">Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="ecole in ecoles" :key="ecole.idEcole">
          <td>{{ ecole.nom }}</td>
          <td>{{ ecole.type }}</td>
          <td>{{ ecole.emailContact }}</td>
          <td v-if="canModify">
            <!-- Bouton Modifier -->
            <button 
              v-if="hasPermission('Ecole.Update')"
              @click="editEcole(ecole)">
              ✏️ Modifier
            </button>
            
            <!-- Bouton Supprimer (Super-Admin uniquement) -->
            <button 
              v-if="hasPermission('Ecole.Delete')"
              @click="deleteEcole(ecole)"
              class="btn-danger">
              🗑️ Supprimer
            </button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue';
import { usePermissions } from '@/composables/usePermissions';

const { hasPermission, hasAnyPermission } = usePermissions();

const ecoles = ref([]);
const showCreateModal = ref(false);

// Permissions calculées
const canModify = computed(() => 
  hasAnyPermission('Ecole.Update', 'Ecole.Delete')
);

// Méthodes
const editEcole = (ecole) => {
  // Logique de modification
};

const deleteEcole = async (ecole) => {
  if (confirm('Êtes-vous sûr ?')) {
    // Appel API (toujours vérifié côté backend)
    await fetch(`/api/Ecole/${ecole.idEcole}`, {
      method: 'DELETE',
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
      }
    });
  }
};
</script>
```

---

## 🎯 Résumé

| Action | Code | Description |
|--------|------|-------------|
| **Stocker** | `localStorage.setItem('permissions', JSON.stringify(data.permissions))` | Sauvegarder à la connexion |
| **Récupérer** | `JSON.parse(localStorage.getItem('permissions'))` | Lire les permissions |
| **Vérifier** | `permissions.includes('Ecole.Create')` | Tester une permission |
| **Afficher** | `v-if="hasPermission('...')"` | Masquer/afficher UI |
| **Désactiver** | `:disabled="!hasPermission('...')"` | Désactiver boutons |

---

## ✅ Bonnes Pratiques

1. ✅ **Toujours stocker** les permissions après l'authentification
2. ✅ **Utiliser un service** centralisé pour vérifier les permissions
3. ✅ **Masquer/désactiver** les éléments UI selon les permissions
4. ✅ **Ne JAMAIS faire confiance** uniquement au frontend
5. ✅ **Toujours vérifier** côté backend avec `[Permission]`
6. ✅ **Forcer la reconnexion** après modification des permissions

---

**Votre frontend peut maintenant offrir une expérience utilisateur optimale avec affichage dynamique selon les permissions ! 🎨✨**

