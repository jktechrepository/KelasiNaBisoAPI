# 📖 DOCUMENTATION - SYSTÈME TITULAIRE DE CLASSE

## 🎯 Vue d'Ensemble

Le système **TitulaireClasse** a été créé pour gérer intelligemment l'affectation des enseignants en **Maternelle** et **Primaire**, où **un seul enseignant est responsable de tous les cours d'une classe**.

---

## 🏫 CONTEXTE : Système Éducatif Congolais

### Différence entre les Niveaux

| Niveau | Système d'Enseignement | Solution Utilisée |
|--------|------------------------|-------------------|
| **MATERNELLE** | 1 enseignant titulaire pour toute la classe | ✅ **TitulaireClasse** |
| **PRIMAIRE** | 1 enseignant titulaire pour toute la classe | ✅ **TitulaireClasse** |
| **SECONDAIRE** | Plusieurs enseignants spécialisés par cours | ✅ **AffectationCours** |

---

## 📊 ARCHITECTURE

### Modèle de Données

```csharp
public class TitulaireClasse
{
    public int IdTitulaireClasse { get; set; }
    public int IdAgent { get; set; }           // Enseignant titulaire
    public int IdClasse { get; set; }          // Classe (Maternelle/Primaire)
    public int IdAnneeScolaire { get; set; }   // Année scolaire
    public DateTime DateDebut { get; set; }    // Début affectation
    public DateTime? DateFin { get; set; }     // Fin affectation (null si actif)
    public bool Statut { get; set; }           // true = actif
    public string? Commentaire { get; set; }
    
    // Navigation
    public Agent Agent { get; set; }
    public Classe Classe { get; set; }
    public AnneeScolaire AnneeScolaire { get; set; }
}
```

### Modification de Direction

```csharp
public class Direction
{
    // ... propriétés existantes ...
    
    public string? NiveauEnseignement { get; set; }
    // Valeurs: "MATERNELLE", "PRIMAIRE", "SECONDAIRE"
}
```

---

## ✅ RÈGLES MÉTIER

### 1️⃣ Un seul titulaire actif par classe

```sql
-- Index unique dans la base de données
CREATE UNIQUE INDEX IX_TitulaireClasse_Unique
ON TitulaireClasses (IdClasse, IdAnneeScolaire, Statut)
WHERE Statut = 1;
```

**Validation :**
- ✅ Une classe ne peut avoir qu'un seul titulaire actif pour une année scolaire
- ❌ Tentative d'affecter un second titulaire = Erreur

### 2️⃣ Un agent ne peut être titulaire que d'une classe

**Validation :**
- ✅ Un enseignant peut être titulaire d'UNE SEULE classe par année
- ❌ Tentative de double affectation = Erreur

### 3️⃣ L'agent doit être un enseignant

**Validation :**
- ✅ Vérification : `agent.RoleAgent == "ENSEIGNANT"`
- ❌ Un agent avec un autre rôle (Directeur, Comptable...) ne peut pas être titulaire

### 4️⃣ Recommandé pour Maternelle/Primaire

**Avertissement :**
- ⚠️ Si affectation à une classe de SECONDAIRE, un warning est généré dans les logs
- 💡 Pour le secondaire, utiliser `AffectationCours`

---

## 🚀 ENDPOINTS DISPONIBLES

### Pagination (Recommandé ✅)

```http
# Tous les titulaires (paginé)
GET /api/TitulaireClasse/paged?PageNumber=1&PageSize=20

# Titulaires d'un agent spécifique
GET /api/TitulaireClasse/agent/{idAgent}/paged?PageSize=10

# Historique d'une classe
GET /api/TitulaireClasse/classe/{idClasse}/paged?PageSize=10

# Titulaires d'une année scolaire
GET /api/TitulaireClasse/annee/{idAnneeScolaire}/paged?PageSize=20
```

### CRUD Classique

```http
# Récupérer tous les titulaires
GET /api/TitulaireClasse

# Récupérer un titulaire par ID
GET /api/TitulaireClasse/{id}

# Récupérer le titulaire actif d'une classe
GET /api/TitulaireClasse/classe/{idClasse}/annee/{idAnneeScolaire}/actif

# Créer un titulaire
POST /api/TitulaireClasse
Content-Type: application/json
{
  "idAgent": 1,
  "idClasse": 2,
  "idAnneeScolaire": 1,
  "dateDebut": "2025-01-27",
  "commentaire": "Titulaire de 2ème Primaire A"
}

# Mettre à jour un titulaire
PUT /api/TitulaireClasse/{id}

# Désactiver/Réactiver un titulaire
PATCH /api/TitulaireClasse/{id}/toggle-statut

# Supprimer un titulaire
DELETE /api/TitulaireClasse/{id}
```

### Validation

```http
# Vérifier si une classe a déjà un titulaire
GET /api/TitulaireClasse/classe/{idClasse}/annee/{idAnneeScolaire}/has-titulaire

# Vérifier si un agent est disponible
GET /api/TitulaireClasse/agent/{idAgent}/annee/{idAnneeScolaire}/disponible
```

---

## 💡 CAS D'USAGE

### Scénario 1 : Affecter un titulaire en Maternelle

```http
POST /api/TitulaireClasse
{
  "idAgent": 5,
  "idClasse": 1,           # Maternelle A
  "idAnneeScolaire": 2,    # 2025-2026
  "dateDebut": "2025-09-01",
  "commentaire": "Mme Kabila - Expérience en maternelle"
}
```

**Résultat :**
- ✅ L'enseignante Kabila devient titulaire de Maternelle A
- ✅ Elle enseigne TOUS les cours de cette classe
- ✅ Pas besoin de créer 10 `AffectationCours` différents

