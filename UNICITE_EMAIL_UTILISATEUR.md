# 📧 UNICITÉ DE L'EMAIL UTILISATEUR - Documentation

## 📋 Vue d'ensemble

Cette fonctionnalité garantit qu'**un email ne peut être utilisé que par un seul utilisateur** dans le système. C'est une règle métier essentielle pour :
- ✅ L'authentification unique et sécurisée
- ✅ La récupération de mot de passe
- ✅ L'intégrité des données
- ✅ Éviter les conflits d'identité

---

## 🎯 Objectif

Empêcher la création ou la modification d'utilisateurs avec un email déjà attribué à un autre utilisateur actif.

---

## 🔧 Implémentation technique

### 1. Validation au niveau du service (`UtilisateurService.cs`)

#### A. Méthode `CreateAsync()` - Création d'utilisateur

Avant de créer un nouvel utilisateur, le système vérifie si l'email existe déjà :

```csharp
public async Task<Utilisateur> CreateAsync(Utilisateur utilisateur)
{
    // ✅ UNICITÉ EMAIL: Vérifier que l'email n'existe pas déjà
    if (!string.IsNullOrEmpty(utilisateur.Email))
    {
        var emailExists = await ExistsByEmailAsync(utilisateur.Email);
        if (emailExists)
        {
            throw new InvalidOperationException(
                $"Un utilisateur avec l'email '{utilisateur.Email}' existe déjà. " +
                $"Chaque email doit être unique dans le système."
            );
        }
    }
    
    // ... reste du code de création
}
```

**Points clés** :
- ✅ Vérifie uniquement si un email est fourni (les emails null sont autorisés)
- ✅ Lance une `InvalidOperationException` si l'email existe
- ✅ Message d'erreur explicite avec l'email concerné

---

#### B. Méthode `UpdateAsync()` - Mise à jour d'utilisateur

Lors de la mise à jour, le système vérifie que le nouvel email n'est pas déjà utilisé **par un autre utilisateur** :

```csharp
public async Task<Utilisateur> UpdateAsync(Utilisateur utilisateur)
{
    var existingUtilisateur = await _context.Utilisateurs.FindAsync(utilisateur.IdUtilisateur);
    if (existingUtilisateur == null)
        return null;

    // ✅ UNICITÉ EMAIL: Vérifier que le nouvel email n'est pas déjà utilisé par un autre utilisateur
    if (!string.IsNullOrEmpty(utilisateur.Email) && utilisateur.Email != existingUtilisateur.Email)
    {
        var emailExistsByOtherUser = await _context.Utilisateurs
            .AnyAsync(u => u.Email == utilisateur.Email && u.IdUtilisateur != utilisateur.IdUtilisateur);
        
        if (emailExistsByOtherUser)
        {
            throw new InvalidOperationException(
                $"Un autre utilisateur avec l'email '{utilisateur.Email}' existe déjà. " +
                $"Chaque email doit être unique dans le système."
            );
        }
    }

    // ... reste du code de mise à jour
}
```

**Points clés** :
- ✅ Vérifie uniquement si l'email change
- ✅ Exclut l'utilisateur courant de la vérification
- ✅ Permet de conserver son propre email
- ✅ Empêche de prendre l'email d'un autre utilisateur

---

### 2. Contrainte au niveau de la base de données (`KelasiNaBisoDbContext.cs`)

Un **index unique** a été ajouté sur la colonne `Email` :

```csharp
// ✅ UNICITÉ EMAIL: Index unique sur l'email
modelBuilder.Entity<Utilisateur>()
    .HasIndex(u => u.Email)
    .IsUnique()
    .HasDatabaseName("IX_Utilisateurs_Email_Unique");
```

**Avantages** :
- ✅ **Double protection** : Application + Base de données
- ✅ **Performance** : Recherche rapide par email (index)
- ✅ **Intégrité** : Impossible d'avoir des doublons même en cas d'accès direct à la BDD

---

## 📊 Scénarios de test

### ✅ Scénario 1 : Création d'un premier utilisateur (RÉUSSI)

**Requête** :
```json
POST /api/Utilisateur
{
  "nomUtilisateur": "Kabongo",
  "email": "kabongo@kelasinabiso.cd",
  "motDePasseHash": "Password123",
  "idRole": 1,
  "idEcole": 1
}
```

**Résultat** :
```json
Status: 201 Created
{
  "idUtilisateur": 5,
  "nomUtilisateur": "Kabongo",
  "email": "kabongo@kelasinabiso.cd",
  ...
}
```

---

### ❌ Scénario 2 : Tentative de création avec email existant (BLOQUÉ)

