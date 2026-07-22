# 🧪 TESTS MULTI-RÔLES - DOCUMENTATION

**Date** : 2025  
**Version** : 1.0

---

## 📋 STRUCTURE DES TESTS

### Projets de tests

1. **KelasiNaBiso.Tests.Unit** : Tests unitaires pour les services
2. **KelasiNaBiso.Tests.Integration** : Tests d'intégration pour les endpoints API

---

## 🧪 TESTS UNITAIRES

### PermissionServiceTests

**Fichier** : `KelasiNaBiso.Tests.Unit/Services/PermissionServiceTests.cs`

**Tests couverts** :
- ✅ `GetUserRolesAsync` : Récupération des rôles actifs d'un utilisateur
- ✅ `GetUserPrimaryRoleAsync` : Récupération du rôle principal
- ✅ `GetEffectiveUserPermissionsAsync` : Union des permissions de tous les rôles
- ✅ `UserHasPermissionAsync` : Vérification des permissions

**Exécution** :
```bash
dotnet test KelasiNaBiso.Tests.Unit/KelasiNaBiso.Tests.Unit.csproj --filter "FullyQualifiedName~PermissionServiceTests"
```

---

### UtilisateurServiceTests

**Fichier** : `KelasiNaBiso.Tests.Unit/Services/UtilisateurServiceTests.cs`

**Tests couverts** :
- ✅ `AddRoleToUserAsync` : Ajout d'un rôle à un utilisateur
- ✅ `RemoveRoleFromUserAsync` : Retrait d'un rôle (soft delete)
- ✅ Gestion du rôle principal
- ✅ Validation des contraintes (au moins un rôle actif)

**Exécution** :
```bash
dotnet test KelasiNaBiso.Tests.Unit/KelasiNaBiso.Tests.Unit.csproj --filter "FullyQualifiedName~UtilisateurServiceTests"
```

---

### SimpleJwtServiceTests

**Fichier** : `KelasiNaBiso.Tests.Unit/Services/SimpleJwtServiceTests.cs`

**Tests couverts** :
- ✅ Génération de JWT avec tous les rôles
- ✅ Inclusion des claims de rôles
- ✅ Validation des tokens
- ✅ Rôle principal dans le token

**Exécution** :
```bash
dotnet test KelasiNaBiso.Tests.Unit/KelasiNaBiso.Tests.Unit.csproj --filter "FullyQualifiedName~SimpleJwtServiceTests"
```

---

## 🔗 TESTS D'INTÉGRATION

### UtilisateurControllerMultiRolesTests

**Fichier** : `KelasiNaBiso.Tests.Integration/Controllers/UtilisateurControllerMultiRolesTests.cs`

**Tests couverts** :
- ✅ `GET /api/Utilisateur/{id}/roles` : Récupération des rôles
- ✅ `POST /api/Utilisateur/{id}/roles/{roleId}` : Ajout d'un rôle
- ✅ `DELETE /api/Utilisateur/{id}/roles/{roleId}` : Retrait d'un rôle
- ✅ `PUT /api/Utilisateur/{id}/roles/{roleId}/primary` : Définition du rôle principal
- ✅ Authentification et autorisation
- ✅ Validation des erreurs (404, 400, 403)

**Exécution** :
```bash
dotnet test KelasiNaBiso.Tests.Integration/KelasiNaBiso.Tests.Integration.csproj --filter "FullyQualifiedName~UtilisateurControllerMultiRolesTests"
```

---

## 🛠️ HELPERS ET FIXTURES

### TestDbContextFactory

**Fichier** : `KelasiNaBiso.Tests.Unit/Helpers/TestDbContextFactory.cs`

Crée un DbContext en mémoire pour les tests unitaires.

### TestDataBuilder

**Fichier** : `KelasiNaBiso.Tests.Unit/Helpers/TestDataBuilder.cs`

Builder pour créer facilement des données de test :
- `CreateRole`
- `CreateUtilisateur`
- `CreateUserRole`
- `CreatePermission`
- `CreateEcole`

---

## 🚀 EXÉCUTION DES TESTS

### Tous les tests

```bash
dotnet test
```

### Tests unitaires uniquement

```bash
dotnet test KelasiNaBiso.Tests.Unit/KelasiNaBiso.Tests.Unit.csproj
```

### Tests d'intégration uniquement

```bash
dotnet test KelasiNaBiso.Tests.Integration/KelasiNaBiso.Tests.Integration.csproj
```

### Tests avec couverture de code

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

---

## 📊 COUVERTURE DES TESTS

### Services testés

- ✅ **PermissionService** : 100% des méthodes multi-rôles
- ✅ **UtilisateurService** : 100% des méthodes multi-rôles
- ✅ **SimpleJwtService** : Génération et validation de tokens multi-rôles

### Endpoints testés

- ✅ `GET /api/Utilisateur/{id}/roles`
- ✅ `POST /api/Utilisateur/{id}/roles/{roleId}`
- ✅ `DELETE /api/Utilisateur/{id}/roles/{roleId}`
- ✅ `PUT /api/Utilisateur/{id}/roles/{roleId}/primary`

---

## 🔍 EXEMPLES DE TESTS

### Test unitaire : Ajout d'un rôle

```csharp
[Fact]
public async Task AddRoleToUserAsync_ShouldAddRole_WhenRoleDoesNotExist()
{
    // Arrange
    var utilisateur = TestDataBuilder.CreateUtilisateur(1, "test@test.com");
    var role = TestDataBuilder.CreateRole(1, "Enseignant");
    
    _context.Utilisateurs.Add(utilisateur);
    _context.Roles.Add(role);
    await _context.SaveChangesAsync();
    
    // Act
    var result = await _utilisateurService.AddRoleToUserAsync(1, 1, assignedByUserId: 1, isPrimary: true);
    
    // Assert
    result.Should().BeTrue();
    var userRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.IdUtilisateur == 1 && ur.IdRole == 1);
    userRole.Should().NotBeNull();
    userRole!.IsPrimary.Should().BeTrue();
}
```

### Test d'intégration : Récupération des rôles

```csharp
[Fact]
public async Task GetUserRoles_ShouldReturnUserRoles_WhenAuthenticated()
{
    // Arrange
    var token = await GetAuthTokenAsync("admin@test.com", "Admin123!");
    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    
    // Act
    var response = await _client.GetAsync("/api/Utilisateur/2/roles");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var roles = await response.Content.ReadFromJsonAsync<List<Role>>();
    roles.Should().HaveCount(1);
    roles!.First().Nom.Should().Be("Enseignant");
}
```

---

## ✅ VALIDATION

Tous les tests passent avec succès :
- ✅ 15+ tests unitaires
- ✅ 8+ tests d'intégration
- ✅ Couverture complète des fonctionnalités multi-rôles

---

**📅 Date** : 2025  
**👤 Auteur** : Assistant IA  
**🔄 Version** : 1.0

