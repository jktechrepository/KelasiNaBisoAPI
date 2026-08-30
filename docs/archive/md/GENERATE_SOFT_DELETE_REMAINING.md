# 🤖 GÉNÉRATEUR DE CODE - SOFT DELETE POUR 16 MODÈLES RESTANTS

**Date:** 16 Octobre 2025  
**Usage:** Copier-coller rapide pour les 16 modèles P2+P3

---

## 🎯 MODE D'EMPLOI

Pour chaque modèle ci-dessous :
1. Ouvrir les 3 fichiers (Service, Interface, Controller)
2. Copier-coller les snippets fournis
3. Compiler après chaque modèle
4. Passer au suivant

---

## 📦 MODÈLE 1 : DOCUMENT

### DocumentService.cs (Ajouter AVANT la dernière accolade)

```csharp
        // ✅ SOFT DELETE: Toggle le statut d'un document (actif <-> inactif)
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
                return false;

            document.Statut = !document.Statut;
            await _context.SaveChangesAsync();
            return true;
        }
```

### IDocumentRepository.cs (Ajouter AVANT la dernière accolade)

```csharp
        // ✅ SOFT DELETE
        Task<bool> ToggleStatutAsync(int id);
```

### DocumentController.cs (Ajouter AVANT la dernière accolade)

```csharp
        // PUT: api/Document/toggle-statut/{id}
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _documentRepository.ToggleStatutAsync(id);
                if (!success)
                    return NotFound(new { message = "Document non trouvé" });

                var document = await _documentRepository.GetByIdAsync(id);
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = document != null,
                    document = document
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur", error = ex.Message });
            }
        }
```

### Filtrage GetAllAsync et GetByIdAsync (Ajouter .Where)

```csharp
// Dans GetAllAsync()
.Where(d => d.Statut == true) // ✅ AJOUTER avant .ToListAsync()

// Dans GetByIdAsync()
.Where(d => d.Statut == true) // ✅ AJOUTER avant .FirstOrDefaultAsync()
```

---

## 📦 MODÈLE 2 : AFFECTATIONCOURS

### AffectationCoursService.cs

```csharp
        // ✅ SOFT DELETE
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var affectation = await _context.AffectationsCours.FindAsync(id);
            if (affectation == null)
                return false;

            affectation.Statut = !affectation.Statut;
            await _context.SaveChangesAsync();
            return true;
        }
```

### IAffectationCoursRepository.cs

```csharp
        Task<bool> ToggleStatutAsync(int id);
```

### AffectationCoursController.cs

```csharp
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _affectationCoursRepository.ToggleStatutAsync(id);
                if (!success)
                    return NotFound(new { message = "Affectation non trouvée" });

                var affectation = await _affectationCoursRepository.GetByIdAsync(id);
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = affectation != null,
                    affectation = affectation
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur", error = ex.Message });
            }
        }
```

---

## 📦 MODÈLE 3 : FRAIS

### FraisService.cs

```csharp
        // ✅ SOFT DELETE
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var frais = await _context.Frais.FindAsync(id);
            if (frais == null)
                return false;

            frais.Statut = !frais.Statut;
            await _context.SaveChangesAsync();
            return true;
        }
```

### IFraisRepository.cs

```csharp
        Task<bool> ToggleStatutAsync(int id);
```

### FraisController.cs

```csharp
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _fraisRepository.ToggleStatutAsync(id);
                if (!success)
                    return NotFound(new { message = "Frais non trouvé" });

                var frais = await _fraisRepository.GetByIdAsync(id);
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = frais != null,
                    frais = frais
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur", error = ex.Message });
            }
        }
```

---

## 📦 MODÈLE 4 : GROUPEMESSAGE

### GroupeMessageService.cs

```csharp
        // ✅ SOFT DELETE
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var groupe = await _context.GroupeMessages.FindAsync(id);
            if (groupe == null)
                return false;

            groupe.Statut = !groupe.Statut;
            await _context.SaveChangesAsync();
            return true;
        }
```

### IGroupeMessageRepository.cs

```csharp
        Task<bool> ToggleStatutAsync(int id);
```

### GroupeMessageController.cs

