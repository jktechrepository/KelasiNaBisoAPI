# 📊 ENDPOINTS REPORTING PRÉSENCE - Version Finale Simplifiée

## 📅 Date de conception finale
**27 octobre 2025**

---

## 🎯 Principe de conception

**Utiliser uniquement le `PresenceController` existant** avec des routes simples et cohérentes :
- ✅ `/api/Presence/eleves/...` pour les reportings élèves
- ✅ `/api/Presence/agents/...` pour les reportings agents
- ✅ Pas de `/api/Reporting/...` (éviter la redondance)
- ✅ Cohérence avec le reste de l'API

---

## 📘 PARTIE 1 : ENDPOINTS ÉLÈVES

### 1.1 Présence individuelle élève

```http
GET /api/Presence/eleves/{idEleve}
    ?date=2025-01-15                              // OU
    ?dateDebut=2025-01-01&dateFin=2025-01-31     // OU
    ?periode=semaine|mois|trimestre|annee
```

---

### 1.2 Pourcentage individuel élève

```http
GET /api/Presence/eleves/{idEleve}/pourcentage
    ?dateDebut=2025-01-01&dateFin=2025-01-31
    ?periode=mois|trimestre|annee
```

**Retour :**
```json
{
  "eleve": {
    "idEleve": 5,
    "nomComplet": "Jean Mukendi",
    "matricule": "ESK25-A3F2B1",
    "classe": "6ème A"
  },
  "periode": {
    "dateDebut": "2025-01-01",
    "dateFin": "2025-01-31",
    "joursOuvrables": 22,
    "libelle": "Janvier 2025"
  },
  "donnees": {
    "presences": 20,
    "absences": 2,
    "retards": 3
  },
  "pourcentages": {
    "tauxPresence": 90.91,
    "tauxAbsence": 9.09,
    "tauxRetard": 13.64,
    "tauxPonctualite": 77.27
  }
}
```

---

### 1.3 Retards élève individuel

```http
GET /api/Presence/eleves/{idEleve}/retards
    ?dateDebut=2025-01-01&dateFin=2025-01-31
    ?periode=mois|trimestre
    &heureReference=08:00
```

---

### 1.4 Groupement par Classe

```http
GET /api/Presence/eleves/classe/{idClasse}
    ?date=2025-01-15                              // OU
    ?dateDebut=2025-01-01&dateFin=2025-01-31     // OU
    ?periode=semaine|mois|trimestre
    &includeDetails=true|false
```

**Retour (jour) :**
```json
{
  "classe": {
    "idClasse": 3,
    "nomClasse": "6ème A",
    "effectifTotal": 35
  },
  "periode": {
    "type": "jour",
    "date": "2025-01-15"
  },
  "statistiques": {
    "presents": 32,
    "absents": 3,
    "retards": 4,
    "tauxPresence": 91.43,
    "tauxRetard": 11.43
  }
}
```

**Retour (intervalle) :**
```json
{
  "classe": {
    "idClasse": 3,
    "nomClasse": "6ème A",
    "effectifTotal": 35
  },
  "periode": {
    "dateDebut": "2025-01-01",
    "dateFin": "2025-01-31",
    "joursOuvrables": 22
  },
  "statistiques": {
    "presencesAttendue": 770,
    "presencesEffectives": 720,
    "absences": 50,
    "retards": 25,
    "tauxPresence": 93.51,
    "tauxAbsence": 6.49,
    "tauxRetard": 3.25
  },
  "statistiquesRetards": {
    "nombreRetards": 25,
    "dureeMoyenne": "15 min",
    "elevesConcernes": 12
  },
  "parEleve": [
    {
      "eleve": { "idEleve": 5, "nomComplet": "Jean Mukendi" },
      "presences": 22,
      "absences": 0,
      "tauxPresence": 100.0
    }
  ]
}
```

---

### 1.5 Groupement par Option

