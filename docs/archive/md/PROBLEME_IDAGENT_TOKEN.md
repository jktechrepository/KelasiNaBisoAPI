# ⚠️ Problème : IdAgent non présent dans le token JWT

**Date** : 30 novembre 2025  
**Status** : 🔴 **EN COURS DE RÉSOLUTION**

---

## 🔍 Problème Identifié

L'utilisateur Admin (`jk2@kelasinabiso.cd`) a bien un `idAgent: 139` dans la **réponse d'authentification**, mais le **token JWT** ne contient pas le claim `IdAgent`.

### Conséquence

L'upload d'un devoir échoue avec l'erreur :
```
Vous devez être un agent pour publier un devoir
```

Car `CurrentUserService.AgentId` retourne `null` car le claim `IdAgent` n'est pas dans le token.

---

## ✅ Corrections Tentées

1. ✅ Ajout du claim `IdAgent` dans `SimpleJwtService.cs`
2. ✅ Modification des noms des claims : `AgentId` → `IdAgent`, `TuteurId` → `IdTuteur`
3. ✅ Mise à jour de `CurrentUserService` pour chercher `IdAgent` et `IdTuteur`
4. ✅ Tentative de restauration de `IdAgent` depuis l'utilisateur initial
5. ✅ Copie de `IdAgent` dans `_utilisateur` avant génération du token

**Résultat** : Le token ne contient toujours pas `IdAgent`

---

## 🔍 Analyse

### Vérifications Effectuées

1. ✅ La réponse d'authentification contient bien `idAgent: 139`
2. ✅ La requête SQL sélectionne bien `u.IdAgent`
3. ❌ Le token JWT ne contient pas le claim `IdAgent`

### Hypothèses

1. **Entity Framework ne charge pas `IdAgent`** : Même si la requête SQL le sélectionne, Entity Framework peut ne pas charger la propriété dans l'objet `Utilisateur` en mémoire
2. **Le code modifié n'est pas utilisé** : L'application n'utilise peut-être pas le nouveau code
3. **Problème de timing** : `IdAgent` est peut-être perdu entre le chargement initial et la génération du token

---

## 💡 Solutions Possibles

### Solution 1 : Récupérer IdAgent depuis la réponse d'authentification

Au lieu de compter sur Entity Framework pour charger `IdAgent`, récupérer directement la valeur depuis la réponse d'authentification et l'ajouter manuellement au token.

### Solution 2 : Forcer le chargement de IdAgent

Modifier `GetByEmailAsync` pour forcer explicitement le chargement de `IdAgent` :
```csharp
public async Task<Utilisateur> GetByEmailAsync(string email)
{
    var user = await _context.Utilisateurs
        .Include(u => u.Ecole)
        .Where(u => u.Statut == true)
        .FirstOrDefaultAsync(u => u.Email == email);
    
    // Forcer le chargement de IdAgent
    if (user != null)
    {
        await _context.Entry(user).Reference(u => u.Agent).LoadAsync();
        // Ou directement depuis la base
        var idAgent = await _context.Utilisateurs
            .Where(u => u.IdUtilisateur == user.IdUtilisateur)
            .Select(u => u.IdAgent)
            .FirstOrDefaultAsync();
        user.IdAgent = idAgent;
    }
    
    return user;
}
```

### Solution 3 : Utiliser une requête directe

Au lieu de passer l'objet `Utilisateur` à `GenerateToken`, passer directement les valeurs nécessaires :
```csharp
var accessToken = _jwtService.GenerateToken(
    _utilisateur,
    utilisateur.IdAgent,  // Passer explicitement
    utilisateur.IdTuteur  // Passer explicitement
);
```

---

## 🎯 Prochaine Étape Recommandée

**Solution 1** semble la plus simple et la plus fiable :
1. Récupérer `IdAgent` depuis l'utilisateur initial (`utilisateur.IdAgent`)
2. S'assurer qu'il est copié dans `_utilisateur` avant la génération du token
3. Vérifier que `SimpleJwtService.GenerateToken` ajoute bien le claim

**OU**

Modifier `GenerateToken` pour accepter `IdAgent` et `IdTuteur` en paramètres séparés :
```csharp
public string GenerateToken(Utilisateur utilisateur, int? idAgent = null, int? idTuteur = null)
{
    // Utiliser idAgent et idTuteur passés en paramètres si disponibles
    var agentId = idAgent ?? utilisateur.IdAgent;
    var tuteurId = idTuteur ?? utilisateur.IdTuteur;
    
    if (agentId.HasValue)
    {
        claims.Add(new Claim("IdAgent", agentId.Value.ToString()));
    }
    // ...
}
```

---

## 📝 Fichiers Modifiés

- ✅ `Services/SimpleJwtService.cs` : Ajout du claim `IdAgent`
- ✅ `Services/CurrentUserService.cs` : Recherche du claim `IdAgent`
- ✅ `Controllers/UtilisateurController.cs` : Tentative de restauration de `IdAgent`

---

**En attente de résolution...** 🔧

