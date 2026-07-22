# 🔐 Guide d'Utilisation du Système RBAC avec Permissions

## ✅ Implémentation Complète

Votre API **KelasiNaBisoAPI** dispose maintenant d'un système complet de **Role-Based Access Control (RBAC)** avec gestion granulaire des permissions.

---

## 📊 Architecture du Système

### 1. **Modèles**
- `Permission` : Représente une permission (ex: "Ecole.Create", "Paiement.Read")
- `RolePermission` : Table de liaison Many-to-Many entre Role et Permission
- `Role` : Rôle utilisateur avec niveau hiérarchique
- `UserRoles` (Enum) : Énumération des rôles du système

### 2. **Services**
- `IPermissionService` / `PermissionService` : Gestion complète des permissions
- `ICurrentUserService` / `CurrentUserService` : Accès facile aux informations de l'utilisateur connecté

### 3. **Attribut de Sécurité**
- `[Permission("Nom.Action")]` : Protège les endpoints par permission

---

## 🎯 Rôles et Permissions par Défaut

### Rôles disponibles
| Rôle | Niveau Hiérarchique | Description |
|------|---------------------|-------------|
| **Super-Admin** | 0 | Accès total (toutes les permissions) |
| **Directeur** | 10 | Gestion complète de son école |
| **Comptable** | 20 | Gestion financière (paiements, frais) |
| **Secrétaire** | 30 | Gestion élèves et inscriptions |
| **Enseignant** | 40 | Notes, présences, cours |
| **Parent** | 50 | Consultation données de ses enfants |
| **Élève** | 60 | Consultation de ses propres données |

### Catégories de Permissions (80+ permissions)
- **Ecole** : Create, Read, ReadAll, Update, Delete
- **Utilisateur** : Create, Read, ReadAll, Update, Delete, ChangePassword
- **Élève** : Create, Read, ReadAll, ReadOwn, ReadChildren, Update, Delete
- **Agent** : Create, Read, ReadAll, Update, Delete
- **Paiement** : Create, Read, ReadAll, ReadOwn, Update, Delete, Validate
- **Note** : Create, Read, ReadAll, ReadOwn, ReadChildren, Update, Delete
- **Tuteur** : Create, Read, ReadAll, Update, Delete
- **Rôle** : Create, Read, ReadAll, Update, Delete
- **Permission** : Create, Read, ReadAll, Update, Delete, Assign, Revoke
- **Classe, Frais, Inscription, Présence, Cours** : CRUD complet

---

## 🚀 Utilisation dans vos Controllers

### Option 1 : Protéger un endpoint avec `[Permission]`

```csharp
[HttpPost]
[Permission("Ecole.Create")] // Seuls les utilisateurs avec cette permission peuvent accéder
public async Task<ActionResult<Ecole>> CreateEcole(Ecole ecole)
{
    // Votre logique ici
}
```

### Option 2 : Vérifier manuellement dans le code

```csharp
[HttpGet]
public async Task<IActionResult> GetSensitiveData()
{
    var currentUser = _currentUserService.GetCurrentUser();
    
    if (currentUser == null)
    {
        return Unauthorized();
    }
    
    // Vérifier si l'utilisateur a la permission
    var hasPermission = await _permissionService.UserHasPermissionAsync(
        currentUser.UserId, 
        "Data.ReadSensitive"
    );
    
    if (!hasPermission)
    {
        return Forbid();
    }
    
    // Logique métier...
}
```

### Option 3 : Filtrage par école (Multi-tenant)

```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<Eleve>>> GetEleves()
{
    var currentUser = _currentUserService.GetCurrentUser();
    
    if (currentUser == null)
    {
        return Unauthorized();
    }
    
    // Super-Admin voit tout
    if (currentUser.Role == UserRoles.SUPER_ADMIN)
    {
        return Ok(await _eleveRepository.GetAllAsync());
    }
    
    // Autres rôles : filtrer par école
    return Ok(await _eleveRepository.GetByEcoleAsync(currentUser.EcoleId));
}
```

---

## 📡 API Endpoints - PermissionController

### 🔍 Consultation des Permissions

#### Lister toutes les permissions
```http
GET /api/Permission
Authorization: Bearer {token}
```

#### Voir mes permissions (utilisateur connecté)
```http
GET /api/Permission/my-permissions
Authorization: Bearer {token}
```
**Réponse** :
```json
[
  "Ecole.Read",
  "Eleve.Create",
  "Eleve.Read",
  "Paiement.Create",
  "Note.Read"
]
```

#### Permissions par catégorie
```http
GET /api/Permission/by-category/Paiement
Authorization: Bearer {token}
```

#### Permissions d'un rôle spécifique
```http
GET /api/Permission/role/3
Authorization: Bearer {token}
```

### ✏️ Gestion des Permissions (Super-Admin uniquement)

#### Créer une permission
```http
POST /api/Permission
Authorization: Bearer {token}
Content-Type: application/json

{
  "nom": "Rapport.Export",
  "categorie": "Rapport",
  "action": "Export",
  "description": "Exporter les rapports en PDF/Excel",
  "statut": true
}
```

#### Assigner une permission à un rôle
```http
POST /api/Permission/assign
Authorization: Bearer {token}
Content-Type: application/json

{
  "roleId": 3,
  "permissionId": 15
}
```

#### Assigner plusieurs permissions en une fois
```http
POST /api/Permission/assign-bulk
Authorization: Bearer {token}
Content-Type: application/json

{
  "roleId": 3,
  "permissionIds": [15, 16, 17, 18, 19]
}
```

#### Retirer une permission d'un rôle
```http
POST /api/Permission/revoke
Authorization: Bearer {token}
Content-Type: application/json

{
  "roleId": 3,
  "permissionId": 15
}
```