```http
GET /api/Presence/eleves/option/{idOption}
    ?date=2025-01-15                              // OU
    ?dateDebut=2025-01-01&dateFin=2025-01-31     // OU
    ?periode=mois|trimestre
    &groupBy=classe
```

---

### 1.6 Groupement par Section

```http
GET /api/Presence/eleves/section/{idSection}
    ?date=2025-01-15                              // OU
    ?dateDebut=2025-01-01&dateFin=2025-01-31     // OU
    ?periode=mois|trimestre
    &groupBy=option|classe
```

---

### 1.7 Groupement par Direction

```http
GET /api/Presence/eleves/direction/{idDirection}
    ?date=2025-01-15                              // OU
    ?dateDebut=2025-01-01&dateFin=2025-01-31     // OU
    ?periode=mois|trimestre
    &groupBy=section|option|classe
```

---

### 1.8 Groupement par École

```http
GET /api/Presence/eleves/ecole/{idEcole}
    ?date=2025-01-15                              // OU
    ?dateDebut=2025-01-01&dateFin=2025-01-31     // OU
    ?periode=mois|trimestre|annee
    &groupBy=direction|section|option|classe
```

---

### 1.9 Analyse retards (global élèves)

```http
GET /api/Presence/eleves/retards/analyse
    ?idEcole=5
    &dateDebut=2025-01-01&dateFin=2025-01-31
    ?periode=mois|trimestre
    &heureReference=08:00
    &groupBy=classe|option|section
```

---

## 📗 PARTIE 2 : ENDPOINTS AGENTS

### 2.1 Présence individuelle agent

```http
GET /api/Presence/agents/{idAgent}
    ?date=2025-01-15                              // OU
    ?dateDebut=2025-01-01&dateFin=2025-01-31     // OU
    ?periode=semaine|mois|trimestre
```

---

### 2.2 Pourcentage individuel agent

```http
GET /api/Presence/agents/{idAgent}/pourcentage
    ?dateDebut=2025-01-01&dateFin=2025-01-31
    ?periode=mois|trimestre
```

---

### 2.3 Retards agent individuel

```http
GET /api/Presence/agents/{idAgent}/retards
    ?dateDebut=2025-01-01&dateFin=2025-01-31
    ?periode=mois|trimestre
    &heureReference=07:30
```

---

### 2.4 Groupement par Fonction

```http
GET /api/Presence/agents/fonction/{fonction}
    ?idEcole=5
    &date=2025-01-15                              // OU
    &dateDebut=2025-01-01&dateFin=2025-01-31     // OU
    &periode=mois|trimestre
```

**Exemple :**
```http
GET /api/Presence/agents/fonction/Professeur?idEcole=5&dateDebut=2025-01-01&dateFin=2025-01-31
```

---

### 2.5 Comparatif toutes fonctions

```http
GET /api/Presence/agents/fonctions
    ?idEcole=5
    &dateDebut=2025-01-01&dateFin=2025-01-31
    ?periode=mois|trimestre
```

**Retour :**
```json
{
  "ecole": "École Primaire Kasai",
  "periode": "Janvier 2025",
  "parFonction": [
    {
      "fonction": "Manager Général",
      "effectif": 1,
      "tauxPresence": 100.0,
      "tauxRetard": 0.0
    },
    {
      "fonction": "Professeur",
      "effectif": 25,
      "tauxPresence": 94.55,
      "tauxRetard": 2.73
    }
  ]
}
```

---

### 2.6 Groupement par École (tous agents)

```http
GET /api/Presence/agents/ecole/{idEcole}
    ?date=2025-01-15                              // OU
    ?dateDebut=2025-01-01&dateFin=2025-01-31     // OU
    ?periode=mois|trimestre
    &groupBy=fonction
```

---

### 2.7 Analyse retards (global agents)

