# 📋 GUIDE COMPLET - AUDIT TRAIL SYSTÈME

## 🎯 Vue d'ensemble

L'**Audit Trail** de KelasiNaBiso trace TOUTES les modifications (CREATE, UPDATE, DELETE) effectuées dans le système, fournissant une traçabilité complète pour :
- ✅ Conformité légale
- ✅ Sécurité et détection de fraudes
- ✅ Support technique et debugging
- ✅ Business intelligence

---

## 🏗️ ARCHITECTURE

### 1. Table Centrale : `AuditLog`

Une seule table pour TOUT le système :

```sql
CREATE TABLE AuditLog (
    IdAudit BIGINT PRIMARY KEY AUTO_INCREMENT,
    TableName VARCHAR(100) NOT NULL,
    RecordId INT NOT NULL,
    Action VARCHAR(20) NOT NULL,
    UserId INT NOT NULL,
    UserName VARCHAR(200) NOT NULL,
    UserRole VARCHAR(50),
    IdEcole INT,
    DateAction DATETIME NOT NULL,
    OldValues TEXT,
    NewValues TEXT,
    ChangedFields VARCHAR(500),
    IpAddress VARCHAR(50),
    UserAgent VARCHAR(500),
    Commentaire TEXT,
    HttpMethod VARCHAR(10),
    Endpoint VARCHAR(500),
    DurationMs INT,
    Success BOOLEAN DEFAULT TRUE,
    ErrorMessage VARCHAR(1000),
    
    INDEX IX_AuditLog_Table_Record (TableName, RecordId),
    INDEX IX_AuditLog_UserId (UserId),
    INDEX IX_AuditLog_DateAction (DateAction),
    INDEX IX_AuditLog_IdEcole (IdEcole),
    INDEX IX_AuditLog_Action (Action)
);
```

---

## 📝 UTILISATION DANS LES CONTROLLERS

### Exemple Complet : PaiementController

```csharp
using KelasiNaBiso.Helpers;

public class PaiementController : ControllerBase
{
    private readonly IPaiementRepository _paiementRepository;
    private readonly IAuditService _auditService;

    // 1️⃣ Injecter IAuditService
    public PaiementController(
        IPaiementRepository paiementRepository, 
        IAuditService auditService)
    {
        _paiementRepository = paiementRepository;
        _auditService = auditService;
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Super-Admin")]
    public async Task<ActionResult<Paiement>> UpdatePaiement(
        int id, 
        [FromBody] UpdatePaiementDto dto)
    {
        // Validation...
        
        var existingPaiement = await _paiementRepository.GetByIdAsync(id);
        if (existingPaiement == null)
            return NotFound();

        // 2️⃣ Capturer l'état AVANT modification
        var oldPaiement = new Paiement
        {
            IdPaiement = existingPaiement.IdPaiement,
            Montant = existingPaiement.Montant,
            Commentaire = existingPaiement.Commentaire,
            StatutPaiement = existingPaiement.StatutPaiement
            // ... tous les champs modifiables
        };

        // 3️⃣ Modifier les données
        existingPaiement.Montant = dto.Montant;
        existingPaiement.Commentaire = dto.Commentaire;
        existingPaiement.StatutPaiement = dto.StatutPaiement;

        // 4️⃣ Sauvegarder
        var updated = await _paiementRepository.UpdateAsync(existingPaiement);

        // 5️⃣ Enregistrer l'audit (NOUVELLE PARTIE)
        var auditContext = this.GetAuditContext();
        await _auditService.LogUpdateAsync(
            oldPaiement,
            updated,
            auditContext.UserId,
            auditContext.UserName,
            auditContext.UserRole,
            auditContext.IdEcole,
            auditContext.IpAddress,
            auditContext.UserAgent,
            dto.Commentaire // Raison de la modification
        );

        return Ok(updated);
    }
}
```

---

## 🔧 HELPERS DISPONIBLES

### Extensions de ControllerBase

```csharp
// Dans n'importe quel controller :

// Obtenir l'ID de l'utilisateur connecté
int userId = this.GetCurrentUserId();

// Obtenir le nom de l'utilisateur
string userName = this.GetCurrentUserName();

// Obtenir le rôle
string? role = this.GetCurrentUserRole();

// Obtenir l'ID de l'école
int? schoolId = this.GetCurrentUserSchoolId();

// Obtenir l'IP du client
string? ip = this.GetClientIpAddress();

// Obtenir le User-Agent
string? userAgent = this.GetUserAgent();

// ✅ TOUT EN UNE SEULE FOIS :
var auditContext = this.GetAuditContext();
// → Contient : UserId, UserName, UserRole, IdEcole, IpAddress, UserAgent
```

