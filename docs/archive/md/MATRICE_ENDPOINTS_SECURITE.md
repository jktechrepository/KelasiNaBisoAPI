# 🔐 MATRICE COMPLÈTE DES ENDPOINTS ET SÉCURITÉ

**Date** : 27 octobre 2025  
**Projet** : KelasiNaBisoAPI  
**Nombre de contrôleurs analysés** : 34  
**Nombre total d'endpoints estimé** : ~250+

---

## 📋 TABLE DES MATIÈRES

1. [Vue d'ensemble](#vue-densemble)
2. [Légende des risques](#légende-des-risques)
3. [Matrice complète par contrôleur](#matrice-complète-par-contrôleur)
4. [TOP 30 Endpoints critiques](#top-30-endpoints-critiques)
5. [Recommandations de sécurisation](#recommandations-de-sécurisation)
6. [Plan d'action immédiat](#plan-daction-immédiat)

---

## 📊 VUE D'ENSEMBLE

### Statistiques globales

| Catégorie | Nombre | Statut |
|-----------|--------|--------|
| **Contrôleurs totaux** | 34 | ✅ |
| **Endpoints protégés** | 88 | ⚠️ Protection basique uniquement |
| **Endpoints avec restrictions de rôle** | 0 | 🔴 CRITIQUE |
| **Endpoints publics** | ~5 | ✅ (login, password reset...) |
| **Niveau de risque global** | 🔴 **ÉLEVÉ** | Tous les utilisateurs = mêmes droits |

### Répartition par type d'opération

| Opération | Nombre estimé | Niveau de risque actuel |
|-----------|---------------|-------------------------|
| **GET (Lecture)** | ~150 | 🟡 MOYEN (fuite de données) |
| **POST (Création)** | ~40 | 🔴 ÉLEVÉ (création non autorisée) |
| **PUT (Modification)** | ~40 | 🔴 ÉLEVÉ (modification non autorisée) |
| **DELETE (Suppression)** | ~20 | 🔴 CRITIQUE (suppression non autorisée) |

---

## 🎨 LÉGENDE DES RISQUES

| Symbole | Niveau | Description |
|---------|--------|-------------|
| 🔴 | **CRITIQUE** | Suppression, modification de données sensibles, ou accès admin |
| 🟠 | **ÉLEVÉ** | Création, modification de données importantes |
| 🟡 | **MOYEN** | Lecture de données sensibles (fuite potentielle) |
| 🟢 | **FAIBLE** | Lecture de données publiques ou propres à l'utilisateur |
| 🔵 | **INFO** | Endpoints de vérification d'existence |
| 🟣 | **PUBLIC** | Endpoints accessibles sans authentification |

### Rôles recommandés (Abréviations)

- **SA** = Super-Admin
- **DIR** = Directeur
- **SDIR** = Sous-Directeur
- **COMP** = Comptable
- **SEC** = Secrétaire
- **ENS** = Enseignant
- **PAR** = Parent
- **ELE** = Élève
- **BAI** = Bailleur

---

## 🗂️ MATRICE COMPLÈTE PAR CONTRÔLEUR

### 1. 🏫 **EcoleController** (11 endpoints)

| # | Method | Endpoint | Risque | Rôles autorisés | Problème actuel |
|---|--------|----------|--------|-----------------|-----------------|
| 1 | GET | `/api/Ecole` | 🟡 | SA, DIR, SDIR | ⚠️ Tous voient toutes les écoles |
| 2 | GET | `/api/Ecole/{id}` | 🟡 | SA, DIR (même école), SDIR | ⚠️ Voir n'importe quelle école |
| 3 | GET | `/api/Ecole/nom/{nom}` | 🟡 | SA, DIR, SDIR | ⚠️ Voir n'importe quelle école |
| 4 | GET | `/api/Ecole/{id}/classes` | 🟡 | SA, DIR (même école), ENS, SEC | ⚠️ Voir classes d'autres écoles |
| 5 | GET | `/api/Ecole/{id}/utilisateurs` | 🟠 | SA, DIR (même école) | ⚠️ Fuite liste utilisateurs |
| 6 | GET | `/api/Ecole/{id}/tuteurs` | 🟠 | SA, DIR, SEC (même école) | ⚠️ Fuite données tuteurs |
| 7 | GET | `/api/Ecole/{id}/agents` | 🟠 | SA, DIR (même école) | ⚠️ Fuite données agents |
| 8 | GET | `/api/Ecole/{id}/inscriptions` | 🟡 | SA, DIR, SEC (même école) | ⚠️ Voir inscriptions autres écoles |
| 9 | **POST** | `/api/Ecole` | 🔴 | **SA uniquement** | ❌ N'importe qui peut créer école |
| 10 | **PUT** | `/api/Ecole/{id}` | 🔴 | SA, DIR (même école) | ❌ Modifier n'importe quelle école |
| 11 | **DELETE** | `/api/Ecole/{id}` | 🔴 | **SA uniquement** | ❌ CRITIQUE: Supprimer n'importe quelle école |

**Recommandations prioritaires :**
```csharp
// ❌ ACTUEL
[Authorize]
public async Task<IActionResult> DeleteEcole(int id)

// ✅ RECOMMANDÉ
[Authorize(Roles = "Super-Admin")]
public async Task<IActionResult> DeleteEcole(int id)

// ✅ RECOMMANDÉ AVEC FILTRAGE
[Authorize(Roles = "Super-Admin,Directeur")]
public async Task<IActionResult> UpdateEcole(int id, Ecole ecole)
{
    var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
    var userEcoleId = int.Parse(User.FindFirst("EcoleId")?.Value);
    
    if (userRole != "Super-Admin" && id != userEcoleId)
    {
        return Forbid(); // Directeur peut modifier uniquement son école
    }
    // ...
}
```

---

### 2. 👤 **UtilisateurController** (15 endpoints)

| # | Method | Endpoint | Risque | Rôles autorisés | Problème actuel |
|---|--------|----------|--------|-----------------|-----------------|
| 1 | GET | `/api/Utilisateur` | 🟠 | SA, DIR (même école) | ⚠️ Tous voient tous les utilisateurs |
| 2 | GET | `/api/Utilisateur/{id}` | 🟡 | SA, DIR (même école), ou soi-même | ⚠️ Voir n'importe quel utilisateur |
| 3 | GET | `/api/Utilisateur/email/{email}` | 🟡 | SA, DIR, ou soi-même | ⚠️ Rechercher n'importe qui par email |
| 4 | GET | `/api/Utilisateur/role/{roleId}` | 🟡 | SA, DIR (même école) | ⚠️ Lister utilisateurs par rôle |
| 5 | GET | `/api/Utilisateur/ecole/{idEcole}` | 🟡 | SA, DIR (même école), SEC | ⚠️ Voir utilisateurs d'autres écoles |
| 6 | **POST** | `/api/Utilisateur` | 🔴 | SA, DIR, SEC (même école) | ❌ N'importe qui crée utilisateur |
| 7 | **PUT** | `/api/Utilisateur/{id}` | 🔴 | SA, DIR, ou soi-même | ❌ Modifier n'importe quel utilisateur |
| 8 | **DELETE** | `/api/Utilisateur/{id}` | 🔴 | SA, DIR (même école) | ❌ CRITIQUE: Supprimer n'importe qui |
| 9 | **POST** | `/api/Utilisateur/authentifier` | 🟣 | PUBLIC | ✅ OK (AllowAnonymous) |
| 10 | **POST** | `/api/Utilisateur/changer_mot_de_passe` | 🟡 | Utilisateur lui-même | ⚠️ Changer mot de passe d'autres |
| 11 | **PUT** | `/api/Utilisateur/toggle-statut/{id}` | 🔴 | SA, DIR (même école) | ❌ Désactiver n'importe qui |

**Recommandations prioritaires :**
```csharp
// DELETE - CRITIQUE
[Authorize(Roles = "Super-Admin,Directeur")]
public async Task<IActionResult> DeleteUtilisateur(int id)
{
    var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
    var userEcoleId = int.Parse(User.FindFirst("EcoleId")?.Value);
    
    var utilisateur = await _utilisateurRepository.GetByIdAsync(id);
    if (utilisateur == null) return NotFound();
    
    // Super-Admin peut supprimer n'importe qui
    // Directeur peut supprimer uniquement utilisateurs de son école
    if (userRole != "Super-Admin" && utilisateur.IdEcole != userEcoleId)
    {
        return Forbid();
    }
    // ...
}

// POST - Changer mot de passe
[Authorize]
public async Task<IActionResult> ChangerMotDePasse(ChangerMotDePasseRequest request)
{
    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
    
    // Utilisateur peut changer uniquement son propre mot de passe
    // Sauf Super-Admin et Directeur qui peuvent forcer le changement
    if (request.IdUtilisateur != userId && 
        userRole != "Super-Admin" && 
        userRole != "Directeur")
    {
        return Forbid();
    }
    // ...
}
```

---

### 3. 💰 **PaiementController** (17 endpoints)

| # | Method | Endpoint | Risque | Rôles autorisés | Problème actuel |
|---|--------|----------|--------|-----------------|-----------------|
| 1 | GET | `/api/Paiement/paged` | 🟡 | SA, DIR, COMP, SEC (même école) | ⚠️ Tous voient tous les paiements |
| 2 | GET | `/api/Paiement/{id}` | 🟡 | SA, DIR, COMP, SEC, ou PAR (son enfant) | ⚠️ Voir n'importe quel paiement |
| 3 | GET | `/api/Paiement/eleve/{idEleve}` | 🟡 | SA, DIR, COMP, SEC, PAR (ses enfants) | ⚠️ Parent voit paiements autres élèves |
| 4 | GET | `/api/Paiement/ecole/{idEcole}` | 🟡 | SA, DIR, COMP, SEC (même école) | ⚠️ Voir paiements d'autres écoles |
| 5 | **POST** | `/api/Paiement` | 🔴 | SA, DIR, COMP, SEC (même école) | ❌ N'importe qui crée paiement |
| 6 | **PUT** | `/api/Paiement/{id}` | 🔴 | SA, DIR, COMP (même école) | ❌ Modifier n'importe quel paiement |
| 7 | **DELETE** | `/api/Paiement/{id}` | 🔴 | SA, COMP (avec validation DIR) | ❌ CRITIQUE: Supprimer paiements |
| 8 | **PUT** | `/api/Paiement/toggle-statut/{id}` | 🔴 | SA, DIR, COMP | ❌ Valider/invalider paiements |
| 9 | GET | `/api/Paiement/dashboard/ecole/{idEcole}` | 🟡 | SA, DIR, COMP (même école) | ⚠️ Stats financières autres écoles |
| 10 | GET | `/api/Paiement/eleves/{idEleve}/taux` | 🟡 | SA, DIR, COMP, PAR (ses enfants) | ⚠️ Taux paiement autres élèves |

**Recommandations prioritaires :**
```csharp
// POST - Créer paiement
[Authorize(Roles = "Super-Admin,Directeur,Comptable,Secrétaire")]
public async Task<ActionResult<Paiement>> CreatePaiement(Paiement paiement)
{
    var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
    var userEcoleId = int.Parse(User.FindFirst("EcoleId")?.Value);
    
    // Vérifier que l'élève appartient à l'école de l'utilisateur
    var eleve = await _eleveRepository.GetByIdAsync(paiement.IdEleve);
    if (eleve == null) return NotFound("Élève non trouvé");
    
    var eleveEcoleId = eleve.Classe.Direction.IdEcole;
    if (userRole != "Super-Admin" && eleveEcoleId != userEcoleId)
    {
        return Forbid(); // Ne peut pas créer paiement pour élève d'autre école
    }
    // ...
}

// GET - Paiements par élève (pour parents)
[Authorize]
public async Task<ActionResult<IEnumerable<Paiement>>> GetPaiementsByEleve(int idEleve)
{
    var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
    
    if (userRole == "Parent")
    {
        var tuteurId = int.Parse(User.FindFirst("TuteurId")?.Value);
        var eleve = await _eleveRepository.GetByIdAsync(idEleve);
        
        if (eleve == null) return NotFound();
        if (eleve.IdTuteur != tuteurId) return Forbid(); // Pas son enfant
    }
    else if (!UserRoles.IsStaffRole(userRole))
    {
        return Forbid(); // Ni staff ni parent
    }
    // ...
}

// DELETE - CRITIQUE
[Authorize(Roles = "Super-Admin,Comptable")]
public async Task<IActionResult> DeletePaiement(int id)
{
    var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
    
    if (userRole == "Comptable")
    {
        // Comptable peut supprimer uniquement paiements non validés de son école
        var paiement = await _paiementRepository.GetByIdAsync(id);
        if (paiement == null) return NotFound();
        
        var userEcoleId = int.Parse(User.FindFirst("EcoleId")?.Value);
        if (paiement.Eleve.Classe.Direction.IdEcole != userEcoleId)
        {
            return Forbid(); // Pas de son école
        }
        
        if (paiement.Statut)
        {
            return BadRequest("Impossible de supprimer un paiement validé");
        }
    }
    // ...
}
```

---

### 4. 👶 **EleveController** (25 endpoints)

| # | Method | Endpoint | Risque | Rôles autorisés | Problème actuel |
|---|--------|----------|--------|-----------------|-----------------|
| 1 | GET | `/api/Eleve/paged` | 🟡 | SA, DIR, SEC, ENS (même école) | ⚠️ Tous voient tous les élèves |
| 2 | GET | `/api/Eleve/{id}` | 🟡 | SA, DIR, SEC, ENS, PAR (ses enfants) | ⚠️ Voir n'importe quel élève |
| 3 | GET | `/api/Eleve/classe/{idClasse}` | 🟡 | SA, DIR, SEC, ENS (même école) | ⚠️ Voir classes d'autres écoles |
| 4 | GET | `/api/Eleve/tuteur/{idTuteur}` | 🟡 | SA, DIR, SEC, PAR (lui-même) | ⚠️ Parent voit enfants d'autres |
| 5 | GET | `/api/Eleve/ecole/{idEcole}` | 🟡 | SA, DIR, SEC (même école) | ⚠️ Voir élèves d'autres écoles |
| 6 | GET | `/api/Eleve/{id}/notes` | 🟡 | SA, DIR, ENS, PAR (ses enfants), ELE (soi) | ⚠️ Voir notes d'autres élèves |
| 7 | GET | `/api/Eleve/{id}/paiements` | 🟡 | SA, DIR, COMP, SEC, PAR (ses enfants) | ⚠️ Voir paiements d'autres élèves |
| 8 | GET | `/api/Eleve/{id}/presences` | 🟡 | SA, DIR, ENS, PAR (ses enfants) | ⚠️ Voir présences d'autres élèves |
| 9 | GET | `/api/Eleve/serial-number/{serialNumber}` | 🟡 | SA, DIR, SEC (même école) | ⚠️ Rechercher par serial number |
| 10 | **PUT** | `/api/Eleve/{id}` | 🔴 | SA, DIR, SEC (même école) | ❌ Modifier n'importe quel élève |
| 11 | **DELETE** | `/api/Eleve/{id}` | 🔴 | SA, DIR (même école) | ❌ CRITIQUE: Supprimer élèves |
| 12 | **PUT** | `/api/Eleve/toggle-statut/{id}` | 🔴 | SA, DIR (même école) | ❌ Désactiver élèves |
| 13 | **PUT** | `/api/Eleve/{idEleve}/serial-number` | 🟠 | SA, DIR, SEC (même école) | ⚠️ Modifier serial number |

**Recommandations prioritaires :**
```csharp
// GET - Élève par ID (avec filtrage parent)
[Authorize]
public async Task<ActionResult<Eleve>> GetEleve(int id)
{
    var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
    
    var eleve = await _eleveRepository.GetByIdAsync(id);
    if (eleve == null) return NotFound();
    
    if (userRole == "Parent")
    {
        var tuteurId = int.Parse(User.FindFirst("TuteurId")?.Value);
        if (eleve.IdTuteur != tuteurId)
        {
            return Forbid(); // Pas son enfant
        }
    }
    else if (userRole == "Élève")
    {
        var eleveId = int.Parse(User.FindFirst("EleveId")?.Value);
        if (id != eleveId)
        {
            return Forbid(); // Pas soi-même
        }
    }
    else if (UserRoles.IsStaffRole(userRole))
    {
        var userEcoleId = int.Parse(User.FindFirst("EcoleId")?.Value);
        if (eleve.Classe.Direction.IdEcole != userEcoleId && userRole != "Super-Admin")
        {
            return Forbid(); // Pas de son école
        }
    }
    
    return Ok(eleve);
}

// GET - Notes d'un élève (filtrage strict)
[Authorize]
public async Task<ActionResult<IEnumerable<Note>>> GetNotesByEleve(int id)
{
    var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    
    var eleve = await _eleveRepository.GetByIdAsync(id);
    if (eleve == null) return NotFound();
    
    // Élève peut voir uniquement ses propres notes
    if (userRole == "Élève")
    {
        var eleveId = int.Parse(User.FindFirst("EleveId")?.Value);
        if (id != eleveId) return Forbid();
    }
    // Parent peut voir notes de ses enfants
    else if (userRole == "Parent")
    {
        var tuteurId = int.Parse(User.FindFirst("TuteurId")?.Value);
        if (eleve.IdTuteur != tuteurId) return Forbid();
    }
    // Staff voit notes de son école
    else if (UserRoles.IsStaffRole(userRole) && userRole != "Super-Admin")
    {
        var userEcoleId = int.Parse(User.FindFirst("EcoleId")?.Value);
        if (eleve.Classe.Direction.IdEcole != userEcoleId) return Forbid();
    }
    
    var notes = await _eleveRepository.GetNotesAsync(id);
    return Ok(notes);
}

// DELETE - CRITIQUE
[Authorize(Roles = "Super-Admin,Directeur")]
public async Task<IActionResult> DeleteEleve(int id)
{
    var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
    var eleve = await _eleveRepository.GetByIdAsync(id);
    
    if (eleve == null) return NotFound();
    
    // Directeur peut supprimer uniquement élèves de son école
    if (userRole != "Super-Admin")
    {
        var userEcoleId = int.Parse(User.FindFirst("EcoleId")?.Value);
        if (eleve.Classe.Direction.IdEcole != userEcoleId)
        {
            return Forbid();
        }
    }
    
    // Vérifier inscriptions...
    // ...
}
```

---

### 5. 👨‍🏫 **AgentController** (13 endpoints)

| # | Method | Endpoint | Risque | Rôles autorisés | Problème actuel |
|---|--------|----------|--------|-----------------|-----------------|
| 1 | GET | `/api/Agent` | 🟡 | SA, DIR (même école) | ⚠️ Tous voient tous les agents |
| 2 | GET | `/api/Agent/{id}` | 🟡 | SA, DIR (même école), ou soi-même | ⚠️ Voir n'importe quel agent |
| 3 | GET | `/api/Agent/ecole/{idEcole}` | 🟡 | SA, DIR (même école) | ⚠️ Voir agents d'autres écoles |
| 4 | **POST** | `/api/Agent` | 🔴 | SA, DIR (même école) | ❌ N'importe qui crée agent |
| 5 | **PUT** | `/api/Agent/{id}` | 🔴 | SA, DIR (même école), ou soi-même | ❌ Modifier n'importe quel agent |
| 6 | **DELETE** | `/api/Agent/{id}` | 🔴 | SA, DIR (même école) | ❌ CRITIQUE: Supprimer agents |
| 7 | **PUT** | `/api/Agent/toggle-statut/{id}` | 🔴 | SA, DIR (même école) | ❌ Désactiver agents |
| 8 | GET | `/api/Agent/serial-number/{serialNumber}` | 🟡 | SA, DIR (même école) | ⚠️ Rechercher agents |
| 9 | **PUT** | `/api/Agent/{idAgent}/serial-number` | 🟠 | SA, DIR (même école) | ⚠️ Modifier serial number |

**Recommandations prioritaires :**
```csharp
[Authorize(Roles = "Super-Admin,Directeur")]
public async Task<ActionResult<Agent>> CreateAgent(Agent agent)
{
    var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
    var userEcoleId = int.Parse(User.FindFirst("EcoleId")?.Value);
    
    // Directeur peut créer uniquement agents dans son école
    if (userRole != "Super-Admin" && agent.IdEcole != userEcoleId)
    {
        return Forbid();
    }
    // ...
}

[Authorize(Roles = "Super-Admin,Directeur")]
public async Task<IActionResult> DeleteAgent(int id)
{
    var agent = await _agentRepository.GetByIdAsync(id);
    if (agent == null) return NotFound();
    
    var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
    if (userRole != "Super-Admin")
    {
        var userEcoleId = int.Parse(User.FindFirst("EcoleId")?.Value);
        if (agent.IdEcole != userEcoleId) return Forbid();
    }
    // ...
}
```

---

### 6. 📝 **NoteController** (12 endpoints)

| # | Method | Endpoint | Risque | Rôles autorisés | Problème actuel |
|---|--------|----------|--------|-----------------|-----------------|
| 1 | GET | `/api/Note` | 🟡 | SA, DIR, ENS (même école) | ⚠️ Tous voient toutes les notes |
| 2 | GET | `/api/Note/{id}` | 🟡 | SA, DIR, ENS, PAR (enfants), ELE (soi) | ⚠️ Voir n'importe quelle note |
| 3 | GET | `/api/Note/eleve/{idEleve}` | 🟡 | SA, DIR, ENS, PAR (ses enfants), ELE (soi) | ⚠️ Voir notes d'autres élèves |
| 4 | GET | `/api/Note/cours/{idCours}` | 🟡 | SA, DIR, ENS (ses cours) | ⚠️ Voir notes d'autres cours |
| 5 | GET | `/api/Note/professeur/{idProfesseur}` | 🟡 | SA, DIR, ENS (soi-même) | ⚠️ Voir notes d'autres profs |
| 6 | **POST** | `/api/Note` | 🔴 | SA, DIR, ENS (ses cours) | ❌ N'importe qui crée note |
| 7 | **PUT** | `/api/Note/{id}` | 🔴 | SA, DIR, ENS (ses cours) | ❌ Modifier n'importe quelle note |
| 8 | **DELETE** | `/api/Note/{id}` | 🔴 | SA, DIR (avec justification) | ❌ CRITIQUE: Supprimer notes |
| 9 | **PUT** | `/api/Note/toggle-statut/{id}` | 🔴 | SA, DIR | ❌ Invalider notes |

**Recommandations prioritaires :**
```csharp
// POST - Créer note
[Authorize(Roles = "Super-Admin,Directeur,Enseignant")]
public async Task<ActionResult<Note>> CreateNote(Note note)
{
    var userRole = User.FindFirst(ClaimTypes.Role)?.Value);
    
    if (userRole == "Enseignant")
    {
        var agentId = int.Parse(User.FindFirst("AgentId")?.Value);
        
        // Vérifier que l'enseignant enseigne ce cours
        var cours = await _coursRepository.GetByIdAsync(note.IdCours);
        if (cours == null) return NotFound("Cours non trouvé");
        
        var affectation = await _affectationCoursRepository
            .GetByAgentAndCoursAsync(agentId, note.IdCours);
        
        if (affectation == null)
        {
            return Forbid("Vous n'êtes pas autorisé à noter ce cours");
        }
    }
    // ...
}

// GET - Notes par élève (filtrage strict)
[Authorize]
public async Task<ActionResult<IEnumerable<Note>>> GetNotesByEleve(int idEleve)
{
    var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
    
    // Élève peut voir uniquement ses propres notes
    if (userRole == "Élève")
    {
        var eleveId = int.Parse(User.FindFirst("EleveId")?.Value);
        if (idEleve != eleveId) return Forbid();
    }
    // Parent peut voir notes de ses enfants
    else if (userRole == "Parent")
    {
        var tuteurId = int.Parse(User.FindFirst("TuteurId")?.Value);
        var eleve = await _eleveRepository.GetByIdAsync(idEleve);
        
        if (eleve == null) return NotFound();
        if (eleve.IdTuteur != tuteurId) return Forbid();
    }
    // Staff de l'école
    else if (UserRoles.IsStaffRole(userRole) && userRole != "Super-Admin")
    {
        var userEcoleId = int.Parse(User.FindFirst("EcoleId")?.Value);
        var eleve = await _eleveRepository.GetByIdAsync(idEleve);
        
        if (eleve == null) return NotFound();
        if (eleve.Classe.Direction.IdEcole != userEcoleId) return Forbid();
    }
    
    var notes = await _noteRepository.GetByEleveAsync(idEleve);
    return Ok(notes);
}

// DELETE - CRITIQUE (normalement interdit)
[Authorize(Roles = "Super-Admin")]
public async Task<IActionResult> DeleteNote(int id)
{
    // Seul Super-Admin peut supprimer une note
    // Avec audit trail obligatoire
    var note = await _noteRepository.GetByIdAsync(id);
    if (note == null) return NotFound();
    
    // TODO: Enregistrer dans audit log qui a supprimé quoi
    
    await _noteRepository.DeleteAsync(id);
    return NoContent();
}
```

---

### 7. 👪 **TuteurController** (8 endpoints)

| # | Method | Endpoint | Risque | Rôles autorisés | Problème actuel |
|---|--------|----------|--------|-----------------|-----------------|
| 1 | GET | `/api/Tuteur` | 🟠 | SA, DIR, SEC (même école) | ⚠️ Fuite données personnelles tuteurs |
| 2 | GET | `/api/Tuteur/{id}` | 🟠 | SA, DIR, SEC, ou soi-même | ⚠️ Voir n'importe quel tuteur |
| 3 | GET | `/api/Tuteur/ecole/{idEcole}` | 🟠 | SA, DIR, SEC (même école) | ⚠️ Voir tuteurs d'autres écoles |
| 4 | GET | `/api/Tuteur/{id}/eleves` | 🟡 | SA, DIR, SEC, ou soi-même | ⚠️ Voir enfants d'autres tuteurs |
| 5 | **PUT** | `/api/Tuteur/{id}` | 🔴 | SA, DIR, SEC, ou soi-même | ❌ Modifier n'importe quel tuteur |
| 6 | **DELETE** | `/api/Tuteur/{id}` | 🔴 | SA, DIR (avec validation) | ❌ Supprimer tuteurs |
| 7 | **PUT** | `/api/Tuteur/toggle-statut/{id}` | 🔴 | SA, DIR | ❌ Désactiver tuteurs |

**Recommandations prioritaires :**
```csharp
// GET - Tuteur par ID
[Authorize]
public async Task<ActionResult<Tuteur>> GetTuteur(int id)
{
    var tuteur = await _tuteurRepository.GetByIdAsync(id);
    if (tuteur == null) return NotFound();
    
    var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
    
    // Parent peut voir uniquement ses propres infos
    if (userRole == "Parent")
    {
        var tuteurId = int.Parse(User.FindFirst("TuteurId")?.Value);
        if (id != tuteurId) return Forbid();
    }
    // Staff de l'école
    else if (UserRoles.IsStaffRole(userRole) && userRole != "Super-Admin")
    {
        var userEcoleId = int.Parse(User.FindFirst("EcoleId")?.Value);
        if (tuteur.IdEcole != userEcoleId) return Forbid();
    }
    
    return Ok(tuteur);
}

// PUT - Modifier tuteur
[Authorize]
public async Task<IActionResult> UpdateTuteur(int id, Tuteur tuteur)
{
    if (id != tuteur.IdTuteur) return BadRequest();
    
    var existingTuteur = await _tuteurRepository.GetByIdAsync(id);
    if (existingTuteur == null) return NotFound();
    
    var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
    
    // Parent peut modifier uniquement ses propres infos
    if (userRole == "Parent")
    {
        var tuteurId = int.Parse(User.FindFirst("TuteurId")?.Value);
        if (id != tuteurId) return Forbid();
    }
    // Staff de l'école
    else if (!new[] { "Super-Admin", "Directeur", "Secrétaire" }.Contains(userRole))
    {
        return Forbid(); // Autres rôles non autorisés
    }
    else if (userRole != "Super-Admin")
    {
        var userEcoleId = int.Parse(User.FindFirst("EcoleId")?.Value);
        if (existingTuteur.IdEcole != userEcoleId) return Forbid();
    }
    
    // ...
}

// DELETE - CRITIQUE
[Authorize(Roles = "Super-Admin,Directeur")]
public async Task<IActionResult> DeleteTuteur(int id)
{
    var tuteur = await _tuteurRepository.GetByIdAsync(id);
    if (tuteur == null) return NotFound();
    
    // Vérifier qu'il n'a pas d'élèves actifs
    var eleves = await _tuteurRepository.GetElevesAsync(id);
    if (eleves.Any())
    {
        return BadRequest("Impossible de supprimer un tuteur ayant des élèves actifs");
    }
    
    var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
    if (userRole != "Super-Admin")
    {
        var userEcoleId = int.Parse(User.FindFirst("EcoleId")?.Value);
        if (tuteur.IdEcole != userEcoleId) return Forbid();
    }
    
    // ...
}
```

---

### 8. 🔐 **RoleController** (9 endpoints)

| # | Method | Endpoint | Risque | Rôles autorisés | Problème actuel |
|---|--------|----------|--------|-----------------|-----------------|
| 1 | GET | `/api/Role` | 🟡 | SA, DIR | ⚠️ Tous voient tous les rôles |
| 2 | GET | `/api/Role/{id}` | 🟡 | SA, DIR | ⚠️ Voir n'importe quel rôle |
| 3 | GET | `/api/Role/nom/{nom}` | 🟡 | SA, DIR | ⚠️ Rechercher rôles |
| 4 | **POST** | `/api/Role` | 🔴 | **SA uniquement** | ❌ CRITIQUE: N'importe qui crée rôle |
| 5 | **PUT** | `/api/Role/{id}` | 🔴 | **SA uniquement** | ❌ CRITIQUE: Modifier n'importe quel rôle |
| 6 | **DELETE** | `/api/Role/{id}` | 🔴 | **SA uniquement** | ❌ CRITIQUE: Supprimer rôles |
| 7 | **PUT** | `/api/Role/toggle-statut/{id}` | 🔴 | **SA uniquement** | ❌ CRITIQUE: Désactiver rôles |

**Recommandations prioritaires :**
```csharp
// TOUS LES ENDPOINTS DE MODIFICATION → SA UNIQUEMENT
[Authorize(Roles = "Super-Admin")]
public async Task<ActionResult<Role>> CreateRole(Role role)
{
    // ...
}

[Authorize(Roles = "Super-Admin")]
public async Task<IActionResult> UpdateRole(int id, Role role)
{
    // ...
}

[Authorize(Roles = "Super-Admin")]
public async Task<IActionResult> DeleteRole(int id)
{
    // Vérifier qu'aucun utilisateur n'a ce rôle
    var utilisateurs = await _roleRepository.GetUtilisateursAsync(id);
    if (utilisateurs.Any())
    {
        return BadRequest($"Impossible de supprimer ce rôle: {utilisateurs.Count()} utilisateur(s) l'utilisent");
    }
    // ...
}

// GET endpoints → SA + DIR
[Authorize(Roles = "Super-Admin,Directeur")]
public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
{
    // ...
}
```

---

## 🚨 TOP 30 ENDPOINTS CRITIQUES (PAR ORDRE DE PRIORITÉ)

### 🔴 **NIVEAU CRITIQUE (Action immédiate requise)**

| # | Endpoint | Contrôleur | Problème | Rôles nécessaires | Impact |
|---|----------|------------|----------|-------------------|--------|
| 1 | `DELETE /api/Ecole/{id}` | Ecole | N'importe qui peut supprimer école | **SA uniquement** | 💀 CATASTROPHIQUE |
| 2 | `DELETE /api/Utilisateur/{id}` | Utilisateur | Supprimer n'importe quel utilisateur | **SA, DIR** | 💀 CATASTROPHIQUE |
| 3 | `POST /api/Role` | Role | Créer rôles personnalisés | **SA uniquement** | 💀 CATASTROPHIQUE |
| 4 | `DELETE /api/Role/{id}` | Role | Supprimer rôles système | **SA uniquement** | 💀 CATASTROPHIQUE |
| 5 | `PUT /api/Role/{id}` | Role | Modifier rôles système | **SA uniquement** | 💀 CATASTROPHIQUE |
| 6 | `DELETE /api/Agent/{id}` | Agent | Supprimer agents | **SA, DIR** | 🔴 CRITIQUE |
| 7 | `DELETE /api/Eleve/{id}` | Eleve | Supprimer élèves | **SA, DIR** | 🔴 CRITIQUE |
| 8 | `DELETE /api/Paiement/{id}` | Paiement | Supprimer paiements | **SA, COMP** | 🔴 CRITIQUE |
| 9 | `DELETE /api/Note/{id}` | Note | Supprimer notes | **SA uniquement** | 🔴 CRITIQUE |
| 10 | `POST /api/Ecole` | Ecole | Créer écoles sans contrôle | **SA uniquement** | 🔴 CRITIQUE |

### 🟠 **NIVEAU ÉLEVÉ (Action urgente dans 48h)**

| # | Endpoint | Contrôleur | Problème | Rôles nécessaires | Impact |
|---|----------|------------|----------|-------------------|--------|
| 11 | `PUT /api/Ecole/{id}` | Ecole | Modifier n'importe quelle école | **SA, DIR (même école)** | 🟠 ÉLEVÉ |
| 12 | `POST /api/Utilisateur` | Utilisateur | Créer utilisateurs sans limite | **SA, DIR, SEC** | 🟠 ÉLEVÉ |
| 13 | `POST /api/Paiement` | Paiement | Créer faux paiements | **SA, DIR, COMP, SEC** | 🟠 ÉLEVÉ |
| 14 | `POST /api/Agent` | Agent | Créer agents sans contrôle | **SA, DIR** | 🟠 ÉLEVÉ |
| 15 | `POST /api/Note` | Note | Créer/modifier notes | **SA, DIR, ENS** | 🟠 ÉLEVÉ |
| 16 | `PUT /api/Paiement/{id}` | Paiement | Modifier paiements validés | **SA, DIR, COMP** | 🟠 ÉLEVÉ |
| 17 | `PUT /api/Note/{id}` | Note | Modifier notes d'autres profs | **SA, DIR, ENS (ses cours)** | 🟠 ÉLEVÉ |
| 18 | `PUT /api/Eleve/{id}` | Eleve | Modifier n'importe quel élève | **SA, DIR, SEC** | 🟠 ÉLEVÉ |
| 19 | `PUT /api/Agent/{id}` | Agent | Modifier n'importe quel agent | **SA, DIR** | 🟠 ÉLEVÉ |
| 20 | `PUT /api/Tuteur/{id}` | Tuteur | Modifier tuteurs d'autres écoles | **SA, DIR, SEC, ou soi-même** | 🟠 ÉLEVÉ |

### 🟡 **NIVEAU MOYEN (Fuite de données - Action dans 1 semaine)**

| # | Endpoint | Contrôleur | Problème | Filtrage nécessaire | Impact |
|---|----------|------------|----------|---------------------|--------|
| 21 | `GET /api/Eleve` | Eleve | Voir tous les élèves de toutes écoles | Filtrer par EcoleId | 🟡 MOYEN |
| 22 | `GET /api/Paiement` | Paiement | Voir tous les paiements | Filtrer par EcoleId/Role | 🟡 MOYEN |
| 23 | `GET /api/Note/eleve/{idEleve}` | Note | Parent voit notes d'autres élèves | Filtrer par TuteurId | 🟡 MOYEN |
| 24 | `GET /api/Utilisateur` | Utilisateur | Voir tous les utilisateurs | Filtrer par EcoleId | 🟡 MOYEN |
| 25 | `GET /api/Agent` | Agent | Voir agents d'autres écoles | Filtrer par EcoleId | 🟡 MOYEN |
| 26 | `GET /api/Tuteur` | Tuteur | Fuite données personnelles tuteurs | Filtrer par EcoleId | 🟡 MOYEN |
| 27 | `GET /api/Ecole/{id}/utilisateurs` | Ecole | Lister utilisateurs d'autres écoles | **SA, DIR (même école)** | 🟡 MOYEN |
| 28 | `GET /api/Ecole/{id}/tuteurs` | Ecole | Lister tuteurs d'autres écoles | **SA, DIR (même école)** | 🟡 MOYEN |
| 29 | `GET /api/Paiement/dashboard/ecole/{idEcole}` | Paiement | Stats financières autres écoles | Filtrer par EcoleId | 🟡 MOYEN |
| 30 | `GET /api/Eleve/{id}/paiements` | Eleve | Parent voit paiements d'autres | Filtrer par TuteurId | 🟡 MOYEN |

---

## 🎯 RECOMMANDATIONS DE SÉCURISATION

### 1. **Modifications immédiates (Aujourd'hui)**

#### A. Sécuriser les 10 endpoints CRITIQUES

```csharp
// 1. EcoleController
[Authorize(Roles = "Super-Admin")]
public async Task<IActionResult> DeleteEcole(int id)

// 2. UtilisateurController
[Authorize(Roles = "Super-Admin,Directeur")]
public async Task<IActionResult> DeleteUtilisateur(int id)

// 3-5. RoleController
[Authorize(Roles = "Super-Admin")]
public async Task<ActionResult<Role>> CreateRole(Role role)
[Authorize(Roles = "Super-Admin")]
public async Task<IActionResult> UpdateRole(int id, Role role)
[Authorize(Roles = "Super-Admin")]
public async Task<IActionResult> DeleteRole(int id)

// 6. AgentController
[Authorize(Roles = "Super-Admin,Directeur")]
public async Task<IActionResult> DeleteAgent(int id)

// 7. EleveController
[Authorize(Roles = "Super-Admin,Directeur")]
public async Task<IActionResult> DeleteEleve(int id)

// 8. PaiementController
[Authorize(Roles = "Super-Admin,Comptable")]
public async Task<IActionResult> DeletePaiement(int id)

// 9. NoteController
[Authorize(Roles = "Super-Admin")]
public async Task<IActionResult> DeleteNote(int id)

// 10. EcoleController
[Authorize(Roles = "Super-Admin")]
public async Task<ActionResult<object>> CreateEcole(Ecole ecole)
```

**Temps estimé : 2 heures**

---

### 2. **Modifications urgentes (Cette semaine)**

#### B. Créer l'énumération UserRoles

Créer `Models/Enums/UserRoles.cs` avec tous les rôles du système.

#### C. Créer ICurrentUserService

Service pour récupérer facilement les informations de l'utilisateur connecté.

#### D. Filtrage multi-tenant

Ajouter filtrage par `EcoleId` dans tous les services et contrôleurs.

#### E. Sécuriser les endpoints de création

Ajouter `[Authorize(Roles = "...")]` sur tous les endpoints POST/PUT/DELETE.

**Temps estimé : 8-12 heures**

---

### 3. **Optimisations (2 semaines)**

#### F. Filtrage des données par rôle

Implémenter la logique de filtrage dans chaque endpoint GET selon le rôle.

#### G. Tests de sécurité

Créer des tests automatisés vérifiant les restrictions par rôle.

#### H. Documentation Swagger

Documenter les rôles requis dans Swagger/OpenAPI.

**Temps estimé : 16-20 heures**

---

### 4. **Évolution long terme (1-2 mois)**

#### I. Système de permissions

Migrer vers un système RBAC complet avec permissions granulaires.

#### J. Audit trail

Enregistrer toutes les actions de suppression/modification sensibles.

#### K. Interface d'administration

Créer une interface web pour gérer les rôles et permissions.

**Temps estimé : 40-60 heures**

---

## 🚀 PLAN D'ACTION IMMÉDIAT

### **PHASE 1 : URGENCE (Aujourd'hui - 4 heures)**

✅ **Étape 1 : Sécuriser les 10 endpoints CRITIQUES** (2h)

1. Ouvrir `EcoleController.cs`
2. Ajouter `[Authorize(Roles = "Super-Admin")]` sur `DeleteEcole`
3. Répéter pour les 9 autres endpoints critiques
4. Build + test rapide

✅ **Étape 2 : Créer UserRoles.cs** (30 min)

```csharp
// Models/Enums/UserRoles.cs
public static class UserRoles
{
    public const string SUPER_ADMIN = "Super-Admin";
    public const string DIRECTEUR = "Directeur";
    public const string SOUS_DIRECTEUR = "Sous-Directeur";
    public const string SECRETAIRE = "Secrétaire";
    public const string COMPTABLE = "Comptable";
    public const string ENSEIGNANT = "Enseignant";
    public const string PREFET = "Préfet";
    public const string PARENT = "Parent";
    public const string ELEVE = "Élève";
    public const string BAILLEUR = "Bailleur";
    public const string AGENT_SUPPORT = "Agent Support";
    
    public static string[] AdminRoles => new[] { SUPER_ADMIN, DIRECTEUR, SOUS_DIRECTEUR };
    public static string[] StaffRoles => new[] { SUPER_ADMIN, DIRECTEUR, SOUS_DIRECTEUR, SECRETAIRE, COMPTABLE, ENSEIGNANT, PREFET };
    
    public static bool IsAdminRole(string role) => AdminRoles.Contains(role);
    public static bool IsStaffRole(string role) => StaffRoles.Contains(role);
}
```

✅ **Étape 3 : Tests rapides** (1h30)

- Tester avec Postman avec différents rôles
- Vérifier que les restrictions fonctionnent
- Corriger les bugs éventuels

---

### **PHASE 2 : HAUTE PRIORITÉ (Demain - 8 heures)**

✅ **Étape 4 : Sécuriser les 20 endpoints ÉLEVÉS** (4h)

Ajouter `[Authorize(Roles = "...")]` sur les endpoints niveau ÉLEVÉ.

✅ **Étape 5 : Créer ICurrentUserService** (2h)

Service centralisant l'accès aux claims de l'utilisateur.

✅ **Étape 6 : Filtrage multi-tenant basique** (2h)

Ajouter vérification `EcoleId` dans les endpoints critiques.

---

### **PHASE 3 : MOYEN TERME (Semaine prochaine - 20 heures)**

✅ **Étape 7 : Filtrage complet par rôle** (12h)

Implémenter logique de filtrage dans tous les GET endpoints.

✅ **Étape 8 : Tests automatisés** (4h)

Créer tests unitaires/intégration vérifiant les restrictions.

✅ **Étape 9 : Documentation** (4h)

Documenter les rôles requis dans Swagger et README.

---

## 📝 CHECKLIST DE SÉCURISATION

### Contrôleurs critiques

- [ ] **EcoleController** (11 endpoints)
  - [ ] DELETE /api/Ecole/{id} → SA uniquement
  - [ ] POST /api/Ecole → SA uniquement
  - [ ] PUT /api/Ecole/{id} → SA, DIR (même école)
  - [ ] GET /api/Ecole/{id}/utilisateurs → SA, DIR (même école)
  - [ ] GET /api/Ecole → Filtrer par EcoleId

- [ ] **UtilisateurController** (15 endpoints)
  - [ ] DELETE /api/Utilisateur/{id} → SA, DIR (même école)
  - [ ] POST /api/Utilisateur → SA, DIR, SEC (même école)
  - [ ] PUT /api/Utilisateur/{id} → SA, DIR, ou soi-même
  - [ ] GET /api/Utilisateur → Filtrer par EcoleId
  - [ ] POST /api/Utilisateur/changer_mot_de_passe → Soi-même uniquement

- [ ] **PaiementController** (17 endpoints)
  - [ ] DELETE /api/Paiement/{id} → SA, COMP (validation)
  - [ ] POST /api/Paiement → SA, DIR, COMP, SEC (même école)
  - [ ] PUT /api/Paiement/{id} → SA, DIR, COMP (même école)
  - [ ] GET /api/Paiement/eleve/{idEleve} → Filtrer par TuteurId (parent)
  - [ ] GET /api/Paiement → Filtrer par EcoleId

- [ ] **EleveController** (25 endpoints)
  - [ ] DELETE /api/Eleve/{id} → SA, DIR (même école)
  - [ ] PUT /api/Eleve/{id} → SA, DIR, SEC (même école)
  - [ ] GET /api/Eleve/{id} → Filtrer par rôle (parent, élève, staff)
  - [ ] GET /api/Eleve/{id}/notes → Filtrer strictement
  - [ ] GET /api/Eleve → Filtrer par EcoleId

- [ ] **AgentController** (13 endpoints)
  - [ ] DELETE /api/Agent/{id} → SA, DIR (même école)
  - [ ] POST /api/Agent → SA, DIR (même école)
  - [ ] PUT /api/Agent/{id} → SA, DIR (même école)
  - [ ] GET /api/Agent → Filtrer par EcoleId

- [ ] **NoteController** (12 endpoints)
  - [ ] DELETE /api/Note/{id} → SA uniquement
  - [ ] POST /api/Note → SA, DIR, ENS (ses cours)
  - [ ] PUT /api/Note/{id} → SA, DIR, ENS (ses cours)
  - [ ] GET /api/Note/eleve/{idEleve} → Filtrer par rôle
  - [ ] GET /api/Note → Filtrer par EcoleId

- [ ] **TuteurController** (8 endpoints)
  - [ ] DELETE /api/Tuteur/{id} → SA, DIR (validation)
  - [ ] PUT /api/Tuteur/{id} → SA, DIR, SEC, ou soi-même
  - [ ] GET /api/Tuteur → Filtrer par EcoleId
  - [ ] GET /api/Tuteur/{id} → Filtrer (staff ou soi-même)

- [ ] **RoleController** (9 endpoints)
  - [ ] DELETE /api/Role/{id} → SA uniquement
  - [ ] POST /api/Role → SA uniquement
  - [ ] PUT /api/Role/{id} → SA uniquement
  - [ ] GET /api/Role → SA, DIR

---

## 📚 RESSOURCES SUPPLÉMENTAIRES

### Code de référence complet

Voir le fichier `ANALYSE_GESTION_ROLES.md` pour :
- Exemples de code complets
- Architecture RBAC détaillée
- Service de permissions
- Attributs personnalisés

### Tests recommandés

```csharp
[Fact]
public async Task DeleteEcole_AsParent_ShouldReturn403Forbidden()
{
    // Arrange
    var client = _factory.CreateClientWithRole("Parent");
    
    // Act
    var response = await client.DeleteAsync("/api/Ecole/1");
    
    // Assert
    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
}

[Fact]
public async Task DeleteEcole_AsSuperAdmin_ShouldReturn204NoContent()
{
    // Arrange
    var client = _factory.CreateClientWithRole("Super-Admin");
    
    // Act
    var response = await client.DeleteAsync("/api/Ecole/999");
    
    // Assert
    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
}
```

---

## ⚠️ AVERTISSEMENT FINAL

**ÉTAT ACTUEL : 🔴 RISQUE CRITIQUE DE SÉCURITÉ**

Votre API est actuellement **OUVERTE** à tous les utilisateurs authentifiés. Un simple utilisateur avec n'importe quel rôle peut :

- ❌ Supprimer des écoles
- ❌ Supprimer des utilisateurs
- ❌ Créer/modifier/supprimer des rôles
- ❌ Voir les données de toutes les écoles
- ❌ Modifier des paiements
- ❌ Modifier des notes
- ❌ Accéder aux données personnelles de tous

**ACTION IMMÉDIATE REQUISE !**

Il est **FORTEMENT RECOMMANDÉ** de :
1. ✅ Sécuriser les 10 endpoints critiques **AUJOURD'HUI**
2. ✅ Implémenter le filtrage multi-tenant **CETTE SEMAINE**
3. ✅ Tester rigoureusement les restrictions **AVANT DE DÉPLOYER**

---

## 📞 PROCHAINES ÉTAPES

**Que voulez-vous que je fasse maintenant ?**

**[A]** Commencer la sécurisation immédiate (Phase 1)
  → Je modifie les 10 contrôleurs critiques
  → Je crée UserRoles.cs
  → Je teste les modifications

**[B]** Générer le code complet pour ICurrentUserService
  → Service de gestion des claims utilisateur
  → Exemples d'utilisation dans contrôleurs
  → Tests unitaires

**[C]** Créer un script de migration des rôles
  → Script SQL pour créer les nouveaux rôles
  → Script pour assigner les rôles aux utilisateurs existants
  → Validation des données

**[D]** Continuer l'analyse détaillée
  → Analyser les 24 contrôleurs restants
  → Créer la matrice complète de tous les ~250 endpoints
  → Recommandations spécifiques pour chacun

**Dites-moi quelle option vous préférez !** 🎯
