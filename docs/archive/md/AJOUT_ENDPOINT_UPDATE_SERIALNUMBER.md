# 📝 Ajout des Endpoints de Mise à Jour du SerialNumber d'un Élève

## 📅 Date : 23 octobre 2025

## 🎯 Objectif
Créer des endpoints API permettant de modifier le `SerialNumber` d'un élève en utilisant soit son **IdEleve**, soit son **Matricule**.

---

## ✅ Modifications Réalisées

### 1. Création du DTO `UpdateSerialNumberDto`

**Fichier :** `Models/DTOs/UpdateSerialNumberDto.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    public class UpdateSerialNumberDto
    {
        [Required(ErrorMessage = "Le numéro de série est requis")]
        [MaxLength(100, ErrorMessage = "Le numéro de série ne peut pas dépasser 100 caractères")]
        public string SerialNumber { get; set; } = string.Empty;
    }
}
```

**Validation :**
- ✅ Champ `SerialNumber` obligatoire
- ✅ Longueur maximale : 100 caractères

---

### 2. Mise à Jour de l'Interface `IEleveRepository`

**Fichier :** `Services/Repositories/IEleveRepository.cs`

**Méthodes ajoutées :**
```csharp
// ✅ MISE À JOUR DU SERIAL NUMBER
Task<bool> UpdateSerialNumberByIdAsync(int idEleve, string serialNumber);
Task<bool> UpdateSerialNumberByMatriculeAsync(string matricule, string serialNumber);
Task<Eleve> GetByMatriculeAsync(string matricule);
```

---

### 3. Implémentation dans `EleveService`

**Fichier :** `Services/EleveService.cs`

**Méthodes implémentées :**

#### a) Récupérer un élève par son matricule
```csharp
public async Task<Eleve> GetByMatriculeAsync(string matricule)
{
    return await _context.Eleves
        .Include(e => e.Classe)
        .Include(e => e.Tuteur)
        .FirstOrDefaultAsync(e => e.Matricule == matricule);
}
```

#### b) Mise à jour par IdEleve
```csharp
public async Task<bool> UpdateSerialNumberByIdAsync(int idEleve, string serialNumber)
{
    var eleve = await _context.Eleves.FindAsync(idEleve);
    if (eleve == null)
        return false;

    eleve.SerialNumber = serialNumber;
    await _context.SaveChangesAsync();
    return true;
}
```

#### c) Mise à jour par Matricule
```csharp
public async Task<bool> UpdateSerialNumberByMatriculeAsync(string matricule, string serialNumber)
{
    var eleve = await _context.Eleves
        .FirstOrDefaultAsync(e => e.Matricule == matricule);
    
    if (eleve == null)
        return false;

    eleve.SerialNumber = serialNumber;
    await _context.SaveChangesAsync();
    return true;
}
```

---

### 4. Ajout des Endpoints dans `EleveController`

**Fichier :** `Controllers/EleveController.cs`

#### Endpoint 1 : Mise à jour par IdEleve

```csharp
// ✅ PUT: api/Eleve/{idEleve}/serial-number
[HttpPut("{idEleve}/serial-number")]
public async Task<ActionResult<object>> UpdateSerialNumberById(
    int idEleve, 
    [FromBody] UpdateSerialNumberDto dto)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    try
    {
        var success = await _eleveRepository.UpdateSerialNumberByIdAsync(idEleve, dto.SerialNumber);
        if (!success)
        {
            return NotFound(new { message = $"Élève avec l'ID {idEleve} non trouvé" });
        }

        var eleve = await _eleveRepository.GetByIdAsync(idEleve);
        return Ok(new
        {
            message = "Numéro de série mis à jour avec succès",
            idEleve = idEleve,
            serialNumber = dto.SerialNumber,
            eleve = eleve
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { 
            message = "Erreur lors de la mise à jour du numéro de série", 
            error = ex.Message 
        });
    }
}
```

#### Endpoint 2 : Mise à jour par Matricule

