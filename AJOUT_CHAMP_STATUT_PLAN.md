# 📋 PLAN D'AJOUT DU CHAMP STATUT (SOFT DELETE)

## 🎯 Objectif
Ajouter un champ `Statut` (boolean) avec valeur par défaut `true` dans toutes les tables qui n'en disposent pas déjà, et créer des endpoints pour désactiver/activer ce statut (soft delete pattern).

## 📊 État actuel

### ✅ Modèles AVEC champ Statut (bool) - Déjà implémenté (6 modèles)
1. ✅ **Utilisateur** : `public bool Statut { get; set; } = true;`
2. ✅ **Eleve** : `public bool Statut { get; set; }`
3. ✅ **Tuteur** : `public bool Statut { get; set; }`
4. ✅ **Enseignant** : `public bool Statut { get; set; }`
5. ✅ **AffectationCours** : `public bool Statut { get; set; }`
6. ✅ **Frais** : `public bool? Statut { get; set; }` (nullable)
7. ✅ **Notification** : `public bool EstActive { get; set; } = true;` (similaire)

### ⚠️ Modèles avec champ Statut STRING (pas boolean) - Ne pas modifier
1. **Presence** : `public string Statut` (Present, Absent, Justifie)
2. **Paiement** : `public string Statut` (En attente, Confirme, Echoue)
3. **Inscription** : `public string StatutInscription` (En attente, Annulé, Confirmé)

### ❌ Modèles SANS champ Statut (bool) - À ajouter (17 modèles)

#### Gestion scolaire (8)
1. ❌ **Ecole**
2. ❌ **Direction**
3. ❌ **Classe**
4. ❌ **Section**
5. ❌ **Option**
6. ❌ **AnneeScolaire**
7. ❌ **Role**
8. ❌ **Vacation**

#### Académique (4)
9. ❌ **Cours**
10. ❌ **Note**
11. ❌ **Evaluation**
12. ❌ **Horaire**

#### Communication et Documents (4)
13. ❌ **Message**
14. ❌ **GroupeMessage**
15. ❌ **Document**
16. ❌ **RessourcePedagogique**

#### Note
17. ❌ **Inscription** (a déjà StatutInscription string, ajouter Statut bool séparé)

## 🔧 Plan d'action

### ÉTAPE 1 : Modification des modèles ✅ (En cours)

Pour chaque modèle sans Statut, ajouter :
```csharp
public bool Statut { get; set; } = true;
```

**Ordre de modification** :
1. Gestion scolaire : Ecole, Direction, Classe, Section, Option, AnneeScolaire, Role, Vacation
2. Académique : Cours, Note, Evaluation, Horaire
3. Communication : Message, GroupeMessage, Document, RessourcePedagogique
4. Inscription (cas spécial)

### ÉTAPE 2 : Création de la migration Entity Framework

```bash
cd KelasiNaBisoAPI
dotnet ef migrations add AjoutChampStatut
```

### ÉTAPE 3 : Mise à jour DbContext

Dans `OnModelCreating()`, ajouter les valeurs par défaut :

```csharp
modelBuilder.Entity<Ecole>()
    .Property(e => e.Statut)
    .HasDefaultValue(true);

// Répéter pour chaque entité...
```

### ÉTAPE 4 : Création des endpoints de désactivation/activation

Pour chaque contrôleur, ajouter :

```csharp
// Désactiver (soft delete)
[HttpPut("{id}/desactiver")]
public async Task<IActionResult> Desactiver(int id)
{
    var entity = await _repository.GetByIdAsync(id);
    if (entity == null) return NotFound();
    
    entity.Statut = false;
    await _repository.UpdateAsync(entity);
    return NoContent();
}

// Activer
[HttpPut("{id}/activer")]
public async Task<IActionResult> Activer(int id)
{
    var entity = await _repository.GetByIdAsync(id);
    if (entity == null) return NotFound();
    
    entity.Statut = true;
    await _repository.UpdateAsync(entity);
    return NoContent();
}

// Basculer
[HttpPut("{id}/basculer-statut")]
public async Task<IActionResult> BasculerStatut(int id)
{
    var entity = await _repository.GetByIdAsync(id);
    if (entity == null) return NotFound();
    
    entity.Statut = !entity.Statut;
    await _repository.UpdateAsync(entity);
    return NoContent();
}
```

### ÉTAPE 5 : Mise à jour des méthodes GetAll()

