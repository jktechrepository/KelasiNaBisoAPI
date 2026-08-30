# 🚀 Guide de Démarrage Rapide - Développeur Frontend

> **Temps estimé**: 15 minutes  
> **Prérequis**: Connaissances de base en JavaScript/TypeScript et appels API

---

## 📋 Étapes de Configuration

### 1️⃣ Importer la Collection Postman

1. Ouvrez **Postman**
2. Cliquez sur **Import** (en haut à gauche)
3. Sélectionnez les fichiers :
   - `KelasiNaBiso_API.postman_collection.json`
   - `KelasiNaBiso_Dev.postman_environment.json`
4. Sélectionnez l'environnement **KelasiNaBiso - Development**

✅ Vous êtes prêt à tester l'API !

---

### 2️⃣ Tester l'Authentification

#### Dans Postman

1. Ouvrez la requête **Authentification → Se Connecter**
2. Vérifiez le Body :
   ```json
   {
     "emailOuTelephone": "+243999999999",
     "motDePasse": "Super-Admin"
   }
   ```
3. Cliquez sur **Send**
4. ✨ Le token est **automatiquement sauvegardé** dans l'environnement !

#### Dans votre Code JavaScript/TypeScript

```javascript
// auth.service.js
const API_URL = 'https://localhost:7105/api';

async function login(emailOuTelephone, motDePasse) {
  try {
    const response = await fetch(`${API_URL}/Utilisateur/authentifier`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ emailOuTelephone, motDePasse })
    });

    if (!response.ok) {
      throw new Error('Authentification échouée');
    }

    const data = await response.json();
    
    // Sauvegarder les infos importantes
    localStorage.setItem('accessToken', data.accessToken);
    localStorage.setItem('permissions', JSON.stringify(data.permissions));
    localStorage.setItem('user', JSON.stringify(data.utilisateur));
    
    return data;
  } catch (error) {
    console.error('Erreur de connexion:', error);
    throw error;
  }
}

// Utilisation
login('+243999999999', 'Super-Admin')
  .then(data => {
    console.log('✅ Connecté:', data.utilisateur.nomComplet);
    console.log('🔑 Permissions:', data.permissions);
  })
  .catch(err => console.error('❌ Erreur:', err));
```

---

### 3️⃣ Faire un Appel API Authentifié

```javascript
// api.service.js
async function apiCall(endpoint, options = {}) {
  const token = localStorage.getItem('accessToken');
  
  if (!token) {
    throw new Error('Non authentifié. Veuillez vous connecter.');
  }
  
  const response = await fetch(`${API_URL}${endpoint}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`,
      ...options.headers
    }
  });
  
  // Gérer l'expiration du token
  if (response.status === 401) {
    localStorage.clear();
    window.location.href = '/login';
    throw new Error('Session expirée');
  }
  
  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message || 'Erreur API');
  }
  
  return response.json();
}

// Exemple: Récupérer les élèves
async function getEleves(pageNumber = 1, pageSize = 20) {
  return apiCall(`/Eleve/paginated?pageNumber=${pageNumber}&pageSize=${pageSize}`);
}

// Utilisation
getEleves(1, 20)
  .then(data => {
    console.log('Total élèves:', data.totalCount);
    console.log('Élèves:', data.items);
  });
```

---

### 4️⃣ Gérer les Permissions dans l'UI

```javascript
// permission.service.js
class PermissionService {
  /**
   * Vérifie si l'utilisateur a une permission
   */
  static hasPermission(permissionName) {
    const permissions = JSON.parse(localStorage.getItem('permissions') || '[]');
    return permissions.includes(permissionName);
  }

  /**
   * Vérifie si l'utilisateur a AU MOINS UNE des permissions
   */
  static hasAnyPermission(...permissionNames) {
    const permissions = JSON.parse(localStorage.getItem('permissions') || '[]');
    return permissionNames.some(p => permissions.includes(p));
  }

  /**
   * Vérifie si l'utilisateur a TOUTES les permissions
   */
  static hasAllPermissions(...permissionNames) {
    const permissions = JSON.parse(localStorage.getItem('permissions') || '[]');
    return permissionNames.every(p => permissions.includes(p));
  }
}

// Utilisation dans l'UI
if (PermissionService.hasPermission('Ecole.Create')) {
  // Afficher le bouton "Créer une école"
  document.getElementById('btnCreateEcole').style.display = 'block';
}

if (PermissionService.hasPermission('Paiement.Validate')) {
  // Activer le bouton de validation
  document.getElementById('btnValidate').disabled = false;
}

// Exemple avec React
function EcoleList() {
  const canCreate = PermissionService.hasPermission('Ecole.Create');
  const canUpdate = PermissionService.hasPermission('Ecole.Update');
  
  return (
    <div>
      {canCreate && (
        <button onClick={handleCreate}>
          ➕ Créer une école
        </button>
      )}
      
      {canUpdate && (
        <button onClick={handleEdit}>
          ✏️ Modifier
        </button>
      )}
    </div>
  );
}
```

