# 🎯 Plan d'Implémentation : Attribution Automatique des Permissions

> **Date** : 28 Octobre 2025  
> **Contexte** : Système RBAC hybride (Permissions par Rôle + Permissions personnalisées par Utilisateur)

---

## ✅ ÉTAPE 1 : Modèles de Données (COMPLÉTÉ)

### 1.1 Vérification des Modèles

- ✅ **Agent** : Pas de relation directe avec `Role` (seulement via `Utilisateur`)
- ✅ **Tuteur** : Pas de relation directe avec `Role` (seulement via `Utilisateur`)
- ✅ **Utilisateur** : Relation avec `Role` (`IdRole`)

### 1.2 Nouveau Modèle

- ✅ **UserPermission** :
  - `IdUserPermission` (PK)
  - `IdUtilisateur` (FK → Utilisateur)
  - `IdPermission` (FK → Permission)
  - `IsGranted` (bool) - true = ajout, false = retrait
  - `DateAttribution`
  - `DateExpiration` (optionnel)
  - `Commentaire` (optionnel)
  - `AttribueParIdUtilisateur` (optionnel)

- ✅ **Migration** : `AddUserPermissionsTable` appliquée avec succès

---

## 🔄 ÉTAPE 2 : Mise à Jour du PermissionService (EN COURS)

### 2.1 Nouvelles Méthodes à Ajouter dans IPermissionService

```csharp
// ═══════════════════════════════════════════════════════════════════
// GESTION DES PERMISSIONS PERSONNALISÉES PAR UTILISATEUR
// ═══════════════════════════════════════════════════════════════════

/// <summary>
/// Ajoute une permission personnalisée à un utilisateur (en plus de celles de son rôle)
/// </summary>
Task<bool> GrantUserPermissionAsync(int userId, int permissionId, int? grantedByUserId = null, DateTime? expiresAt = null, string? comment = null);

/// <summary>
/// Retire une permission personnalisée d'un utilisateur (override du rôle)
/// </summary>
Task<bool> DenyUserPermissionAsync(int userId, int permissionId, int? deniedByUserId = null, string? comment = null);

/// <summary>
/// Supprime une permission personnalisée (retour aux permissions du rôle)
/// </summary>
Task<bool> RemoveUserPermissionOverrideAsync(int userId, int permissionId);

/// <summary>
/// Récupère toutes les permissions personnalisées d'un utilisateur
/// </summary>
Task<IEnumerable<UserPermission>> GetUserCustomPermissionsAsync(int userId);

/// <summary>
/// Récupère toutes les permissions EFFECTIVES d'un utilisateur (Rôle + Custom)
/// Logique : Rôle + GrantedCustom - DeniedCustom
/// </summary>
Task<IEnumerable<string>> GetEffectiveUserPermissionsAsync(int userId);
```

### 2.2 Modification de la Logique Existante

**UserHasPermissionAsync** :
```csharp
public async Task<bool> UserHasPermissionAsync(int userId, string permissionName)
{
    // 1. Vérifier les permissions DENIED personnalisées (priorité haute)
    var deniedCustom = await _context.UserPermissions
        .Include(up => up.Permission)
        .Where(up => up.IdUtilisateur == userId 
                  && up.Permission.Nom == permissionName 
                  && !up.IsGranted 
                  && up.Permission.Statut)
        .FirstOrDefaultAsync();
    
    if (deniedCustom != null && deniedCustom.IsValid())
    {
        return false; // Permission explicitement retirée
    }
    
    // 2. Vérifier les permissions GRANTED personnalisées
    var grantedCustom = await _context.UserPermissions
        .Include(up => up.Permission)
        .Where(up => up.IdUtilisateur == userId 
                  && up.Permission.Nom == permissionName 
                  && up.IsGranted 
                  && up.Permission.Statut)
        .FirstOrDefaultAsync();
    
    if (grantedCustom != null && grantedCustom.IsValid())
    {
        return true; // Permission personnalisée accordée
    }
    
    // 3. Vérifier les permissions du rôle (comportement par défaut)
    var user = await _context.Utilisateurs
        .Include(u => u.Role)
            .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
        .FirstOrDefaultAsync(u => u.IdUtilisateur == userId);
    
    if (user == null || user.Role == null)
    {
        return false;
    }
    
    return user.Role.RolePermissions
        .Any(rp => rp.Permission.Nom == permissionName && rp.Permission.Statut);
}
```

