# 📊 Logs attendus - Notifications Push

**Date :** 2025-11-05  
**Devices actifs :** ✅ 3 (User IDs: 223, 381, 382)

---

## 🔍 **Logs générés lors de l'envoi de notifications**

### **📍 SCÉNARIO 1 : Notification PRÉSENCE**

Quand tu enregistres une présence (`POST /api/Presence`), voici les logs attendus :

---

#### **✅ CAS 1 : Succès complet (device actif)**

```
[INFO] ✅ Notification PUSH Firebase envoyée au tuteur Jean Kabila (User ID: 223) pour présence élève Marie Mukendi
[INFO] Notification envoyée à l'utilisateur 223. Succès: 1/1
[INFO] ✅ Notification SignalR présence envoyée au tuteur Jean Kabila
[INFO] 📧 SMS envoyé à +243999999999 avec succès (SID: SM...)
```

**Explication :**
1. ✅ Notification **PUSH Firebase** envoyée avec succès
2. ✅ **1/1** = 1 token actif, 1 notification réussie
3. ✅ Notification **SignalR** envoyée (temps réel web)
4. ✅ **SMS** envoyé en parallèle (fallback)

---

#### **⚠️ CAS 2 : Aucun device actif**

```
[WARN] Aucun token FCM actif trouvé pour l'utilisateur 223
[WARN] ⚠️ Échec notification PUSH Firebase pour tuteur Jean Kabila (aucun device actif ou erreur Firebase)
[INFO] 📧 SMS envoyé à +243999999999 avec succès (SID: SM...)
```

**Explication :**
1. ⚠️ Pas de device → Notification push **non envoyée**
2. ✅ SMS envoyé à la place (fallback)

---

#### **❌ CAS 3 : Erreur Firebase**

```
[ERR] ❌ Erreur lors de l'envoi notification PUSH Firebase pour présence 456
System.Exception: ...
[INFO] 📧 SMS envoyé à +243999999999 avec succès (SID: SM...)
```

**Explication :**
1. ❌ Erreur Firebase (credentials, token invalide, etc.)
2. ✅ SMS envoyé quand même (fallback)

---

### **💰 SCÉNARIO 2 : Notification PAIEMENT**

Quand tu enregistres un paiement (`POST /api/Paiement`), logs attendus :

---

#### **✅ CAS 1 : Succès complet**

```
[INFO] ✅ Notification PUSH Firebase envoyée au tuteur Jean Kabila (User ID: 223) pour paiement élève Marie Mukendi
[INFO] Notification envoyée à l'utilisateur 223. Succès: 1/1
[INFO] ✅ Notification SignalR paiement envoyée au tuteur Jean Kabila
[INFO] 📧 SMS paiement envoyé à +243999999999 avec succès
```

---

#### **⚠️ CAS 2 : Aucun device**

```
[WARN] Aucun token FCM actif trouvé pour l'utilisateur 223
[WARN] ⚠️ Échec notification PUSH Firebase pour paiement
[INFO] 📧 SMS paiement envoyé à +243999999999
```

---

## 📋 **Tous les logs possibles (liste complète)**

### **Logs de Firebase (FirebaseNotificationService.cs)**

| Niveau | Message | Signification |
|--------|---------|---------------|
| **[INFO]** | `Notification envoyée à l'utilisateur {id}. Succès: 1/1` | ✅ Notification envoyée avec succès |
| **[INFO]** | `Notification envoyée. Succès: 3/3` | ✅ 3 notifications envoyées (multi-devices) |
| **[WARN]** | `Aucun token FCM actif trouvé pour l'utilisateur {id}` | ⚠️ Pas de device actif |
| **[WARN]** | `Token FCM invalide désactivé: {token}` | ⚠️ Token expiré/invalide |
| **[ERR]** | `Erreur lors de l'envoi de notification à l'utilisateur {id}` | ❌ Erreur Firebase |

---

### **Logs de Présence (PresenceService.cs)**

| Niveau | Message | Signification |
|--------|---------|---------------|
| **[INFO]** | `✅ Notification PUSH Firebase envoyée au tuteur {nom} (User ID: {id})` | ✅ Push envoyé |
| **[WARN]** | `⚠️ Échec notification PUSH Firebase pour tuteur {nom}` | ⚠️ Échec (pas de device) |
| **[ERR]** | `❌ Erreur lors de l'envoi notification PUSH Firebase pour présence {id}` | ❌ Exception |
| **[INFO]** | `✅ Notification SignalR présence envoyée au tuteur {nom}` | ✅ SignalR envoyé |
| **[INFO]** | `📧 SMS envoyé à {tel} avec succès (SID: {sid})` | ✅ SMS envoyé |

---

### **Logs de Paiement (PaiementService.cs)**