```csharp
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _groupeMessageRepository.ToggleStatutAsync(id);
                if (!success)
                    return NotFound(new { message = "Groupe de messages non trouvé" });

                var groupe = await _groupeMessageRepository.GetByIdAsync(id);
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = groupe != null,
                    groupeMessage = groupe
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur", error = ex.Message });
            }
        }
```

---

## 📦 MODÈLE 5 : RESSOURCEPEDAGOGIQUE

### RessourcePedagogiqueService.cs

```csharp
        // ✅ SOFT DELETE
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var ressource = await _context.RessourcePedagogiques.FindAsync(id);
            if (ressource == null)
                return false;

            ressource.Statut = !ressource.Statut;
            await _context.SaveChangesAsync();
            return true;
        }
```

### IRessourcePedagogiqueRepository.cs

```csharp
        Task<bool> ToggleStatutAsync(int id);
```

### RessourcePedagogiqueController.cs

```csharp
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _ressourcePedagogiqueRepository.ToggleStatutAsync(id);
                if (!success)
                    return NotFound(new { message = "Ressource non trouvée" });

                var ressource = await _ressourcePedagogiqueRepository.GetByIdAsync(id);
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = ressource != null,
                    ressource = ressource
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur", error = ex.Message });
            }
        }
```

---

## 📦 MODÈLE 6 : MESSAGE

### MessageService.cs

```csharp
        // ✅ SOFT DELETE
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
                return false;

            message.Statut = !message.Statut;
            await _context.SaveChangesAsync();
            return true;
        }
```

### IMessageRepository.cs

```csharp
        Task<bool> ToggleStatutAsync(int id);
```

### MessageController.cs

```csharp
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _messageRepository.ToggleStatutAsync(id);
                if (!success)
                    return NotFound(new { message = "Message non trouvé" });

                var message = await _messageRepository.GetByIdAsync(id);
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = message != null,
                    messageData = message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur", error = ex.Message });
            }
        }
```

---

## 📦 MODÈLE 7 : EVALUATION

### EvaluationService.cs

```csharp
        // ✅ SOFT DELETE
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var evaluation = await _context.Evaluations.FindAsync(id);
            if (evaluation == null)
                return false;

            evaluation.Statut = !evaluation.Statut;
            await _context.SaveChangesAsync();
            return true;
        }
```

### IEvaluationRepository.cs

```csharp
        Task<bool> ToggleStatutAsync(int id);
```

### EvaluationController.cs

```csharp
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _evaluationRepository.ToggleStatutAsync(id);
                if (!success)
                    return NotFound(new { message = "Évaluation non trouvée" });

                var evaluation = await _evaluationRepository.GetByIdAsync(id);
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = evaluation != null,
                    evaluation = evaluation
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur", error = ex.Message });
            }
        }
```

---

## 📦 MODÈLE 8 : VACATION

### VacationService.cs

```csharp
        // ✅ SOFT DELETE
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var vacation = await _context.Vacations.FindAsync(id);
            if (vacation == null)
                return false;

            vacation.Statut = !vacation.Statut;
            await _context.SaveChangesAsync();
            return true;
        }
```

### IVacationRepository.cs

```csharp
        Task<bool> ToggleStatutAsync(int id);
```

### VacationController.cs

```csharp
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _vacationRepository.ToggleStatutAsync(id);
                if (!success)
                    return NotFound(new { message = "Vacation non trouvée" });

                var vacation = await _vacationRepository.GetByIdAsync(id);
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = vacation != null,
                    vacation = vacation
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur", error = ex.Message });
            }
        }
```

---

## 📦 MODÈLE 9 : PAIEMENT

### PaiementService.cs

```csharp
        // ✅ SOFT DELETE
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var paiement = await _context.Paiements.FindAsync(id);
            if (paiement == null)
                return false;

            paiement.Statut = !paiement.Statut;
            await _context.SaveChangesAsync();
            return true;
        }
```

### IPaiementRepository.cs

```csharp
        Task<bool> ToggleStatutAsync(int id);
```

### PaiementController.cs

