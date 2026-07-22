# 📚 Documentation API KelasiNaBiso

## 🌟 Vue d'ensemble

L'API KelasiNaBiso est une API REST complète pour la gestion d'un système scolaire. Elle permet de gérer les écoles, les élèves, les enseignants, les classes, les notes, les présences, les paiements et bien plus encore.

### 🔗 Informations de base
- **URL de base (Développement)**: `http://192.168.100.17:5001` (HTTP)
- **URL de base (Production)**: `https://localhost:7102` (HTTPS) ou `http://localhost:5002` (HTTP)
- **Format des données**: JSON
- **Authentification**: Par email/téléphone et mot de passe
- **Documentation Swagger**: `http://192.168.100.17:5001/swagger` (dev) ou `https://localhost:7102/swagger` (prod)

---

## 🔐 Authentification

### Connexion utilisateur
**POST** `/api/Utilisateur/authentifier`

Authentifie un utilisateur avec son email/téléphone et mot de passe. **Important** : Seuls les utilisateurs avec `statut = true` peuvent s'authentifier.

#### Paramètres de requête
```json
{
  "emailOuTelephone": "string",  // Email ou numéro de téléphone
  "motDePasse": "string"         // Mot de passe en clair
}
```

#### Exemple de requête
```bash
# Développement
curl -X POST "http://192.168.100.17:5001/api/Utilisateur/authentifier" \
  -H "Content-Type: application/json" \
  -d '{
    "emailOuTelephone": "admin@example.com",
    "motDePasse": "password123"
  }'

# Production
curl -X POST "https://localhost:7155/api/Utilisateur/authentifier" \
  -H "Content-Type: application/json" \
  -d '{
    "emailOuTelephone": "admin@example.com",
    "motDePasse": "password123"
  }'
```

#### Réponse de succès (200)
```json
{
  "idUtilisateur": 1,
  "referenceUtilisateur": "550e8400-e29b-41d4-a716-446655440000",
  "nomUtilisateur": "Doe",
  "postNomUtilisateur": "Smith",
  "prenomUtilisateur": "John",
  "email": "admin@example.com",
  "téléphone": "+243123456789",
  "photoUrl": "https://example.com/photo.jpg",
  "lieuNaissance": "Kinshasa",
  "dateNaissance": "1990-01-01T00:00:00",
  "genre": "M",
  "statut": true,
  "dateCreation": "2024-01-01T00:00:00",
  "isConnecte": true,
  "province": "Kinshasa",
  "ville": "Kinshasa",
  "commune": "Gombe",
  "quartier": "Centre-ville",
  "avenue": "Avenue du Commerce",
  "numero": "123",
  "idRole": 1,
  "nomRole": "Administrateur",
  "dateCreationRole": "2024-01-01T00:00:00",
  "idEcole": 1,
  "nomEcole": "Ecole KelasiNaBiso",
  "sloganEcole": "L'éducation pour tous",
  "typeEcole": "Privée",
  "logoUrlEcole": "https://example.com/logo.jpg",
  "téléphoneEcole": "+243123456789",
  "emailContactEcole": "contact@ecole.com",
  "siteWebEcole": "https://ecole.com",
  "capaciteEleveEcole": 500,
  "nombreSallesEcole": 20,
  "descriptionEcole": "Une école d'excellence",
  "dateCréationEcole": "2024-01-01T00:00:00",
  "provinceEcole": "Kinshasa",
  "villeEcole": "Kinshasa",
  "communeEcole": "Gombe",
  "quartierEcole": "Centre-ville",
  "avenueEcole": "Avenue du Commerce",
  "numeroEcole": "123"
}
```

#### Codes d'erreur
- **400**: Données de requête invalides
- **401**: Email/téléphone ou mot de passe incorrect, ou compte désactivé (statut = false)
- **404**: Informations utilisateur non trouvées
- **500**: Erreur serveur

#### Messages d'erreur spécifiques
- `"Email/Téléphone ou mot de passe incorrect"` : Identifiants incorrects
- `"Compte désactivé"` : Utilisateur trouvé mais avec statut = false
- `"Compte non configuré correctement"` : Mot de passe manquant

### Changer le mot de passe
**POST** `/api/Utilisateur/changer_mot_de_passe`

Permet à un utilisateur de changer son mot de passe en vérifiant l'ancien mot de passe.

