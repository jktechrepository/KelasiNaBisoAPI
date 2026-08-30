# 🧪 Guide de test - Dashboard Global

**Date :** 2025-11-06  
**Endpoint :** `GET /api/Dashboard/global?idEcole={id}`

---

## 🎯 **Objectif du test**

Vérifier que le nouveau format du Dashboard Global fonctionne correctement avec :
1. ✅ Période sur **un mois complet** (1er au dernier jour du mois)
2. ✅ Statistiques générales (Classes, Élèves, Enseignants, Directions)
3. ✅ Données de présence mensuelles
4. ✅ Données de paiement mensuelles

---

## 📋 **Prérequis**

- [x] Application démarrée
- [ ] Authentification avec un token JWT valide
- [ ] École de test : `idEcole = 18` (Ekelasi School)

---

## 🧪 **Étapes de test**

### **1. Ouvrir Swagger UI**

URL : `https://localhost:7102/swagger`

---

### **2. S'authentifier**

**Endpoint :** `POST /api/Utilisateur/authentifier`

```json
{
  "identifiant": "superadmin@kelasinabiso.cd",
  "motDePasse": "Super-Admin"
}
```

**Copier le `token` de la réponse.**

---

### **3. Autoriser dans Swagger**

1. Cliquer sur le bouton **🔓 Authorize** en haut à droite
2. Entrer : `Bearer {ton_token}`
3. Cliquer sur **Authorize**
4. Fermer la popup

---

### **4. Tester le Dashboard Global**

**Endpoint :** `GET /api/Dashboard/global`

**Paramètres :**
- `idEcole` : `18`

**Cliquer sur "Execute"**

---

## ✅ **Résultats attendus**

### **Structure de la réponse :**

```json
{
  "ecole": {
    "idEcole": 18,
    "nom": "Ekelasi School",
    "type": "...",
    "commune": "...",
    "ville": "..."
  },
  "periode": {
    "dateDebut": "2025-11-01T00:00:00",
    "dateFin": "2025-11-30T23:59:59",
    "libelle": "novembre 2025"
  },
  "statistiques": {
    "nombreDirections": 2,
    "nombreClasses": 15,
    "nombreEleves": 450,
    "nombreElevesActifs": 425,
    "nombreEnseignants": 30,
    "nombreEnseignantsActifs": 28
  },
  "presence": {
    "resumeEleves": {
      "total": 425,
      "presents": 380,
      "absents": 45,
      "tauxPresence": 89.41,
      "retards": 12
    },
    "resumeAgents": {
      "total": 28,
      "presents": 26,
      "absents": 2,
      "tauxPresence": 92.86,
      "retards": 1
    },
    "alertes": [
      {
        "type": "TAUX_FAIBLE",
        "niveau": "WARNING",
        "message": "...",
        "valeur": 89.41
      }
    ],
    "classesProblematiques": [...],
    "agentsAbsents": [...]
  },
  "paiement": {
    "resume": {
      "montantAttendu": 15000.00,
      "montantRecouvert": 12000.00,
      "montantEnAttente": 3000.00,
      "tauxRecouvrement": 80.00,
      "nombrePaiements": 320,
      "nombreElevesAyantPaye": 320,
      "nombreElevesEnAttente": 105
    },
    "repartitionParMode": {
      "especes": 8000.00,
      "mobileMoney": 3500.00,
      "virement": 500.00
    },
    "top5Frais": [...]
  }
}
```

---

## ✅ **Points de vérification**

### **Clé `periode` :**
- [ ] `dateDebut` = 2025-11-01T00:00:00
- [ ] `dateFin` = 2025-11-30T23:59:59
- [ ] `libelle` = "novembre 2025"

### **Clé `statistiques` (NOUVELLE) :**
- [ ] `nombreDirections` > 0
- [ ] `nombreClasses` > 0
- [ ] `nombreEleves` > 0
- [ ] `nombreElevesActifs` > 0
- [ ] `nombreEnseignants` > 0
- [ ] `nombreEnseignantsActifs` > 0

### **Clé `presence` :**
- [ ] `resumeEleves` avec données
- [ ] `resumeAgents` avec données
- [ ] `alertes` (peut être vide ou avec alertes)
- [ ] `classesProblematiques` (peut être vide)
- [ ] `agentsAbsents` (peut être vide)

### **Clé `paiement` :**
- [ ] `resume` avec montants
- [ ] `repartitionParMode` avec modes de paiement
- [ ] `top5Frais` avec top 5

---

## ❌ **Erreurs possibles**

### **Erreur 1 : InvalidCastException**

```
System.InvalidCastException: Unable to cast object of type 'System.DBNull' to type 'System.Int32'
```

**Cause :** Script SQL `APPLIQUER_MIGRATION_STATUT_NULLABLE.sql` non exécuté.

**Solution :** Ce n'est pas bloquant pour le Dashboard, mais à corriger plus tard.

---

### **Erreur 2 : 404 Not Found**

```json
{
  "message": "École non trouvée avec l'ID 18"
}
```

**Cause :** `idEcole` invalide ou école inexistante.

**Solution :** Vérifier l'ID de l'école dans la base de données.

---

### **Erreur 3 : 500 Internal Server Error**

**Cause :** Problème dans le calcul des statistiques ou des dashboards.

**Solution :** Vérifier les logs de l'API pour l'erreur exacte.

---

## 🎯 **Après le test**

Si le test réussit :
- ✅ Noter le format exact de la réponse
- ✅ Vérifier que toutes les clés sont présentes
- ✅ Documenter pour le frontend
- 🚀 Passer au test Firebase

Si le test échoue :
- 📝 Noter l'erreur exacte
- 🔍 Analyser les logs
- 🔧 Corriger et retester

---

**Attends que l'API démarre (tu verras `✅ KelasiNaBisoAPI démarré et prêt`) puis ouvre Swagger UI ! 🚀**

**URL Swagger :** `https://localhost:7102/swagger`

