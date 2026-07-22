# ⚡ GUIDE DE DÉMARRAGE RAPIDE - TITULAIRECLASSE

> Pour les développeurs qui veulent comprendre et utiliser le nouveau système en **5 minutes**.

---

## 🎯 C'EST QUOI ?

**TitulaireClasse** = Un système pour affecter **1 enseignant** à **1 classe** en Maternelle/Primaire.

```
AVANT :
  Affecter un prof en Primaire = 10+ requêtes HTTP 😰
  (1 par cours : Français, Math, Sciences...)

MAINTENANT :
  Affecter un prof = 1 seule requête HTTP 🎉
```

---

## 🚀 DÉMARRAGE EN 3 ÉTAPES

### 1️⃣ Appliquer la migration

```bash
cd G:\KelasiNaBiso\KelasiNaBisoAPI
dotnet ef migrations add AjoutTitulaireClasse
dotnet ef database update
```

### 2️⃣ Lancer l'API

```bash
dotnet run
```

### 3️⃣ Tester avec un exemple

```http
### Affecter un titulaire à une classe de Primaire
POST http://localhost:5000/api/TitulaireClasse
Authorization: Bearer YOUR_TOKEN
Content-Type: application/json

{
  "idAgent": 1,
  "idClasse": 2,
  "idAnneeScolaire": 1,
  "dateDebut": "2025-09-01",
  "commentaire": "Enseignant titulaire de 1ère Primaire A"
}
```

✅ **C'est tout !** L'enseignant est maintenant titulaire de la classe.

---

## 📖 LES 5 ENDPOINTS LES PLUS UTILISÉS

### 1. Créer un titulaire

```http
POST /api/TitulaireClasse
{
  "idAgent": 5,
  "idClasse": 10,
  "idAnneeScolaire": 2,
  "dateDebut": "2025-09-01"
}
```

### 2. Voir tous les titulaires (paginé)

```http
GET /api/TitulaireClasse/paged?PageSize=20
```

### 3. Voir le titulaire actuel d'une classe

```http
GET /api/TitulaireClasse/classe/10/annee/2/actif
```

### 4. Voir les classes d'un enseignant

```http
GET /api/TitulaireClasse/agent/5
```

### 5. Désactiver un titulaire (changement d'enseignant)

```http
PATCH /api/TitulaireClasse/12/toggle-statut
```

---

## ⚠️ LES 3 RÈGLES À CONNAÎTRE

### Règle 1 : 1 classe = 1 seul titulaire

```http
# ❌ Ceci échouera si la classe a déjà un titulaire
POST /api/TitulaireClasse
{
  "idAgent": 8,
  "idClasse": 10,  # Cette classe a déjà un titulaire !
  "idAnneeScolaire": 2
}

# ✅ Solution : Désactiver l'ancien titulaire d'abord
PATCH /api/TitulaireClasse/{id}/toggle-statut
```

### Règle 2 : 1 enseignant = 1 seule classe par année

```http
# ❌ Un enseignant ne peut pas être titulaire de 2 classes en même temps
POST /api/TitulaireClasse
{
  "idAgent": 5,      # Cet agent est déjà titulaire ailleurs !
  "idClasse": 15,
  "idAnneeScolaire": 2
}
```

### Règle 3 : Seuls les ENSEIGNANTS peuvent être titulaires

```http
# ❌ Un Directeur, Comptable, etc. ne peut pas être titulaire
POST /api/TitulaireClasse
{
  "idAgent": 3,  # Cet agent n'est pas enseignant !
  "idClasse": 10,
  "idAnneeScolaire": 2
}

# ✅ Vérifiez que agent.RoleAgent = "ENSEIGNANT"
```

---

## 🔍 VALIDATION AVANT AFFECTATION

Avant de créer un titulaire, vérifiez que c'est possible :

```http
### 1. Est-ce que la classe a déjà un titulaire ?
GET /api/TitulaireClasse/classe/10/annee/2/has-titulaire

# Réponse :
{
  "hasTitulaire": false,
  "message": "Cette classe n'a pas de titulaire actif"
}

### 2. Est-ce que l'enseignant est disponible ?
GET /api/TitulaireClasse/agent/5/annee/2/disponible

# Réponse :
{
  "estDisponible": true,
  "message": "L'agent est disponible pour être titulaire"
}

### 3. Si les 2 réponses sont OK, créez le titulaire
POST /api/TitulaireClasse { ... }
```

---

## 🎓 QUAND UTILISER QUOI ?

### Utilisez **TitulaireClasse** pour :

✅ Maternelle (1 enseignant pour tout)  
✅ Primaire (1 enseignant pour tout)  

### Utilisez **AffectationCours** pour :

✅ Secondaire (plusieurs enseignants spécialisés par cours)

---

## 📊 EXEMPLES DE CODE C#

### Créer un titulaire dans votre service

```csharp
public async Task<TitulaireClasse> AffecterTitulaireAsync(
    int idAgent, 
    int idClasse, 
    int idAnneeScolaire)
{
    // 1. Vérifier que la classe n'a pas déjà un titulaire
    var hasTitulaire = await _titulaireRepository
        .HasTitulaireActifAsync(idClasse, idAnneeScolaire);
    
    if (hasTitulaire)
    {
        throw new InvalidOperationException("Cette classe a déjà un titulaire");
    }

    // 2. Créer le titulaire
    var titulaire = new TitulaireClasse
    {
        IdAgent = idAgent,
        IdClasse = idClasse,
        IdAnneeScolaire = idAnneeScolaire,
        DateDebut = DateTime.Now,
        Statut = true
    };

    return await _titulaireRepository.CreateAsync(titulaire);
}
```

