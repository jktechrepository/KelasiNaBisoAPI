# 📋 Résumé des Modifications : Évaluation Automatique pour Devoirs

**Date :** 2024-01-15  
**Objectif :** Créer automatiquement une évaluation lors de la création d'un devoir à domicile

---

## 🎯 Modifications Effectuées

### 1. Modèle `Evaluation` ✅
**Fichier :** `Models/Evaluation.cs`

**Changement :**
- ✅ Ajout du champ `TitreEvaluation` (string?, nullable)
  - Permet de stocker le titre de l'évaluation (ex: titre du devoir)

```csharp
public string? TitreEvaluation { get; set; } // Titre de l'évaluation (ex: titre du devoir)
```

---

### 2. Modèle `DevoirADomicile` ✅
**Fichier :** `Models/DevoirADomicile.cs`

**Changement :**
- ✅ Ajout du champ `CoefficientDevoir` (int, valeur par défaut = 1)
  - Stocke le coefficient utilisé pour l'évaluation associée

```csharp
public int CoefficientDevoir { get; set; } = 1; // Coefficient pour l'évaluation associée
```

---

### 3. DTO `CreateDevoirADomicileDto` ✅
**Fichier :** `Models/DTOs/DevoirADomicile/CreateDevoirADomicileDto.cs`

**Changement :**
- ✅ Ajout du champ `CoefficientDevoir` (int, valeur par défaut = 1)
  - Validation : doit être supérieur à 0

```csharp
[Range(1, int.MaxValue, ErrorMessage = "Le coefficient doit être supérieur à 0")]
public int CoefficientDevoir { get; set; } = 1; // Coefficient pour l'évaluation associée
```

---

### 4. Contrôleur `DevoirADomicileController` ✅
**Fichier :** `Controllers/DevoirADomicileController.cs`

**Changements :**
1. ✅ Ajout de `CoefficientDevoir` lors de la création du devoir
2. ✅ Création automatique d'une `Evaluation` après la création du devoir

**Logique de création automatique :**
- L'évaluation est créée **uniquement** si `IdCours` est fourni (car `IdCours` est obligatoire dans `Evaluation`)
- Si `IdCours` n'est pas fourni, un avertissement est loggé mais la création du devoir continue

**Données de l'évaluation créée :**
```csharp
var evaluation = new Evaluation
{
    TypeEvaluation = "Devoir",
    TitreEvaluation = dto.Titre,              // Titre du devoir
    Coefficient = dto.CoefficientDevoir,       // Coefficient du devoir
    IdCours = dto.IdCours.Value,              // Même cours que le devoir
    IdClasse = dto.IdClasse,                   // Même classe que le devoir
    Statut = true,
    DateCreation = DateTime.Now
};
```

---

### 5. Script de Migration SQL ✅
**Fichier :** `Migrations/ADD_TITRE_EVALUATION_AND_COEFFICIENT_DEVOIR.sql`

**Contenu :**
- ✅ Ajoute la colonne `TitreEvaluation` dans la table `Evaluations`
- ✅ Ajoute la colonne `CoefficientDevoir` dans la table `DevoirsADomicile`
- ✅ Met à jour les devoirs existants avec un coefficient par défaut de 1
- ✅ Vérifications de sécurité (évite les doublons)

---

## 🔄 Flux de Création d'un Devoir

```
1. Frontend envoie CreateDevoirADomicileDto avec :
   - Titre
   - IdClasse
   - IdCours (optionnel mais recommandé)
   - CoefficientDevoir (par défaut = 1)
   - Autres champs...

2. Backend crée le DevoirADomicile avec CoefficientDevoir

3. Si IdCours est fourni :
   ✅ Création automatique d'une Evaluation avec :
      - TypeEvaluation = "Devoir"
      - TitreEvaluation = Titre du devoir
      - Coefficient = CoefficientDevoir
      - IdCours = IdCours du devoir
      - IdClasse = IdClasse du devoir
      - Statut = true

4. Si IdCours n'est PAS fourni :
   ⚠️ Avertissement loggé, devoir créé sans évaluation
```

---

## 📊 Exemple de Requête

### Requête POST `/api/DevoirADomicile`

