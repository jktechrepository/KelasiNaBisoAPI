# ⚡ QUICK START - SOFT DELETE GLOBAL
## Guide Rapide d'Implémentation

**Date:** 16 Octobre 2025  
**Objectif:** Implémenter le soft delete dans tous les contrôleurs rapidement

---

## 🎯 SITUATION ACTUELLE

**Bonne nouvelle** : **25 modèles ont déjà le champ `Statut` bool** ! 🎉

**Mauvaise nouvelle** : Les services ne filtrent pas dessus, et il n'y a pas d'endpoint toggle.

---

## ⚡ SOLUTION RAPIDE

### Étape 1 : Copier les 3 snippets ci-dessous

### Étape 2 : Appliquer à chaque modèle en remplaçant les noms

### Étape 3 : Tester

---

## 📝 SNIPPET 1 : Service (Ajouter en fin de fichier)

```csharp
// ✅ SOFT DELETE: Toggle le statut d'un {MODELE} (actif <-> inactif)
public async Task<bool> ToggleStatutAsync(int id)
{
    var {modele} = await _context.{Modeles}.FindAsync(id);
    if ({modele} == null)
        return false;

    {modele}.Statut = !{modele}.Statut;
    await _context.SaveChangesAsync();
    return true;
}
```

**Remplacer:**
- `{MODELE}` → Nom du modèle (ex: Eleve, Utilisateur)
- `{modele}` → Nom variable (ex: eleve, utilisateur)
- `{Modeles}` → Nom table pluriel (ex: Eleves, Utilisateurs)

---

## 📝 SNIPPET 2 : Interface (Ajouter à l'interface)

```csharp
Task<bool> ToggleStatutAsync(int id);
```

---

## 📝 SNIPPET 3 : Contrôleur (Ajouter en fin)

```csharp
// PUT: api/{Controller}/toggle-statut/{id}
[HttpPut("toggle-statut/{id}")]
public async Task<ActionResult<object>> ToggleStatut(int id)
{
    try
    {
        var success = await _{modele}Repository.ToggleStatutAsync(id);
        if (!success)
        {
            return NotFound(new { message = "{MODELE} non trouvé" });
        }

        var {modele} = await _{modele}Repository.GetByIdAsync(id);
        
        return Ok(new { 
            message = "Statut modifié avec succès", 
            {modele} = {modele},
            nouveauStatut = {modele}?.Statut 
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

**Remplacer:**
- `{Controller}` → Nom controller (ex: Eleve, Utilisateur)
- `{MODELE}` → Nom affiché (ex: Élève, Utilisateur)
- `{modele}` → Nom variable (ex: eleve, utilisateur)

---

## 🚀 EXEMPLE COMPLET : ELEVE

### EleveService.cs - Ajouter à la fin

```csharp
// ✅ SOFT DELETE: Toggle le statut d'un élève (actif <-> inactif)
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

### IEleveRepository.cs - Ajouter

```csharp
Task<bool> ToggleStatutAsync(int id);
```

### EleveController.cs - Ajouter à la fin

```csharp
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

## 📋 LISTE DES MODÈLES À TRAITER

### P0 - Critiques (6 modèles)

```
✅ Presence (FAIT)
⏳ Eleve → EleveService.cs, IEleveRepository.cs, EleveController.cs
⏳ Utilisateur → UtilisateurService.cs, IUtilisateurRepository.cs, UtilisateurController.cs
⏳ Inscription → InscriptionService.cs, IInscriptionRepository.cs, InscriptionController.cs
⏳ Note → NoteService.cs, INoteRepository.cs, NoteController.cs
⏳ Cours → CoursService.cs, ICoursRepository.cs, CoursController.cs
```

### P1 - Importants (4 modèles)

```
⏳ Enseignant → EnseignantService.cs, IEnseignantRepository.cs, EnseignantController.cs
⏳ Tuteur → TuteurService.cs, ITuteurRepository.cs, TuteurController.cs
⏳ Classe → ClasseService.cs, IClasseRepository.cs, ClasseController.cs
⏳ Ecole → EcoleService.cs, IEcoleRepository.cs, EcoleController.cs
```

### P2 - Standards (10 modèles)

```
⏳ Document, AffectationCours, Horaire, GroupeMessage, Frais
⏳ RessourcePedagogique, Message, Evaluation, Vacation, Paiement
```

### P3 - Configuration (6 modèles)

```
⏳ AnneeScolaire, Option, Role, Direction, Section, Notification
```

---

## 🔧 FILTRAGE OPTIONNEL

**Important** : Certains services ont déjà des méthodes qui filtrent les données. 

Si vous voulez que les `GET` retournent **uniquement les éléments actifs**, ajoutez :

```csharp
.Where(x => x.Statut == true)
```

**Exemple:**

```csharp
// AVANT
public async Task<Eleve> GetByIdAsync(int id)
{
    return await _context.Eleves
        .Include(e => e.Classe)
        .Include(e => e.Tuteur)
        .FirstOrDefaultAsync(e => e.IdEleve == id);
}