---

## 📝 ÉTAPE 3 : Compléter le PermissionSeeder

### 3.1 Définir les Permissions pour TOUS les Rôles

**Super-Admin** : TOUTES les permissions (déjà fait ✅)

**Admin** : Gestion complète de son école
```csharp
var adminPermissions = new[]
{
    // Écoles
    "Ecole.Read", "Ecole.Update",
    
    // Élèves
    "Eleve.Create", "Eleve.Read", "Eleve.Update", "Eleve.Delete",
    
    // Agents
    "Agent.Create", "Agent.Read", "Agent.Update", "Agent.Delete",
    
    // Classes
    "Classe.Create", "Classe.Read", "Classe.Update", "Classe.Delete",
    
    // Paiements
    "Paiement.Create", "Paiement.Read", "Paiement.Update", "Paiement.Validate",
    
    // Notes
    "Note.Read", "Note.Update",
    
    // Utilisateurs
    "Utilisateur.Create", "Utilisateur.Read", "Utilisateur.Update",
    
    // Dashboard
    "Dashboard.ViewSchool"
};
```

**Directeur** : Gestion pédagogique
```csharp
var directeurPermissions = new[]
{
    // Élèves
    "Eleve.Create", "Eleve.Read", "Eleve.Update",
    
    // Agents (limité)
    "Agent.Read",
    
    // Classes
    "Classe.Create", "Classe.Read", "Classe.Update",
    
    // Cours et Affectations
    "Cours.Create", "Cours.Read", "Cours.Update",
    "AffectationCours.Create", "AffectationCours.Read", "AffectationCours.Update",
    
    // Notes
    "Note.Create", "Note.Read", "Note.Update", "Note.Delete",
    
    // Présences
    "Presence.Read",
    
    // Dashboard
    "Dashboard.ViewSchool"
};
```

**Enseignant** : Gestion des notes et présences
```csharp
var enseignantPermissions = new[]
{
    // Élèves (lecture seule)
    "Eleve.Read",
    
    // Notes (pour SES élèves uniquement)
    "Note.Create", "Note.Read", "Note.Update",
    
    // Présences (pour SES élèves uniquement)
    "Presence.Create", "Presence.Read", "Presence.Update",
    
    // Cours (lecture seule)
    "Cours.Read"
};
```

**Comptable** : Gestion financière
```csharp
var comptablePermissions = new[]
{
    // Élèves (lecture seule)
    "Eleve.Read",
    
    // Paiements
    "Paiement.Create", "Paiement.Read", "Paiement.Update", "Paiement.Validate",
    
    // Frais
    "Frais.Read",
    
    // Dashboard financier
    "Dashboard.ViewFinance"
};
```

**Parent** : Consultation
```csharp
var parentPermissions = new[]
{
    // Élèves (SES enfants uniquement)
    "Eleve.ReadOwn",
    
    // Notes (SES enfants uniquement)
    "Note.ReadOwn",
    
    // Paiements (SES enfants uniquement)
    "Paiement.ReadOwn",
    
    // Présences (SES enfants uniquement)
    "Presence.ReadOwn"
};
```

**Élève** : Consultation personnelle
```csharp
var elevePermissions = new[]
{
    // Ses propres données
    "Eleve.ReadSelf",
    "Note.ReadSelf",
    "Presence.ReadSelf",
    "Paiement.ReadSelf"
};
```

### 3.2 Implémenter la Méthode d'Attribution

```csharp
private static async Task AssignPermissionsToRoleAsync(
    KelasiNaBisoDbContext context, 
    string roleName, 
    string[] permissionNames)
{
    var role = await context.Roles.FirstOrDefaultAsync(r => r.Nom == roleName);
    if (role == null)
    {
        Console.WriteLine($"❌ Rôle '{roleName}' non trouvé");
        return;
    }
    
    int count = 0;
    foreach (var permName in permissionNames)
    {
        var permission = await context.Permissions
            .FirstOrDefaultAsync(p => p.Nom == permName);
        
        if (permission == null)
        {
            Console.WriteLine($"⚠️  Permission '{permName}' non trouvée");
            continue;
        }
        
        // Vérifier si l'association existe déjà
        var exists = await context.RolePermissions
            .AnyAsync(rp => rp.IdRole == role.IdRole && rp.IdPermission == permission.IdPermission);
        
        if (exists)
        {
            continue; // Déjà assignée
        }
        
        context.RolePermissions.Add(new RolePermission
        {
            IdRole = role.IdRole,
            IdPermission = permission.IdPermission,
            DateAttribution = DateTime.UtcNow
        });
        
        count++;
    }
    
    await context.SaveChangesAsync();
    Console.WriteLine($"✅ {count} permission(s) assignée(s) au rôle '{roleName}'");
}
```

