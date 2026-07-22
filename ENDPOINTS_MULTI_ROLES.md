# 🔐 ENDPOINTS MULTI-RÔLES - DOCUMENTATION

**Date** : 2025  
**Version** : 1.0

---

## 📋 ENDPOINTS DISPONIBLES

### 1. GET `/api/Utilisateur/{id}/roles`

**Description** : Récupérer tous les rôles actifs d'un utilisateur

**Autorisation** : Admin, Super-Admin

**Paramètres** :
- `id` (int, path) : ID de l'utilisateur

**Réponse 200** :
```json
[
  {
    "idRole": 1,
    "nom": "Enseignant",
    "description": "Enseignant de l'école",
    "niveau": 4,
    "statut": true
  },
  {
    "idRole": 5,
    "nom": "Parent",
    "description": "Parent d'élève",
    "niveau": 5,
    "statut": true
  }
]
```

**Exemple de requête** :
```bash
GET /api/Utilisateur/123/roles
Authorization: Bearer {token}
```

---

### 2. POST `/api/Utilisateur/{id}/roles/{roleId}`

**Description** : Ajouter un rôle à un utilisateur

**Autorisation** : Admin, Super-Admin

**Paramètres** :
- `id` (int, path) : ID de l'utilisateur
- `roleId` (int, path) : ID du rôle à ajouter
- `isPrimary` (bool, query, optionnel) : Définir ce rôle comme principal (défaut: false)

**Réponse 200** :
```json
{
  "message": "Rôle ajouté avec succès",
  "utilisateurId": 123,
  "roleId": 5,
  "roleNom": "Parent",
  "isPrimary": false,
  "roles": [
    {
      "idRole": 1,
      "nom": "Enseignant"
    },
    {
      "idRole": 5,
      "nom": "Parent"
    }
  ]
}
```

**Exemple de requête** :
```bash
POST /api/Utilisateur/123/roles/5?isPrimary=false
Authorization: Bearer {token}
```

**Cas d'erreur** :
- 404 : Utilisateur ou rôle introuvable
- 400 : Rôle déjà assigné
- 403 : Pas autorisé à modifier cet utilisateur

---

### 3. DELETE `/api/Utilisateur/{id}/roles/{roleId}`

**Description** : Retirer un rôle d'un utilisateur

**Autorisation** : Admin, Super-Admin

**Paramètres** :
- `id` (int, path) : ID de l'utilisateur
- `roleId` (int, path) : ID du rôle à retirer

**Réponse 200** :
```json
{
  "message": "Rôle retiré avec succès",
  "utilisateurId": 123,
  "roleId": 5,
  "roleNom": "Parent",
  "roles": [
    {
      "idRole": 1,
      "nom": "Enseignant"
    }
  ]
}
```

**Exemple de requête** :
```bash
DELETE /api/Utilisateur/123/roles/5
Authorization: Bearer {token}
```

**Cas d'erreur** :
- 404 : Utilisateur ou rôle introuvable
- 400 : L'utilisateur n'a pas ce rôle
- 400 : Impossible de retirer le dernier rôle actif
- 403 : Pas autorisé à modifier cet utilisateur

---

### 4. PUT `/api/Utilisateur/{id}/roles/{roleId}/primary`

**Description** : Définir le rôle principal d'un utilisateur

**Autorisation** : Admin, Super-Admin

**Paramètres** :
- `id` (int, path) : ID de l'utilisateur
- `roleId` (int, path) : ID du rôle à définir comme principal

**Réponse 200** :
```json
{
  "message": "Rôle principal défini avec succès",
  "utilisateurId": 123,
  "roleId": 5,
  "roleNom": "Parent",
  "primaryRole": {
    "idRole": 5,
    "nom": "Parent"
  },
  "roles": [
    {
      "idRole": 1,
      "nom": "Enseignant"
    },
    {
      "idRole": 5,
      "nom": "Parent"
    }
  ]
}
```

**Exemple de requête** :
```bash
PUT /api/Utilisateur/123/roles/5/primary
Authorization: Bearer {token}
```

**Cas d'erreur** :
- 404 : Utilisateur ou rôle introuvable
- 400 : L'utilisateur n'a pas ce rôle. Ajoutez-le d'abord.
- 403 : Pas autorisé à modifier cet utilisateur

---

## 🔐 SÉCURITÉ

### Règles d'autorisation

1. **Super-Admin** : Peut modifier tous les utilisateurs de toutes les écoles
2. **Admin** : Peut modifier uniquement les utilisateurs de son école

### Validations

- ✅ Un utilisateur doit avoir au moins un rôle actif
- ✅ Un utilisateur ne peut avoir qu'un seul rôle principal à la fois
- ✅ Impossible d'ajouter un rôle déjà assigné (soft delete réactivé si inactif)
- ✅ Impossible de retirer le dernier rôle actif

---

## 📊 EXEMPLE D'UTILISATION

### Scénario : Un enseignant qui est aussi parent

**1. Récupérer les rôles actuels** :
```bash
GET /api/Utilisateur/123/roles
# Retourne : [{"idRole": 1, "nom": "Enseignant"}]
```

**2. Ajouter le rôle Parent** :
```bash
POST /api/Utilisateur/123/roles/5
# Ajoute le rôle "Parent" (idRole=5)
```

**3. Vérifier les rôles** :
```bash
GET /api/Utilisateur/123/roles
# Retourne : [
#   {"idRole": 1, "nom": "Enseignant"},
#   {"idRole": 5, "nom": "Parent"}
# ]
```

**4. Définir Parent comme rôle principal** :
```bash
PUT /api/Utilisateur/123/roles/5/primary
# Définit "Parent" comme rôle principal
```

**5. Résultat** :
- L'utilisateur a maintenant 2 rôles : Enseignant + Parent
- Le rôle principal est "Parent"
- Les permissions sont l'union des permissions des deux rôles
- Le JWT contient les deux rôles

---

## 🔄 IMPACT SUR L'AUTHENTIFICATION

L'endpoint `/api/Utilisateur/authentifier` retourne maintenant :

```json
{
  "success": true,
  "accessToken": "...",
  "utilisateur": {...},
  "nomRole": "Parent",  // Rôle principal
  "roles": [            // ✅ NOUVEAU : Tous les rôles
    {"idRole": 1, "nom": "Enseignant"},
    {"idRole": 5, "nom": "Parent"}
  ],
  "primaryRole": {      // ✅ NOUVEAU : Rôle principal
    "idRole": 5,
    "nom": "Parent"
  },
  "permissions": [...]  // Union des permissions de tous les rôles
}
```

---

**📅 Date** : 2025  
**👤 Auteur** : Assistant IA  
**🔄 Version** : 1.0

