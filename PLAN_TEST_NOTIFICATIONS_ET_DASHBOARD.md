# 🧪 Plan de test : Notifications Push + Dashboard Global

**Date :** 2025-11-06  
**Ordre :** 1️⃣ Notifications → 2️⃣ Dashboard

---

## 🔔 **TEST 1 : Notifications Push Firebase**

### **Objectif**
Vérifier que les notifications push fonctionnent correctement avec le nouveau fichier Firebase.

### **Prérequis**
- ✅ Fichier `firebase-credentiels.json` mis à jour (avec `private_key`)
- ✅ Application démarrée
- ⚠️ **Script SQL `APPLIQUER_MIGRATION_STATUT_NULLABLE.sql` NON EXÉCUTÉ**
  - **Risque** : `InvalidCastException` sur `Statut` et `HoraireIdHoraire`

### **Scénario de test**

#### **Contexte :**
- **Élève** : `IdEleve = 424` (Zozo machine mu tutu)
- **Tuteur** : kansa de kansa
- **User ID** : 382
- **Device actif** : Oui (vérifié précédemment)

#### **Action :**
```http
POST /api/Presence
Content-Type: application/json
Authorization: Bearer {token}

{
  "idEleve": 424,
  "dateDuJour": "2025-11-06",
  "heureArrivee": "08:30:00",
  "isPresent": true,
  "typePresence": "ELEVE",
  "observation": "Test notifications push avec nouveau Firebase"
}
```

#### **Résultats attendus :**

**✅ Si le script SQL a été exécuté :**
```
[INFO] ✅ Présence créée avec succès
[INFO] 1 token(s) actif(s) trouvé(s) pour utilisateur 382
[INFO] ✅ Notification PUSH Firebase envoyée au tuteur kansa de kansa (User ID: 382)
[INFO] Notification envoyée à l'utilisateur 382. Succès: 1/1
[INFO] ✅ Notification SignalR présence envoyée
[INFO] ✅ SMS envoyé avec succès
```

**📱 Mobile (kansadekansa678) reçoit :**
```
📍 Pointage de Zozo machine mu tutu
✅ PRÉSENT le 06/11/2025 à 08:30
📝 Test notifications push avec nouveau Firebase
```

**❌ Si le script SQL N'a PAS été exécuté :**
```
[ERR] An exception occurred while iterating over the results of a query
System.InvalidCastException: Unable to cast object of type 'System.DBNull' to type 'System.Int32'
at PresenceService.GetTodayPresenceAsync:line 441
```

**→ Solution :** Exécuter `APPLIQUER_MIGRATION_STATUT_NULLABLE.sql` dans HeidiSQL

---

## 📊 **TEST 2 : Dashboard Global**

### **Objectif**
Vérifier le nouveau format avec période mensuelle et statistiques générales.

### **Prérequis**
- ✅ Application démarrée
- ✅ Modifications appliquées (période mois + statistiques)

### **Scénario de test**

#### **Action :**
```http
GET /api/Dashboard/global?idEcole=18
Authorization: Bearer {token}
```

#### **Résultats attendus :**

**Structure de la réponse :**
```json
{
  "ecole": {
    "idEcole": 18,
    "nom": "...",
    "type": "..."
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
      "tauxPresence": 89.41
    },
    "resumeAgents": {
      "total": 28,
      "presents": 26,
      "absents": 2,
      "tauxPresence": 92.86
    },
    "alertes": [...],
    "classesProblematiques": [...],
    "agentsAbsents": [...]
  },
  "paiement": {
    "resume": {
      "montantAttendu": 15000.00,
      "montantRecouvert": 12000.00,
      "tauxRecouvrement": 80.00
    },
    "repartitionParMode": {...},
    "top5Frais": [...]
  }
}
```

**Points de vérification :**
- ✅ `periode.dateDebut` = 1er novembre 2025
- ✅ `periode.dateFin` = 30 novembre 2025
- ✅ `periode.libelle` = "novembre 2025"
- ✅ `statistiques` présent avec 6 propriétés
- ✅ `statistiques.nombreClasses` > 0
- ✅ `statistiques.nombreEleves` > 0
- ✅ `statistiques.nombreEnseignants` > 0
- ✅ Données de présence sur le mois (pas juste un jour)
- ✅ Données de paiement sur le mois

---

## 📝 **Checklist de test**

### **Notifications Push**
- [ ] Application démarrée sans erreur
- [ ] Log Firebase : `✅ Firebase Admin SDK initialisé avec succès`
- [ ] Créer présence élève 424
- [ ] Vérifier logs : pas d'`InvalidCastException`
- [ ] Vérifier logs : `✅ Notification PUSH Firebase envoyée`
- [ ] Vérifier logs : `Notification envoyée à l'utilisateur 382. Succès: 1/1`
- [ ] Vérifier mobile : notification reçue

### **Dashboard Global**
- [ ] Accéder à Swagger UI : `https://localhost:7102/swagger`
- [ ] S'authentifier avec un token
- [ ] Tester `GET /api/Dashboard/global?idEcole=18`
- [ ] Vérifier période : mois complet
- [ ] Vérifier `statistiques` présent
- [ ] Vérifier `statistiques.nombreClasses` a une valeur
- [ ] Vérifier `statistiques.nombreEleves` a une valeur
- [ ] Vérifier `statistiques.nombreEnseignants` a une valeur
- [ ] Vérifier `presence` a des données mensuelles
- [ ] Vérifier `paiement` a des données mensuelles

---

## ⚠️ **Problèmes connus**

### **1. InvalidCastException**
**Cause :** Script SQL `APPLIQUER_MIGRATION_STATUT_NULLABLE.sql` non exécuté  
**Solution :** Exécuter le script dans HeidiSQL avant de tester

### **2. Firebase non initialisé**
**Cause :** Ancien fichier `firebase-credentiels.json` incomplet  
**Status :** ✅ Corrigé (nouveau fichier téléchargé)

### **3. ObjectDisposedException**
**Cause :** `DbContext` disposé dans `Task.Run`  
**Status :** ✅ Corrigé (`IServiceScopeFactory` implémenté)

---

## 🎯 **Actions post-test**

Si les tests échouent :
1. Vérifier les logs de la console
2. Identifier l'erreur exacte
3. Appliquer les corrections nécessaires
4. Redémarrer l'API
5. Retester

Si les tests réussissent :
1. ✅ Marquer les notifications push comme fonctionnelles
2. ✅ Marquer le dashboard global comme fonctionnel
3. 📝 Documenter pour le frontend
4. 🚀 Déployer en production (après tests complets)

---

**Attends que l'application démarre et ouvre Swagger UI pour commencer les tests ! 🚀**

