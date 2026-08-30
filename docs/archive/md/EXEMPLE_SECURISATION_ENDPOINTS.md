# 🔐 Exemples de Sécurisation des Endpoints avec RBAC

## 📌 Guide Pratique de Sécurisation

Ce document vous montre comment sécuriser vos endpoints avec le système RBAC implémenté.

---

## 🎯 Scénario 1 : Sécuriser le EcoleController

### ❌ AVANT (Non sécurisé)
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Tous authentifiés peuvent tout faire
public class EcoleController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Ecole>> CreateEcole(Ecole ecole)
    {
        // N'importe quel utilisateur authentifié peut créer une école !
        var created = await _ecoleRepository.CreateAsync(ecole);
        return CreatedAtAction(nameof(GetEcole), new { id = created.IdEcole }, created);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEcole(int id)
    {
        // N'importe quel utilisateur peut supprimer une école !
        await _ecoleRepository.DeleteAsync(id);
        return NoContent();
    }
}
```

### ✅ APRÈS (Sécurisé avec permissions)
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EcoleController : ControllerBase
{
    private readonly IEcoleRepository _ecoleRepository;
    private readonly ICurrentUserService _currentUserService;
    
    public EcoleController(
        IEcoleRepository ecoleRepository,
        ICurrentUserService currentUserService)
    {
        _ecoleRepository = ecoleRepository;
        _currentUserService = currentUserService;
    }
    
    // Seul le Super-Admin peut créer une école
    [HttpPost]
    [Permission("Ecole.Create")]
    public async Task<ActionResult<Ecole>> CreateEcole(Ecole ecole)
    {
        var created = await _ecoleRepository.CreateAsync(ecole);
        return CreatedAtAction(nameof(GetEcole), new { id = created.IdEcole }, created);
    }
    
    // Seul le Super-Admin peut supprimer une école
    [HttpDelete("{id}")]
    [Permission("Ecole.Delete")]
    public async Task<IActionResult> DeleteEcole(int id)
    {
        await _ecoleRepository.DeleteAsync(id);
        return NoContent();
    }
    
    // Tout le monde peut lire, mais filtré par école
    [HttpGet]
    [Permission("Ecole.ReadAll")]
    public async Task<ActionResult<IEnumerable<Ecole>>> GetEcoles()
    {
        var currentUser = _currentUserService.GetCurrentUser();
        
        // Super-Admin voit toutes les écoles
        if (currentUser.Role == UserRoles.SUPER_ADMIN)
        {
            return Ok(await _ecoleRepository.GetAllAsync());
        }
        
        // Les autres ne voient que leur école
        var ecole = await _ecoleRepository.GetByIdAsync(currentUser.EcoleId);
        return Ok(new[] { ecole });
    }
}
```

---

## 🎯 Scénario 2 : Sécuriser le PaiementController

