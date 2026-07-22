# 📊 Documentation : Route Metrics pour le Monitoring de l'API

## 📋 Table des Matières

1. [Vue d'ensemble](#vue-densemble)
2. [Endpoints Disponibles](#endpoints-disponibles)
3. [Authentification et Autorisation](#authentification-et-autorisation)
4. [Détails des Endpoints](#détails-des-endpoints)
5. [Exemples d'Utilisation](#exemples-dutilisation)
6. [Intégration avec des Outils de Monitoring](#intégration-avec-des-outils-de-monitoring)
7. [Cas d'Usage](#cas-dusage)

---

## 🎯 Vue d'ensemble

La route **Metrics** expose des métriques de monitoring pour votre API KelasiNaBiso. Elle permet de surveiller :

- ✅ **Métriques générales** : Utilisateurs, écoles, élèves, agents, etc.
- ✅ **Métriques financières** : Paiements, frais, répartitions
- ✅ **Métriques académiques** : Inscriptions, notes, cours, évaluations
- ✅ **Métriques de santé** : État de la base de données, utilisation système
- ✅ **Métriques par école** : Statistiques spécifiques à une école

### Base URL

```
https://votre-api.com/api/Metrics
```

---

## 🔌 Endpoints Disponibles

| Endpoint | Méthode | Authentification | Description |
|----------|---------|------------------|-------------|
| `/api/Metrics/general` | GET | ✅ Requise | Métriques générales du système |
| `/api/Metrics/financial` | GET | ✅ Requise | Métriques financières |
| `/api/Metrics/academic` | GET | ✅ Requise | Métriques académiques |
| `/api/Metrics/health` | GET | ❌ Non requise | Métriques de santé (health check) |
| `/api/Metrics/ecole` | GET | ✅ Requise | Métriques spécifiques à une école |
| `/api/Metrics/complete` | GET | ✅ Super-Admin | Toutes les métriques combinées |

---

## 🔐 Authentification et Autorisation

### Endpoints Publics

- **`/api/Metrics/health`** : Accessible sans authentification (pour les health checks)

### Endpoints Authentifiés

- **`/api/Metrics/general`** : Tous les utilisateurs authentifiés
- **`/api/Metrics/financial`** : Tous les utilisateurs authentifiés
- **`/api/Metrics/academic`** : Tous les utilisateurs authentifiés
- **`/api/Metrics/ecole`** : Tous les utilisateurs authentifiés (filtre automatique par école)

### Endpoints Super-Admin

- **`/api/Metrics/complete`** : Super-Admin uniquement

---

## 📝 Détails des Endpoints

### 1. Métriques Générales

**Endpoint** : `GET /api/Metrics/general`

**Authentification** : Requise (JWT Token)

**Description** : Retourne les statistiques générales du système (toutes écoles confondues)

**Réponse** :
```json
{
  "timestamp": "2025-01-16T10:30:00Z",
  "ecoles": {
    "total": 15,
    "actifs": 15
  },
  "utilisateurs": {
    "total": 250,
    "actifs": 230
  },
  "eleves": {
    "total": 5000,
    "actifs": 4800
  },
  "agents": {
    "total": 300,
    "actifs": 280
  },
  "classes": {
    "total": 200,
    "actifs": 195
  },
  "tuteurs": {
    "total": 4500,
    "actifs": 4400
  },
  "inscriptions": {
    "total": 5200,
    "actifs": 4800
  }
}
```

---

### 2. Métriques Financières

**Endpoint** : `GET /api/Metrics/financial`

**Authentification** : Requise (JWT Token)

**Paramètres de Requête (Optionnels)** :

| Paramètre | Type | Description | Par défaut |
|-----------|------|-------------|------------|
| `dateDebut` | DateTime | Date de début de la période | 1er jour du mois en cours |
| `dateFin` | DateTime | Date de fin de la période | Dernier jour du mois en cours |

**Exemple de requête** :
```
GET /api/Metrics/financial?dateDebut=2025-01-01&dateFin=2025-01-31
```

**Réponse** :
```json
{
  "timestamp": "2025-01-16T10:30:00Z",
  "periode": {
    "dateDebut": "2025-01-01T00:00:00Z",
    "dateFin": "2025-01-31T23:59:59Z"
  },
  "paiements": {
    "total": 1250,
    "montantTotal": 125000.50,
    "montantMoyen": 100.00,
    "repartitionDevise": [
      {
        "devise": "USD",
        "nombre": 800,
        "montant": 80000.00,
        "pourcentage": 64.00
      },
      {
        "devise": "CDF",
        "nombre": 450,
        "montant": 45000.50,
        "pourcentage": 36.00
      }
    ],
    "repartitionMode": [
      {
        "mode": "Cash",
        "nombre": 600,
        "montant": 60000.00,
        "pourcentage": 48.00
      },
      {
        "mode": "Mobile Money",
        "nombre": 400,
        "montant": 40000.00,
        "pourcentage": 32.00
      },
      {
        "mode": "Carte",
        "nombre": 250,
        "montant": 25000.50,
        "pourcentage": 20.00
      }
    ]
  },
  "frais": {
    "total": 50,
    "actifs": 45
  },
  "paiementsEchoues": 12
}
```

---

### 3. Métriques Académiques

**Endpoint** : `GET /api/Metrics/academic`

**Authentification** : Requise (JWT Token)

**Paramètres de Requête (Optionnels)** :

| Paramètre | Type | Description | Par défaut |
|-----------|------|-------------|------------|
| `dateDebut` | DateTime | Date de début de la période | 12 mois avant aujourd'hui |
| `dateFin` | DateTime | Date de fin de la période | Aujourd'hui |

**Réponse** :
```json
{
  "timestamp": "2025-01-16T10:30:00Z",
  "periode": {
    "dateDebut": "2024-01-16T00:00:00Z",
    "dateFin": "2025-01-16T23:59:59Z"
  },
  "inscriptions": {
    "total": 1200,
    "repartitionParType": [
      {
        "type": "Inscription",
        "nombre": 800,
        "pourcentage": 66.67
      },
      {
        "type": "Réinscription",
        "nombre": 350,
        "pourcentage": 29.17
      },
      {
        "type": "Transfert",
        "nombre": 50,
        "pourcentage": 4.16
      }
    ]
  },
  "notes": {
    "total": 15000,
    "moyenneGenerale": 14.5
  },
  "cours": {
    "total": 100,
    "actifs": 95
  },
  "evaluations": {
    "total": 200,
    "actives": 180
  }
}
```

---

### 4. Métriques de Santé (Health Check)

**Endpoint** : `GET /api/Metrics/health`

**Authentification** : ❌ Non requise (public)

**Description** : Vérifie l'état de santé du système (base de données, mémoire, CPU)

**Réponse** :
```json
{
  "timestamp": "2025-01-16T10:30:00Z",
  "status": "Healthy",
  "database": {
    "connected": true,
    "responseTimeMs": 15
  },
  "system": {
    "memoryUsageMB": 512,
    "cpuTimeMs": 123456,
    "uptimeSeconds": 86400
  },
  "responseTimeMs": 20
}
```

**Codes de Statut** :
- `200 OK` : Système sain
- `500 Internal Server Error` : Erreur lors de la vérification

**Utilisation** : Idéal pour les health checks automatiques (Prometheus, Grafana, etc.)

---

### 5. Métriques par École

**Endpoint** : `GET /api/Metrics/ecole`

**Authentification** : Requise (JWT Token)

**Paramètres de Requête (Optionnels)** :

| Paramètre | Type | Description | Par défaut |
|-----------|------|-------------|------------|
| `idEcole` | int | ID de l'école | École de l'utilisateur connecté |

**Description** : Si `idEcole` n'est pas fourni, l'école de l'utilisateur connecté est utilisée automatiquement.

**Réponse** :
```json
{
  "timestamp": "2025-01-16T10:30:00Z",
  "ecole": {
    "idEcole": 5,
    "nom": "École Primaire ABC"
  },
  "statistiques": {
    "nombreEleves": 500,
    "nombreAgents": 30,
    "nombreClasses": 20
  },
  "paiementsMois": {
    "nombre": 150,
    "montant": 15000.00
  },
  "inscriptionsAnnee": 120
}
```

---

### 6. Métriques Complètes (Super-Admin)

**Endpoint** : `GET /api/Metrics/complete`

**Authentification** : Requise (JWT Token)

**Autorisation** : Super-Admin uniquement

**Description** : Retourne toutes les métriques combinées (générales, financières, académiques, santé)

**Réponse** :
```json
{
  "timestamp": "2025-01-16T10:30:00Z",
  "general": {
    // ... métriques générales
  },
  "financial": {
    // ... métriques financières
  },
  "academic": {
    // ... métriques académiques
  },
  "health": {
    // ... métriques de santé
  }
}
```

---

## 💡 Exemples d'Utilisation

### Exemple 1 : Health Check Automatique (JavaScript)

```javascript
async function checkHealth() {
    try {
        const response = await fetch('/api/Metrics/health');
        const health = await response.json();
        
        if (health.status === 'Healthy') {
            console.log('✅ Système sain');
            console.log(`Base de données: ${health.database.responseTimeMs}ms`);
            console.log(`Mémoire: ${health.system.memoryUsageMB}MB`);
        } else {
            console.error('❌ Système en panne');
        }
    } catch (error) {
        console.error('Erreur lors du health check:', error);
    }
}

// Vérifier toutes les 30 secondes
setInterval(checkHealth, 30000);
```

---

### Exemple 2 : Récupération des Métriques Générales (avec authentification)

```javascript
async function getGeneralMetrics() {
    const token = localStorage.getItem('token');
    
    const response = await fetch('/api/Metrics/general', {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });
    
    if (response.ok) {
        const metrics = await response.json();
        console.log(`Total élèves: ${metrics.eleves.total}`);
        console.log(`Élèves actifs: ${metrics.eleves.actifs}`);
        console.log(`Total écoles: ${metrics.ecoles.total}`);
    } else {
        console.error('Erreur:', response.statusText);
    }
}
```

---

### Exemple 3 : Métriques Financières avec Période Personnalisée

```javascript
async function getFinancialMetrics(dateDebut, dateFin) {
    const token = localStorage.getItem('token');
    
    const url = new URL('/api/Metrics/financial', window.location.origin);
    url.searchParams.append('dateDebut', dateDebut.toISOString());
    url.searchParams.append('dateFin', dateFin.toISOString());
    
    const response = await fetch(url, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });
    
    if (response.ok) {
        const metrics = await response.json();
        console.log(`Total paiements: ${metrics.paiements.total}`);
        console.log(`Montant total: ${metrics.paiements.montantTotal}`);
        console.log(`Montant moyen: ${metrics.paiements.montantMoyen}`);
        
        // Afficher la répartition par devise
        metrics.paiements.repartitionDevise.forEach(devise => {
            console.log(`${devise.devise}: ${devise.montant} (${devise.pourcentage}%)`);
        });
    }
}
```

---

### Exemple 4 : Monitoring avec cURL

```bash
# Health check (sans authentification)
curl -X GET https://votre-api.com/api/Metrics/health

# Métriques générales (avec authentification)
curl -X GET https://votre-api.com/api/Metrics/general \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"

# Métriques financières avec période
curl -X GET "https://votre-api.com/api/Metrics/financial?dateDebut=2025-01-01&dateFin=2025-01-31" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

---

## 🔗 Intégration avec des Outils de Monitoring

### Prometheus

**Configuration** : Créez un exporter Prometheus qui appelle périodiquement les endpoints Metrics.

**Exemple de configuration** (`prometheus.yml`) :
```yaml
scrape_configs:
  - job_name: 'kelasinabiso-api'
    scrape_interval: 30s
    metrics_path: '/api/Metrics/health'
    static_configs:
      - targets: ['votre-api.com:7102']
```

**Exporter personnalisé** :
```csharp
// Exemple d'exporter Prometheus
public class PrometheusExporter
{
    private readonly HttpClient _httpClient;
    
    public async Task<PrometheusMetrics> ExportAsync()
    {
        var health = await _httpClient.GetAsync("/api/Metrics/health");
        var general = await _httpClient.GetAsync("/api/Metrics/general");
        
        // Convertir en format Prometheus
        return new PrometheusMetrics
        {
            // Format Prometheus
        };
    }
}
```

---

### Grafana

**Dashboard** : Créez un dashboard Grafana qui affiche les métriques.

**Requêtes recommandées** :
- Health check toutes les 30 secondes
- Métriques générales toutes les 5 minutes
- Métriques financières toutes les heures
- Métriques académiques quotidiennes

---

### Application Insights / Azure Monitor

**Intégration** : Utilisez les endpoints Metrics pour envoyer des données à Application Insights.

**Exemple** :
```csharp
// Dans votre service de monitoring
public async Task SendToApplicationInsights()
{
    var metrics = await GetCompleteMetricsAsync();
    
    _telemetryClient.TrackMetric("TotalEleves", metrics.General.Eleves.Total);
    _telemetryClient.TrackMetric("TotalEcoles", metrics.General.Ecoles.Total);
    _telemetryClient.TrackMetric("DatabaseResponseTime", metrics.Health.Database.ResponseTimeMs);
}
```

---

## 📊 Cas d'Usage

### Cas 1 : Monitoring en Temps Réel

**Scénario** : Surveiller l'état de santé de l'API en temps réel

**Solution** :
- Appeler `/api/Metrics/health` toutes les 30 secondes
- Alerter si `status !== "Healthy"` ou si `database.responseTimeMs > 1000`

---

### Cas 2 : Tableau de Bord Administrateur

**Scénario** : Afficher les statistiques générales dans un tableau de bord

**Solution** :
- Appeler `/api/Metrics/general` au chargement de la page
- Afficher les compteurs (écoles, élèves, utilisateurs, etc.)
- Rafraîchir toutes les 5 minutes

---

### Cas 3 : Rapport Financier Mensuel

**Scénario** : Générer un rapport financier pour le mois écoulé

**Solution** :
- Appeler `/api/Metrics/financial?dateDebut=2025-01-01&dateFin=2025-01-31`
- Extraire les données (montant total, répartition par devise/mode)
- Générer un PDF ou Excel

---

### Cas 4 : Alertes Automatiques

**Scénario** : Alerter si le système est en panne ou surchargé

**Solution** :
```javascript
async function checkAndAlert() {
    const health = await fetch('/api/Metrics/health').then(r => r.json());
    
    if (health.status !== 'Healthy') {
        // Envoyer une alerte (email, SMS, Slack, etc.)
        sendAlert('Système en panne !');
    }
    
    if (health.system.memoryUsageMB > 2048) {
        sendAlert('Utilisation mémoire élevée !');
    }
    
    if (health.database.responseTimeMs > 1000) {
        sendAlert('Base de données lente !');
    }
}
```

---

## 📌 Notes Importantes

1. **Performance** : Les endpoints Metrics peuvent être coûteux en ressources. Utilisez-les avec modération (pas plus d'une fois par minute pour les métriques générales).

2. **Cache** : Considérez l'implémentation d'un cache pour les métriques qui ne changent pas fréquemment (ex: nombre total d'écoles).

3. **Sécurité** : L'endpoint `/health` est public pour permettre les health checks, mais les autres endpoints nécessitent une authentification.

4. **Période par défaut** : Si vous ne spécifiez pas de période, les métriques financières et académiques utilisent des périodes par défaut (mois en cours, 12 derniers mois).

5. **Filtrage automatique** : L'endpoint `/ecole` filtre automatiquement par l'école de l'utilisateur connecté si `idEcole` n'est pas fourni.

---

## 🔗 Liens Utiles

- **Contrôleur** : `Controllers/MetricsController.cs`
- **Documentation API** : Swagger UI (`/swagger`)
- **Health Check** : `/api/Metrics/health`

---

**Version** : 1.0  
**Date de création** : 2025-01-16  
**Dernière mise à jour** : 2025-01-16
