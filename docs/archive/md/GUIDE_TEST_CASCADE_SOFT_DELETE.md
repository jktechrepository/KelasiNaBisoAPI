# 🧪 Guide de Test : Cascade Soft Delete Élève → Inscriptions

**Date** : 2025-01-16  
**Version** : 1.0  
**Objectif** : Valider que la désactivation d'un élève désactive automatiquement ses inscriptions actives

---

## 📋 Prérequis

1. ✅ API en cours d'exécution (`dotnet run`)
2. ✅ Base de données accessible
3. ✅ Identifiants de connexion valides
4. ✅ Au moins un élève actif avec des inscriptions actives dans la base

---

## 🔧 Outils de Test

### **Option 1 : Fichier HTTP (Recommandé)**

Utiliser le fichier `test-cascade-soft-delete-eleve.http` avec l'extension REST Client dans VS Code.

### **Option 2 : Postman / Insomnia**

Importer les requêtes depuis le fichier HTTP.

### **Option 3 : curl / Terminal**

Utiliser les commandes curl fournies ci-dessous.

---

## 📝 Étapes de Test

### **ÉTAPE 1 : Authentification** 🔐

**Requête** :
```http
POST http://localhost:5000/api/Auth/login
Content-Type: application/json

{
  "emailOuTelephone": "jk2@kelasinabiso.cd",
  "motDePasse": "12345678"
}
```