// APRÈS
public async Task<Eleve> GetByIdAsync(int id)
{
    return await _context.Eleves
        .Include(e => e.Classe)
        .Include(e => e.Tuteur)
        .Where(e => e.Statut == true) // ✅ AJOUTER
        .FirstOrDefaultAsync(e => e.IdEleve == id);
}
```

**À ajouter dans:**
- `GetAllAsync()`
- `GetByIdAsync()`
- Tous les autres GET

---

## ⚠️ CAS SPÉCIAUX

### Vues (V_Eleve, V_Utilisateur)

Les **vues** ne peuvent pas être filtrées directement dans le code. Le filtrage doit être fait dans la définition SQL de la vue.

**Solution:** 
- Recréer la vue avec `WHERE Statut = 1`
- OU filtrer après lecture : `.Where(x => x.Statut == true)`

---

## 🧪 TEST RAPIDE

Créer `test-toggle-all.http` :

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

### Test Note
PUT http://localhost:5002/api/Note/toggle-statut/1
GET http://localhost:5002/api/Note

### Test Cours
PUT http://localhost:5002/api/Cours/toggle-statut/1
GET http://localhost:5002/api/Cours
```

---

## 📊 TABLEAU DE SUIVI

| Modèle | Service ✅ | Interface ✅ | Controller ✅ | Testé ✅ |
|--------|-----------|-------------|--------------|----------|
| Presence | ✅ | ✅ | ✅ | ✅ |
| Eleve | ⏳ | ⏳ | ⏳ | ⏳ |
| Utilisateur | ⏳ | ⏳ | ⏳ | ⏳ |
| Inscription | ⏳ | ⏳ | ⏳ | ⏳ |
| Note | ⏳ | ⏳ | ⏳ | ⏳ |
| Cours | ⏳ | ⏳ | ⏳ | ⏳ |
| Enseignant | ⏳ | ⏳ | ⏳ | ⏳ |
| Tuteur | ⏳ | ⏳ | ⏳ | ⏳ |
| Classe | ⏳ | ⏳ | ⏳ | ⏳ |
| Ecole | ⏳ | ⏳ | ⏳ | ⏳ |
| ... | ⏳ | ⏳ | ⏳ | ⏳ |

---

## 💡 CONSEILS

### ✅ Ordre Recommandé

1. Commencer par **Eleve** (très utilisé)
2. Puis **Utilisateur** (critique)
3. Puis **Inscription** (important)
4. Puis le reste selon priorité

### ⚡ Gain de Temps

- Utiliser Find & Replace dans VSCode
- Créer un snippet VSCode pour les 3 blocs de code
- Copier-coller depuis Presence (déjà fait)

### 🔍 Vérification

Après chaque modèle :
```powershell
# Compiler
dotnet build

# Vérifier les erreurs
# Corriger si nécessaire
```

---

## 🎯 ESTIMATION

| Phase | Modèles | Temps |
|-------|---------|-------|
| P0 (Critiques) | 5 modèles | 1h |
| P1 (Importants) | 4 modèles | 40min |
| P2 (Standards) | 10 modèles | 1.5h |
| P3 (Config) | 6 modèles | 1h |
| **TOTAL** | **25 modèles** | **~4h** |

---

## 🚀 COMMANDE FINALE

Une fois tous les modèles traités :

```powershell
# Compiler
dotnet build

# Lancer l'API
dotnet run

# Tester dans Swagger
# http://localhost:5002/swagger
# Chercher "toggle-statut" dans chaque contrôleur
```

---

## 📚 DOCUMENTATION

- **Guide complet** : `IMPLEMENTATION_SOFT_DELETE_GLOBAL.md`
- **Exemple Presence** : `IMPLEMENTATION_SOFT_DELETE_PRESENCE.md`
- **Ce guide** : `QUICK_START_SOFT_DELETE_GLOBAL.md`

---

**Date:** 16 Octobre 2025  
**Status:** ✅ GUIDE PRÊT  
**Auteur:** Assistant IA

---

**🚀 PRÊT À IMPLÉMENTER !**

**Commencez par Eleve, puis continuez avec les autres modèles selon vos priorités.**