---

## 🌐 API ENDPOINTS D'AUDIT

### 1. Historique d'un enregistrement

```http
GET /api/Audit/history/{tableName}/{recordId}
Authorization: Bearer {token}

Exemple:
GET /api/Audit/history/Paiement/123

Réponse:
{
  "tableName": "Paiement",
  "recordId": 123,
  "totalChanges": 5,
  "history": [
    {
      "idAudit": 5001,
      "action": "UPDATE",
      "userName": "Jean KABAMBA",
      "userRole": "Admin",
      "dateAction": "2025-11-01T14:30:45",
      "changedFields": "Montant,Commentaire",
      "oldValues": "{\"Montant\":100.00,\"Commentaire\":\"Initial\"}",
      "newValues": "{\"Montant\":50.00,\"Commentaire\":\"Ajustement\"}",
      "ipAddress": "192.168.1.50"
    },
    {
      "idAudit": 4850,
      "action": "CREATE",
      "userName": "Marie LUKUSA",
      "dateAction": "2025-10-15T09:15:30",
      "newValues": "{\"Montant\":100.00,\"IdEleve\":45,...}"
    }
  ]
}
```

### 2. Actions d'un utilisateur

```http
GET /api/Audit/user/{userId}?from=2025-10-01&to=2025-11-01&page=1&pageSize=50
Authorization: Bearer {token}

Réponse:
{
  "userId": 5,
  "from": "2025-10-01",
  "to": "2025-11-01",
  "page": 1,
  "pageSize": 50,
  "totalResults": 145,
  "actions": [ ... ]
}
```

### 3. Activités récentes

```http
GET /api/Audit/recent?limit=50&tableName=Paiement&action=UPDATE
Authorization: Bearer {token}

Réponse:
{
  "limit": 50,
  "tableName": "Paiement",
  "action": "UPDATE",
  "totalResults": 23,
  "activities": [ ... ]
}
```

### 4. Activités d'une école

```http
GET /api/Audit/school/1?from=2025-10-01&page=1&pageSize=100
Authorization: Bearer {token}

Réponse:
{
  "idEcole": 1,
  "from": "2025-10-01",
  "totalResults": 456,
  "activities": [ ... ]
}
```

### 5. Recherche avancée

```http
GET /api/Audit/search?tableName=Note&userId=5&action=UPDATE&from=2025-10-01
Authorization: Bearer {token}

Filtres disponibles:
- tableName : Nom de la table
- recordId : ID de l'enregistrement
- userId : ID de l'utilisateur
- action : CREATE, UPDATE ou DELETE
- from : Date de début
- to : Date de fin
- page : Numéro de page
- pageSize : Taille de page
```

### 6. Statistiques

```http
GET /api/Audit/statistics?from=2025-10-01&to=2025-11-01&idEcole=1
Authorization: Bearer {token}

Réponse:
{
  "totalActions": 1245,
  "creates": 345,
  "updates": 756,
  "deletes": 144,
  "actionsByTable": {
    "Paiement": 456,
    "Note": 345,
    "Inscription": 234,
    ...
  },
  "actionsByUser": {
    "5": 234,
    "12": 189,
    ...
  },
  "firstAction": "2025-10-01T08:00:00",
  "lastAction": "2025-11-01T17:30:00"
}
```

### 7. Détection d'activités suspectes (Super-Admin uniquement)

```http
GET /api/Audit/suspicious?threshold=10&windowMinutes=5
Authorization: Bearer {token}

Paramètres:
- threshold : Nombre minimum de modifications pour alerte (défaut: 10)
- windowMinutes : Fenêtre de temps en minutes (défaut: 5)

Réponse:
{
  "threshold": 10,
  "windowMinutes": 5,
  "alertCount": 2,
  "suspicious": [
    {
      "userId": 7,
      "userName": "Paul MUKENDI",
      "actions": 15,
      "details": [ ... ]
    }
  ]
}
```

### 8. Mes propres actions

```http
GET /api/Audit/me?from=2025-10-01&page=1
Authorization: Bearer {token}

Accessible à TOUS les utilisateurs authentifiés
pour voir leur propre historique d'actions.
```