**Requête** :
```json
POST /api/Utilisateur
{
  "nomUtilisateur": "Mbuyi",
  "email": "kabongo@kelasinabiso.cd",  ← MÊME EMAIL
  "motDePasseHash": "Password456",
  "idRole": 2,
  "idEcole": 1
}
```

**Résultat** :
```json
Status: 400 Bad Request
{
  "message": "Un utilisateur avec l'email 'kabongo@kelasinabiso.cd' existe déjà. Chaque email doit être unique dans le système."
}
```

---

### ✅ Scénario 3 : Création avec email différent (RÉUSSI)

**Requête** :
```json
POST /api/Utilisateur
{
  "nomUtilisateur": "Tshala",
  "email": "tshala@kelasinabiso.cd",  ← EMAIL DIFFÉRENT
  "motDePasseHash": "Password789",
  "idRole": 1,
  "idEcole": 1
}
```

**Résultat** :
```json
Status: 201 Created
{
  "idUtilisateur": 6,
  "email": "tshala@kelasinabiso.cd",
  ...
}
```

---

### ✅ Scénario 4 : Création sans email (RÉUSSI)

**Requête** :
```json
POST /api/Utilisateur
{
  "nomUtilisateur": "Muamba",
  "email": null,  ← PAS D'EMAIL
  "telephone": "+243812345678",
  "motDePasseHash": "Password101",
  "idRole": 2,
  "idEcole": 1
}
```

**Résultat** :
```json
Status: 201 Created
{
  "idUtilisateur": 7,
  "email": null,
  ...
}
```

**Raison** : Les emails `null` sont autorisés (utilisateurs sans email, authentification par téléphone).

---

### ✅ Scénario 5 : MAJ sans changer l'email (RÉUSSI)

**Requête** :
```json
PUT /api/Utilisateur/5
{
  "idUtilisateur": 5,
  "nomUtilisateur": "Kabongo-Modifié",
  "email": "kabongo@kelasinabiso.cd",  ← MÊME EMAIL (le sien)
  "idRole": 1,
  "idEcole": 1
}
```

**Résultat** :
```json
Status: 200 OK ou 204 No Content
```

**Raison** : On peut garder son propre email.

---

### ✅ Scénario 6 : MAJ avec nouvel email unique (RÉUSSI)

**Requête** :
```json
PUT /api/Utilisateur/6
{
  "idUtilisateur": 6,
  "nomUtilisateur": "Tshala",
  "email": "nouveau.email@kelasinabiso.cd",  ← NOUVEL EMAIL UNIQUE
  "idRole": 1,
  "idEcole": 1
}
```

**Résultat** :
```json
Status: 200 OK ou 204 No Content
```

**Raison** : Le nouvel email n'est utilisé par personne d'autre.

---

### ❌ Scénario 7 : MAJ avec email d'un autre utilisateur (BLOQUÉ)

**Requête** :
```json
PUT /api/Utilisateur/6
{
  "idUtilisateur": 6,
  "nomUtilisateur": "Tshala",
  "email": "kabongo@kelasinabiso.cd",  ← EMAIL D'UN AUTRE UTILISATEUR
  "idRole": 1,
  "idEcole": 1
}
```

**Résultat** :
```json
Status: 400 Bad Request
{
  "message": "Un autre utilisateur avec l'email 'kabongo@kelasinabiso.cd' existe déjà. Chaque email doit être unique dans le système."
}
```

---

## 🔍 Cas particuliers

### Cas 1 : Emails null autorisés

**Question** : Peut-on avoir plusieurs utilisateurs sans email (email = null) ?

**Réponse** : ✅ **OUI**. Les valeurs `null` ne sont pas soumises à la contrainte d'unicité.

**Raison** : Certains utilisateurs peuvent ne pas avoir d'email (authentification par téléphone, élèves mineurs, etc.).

**Exemple** :
```sql
SELECT * FROM Utilisateurs WHERE Email IS NULL;

IdUtilisateur | NomUtilisateur | Email | Telephone
7             | Muamba         | NULL  | +243812345678
8             | Kasongo        | NULL  | +243823456789
9             | Ilunga         | NULL  | +243834567890
```

Tous ont `email = NULL` → **Autorisé**.

---

### Cas 2 : Sensibilité à la casse (case-sensitive)

**Question** : `admin@test.cd` et `ADMIN@TEST.CD` sont-ils considérés comme différents ?

**Réponse** : Dépend de la configuration de la base de données.

