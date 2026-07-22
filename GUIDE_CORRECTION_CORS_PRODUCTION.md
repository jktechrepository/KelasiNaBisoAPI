# 🔧 Guide de correction CORS pour la production

## 🔍 Problème identifié

### ❌ Erreur dans le frontend web :
```
Blocage d'une requête multiorigine (Cross-Origin Request) : 
la politique « Same Origin » ne permet pas de consulter la ressource distante
Raison : l'en-tête « cache-control » n'est pas autorisé
```

### ✅ Pourquoi le mobile fonctionne mais pas le web ?

| Client | Comportement | Headers envoyés |
|--------|-------------|----------------|
| **Mobile** | ✅ Fonctionne | Headers de base uniquement |
| **Web (navigateur)** | ❌ Bloqué | Headers de base + `cache-control`, `pragma`, `expires` |

---

## 🔧 Corrections appliquées

### 1. **Dans `Program.cs`** (lignes 268-277)

Ajout explicite des headers web manquants :

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
```

### 2. **Dans `appsettings.json`** (lignes 205-215)

Ajout de la configuration des origines autorisées :

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

## 🚀 Déploiement en production

### Étape 1 : Compiler l'application

```bash
dotnet build --configuration Release
dotnet publish --configuration Release --output ./publish
```

### Étape 2 : Déployer les fichiers

Copier le contenu du dossier `publish/` vers le serveur de production.

### Étape 3 : Vérifier la configuration

Sur le serveur, éditer `appsettings.json` et **ajouter/modifier** les origines autorisées si nécessaire :

```json
"Cors": {
  "AllowedOrigins": [
    "https://dev-knb.kansaconsulting.com",
    "https://knb.kansaconsulting.com"
  ]
}
```

### Étape 4 : Redémarrer l'API

```bash
# Sur le serveur
sudo systemctl restart kelasinabiso-api
# OU
pm2 restart kelasinabiso-api
```

---

## ✅ Vérification après déploiement

### Test rapide dans la console du navigateur :

```javascript
fetch('https://knb.asdc-rdc.org/api/Utilisateur/authentifier', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Cache-Control': 'no-cache'  // Test du header problématique
  },
  body: JSON.stringify({
    emailOuTelephone: 'test@test.com',
    motDePasse: 'test'
  })
})
.then(r => console.log('✅ CORS OK:', r.status))
.catch(e => console.error('❌ CORS Erreur:', e));
```

### Résultat attendu :
- ✅ **Aucune erreur CORS** dans la console
- ✅ Réponse HTTP (200, 401, ou 400) au lieu d'une erreur réseau

---

## 🔒 Sécurité CORS en production

### ⚠️ IMPORTANT : Ne jamais utiliser en production :

```csharp
// ❌ DANGEREUX en production
policy.SetIsOriginAllowed(origin => true)
      .AllowAnyHeader()
      .AllowAnyMethod();
```

### ✅ TOUJOURS utiliser :

```csharp
// ✅ SÉCURISÉ en production
policy.WithOrigins("https://knb.kansaconsulting.com")
      .WithHeaders("Content-Type", "Authorization", "Cache-Control")
      .WithMethods("GET", "POST", "PUT", "DELETE");
```

---

## 📝 Checklist de déploiement

- [ ] `Program.cs` mis à jour avec les headers CORS complets
- [ ] `appsettings.json` contient la section `Cors:AllowedOrigins`
- [ ] Les origines listées correspondent aux domaines réels du frontend
- [ ] Application compilée en mode Release
- [ ] Fichiers déployés sur le serveur
- [ ] `appsettings.json` du serveur vérifié
- [ ] API redémarrée
- [ ] Test de connexion depuis le frontend web réussi

---

## ❓ Dépannage

### Problème : Toujours bloqué après déploiement

**Solutions :**

1. **Vérifier les logs du serveur** :
   ```bash
   tail -f /var/log/kelasinabiso-api/log-*.txt
   ```

2. **Activer les logs CORS détaillés** (temporairement) :
   Dans `appsettings.json` (serveur) :
   ```json
   "Logging": {
     "LogLevel": {
       "Microsoft.AspNetCore.Cors": "Debug"
     }
   }
   ```

3. **Tester avec curl** depuis le serveur :
   ```bash
   curl -X OPTIONS https://knb.asdc-rdc.org/api/Utilisateur/authentifier \
     -H "Origin: https://dev-knb.kansaconsulting.com" \
     -H "Access-Control-Request-Method: POST" \
     -H "Access-Control-Request-Headers: cache-control,content-type" \
     -v
   ```

   Résultat attendu :
   ```
   Access-Control-Allow-Origin: https://dev-knb.kansaconsulting.com
   Access-Control-Allow-Headers: cache-control,content-type,...
   Access-Control-Allow-Methods: POST,...
   ```

---

## 🎉 Succès !

Une fois déployé et redémarré, ton frontend web devrait pouvoir se connecter sans erreur CORS ! ✅

