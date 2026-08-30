# 📧 UNICITÉ EMAIL TUTEUR - Documentation Complète

## 📅 Date d'implémentation
**27 octobre 2025**

---

## 🎯 Objectif

Garantir que chaque tuteur dans le système possède une adresse email unique, évitant ainsi les doublons et les conflits d'identification. Cette règle s'applique au champ `Email` du modèle `Tuteur`.

---

## ⚙️ Implémentation

### 1️⃣ **Validation au niveau du service (TuteurService.cs)**

#### a) Validation lors de la création (`CreateAsync`)

```csharp
public async Task<Tuteur> CreateAsync(Tuteur tuteur)
{
    // ✅ UNICITÉ EMAIL TUTEUR: Vérifier que l'email n'existe pas déjà
    if (!string.IsNullOrEmpty(tuteur.Email))
    {
        var emailExists = await ExistsByEmailAsync(tuteur.Email);
        if (emailExists)
        {
            throw new InvalidOperationException(
                $"Un tuteur avec l'email '{tuteur.Email}' existe déjà. " +
                $"Chaque email tuteur doit être unique dans le système."
            );
        }
    }
    // ... reste du code
}
```

**Comportement :**
- ✅ Si l'email est unique → Le tuteur est créé
- ✅ Si l'email est NULL ou vide → Le tuteur est créé (email facultatif)
- ❌ Si l'email existe déjà → Exception `InvalidOperationException` avec message clair

---

#### b) Validation lors de la mise à jour (`UpdateAsync`)

```csharp
public async Task<Tuteur> UpdateAsync(Tuteur tuteur)
{
    var existingTuteur = await _context.Tuteurs.FindAsync(tuteur.IdTuteur);
    if (existingTuteur == null)
        return null;

    // ✅ UNICITÉ EMAIL TUTEUR: Vérifier que le nouvel email n'est pas déjà utilisé par un autre tuteur
    if (!string.IsNullOrEmpty(tuteur.Email) && tuteur.Email != existingTuteur.Email)
    {
        var emailExistsByOtherTuteur = await _context.Tuteurs
            .AnyAsync(t => t.Email == tuteur.Email && t.IdTuteur != tuteur.IdTuteur);
        
        if (emailExistsByOtherTuteur)
        {
            throw new InvalidOperationException(
                $"Un autre tuteur avec l'email '{tuteur.Email}' existe déjà. " +
                $"Chaque email tuteur doit être unique dans le système."
            );
        }
    }
    // ... reste du code
}
```

**Comportement :**
- ✅ Si l'email reste inchangé → Pas de vérification (le tuteur garde son email)
- ✅ Si le nouvel email est unique → Mise à jour acceptée
- ❌ Si le nouvel email existe déjà chez un autre tuteur → Exception

---

#### c) Méthode helper pour vérifier l'existence

```csharp
public async Task<bool> ExistsByEmailAsync(string email)
{
    return await _context.Tuteurs.AnyAsync(t => t.Email == email);
}
```

---

### 2️⃣ **Interface ITuteurRepository**

Ajout de la signature de méthode :

```csharp
public interface ITuteurRepository
{
    // ... autres méthodes
    Task<bool> ExistsByEmailAsync(string email); // ✅ UNICITÉ EMAIL
}
```

---

### 3️⃣ **Contrainte d'unicité en base de données (KelasiNaBisoDbContext.cs)**

```csharp
// ✅ UNICITÉ EMAIL TUTEUR: Index unique sur l'email
modelBuilder.Entity<Tuteur>()
    .HasIndex(t => t.Email)
    .IsUnique()
    .HasDatabaseName("IX_Tuteurs_Email_Unique");
```

**Avantages :**
- 🔒 **Protection au niveau base de données** : Même si la validation applicative est contournée, la BDD rejette les doublons
- 🚀 **Performance** : Index optimise les recherches par email
- 🛡️ **Sécurité renforcée** : Double couche de protection (application + BDD)

---

## 📊 Stratégie de protection à deux niveaux

