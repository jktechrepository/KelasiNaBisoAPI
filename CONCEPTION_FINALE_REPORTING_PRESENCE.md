# 📊 SYSTÈME DE REPORTING PRÉSENCE - Conception Finale avec Périodicité

## 📅 Date de conception finale
**27 octobre 2025**

---

## 🎯 Principes fondamentaux

### 1. Périodicité obligatoire
**TOUS les endpoints de reporting doivent supporter :**
- ✅ **Date unique** : `?date=2025-01-15` (présences d'un jour spécifique)
- ✅ **Intervalle de dates** : `?dateDebut=2025-01-01&dateFin=2025-01-31`
- ✅ **Périodes prédéfinies** : `?periode=aujourd'hui|semaine|mois|trimestre|annee`

### 2. Groupement hiérarchique
École → Direction → Section → Option → Classe → Élève

### 3. Calculs automatiques
Tous les endpoints calculent automatiquement :
- Taux de présence, absence, retard, ponctualité
- Statistiques retards complètes
- Comparaisons vs moyennes

---

## 📘 PARTIE 1 : REPORTING ÉLÈVES (avec périodicité)

### 1.1 📊 Présence individuelle élève

```http
GET /api/Reporting/Presence/Eleves/{idEleve}
    ?date=2025-01-15                              // OU
    ?dateDebut=2025-01-01&dateFin=2025-01-31     // OU
    ?periode=semaine|mois|trimestre|annee         // OU
    ?anneeScolaire=2024-2025
```

**Paramètres :**
- `date` : Date unique (présence d'un jour)
- `dateDebut` + `dateFin` : Intervalle personnalisé
- `periode` : Période prédéfinie (semaine en cours, mois en cours, etc.)
- `anneeScolaire` : Année scolaire complète

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
    "type": "intervalle",               // ou "jour", "semaine", "mois", "annee"
    "dateDebut": "2025-01-01",
    "dateFin": "2025-01-31",
    "joursOuvrables": 22,
    "libelle": "Janvier 2025"
  },
  "donneesBrutes": {
    "presences": 20,
    "absences": 2,
    "retards": 3,
    "presencesPonctuelles": 17
  },
  "pourcentages": {
    "tauxPresence": 90.91,               // (20 / 22) × 100
    "tauxAbsence": 9.09,                 // (2 / 22) × 100
    "tauxRetard": 13.64,                 // (3 / 22) × 100
    "tauxPonctualite": 77.27             // (17 / 22) × 100
  },
  "statistiquesRetards": {
    "nombreRetards": 3,
    "dureeMoyenne": "12 minutes",
    "dureeMax": "30 minutes",
    "joursRetard": ["2025-01-08", "2025-01-15", "2025-01-22"]
  },
  "detailsPresences": [
    {
      "date": "2025-01-15",
      "heureArrivee": "07:30",
      "heureDepart": "15:00",
      "isPresent": true,
      "enRetard": false,
      "observation": "Présent"
    }
  ]
}
```

---

### 1.2 📊 Groupement par Classe

```http
GET /api/Reporting/Presence/Eleves/Classe/{idClasse}
    ?date=2025-01-15                              // Jour spécifique OU
    ?dateDebut=2025-01-01&dateFin=2025-01-31     // Intervalle OU
    ?periode=semaine|mois|trimestre|annee         // Période prédéfinie
    &includeDetails=true|false
    &includeAbsents=true|false
    &includeRetards=true|false
