# 🔧 Correction : Extraction IdEcole depuis le Token JWT

**Date :** 2024-01-15  
**Problème :** Erreur "Token JWT invalide : aucune école associée à l'utilisateur" lors du bulk insert inscription Excel

---

## 🔍 Analyse du Problème

### Symptôme
Lors de l'appel à `POST /api/Inscription/bulk-excel`, l'erreur suivante est retournée :
```json
{
  "message": "Token JWT invalide : aucune école associée à l'utilisateur"
}
```

### Cause Identifiée

Le problème venait de plusieurs points :

1. **Création du claim avec valeur vide** : Si `utilisateur.IdEcole` était `null`, le token contenait un claim `"idEcole"` avec une chaîne vide `""`
2. **Extraction du claim vide** : Le middleware créait un claim même si la valeur était vide
3. **Parsing échoué** : `GetCurrentUserSchoolId()` essayait de parser `""` en `int`, ce qui échouait et retournait `null`

---

## ✅ Corrections Effectuées

### 1. Service `SimpleJwtService` ✅
**Fichier :** `Services/SimpleJwtService.cs`

**Changement :**
- ✅ Ne créer le claim `idEcole` que si `IdEcole` a une valeur valide (> 0)
- ✅ Ne créer le claim `ecole` que si le nom de l'école existe

**Avant :**
```csharp
new Claim("idEcole", utilisateur.IdEcole?.ToString() ?? ""), // Créait toujours un claim, même vide
```

**Après :**
```csharp
// Ajouter IdEcole uniquement s'il existe (ne pas créer de claim vide)
if (utilisateur.IdEcole.HasValue && utilisateur.IdEcole.Value > 0)
{
    claims.Add(new Claim("idEcole", utilisateur.IdEcole.Value.ToString()));
}
```

---

### 2. Helper `AuditHelpers` ✅
**Fichier :** `Helpers/AuditHelpers.cs`

**Changement :**
- ✅ Amélioration de `GetCurrentUserSchoolId()` pour gérer les chaînes vides
- ✅ Vérification de plusieurs noms de claims possibles (`IdEcole` et `idEcole`)
- ✅ Validation que la valeur n'est pas vide avant de parser

**Avant :**
```csharp
var schoolClaim = controller.User.FindFirst("IdEcole");
if (schoolClaim != null && int.TryParse(schoolClaim.Value, out int idEcole))
{
    return idEcole;
}
```

**Après :**
```csharp
// Essayer plusieurs noms de claims possibles
var schoolClaim = controller.User.FindFirst("IdEcole")
               ?? controller.User.FindFirst("idEcole");

if (schoolClaim != null && !string.IsNullOrWhiteSpace(schoolClaim.Value))
{
    if (int.TryParse(schoolClaim.Value, out int idEcole) && idEcole > 0)
    {
        return idEcole;
    }
}
```

---

### 3. Middleware `SimpleAuthMiddleware` ✅
**Fichier :** `Middleware/SimpleAuthMiddleware.cs`

**Changement :**
- ✅ Ne créer le claim `IdEcole` que si la valeur n'est pas vide

**Avant :**
```csharp
if (payload.TryGetProperty("idEcole", out var idEcole))
    claims.Add(new Claim("IdEcole", idEcole.GetString() ?? "")); // Créait même si vide
```

**Après :**
```csharp
if (payload.TryGetProperty("idEcole", out var idEcole))
{
    var idEcoleValue = idEcole.GetString();
    // Ne créer le claim que si la valeur n'est pas vide
    if (!string.IsNullOrWhiteSpace(idEcoleValue))
    {
        claims.Add(new Claim("IdEcole", idEcoleValue));
    }
}
```

---

### 4. Middleware `JwtMiddleware` ✅
**Fichier :** `Middleware/JwtMiddleware.cs`

**Changement :**
- ✅ Même correction que `SimpleAuthMiddleware` pour cohérence

---

### 5. Contrôleurs (Messages d'erreur améliorés) ✅
**Fichiers :** 
- `Controllers/InscriptionController.cs`
- `Controllers/PaiementController.cs`
- `Controllers/EleveController.cs`

**Changement :**
- ✅ Messages d'erreur plus détaillés pour diagnostiquer le problème
- ✅ Distinction entre "claim absent" et "claim présent mais valeur invalide"

