# 📱 UNICITÉ SERIAL NUMBER AGENT - Documentation Complète

## 📅 Date d'implémentation
**27 octobre 2025**

---

## 🎯 Objectif

Garantir qu'un appareil (device) physique ne peut être lié qu'à un seul agent dans le système. Cela permet de :
- **Sécuriser l'accès** : Un device ne peut être utilisé que par un agent autorisé
- **Tracer les actions** : Chaque action est liée à un device unique
- **Éviter les conflits** : Pas de partage non contrôlé d'appareils
- **Gérer les devices** : Historique clair de l'attribution des appareils

---

## ⚙️ Implémentation

### 1️⃣ **Validation au niveau du service (AgentService.cs)**

#### a) Validation lors de la création (`CreateAsync`)

```csharp
public async Task<Agent> CreateAsync(Agent agent)
{
    // ... autres validations ...

    // ✅ UNICITÉ SERIAL NUMBER AGENT: Vérifier que le SerialNumber n'existe pas déjà
    if (!string.IsNullOrEmpty(agent.SerialNumber))
    {
        var serialNumberExists = await ExistsBySerialNumberAsync(agent.SerialNumber);
        if (serialNumberExists)
        {
            throw new InvalidOperationException(
                $"Un agent avec le SerialNumber '{agent.SerialNumber}' existe déjà. " +
                $"Chaque SerialNumber doit être unique dans le système. " +
                $"Cet appareil est peut-être déjà lié à un autre agent."
            );
        }
    }
    
    // ... reste du code
}
```

**Comportement :**
- ✅ Si le SerialNumber est unique → L'agent est créé
- ✅ Si le SerialNumber est NULL ou vide → L'agent est créé (SerialNumber facultatif)
- ❌ Si le SerialNumber existe déjà → Exception `InvalidOperationException` avec message explicite

---

#### b) Validation lors de la mise à jour du SerialNumber (`UpdateSerialNumberByIdAsync`)

```csharp
public async Task<bool> UpdateSerialNumberByIdAsync(int idAgent, string serialNumber)
{
    var agent = await _context.Agents.FindAsync(idAgent);
    if (agent == null)
        return false;

    // ✅ UNICITÉ SERIAL NUMBER: Vérifier que le nouveau SerialNumber n'est pas déjà utilisé
    if (!string.IsNullOrEmpty(serialNumber) && serialNumber != agent.SerialNumber)
    {
        var serialNumberExistsByOtherAgent = await _context.Agents
            .AnyAsync(a => a.SerialNumber == serialNumber && a.IdAgent != idAgent);
        
        if (serialNumberExistsByOtherAgent)
        {
            throw new InvalidOperationException(
                $"Un autre agent avec le SerialNumber '{serialNumber}' existe déjà. " +
                $"Chaque SerialNumber doit être unique dans le système. " +
                $"Cet appareil est peut-être déjà lié à un autre agent."
            );
        }
    }

    agent.SerialNumber = serialNumber;
    await _context.SaveChangesAsync();
    return true;
}
```

**Comportement :**
- ✅ Si le SerialNumber reste inchangé → Pas de vérification
- ✅ Si le nouveau SerialNumber est unique → Mise à jour acceptée
- ❌ Si le nouveau SerialNumber existe déjà chez un autre agent → Exception

---

#### c) Validation lors de la mise à jour par Matricule (`UpdateSerialNumberByMatriculeAsync`)

Même logique que `UpdateSerialNumberByIdAsync`, mais recherche l'agent par matricule.

---

#### d) Méthode helper pour vérifier l'existence

```csharp
public async Task<bool> ExistsBySerialNumberAsync(string serialNumber)
{
    return await _context.Agents.AnyAsync(a => a.SerialNumber == serialNumber);
}
```

---

### 2️⃣ **Interface IAgentRepository**

Ajout de la signature de méthode :

```csharp
public interface IAgentRepository
{
    // ... autres méthodes
    Task<bool> ExistsBySerialNumberAsync(string serialNumber); // ✅ UNICITÉ SERIAL NUMBER
}
```

---

### 3️⃣ **Contrainte d'unicité en base de données (KelasiNaBisoDbContext.cs)**

```csharp
// ✅ UNICITÉ SERIAL NUMBER AGENT: Index unique sur le SerialNumber
modelBuilder.Entity<Agent>()
    .HasIndex(a => a.SerialNumber)
    .IsUnique()
    .HasDatabaseName("IX_Agents_SerialNumber_Unique");
```

**Avantages :**
- 🔒 **Protection au niveau base de données** : Garantie absolue d'unicité
- 🚀 **Performance** : Index optimise les recherches par SerialNumber
- 🛡️ **Sécurité renforcée** : Double couche de protection (application + BDD)

---

## 📊 Stratégie de protection à deux niveaux

| Niveau | Outil | Description | Avantage |
|--------|-------|-------------|----------|
| **1. Application** | `AgentService` | Validation avant insertion/mise à jour | Message d'erreur clair et métier |
| **2. Base de données** | Index unique | Contrainte SQL `UNIQUE` | Garantie absolue d'unicité |

---

## 🧪 Scénarios de test