```

**Exemple 1 : Jour spécifique**
```json
// GET /api/Reporting/Presence/Eleves/Classe/3?date=2025-01-15
{
  "classe": {
    "idClasse": 3,
    "nomClasse": "6ème A",
    "effectifTotal": 35
  },
  "periode": {
    "type": "jour",
    "date": "2025-01-15",
    "jourSemaine": "Mercredi"
  },
  "statistiques": {
    "elevesAttendus": 35,
    "presents": 32,
    "absents": 3,
    "retards": 4,
    "tauxPresence": 91.43,               // (32 / 35) × 100
    "tauxAbsence": 8.57,
    "tauxRetard": 11.43                  // (4 / 35) × 100
  },
  "statistiquesRetards": {
    "nombreRetards": 4,
    "dureeMoyenne": "15 minutes",
    "elevesPlusEnRetard": "Marie Kabongo (30 min)"
  },
  "details": [
    {
      "eleve": {
        "idEleve": 5,
        "nomComplet": "Jean Mukendi",
        "matricule": "ESK25-A3F2B1"
      },
      "isPresent": true,
      "heureArrivee": "07:30",
      "enRetard": false
    },
    {
      "eleve": {
        "idEleve": 8,
        "nomComplet": "Marie Kabongo",
        "matricule": "ESK25-B4C5D6"
      },
      "isPresent": true,
      "heureArrivee": "08:30",
      "enRetard": true,
      "dureeRetard": "30 minutes"
    },
    {
      "eleve": {
        "idEleve": 10,
        "nomComplet": "Paul Tshisekedi",
        "matricule": "ESK25-C5D6E7"
      },
      "isPresent": false,
      "observation": "Absent non justifié"
    }
  ]
}
```

**Exemple 2 : Intervalle mensuel**
```json
// GET /api/Reporting/Presence/Eleves/Classe/3?dateDebut=2025-01-01&dateFin=2025-01-31
{
  "classe": {
    "idClasse": 3,
    "nomClasse": "6ème A",
    "effectifTotal": 35
  },
  "periode": {
    "type": "intervalle",
    "dateDebut": "2025-01-01",
    "dateFin": "2025-01-31",
    "joursOuvrables": 22,
    "libelle": "Janvier 2025"
  },
  "statistiques": {
    "presencesAttendue": 770,            // 35 élèves × 22 jours
    "presencesEffectives": 720,
    "absencesTotales": 50,
    "retardsTotaux": 25,
    "tauxPresence": 93.51,               // (720 / 770) × 100
    "tauxAbsence": 6.49,
    "tauxRetard": 3.25                   // (25 / 770) × 100
  },
  "statistiquesRetards": {
    "nombreRetards": 25,
    "elevesConcernes": 12,
    "pourcentageElevesRetardataires": 34.29,  // (12 / 35) × 100
    "dureeMoyenne": "18 minutes",
    "repartitionParDuree": {
      "0-10min": 10,
      "10-20min": 8,
      "20-30min": 5,
      "30-60min": 2
    },
    "repartitionParJour": {
      "Lundi": 8,
      "Mardi": 4,
      "Mercredi": 3,
      "Jeudi": 5,
      "Vendredi": 5
    }
  },
  "parEleve": [
    {
      "eleve": {
        "idEleve": 5,
        "nomComplet": "Jean Mukendi",
        "matricule": "ESK25-A3F2B1"
      },
      "presences": 22,
      "absences": 0,
      "retards": 2,
      "tauxPresence": 100.0,
      "tauxRetard": 9.09
    }
  ]
}
```

---

### 1.3 📊 Groupement par Option

```http
GET /api/Reporting/Presence/Eleves/Option/{idOption}
    ?date=2025-01-15                              // Jour OU
    ?dateDebut=2025-01-01&dateFin=2025-01-31     // Intervalle OU
    ?periode=semaine|mois|trimestre
    &groupBy=classe
```

**Retour (intervalle) :**
```json
{
  "option": {
    "idOption": 2,
    "nomOption": "Scientifique",
    "section": "Secondaire",
    "nombreClasses": 4,
    "effectifTotal": 140
  },
  "periode": {
    "dateDebut": "2025-01-01",
    "dateFin": "2025-01-31",
    "joursOuvrables": 22,
    "libelle": "Janvier 2025"
  },
  "statistiquesGlobales": {
    "presencesAttendue": 3080,           // 140 × 22
    "presencesEffectives": 2800,
    "absences": 280,
    "retards": 95,
    "tauxPresence": 90.91,
    "tauxAbsence": 9.09,
    "tauxRetard": 3.08
  },
  "parClasse": [
    {
      "classe": "3ème Scientifique A",
      "effectif": 35,
      "presences": 720,
      "absences": 50,
      "retards": 20,
      "tauxPresence": 93.51,
      "tauxRetard": 2.78
    }
  ]
}
```

---

### 1.4 📊 Groupement par Section

```http
GET /api/Reporting/Presence/Eleves/Section/{idSection}
    ?date=2025-01-15                              // Jour OU
    ?dateDebut=2025-01-01&dateFin=2025-01-31     // Intervalle OU
    ?periode=semaine|mois|trimestre|annee
    &groupBy=option|classe
