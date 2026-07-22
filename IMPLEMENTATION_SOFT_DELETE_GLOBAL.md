# 🌐 IMPLÉMENTATION SOFT DELETE GLOBAL
## Application Systématique à Tous les Contrôleurs

**Date:** 16 Octobre 2025  
**Status:** 🔄 EN COURS

---

## 📊 VUE D'ENSEMBLE

### Modèles avec Statut Bool (25 modèles)

| # | Modèle | Controller | Status | Priorité |
|---|--------|------------|--------|----------|
| 1 | ✅ **Presence** | PresenceController | ✅ FAIT | P0 |
| 2 | 🔄 **Eleve** | EleveController | 🔄 EN COURS | P0 |
| 3 | 🔄 **Utilisateur** | UtilisateurController | 🔄 EN COURS | P0 |
| 4 | 🔄 **Inscription** | InscriptionController | 🔄 EN COURS | P0 |
| 5 | 🔄 **Note** | NoteController | 🔄 EN COURS | P0 |
| 6 | 🔄 **Cours** | CoursController | 🔄 EN COURS | P0 |
| 7 | ⏳ Enseignant | EnseignantController | ⏳ À FAIRE | P1 |
| 8 | ⏳ Tuteur | TuteurController | ⏳ À FAIRE | P1 |
| 9 | ⏳ Classe | ClasseController | ⏳ À FAIRE | P1 |
| 10 | ⏳ Ecole | EcoleController | ⏳ À FAIRE | P1 |
| 11 | ⏳ Document | DocumentController | ⏳ À FAIRE | P2 |
| 12 | ⏳ AffectationCours | AffectationCoursController | ⏳ À FAIRE | P2 |
| 13 | ⏳ Horaire | HoraireController | ⏳ À FAIRE | P2 |
| 14 | ⏳ GroupeMessage | GroupeMessageController | ⏳ À FAIRE | P2 |
| 15 | ⏳ Frais | FraisController | ⏳ À FAIRE | P2 |
| 16 | ⏳ RessourcePedagogique | RessourcePedagogiqueController | ⏳ À FAIRE | P2 |
| 17 | ⏳ Message | MessageController | ⏳ À FAIRE | P2 |
| 18 | ⏳ Evaluation | EvaluationController | ⏳ À FAIRE | P2 |
| 19 | ⏳ Vacation | VacationController | ⏳ À FAIRE | P2 |
| 20 | ⏳ AnneeScolaire | AnneeScolaireController | ⏳ À FAIRE | P3 |
| 21 | ⏳ Option | OptionController | ⏳ À FAIRE | P3 |
| 22 | ⏳ Role | RoleController | ⏳ À FAIRE | P3 |
| 23 | ⏳ Direction | DirectionController | ⏳ À FAIRE | P3 |
| 24 | ⏳ Section | SectionController | ⏳ À FAIRE | P3 |
| 25 | ⏳ Paiement | PaiementController | ⏳ À FAIRE | P3 |
| 26 | ⏳ Notification | NotificationController | ⏳ À FAIRE | P3 |

---

## 🎯 TEMPLATE RÉUTILISABLE

Pour chaque modèle, il faut modifier **3 fichiers** :

### 1️⃣ Service (Exemple: EleveService.cs)

#### Ajouter le filtrage dans TOUS les GET

```csharp
// AVANT
public async Task<IEnumerable<Eleve>> GetAllAsync()
{
    return await _context.Eleves
        .Include(e => e.Classe)
        .ToListAsync();
}

// APRÈS
public async Task<IEnumerable<Eleve>> GetAllAsync()
{
    return await _context.Eleves
        .Include(e => e.Classe)
        .Where(e => e.Statut == true) // ✅ AJOUTER cette ligne
        .ToListAsync();
}
```

#### Ajouter ToggleStatutAsync

```csharp
// AJOUTER à la fin du service
public async Task<bool> ToggleStatutAsync(int id)
{
    var eleve = await _context.Eleves.FindAsync(id);
    if (eleve == null)
        return false;

    eleve.Statut = !eleve.Statut;
    await _context.SaveChangesAsync();
    return true;
}
```

---

### 2️⃣ Interface Repository (Exemple: IEleveRepository.cs)

```csharp
// AJOUTER cette ligne à l'interface
Task<bool> ToggleStatutAsync(int id);
```

---

### 3️⃣ Controller (Exemple: EleveController.cs)

```csharp
// AJOUTER cet endpoint à la fin du contrôleur
// PUT: api/Eleve/toggle-statut/{id}
[HttpPut("toggle-statut/{id}")]
public async Task<ActionResult<object>> ToggleStatut(int id)
{
    try
    {
        var success = await _eleveRepository.ToggleStatutAsync(id);
        if (!success)
        {
            return NotFound(new { message = "Élève non trouvé" });
        }

        var eleve = await _eleveRepository.GetByIdAsync(id);
        
        return Ok(new { 
            message = "Statut modifié avec succès", 
            eleve = eleve,
            nouveauStatut = eleve?.Statut 
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { 
            message = "Erreur lors du changement de statut", 
            error = ex.Message 
        });
    }
}
```

---

## 📋 CHECKLIST PAR MODÈLE

Pour chaque modèle, vérifier :

```
Service:
□ Ajouter .Where(x => x.Statut == true) dans GetAllAsync()
□ Ajouter .Where(x => x.Statut == true) dans GetByIdAsync()
□ Ajouter .Where(x => x.Statut == true) dans TOUS les autres GET
□ Ajouter méthode ToggleStatutAsync(int id)

Interface Repository:
□ Ajouter signature Task<bool> ToggleStatutAsync(int id);

Controller:
□ Ajouter endpoint PUT /toggle-statut/{id}
□ Utiliser le bon nom de variable (eleve, utilisateur, etc.)
□ Utiliser le bon repository (_eleveRepository, etc.)

Tests:
□ Créer fichier test-{modele}-soft-delete.http
□ Tester création → visible
□ Tester toggle → invisible
□ Tester toggle → visible à nouveau
```