---

### 5️⃣ Exemples Complets par Framework

#### 🟢 Vue.js 3 (Composition API)

```vue
<!-- stores/auth.js -->
<script setup>
import { ref, computed } from 'vue';
import { defineStore } from 'pinia';

export const useAuthStore = defineStore('auth', () => {
  const user = ref(null);
  const token = ref(localStorage.getItem('accessToken'));
  const permissions = ref(JSON.parse(localStorage.getItem('permissions') || '[]'));

  const isAuthenticated = computed(() => !!token.value);

  async function login(emailOuTelephone, motDePasse) {
    const response = await fetch('/api/Utilisateur/authentifier', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ emailOuTelephone, motDePasse })
    });

    const data = await response.json();
    
    token.value = data.accessToken;
    user.value = data.utilisateur;
    permissions.value = data.permissions;
    
    localStorage.setItem('accessToken', data.accessToken);
    localStorage.setItem('permissions', JSON.stringify(data.permissions));
    localStorage.setItem('user', JSON.stringify(data.utilisateur));
  }

  function hasPermission(permissionName) {
    return permissions.value.includes(permissionName);
  }

  return { user, token, permissions, isAuthenticated, login, hasPermission };
});
</script>

<!-- Utilisation dans un composant -->
<template>
  <div>
    <button v-if="authStore.hasPermission('Ecole.Create')" @click="createEcole">
      ➕ Créer une école
    </button>
  </div>
</template>

<script setup>
import { useAuthStore } from '@/stores/auth';
const authStore = useAuthStore();
</script>
```

#### 🔵 React (avec Context API)

```jsx
// AuthContext.jsx
import React, { createContext, useState, useContext, useEffect } from 'react';

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [token, setToken] = useState(localStorage.getItem('accessToken'));
  const [permissions, setPermissions] = useState(
    JSON.parse(localStorage.getItem('permissions') || '[]')
  );

  const login = async (emailOuTelephone, motDePasse) => {
    const response = await fetch('/api/Utilisateur/authentifier', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ emailOuTelephone, motDePasse })
    });

    const data = await response.json();
    
    setToken(data.accessToken);
    setUser(data.utilisateur);
    setPermissions(data.permissions);
    
    localStorage.setItem('accessToken', data.accessToken);
    localStorage.setItem('permissions', JSON.stringify(data.permissions));
  };

  const hasPermission = (permissionName) => {
    return permissions.includes(permissionName);
  };

  return (
    <AuthContext.Provider value={{ user, token, permissions, login, hasPermission }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => useContext(AuthContext);

// Utilisation
function EcoleList() {
  const { hasPermission } = useAuth();
  
  return (
    <div>
      {hasPermission('Ecole.Create') && (
        <button onClick={createEcole}>➕ Créer une école</button>
      )}
    </div>
  );
}
```

#### 🅰️ Angular (Service)

```typescript
// auth.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

interface AuthResponse {
  accessToken: string;
  utilisateur: any;
  permissions: string[];
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private tokenSubject = new BehaviorSubject<string | null>(
    localStorage.getItem('accessToken')
  );
  private permissionsSubject = new BehaviorSubject<string[]>(
    JSON.parse(localStorage.getItem('permissions') || '[]')
  );

  constructor(private http: HttpClient) {}

  login(emailOuTelephone: string, motDePasse: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/Utilisateur/authentifier', {
      emailOuTelephone,
      motDePasse
    }).pipe(
      tap(response => {
        this.tokenSubject.next(response.accessToken);
        this.permissionsSubject.next(response.permissions);
        
        localStorage.setItem('accessToken', response.accessToken);
        localStorage.setItem('permissions', JSON.stringify(response.permissions));
      })
    );
  }

  hasPermission(permissionName: string): boolean {
    const permissions = this.permissionsSubject.value;
    return permissions.includes(permissionName);
  }

  getToken(): string | null {
    return this.tokenSubject.value;
  }
}

// Utilisation dans un composant
export class EcoleListComponent {
  constructor(public authService: AuthService) {}

  canCreate = this.authService.hasPermission('Ecole.Create');
}
```