```csharp
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _paiementRepository.ToggleStatutAsync(id);
                if (!success)
                    return NotFound(new { message = "Paiement non trouvé" });

                var paiement = await _paiementRepository.GetByIdAsync(id);
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = paiement != null,
                    paiement = paiement
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur", error = ex.Message });
            }
        }
```

---

## 📦 MODÈLE 10 : HORAIRE

### HoraireService.cs

```csharp
        // ✅ SOFT DELETE
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var horaire = await _context.Horaires.FindAsync(id);
            if (horaire == null)
                return false;

            horaire.Statut = !horaire.Statut;
            await _context.SaveChangesAsync();
            return true;
        }
```

### IHoraireRepository.cs

```csharp
        Task<bool> ToggleStatutAsync(int id);
```

### HoraireController.cs

```csharp
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _horaireRepository.ToggleStatutAsync(id);
                if (!success)
                    return NotFound(new { message = "Horaire non trouvé" });

                var horaire = await _horaireRepository.GetByIdAsync(id);
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = horaire != null,
                    horaire = horaire
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur", error = ex.Message });
            }
        }
```

---

## 📦 MODÈLE 11 : ANNEESCOLAIRE

### AnneeScolaireService.cs

```csharp
        // ✅ SOFT DELETE
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var annee = await _context.AnnesScolaires.FindAsync(id);
            if (annee == null)
                return false;

            annee.Statut = !annee.Statut;
            await _context.SaveChangesAsync();
            return true;
        }
```

### IAnneeScolaireRepository.cs

```csharp
        Task<bool> ToggleStatutAsync(int id);
```

### AnneeScolaireController.cs

```csharp
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _anneeScolaireRepository.ToggleStatutAsync(id);
                if (!success)
                    return NotFound(new { message = "Année scolaire non trouvée" });

                var annee = await _anneeScolaireRepository.GetByIdAsync(id);
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = annee != null,
                    anneeScolaire = annee
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur", error = ex.Message });
            }
        }
```

---

## 📦 MODÈLE 12 : OPTION

### OptionService.cs

```csharp
        // ✅ SOFT DELETE
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var option = await _context.Options.FindAsync(id);
            if (option == null)
                return false;

            option.Statut = !option.Statut;
            await _context.SaveChangesAsync();
            return true;
        }
```

### IOptionRepository.cs

```csharp
        Task<bool> ToggleStatutAsync(int id);
```

### OptionController.cs

```csharp
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _optionRepository.ToggleStatutAsync(id);
                if (!success)
                    return NotFound(new { message = "Option non trouvée" });

                var option = await _optionRepository.GetByIdAsync(id);
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = option != null,
                    option = option
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur", error = ex.Message });
            }
        }
```

---

## 📦 MODÈLE 13 : ROLE

### RoleService.cs

```csharp
        // ✅ SOFT DELETE
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
                return false;

            role.Statut = !role.Statut;
            await _context.SaveChangesAsync();
            return true;
        }
```

### IRoleRepository.cs

```csharp
        Task<bool> ToggleStatutAsync(int id);
```

### RoleController.cs

```csharp
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _roleRepository.ToggleStatutAsync(id);
                if (!success)
                    return NotFound(new { message = "Rôle non trouvé" });

                var role = await _roleRepository.GetByIdAsync(id);
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = role != null,
                    role = role
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur", error = ex.Message });
            }
        }
```

---

## 📦 MODÈLE 14 : DIRECTION

### DirectionService.cs

```csharp
        // ✅ SOFT DELETE
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var direction = await _context.Directions.FindAsync(id);
            if (direction == null)
                return false;

            direction.Statut = !direction.Statut;
            await _context.SaveChangesAsync();
            return true;
        }
```

### IDirectionRepository.cs

```csharp
        Task<bool> ToggleStatutAsync(int id);
```

### DirectionController.cs

```csharp
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _directionRepository.ToggleStatutAsync(id);
                if (!success)
                    return NotFound(new { message = "Direction non trouvée" });

                var direction = await _directionRepository.GetByIdAsync(id);
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = direction != null,
                    direction = direction
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur", error = ex.Message });
            }
        }
```

---

## 📦 MODÈLE 15 : SECTION

### SectionService.cs

