# 📱 Guide complet de test des notifications push

**Date :** 2025-11-05  
**Prérequis :** ✅ Migration SQL exécutée

---

## 🎯 **3 façons de tester les notifications push**

---

## 🧪 **MÉTHODE 1 : Test manuel via Swagger (SIMPLE)** ⭐

### **Étape 1 : Ouvrir Swagger**
```
https://localhost:7102/swagger
```

### **Étape 2 : Authentification**
1. Section **Utilisateur**
2. `POST /api/Utilisateur/authentifier`
3. Body :
   ```json
   {
     "emailOuTelephone": "+243999999999",
     "motDePasse": "Super-Admin"
   }
   ```
4. Copie le `token`
5. 🔓 **Authorize** → `Bearer {token}`

### **Étape 3 : Test notification**
1. Section **NotificationPush**
2. `POST /api/NotificationPush/test`
3. Body :
   ```json
   {
     "idUtilisateur": 223,
     "titre": "Test notification push",
     "corps": "Ceci est un test pour vérifier que Firebase fonctionne"
   }
   ```
4. **Execute**

### **✅ Résultat attendu :**

**Si l'utilisateur a un device actif :**
```json
{
  "success": true,
  "message": "Notification envoyée avec succès",
  "devicesNotifies": 1
}
```

**Si l'utilisateur n'a PAS de device :**
```json
{
  "success": false,
  "message": "Aucun appareil actif trouvé pour cet utilisateur"
}
```

### **📱 Si tu as un mobile connecté :**
→ Le mobile devrait recevoir la notification push immédiatement !

---

## 🧪 **MÉTHODE 2 : Test via une vraie action (RÉALISTE)** ⭐⭐

### **Scénario : Enregistrer une présence → Notification au tuteur**

### **Étape 1 : Préparer un compte parent mobile**

1. **Connecte-toi depuis l'app mobile** avec un compte Parent :
   ```json
   {
     "emailOuTelephone": "parent@test.com",
     "motDePasse": "Password123!",
     "fcmToken": "fKx7Y...Jz9M",  // ✅ Token FCM récupéré par l'app
     "deviceType": "Android"
   }
   ```

2. **Vérifie que le token est enregistré** :
   ```sql
   SELECT * FROM UserDevices
   WHERE IdUtilisateur = 223
     AND Statut = 1;
   ```

### **Étape 2 : Enregistrer une présence (depuis Swagger ou app)**

```http
POST /api/Presence
Body: {
  "idEleve": 45,
  "dateDuJour": "2025-11-05",
  "heureArrivee": "07:45:00",
  "isPresent": true,
  "typePresence": "ELEVE"
}
```

### **Étape 3 : Observer**

**📱 Le mobile du parent devrait recevoir :**
```
📍 Pointage de Jean Mukendi
✅ PRÉSENT le 05/11/2025 à 07:45
```

**📊 Logs attendus :**
```
✅ Notification PUSH Firebase envoyée au tuteur NomTuteur (User ID: 223)
```

---

## 🧪 **MÉTHODE 3 : Test direct Firebase (TECHNIQUE)** ⭐⭐⭐

### **Si tu veux tester SANS mobile**

Tu peux utiliser l'endpoint de test avec un **token FCM de test** généré par Firebase Console.

### **Étape 1 : Générer un token de test**