```

---

### 1.5 📊 Groupement par Direction

```http
GET /api/Reporting/Presence/Eleves/Direction/{idDirection}
    ?date=2025-01-15                              // Jour OU
    ?dateDebut=2025-01-01&dateFin=2025-01-31     // Intervalle OU
    ?periode=semaine|mois|trimestre|annee
    &groupBy=section|option|classe
```

---

### 1.6 📊 Groupement par École (Vue globale)

```http
GET /api/Reporting/Presence/Eleves/Ecole/{idEcole}
    ?date=2025-01-15                              // Jour OU
    ?dateDebut=2025-01-01&dateFin=2025-01-31     // Intervalle OU
    ?periode=semaine|mois|trimestre|annee|anneeScolaire
    &groupBy=direction|section|option|classe
```

**Exemple avec période prédéfinie :**
```json
// GET /api/Reporting/Presence/Eleves/Ecole/5?periode=mois
// (mois en cours automatiquement)
{
  "ecole": {
    "idEcole": 5,
    "nomEcole": "École Primaire Kasai",
    "effectifTotal": 1050
  },
  "periode": {
    "type": "mois",
    "dateDebut": "2025-10-01",
    "dateFin": "2025-10-31",
    "joursOuvrables": 22,
    "libelle": "Octobre 2025"
  },
  "statistiquesGlobales": {
    "presencesAttendue": 23100,
    "presencesEffectives": 21000,
    "absences": 2100,
    "retards": 650,
    "tauxPresence": 90.91,
    "tauxAbsence": 9.09,
    "tauxRetard": 2.81
  }
}
```

---

## 📗 PARTIE 2 : REPORTING AGENTS (avec périodicité)

### 2.1 📊 Présence individuelle agent

```http
GET /api/Reporting/Presence/Agents/{idAgent}
    ?date=2025-01-15                              // Jour OU
    ?dateDebut=2025-01-01&dateFin=2025-01-31     // Intervalle OU
    ?periode=semaine|mois|trimestre|annee
```

**Retour (jour spécifique) :**
```json
// GET /api/Reporting/Presence/Agents/12?date=2025-01-15
{
  "agent": {
    "idAgent": 12,
    "nomComplet": "Jean Mukendi Kalala",
    "matricule": "NAT25-C9D3E7",
    "fonction": "Manager Général"
  },
  "periode": {
    "type": "jour",
    "date": "2025-01-15",
    "jourSemaine": "Mercredi"
  },
  "presence": {
    "isPresent": true,
    "heureArrivee": "07:30",
    "heureDepart": "16:00",
    "dureePresence": "8h30",
    "enRetard": false,
    "observation": "Présent"
  }
}
```

**Retour (intervalle mensuel) :**
```json
// GET /api/Reporting/Presence/Agents/12?dateDebut=2025-01-01&dateFin=2025-01-31
{
  "agent": {
    "idAgent": 12,
    "nomComplet": "Jean Mukendi Kalala",
    "matricule": "NAT25-C9D3E7",
    "fonction": "Manager Général"
  },
  "periode": {
    "type": "intervalle",
    "dateDebut": "2025-01-01",
    "dateFin": "2025-01-31",
    "joursOuvrables": 22,
    "libelle": "Janvier 2025"
  },
  "donneesBrutes": {
    "presences": 21,
    "absences": 1,
    "retards": 0,
    "presencesPonctuelles": 21
  },
  "pourcentages": {
    "tauxPresence": 95.45,
    "tauxAbsence": 4.55,
    "tauxRetard": 0.0,
    "tauxPonctualite": 95.45
  },
  "heures": {
    "totalHeuresTravaillees": 178.5,
    "heureMoyenneArrivee": "07:30",
    "heureMoyenneDepart": "16:00",
    "dureeMoyenneJournaliere": "8h30"
  },
  "detailsPresences": [
    {
      "date": "2025-01-15",
      "heureArrivee": "07:30",
      "heureDepart": "16:00",
      "duree": "8h30",
      "enRetard": false
    }
  ]
}
```

---

### 2.2 📊 Groupement par Fonction

```http
GET /api/Reporting/Presence/Agents/Fonction/{fonction}
    ?idEcole=5
    &date=2025-01-15                              // Jour OU
    &dateDebut=2025-01-01&dateFin=2025-01-31     // Intervalle OU
    &periode=semaine|mois|trimestre
