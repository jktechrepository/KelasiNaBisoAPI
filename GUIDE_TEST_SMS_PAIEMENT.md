# 📋 Guide de test : Envoi SMS lors du paiement

## 🎯 Objectif

Tester que le système envoie correctement un SMS au tuteur lors de la création d'un paiement pour un élève.

---

## 📋 Prérequis

### **1. Configuration vérifiée**

✅ **Twilio configuré dans `appsettings.json` :**
```json
"Twilio": {
  "AccountSid": "YOUR_TWILIO_ACCOUNT_SID",
  "AuthToken": "YOUR_TWILIO_AUTH_TOKEN",
  "SenderId": "YOUR_TWILIO_MESSAGING_SERVICE_SID",
  "Enabled": true
}
```

### **2. Données nécessaires**

Pour tester, vous devez avoir :
- ✅ Un élève existant dans la base de données
- ✅ Un tuteur lié à cet élève
- ✅ Un numéro de téléphone valide pour le tuteur (format international : `+243XXXXXXXXX`)

---

## 🚀 Étapes de test

### **Étape 1 : Lancer l'application**

```bash
dotnet run
```

L'application sera accessible sur :
- 🔗 Swagger UI : `https://localhost:7105/swagger`
- 🔗 API : `https://localhost:7105`

---

### **Étape 2 : Obtenir un token JWT (authentification requise)**

L'endpoint `POST /api/Paiement` nécessite une authentification JWT.

#### **Option A : Via Swagger UI**

1. Ouvrez Swagger UI : `https://localhost:7105/swagger`
2. Trouvez l'endpoint `/api/Utilisateur/login`
3. Connectez-vous avec :
   ```json
   {
     "nomUtilisateur": "superadmin",
     "motDePasse": "Super-Admin"
   }
   ```
4. Copiez le `token` de la réponse

#### **Option B : Via curl/PowerShell**

```powershell
$loginBody = @{
    nomUtilisateur = "superadmin"
    motDePasse = "Super-Admin"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "https://localhost:7105/api/Utilisateur/login" -Method POST -Body $loginBody -ContentType "application/json"
$token = $response.token
Write-Host "Token: $token"
```

---

### **Étape 3 : Vérifier qu'un élève existe avec un tuteur ayant un téléphone**

#### **Via Swagger UI**

1. Endpoint : `GET /api/Eleve`
2. Utilisez le token dans le bouton "Authorize" en haut de la page Swagger
3. Recherchez un élève et notez :
   - `IdEleve`
   - `IdTuteur`
   - Vérifiez que le tuteur a un numéro de téléphone valide

#### **Via PowerShell**

```powershell
$headers = @{
    "Authorization" = "Bearer $token"
}

# Récupérer tous les élèves
$eleves = Invoke-RestMethod -Uri "https://localhost:7105/api/Eleve" -Method GET -Headers $headers

# Afficher les élèves avec leurs tuteurs
$eleves | ForEach-Object {
    Write-Host "ID: $($_.idEleve) - Nom: $($_.nomComplet) - Tuteur ID: $($_.idTuteur)"
}
```

#### **Vérifier le téléphone du tuteur**

```powershell
# Récupérer un tuteur spécifique (remplacez {idTuteur} par l'ID réel)
$tuteur = Invoke-RestMethod -Uri "https://localhost:7105/api/Tuteur/{idTuteur}" -Method GET -Headers $headers
Write-Host "Téléphone tuteur: $($tuteur.telephone)"
```

⚠️ **Important** : Le numéro doit être au format international (ex: `+243825099299`)

---

### **Étape 4 : Créer un paiement de test**

#### **Via Swagger UI**

1. Endpoint : `POST /api/Paiement`
2. Cliquez sur "Try it out"
3. Remplissez le body JSON :

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

⚠️ **Important** : Remplacez `idEleve` par l'ID réel d'un élève qui a un tuteur avec téléphone.

4. Cliquez sur "Execute"

#### **Via PowerShell**

```powershell
$paiementBody = @{
    datePaiement = "2025-01-27T10:00:00"
    montant = 50
    devise = "USD"
    modePaiement = "Cash"
    statutPaiement = "Confirme"
    statut = $true
    referenceTransaction = "TEST-SMS-001"
    commentaire = "Test d'envoi SMS"
    idEleve = 1  # ⚠️ Remplacez par l'ID réel
    idUtilisateur = 1
} | ConvertTo-Json

$headers = @{
    "Authorization" = unauthorized
    "Content-Type" = "application/json"
}

$paiement = Invoke-RestMethod -Uri "https://localhost:7105/api/Paiement" -Method POST -Body作文 $paiementBody -Headers $headers
Write-Host "Paiement créé avec ID: $($paiement.idPaiement)"
```

---

### **Étape 5 : Vérifier les logs de l'application**

#### **Dans la console de l'application**

Vous devriez voir des logs comme :

