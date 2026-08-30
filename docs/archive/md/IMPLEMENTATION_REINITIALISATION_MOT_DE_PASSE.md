# ✅ Implémentation - Réinitialisation mot de passe

**Date :** 2025-11-06  
**Fonctionnalité :** Réinitialisation de mot de passe (masse + individuelle)

---

## 🎯 **Objectif**

Permettre aux Admin et Super-Admin de réinitialiser les mots de passe :
1. **En masse** : Tous les utilisateurs d'une école avec un rôle spécifique
2. **Individuellement** : Un utilisateur spécifique

---

## 📋 **Endpoints créés**

### **1. Réinitialisation en masse**

```http
POST /api/Utilisateur/reinitialiser-masse
Authorization: Bearer {token}
Content-Type: application/json

{
  "idEcole": 18,
  "idRole": 3,
  "nouveauMotDePasse": "EcoleNAT2025!"
}
```

**Réponse (200 OK) :**
```json
{
  "success": true,
  "message": "45 utilisateur(s) réinitialisé(s) avec succès",
  "nombreUtilisateurs": 45,
  "details": {
    "ecole": "Ekelasi School",
    "role": "Parent",
    "motDePasseChange": true,
    "doitChangerMotDePasse": true
  }
}
```

---

### **2. Réinitialisation individuelle**

```http
POST /api/Utilisateur/reinitialiser-un
Authorization: Bearer {token}
Content-Type: application/json

{
  "idUtilisateur": 382,
  "nouveauMotDePasse": "NouveauPass2025!"
}
```

**Réponse (200 OK) :**
```json
{
  "success": true,
  "message": "Mot de passe réinitialisé avec succès",
  "utilisateur": {
    "idUtilisateur": 382,
    "nomComplet": "kansa de kansa",
    "email": "kansa@example.com",
    "telephone": "+243825099299",
    "doitChangerMotDePasse": true
  }
}
```

---

## 🔒 **Règles de sécurité implémentées**

### **Point 1 : Pas de paramètre `IdRole` dans réinitialisation individuelle**
✅ Implémenté - Le 2ème endpoint ne demande que `IdUtilisateur` + `NouveauMotDePasse`

### **Point 2 : Forcer changement de mot de passe**
✅ Implémenté - `DoitChangerMotDePasse = true` après réinitialisation

### **Point 4 : Restrictions de sécurité**

| Utilisateur | Peut réinitialiser | Ne peut PAS réinitialiser |
|-------------|-------------------|---------------------------|
| **Super-Admin** | ✅ Tous les utilisateurs (toutes écoles) | N/A |
| **Admin** | ✅ Utilisateurs de son école (Parents, Enseignants, etc.) | ❌ Super-Admin<br>❌ Autres Admin<br>❌ Utilisateurs d'autres écoles |
| **Autres rôles** | ❌ Aucun | Tous |

---

## 🛡️ **Contrôles de sécurité**

### **Réinitialisation en masse :**

1. ✅ Vérification token JWT (utilisateur authentifié)
2. ✅ Vérification rôle (Admin ou Super-Admin uniquement)
3. ✅ Admin : Vérification `IdEcole` correspond à son école
4. ✅ Admin : Blocage si `RoleCible` = "Super-Admin" ou "Admin"
5. ✅ Vérification existence école
6. ✅ Vérification existence rôle
7. ✅ Logs détaillés (tentatives, succès, refus)

### **Réinitialisation individuelle :**

1. ✅ Vérification token JWT (utilisateur authentifié)
2. ✅ Vérification rôle (Admin ou Super-Admin uniquement)
3. ✅ Vérification existence utilisateur cible
4. ✅ Admin : Vérification `IdEcole` correspond à son école
5. ✅ Admin : Blocage si cible = "Super-Admin" ou "Admin"
6. ✅ Logs détaillés (tentatives, succès, refus)

---

## 📊 **Comportement après réinitialisation**

### **Pour l'utilisateur réinitialisé :**

1. **Premier login après réinitialisation :**
   ```json
   {
     "doitChangerMotDePasse": true
   }
   ```

2. **Frontend doit afficher :**
   ```
   🔐 Changement de mot de passe requis
   
   Votre mot de passe a été réinitialisé par un administrateur.
   Pour des raisons de sécurité, veuillez choisir un nouveau mot de passe.
   
   [Formulaire de changement de mot de passe]
   ```

3. **Après changement du mot de passe :**
   ```json
   {
     "doitChangerMotDePasse": false
   }
   ```

---

## 🧪 **Exemples d'utilisation**

### **Cas 1 : Rentrée scolaire (réinitialisation en masse)**

**Contexte :** L'admin de l'école veut réinitialiser tous les parents pour la nouvelle année.

**Requête :**
```http
POST /api/Utilisateur/reinitialiser-masse
Authorization: Bearer {admin_token}

{
  "idEcole": 18,
  "idRole": 3,  // Rôle "Parent"
  "nouveauMotDePasse": "Rentree2025!"
}
```

**Résultat :** 
- 127 parents réinitialisés
- Tous doivent changer leur mot de passe au premier login

---

### **Cas 2 : Oubli de mot de passe (réinitialisation individuelle)**

**Contexte :** Un parent a oublié son mot de passe.

**Requête :**
```http
POST /api/Utilisateur/reinitialiser-un
Authorization: Bearer {admin_token}

{
  "idUtilisateur": 382,
  "nouveauMotDePasse": "TempPass2025!"
}
```

**Résultat :**
- Mot de passe de l'utilisateur 382 réinitialisé
- Il doit le changer au premier login

---

### **Cas 3 : Tentative non autorisée (Admin → Super-Admin)**

**Requête :**
```http
POST /api/Utilisateur/reinitialiser-un
Authorization: Bearer {admin_token}

{
  "idUtilisateur": 296,  // Super-Admin
  "nouveauMotDePasse": "HackAttempt!"
}
```

**Réponse (403 Forbidden) :**
```json
{
  "message": "Vous ne pouvez pas réinitialiser le mot de passe des Super-Admin"
}
```

**Log :**
```
⛔ Accès refusé: Admin (User 300) a essayé de réinitialiser un Super-Admin (User 296)
```

---

## 📂 **Fichiers créés/modifiés**

| Fichier | Type | Description |
|---------|------|-------------|
| `Models/DTOs/ReinitialiserMotDePasseMasseDto.cs` | Créé | DTOs + Réponses |
| `Services/Repositories/IUtilisateurRepository.cs` | Modifié | Ajout 2 méthodes |
| `Services/UtilisateurService.cs` | Modifié | Implémentation 2 méthodes |
| `Controllers/UtilisateurController.cs` | Modifié | Ajout 2 endpoints + sécurité |

---

## ✅ **Validation**

- [x] DTOs créés avec validation `[Required]` et `[MinLength(6)]`
- [x] Interface `IUtilisateurRepository` mise à jour
- [x] Implémentation dans `UtilisateurService`
- [x] Endpoints dans `UtilisateurController` avec sécurité
- [x] Logs détaillés pour audit
- [x] `DoitChangerMotDePasse = true` après réinitialisation
- [x] Restrictions Admin vs Super-Admin
- [x] Compilation réussie (0 erreurs)

---

## 🚀 **Prochaines étapes**

1. Tester les endpoints dans Swagger
2. Vérifier les logs de sécurité
3. Tester les restrictions de rôles
4. Documenter pour le frontend

---

**Implémentation terminée ! Prêt pour les tests ! ✅**

