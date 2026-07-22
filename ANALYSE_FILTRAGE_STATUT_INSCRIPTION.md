# 🔍 Analyse : Filtrage par Statut dans `/api/Inscription/ecoles/{idecole}/paged`

**Date** : 2025-01-16  
**Version** : 1.0

---

## 📍 Endpoint Analysé

```
GET /api/Inscription/ecoles/{idecole}/paged
```

---

## 🔍 Code Analysé

### **1. Controller** (`Controllers/InscriptionController.cs`)

```csharp
[HttpGet("ecoles/{idecole}/paged")]
[ProducesResponseType(typeof(PagedResult<Inscription>), 200)]
public async Task<ActionResult<PagedResult<Inscription>>> GetInscriptionsByEcolePaged(int idecole, [FromQuery] PagedRequest request)
{
    var result = await _inscriptionRepository.GetByEcolePagedAsync(idecole, request);
    return Ok(result);
}
```

### **2. Service** (`Services/InscriptionService.cs` - Lignes 1032-1073)

```csharp
public async Task<PagedResult<Inscription>> GetByEcolePagedAsync(int idEcole, PagedRequest request)
{
    var query = _context.Inscriptions
        .Include(i => i.Eleve)
        .Include(i => i.Classe)
        .Include(i => i.AnneeScolaire)
        .Where(i => i.IdEcole == idEcole)
        .AsQueryable();

    // ⚠️ FILTRAGE CONDITIONNEL PAR STATUT
    // Filtrer par statut
    if (!request.IncludeInactive)
    {
        query = query.Where(i => i.Statut == true);
    }

    // ... reste du code (recherche, tri, pagination)
    
    return await query.ToPagedAsync(request);
}
```

### **3. DTO** (`Models/DTOs/Pagination/PagedRequest.cs`)

```csharp
/// <summary>
/// Inclure les éléments désactivés (Statut = false)
/// </summary>
public bool IncludeInactive { get; set; } = false; // ✅ Valeur par défaut : false
```

---

## ✅ Conclusion

### **Comportement Actuel**

1. **Par défaut** (sans paramètre `IncludeInactive`) :
   - ✅ **Seules les inscriptions avec `Statut == true` sont retournées**
   - `IncludeInactive` a une valeur par défaut de `false`
   - Le filtre `query.Where(i => i.Statut == true)` est appliqué

2. **Si `IncludeInactive=true` est passé explicitement** :
   - ⚠️ **Toutes les inscriptions sont retournées** (actives ET inactives)
   - Le filtre par statut n'est pas appliqué

---

## 📊 Exemples de Requêtes

### **Exemple 1 : Sans paramètre (comportement par défaut)**

```http
GET /api/Inscription/ecoles/9/paged?pageNumber=1&pageSize=15
```

**Résultat** : ✅ Seules les inscriptions avec `Statut == true` sont retournées

---

### **Exemple 2 : Avec IncludeInactive=false (explicite)**

```http
GET /api/Inscription/ecoles/9/paged?pageNumber=1&pageSize=15&IncludeInactive=false
```

**Résultat** : ✅ Seules les inscriptions avec `Statut == true` sont retournées

---

### **Exemple 3 : Avec IncludeInactive=true (explicite)**

```http
GET /api/Inscription/ecoles/9/paged?pageNumber=1&pageSize=15&IncludeInactive=true
```

**Résultat** : ⚠️ **Toutes les inscriptions sont retournées** (actives ET inactives)

---

## 🎯 Réponse à Votre Question

**Question** : "J'aimerais me rassurer que le endpoint ne retourne que les inscriptions ayant le 'statut': true"

**Réponse** : 
- ✅ **OUI, par défaut**, l'endpoint retourne uniquement les inscriptions avec `Statut == true`
- ⚠️ **MAIS**, si un utilisateur passe explicitement `IncludeInactive=true` dans la requête, alors toutes les inscriptions seront retournées

---

## 🔧 Recommandation

Si vous voulez **garantir** que seules les inscriptions actives sont **toujours** retournées (même si `IncludeInactive=true` est passé), vous avez deux options :

### **Option 1 : Supprimer le paramètre IncludeInactive** (Recommandé)

Modifier le code pour toujours filtrer sur `Statut == true` :

```csharp
public async Task<PagedResult<Inscription>> GetByEcolePagedAsync(int idEcole, PagedRequest request)
{
    var query = _context.Inscriptions
        .Include(i => i.Eleve)
        .Include(i => i.Classe)
        .Include(i => i.AnneeScolaire)
        .Where(i => i.IdEcole == idEcole)
        .Where(i => i.Statut == true) // ✅ TOUJOURS filtrer sur Statut == true
        .AsQueryable();

    // Supprimer la condition if (!request.IncludeInactive)
    // ... reste du code
}
```

### **Option 2 : Garder le paramètre mais documenter clairement**

Si vous voulez garder la flexibilité, documenter clairement dans les commentaires XML que `IncludeInactive=true` retourne toutes les inscriptions.

---

## 📝 Fichiers Concernés

- **Controller** : `Controllers/InscriptionController.cs` (ligne 106-111)
- **Service** : `Services/InscriptionService.cs` (lignes 1032-1073)
- **DTO** : `Models/DTOs/Pagination/PagedRequest.cs` (ligne 53)

---

## ✅ Vérification

Pour tester le comportement actuel :

1. **Test 1** : Appeler l'endpoint sans `IncludeInactive`
   ```http
   GET /api/Inscription/ecoles/9/paged
   ```
   → Vérifier que seules les inscriptions avec `Statut == true` sont retournées

2. **Test 2** : Appeler l'endpoint avec `IncludeInactive=false`
   ```http
   GET /api/Inscription/ecoles/9/paged?IncludeInactive=false
   ```
   → Vérifier que seules les inscriptions avec `Statut == true` sont retournées

3. **Test 3** : Appeler l'endpoint avec `IncludeInactive=true`
   ```http
   GET /api/Inscription/ecoles/9/paged?IncludeInactive=true
   ```
   → Vérifier que toutes les inscriptions sont retournées (actives + inactives)

---

**Version** : 1.0  
**Date** : 2025-01-16