```http
GET /api/Presence/agents/retards/analyse
    ?idEcole=5
    &dateDebut=2025-01-01&dateFin=2025-01-31
    ?periode=mois|trimestre
    &heureReference=07:30
    &groupBy=fonction
```

---

## 📙 PARTIE 3 : DASHBOARDS ET VUES GLOBALES

### 3.1 Dashboard école (jour)

```http
GET /api/Presence/dashboard/ecole/{idEcole}
    ?date=2025-01-15                              // Jour spécifique
```

**Retour :**
```json
{
  "ecole": "École Primaire Kasai",
  "date": "2025-01-15",
  "resumeEleves": {
    "effectif": 1050,
    "presents": 950,
    "absents": 100,
    "retards": 35,
    "tauxPresence": 90.48
  },
  "resumeAgents": {
    "effectif": 30,
    "presents": 28,
    "absents": 2,
    "retards": 1,
    "tauxPresence": 93.33
  },
  "alertes": [
    {
      "type": "danger",
      "message": "Classe 5ème B : 65% présence"
    }
  ]
}
```

---

### 3.2 Dashboard école (période)

```http
GET /api/Presence/dashboard/ecole/{idEcole}
    ?dateDebut=2025-01-01&dateFin=2025-01-31     // Intervalle
```

---

### 3.3 Hiérarchie complète

```http
GET /api/Presence/hierarchie/{idEcole}
    ?dateDebut=2025-01-01&dateFin=2025-01-31
    ?periode=mois|trimestre
    &type=ELEVE|AGENT|TOUS
```

**Retour :**
```json
{
  "ecole": {
    "idEcole": 5,
    "nom": "École Primaire Kasai",
    "tauxPresence": 90.91
  },
  "directions": [
    {
      "idDirection": 1,
      "nom": "Direction Primaire",
      "tauxPresence": 88.5,
      "sections": [
        {
          "idSection": 1,
          "nom": "Primaire",
          "tauxPresence": 88.5,
          "options": [
            {
              "idOption": 1,
              "nom": "Générale",
              "tauxPresence": 88.5,
              "classes": [
                {
                  "idClasse": 3,
                  "nom": "6ème A",
                  "effectif": 35,
                  "tauxPresence": 93.51
                }
              ]
            }
          ]
        }
      ]
    }
  ]
}
```

---

## 📋 LISTE COMPLÈTE DES ENDPOINTS (Structure simplifiée)

### 📘 GROUPE ÉLÈVES (Base : `/api/Presence/eleves`)

| # | Endpoint | Périodicité | Description |
|---|----------|-------------|-------------|
| 1 | `GET /api/Presence/eleves/{id}` | ✅ date, intervalle, période | Historique individuel |
| 2 | `GET /api/Presence/eleves/{id}/pourcentage` | ✅ intervalle, période | Pourcentages élève |
| 3 | `GET /api/Presence/eleves/{id}/retards` | ✅ intervalle, période | Retards élève |
| 4 | `GET /api/Presence/eleves/classe/{id}` | ✅ date, intervalle, période | Groupement classe |
| 5 | `GET /api/Presence/eleves/option/{id}` | ✅ date, intervalle, période | Groupement option |
| 6 | `GET /api/Presence/eleves/section/{id}` | ✅ date, intervalle, période | Groupement section |
| 7 | `GET /api/Presence/eleves/direction/{id}` | ✅ date, intervalle, période | Groupement direction |
| 8 | `GET /api/Presence/eleves/ecole/{id}` | ✅ date, intervalle, période | Groupement école |
| 9 | `GET /api/Presence/eleves/retards/analyse` | ✅ intervalle, période | Analyse retards globale |

### 📗 GROUPE AGENTS (Base : `/api/Presence/agents`)

