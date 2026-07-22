# 📊 Analyse Complète : Endpoints `/api/Dashboard/`

**Date** : 2025-01-16  
**Version** : 1.0  
**Statut** : ✅ Documentation complète

---

## 📋 Table des Matières

1. [Vue d'ensemble](#vue-densemble)
2. [Endpoint 1 : Dashboard Global](#endpoint-1-dashboard-global)
3. [Endpoint 2 : Dashboard Présence](#endpoint-2-dashboard-présence)
4. [Endpoint 3 : Dashboard Paiement](#endpoint-3-dashboard-paiement)
5. [Endpoint 4 : Dashboard Super-Admin](#endpoint-4-dashboard-super-admin)
6. [Endpoint 5 : Dashboard Comparaison](#endpoint-5-dashboard-comparaison)
7. [Endpoint 6 : Dashboard KPI](#endpoint-6-dashboard-kpi)
8. [Structure des DTOs](#structure-des-dtos)
9. [Exemples de Réponses JSON](#exemples-de-réponses-json)
10. [Calculs et Logique Métier](#calculs-et-logique-métier)

---

## 🎯 Vue d'ensemble

Le contrôleur `DashboardController` expose **6 endpoints** pour fournir des statistiques et analyses sur les écoles :

| Endpoint | Route | Statut | Description |
|----------|-------|--------|-------------|
| 1. Global | `GET /api/Dashboard/global` | ✅ Opérationnel | Dashboard combiné (Présence + Paiement) |
| 2. Présence | `GET /api/Dashboard/presence` | ✅ Opérationnel | Dashboard présence uniquement |
| 3. Paiement | `GET /api/Dashboard/paiement` | ✅ Opérationnel | Dashboard paiement uniquement |
| 4. Super-Admin | `GET /api/Dashboard/super-admin` | ✅ Opérationnel | Vue globale toutes écoles |
| 5. Comparaison | `GET /api/Dashboard/comparaison` | ⏳ En développement | Comparaison multi-écoles |
| 6. KPI | `GET /api/Dashboard/kpi` | ⏳ En développement | Indicateurs clés de performance |

**Authentification** : Tous les endpoints nécessitent un **token JWT** (`[Authorize]`)

---

## 📊 Endpoint 1 : Dashboard Global

### **Route**
```http
GET /api/Dashboard/global?idEcole={idEcole}
```

### **Paramètres**
- `idEcole` (requis) : ID de l'école

### **Période**
- **Par défaut** : Mois en cours (du 1er au dernier jour du mois)
- Calcul automatique : `debutMois = new DateTime(aujourdhui.Year, aujourdhui.Month, 1)`
- `finMois = debutMois.AddMonths(1).AddDays(-1)`

### **Réponse : `DashboardGlobalDto`**

#### **Structure Complète**

```json
{
  "ecole": {
    "idEcole": 18,
    "nomEcole": "Ekelasi School",
    "logo": "https://..."
  },
  "periode": {
    "dateDebut": "2025-01-01T00:00:00Z",
    "dateFin": "2025-01-31T23:59:59Z",
    "libelle": "janvier 2025"
  },
  "statistiques": {
    "nombreDirections": 3,
    "nombreClasses": 25,
    "nombreEleves": 1050,
    "nombreElevesActifs": 1045,
    "nombreEnseignants": 30,
    "nombreEnseignantsActifs": 28
  },
  "repartitionEleves": {
    "totalEleves": 1050,
    "totalElevesActifs": 1045,
    "parDirection": [
      {
        "idDirection": 1,
        "nomDirection": "Primaire",
        "nombreEleves": 450,
        "nombreElevesActifs": 448,
        "pourcentage": 42.86
      },
      {
        "idDirection": 2,
        "nomDirection": "Secondaire",
        "nombreEleves": 600,
        "nombreElevesActifs": 597,
        "pourcentage": 57.14
      }
    ],
    "parSection": [
      {
        "idSection": 1,
        "nomSection": "Générale",
        "nombreEleves": 800,
        "nombreElevesActifs": 798,
        "pourcentage": 76.19
      },
      {
        "idSection": 2,
        "nomSection": "Technique",
        "nombreEleves": 250,
        "nombreElevesActifs": 247,
        "pourcentage": 23.81
      }
    ],
    "parOption": [
      {
        "idOption": 1,
        "nomOption": "Math-Physique",
        "idSection": 1,
        "nomSection": "Générale",
        "nombreEleves": 300,
        "nombreElevesActifs": 299,
        "pourcentage": 28.57
      }
    ]
  },
  "presence": {
    "resumeEleves": {
      "effectifTotal": 1050,
      "presents": 950,
      "absents": 25,
      "retards": 35,
      "tauxPresence": 90.48,
      "tauxAbsence": 2.38,
      "tauxRetard": 3.33
    },
    "resumeAgents": {
      "effectifTotal": 30,
      "presents": 28,
      "absents": 1,
      "retards": 1,
      "tauxPresence": 93.33,
      "tauxAbsence": 3.33,
      "tauxRetard": 3.33
    },
    "alertes": [
      {
        "type": "warning",
        "message": "📚 2 agent(s) absent(s) avec cours affectés",
        "action": "Organiser des remplacements"
      },
      {
        "type": "danger",
        "message": "⚠️ Taux de présence élèves en dessous de 80%",
        "action": "Contacter les parents"
      }
    ],
    "classesProblematiques": [
      {
        "idClasse": 12,
        "nomClasse": "5ème B",
        "presents": 18,
        "absents": 10,
        "tauxPresence": 64.29,
        "status": "attention"
      },
      {
        "idClasse": 15,
        "nomClasse": "6ème C",
        "presents": 15,
        "absents": 12,
        "tauxPresence": 55.56,
        "status": "critique"
      }
    ],
    "agentsAbsents": [
      {
        "idAgent": 5,
        "nomComplet": "Jean Kabila",
        "fonction": "Professeur",
        "coursAffectes": 4,
        "status": "critique"
      }
    ]
  },
  "paiement": {
    "resume": {
      "nombrePaiements": 450,
      "montantTotal": 125000.00,
      "montantAttendu": 150000.00,
      "tauxRecouvrement": 83.33,
      "nombreEleves": 1050,
      "elevesAyantPaye": 420,
      "elevesEnRetard": 630,
      "tauxPaiementEleves": 40.00,
      "devisePrincipale": "USD"
    },
    "repartitionParMode": {
      "modes": {
        "Espèces": {
          "nombre": 250,
          "montant": 75000.00,
          "pourcentage": 55.56
        },
        "Mobile Money": {
          "nombre": 150,
          "montant": 40000.00,
          "pourcentage": 33.33
        },
        "Virement": {
          "nombre": 50,
          "montant": 10000.00,
          "pourcentage": 11.11
        }
      }
    },
    "top5Frais": [
      {
        "rang": 1,
        "libelleFrais": "Minerval Janvier",
        "montantTotal": 50000.00,
        "nombrePaiements": 200,
        "pourcentage": 40.00
      },
      {
        "rang": 2,
        "libelleFrais": "Frais de scolarité",
        "montantTotal": 30000.00,
        "nombrePaiements": 150,
        "pourcentage": 24.00
      },
      {
        "rang": 3,
        "libelleFrais": "Frais d'inscription",
        "montantTotal": 25000.00,
        "nombrePaiements": 80,
        "pourcentage": 20.00
      },
      {
        "rang": 4,
        "libelleFrais": "Frais de transport",
        "montantTotal": 15000.00,
        "nombrePaiements": 20,
        "pourcentage": 12.00
      },
      {
        "rang": 5,
        "libelleFrais": "Frais de cantine",
        "montantTotal": 5000.00,
        "nombrePaiements": 10,
        "pourcentage": 4.00
      }
    ]
  }
}
```

### **Détails des Sections**

#### **1. `ecole` (EcoleInfoDto)**
- `idEcole` : Identifiant unique de l'école
- `nomEcole` : Nom de l'école
- `logo` : URL du logo (optionnel)

#### **2. `periode` (PeriodeDto)**
- `dateDebut` : Date de début de la période (1er jour du mois)
- `dateFin` : Date de fin de la période (dernier jour du mois)
- `libelle` : Libellé formaté (ex: "janvier 2025")

#### **3. `statistiques` (StatistiquesGeneralesDto)**
- `nombreDirections` : Nombre de directions actives
- `nombreClasses` : Nombre de classes actives
- `nombreEleves` : Nombre total d'élèves
- `nombreElevesActifs` : Nombre d'élèves actifs (statut = true)
- `nombreEnseignants` : Nombre total d'enseignants/agents
- `nombreEnseignantsActifs` : Nombre d'enseignants actifs

#### **4. `repartitionEleves` (RepartitionElevesDto)**
- `totalEleves` : Total d'élèves (redondant avec statistiques)
- `totalElevesActifs` : Total d'élèves actifs
- `parDirection` : Liste des répartitions par direction (triée par nombre décroissant)
- `parSection` : Liste des répartitions par section (triée par nombre décroissant)
- `parOption` : Liste des répartitions par option (triée par nombre décroissant)

**Chaque répartition contient** :
- Identifiant et nom
- Nombre d'élèves total
- Nombre d'élèves actifs
- Pourcentage par rapport au total

#### **5. `presence` (DashboardPresenceResumeDto)**

##### **5.1. `resumeEleves` (ResumePresenceDto)**
- `effectifTotal` : Nombre total d'élèves
- `presents` : Nombre d'élèves présents
- `absents` : Nombre d'élèves absents
- `retards` : Nombre d'élèves en retard
- `tauxPresence` : Pourcentage de présence (calculé)
- `tauxAbsence` : Pourcentage d'absence (calculé)
- `tauxRetard` : Pourcentage de retard (calculé)

##### **5.2. `resumeAgents` (ResumePresenceDto)**
- Même structure que `resumeEleves` mais pour les agents/enseignants

##### **5.3. `alertes` (List<AlerteDto>)**
- `type` : "danger", "warning", "info"
- `message` : Message descriptif
- `action` : Action recommandée (optionnel)

##### **5.4. `classesProblematiques` (List<ClasseProblematiqueDto>)**
- `idClasse` : Identifiant de la classe
- `nomClasse` : Nom de la classe
- `presents` : Nombre de présents
- `absents` : Nombre d'absents
- `tauxPresence` : Taux de présence
- `status` : "critique", "attention", "normal"

##### **5.5. `agentsAbsents` (List<AgentAbsentDto>)**
- `idAgent` : Identifiant de l'agent
- `nomComplet` : Nom complet
- `fonction` : Fonction de l'agent
- `coursAffectes` : Nombre de cours affectés
- `status` : Statut de l'absence

#### **6. `paiement` (DashboardPaiementResumeDto)**

##### **6.1. `resume` (ResumePaiementDto)**
- `nombrePaiements` : Nombre total de paiements
- `montantTotal` : Montant total collecté
- `montantAttendu` : Montant attendu (calculé)
- `tauxRecouvrement` : Taux de recouvrement (calculé)
- `nombreEleves` : Nombre total d'élèves
- `elevesAyantPaye` : Nombre d'élèves ayant payé
- `elevesEnRetard` : Nombre d'élèves en retard
- `tauxPaiementEleves` : Taux de paiement (calculé)
- `devisePrincipale` : Devise principale (ex: "USD")

##### **6.2. `repartitionParMode` (RepartitionModePaiementDto)**
- `modes` : Dictionnaire avec les modes de paiement
  - Clé : Nom du mode (ex: "Espèces", "Mobile Money")
  - Valeur : `ModePaiementStatsDto`
    - `nombre` : Nombre de paiements
    - `montant` : Montant total
    - `pourcentage` : Pourcentage par rapport au total

##### **6.3. `top5Frais` (List<Top5FraisDto>)**
- `rang` : Rang (1 à 5)
- `libelleFrais` : Libellé du frais
- `montantTotal` : Montant total collecté
- `nombrePaiements` : Nombre de paiements
- `pourcentage` : Pourcentage par rapport au total

---

## 👥 Endpoint 2 : Dashboard Présence

### **Route**
```http
GET /api/Dashboard/presence?idEcole={idEcole}&date={date}&dateDebut={dateDebut}&dateFin={dateFin}
```

### **Paramètres**
- `idEcole` (requis) : ID de l'école
- `date` (optionnel) : Date spécifique
- `dateDebut` (optionnel) : Date de début
- `dateFin` (optionnel) : Date de fin

### **Réponse : `DashboardPresenceDto`**

```json
{
  "ecole": {
    "idEcole": 18,
    "nomEcole": "Ekelasi School",
    "logo": "https://..."
  },
  "periode": {
    "dateDebut": "2025-01-01T00:00:00Z",
    "dateFin": "2025-01-31T23:59:59Z",
    "libelle": "janvier 2025"
  },
  "resumeEleves": {
    "effectifTotal": 1050,
    "presents": 950,
    "absents": 25,
    "retards": 35,
    "tauxPresence": 90.48,
    "tauxAbsence": 2.38,
    "tauxRetard": 3.33
  },
  "resumeAgents": {
    "effectifTotal": 30,
    "presents": 28,
    "absents": 1,
    "retards": 1,
    "tauxPresence": 93.33,
    "tauxAbsence": 3.33,
    "tauxRetard": 3.33
  },
  "alertes": [
    {
      "type": "warning",
      "message": "📚 2 agent(s) absent(s) avec cours affectés",
      "action": "Organiser des remplacements"
    }
  ],
  "classesProblematiques": [
    {
      "idClasse": 12,
      "nomClasse": "5ème B",
      "presents": 18,
      "absents": 10,
      "tauxPresence": 64.29,
      "status": "attention"
    }
  ],
  "agentsAbsents": [
    {
      "idAgent": 5,
      "nomComplet": "Jean Kabila",
      "fonction": "Professeur",
      "coursAffectes": 4,
      "status": "critique"
    }
  ]
}
```

**Note** : Structure identique à la section `presence` du dashboard global, mais avec plus de détails possibles.

---

## 💰 Endpoint 3 : Dashboard Paiement

### **Route**
```http
GET /api/Dashboard/paiement?idEcole={idEcole}&date={date}&dateDebut={dateDebut}&dateFin={dateFin}&periode={periode}
```

### **Paramètres**
- `idEcole` (requis) : ID de l'école
- `date` (optionnel) : Date spécifique
- `dateDebut` (optionnel) : Date de début
- `dateFin` (optionnel) : Date de fin
- `periode` (optionnel) : "semaine", "mois", "trimestre", "annee"

### **Réponse : `DashboardPaiementDto`**

```json
{
  "ecole": {
    "idEcole": 18,
    "nomEcole": "Ekelasi School",
    "logo": "https://..."
  },
  "periode": {
    "dateDebut": "2025-01-01T00:00:00Z",
    "dateFin": "2025-01-31T23:59:59Z",
    "libelle": "janvier 2025"
  },
  "resume": {
    "nombrePaiements": 450,
    "montantTotal": 125000.00,
    "montantAttendu": 150000.00,
    "tauxRecouvrement": 83.33,
    "nombreEleves": 1050,
    "elevesAyantPaye": 420,
    "elevesEnRetard": 630,
    "tauxPaiementEleves": 40.00,
    "devisePrincipale": "USD"
  },
  "repartitionParMode": {
    "modes": {
      "Espèces": {
        "nombre": 250,
        "montant": 75000.00,
        "pourcentage": 55.56
      },
      "Mobile Money": {
        "nombre": 150,
        "montant": 40000.00,
        "pourcentage": 33.33
      }
    }
  },
  "top5Frais": [
    {
      "rang": 1,
      "libelleFrais": "Minerval Janvier",
      "montantTotal": 50000.00,
      "nombrePaiements": 200,
      "pourcentage": 40.00
    }
  ],
  "parStatut": [
    {
      "statut": "Validé",
      "nombre": 400,
      "montant": 100000.00,
      "pourcentage": 88.89
    },
    {
      "statut": "En attente",
      "nombre": 50,
      "montant": 25000.00,
      "pourcentage": 11.11
    }
  ]
}
```

**Note** : Structure identique à la section `paiement` du dashboard global, avec en plus `parStatut` (répartition par statut de paiement).

---

## 🏫 Endpoint 4 : Dashboard Super-Admin

### **Route**
```http
GET /api/Dashboard/super-admin
```

### **Paramètres**
- Aucun (période = mois en cours)

### **Autorisation**
- **Rôle requis** : `Super-Admin` uniquement
- Vérification via `_currentUserService.IsSuperAdmin`
- Retourne `403 Forbidden` si non autorisé

### **Réponse : `DashboardSuperAdminDto`**

```json
{
  "periode": {
    "dateDebut": "2025-01-01T00:00:00Z",
    "dateFin": "2025-01-31T23:59:59Z",
    "libelle": "janvier 2025"
  },
  "statistiquesGlobales": {
    "totalEcoles": 50,
    "totalDirections": 150,
    "totalClasses": 1250,
    "totalEleves": 50000,
    "totalElevesActifs": 49500,
    "totalElevesFilles": 25000,
    "totalElevesGarcons": 25000,
    "totalElevesFillesActives": 24750,
    "totalElevesGarconsActifs": 24750,
    "pourcentageFilles": 50.00,
    "pourcentageGarcons": 50.00,
    "totalEnseignants": 1500,
    "totalEnseignantsActifs": 1450,
    "moyennes": {
      "elevesParEcole": 1000.00,
      "enseignantsParEcole": 30.00,
      "classesParEcole": 25.00,
      "ratioElevesEnseignants": 33.33
    }
  },
  "parEcole": [
    {
      "idEcole": 18,
      "nomEcole": "Ekelasi School",
      "province": "Kinshasa",
      "ville": "Gombe",
      "nombreEleves": 1050,
      "nombreElevesActifs": 1045,
      "nombreElevesFilles": 525,
      "nombreElevesGarcons": 525,
      "nombreElevesFillesActives": 523,
      "nombreElevesGarconsActifs": 522,
      "pourcentage": 2.10,
      "pourcentageFilles": 50.00,
      "pourcentageGarcons": 50.00
    }
  ],
  "parProvince": [
    {
      "province": "Kinshasa",
      "nombreEleves": 20000,
      "nombreElevesActifs": 19800,
      "nombreElevesFilles": 10000,
      "nombreElevesGarcons": 10000,
      "nombreEcoles": 20,
      "pourcentage": 40.00,
      "pourcentageFilles": 50.00,
      "pourcentageGarcons": 50.00
    },
    {
      "province": "Haut-Katanga",
      "nombreEleves": 15000,
      "nombreElevesActifs": 14850,
      "nombreElevesFilles": 7500,
      "nombreElevesGarcons": 7500,
      "nombreEcoles": 15,
      "pourcentage": 30.00,
      "pourcentageFilles": 50.00,
      "pourcentageGarcons": 50.00
    }
  ],
  "parVille": [
    {
      "ville": "Kinshasa",
      "province": "Kinshasa",
      "nombreEleves": 18000,
      "nombreElevesActifs": 17820,
      "nombreElevesFilles": 9000,
      "nombreElevesGarcons": 9000,
      "nombreEcoles": 18,
      "pourcentage": 36.00,
      "pourcentageFilles": 50.00,
      "pourcentageGarcons": 50.00
    }
  ],
  "presenceGlobale": {
    "resumeEleves": {
      "totalPresences": 450000,
      "totalAbsences": 50000,
      "tauxPresence": 90.00
    },
    "resumeAgents": {
      "totalPresences": 43500,
      "totalAbsences": 1500,
      "tauxPresence": 96.67
    },
    "tauxPresenceMoyen": 93.34,
    "ecolesProblematiques": 5
  },
  "paiementGlobal": {
    "montantTotalCollecte": 6250000.00,
    "nombreTotalPaiements": 22500,
    "tauxRecouvrementMoyen": 75.00,
    "repartitionParMode": [
      {
        "mode": "Espèces",
        "nombre": 12500,
        "montant": 3750000.00,
        "pourcentage": 55.56
      },
      {
        "mode": "Mobile Money",
        "nombre": 7500,
        "montant": 2000000.00,
        "pourcentage": 33.33
      },
      {
        "mode": "Virement",
        "nombre": 2500,
        "montant": 500000.00,
        "pourcentage": 11.11
      }
    ]
  },
  "top10Ecoles": [
    {
      "idEcole": 18,
      "nomEcole": "Ekelasi School",
      "province": "Kinshasa",
      "ville": "Gombe",
      "nombreEleves": 1050,
      "nombreElevesActifs": 1045,
      "nombreElevesFilles": 525,
      "nombreElevesGarcons": 525,
      "nombreElevesFillesActives": 523,
      "nombreElevesGarconsActifs": 522,
      "pourcentage": 2.10,
      "pourcentageFilles": 50.00,
      "pourcentageGarcons": 50.00
    }
  ],
  "alertes": [
    {
      "type": "SYSTEME",
      "niveau": "WARNING",
      "message": "Ratio élèves/enseignants élevé : 33.3 élèves par enseignant",
      "ecolesConcernees": 50,
      "details": {
        "ratioActuel": 33.33,
        "ratioRecommande": 25
      }
    },
    {
      "type": "PRESENCE",
      "niveau": "ERROR",
      "message": "Taux de présence global faible : 90%",
      "ecolesConcernees": 5,
      "details": {
        "tauxActuel": 90.00,
        "seuilMinimum": 80
      }
    },
    {
      "type": "PAIEMENT",
      "niveau": "WARNING",
      "message": "Taux de recouvrement faible : 75%",
      "ecolesConcernees": 50,
      "details": {
        "tauxActuel": 75.00,
        "objectifMinimum": 70
      }
    },
    {
      "type": "SYSTEME",
      "niveau": "INFO",
      "message": "Système gérant 50000 élèves dans 50 écoles",
      "ecolesConcernees": 50,
      "details": {
        "totalEleves": 50000,
        "totalEcoles": 50,
        "moyenneElevesParEcole": 1000.00
      }
    }
  ]
}
```

### **Détails des Sections**

#### **1. `statistiquesGlobales` (StatistiquesGeneralesGlobalesDto)**
- Statistiques agrégées sur **toutes les écoles**
- Inclut les moyennes par école et le ratio élèves/enseignants

#### **2. `parEcole` (List<RepartitionEcoleDto>)**
- Répartition des élèves par école
- Inclut province, ville, genre (filles/garçons)
- Triée par nombre d'élèves décroissant

#### **3. `parProvince` (List<RepartitionProvinceDto>)**
- Répartition des élèves par province
- Inclut le nombre d'écoles par province
- Triée par nombre d'élèves décroissant

#### **4. `parVille` (List<RepartitionVilleDto>)**
- Répartition des élèves par ville
- Inclut la province associée
- Triée par nombre d'élèves décroissant

#### **5. `presenceGlobale` (DashboardPresenceGlobalDto)**
- Résumé des présences agrégé toutes écoles
- Taux de présence moyen
- Nombre d'écoles problématiques

#### **6. `paiementGlobal` (DashboardPaiementGlobalDto)**
- Résumé des paiements agrégé toutes écoles
- Montant total collecté
- Taux de recouvrement moyen
- Répartition par mode de paiement

#### **7. `top10Ecoles` (List<RepartitionEcoleDto>)**
- Top 10 des écoles par nombre d'élèves
- Même structure que `parEcole` mais limité à 10

#### **8. `alertes` (List<AlerteGlobaleDto>)**
- Alertes globales pour le Super-Admin
- Types : "SYSTEME", "PRESENCE", "PAIEMENT"
- Niveaux : "INFO", "WARNING", "ERROR", "CRITICAL"
- Inclut le nombre d'écoles concernées et des détails

---

## 🆚 Endpoint 5 : Dashboard Comparaison

### **Route**
```http
GET /api/Dashboard/comparaison?idEcoles={idEcoles}&date={date}&dateDebut={dateDebut}&dateFin={dateFin}
```

### **Statut**
⏳ **En développement** (retourne `501 Not Implemented`)

### **Réponse Prévue : `DashboardComparaisonDto`**

```json
{
  "periode": {
    "dateDebut": "2025-01-01T00:00:00Z",
    "dateFin": "2025-01-31T23:59:59Z",
    "libelle": "janvier 2025"
  },
  "ecoles": [
    {
      "idEcole": 18,
      "nomEcole": "Ekelasi School",
      "tauxPresenceEleves": 90.48,
      "tauxPresenceAgents": 93.33,
      "tauxRecouvrement": 83.33,
      "nombreEleves": 1050,
      "nombreAgents": 30,
      "classement": "Excellent"
    }
  ],
  "statistiquesGlobales": {
    "nombreTotalEcoles": 50,
    "nombreTotalEleves": 50000,
    "nombreTotalAgents": 1500,
    "tauxPresenceMoyenEleves": 82.5,
    "tauxPresenceMoyenAgents": 87.2,
    "tauxRecouvrementMoyen": 74.8,
    "montantTotalRecouvert": 5000000.00
  }
}
```

---

## 📈 Endpoint 6 : Dashboard KPI

### **Route**
```http
GET /api/Dashboard/kpi?idEcole={idEcole}&dateDebut={dateDebut}&dateFin={dateFin}
```

### **Statut**
⏳ **En développement** (retourne `501 Not Implemented`)

### **Réponse Prévue : `DashboardKpiDto`**

```json
{
  "ecole": {
    "idEcole": 18,
    "nomEcole": "Ekelasi School",
    "logo": "https://..."
  },
  "periode": {
    "dateDebut": "2025-01-01T00:00:00Z",
    "dateFin": "2025-01-31T23:59:59Z",
    "libelle": "janvier 2025"
  },
  "kpiPresence": {
    "tauxPresenceEleves": 90.48,
    "evolutionTauxPresenceEleves": 5.2,
    "tauxPresenceAgents": 93.33,
    "evolutionTauxPresenceAgents": 2.1,
    "nombreClassesCritiques": 2,
    "nombreAgentsAbsents": 1
  },
  "kpiPaiement": {
    "tauxRecouvrement": 83.33,
    "evolutionTauxRecouvrement": 10.5,
    "montantTotalRecouvert": 125000.00,
    "evolutionMontantRecouvert": 15000.00,
    "nombreElevesEnRetard": 630,
    "pourcentageElevesEnRetard": 60.00
  },
  "tendances": [
    {
      "date": "2025-10-01T00:00:00Z",
      "tauxPresence": 85.2,
      "tauxRecouvrement": 72.8
    },
    {
      "date": "2025-11-01T00:00:00Z",
      "tauxPresence": 90.4,
      "tauxRecouvrement": 83.3
    }
  ]
}
```

---

## 🔢 Calculs et Logique Métier

### **Calculs de Présence**

#### **Taux de Présence**
```csharp
tauxPresence = (presents / effectifTotal) * 100
```

#### **Taux d'Absence**
```csharp
tauxAbsence = (absents / effectifTotal) * 100
```

#### **Taux de Retard**
```csharp
tauxRetard = (retards / effectifTotal) * 100
```

### **Calculs de Paiement**

#### **Taux de Recouvrement**
```csharp
tauxRecouvrement = (montantTotal / montantAttendu) * 100
```

#### **Taux de Paiement Élèves**
```csharp
tauxPaiementEleves = (elevesAyantPaye / nombreEleves) * 100
```

### **Calculs de Pourcentage**

#### **Pourcentage de Répartition**
```csharp
pourcentage = (nombreEleves / totalEleves) * 100
```

#### **Pourcentage par Mode de Paiement**
```csharp
pourcentage = (nombrePaiementsMode / nombreTotalPaiements) * 100
```

### **Calculs de Moyennes (Super-Admin)**

#### **Moyenne d'Élèves par École**
```csharp
elevesParEcole = totalEleves / totalEcoles
```

#### **Ratio Élèves/Enseignants**
```csharp
ratioElevesEnseignants = totalEleves / totalEnseignants
```

### **Détermination des Statuts**

#### **Classe Problématique**
- `status = "critique"` : `tauxPresence < 60%`
- `status = "attention"` : `60% <= tauxPresence < 80%`
- `status = "normal"` : `tauxPresence >= 80%`

#### **Agent Absent**
- `status = "critique"` : `coursAffectes >= 3`
- `status = "attention"` : `1 <= coursAffectes < 3`
- `status = "normal"` : `coursAffectes = 0`

#### **Classement École (Comparaison)**
- `"Excellent"` : Tous les taux > 90%
- `"Bon"` : Tous les taux > 75%
- `"Moyen"` : Tous les taux > 60%
- `"Faible"` : Au moins un taux < 60%

---

## 📊 Exemples de Requêtes

### **Dashboard Global**
```http
GET /api/Dashboard/global?idEcole=18
Authorization: Bearer {token}
```

### **Dashboard Présence (Période spécifique)**
```http
GET /api/Dashboard/presence?idEcole=18&dateDebut=2025-01-01&dateFin=2025-01-31
Authorization: Bearer {token}
```

### **Dashboard Paiement (Mois)**
```http
GET /api/Dashboard/paiement?idEcole=18&periode=mois
Authorization: Bearer {token}
```

### **Dashboard Super-Admin**
```http
GET /api/Dashboard/super-admin
Authorization: Bearer {token}
```

---

## ⚠️ Codes d'Erreur

| Code | Signification | Exemple |
|------|---------------|---------|
| 200 | Succès | Données retournées |
| 400 | Requête invalide | `idEcole` manquant |
| 401 | Non authentifié | Token JWT manquant ou invalide |
| 403 | Interdit | Accès Super-Admin sans le rôle |
| 404 | Non trouvé | École inexistante |
| 500 | Erreur serveur | Erreur lors du calcul |
| 501 | Non implémenté | Endpoint en développement |

---

## 🔍 Notes Importantes

1. **Période par défaut** : Mois en cours (du 1er au dernier jour)
2. **Tri des listes** : Par nombre décroissant (plus grand en premier)
3. **Calculs de pourcentage** : Arrondis à 2 décimales
4. **Filtrage** : Seuls les éléments actifs (`Statut = true`) sont comptés
5. **Performance** : Les requêtes utilisent `Include()` pour éviter les N+1 queries
6. **Cache** : Non implémenté dans le contrôleur actuel (à prévoir)

---

**Version** : 1.0  
**Date** : 2025-01-16  
**Auteur** : Analyse complète des endpoints Dashboard