Filtrer par défaut les entités actives :

```csharp
public async Task<IEnumerable<Entity>> GetAllAsync()
{
    return await _context.Entities
        .Where(e => e.Statut == true)
        .ToListAsync();
}

// Optionnel : Méthode pour récupérer TOUT (incluant inactifs)
public async Task<IEnumerable<Entity>> GetAllIncludingInactiveAsync()
{
    return await _context.Entities.ToListAsync();
}
```

### ÉTAPE 6 : Tests

1. Appliquer la migration : `dotnet ef database update`
2. Tester endpoints de désactivation/activation
3. Vérifier filtres GetAll()

## 📝 Détails techniques

### Modification des modèles

**Emplacement** : `KelasiNaBisoAPI/Models/`

**Ajout à faire** :
```csharp
public bool Statut { get; set; } = true;
```

**Position recommandée** : Avant les attributs techniques (DateCreation, etc.)

### Cas spéciaux

#### 1. Frais
- A déjà `public bool? Statut { get; set; }` (nullable)
- **Action** : Modifier en `public bool Statut { get; set; } = true;` (non-nullable)

#### 2. Inscription
- A déjà `public string StatutInscription { get; set; }`
- **Action** : Ajouter `public bool Statut { get; set; } = true;` séparément
- Les deux champs coexisteront :
  - `Statut` (bool) : Actif/Inactif
  - `StatutInscription` (string) : En attente/Confirmé/Annulé

#### 3. Notification
- A déjà `public bool EstActive { get; set; } = true;`
- **Action** : Garder tel quel (déjà implémenté)

#### 4. Presence, Paiement
- Ont `public string Statut` pour l'état métier
- **Action** : NE PAS modifier, créer un nouveau champ `EstActif` si nécessaire
- **Recommandation** : Garder tel quel pour éviter confusion

## 🎯 Endpoints à créer

### Format standard

Pour chaque entité (exemple avec Ecole) :

```http
PUT /api/Ecole/{id}/desactiver
PUT /api/Ecole/{id}/activer
PUT /api/Ecole/{id}/basculer-statut
GET /api/Ecole?includeInactive=true
```

### Liste des contrôleurs à modifier (17)

1. EcoleController
2. DirectionController
3. ClasseController
4. SectionController
5. OptionController
6. AnneeScolaireController
7. RoleController
8. VacationController
9. CoursController
10. NoteController
11. EvaluationController
12. HoraireController (si existe)
13. MessageController
14. GroupeMessageController
15. DocumentController
16. RessourcePedagogiqueController
17. InscriptionController

## 🔍 Vérifications à faire

### Après modification des modèles
- [ ] Compilation réussie
- [ ] Pas d'erreur de typage
- [ ] Valeur par défaut bien définie

### Après migration
- [ ] Migration créée sans erreur
- [ ] Colonnes ajoutées dans la base
- [ ] Valeur par défaut SQL = 1 (true)
- [ ] Données existantes ont Statut = true

### Après endpoints
- [ ] Endpoints compilent
- [ ] Désactivation fonctionne
- [ ] Activation fonctionne
- [ ] Filtres GetAll() fonctionnent
- [ ] Swagger mis à jour

## 📅 Estimation de temps

- ✅ Analyse : 30 min (fait)
- 🔄 Modification modèles : 20 min (en cours)
- ⏳ Migration : 10 min
- ⏳ Endpoints : 1h30
- ⏳ Tests : 30 min
- **Total** : ~3h

## 🚀 Bénéfices

1. **Soft Delete** : Pas de suppression physique
2. **Historique** : Conservation des données
3. **Réversibilité** : Réactivation possible
4. **Conformité** : Respect des normes (RGPD, etc.)
5. **Performance** : Filtrage automatique des inactifs

## ⚠️ Points d'attention

1. **Cohérence** : Toujours utiliser le même nom "Statut"
2. **Non-nullable** : Ne pas autoriser null (sauf cas spécial)
3. **Valeur par défaut** : Toujours true
4. **Filtres** : Penser à filtrer dans GetAll()
5. **Documentation** : Mettre à jour Swagger

## 🔄 Évolution future

- Ajouter `DateDesactivation` pour traçabilité
- Ajouter `DesactivePar` (IdUtilisateur)
- Ajouter `RaisonDesactivation` (string)

---

**Date** : 16 octobre 2025  
**Status** : 🟡 En cours - Étape 1/6