#### Paramètres de requête
```json
{
  "idUtilisateur": 1,
  "ancienMotDePasse": "string",        // Ancien mot de passe
  "nouveauMotDePasse": "string",       // Nouveau mot de passe (min 3 caractères)
  "confirmerNouveauMotDePasse": "string" // Confirmation du nouveau mot de passe
}
```

#### Exemple de requête
```bash
curl -X POST "http://192.168.100.17:5001/api/Utilisateur/changer_mot_de_passe" \
  -H "Content-Type: application/json" \
  -d '{
    "idUtilisateur": 1,
    "ancienMotDePasse": "password123",
    "nouveauMotDePasse": "nouveauPassword456",
    "confirmerNouveauMotDePasse": "nouveauPassword456"
  }'
```

#### Réponse de succès (200)
```json
{
  "message": "Mot de passe changé avec succès"
}
```

#### Codes d'erreur
- **400**: Données de requête invalides, ancien mot de passe incorrect, ou confirmation incorrecte
- **404**: Utilisateur non trouvé
- **500**: Erreur serveur

#### Messages d'erreur spécifiques
- `"Ancien mot de passe incorrect ou utilisateur non trouvé"` : L'ancien mot de passe ne correspond pas
- `"La confirmation du mot de passe ne correspond pas"` : Les nouveaux mots de passe ne correspondent pas
- `"Le nouveau mot de passe doit contenir au moins 3 caractères"` : Mot de passe trop court

---

## 👥 Gestion des Utilisateurs

### Récupérer tous les utilisateurs
**GET** `/api/Utilisateur`

### Récupérer un utilisateur par ID
**GET** `/api/Utilisateur/{id}`

### Récupérer un utilisateur par email
**GET** `/api/Utilisateur/email/{email}`

### Récupérer les utilisateurs par rôle
**GET** `/api/Utilisateur/role/{roleId}`

### Récupérer les utilisateurs par statut
**GET** `/api/Utilisateur/statut/{statut}`

### Récupérer les utilisateurs d'une école
**GET** `/api/Utilisateur/ecole/{idEcole}`

### Créer un utilisateur
**POST** `/api/Utilisateur`

#### Modèle Utilisateur
```json
{
  "nomUtilisateur": "string",
  "postNomUtilisateur": "string",
  "prenomUtilisateur": "string",
  "email": "string",
  "téléphone": "string",
  "photoUrl": "string",
  "lieuNaissance": "string",
  "dateNaissance": "2024-01-01T00:00:00",
  "genre": "string",
  "statut": true,
  "motDePasseHash": "string",
  "idRole": 1,
  "idEcole": 1,
  "province": "string",
  "ville": "string",
  "commune": "string",
  "quartier": "string",
  "avenue": "string",
  "numero": "string"
}
```

### Modifier un utilisateur
**PUT** `/api/Utilisateur/{id}`

**⚠️ Important** : Cette méthode ne peut pas modifier le mot de passe. Pour changer le mot de passe, utilisez l'endpoint dédié `POST /api/Utilisateur/changer_mot_de_passe`.

#### Modèle Utilisateur (pour modification)
```json
{
  "idUtilisateur": 1,
  "nomUtilisateur": "string",
  "postNomUtilisateur": "string",
  "prenomUtilisateur": "string",
  "email": "string",
  "téléphone": "string",
  "photoUrl": "string",
  "lieuNaissance": "string",
  "dateNaissance": "2024-01-01T00:00:00",
  "genre": "string",
  "statut": true,
  "idRole": 1,
  "idEcole": 1,
  "province": "string",
  "ville": "string",
  "commune": "string",
  "quartier": "string",
  "avenue": "string",
  "numero": "string"
}
```

#### Exemple de requête
```bash
curl -X PUT "http://192.168.100.17:5001/api/Utilisateur/1" \
  -H "Content-Type: application/json" \
  -d '{
    "idUtilisateur": 1,
    "nomUtilisateur": "Doe",
    "email": "john.doe@example.com",
    "téléphone": "+243123456789",
    "statut": true,
    "idRole": 1,
    "idEcole": 1
  }'
```

#### Réponse de succès (204)
Pas de contenu retourné.

#### Codes d'erreur
- **400**: Données de requête invalides ou ID incorrect
- **404**: Utilisateur non trouvé
- **500**: Erreur serveur

### Supprimer un utilisateur
**DELETE** `/api/Utilisateur/{id}`

---

## 🏫 Gestion des Écoles