### Récupérer le titulaire actuel d'une classe

```csharp
public async Task<TitulaireClasse?> GetTitulaireActuelAsync(
    int idClasse, 
    int idAnneeScolaire)
{
    return await _titulaireRepository
        .GetTitulaireActifByClasseAsync(idClasse, idAnneeScolaire);
}
```

### Changer de titulaire

```csharp
public async Task ChangerTitulaireAsync(
    int idClasse, 
    int idAnneeScolaire, 
    int nouveauIdAgent)
{
    // 1. Désactiver l'ancien titulaire
    var ancienTitulaire = await _titulaireRepository
        .GetTitulaireActifByClasseAsync(idClasse, idAnneeScolaire);
    
    if (ancienTitulaire != null)
    {
        await _titulaireRepository.ToggleStatutAsync(ancienTitulaire.IdTitulaireClasse);
    }

    // 2. Créer le nouveau titulaire
    var nouveauTitulaire = new TitulaireClasse
    {
        IdAgent = nouveauIdAgent,
        IdClasse = idClasse,
        IdAnneeScolaire = idAnneeScolaire,
        DateDebut = DateTime.Now,
        Statut = true,
        Commentaire = "Remplacement"
    };

    await _titulaireRepository.CreateAsync(nouveauTitulaire);
}
```

---

## 🐛 ERREURS COURANTES & SOLUTIONS

### Erreur : "Un titulaire actif existe déjà"

**Cause :** Vous essayez d'affecter un 2ème titulaire à une classe.

**Solution :**
```http
# Désactiver l'ancien titulaire d'abord
PATCH /api/TitulaireClasse/{id}/toggle-statut
```

### Erreur : "L'agent n'est pas un enseignant"

**Cause :** L'agent n'a pas `RoleAgent = "ENSEIGNANT"`.

**Solution :**
```http
# Vérifier le rôle
GET /api/Agent/{id}

# Mettre à jour si nécessaire
PUT /api/Agent/{id}
{
  "roleAgent": "ENSEIGNANT",
  ...
}
```

### Erreur : "L'agent est déjà titulaire d'une autre classe"

**Cause :** Un enseignant ne peut être titulaire que d'une classe par année.

**Solution :**
```http
# Voir les classes de cet enseignant
GET /api/TitulaireClasse/agent/{id}

# Désactiver l'ancienne affectation si nécessaire
PATCH /api/TitulaireClasse/{id}/toggle-statut
```

---

## 📚 RESSOURCES

### Documentation Complète
📖 [DOCUMENTATION_TITULAIRE_CLASSE.md](./DOCUMENTATION_TITULAIRE_CLASSE.md)

### Tests HTTP
🧪 [test-titulaire-classe.http](./test-titulaire-classe.http)

### Récapitulatif Complet
📋 [RECAP_SYSTEME_AFFECTATION_INTELLIGENT.md](./RECAP_SYSTEME_AFFECTATION_INTELLIGENT.md)

### Migration SQL
🔄 [Scripts/Migration_AffectationCours_to_TitulaireClasse.sql](./Scripts/Migration_AffectationCours_to_TitulaireClasse.sql)

---

## ✅ CHECKLIST RAPIDE

Pour commencer à utiliser TitulaireClasse :

- [ ] Migration appliquée (`dotnet ef database update`)
- [ ] Remplir `NiveauEnseignement` dans la table Directions
- [ ] API lancée (`dotnet run`)
- [ ] Premier titulaire créé via POST
- [ ] Tests HTTP exécutés
- [ ] Documentation lue

---

## 💡 ASTUCE PRO

### Créer un endpoint personnalisé qui fait tout d'un coup

```csharp
[HttpPost("affecter-intelligent")]
public async Task<ActionResult> AffecterIntelligent(
    [FromBody] AffectationRequest request)
{
    // 1. Désactiver l'ancien titulaire automatiquement
    var ancien = await _titulaireRepository
        .GetTitulaireActifByClasseAsync(request.IdClasse, request.IdAnneeScolaire);
    
    if (ancien != null)
    {
        await _titulaireRepository.ToggleStatutAsync(ancien.IdTitulaireClasse);
    }

    // 2. Créer le nouveau
    var nouveau = new TitulaireClasse
    {
        IdAgent = request.IdAgent,
        IdClasse = request.IdClasse,
        IdAnneeScolaire = request.IdAnneeScolaire,
        DateDebut = DateTime.Now,
        Statut = true
    };

    var created = await _titulaireRepository.CreateAsync(nouveau);
    return Ok(created);
}
```

**Utilisation :**
```http
POST /api/TitulaireClasse/affecter-intelligent
{
  "idAgent": 5,
  "idClasse": 10,
  "idAnneeScolaire": 2
}
```

✅ Ça désactive l'ancien ET crée le nouveau en 1 seule requête !

---

**C'est tout ! Vous êtes prêt à utiliser TitulaireClasse.** 🚀

---

**Dernière mise à jour** : 27 janvier 2025  
**Version** : 1.0.0