---

## 🔒 SÉCURITÉ

### Contrôles d'accès :

| Endpoint | Rôles autorisés | Restrictions |
|----------|----------------|--------------|
| `GET /history/{table}/{id}` | Admin, Super-Admin | Admin = école seulement |
| `GET /user/{userId}` | Admin, Super-Admin | Admin = école seulement |
| `GET /recent` | Admin, Super-Admin | Admin = école seulement |
| `GET /school/{idEcole}` | Admin, Super-Admin | Admin = leur école uniquement |
| `GET /search` | Admin, Super-Admin | Admin = école seulement |
| `GET /statistics` | Admin, Super-Admin | Admin = école seulement |
| `GET /suspicious` | **Super-Admin UNIQUEMENT** | Toutes écoles |
| `GET /me` | **TOUS authentifiés** | Propres actions uniquement |

### Champs sensibles EXCLUS :

- ❌ `MotDePasseHash` - JAMAIS logué
- ❌ `SerialNumber` - JAMAIS logué
- ❌ `DateCreation` - Technique, pas besoin
- ❌ `DateModification` - Technique, pas besoin

---

## 📊 CAS D'USAGE

### Cas 1 : Parent conteste un paiement

**Problème** : "Je n'ai jamais payé 200$ !"

**Solution** :
```bash
GET /api/Audit/history/Paiement/456
```

**Résultat** : Vous voyez que l'admin "Jean" a modifié le montant de 100$ à 200$ le 01/11/2025 à 14h30 depuis l'IP 192.168.1.5

➡️ **Preuve légale + identification responsable**

---

### Cas 2 : Note mystérieusement changée

**Problème** : "La note de mon fils était 16/20, maintenant c'est 10/20 !"

**Solution** :
```bash
GET /api/Audit/history/Note/789
```

**Résultat** : L'enseignant "Paul" a corrigé une erreur de saisie le 30/10/2025

➡️ **Transparence totale**

---

### Cas 3 : Audit de fin de mois

**Question** : "Qui a fait quoi ce mois-ci ?"

**Solution** :
```bash
GET /api/Audit/statistics?from=2025-10-01&to=2025-10-31&idEcole=1
```

**Résultat** : Rapport complet avec graphiques par utilisateur, par table, par action

➡️ **Business Intelligence**

---

### Cas 4 : Détection de fraude

**Question** : "Y a-t-il des modifications suspectes ?"

**Solution** :
```bash
GET /api/Audit/suspicious?threshold=20&windowMinutes=10
```

**Résultat** : L'utilisateur 7 a fait 25 modifications en 8 minutes → Investigation

➡️ **Prévention fraude**

---

## 🎨 INTERFACE FRONTEND (Suggestions)

### Dashboard d'Audit

```javascript
// React/Flutter Example
<AuditDashboard>
  <RecentActivities limit={20} />
  <StatisticsChart from={lastMonth} />
  <SuspiciousAlerts threshold={10} />
  <UserActivityTable userId={currentUserId} />
</AuditDashboard>
```

### Historique sur chaque entité

```javascript
// Sur la page de détail d'un paiement
<PaiementDetails id={123}>
  <PaiementInfo />
  <AuditHistory tableName="Paiement" recordId={123} />
</PaiementDetails>

// Affiche :
// 📅 01/11/2025 14:30 - Jean KABAMBA (Admin)
//    Montant: 100.00 USD → 50.00 USD
//    Raison: "Ajustement suite erreur"
```

---

## ⚡ PERFORMANCE

### Optimisations intégrées :

1. **Index multiples** 📇
   - Requêtes ultra-rapides même avec millions d'enregistrements
   - Index composites (TableName + RecordId)

2. **Async/Await** ⚙️
   - L'audit ne ralentit PAS les opérations
   - Fire-and-forget pour performance maximale

3. **Pagination** 📄
   - Tous les endpoints supportent la pagination
   - Limite par défaut : 50 résultats

4. **JSON compact** 💾
   - Seuls les champs modifiés sont stockés
   - Compression automatique

---

## 🔧 CONFIGURATION

### appsettings.json

```json
{
  "AuditTrail": {
    "Enabled": true,
    "ExcludedTables": [],
    "ExcludedFields": [
      "MotDePasseHash",
      "SerialNumber",
      "DateCreation",
      "DateModification"
    ],
    "RetentionDays": 730,
    "EnableSuspiciousDetection": true,
    "SuspiciousThreshold": 10,
    "SuspiciousWindowMinutes": 5
  }
}
```

