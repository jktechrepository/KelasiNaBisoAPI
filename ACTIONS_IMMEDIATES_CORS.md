# ⚡ Actions immédiates pour corriger CORS en production

## 🎯 Objectif
Corriger l'erreur CORS pour le frontend web (https://dev-knb.kansaconsulting.com)

---

## 📦 Fichiers prêts pour le déploiement

Les fichiers compilés sont dans :
```
G:\KelasiNaBiso\KelasiNaBisoAPI\publish\
```

---

## 🚀 Étapes à suivre MAINTENANT

### 1️⃣ **Transférer les fichiers vers le serveur de production**

**Méthode recommandée : SFTP/FTP**

1. Connecte-toi à ton serveur de production
2. **Sauvegarde l'ancien dossier** (au cas où) :
   ```bash
   mv /path/to/kelasinabiso-api /path/to/kelasinabiso-api.backup
   ```
3. Copie **tout le contenu** du dossier `publish/` vers `/path/to/kelasinabiso-api/`

---

### 2️⃣ **Modifier `appsettings.json` sur le serveur**

**Ouvre le fichier** `/path/to/kelasinabiso-api/appsettings.json` **sur le serveur**

**Ajoute la section CORS** (après la section `AllowedHosts`) :

```json
  "AllowedHosts": "*",
  "Cors": {
    "AllowedOrigins": [
      "https://dev-knb.kansaconsulting.com",
      "https://knb.kansaconsulting.com"
    ]
  }
```

**⚠️ IMPORTANT :**
- ✅ **HTTPS uniquement** en production (pas de `http://`)
- ✅ **Pas de `localhost`** en production
- ✅ Vérifie que la chaîne de connexion MySQL est correcte pour la production

---

### 3️⃣ **Redémarrer l'API sur le serveur**

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
1. IIS Manager → Sélectionne le site
2. Clic droit → "Manage Website" → "Restart"

---

### 4️⃣ **Tester depuis le frontend web**

1. Ouvre https://dev-knb.kansaconsulting.com
2. Essaie de te connecter
3. Ouvre la **Console du navigateur** (F12)

**Résultat attendu :**
- ✅ **Aucune erreur CORS** rouge
- ✅ La connexion fonctionne (ou erreur 401 si mauvais identifiants, mais pas CORS)

---

## ✅ Checklist rapide

- [ ] Fichiers du dossier `publish/` copiés sur le serveur
- [ ] Section `"Cors"` ajoutée dans `appsettings.json` du serveur
- [ ] Origines HTTPS correctes (pas de localhost)
- [ ] API redémarrée
- [ ] Test de connexion depuis le frontend web réussi

---

## 🆘 Si ça ne marche pas

### Test rapide dans la console du navigateur :

```javascript
fetch('https://knb.asdc-rdc.org/api/Utilisateur/authentifier', {
  method: 'OPTIONS',
  headers: {
    'Origin': 'https://dev-knb.kansaconsulting.com',
    'Access-Control-Request-Method': 'POST',
    'Access-Control-Request-Headers': 'content-type,cache-control'
  }
})
.then(r => {
  console.log('✅ Preflight OK:', r.status);
  console.log('Headers:', Array.from(r.headers.entries()));
})
.catch(e => console.error('❌ Erreur:', e));
```

**Si l'erreur persiste :**
1. Vérifie les logs du serveur : `tail -f /path/to/logs/log-*.txt`
2. Vérifie que `appsettings.json` contient bien la section `Cors`
3. Vide le cache du navigateur (Ctrl + Shift + Delete)

---

## 📞 Résumé ultra-rapide

```bash
# 1. Copier les fichiers vers le serveur
scp -r publish/* user@server:/path/to/kelasinabiso-api/

# 2. Éditer appsettings.json sur le serveur
nano /path/to/kelasinabiso-api/appsettings.json
# Ajouter la section "Cors" avec les origines

# 3. Redémarrer l'API
sudo systemctl restart kelasinabiso-api
# OU
pm2 restart kelasinabiso-api

# 4. Tester depuis le frontend web
```

---

**C'est tout ! 🎉**

Une fois ces 4 étapes terminées, ton problème CORS devrait être **résolu définitivement** ! ✅


