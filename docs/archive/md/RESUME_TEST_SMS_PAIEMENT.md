# 📋 Résumé : Test SMS Paiement

## ✅ Application démarrée avec succès

**URL** : `https://localhost:7105`  
**Swagger UI** : `https://localhost:7105/swagger`  
**Statut** : Prêt à recevoir des requêtes ✅

---

## 🎯 Prêt pour le test

### **📄 Documentation créée**

- ✅ `GUIDE_TEST_SMS_PAIEMENT.md` : Guide complet de test
- ✅ `test-sms-paiement.ps1` : Script PowerShell automatisé
- ✅ `RESUME_TEST_SMS_PAIEMENT.md` : Ce document

---

## 🚀 Comment tester maintenant

### **Option 1 : Script PowerShell automatisé (Recommandé)**

1. Dans un **nouveau terminal PowerShell** (laisser l'application tourner) :

```powershell
cd G:\KelasiNaBiso\KelasiNaBisoAPI
.\test-sms-paiement.ps1
```

Le script va automatiquement :
- ✅ Se connecter à l'API
- ✅ Trouver un élève avec un tuteur qui a un téléphone
- ✅ Créer un paiement de test
- ✅ Vérifier les logs SMS

---

### **Option 2 : Via Swagger UI (Manuel)**

1. Ouvrir dans le navigateur : `https://localhost:7105/swagger`

2. **S'authentifier** :
   - Endpoint : `POST /api/Utilisateur/login`
   - Body :
     ```json
     {
       "nomUtilisateur": "superadmin",
       "motDePasse": "Super-Admin"
     }
     ```
   - Copier le `token`

3. **Autoriser dans Swagger** :
   - Cliquer sur le bouton "**Authorize**" en haut
   - Coller le token : `Bearer {votre-token}`
   - Cliquer "Authorize"

4. **Vérifier qu'un élève existe avec un tuteur** :
   - Endpoint : `GET /api/Eleve`
   - Vérifier qu'un élève a un `IdTuteur` non null

5. **Vérifier le téléphone du tuteur** :
   - Endpoint : `GET /api/Tuteur/{idTuteur}`
   - Vérifier que le champ `telephone` est rempli (format : `+243XXXXXXXXX`)

6. **Créer un paiement de test** :
   - Endpoint : `POST /api/Paiement`
   - Body :
     ```json
     {
       "datePaiement": "2025-01-27T10:00:00",
       "montant": 50,
       "devise": "USD",
       "modePaiement": "Cash",
       "statutPaiement": "Confirme",
       "statut": true,
       "referenceTransaction": "TEST-SMS-001",
       "commentaire": "Test d'envoi SMS",
       "idEleve": 1,
       "idUtilisateur": 1
     }
     ```

7. **Observer les logs dans la console de l'application** :
   - Cherchez : `✅ SMS paiement envoyé avec succès...`

---

### **Option 3 : Via curl/PowerShell (Manuel)**

Dans un **nouveau terminal PowerShell** :

```powershell
# 1. Se connecter
$loginBody = @{
    nomUtilisateur = "superadmin"
    motDePasse = "Super-Admin"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "https://localhost:7105/api/Utilisateur/login" `
    -Method POST -Body $loginBody -ContentType "application/json"
$token = $response.token

# 2. Récupérer les élèves
$headers = @{ "Authorization" = "Bearer $token" }
$eleves = Invoke-RestMethod -Uri "https://localhost:7105/api/Eleve" `
    -Method GET -Headers $headers

# Afficher les élèves
$eleves | Select-Object -First 5 | Format-Table idEleve, nomComplet, idTuteur

# 3. Choisir un élève (par exemple, le premier)
$eleve = $eleves[0]
Write-Host "Utilisation de l'élève : $($eleve.nomComplet) (ID: $($eleve.idEleve))"

# 4. Vérifier le tuteur
if ($eleve.idTuteur) {
    $tuteur = Invoke-RestMethod -Uri "https://localhost:7105/api/Tuteur/$($eleve.idTuteur)" `
        -Method GET -Headers $headers
    Write-Host "Tuteur : $($tuteur.nomComplet)"
    Write-Host "Téléphone : $($tuteur.telephone)"
    
    if (-not $tuteur.telephone) {
        Write-Host "⚠️  Le tuteur n'a pas de téléphone !" -ForegroundColor Yellow
        exit
    }
}

# 5. Créer un paiement
$paiementBody = @{
    datePaiement = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")
    montant = 50
    devise = "USD"
    modePaiement = "Cash"
    statutPaiement = "Confirme"
    statut = $true
    referenceTransaction = "TEST-SMS-PS-$(Get-Date -Format 'yyyyMMddHHmmss')"
    commentaire = "Test PowerShell"
    idEleve = $eleve.idEleve
    idUtilisateur = 1
} | ConvertTo-Json