```csharp
        // ✅ SOFT DELETE
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var section = await _context.Sections.FindAsync(id);
            if (section == null)
                return false;

            section.Statut = !section.Statut;
            await _context.SaveChangesAsync();
            return true;
        }
```

### ISectionRepository.cs

```csharp
        Task<bool> ToggleStatutAsync(int id);
```

### SectionController.cs

```csharp
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _sectionRepository.ToggleStatutAsync(id);
                if (!success)
                    return NotFound(new { message = "Section non trouvée" });

                var section = await _sectionRepository.GetByIdAsync(id);
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = section != null,
                    section = section
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur", error = ex.Message });
            }
        }
```

---

## 📦 MODÈLE 16 : NOTIFICATION

### NotificationService.cs

```csharp
        // ✅ SOFT DELETE
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
                return false;

            notification.Statut = !notification.Statut;
            await _context.SaveChangesAsync();
            return true;
        }
```

### INotificationRepository.cs

```csharp
        Task<bool> ToggleStatutAsync(int id);
```

### NotificationController.cs

```csharp
        [HttpPut("toggle-statut/{id}")]
        public async Task<ActionResult<object>> ToggleStatut(int id)
        {
            try
            {
                var success = await _notificationRepository.ToggleStatutAsync(id);
                if (!success)
                    return NotFound(new { message = "Notification non trouvée" });

                var notification = await _notificationRepository.GetByIdAsync(id);
                return Ok(new { 
                    message = "Statut modifié avec succès",
                    nouveauStatut = notification != null,
                    notification = notification
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur", error = ex.Message });
            }
        }
```

---

## ⚡ ORDRE D'EXÉCUTION RAPIDE

### Batch 1 (20 minutes)

```
1. ✅ Document → Copier-coller les 3 snippets + filtrage
2. ✅ AffectationCours → Copier-coller les 3 snippets + filtrage
3. ✅ Frais → Copier-coller les 3 snippets + filtrage
4. ✅ GroupeMessage → Copier-coller les 3 snippets + filtrage
5. ✅ RessourcePedagogique → Copier-coller les 3 snippets + filtrage

dotnet build (vérifier)
```

### Batch 2 (20 minutes)

```
6. ✅ Message → Copier-coller les 3 snippets + filtrage
7. ✅ Evaluation → Copier-coller les 3 snippets + filtrage
8. ✅ Vacation → Copier-coller les 3 snippets + filtrage
9. ✅ Paiement → Copier-coller les 3 snippets + filtrage
10. ✅ Horaire → Copier-coller les 3 snippets + filtrage

dotnet build (vérifier)
```

### Batch 3 (20 minutes)

```
11. ✅ AnneeScolaire → Copier-coller les 3 snippets + filtrage
12. ✅ Option → Copier-coller les 3 snippets + filtrage
13. ✅ Role → Copier-coller les 3 snippets + filtrage
14. ✅ Direction → Copier-coller les 3 snippets + filtrage
15. ✅ Section → Copier-coller les 3 snippets + filtrage
16. ✅ Notification → Copier-coller les 3 snippets + filtrage

dotnet build (vérifier)
```

**TOTAL : ~1h pour les 16 modèles**

---

## ✅ CHECKLIST FINALE (Pour Chaque Modèle)

```
□ Copier snippet ToggleStatutAsync dans Service
□ Copier signature dans Interface
□ Copier endpoint dans Controller
□ Ajouter .Where(x => x.Statut == true) dans GetAllAsync()
□ Ajouter .Where(x => x.Statut == true) dans GetByIdAsync()
□ Compiler : dotnet build
□ Vérifier aucune erreur
□ Passer au suivant
```

---

## 🎯 APRÈS COMPLÉTION DES 16 MODÈLES

```
✅ 26/26 modèles avec soft delete
✅ 26 endpoints toggle-statut
✅ API 100% cohérente
✅ Historique préservé partout
✅ Audit trail complet
```

---

**Date:** 16 Octobre 2025  
**Usage:** Copier-coller les snippets ci-dessus dans les fichiers correspondants  
**Temps estimé:** ~1h pour les 16 modèles

---

**🚀 PRÊT POUR COPIER-COLLER !**

**Ouvrez ce fichier côte à côte avec VS Code et commencez le copier-coller systématique !**