**Résultat attendu** :
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "...",
  "utilisateur": { ... }
}
```

**Action** : Copier le `token` pour les requêtes suivantes.

---

### **ÉTAPE 2 : Identifier un élève avec inscriptions actives** 🔍

**Requête** :
```http
GET http://localhost:5000/api/Eleve/ecole/{idEcole}?pageNumber=1&pageSize=10
Authorization: Bearer {token}
```

**Action** : 
1. Choisir un `IdEcole` valide
2. Parcourir la liste des élèves
3. Noter un `IdEleve` qui a probablement des inscriptions actives

**Alternative** : Utiliser l'endpoint des inscriptions pour trouver un élève :
```http
GET http://localhost:5000/api/Inscription/ecoles/{idEcole}/paged?pageNumber=1&pageSize=10
Authorization: Bearer {token}
```

---

### **ÉTAPE 3 : Vérifier les inscriptions AVANT désactivation** 📊

**Requête** :
```http
GET http://localhost:5000/api/Inscription/eleve/{idEleve}?pageNumber=1&pageSize=10
Authorization: Bearer {token}
```

**Résultat attendu** :
```json
{
  "items": [
    {
      "idInscription": 123,
      "idEleve": {idEleve},
      "statut": true,  // ← Inscription active
      ...
    },
    {
      "idInscription": 456,
      "idEleve": {idEleve},
      "statut": true,  // ← Inscription active
      ...
    }
  ],
  "totalCount": 2,
  ...
}
```

**Action** : 
- ✅ Noter le nombre d'inscriptions actives (`statut: true`)
- ✅ Noter les `IdInscription` concernés
- ✅ Prendre une capture d'écran si possible

**Exemple** : Si vous voyez 2 inscriptions actives, notez `NombreInscriptionsActivesAvant = 2`.

---

### **ÉTAPE 4 : Désactiver l'élève (CASCADE AUTOMATIQUE)** 🔄

**Requête** :
```http
PUT http://localhost:5000/api/Eleve/toggle-statut/{idEleve}
Authorization: Bearer {token}
```

**Résultat attendu** :
```json
{
  "message": "Statut modifié avec succès",
  "eleve": {
    "idEleve": {idEleve},
    "statut": false,  // ← Élève désactivé
    ...
  },
  "nouveauStatut": false
}
```

**Vérifications** :

1. **Dans la réponse API** :
   - ✅ `eleve.statut` doit être `false`
   - ✅ Message de succès retourné

2. **Dans les logs de l'application** (console) :
   ```
   ✅ Statut de l'élève ID: {idEleve} modifié de True à False
   🔄 Désactivation automatique des inscriptions de l'élève ID: {idEleve} (cascade logicielle)
   🔄 Désactivation des inscriptions actives pour l'élève ID: {idEleve}
      → Inscription #123 désactivée
      → Inscription #456 désactivée
   ✅ 2 inscription(s) désactivée(s) pour l'élève ID: {idEleve}
   ✅ 2 inscription(s) désactivée(s) automatiquement pour l'élève ID: {idEleve}
   ```

3. **Si aucune inscription active** :
   ```
   ℹ️ Aucune inscription active trouvée pour l'élève ID: {idEleve}
   ✅ 0 inscription(s) désactivée(s) automatiquement pour l'élève ID: {idEleve}
   ```

---

### **ÉTAPE 5 : Vérifier les inscriptions APRÈS désactivation** ✅

**Requête** :
```http
GET http://localhost:5000/api/Inscription/eleve/{idEleve}?pageNumber=1&pageSize=10
Authorization: Bearer {token}
```

**Résultat attendu** :

**Si l'endpoint filtre par défaut sur `Statut = true`** :
```json
{
  "items": [],  // ← Aucune inscription active
  "totalCount": 0,
  ...
}
```

**Si vous voulez voir les inscriptions inactives** :
```http
GET http://localhost:5000/api/Inscription/eleve/{idEleve}?pageNumber=1&pageSize=10&includeInactive=true
Authorization: Bearer {token}
```

**Résultat attendu** :
```json
{
  "items": [
    {
      "idInscription": 123,
      "idEleve": {idEleve},
      "statut": false,  // ← Désactivée automatiquement
      ...
    },
    {
      "idInscription": 456,
      "idEleve": {idEleve},
      "statut": false,  // ← Désactivée automatiquement
      ...
    }
  ],
  "totalCount": 2,
  ...
}
```

**Vérifications** :
- ✅ Toutes les inscriptions précédemment actives ont maintenant `statut: false`
- ✅ Le nombre d'inscriptions actives est 0 (ou filtrées par défaut)
- ✅ Le nombre total d'inscriptions reste le même (seul le statut change)

---

### **ÉTAPE 6 : Vérifier que l'élève est bien désactivé** 🔍

**Requête** :
```http
GET http://localhost:5000/api/Eleve/{idEleve}
Authorization: Bearer {token}
```

**Résultat attendu** :

**Si l'endpoint filtre par défaut sur `Statut = true`** :
```json
{
  "status": 404,
  "message": "Élève non trouvé"
}
```
✅ **C'est normal** : L'élève désactivé est filtré par défaut.

**Si l'endpoint ne filtre pas** :
```json
{
  "idEleve": {idEleve},
  "statut": false,  // ← Élève désactivé
  ...
}
```

---

### **ÉTAPE 7 : Réactiver l'élève (PAS DE CASCADE INVERSE)** 🔄

**Requête** :
```http
PUT http://localhost:5000/api/Eleve/toggle-statut/{idEleve}
Authorization: Bearer {token}
```

**Résultat attendu** :
```json
{
  "message": "Statut modifié avec succès",
  "eleve": {
    "idEleve": {idEleve},
    "statut": true,  // ← Élève réactivé
    ...
  },
  "nouveauStatut": true
}
```

**Vérifications** :
- ✅ L'élève est réactivé (`statut: true`)
- ✅ **Aucun log de cascade** dans la console (pas de désactivation/réactivation d'inscriptions)
- ✅ Les inscriptions restent inactives (pas de cascade inverse)

---

### **ÉTAPE 8 : Vérifier les inscriptions après réactivation** ✅

**Requête** :
```http
GET http://localhost:5000/api/Inscription/eleve/{idEleve}?pageNumber=1&pageSize=10&includeInactive=true
Authorization: Bearer {token}
```

**Résultat attendu** :
```json
{
  "items": [
    {
      "idInscription": 123,
      "idEleve": {idEleve},
      "statut": false,  // ← Reste inactive (pas de cascade inverse)
      ...
    },
    {
      "idInscription": 456,
      "idEleve": {idEleve},
      "statut": false,  // ← Reste inactive (pas de cascade inverse)
      ...
    }
  ],
  "totalCount": 2,
  ...
}
```

**Vérifications** :
- ✅ Les inscriptions restent inactives (`statut: false`)
- ✅ Pas de réactivation automatique (décision métier)

---

## 📊 Vérification SQL (Optionnel)

Pour une vérification complète, exécuter le script SQL :

**Fichier** : `SCRIPTS_SQL/analyser_incoherence_eleves_inscriptions.sql`

**Vérifier** :
- ✅ `TotalInscriptionsActivesAvecElevesActifs` = `TotalElevesActifsAvecInscriptionsActives`
- ✅ Aucune inscription active pour un élève inactif

---

## ✅ Checklist de Validation

### **Test 1 : Cascade lors de la désactivation**

- [ ] L'élève est désactivé (`statut: false`)
- [ ] Toutes les inscriptions actives sont désactivées automatiquement
- [ ] Les logs montrent le nombre d'inscriptions désactivées
- [ ] Le nombre d'inscriptions actives passe à 0

### **Test 2 : Pas de cascade inverse lors de la réactivation**

- [ ] L'élève est réactivé (`statut: true`)
- [ ] Les inscriptions restent inactives (`statut: false`)
- [ ] Aucun log de cascade dans la console

### **Test 3 : Cas limite - Élève sans inscriptions**

- [ ] Désactiver un élève sans inscriptions actives
- [ ] Aucune erreur levée
- [ ] Log : "Aucune inscription active trouvée"

### **Test 4 : Performance**

- [ ] Désactiver un élève avec beaucoup d'inscriptions (> 10)
- [ ] Vérifier que toutes sont désactivées
- [ ] Temps d'exécution acceptable (< 5 secondes)

---

## 🐛 Dépannage

### **Problème 1 : Token expiré**

**Symptôme** : `401 Unauthorized`

**Solution** : Ré-authentifier et mettre à jour le token.

---

### **Problème 2 : Élève non trouvé**

**Symptôme** : `404 Not Found` lors de la récupération de l'élève après désactivation

**Solution** : C'est normal si l'endpoint filtre par défaut sur `Statut = true`. Utiliser `includeInactive=true` si disponible.

---

### **Problème 3 : Aucune inscription désactivée**

**Symptôme** : Log indique "0 inscription(s) désactivée(s)"

**Vérifications** :
1. L'élève avait-il vraiment des inscriptions actives avant ?
2. Vérifier avec `includeInactive=true` pour voir toutes les inscriptions

---

### **Problème 4 : Erreur lors de la cascade**

**Symptôme** : Exception dans les logs

**Solution** :
1. Vérifier les logs détaillés
2. Vérifier que `IInscriptionRepository` est bien injecté dans `EleveService`
3. Vérifier la connexion à la base de données

---

## 📝 Résultats Attendus

### **Scénario Réussi**

```
✅ Élève désactivé : ID 123
✅ Inscriptions désactivées automatiquement : 2
✅ Logs : "✅ 2 inscription(s) désactivée(s) automatiquement pour l'élève ID: 123"
✅ Vérification : 0 inscription active après désactivation
✅ Réactivation : Élève réactivé, inscriptions restent inactives
```

---

## 📚 Fichiers de Test

- **Fichier HTTP** : `test-cascade-soft-delete-eleve.http`
- **Guide** : `GUIDE_TEST_CASCADE_SOFT_DELETE.md` (ce fichier)
- **Documentation** : `IMPLEMENTATION_CASCADE_SOFT_DELETE_ELEVE_INSCRIPTION.md`

---

**Version** : 1.0  
**Date** : 2025-01-16  
**Statut** : ✅ Prêt pour tests