| Niveau | Message | Signification |
|--------|---------|---------------|
| **[INFO]** | `✅ Notification PUSH Firebase envoyée au tuteur {nom} pour paiement` | ✅ Push envoyé |
| **[WARN]** | `⚠️ Échec notification PUSH Firebase pour paiement` | ⚠️ Échec |
| **[ERR]** | `❌ Erreur lors de l'envoi notification PUSH Firebase pour paiement {id}` | ❌ Exception |
| **[INFO]** | `📧 SMS paiement envoyé à {tel}` | ✅ SMS envoyé |

---

### **Logs de base (UserDeviceRepository)**

| Niveau | Message | Signification |
|--------|---------|---------------|
| **[INFO]** | `Récupération des tokens actifs pour utilisateur {id}` | Recherche de devices |
| **[INFO]** | `{count} token(s) actif(s) trouvé(s) pour utilisateur {id}` | Devices trouvés |

---

## 🔍 **Où chercher dans les logs**

### **Filtre les logs par mot-clé :**

```powershell
# Chercher les notifications Firebase
Get-Content .\logs\kelasinabiso-*.log | Select-String "Firebase|PUSH"

# Chercher les succès
Get-Content .\logs\kelasinabiso-*.log | Select-String "✅ Notification"

# Chercher les échecs
Get-Content .\logs\kelasinabiso-*.log | Select-String "⚠️|❌" | Select-String "notification"
```

---

## 📊 **Exemple de logs COMPLETS après test**

### **Quand tu enregistres une PRÉSENCE :**

```
[17:55:30 INF] 📍 Création d'une présence pour élève ID: 82
[17:55:30 INF] ✅ Présence créée avec succès (ID: 456)
[17:55:30 INF] 🔔 Envoi notification au tuteur...
[17:55:30 INF] Récupération des tokens actifs pour utilisateur 223
[17:55:30 INF] 1 token(s) actif(s) trouvé(s) pour utilisateur 223
[17:55:31 INF] Notification envoyée à l'utilisateur 223. Succès: 1/1
[17:55:31 INF] ✅ Notification PUSH Firebase envoyée au tuteur KANSA (User ID: 223) pour présence élève Marie Mukendi
[17:55:31 INF] ✅ Notification SignalR présence envoyée au tuteur KANSA
[17:55:32 INF] 📧 SMS envoyé à +243999999999 avec succès (SID: SM12345...)
```

---

### **Quand tu enregistres un PAIEMENT :**

```
[17:56:15 INF] 💰 Création d'un paiement pour élève ID: 82
[17:56:15 INF] ✅ Paiement créé avec succès (ID: 789)
[17:56:15 INF] 🔔 Envoi notification au tuteur...
[17:56:15 INF] Récupération des tokens actifs pour utilisateur 223
[17:56:15 INF] 1 token(s) actif(s) trouvé(s) pour utilisateur 223
[17:56:16 INF] Notification envoyée à l'utilisateur 223. Succès: 1/1
[17:56:16 INF] ✅ Notification PUSH Firebase envoyée au tuteur KANSA pour paiement élève Marie Mukendi
[17:56:16 INF] ✅ Notification SignalR paiement envoyée au tuteur KANSA
[17:56:17 INF] 📧 SMS paiement envoyé à +243999999999 avec succès
```

---

## 🎯 **Que chercher dans tes logs**

### **Après avoir testé dans Swagger, cherche :**

#### **✅ Succès Firebase :**
```
✅ Notification PUSH Firebase envoyée
Succès: 1/1
```

#### **⚠️ Pas de device (normal) :**
```
Aucun token FCM actif trouvé
⚠️ Échec notification PUSH Firebase
```

#### **❌ Erreur :**
```
❌ Erreur lors de l'envoi notification PUSH Firebase
```

---

## 🧪 **Pour voir les logs EN DIRECT**

### **Dans le terminal actuel :**

Les logs s'affichent **automatiquement** en temps réel quand tu exécutes les tests dans Swagger.

**Après avoir cliqué "Execute" dans Swagger** :
1. Regarde immédiatement le terminal
2. Tu devrais voir les logs défiler en quelques secondes
3. Cherche les messages avec ✅ ou ⚠️

---

## 📱 **Résumé**

### **OUI, les notifications push génèrent BEAUCOUP de logs !**

**Logs pour chaque notification :**
1. ✅ **Succès** : `✅ Notification PUSH Firebase envoyée`
2. ⚠️ **Pas de device** : `Aucun token FCM actif trouvé`
3. ✅ **SignalR** : `✅ Notification SignalR envoyée`
4. ✅ **SMS** : `📧 SMS envoyé avec succès`
5. ❌ **Erreur** : `❌ Erreur lors de l'envoi notification`

---

**Maintenant, teste dans Swagger et surveille les logs du terminal ! Tu devrais voir les messages s'afficher immédiatement !** 🔍

**Dis-moi ce que tu vois dans les logs après le test !** 😊