---

## 🤖 ÉTAPE 4 : Logique d'Attribution Automatique lors de Création

### 4.1 Création d'un Agent

**Fichier** : `Controllers/AgentController.cs` ou `Services/AgentService.cs`

```csharp
[HttpPost]
public async Task<ActionResult<Agent>> CreateAgent(AgentDto agentDto)
{
    // 1. Créer l'agent
    var agent = new Agent
    {
        Nom = agentDto.Nom,
        Postnom = agentDto.Postnom,
        Prenom = agentDto.Prenom,
        Fonction = agentDto.Fonction,
        IdEcole = agentDto.IdEcole,
        // ... autres propriétés
    };
    
    agent = await _agentRepository.CreateAsync(agent);
    
    // 2. Déterminer le rôle selon la fonction
    string roleName = DetermineRoleFromFonction(agentDto.Fonction);
    var role = await _context.Roles.FirstOrDefaultAsync(r => r.Nom == roleName);
    
    if (role == null)
    {
        return BadRequest($"Rôle '{roleName}' introuvable");
    }
    
    // 3. Créer un compte utilisateur associé
    var utilisateur = new Utilisateur
    {
        IdAgent = agent.IdAgent,  // ✨ Lien avec l'agent
        NomUtilisateur = agent.Nom,
        PrenomUtilisateur = agent.Prenom,
        Email = agent.EmailAgent,
        Telephone = agent.TelephoneAgent,
        Genre = agent.Genre,
        IdRole = role.IdRole,  // ✅ Rôle attribué automatiquement
        IdEcole = agent.IdEcole,
        MotDePasseHash = HashPassword(GenerateTemporaryPassword()),
        DoitChangerMotDePasse = true,  // Force le changement au 1er login
        Statut = true
    };
    
    await _utilisateurRepository.CreateAsync(utilisateur);
    
    _logger.LogInformation($"✅ Utilisateur créé pour l'agent {agent.IdAgent} avec le rôle '{roleName}'");
    
    return CreatedAtAction(nameof(GetAgent), new { id = agent.IdAgent }, agent);
}

private string DetermineRoleFromFonction(string fonction)
{
    return fonction switch
    {
        "Directeur" => "Directeur",
        "Enseignant" or "Professeur" => "Enseignant",
        "Comptable" => "Comptable",
        _ => "Agent" // Rôle générique par défaut
    };
}
```

### 4.2 Création d'un Tuteur (lors de l'inscription d'un élève)

**Fichier** : `Controllers/EleveController.cs` ou `Services/InscriptionService.cs`

