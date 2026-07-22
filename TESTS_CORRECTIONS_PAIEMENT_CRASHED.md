# 🧪 Tests des Corrections PaiementCrashed

**Date** : 12 décembre 2024  
**Objectif** : Valider les corrections apportées à `SaveCrashedPaiementsAsync`

---

## 📋 Corrections Testées

1. ✅ **ErreursJson sérialisation sécurisée** : Gestion des cas null et erreurs de sérialisation
2. ✅ **DatePaiement nullable** : Retrait de `[Required]` 
3. ✅ **DateTime.UtcNow** : Utilisation cohérente avec le reste du système
4. ✅ **NumeroLigne validation** : Protection contre valeurs négatives
5. ✅ **Gestion d'erreur améliorée** : Distinction entre DbUpdateException et autres exceptions

---

## 🧪 Scénarios de Test

### Test 1 : Erreurs null

**Objectif** : Vérifier que `ErreursJson` est bien `"[]"` quand `Erreurs` est null

```csharp
// Arrange
var paiementDto = new PaiementExcelDto
{
    NumeroLigne = 1,
    Erreurs = null // ❌ Null
};

// Act
await SaveCrashedPaiementsAsync(...);

// Assert
var saved = await _context.PaiementsCrashed.FirstAsync();
Assert.Equal("[]", saved.ErreursJson);
```

---

### Test 2 : Erreurs vide

**Objectif** : Vérifier que `ErreursJson` est bien `"[]"` quand `Erreurs` est vide

```csharp
// Arrange
var paiementDto = new PaiementExcelDto
{
    NumeroLigne = 1,
    Erreurs = new List<string>() // ✅ Liste vide
};

// Act
await SaveCrashedPaiementsAsync(...);

// Assert
var saved = await _context.PaiementsCrashed.FirstAsync();
Assert.Equal("[]", saved.ErreursJson);
```

---

### Test 3 : Erreurs avec contenu

**Objectif** : Vérifier que `ErreursJson` est bien sérialisé quand `Erreurs` contient des erreurs

```csharp
// Arrange
var paiementDto = new PaiementExcelDto
{
    NumeroLigne = 1,
    Erreurs = new List<string> { "Erreur 1", "Erreur 2" }
};

// Act
await SaveCrashedPaiementsAsync(...);

// Assert
var saved = await _context.PaiementsCrashed.FirstAsync();
var erreurs = JsonSerializer.Deserialize<List<string>>(saved.ErreursJson);
Assert.Equal(2, erreurs.Count);
Assert.Contains("Erreur 1", erreurs);
Assert.Contains("Erreur 2", erreurs);
```

---

### Test 4 : DatePaiement null

**Objectif** : Vérifier que l'insertion réussit même si `DatePaiement` est null

```csharp
// Arrange
var paiementDto = new PaiementExcelDto
{
    NumeroLigne = 1,
    DatePaiement = null, // ✅ Null autorisé
    Erreurs = new List<string> { "Test" }
};

// Act
await SaveCrashedPaiementsAsync(...);

// Assert
var saved = await _context.PaiementsCrashed.FirstAsync();
Assert.Null(saved.DatePaiement);
```

---

### Test 5 : NumeroLigne négatif

**Objectif** : Vérifier que `NumeroLigne` négatif est corrigé à 0

```csharp
// Arrange
var paiementDto = new PaiementExcelDto
{
    NumeroLigne = -5, // ❌ Négatif
    Erreurs = new List<string> { "Test" }
};

// Act
await SaveCrashedPaiementsAsync(...);

// Assert
var saved = await _context.PaiementsCrashed.FirstAsync();
Assert.Equal(0, saved.NumeroLigne);
```

---

### Test 6 : NumeroLigne = 0

**Objectif** : Vérifier que `NumeroLigne` = 0 est accepté

```csharp
// Arrange
var paiementDto = new PaiementExcelDto
{
    NumeroLigne = 0, // ✅ 0 accepté
    Erreurs = new List<string> { "Test" }
};

// Act
await SaveCrashedPaiementsAsync(...);

// Assert
var saved = await _context.PaiementsCrashed.FirstAsync();
Assert.Equal(0, saved.NumeroLigne);
```

---

### Test 7 : DateTime.UtcNow

