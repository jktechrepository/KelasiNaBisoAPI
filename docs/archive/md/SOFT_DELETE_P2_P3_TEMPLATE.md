# 🔄 SOFT DELETE P2+P3 - TEMPLATE RAPIDE
## 16 Modèles Restants

**Date:** 16 Octobre 2025  
**Status:** Template pour implémentation rapide

---

## 📋 MODÈLES À TRAITER (16)

### P2 - Standards (10 modèles)

1. ⏳ Document
2. ⏳ AffectationCours
3. ⏳ Horaire (Vacation)
4. ⏳ GroupeMessage
5. ⏳ Frais
6. ⏳ RessourcePedagogique
7. ⏳ Message
8. ⏳ Evaluation
9. ⏳ Vacation
10. ⏳ Paiement

### P3 - Configuration (6 modèles)

11. ⏳ AnneeScolaire
12. ⏳ Option
13. ⏳ Role
14. ⏳ Direction
15. ⏳ Section
16. ⏳ Notification

---

## ⚡ TEMPLATE RAPIDE (Copier-Coller)

Pour chaque modèle ci-dessous, appliquer les 3 modifications :

### Modification 1️⃣ : Service (Ajouter à la fin)

```csharp
// ✅ SOFT DELETE: Toggle le statut d'un {modele} (actif <-> inactif)
public async Task<bool> ToggleStatutAsync(int id)
{
    var {variable} = await _context.{Table}.FindAsync(id);
    if ({variable} == null)
        return false;

    {variable}.Statut = !{variable}.Statut;
    await _context.SaveChangesAsync();
    return true;
}
```

### Modification 2️⃣ : Interface (Ajouter)

```csharp
// ✅ SOFT DELETE
Task<bool> ToggleStatutAsync(int id);
```

### Modification 3️⃣ : Controller (Ajouter à la fin)

```csharp
// PUT: api/{Controller}/toggle-statut/{id}
[HttpPut("toggle-statut/{id}")]
public async Task<ActionResult<object>> ToggleStatut(int id)
{
    try
    {
        var success = await _{repository}.ToggleStatutAsync(id);
        if (!success)
        {
            return NotFound(new { message = "{Modele} non trouvé" });
        }

        var {variable} = await _{repository}.GetByIdAsync(id);
        var estActif = {variable} != null;
        
        return Ok(new { 
            message = "Statut modifié avec succès",
            nouveauStatut = estActif,
            {variable} = {variable}
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = "Erreur lors du changement de statut", error = ex.Message });
    }
}
```

### Modification 4️⃣ : Ajouter filtrage dans GetAllAsync et GetByIdAsync

```csharp
// Dans GetAllAsync()
.Where(x => x.Statut == true) // ✅ AJOUTER

// Dans GetByIdAsync()
.Where(x => x.Statut == true) // ✅ AJOUTER
```

---

## 📝 VALEURS DE REMPLACEMENT PAR MODÈLE

### 1. Document

```
{modele} / {Modele} : document / Document
{variable} : document
{Table} : Documents
{repository} : documentRepository
{Controller} : Document
Service : DocumentService.cs
Interface : IDocumentRepository.cs
Controller : DocumentController.cs
```

### 2. AffectationCours

```
{modele} / {Modele} : affectation / Affectation de cours
{variable} : affectation
{Table} : AffectationsCours
{repository} : affectationCoursRepository
{Controller} : AffectationCours
Service : AffectationCoursService.cs
Interface : IAffectationCoursRepository.cs
Controller : AffectationCoursController.cs
```

### 3. Horaire (Vacation)

```
{modele} / {Modele} : horaire / Horaire
{variable} : horaire
{Table} : Horaires
{repository} : horaireRepository
{Controller} : Horaire
Service : HoraireService.cs
Interface : IHoraireRepository.cs
Controller : HoraireController.cs
```

### 4. GroupeMessage

```
{modele} / {Modele} : groupe message / Groupe de messages
{variable} : groupeMessage
{Table} : GroupesMessages
{repository} : groupeMessageRepository
{Controller} : GroupeMessage
Service : GroupeMessageService.cs
Interface : IGroupeMessageRepository.cs
Controller : GroupeMessageController.cs
```

### 5. Frais

```
{modele} / {Modele} : frais / Frais
{variable} : frais
{Table} : Frais
{repository} : fraisRepository
{Controller} : Frais
Service : FraisService.cs
Interface : IFraisRepository.cs
Controller : FraisController.cs
```

### 6. RessourcePedagogique

