# 📱 UNICITÉ SERIAL NUMBER ÉLÈVE - Documentation Complète

## 📅 Date d'implémentation
**27 octobre 2025**

---

## 🎯 Objectif

Garantir qu'un appareil (device) physique ne peut être lié qu'à un seul élève dans le système. Cette fonctionnalité est particulièrement importante pour :
- **Sécuriser l'accès parents** : Le device du parent est lié à son enfant
- **Tracer les consultations** : Chaque accès est lié à un device unique
- **Éviter les conflits** : Pas de partage non contrôlé d'appareils
- **Gérer les devices parents** : Un parent peut consulter uniquement les infos de son enfant depuis son appareil autorisé

---

## ⚙️ Implémentation

### 1️⃣ **Validation au niveau du service (EleveService.cs)**

#### a) Validation lors de la création (`CreateAsync`)

```csharp
public async Task<Eleve> CreateAsync(Eleve eleve)
{
    // ✅ UNICITÉ SERIAL NUMBER ÉLÈVE: Vérifier que le SerialNumber n'existe pas déjà
    if (!string.IsNullOrEmpty(eleve.SerialNumber))
    {
        var serialNumberExists = await ExistsBySerialNumberAsync(eleve.SerialNumber);
        if (serialNumberExists)
        {
            throw new InvalidOperationException(
                $"Un élève avec le SerialNumber '{eleve.SerialNumber}' existe déjà. " +
                $"Chaque SerialNumber doit être unique dans le système. " +
                $"Cet appareil est peut-être déjà lié à un autre élève."
            );
        }
    }
    
    // ... reste du code
}
```

**Comportement :**
- ✅ Si le SerialNumber est unique → L'élève est créé
- ✅ Si le SerialNumber est NULL ou vide → L'élève est créé (SerialNumber facultatif)
- ❌ Si le SerialNumber existe déjà → Exception `InvalidOperationException`

---

#### b) Validation lors de la mise à jour du SerialNumber (`UpdateSerialNumberByIdAsync`)

```csharp
public async Task<bool> UpdateSerialNumberByIdAsync(int idEleve, string serialNumber)
{
    var eleve = await _context.Eleves.FindAsync(idEleve);
    if (eleve == null)
        return false;

    // ✅ UNICITÉ SERIAL NUMBER: Vérifier que le nouveau SerialNumber n'est pas déjà utilisé
    if (!string.IsNullOrEmpty(serialNumber) && serialNumber != eleve.SerialNumber)
    {
        var serialNumberExistsByOtherEleve = await _context.Eleves
            .AnyAsync(e => e.SerialNumber == serialNumber && e.IdEleve != idEleve);
        
        if (serialNumberExistsByOtherEleve)
        {
            throw new InvalidOperationException(
                $"Un autre élève avec le SerialNumber '{serialNumber}' existe déjà. " +
                $"Chaque SerialNumber doit être unique dans le système. " +
                $"Cet appareil est peut-être déjà lié à un autre élève."
            );
        }
    }

    eleve.SerialNumber = serialNumber;
    await _context.SaveChangesAsync();
    return true;
}
```

---

#### c) Méthode helper pour vérifier l'existence

```csharp
public async Task<bool> ExistsBySerialNumberAsync(string serialNumber)
{
    return await _context.Eleves.AnyAsync(e => e.SerialNumber == serialNumber);
}
```

---

### 2️⃣ **Interface IEleveRepository**

Ajout de la signature de méthode :

```csharp
public interface IEleveRepository
{
    // ... autres méthodes
    Task<bool> ExistsBySerialNumberAsync(string serialNumber); // ✅ UNICITÉ SERIAL NUMBER
}
```

---

### 3️⃣ **Contrainte d'unicité en base de données (KelasiNaBisoDbContext.cs)**

```csharp
// ✅ UNICITÉ SERIAL NUMBER ÉLÈVE: Index unique sur le SerialNumber
modelBuilder.Entity<Eleve>()
    .HasIndex(e => e.SerialNumber)
    .IsUnique()
    .HasDatabaseName("IX_Eleves_SerialNumber_Unique");
```

**Avantages :**
- 🔒 **Protection au niveau base de données** : Garantie absolue d'unicité
- 🚀 **Performance** : Index optimise les recherches par SerialNumber
- 🛡️ **Sécurité renforcée** : Double couche de protection (application + BDD)

---

## 📊 Stratégie de protection à deux niveaux

| Niveau | Outil | Description | Avantage |
|--------|-------|-------------|----------|
| **1. Application** | `EleveService` | Validation avant insertion/mise à jour | Message d'erreur clair et métier |
| **2. Base de données** | Index unique | Contrainte SQL `UNIQUE` | Garantie absolue d'unicité |

---

