# 🧪 GUIDE DE TEST : Device Info lors de l'Authentification

## ⚠️ IMPORTANT : Valeurs "string" vs Valeurs réelles

Le système **rejette automatiquement** les valeurs par défaut de Swagger :
- `"string"` → ❌ Rejeté
- `"null"` → ❌ Rejeté
- `""` (vide) → ❌ Rejeté

Vous **DEVEZ** utiliser des **valeurs réelles** pour que le device soit enregistré.

---

## ✅ TEST 1 : Authentification avec device Android (VALEURS RÉELLES)

### Request (Swagger ou HTTP file)

```http
POST https://localhost:7105/api/Utilisateur/authentifier
Content-Type: application/json

{
  "emailOuTelephone": "admin@kelasinabiso.cd",
  "motDePasse": "Admin",
  "fcmToken": "fKx7YzQ9mT3pNvB8sH2jL5wC1dX6rA4eG0uJ7iM9kP3qN8hT2vR5",
  "deviceType": "Android",
  "deviceModel": "Samsung Galaxy S21",
  "osVersion": "Android 12"
}
```

### ✅ Résultat attendu

**Response** :
```json
{
  "success": true,
  "message": "Authentification réussie",
  "accessToken": "eyJhbGci...",
  "tokenType": "Bearer",
  "expiresIn": 86400,
  ...
}
```

**Logs** :
```
info: ✅ Utilisateur trouvé via EMAIL: 2 - admin@kelasinabiso.cd
info: ✅ Authentification réussie pour l'utilisateur 2
info: ✅ Utilisateur 2 marqué comme connecté
info: ✅ Token FCM enregistré pour l'utilisateur 2 - Device: Android Samsung Galaxy S21
info: ✅ Token JWT généré avec succès pour l'utilisateur 2
```

**Base de données** :
```sql
SELECT * FROM UserDevices WHERE IdUtilisateur = 2;
```

**Résultat** :
```
IdUserDevice | IdUtilisateur | FcmToken                | DeviceType | DeviceModel         | OsVersion   | Statut
1            | 2             | fKx7YzQ9mT3pNvB8sH2j... | Android    | Samsung Galaxy S21  | Android 12  | 1
```

---

## ❌ TEST 2 : Authentification avec valeurs "string" (REJETÉES)

### Request (avec valeurs par défaut Swagger)

```http
POST https://localhost:7105/api/Utilisateur/authentifier
Content-Type: application/json

{
  "emailOuTelephone": "admin@kelasinabiso.cd",
  "motDePasse": "Admin",
  "fcmToken": "string",
  "deviceType": "string",
  "deviceModel": "string",
  "osVersion": "string"
}
```

### ⚠️ Résultat attendu

**Response** :
```json
{
  "success": true,
  "message": "Authentification réussie",
  "accessToken": "eyJhbGci...",
  ...
}
```

**Logs** :
```
info: ✅ Utilisateur trouvé via EMAIL: 2 - admin@kelasinabiso.cd
info: ✅ Authentification réussie pour l'utilisateur 2
info: ✅ Utilisateur 2 marqué comme connecté
warn: ⚠️ Token FCM ou DeviceType manquant/invalide pour l'utilisateur 2 - Token: 'string', Device: 'string'
info: ✅ Token JWT généré avec succès pour l'utilisateur 2
```

**Base de données** :
```sql
SELECT * FROM UserDevices WHERE IdUtilisateur = 2;
```

**Résultat** :
```
Aucune ligne (ou lignes précédentes uniquement)
```

---

## ✅ TEST 3 : Multi-device (Android + iOS)

### Étape 1 : Connexion Android

```json
{
  "emailOuTelephone": "admin@kelasinabiso.cd",
  "motDePasse": "Admin",
  "fcmToken": "ANDROID_TOKEN_ABC123XYZ",
  "deviceType": "Android",
  "deviceModel": "Samsung Galaxy S21",
  "osVersion": "Android 12"
}
```

**Résultat** :
- ✅ Device Android créé (IdUserDevice = 1)

---

### Étape 2 : Connexion iOS (même utilisateur)

```json
{
  "emailOuTelephone": "admin@kelasinabiso.cd",
  "motDePasse": "Admin",
  "fcmToken": "IOS_TOKEN_DEF456UVW",
  "deviceType": "iOS",
  "deviceModel": "iPhone 13",
  "osVersion": "iOS 15.2"
}
```

**Résultat** :
- ✅ Device iOS créé (IdUserDevice = 2)

---

### Étape 3 : Vérification

```sql
SELECT * FROM UserDevices WHERE IdUtilisateur = 2;
```

**Résultat attendu** :
```
IdUserDevice | IdUtilisateur | FcmToken           | DeviceType | DeviceModel         | OsVersion
1            | 2             | ANDROID_TOKEN_...  | Android    | Samsung Galaxy S21  | Android 12
2            | 2             | IOS_TOKEN_...      | iOS        | iPhone 13           | iOS 15.2
```

---

## ✅ TEST 4 : Mise à jour du token FCM (même device)

### Étape 1 : Première connexion Android

```json
{
  "emailOuTelephone": "admin@kelasinabiso.cd",
  "motDePasse": "Admin",
  "fcmToken": "OLD_ANDROID_TOKEN_123",
  "deviceType": "Android",
  "deviceModel": "Samsung Galaxy S21",
  "osVersion": "Android 12"
}
```

**Résultat** :
- ✅ Device Android créé

