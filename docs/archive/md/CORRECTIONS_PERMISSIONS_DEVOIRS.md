# ✅ Corrections des Permissions - Devoirs à Domicile

**Date** : 30 novembre 2025  
**Statut** : ✅ **TOUTES LES CORRECTIONS APPLIQUÉES**

---

## 📋 Résumé des Corrections

Toutes les permissions pour les devoirs à domicile ont été corrigées selon les exigences métier.

---

## 🔧 Modifications Effectuées

### 1. ✅ Attributs `[Authorize]` Mis à Jour

**Endpoints modifiés :**

| Endpoint | Avant | Après |
|----------|-------|-------|
| `POST /api/DevoirADomicile` | Enseignant, Directeur | **+ Admin, Super-Admin** |
| `GET /api/DevoirADomicile/mes-devoirs` | Enseignant, Directeur | **+ Admin, Super-Admin** |
| `GET /api/DevoirADomicile/classe/{idClasse}` | Parent, Élève | **+ Enseignant, Directeur, Admin, Super-Admin** |
| `GET /api/DevoirADomicile/{id}/telecharger` | Parent, Élève | **+ Enseignant, Directeur, Admin, Super-Admin** |
| `PUT /api/DevoirADomicile/{id}` | Enseignant, Directeur | **+ Admin, Super-Admin** |
| `DELETE /api/DevoirADomicile/{id}` | Enseignant, Directeur | **+ Admin, Super-Admin** |

### 2. ✅ Méthode `UserPeutAccederAClasseAsync` Corrigée

**Ajouts :**
- ✅ Vérification **Super-Admin** : Accès à toutes les classes de toutes les écoles
- ✅ Vérification **Admin** : Accès à toutes les classes de son école
- ✅ Vérification **Directeur** : Déjà présente (maintenue)
- ✅ Vérification **Enseignant** : Déjà présente (maintenue)
- ✅ Vérification **Parent** : Déjà présente (maintenue)
- ✅ Vérification **Élève** : Déjà présente (maintenue)

**Code ajouté :**
```csharp
// Super-Admin : Accès à toutes les classes de toutes les écoles
if (role == UserRoles.SUPER_ADMIN)
{
    return true;
}

// Admin : Accès à toutes les classes de son école
if (role == UserRoles.ADMIN)
{
    // Vérifie que la classe appartient à l'école de l'admin
    return classe.Direction?.IdEcole == adminEcoleId;
}
```

### 3. ✅ Méthode `AgentPeutPublierPourClasseAsync` Corrigée

**Ajouts :**
- ✅ Vérification **Super-Admin** : Peut publier pour toutes les classes
- ✅ Vérification **Admin** : Peut publier pour toutes les classes de son école
- ✅ Vérification **Directeur** : Déjà présente (maintenue)
- ✅ Vérification **Enseignant** : Déjà présente (maintenue)

**Code ajouté :**
```csharp
// Super-Admin : Peut publier pour toutes les classes
if (role == UserRoles.SUPER_ADMIN)
{
    return true;
}

// Admin : Peut publier pour toutes les classes de son école
if (role == UserRoles.ADMIN)
{
    // Vérifie que la classe appartient à l'école de l'admin
    return classe.Direction?.IdEcole == adminEcoleId;
}
```

### 4. ✅ Endpoint `mes-devoirs` Amélioré

**Logique selon le rôle :**

| Rôle | Comportement |
|------|--------------|
| **Super-Admin** | Voir **tous les devoirs** de toutes les écoles |
| **Admin** | Voir **tous les devoirs** de son école |
| **Directeur** | Voir **tous les devoirs** de son école |
| **Enseignant** | Voir **uniquement ses propres devoirs** |

**Code ajouté :**
```csharp
// Super-Admin : Voir tous les devoirs
if (role == UserRoles.SUPER_ADMIN)
{
    devoirs = await _devoirRepository.GetAllAsync();
}

// Admin ou Directeur : Voir tous les devoirs de leur école
else if (role == UserRoles.ADMIN || role == UserRoles.DIRECTEUR)
{
    devoirs = await _devoirRepository.GetByEcoleAsync(idEcole);
}

// Enseignant : Voir uniquement ses propres devoirs
else
{
    devoirs = await _devoirRepository.GetByAgentAsync(idAgent.Value);
}
```

### 5. ✅ Méthode `GetAllAsync` Ajoutée

**Nouvelle méthode dans `IDevoirADomicileRepository` :**
- Permet à Super-Admin de récupérer tous les devoirs de toutes les écoles
- Inclut les relations nécessaires (Ecole, Direction, Agent, Classe, Cours)
- Filtre uniquement les devoirs actifs (`Statut == true`)
- Tri par date de publication décroissante

