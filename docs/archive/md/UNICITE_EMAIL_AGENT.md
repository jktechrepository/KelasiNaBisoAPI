# 📧 UNICITÉ EMAIL AGENT - Documentation Complète

## 📅 Date d'implémentation
**27 octobre 2025**

---

## 🎯 Objectif

Garantir que chaque agent dans le système possède une adresse email unique (`EmailAgent`), évitant ainsi les doublons et les conflits d'identification.

---

## ⚙️ Implémentation

### 1️⃣ **Validation au niveau du service (AgentService.cs)**

#### a) Validation lors de la création (`CreateAsync`)

```csharp
public async Task<Agent> CreateAsync(Agent agent)
{
    // ✅ UNICITÉ EMAIL AGENT: Vérifier que l'email n'existe pas déjà
    if (!string.IsNullOrEmpty(agent.EmailAgent))
    {
        var emailExists = await ExistsByEmailAsync(agent.EmailAgent);
        if (emailExists)
        {
            throw new InvalidOperationException(
                $"Un agent avec l'email '{agent.EmailAgent}' existe déjà. " +
                $"Chaque email agent doit être unique dans le système."
            );
        }
    }
    // ... reste du code
}
```

**Comportement :**
- ✅ Si l'email est unique → L'agent est créé
- ❌ Si l'email existe déjà → Exception `InvalidOperationException` avec message clair

---

#### b) Validation lors de la mise à jour (`UpdateAsync`)

```csharp
public async Task<Agent> UpdateAsync(Agent agent)
{
    var existingAgent = await _context.Agents.FindAsync(agent.IdAgent);
    if (existingAgent == null)
        return null;

    // ✅ UNICITÉ EMAIL AGENT: Vérifier que le nouvel email n'est pas déjà utilisé par un autre agent
    if (!string.IsNullOrEmpty(agent.EmailAgent) && agent.EmailAgent != existingAgent.EmailAgent)
    {
        var emailExistsByOtherAgent = await _context.Agents
            .AnyAsync(a => a.EmailAgent == agent.EmailAgent && a.IdAgent != agent.IdAgent);
        
        if (emailExistsByOtherAgent)
        {
            throw new InvalidOperationException(
                $"Un autre agent avec l'email '{agent.EmailAgent}' existe déjà. " +
                $"Chaque email agent doit être unique dans le système."
            );
        }
    }
    // ... reste du code
}
```

**Comportement :**
- ✅ Si l'email reste inchangé → Pas de vérification (l'agent garde son email)
- ✅ Si le nouvel email est unique → Mise à jour acceptée
- ❌ Si le nouvel email existe déjà chez un autre agent → Exception

---

#### c) Méthode helper pour vérifier l'existence

```csharp
public async Task<bool> ExistsByEmailAsync(string email)
{
    return await _context.Agents.AnyAsync(a => a.EmailAgent == email);
}
```

---

### 2️⃣ **Interface IAgentRepository**

Ajout de la signature de méthode :

```csharp
public interface IAgentRepository
{
    // ... autres méthodes
    Task<bool> ExistsByEmailAsync(string email); // ✅ UNICITÉ EMAIL
}
```

---

### 3️⃣ **Contrainte d'unicité en base de données (KelasiNaBisoDbContext.cs)**

```csharp
// ✅ UNICITÉ EMAIL AGENT: Index unique sur l'email
modelBuilder.Entity<Agent>()
    .HasIndex(a => a.EmailAgent)
    .IsUnique()
    .HasDatabaseName("IX_Agents_Email_Unique");
```

**Avantages :**
- 🔒 **Protection au niveau base de données** : Même si la validation applicative est contournée, la BDD rejette les doublons
- 🚀 **Performance** : Index optimise les recherches par email
- 🛡️ **Sécurité renforcée** : Double couche de protection (application + BDD)

---

## 📊 Stratégie de protection à deux niveaux

| Niveau | Outil | Description | Avantage |
|--------|-------|-------------|----------|
| **1. Application** | `AgentService` | Validation avant insertion/mise à jour | Message d'erreur clair et métier |
| **2. Base de données** | Index unique | Contrainte SQL `UNIQUE` | Garantie absolue d'unicité |

---

## 🧪 Scénarios de test

### ✅ Scénario 1 : Création avec email unique
**Action :** Créer un agent avec `emailAgent = "jean.mbala@test.com"`  
**Résultat attendu :** ✅ Agent créé avec succès

---

### ❌ Scénario 2 : Création avec email existant
**Action :** Créer un deuxième agent avec `emailAgent = "jean.mbala@test.com"`  
**Résultat attendu :** ❌ Erreur 400 - `"Un agent avec l'email 'jean.mbala@test.com' existe déjà"`

---