| # | Endpoint | Périodicité | Description |
|---|----------|-------------|-------------|
| 10 | `GET /api/Presence/agents/{id}` | ✅ date, intervalle, période | Historique individuel |
| 11 | `GET /api/Presence/agents/{id}/pourcentage` | ✅ intervalle, période | Pourcentages agent |
| 12 | `GET /api/Presence/agents/{id}/retards` | ✅ intervalle, période | Retards agent |
| 13 | `GET /api/Presence/agents/fonction/{fonction}` | ✅ date, intervalle, période | Groupement fonction |
| 14 | `GET /api/Presence/agents/fonctions` | ✅ intervalle, période | Comparatif fonctions |
| 15 | `GET /api/Presence/agents/ecole/{id}` | ✅ date, intervalle, période | Groupement école |
| 16 | `GET /api/Presence/agents/retards/analyse` | ✅ intervalle, période | Analyse retards globale |

### 📙 GROUPE DASHBOARDS (Base : `/api/Presence`)

| # | Endpoint | Périodicité | Description |
|---|----------|-------------|-------------|
| 17 | `GET /api/Presence/dashboard/ecole/{id}` | ✅ date, intervalle | Dashboard école |
| 18 | `GET /api/Presence/hierarchie/{id}` | ✅ intervalle, période | Structure hiérarchique |

---

## 🎯 Cohérence avec l'existant

### Endpoints existants (à conserver)
```http
GET /api/Presence                          # Liste toutes les présences
GET /api/Presence/{id}                     # Une présence spécifique
GET /api/Presence/eleve/{idEleve}          # Déjà existant - historique élève
GET /api/Presence/agent/{idAgent}          # Déjà existant - historique agent
GET /api/Presence/date/{date}              # Présences d'un jour
GET /api/Presence/type/{type}              # ELEVE ou AGENT
GET /api/Presence/type/{type}/date/{date}  # Type + Date
POST /api/Presence                         # Créer présence
PUT /api/Presence/{id}                     # Modifier présence
DELETE /api/Presence/{id}                  # Supprimer présence
PUT /api/Presence/toggle-statut/{id}       # Toggle statut
```

### Nouveaux endpoints (à ajouter)
```http
# ÉLÈVES - Reporting avancé
GET /api/Presence/eleves/{id}/pourcentage
GET /api/Presence/eleves/{id}/retards
GET /api/Presence/eleves/classe/{id}
GET /api/Presence/eleves/option/{id}
GET /api/Presence/eleves/section/{id}
GET /api/Presence/eleves/direction/{id}
GET /api/Presence/eleves/ecole/{id}
GET /api/Presence/eleves/retards/analyse

# AGENTS - Reporting avancé
GET /api/Presence/agents/{id}/pourcentage
GET /api/Presence/agents/{id}/retards
GET /api/Presence/agents/fonction/{fonction}
GET /api/Presence/agents/fonctions
GET /api/Presence/agents/ecole/{id}
GET /api/Presence/agents/retards/analyse

# DASHBOARDS
GET /api/Presence/dashboard/ecole/{id}
GET /api/Presence/hierarchie/{id}
```

---

## 🔧 Modification du PresenceController

