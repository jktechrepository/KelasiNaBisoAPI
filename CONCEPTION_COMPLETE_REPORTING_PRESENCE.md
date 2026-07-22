# 📊 SYSTÈME DE REPORTING PRÉSENCE - Conception Complète et Détaillée

## 📅 Date de conception
**27 octobre 2025**

---

## 🎯 Objectifs du système de reporting

1. ✅ **Groupement hiérarchique** : Classe → Option → Section → Direction → École
2. ✅ **Calcul de pourcentages** : Taux de présence, taux d'absence, taux de retard
3. ✅ **Statistiques retards** : Analyse complète des retards (nombre, durée moyenne, fréquence)
4. ✅ **Différenciation élèves/agents** : Reportings adaptés à chaque type
5. ✅ **Période flexible** : Jour, semaine, mois, trimestre, année
6. ✅ **Performance** : Requêtes optimisées avec agrégation BDD

---

## 📘 PARTIE 1 : REPORTING ÉLÈVES

### 1.1 📊 Groupement hiérarchique des présences

#### A) Par Classe
```http
GET /api/Reporting/Presence/Eleves/Classe/{idClasse}
    ?dateDebut=2025-01-01
    &dateFin=2025-01-31
    &includeDetails=true
```

**Retour :**
```json
{
  "classe": {
    "idClasse": 3,
    "nomClasse": "6ème A",
    "section": "Primaire",
    "option": "Générale",
    "direction": "Direction Primaire",
    "ecole": "École Primaire Kasai",
    "effectifTotal": 35
  },
  "periode": {
    "dateDebut": "2025-01-01",
    "dateFin": "2025-01-31",
    "joursOuvrables": 22
  },
  "statistiquesGlobales": {
    "totalPresencesAttendue": 770,      // 35 élèves × 22 jours
    "totalPresencesEffectives": 720,
    "totalAbsences": 50,
    "tauxPresence": 93.51,              // ✅ POURCENTAGE
    "tauxAbsence": 6.49,                // ✅ POURCENTAGE
    "totalRetards": 25,
    "tauxRetard": 3.25                  // ✅ POURCENTAGE RETARD
  },
  "statistiquesRetards": {              // ✅ STATS RETARDS DÉTAILLÉES
    "nombreTotalRetards": 25,
    "elevesConcernes": 12,
    "dureeMoyenneRetard": "15 minutes",
    "retardMaximum": "45 minutes",
    "joursAvecPlusRetards": "2025-01-10",
    "plageHoraireRetards": "07:30-08:30"
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
      "tauxPresence": 100.0,            // ✅ POURCENTAGE INDIVIDUEL
      "tauxRetard": 9.09                // ✅ POURCENTAGE RETARD
    }
  ]
}
```

---

#### B) Par Option
```http
GET /api/Reporting/Presence/Eleves/Option/{idOption}
    ?dateDebut=2025-01-01
    &dateFin=2025-01-31
    &groupBy=classe
```

**Retour :**
```json
{
  "option": {
    "idOption": 2,
    "nomOption": "Scientifique",
    "section": "Secondaire",
    "nombreClasses": 4,
    "effectifTotal": 140
  },
  "periode": "Janvier 2025",
  "statistiquesGlobales": {
    "totalPresencesAttendue": 3080,     // 140 élèves × 22 jours
    "totalPresencesEffectives": 2800,
    "tauxPresence": 90.91,
    "tauxAbsence": 9.09,
    "totalRetards": 95,
    "tauxRetard": 3.08
  },
  "parClasse": [
    {
      "classe": "3ème Scientifique A",
      "effectif": 35,
      "tauxPresence": 92.5,
      "tauxRetard": 2.8
    },
    {
      "classe": "3ème Scientifique B",
      "effectif": 35,
      "tauxPresence": 88.2,
      "tauxRetard": 4.1
    }
  ]
}
```

---