```

**Exemple jour spécifique :**
```json
// GET /api/Reporting/Presence/Agents/Fonction/Professeur?idEcole=5&date=2025-01-15
{
  "fonction": "Professeur",
  "ecole": "École Primaire Kasai",
  "periode": {
    "type": "jour",
    "date": "2025-01-15",
    "jourSemaine": "Mercredi"
  },
  "statistiques": {
    "effectifTotal": 25,
    "presents": 23,
    "absents": 2,
    "retards": 3,
    "tauxPresence": 92.0,
    "tauxRetard": 12.0                   // (3 / 25) × 100
  },
  "detailsAgents": [
    {
      "agent": {
        "nomComplet": "Marie Kabongo",
        "matricule": "NAT25-D7E2F9"
      },
      "isPresent": true,
      "heureArrivee": "07:45",
      "enRetard": true,
      "dureeRetard": "15 minutes"
    }
  ]
}
```

**Exemple intervalle mensuel :**
```json
// GET /api/Reporting/Presence/Agents/Fonction/Professeur?idEcole=5&dateDebut=2025-01-01&dateFin=2025-01-31
{
  "fonction": "Professeur",
  "ecole": "École Primaire Kasai",
  "periode": {
    "type": "intervalle",
    "dateDebut": "2025-01-01",
    "dateFin": "2025-01-31",
    "joursOuvrables": 22
  },
  "statistiquesGlobales": {
    "effectif": 25,
    "presencesAttendue": 550,            // 25 × 22
    "presencesEffectives": 520,
    "absences": 30,
    "retards": 15,
    "tauxPresence": 94.55,
    "tauxAbsence": 5.45,
    "tauxRetard": 2.73,
    "totalHeuresTravaillees": 4160.0
  },
  "statistiquesRetards": {
    "nombreRetards": 15,
    "agentsConcernes": 8,
    "pourcentageAgentsRetardataires": 32.0,
    "dureeMoyenne": "12 minutes",
    "jourAvecPlusRetards": "Lundi"
  },
  "parAgent": [
    {
      "agent": {
        "nomComplet": "Marie Kabongo",
        "matricule": "NAT25-D7E2F9"
      },
      "presences": 21,
      "absences": 1,
      "retards": 2,
      "tauxPresence": 95.45,
      "tauxRetard": 9.09
    }
  ]
}
```

---

### 2.3 📊 Groupement par École (Tous agents)

```http
GET /api/Reporting/Presence/Agents/Ecole/{idEcole}
    ?date=2025-01-15                              // Jour OU
    ?dateDebut=2025-01-01&dateFin=2025-01-31     // Intervalle OU
    ?periode=semaine|mois|trimestre|annee
    &groupBy=fonction
```

---

## 📙 PARTIE 3 : STATISTIQUES RETARDS (avec périodicité)

### 3.1 📉 Analyse retards élèves

```http
GET /api/Reporting/Presence/Eleves/Retards/Analyse
    ?idEcole=5
    &date=2025-01-15                              // Jour OU
    &dateDebut=2025-01-01&dateFin=2025-01-31     // Intervalle OU
    &periode=semaine|mois|trimestre
    &heureReference=08:00
    &groupBy=classe|option|section