### ✅ Implémentation Complète
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaiementController : ControllerBase
{
    private readonly IPaiementRepository _paiementRepository;
    private readonly ICurrentUserService _currentUserService;
    
    // Comptable et Secrétaire peuvent créer des paiements
    [HttpPost]
    [Permission("Paiement.Create")]
    public async Task<ActionResult<Paiement>> CreatePaiement(Paiement paiement)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        
        // Vérifier que le paiement concerne la bonne école
        if (currentUser.Role != UserRoles.SUPER_ADMIN)
        {
            var eleve = await _eleveRepository.GetByIdAsync(paiement.IdEleve);
            
            if (eleve.IdEcole != currentUser.EcoleId)
            {
                return Forbid("Vous ne pouvez créer que des paiements pour votre école");
            }
        }
        
        paiement.IdUtilisateurCreation = currentUser.UserId;
        var created = await _paiementRepository.CreateAsync(paiement);
        
        return CreatedAtAction(nameof(GetPaiement), new { id = created.IdPaiement }, created);
    }
    
    // Seul le Comptable peut valider un paiement
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
        
        // Filtrage multi-tenant
        if (currentUser.Role != UserRoles.SUPER_ADMIN)
        {
            var eleve = await _eleveRepository.GetByIdAsync(paiement.IdEleve);
            
            if (eleve.IdEcole != currentUser.EcoleId)
            {
                return Forbid("Vous ne pouvez valider que les paiements de votre école");
            }
        }
        
        paiement.EstValide = true;
        paiement.DateValidation = DateTime.Now;
        paiement.IdUtilisateurValidation = currentUser.UserId;
        
        await _paiementRepository.UpdateAsync(paiement);
        
        return Ok(new { message = "Paiement validé avec succès" });
    }
    
    // Les parents voient leurs paiements, les autres tous les paiements de leur école
    [HttpGet]
    [Permission("Paiement.ReadAll")]
    public async Task<ActionResult<IEnumerable<Paiement>>> GetPaiements()
    {
        var currentUser = _currentUserService.GetCurrentUser();
        
        // Super-Admin voit tout
        if (currentUser.Role == UserRoles.SUPER_ADMIN)
        {
            return Ok(await _paiementRepository.GetAllAsync());
        }
        
        // Parent voit uniquement les paiements de ses enfants
        if (currentUser.Role == UserRoles.PARENT)
        {
            var tuteur = await _tuteurRepository.GetByUtilisateurIdAsync(currentUser.UserId);
            return Ok(await _paiementRepository.GetByTuteurIdAsync(tuteur.IdTuteur));
        }
        
        // Autres rôles : paiements de leur école
        return Ok(await _paiementRepository.GetByEcoleIdAsync(currentUser.EcoleId));
    }
}
```

---

## 🎯 Scénario 3 : Sécuriser le NoteController

### ✅ Implémentation avec Logique Métier
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NoteController : ControllerBase
{
    private readonly INoteRepository _noteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEleveRepository _eleveRepository;
    
    // Seul l'enseignant peut créer des notes
    [HttpPost]
    [Permission("Note.Create")]
    public async Task<ActionResult<Note>> CreateNote(Note note)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        
        // Vérifier que la note concerne la bonne école
        if (currentUser.Role != UserRoles.SUPER_ADMIN)
        {
            var eleve = await _eleveRepository.GetByIdAsync(note.IdEleve);
            
            if (eleve.IdEcole != currentUser.EcoleId)
            {
                return Forbid("Vous ne pouvez créer des notes que pour votre école");
            }
        }
        
        note.IdProfesseur = currentUser.UserId;
        var created = await _noteRepository.CreateAsync(note);
        
        return CreatedAtAction(nameof(GetNote), new { id = created.IdNote }, created);
    }
    
    // Les élèves voient leurs notes, les parents celles de leurs enfants
    [HttpGet("eleve/{eleveId}")]
    public async Task<ActionResult<IEnumerable<Note>>> GetNotesByEleve(int eleveId)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        var eleve = await _eleveRepository.GetByIdAsync(eleveId);
        
        if (eleve == null)
        {
            return NotFound();
        }
        
        // Super-Admin et Directeur : accès total
        if (currentUser.Role == UserRoles.SUPER_ADMIN || 
            currentUser.Role == UserRoles.DIRECTEUR)
        {
            return Ok(await _noteRepository.GetByEleveIdAsync(eleveId));
        }
        
        // Élève : uniquement SES notes
        if (currentUser.Role == UserRoles.ELEVE)
        {
            var eleveUser = await _eleveRepository.GetByUtilisateurIdAsync(currentUser.UserId);
            
            if (eleveUser.IdEleve != eleveId)
            {
                return Forbid("Vous ne pouvez consulter que vos propres notes");
            }
            
            return Ok(await _noteRepository.GetByEleveIdAsync(eleveId));
        }
        
        // Parent : uniquement les notes de SES enfants
        if (currentUser.Role == UserRoles.PARENT)
        {
            var tuteur = await _tuteurRepository.GetByUtilisateurIdAsync(currentUser.UserId);
            var estSonEnfant = await _eleveRepository.IsEnfantDuTuteurAsync(eleveId, tuteur.IdTuteur);
            
            if (!estSonEnfant)
            {
                return Forbid("Vous ne pouvez consulter que les notes de vos enfants");
            }
            
            return Ok(await _noteRepository.GetByEleveIdAsync(eleveId));
        }
        
        // Enseignant : notes de son école
        if (eleve.IdEcole != currentUser.EcoleId)
        {
            return Forbid("Vous ne pouvez consulter que les notes de votre école");
        }
        
        return Ok(await _noteRepository.GetByEleveIdAsync(eleveId));
    }
    
    // Seul l'enseignant qui a créé la note peut la modifier
    [HttpPut("{id}")]
    [Permission("Note.Update")]
    public async Task<IActionResult> UpdateNote(int id, Note note)
    {
        var existingNote = await _noteRepository.GetByIdAsync(id);
        
        if (existingNote == null)
        {
            return NotFound();
        }
        
        var currentUser = _currentUserService.GetCurrentUser();
        
        // Super-Admin et Directeur peuvent tout modifier
        if (currentUser.Role != UserRoles.SUPER_ADMIN && 
            currentUser.Role != UserRoles.DIRECTEUR)
        {
            // Enseignant : uniquement SES notes
            if (existingNote.IdProfesseur != currentUser.UserId)
            {
                return Forbid("Vous ne pouvez modifier que vos propres notes");
            }
        }
        
        await _noteRepository.UpdateAsync(note);
        return NoContent();
    }
}
```