```csharp
[HttpPost("inscrire")]
public async Task<ActionResult> InscrireEleve(InscriptionDto inscriptionDto)
{
    // 1. Créer ou récupérer le tuteur
    var tuteur = await _tuteurRepository.GetByEmailAsync(inscriptionDto.EmailTuteur);
    
    if (tuteur == null)
    {
        // Créer le tuteur
        tuteur = new Tuteur
        {
            NomComplet = inscriptionDto.NomCompletTuteur,
            Email = inscriptionDto.EmailTuteur,
            Telephone = inscriptionDto.TelephoneTuteur,
            Genre = inscriptionDto.GenreTuteur,
            IdEcole = inscriptionDto.IdEcole,
            Statut = true
        };
        
        tuteur = await _tuteurRepository.CreateAsync(tuteur);
        
        // 2. Créer un compte utilisateur pour le tuteur (Parent)
        var roleParent = await _context.Roles.FirstOrDefaultAsync(r => r.Nom == "Parent");
        
        if (roleParent != null)
        {
            var utilisateur = new Utilisateur
            {
                IdTuteur = tuteur.IdTuteur,  // ✨ Lien avec le tuteur
                NomUtilisateur = tuteur.NomComplet,
                Email = tuteur.Email,
                Telephone = tuteur.Telephone,
                Genre = tuteur.Genre,
                IdRole = roleParent.IdRole,  // ✅ Rôle "Parent" attribué
                IdEcole = tuteur.IdEcole.Value,
                MotDePasseHash = HashPassword(GenerateTemporaryPassword()),
                DoitChangerMotDePasse = true,
                Statut = true
            };
            
            await _utilisateurRepository.CreateAsync(utilisateur);
            
            _logger.LogInformation($"✅ Compte Parent créé pour le tuteur {tuteur.IdTuteur}");
        }
    }
    
    // 3. Créer l'élève
    var eleve = new Eleve
    {
        Nom = inscriptionDto.NomEleve,
        Postnom = inscriptionDto.PostnomEleve,
        Prenom = inscriptionDto.PrenomEleve,
        IdTuteur = tuteur.IdTuteur,
        IdEcole = inscriptionDto.IdEcole,
        // ... autres propriétés
    };
    
    eleve = await _eleveRepository.CreateAsync(eleve);
    
    // 4. (Optionnel) Créer un compte utilisateur pour l'élève
    var roleEleve = await _context.Roles.FirstOrDefaultAsync(r => r.Nom == "Eleve");
    
    if (roleEleve != null)
    {
        var utilisateurEleve = new Utilisateur
        {
            IdEleve = eleve.IdEleve,  // ✨ Lien avec l'élève
            NomUtilisateur = eleve.Nom,
            PrenomUtilisateur = eleve.Prenom,
            Email = $"{eleve.Matricule}@student.school.com",  // Email générique
            Genre = eleve.Genre,
            IdRole = roleEleve.IdRole,  // ✅ Rôle "Eleve" attribué
            IdEcole = eleve.IdEcole,
            MotDePasseHash = HashPassword(eleve.Matricule),  // Mot de passe = matricule
            DoitChangerMotDePasse = true,
            Statut = true
        };
        
        await _utilisateurRepository.CreateAsync(utilisateurEleve);
        
        _logger.LogInformation($"✅ Compte Élève créé pour {eleve.Matricule}");
    }
    
    return Ok(new { Message = "Inscription réussie", EleveId = eleve.IdEleve });
}
```

---

## 🧪 ÉTAPE 5 : Tests

### 5.1 Tests Unitaires

- Test : Création d'un Agent → Utilisateur créé avec bon rôle
- Test : Création d'un Tuteur → Utilisateur créé avec rôle "Parent"
- Test : Permission accordée personnalisée → Override du rôle
- Test : Permission retirée personnalisée → Même si le rôle l'a
- Test : Permissions expirées → Ne sont plus prises en compte

### 5.2 Tests d'Intégration

- Scénario 1 : Directeur crée un Agent Enseignant → Compte Enseignant créé
- Scénario 2 : Admin inscrit un élève → Comptes Parent + Élève créés
- Scénario 3 : Super-Admin accorde temporairement "Paiement.Validate" à un Directeur
- Scénario 4 : Super-Admin retire "Eleve.Delete" à un Admin spécifique

---

## 📚 ÉTAPE 6 : Documentation

### 6.1 Mettre à Jour les Documents Existants

- ✅ `ANALYSE_ATTRIBUTION_PERMISSIONS_AUTO.md` (créé)
- ✅ `RBAC_GUIDE_UTILISATION.md` (à mettre à jour)
- ✅ `EXEMPLE_SECURISATION_ENDPOINTS.md` (à mettre à jour)

### 6.2 Créer Nouveaux Documents

- Guide d'utilisation des permissions personnalisées
- Matrice complète des permissions par rôle
- Guide de dépannage RBAC

---

## 🎯 Priorités Immédiates

1. ✅ **FAIT** : Modèles de données (Agent, Tuteur, UserPermission)
2. ✅ **FAIT** : Migration et création table UserPermissions
3. **EN COURS** : Mise à jour du PermissionService (méthodes hybrides)
4. **À FAIRE** : Compléter PermissionSeeder avec tous les rôles
5. **À FAIRE** : Implémenter logique création Agent → Utilisateur
6. **À FAIRE** : Implémenter logique création Tuteur → Utilisateur
7. **À FAIRE** : Tests et validation

---

**Prochaine action** : Compléter l'implémentation du PermissionService avec les permissions hybrides

