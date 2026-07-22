# 🔧 Guide de Dépannage CORS - API KelasiNaBiso

## 🚨 Problème CORS Résolu

Le problème CORS que vous rencontriez a été résolu. Voici les modifications apportées :

### ✅ Modifications effectuées

1. **Désactivation de la redirection HTTPS en développement**
2. **Configuration CORS améliorée**
3. **Ajout des origines autorisées**

---

## 🔍 Diagnostic du problème

### Erreur rencontrée
```
Access to fetch at 'http://192.168.100.17:5001/api/Utilisateur/authentifier' 
from origin 'http://192.168.100.19:5501' has been blocked by CORS policy: 
Response to preflight request doesn't pass access control check: 
Redirect is not allowed for a preflight request.
```

### Cause du problème
- L'API redirige automatiquement HTTP vers HTTPS
- Les requêtes preflight (OPTIONS) ne peuvent pas être redirigées
- Le navigateur bloque la requête

---

## 🛠️ Solutions appliquées

### 1. Configuration CORS améliorée

```csharp
// Configuration CORS améliorée
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            if (builder.Environment.IsDevelopment())
            {
                // En développement, permettre toutes les origines
                policy.SetIsOriginAllowed(origin => true)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            }
            else
            {
                // En production, utiliser les origines configurées
                var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
                if (allowedOrigins != null && allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                }
                else
                {
                    policy.SetIsOriginAllowed(origin => true)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                }
            }
        });
});
```

### 2. Redirection HTTPS conditionnelle

```csharp
// Redirection HTTPS seulement en production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
```

### 3. Origines autorisées en développement

```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://192.168.100.19:5501",
      "http://localhost:3000",
      "http://localhost:5501",
      "http://127.0.0.1:5501"
    ]
  }
}
```

---

## 🚀 Instructions pour les développeurs frontend

### 1. Redémarrez l'API
```bash
# Arrêtez l'API (Ctrl+C)
# Puis relancez
dotnet run
```

### 2. Testez avec les outils fournis
```bash
# Test côté serveur (PowerShell)
.\test-api.ps1

# Test côté navigateur
# Ouvrez test-cors.html dans votre navigateur
```

### 2. Vérifiez l'URL de l'API
Assurez-vous d'utiliser la bonne URL :

```javascript
// ✅ Correct - Utilisez HTTP en développement
const API_BASE_URL = 'http://192.168.100.17:5001';

// ❌ Incorrect - N'utilisez pas HTTPS en développement
const API_BASE_URL = 'https://192.168.100.17:5001';
```

### 3. Configuration frontend mise à jour

```javascript
// Configuration pour le développement
const API_CONFIG = {
  baseURL: process.env.NODE_ENV === 'development' 
    ? 'http://192.168.100.17:5001'  // HTTP en dev
    : 'https://192.168.100.17:5001', // HTTPS en prod
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true, // Important pour CORS
};

// Exemple avec fetch
const authenticateUser = async (emailOrPhone, password) => {
  try {
    const response = await fetch(`${API_CONFIG.baseURL}/api/Utilisateur/authentifier`, {
      method: 'POST',
      headers: API_CONFIG.headers,
      credentials: 'include', // Important pour CORS
      body: JSON.stringify({
        emailOuTelephone: emailOrPhone,
        motDePasse: password,
      }),
    });

    if (!response.ok) {
      throw new Error('Authentification échouée');
    }

    return await response.json();
  } catch (error) {
    console.error('Erreur d\'authentification:', error);
    throw error;
  }
};
```

### 4. Configuration Axios mise à jour

```javascript
import axios from 'axios';

const api = axios.create({
  baseURL: process.env.NODE_ENV === 'development' 
    ? 'http://192.168.100.17:5001'  // HTTP en dev
    : 'https://192.168.100.17:5001', // HTTPS en prod
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true, // Important pour CORS
});

// Intercepteur pour ajouter le token d'authentification
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('authToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default api;
```

---

## 🧪 Test de la connexion

### 1. Test avec curl
```bash
# Test de l'endpoint d'authentification
curl -X POST "http://192.168.100.17:5001/api/Utilisateur/authentifier" \
  -H "Content-Type: application/json" \
  -d '{
    "emailOuTelephone": "admin@example.com",
    "motDePasse": "password123"
  }'
```

### 2. Test avec Postman
1. Ouvrez Postman
2. Créez une nouvelle requête POST
3. URL : `http://192.168.100.17:5001/api/Utilisateur/authentifier`
4. Headers : `Content-Type: application/json`
5. Body (raw JSON) :
```json
{
  "emailOuTelephone": "admin@example.com",
  "motDePasse": "password123"
}
```

### 3. Test dans le navigateur
```javascript
// Test dans la console du navigateur
fetch('http://192.168.100.17:5001/api/Utilisateur/authentifier', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
  },
  credentials: 'include',
  body: JSON.stringify({
    emailOuTelephone: 'admin@example.com',
    motDePasse: 'password123'
  })
})
.then(response => response.json())
.then(data => console.log('Succès:', data))
.catch(error => console.error('Erreur:', error));
```

---

## 🔧 Dépannage supplémentaire

### Si le problème persiste

1. **Vérifiez que l'API est bien redémarrée**
   ```bash
   # Arrêtez complètement l'API (Ctrl+C)
   # Puis relancez
   dotnet run
   ```

2. **Utilisez les outils de diagnostic fournis**
   ```bash
   # Test PowerShell complet
   .\test-api.ps1
   
   # Test navigateur
   # Ouvrez test-cors.html dans votre navigateur
   ```

3. **Vérifiez l'ordre des middlewares**
   - `app.UseCors()` doit être **avant** `app.UseRouting()`
   - Cette correction a été appliquée dans `Program.cs`

2. **Vérifiez les logs de l'API**
   - Regardez la console où l'API tourne
   - Vérifiez les messages d'erreur

3. **Vérifiez la configuration réseau**
   - Assurez-vous que les IPs sont correctes
   - Vérifiez que le port 5001 est accessible

4. **Testez avec une URL locale**
   ```javascript
   // Test avec localhost
   const API_BASE_URL = 'http://localhost:5001';
   ```

### Ajout d'une nouvelle origine

Si vous développez depuis une nouvelle adresse IP, ajoutez-la dans `appsettings.Development.json` :

```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://192.168.100.19:5501",
      "http://192.168.100.20:3000",  // Nouvelle IP
      "http://localhost:3000",
      "http://localhost:5501",
      "http://127.0.0.1:5501"
    ]
  }
}
```

---

## 📞 Support

Si le problème persiste après ces modifications :

1. **Vérifiez les logs de l'API** dans la console
2. **Testez avec Postman** pour isoler le problème
3. **Contactez l'équipe backend** avec les logs d'erreur

---

*Guide mis à jour le: ${new Date().toLocaleDateString('fr-FR')}*
