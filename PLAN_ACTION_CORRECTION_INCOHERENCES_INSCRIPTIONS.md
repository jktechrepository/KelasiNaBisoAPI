# 📋 Plan d'Action : Correction des Incohérences Inscriptions/Élèves

**Date** : 2025-01-16  
**Version** : 1.0  
**Objectif** : Garantir la cohérence entre les inscriptions actives et les élèves actifs

---

## 🎯 Objectif

Corriger l'incohérence où le nombre d'inscriptions actives peut être supérieur au nombre d'élèves actifs, en raison de l'absence de cascade automatique lors du soft delete des élèves.

---

## 📊 Phases du Plan

### **Phase 1 : Diagnostic** ✅ (FAIT)

**Durée estimée** : 15 minutes

**Actions** :
- [x] Créer le script SQL de diagnostic
- [x] Documenter l'analyse de l'existant
- [ ] Exécuter le script SQL en production (à faire par l'utilisateur)
- [ ] Analyser les résultats

**Livrables** :
- ✅ `SCRIPTS_SQL/identifier_incohérences_inscriptions_eleves_inactifs.sql`
- ✅ `ANALYSE_INCOHERENCES_INSCRIPTIONS_ELEVES.md`

---

### **Phase 2 : Correction Immédiate (Filtrage)** 🔄

**Durée estimée** : 1-2 heures

**Objectif** : Modifier toutes les requêtes d'inscriptions pour filtrer sur `Eleve.Statut == true`

**Actions** :

#### **2.1. Modifier InscriptionService.GetAllPagedAsync**

**Fichier** : `Services/InscriptionService.cs` (Ligne ~948)

**Avant** :
```csharp
var query = _context.Inscriptions
    .Include(i => i.Eleve)
    .Include(i => i.Classe)
    .Include(i => i.Ecole)
    .Include(i => i.AnneeScolaire)
    .AsQueryable();

if (!request.IncludeInactive)
{
    query = query.Where(i => i.Statut == true);
}
```

**Après** :
```csharp
var query = _context.Inscriptions
    .Include(i => i.Eleve)
    .Include(i => i.Classe)
    .Include(i => i.Ecole)
    .Include(i => i.AnneeScolaire)
    .AsQueryable();

if (!request.IncludeInactive)
{
    query = query
        .Where(i => i.Statut == true)
        .Where(i => i.Eleve.Statut == true); // ✅ NOUVEAU
}
```

---

#### **2.2. Modifier InscriptionService.GetByElevePagedAsync**

**Fichier** : `Services/InscriptionService.cs` (Ligne ~991)

**Avant** :
```csharp
var query = _context.Inscriptions
    .Include(i => i.Classe)
    .Include(i => i.Ecole)
    .Include(i => i.AnneeScolaire)
    .Where(i => i.IdEleve == idEleve)
    .AsQueryable();

if (!request.IncludeInactive)
{
    query = query.Where(i => i.Statut == true);
}
```

**Après** :
```csharp
var query = _context.Inscriptions
    .Include(i => i.Eleve) // ✅ AJOUTER Include pour Eleve
    .Include(i => i.Classe)
    .Include(i => i.Ecole)
    .Include(i => i.AnneeScolaire)
    .Where(i => i.IdEleve == idEleve)
    .AsQueryable();

if (!request.IncludeInactive)
{
    query = query
        .Where(i => i.Statut == true)
        .Where(i => i.Eleve.Statut == true); // ✅ NOUVEAU
}
```

---

#### **2.3. Modifier InscriptionService.GetByEcolePagedAsync**

**Fichier** : `Services/InscriptionService.cs` (Ligne ~1032)

**Avant** :
```csharp
var query = _context.Inscriptions
    .Include(i => i.Eleve)
    .Include(i => i.Classe)
    .Include(i => i.AnneeScolaire)
    .Where(i => i.IdEcole == idEcole)
    .Where(i => i.Statut == true) // Déjà filtré
    .AsQueryable();
```

**Après** :
```csharp
var query = _context.Inscriptions
    .Include(i => i.Eleve)
    .Include(i => i.Classe)
    .Include(i => i.AnneeScolaire)
    .Where(i => i.IdEcole == idEcole)
    .Where(i => i.Statut == true)
    .Where(i => i.Eleve.Statut == true) // ✅ NOUVEAU
    .AsQueryable();
```

---

#### **2.4. Modifier InscriptionService.GetByClassePagedAsync**

**Fichier** : `Services/InscriptionService.cs` (Ligne ~1070)

**Avant** :
```csharp
var query = _context.Inscriptions
    .Include(i => i.Eleve)
    .Include(i => i.Ecole)
    .Include(i => i.AnneeScolaire)
    .Where(i => i.IdClasse == idClasse)
    .AsQueryable();

if (!request.IncludeInactive)
{
    query = query.Where(i => i.Statut == true);
}
```

**Après** :
```csharp
var query = _context.Inscriptions
    .Include(i => i.Eleve)
    .Include(i => i.Ecole)
    .Include(i => i.AnneeScolaire)
    .Where(i => i.IdClasse == idClasse)
    .AsQueryable();

if (!request.IncludeInactive)
{
    query = query
        .Where(i => i.Statut == true)
        .Where(i => i.Eleve.Statut == true); // ✅ NOUVEAU
}
```

---

#### **2.5. Modifier InscriptionService.GetByStatutPagedAsync**

**Fichier** : `Services/InscriptionService.cs` (Ligne ~1113)

**Avant** :
```csharp
var query = _context.Inscriptions
    .Include(i => i.Eleve)
    .Include(i => i.Classe)
    .Include(i => i.Ecole)
    .Include(i => i.AnneeScolaire)
    .Where(i => i.Statut == statut)
    .AsQueryable();
```

**Après** :
```csharp
var query = _context.Inscriptions
    .Include(i => i.Eleve)
    .Include(i => i.Classe)
    .Include(i => i.Ecole)
    .Include(i => i.AnneeScolaire)
    .Where(i => i.Statut == statut)
    .Where(i => i.Eleve.Statut == true) // ✅ NOUVEAU : Toujours filtrer sur Eleve.Statut
    .AsQueryable();
```

---

#### **2.6. Modifier InscriptionService.GetByStatutAsync**

**Fichier** : `Services/InscriptionService.cs` (Ligne ~935)

**Avant** :
```csharp
return await _context.Inscriptions
    .Include(i => i.Eleve)
    .Include(i => i.Classe)
    .Include(i => i.Ecole)
    .Include(i => i.AnneeScolaire)
    .Where(i => i.Statut == statut)
    .OrderByDescending(i => i.DateInscription)
    .ToListAsync();
```

**Après** :
```csharp
return await _context.Inscriptions
    .Include(i => i.Eleve)
    .Include(i => i.Classe)
    .Include(i => i.Ecole)
    .Include(i => i.AnneeScolaire)
    .Where(i => i.Statut == statut)
    .Where(i => i.Eleve.Statut == true) // ✅ NOUVEAU
    .OrderByDescending(i => i.DateInscription)
    .ToListAsync();
```

---

#### **2.7. Vérifier les autres méthodes**

**Méthodes à vérifier** :
- `GetAllAsync` (si elle existe)
- `GetByEcoleAsync` (si elle existe)
- `GetByClasseAsync` (si elle existe)
- `GetByEleveAsync` (si elle existe)

**Action** : Ajouter le filtre `i.Eleve.Statut == true` dans toutes ces méthodes.

---

**Tests à effectuer** :
- [ ] Tester `GetAllPagedAsync` avec et sans `IncludeInactive`
- [ ] Tester `GetByEcolePagedAsync`
- [ ] Tester `GetByClassePagedAsync`
- [ ] Tester `GetByElevePagedAsync`
- [ ] Vérifier que les rapports sont cohérents

---

### **Phase 3 : Correction des Données Existantes** 🔄

**Durée estimée** : 30 minutes (après validation)

**Objectif** : Désactiver toutes les inscriptions d'élèves inactifs dans la base de données

**Actions** :

#### **3.1. Créer le script SQL de correction**

**Fichier à créer** : `SCRIPTS_SQL/corriger_incohérences_inscriptions_eleves_inactifs.sql`

**Contenu** :
```sql
-- ============================================================================
-- SCRIPT SQL : Corriger les Incohérences entre Inscriptions et Élèves
-- ============================================================================
-- Description : Désactive toutes les inscriptions actives dont l'élève 
--               associé est inactif
-- 
-- ⚠️ AVERTISSEMENT : Faire un BACKUP avant d'exécuter ce script
-- ============================================================================

-- ═══════════════════════════════════════════════════════════════════════════
-- 1. AFFICHER LES INSCRIPTIONS QUI SERONT DÉSACTIVÉES (VÉRIFICATION)
-- ═══════════════════════════════════════════════════════════════════════════

SELECT 
    i.IdInscription,
    i.IdEleve,
    e.NomComplet AS NomEleve,
    e.Statut AS StatutEleve,
    i.Statut AS StatutInscriptionAvant,
    ec.Nom AS NomEcole
FROM 
    Inscriptions i
    INNER JOIN Eleves e ON i.IdEleve = e.IdEleve
    LEFT JOIN Ecoles ec ON i.IdEcole = ec.IdEcole
WHERE 
    (i.Statut = 1 OR i.Statut IS NULL)
    AND 
    (e.Statut = 0 OR e.Statut IS NULL)
ORDER BY 
    ec.Nom, e.NomComplet;

-- ═══════════════════════════════════════════════════════════════════════════
-- 2. CORRECTION : DÉSACTIVER LES INSCRIPTIONS (DÉCOMMENTEZ APRÈS VÉRIFICATION)
-- ═══════════════════════════════════════════════════════════════════════════

/*
UPDATE Inscriptions i
INNER JOIN Eleves e ON i.IdEleve = e.IdEleve
SET i.Statut = 0
WHERE 
    (i.Statut = 1 OR i.Statut IS NULL)
    AND 
    (e.Statut = 0 OR e.Statut IS NULL);
*/

-- ═══════════════════════════════════════════════════════════════════════════
-- 3. VÉRIFICATION APRÈS CORRECTION
-- ═══════════════════════════════════════════════════════════════════════════

/*
SELECT 
    COUNT(*) AS NombreInscriptionsActivesAvecElevesInactifs
FROM 
    Inscriptions i
    INNER JOIN Eleves e ON i.IdEleve = e.IdEleve
WHERE 
    (i.Statut = 1 OR i.Statut IS NULL)
    AND 
    (e.Statut = 0 OR e.Statut IS NULL);

-- Résultat attendu : 0
*/
```

---

#### **3.2. Exécuter le script**

**Étapes** :
1. **Backup** de la base de données
2. **Exécuter** la requête de vérification (section 1)
3. **Valider** les résultats
4. **Décommenter** et exécuter la correction (section 2)
5. **Vérifier** qu'il n'y a plus d'incohérences (section 3)

---

### **Phase 4 : Prévention (Cascade Logicielle)** 🔄

**Durée estimée** : 1-2 heures

**Objectif** : Implémenter une cascade logicielle pour désactiver automatiquement les inscriptions quand un élève est désactivé

**Actions** :

#### **4.1. Ajouter une méthode dans InscriptionService**

**Fichier** : `Services/InscriptionService.cs`

**Nouvelle méthode** :
```csharp
/// <summary>
/// Désactive toutes les inscriptions d'un élève (cascade logicielle)
/// </summary>
public async Task<int> DesactiverInscriptionsParEleveAsync(int idEleve)
{
    var inscriptions = await _context.Inscriptions
        .Where(i => i.IdEleve == idEleve && (i.Statut == true || i.Statut == null))
        .ToListAsync();

    if (!inscriptions.Any())
        return 0;

    foreach (var inscription in inscriptions)
    {
        inscription.Statut = false;
    }

    await _context.SaveChangesAsync();
    return inscriptions.Count;
}
```

---

#### **4.2. Modifier EleveService.ToggleStatutAsync**

**Fichier** : `Services/EleveService.cs`

**Avant** :
```csharp
public async Task<bool> ToggleStatutAsync(int id)
{
    var eleve = await _context.Eleves.FindAsync(id);
    if (eleve == null)
        return false;

    eleve.Statut = eleve.Statut != true;
    await _context.SaveChangesAsync();
    return true;
}
```

**Après** :
```csharp
public async Task<bool> ToggleStatutAsync(int id)
{
    var eleve = await _context.Eleves.FindAsync(id);
    if (eleve == null)
        return false;

    var ancienStatut = eleve.Statut;
    eleve.Statut = eleve.Statut != true;
    await _context.SaveChangesAsync();

    // ✅ NOUVEAU : Cascade logicielle - Désactiver les inscriptions si l'élève est désactivé
    if (ancienStatut == true && eleve.Statut == false)
    {
        var inscriptionService = new InscriptionService(_context, _logger);
        await inscriptionService.DesactiverInscriptionsParEleveAsync(id);
    }

    return true;
}
```

**Note** : Il faudra peut-être injecter `IInscriptionRepository` ou `IInscriptionService` dans `EleveService` pour éviter la création directe.

---

#### **4.3. Alternative : Utiliser un événement ou un interceptor**

Si vous préférez une approche plus découplée, vous pouvez utiliser :
- Un événement dans le DbContext
- Un interceptor Entity Framework
- Un service de notification

---

**Tests à effectuer** :
- [ ] Tester la désactivation d'un élève → Vérifier que ses inscriptions sont désactivées
- [ ] Tester la réactivation d'un élève → Vérifier que ses inscriptions restent inactives (ou décider de les réactiver)
- [ ] Tester avec plusieurs inscriptions par élève
- [ ] Tester les performances

---

### **Phase 5 : Tests et Validation** 🔄

**Durée estimée** : 2-3 heures

**Actions** :

#### **5.1. Tests Unitaires**

Créer des tests pour :
- [ ] `InscriptionService.GetAllPagedAsync` avec filtre `Eleve.Statut`
- [ ] `InscriptionService.GetByEcolePagedAsync` avec filtre `Eleve.Statut`
- [ ] `EleveService.ToggleStatutAsync` avec cascade

#### **5.2. Tests d'Intégration**

- [ ] Tester les endpoints d'inscriptions
- [ ] Vérifier la cohérence des rapports Dashboard
- [ ] Vérifier qu'il n'y a plus d'incohérences

#### **5.3. Tests de Performance**

- [ ] Vérifier que l'ajout du filtre `Eleve.Statut` n'impacte pas les performances
- [ ] Optimiser les requêtes si nécessaire (index, etc.)

---

## 📊 Checklist Complète

### **Phase 2 : Filtrage**
- [ ] Modifier `GetAllPagedAsync`
- [ ] Modifier `GetByElevePagedAsync`
- [ ] Modifier `GetByEcolePagedAsync`
- [ ] Modifier `GetByClassePagedAsync`
- [ ] Modifier `GetByStatutPagedAsync`
- [ ] Modifier `GetByStatutAsync`
- [ ] Vérifier les autres méthodes
- [ ] Tester toutes les modifications

### **Phase 3 : Correction Données**
- [ ] Créer le script SQL de correction
- [ ] Faire un backup
- [ ] Exécuter le script de vérification
- [ ] Valider les résultats
- [ ] Exécuter le script de correction
- [ ] Vérifier qu'il n'y a plus d'incohérences

### **Phase 4 : Prévention**
- [ ] Ajouter `DesactiverInscriptionsParEleveAsync` dans `InscriptionService`
- [ ] Modifier `EleveService.ToggleStatutAsync`
- [ ] Tester la cascade logicielle

### **Phase 5 : Tests**
- [ ] Tests unitaires
- [ ] Tests d'intégration
- [ ] Tests de performance
- [ ] Validation finale

---

## 🎯 Résultat Attendu

Après l'implémentation complète :

1. ✅ **Toutes les requêtes d'inscriptions** filtrent sur `Eleve.Statut == true`
2. ✅ **Les données existantes** sont corrigées (inscriptions d'élèves inactifs désactivées)
3. ✅ **La cascade logicielle** prévient les futures incohérences
4. ✅ **Les rapports** sont cohérents (nombre d'élèves actifs ≥ nombre d'inscriptions actives)

---

## ⚠️ Points d'Attention

1. **Performance** : L'ajout du filtre `Eleve.Statut` nécessite un `Include(i => i.Eleve)`. Vérifier que cela n'impacte pas les performances.

2. **Index** : S'assurer qu'il y a un index sur `Eleves.Statut` pour optimiser les requêtes.

3. **Logique Métier** : Décider si la réactivation d'un élève doit réactiver ses inscriptions ou non.

4. **Backup** : Toujours faire un backup avant d'exécuter des scripts SQL de correction.

---

**Version** : 1.0  
**Date** : 2025-01-16  
**Statut** : 📋 Plan d'action prêt pour implémentation