---

## 📊 Matrice des Permissions Finale

| Action | Super-Admin | Admin | Directeur | Enseignant | Parent | Élève |
|--------|-------------|-------|-----------|------------|--------|-------|
| **Publier devoir** | ✅ Toutes écoles | ✅ Son école | ✅ Son école | ✅ Sa classe | ❌ | ❌ |
| **Voir tous devoirs école** | ✅ Toutes écoles | ✅ Son école | ✅ Son école | ❌ | ❌ | ❌ |
| **Voir devoirs classe** | ✅ Toutes classes | ✅ Classes son école | ✅ Classes son école | ✅ Sa classe | ✅ Classe enfant | ✅ Sa classe |
| **Télécharger devoir** | ✅ Tous | ✅ Son école | ✅ Son école | ✅ Sa classe | ✅ Classe enfant | ✅ Sa classe |
| **Modifier devoir** | ✅ Tous | ✅ Son école | ✅ Son école | ✅ Ses devoirs | ❌ | ❌ |
| **Supprimer devoir** | ✅ Tous | ✅ Son école | ✅ Son école | ✅ Ses devoirs | ❌ | ❌ |

---

## ✅ Tests Recommandés

### Test 1 : Super-Admin
- [ ] Publier un devoir pour une classe d'une école
- [ ] Voir tous les devoirs de toutes les écoles
- [ ] Télécharger un devoir de n'importe quelle école
- [ ] Modifier/Supprimer un devoir de n'importe quelle école

### Test 2 : Admin
- [ ] Publier un devoir pour une classe de son école
- [ ] Voir tous les devoirs de son école (pas seulement les siens)
- [ ] Télécharger un devoir de son école
- [ ] Ne peut PAS publier pour une classe d'une autre école

### Test 3 : Directeur
- [ ] Publier un devoir pour une classe de son école
- [ ] Voir tous les devoirs de son école
- [ ] Télécharger un devoir de son école
- [ ] Ne peut PAS publier pour une classe d'une autre école

### Test 4 : Enseignant
- [ ] Publier un devoir pour sa classe uniquement
- [ ] Voir uniquement ses propres devoirs
- [ ] Télécharger un devoir de sa classe
- [ ] Ne peut PAS publier pour une classe où il n'enseigne pas

### Test 5 : Parent
- [ ] Voir les devoirs de la classe de son enfant
- [ ] Télécharger un devoir de la classe de son enfant
- [ ] Ne peut PAS voir les devoirs d'autres classes

### Test 6 : Élève
- [ ] Voir les devoirs de sa classe
- [ ] Télécharger un devoir de sa classe
- [ ] Ne peut PAS voir les devoirs d'autres classes

---

## 🔍 Fichiers Modifiés

1. ✅ `Controllers/DevoirADomicileController.cs`
   - Tous les attributs `[Authorize]` mis à jour
   - Logique de l'endpoint `mes-devoirs` améliorée
   - Messages d'erreur améliorés

2. ✅ `Services/DevoirADomicileService.cs`
   - `UserPeutAccederAClasseAsync` : Ajout Super-Admin et Admin
   - `AgentPeutPublierPourClasseAsync` : Ajout Super-Admin et Admin
   - `GetAllAsync` : Nouvelle méthode pour Super-Admin

3. ✅ `Services/Repositories/IDevoirADomicileRepository.cs`
   - Ajout de la méthode `GetAllAsync`

---

## ⚠️ Points d'Attention

1. **Super-Admin** : Accès complet à toutes les écoles
   - Peut publier/voir/modifier/supprimer pour n'importe quelle école
   - Utilise `GetAllAsync()` pour récupérer tous les devoirs

2. **Admin vs Directeur** : Même niveau de permissions pour leur école
   - Tous deux peuvent gérer tous les devoirs de leur école
   - Utilisent `GetByEcoleAsync()` pour récupérer les devoirs de leur école

3. **Enseignant** : Accès limité à sa classe
   - Ne peut voir/publier que pour les classes où il enseigne
   - Utilise `GetByAgentAsync()` pour récupérer uniquement ses devoirs

4. **Cohérence métier** : Tous les rôles (Super-Admin, Admin, Directeur, Enseignant) ont un `IdAgent`
   - Selon la documentation, ils sont tous liés à un Agent avec fonction "Manager Général" ou autre
   - La vérification `AgentId` est donc valide pour tous ces rôles

---

## ✅ Statut Final

- ✅ **Compilation** : 0 erreur
- ✅ **Tous les endpoints** : Permissions corrigées
- ✅ **Méthodes de vérification** : Complètes
- ✅ **Logique métier** : Conforme aux exigences

**Prêt pour les tests !** 🎉