---

## 🎯 Scénario 4 : Endpoint Public (Sans Permission)

Certains endpoints peuvent être accessibles sans permission spécifique, mais avec authentification :

```csharp
[ApiController]
[Route("api/[controller]")]
public class PublicController : ControllerBase
{
    // Accessible sans authentification
    [HttpGet("info")]
    [AllowAnonymous]
    public IActionResult GetPublicInfo()
    {
        return Ok(new { 
            version = "1.0", 
            nom = "KelasiNaBiso API" 
        });
    }
    
    // Accessible avec authentification, mais sans permission spécifique
    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetMyProfile()
    {
        var currentUser = _currentUserService.GetCurrentUser();
        
        if (currentUser == null)
        {
            return Unauthorized();
        }
        
        var profile = await _utilisateurRepository.GetByIdAsync(currentUser.UserId);
        return Ok(profile);
    }
}
```

---

## 📋 Checklist de Sécurisation

Avant de déployer un endpoint, vérifiez :

### ✅ Questions à se poser
1. **Qui peut accéder ?** → `[Permission("...")]`
2. **Multi-tenant ?** → Filtrer par `currentUser.EcoleId`
3. **Ownership ?** → Vérifier `IdUtilisateur` ou `IdProfesseur`
4. **Données sensibles ?** → Permission spécifique + validation
5. **Super-Admin bypass ?** → Ajouter exception pour Super-Admin

### ✅ Pattern de Code
```csharp
[HttpPost/Put/Delete]
[Permission("Resource.Action")]
public async Task<ActionResult> SecureEndpoint(...)
{
    // 1. Récupérer l'utilisateur actuel
    var currentUser = _currentUserService.GetCurrentUser();
    
    // 2. Vérifications de base
    if (currentUser == null)
        return Unauthorized();
    
    // 3. Filtrage multi-tenant (sauf Super-Admin)
    if (currentUser.Role != UserRoles.SUPER_ADMIN)
    {
        // Vérifier EcoleId, Ownership, etc.
        if (resource.IdEcole != currentUser.EcoleId)
            return Forbid("Accès refusé");
    }
    
    // 4. Logique métier
    // ...
    
    return Ok(result);
}
```

---

## 🔍 Tester la Sécurité

### Test 1 : Sans token
```bash
curl -X GET http://localhost:5005/api/Paiement
# Attendu : 401 Unauthorized
```