**Avant :**
```csharp
return Unauthorized(new { message = "Token JWT invalide : aucune école associée à l'utilisateur" });
```

**Après :**
```csharp
// Vérifier si le claim existe mais est vide
var schoolClaim = User.FindFirst("IdEcole") ?? User.FindFirst("idEcole");
if (schoolClaim != null)
{
    return Unauthorized(new { 
        message = "Aucune école associée à l'utilisateur. Veuillez contacter l'administrateur pour associer une école à votre compte.",
        details = $"Claim IdEcole trouvé mais valeur invalide: '{schoolClaim.Value}'"
    });
}
else
{
    return Unauthorized(new { 
        message = "Token JWT invalide : aucune école associée à l'utilisateur. Le token ne contient pas d'information sur l'école.",
        details = "Le claim IdEcole est absent du token. L'utilisateur doit se reconnecter après avoir été associé à une école."
    });
}
```

---

## 🔄 Comportement Après Correction

### Scénario 1 : Utilisateur avec IdEcole valide
1. ✅ Le token contient le claim `idEcole` avec la valeur correcte
2. ✅ `GetCurrentUserSchoolId()` extrait correctement l'ID
3. ✅ L'endpoint fonctionne normalement

### Scénario 2 : Utilisateur sans IdEcole (null)
1. ✅ Le token **ne contient pas** le claim `idEcole` (au lieu d'un claim vide)
2. ✅ `GetCurrentUserSchoolId()` retourne `null`
3. ✅ Message d'erreur clair indiquant que l'utilisateur doit être associé à une école

### Scénario 3 : Utilisateur avec IdEcole = 0 ou vide
1. ✅ Le token **ne contient pas** le claim `idEcole`
2. ✅ `GetCurrentUserSchoolId()` retourne `null`
3. ✅ Message d'erreur clair

---

## 🧪 Tests à Effectuer

### Test 1 : Utilisateur avec IdEcole valide
```http
POST /api/Inscription/bulk-excel
Authorization: Bearer {token_avec_idEcole}
```
**Résultat attendu :** ✅ Succès

### Test 2 : Utilisateur sans IdEcole
```http
POST /api/Inscription/bulk-excel
Authorization: Bearer {token_sans_idEcole}
```
**Résultat attendu :** 
```json
{
  "message": "Token JWT invalide : aucune école associée à l'utilisateur...",
  "details": "Le claim IdEcole est absent du token..."
}
```

### Test 3 : Vérifier le token
Décoder le token JWT et vérifier :
- Si `IdEcole` est présent dans le payload
- Si la valeur est correcte

---

## 🔍 Diagnostic

### Comment vérifier si le problème persiste

1. **Vérifier le token JWT** :
   - Décoder le token sur https://jwt.io
   - Vérifier si `idEcole` est présent dans le payload
   - Vérifier la valeur de `idEcole`

2. **Vérifier l'utilisateur en base** :
   ```sql
   SELECT IdUtilisateur, IdEcole, NomUtilisateur 
   FROM Utilisateurs 
   WHERE IdUtilisateur = {id_utilisateur};
   ```

3. **Vérifier les logs** :
   - Les logs devraient maintenant indiquer clairement si le claim est absent ou invalide

---

## 📝 Actions Recommandées

### Si l'utilisateur n'a pas d'IdEcole

1. **Associer l'utilisateur à une école** :
   ```sql
   UPDATE Utilisateurs 
   SET IdEcole = {idEcole}
   WHERE IdUtilisateur = {idUtilisateur};
   ```

2. **Demander à l'utilisateur de se reconnecter** :
   - Le nouveau token contiendra l'IdEcole
   - L'endpoint fonctionnera correctement

### Si le problème persiste

1. Vérifier que le middleware JWT est bien configuré dans `Program.cs`
2. Vérifier que le token est bien généré avec `IdEcole`
3. Vérifier les logs pour voir les détails de l'erreur

---

## ✅ Fichiers Modifiés

1. ✅ `Services/SimpleJwtService.cs`
2. ✅ `Helpers/AuditHelpers.cs`
3. ✅ `Middleware/SimpleAuthMiddleware.cs`
4. ✅ `Middleware/JwtMiddleware.cs`
5. ✅ `Controllers/InscriptionController.cs`
6. ✅ `Controllers/PaiementController.cs`
7. ✅ `Controllers/EleveController.cs`

---

**Dernière mise à jour :** 2024-01-15