#### C) Par Section
```http
GET /api/Reporting/Presence/Eleves/Section/{idSection}
    ?dateDebut=2025-01-01
    &dateFin=2025-01-31
    &groupBy=option|classe
```

**Retour :**
```json
{
  "section": {
    "idSection": 1,
    "nomSection": "Secondaire",
    "nombreOptions": 3,
    "nombreClasses": 12,
    "effectifTotal": 420
  },
  "periode": "Janvier 2025",
  "statistiquesGlobales": {
    "totalPresencesAttendue": 9240,
    "totalPresencesEffectives": 8400,
    "tauxPresence": 90.91,
    "tauxAbsence": 9.09,
    "totalRetards": 280,
    "tauxRetard": 3.03
  },
  "parOption": [
    {
      "option": "Scientifique",
      "effectif": 140,
      "tauxPresence": 92.3,
      "tauxRetard": 2.5
    },
    {
      "option": "Littéraire",
      "effectif": 140,
      "tauxPresence": 89.8,
      "tauxRetard": 3.8
    }
  ]
}
```

---

#### D) Par Direction
```http
GET /api/Reporting/Presence/Eleves/Direction/{idDirection}
    ?dateDebut=2025-01-01
    &dateFin=2025-01-31
    &groupBy=section|option|classe
```

**Retour :**
```json
{
  "direction": {
    "idDirection": 1,
    "nomDirection": "Direction Générale Académique",
    "nombreSections": 2,
    "nombreClasses": 20,
    "effectifTotal": 700
  },
  "periode": "Janvier 2025",
  "statistiquesGlobales": {
    "totalPresencesAttendue": 15400,
    "totalPresencesEffectives": 14000,
    "tauxPresence": 90.91,
    "tauxAbsence": 9.09,
    "totalRetards": 450,
    "tauxRetard": 2.92
  },
  "parSection": [
    {
      "section": "Primaire",
      "effectif": 280,
      "tauxPresence": 88.5,
      "tauxRetard": 3.2
    },
    {
      "section": "Secondaire",
      "effectif": 420,
      "tauxPresence": 92.3,
      "tauxRetard": 2.7
    }
  ]
}
```

---

#### E) Par École (Global élèves)
```http
GET /api/Reporting/Presence/Eleves/Ecole/{idEcole}
    ?dateDebut=2025-01-01
    &dateFin=2025-01-31
    &groupBy=direction|section|option|classe
```

**Retour :**
```json
{
  "ecole": {
    "idEcole": 5,
    "nomEcole": "École Primaire Kasai",
    "nombreDirections": 2,
    "nombreClasses": 30,
    "effectifTotal": 1050
  },
  "periode": {
    "dateDebut": "2025-01-01",
    "dateFin": "2025-01-31",
    "joursOuvrables": 22
  },
  "statistiquesGlobales": {
    "totalPresencesAttendue": 23100,    // 1050 élèves × 22 jours
    "totalPresencesEffectives": 21000,
    "totalAbsences": 2100,
    "totalRetards": 650,
    "tauxPresence": 90.91,              // ✅ POURCENTAGE GLOBAL
    "tauxAbsence": 9.09,                // ✅ POURCENTAGE
    "tauxRetard": 2.81                  // ✅ POURCENTAGE
  },
  "statistiquesRetards": {              // ✅ STATS RETARDS ÉCOLE
    "nombreTotalRetards": 650,
    "elevesRetardataires": 285,
    "pourcentageElevesRetardataires": 27.14,
    "dureeMoyenneRetard": "18 minutes",
    "retardMaximum": "60 minutes",
    "jourAvecPlusRetards": {
      "date": "2025-01-15",
      "nombreRetards": 85
    },
    "plageHorairePlusRetards": "07:30-08:00"
  },
  "parDirection": [
    {
      "direction": "Direction Primaire",
      "effectif": 450,
      "tauxPresence": 88.5,
      "tauxRetard": 3.2
    },
    {
      "direction": "Direction Secondaire",
      "effectif": 600,
      "tauxPresence": 92.3,
      "tauxRetard": 2.5
    }
  ]
}
```