```
{modele} / {Modele} : ressource / Ressource pédagogique
{variable} : ressource
{Table} : RessourcesPedagogiques
{repository} : ressourcePedagogiqueRepository
{Controller} : RessourcePedagogique
Service : RessourcePedagogiqueService.cs
Interface : IRessourcePedagogiqueRepository.cs
Controller : RessourcePedagogiqueController.cs
```

### 7. Message

```
{modele} / {Modele} : message / Message
{variable} : message
{Table} : Messages
{repository} : messageRepository
{Controller} : Message
Service : MessageService.cs
Interface : IMessageRepository.cs
Controller : MessageController.cs
```

### 8. Evaluation

```
{modele} / {Modele} : évaluation / Évaluation
{variable} : evaluation
{Table} : Evaluations
{repository} : evaluationRepository
{Controller} : Evaluation
Service : EvaluationService.cs
Interface : IEvaluationRepository.cs
Controller : EvaluationController.cs
```

### 9. Vacation

```
{modele} / {Modele} : vacation / Vacation
{variable} : vacation
{Table} : Vacations
{repository} : vacationRepository
{Controller} : Vacation
Service : VacationService.cs
Interface : IVacationRepository.cs
Controller : VacationController.cs
```

### 10. Paiement

```
{modele} / {Modele} : paiement / Paiement
{variable} : paiement
{Table} : Paiements
{repository} : paiementRepository
{Controller} : Paiement
Service : PaiementService.cs
Interface : IPaiementRepository.cs
Controller : PaiementController.cs
```

### 11. AnneeScolaire

```
{modele} / {Modele} : année scolaire / Année scolaire
{variable} : anneeScolaire
{Table} : AnnesScolaires
{repository} : anneeScolaireRepository
{Controller} : AnneeScolaire
Service : AnneeScolaireService.cs
Interface : IAnneeScolaireRepository.cs
Controller : AnneeScolaireController.cs
```

### 12. Option

```
{modele} / {Modele} : option / Option
{variable} : option
{Table} : Options
{repository} : optionRepository
{Controller} : Option
Service : OptionService.cs
Interface : IOptionRepository.cs
Controller : OptionController.cs
```

### 13. Role

```
{modele} / {Modele} : rôle / Rôle
{variable} : role
{Table} : Roles
{repository} : roleRepository
{Controller} : Role
Service : RoleService.cs
Interface : IRoleRepository.cs
Controller : RoleController.cs
```

### 14. Direction

```
{modele} / {Modele} : direction / Direction
{variable} : direction
{Table} : Directions
{repository} : directionRepository
{Controller} : Direction
Service : DirectionService.cs
Interface : IDirectionRepository.cs
Controller : DirectionController.cs
```

### 15. Section

```
{modele} / {Modele} : section / Section
{variable} : section
{Table} : Sections
{repository} : sectionRepository
{Controller} : Section
Service : SectionService.cs
Interface : ISectionRepository.cs
Controller : SectionController.cs
```

### 16. Notification

```
{modele} / {Modele} : notification / Notification
{variable} : notification
{Table} : Notifications
{repository} : notificationRepository
{Controller} : Notification
Service : NotificationService.cs
Interface : INotificationRepository.cs
Controller : NotificationController.cs
```

---

## ⏱️ ESTIMATION TEMPS

| Modèle | Temps | Difficulté |
|--------|-------|------------|
| 1-5 | 10 min/modèle | Facile |
| 6-10 | 8 min/modèle | Facile |
| 11-16 | 8 min/modèle | Facile |
| **TOTAL** | **~2h30** | |

---

## ✅ CHECKLIST PAR MODÈLE

Pour chaque modèle :

```
□ Service : Ajouter ToggleStatutAsync() à la fin
□ Service : Ajouter .Where(x => x.Statut == true) dans GetAllAsync()
□ Service : Ajouter .Where(x => x.Statut == true) dans GetByIdAsync()
□ Interface : Ajouter Task<bool> ToggleStatutAsync(int id);
□ Controller : Ajouter endpoint PUT /toggle-statut/{id}
□ Compiler : dotnet build (vérifier aucune erreur)
```

---

## 🚀 ORDRE RECOMMANDÉ

### Batch 1 (P2-A) - 30 min

```
1. Document
2. AffectationCours
3. Horaire
4. GroupeMessage
5. Frais
```

### Batch 2 (P2-B) - 30 min

```
6. RessourcePedagogique
7. Message
8. Evaluation
9. Vacation
10. Paiement
```

### Batch 3 (P3) - 40 min

```
11. AnneeScolaire
12. Option
13. Role
14. Direction
15. Section
16. Notification
```

**Compiler après chaque batch !**

---

**Date:** 16 Octobre 2025  
**Status:** ✅ Template prêt pour implémentation

---

**Temps estimé restant : 2h30 pour les 16 modèles**