#### Vérifier si j'ai une permission
```http
GET /api/Permission/check/Paiement.Validate
Authorization: Bearer {token}
```
**Réponse** :
```json
{
  "permissionName": "Paiement.Validate",
  "hasPermission": true
}
```

---

## 🧪 Tests d'Authentification et Permissions

### 1. Se connecter en tant que Super-Admin
```http
POST /api/Utilisateur/authentifier
Content-Type: application/json

{
  "telephone": "+243999999999",
  "motDePasse": "Super-Admin"
}
```

**Réponse** :
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "utilisateur": {
    "idUtilisateur": 1,
    "nom": "ADMIN",
    "prenom": "Super",
    "telephone": "+243999999999",
    "email": "superadmin@ekelasi.com",
    "role": {
      "idRole": 1,
      "nom": "Super-Admin"
    }
  }
}
```

### 2. Utiliser le token pour accéder aux endpoints protégés
```http
GET /api/Permission/my-permissions
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## 🔧 Configuration et Personnalisation

### Ajouter une nouvelle permission

1. **Ajoutez-la dans `PermissionSeeder.cs`** :
```csharp
new Permission { 
    Nom = "Rapport.Export", 
    Categorie = "Rapport", 
    Action = "Export", 
    Description = "Exporter les rapports", 
    Statut = true 
}
```

2. **Assignez-la aux rôles appropriés** :
```csharp
var directeurPermissions = allPermissions.Where(p =>
    p.Categorie == "Rapport" || // <-- Nouvelle catégorie
    // ... autres catégories
).ToList();
```

3. **Recréez la migration** (optionnel si vous voulez réinitialiser) :
```bash
dotnet ef migrations add AddNewPermissions
dotnet ef database update
```

### Protéger un nouveau endpoint

```csharp
[HttpGet("export")]
[Permission("Rapport.Export")] // 🔒 Protection par permission
public async Task<IActionResult> ExportRapport()
{
    // Votre logique d'export
}
```

---

## 🎨 Exemples Concrets d'Usage

### Exemple 1 : Parent consultant les notes de ses enfants

```csharp
[HttpGet("parent/{parentId}/enfants/notes")]
[Permission("Note.ReadChildren")]
public async Task<ActionResult<IEnumerable<Note>>> GetNotesEnfants(int parentId)
{
    var currentUser = _currentUserService.GetCurrentUser();
    
    // Vérifier que le parent consulte bien SES enfants
    if (currentUser.Role != UserRoles.SUPER_ADMIN && 
        currentUser.Role != UserRoles.DIRECTEUR)
    {
        // Les parents ne peuvent voir que les notes de LEURS enfants
        var tuteur = await _tuteurRepository.GetByUtilisateurIdAsync(currentUser.UserId);
        
        if (tuteur == null || tuteur.IdTuteur != parentId)
        {
            return Forbid("Vous ne pouvez consulter que les notes de vos propres enfants");
        }
    }
    
    // Récupérer les notes des enfants du parent
    var notes = await _noteRepository.GetByParentIdAsync(parentId);
    return Ok(notes);
}
```

### Exemple 2 : Comptable validant un paiement

```csharp
[HttpPost("{id}/valider")]
[Permission("Paiement.Validate")]
public async Task<IActionResult> ValiderPaiement(int id)
{
    var paiement = await _paiementRepository.GetByIdAsync(id);
    
    if (paiement == null)
    {
        return NotFound();
    }
    
    var currentUser = _currentUserService.GetCurrentUser();
    
    // Filtrage multi-tenant : ne valider que les paiements de son école
    if (currentUser.Role != UserRoles.SUPER_ADMIN && 
        paiement.Eleve.IdEcole != currentUser.EcoleId)
    {
        return Forbid("Vous ne pouvez valider que les paiements de votre école");
    }
    
    paiement.EstValide = true;
    paiement.DateValidation = DateTime.Now;
    paiement.IdUtilisateurValidation = currentUser.UserId;
    
    await _paiementRepository.UpdateAsync(paiement);
    
    return Ok(new { message = "Paiement validé avec succès" });
}
```

---

## 📚 Bonnes Pratiques

### ✅ À FAIRE
- ✅ Toujours utiliser `[Permission]` pour protéger les endpoints sensibles
- ✅ Filtrer par `EcoleId` pour le multi-tenant (sauf Super-Admin)
- ✅ Vérifier les permissions dans la logique métier quand nécessaire
- ✅ Utiliser `ICurrentUserService` pour accéder aux infos utilisateur
- ✅ Créer des permissions granulaires (Read vs ReadAll, ReadOwn vs ReadChildren)

### ❌ À ÉVITER
- ❌ Ne jamais exposer les endpoints de gestion des permissions sans protection
- ❌ Ne pas se fier uniquement aux rôles, utiliser les permissions
- ❌ Ne pas oublier le filtrage multi-tenant (EcoleId)
- ❌ Ne pas hardcoder les permissions dans le code (utiliser des constantes)

---

## 🔍 Debugging et Logs

### Voir les claims JWT d'un utilisateur

```csharp
var claims = User.Claims.Select(c => new { c.Type, c.Value });
return Ok(claims);
```

### Logs utiles

```csharp
_logger.LogInformation("Utilisateur {UserId} avec rôle {Role} a tenté d'accéder à {Action}", 
    currentUser.UserId, 
    currentUser.Role, 
    HttpContext.Request.Path);
```

---

## 📞 Support

Si vous avez des questions ou besoin d'assistance :
- 📧 Email : support@ekelasi.com
- 📖 Documentation complète : `IMPLEMENTATION_RBAC_GUIDE_FINAL.md`

---

🎉 **Félicitations ! Votre système RBAC est maintenant opérationnel !** 🎉