### Scénario 2 : Changement de titulaire en cours d'année

```http
# 1. Désactiver l'ancien titulaire
PATCH /api/TitulaireClasse/12/toggle-statut

# 2. Affecter le nouveau titulaire
POST /api/TitulaireClasse
{
  "idAgent": 8,
  "idClasse": 1,
  "idAnneeScolaire": 2,
  "dateDebut": "2025-12-01",
  "commentaire": "Remplacement suite à congé maternité"
}
```

### Scénario 3 : Vérification avant affectation

```http
# 1. Vérifier si la classe a déjà un titulaire
GET /api/TitulaireClasse/classe/1/annee/2/has-titulaire
# Réponse: { "hasTitulaire": false, "message": "..." }

# 2. Vérifier si l'enseignant est disponible
GET /api/TitulaireClasse/agent/5/annee/2/disponible
# Réponse: { "estDisponible": true, "message": "..." }

# 3. Procéder à l'affectation
POST /api/TitulaireClasse { ... }
```

---

## 📈 AVANTAGES DU SYSTÈME

### ✅ Simplicité

**Avant (AffectationCours uniquement) :**
```
Pour affecter un enseignant de Primaire, il fallait :
1. Créer Cours "Français" → Affecter Agent
2. Créer Cours "Math" → Affecter Agent
3. Créer Cours "Sciences" → Affecter Agent
4. Créer Cours "Histoire" → Affecter Agent
5. ... (10+ affectations différentes)
```

**Maintenant (TitulaireClasse) :**
```
1. Affecter Agent comme Titulaire → TERMINÉ ✅
```

### ✅ Conformité au Terrain

- Reflète exactement le système congolais
- Facilite la gestion administrative
- Simplifie l'emploi du temps

### ✅ Reporting Simplifié

```sql
-- Qui est le titulaire de 1ère A ?
SELECT Agent.Nom, Agent.Prenom
FROM TitulaireClasse
WHERE IdClasse = 5 AND IdAnneeScolaire = 2 AND Statut = 1;
```

### ✅ Interface Utilisateur Intuitive

```
[Interface Web]
┌─────────────────────────────────────┐
│ Classe: 1ère Primaire A             │
│                                     │
│ Titulaire: Mme Marie MUKENDI        │
│ Depuis: 01/09/2025                  │
│                                     │
│ [Changer Titulaire]                 │
└─────────────────────────────────────┘
```

---

## 🔄 DIFFÉRENCE AVEC AFFECTATIONCOURS

| Critère | TitulaireClasse | AffectationCours |
|---------|-----------------|------------------|
| **Niveau** | Maternelle, Primaire | Secondaire |
| **Nombre d'enseignants** | 1 seul (titulaire) | Plusieurs (spécialisés) |
| **Relation** | Agent → Classe | Agent → Cours → Classe |
| **Complexité** | Simple | Plus complexe |
| **Cas d'usage** | Enseignement polyvalent | Enseignement spécialisé |

---

## 🎯 MIGRATION DES DONNÉES

Si vous avez déjà utilisé `AffectationCours` pour Maternelle/Primaire :

```sql
-- Script de migration (exemple)
INSERT INTO TitulaireClasse (IdAgent, IdClasse, IdAnneeScolaire, DateDebut, Statut)
SELECT DISTINCT 
    ac.IdAgent,
    c.IdClasse,
    ac.IdAnneeScolaire,
    MIN(ac.DateAffectation) as DateDebut,
    1 as Statut
FROM AffectationCours ac
INNER JOIN Cours co ON ac.IdCours = co.IdCours
INNER JOIN Classe c ON co.IdClasse = c.IdClasse
INNER JOIN Direction d ON c.IdDirection = d.IdDirection
WHERE d.NiveauEnseignement IN ('MATERNELLE', 'PRIMAIRE')
GROUP BY ac.IdAgent, c.IdClasse, ac.IdAnneeScolaire;
```

---

## 🐛 DÉPANNAGE

### Erreur : "Un titulaire actif existe déjà"

**Cause :** Vous tentez d'affecter un second titulaire à une classe qui en a déjà un.

**Solution :**
```http
# 1. Vérifier le titulaire actuel
GET /api/TitulaireClasse/classe/1/annee/2/actif

# 2. Le désactiver si nécessaire
PATCH /api/TitulaireClasse/{id}/toggle-statut

# 3. Affecter le nouveau titulaire
POST /api/TitulaireClasse { ... }
```

### Erreur : "L'agent n'est pas un enseignant"

**Cause :** L'agent n'a pas le rôle "ENSEIGNANT".

**Solution :**
```http
# Vérifier le rôle de l'agent
GET /api/Agent/{id}
# S'assurer que RoleAgent = "ENSEIGNANT"

# Modifier le rôle si nécessaire
PUT /api/Agent/{id}
{ "roleAgent": "ENSEIGNANT", ... }
```

### Erreur : "L'agent est déjà titulaire d'une autre classe"

**Cause :** Un enseignant ne peut être titulaire que d'une seule classe par année.

**Solution :**
```http
# Voir les classes titularisées par cet agent
GET /api/TitulaireClasse/agent/{idAgent}

# Désactiver l'ancienne affectation si nécessaire
PATCH /api/TitulaireClasse/{id}/toggle-statut
```

---

## 📞 SUPPORT

Pour toute question ou suggestion :
- 📧 Email : support@kelasinabiso.com
- 📚 Documentation complète : `/docs`
- 🐛 Issues : Créer un ticket

---

**Dernière mise à jour** : 27 janvier 2025  
**Version** : 1.0.0  
**Auteur** : Équipe KelasiNaBiso