### Récupérer toutes les écoles
**GET** `/api/Ecole`

### Récupérer une école par ID
**GET** `/api/Ecole/{id}`

### Récupérer une école par nom
**GET** `/api/Ecole/nom/{nom}`

### Récupérer les classes d'une école
**GET** `/api/Ecole/{id}/classes`

### Récupérer les utilisateurs d'une école
**GET** `/api/Ecole/{id}/utilisateurs`

### Récupérer les tuteurs d'une école
**GET** `/api/Ecole/{id}/tuteurs`

### Récupérer les enseignants d'une école
**GET** `/api/Ecole/{id}/enseignants`

### Créer une école
**POST** `/api/Ecole`

#### Modèle Ecole
```json
{
  "nom": "string",
  "slogan": "string",
  "type": "string",
  "logoUrl": "string",
  "téléphone": "string",
  "emailContact": "string",
  "siteWeb": "string",
  "capaciteEleve": 0,
  "nombreSalles": 0,
  "description": "string",
  "statut": true,
  "province": "string",
  "ville": "string",
  "commune": "string",
  "quartier": "string",
  "avenue": "string",
  "numero": "string"
}
```

### Modifier une école
**PUT** `/api/Ecole/{id}`

### Supprimer une école
**DELETE** `/api/Ecole/{id}`

---

## 👨‍🎓 Gestion des Élèves

### Récupérer tous les élèves (vue complète)
**GET** `/api/V_Eleve`

### Récupérer un élève par ID
**GET** `/api/Eleve/{id}`

### Récupérer un élève par référence
**GET** `/api/Eleve/reference/{reference}`

### Récupérer les élèves d'une classe
**GET** `/api/Eleve/classe/{idClasse}`

### Récupérer les élèves d'un tuteur
**GET** `/api/Eleve/tuteur/{idTuteur}`

### Récupérer les élèves d'une école
**GET** `/api/Eleve/ecole/{idEcole}`

### Récupérer les élèves par statut
**GET** `/api/Eleve/statut/{statut}`

### Récupérer les notes d'un élève
**GET** `/api/Eleve/{id}/notes`

### Récupérer les inscriptions d'un élève
**GET** `/api/Eleve/{id}/inscriptions`

### Récupérer les paiements d'un élève
**GET** `/api/Eleve/{id}/paiements`

### Créer un élève
**POST** `/api/Eleve`

#### Modèle Eleve
```json
{
  "matricule": "string",
  "nom": "string",
  "postNom": "string",
  "prenom": "string",
  "dateNaissance": "2024-01-01T00:00:00",
  "lieuNaissance": "string",
  "genre": "string",
  "nationalite": "string",
  "photoUrl": "string",
  "statut": true,
  "idClasse": 1,
  "idTuteur": 1,
  "province": "string",
  "ville": "string",
  "commune": "string",
  "quartier": "string",
  "avenue": "string",
  "numero": "string"
}
```

### Modifier un élève
**PUT** `/api/Eleve/{id}`

### Supprimer un élève
**DELETE** `/api/Eleve/{id}`

---

## 📚 Gestion des Classes

### Récupérer toutes les classes
**GET** `/api/Classe`

### Récupérer une classe par ID
**GET** `/api/Classe/{id}`

### Récupérer les classes d'une école
**GET** `/api/Classe/ecole/{idEcole}`

### Récupérer les classes d'une section
**GET** `/api/Classe/section/{idSection}`

### Récupérer les classes d'une option
**GET** `/api/Classe/option/{idOption}`

### Récupérer les élèves d'une classe
**GET** `/api/Classe/{id}/eleves`

### Récupérer les cours d'une classe
**GET** `/api/Classe/{id}/cours`

### Créer une classe
**POST** `/api/Classe`

#### Modèle Classe
```json
{
  "nom": "string",
  "description": "string",
  "capacite": 0,
  "statut": true,
  "idEcole": 1,
  "idSection": 1,
  "idOption": 1
}
```

### Modifier une classe
**PUT** `/api/Classe/{id}`

### Supprimer une classe
**DELETE** `/api/Classe/{id}`

---

## 📝 Gestion des Notes

### Récupérer toutes les notes
**GET** `/api/Note`

### Récupérer une note par ID
**GET** `/api/Note/{id}`

### Récupérer les notes d'un élève
**GET** `/api/Note/eleve/{idEleve}`

### Récupérer les notes d'un cours
**GET** `/api/Note/cours/{idCours}`