---

### 1.2 📈 Calcul des pourcentages (Élèves)

#### Endpoint dédié au calcul de pourcentage individuel
```http
GET /api/Reporting/Presence/Eleves/{idEleve}/Pourcentage
    ?dateDebut=2025-01-01
    &dateFin=2025-01-31
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
  "calculs": {
    "joursOuvrables": 22,
    "presences": 20,
    "absences": 2,
    "retards": 3,
    
    "pourcentages": {
      "tauxPresence": 90.91,            // ✅ (20 / 22) × 100
      "tauxAbsence": 9.09,              // ✅ (2 / 22) × 100
      "tauxRetard": 13.64               // ✅ (3 / 22) × 100
    },
    
    "appreciation": "Très Bien",        // ≥ 90%
    "tendance": {
      "evolution": "stable",
      "comparaisonMoisPrecedent": -1.2
    }
  }
}
```

---

#### Pourcentage avec comparaison classe
```http
GET /api/Reporting/Presence/Eleves/{idEleve}/PourcentageAvecComparaison
    ?dateDebut=2025-01-01
    &dateFin=2025-01-31
```

**Retour :**
```json
{
  "eleve": {
    "nomComplet": "Jean Mukendi",
    "tauxPresence": 90.91
  },
  "moyenneClasse": {
    "classe": "6ème A",
    "tauxPresenceMoyen": 93.5
  },
  "moyenneEcole": {
    "ecole": "École Primaire Kasai",
    "tauxPresenceMoyen": 88.5
  },
  "comparaison": {
    "vsClasse": -2.59,                  // Inférieur de 2.59% à la classe
    "vsEcole": 2.41,                    // Supérieur de 2.41% à l'école
    "classement": {
      "rangClasse": 12,
      "totalElèvesClasse": 35
    }
  }
}
```

---

### 1.3 📉 Statistiques détaillées sur les retards (Élèves)

#### A) Analyse globale des retards
```http
GET /api/Reporting/Presence/Eleves/Retards/Analyse
    ?idEcole=5
    &dateDebut=2025-01-01
    &dateFin=2025-01-31
    &heureReferenceMatin=08:00
```

**Retour :**
```json
{
  "ecole": "École Primaire Kasai",
  "periode": "Janvier 2025",
  "heureReference": "08:00",
  
  "statistiquesGlobales": {
    "nombreTotalRetards": 650,
    "nombreElevesRetardataires": 285,
    "pourcentageElevesRetardataires": 27.14,  // ✅ % élèves en retard au moins 1 fois
    "tauxRetardGlobal": 2.81,                   // ✅ % retards sur total présences
    "dureeMoyenneRetard": "18 minutes",
    "dureeMedianeRetard": "12 minutes",
    "retardMinimum": "1 minute",
    "retardMaximum": "60 minutes"
  },
  
  "repartitionParDuree": {                      // ✅ Distribution des retards
    "0-10min": 250,
    "10-20min": 280,
    "20-30min": 90,
    "30-60min": 30
  },
  
  "repartitionParJourSemaine": {                // ✅ Analyse par jour
    "lundi": { "retards": 150, "pourcentage": 23.08 },
    "mardi": { "retards": 110, "pourcentage": 16.92 },
    "mercredi": { "retards": 100, "pourcentage": 15.38 },
    "jeudi": { "retards": 120, "pourcentage": 18.46 },
    "vendredi": { "retards": 170, "pourcentage": 26.15 }
  },
  
  "topRetardataires": [                         // ✅ Top 10 retardataires
    {
      "rang": 1,
      "eleve": {
        "nomComplet": "Paul Tshisekedi",
        "matricule": "ESK25-C5D6E7",
        "classe": "6ème A"
      },
      "nombreRetards": 12,
      "pourcentageRetardSurPresences": 54.55,   // ✅ % de retard
      "dureeMoyenneRetard": "25 minutes"
    }
  ],
  
  "classesAvecPlusRetards": [                   // ✅ Classes problématiques
    {
      "classe": "5ème B",
      "nombreRetards": 85,
      "tauxRetard": 11.2
    }
  ]
}
```