**MySQL/MariaDB** : Par défaut **case-insensitive** (pas de distinction majuscule/minuscule).
- `admin@test.cd` = `ADMIN@TEST.CD` = `Admin@Test.CD`

**PostgreSQL** : Par défaut **case-sensitive** (distinction majuscule/minuscule).
- `admin@test.cd` ≠ `ADMIN@TEST.CD`

**Recommandation** : Normaliser les emails en minuscules avant insertion :

```csharp
if (!string.IsNullOrEmpty(utilisateur.Email))
{
    utilisateur.Email = utilisateur.Email.Trim().ToLowerInvariant();
}
```

---

### Cas 3 : Espaces dans l'email

**Question** : `" admin@test.cd "` (avec espaces) est-il accepté ?

**Réponse** : ❌ **NON** (si validation `[EmailAddress]` est appliquée).

La validation `[EmailAddress]` du modèle `Utilisateur` rejette les emails mal formés.

**Recommandation** : Toujours trim les emails :

```csharp
if (!string.IsNullOrEmpty(utilisateur.Email))
{
    utilisateur.Email = utilisateur.Email.Trim();
}
```

---

### Cas 4 : Utilisateurs désactivés (Statut = false)

**Question** : Peut-on réutiliser l'email d'un utilisateur désactivé ?

**Réponse** : ❌ **NON**. La contrainte d'unicité s'applique à **tous** les utilisateurs, actifs ou inactifs.

**Raison** : Un utilisateur désactivé peut être réactivé. Si son email a été réutilisé, cela crée un conflit.

**Solution** : Si vous voulez vraiment réutiliser un email :
1. Supprimer définitivement l'ancien utilisateur (`DELETE`)
2. Ou modifier son email avant de le désactiver

---

## 🎨 Messages d'erreur

### Message de création

```
Un utilisateur avec l'email 'test@example.com' existe déjà. Chaque email doit être unique dans le système.
```

### Message de mise à jour

```
Un autre utilisateur avec l'email 'test@example.com' existe déjà. Chaque email doit être unique dans le système.
```

**Format** :
- 📧 Email concerné entre guillemets
- 💬 Message explicite de la règle
- 🔍 Facile à comprendre pour l'utilisateur

---

## 🧪 Fichier de tests

Un fichier `test-unicite-email.http` a été créé avec **16 scénarios de test** :

| Test | Description | Résultat attendu |
|------|-------------|------------------|
| 1 | Authentification | 200 OK + Token |
| 2 | Création email unique | 201 Created ✅ |
| 3 | Création email existant | 400 Bad Request ❌ |
| 4 | Création email différent | 201 Created ✅ |
| 5 | Création sans email (null) | 201 Created ✅ |
| 6 | MAJ sans changer email | 200 OK ✅ |
| 7 | MAJ avec nouvel email unique | 200 OK ✅ |
| 8 | MAJ avec email existant | 400 Bad Request ❌ |
| 9 | Vérif email existe | 200 OK (true) |
| 10 | Vérif email inexistant | 200 OK (false) |
| 11 | Récupérer par email | 200 OK + Utilisateur |
| 12 | Test majuscules/minuscules | Dépend BDD |
| 13 | Test avec espaces | 400 Bad Request ❌ |
| 14-16 | Nettoyage (suppression) | 204 No Content |

**Utilisation** :
1. Ouvrez `test-unicite-email.http` dans VS Code
2. Exécutez les tests dans l'ordre (1 → 16)
3. Vérifiez que les résultats correspondent aux attentes

---

## 📈 Impact sur les fonctionnalités existantes

### ✅ Fonctionnalités non affectées

- ✅ Création d'utilisateur avec email **unique**
- ✅ Création d'utilisateur **sans email** (null)
- ✅ Mise à jour d'utilisateur **sans changer l'email**
- ✅ Mise à jour d'utilisateur avec **nouvel email unique**
- ✅ Récupération des utilisateurs (GET)
- ✅ Authentification
- ✅ Suppression d'utilisateurs

### ⚠️ Changement de comportement

**AVANT** :
- Plusieurs utilisateurs pouvaient avoir le même email
- Risque de conflits lors de l'authentification
- Pas de garantie d'unicité

**APRÈS** :
- Un email ne peut être utilisé que par **un seul utilisateur**
- Tentative de doublon → **Erreur 400 Bad Request**
- Garantie d'unicité au niveau **application** et **base de données**

---

## 🔐 Sécurité

- ✅ Validation côté **serveur** (pas seulement client)
- ✅ Exception levée **avant** l'insertion en base
- ✅ **Double protection** : Service + Index unique BDD
- ✅ Message d'erreur **informatif** sans exposer de données sensibles
- ✅ Authentification **JWT requise** sur l'endpoint

