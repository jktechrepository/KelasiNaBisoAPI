# 🚀 Guide de déploiement CORS - Production

## 📋 Résumé des modifications CORS

### ✅ Ce qui a été corrigé

**Problème identifié :**
- ❌ Frontend web (https://dev-knb.kansaconsulting.com) bloqué par CORS
- ✅ Mobile fonctionne (car n'envoie pas les headers `cache-control`, `pragma`, `expires`)

**Solution appliquée :**
1. Ajout des headers web manquants dans `Program.cs`
2. Configuration des origines autorisées dans `appsettings.json`

---

## 🔧 Fichiers modifiés

### 1. **Program.cs** (lignes 268-280)

```csharp
policy.WithHeaders(
    "Content-Type",
    "Authorization",
    "Accept",
    "Origin",
    "X-Requested-With",
    "Cache-Control",  // ✅ AJOUTÉ pour le web
    "Pragma",         // ✅ AJOUTÉ pour le web
    "Expires"         // ✅ AJOUTÉ pour le web
)
.WithMethods("GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS")
.AllowCredentials()
.SetPreflightMaxAge(TimeSpan.FromMinutes(10));
```

### 2. **appsettings.json** (lignes 205-215)

```json
"Cors": {
  "AllowedOrigins": [
    "https://dev-knb.kansaconsulting.com",
    "http://dev-knb.kansaconsulting.com",
    "https://knb.kansaconsulting.com",
    "http://knb.kansaconsulting.com",
    "http://localhost:6600",
    "http://localhost:8080",
    "http://localhost:3000"
  ]
}
```

---

## 📦 Déploiement en production

### Étape 1 : Préparer les fichiers

Les fichiers compilés sont dans le dossier **`publish/`** :

```
G:\KelasiNaBiso\KelasiNaBisoAPI\publish\
```

### Étape 2 : Transférer vers le serveur

**Option A : Via FTP/SFTP**
1. Connecte-toi à ton serveur de production
2. Copie **tout le contenu** du dossier `publish/` vers le répertoire de l'API sur le serveur

**Option B : Via Git (recommandé)**
```bash
# Sur le serveur
cd /path/to/kelasinabiso-api
git pull origin main
dotnet publish --configuration Release --output ./publish
```

### Étape 3 : Vérifier/Modifier `appsettings.json` sur le serveur

**IMPORTANT :** Sur le serveur, ouvre `appsettings.json` et **vérifie/modifie** :

#### A. Configuration CORS
```json
"Cors": {
  "AllowedOrigins": [
    "https://dev-knb.kansaconsulting.com",
    "https://knb.kansaconsulting.com"
  ]
}
```

⚠️ **N'inclus PAS les origines `localhost` en production !**

#### B. Chaîne de connexion MySQL
```json
"ConnectionStrings": {
  "KelasiConnection": "Server=localhost;Port=3306;Database=KnbV2_db;User=ton_user_prod;Password=ton_password_prod;CharSet=utf8mb4;"
}
```

#### C. Vérifier les autres services
- Firebase credentials
- Twilio (si utilisé en production)
- Email settings
- Serilog (logs MySQL)

### Étape 4 : Redémarrer l'API sur le serveur

**Si tu utilises systemd :**
```bash
sudo systemctl restart kelasinabiso-api
sudo systemctl status kelasinabiso-api
```

**Si tu utilises PM2 :**
```bash
pm2 restart kelasinabiso-api
pm2 logs kelasinabiso-api --lines 50
```

**Si tu utilises IIS :**
1. Ouvre IIS Manager
2. Sélectionne le site "KelasiNaBisoAPI"
3. Clic droit → "Manage Website" → "Restart"

---

## ✅ Vérification après déploiement

### Test 1 : Vérifier que l'API répond

```bash
curl https://knb.asdc-rdc.org/api/health
```

Résultat attendu : `200 OK` avec un message de santé

### Test 2 : Tester CORS depuis le navigateur

Ouvre la console de ton frontend web (https://dev-knb.kansaconsulting.com) et exécute :

```javascript
fetch('https://knb.asdc-rdc.org/api/Utilisateur/authentifier', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Cache-Control': 'no-cache'  // Header qui causait le problème
  },
  credentials: 'include',
  body: JSON.stringify({
    emailOuTelephone: 'test@test.com',
    motDePasse: 'test'
  })
})
.then(r => {
  console.log('✅ CORS OK! Status:', r.status);
  return r.json();
})
.then(data => console.log('Réponse:', data))
.catch(e => console.error('❌ Erreur:', e));
```

**Résultat attendu :**
- ✅ **Aucune erreur CORS** dans la console
- ✅ Réponse HTTP (200, 401, 400, etc.) au lieu d'une erreur réseau

### Test 3 : Vérifier les logs du serveur

```bash
# Logs en temps réel
tail -f /path/to/logs/log-*.txt

# Rechercher les erreurs CORS
grep -i "cors" /path/to/logs/log-*.txt
```

---

## 🔒 Sécurité CORS en production

### ✅ Configuration SÉCURISÉE (Recommandée)

```json
"Cors": {
  "AllowedOrigins": [
    "https://dev-knb.kansaconsulting.com",
    "https://knb.kansaconsulting.com"
  ]
}
```

### ❌ Configuration DANGEREUSE (À éviter)

```json
"Cors": {
  "AllowedOrigins": ["*"]  // ❌ JAMAIS en production !
}
```

Ou dans `Program.cs` :
```csharp
// ❌ DANGEREUX en production
policy.SetIsOriginAllowed(origin => true)
      .AllowAnyHeader()
      .AllowAnyMethod();
```

---

## 🐛 Dépannage

### Problème 1 : CORS toujours bloqué après déploiement

**Solutions :**

1. **Vérifier que le serveur utilise bien la nouvelle version**
   ```bash
   # Sur le serveur
   grep -A 5 "Cors" appsettings.json
   ```

2. **Vider le cache du navigateur**
   - Chrome/Edge : `Ctrl + Shift + Delete` → Cocher "Cached images and files"
   - Firefox : `Ctrl + Shift + Delete` → Cocher "Cache"

3. **Activer les logs CORS détaillés**
   
   Dans `appsettings.json` (serveur) :
   ```json
   "Logging": {
     "LogLevel": {
       "Microsoft.AspNetCore.Cors": "Debug"
     }
   }
   ```
   
   Redémarrer l'API, puis consulter les logs.

4. **Tester avec curl en mode OPTIONS (Preflight)**
   ```bash
   curl -X OPTIONS https://knb.asdc-rdc.org/api/Utilisateur/authentifier \
     -H "Origin: https://dev-knb.kansaconsulting.com" \
     -H "Access-Control-Request-Method: POST" \
     -H "Access-Control-Request-Headers: cache-control,content-type" \
     -v
   ```
   
   **Résultat attendu :**
   ```
   Access-Control-Allow-Origin: https://dev-knb.kansaconsulting.com
   Access-Control-Allow-Headers: cache-control,content-type,...
   Access-Control-Allow-Methods: POST,...
   Access-Control-Allow-Credentials: true
   ```

### Problème 2 : Mobile fonctionne, mais web non

**Cause probable :** Les headers web (`cache-control`, `pragma`, `expires`) ne sont pas autorisés.

**Solution :** Vérifier que `Program.cs` contient bien les 3 headers ajoutés (lignes 274-276).

### Problème 3 : Erreur "credentials mode is 'include'"

**Cause :** Tu utilises `.AllowCredentials()` mais l'origine n'est pas explicitement listée.

**Solution :** Ne pas utiliser `"*"` dans `AllowedOrigins`, lister les origines exactes.

---

## 📝 Checklist de déploiement

- [ ] ✅ Compilation en mode Release réussie
- [ ] ✅ Fichiers copiés vers le serveur
- [ ] ✅ `appsettings.json` du serveur vérifié/modifié
- [ ] ✅ Origines CORS correspondent aux domaines réels
- [ ] ✅ Pas d'origines `localhost` en production
- [ ] ✅ Chaîne de connexion MySQL correcte
- [ ] ✅ API redémarrée sur le serveur
- [ ] ✅ Test de santé API réussi (`/api/health`)
- [ ] ✅ Test CORS depuis le frontend web réussi
- [ ] ✅ Aucune erreur dans les logs du serveur

---

## 🎉 Succès !

Une fois toutes les étapes complétées, ton frontend web (https://dev-knb.kansaconsulting.com) devrait pouvoir se connecter à l'API (https://knb.asdc-rdc.org) **sans aucune erreur CORS** ! ✅

---

## 📞 Support

Si le problème persiste après avoir suivi ce guide :

1. ✅ Vérifie les logs du serveur
2. ✅ Teste avec `curl` (commandes ci-dessus)
3. ✅ Capture l'erreur exacte dans la console du navigateur
4. ✅ Vérifie que l'API utilise bien la bonne configuration

**Date de création :** 2025-11-04  
**Version de l'API :** KelasiNaBisoAPI v2.0  
**Framework :** ASP.NET Core 6.0