```

**Retour (intervalle) :**
```json
{
  "ecole": "École Primaire Kasai",
  "periode": {
    "dateDebut": "2025-01-01",
    "dateFin": "2025-01-31",
    "joursOuvrables": 22
  },
  "heureReference": "08:00",
  
  "statistiquesGlobales": {
    "nombreTotalRetards": 650,
    "elevesRetardataires": 285,
    "pourcentageElevesRetardataires": 27.14,
    "tauxRetardGlobal": 2.81,            // % retards sur total présences
    "dureeMoyenne": "18 minutes",
    "dureeMediane": "12 minutes",
    "dureeMin": "1 minute",
    "dureeMax": "60 minutes"
  },
  
  "repartitionParDuree": {
    "0-10min": { "nombre": 250, "pourcentage": 38.46 },
    "10-20min": { "nombre": 280, "pourcentage": 43.08 },
    "20-30min": { "nombre": 90, "pourcentage": 13.85 },
    "30-60min": { "nombre": 30, "pourcentage": 4.62 }
  },
  
  "repartitionParJourSemaine": {
    "Lundi": { "retards": 150, "pourcentage": 23.08, "dureeMoyenne": "20 min" },
    "Mardi": { "retards": 110, "pourcentage": 16.92, "dureeMoyenne": "15 min" },
    "Mercredi": { "retards": 100, "pourcentage": 15.38, "dureeMoyenne": "12 min" },
    "Jeudi": { "retards": 120, "pourcentage": 18.46, "dureeMoyenne": "18 min" },
    "Vendredi": { "retards": 170, "pourcentage": 26.15, "dureeMoyenne": "22 min" }
  },
  
  "topRetardataires": [
    {
      "rang": 1,
      "eleve": {
        "nomComplet": "Paul Tshisekedi",
        "matricule": "ESK25-C5D6E7",
        "classe": "5ème B"
      },
      "nombreRetards": 12,
      "pourcentageRetards": 54.55,       // % de ses présences
      "dureeMoyenne": "25 minutes",
      "dureeMax": "45 minutes",
      "jourSemaineFrequent": "Lundi"
    }
  ],
  
  "parClasse": [
    {
      "classe": "5ème B",
      "retards": 85,
      "tauxRetard": 11.2
    }
  ]
}
```

---

### 3.2 📉 Retards élève individuel

```http
GET /api/Reporting/Presence/Eleves/{idEleve}/Retards
    ?dateDebut=2025-01-01&dateFin=2025-01-31     // Intervalle OU
    ?periode=semaine|mois|trimestre
    &heureReference=08:00
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
    "joursOuvrables": 22
  },
  "heureReference": "08:00",
  
  "statistiques": {
    "nombreRetards": 3,
    "nombrePresences": 20,
    "pourcentageRetards": 15.0,          // (3 / 20) × 100
    "dureeMoyenne": "12 minutes",
    "dureeMediane": "10 minutes",
    "dureeMin": "5 minutes",
    "dureeMax": "30 minutes"
  },
  
  "repartitionParJour": {
    "Lundi": 2,
    "Mercredi": 1
  },
  
  "listeRetards": [
    {
      "date": "2025-01-08",
      "jourSemaine": "Lundi",
      "heureReference": "08:00",
      "heureArrivee": "08:05",
      "dureeRetard": "5 minutes",
      "observation": "Retard mineur"
    },
    {
      "date": "2025-01-15",
      "jourSemaine": "Lundi",
      "heureReference": "08:00",
      "heureArrivee": "08:30",
      "dureeRetard": "30 minutes",
      "observation": "Retard important - Transport"
    },
    {
      "date": "2025-01-22",
      "jourSemaine": "Mercredi",
      "heureReference": "08:00",
      "heureArrivee": "08:10",
      "dureeRetard": "10 minutes",
      "observation": null
    }
  ],
  
  "comparaison": {
    "moyenneClasse": {
      "tauxRetard": 3.2,
      "dureeMoyenne": "10 minutes"
    },
    "position": "Au-dessus de la moyenne (15% vs 3.2%)",
    "appreciation": "À améliorer"
  }
}
```

---

### 3.3 📉 Retards agents

```http
GET /api/Reporting/Presence/Agents/Retards/Analyse
    ?idEcole=5
    &dateDebut=2025-01-01&dateFin=2025-01-31     // Intervalle OU
    &periode=semaine|mois|trimestre
    &heureReference=07:30
    &groupBy=fonction