### ✅ Scénario 1 : Création avec SerialNumber unique
**Action :** Créer un agent avec `serialNumber = "SN-AGENT-001"`  
**Résultat attendu :** ✅ Agent créé avec succès

---

### ❌ Scénario 2 : Création avec SerialNumber existant
**Action :** Créer un deuxième agent avec `serialNumber = "SN-AGENT-001"`  
**Résultat attendu :** ❌ Erreur 400 - `"Un agent avec le SerialNumber 'SN-AGENT-001' existe déjà"`

---

### ✅ Scénario 3 : Création sans SerialNumber (NULL)
**Action :** Créer un agent avec `serialNumber = null`  
**Résultat attendu :** ✅ Agent créé avec succès (SerialNumber facultatif)

---

### ✅ Scénario 4 : Mise à jour sans changer le SerialNumber
**Action :** Mettre à jour un agent en gardant le même SerialNumber  
**Résultat attendu :** ✅ Mise à jour réussie (pas de vérification d'unicité)

---

### ✅ Scénario 5 : Mise à jour avec un nouveau SerialNumber unique
**Action :** Changer le SerialNumber d'un agent vers `"SN-NEW-DEVICE"`  
**Résultat attendu :** ✅ Mise à jour réussie

---

### ❌ Scénario 6 : Mise à jour avec SerialNumber d'un autre agent
**Action :** Essayer de changer le SerialNumber vers un SerialNumber déjà utilisé  
**Résultat attendu :** ❌ Erreur 400 - `"Un autre agent avec le SerialNumber '...' existe déjà"`

---

## 🔄 Migration base de données

Pour appliquer la contrainte d'unicité sur une base existante :

### Option 1 : Migration Entity Framework (Recommandé)

```bash
# Créer une nouvelle migration
dotnet ef migrations add Add_Unique_SerialNumber_Agent_Index

# Appliquer la migration
dotnet ef database update
```

### Option 2 : Script SQL direct

```sql
-- Vérifier et supprimer les doublons existants (si nécessaire)
WITH CTE AS (
    SELECT 
        SerialNumber,
        ROW_NUMBER() OVER(PARTITION BY SerialNumber ORDER BY DateCreation) AS RowNum
    FROM Agents
    WHERE SerialNumber IS NOT NULL
)
SELECT * FROM CTE WHERE RowNum > 1; -- Afficher les doublons

-- Créer l'index unique
CREATE UNIQUE INDEX IX_Agents_SerialNumber_Unique 
ON Agents(SerialNumber)
WHERE SerialNumber IS NOT NULL;
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
  "detail": "Un agent avec le SerialNumber 'SN-AGENT-001' existe déjà. Chaque SerialNumber doit être unique dans le système. Cet appareil est peut-être déjà lié à un autre agent."
}
```

### Lors de la mise à jour
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Un autre agent avec le SerialNumber 'SN-AGENT-001' existe déjà. Chaque SerialNumber doit être unique dans le système. Cet appareil est peut-être déjà lié à un autre agent."
}
```

---

## ✅ Avantages de cette implémentation

| Avantage | Description |
|----------|-------------|
| 🛡️ **Double protection** | Validation applicative + contrainte BDD |
| 📊 **Performance optimale** | Index accélère les recherches |
| 🎯 **Messages clairs** | Erreurs explicites pour l'utilisateur/développeur |
| 🔄 **Mise à jour intelligente** | Pas de vérification si le SerialNumber ne change pas |
| 🔐 **Sécurité renforcée** | Un device = Un agent uniquement |
| ✨ **Flexibilité** | SerialNumber facultatif (NULL autorisé) |

---

## 🔗 Cas d'usage pratiques

### 1. Premier login mobile
```javascript
// 1. L'agent se connecte
const response = await authenticateUser(email, password);

// 2. Récupérer le SerialNumber du device
const deviceSerial = await getDeviceSerialNumber();

// 3. Enregistrer le SerialNumber
await updateAgentSerialNumber(agent.id, deviceSerial);

// 4. Prochaines connexions : vérifier le SerialNumber
if (agent.serialNumber !== deviceSerial) {
    throw new Error("Device non autorisé");
}
```

---

### 2. Changement de device
```javascript
// Scénario : L'agent a un nouveau téléphone

// 1. Connexion depuis le nouveau device
const newDeviceSerial = await getDeviceSerialNumber();

// 2. Détection du changement
if (agent.serialNumber !== newDeviceSerial) {
    // Demander confirmation
    const confirm = await askUserConfirmation(
        "Vous vous connectez depuis un nouvel appareil. Voulez-vous lier ce device à votre compte ?"
    );
    
    if (confirm) {
        // Mettre à jour le SerialNumber
        await updateAgentSerialNumber(agent.id, newDeviceSerial);
    } else {
        // Bloquer l'accès
        throw new Error("Accès refusé");
    }
}
```

---

### 3. Pointage de présence sécurisé
```javascript
// Vérifier que l'agent pointe depuis son device autorisé
async function markAttendance(agentId) {
    const currentDeviceSerial = await getDeviceSerialNumber();
    const agent = await getAgentById(agentId);
    
    if (agent.serialNumber !== currentDeviceSerial) {
        throw new Error(
            "Vous ne pouvez pointer votre présence que depuis votre appareil autorisé. " +
            "Device actuel: " + currentDeviceSerial + ", " +
            "Device autorisé: " + agent.serialNumber
        );
    }
    
    // Pointer la présence
    await createPresence(agentId);
}
```

---

## 🔐 Sécurité et bonnes pratiques

### 1. Obtention du SerialNumber

#### Android
```java
import android.os.Build;
import android.provider.Settings;