---

#### B) Retards par élève individuel
```http
GET /api/Reporting/Presence/Eleves/{idEleve}/Retards
    ?dateDebut=2025-01-01
    &dateFin=2025-01-31
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
  "periode": "Janvier 2025",
  "heureReference": "08:00",
  
  "statistiquesRetards": {
    "nombreTotalRetards": 5,
    "nombrePresences": 20,
    "pourcentageRetard": 25.0,                  // ✅ % retards sur présences
    "dureeMoyenneRetard": "12 minutes",
    "dureeMaxRetard": "30 minutes",
    "jourSemaineFrequent": "Lundi"
  },
  
  "listeRetards": [
    {
      "date": "2025-01-08",
      "heureArrivee": "08:15",
      "heureReference": "08:00",
      "dureeRetard": "15 minutes",
      "jourSemaine": "Lundi",
      "observation": "Retard justifié - Transport"
    },
    {
      "date": "2025-01-15",
      "heureArrivee": "08:30",
      "heureReference": "08:00",
      "dureeRetard": "30 minutes",
      "jourSemaine": "Lundi",
      "observation": "Retard non justifié"
    }
  ],
  
  "comparaison": {
    "moyenneClasse": {
      "tauxRetard": 3.2,
      "dureeMoyenne": "10 minutes"
    },
    "position": "Supérieur à la moyenne (25% vs 3.2%)"
  }
}
```

---

#### C) Par Option (toutes les classes de l'option)
```http
GET /api/Reporting/Presence/Eleves/Option/{idOption}
    ?dateDebut=2025-01-01
    &dateFin=2025-01-31
```

---

#### D) Par Section (toutes les options de la section)
```http
GET /api/Reporting/Presence/Eleves/Section/{idSection}
    ?dateDebut=2025-01-01
    &dateFin=2025-01-31
```

---

#### E) Par Direction (toutes les sections)
```http
GET /api/Reporting/Presence/Eleves/Direction/{idDirection}
    ?dateDebut=2025-01-01
    &dateFin=2025-01-31
```

---

#### F) Par École (vue globale complète)
```http
GET /api/Reporting/Presence/Eleves/Ecole/{idEcole}
    ?dateDebut=2025-01-01
    &dateFin=2025-01-31
```

---

### 1.4 📊 Pourcentages élèves - Tous les calculs

#### Endpoint complet de calcul
```http
GET /api/Reporting/Presence/Eleves/{idEleve}/PourcentagesComplets
    ?dateDebut=2025-01-01
    &dateFin=2025-01-31
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
  "periode": "Janvier 2025 (22 jours ouvrables)",
  
  "donneesBrutes": {
    "joursOuvrables": 22,
    "presences": 20,
    "absences": 2,
    "retards": 3,
    "presencesPonctuelles": 17           // Présences sans retard
  },
  
  "pourcentages": {
    "tauxPresence": 90.91,               // ✅ (20 / 22) × 100
    "tauxAbsence": 9.09,                 // ✅ (2 / 22) × 100
    "tauxRetard": 13.64,                 // ✅ (3 / 22) × 100
    "tauxPonctualite": 77.27             // ✅ (17 / 22) × 100 (ni absent ni retard)
  },
  
  "appreciation": {
    "global": "Très Bien",               // ≥ 90%
    "ponctualite": "Bien",               // ≥ 75%
    "commentaire": "Excellente assiduité mais quelques retards à améliorer"
  },
  
  "comparaisons": {
    "vsClasse": {
      "tauxPresenceMoyenClasse": 93.5,
      "ecart": -2.59,
      "position": "Inférieur à la moyenne"
    },
    "vsEcole": {
      "tauxPresenceMoyenEcole": 88.5,
      "ecart": 2.41,
      "position": "Supérieur à la moyenne"
    }
  },
  
  "evolutionMensuelle": [
    {
      "mois": "Décembre 2024",
      "tauxPresence": 92.0,
      "evolution": -1.09                 // ✅ Évolution en %
    },
    {
      "mois": "Janvier 2025",
      "tauxPresence": 90.91
    }
  ]
}
```

