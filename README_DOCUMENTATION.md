# 📚 Documentation KelasiNaBiso API - Index Central

> **Bienvenue dans la documentation complète de l'API KelasiNaBiso !**  
> Cette page vous guide vers la documentation appropriée selon votre rôle et vos besoins.

---

## 🎯 Par Où Commencer ?

### 👨‍💻 **Vous êtes Développeur Frontend ?**

1. **Démarrage rapide** (15 minutes) :  
   📄 [`QUICK_START_FRONTEND.md`](./QUICK_START_FRONTEND.md)

2. **Documentation API complète** :  
   📄 [`API_DOCUMENTATION_FRONTEND.md`](./API_DOCUMENTATION_FRONTEND.md)

3. **Gestion des permissions dans l'UI** :  
   📄 [`GUIDE_FRONTEND_PERMISSIONS.md`](./GUIDE_FRONTEND_PERMISSIONS.md)

4. **Outils Postman** :
   - 📦 [`KelasiNaBiso_API.postman_collection.json`](./KelasiNaBiso_API.postman_collection.json)
   - ⚙️ [`KelasiNaBiso_Dev.postman_environment.json`](./KelasiNaBiso_Dev.postman_environment.json)

---

### 🔧 **Vous êtes Développeur Backend ?**

1. **Architecture du projet** :  
   📄 [`STRUCTURE_PROJET.md`](./STRUCTURE_PROJET.md)

2. **Guide RBAC (Sécurité)** :  
   📄 [`RBAC_GUIDE_UTILISATION.md`](./RBAC_GUIDE_UTILISATION.md)

3. **Exemples de sécurisation** :  
   📄 [`EXEMPLE_SECURISATION_ENDPOINTS.md`](./EXEMPLE_SECURISATION_ENDPOINTS.md)

4. **Implémentation RBAC complète** :  
   📄 [`IMPLEMENTATION_RBAC_GUIDE_FINAL.md`](./IMPLEMENTATION_RBAC_GUIDE_FINAL.md)

5. **Matrice de sécurité** :  
   📄 [`MATRICE_ENDPOINTS_SECURITE.md`](./MATRICE_ENDPOINTS_SECURITE.md)

---

### 👔 **Vous êtes Chef de Projet / Product Owner ?**

1. **Vue d'ensemble de l'architecture** :  
   📄 [`STRUCTURE_PROJET.md`](./STRUCTURE_PROJET.md)

2. **Matrice des permissions par rôle** :  
   📄 [`MATRICE_ENDPOINTS_SECURITE.md`](./MATRICE_ENDPOINTS_SECURITE.md)

3. **Documentation API pour comprendre les fonctionnalités** :  
   📄 [`API_DOCUMENTATION_FRONTEND.md`](./API_DOCUMENTATION_FRONTEND.md)

---

## 📖 Documentation par Catégorie

### 🚀 **Guides de Démarrage**

| Document | Description | Public Cible |
|----------|-------------|--------------|
| [`QUICK_START_FRONTEND.md`](./QUICK_START_FRONTEND.md) | Guide rapide de démarrage (15 min) avec exemples Vue.js, React, Angular | Frontend Devs |
| [`STRUCTURE_PROJET.md`](./STRUCTURE_PROJET.md) | Architecture et organisation du projet backend | Backend Devs, Nouveaux arrivants |

---

### 📘 **Documentation API**

| Document | Description | Public Cible |
|----------|-------------|--------------|
| [`API_DOCUMENTATION_FRONTEND.md`](./API_DOCUMENTATION_FRONTEND.md) | Documentation complète de tous les endpoints avec exemples | Frontend Devs, QA, Product Owners |
| [`GUIDE_FRONTEND_PERMISSIONS.md`](./GUIDE_FRONTEND_PERMISSIONS.md) | Intégration des permissions côté frontend | Frontend Devs |

---

### 🔐 **Sécurité & RBAC**

| Document | Description | Public Cible |
|----------|-------------|--------------|
| [`RBAC_GUIDE_UTILISATION.md`](./RBAC_GUIDE_UTILISATION.md) | Guide d'utilisation du système de permissions | Backend Devs, Admins |
| [`EXEMPLE_SECURISATION_ENDPOINTS.md`](./EXEMPLE_SECURISATION_ENDPOINTS.md) | Exemples pratiques de sécurisation d'endpoints | Backend Devs |
| [`IMPLEMENTATION_RBAC_GUIDE_FINAL.md`](./IMPLEMENTATION_RBAC_GUIDE_FINAL.md) | Guide d'implémentation RBAC complet | Backend Devs (avancé) |
| [`MATRICE_ENDPOINTS_SECURITE.md`](./MATRICE_ENDPOINTS_SECURITE.md) | Matrice de sécurité et permissions requises | Tous les développeurs, Product Owners |

