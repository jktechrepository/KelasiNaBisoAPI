# 🧪 Test des endpoints Dashboard améliorés

**Date :** 2025-11-05  
**Serveur :** ✅ Démarré sur https://localhost:7102

---

## ✅ Serveur opérationnel

```
✅ KelasiNaBisoAPI démarré et prêt à recevoir des requêtes
📊 Environnement : Development
🔗 Swagger UI : https://localhost:7102/swagger
```

---

## 🧪 Tests à effectuer

### **Test 1 : Dashboard Présence (jour spécifique)**

**Endpoint :**
```http
GET https://localhost:7102/api/Presence/dashboard/ecole/18?date=2025-11-05
```

**En-têtes :**
```
Authorization: Bearer {votre_token_jwt}
Accept: application/json
```

**Résultats attendus :**
- ✅ `absents` = nombre réel (pas calculé théoriquement)
- ✅ `alertes` : tableau avec alertes si taux < 85%
- ✅ `classesProblematiques` : classes avec taux < 75%
- ✅ `agentsAbsents` : agents absents avec cours affectés
- ✅ Temps de réponse ~800ms (1er appel)
- ✅ Temps de réponse ~50ms (2ème appel - cache)

---

### **Test 2 : Dashboard Présence (période)**

**Endpoint :**
```http
GET https://localhost:7102/api/Presence/dashboard/ecole/18?dateDebut=2025-11-01&dateFin=2025-11-05
```

**Résultats attendus :**
- ✅ Statistiques agrégées sur 5 jours
- ✅ Alertes basées sur les seuils
- ✅ Classes problématiques identifiées

---

### **Test 3 : Dashboard Paiement (mois)**

**Endpoint :**
```http
GET https://localhost:7102/api/Paiement/dashboard/ecole/18?periode=mois
```

**Résultats attendus :**
- ✅ `montantTotal` : somme des paiements du mois
- ✅ `tauxRecouvrement` : (montantTotal / montantAttendu) * 100
- ✅ `repartitionParMode` : Espèces, Mobile Money, etc.
- ✅ `top5Frais` : Top 5 des frais payés
- ✅ Cache actif (2ème appel plus rapide)

---

### **Test 4 : Vérification du cache**

**Procédure :**
1. **1er appel** : Dashboard Présence
   - Observer les logs : `❌ Cache MISS : dashboard_presence_18_20251105_20251105`
   - Temps de réponse : ~800ms

2. **2ème appel** (immédiat) : Même dashboard
   - Observer les logs : `✅ Cache HIT : dashboard_presence_18_20251105_20251105`
   - Temps de réponse : ~50ms ✅

3. **Attendre 6 minutes** : Cache expiré
   - 3ème appel : Cache MISS à nouveau
   - Cache reconstruit

---

## 📊 Exemple de réponse attendue

### **Dashboard Présence**