---

## 📗 PARTIE 2 : REPORTING AGENTS

### 2.1 📊 Groupement par fonction et département

#### A) Par Fonction
```http
GET /api/Reporting/Presence/Agents/Fonction/{fonction}
    ?idEcole=5
    &dateDebut=2025-01-01
    &dateFin=2025-01-31
```

**Retour :**
```json
{
  "fonction": "Professeur",
  "ecole": "École Primaire Kasai",
  "periode": "Janvier 2025",
  
  "statistiquesGlobales": {
    "nombreAgents": 25,
    "joursOuvrables": 22,
    "totalPresencesAttendue": 550,
    "totalPresencesEffectives": 520,
    "totalAbsences": 30,
    "totalRetards": 15,
    "tauxPresence": 94.55,               // ✅ POURCENTAGE
    "tauxAbsence": 5.45,                 // ✅ POURCENTAGE
    "tauxRetard": 2.73,                  // ✅ POURCENTAGE
    "totalHeuresTravaillees": 4160.5
  },
  
  "statistiquesRetards": {               // ✅ STATS RETARDS FONCTION
    "nombreTotalRetards": 15,
    "agentsRetardataires": 8,
    "pourcentageAgentsRetardataires": 32.0,
    "dureeMoyenneRetard": "10 minutes",
    "retardMaximum": "25 minutes"
  },
  
  "parAgent": [
    {
      "agent": {
        "idAgent": 15,
        "nomComplet": "Marie Kabongo",
        "matricule": "NAT25-D7E2F9"
      },
      "presences": 21,
      "absences": 1,
      "retards": 2,
      "tauxPresence": 95.45,             // ✅ POURCENTAGE INDIVIDUEL
      "tauxRetard": 9.09,
      "heuresTravaillees": 168.5
    }
  ]
}
```

---

#### B) Toutes les fonctions (vue d'ensemble)
```http
GET /api/Reporting/Presence/Agents/Fonctions/Comparatif
    ?idEcole=5
    &dateDebut=2025-01-01
    &dateFin=2025-01-31
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
      "tauxRetard": 0.0,
      "heuresMoyennesTravail": 180.0
    },
    {
      "fonction": "Directeur",
      "effectif": 2,
      "tauxPresence": 97.5,
      "tauxRetard": 2.5,
      "heuresMoyennesTravail": 175.0
    },
    {
      "fonction": "Professeur",
      "effectif": 25,
      "tauxPresence": 94.55,
      "tauxRetard": 2.73,
      "heuresMoyennesTravail": 168.5
    },
    {
      "fonction": "Secrétaire",
      "effectif": 2,
      "tauxPresence": 95.0,
      "tauxRetard": 5.0,
      "heuresMoyennesTravail": 170.0
    }
  ]
}
```

---

#### C) Par École (Global agents)
```http
GET /api/Reporting/Presence/Agents/Ecole/{idEcole}
    ?dateDebut=2025-01-01
    &dateFin=2025-01-31
    &groupBy=fonction
```

