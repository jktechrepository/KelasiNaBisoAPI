# 📘 Documentation API KelasiNaBiso - Guide Frontend

> **Version**: 1.0  
> **Base URL**: `https://localhost:7105/api` (Dev) | `https://api.kelasinabiso.com/api` (Prod)  
> **Authentification**: Bearer Token (JWT)

---

## 📋 Table des Matières

1. [Introduction](#introduction)
2. [Authentification](#authentification)
3. [Gestion des Permissions](#gestion-des-permissions)
4. [Écoles](#écoles)
5. [Utilisateurs](#utilisateurs)
6. [Élèves](#élèves)
7. [Agents](#agents)
8. [Paiements](#paiements)
9. [Notes](#notes)
10. [Classes](#classes)
11. [Codes d'Erreur](#codes-derreur)
12. [Exemples Complets](#exemples-complets)

---

## 🔐 Introduction

### Format de Réponse Standard

Toutes les réponses de l'API suivent ce format :

```json
// Succès
{
  "data": { /* Données */ },
  "message": "Opération réussie"
}

// Erreur
{
  "error": "Message d'erreur",
  "statusCode": 400
}
```

### Headers Requis

```http
Content-Type: application/json
Authorization: Bearer {votre_token_jwt}
```

---

## 🔑 Authentification

### 1. Se Connecter

**Endpoint**: `POST /api/Utilisateur/authentifier`  
**Authentification**: ❌ Non requise (endpoint public)

#### Requête

```http
POST /api/Utilisateur/authentifier
Content-Type: application/json

{
  "emailOuTelephone": "+243999999999",
  "motDePasse": "Super-Admin",
  "fcmToken": "fcm_token_device_123", // Optionnel
  "deviceType": "Android", // Optionnel
  "deviceModel": "Samsung Galaxy S21", // Optionnel
  "osVersion": "Android 12" // Optionnel
}
```

#### Réponse (200 OK)

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
      "nom": "Ekelasi School",
      "logo": null
    },
    "idRole": 1
  },
  "nomRole": "Super-Admin",
  "nomEcole": "Ekelasi School",
  "permissions": [
    "Ecole.Create",
    "Ecole.Read",
    "Eleve.Create",
    "Paiement.Validate",
    "Note.Update"
  ]
}
```

#### Codes d'Erreur

| Code | Description |
|------|-------------|
| 400 | Données invalides |
| 401 | Email/Mot de passe incorrect ou compte désactivé |
| 500 | Erreur serveur |

#### Exemple JavaScript

```javascript
async function login(emailOuTelephone, motDePasse) {
  const response = await fetch('/api/Utilisateur/authentifier', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ emailOuTelephone, motDePasse })
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message);
  }

  const data = await response.json();
  
  // Stocker le token et les permissions
  localStorage.setItem('accessToken', data.accessToken);
  localStorage.setItem('permissions', JSON.stringify(data.permissions));
  localStorage.setItem('user', JSON.stringify(data.utilisateur));
  
  return data;
}
```

### 2. Changer le Mot de Passe

**Endpoint**: `POST /api/Utilisateur/changer-mot-de-passe`  
**Authentification**: ✅ Requise

#### Requête

```http
POST /api/Utilisateur/changer-mot-de-passe
Authorization: Bearer {token}
Content-Type: application/json

{
  "idUtilisateur": 1,
  "ancienMotDePasse": "Super-Admin",
  "nouveauMotDePasse": "NewPassword123!"
}
```

#### Réponse (200 OK)

```json
{
  "message": "Mot de passe modifié avec succès"
}
```

---

## 🔐 Gestion des Permissions

### 1. Voir Mes Permissions

**Endpoint**: `GET /api/Permission/my-permissions`  
**Authentification**: ✅ Requise  
**Permission**: ❌ Aucune (tout utilisateur authentifié)

#### Requête

```http
GET /api/Permission/my-permissions
Authorization: Bearer {token}
```

#### Réponse (200 OK)

```json
[
  "Ecole.Read",
  "Eleve.Create",
  "Eleve.Read",
  "Paiement.Create",
  "Note.Read"
]
```

### 2. Lister Toutes les Permissions

**Endpoint**: `GET /api/Permission`  
**Authentification**: ✅ Requise  
**Permission**: ✅ `Permission.ReadAll`

#### Requête

```http
GET /api/Permission
Authorization: Bearer {token}
```

#### Réponse (200 OK)

```json
[
  {
    "idPermission": 1,
    "nom": "Ecole.Create",
    "categorie": "Ecole",
    "action": "Create",
    "description": "Créer une école",
    "statut": true,
    "dateCreation": "2025-10-28T00:00:00Z"
  },
  {
    "idPermission": 2,
    "nom": "Eleve.Read",
    "categorie": "Eleve",
    "action": "Read",
    "description": "Voir un élève",
    "statut": true,
    "dateCreation": "2025-10-28T00:00:00Z"
  }
]
```

### 3. Vérifier une Permission

**Endpoint**: `GET /api/Permission/check/{permissionName}`  
**Authentification**: ✅ Requise

#### Requête

```http
GET /api/Permission/check/Paiement.Validate
Authorization: Bearer {token}
```

#### Réponse (200 OK)

```json
{
  "permissionName": "Paiement.Validate",
  "hasPermission": true
}
```

---

## 🏫 Écoles

### 1. Lister les Écoles

**Endpoint**: `GET /api/Ecole`  
**Authentification**: ✅ Requise  
**Permission**: ✅ `Ecole.ReadAll`

#### Requête

```http
GET /api/Ecole
Authorization: Bearer {token}
```

#### Réponse (200 OK)

```json
[
  {
    "idEcole": 1,
    "referenceEcole": "550003f06f0f4fac0d56697ac505033e",
    "nom": "École Primaire ABC",
    "slogan": "Excellence et Discipline",
    "type": "Primaire",
    "logo": "https://example.com/logo.png",
    "telephone": "+243999999999",
    "emailContact": "contact@ecole-abc.com",
    "siteWeb": "https://ecole-abc.com",
    "provinceEducationnel": "Kinshasa",
    "nomCompletResponsable": "Jean Dupont",
    "description": "École primaire d'excellence",
    "statut": true,
    "dateCreation": "2025-01-15T00:00:00Z",
    "province": "Kinshasa",
    "ville": "Kinshasa",
    "commune": "Gombe",
    "quartier": "Centre-Ville",
    "avenue": "Avenue de la Liberté",
    "numero": "123"
  }
]
```

### 2. Créer une École

**Endpoint**: `POST /api/Ecole`  
**Authentification**: ✅ Requise  
**Permission**: ✅ `Ecole.Create`

#### Requête

```http
POST /api/Ecole
Authorization: Bearer {token}
Content-Type: application/json

{
  "nom": "Nouvelle École",
  "slogan": "Excellence et Innovation",
  "type": "Secondaire",
  "telephone": "+243999888777",
  "emailContact": "contact@nouvelle-ecole.com",
  "provinceEducationnel": "Kinshasa",
  "nomCompletResponsable": "Marie Kabila",
  "description": "École secondaire moderne",
  "province": "Kinshasa",
  "ville": "Kinshasa",
  "commune": "Limete",
  "quartier": "Industriel",
  "avenue": "Avenue Commerce",
  "numero": "456"
}
```

#### Réponse (201 Created)

```json
{
  "idEcole": 2,
  "referenceEcole": "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6",
  "nom": "Nouvelle École",
  "statut": true,
  "dateCreation": "2025-10-28T12:30:00Z"
}
```

### 3. Modifier une École

**Endpoint**: `PUT /api/Ecole/{id}`  
**Authentification**: ✅ Requise  
**Permission**: ✅ `Ecole.Update`

#### Requête

```http
PUT /api/Ecole/2
Authorization: Bearer {token}
Content-Type: application/json

{
  "idEcole": 2,
  "nom": "Nouvelle École (Modifiée)",
  "telephone": "+243999888777",
  "emailContact": "nouveau@ecole.com"
}
```

#### Réponse (204 No Content)

### 4. Supprimer une École

**Endpoint**: `DELETE /api/Ecole/{id}`  
**Authentification**: ✅ Requise  
**Permission**: ✅ `Ecole.Delete` (Super-Admin uniquement)

#### Requête

```http
DELETE /api/Ecole/2
Authorization: Bearer {token}
```

#### Réponse (204 No Content)

---

## 👥 Utilisateurs

### 1. Lister les Utilisateurs

**Endpoint**: `GET /api/Utilisateur`  
**Authentification**: ✅ Requise  
**Permission**: ✅ `Utilisateur.ReadAll`

#### Requête

```http
GET /api/Utilisateur
Authorization: Bearer {token}
```

#### Réponse (200 OK)

```json
[
  {
    "idUtilisateur": 1,
    "nomUtilisateur": "ADMIN",
    "prenomUtilisateur": "Super",
    "email": "admin@ekelasi.com",
    "telephone": "+243999999999",
    "genre": "Masculin",
    "statut": true,
    "idRole": 1,
    "role": {
      "idRole": 1,
      "nom": "Super-Admin"
    },
    "idEcole": 1
  }
]
```

### 2. Créer un Utilisateur

**Endpoint**: `POST /api/Utilisateur`  
**Authentification**: ✅ Requise  
**Permission**: ✅ `Utilisateur.Create`

#### Requête

```http
POST /api/Utilisateur
Authorization: Bearer {token}
Content-Type: application/json

{
  "nomUtilisateur": "DUPONT",
  "prenomUtilisateur": "Jean",
  "email": "jean.dupont@email.com",
  "telephone": "+243998877665",
  "genre": "Masculin",
  "motDePasseHash": "Password123!",
  "idRole": 2,
  "idEcole": 1,
  "dateNaissance": "1990-05-15",
  "lieuNaissance": "Kinshasa"
}
```

#### Réponse (201 Created)

```json
{
  "idUtilisateur": 5,
  "referenceUtilisateur": "abc123def456...",
  "email": "jean.dupont@email.com",
  "statut": true
}
```

---

## 👨‍🎓 Élèves

### 1. Lister les Élèves (avec Pagination)

**Endpoint**: `GET /api/Eleve/paginated`  
**Authentification**: ✅ Requise  
**Permission**: ✅ `Eleve.ReadAll`

#### Paramètres Query

| Paramètre | Type | Défaut | Description |
|-----------|------|--------|-------------|
| `pageNumber` | int | 1 | Numéro de page |
| `pageSize` | int | 20 | Nombre d'éléments par page |
| `searchTerm` | string | null | Recherche par nom/matricule |
| `idEcole` | int | null | Filtrer par école |
| `idClasse` | int | null | Filtrer par classe |

#### Requête

```http
GET /api/Eleve/paginated?pageNumber=1&pageSize=20&searchTerm=Jean
Authorization: Bearer {token}
```

#### Réponse (200 OK)

```json
{
  "items": [
    {
      "idEleve": 1,
      "referenceEleve": "ref123",
      "matricule": "MAT001",
      "nom": "MUKENDI",
      "postnom": "Jean",
      "prenom": "Pierre",
      "nomComplet": "MUKENDI Jean Pierre",
      "genre": "Masculin",
      "dateNaissance": "2010-05-15",
      "lieuNaissance": "Kinshasa",
      "photoUrl": null,
      "nationalite": "Congolaise",
      "statut": true,
      "idTuteur": 1,
      "tuteur": {
        "idTuteur": 1,
        "nomComplet": "Papa MUKENDI",
        "telephone": "+243999888777"
      }
    }
  ],
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 150,
  "totalPages": 8,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### 2. Créer un Élève

**Endpoint**: `POST /api/Eleve`  
**Authentification**: ✅ Requise  
**Permission**: ✅ `Eleve.Create`

#### Requête

```http
POST /api/Eleve
Authorization: Bearer {token}
Content-Type: application/json

{
  "nom": "KABAMBA",
  "postnom": "Marie",
  "prenom": "Grace",
  "genre": "Féminin",
  "dateNaissance": "2012-08-20",
  "lieuNaissance": "Lubumbashi",
  "nationalite": "Congolaise",
  "idTuteur": 2,
  "province": "Kinshasa",
  "ville": "Kinshasa",
  "commune": "Lemba"
}
```

#### Réponse (201 Created)

```json
{
  "idEleve": 25,
  "referenceEleve": "xyz789...",
  "matricule": "MAT025",
  "nomComplet": "KABAMBA Marie Grace",
  "statut": true
}
```

---

## 👔 Agents

### 1. Lister les Agents

**Endpoint**: `GET /api/Agent`  
**Authentification**: ✅ Requise  
**Permission**: ✅ `Agent.ReadAll`

#### Requête

```http
GET /api/Agent
Authorization: Bearer {token}
```

#### Réponse (200 OK)

```json
[
  {
    "idAgent": 1,
    "referenceAgent": "agent001",
    "matricule": "AGT001",
    "nom": "MBUYI",
    "postnom": "Emmanuel",
    "prenom": "David",
    "nomComplet": "MBUYI Emmanuel David",
    "genre": "Masculin",
    "fonction": "Enseignant",
    "specialite": "Mathématiques",
    "dateNaissance": "1985-03-10",
    "lieuNaissance": "Kinshasa",
    "telephone": "+243998765432",
    "email": "mbuyi@ecole.com",
    "statut": true,
    "idEcole": 1
  }
]
```

### 2. Créer un Agent

**Endpoint**: `POST /api/Agent`  
**Authentification**: ✅ Requise  
**Permission**: ✅ `Agent.Create`

#### Requête

```http
POST /api/Agent
Authorization: Bearer {token}
Content-Type: application/json

{
  "nom": "KASONGO",
  "postnom": "Jean",
  "prenom": "Paul",
  "genre": "Masculin",
  "fonction": "Professeur",
  "specialite": "Français",
  "dateNaissance": "1988-07-20",
  "lieuNaissance": "Lubumbashi",
  "telephone": "+243997654321",
  "email": "kasongo@ecole.com",
  "idEcole": 1
}
```

---

## 💰 Paiements

### 1. Lister les Paiements (avec Pagination)

**Endpoint**: `GET /api/Paiement/paginated`  
**Authentification**: ✅ Requise  
**Permission**: ✅ `Paiement.ReadAll`

#### Paramètres Query

| Paramètre | Type | Description |
|-----------|------|-------------|
| `pageNumber` | int | Numéro de page |
| `pageSize` | int | Éléments par page |
| `idEcole` | int | Filtrer par école |
| `idEleve` | int | Filtrer par élève |
| `estValide` | bool | Filtrer par statut de validation |

#### Requête

```http
GET /api/Paiement/paginated?pageNumber=1&pageSize=20&idEcole=1&estValide=true
Authorization: Bearer {token}
```

#### Réponse (200 OK)

```json
{
  "items": [
    {
      "idPaiement": 1,
      "referencePaiement": "PAY001",
      "montantPaye": 50000,
      "devise": "CDF",
      "datePaiement": "2025-10-15T10:30:00Z",
      "modePaiement": "Espèces",
      "estValide": true,
      "dateValidation": "2025-10-15T15:00:00Z",
      "idEleve": 10,
      "eleve": {
        "nomComplet": "MUKENDI Jean Pierre",
        "matricule": "MAT010"
      },
      "idFrais": 1,
      "frais": {
        "libelleFrais": "Frais de scolarité Trimestre 1"
      }
    }
  ],
  "totalCount": 500,
  "totalPages": 25
}
```

### 2. Créer un Paiement

**Endpoint**: `POST /api/Paiement`  
**Authentification**: ✅ Requise  
**Permission**: ✅ `Paiement.Create`

#### Requête

```http
POST /api/Paiement
Authorization: Bearer {token}
Content-Type: application/json

{
  "idEleve": 10,
  "idFrais": 1,
  "montantPaye": 50000,
  "devise": "CDF",
  "datePaiement": "2025-10-28T10:00:00Z",
  "modePaiement": "Mobile Money",
  "referenceTransaction": "MM20251028123456",
  "commentaire": "Paiement via M-Pesa"
}
```

#### Réponse (201 Created)

```json
{
  "idPaiement": 150,
  "referencePaiement": "PAY150",
  "estValide": false,
  "montantPaye": 50000
}
```

### 3. Valider un Paiement

**Endpoint**: `POST /api/Paiement/{id}/valider`  
**Authentification**: ✅ Requise  
**Permission**: ✅ `Paiement.Validate` (Comptable uniquement)

#### Requête

```http
POST /api/Paiement/150/valider
Authorization: Bearer {token}
```

#### Réponse (200 OK)

```json
{
  "message": "Paiement validé avec succès",
  "idPaiement": 150,
  "dateValidation": "2025-10-28T14:30:00Z"
}
```

---

## 📝 Notes

### 1. Lister les Notes d'un Élève

**Endpoint**: `GET /api/Note/eleve/{eleveId}`  
**Authentification**: ✅ Requise  
**Permission**: ✅ `Note.Read` ou `Note.ReadOwn` (pour l'élève)

#### Requête

```http
GET /api/Note/eleve/10
Authorization: Bearer {token}
```

#### Réponse (200 OK)

```json
[
  {
    "idNote": 1,
    "cote": 15.5,
    "coteSur": 20,
    "session": "Session 1",
    "appreciation": "Bon travail",
    "dateEnregistrement": "2025-10-20T00:00:00Z",
    "idEleve": 10,
    "idCours": 5,
    "cours": {
      "idCours": 5,
      "nomCours": "Mathématiques"
    },
    "idProfesseur": 3,
    "professeur": {
      "nomComplet": "MBUYI Emmanuel David"
    }
  }
]
```

### 2. Créer une Note

**Endpoint**: `POST /api/Note`  
**Authentification**: ✅ Requise  
**Permission**: ✅ `Note.Create` (Enseignant)

#### Requête

```http
POST /api/Note
Authorization: Bearer {token}
Content-Type: application/json

{
  "idEleve": 10,
  "idCours": 5,
  "idAnneeScolaire": 1,
  "cote": 16,
  "coteSur": 20,
  "session": "Session 2",
  "appreciation": "Très bon",
  "dateEnregistrement": "2025-10-28T00:00:00Z"
}
```

#### Réponse (201 Created)

```json
{
  "idNote": 50,
  "cote": 16,
  "session": "Session 2"
}
```

---

## 🏛️ Classes

### 1. Lister les Classes

**Endpoint**: `GET /api/Classe`  
**Authentification**: ✅ Requise  
**Permission**: ✅ `Classe.ReadAll`

#### Requête

```http
GET /api/Classe
Authorization: Bearer {token}
```

#### Réponse (200 OK)

```json
[
  {
    "idClasse": 1,
    "nomClasse": "6ème Année Primaire A",
    "capacite": 40,
    "idDirection": 1,
    "direction": {
      "nomDirection": "Primaire"
    },
    "idEcole": 1,
    "statut": true
  }
]
```

---

## ⚠️ Codes d'Erreur

| Code | Signification | Description |
|------|---------------|-------------|
| 200 | OK | Requête réussie |
| 201 | Created | Ressource créée avec succès |
| 204 | No Content | Opération réussie sans contenu à retourner |
| 400 | Bad Request | Données invalides |
| 401 | Unauthorized | Non authentifié ou token invalide |
| 403 | Forbidden | Permission insuffisante |
| 404 | Not Found | Ressource introuvable |
| 500 | Internal Server Error | Erreur serveur |

---

## 📦 Exemples Complets

### Exemple 1 : Flux d'Authentification Complet

```javascript
// 1. Se connecter
async function login(email, password) {
  const response = await fetch('/api/Utilisateur/authentifier', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      emailOuTelephone: email,
      motDePasse: password
    })
  });

  if (!response.ok) {
    throw new Error('Authentification échouée');
  }

  const data = await response.json();
  
  // Stocker dans localStorage
  localStorage.setItem('accessToken', data.accessToken);
  localStorage.setItem('permissions', JSON.stringify(data.permissions));
  localStorage.setItem('user', JSON.stringify(data.utilisateur));
  localStorage.setItem('tokenExpiry', data.expiresAt);
  
  return data;
}

// 2. Vérifier si connecté
function isAuthenticated() {
  const token = localStorage.getItem('accessToken');
  const expiry = localStorage.getItem('tokenExpiry');
  
  if (!token || !expiry) return false;
  
  return new Date(expiry) > new Date();
}

// 3. Faire un appel API authentifié
async function apiCall(endpoint, options = {}) {
  const token = localStorage.getItem('accessToken');
  
  if (!token) {
    throw new Error('Non authentifié');
  }
  
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
    return;
  }
  
  return response.json();
}

// 4. Se déconnecter
function logout() {
  localStorage.clear();
  window.location.href = '/login';
}
```

### Exemple 2 : Service de Gestion des Élèves

```javascript
class EleveService {
  static async getEleves(pageNumber = 1, pageSize = 20, searchTerm = '') {
    return apiCall(
      `/Eleve/paginated?pageNumber=${pageNumber}&pageSize=${pageSize}&searchTerm=${searchTerm}`
    );
  }

  static async getEleveById(id) {
    return apiCall(`/Eleve/${id}`);
  }

  static async createEleve(eleveData) {
    return apiCall('/Eleve', {
      method: 'POST',
      body: JSON.stringify(eleveData)
    });
  }

  static async updateEleve(id, eleveData) {
    return apiCall(`/Eleve/${id}`, {
      method: 'PUT',
      body: JSON.stringify(eleveData)
    });
  }

  static async deleteEleve(id) {
    return apiCall(`/Eleve/${id}`, {
      method: 'DELETE'
    });
  }
}
```

### Exemple 3 : Vérification de Permission

```javascript
// Service de permissions
class PermissionService {
  static hasPermission(permissionName) {
    const permissions = JSON.parse(localStorage.getItem('permissions') || '[]');
    return permissions.includes(permissionName);
  }

  static hasAnyPermission(...permissionNames) {
    const permissions = JSON.parse(localStorage.getItem('permissions') || '[]');
    return permissionNames.some(p => permissions.includes(p));
  }
}

// Utilisation dans l'UI
if (PermissionService.hasPermission('Ecole.Create')) {
  // Afficher le bouton "Créer une école"
  document.getElementById('btnCreateEcole').style.display = 'block';
}

if (PermissionService.hasPermission('Paiement.Validate')) {
  // Afficher le bouton "Valider"
  document.getElementById('btnValidate').disabled = false;
}
```

---

## 📞 Support

Pour toute question ou problème :
- 📧 Email : support@kelasinabiso.com
- 📱 WhatsApp : +243 999 999 999
- 🌐 Documentation complète : https://docs.kelasinabiso.com

---

**Version**: 1.0 | **Dernière mise à jour**: 28 Octobre 2025

