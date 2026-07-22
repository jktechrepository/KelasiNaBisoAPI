# 🔐 Analyse des Permissions - Devoirs à Domicile

**Date** : 30 novembre 2025  
**Objectif** : Vérifier et corriger les permissions pour les devoirs à domicile selon les rôles

---

## 📋 Exigences Métier

### Permissions Requises

| Rôle | Émettre un devoir | Lire les devoirs | Portée |
|------|-------------------|------------------|--------|
| **Super-Admin** | ✅ OUI | ✅ OUI | Toutes les écoles |
| **Admin** | ✅ OUI | ✅ OUI | Son école uniquement |
| **Directeur** | ✅ OUI | ✅ OUI | Son école uniquement |
| **Enseignant** | ✅ OUI | ✅ OUI | Sa classe uniquement |
| **Parent** | ❌ NON | ✅ OUI | Classe de son enfant uniquement |
| **Élève** | ❌ NON | ✅ OUI | Sa classe uniquement |

---

## 🔍 Analyse de l'État Actuel

### 1. **Publier un devoir** (`POST /api/DevoirADomicile`)

**État actuel :**
```csharp
[Authorize(Roles = $"{UserRoles.ENSEIGNANT},{UserRoles.DIRECTEUR}")]
```

**Problèmes identifiés :**
- ❌ **ADMIN** ne peut pas publier
- ❌ **SUPER_ADMIN** ne peut pas publier
- ✅ Enseignant peut publier (correct)
- ✅ Directeur peut publier (correct)

**Vérification métier :**
- ✅ Vérifie que l'agent peut publier pour la classe (`AgentPeutPublierPourClasseAsync`)
- ✅ Vérifie que l'agent a un `IdAgent` (correct)

---

### 2. **Voir mes devoirs publiés** (`GET /api/DevoirADomicile/mes-devoirs`)

**État actuel :**
```csharp
[Authorize(Roles = $"{UserRoles.ENSEIGNANT},{UserRoles.DIRECTEUR}")]
```

**Problèmes identifiés :**
- ❌ **ADMIN** ne peut pas voir les devoirs de son école
- ❌ **SUPER_ADMIN** ne peut pas voir les devoirs
- ✅ Enseignant voit ses devoirs (correct)
- ✅ Directeur voit ses devoirs (correct)
- ⚠️ **Mais** : Admin/Directeur/Super-Admin devraient voir TOUS les devoirs de leur école, pas seulement ceux qu'ils ont publiés

**Logique actuelle :**
- Récupère uniquement les devoirs publiés par l'agent (`GetByAgentAsync`)
- **Problème** : Admin/Directeur devraient voir tous les devoirs de leur école

---

### 3. **Voir les devoirs d'une classe** (`GET /api/DevoirADomicile/classe/{idClasse}`)

**État actuel :**
```csharp
[Authorize(Roles = $"{UserRoles.PARENT},{UserRoles.ELEVE}")]
```