| Niveau | Outil | Description | Avantage |
|--------|-------|-------------|----------|
| **1. Application** | `TuteurService` | Validation avant insertion/mise à jour | Message d'erreur clair et métier |
| **2. Base de données** | Index unique | Contrainte SQL `UNIQUE` | Garantie absolue d'unicité |

---

## 🧪 Scénarios de test

### ✅ Scénario 1 : Création avec email unique
**Action :** Créer un tuteur avec `email = "jean.mbala@tuteur.com"`  
**Résultat attendu :** ✅ Tuteur créé avec succès

---

### ❌ Scénario 2 : Création avec email existant
**Action :** Créer un deuxième tuteur avec `email = "jean.mbala@tuteur.com"`  
**Résultat attendu :** ❌ Erreur 400 - `"Un tuteur avec l'email 'jean.mbala@tuteur.com' existe déjà"`

---

### ✅ Scénario 3 : Création sans email (NULL)
**Action :** Créer un tuteur avec `email = null`  
**Résultat attendu :** ✅ Tuteur créé avec succès (l'email est facultatif)

---

### ✅ Scénario 4 : Mise à jour sans changer l'email
**Action :** Modifier un tuteur (nom, téléphone) en gardant le même email  
**Résultat attendu :** ✅ Mise à jour réussie (pas de vérification d'unicité)

---

### ✅ Scénario 5 : Mise à jour avec un nouvel email unique
**Action :** Changer l'email d'un tuteur vers `"nouvel.email@tuteur.com"` (non utilisé)  
**Résultat attendu :** ✅ Mise à jour réussie

---

### ❌ Scénario 6 : Mise à jour avec email d'un autre tuteur
**Action :** Changer l'email d'un tuteur vers un email déjà utilisé par un autre tuteur  
**Résultat attendu :** ❌ Erreur 400 - `"Un autre tuteur avec l'email '...' existe déjà"`

---

## 🔄 Migration base de données

Pour appliquer la contrainte d'unicité sur une base existante :

### Option 1 : Migration Entity Framework (Recommandé)

```bash
# Créer une nouvelle migration
dotnet ef migrations add Add_Unique_Email_Tuteur_Index

# Appliquer la migration
dotnet ef database update
```

### Option 2 : Script SQL direct

```sql
-- Vérifier et supprimer les doublons existants (si nécessaire)
WITH CTE AS (
    SELECT 
        Email,
        ROW_NUMBER() OVER(PARTITION BY Email ORDER BY DateCreation) AS RowNum
    FROM Tuteurs
    WHERE Email IS NOT NULL
)
SELECT * FROM CTE WHERE RowNum > 1; -- Afficher les doublons

-- Créer l'index unique
CREATE UNIQUE INDEX IX_Tuteurs_Email_Unique 
ON Tuteurs(Email)
WHERE Email IS NOT NULL;
```

⚠️ **Important :** Si des doublons existent déjà, nettoyez-les avant d'appliquer l'index unique.

---

## 📝 Messages d'erreur

### Lors de la création
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Un tuteur avec l'email 'jean.mbala@tuteur.com' existe déjà. Chaque email tuteur doit être unique dans le système."
}
```

### Lors de la mise à jour
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Un autre tuteur avec l'email 'jean.mbala@tuteur.com' existe déjà. Chaque email tuteur doit être unique dans le système."
}
```

---

## ✅ Avantages de cette implémentation

| Avantage | Description |
|----------|-------------|
| 🛡️ **Double protection** | Validation applicative + contrainte BDD |
| 📊 **Performance optimale** | Index accélère les recherches |
| 🎯 **Messages clairs** | Erreurs explicites pour l'utilisateur/développeur |
| 🔄 **Mise à jour intelligente** | Pas de vérification si l'email ne change pas |
| 🌐 **Cohérence système** | Garantit l'intégrité des données |
| ✨ **Flexibilité** | Email facultatif (NULL autorisé) |

---

## 🔗 Cohérence avec les autres restrictions d'unicité

Cette implémentation suit le même pattern que :
- ✅ `Utilisateur.Email` (unicité globale)
- ✅ `Agent.EmailAgent` (unicité agents)
- ✅ `Eleve.Matricule` (unicité élèves)
- ✅ `Agent.Matricule` (unicité agents)

**Philosophie :** Chaque identifiant unique (email, matricule) doit être protégé à deux niveaux.

---

## 🧪 Fichier de test

Un fichier de test complet est disponible : **`test-unicite-email-tuteur.http`**

Il couvre tous les scénarios :
1. ✅ Création avec email unique
2. ❌ Création avec email existant
3. ✅ Création sans email (NULL)
4. ✅ Mise à jour avec nouvel email unique
5. ❌ Mise à jour avec email d'un autre tuteur
6. ✅ Mise à jour sans changer l'email

---

## 📌 Notes importantes

1. **Emails vides ou NULL** : La validation est ignorée si `Email` est vide ou NULL (tuteurs sans email autorisés)
2. **Casse (majuscules/minuscules)** : Par défaut, SQL Server est insensible à la casse pour les emails
3. **Espaces** : Pensez à trimmer les emails côté frontend pour éviter `"test@mail.com"` ≠ `" test@mail.com "`
4. **Email facultatif** : Contrairement à `Agent.EmailAgent`, l'email du tuteur peut être NULL

---

## 🎓 Exemple d'utilisation dans le code client

```csharp
try
{
    var tuteur = new Tuteur
    {
        NomComplet = "MBALA KABILA Jean",
        Email = "jean.mbala@tuteur.com",
        Telephone = "+243998877665",
        // ... autres champs
    };
    
    var createdTuteur = await tuteurService.CreateAsync(tuteur);
    Console.WriteLine($"✅ Tuteur créé avec l'email : {createdTuteur.Email}");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"❌ Erreur : {ex.Message}");
    // Afficher un message à l'utilisateur : "Cet email est déjà utilisé"
}
```

---

## 🔍 Différence avec Agent.EmailAgent

| Critère | `Tuteur.Email` | `Agent.EmailAgent` |
|---------|----------------|-------------------|
| **Obligatoire** | ❌ Non (NULL autorisé) | ❌ Non (NULL autorisé) |
| **Unicité** | ✅ Oui | ✅ Oui |
| **Index BDD** | ✅ `IX_Tuteurs_Email_Unique` | ✅ `IX_Agents_Email_Unique` |
| **Validation** | ✅ Double niveau | ✅ Double niveau |
| **Espace d'unicité** | Tuteurs uniquement | Agents uniquement |

**Note :** Un tuteur et un agent peuvent avoir le même email (espaces d'unicité séparés).

---

## ✅ Checklist d'implémentation

- [x] Ajout de `ExistsByEmailAsync()` dans `ITuteurRepository`
- [x] Implémentation de `ExistsByEmailAsync()` dans `TuteurService`
- [x] Validation dans `CreateAsync()`
- [x] Validation dans `UpdateAsync()`
- [x] Index unique en base de données (`IX_Tuteurs_Email_Unique`)
- [x] Fichier de tests HTTP (`test-unicite-email-tuteur.http`)
- [x] Documentation complète (`UNICITE_EMAIL_TUTEUR.md`)

---

## 🚀 Prochaines étapes suggérées

1. ✅ **Appliquer la migration** pour créer l'index unique
2. ✅ **Tester avec le fichier .http** pour valider tous les scénarios
3. ✅ **Nettoyer les doublons existants** si nécessaire avant la migration
4. 📱 **Mettre à jour le frontend** pour afficher des messages d'erreur clairs
5. 📚 **Former les utilisateurs** sur l'importance de l'unicité des emails

---

## 🔐 Considérations de sécurité et vie privée

1. **Protection RGPD** : L'unicité de l'email facilite l'identification des tuteurs pour les demandes RGPD
2. **Authentification** : Permet d'envisager une authentification future par email pour les tuteurs
3. **Communication** : Garantit qu'une notification envoyée par email atteint le bon tuteur
4. **Audit** : Facilite la traçabilité des actions liées à un tuteur spécifique

---

**🎉 L'unicité des emails tuteurs est maintenant pleinement implémentée et documentée !**