### Récupérer les notes d'une évaluation
**GET** `/api/Note/evaluation/{idEvaluation}`

### Créer une note
**POST** `/api/Note`

#### Modèle Note
```json
{
  "valeur": 0.0,
  "coefficient": 1.0,
  "commentaire": "string",
  "dateEvaluation": "2024-01-01T00:00:00",
  "idEleve": 1,
  "idCours": 1,
  "idEvaluation": 1
}
```

### Modifier une note
**PUT** `/api/Note/{id}`

### Supprimer une note
**DELETE** `/api/Note/{id}`

---

## 📊 Gestion des Présences

### Récupérer toutes les présences
**GET** `/api/Presence`

### Récupérer une présence par ID
**GET** `/api/Presence/{id}`

### Récupérer les présences d'un élève
**GET** `/api/Presence/eleve/{idEleve}`

### Récupérer les présences d'une classe
**GET** `/api/Presence/classe/{idClasse}`

### Récupérer les présences d'une date
**GET** `/api/Presence/date/{date}`

### Créer une présence
**POST** `/api/Presence`

#### Modèle Presence
```json
{
  "date": "2024-01-01T00:00:00",
  "heureArrivee": "08:00:00",
  "heureDepart": "16:00:00",
  "statut": "Present",
  "commentaire": "string",
  "idEleve": 1,
  "idVacation": 1
}
```

### Modifier une présence
**PUT** `/api/Presence/{id}`

### Supprimer une présence
**DELETE** `/api/Presence/{id}`

---

## 💰 Gestion des Paiements

### Récupérer tous les paiements
**GET** `/api/Paiement`

### Récupérer un paiement par ID
**GET** `/api/Paiement/{id}`

### Récupérer les paiements d'un élève
**GET** `/api/Paiement/eleve/{idEleve}`

### Récupérer les paiements d'une école
**GET** `/api/Paiement/ecole/{idEcole}`

### Récupérer les paiements d'une date
**GET** `/api/Paiement/date/{date}`

### Créer un paiement
**POST** `/api/Paiement`

#### Modèle Paiement
```json
{
  "montant": 0.0,
  "datePaiement": "2024-01-01T00:00:00",
  "methodePaiement": "string",
  "referencePaiement": "string",
  "statut": "Payé",
  "commentaire": "string",
  "idEleve": 1,
  "idFrais": 1
}
```

### Modifier un paiement
**PUT** `/api/Paiement/{id}`

### Supprimer un paiement
**DELETE** `/api/Paiement/{id}`

---

## 📋 Gestion des Inscriptions

### Récupérer toutes les inscriptions
**GET** `/api/Inscription`

### Récupérer une inscription par ID
**GET** `/api/Inscription/{id}`

### Récupérer les inscriptions d'un élève
**GET** `/api/Inscription/eleve/{idEleve}`

### Récupérer les inscriptions d'une école
**GET** `/api/Inscription/ecole/{idEcole}`

### Récupérer les inscriptions d'une année scolaire
**GET** `/api/Inscription/annee/{idAnneeScolaire}`

### Créer une inscription
**POST** `/api/Inscription`

#### Modèle Inscription
```json
{
  "dateInscription": "2024-01-01T00:00:00",
  "statut": "Inscrit",
  "commentaire": "string",
  "idEleve": 1,
  "idClasse": 1,
  "idAnneeScolaire": 1
}
```

### Modifier une inscription
**PUT** `/api/Inscription/{id}`

### Supprimer une inscription
**DELETE** `/api/Inscription/{id}`

---

## 📚 Gestion des Cours

### Récupérer tous les cours
**GET** `/api/Cours`

### Récupérer un cours par ID
**GET** `/api/Cours/{id}`

### Récupérer les cours d'une classe
**GET** `/api/Cours/classe/{idClasse}`

### Récupérer les cours d'un enseignant
**GET** `/api/Cours/enseignant/{idEnseignant}`

### Créer un cours
**POST** `/api/Cours`

#### Modèle Cours
```json
{
  "nom": "string",
  "description": "string",
  "coefficient": 1.0,
  "statut": true,
  "idClasse": 1,
  "idEnseignant": 1
}
```

### Modifier un cours
**PUT** `/api/Cours/{id}`

### Supprimer un cours
**DELETE** `/api/Cours/{id}`

---

## 👨‍🏫 Gestion des Enseignants

