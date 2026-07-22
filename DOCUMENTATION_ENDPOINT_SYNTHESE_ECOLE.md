# 📊 Documentation : Endpoint SyntheseEcole

**Date** : 2025-01-16  
**Version** : 1.0  
**Statut** : ✅ Implémenté

---

## 🎯 Description

Endpoint permettant de récupérer une synthèse complète de toutes les écoles avec leurs statistiques détaillées (nombre d'élèves, répartition par genre, pourcentages).

**🔒 Accès réservé aux Super-Admin uniquement**

---

## 📍 Endpoint

```
GET /api/Dashboard/synthese-ecole
```

---

## 🔐 Autorisation

- **Rôle requis** : `Super-Admin`
- **Authentification** : JWT Token requis
- **Autorisation** : `[Authorize(Roles = "Super-Admin")]`

**Réponses d'erreur** :
- `401 Unauthorized` : Token manquant ou invalide
- `403 Forbidden` : Utilisateur n'est pas Super-Admin

---

## 📥 Paramètres

Aucun paramètre requis.

---

## 📤 Réponse

### **Format** : `application/json`

### **Structure** : `List<RepartitionEcoleDto>`

### **Exemple de réponse** :

```json
[
  {
    "idEcole": 9,
    "nomEcole": "Ecole Lusay",
    "province": "KInshasa",
    "ville": "KINSHASA",
    "nombreEleves": 291,
    "nombreElevesActifs": 287,
    "nombreElevesFilles": 163,
    "nombreElevesGarcons": 128,
    "nombreElevesFillesActives": 161,
    "nombreElevesGarconsActifs": 126,
    "pourcentage": 27.58,
    "pourcentageFilles": 56.01,
    "pourcentageGarcons": 43.99
  },
  {
    "idEcole": 10,
    "nomEcole": "Ecole Exemple",
    "province": "Kinshasa",
    "ville": "KINSHASA",
    "nombreEleves": 150,
    "nombreElevesActifs": 145,
    "nombreElevesFilles": 80,
    "nombreElevesGarcons": 70,
    "nombreElevesFillesActives": 78,
    "nombreElevesGarconsActifs": 67,
    "pourcentage": 14.20,
    "pourcentageFilles": 53.33,
    "pourcentageGarcons": 46.67
  }
]
```

---

## 📊 Propriétés de la Réponse

| Propriété | Type | Description |
|-----------|------|-------------|
| `idEcole` | `int` | Identifiant unique de l'école |
| `nomEcole` | `string` | Nom de l'école |
| `province` | `string?` | Province où se trouve l'école (nullable) |
| `ville` | `string?` | Ville où se trouve l'école (nullable) |
| `nombreEleves` | `int` | Nombre total d'élèves (tous statuts confondus) |
| `nombreElevesActifs` | `int` | Nombre d'élèves actifs (`Statut == true`) |
| `nombreElevesFilles` | `int` | Nombre total d'élèves filles (tous statuts) |
| `nombreElevesGarcons` | `int` | Nombre total d'élèves garçons (tous statuts) |
| `nombreElevesFillesActives` | `int` | Nombre d'élèves filles actives |
| `nombreElevesGarconsActifs` | `int` | Nombre d'élèves garçons actifs |
| `pourcentage` | `decimal` | Pourcentage d'élèves de cette école par rapport au total global (toutes écoles) |
| `pourcentageFilles` | `decimal` | Pourcentage de filles dans cette école |
| `pourcentageGarcons` | `decimal` | Pourcentage de garçons dans cette école |

---

## 📝 Notes Importantes

### **Calculs des Pourcentages**

1. **`pourcentage`** : 
   - Calculé comme : `(nombreEleves de l'école / totalEleves global) * 100`
   - Représente la part de cette école dans le total global de toutes les écoles

2. **`pourcentageFilles`** et **`pourcentageGarcons`** :
   - Calculés comme : `(nombreElevesFilles / nombreEleves) * 100`
   - Représentent la répartition par genre au sein de chaque école

### **Tri**

Les écoles sont triées par **nombre d'élèves décroissant** (de la plus grande à la plus petite).

### **Filtrage**

- Seules les écoles ayant au moins un élève sont incluses dans la réponse
- Les écoles sans élèves ne sont pas listées

---

## 🔧 Implémentation Technique

### **Fichier** : `Controllers/DashboardController.cs`

### **Méthode** : `GetSyntheseEcole()`

### **Méthode privée** : `CalculerSyntheseEcolesAsync()`

### **DTO utilisé** : `RepartitionEcoleDto` (défini dans `Models/DTOs/Reporting/DashboardGlobalDto.cs`)

---

## 🧪 Exemple d'Utilisation

### **cURL**

```bash
curl -X GET "https://localhost:7102/api/Dashboard/synthese-ecole" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json"
```

### **HTTP (REST Client)**

```http
GET /api/Dashboard/synthese-ecole
Authorization: Bearer YOUR_JWT_TOKEN
```

### **JavaScript (Fetch)**

```javascript
const response = await fetch('https://localhost:7102/api/Dashboard/synthese-ecole', {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
});

const syntheseEcoles = await response.json();
console.log(syntheseEcoles);
```

---

## ✅ Codes de Réponse

| Code | Description |
|------|-------------|
| `200 OK` | Succès - Liste des écoles retournée |
| `401 Unauthorized` | Token manquant ou invalide |
| `403 Forbidden` | Utilisateur n'est pas Super-Admin |
| `500 Internal Server Error` | Erreur serveur lors du calcul |

---

## 🔍 Exemple de Réponse d'Erreur

### **403 Forbidden**

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "traceId": "0HNHUN4V5J019:00000003"
}
```

### **500 Internal Server Error**

```json
{
  "message": "Erreur lors de la récupération de la synthèse des écoles",
  "error": "Détails de l'erreur..."
}
```

---

## 📋 Checklist de Test

- [ ] Tester avec un utilisateur Super-Admin → Doit retourner la liste
- [ ] Tester avec un utilisateur non Super-Admin → Doit retourner 403
- [ ] Tester sans token → Doit retourner 401
- [ ] Vérifier que `nombreElevesActifs <= nombreEleves`
- [ ] Vérifier que `nombreElevesFillesActives <= nombreElevesFilles`
- [ ] Vérifier que `nombreElevesGarconsActifs <= nombreElevesGarcons`
- [ ] Vérifier que `pourcentageFilles + pourcentageGarcons ≈ 100` (avec tolérance d'arrondi)
- [ ] Vérifier que la somme des `pourcentage` ≈ 100% (toutes écoles)
- [ ] Vérifier que les écoles sont triées par nombre d'élèves décroissant

---

## 🔄 Différences avec `/api/Dashboard/super-admin`

| Aspect | `/synthese-ecole` | `/super-admin` |
|--------|-------------------|----------------|
| **Données retournées** | Liste des écoles uniquement | Dashboard complet (statistiques, répartitions, présences, paiements) |
| **Format** | `List<RepartitionEcoleDto>` | `DashboardSuperAdminDto` |
| **Performance** | Plus rapide (moins de calculs) | Plus complet (plus de calculs) |
| **Usage** | Vue synthétique rapide | Vue d'ensemble complète |

---

**Version** : 1.0  
**Date** : 2025-01-16  
**Statut** : ✅ Implémenté et prêt pour tests