---

## 🚀 AUTOMATISATION (SCRIPT ASSISTANT)

Je vais créer les implémentations pour les 5 modèles critiques automatiquement.

Vous pouvez ensuite appliquer le même pattern aux 20 autres modèles en suivant le template ci-dessus.

---

## 📝 EXEMPLE COMPLET : ELEVE

### EleveService.cs

```csharp
// Méthodes GET à modifier (ajouter filtrage)
public async Task<IEnumerable<Eleve>> GetAllAsync()
{
    return await _context.Eleves
        .Include(e => e.Classe)
        .Include(e => e.Tuteur)
        .Where(e => e.Statut == true) // ✅ AJOUTER
        .ToListAsync();
}

public async Task<Eleve> GetByIdAsync(int id)
{
    return await _context.Eleves
        .Include(e => e.Classe)
        .Include(e => e.Tuteur)
        .Where(e => e.Statut == true) // ✅ AJOUTER
        .FirstOrDefaultAsync(e => e.IdEleve == id);
}

// Nouvelle méthode à ajouter
public async Task<bool> ToggleStatutAsync(int id)
{
    var eleve = await _context.Eleves.FindAsync(id);
    if (eleve == null)
        return false;

    eleve.Statut = !eleve.Statut;
    await _context.SaveChangesAsync();
    return true;
}
```

### IEleveRepository.cs

```csharp
public interface IEleveRepository
{
    // ... méthodes existantes ...
    Task<bool> ToggleStatutAsync(int id); // ✅ AJOUTER
}
```

### EleveController.cs

```csharp
// AJOUTER cet endpoint
[HttpPut("toggle-statut/{id}")]
public async Task<ActionResult<object>> ToggleStatut(int id)
{
    try
    {
        var success = await _eleveRepository.ToggleStatutAsync(id);
        if (!success)
        {
            return NotFound(new { message = "Élève non trouvé" });
        }

        var eleve = await _eleveRepository.GetByIdAsync(id);
        
        return Ok(new { 
            message = "Statut modifié avec succès", 
            eleve = eleve,
            nouveauStatut = eleve?.Statut 
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { 
            message = "Erreur lors du changement de statut", 
            error = ex.Message 
        });
    }
}
```

---

## 📊 PROGRESSION

### Phase 1 : Modèles Critiques (P0) - 6 modèles

```
✅ Presence (FAIT)
🔄 Eleve (EN COURS)
🔄 Utilisateur (EN COURS)
🔄 Inscription (EN COURS)
🔄 Note (EN COURS)
🔄 Cours (EN COURS)
```

### Phase 2 : Modèles Importants (P1) - 4 modèles

```
⏳ Enseignant
⏳ Tuteur
⏳ Classe
⏳ Ecole
```

### Phase 3 : Modèles Standards (P2) - 10 modèles

```
⏳ Document, AffectationCours, Horaire, GroupeMessage, Frais
⏳ RessourcePedagogique, Message, Evaluation, Vacation
```

### Phase 4 : Modèles Configuration (P3) - 6 modèles

```
⏳ AnneeScolaire, Option, Role, Direction, Section, Paiement, Notification
```

---

## 🎯 ESTIMATION TEMPS

| Phase | Modèles | Temps/Modèle | Total |
|-------|---------|--------------|-------|
| Phase 1 (P0) | 6 | 15 min | 1.5h |
| Phase 2 (P1) | 4 | 15 min | 1h |
| Phase 3 (P2) | 10 | 10 min | 1.7h |
| Phase 4 (P3) | 6 | 10 min | 1h |
| **TOTAL** | **26** | | **~5h** |

**Avec automatisation partielle : ~3h**

---

## 🔍 VALIDATION

### Tests Globaux

Créer un fichier `test-soft-delete-all.http` avec :

```http
### Test Eleve
PUT http://localhost:5002/api/Eleve/toggle-statut/1
GET http://localhost:5002/api/Eleve

### Test Utilisateur
PUT http://localhost:5002/api/Utilisateur/toggle-statut/1
GET http://localhost:5002/api/Utilisateur

### Test Inscription
PUT http://localhost:5002/api/Inscription/toggle-statut/1
GET http://localhost:5002/api/Inscription

# ... etc pour tous les modèles
```

---

## 💡 BONNES PRATIQUES

### ✅ À FAIRE

```
1. Appliquer le pattern systématiquement
2. Tester chaque modèle individuellement
3. Documenter les exceptions (modèles sans soft delete)
4. Créer tests HTTP pour chaque modèle
5. Mettre à jour Swagger documentation
```

### ❌ À ÉVITER

```
1. Ne pas mélanger Statut (bool) avec d'autres champs statut (string)
2. Ne pas oublier le filtrage dans TOUS les GET
3. Ne pas supprimer définitivement sans raison valable
4. Ne pas exposer les données désactivées aux utilisateurs normaux
```

---

## 🚀 PROCHAINES ÉTAPES

1. ✅ Template créé
2. 🔄 Implémenter Phase 1 (6 modèles critiques)
3. ⏳ Guide pour Phase 2-4 (20 modèles restants)
4. ⏳ Tests globaux
5. ⏳ Documentation finale

---

**Status:** 🔄 EN COURS - Template prêt, implémentation des modèles critiques en cours

---

**Date:** 16 Octobre 2025  
**Auteur:** Assistant IA