**Objectif** : Vérifier que `DateEchec` et `DateCreation` utilisent `UtcNow`

```csharp
// Arrange
var before = DateTime.UtcNow;
var paiementDto = new PaiementExcelDto
{
    NumeroLigne = 1,
    Erreurs = new List<string> { "Test" }
};

// Act
await SaveCrashedPaiementsAsync(...);
var after = DateTime.UtcNow;

// Assert
var saved = await _context.PaiementsCrashed.FirstAsync();
Assert.NotNull(saved.DateEchec);
Assert.NotNull(saved.DateCreation);
Assert.True(saved.DateEchec >= before && saved.DateEchec <= after);
Assert.True(saved.DateCreation >= before && saved.DateCreation <= after);
```

---

### Test 8 : DbUpdateException gérée

**Objectif** : Vérifier que `DbUpdateException` est bien loggée avec détails

```csharp
// Arrange
var paiementDto = new PaiementExcelDto
{
    NumeroLigne = 1,
    IdEcole = 99999, // ❌ École inexistante (violation FK)
    Erreurs = new List<string> { "Test" }
};

// Act & Assert
// Ne doit pas lever d'exception, mais logger l'erreur
await SaveCrashedPaiementsAsync(...);

// Vérifier que l'erreur a été loggée
// (nécessite un mock du logger)
```

---

### Test 9 : Liste vide de paiements échoués

**Objectif** : Vérifier que rien n'est inséré si la liste est vide

```csharp
// Arrange
var paiementsEchoues = new List<PaiementExcelDto>(); // ✅ Liste vide

// Act
await SaveCrashedPaiementsAsync(paiementsEchoues, ...);

// Assert
var count = await _context.PaiementsCrashed.CountAsync();
Assert.Equal(0, count);
```

---

### Test 10 : Données brutes manquantes

**Objectif** : Vérifier que l'insertion réussit même si les données brutes sont manquantes

```csharp
// Arrange
var paiementDto = new PaiementExcelDto
{
    NumeroLigne = 999, // ❌ N'existe pas dans paiementsRawDict
    Erreurs = new List<string> { "Test" }
};
var paiementsRawDict = new Dictionary<int, PaiementExcelRaw>(); // ✅ Vide

// Act
await SaveCrashedPaiementsAsync(...);

// Assert
var saved = await _context.PaiementsCrashed.FirstAsync();
Assert.Null(saved.NomCompletEleve);
Assert.Null(saved.LibelleFrais);
```

---

## 🔧 Script de Test Manuel

### Prérequis

1. Base de données de test configurée
2. École de test avec ID = 1
3. Utilisateur de test avec ID = 1

### Test via API

```bash
# 1. Créer un fichier Excel avec des données invalides
python3 create_test_excel_paiements_from_template.py

# 2. Uploader le fichier
curl -X POST "https://localhost:7102/api/Paiement/bulk-excel" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -F "file=@test_paiements.xlsx"

# 3. Vérifier les paiements échoués
curl -X GET "https://localhost:7102/api/PaiementCrashed/ecole" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

## ✅ Checklist de Validation

- [ ] Test 1 : Erreurs null → ErreursJson = "[]"
- [ ] Test 2 : Erreurs vide → ErreursJson = "[]"
- [ ] Test 3 : Erreurs avec contenu → Sérialisation correcte
- [ ] Test 4 : DatePaiement null → Insertion réussie
- [ ] Test 5 : NumeroLigne négatif → Corrigé à 0
- [ ] Test 6 : NumeroLigne = 0 → Accepté
- [ ] Test 7 : DateTime.UtcNow → Utilisé correctement
- [ ] Test 8 : DbUpdateException → Loggée avec détails
- [ ] Test 9 : Liste vide → Rien inséré
- [ ] Test 10 : Données brutes manquantes → Insertion réussie

---

## 📊 Résultats Attendus

Tous les tests doivent passer avec les corrections appliquées. Les insertions dans `PaiementsCrashed` doivent réussir même avec :
- Erreurs null ou vides
- DatePaiement null
- NumeroLigne invalide
- Données brutes manquantes
- Violations de contraintes FK (avec logging approprié)

---

**Document créé le** : 12 décembre 2024  
**Statut** : ✅ Prêt pour exécution










