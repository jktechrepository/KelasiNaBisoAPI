# 🧪 TESTS D'INTÉGRATION - KelasiNaBisoAPI

## 📋 Fonctionnalités intégrées d'AkademiaAPI

Les 3 fonctionnalités suivantes ont été intégrées avec succès :

1. ✅ **Notifications Push** (Firebase + Email + SignalR)
2. ✅ **Création automatique de comptes utilisateur** (Tuteur/Parent + Agent)
3. ✅ **Changement de mot de passe**

---

## 🔗 Accès à l'application

- **API** : `http://localhost:5002` ou `https://localhost:7102`
- **Swagger UI** : `http://localhost:5002/swagger`
- **SignalR Hub** : `http://localhost:5002/notificationHub`

---

## 📱 POINT 1 : Tests des Notifications Push

### 1.1 Enregistrer un appareil (FCM Token)

**Endpoint** : `POST /api/UserDevice/register`

**Body** :
```json
{
  "idUtilisateur": 1,
  "fcmToken": "test-fcm-token-12345",
  "deviceType": "Android",
  "deviceModel": "Samsung Galaxy S21",
  "osVersion": "13"
}
```

### 1.2 Envoyer une notification push à un utilisateur

**Endpoint** : `POST /api/NotificationPush/utilisateur/{idUtilisateur}`

**Body** :
```json
{
  "titre": "Test de notification",
  "corps": "Ceci est une notification de test pour vérifier Firebase",
  "typeNotification": "INFO",
  "lienAction": "/dashboard",
  "icone": "bell"
}
```

### 1.3 Envoyer une notification à une école

**Endpoint** : `POST /api/NotificationPush/ecole/{idEcole}`

**Body** :
```json
{
  "titre": "Annonce importante",
  "corps": "Réunion des parents le 25 octobre à 10h",
  "typeNotification": "WARNING"
}
```

### 1.4 Récupérer les notifications d'un utilisateur

**Endpoint** : `GET /api/Notification/utilisateur/{idUtilisateur}`

---

## 👤 POINT 2 : Tests de Création Automatique de Comptes

### 2.1 Test - Inscription d'un nouvel élève avec nouveau tuteur

**Endpoint** : `POST /api/Inscription/new/{nomEcole}`

**URL** : `POST /api/Inscription/new/Institut%20Kelasi`

**Body** :
```json
{
  "type": "Inscription",
  "idEcole": 1,
  "idClasse": 1,
  "idAnneeScolaire": 1,
  "dateInscription": "2025-10-23T00:00:00",
  "statutInscription": "Confirmée",
  "nomEleve": "MUKENDI",
  "postnomEleve": "KALALA",
  "prenomEleve": "Jean",
  "genreEleve": "Masculin",
  "dateNaissanceEleve": "2010-05-15T00:00:00",
  "lieuNaissanceEleve": "Kinshasa",
  "nationaliteEleve": "Congolaise",
  "provinceEleve": "Kinshasa",
  "villeEleve": "Kinshasa",
  "communeEleve": "Lemba",
  "quartierEleve": "Righini",
  "avenueEleve": "Université",
  "numeroEleve": "123",
  "nomCompletTuteur": "MUKENDI Marie",
  "genreTuteur": "Féminin",
  "emailTuteur": "marie.mukendi@gmail.com",
  "telephoneTuteur": "+243812345678"
}
```

**Résultat attendu** :
- ✅ Élève créé
- ✅ Tuteur créé
- ✅ **Compte utilisateur Parent créé automatiquement** avec :
  - DefaultUsername : Format `T{4 caractères}{2025}` (ex: `TA7K92025`)
  - Mot de passe par défaut : `123456`
  - DoitChangerMotDePasse : `true`
  - Email de bienvenue envoyé
- ✅ Inscription créée
- ✅ Retour de `CompteUtilisateurTuteur` dans la réponse

### 2.2 Test - Création d'un nouvel agent

**Endpoint** : `POST /api/Agent`

**Body** :
```json
{
  "matricule": "AGT001",
  "nom": "KABONGO",
  "postnom": "MBUYI",
  "prenom": "Pierre",
  "genre": "Masculin",
  "dateNaissance": "1985-03-20T00:00:00",
  "telephoneAgent": "+243823456789",
  "emailAgent": "pierre.kabongo@kelasi.cd",
  "etatCivil": "Marié",
  "fonction": "Enseignant",
  "roleAgent": "Professeur",
  "idEcole": 1,
  "provinceEleve": "Kinshasa",
  "ville": "Kinshasa",
  "commune": "Ngaliema",
  "quartier": "Joli Parc",
  "avenue": "Université",
  "numero": "45"
}
```

**Résultat attendu** :
- ✅ Agent créé
- ✅ **Compte utilisateur Agent créé automatiquement** avec :
  - DefaultUsername : Format `A{4 caractères}{2025}` (ex: `AZ3P42025`)
  - Mot de passe par défaut : `123456`
  - DoitChangerMotDePasse : `true`
  - Email de bienvenue envoyé

### 2.3 Vérifier les comptes créés

**Endpoint** : `GET /api/Utilisateur`

**Vérifications** :
- Les nouveaux utilisateurs Parent et Agent sont présents
- Ils ont un `DefaultUsername` unique
- Le champ `DoitChangerMotDePasse` est à `true`
- Le `MotDePasseHash` est bien hashé (BCrypt)

---

## 🔒 POINT 3 : Tests du Changement de Mot de Passe