---

## 📋 CONTROLLERS AVEC AUDIT INTÉGRÉ

### ⭐⭐⭐ Priorité CRITIQUE (Audit actif)

1. ✅ **PaiementController** - Argent
2. ✅ **UtilisateurController** - Comptes utilisateurs
3. 🔄 **NoteController** - Résultats scolaires
4. 🔄 **InscriptionController** - Inscriptions
5. 🔄 **PresenceController** - Pointage
6. 🔄 **EcoleController** - Configuration école
7. 🔄 **ClasseController** - Gestion classes
8. 🔄 **AgentController** - Personnel
9. 🔄 **EleveController** - Élèves

### ⭐⭐ Priorité HAUTE (À implémenter)

- EvaluationController
- CoursController
- FraisController

### ⭐ Priorité NORMALE (Optionnel)

- Tous les autres controllers

---

## 📊 EXEMPLES DE REQUÊTES

### 1. Qui a modifié le paiement #123 ?

```http
GET /api/Audit/history/Paiement/123
```

### 2. Toutes les modifications de Jean ce mois-ci

```http
GET /api/Audit/user/5?from=2025-11-01&to=2025-11-30
```

### 3. Tous les paiements supprimés

```http
GET /api/Audit/search?tableName=Paiement&action=DELETE
```

### 4. Modifications dans mon école aujourd'hui

```http
GET /api/Audit/school/1?from=2025-11-01T00:00:00
```

### 5. Détection de modifications massives

```http
GET /api/Audit/suspicious?threshold=15&windowMinutes=10
```

---

## 🎯 BONNES PRATIQUES

### ✅ À FAIRE :

1. **Toujours capturer l'ancien état**
   ```csharp
   var oldEntity = new Entity { /* copier les valeurs */ };
   ```

2. **Utiliser le helper GetAuditContext()**
   ```csharp
   var ctx = this.GetAuditContext();
   ```

3. **Ajouter un commentaire pertinent**
   ```csharp
   commentaire: "Correction suite erreur de saisie"
   ```

4. **Logger aussi les CREATE et DELETE**
   ```csharp
   await _auditService.LogCreateAsync(...);
   await _auditService.LogDeleteAsync(...);
   ```

### ❌ À ÉVITER :

1. **Ne pas oublier le snapshot**
   - ❌ Modifier puis capturer = FAUX
   - ✅ Capturer puis modifier = CORRECT

2. **Ne pas logger les champs sensibles**
   - Déjà géré automatiquement

3. **Ne pas bloquer sur les erreurs d'audit**
   - L'audit ne doit JAMAIS faire échouer une opération
   - Déjà géré avec try/catch

---

## 🚀 PROCHAINES ÉTAPES

### Phase 1 : Intégration (MAINTENANT)
- ✅ PaiementController
- ✅ UtilisateurController
- 🔄 7 autres controllers critiques

### Phase 2 : Tests (1 jour)
- ✅ Script PowerShell de test complet
- ✅ Validation de tous les scénarios

### Phase 3 : Production (Après tests)
- ✅ Migration de la base de données
- ✅ Monitoring des performances

### Phase 4 : Dashboard (Futur)
- 📊 Interface graphique pour les admins
- 📈 Graphiques et statistiques
- 🔔 Alertes automatiques

---

## 📞 SUPPORT

### En cas de problème :

1. Vérifier les logs : `logs/audit-*.log`
2. Consulter la table : `SELECT * FROM AuditLog ORDER BY DateAction DESC LIMIT 100`
3. Désactiver temporairement : `AuditTrail:Enabled = false` dans appsettings.json

---

## ✨ BÉNÉFICES FINAUX

✅ **Traçabilité complète** - Qui, Quoi, Quand, Où, Pourquoi  
✅ **Conformité légale** - Preuves en cas de litige  
✅ **Sécurité renforcée** - Détection rapide des anomalies  
✅ **Support facilité** - Debugging rapide des problèmes  
✅ **Business Intelligence** - Analyse des comportements  
✅ **Transparence** - Confiance des utilisateurs  

---

*Guide créé le 1er novembre 2025*  
*Version : 1.0*  
*Statut : ✅ IMPLÉMENTATION EN COURS*