```
✅ SMS paiement envoyé avec succès pour {NomEleve} (MessageSid: SM..., Coût: 0.0467 USD, Montant: 50.00 USD)
```

ou en cas d'erreur :

```
⚠️ SMS paiement échoué pour {NomEleve}: {MessageErreur}
❌ SenderID non configuré pour envoi SMS
```

#### **Vérifier les logs fichiers**

Les logs sont enregistrés dans les fichiers configurés (Serilog).

---

### **Étape 6 : Vérifier dans la base de données**

#### **Requête SQL pour vérifier les SMS envoyés**

```sql
-- Voir les derniers SMS envoyés
SELECT 
    IdSmsLog,
    NumeroDestinataire,
    Message,
    TypeNotification,
    Statut,
    TekterExpediteur,
    MessageSid,
    CoutUsd,
    DateEnvoi,
    MessageErreur
FROM SmsLogs
WHERE TypeNotification = 'PAIEMENT_ELEVE'
ORDER BY DateEnvoi DESC
LIMIT 10;
```

#### **Via l'API (si endpoint existe)**

```powershell
# Vérifier les SMS envoyés pour paiement
$smsLogs = Invoke-RestMethod -Uri "https://localhost:7105/api/SmsLog?type=PAIEMENT_ELEVE" -Method GET -Headers $headers
$smsLogs | Format-Table
```

---

## ✅ Critères de succès

Le test est **réussi** si :

1. ✅ Le paiement est créé avec succès (status 201)
2. ✅ Un log SMS apparaît dans la console :
   - `✅ SMS paiement envoyé avec succès...` (succès)
   - ou `⚠️ SMS paiement échoué...` (vérifier la raison)
3. ✅ Un enregistrement apparaît dans la table `SmsLogs` avec :
   - `TypeNotification = "PAIEMENT_ELEVE"`
   - `Statut = "DELIVERED"` ou `"SENT"` (succès)
   - `MessageSid` rempli (identifiant Twilio)
   - `NumeroExpediteur = "YOUR_TWILIO_MESSAGING_SERVICE_SID"` (SenderID)

---

## 🔍 Dépannage

### **Problème : SMS non envoyé**

#### **1. Vérifier la configuration Twilio**

```powershell
# Vérifier appsettings.json
Get-Content appsettings.json | Select-String -Pattern "Twilio" -Context 5
```

Vérifiez que :
- ✅ `Enabled: true`
- ✅ `SenderId` est configuré
- ✅ `AccountSid` et `AuthToken` sont valides

#### **2. Vérifier que le tuteur a un téléphone**

```sql
SELECT 
    t.IdTuteur,
    t.NomComplet,
    t.Telephone
FROM Tuteurs t
INNER JOIN Eleves e ON e.IdTuteur = t.IdTuteur
WHERE e.IdEleve = {idEleve};
```

#### **3. Vérifier les logs d'erreur**

```powershell
# Dans la console de l'application, cherchez :
- "❌ SenderID non configuré"
- "⚠️ SMS paiement échoué"
- "❌ Erreur lors de l'envoi SMS"
```

#### **4. Vérifier la connexion Twilio**

Testez directement avec l'API Twilio si les credentials sont valides.

---

### **Problème : "401 Unauthorized"**

- ✅ Vérifiez que vous êtes connecté
- ✅ Vérifiez que le token JWT est valide
- ✅ Utilisez le bouton "Authorize" dans Swagger

---

### **Problème : "404 Not Found" - Élève introuvable**

- ✅ Vérifiez que l'élève existe
- ✅ Vérifiez que `IdEleve` est correct
- ✅ Utilisez `GET /api/Eleve` pour lister les élèves disponibles

---

## 📊 Exemple de réponse attendue

### **Réponse de création de paiement**

```json
{
  "idPaiement": 123,
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

### **Log SMS attendu dans la console**

```
[INFO] ✅ SMS paiement envoyé avec succès pour Jean Dupont (MessageSid: SM1234567890abcdef, Coût: 0.0467 USD, Montant: 50.00 USD)
```

### **Enregistrement SmsLog attendu**

```json
{
  "idSmsLog": 456,
  "numeroDestinataire": "+243825099299",
  "message": "Jean Dupont a payé 50 USD pour Frais le 27/01/2025. Réf: TEST-SMS-001 (Confirmé)",
  "typeNotification": "PAIEMENT_ELEVE",
  "statut": "DELIVERED",
  "numeroExpediteur": "YOUR_TWILIO_MESSAGING_SERVICE_SID",
  "messageSid": "SM1234567890abcdef",
  "coutUsd": 0.0467,
  "dateEnvoi": "2025-01-27T10:00:05"
}
```

---

## 🎉 Test réussi !

Si tous les critères sont remplis, le système SMS fonctionne correctement pour les paiements ! 🎊

---
*Dernière mise à jour : 2025-01-27*