### 3.1 Test - Changement de mot de passe réussi

**Endpoint** : `POST /api/Utilisateur/changer_mot_de_passe`

**Body** :
```json
{
  "idUtilisateur": 1,
  "ancienMotDePasse": "123456",
  "nouveauMotDePasse": "MonNouveauMotDePasse2025!",
  "confirmerNouveauMotDePasse": "MonNouveauMotDePasse2025!"
}
```

**Résultat attendu** :
- ✅ Code 200 OK
- ✅ Message : "Mot de passe changé avec succès"
- ✅ Le champ `DoitChangerMotDePasse` passe à `false`

### 3.2 Test - Ancien mot de passe incorrect

**Body** :
```json
{
  "idUtilisateur": 1,
  "ancienMotDePasse": "mauvais_mot_de_passe",
  "nouveauMotDePasse": "NouveauMotDePasse123!",
  "confirmerNouveauMotDePasse": "NouveauMotDePasse123!"
}
```

**Résultat attendu** :
- ✅ Code 400 Bad Request
- ✅ Message : "Ancien mot de passe incorrect ou utilisateur non trouvé"

### 3.3 Test - Confirmation ne correspond pas

**Body** :
```json
{
  "idUtilisateur": 1,
  "ancienMotDePasse": "123456",
  "nouveauMotDePasse": "NouveauMotDePasse123!",
  "confirmerNouveauMotDePasse": "AutreMotDePasse!"
}
```

**Résultat attendu** :
- ✅ Code 400 Bad Request
- ✅ Erreur de validation : "La confirmation du mot de passe ne correspond pas"

### 3.4 Test - Nouveau mot de passe trop court

**Body** :
```json
{
  "idUtilisateur": 1,
  "ancienMotDePasse": "123456",
  "nouveauMotDePasse": "12",
  "confirmerNouveauMotDePasse": "12"
}
```

**Résultat attendu** :
- ✅ Code 400 Bad Request
- ✅ Erreur de validation : "Le nouveau mot de passe doit contenir au moins 3 caractères"

---

## 🔄 Flux complet de test

### Scénario 1 : Nouveau Parent

1. **Inscrire un nouvel élève** avec un nouveau tuteur → Compte Parent créé automatiquement
2. **Vérifier l'email** → Email de bienvenue reçu avec username et mot de passe
3. **Se connecter** avec `DefaultUsername` et mot de passe `123456`
4. **Vérifier** que `DoitChangerMotDePasse = true`
5. **Changer le mot de passe** via `/api/Utilisateur/changer_mot_de_passe`
6. **Se reconnecter** avec le nouveau mot de passe
7. **Vérifier** que `DoitChangerMotDePasse = false`

### Scénario 2 : Nouvel Agent

1. **Créer un nouvel agent** → Compte Agent créé automatiquement
2. **Vérifier l'email** → Email de bienvenue reçu avec username et mot de passe
3. **Se connecter** avec `DefaultUsername` et mot de passe `123456`
4. **Vérifier** que `DoitChangerMotDePasse = true`
5. **Changer le mot de passe** via `/api/Utilisateur/changer_mot_de_passe`
6. **Vérifier** que `DoitChangerMotDePasse = false`

---

## 📊 Endpoints principaux à tester

### Notifications
- `POST /api/NotificationPush/utilisateur/{id}` - Envoyer notification à un utilisateur
- `POST /api/NotificationPush/ecole/{id}` - Envoyer notification à une école
- `POST /api/NotificationPush/classe/{id}` - Envoyer notification à une classe
- `POST /api/UserDevice/register` - Enregistrer un appareil
- `GET /api/Notification/utilisateur/{id}` - Récupérer les notifications

### Inscription et Comptes
- `POST /api/Inscription/new/{nomEcole}` - Nouvelle inscription (crée compte Parent)
- `POST /api/Agent` - Créer un agent (crée compte Agent)
- `GET /api/Utilisateur` - Liste des utilisateurs

### Changement de mot de passe
- `POST /api/Utilisateur/changer_mot_de_passe` - Changer le mot de passe

---

## ✅ Points de vérification

### Pour les notifications
- [ ] Firebase token enregistré correctement
- [ ] Notification envoyée avec succès
- [ ] Email de bienvenue reçu
- [ ] Notification SignalR reçue en temps réel

### Pour la création de comptes
- [ ] Compte Parent créé lors de l'inscription
- [ ] Compte Agent créé lors de la création d'un agent
- [ ] `DefaultUsername` unique généré
- [ ] `DoitChangerMotDePasse = true` par défaut
- [ ] Mot de passe hashé avec BCrypt
- [ ] Email de bienvenue envoyé

### Pour le changement de mot de passe
- [ ] Ancien mot de passe vérifié correctement
- [ ] Nouveau mot de passe validé (longueur minimale)
- [ ] Confirmation du mot de passe vérifiée
- [ ] `DoitChangerMotDePasse` passe à `false` après changement
- [ ] Nouveau mot de passe hashé avec BCrypt

---

## 🎯 Résultat final attendu

**Toutes les fonctionnalités doivent être opérationnelles** :
- ✅ Notifications push (Firebase + Email + SignalR)
- ✅ Création automatique de comptes (Parent + Agent)
- ✅ Changement de mot de passe avec flag `DoitChangerMotDePasse`

---

**Date de test** : 23 octobre 2025  
**Version** : KelasiNaBisoAPI avec intégration complète d'AkademiaAPI