## 🧪 Scénarios de test

### ✅ Scénario 1 : Création avec SerialNumber unique
**Action :** Créer un élève avec `serialNumber = "SN-ELEVE-001"`  
**Résultat attendu :** ✅ Élève créé avec succès

---

### ❌ Scénario 2 : Création avec SerialNumber existant
**Action :** Créer un deuxième élève avec `serialNumber = "SN-ELEVE-001"`  
**Résultat attendu :** ❌ Erreur 400 - `"Un élève avec le SerialNumber 'SN-ELEVE-001' existe déjà"`

---

### ✅ Scénario 3 : Création sans SerialNumber (NULL)
**Action :** Créer un élève avec `serialNumber = null`  
**Résultat attendu :** ✅ Élève créé avec succès (SerialNumber facultatif)

---

### ✅ Scénario 4 : Mise à jour avec un nouveau SerialNumber unique
**Action :** Changer le SerialNumber d'un élève vers `"SN-NEW-DEVICE"`  
**Résultat attendu :** ✅ Mise à jour réussie

---

### ❌ Scénario 5 : Mise à jour avec SerialNumber d'un autre élève
**Action :** Essayer de changer le SerialNumber vers un SerialNumber déjà utilisé  
**Résultat attendu :** ❌ Erreur 400 - `"Un autre élève avec le SerialNumber '...' existe déjà"`

---

## 🔄 Cas d'usage pratiques

### 1. Application mobile parent - Premier login

```javascript
// 1. Le parent se connecte avec les identifiants de son enfant
const response = await authenticateParent(eleveMatricule, password);

// 2. Récupérer le SerialNumber du device du parent
const deviceSerial = await getDeviceSerialNumber();

// 3. Enregistrer le SerialNumber de l'élève
await updateEleveSerialNumber(eleve.id, deviceSerial);

// 4. Prochaines connexions : vérifier le SerialNumber
if (eleve.serialNumber !== deviceSerial) {
    throw new Error("Appareil non autorisé pour cet élève");
}
```

---

### 2. Consultation des notes par le parent

```javascript
// Vérifier que le parent accède depuis son device autorisé
async function getEleveNotes(eleveId) {
    const currentDeviceSerial = await getDeviceSerialNumber();
    const eleve = await getEleveById(eleveId);
    
    if (eleve.serialNumber && eleve.serialNumber !== currentDeviceSerial) {
        throw new Error(
            "Vous ne pouvez accéder à ces informations que depuis votre appareil autorisé. " +
            "Device actuel: " + currentDeviceSerial + ", " +
            "Device autorisé: " + eleve.serialNumber
        );
    }
    
    // Récupérer les notes
    return await getNotesByEleve(eleveId);
}
```

---

### 3. Changement de téléphone parent

```javascript
// Scénario : Le parent a un nouveau téléphone

// 1. Connexion depuis le nouveau device
const newDeviceSerial = await getDeviceSerialNumber();

// 2. Détection du changement
if (eleve.serialNumber !== newDeviceSerial) {
    // Demander confirmation
    const confirm = await askParentConfirmation(
        "Vous vous connectez depuis un nouvel appareil. " +
        "Voulez-vous lier ce device pour consulter les infos de votre enfant ?"
    );
    
    if (confirm) {
        // Mettre à jour le SerialNumber
        await updateEleveSerialNumber(eleve.id, newDeviceSerial);
    } else {
        // Bloquer l'accès
        throw new Error("Accès refusé");
    }
}
```

---

## 🔐 Différences Agent vs Élève

| Critère | `Agent.SerialNumber` | `Eleve.SerialNumber` |
|---------|---------------------|---------------------|
| **Utilisateur** | L'agent lui-même | Parent de l'élève |
| **Objectif** | Pointage présence agent | Consultation infos élève |
| **Table** | `Agents` | `Eleves` |
| **Espace unicité** | Agents uniquement | Élèves uniquement |
| **Protection** | ✅ Double | ✅ Double |
| **Peuvent partager** | ✅ Oui (espaces séparés) | ✅ Oui (espaces séparés) |

**Important :** Un agent et un élève **PEUVENT** avoir le même SerialNumber car ils sont dans des espaces d'unicité séparés.

---

## 📝 Messages d'erreur

### Lors de la création
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Un élève avec le SerialNumber 'SN-ELEVE-001' existe déjà. Chaque SerialNumber doit être unique dans le système. Cet appareil est peut-être déjà lié à un autre élève."
}
```

### Lors de la mise à jour
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Un autre élève avec le SerialNumber 'SN-ELEVE-001' existe déjà. Chaque SerialNumber doit être unique dans le système. Cet appareil est peut-être déjà lié à un autre élève."
}
```

---

## ✅ Avantages de cette implémentation