### Récupérer tous les enseignants
**GET** `/api/Enseignant`

### Récupérer un enseignant par ID
**GET** `/api/Enseignant/{id}`

### Récupérer les enseignants d'une école
**GET** `/api/Enseignant/ecole/{idEcole}`

### Créer un enseignant
**POST** `/api/Enseignant`

#### Modèle Enseignant
```json
{
  "specialite": "string",
  "diplome": "string",
  "anneesExperience": 0,
  "statut": true,
  "idUtilisateur": 1,
  "idEcole": 1
}
```

### Modifier un enseignant
**PUT** `/api/Enseignant/{id}`

### Supprimer un enseignant
**DELETE** `/api/Enseignant/{id}`

---

## 👨‍👩‍👧‍👦 Gestion des Tuteurs

### Récupérer tous les tuteurs
**GET** `/api/Tuteur`

### Récupérer un tuteur par ID
**GET** `/api/Tuteur/{id}`

### Récupérer les tuteurs d'une école
**GET** `/api/Tuteur/ecole/{idEcole}`

### Créer un tuteur
**POST** `/api/Tuteur`

#### Modèle Tuteur
```json
{
  "relation": "string",
  "profession": "string",
  "statut": true,
  "idUtilisateur": 1,
  "idEcole": 1
}
```

### Modifier un tuteur
**PUT** `/api/Tuteur/{id}`

### Supprimer un tuteur
**DELETE** `/api/Tuteur/{id}`

---

## 📅 Gestion des Vacations

### Récupérer toutes les vacations
**GET** `/api/Vacation`

### Récupérer une vacation par ID
**GET** `/api/Vacation/{id}`

### Récupérer les vacations d'une classe
**GET** `/api/Vacation/classe/{idClasse}`

### Récupérer les vacations d'un cours
**GET** `/api/Vacation/cours/{idCours}`

### Créer une vacation
**POST** `/api/Vacation`

#### Modèle Vacation
```json
{
  "date": "2024-01-01T00:00:00",
  "heureDebut": "08:00:00",
  "heureFin": "09:00:00",
  "salle": "string",
  "statut": "Planifiée",
  "idClasse": 1,
  "idCours": 1
}
```

### Modifier une vacation
**PUT** `/api/Vacation/{id}`

### Supprimer une vacation
**DELETE** `/api/Vacation/{id}`

---

## 💸 Gestion des Frais

### Récupérer tous les frais
**GET** `/api/Frais`

### Récupérer un frais par ID
**GET** `/api/Frais/{id}`

### Récupérer les frais d'une classe
**GET** `/api/Frais/classe/{idClasse}`

### Récupérer les frais d'une école
**GET** `/api/Frais/ecole/{idEcole}`

### Créer un frais
**POST** `/api/Frais`

#### Modèle Frais
```json
{
  "nom": "string",
  "description": "string",
  "montant": 0.0,
  "periode": "string",
  "dateEcheance": "2024-01-01T00:00:00",
  "statut": true,
  "idClasse": 1
}
```

### Modifier un frais
**PUT** `/api/Frais/{id}`

### Supprimer un frais
**DELETE** `/api/Frais/{id}`

---

## 📧 Gestion des Messages

### Récupérer tous les messages
**GET** `/api/Message`

### Récupérer un message par ID
**GET** `/api/Message/{id}`

### Récupérer les messages d'un groupe
**GET** `/api/Message/groupe/{idGroupe}`

### Récupérer les messages d'un utilisateur
**GET** `/api/Message/utilisateur/{idUtilisateur}`

### Créer un message
**POST** `/api/Message`

#### Modèle Message
```json
{
  "contenu": "string",
  "dateEnvoi": "2024-01-01T00:00:00",
  "statut": "Envoyé",
  "idExpediteur": 1,
  "idDestinataire": 1,
  "idGroupe": 1
}
```

### Modifier un message
**PUT** `/api/Message/{id}`

### Supprimer un message
**DELETE** `/api/Message/{id}`

---

## 🔔 Gestion des Notifications

### Récupérer toutes les notifications
**GET** `/api/Notification`

### Récupérer une notification par ID
**GET** `/api/Notification/{id}`

### Récupérer les notifications d'un utilisateur
**GET** `/api/Notification/utilisateur/{idUtilisateur}`

### Créer une notification
**POST** `/api/Notification`