**Problèmes identifiés :**
- ❌ **ADMIN** ne peut pas voir les devoirs d'une classe de son école
- ❌ **DIRECTEUR** ne peut pas voir les devoirs d'une classe de son école
- ❌ **SUPER_ADMIN** ne peut pas voir les devoirs
- ❌ **ENSEIGNANT** ne peut pas voir les devoirs de sa classe (via cet endpoint)
- ✅ Parent peut voir (correct, avec vérification d'accès)
- ✅ Élève peut voir (correct, avec vérification d'accès)

**Vérification métier :**
- ✅ Vérifie l'accès à la classe (`UserPeutAccederAClasseAsync`)
- ⚠️ **Mais** : La méthode `UserPeutAccederAClasseAsync` ne vérifie pas ADMIN et SUPER_ADMIN

---

### 4. **Télécharger un devoir** (`GET /api/DevoirADomicile/{id}/telecharger`)

**État actuel :**
```csharp
[Authorize(Roles = $"{UserRoles.PARENT},{UserRoles.ELEVE}")]
```

**Problèmes identifiés :**
- ❌ **ADMIN** ne peut pas télécharger
- ❌ **DIRECTEUR** ne peut pas télécharger
- ❌ **SUPER_ADMIN** ne peut pas télécharger
- ❌ **ENSEIGNANT** ne peut pas télécharger
- ✅ Parent peut télécharger (correct)
- ✅ Élève peut télécharger (correct)

**Vérification métier :**
- ✅ Vérifie l'accès au devoir (`UserPeutAccederAuDevoirAsync`)
- ⚠️ **Mais** : Même problème que ci-dessus

---

### 5. **Voir un devoir** (`GET /api/DevoirADomicile/{id}`)

**État actuel :**
```csharp
[Authorize] // Authentification requise uniquement
```

**Problèmes identifiés :**
- ⚠️ Pas de restriction de rôle (tous les utilisateurs authentifiés peuvent voir)
- ✅ Vérifie l'accès au devoir (`UserPeutAccederAuDevoirAsync`)
- ⚠️ **Mais** : La méthode de vérification ne gère pas ADMIN et SUPER_ADMIN

---

## 🔧 Méthodes de Vérification d'Accès

### `UserPeutAccederAClasseAsync`

**État actuel :**
- ✅ Vérifie DIRECTEUR (accès à toutes les classes de son école)
- ✅ Vérifie ENSEIGNANT (accès à sa classe)
- ✅ Vérifie PARENT (accès si enfant dans la classe)
- ✅ Vérifie ELEVE (accès si dans la classe)
- ❌ **Manque** : ADMIN (accès à toutes les classes de son école)
- ❌ **Manque** : SUPER_ADMIN (accès à toutes les classes de toutes les écoles)

### `AgentPeutPublierPourClasseAsync`

**État actuel :**
- ✅ Vérifie si Directeur (accès à toutes les classes de son école)
- ✅ Vérifie si Titulaire de classe
- ✅ Vérifie si enseigne un cours dans la classe
- ❌ **Manque** : ADMIN (peut publier pour toutes les classes de son école)
- ❌ **Manque** : SUPER_ADMIN (peut publier pour toutes les classes)

---

## ✅ Corrections Nécessaires

### 1. Ajouter ADMIN et SUPER_ADMIN aux endpoints

**Endpoints à modifier :**
- `POST /api/DevoirADomicile` (Publier)
- `GET /api/DevoirADomicile/mes-devoirs` (Voir mes devoirs)
- `GET /api/DevoirADomicile/classe/{idClasse}` (Voir devoirs d'une classe)
- `GET /api/DevoirADomicile/{id}/telecharger` (Télécharger)

### 2. Améliorer la logique "mes-devoirs"

**Problème actuel :**
- Admin/Directeur/Super-Admin voient uniquement leurs propres devoirs
- **Solution** : Créer un endpoint séparé ou modifier la logique pour retourner tous les devoirs de l'école selon le rôle

### 3. Corriger `UserPeutAccederAClasseAsync`

**Ajouter :**
- Vérification ADMIN (accès à toutes les classes de son école)
- Vérification SUPER_ADMIN (accès à toutes les classes)

### 4. Corriger `AgentPeutPublierPourClasseAsync`

**Ajouter :**
- Vérification ADMIN (peut publier pour toutes les classes de son école)
- Vérification SUPER_ADMIN (peut publier pour toutes les classes)

---

## 📊 Matrice des Permissions (Après Corrections)

| Action | Super-Admin | Admin | Directeur | Enseignant | Parent | Élève |
|--------|-------------|-------|-----------|------------|--------|-------|
| **Publier devoir** | ✅ Toutes écoles | ✅ Son école | ✅ Son école | ✅ Sa classe | ❌ | ❌ |
| **Voir tous devoirs école** | ✅ Toutes écoles | ✅ Son école | ✅ Son école | ❌ | ❌ | ❌ |
| **Voir devoirs classe** | ✅ Toutes classes | ✅ Classes son école | ✅ Classes son école | ✅ Sa classe | ✅ Classe enfant | ✅ Sa classe |
| **Télécharger devoir** | ✅ Tous | ✅ Son école | ✅ Son école | ✅ Sa classe | ✅ Classe enfant | ✅ Sa classe |

---

## 🎯 Plan d'Action

1. ✅ Modifier les attributs `[Authorize]` pour inclure ADMIN et SUPER_ADMIN
2. ✅ Corriger `UserPeutAccederAClasseAsync` pour gérer ADMIN et SUPER_ADMIN
3. ✅ Corriger `AgentPeutPublierPourClasseAsync` pour gérer ADMIN et SUPER_ADMIN
4. ✅ Améliorer l'endpoint "mes-devoirs" pour retourner tous les devoirs de l'école pour Admin/Directeur
5. ✅ Tester toutes les combinaisons de rôles

---

## ⚠️ Points d'Attention

1. **Super-Admin** : Accès à TOUTES les écoles
   - Doit pouvoir voir/publier pour n'importe quelle école
   - Nécessite une vérification spéciale

2. **Admin vs Directeur** : Même niveau de permissions pour leur école
   - Tous deux peuvent gérer tous les devoirs de leur école
   - La différence est dans d'autres fonctionnalités (gestion utilisateurs, etc.)

3. **Enseignant** : Accès limité à sa classe
   - Ne peut voir/publier que pour les classes où il enseigne
   - Vérification via `AffectationCours` ou `TitulaireClasse`

4. **Parent** : Accès limité à la classe de son enfant
   - Ne peut voir que les devoirs de la classe où son enfant est inscrit
   - Vérification via `Eleve.IdTuteur`