$paiement = Invoke-RestMethod -Uri "https://localhost:7105/api/Paiement" `
    -Method POST -Body $paiementBody -Headers $headers -ContentType "application/json"

Write-Host "✅ Paiement créé : ID $($paiement.idPaiement)" -ForegroundColor Green

# 6. Attendre et vérifier les logs
Write-Host "⏳ Attente de 3 secondes pour l'envoi SMS..."
Start-Sleep -Seconds 3

Write-Host "✅ Consultez les logs de l'application pour voir l'envoi SMS" -ForegroundColor Cyan
```

---

## 🔍 Vérifications après le test

### **1. Logs dans la console de l'application**

Recherchez dans la console où tourne `dotnet run` :

#### **✅ Succès attendu :**
```
[INFO] ✅ SMS paiement envoyé avec succès pour {NomEleve} (MessageSid: SM..., Coût: 0.0467 USD, Montant: 50.00 USD)
```

#### **❌ Erreur possible :**
```
[WARN] ⚠️ SMS paiement échoué pour {NomEleve}: {Raison}
[ERROR] ❌ SenderID non configuré pour envoi SMS
```

---

### **2. Table SmsLogs dans la base de données**

Si vous avez accès à la base de données, exécutez :

```sql
SELECT 
    IdSmsLog,
    NumeroDestinataire,
    Message,
    TypeNotification,
    Statut,
    NumeroExpediteur,
    MessageSid,
    CoutUsd,
    DateEnvoi
FROM SmsLogs
WHERE TypeNotification = 'PAIEMENT_ELEVE'
ORDER BY DateEnvoi DESC
LIMIT 5;
```

**Résultat attendu** :
- `TypeNotification = 'PAIEMENT_ELEVE'`
- `NumeroExpediteur = 'YOUR_TWILIO_MESSAGING_SERVICE_SID'` (SenderID)
- `NumeroDestinataire = '+243XXXXXXXXX'` (téléphone du tuteur)
- `Statut = 'DELIVERED'` ou `'SENT'`
- `MessageSid` rempli

---

### **3. Téléphone du tuteur**

Si vous avez un numéro de téléphone valide configuré pour le tuteur, vous devriez recevoir un SMS réel avec le message :

> "{NomEleve} a payé 50 USD pour Frais le {Date}. Réf: TEST-SMS-XXX (Confirmé)"

---

## ✅ Critères de succès

Le test est **réussi** si :

1. ✅ Le paiement est créé (status 201)
2. ✅ Un log SMS apparaît dans la console avec "✅ SMS paiement envoyé..."
3. ✅ Un enregistrement existe dans `SmsLogs` avec :
   - Type : `PAIEMENT_ELEVE`
   - Statut : `DELIVERED` ou `SENT`
   - SenderID : `YOUR_TWILIO_MESSAGING_SERVICE_SID`
4. ✅ Le `MessageSid` est rempli

---

## 🔧 Dépannage rapide

### **Problème : "SMS non envoyé"**

**Causes possibles** :
1. Tuteur sans téléphone → Ajoutez un numéro au tuteur
2. SenderID non configuré → Vérifiez `appsettings.json`
3. Twilio désactivé → Vérifiez `Enabled: true`
4. Credentials Twilio invalides → Vérifiez `AccountSid` et `AuthToken`

**Solution** :
```powershell
# Vérifier la config
Get-Content appsettings.json | Select-String -Pattern "Twilio" -Context 3
```

---

### **Problème : "401 Unauthorized"**

**Cause** : Token JWT invalide ou expiré  
**Solution** : Reconnectez-vous et obtenez un nouveau token

---

### **Problème : "404 Not Found" - Élève**

**Cause** : Aucun élève dans la base de données ou ID incorrect  
**Solution** : Vérifiez `GET /api/Eleve` pour lister les élèves disponibles

---

## 📊 Récapitulatif

| Élément | État |
|---------|------|
| Application démarrée | ✅ |
| Swagger disponible | ✅ |
| Documentation créée | ✅ |
| Script de test créé | ✅ |
| Configuration SMS | ✅ |

**🎯 Le système est prêt pour les tests !**

---

## 🎉 Prochaines étapes

1. **Exécuter le test** (script ou manuel)
2. **Observer les logs** dans la console
3. **Vérifier les résultats** (SmsLogs)
4. **Confirmer l'envoi** si téléphone valide

---

**✅ Tout est prêt ! Vous pouvez maintenant lancer le test.** 🚀

---
*Dernière mise à jour : 2025-01-27*