```

---

## 📕 PARTIE 4 : DASHBOARDS (avec périodicité)

### 4.1 Dashboard global école

```http
GET /api/Reporting/Presence/Dashboard/Ecole/{idEcole}
    ?date=2025-01-15                              // Jour OU
    ?dateDebut=2025-01-01&dateFin=2025-01-31     // Intervalle
```

**Exemple jour spécifique :**
```json
// GET /api/Reporting/Presence/Dashboard/Ecole/5?date=2025-01-15
{
  "ecole": "École Primaire Kasai",
  "periode": {
    "type": "jour",
    "date": "2025-01-15",
    "jourSemaine": "Mercredi"
  },
  
  "resumeEleves": {
    "effectifTotal": 1050,
    "presents": 950,
    "absents": 100,
    "retards": 35,
    "tauxPresence": 90.48,
    "tauxAbsence": 9.52,
    "tauxRetard": 3.33
  },
  
  "resumeAgents": {
    "effectifTotal": 30,
    "presents": 28,
    "absents": 2,
    "retards": 1,
    "tauxPresence": 93.33,
    "tauxAbsence": 6.67,
    "tauxRetard": 3.33
  },
  
  "alertes": [
    {
      "type": "danger",
      "message": "Classe 5ème B : 12 absents sur 35 (65.7% présence)"
    }
  ],
  
  "classesProblematiques": [
    {
      "classe": "5ème B",
      "presents": 23,
      "absents": 12,
      "tauxPresence": 65.71
    }
  ]
}
```

**Exemple intervalle mensuel :**
```json
// GET /api/Reporting/Presence/Dashboard/Ecole/5?dateDebut=2025-01-01&dateFin=2025-01-31
{
  "ecole": "École Primaire Kasai",
  "periode": {
    "type": "intervalle",
    "dateDebut": "2025-01-01",
    "dateFin": "2025-01-31",
    "joursOuvrables": 22,
    "libelle": "Janvier 2025"
  },
  
  "resumeEleves": {
    "effectifTotal": 1050,
    "presencesAttendue": 23100,
    "presencesEffectives": 21000,
    "absences": 2100,
    "retards": 650,
    "tauxPresence": 90.91,
    "tauxRetard": 2.81
  },
  
  "resumeAgents": {
    "effectifTotal": 30,
    "presencesAttendue": 660,
    "presencesEffectives": 620,
    "absences": 40,
    "retards": 20,
    "tauxPresence": 93.94,
    "tauxRetard": 3.03
  },
  
  "tendances": {
    "evolutionTauxPresenceEleves": {
      "moisPrecedent": 88.5,
      "moisActuel": 90.91,
      "evolution": "+2.41%"
    },
    "evolutionTauxPresenceAgents": {
      "moisPrecedent": 92.0,
      "moisActuel": 93.94,
      "evolution": "+1.94%"
    }
  },
  
  "classementClasses": [
    {
      "rang": 1,
      "classe": "6ème A",
      "tauxPresence": 93.51
    },
    {
      "rang": 2,
      "classe": "4ème C",
      "tauxPresence": 92.8
    }
  ],
  
  "classesProblematiques": [
    {
      "classe": "5ème B",
      "tauxPresence": 75.3,
      "status": "attention"
    }
  ]
}
```

---

## 📊 PARTIE 5 : PÉRIODES PRÉDÉFINIES

### Gestion automatique des périodes

```csharp
public class PeriodeHelper
{
    public static (DateTime dateDebut, DateTime dateFin) GetPeriode(string periode)
    {
        var aujourd'hui = DateTime.Now.Date;
        
        return periode.ToLower() switch
        {
            "aujourd'hui" => (aujourd'hui, aujourd'hui),
            
            "semaine" => (
                aujourd'hui.AddDays(-(int)aujourd'hui.DayOfWeek + 1), // Lundi
                aujourd'hui.AddDays(7 - (int)aujourd'hui.DayOfWeek)   // Dimanche
            ),
            