---

## 📊 Endpoints Essentiels

| Endpoint | Méthode | Permission | Description |
|----------|---------|------------|-------------|
| `/Utilisateur/authentifier` | POST | ❌ Aucune | Se connecter |
| `/Permission/my-permissions` | GET | ✅ Auth | Mes permissions |
| `/Eleve/paginated` | GET | `Eleve.ReadAll` | Liste paginée des élèves |
| `/Paiement/paginated` | GET | `Paiement.ReadAll` | Liste paginée des paiements |
| `/Paiement/{id}/valider` | POST | `Paiement.Validate` | Valider un paiement |
| `/Note/eleve/{id}` | GET | `Note.Read` | Notes d'un élève |
| `/Ecole` | GET | `Ecole.ReadAll` | Liste des écoles |

---

## 🔐 Liste des Permissions Principales

| Catégorie | Permissions | Description |
|-----------|-------------|-------------|
| **Écoles** | `Ecole.Create`, `Ecole.Read`, `Ecole.Update`, `Ecole.Delete` | Gestion des écoles |
| **Élèves** | `Eleve.Create`, `Eleve.Read`, `Eleve.Update`, `Eleve.Delete` | Gestion des élèves |
| **Paiements** | `Paiement.Create`, `Paiement.Read`, `Paiement.Update`, `Paiement.Validate` | Gestion des paiements |
| **Notes** | `Note.Create`, `Note.Read`, `Note.Update`, `Note.Delete` | Gestion des notes |
| **Agents** | `Agent.Create`, `Agent.Read`, `Agent.Update`, `Agent.Delete` | Gestion des agents |

---

## 🚨 Gestion des Erreurs

```javascript
// error-handler.js
export function handleApiError(error) {
  // Erreur réseau
  if (!error.response) {
    return 'Erreur de connexion. Vérifiez votre réseau.';
  }

  // Erreurs API
  switch (error.response.status) {
    case 400:
      return 'Données invalides. Vérifiez votre saisie.';
    case 401:
      localStorage.clear();
      window.location.href = '/login';
      return 'Session expirée. Veuillez vous reconnecter.';
    case 403:
      return "⛔ Vous n'avez pas la permission d'effectuer cette action.";
    case 404:
      return 'Ressource introuvable.';
    case 500:
      return 'Erreur serveur. Veuillez réessayer plus tard.';
    default:
      return 'Une erreur est survenue.';
  }
}

// Utilisation
try {
  await apiCall('/Ecole', { method: 'POST', body: JSON.stringify(data) });
} catch (error) {
  alert(handleApiError(error));
}
```

---

## 📚 Ressources Supplémentaires

- 📘 **Documentation complète** : `API_DOCUMENTATION_FRONTEND.md`
- 🔐 **Guide des permissions** : `GUIDE_FRONTEND_PERMISSIONS.md`
- 🔒 **Exemples de sécurisation** : `EXEMPLE_SECURISATION_ENDPOINTS.md`
- 📋 **Collection Postman** : `KelasiNaBiso_API.postman_collection.json`

---

## ✅ Checklist de Démarrage

- [ ] Importer la collection Postman
- [ ] Tester l'authentification dans Postman
- [ ] Créer un service d'authentification dans votre frontend
- [ ] Implémenter la vérification des permissions
- [ ] Tester un appel API authentifié
- [ ] Gérer les erreurs 401 (déconnexion automatique)
- [ ] Afficher/masquer les éléments UI selon les permissions

---

## 🆘 Support

**Des questions ?**
- 📧 Email : support@kelasinabiso.com
- 📱 WhatsApp : +243 999 999 999
- 📖 Documentation : https://docs.kelasinabiso.com

---

**Bon développement ! 🚀**

