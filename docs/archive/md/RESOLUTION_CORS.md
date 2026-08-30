# 🚀 Guide de Résolution CORS - API KelasiNaBiso

## 📋 Checklist de résolution

### ✅ Étape 1: Vérifications préliminaires

- [ ] L'API est démarrée avec `dotnet run`
- [ ] L'API répond sur `http://192.168.100.17:5001`
- [ ] Swagger est accessible sur `http://192.168.100.17:5001/swagger`

### ✅ Étape 2: Tests de diagnostic

1. **Test PowerShell rapide :**
   ```powershell
   .\quick-check.ps1
   ```

2. **Test complet :**
   ```powershell
   .\test-api.ps1
   ```

3. **Test navigateur :**
   - Ouvrez `test-cors.html` dans votre navigateur
   - Cliquez sur "Test Preflight (OPTIONS)"
   - Cliquez sur "Test Authentification (POST)"

### ✅ Étape 3: Vérification des corrections appliquées

#### 3.1 Ordre des middlewares (Program.cs)
```csharp
// ✅ CORRECT - CORS avant Routing
app.UseCors("AllowFrontend");
app.UseRouting();
app.UseAuthorization();
```

#### 3.2 Configuration CORS (Program.cs)
```csharp
// ✅ Configuration CORS améliorée
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            if (builder.Environment.IsDevelopment())
            {
                policy.SetIsOriginAllowed(origin => true)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            }
            // ... configuration production
        });
});
```

#### 3.3 Redirection HTTPS conditionnelle (Program.cs)
```csharp
// ✅ HTTPS seulement en production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
```

#### 3.4 Origines autorisées (appsettings.Development.json)
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

### ✅ Étape 4: Configuration frontend

#### 4.1 URL correcte
```javascript
// ✅ Utilisez HTTP en développement
const API_BASE_URL = 'http://192.168.100.17:5001';
```

#### 4.2 Headers CORS
```javascript
// ✅ Incluez credentials
const response = await fetch(`${API_BASE_URL}/api/Utilisateur/authentifier`, {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  credentials: 'include', // Important !
  body: JSON.stringify({
    emailOuTelephone: 'admin@example.com',
    motDePasse: 'password123'
  })
});
```

### ✅ Étape 5: Tests de validation

#### 5.1 Test avec curl
```bash
curl -X POST "http://192.168.100.17:5001/api/Utilisateur/authentifier" \
  -H "Content-Type: application/json" \
  -d '{
    "emailOuTelephone": "admin@example.com",
    "motDePasse": "password123"
  }'
```

#### 5.2 Test avec Postman
- Utilisez la collection `KelasiNaBiso_API_Collection.postman_collection.json`
- Vérifiez que la variable `baseUrl` est définie sur `http://192.168.100.17:5001`

#### 5.3 Test dans le navigateur
- Ouvrez la console du navigateur
- Exécutez le code de test dans `test-cors.html`

## 🔧 Dépannage avancé

### Si les tests échouent encore :

1. **Vérifiez les logs de l'API**
   ```bash
   # Regardez la console où l'API tourne
   # Recherchez les erreurs CORS
   ```

2. **Vérifiez la connectivité réseau**
   ```bash
   # Test de ping
   ping 192.168.100.17
   
   # Test de port
   telnet 192.168.100.17 5001
   ```

3. **Vérifiez le firewall**
   - Assurez-vous que le port 5001 n'est pas bloqué
   - Vérifiez les règles Windows Firewall

4. **Testez avec localhost**
   ```javascript
   // Si l'IP externe ne fonctionne pas
   const API_BASE_URL = 'http://localhost:5001';
   ```

## 📊 Résultats attendus

### ✅ Tests réussis
- [ ] `quick-check.ps1` : Tous les tests passent
- [ ] `test-cors.html` : Preflight et authentification réussis
- [ ] Postman : Collection fonctionne
- [ ] Frontend : Connexion établie

### ❌ Si les tests échouent
1. Vérifiez que l'API est redémarrée
2. Vérifiez l'ordre des middlewares
3. Vérifiez la configuration CORS
4. Vérifiez la connectivité réseau
5. Consultez les logs de l'API

## 🆘 Support

Si le problème persiste :
1. Exécutez `.\test-api.ps1` et partagez les résultats
2. Ouvrez `test-cors.html` et partagez les logs
3. Vérifiez les logs de l'API dans la console
4. Contactez l'équipe backend avec les détails

---

*Guide de résolution mis à jour le: ${new Date().toLocaleDateString('fr-FR')}*