**Retour :**
```json
{
  "ecole": {
    "idEcole": 5,
    "nomEcole": "École Primaire Kasai",
    "nombreAgents": 30
  },
  "periode": "Janvier 2025 (22 jours ouvrables)",
  
  "statistiquesGlobales": {
    "totalPresencesAttendue": 660,      // 30 agents × 22 jours
    "totalPresencesEffectives": 620,
    "totalAbsences": 40,
    "totalRetards": 20,
    "tauxPresence": 93.94,               // ✅ POURCENTAGE
    "tauxAbsence": 6.06,                 // ✅ POURCENTAGE
    "tauxRetard": 3.03,                  // ✅ POURCENTAGE
    "totalHeuresTravaillees": 5040.0,
    "heureMoyenneArrivee": "07:45",
    "heureMoyenneDepart": "16:15"
  },
  
  "statistiquesRetards": {               // ✅ STATS RETARDS AGENTS
    "nombreTotalRetards": 20,
    "agentsRetardataires": 12,
    "pourcentageAgentsRetardataires": 40.0,
    "dureeMoyenneRetard": "15 minutes",
    "retardMaximum": "40 minutes",
    "fonctionPlusRetards": "Professeur"
  }
}
```

---

### 2.2 📈 Pourcentages agents

#### Endpoint individuel agent
```http
GET /api/Reporting/Presence/Agents/{idAgent}/Pourcentage
    ?dateDebut=2025-01-01
    &dateFin=2025-01-31
```

**Retour :**
```json
{
  "agent": {
    "idAgent": 12,
    "nomComplet": "Jean Mukendi Kalala",
    "matricule": "NAT25-C9D3E7",
    "fonction": "Manager Général"
  },
  "periode": "Janvier 2025 (22 jours ouvrables)",
  
  "calculs": {
    "joursOuvrables": 22,
    "presences": 21,
    "absences": 1,
    "retards": 0,
    
    "pourcentages": {
      "tauxPresence": 95.45,             // ✅ (21 / 22) × 100
      "tauxAbsence": 4.55,               // ✅ (1 / 22) × 100
      "tauxRetard": 0.0,                 // ✅ (0 / 22) × 100
      "tauxPonctualite": 95.45           // ✅ Présences sans retard
    },
    
    "heures": {
      "totalHeuresTravaillees": 168.5,
      "heureMoyenneArrivee": "07:30",
      "heureMoyenneDepart": "16:00",
      "dureeJournaliereMoyenne": "8h02"
    }
  },
  
  "appreciation": "Excellent",
  
  "comparaison": {
    "vsFonction": {
      "fonction": "Manager Général",
      "tauxMoyen": 100.0,
      "ecart": -4.55
    },
    "vsEcole": {
      "tauxMoyen": 93.94,
      "ecart": 1.51,
      "position": "Supérieur à la moyenne"
    }
  }
}
```

---

### 2.3 📉 Statistiques retards agents

#### Analyse globale retards agents
```http
GET /api/Reporting/Presence/Agents/Retards/Analyse
    ?idEcole=5
    &dateDebut=2025-01-01
    &dateFin=2025-01-31
    &heureReference=07:30
```

**Retour :**
```json
{
  "ecole": "École Primaire Kasai",
  "periode": "Janvier 2025",
  "heureReference": "07:30",
  
  "statistiquesGlobales": {
    "nombreTotalRetards": 20,
    "nombreAgentsRetardataires": 12,
    "pourcentageAgentsRetardataires": 40.0,    // ✅ % agents en retard
    "tauxRetardGlobal": 3.03,                  // ✅ % retards sur presences
    "dureeMoyenneRetard": "15 minutes",
    "retardMaximum": "40 minutes"
  },
  
  "parFonction": {
    "Manager Général": {
      "retards": 0,
      "tauxRetard": 0.0
    },
    "Directeur": {
      "retards": 1,
      "tauxRetard": 2.27
    },
    "Professeur": {
      "retards": 18,
      "tauxRetard": 3.27
    },
    "Secrétaire": {
      "retards": 1,
      "tauxRetard": 2.27
    }
  },
  
  "topRetardataires": [
    {
      "rang": 1,
      "agent": {
        "nomComplet": "Paul Tshisekedi",
        "matricule": "NAT25-E8F3A2",
        "fonction": "Professeur"
      },
      "nombreRetards": 5,
      "pourcentageRetardSurPresences": 22.73,
      "dureeMoyenneRetard": "20 minutes"
    }
  ]
}
```

