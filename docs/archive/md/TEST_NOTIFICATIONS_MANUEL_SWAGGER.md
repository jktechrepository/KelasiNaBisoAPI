# 🧪 Test manuel Notifications Push - Swagger UI

**Date :** 2025-11-05  
**Application :** ✅ Démarrée sur https://localhost:7102

---

## 🎯 **Tests à effectuer**

### **✅ PRÉREQUIS**
- Application démarrée : ✅
- Migration SQL exécutée : ✅
- Swagger UI : https://localhost:7102/swagger

---

## 📋 **TEST 1 : Notification PRÉSENCE**

### **Étape 1 : Authentification**

1. Ouvre Swagger : `https://localhost:7102/swagger`
2. Section **Utilisateur**
3. `POST /api/Utilisateur/authentifier`
4. Try it out
5. Body :
   ```json
   {
     "emailOuTelephone": "+243999999999",
     "motDePasse": "Super-Admin"
   }
   ```
6. Execute
7. Copie le `token`
8. 🔓 **Authorize** → `Bearer {token}` → Authorize → Close

---

### **Étape 2 : Enregistrer une présence**

1. Section **Presence**
2. `POST /api/Presence`
3. Try it out
4. Body :
   ```json
   {
     "idEleve": 82,
     "dateDuJour": "2025-11-05",
     "heureArrivee": "07:45:00",
     "isPresent": true,
     "typePresence": "ELEVE",
     "observation": "Test notification push"
   }
   ```
5. **Execute**

---

### **Étape 3 : Observer les résultats**

#### **A. Dans Swagger (réponse) :**
```json
{
  "idPresence": 456,
  "dateDuJour": "2025-11-05",
  "heureArrivee": "07:45:00",
  "isPresent": true
}
```
✅ **Code 200** = Présence créée avec succès

---

#### **B. Dans le terminal (logs) :**

Cherche dans les logs du terminal :

**✅ Succès (si tuteur a un device) :**
```
✅ Notification PUSH Firebase envoyée au tuteur ... (User ID: 223) pour présence élève ...
✅ Notification SignalR présence envoyée au tuteur ...
📧 SMS envoyé à +243... (envoi en parallèle)
```

**⚠️ Pas de device (normal si pas de mobile connecté) :**
```
⚠️ Aucun device actif trouvé pour l'utilisateur 223
📧 SMS envoyé à +243... (fallback)
```

---

#### **C. Sur le mobile du tuteur (si connecté) :**

Le mobile devrait recevoir :
```
📍 Pointage de [Nom Élève]
✅ PRÉSENT le 05/11/2025 à 07:45
📝 Test notification push
```

---

## 📋 **TEST 2 : Notification PAIEMENT**

### **Étape 1 : Vérifier les frais disponibles**

1. Section **Frais**
2. `GET /api/Frais/paged`
3. PageNumber = 1, PageSize = 10
4. Execute
5. **Note un `idFrais`** (exemple : 39)

---

### **Étape 2 : Enregistrer un paiement**

1. Section **Paiement**
2. `POST /api/Paiement`
3. Try it out
4. Body :
   ```json
   {
     "idEleve": 82,
     "idFrais": 39,
     "montant": 50.00,
     "devise": "USD",
     "datePaiement": "2025-11-05T10:30:00",
     "modePaiement": "Mobile Money",
     "statutPaiement": "Payé",
     "commentaire": "Test notification push paiement"
   }
   ```
5. **Execute**

---

### **Étape 3 : Observer les résultats**

#### **A. Dans Swagger (réponse) :**
```json
{
  "idPaiement": 789,
  "montant": 50.00,
  "devise": "USD",
  "statutPaiement": "Payé"
}
```
✅ **Code 200** = Paiement créé avec succès

---

#### **B. Dans le terminal (logs) :**

**✅ Succès :**
```
✅ Notification PUSH Firebase envoyée au tuteur ... pour paiement élève ...
✅ Notification SignalR paiement envoyée au tuteur ...
📧 SMS paiement envoyé à +243...
```

**⚠️ Pas de device :**
```
⚠️ Aucun device actif trouvé pour l'utilisateur 223
📧 SMS envoyé à +243... (fallback)
```

---

#### **C. Sur le mobile du tuteur (si connecté) :**

Le mobile devrait recevoir :
```
💰 Paiement enregistré
Paiement de 50.00 USD reçu pour [Nom Élève] - [Nom Frais]
```

---

## 🔍 **Vérification dans les logs**

### **Messages clés à chercher :**

#### **✅ Firebase fonctionne :**
```
✅ Notification PUSH Firebase envoyée au tuteur
```

#### **⚠️ Pas de mobile (normal) :**
```
⚠️ Aucun device actif trouvé pour l'utilisateur
```

#### **✅ SMS envoyé (fallback) :**
```
📧 SMS envoyé à +243999999999
✅ SMS envoyé avec succès (SID: SM...)
```

#### **❌ Erreur Firebase :**
```
❌ Erreur lors de l'envoi notification PUSH Firebase
```

---

## 📊 **Interprétation des résultats**

| Résultat dans logs | Signification | Action |
|-------------------|---------------|--------|
| ✅ Notification PUSH envoyée | **Firebase fonctionne !** 📱 | Vérifier le mobile |
| ⚠️ Aucun device actif | Pas de mobile connecté | Normal si pas d'app mobile |
| ✅ SMS envoyé | Fallback fonctionne | Vérifier SMS reçu |
| ❌ Erreur Firebase | Problème credentials | Vérifier firebase-credentials.json |

---

## 🎯 **Résumé rapide**

### **Pour tester :**

1. **Ouvre Swagger** : https://localhost:7102/swagger
2. **Authentifie-toi** (Super-Admin)
3. **Teste Présence** : `POST /api/Presence` avec body ci-dessus
4. **Teste Paiement** : `POST /api/Paiement` avec body ci-dessus
5. **Regarde les logs** du terminal

---

### **Résultats attendus :**

**✅ Si mobile connecté :**
- 📱 2 notifications push reçues
- 📧 2 SMS reçus (en parallèle)
- ✅ Logs : "Notification PUSH Firebase envoyée"

**⚠️ Si pas de mobile :**
- 📧 2 SMS reçus uniquement
- ⚠️ Logs : "Aucun device actif trouvé"
- ✅ **Firebase fonctionne quand même** (juste pas de destinataire)

---

**Vas-y, teste maintenant dans Swagger et dis-moi ce que tu vois dans les logs !** 🚀