---

## 🚀 Migration et déploiement

### Migration de base de données requise

Pour appliquer l'index unique, une migration Entity Framework est nécessaire :

```bash
# Créer la migration
dotnet ef migrations add AddUniqueIndexOnUtilisateurEmail

# Appliquer la migration
dotnet ef database update
```

**SQL généré** :
```sql
CREATE UNIQUE INDEX `IX_Utilisateurs_Email_Unique` 
ON `Utilisateurs` (`Email`);
```

### Attention aux données existantes

**⚠️ IMPORTANT** : Si des doublons d'email existent déjà dans la base de données, la migration **échouera**.

**Solution** :
1. **Identifier les doublons** :
```sql
SELECT Email, COUNT(*) as Nombre
FROM Utilisateurs
WHERE Email IS NOT NULL
GROUP BY Email
HAVING COUNT(*) > 1;
```

2. **Résoudre les doublons manuellement** :
```sql
-- Exemple: Ajouter un suffixe aux doublons
UPDATE Utilisateurs 
SET Email = CONCAT(Email, '_OLD_', IdUtilisateur)
WHERE IdUtilisateur IN (
    SELECT IdUtilisateur FROM (
        SELECT IdUtilisateur, Email,
        ROW_NUMBER() OVER (PARTITION BY Email ORDER BY DateCreation) as rn
        FROM Utilisateurs
        WHERE Email IS NOT NULL
    ) t WHERE rn > 1
);
```

3. **Relancer la migration** :
```bash
dotnet ef database update
```

---

## 🎓 Règles métier finales

| Règle | Description | Exemple |
|-------|-------------|---------|
| **1 email = 1 utilisateur** | Un email ne peut être attribué qu'à un seul utilisateur | ✅ user1@test.cd → ❌ user2@test.cd (même email) |
| **Emails null autorisés** | Plusieurs utilisateurs peuvent avoir email = null | ✅ NULL, ✅ NULL, ✅ NULL |
| **MAJ : garder son email** | On peut conserver son propre email lors d'une MAJ | ✅ User 1 : test@mail.cd → test@mail.cd |
| **MAJ : nouvel email unique** | On peut changer pour un email non utilisé | ✅ User 1 : old@mail.cd → new@mail.cd |
| **MAJ : email d'autrui bloqué** | On ne peut pas prendre l'email d'un autre | ❌ User 1 : test@mail.cd → user2@mail.cd |

---

## 📝 Code summary

### Fichiers modifiés

| Fichier | Modifications |
|---------|---------------|
| `UtilisateurService.cs` | ✅ Validation dans `CreateAsync()` |
| `UtilisateurService.cs` | ✅ Validation dans `UpdateAsync()` |
| `KelasiNaBisoDbContext.cs` | ✅ Index unique sur `Email` |

### Fichiers créés

| Fichier | Description |
|---------|-------------|
| `test-unicite-email.http` | ✅ 16 scénarios de test |
| `UNICITE_EMAIL_UTILISATEUR.md` | ✅ Documentation complète |

---

## 🎉 Avantages de cette approche

| Avantage | Description |
|----------|-------------|
| **Sécurisé** | Double protection (app + BDD) |
| **Performant** | Index pour recherche rapide |
| **Flexible** | Emails null autorisés |
| **Clair** | Messages d'erreur explicites |
| **Testable** | 16 scénarios de test fournis |
| **Maintenable** | Code bien documenté |

---

## 🔄 Alternatives envisagées

| Alternative | Raison du rejet |
|-------------|-----------------|
| **Contrainte BDD uniquement** | Pas de message d'erreur personnalisé |
| **Validation client uniquement** | Pas sécurisé (contournable) |
| **Emails obligatoires** | Pas flexible (certains users sans email) |
| **Unicité Email + Téléphone** | Trop restrictif, complexe à gérer |

---

## ✅ Conclusion

L'**unicité de l'email** est maintenant garantie à deux niveaux :
1. ✅ **Application** : Validation dans `UtilisateurService`
2. ✅ **Base de données** : Index unique

Cette règle assure l'intégrité des données et évite les conflits d'authentification.

**Pour tester** : Exécutez `test-unicite-email.http` ! 🧪

---

## 📚 Références

- Entity Framework Core : [Indexes](https://learn.microsoft.com/en-us/ef/core/modeling/indexes)
- ASP.NET Core : [Model Validation](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation)
- MySQL : [UNIQUE Constraints](https://dev.mysql.com/doc/refman/8.0/en/create-index.html)