---

## 📙 PARTIE 3 : ENDPOINTS DE GROUPEMENT COMPLET

### 3.1 Hiérarchie complète École → Direction → Section → Option → Classe

```http
GET /api/Reporting/Presence/Hierarchie/{idEcole}
    ?dateDebut=2025-01-01
    &dateFin=2025-01-31
    &type=ELEVE|AGENT|TOUS
```

**Retour (structure hiérarchique complète) :**
```json
{
  "ecole": {
    "idEcole": 5,
    "nomEcole": "École Primaire Kasai",
    "tauxPresence": 90.91,
    "tauxRetard": 2.81
  },
  "directions": [
    {
      "idDirection": 1,
      "nomDirection": "Direction Primaire",
      "tauxPresence": 88.5,
      "tauxRetard": 3.2,
      "sections": [
        {
          "idSection": 1,
          "nomSection": "Primaire",
          "tauxPresence": 88.5,
          "tauxRetard": 3.2,
          "options": [
            {
              "idOption": 1,
              "nomOption": "Générale",
              "tauxPresence": 88.5,
              "tauxRetard": 3.2,
              "classes": [
                {
                  "idClasse": 3,
                  "nomClasse": "6ème A",
                  "effectif": 35,
                  "tauxPresence": 93.51,
                  "tauxRetard": 2.5
                },
                {
                  "idClasse": 4,
                  "nomClasse": "6ème B",
                  "effectif": 33,
                  "tauxPresence": 85.7,
                  "tauxRetard": 4.1
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

## 📕 PARTIE 4 : ENDPOINTS STATISTIQUES AVANCÉS

### 4.1 Tableau de bord multi-niveaux

#### Dashboard par classe
```http
GET /api/Reporting/Presence/Dashboard/Classe/{idClasse}
    ?date=2025-01-15
```

#### Dashboard par section
```http
GET /api/Reporting/Presence/Dashboard/Section/{idSection}
    ?date=2025-01-15
```

#### Dashboard par école
```http
GET /api/Reporting/Presence/Dashboard/Ecole/{idEcole}
    ?date=2025-01-15
```

**Retour Dashboard École :**
```json
{
  "ecole": "École Primaire Kasai",
  "date": "2025-01-15",
  
  "resumeJour": {
    "eleves": {
      "effectifTotal": 1050,
      "presents": 950,
      "absents": 100,
      "retards": 35,
      "tauxPresence": 90.48,
      "tauxRetard": 3.33
    },
    "agents": {
      "effectifTotal": 30,
      "presents": 28,
      "absents": 2,
      "retards": 1,
      "tauxPresence": 93.33,
      "tauxRetard": 3.33
    }
  },
  
  "alertes": [
    {
      "type": "danger",
      "message": "Classe 5ème B : Taux de présence 65% (seuil: 80%)",
      "action": "Contacter le directeur de section"
    },
    {
      "type": "warning",
      "message": "15 élèves en retard de plus de 30 minutes",
      "action": "Vérifier problème transport"
    }
  ],
  
  "classesProblematiques": [
    {
      "classe": "5ème B",
      "tauxPresence": 65.0,
      "absents": 12,
      "status": "critique"
    }
  ],
  
  "agentsAbsents": [
    {
      "agent": "Marie Kabongo",
      "fonction": "Professeur",
      "coursAffectes": 3,
      "status": "À remplacer"
    }
  ]
}
```

---

### 4.2 Comparatifs et analyses croisées

#### Comparaison multi-critères
```http
POST /api/Reporting/Presence/Analyse/Comparaison
Content-Type: application/json