```json
{
  "ecole": {
    "idEcole": 18,
    "nomEcole": "Ekelasi School",
    "logo": "https://..."
  },
  "periode": {
    "type": "jour",
    "date": "2025-11-05",
    "dateDebut": "2025-11-05",
    "dateFin": "2025-11-05",
    "joursOuvrables": 1,
    "libelle": "mardi 05 novembre 2025"
  },
  "resumeEleves": {
    "effectifTotal": 1050,
    "presents": 950,
    "absents": 25,        // ✅ Corrigé (seulement absences enregistrées)
    "retards": 35,
    "tauxPresence": 90.48,
    "tauxAbsence": 2.38,
    "tauxRetard": 3.33
  },
  "resumeAgents": {
    "effectifTotal": 30,
    "presents": 28,
    "absents": 1,         // ✅ Corrigé
    "retards": 1,
    "tauxPresence": 93.33,
    "tauxAbsence": 3.33,
    "tauxRetard": 3.33
  },
  "alertes": [            // ✅ NOUVEAU
    {
      "type": "warning",
      "message": "⚡ Taux de présence élèves sous la normale : 82%",
      "action": "Surveiller les absences répétées"
    },
    {
      "type": "warning",
      "message": "📚 2 agent(s) absent(s) avec cours affectés",
      "action": "Organiser des remplacements"
    }
  ],
  "classesProblematiques": [  // ✅ NOUVEAU
    {
      "idClasse": 12,
      "nomClasse": "5ème B",
      "presents": 18,
      "absents": 10,
      "tauxPresence": 64.29,
      "status": "attention"
    },
    {
      "idClasse": 8,
      "nomClasse": "3ème A",
      "presents": 12,
      "absents": 15,
      "tauxPresence": 44.44,
      "status": "critique"
    }
  ],
  "agentsAbsents": [            // ✅ NOUVEAU
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

---

## 🔍 Logs à surveiller

### **Dans la console :**

#### **Cache MISS (1er appel)**
```
[DEBUG] ❌ Cache MISS : dashboard_presence_18_20251105_20251105 - Exécution de la requête
[INFO] Executed DbCommand (45ms) [...]
[INFO] Executed DbCommand (38ms) [...]
```

#### **Cache HIT (appels suivants)**
```
[DEBUG] ✅ Cache HIT : dashboard_presence_18_20251105_20251105
```

---

## 🎯 Checklist de validation

### **Fonctionnalités corrigées :**
- [ ] **Absences** comptées correctement (pas sur-estimées)
- [ ] **Cache** fonctionne (2ème appel rapide)
- [ ] **Alertes** générées automatiquement
- [ ] **Classes problématiques** identifiées
- [ ] **Agents absents** détectés avec cours affectés

### **Seuils d'alerte :**
- [ ] Alerte si taux présence élèves < 70% (danger)
- [ ] Alerte si taux présence élèves < 85% (warning)
- [ ] Alerte si taux présence agents < 80% (danger)
- [ ] Alerte si classes avec taux < 60% (critique)
- [ ] Alerte si agents absents avec cours (warning)
- [ ] Alerte si taux retard > 15% (info)

### **Performance :**
- [ ] 1er appel : ~800ms
- [ ] 2ème appel : ~50ms (amélioration 94%)
- [ ] Logs cache visibles dans la console
- [ ] Cache expire après 5 minutes

---

## 📱 Test via Swagger UI

**URL :** https://localhost:7102/swagger

1. **Authentification :**
   - Endpoint : `POST /api/Authentification/login`
   - Body :
     ```json
     {
       "telephone": "+243999999999",
       "motDePasse": "Super-Admin"
     }
     ```
   - Copier le `token` de la réponse

2. **Autoriser Swagger :**
   - Cliquer sur le bouton 🔓 **Authorize** (en haut)
   - Entrer : `Bearer {votre_token}`
   - Cliquer **Authorize**

3. **Tester Dashboard Présence :**
   - Section : **Presence**
   - Endpoint : `GET /api/Presence/dashboard/ecole/{idEcole}`
   - Paramètres :
     - `idEcole` : 18
     - `date` : 2025-11-05
   - Cliquer **Execute**
   - Observer la réponse

4. **Tester Cache (2ème appel) :**
   - Cliquer à nouveau **Execute** immédiatement
   - Comparer le temps de réponse (devrait être ~94% plus rapide)
   - Vérifier les logs dans la console

5. **Tester Dashboard Paiement :**
   - Section : **Paiement**
   - Endpoint : `GET /api/Paiement/dashboard/ecole/{idEcole}`
   - Paramètres :
     - `idEcole` : 18
     - `periode` : mois
   - Cliquer **Execute**

---

## 🐛 Problèmes potentiels

### **1. Token expiré**
**Symptôme :** `401 Unauthorized`  
**Solution :** Se reconnecter via `/api/Authentification/login`

### **2. École sans données**
**Symptôme :** `alertes`, `classesProblematiques`, `agentsAbsents` = null  
**Normal si :** Pas de présences enregistrées ou tous les taux > seuils

### **3. Cache ne fonctionne pas**
**Symptôme :** Toujours "Cache MISS"  
**Vérification :** 
- Vérifier que `ICacheService` est enregistré dans `Program.cs`
- Vérifier les logs de debug

### **4. Erreur "IdClasse nullable"**
**Symptôme :** Erreur lors du calcul des classes problématiques  
**Statut :** ✅ Corrigé (ligne 687 : `IdClasse = g.Key.IdClasse ?? 0`)

---

## 📊 Résultats des tests

| Test | Endpoint | Statut | Temps (1er) | Temps (2ème) | Notes |
|------|----------|--------|-------------|--------------|-------|
| **Dashboard Présence (jour)** | `/api/Presence/dashboard/ecole/18?date=2025-11-05` | ⏳ | - | - | À tester |
| **Dashboard Présence (période)** | `/api/Presence/dashboard/ecole/18?dateDebut=...` | ⏳ | - | - | À tester |
| **Dashboard Paiement (mois)** | `/api/Paiement/dashboard/ecole/18?periode=mois` | ⏳ | - | - | À tester |
| **Cache (2ème appel)** | Même endpoint répété | ⏳ | - | - | À tester |

---

**🎯 Objectif :** Valider que les 3 Quick Wins fonctionnent correctement !

**✅ Serveur prêt à tester : https://localhost:7102/swagger**