### ✅ Scénario 3 : Mise à jour sans changer l'email
**Action :** Modifier un agent (nom, fonction, salaire) en gardant le même email  
**Résultat attendu :** ✅ Mise à jour réussie (pas de vérification d'unicité)

---

### ✅ Scénario 4 : Mise à jour avec un nouvel email unique
**Action :** Changer l'email d'un agent vers `"nouvel.email@test.com"` (non utilisé)  
**Résultat attendu :** ✅ Mise à jour réussie

---

### ❌ Scénario 5 : Mise à jour avec email d'un autre agent
**Action :** Changer l'email d'un agent vers un email déjà utilisé par un autre agent  
**Résultat attendu :** ❌ Erreur 400 - `"Un autre agent avec l'email '...' existe déjà"`

---

## 🔄 Migration base de données

Pour appliquer la contrainte d'unicité sur une base existante :

### Option 1 : Migration Entity Framework (Recommandé)

```bash
# Créer une nouvelle migration
dotnet ef migrations add Add_Unique_Email_Agent_Index

# Appliquer la migration
dotnet ef database update
```

### Option 2 : Script SQL direct

```sql
-- Vérifier et supprimer les doublons existants (si nécessaire)
WITH CTE AS (
    SELECT 
        EmailAgent,
        ROW_NUMBER() OVER(PARTITION BY EmailAgent ORDER BY DateCreation) AS RowNum
    FROM Agents
    WHERE EmailAgent IS NOT NULL
)
SELECT * FROM CTE WHERE RowNum > 1; -- Afficher les doublons

-- Créer l'index unique
CREATE UNIQUE INDEX IX_Agents_Email_Unique 
ON Agents(EmailAgent)
WHERE EmailAgent IS NOT NULL;
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
  "detail": "Un agent avec l'email 'jean.mbala@test.com' existe déjà. Chaque email agent doit être unique dans le système."
}
```

### Lors de la mise à jour
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Un autre agent avec l'email 'jean.mbala@test.com' existe déjà. Chaque email agent doit être unique dans le système."
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

---

## 🔗 Cohérence avec les autres restrictions d'unicité

Cette implémentation suit le même pattern que :
- ✅ `Utilisateur.Email` (unicité globale)
- ✅ `Eleve.Matricule` (unicité élèves)
- ✅ `Agent.Matricule` (unicité agents)

**Philosophie :** Chaque identifiant unique (email, matricule) doit être protégé à deux niveaux.

---

## 🧪 Fichier de test

Un fichier de test complet est disponible : **`test-unicite-email-agent.http`**

Il couvre tous les scénarios :
1. ✅ Création avec email unique
2. ❌ Création avec email existant
3. ✅ Mise à jour avec nouvel email unique
4. ❌ Mise à jour avec email d'un autre agent
5. ✅ Mise à jour sans changer l'email

---

## 📌 Notes importantes

1. **Emails vides ou NULL** : La validation est ignorée si `EmailAgent` est vide ou NULL (agents sans email autorisés)
2. **Casse (majuscules/minuscules)** : Par défaut, SQL Server est insensible à la casse pour les emails
3. **Espaces** : Pensez à trimmer les emails côté frontend pour éviter `"test@mail.com"` ≠ `" test@mail.com "`

---

## 🎓 Exemple d'utilisation dans le code client

```csharp
try
{
    var agent = new Agent
    {
        Nom = "MBALA",
        EmailAgent = "jean.mbala@test.com",
        // ... autres champs
    };
    
    var createdAgent = await agentService.CreateAsync(agent);
    Console.WriteLine($"✅ Agent créé avec l'email : {createdAgent.EmailAgent}");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"❌ Erreur : {ex.Message}");
    // Afficher un message à l'utilisateur : "Cet email est déjà utilisé"
}
```

---

## ✅ Checklist d'implémentation

- [x] Ajout de `ExistsByEmailAsync()` dans `IAgentRepository`
- [x] Implémentation de `ExistsByEmailAsync()` dans `AgentService`
- [x] Validation dans `CreateAsync()`
- [x] Validation dans `UpdateAsync()`
- [x] Index unique en base de données (`IX_Agents_Email_Unique`)
- [x] Fichier de tests HTTP (`test-unicite-email-agent.http`)
- [x] Documentation complète (`UNICITE_EMAIL_AGENT.md`)

---

## 🚀 Prochaines étapes suggérées

1. ✅ **Appliquer la migration** pour créer l'index unique
2. ✅ **Tester avec le fichier .http** pour valider tous les scénarios
3. ✅ **Nettoyer les doublons existants** si nécessaire avant la migration
4. 📱 **Mettre à jour le frontend** pour afficher des messages d'erreur clairs
5. 📚 **Former les utilisateurs** sur l'importance de l'unicité des emails

---

**🎉 L'unicité des emails agents est maintenant pleinement implémentée et documentée !**

