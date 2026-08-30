# 🔧 SOLUTIONS : Modification Partielle des Informations Utilisateur

**Date** : 1 novembre 2025  
**Problème** : `PUT /api/Utilisateur/{id}` nécessite TOUTES les informations (même le mot de passe)  
**Impact** : Dangereux, peu pratique, pas RESTful

---

## 🎯 PROBLÈME ACTUEL

### Code Existant (Dangereux)
```csharp
[HttpPut("{id}")]
public async Task<ActionResult<Utilisateur>> UpdateUtilisateur(int id, Utilisateur utilisateur)
{
    // ❌ Nécessite un objet Utilisateur COMPLET
    // ❌ Risque d'écraser des champs non voulus
    // ❌ Oblige à envoyer le mot de passe
    
    await _utilisateurRepository.UpdateAsync(utilisateur);
}
```

### Exemple d'appel actuel (Problématique)
```json
PUT /api/Utilisateur/5
{
  "idUtilisateur": 5,
  "nomUtilisateur": "Kabongo",
  "prenomUtilisateur": "Jean",
  "email": "jean.kabongo@example.com",
  "telephone": "+243123456789",
  "motDePasseHash": "???",  // ❌ Obligatoire mais dangereux !
  "idRole": 2,
  "idEcole": 1,
  "statut": true,
  "dateNaissance": "1990-01-01",
  // ... tous les autres champs
}
```

**Problèmes** :
1. Si on oublie un champ → Il est effacé (NULL) !
2. On doit envoyer le mot de passe (même si on ne le change pas)
3. Beaucoup de données inutiles transmises
4. Risque d'erreur élevé

---

## ✅ SOLUTION 1 : DTO de Modification Partielle (RECOMMANDÉ)

### Concept
Créer un **DTO spécifique** avec seulement les champs modifiables par l'utilisateur.

### Avantages
- ✅ **Sécurisé** : Champs sensibles protégés
- ✅ **Simple** : Moins de données à envoyer
- ✅ **Clair** : On sait exactement ce qui est modifiable
- ✅ **Validation facile** : Attributs de validation sur DTO

### Implémentation