1. Va sur [Firebase Console](https://console.firebase.google.com/)
2. Sélectionne ton projet : `kelasinabiso-de502`
3. **Cloud Messaging** → **Test de notification**
4. Copie un token FCM de test

### **Étape 2 : Créer un UserDevice temporaire**

```sql
INSERT INTO UserDevices (IdUtilisateur, FcmToken, DeviceType, Statut, DateCreation)
VALUES (223, 'TOKEN_FCM_TEST_ICI', 'Android', 1, NOW());
```

### **Étape 3 : Tester via Swagger**

```http
POST /api/NotificationPush/test
Body: {
  "idUtilisateur": 223,
  "titre": "Test Firebase",
  "corps": "Test sans mobile réel"
}
```

### **Étape 4 : Vérifier dans Firebase Console**

Firebase Console → Cloud Messaging → **Statistiques**

Tu devrais voir :
- ✅ 1 notification envoyée
- ✅ Statut : Delivered ou Opened

---

## 📊 **Vérifications dans la base de données**

### **1. Vérifier les devices actifs**

```sql
SELECT 
    ud.IdDevice,
    ud.IdUtilisateur,
    u.NomUtilisateur,
    u.DefaultUsername,
    ud.FcmToken,
    ud.DeviceType,
    ud.Statut,
    ud.DateDerniereUtilisation
FROM UserDevices ud
INNER JOIN Utilisateurs u ON ud.IdUtilisateur = u.IdUtilisateur
WHERE ud.Statut = 1
ORDER BY ud.DateDerniereUtilisation DESC;
```

**✅ Si tu vois des résultats**, il y a des devices actifs prêts à recevoir des notifications.

---

### **2. Vérifier qu'un parent a un device**

```sql
SELECT 
    u.IdUtilisateur,
    u.DefaultUsername,
    u.IdTuteur,
    r.Nom AS Role,
    ud.FcmToken,
    ud.DeviceType,
    ud.Statut AS DeviceActif
FROM Utilisateurs u
INNER JOIN Roles r ON u.IdRole = r.IdRole
LEFT JOIN UserDevices ud ON u.IdUtilisateur = ud.IdUtilisateur
WHERE r.Nom = 'Parent'
  AND u.IdTuteur IS NOT NULL
  AND u.Statut = 1;
```

**✅ Si `FcmToken` est renseigné**, ce parent peut recevoir des notifications.

---

## 🔍 **Logs à surveiller**

### **Lors de l'envoi de notification (après une présence) :**

#### **✅ Succès :**
```
✅ Notification PUSH Firebase envoyée au tuteur Jean Kabila (User ID: 223) pour présence élève Marie Mukendi
```

#### **⚠️ Pas de device :**
```
⚠️ Aucun device actif trouvé pour l'utilisateur 223
📧 Fallback SMS envoyé à +243999999999
```

#### **❌ Erreur Firebase :**
```
❌ Erreur lors de l'envoi notification PUSH Firebase pour présence 123
```

---

## 🧪 **Test complet pas à pas**

### **Scénario complet : Parent reçoit notification de présence**

#### **1. Préparer les données (dans HeidiSQL)**

```sql
-- Vérifier qu'un parent a un device
SELECT u.IdUtilisateur, u.DefaultUsername, ud.FcmToken, ud.DeviceType
FROM Utilisateurs u
INNER JOIN Roles r ON u.IdRole = r.IdRole
LEFT JOIN UserDevices ud ON u.IdUtilisateur = ud.IdUtilisateur
WHERE r.Nom = 'Parent' AND u.IdTuteur IS NOT NULL
LIMIT 1;
```

**Note les valeurs :**
- `IdUtilisateur` : **223**
- `FcmToken` : **fKx7Y...** (si existe)
- `IdTuteur` : **25** (doit être renseigné)

#### **2. Vérifier l'élève du tuteur**

```sql
SELECT IdEleve, NomComplet, IdTuteur
FROM Eleves
WHERE IdTuteur = 25  -- Utilise l'IdTuteur du parent
  AND Statut = 1
LIMIT 1;
```

**Note :** `IdEleve` = **45**

#### **3. Enregistrer une présence (Swagger)**

```http
POST /api/Presence
Body: {
  "idEleve": 45,
  "dateDuJour": "2025-11-05",
  "heureArrivee": "07:45:00",
  "isPresent": true,
  "typePresence": "ELEVE"
}
```

#### **4. Observer les logs**

Tu devrais voir dans le terminal :
```
✅ Notification PUSH Firebase envoyée au tuteur ... (User ID: 223)
📧 SMS envoyé à +243999999999 (Fallback parallèle)
```

#### **5. Vérifier sur le mobile**

📱 Le mobile devrait afficher :
```
📍 Pointage de Jean Mukendi
✅ PRÉSENT le 05/11/2025 à 07:45
```

---

## 🚀 **Test rapide (recommandé)**

### **Si tu n'as PAS de mobile connecté :**

#### **Option A : Endpoint de test**
```http
POST /api/NotificationPush/test
Body: {
  "idUtilisateur": 223,
  "titre": "Test",
  "corps": "Test notification"
}
```

**Vérifie les logs :**
- ✅ `Notification envoyée` → Firebase fonctionne
- ⚠️ `Aucun device actif` → Pas de mobile connecté (normal)

---

#### **Option B : Vérifier Firebase Console**

1. Va sur [Firebase Console](https://console.firebase.google.com/)
2. Projet : `kelasinabiso-de502`
3. **Cloud Messaging**
4. Envoie une notification de test depuis la console
5. Copie un token FCM de test

---

## ✅ **Checklist de test**

### **Avant de tester :**
- [x] Migration SQL exécutée
- [x] Application redémarrée
- [ ] Swagger ouvert (https://localhost:7102/swagger)
- [ ] Authentifié (token JWT)

### **Test 1 : Endpoint de test**
- [ ] `POST /api/NotificationPush/test` avec `idUtilisateur=223`
- [ ] Vérifier logs : "Notification envoyée" ou "Aucun device"

### **Test 2 : Vérifier les devices en DB**
- [ ] `SELECT * FROM UserDevices WHERE Statut = 1`
- [ ] Au moins 1 device actif ?

### **Test 3 : Vraie notification (si mobile disponible)**
- [ ] Connecter mobile avec FCM Token
- [ ] Enregistrer une présence
- [ ] Mobile reçoit notification ?

---

## 🎯 **Résumé**

### **Test le plus simple (SANS mobile) :**
```http
POST /api/NotificationPush/test
Body: {"idUtilisateur": 223, "titre": "Test", "corps": "Test"}
```

**✅ Si Code 200** → Firebase fonctionne !  
**⚠️ Si "Aucun device"** → Normal si pas de mobile connecté

### **Test complet (AVEC mobile) :**
1. Connecte mobile avec FCM Token
2. Enregistre une présence
3. Mobile reçoit notification

---

**🚀 L'application est en train de redémarrer ! Dans quelques secondes tu pourras tester sur Swagger !** 

**Dis-moi quand tu es prêt et je te guide pour le test !** 😊