#### Modèle Notification
```json
{
  "titre": "string",
  "contenu": "string",
  "type": "string",
  "dateCreation": "2024-01-01T00:00:00",
  "statut": "Non lu",
  "idDestinataire": 1
}
```

### Modifier une notification
**PUT** `/api/Notification/{id}`

### Supprimer une notification
**DELETE** `/api/Notification/{id}`

---

## 📊 Vues Spécialisées

### Vue des élèves par école
**GET** `/api/EleveParEcole`

### Vue des paiements et frais par école
**GET** `/api/VuePaiementsFraisParEcole`

### Vue des pointages de présence par école
**GET** `/api/VuePointagePresenceParEcole`

### Vue du répertoire des enseignants par parent
**GET** `/api/VueRepertoireEnseignantsParParent`

---

## 🚨 Codes d'erreur HTTP

| Code | Description |
|------|-------------|
| 200 | Succès - Requête traitée avec succès |
| 201 | Créé - Ressource créée avec succès |
| 400 | Requête incorrecte - Données invalides |
| 401 | Non autorisé - Authentification requise |
| 403 | Interdit - Accès refusé |
| 404 | Non trouvé - Ressource introuvable |
| 409 | Conflit - Ressource en conflit |
| 422 | Entité non traitable - Validation échouée |
| 500 | Erreur serveur - Erreur interne |

---

## 📝 Exemples d'utilisation

### Authentification et récupération des données utilisateur
```javascript
// Configuration de l'API selon l'environnement
const API_BASE_URL = process.env.NODE_ENV === 'development' 
  ? 'http://192.168.100.17:5001'  // Développement
  : 'https://localhost:7155';     // Production

// 1. Authentification
const authResponse = await fetch(`${API_BASE_URL}/api/Utilisateur/authentifier`, {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  credentials: 'include', // Important pour CORS
  body: JSON.stringify({
    emailOuTelephone: 'admin@example.com',
    motDePasse: 'password123'
  })
});

const userData = await authResponse.json();

// 2. Récupération des élèves de l'école
const elevesResponse = await fetch(`${API_BASE_URL}/api/Eleve/ecole/${userData.idEcole}`, {
  credentials: 'include'
});
const eleves = await elevesResponse.json();

// 3. Récupération des classes de l'école
const classesResponse = await fetch(`${API_BASE_URL}/api/Classe/ecole/${userData.idEcole}`, {
  credentials: 'include'
});
const classes = await classesResponse.json();
```

### Création d'un nouvel élève
```javascript
const newEleve = {
  matricule: "ELEVE001",
  nom: "Doe",
  postNom: "Smith",
  prenom: "John",
  dateNaissance: "2010-05-15T00:00:00",
  lieuNaissance: "Kinshasa",
  genre: "M",
  nationalite: "Congolais",
  statut: true,
  idClasse: 1,
  idTuteur: 1,
  province: "Kinshasa",
  ville: "Kinshasa",
  commune: "Gombe",
  quartier: "Centre-ville",
  avenue: "Avenue du Commerce",
  numero: "123"
};

const response = await fetch(`${API_BASE_URL}/api/Eleve`, {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  credentials: 'include',
  body: JSON.stringify(newEleve)
});

const createdEleve = await response.json();
```

### Gestion des erreurs
```javascript
try {
  const response = await fetch(`${API_BASE_URL}/api/Eleve/999`, {
    credentials: 'include'
  });
  
  if (!response.ok) {
    if (response.status === 404) {
      console.error('Élève non trouvé');
    } else if (response.status === 401) {
      console.error('Authentification requise');
    } else {
      console.error('Erreur serveur:', response.status);
    }
    return;
  }
  
  const eleve = await response.json();
} catch (error) {
  console.error('Erreur de connexion:', error);
}
```

---

## 🔧 Configuration CORS

L'API est configurée pour accepter les requêtes depuis n'importe quelle origine en développement. Pour la production, vous devrez configurer les origines autorisées.

### Configuration actuelle
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});
```

---

## 📚 Ressources supplémentaires

- **Documentation Swagger**: `https://localhost:7155/swagger`
- **Architecture du projet**: Voir le fichier `ARCHITECTURE.md`
- **Tests HTTP**: Voir les fichiers `.http` dans le répertoire racine

---

## 🤝 Support

Pour toute question ou problème avec l'API, veuillez contacter l'équipe de développement backend.

---

*Documentation générée le: ${new Date().toLocaleDateString('fr-FR')}*