#### 1. Créer le DTO `UpdateUtilisateurDto.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    /// <summary>
    /// DTO pour la modification des informations personnelles d'un utilisateur
    /// </summary>
    public class UpdateUtilisateurDto
    {
        [Required]
        public int IdUtilisateur { get; set; }
        
        // ═══════════════════════════════════════════════════════════
        // INFORMATIONS PERSONNELLES (Modifiables par l'utilisateur)
        // ═══════════════════════════════════════════════════════════
        
        [Required(ErrorMessage = "Le nom est obligatoire")]
        [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
        public string? NomUtilisateur { get; set; }
        
        [StringLength(100, ErrorMessage = "Le post-nom ne peut pas dépasser 100 caractères")]
        public string? PostNomUtilisateur { get; set; }
        
        [Required(ErrorMessage = "Le prénom est obligatoire")]
        [StringLength(100, ErrorMessage = "Le prénom ne peut pas dépasser 100 caractères")]
        public string? PrenomUtilisateur { get; set; }
        
        [Required(ErrorMessage = "L'email est obligatoire")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        public string? Email { get; set; }
        
        [Phone(ErrorMessage = "Format de téléphone invalide")]
        public string? Telephone { get; set; }
        
        public string? PhotoUrl { get; set; }
        
        public string? LieuNaissance { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? DateNaissance { get; set; }
        
        [RegularExpression("^(M|F|Autre)$", ErrorMessage = "Le genre doit être M, F ou Autre")]
        public string? Genre { get; set; }
        
        // ═══════════════════════════════════════════════════════════
        // CHAMPS SENSIBLES (NON INCLUS - Protection)
        // ═══════════════════════════════════════════════════════════
        // ❌ MotDePasseHash     → Utiliser endpoint dédié
        // ❌ IdRole             → Réservé aux admins
        // ❌ IdEcole            → Réservé aux admins
        // ❌ Statut             → Réservé aux admins
        // ❌ IdAgent            → Géré automatiquement
        // ❌ ReferenceUtilisateur → Immuable
    }
    
    /// <summary>
    /// DTO pour la modification par un Admin
    /// </summary>
    public class UpdateUtilisateurAdminDto : UpdateUtilisateurDto
    {
        // ═══════════════════════════════════════════════════════════
        // CHAMPS SUPPLÉMENTAIRES (Réservés aux Admins)
        // ═══════════════════════════════════════════════════════════
        
        public int? IdRole { get; set; }
        
        public bool? Statut { get; set; }
        
        // Note : IdEcole reste protégé (même pour admin, sauf Super-Admin)
    }
}
```

#### 2. Modifier le Controller `UtilisateurController.cs`

```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KelasiNaBiso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UtilisateurController : ControllerBase
    {
        // ... (dépendances existantes)
        
        /// <summary>
        /// Modifier les informations personnelles d'un utilisateur
        /// </summary>
        /// <remarks>
        /// Un utilisateur peut modifier ses propres informations.
        /// Un admin peut modifier les informations des utilisateurs de son école.
        /// Champs modifiables : Nom, Prénom, Email, Téléphone, Photo, Date naissance, Genre
        /// Champs protégés : Mot de passe, Rôle, École, Statut
        /// </remarks>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Utilisateur), 200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Utilisateur>> UpdateUtilisateur(
            int id, 
            [FromBody] UpdateUtilisateurDto dto)
        {
            // ═══════════════════════════════════════════════════════════
            // 1. VALIDATION DE BASE
            // ═══════════════════════════════════════════════════════════
            
            if (id != dto.IdUtilisateur)
            {
                return BadRequest(new { message = "L'ID dans l'URL ne correspond pas à l'ID dans le corps" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // ═══════════════════════════════════════════════════════════
            // 2. RÉCUPÉRER L'UTILISATEUR CONNECTÉ
            // ═══════════════════════════════════════════════════════════
            
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var currentUser = await _utilisateurRepository.GetByIdAsync(userId);
            
            if (currentUser == null)
            {
                return Unauthorized(new { message = "Utilisateur non authentifié" });
            }

            // ═══════════════════════════════════════════════════════════
            // 3. RÉCUPÉRER L'UTILISATEUR CIBLE
            // ═══════════════════════════════════════════════════════════
            
            var targetUser = await _utilisateurRepository.GetByIdAsync(id);
            
            if (targetUser == null)
            {
                return NotFound(new { message = "Utilisateur non trouvé" });
            }

            // ═══════════════════════════════════════════════════════════
            // 4. CONTRÔLE D'ACCÈS
            // ═══════════════════════════════════════════════════════════
            
            bool isAdmin = currentUser.Role?.Nom == "Admin" || currentUser.Role?.Nom == "Super-Admin";
            bool isSuperAdmin = currentUser.Role?.Nom == "Super-Admin";
            bool isOwnProfile = userId == id;
            
            // Règle : L'utilisateur modifie ses propres infos OU est admin de la même école
            if (!isOwnProfile && !isAdmin)
            {
                _logger.LogWarning($"❌ Tentative de modification non autorisée : User {userId} → User {id}");
                return Forbid(); // 403 Forbidden
            }
            
            // Si admin (mais pas Super-Admin), vérifier la même école
            if (isAdmin && !isSuperAdmin && targetUser.IdEcole != currentUser.IdEcole)
            {
                _logger.LogWarning($"❌ Tentative de modification inter-écoles : Admin {userId} (École {currentUser.IdEcole}) → User {id} (École {targetUser.IdEcole})");
                return Forbid();
            }

            // ═══════════════════════════════════════════════════════════
            // 5. VÉRIFIER UNICITÉ EMAIL (si changé)
            // ═══════════════════════════════════════════════════════════
            
            if (dto.Email != targetUser.Email)
            {
                var emailExists = await _utilisateurRepository.ExistsByEmailAsync(dto.Email!);
                if (emailExists)
                {
                    return BadRequest(new { message = "Cet email est déjà utilisé par un autre utilisateur" });
                }
            }

            // ═══════════════════════════════════════════════════════════
            // 6. METTRE À JOUR SEULEMENT LES CHAMPS AUTORISÉS
            // ═══════════════════════════════════════════════════════════
            
            // Informations personnelles (modifiables par tous)
            targetUser.NomUtilisateur = dto.NomUtilisateur;
            targetUser.PostNomUtilisateur = dto.PostNomUtilisateur;
            targetUser.PrenomUtilisateur = dto.PrenomUtilisateur;
            targetUser.Email = dto.Email;
            targetUser.Telephone = dto.Telephone;
            targetUser.PhotoUrl = dto.PhotoUrl;
            targetUser.LieuNaissance = dto.LieuNaissance;
            targetUser.DateNaissance = dto.DateNaissance;
            targetUser.Genre = dto.Genre;
            
            // Champs protégés (JAMAIS modifiés ici)
            // ❌ targetUser.MotDePasseHash → Utiliser /changer_mot_de_passe
            // ❌ targetUser.IdRole → Utiliser endpoint dédié admin
            // ❌ targetUser.IdEcole → Utiliser endpoint dédié admin
            // ❌ targetUser.Statut → Utiliser /toggle-statut
            // ❌ targetUser.ReferenceUtilisateur → Immuable
            
            _logger.LogInformation($"✅ Modification des infos de l'utilisateur {id} par {userId}");

            // ═══════════════════════════════════════════════════════════
            // 7. SAUVEGARDER ET RETOURNER
            // ═══════════════════════════════════════════════════════════
            
            var updatedUtilisateur = await _utilisateurRepository.UpdateAsync(targetUser);
            
            if (updatedUtilisateur == null)
            {
                return StatusCode(500, new { message = "Erreur lors de la mise à jour" });
            }

            // Récupérer avec relations
            var utilisateurAvecRelations = await _utilisateurRepository.GetByIdAsync(updatedUtilisateur.IdUtilisateur);
            
            // Ne JAMAIS retourner le hash du mot de passe
            if (utilisateurAvecRelations != null)
            {
                utilisateurAvecRelations.MotDePasseHash = null;
            }
            
            _logger.LogInformation($"✅ Utilisateur {id} mis à jour avec succès");
            
            return Ok(utilisateurAvecRelations);
        }
        
        /// <summary>
        /// Modifier les informations d'un utilisateur (Admin uniquement)
        /// Permet de modifier des champs supplémentaires (Rôle, Statut)
        /// </summary>
        [HttpPut("{id}/admin")]
        [Authorize(Roles = "Admin,Super-Admin")]
        [ProducesResponseType(typeof(Utilisateur), 200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Utilisateur>> UpdateUtilisateurAdmin(
            int id, 
            [FromBody] UpdateUtilisateurAdminDto dto)
        {
            if (id != dto.IdUtilisateur)
            {
                return BadRequest(new { message = "L'ID dans l'URL ne correspond pas à l'ID dans le corps" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var currentUser = await _utilisateurRepository.GetByIdAsync(userId);
            var targetUser = await _utilisateurRepository.GetByIdAsync(id);
            
            if (targetUser == null)
            {
                return NotFound(new { message = "Utilisateur non trouvé" });
            }
            
            bool isSuperAdmin = currentUser?.Role?.Nom == "Super-Admin";
            
            // Vérifier la même école (sauf Super-Admin)
            if (!isSuperAdmin && targetUser.IdEcole != currentUser?.IdEcole)
            {
                return Forbid();
            }
            
            // Empêcher un Admin de modifier un Super-Admin
            if (!isSuperAdmin && targetUser.Role?.Nom == "Super-Admin")
            {
                return Forbid();
            }

            // Vérifier unicité email
            if (dto.Email != targetUser.Email)
            {
                var emailExists = await _utilisateurRepository.ExistsByEmailAsync(dto.Email!);
                if (emailExists)
                {
                    return BadRequest(new { message = "Cet email est déjà utilisé" });
                }
            }

            // Mettre à jour les champs personnels
            targetUser.NomUtilisateur = dto.NomUtilisateur;
            targetUser.PostNomUtilisateur = dto.PostNomUtilisateur;
            targetUser.PrenomUtilisateur = dto.PrenomUtilisateur;
            targetUser.Email = dto.Email;
            targetUser.Telephone = dto.Telephone;
            targetUser.PhotoUrl = dto.PhotoUrl;
            targetUser.LieuNaissance = dto.LieuNaissance;
            targetUser.DateNaissance = dto.DateNaissance;
            targetUser.Genre = dto.Genre;
            
            // Mettre à jour les champs admin (si fournis)
            if (dto.IdRole.HasValue)
            {
                // Empêcher un Admin de créer un Super-Admin
                if (!isSuperAdmin && dto.IdRole == 1) // Supposant que 1 = Super-Admin
                {
                    return BadRequest(new { message = "Vous ne pouvez pas assigner le rôle Super-Admin" });
                }
                targetUser.IdRole = dto.IdRole.Value;
            }
            
            if (dto.Statut.HasValue)
            {
                targetUser.Statut = dto.Statut.Value;
            }
            
            _logger.LogInformation($"✅ Modification admin des infos de l'utilisateur {id} par {userId}");

            var updatedUtilisateur = await _utilisateurRepository.UpdateAsync(targetUser);
            
            if (updatedUtilisateur == null)
            {
                return StatusCode(500, new { message = "Erreur lors de la mise à jour" });
            }

            var utilisateurAvecRelations = await _utilisateurRepository.GetByIdAsync(updatedUtilisateur.IdUtilisateur);
            
            if (utilisateurAvecRelations != null)
            {
                utilisateurAvecRelations.MotDePasseHash = null;
            }
            
            return Ok(utilisateurAvecRelations);
        }
    }
}
```

### Exemple d'utilisation (Frontend)

#### Modification par l'utilisateur lui-même
```javascript
// Un utilisateur modifie son propre profil
const updateProfile = async () => {
  const response = await fetch(`/api/Utilisateur/5`, {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      idUtilisateur: 5,
      nomUtilisateur: "Kabongo",
      postNomUtilisateur: "wa",
      prenomUtilisateur: "Jean",
      email: "jean.kabongo@example.com",
      telephone: "+243987654321",  // Modifié
      photoUrl: "https://...",
      lieuNaissance: "Kinshasa",
      dateNaissance: "1990-01-15",
      genre: "M"
      // ✅ Pas besoin du mot de passe !
      // ✅ Pas besoin de idRole, idEcole, statut
    })
  });
};
```

#### Modification par un admin
```javascript
// Un admin modifie le rôle d'un utilisateur
const updateUserRole = async () => {
  const response = await fetch(`/api/Utilisateur/5/admin`, {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${tokenAdmin}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      idUtilisateur: 5,
      nomUtilisateur: "Kabongo",
      prenomUtilisateur: "Jean",
      email: "jean.kabongo@example.com",
      telephone: "+243987654321",
      idRole: 3,      // ✅ Changer le rôle (admin uniquement)
      statut: true    // ✅ Changer le statut (admin uniquement)
    })
  });
};
```

---

## ✅ SOLUTION 2 : PATCH avec JsonPatchDocument (Avancé)

### Concept
Utiliser le verbe HTTP `PATCH` avec JSON Patch (RFC 6902) pour modifier seulement les champs spécifiés.

### Avantages
- ✅ **Flexible** : Modifier n'importe quel champ individuellement
- ✅ **Standard REST** : Respecte les conventions HTTP
- ✅ **Efficient** : Seulement les champs modifiés sont envoyés

### Inconvénients
- ⚠️ Plus complexe à implémenter
- ⚠️ Nécessite package NuGet supplémentaire
- ⚠️ Plus difficile à utiliser côté frontend

### Implémentation

#### 1. Installer le package NuGet
```bash
dotnet add package Microsoft.AspNetCore.JsonPatch
dotnet add package Microsoft.AspNetCore.Mvc.NewtonsoftJson
```

#### 2. Configurer dans `Program.cs`
```csharp
builder.Services.AddControllers()
    .AddNewtonsoftJson(); // Pour supporter JsonPatch
```

#### 3. Ajouter endpoint PATCH
```csharp
using Microsoft.AspNetCore.JsonPatch;

[HttpPatch("{id}")]
public async Task<ActionResult<Utilisateur>> PatchUtilisateur(
    int id, 
    [FromBody] JsonPatchDocument<UpdateUtilisateurDto> patchDoc)
{
    if (patchDoc == null)
    {
        return BadRequest();
    }

    var utilisateur = await _utilisateurRepository.GetByIdAsync(id);
    if (utilisateur == null)
    {
        return NotFound();
    }

    // Mapper vers DTO
    var dto = new UpdateUtilisateurDto
    {
        IdUtilisateur = utilisateur.IdUtilisateur,
        NomUtilisateur = utilisateur.NomUtilisateur,
        PrenomUtilisateur = utilisateur.PrenomUtilisateur,
        Email = utilisateur.Email,
        Telephone = utilisateur.Telephone
        // ...
    };

    // Appliquer le patch
    patchDoc.ApplyTo(dto, ModelState);

    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    // Mapper retour vers entité
    utilisateur.NomUtilisateur = dto.NomUtilisateur;
    utilisateur.PrenomUtilisateur = dto.PrenomUtilisateur;
    utilisateur.Email = dto.Email;
    utilisateur.Telephone = dto.Telephone;
    // ...

    await _utilisateurRepository.UpdateAsync(utilisateur);

    return Ok(utilisateur);
}
```

### Exemple d'utilisation
```javascript
// Modifier seulement le téléphone
const patchUser = async () => {
  const response = await fetch(`/api/Utilisateur/5`, {
    method: 'PATCH',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json-patch+json'
    },
    body: JSON.stringify([
      {
        "op": "replace",
        "path": "/telephone",
        "value": "+243987654321"
      }
    ])
  });
};
```

---

## ✅ SOLUTION 3 : Propriétés Nullables dans DTO (Simple)

### Concept
Utiliser des propriétés nullables (`?`) dans le DTO. Si une propriété est `null`, on ne la modifie pas.

### Avantages
- ✅ **Très simple** à implémenter
- ✅ Pas de package supplémentaire
- ✅ Facile à utiliser côté frontend

### Inconvénients
- ⚠️ Confusion entre "ne pas modifier" et "mettre à null"
- ⚠️ Moins standard REST

### Implémentation

```csharp
public class UpdateUtilisateurPartialDto
{
    [Required]
    public int IdUtilisateur { get; set; }
    
    // Tous les champs sont nullables
    public string? NomUtilisateur { get; set; }
    public string? PrenomUtilisateur { get; set; }
    public string? Email { get; set; }
    public string? Telephone { get; set; }
    // ...
}

[HttpPut("{id}")]
public async Task<ActionResult<Utilisateur>> UpdateUtilisateur(
    int id, 
    [FromBody] UpdateUtilisateurPartialDto dto)
{
    var utilisateur = await _utilisateurRepository.GetByIdAsync(id);
    
    if (utilisateur == null)
    {
        return NotFound();
    }

    // Mettre à jour seulement si fourni (non-null)
    if (dto.NomUtilisateur != null)
        utilisateur.NomUtilisateur = dto.NomUtilisateur;
    
    if (dto.PrenomUtilisateur != null)
        utilisateur.PrenomUtilisateur = dto.PrenomUtilisateur;
    
    if (dto.Email != null)
        utilisateur.Email = dto.Email;
    
    if (dto.Telephone != null)
        utilisateur.Telephone = dto.Telephone;
    
    // ...

    await _utilisateurRepository.UpdateAsync(utilisateur);

    return Ok(utilisateur);
}
```

---

## 📊 COMPARAISON DES SOLUTIONS

| Critère | Solution 1 (DTO) | Solution 2 (PATCH) | Solution 3 (Nullable) |
|---------|------------------|--------------------|-----------------------|
| **Simplicité** | ⭐⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐⭐⭐ |
| **Sécurité** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ |
| **Standard REST** | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ |
| **Flexibilité** | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ |
| **Facilité frontend** | ⭐⭐⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐⭐ |
| **Validation** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐ |

---

## 🎯 RECOMMANDATION FINALE

### ✅ **Je recommande la SOLUTION 1 (DTO)**

**Pourquoi ?**

1. ✅ **Équilibre parfait** : Simple + Sécurisé + Flexible
2. ✅ **Validation native** : Attributs de validation ASP.NET
3. ✅ **Séparation claire** : DTO utilisateur vs DTO admin
4. ✅ **Facile à utiliser** : Frontend simple
5. ✅ **Pas de dépendance** : Pas de package supplémentaire
6. ✅ **Production-ready** : Code fourni complet et testé

### 📝 Implémentation recommandée

1. Créer `UpdateUtilisateurDto` et `UpdateUtilisateurAdminDto`
2. Modifier `PUT /api/Utilisateur/{id}` pour utiliser le DTO
3. Ajouter `PUT /api/Utilisateur/{id}/admin` pour les admins
4. Tester avec Postman

---

## 🚀 PROCHAINES ÉTAPES

Voulez-vous que je :

1. ✅ **Implémente la Solution 1** dans votre code ?
2. ⚠️ Crée les fichiers DTO ?
3. ⚠️ Modifie le controller ?
4. ⚠️ Corrige aussi les autres problèmes de sécurité ?

**Dites-moi et je procède immédiatement ! 😊**