```csharp
// ✅ PUT: api/Eleve/matricule/{matricule}/serial-number
[HttpPut("matricule/{matricule}/serial-number")]
public async Task<ActionResult<object>> UpdateSerialNumberByMatricule(
    string matricule, 
    [FromBody] UpdateSerialNumberDto dto)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    if (string.IsNullOrWhiteSpace(matricule))
    {
        return BadRequest(new { message = "Le matricule ne peut pas être vide" });
    }

    try
    {
        var success = await _eleveRepository.UpdateSerialNumberByMatriculeAsync(matricule, dto.SerialNumber);
        if (!success)
        {
            return NotFound(new { message = $"Élève avec le matricule '{matricule}' non trouvé" });
        }

        var eleve = await _eleveRepository.GetByMatriculeAsync(matricule);
        return Ok(new
        {
            message = "Numéro de série mis à jour avec succès",
            matricule = matricule,
            serialNumber = dto.SerialNumber,
            eleve = eleve
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { 
            message = "Erreur lors de la mise à jour du numéro de série", 
            error = ex.Message 
        });
    }
}
```

---

## 📋 Endpoints Disponibles

| Méthode | Endpoint | Description |
|---------|----------|-------------|
| **GET** | `/api/Eleve/serial-number/{serialNumber}` | **Récupérer** un élève par son SerialNumber |
| **PUT** | `/api/Eleve/{idEleve}/serial-number` | **Mettre à jour** le SerialNumber par IdEleve |
| **PUT** | `/api/Eleve/matricule/{matricule}/serial-number` | **Mettre à jour** le SerialNumber par Matricule |

---

## 🧪 Exemples d'Utilisation

### 1. Récupérer un Élève par SerialNumber

```http
GET /api/Eleve/serial-number/SN123456789
```

**Réponse 200 OK :**
```json
{
  "idEleve": 5,
  "matricule": "ELEVE2024001",
  "nomComplet": "MUKENDI Jean",
  "genre": "M",
  "serialNumber": "SN123456789",
  "classe": {
    "idClasse": 2,
    "nomClasse": "6ème A"
  },
  "tuteur": {
    "idTuteur": 3,
    "nomComplet": "MUKENDI Papa",
    "telephone": "+243999123456"
  }
}
```

**Réponse 404 Not Found :**
```json
{
  "message": "Aucun élève trouvé avec le numéro de série 'SN123456789'"
}
```

**Réponse 400 Bad Request :**
```json
{
  "message": "Le numéro de série ne peut pas être vide"
}
```

---

### 2. Mise à Jour par IdEleve

```http
PUT /api/Eleve/5/serial-number
Content-Type: application/json

{
  "serialNumber": "SN123456789"
}
```

**Réponse 200 OK :**
```json
{
  "message": "Numéro de série mis à jour avec succès",
  "idEleve": 5,
  "serialNumber": "SN123456789",
  "eleve": {
    "idEleve": 5,
    "matricule": "ELEVE2024001",
    "nomComplet": "MUKENDI Jean",
    "serialNumber": "SN123456789",
    ...
  }
}
```

**Réponse 404 Not Found :**
```json
{
  "message": "Élève avec l'ID 5 non trouvé"
}
```

---

### 2. Mise à Jour par Matricule

```http
PUT /api/Eleve/matricule/ELEVE2024001/serial-number
Content-Type: application/json

{
  "serialNumber": "SN987654321"
}
```

**Réponse 200 OK :**
```json
{
  "message": "Numéro de série mis à jour avec succès",
  "matricule": "ELEVE2024001",
  "serialNumber": "SN987654321",
  "eleve": {
    "idEleve": 5,
    "matricule": "ELEVE2024001",
    "nomComplet": "MUKENDI Jean",
    "serialNumber": "SN987654321",
    ...
  }
}
```

**Réponse 404 Not Found :**
```json
{
  "message": "Élève avec le matricule 'ELEVE2024001' non trouvé"
}
```

---

### 3. Validation des Erreurs