### Structure recommandée

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PresenceController : ControllerBase
{
    private readonly IPresenceRepository _presenceRepository;
    private readonly IPresenceReportingService _reportingService; // ✨ NOUVEAU
    
    // ... endpoints existants ...
    
    // ═══════════════════════════════════════════════════════
    // SECTION REPORTING ÉLÈVES
    // ═══════════════════════════════════════════════════════
    
    /// <summary>
    /// Obtient le pourcentage de présence d'un élève sur une période
    /// </summary>
    [HttpGet("eleves/{idEleve}/pourcentage")]
    public async Task<ActionResult<PourcentageEleveDto>> GetElevePourcentage(
        int idEleve,
        [FromQuery] DateTime? dateDebut,
        [FromQuery] DateTime? dateFin,
        [FromQuery] string? periode)
    {
        var result = await _reportingService.GetElevePourcentageAsync(
            idEleve, dateDebut, dateFin, periode);
        return Ok(result);
    }
    
    /// <summary>
    /// Obtient l'analyse des retards d'un élève
    /// </summary>
    [HttpGet("eleves/{idEleve}/retards")]
    public async Task<ActionResult<RetardsEleveDto>> GetEleveRetards(
        int idEleve,
        [FromQuery] DateTime? dateDebut,
        [FromQuery] DateTime? dateFin,
        [FromQuery] string? periode,
        [FromQuery] TimeSpan? heureReference)
    {
        var result = await _reportingService.GetEleveRetardsAsync(
            idEleve, dateDebut, dateFin, periode, heureReference ?? TimeSpan.Parse("08:00"));
        return Ok(result);
    }
    
    /// <summary>
    /// Obtient le reporting de présence pour une classe
    /// </summary>
    [HttpGet("eleves/classe/{idClasse}")]
    public async Task<ActionResult<ClassePresenceDto>> GetClassePresences(
        int idClasse,
        [FromQuery] DateTime? date,
        [FromQuery] DateTime? dateDebut,
        [FromQuery] DateTime? dateFin,
        [FromQuery] string? periode,
        [FromQuery] bool includeDetails = false)
    {
        var result = await _reportingService.GetClassePresencesAsync(
            idClasse, date, dateDebut, dateFin, periode, includeDetails);
        return Ok(result);
    }
    
    // ... autres endpoints élèves ...
    
    // ═══════════════════════════════════════════════════════
    // SECTION REPORTING AGENTS
    // ═══════════════════════════════════════════════════════
    
    /// <summary>
    /// Obtient le pourcentage de présence d'un agent sur une période
    /// </summary>
    [HttpGet("agents/{idAgent}/pourcentage")]
    public async Task<ActionResult<PourcentageAgentDto>> GetAgentPourcentage(
        int idAgent,
        [FromQuery] DateTime? dateDebut,
        [FromQuery] DateTime? dateFin,
        [FromQuery] string? periode)
    {
        var result = await _reportingService.GetAgentPourcentageAsync(
            idAgent, dateDebut, dateFin, periode);
        return Ok(result);
    }
    
    // ... autres endpoints agents ...
    
    // ═══════════════════════════════════════════════════════
    // SECTION DASHBOARDS
    // ═══════════════════════════════════════════════════════
    
    /// <summary>
    /// Obtient le dashboard de présence pour une école
    /// </summary>
    [HttpGet("dashboard/ecole/{idEcole}")]
    public async Task<ActionResult<DashboardPresenceDto>> GetDashboardEcole(
        int idEcole,
        [FromQuery] DateTime? date,
        [FromQuery] DateTime? dateDebut,
        [FromQuery] DateTime? dateFin)
    {
        var result = await _reportingService.GetDashboardEcoleAsync(
            idEcole, date, dateDebut, dateFin);
        return Ok(result);
    }
}
```

---

## 🎨 Structure des routes (Vue d'ensemble)

```
/api/Presence
    │
    ├─ /                                # GET all, POST create
    ├─ /{id}                           # GET, PUT, DELETE
    ├─ /toggle-statut/{id}             # PUT
    ├─ /date/{date}                    # GET
    ├─ /type/{type}                    # GET
    │
    ├─ /eleves/
    │   ├─ /{id}                       # GET historique
    │   ├─ /{id}/pourcentage           # GET pourcentages
    │   ├─ /{id}/retards               # GET retards
    │   ├─ /classe/{id}                # GET classe
    │   ├─ /option/{id}                # GET option
    │   ├─ /section/{id}               # GET section
    │   ├─ /direction/{id}             # GET direction
    │   ├─ /ecole/{id}                 # GET école
    │   └─ /retards/analyse            # GET analyse retards
    │
    ├─ /agents/
    │   ├─ /{id}                       # GET historique
    │   ├─ /{id}/pourcentage           # GET pourcentages
    │   ├─ /{id}/retards               # GET retards
    │   ├─ /fonction/{fonction}        # GET fonction
    │   ├─ /fonctions                  # GET toutes fonctions
    │   ├─ /ecole/{id}                 # GET école
    │   └─ /retards/analyse            # GET analyse retards
    │
    ├─ /dashboard/
    │   └─ /ecole/{id}                 # GET dashboard
    │
    └─ /hierarchie/{id}                # GET structure complète