            "mois" => (
                new DateTime(aujourd'hui.Year, aujourd'hui.Month, 1),
                new DateTime(aujourd'hui.Year, aujourd'hui.Month, DateTime.DaysInMonth(aujourd'hui.Year, aujourd'hui.Month))
            ),
            
            "trimestre" => GetTrimestre(aujourd'hui),
            
            "annee" => (
                new DateTime(aujourd'hui.Year, 1, 1),
                new DateTime(aujourd'hui.Year, 12, 31)
            ),
            
            "anneescolaire" => GetAnneeScolaire(aujourd'hui),
            
            _ => throw new ArgumentException("Période invalide")
        };
    }
}
```

---

## 📋 RÉCAPITULATIF COMPLET DES ENDPOINTS

### ÉLÈVES (avec périodicité complète)

| # | Endpoint | Périodicité | Groupement |
|---|----------|-------------|------------|
| 1 | `GET /Eleves/{id}` | ✅ date, intervalle, période | Individuel |
| 2 | `GET /Eleves/{id}/Pourcentage` | ✅ intervalle, période | Individuel |
| 3 | `GET /Eleves/{id}/Retards` | ✅ intervalle, période | Individuel |
| 4 | `GET /Eleves/Classe/{id}` | ✅ date, intervalle, période | Classe |
| 5 | `GET /Eleves/Option/{id}` | ✅ date, intervalle, période | Option |
| 6 | `GET /Eleves/Section/{id}` | ✅ date, intervalle, période | Section |
| 7 | `GET /Eleves/Direction/{id}` | ✅ date, intervalle, période | Direction |
| 8 | `GET /Eleves/Ecole/{id}` | ✅ date, intervalle, période | École |
| 9 | `GET /Eleves/Retards/Analyse` | ✅ date, intervalle, période | Global |

### AGENTS (avec périodicité complète)

| # | Endpoint | Périodicité | Groupement |
|---|----------|-------------|------------|
| 10 | `GET /Agents/{id}` | ✅ date, intervalle, période | Individuel |
| 11 | `GET /Agents/{id}/Pourcentage` | ✅ intervalle, période | Individuel |
| 12 | `GET /Agents/{id}/Retards` | ✅ intervalle, période | Individuel |
| 13 | `GET /Agents/Fonction/{fonction}` | ✅ date, intervalle, période | Fonction |
| 14 | `GET /Agents/Fonctions/Comparatif` | ✅ intervalle, période | Toutes fonctions |
| 15 | `GET /Agents/Ecole/{id}` | ✅ date, intervalle, période | École |
| 16 | `GET /Agents/Retards/Analyse` | ✅ date, intervalle, période | Global |

### DASHBOARDS (avec périodicité)

| # | Endpoint | Périodicité |
|---|----------|-------------|
| 17 | `GET /Dashboard/Ecole/{id}` | ✅ date, intervalle |
| 18 | `GET /Hierarchie/{id}` | ✅ intervalle, période |

---

## ✅ CHECKLIST COMPLÈTE

### Périodicité
- [x] Jour spécifique (`?date=2025-01-15`)
- [x] Intervalle personnalisé (`?dateDebut=...&dateFin=...`)
- [x] Périodes prédéfinies (`?periode=semaine|mois|trimestre|annee`)
- [x] Année scolaire (`?anneeScolaire=2024-2025`)

### Groupement élèves
- [x] Individuel
- [x] Par Classe
- [x] Par Option
- [x] Par Section
- [x] Par Direction
- [x] Par École

### Groupement agents
- [x] Individuel
- [x] Par Fonction
- [x] Par École

### Pourcentages
- [x] Taux de présence
- [x] Taux d'absence
- [x] Taux de retard
- [x] Taux de ponctualité

### Statistiques retards
- [x] Nombre total
- [x] Durée moyenne/médiane/min/max
- [x] Répartition par durée
- [x] Répartition par jour semaine
- [x] Top retardataires
- [x] % de retardataires

---

**Voulez-vous que je commence l'implémentation maintenant ?** 

Tous les endpoints sont maintenant conçus avec une **périodicité complète** ! 🚀