---

### 🛠️ **Outils**

| Fichier | Description | Utilisation |
|---------|-------------|-------------|
| [`KelasiNaBiso_API.postman_collection.json`](./KelasiNaBiso_API.postman_collection.json) | Collection Postman complète avec tous les endpoints | Importer dans Postman pour tester l'API |
| [`KelasiNaBiso_Dev.postman_environment.json`](./KelasiNaBiso_Dev.postman_environment.json) | Environnement de développement Postman | Importer dans Postman pour configurer les variables |

---

## 🔑 Concepts Clés

### Authentification

- **Type** : JWT (JSON Web Token)
- **Endpoint** : `POST /api/Utilisateur/authentifier`
- **Durée de validité** : 24 heures (configurable)
- **Header requis** : `Authorization: Bearer {token}`

### Permissions (RBAC)

L'API utilise un système **RBAC (Role-Based Access Control)** avec permissions granulaires :

- **Attribut backend** : `[Permission("Nom.Action")]`
- **Exemple** : `[Permission("Ecole.Create")]`
- **Format** : `Categorie.Action` (ex: `Paiement.Validate`, `Note.Update`)

### Multi-tenancy

Chaque utilisateur est associé à une **École** (`EcoleId`). Les données sont automatiquement filtrées selon l'école de l'utilisateur connecté.

---

## 📊 Endpoints Principaux

| Catégorie | Endpoint | Permission | Description |
|-----------|----------|------------|-------------|
| **Auth** | `POST /api/Utilisateur/authentifier` | ❌ Aucune | Se connecter |
| **Permissions** | `GET /api/Permission/my-permissions` | ✅ Auth | Mes permissions |
| **Écoles** | `GET /api/Ecole` | `Ecole.ReadAll` | Liste des écoles |
| **Élèves** | `GET /api/Eleve/paginated` | `Eleve.ReadAll` | Liste paginée des élèves |
| **Paiements** | `POST /api/Paiement/{id}/valider` | `Paiement.Validate` | Valider un paiement |
| **Notes** | `GET /api/Note/eleve/{id}` | `Note.Read` | Notes d'un élève |

---

## 🎭 Rôles Disponibles

| Rôle | Niveau | Description | Accès |
|------|--------|-------------|-------|
| **Super-Admin** | 1 | Administrateur système | Toutes les permissions |
| **Admin** | 2 | Administrateur d'école | Gestion complète de son école |
| **Directeur** | 3 | Directeur d'établissement | Gestion pédagogique et administrative |
| **Enseignant** | 4 | Professeur | Gestion des notes et présences |
| **Comptable** | 5 | Gestionnaire financier | Validation des paiements |
| **Parent** | 6 | Parent d'élève | Consultation des infos de ses enfants |
| **Élève** | 7 | Élève | Consultation de ses propres données |

---

## 🔧 Configuration

### Variables d'Environnement

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=kelasinabiso;User=root;Password=;"
  },
  "Jwt": {
    "SecretKey": "votre_cle_secrete_super_longue_et_securisee",
    "Issuer": "KelasiNaBisoAPI",
    "Audience": "KelasiNaBisoClients",
    "ExpirationMinutes": 1440
  }
}
```

### Base de Données

- **Type** : MySQL / MariaDB
- **Port par défaut** : 3306
- **Nom de la DB** : `kelasinabiso`

---

## 🚀 Commandes Utiles

### Backend

```bash
# Lancer l'application en développement
dotnet run

# Créer une migration
dotnet ef migrations add NomDeLaMigration

# Appliquer les migrations
dotnet ef database update