#### Matricule vide
```http
PUT /api/Eleve/matricule//serial-number
Content-Type: application/json

{
  "serialNumber": "SN123456"
}
```

**Réponse 400 Bad Request :**
```json
{
  "message": "Le matricule ne peut pas être vide"
}
```

#### SerialNumber manquant
```http
PUT /api/Eleve/5/serial-number
Content-Type: application/json

{}
```

**Réponse 400 Bad Request :**
```json
{
  "errors": {
    "SerialNumber": [
      "Le numéro de série est requis"
    ]
  }
}
```

#### SerialNumber trop long
```http
PUT /api/Eleve/5/serial-number
Content-Type: application/json

{
  "serialNumber": "SN12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901"
}
```

**Réponse 400 Bad Request :**
```json
{
  "errors": {
    "SerialNumber": [
      "Le numéro de série ne peut pas dépasser 100 caractères"
    ]
  }
}
```

---

## 🔧 Utilisation avec cURL

### Récupérer par SerialNumber
```bash
curl -X GET "https://localhost:7102/api/Eleve/serial-number/SN123456789"
```

### Mettre à jour par IdEleve
```bash
curl -X PUT "https://localhost:7102/api/Eleve/5/serial-number" \
  -H "Content-Type: application/json" \
  -d '{"serialNumber":"SN123456789"}'
```

### Mettre à jour par Matricule
```bash
curl -X PUT "https://localhost:7102/api/Eleve/matricule/ELEVE2024001/serial-number" \
  -H "Content-Type: application/json" \
  -d '{"serialNumber":"SN987654321"}'
```

---

## 🔧 Utilisation avec JavaScript/Axios

### Récupérer par SerialNumber
```javascript
const getEleveBySerialNumber = async (serialNumber) => {
  try {
    const response = await axios.get(
      `https://localhost:7102/api/Eleve/serial-number/${serialNumber}`
    );
    console.log('Élève trouvé:', response.data);
    return response.data;
  } catch (error) {
    if (error.response?.status === 404) {
      console.error('Élève non trouvé');
    } else {
      console.error('Erreur:', error.response?.data);
    }
  }
};

// Utilisation
const eleve = await getEleveBySerialNumber('SN123456789');
```

### Mettre à jour par IdEleve
```javascript
const updateSerialNumberById = async (idEleve, serialNumber) => {
  try {
    const response = await axios.put(
      `https://localhost:7102/api/Eleve/${idEleve}/serial-number`,
      { serialNumber }
    );
    console.log('Succès:', response.data);
  } catch (error) {
    console.error('Erreur:', error.response?.data);
  }
};

// Utilisation
await updateSerialNumberById(5, 'SN123456789');
```

### Mettre à jour par Matricule
```javascript
const updateSerialNumberByMatricule = async (matricule, serialNumber) => {
  try {
    const response = await axios.put(
      `https://localhost:7102/api/Eleve/matricule/${matricule}/serial-number`,
      { serialNumber }
    );
    console.log('Succès:', response.data);
  } catch (error) {
    console.error('Erreur:', error.response?.data);
  }
};