```json
{
  "titre": "Devoir de Mathématiques - Chapitre 3",
  "description": "Exercices sur les équations",
  "contenu": "Résoudre les équations suivantes...",
  "idEcole": 1,
  "idDirection": 1,
  "idClasse": 5,
  "idCours": 10,
  "coefficientDevoir": 2,
  "dateLimite": "2024-01-20T23:59:59Z"
}
```

### Résultat

1. **Devoir créé** avec `CoefficientDevoir = 2`
2. **Evaluation créée automatiquement** :
   ```json
   {
     "idEvaluation": 123,
     "typeEvaluation": "Devoir",
     "titreEvaluation": "Devoir de Mathématiques - Chapitre 3",
     "coefficient": 2,
     "idCours": 10,
     "idClasse": 5,
     "statut": true
   }
   ```

---

## ⚠️ Points d'Attention

### 1. IdCours Optionnel
- Si `IdCours` n'est **pas fourni**, l'évaluation **ne sera pas créée**
- Un avertissement sera loggé : `⚠️ Aucune évaluation créée pour le devoir X : IdCours non fourni`
- Le devoir sera quand même créé avec succès

### 2. Gestion des Erreurs
- Si la création de l'évaluation échoue, l'erreur est loggée mais **ne bloque pas** la création du devoir
- Cela garantit que le devoir est toujours créé même en cas de problème avec l'évaluation

### 3. Coefficient par Défaut
- Si `CoefficientDevoir` n'est pas fourni, la valeur par défaut est **1**
- Les devoirs existants (avant la migration) auront un coefficient de **1**

---

## 🧪 Tests à Effectuer

### Test 1 : Création avec IdCours
```bash
POST /api/DevoirADomicile
Body: {
  "titre": "Test Devoir",
  "idClasse": 1,
  "idCours": 1,
  "coefficientDevoir": 2,
  ...
}
```
**Résultat attendu :**
- ✅ Devoir créé
- ✅ Evaluation créée avec TypeEvaluation = "Devoir"

### Test 2 : Création sans IdCours
```bash
POST /api/DevoirADomicile
Body: {
  "titre": "Test Devoir",
  "idClasse": 1,
  "coefficientDevoir": 1,
  ...
}
```
**Résultat attendu :**
- ✅ Devoir créé
- ⚠️ Aucune évaluation créée (avertissement dans les logs)

### Test 3 : Création avec coefficient = 0
```bash
POST /api/DevoirADomicile
Body: {
  "titre": "Test Devoir",
  "idClasse": 1,
  "idCours": 1,
  "coefficientDevoir": 0,
  ...
}
```
**Résultat attendu :**
- ❌ Erreur de validation (coefficient doit être > 0)

---

## 📝 Migration Base de Données

### Étape 1 : Exécuter le script SQL
```sql
-- Exécuter le fichier :
Migrations/ADD_TITRE_EVALUATION_AND_COEFFICIENT_DEVOIR.sql
```

### Étape 2 : Vérifier les colonnes
```sql
-- Vérifier TitreEvaluation
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Evaluations' 
AND COLUMN_NAME = 'TitreEvaluation';

-- Vérifier CoefficientDevoir
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'DevoirsADomicile' 
AND COLUMN_NAME = 'CoefficientDevoir';
```

---

## ✅ Checklist de Déploiement

- [ ] Exécuter le script de migration SQL
- [ ] Vérifier que les colonnes sont créées
- [ ] Tester la création d'un devoir avec IdCours
- [ ] Vérifier que l'évaluation est créée automatiquement
- [ ] Tester la création d'un devoir sans IdCours
- [ ] Vérifier les logs pour les avertissements
- [ ] Tester avec différents coefficients
- [ ] Vérifier que les devoirs existants fonctionnent toujours

---

## 🔗 Fichiers Modifiés

1. ✅ `Models/Evaluation.cs`
2. ✅ `Models/DevoirADomicile.cs`
3. ✅ `Models/DTOs/DevoirADomicile/CreateDevoirADomicileDto.cs`
4. ✅ `Controllers/DevoirADomicileController.cs`
5. ✅ `Migrations/ADD_TITRE_EVALUATION_AND_COEFFICIENT_DEVOIR.sql` (nouveau)

---

**Dernière mise à jour :** 2024-01-15