```

---

## ✅ Avantages de cette structure

### 1. Cohérence avec l'existant
```http
# Existant
GET /api/Presence/eleve/{idEleve}

# Nouveau (cohérent)
GET /api/Presence/eleves/{idEleve}/pourcentage
GET /api/Presence/eleves/classe/{idClasse}
```

### 2. Routes simples et claires
```http
# Simple et intuitif
GET /api/Presence/eleves/classe/3?dateDebut=2025-01-01&dateFin=2025-01-31

# Au lieu de (trop verbeux)
GET /api/Reporting/Presence/Eleves/Classe/3?dateDebut=2025-01-01&dateFin=2025-01-31
```

### 3. Un seul contrôleur
- ✅ Tout dans `PresenceController`
- ✅ Pas de `PresenceReportingController` séparé
- ✅ Logique métier dans `PresenceReportingService`
- ✅ Séparation claire : Controller (routes) vs Service (logique)

---

## 📊 Exemples d'utilisation

### Exemple 1 : Présence classe aujourd'hui
```http
GET /api/Presence/eleves/classe/3?date=2025-01-15
```

### Exemple 2 : Pourcentage élève ce mois
```http
GET /api/Presence/eleves/5/pourcentage?periode=mois
```

### Exemple 3 : Retards agents ce trimestre
```http
GET /api/Presence/agents/retards/analyse?idEcole=5&periode=trimestre
```

### Exemple 4 : Dashboard école aujourd'hui
```http
GET /api/Presence/dashboard/ecole/5?date=2025-01-15
```

### Exemple 5 : Fonctions agents janvier
```http
GET /api/Presence/agents/fonctions?idEcole=5&dateDebut=2025-01-01&dateFin=2025-01-31
```

---

## 🔧 Plan d'implémentation

### Étape 1 : Créer les DTOs
```
Models/DTOs/Reporting/
├── PourcentageEleveDto.cs
├── PourcentageAgentDto.cs
├── RetardsEleveDto.cs
├── RetardsAgentDto.cs
├── ClassePresenceDto.cs
├── FonctionPresenceDto.cs
├── DashboardPresenceDto.cs
└── HierarchiePresenceDto.cs
```

### Étape 2 : Créer le service
```
Services/
└── PresenceReportingService.cs
    (Implémente IPresenceReportingService)
```

### Étape 3 : Ajouter les endpoints au contrôleur existant
```
Controllers/
└── PresenceController.cs
    (Ajouter les nouveaux endpoints)
```

### Étape 4 : Enregistrer le service
```
Program.cs
└── builder.Services.AddScoped<IPresenceReportingService, PresenceReportingService>();
```

---

## ✅ CHECKLIST FINALE

- [x] Routes simples : `/api/Presence/eleves/...` et `/api/Presence/agents/...`
- [x] Périodicité sur TOUS les endpoints
- [x] Groupements hiérarchiques complets
- [x] Calculs pourcentages détaillés
- [x] Statistiques retards exhaustives
- [x] Cohérence avec structure existante
- [x] Un seul contrôleur (PresenceController)
- [x] Documentation complète

---

**Voulez-vous que je commence l'implémentation de ces endpoints dans le `PresenceController` existant ?**

Je peux commencer par les endpoints prioritaires comme :
1. Pourcentage élève/agent
2. Groupement par classe
3. Groupement par fonction agent
4. Analyse retards

Qu'en pensez-vous ? 🚀