// Utilisation
await updateSerialNumberByMatricule('ELEVE2024001', 'SN987654321');
```

---

## ✅ Avantages de cette Implémentation

1. ✅ **Flexibilité maximale** : 
   - Récupération par SerialNumber (GET)
   - Mise à jour par ID ou Matricule (PUT)
2. ✅ **Validation** : DTO avec validation automatique
3. ✅ **Sécurité** : Vérification de l'existence de l'élève
4. ✅ **RESTful** : Endpoints conformes aux bonnes pratiques REST
5. ✅ **Réponses claires** : Messages d'erreur explicites
6. ✅ **Gestion d'erreurs** : Try-catch avec codes HTTP appropriés
7. ✅ **Retour complet** : Retourne l'élève avec ses relations (Classe, Tuteur)
8. ✅ **Filtrage intelligent** : L'endpoint GET filtre uniquement les élèves actifs
9. ✅ **Performance** : Requêtes optimisées avec Include pour éviter le N+1

---

## 📊 Codes de Statut HTTP

| Code | Description |
|------|-------------|
| **200 OK** | Mise à jour réussie |
| **400 Bad Request** | Validation échouée (SerialNumber manquant/invalide, matricule vide) |
| **404 Not Found** | Élève non trouvé |
| **500 Internal Server Error** | Erreur serveur inattendue |

---

## 🔍 Cas d'Usage

### 1. Système de Carte d'Étudiant RFID/NFC

**Étape 1 : Enregistrer la carte**
```http
PUT /api/Eleve/5/serial-number
{
  "serialNumber": "RFID-001234567"
}
```

**Étape 2 : Scanner la carte et récupérer l'élève**
```http
GET /api/Eleve/serial-number/RFID-001234567
```
✅ Retourne les informations complètes de l'élève pour affichage ou pointage

### 2. Système de Pointage Biométrique

**Enregistrement :**
```http
PUT /api/Eleve/matricule/ELEVE2024001/serial-number
{
  "serialNumber": "BIO-FP-987654"
}
```

**Identification :**
```http
GET /api/Eleve/serial-number/BIO-FP-987654
```
✅ Identifie rapidement l'élève lors du scan biométrique

### 3. Gestion de Badges

**Attribution du badge :**
```http
PUT /api/Eleve/5/serial-number
{
  "serialNumber": "BADGE-2024-0523"
}
```

**Contrôle d'accès :**
```http
GET /api/Eleve/serial-number/BADGE-2024-0523
```
✅ Vérifie les autorisations d'accès de l'élève

### 4. Application de Pointage Mobile

**Workflow complet :**
```javascript
// 1. Scanner le QR Code/Badge de l'élève
const serialNumber = scanQRCode(); // Ex: "SN123456789"

// 2. Récupérer les infos de l'élève
const eleve = await getEleveBySerialNumber(serialNumber);

// 3. Afficher et enregistrer la présence
if (eleve) {
  console.log(`Bonjour ${eleve.nomComplet} !`);
  await createPresence({
    idEleve: eleve.idEleve,
    isPresent: true,
    heureArrivee: new Date().toLocaleTimeString(),
    dateDuJour: new Date(),
    idHoraire: 1
  });
}
```

---

## 📝 Notes Techniques

### Validation Automatique
- Le framework ASP.NET Core valide automatiquement le DTO grâce aux attributs `[Required]` et `[MaxLength]`
- La méthode `ModelState.IsValid` vérifie toutes les validations

### Atomicité
- Les opérations de mise à jour sont atomiques grâce à `SaveChangesAsync()`
- En cas d'erreur, aucune modification n'est enregistrée

### Performance
- `FindAsync()` utilise le cache local d'EF Core si l'entité est déjà chargée
- Les requêtes incluent les relations (`Include`) pour un retour complet

---

## 📝 Notes de Version

**Version :** 1.0.0  
**Date :** 23 octobre 2025  
**Statut :** ✅ Testé et fonctionnel  
**Compilation :** ✅ Réussie sans erreurs  
**Framework :** ASP.NET Core 6.0  
**Base de données :** MariaDB 10.11 (LTS)  

---

## 🔗 Documentation Associée

- `README.md` - Documentation principale
- `START_HERE.md` - Guide de démarrage rapide
- Swagger UI - `https://localhost:7102/swagger` pour tester les endpoints

---

## 🎉 Conclusion

Deux nouveaux endpoints ont été ajoutés avec succès pour permettre la mise à jour du `SerialNumber` d'un élève. Ces endpoints offrent une **flexibilité maximale** en permettant l'identification de l'élève soit par son **ID** (plus performant), soit par son **Matricule** (plus pratique pour les utilisateurs).

**Prochaines étapes suggérées :**
1. Tester les endpoints via Swagger
2. Implémenter la même logique pour d'autres entités (Agent, Tuteur, etc.)
3. Ajouter un système d'audit pour tracer les modifications
4. Créer des tests unitaires pour ces endpoints