{
  "idEcole": 5,
  "dateDebut": "2025-01-01",
  "dateFin": "2025-01-31",
  "comparaisons": [
    {
      "type": "eleves",
      "groupBy": "classe",
      "metric": "tauxPresence"
    },
    {
      "type": "agents",
      "groupBy": "fonction",
      "metric": "tauxRetard"
    }
  ]
}
```

---

## 📊 PARTIE 5 : FORMULES DE CALCUL

### Formules principales

```csharp
// 1. Taux de présence
TauxPresence = (NombrePresences / JoursOuvrables) × 100

// 2. Taux d'absence
TauxAbsence = (NombreAbsences / JoursOuvrables) × 100

// 3. Taux de retard
TauxRetard = (NombreRetards / JoursOuvrables) × 100

// 4. Taux de ponctualité
TauxPonctualite = (PresencesSansRetard / JoursOuvrables) × 100

// 5. Durée moyenne retard
DureeMoyenneRetard = SommeMinutesRetard / NombreRetards

// 6. Heures travaillées (AGENTS uniquement)
HeuresTravaillees = Σ(HeureDepart - HeureArrivee)

// 7. Pourcentage agents/élèves retardataires
PourcentageRetardataires = (PersonnesAvecRetard / EffectifTotal) × 100
```

---

## 🎯 RÉCAPITULATIF DES ENDPOINTS ESSENTIELS

### ÉLÈVES (Groupement hiérarchique)

| Niveau | Endpoint | Groupement |
|--------|----------|------------|
| **Individuel** | `GET /Eleves/{id}/Pourcentage` | 1 élève |
| **Classe** | `GET /Eleves/Classe/{id}` | ~35 élèves |
| **Option** | `GET /Eleves/Option/{id}` | ~140 élèves (4 classes) |
| **Section** | `GET /Eleves/Section/{id}` | ~420 élèves (12 classes) |
| **Direction** | `GET /Eleves/Direction/{id}` | ~700 élèves (20 classes) |
| **École** | `GET /Eleves/Ecole/{id}` | ~1050 élèves (30 classes) |

### AGENTS (Groupement par fonction)

| Niveau | Endpoint | Groupement |
|--------|----------|------------|
| **Individuel** | `GET /Agents/{id}/Pourcentage` | 1 agent |
| **Fonction** | `GET /Agents/Fonction/{fonction}` | Ex: 25 Professeurs |
| **École** | `GET /Agents/Ecole/{id}` | ~30 agents |

### RETARDS (Analyses spécialisées)

| Type | Endpoint | Détails |
|------|----------|---------|
| **Élèves** | `GET /Eleves/Retards/Analyse` | Stats complètes retards élèves |
| **Élève individuel** | `GET /Eleves/{id}/Retards` | Détail retards 1 élève |
| **Agents** | `GET /Agents/Retards/Analyse` | Stats complètes retards agents |
| **Agent individuel** | `GET /Agents/{id}/Retards` | Détail retards 1 agent |

---

## ✅ Points manquants que vous avez identifiés (corrigés)

### ✅ Groupement par niveau hiérarchique
- [x] Par Classe
- [x] Par Option
- [x] Par Section
- [x] Par Direction
- [x] Par École

### ✅ Calcul des pourcentages
- [x] Taux de présence (individuel et groupé)
- [x] Taux d'absence
- [x] Taux de retard
- [x] Taux de ponctualité
- [x] Comparaison vs moyenne

### ✅ Statistiques sur les retards
- [x] Nombre total de retards
- [x] Durée moyenne des retards
- [x] Durée maximale
- [x] Répartition par durée (0-10min, 10-20min, etc.)
- [x] Répartition par jour de semaine
- [x] Top retardataires
- [x] % de personnes retardataires
- [x] Analyse par fonction (agents) ou classe (élèves)

---

**Voulez-vous que je commence l'implémentation de ces endpoints ?**

Je peux commencer par les endpoints prioritaires comme :
1. Groupement par Classe/Option/Section/Direction/École
2. Calcul des pourcentages individuels
3. Statistiques sur les retards

Qu'en pensez-vous ? 🚀