// Recommandé : Android ID (persiste après factory reset)
String androidId = Settings.Secure.getString(
    context.getContentResolver(), 
    Settings.Secure.ANDROID_ID
);
```

#### iOS
```swift
import UIKit

// Identifier for Vendor (change si app réinstallée)
let deviceId = UIDevice.current.identifierForVendor?.uuidString
```

#### React Native
```javascript
import DeviceInfo from 'react-native-device-info';

const serialNumber = await DeviceInfo.getUniqueId();
```

---

### 2. Gestion des exceptions

```csharp
try
{
    await _agentService.UpdateSerialNumberByIdAsync(agentId, serialNumber);
}
catch (InvalidOperationException ex)
{
    // SerialNumber déjà utilisé
    _logger.LogWarning($"Tentative d'utilisation d'un SerialNumber en doublon: {ex.Message}");
    
    // Envoyer une alerte à l'administrateur
    await _notificationService.SendAdminAlert(
        $"Agent {agentId} a tenté d'utiliser le SerialNumber {serialNumber} déjà lié à un autre agent"
    );
    
    return BadRequest(new { message = "Ce device est déjà lié à un autre agent" });
}
```

---

### 3. Traçabilité

Pour auditer les changements de SerialNumber :

```csharp
// Avant la mise à jour
var oldSerialNumber = agent.SerialNumber;

// Mise à jour
agent.SerialNumber = newSerialNumber;
await _context.SaveChangesAsync();

// Logger le changement
_logger.LogInformation(
    "SerialNumber modifié pour l'agent {AgentId}: {OldSerial} → {NewSerial}",
    agent.IdAgent,
    oldSerialNumber ?? "NULL",
    newSerialNumber
);
```

---

## 📌 Notes importantes

1. **SerialNumber facultatif** : Un agent peut exister sans SerialNumber
2. **NULL autorisé** : Plusieurs agents peuvent avoir `SerialNumber = NULL`
3. **Format libre** : Aucune contrainte de format (UUID, IMEI, custom, etc.)
4. **Sensible à la casse** : SQL Server est case-insensitive par défaut
5. **Espaces** : Pensez à trimmer les SerialNumber côté frontend

---

## 🎓 Exemple d'utilisation complète

```csharp
// CRÉATION D'UN AGENT AVEC SERIAL NUMBER
var agent = new Agent
{
    Nom = "MUKENDI",
    Prenom = "Pierre",
    SerialNumber = "DEVICE-ABC-123",
    // ... autres champs
};

try
{
    var createdAgent = await _agentService.CreateAsync(agent);
    Console.WriteLine($"✅ Agent créé avec SerialNumber : {createdAgent.SerialNumber}");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"❌ Erreur : {ex.Message}");
    // Afficher : "Ce device est déjà lié à un autre agent"
}

// CHANGEMENT DE DEVICE
try
{
    await _agentService.UpdateSerialNumberByIdAsync(agentId, "DEVICE-XYZ-789");
    Console.WriteLine("✅ SerialNumber mis à jour avec succès");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"❌ Erreur : {ex.Message}");
}
```

---

## ✅ Checklist d'implémentation

- [x] Champ `SerialNumber` dans le modèle `Agent`
- [x] Méthode `ExistsBySerialNumberAsync()` dans `IAgentRepository`
- [x] Implémentation dans `AgentService`
- [x] Validation dans `CreateAsync()`
- [x] Validation dans `UpdateSerialNumberByIdAsync()`
- [x] Validation dans `UpdateSerialNumberByMatriculeAsync()`
- [x] Index unique en base de données (`IX_Agents_SerialNumber_Unique`)
- [x] Fichier de tests HTTP (`test-serial-number-agent.http`)
- [x] Documentation complète (`UNICITE_SERIAL_NUMBER_AGENT.md`)
- [ ] Migration base de données appliquée
- [ ] Tests unitaires
- [ ] Tests d'intégration

---

## 🚀 Prochaines étapes suggérées

1. ✅ **Appliquer la migration** pour créer l'index unique
2. ✅ **Tester avec le fichier .http** pour valider tous les scénarios
3. ✅ **Nettoyer les doublons existants** si nécessaire avant la migration
4. 📱 **Intégrer dans l'application mobile** pour lier automatiquement les devices
5. 📊 **Mettre en place un tableau de bord** pour voir les devices liés
6. 🔔 **Configurer des alertes** en cas de tentative de doublon
7. 📚 **Former les utilisateurs** sur la gestion des devices

---

**🎉 L'unicité des SerialNumber agents est maintenant pleinement implémentée et documentée !**