### Test 2 : Avec token mais sans permission
```bash
# S'authentifier comme Élève
curl -X POST http://localhost:5005/api/Utilisateur/authentifier \
  -H "Content-Type: application/json" \
  -d '{"telephone": "+243...", "motDePasse": "..."}'

# Essayer de créer une école (Élève n'a pas cette permission)
curl -X POST http://localhost:5005/api/Ecole \
  -H "Authorization: Bearer {token_eleve}" \
  -H "Content-Type: application/json" \
  -d '{"nom": "Test", ...}'
# Attendu : 403 Forbidden
```

### Test 3 : Avec permission correcte
```bash
# S'authentifier comme Super-Admin
curl -X POST http://localhost:5005/api/Utilisateur/authentifier \
  -H "Content-Type: application/json" \
  -d '{"telephone": "+243999999999", "motDePasse": "Super-Admin"}'

# Créer une école (Super-Admin a toutes les permissions)
curl -X POST http://localhost:5005/api/Ecole \
  -H "Authorization: Bearer {token_superadmin}" \
  -H "Content-Type: application/json" \
  -d '{"nom": "Test", ...}'
# Attendu : 201 Created
```

---

## 🎨 Exemple Complet : AgentController Sécurisé

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AgentController : ControllerBase
{
    private readonly IAgentRepository _agentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AgentController> _logger;
    
    [HttpGet]
    [Permission("Agent.ReadAll")]
    public async Task<ActionResult<IEnumerable<Agent>>> GetAgents()
    {
        var currentUser = _currentUserService.GetCurrentUser();
        
        if (currentUser.Role == UserRoles.SUPER_ADMIN)
        {
            return Ok(await _agentRepository.GetAllAsync());
        }
        
        return Ok(await _agentRepository.GetByEcoleIdAsync(currentUser.EcoleId));
    }
    
    [HttpPost]
    [Permission("Agent.Create")]
    public async Task<ActionResult<Agent>> CreateAgent(Agent agent)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        
        // Forcer l'école de l'agent (sauf Super-Admin)
        if (currentUser.Role != UserRoles.SUPER_ADMIN)
        {
            agent.IdEcole = currentUser.EcoleId;
        }
        
        var created = await _agentRepository.CreateAsync(agent);
        
        _logger.LogInformation(
            "Agent créé : {AgentId} par utilisateur {UserId}", 
            created.IdAgent, 
            currentUser.UserId
        );
        
        return CreatedAtAction(nameof(GetAgent), new { id = created.IdAgent }, created);
    }
    
    [HttpDelete("{id}")]
    [Permission("Agent.Delete")]
    public async Task<IActionResult> DeleteAgent(int id)
    {
        var agent = await _agentRepository.GetByIdAsync(id);
        
        if (agent == null)
        {
            return NotFound();
        }
        
        var currentUser = _currentUserService.GetCurrentUser();
        
        // Empêcher la suppression d'agents d'autres écoles
        if (currentUser.Role != UserRoles.SUPER_ADMIN && 
            agent.IdEcole != currentUser.EcoleId)
        {
            return Forbid("Vous ne pouvez supprimer que les agents de votre école");
        }
        
        await _agentRepository.DeleteAsync(id);
        
        _logger.LogWarning(
            "Agent supprimé : {AgentId} par utilisateur {UserId}", 
            id, 
            currentUser.UserId
        );
        
        return NoContent();
    }
}
```

---

## 🎯 Conclusion

**Règles d'Or** :
1. ✅ Toujours utiliser `[Permission("...")]` sur les endpoints sensibles
2. ✅ Toujours filtrer par `EcoleId` (multi-tenant)
3. ✅ Toujours vérifier l'ownership (IdUtilisateur, IdProfesseur, etc.)
4. ✅ Toujours logger les actions sensibles
5. ✅ Toujours tester avec différents rôles

**Votre API est maintenant sécurisée ! 🔐**