# Build de production
dotnet publish -c Release
```

### Postman

1. **Importer la collection** : `KelasiNaBiso_API.postman_collection.json`
2. **Importer l'environnement** : `KelasiNaBiso_Dev.postman_environment.json`
3. **Se connecter** : Exécuter `Authentification → Se Connecter`
4. Le **token est sauvegardé automatiquement** ! ✨

---

## 📝 Exemples de Code

### JavaScript (Authentification)

```javascript
async function login(emailOuTelephone, motDePasse) {
  const response = await fetch('/api/Utilisateur/authentifier', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ emailOuTelephone, motDePasse })
  });

  const data = await response.json();
  
  // Sauvegarder le token et les permissions
  localStorage.setItem('accessToken', data.accessToken);
  localStorage.setItem('permissions', JSON.stringify(data.permissions));
  
  return data;
}
```

### JavaScript (Appel API Authentifié)

```javascript
async function apiCall(endpoint, options = {}) {
  const token = localStorage.getItem('accessToken');
  
  const response = await fetch(`/api${endpoint}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`,
      ...options.headers
    }
  });
  
  if (response.status === 401) {
    // Token expiré, rediriger vers login
    localStorage.clear();
    window.location.href = '/login';
  }
  
  return response.json();
}
```

### JavaScript (Vérification de Permission)

```javascript
class PermissionService {
  static hasPermission(permissionName) {
    const permissions = JSON.parse(localStorage.getItem('permissions') || '[]');
    return permissions.includes(permissionName);
  }
}

// Utilisation
if (PermissionService.hasPermission('Ecole.Create')) {
  // Afficher le bouton "Créer une école"
  document.getElementById('btnCreateEcole').style.display = 'block';
}
```

---

## ⚠️ Codes d'Erreur HTTP

| Code | Signification | Action Recommandée |
|------|---------------|-------------------|
| 200 | OK | Requête réussie |
| 201 | Created | Ressource créée avec succès |
| 204 | No Content | Opération réussie (pas de contenu à retourner) |
| 400 | Bad Request | Vérifier les données envoyées |
| 401 | Unauthorized | Se reconnecter (token invalide/expiré) |
| 403 | Forbidden | Permission manquante |
| 404 | Not Found | Ressource introuvable |
| 500 | Internal Server Error | Contacter le support |

---

## 🆘 Support

### Pour les Développeurs

- 📧 **Email** : dev@kelasinabiso.com
- 📱 **WhatsApp** : +243 999 999 999
- 🌐 **Documentation** : https://docs.kelasinabiso.com

### Pour les Questions Techniques

- 🐛 **Bugs** : Créer une issue sur GitHub
- 💡 **Suggestions** : dev@kelasinabiso.com

---

## 📅 Versions et Mises à Jour

| Version | Date | Changements Majeurs |
|---------|------|---------------------|
| **1.0** | 28 Oct 2025 | Implémentation RBAC avec permissions granulaires |
| | | Intégration des permissions dans l'authentification |
| | | Documentation complète pour frontend |
| | | Collection Postman complète |

---

## ✅ Checklist d'Intégration

### Frontend

- [ ] Importer la collection Postman
- [ ] Tester l'authentification
- [ ] Implémenter le service d'authentification
- [ ] Implémenter la vérification des permissions
- [ ] Gérer les erreurs 401 (déconnexion automatique)
- [ ] Afficher/masquer les éléments UI selon les permissions

### Backend

- [ ] Comprendre l'architecture du projet
- [ ] Lire le guide RBAC
- [ ] Sécuriser les nouveaux endpoints avec `[Permission]`
- [ ] Tester les permissions dans Postman
- [ ] Documenter les nouveaux endpoints

---

## 🎯 Bonnes Pratiques

### Sécurité

1. **TOUJOURS** utiliser HTTPS en production
2. **NE JAMAIS** stocker le token en clair dans le code
3. **TOUJOURS** vérifier les permissions côté backend (attribut `[Permission]`)
4. **Déconnecter automatiquement** l'utilisateur si le token expire (401)

### Performance

1. **Utiliser la pagination** pour les listes d'éléments
2. **Mettre en cache** les permissions dans localStorage
3. **Optimiser les appels API** (éviter les requêtes inutiles)

### Expérience Utilisateur

1. **Afficher/masquer** les boutons selon les permissions
2. **Afficher des messages clairs** en cas d'erreur
3. **Indiquer visuellement** les actions interdites

---

## 🏆 Contributeurs

Merci à tous les développeurs qui ont contribué à ce projet ! 🙏

---

**Documentation mise à jour le 28 Octobre 2025**  
**Version de l'API** : 1.0  
**Framework** : ASP.NET Core 8.0

---

<div align="center">
  <strong>🚀 Bon développement avec KelasiNaBiso API ! 🚀</strong>
</div>