| Avantage | Description |
|----------|-------------|
| 🛡️ **Double protection** | Validation applicative + contrainte BDD |
| 📊 **Performance optimale** | Index accélère les recherches |
| 👨‍👩‍👧 **Contrôle parental** | Parent accède uniquement depuis son device |
| 🔄 **Mise à jour intelligente** | Pas de vérification si le SerialNumber ne change pas |
| 🔐 **Sécurité renforcée** | Un device parent = Un élève uniquement |
| ✨ **Flexibilité** | SerialNumber facultatif (NULL autorisé) |

---

## 🔄 Migration base de données

Pour appliquer la contrainte d'unicité sur une base existante :

### Option 1 : Migration Entity Framework (Recommandé)

```bash
# Créer une nouvelle migration
dotnet ef migrations add Add_Unique_SerialNumber_Eleve_Index

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
    FROM Eleves
    WHERE SerialNumber IS NOT NULL
)
SELECT * FROM CTE WHERE RowNum > 1; -- Afficher les doublons

-- Créer l'index unique
CREATE UNIQUE INDEX IX_Eleves_SerialNumber_Unique 
ON Eleves(SerialNumber)
WHERE SerialNumber IS NOT NULL;
```

---

## 📌 Notes importantes

1. **SerialNumber facultatif** : Un élève peut exister sans SerialNumber
2. **NULL autorisé** : Plusieurs élèves peuvent avoir `SerialNumber = NULL`
3. **Format libre** : Aucune contrainte de format (UUID, IMEI, custom, etc.)
4. **Espace séparé** : Un agent et un élève peuvent avoir le même SerialNumber
5. **Device parent** : Le SerialNumber représente l'appareil du parent, pas de l'élève

---

## 🎓 Exemple d'utilisation complète

```csharp
// INSCRIPTION ÉLÈVE AVEC SERIAL NUMBER
var eleve = new Eleve
{
    Nom = "MUKENDI",
    Prenom = "Jean",
    SerialNumber = "DEVICE-PARENT-ABC-123",
    // ... autres champs
};

try
{
    var createdEleve = await _eleveService.CreateAsync(eleve);
    Console.WriteLine($"✅ Élève créé avec SerialNumber : {createdEleve.SerialNumber}");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"❌ Erreur : {ex.Message}");
    // Afficher : "Ce device est déjà lié à un autre élève"
}

// CHANGEMENT DE DEVICE PARENT
try
{
    await _eleveService.UpdateSerialNumberByIdAsync(eleveId, "DEVICE-PARENT-XYZ-789");
    Console.WriteLine("✅ SerialNumber mis à jour avec succès");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"❌ Erreur : {ex.Message}");
}
```

---

## ✅ Checklist d'implémentation

- [x] Champ `SerialNumber` dans le modèle `Eleve`
- [x] Méthode `ExistsBySerialNumberAsync()` dans `IEleveRepository`
- [x] Implémentation dans `EleveService`
- [x] Validation dans `CreateAsync()`
- [x] Validation dans `UpdateSerialNumberByIdAsync()`
- [x] Validation dans `UpdateSerialNumberByMatriculeAsync()`
- [x] Index unique en base de données (`IX_Eleves_SerialNumber_Unique`)
- [x] Fichier de tests HTTP (`test-unicite-serial-number-eleve.http`)
- [x] Documentation complète (`UNICITE_SERIAL_NUMBER_ELEVE.md`)
- [ ] Migration base de données appliquée
- [ ] Tests unitaires
- [ ] Tests d'intégration

---

## 🚀 Prochaines étapes suggérées

1. ✅ **Appliquer la migration** pour créer l'index unique
2. ✅ **Tester avec le fichier .http** pour valider tous les scénarios
3. ✅ **Nettoyer les doublons existants** si nécessaire avant la migration
4. 📱 **Intégrer dans l'application mobile parent** pour lier automatiquement les devices
5. 👨‍👩‍👧 **Créer une interface parent** pour gérer les devices autorisés
6. 🔔 **Configurer des alertes** en cas de tentative de doublon
7. 📊 **Tableau de bord** pour voir les devices liés aux élèves

---

## 🔗 Récapitulatif global des contraintes SerialNumber

| Modèle | Index BDD | Espace unicité | Utilisateur |
|--------|-----------|----------------|-------------|
| **Agent** | `IX_Agents_SerialNumber_Unique` | Agents | Agent lui-même |
| **Eleve** | `IX_Eleves_SerialNumber_Unique` | Élèves | Parent de l'élève |

**Note importante :** Les deux espaces sont complètement séparés. Un même SerialNumber peut exister dans les deux tables sans conflit.

---

**🎉 L'unicité des SerialNumber élèves est maintenant pleinement implémentée et documentée !**