---

### Étape 2 : Deuxième connexion Android (nouveau token)

```json
{
  "emailOuTelephone": "admin@kelasinabiso.cd",
  "motDePasse": "Admin",
  "fcmToken": "NEW_ANDROID_TOKEN_789",
  "deviceType": "Android",
  "deviceModel": "Samsung Galaxy S21",
  "osVersion": "Android 12"
}
```

**Résultat** :
- ✅ Device Android **mis à jour** (FcmToken changé)
- ✅ DateDerniereUtilisation actualisée
- ✅ Toujours 1 seul device Android (pas de duplication)

---

### Étape 3 : Vérification

```sql
SELECT * FROM UserDevices WHERE IdUtilisateur = 2 AND DeviceType = 'Android';
```

**Résultat attendu** :
```
IdUserDevice | FcmToken                | DateDerniereUtilisation
1            | NEW_ANDROID_TOKEN_789   | 2025-10-25 12:30:00  (mis à jour)
```

---

## 🔍 Pourquoi les valeurs "string" sont rejetées ?

### Dans Swagger

Quand vous ouvrez Swagger, les champs ont des valeurs par défaut :
```json
{
  "fcmToken": "string",
  "deviceType": "string",
  "deviceModel": "string",
  "osVersion": "string"
}
```

Ce sont des **placeholders**, pas de vraies valeurs !

### Validation dans le code

Le controller vérifie d'abord :
```csharp
if (!string.IsNullOrEmpty(request.FcmToken) && 
    request.FcmToken != "string" && 
    request.FcmToken != "null" &&
    !string.IsNullOrEmpty(request.DeviceType) && 
    request.DeviceType != "string" && 
    request.DeviceType != "null")
{
    // Appeler CreateOrUpdateAsync
}
else
{
    // Warning : données invalides
}
```

Le service valide ensuite :
```csharp
if (string.IsNullOrWhiteSpace(fcmToken) || fcmToken == "string" || fcmToken == "null")
{
    throw new ArgumentException("FCM Token invalide", nameof(fcmToken));
}
```

---

## 📊 Tableau récapitulatif

| Valeur fcmToken | Valeur deviceType | Résultat |
|-----------------|-------------------|----------|
| `"string"` | `"string"` | ⚠️ Warning, pas d'insertion |
| `"null"` | `"Android"` | ⚠️ Warning, pas d'insertion |
| `""` | `"Android"` | ⚠️ Warning, pas d'insertion |
| `"fKx7Y..."` | `"string"` | ⚠️ Warning, pas d'insertion |
| `"fKx7Y..."` | `"Android"` | ✅ Insertion réussie |
| `"ANDROID_TOKEN_123"` | `"Android"` | ✅ Insertion réussie |
| null (absent) | `"Android"` | ⚠️ Warning, pas d'insertion |
| `"fKx7Y..."` | null (absent) | ⚠️ Warning, pas d'insertion |

---

## 🎯 Comment obtenir de vraies valeurs ?

### FcmToken
Dans une vraie application mobile (Android/iOS) :
```javascript
// Firebase SDK
const fcmToken = await messaging().getToken();
// Ex: "fKx7YzQ9mT3pNvB8sH2jL5wC1dX6rA4eG0uJ7iM9kP3qN8hT2vR5"
```

Pour les **tests** :
- Utilisez n'importe quelle chaîne de 20+ caractères
- Ex: `"ANDROID_TOKEN_123456789"`
- Ex: `"fKx7YzQ9mT3pNvB8sH2jL5wC1dX6rA4eG0uJ7iM9kP3qN8hT2vR5"`

### DeviceType
Valeurs possibles :
- `"Android"`
- `"iOS"`
- `"Web"`

### DeviceModel
Exemples :
- `"Samsung Galaxy S21"`
- `"iPhone 13"`
- `"Google Pixel 6"`
- `"OnePlus 9"`

### OsVersion
Exemples :
- `"Android 12"`
- `"iOS 15.2"`
- `"Windows 11"`

---

## ✅ Checklist de test

- [ ] Ouvrir Swagger : `https://localhost:7105/swagger`
- [ ] Aller à `POST /api/Utilisateur/authentifier`
- [ ] **Remplacer TOUTES les valeurs "string" par des valeurs réelles**
- [ ] Exécuter la requête
- [ ] Vérifier les logs → Doit afficher "✅ Token FCM enregistré"
- [ ] Vérifier la base de données → `SELECT * FROM UserDevices WHERE IdUtilisateur = 2`
- [ ] Tester avec un deuxième type de device (iOS)
- [ ] Vérifier qu'il y a maintenant 2 devices dans la table

---

## 📄 Fichiers de test fournis

**Fichier** : `test-auth-with-device-info.http`

Ce fichier contient **13 scénarios** avec des valeurs **déjà configurées** (valides).

**Utilisation recommandée** : Utilisez ce fichier plutôt que Swagger pour éviter les erreurs avec les valeurs "string".

---

## 🎉 Résumé

**Le système fonctionne correctement !**

Le fait que vous voyiez le warning `"Token FCM ou DeviceType manquant/invalide - Token: 'string', Device: 'string'"` prouve que :
- ✅ Le système **détecte** les valeurs invalides
- ✅ Le système **les rejette** correctement
- ✅ L'authentification **fonctionne quand même**

**Solution** : Utilisez des **valeurs réelles** au lieu de `"string"` ! 🚀


