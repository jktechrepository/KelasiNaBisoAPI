# 🚀 Guide d'Utilisation - API KelasiNaBiso avec MariaDB

## 🎯 Démarrage Rapide

Votre API est **déjà démarrée et fonctionnelle** ! 🎉

---

## 📍 Accès à l'API

### URLs disponibles
```
HTTP:  http://localhost:5002
HTTPS: https://localhost:7102
Swagger: http://localhost:5002/swagger  ⭐ Commencez ici !
```

### Ouvrir Swagger
1. **Ouvrez votre navigateur**
2. **Allez sur** : http://localhost:5002/swagger
3. **Explorez les endpoints** disponibles

---

## 🔐 Premier Test : Authentification

### Se connecter en tant que Super-Admin

Dans **Swagger** :

1. Cherchez l'endpoint **`POST /api/Auth/login`**
2. Cliquez sur **"Try it out"**
3. Entrez ces informations :
```json
{
  "email": "superadmin@kelasinabiso.cd",
  "password": "Super-Admin"
}
```
4. Cliquez sur **"Execute"**

### Résultat attendu
```json
{
  "message": "Connexion réussie",
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "user": {
    "id": 1,
    "nom": "Super",
    "postnom": "Admin",
    "prenom": "Administrateur",
    "email": "superadmin@kelasinabiso.cd",
    "role": "Super-Admin",
    "ecole": "Ekelasi School"
  }
}
```

✅ **Vous êtes connecté !**

---

## 📚 Endpoints Principaux

### 🏫 Gestion des Écoles

#### Récupérer toutes les écoles
```
GET /api/Ecole
```

#### Créer une nouvelle école
```
POST /api/Ecole
{
  "nom": "Mon École",
  "slogan": "Excellence et Innovation",
  "type": "Privée",
  "provinceEducationnel": "Kinshasa",
  "nomCompletResponsable": "Directeur Principal",
  "description": "Une école d'excellence"
}
```

**Note** : Un utilisateur Admin sera créé automatiquement !

---

### 👥 Gestion des Utilisateurs

#### Récupérer tous les utilisateurs (avec détails)
```
GET /api/V_Utilisateur
```

#### Créer un utilisateur
```
POST /api/Utilisateur
{
  "nomUtilisateur": "Kabila",
  "postNomUtilisateur": "Mbuyi",
  "prenomUtilisateur": "Joseph",
  "email": "joseph@test.cd",
  "telephone": "+243999111222",
  "motDePasseHash": "password123",  // Sera hashé automatiquement
  "genre": "M",
  "idRole": 1,
  "idEcole": 1
}
```

---

### 👨‍🏫 Gestion des Agents (Enseignants)

#### Récupérer tous les agents
```
GET /api/Agent
```

#### Créer un agent/enseignant
```
POST /api/Agent
{
  "matricule": "AG001",
  "nom": "Mukendi",
  "postnom": "Tshilombo",
  "prenom": "Pierre",
  "genre": "M",
  "dateNaissance": "1985-03-20",
  "telephoneAgent": "+243999333444",
  "emailAgent": "pierre.mukendi@ecole.cd",
  "etatCivil": "Marié",
  "fonction": "Enseignant",
  "roleAgent": "Professeur de Mathématiques",
  "idEcole": 1
}
```

#### Récupérer les agents d'une école
```
GET /api/Agent/ecole/1
```

---

### 👨‍🎓 Gestion des Élèves

#### Récupérer tous les élèves (avec détails complets)
```
GET /api/V_Eleve
```

#### Récupérer les élèves d'une école
```
GET /api/EleveParEcole/ecole/1
```

#### Créer un élève
```
POST /api/Eleve
{
  "nom": "Tshisekedi",
  "postnom": "Lukonde",
  "prenom": "Marie",
  "genre": "F",
  "dateNaissance": "2010-05-15",
  "lieuNaissance": "Kinshasa",
  "nationalite": "Congolaise",
  "matricule": "ELEV001",
  "idClasse": 1,
  "idTuteur": 1
}
```

---

### 📚 Gestion des Classes

#### Récupérer toutes les classes
```
GET /api/Classe
```

#### Créer une classe
```
POST /api/Classe
{
  "nomClasse": "6ème Année Primaire A",
  "idDirection": 1,
  "idSection": 1,
  "statut": true
}
```

---

### 📊 Gestion des Présences

#### Marquer une présence
```
POST /api/Presence
{
  "idEleve": 1,
  "dateDuJour": "2025-10-23",
  "heureArrivee": "08:00:00",
  "heureDepart": "16:00:00",
  "statutPresence": "Present",
  "idVacation": 1
}
```

#### Voir les présences par école
```
GET /api/VuePointagePresenceParEcole/ecole/1
```

---

### 💰 Gestion des Paiements

#### Enregistrer un paiement
```
POST /api/Paiement
{
  "datePaiement": "2025-10-23",
  "montant": 50000,
  "devise": "CDF",
  "modePaiement": "Cash",
  "statut": "Payé",
  "idEleve": 1,
  "idFrais": 1,
  "idUtilisateur": 1
}
```

#### Voir les paiements par école
```
GET /api/VuePaiementsFraisParEcole/ecole/1
```

---

## 🎓 Cas d'usage complets

### Scénario 1 : Créer une nouvelle école avec son admin

1. **Créer l'école** (POST /api/Ecole)
```json
{
  "nom": "Institut Excellence",
  "slogan": "Vers l'excellence",
  "type": "Privée",
  "provinceEducationnel": "Kinshasa",
  "nomCompletResponsable": "Dr. Kabongo",
  "telephone": "+243999888777",
  "emailContact": "contact@institut-excellence.cd"
}
```

2. **Récupérer l'ID de l'école créée** (dans la réponse)
3. **Un utilisateur Admin est créé automatiquement !**
   - Email : `drkabongo1@kelasinabiso.cd` (exemple)
   - Mot de passe : `Admin`

---

### Scénario 2 : Inscription d'un nouvel élève

1. **Créer d'abord un tuteur** (POST /api/Tuteur)
```json
{
  "nomComplet": "Papa Mbala",
  "genre": "M",
  "email": "papa.mbala@email.cd",
  "telephone": "+243999222333",
  "idEcole": 1
}
```

2. **Créer l'élève** (POST /api/Eleve)
```json
{
  "nom": "Mbala",
  "postnom": "Junior",
  "prenom": "Patrick",
  "genre": "M",
  "dateNaissance": "2012-08-10",
  "nationalite": "Congolais",
  "idClasse": 1,
  "idTuteur": 1  // ID du tuteur créé à l'étape 1
}
```

3. **Créer l'inscription** (POST /api/Inscription)
```json
{
  "type": "Inscription",
  "idEleve": 1,  // ID de l'élève créé
  "idEcole": 1,
  "idClasse": 1,
  "idAnneeScolaire": 1,
  "dateInscription": "2025-10-23",
  "statutInscription": "Confirmée"
}
```

---

### Scénario 3 : Affecter un cours à un agent

1. **Créer un cours** (POST /api/Cours)
```json
{
  "nomCours": "Mathématiques",
  "description": "Cours de mathématiques niveau primaire",
  "idClasse": 1
}
```

2. **Créer une affectation** (POST /api/AffectationCours)
```json
{
  "idAgent": 1,
  "idCours": 1,
  "idAnneeScolaire": 1,
  "statut": true
}
```

---

## 🔍 Vues SQL disponibles

Ces vues offrent des données **pré-formatées et optimisées** :

### 1. V_Utilisateur
**Endpoint** : `GET /api/V_Utilisateur`  
**Contenu** : Utilisateurs avec leur rôle et école

### 2. V_Eleve
**Endpoint** : `GET /api/V_Eleve`  
**Contenu** : Élèves avec classe, tuteur, section, option, école

### 3. EleveParEcole
**Endpoint** : `GET /api/EleveParEcole/ecole/{idEcole}`  
**Contenu** : Tous les élèves d'une école avec détails complets

### 4. VuePaiementsFraisParEcole
**Endpoint** : `GET /api/VuePaiementsFraisParEcole/ecole/{idEcole}`  
**Contenu** : Paiements et frais par école avec détails élèves

### 5. VuePointagePresenceParEcole
**Endpoint** : `GET /api/VuePointagePresenceParEcole/ecole/{idEcole}`  
**Contenu** : Présences avec détails élèves, classes, vacations

### 6. Vue_RepertoireAgentsParParent
**Endpoint** : `GET /api/VueRepertoireAgentsParParent/tuteur/{idTuteur}`  
**Contenu** : Répertoire des agents/enseignants des élèves d'un parent

---

## 📱 Intégration Frontend

### Exemple avec JavaScript/React

```javascript
const API_BASE_URL = 'http://localhost:5002';

// 1. Authentification
const login = async (email, password) => {
  const response = await fetch(`${API_BASE_URL}/api/Auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password })
  });
  
  const data = await response.json();
  localStorage.setItem('token', data.token);
  return data;
};

// 2. Récupérer les écoles
const getEcoles = async () => {
  const response = await fetch(`${API_BASE_URL}/api/Ecole`);
  return await response.json();
};

// 3. Récupérer les agents d'une école
const getAgents = async (idEcole) => {
  const response = await fetch(`${API_BASE_URL}/api/Agent/ecole/${idEcole}`);
  return await response.json();
};

// 4. Créer un élève
const createEleve = async (eleveData) => {
  const response = await fetch(`${API_BASE_URL}/api/Eleve`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(eleveData)
  });
  return await response.json();
};
```

---

## 🔧 Commandes PowerShell

### Démarrer l'API
```powershell
cd G:\KelasiNaBiso\KelasiNaBisoAPI
dotnet run
```

### Arrêter l'API
```powershell
# Appuyez sur Ctrl+C dans le terminal
```

### Redémarrer l'API
```powershell
# Arrêter avec Ctrl+C
dotnet run
```

### Tester un endpoint
```powershell
# Test simple
Invoke-RestMethod -Uri "http://localhost:5002/api/Ecole" -Method Get

# Test avec authentification
$body = @{
    email = "superadmin@kelasinabiso.cd"
    password = "Super-Admin"
} | ConvertTo-Json

$response = Invoke-RestMethod `
    -Uri "http://localhost:5002/api/Auth/login" `
    -Method Post `
    -Body $body `
    -ContentType "application/json"

$token = $response.token
Write-Host "Token: $token"
```

---

## 📊 Tableau de bord des données

### Vérifier l'état de la base de données

```sql
-- Se connecter à MariaDB
mysql -u kansa -pkansa2025

-- Utiliser la base
USE KelasiNaBisoDb;

-- Compter les données
SELECT 'Ecoles' AS Table_Name, COUNT(*) AS Count FROM Ecoles
UNION ALL
SELECT 'Utilisateurs', COUNT(*) FROM Utilisateurs
UNION ALL
SELECT 'Agents', COUNT(*) FROM Agents
UNION ALL
SELECT 'Eleves', COUNT(*) FROM Eleves
UNION ALL
SELECT 'Classes', COUNT(*) FROM Classes
UNION ALL
SELECT 'Tuteurs', COUNT(*) FROM Tuteurs
UNION ALL
SELECT 'Inscriptions', COUNT(*) FROM Inscriptions
UNION ALL
SELECT 'Presences', COUNT(*) FROM Presences
UNION ALL
SELECT 'Paiements', COUNT(*) FROM Paiements;
```

---

## 🎯 Flux de travail typique

### 1. Configurer l'école
```
1. Créer une école (POST /api/Ecole)
2. Créer des sections (POST /api/Section)
3. Créer des directions (POST /api/Direction)
4. Créer des classes (POST /api/Classe)
5. Créer une année scolaire (POST /api/AnneeScolaire)
```

### 2. Ajouter le personnel
```
1. Créer des utilisateurs (POST /api/Utilisateur)
2. Créer des agents/enseignants (POST /api/Agent)
3. Créer des cours (POST /api/Cours)
4. Affecter les cours aux agents (POST /api/AffectationCours)
```

### 3. Inscrire des élèves
```
1. Créer des tuteurs (POST /api/Tuteur)
2. Créer des élèves (POST /api/Eleve)
3. Créer des inscriptions (POST /api/Inscription)
4. Définir les frais (POST /api/Frais)
```

### 4. Gestion quotidienne
```
1. Marquer les présences (POST /api/Presence)
2. Enregistrer les notes (POST /api/Note)
3. Enregistrer les paiements (POST /api/Paiement)
4. Envoyer des notifications (POST /api/Notification)
```

---

## 📖 Documentation complète

### Guides d'utilisation
- **API_DOCUMENTATION.md** - Tous les endpoints avec exemples
- **FRONTEND_DEVELOPER_GUIDE.md** - Guide pour développeurs frontend
- **ARCHITECTURE.md** - Architecture du projet

### Guides de migration
- **MIGRATION_MYSQL_TO_MARIADB.md** - Migration complète
- **QUICK_START_MARIADB.md** - Démarrage rapide
- **NETTOYAGE_CODE_OBSOLETE.md** - Nettoyage effectué

### Guides techniques
- **README.md** - Vue d'ensemble
- **RESUME_FINAL_MIGRATION.md** - Résumé des modifications
- **TEST_API_MARIADB.md** - Tests complets

---

## 🛠️ Maintenance

### Redémarrer MariaDB
```powershell
# Arrêter
net stop mariadb

# Démarrer
net start mariadb
```

### Sauvegarder la base de données
```bash
mysqldump -u kansa -pkansa2025 \
  --databases KelasiNaBisoDb \
  --routines --triggers --events \
  > backup_$(date +%Y%m%d).sql
```

### Restaurer une sauvegarde
```bash
mysql -u kansa -pkansa2025 < backup_20251023.sql
```

---

## 🔒 Sécurité

### ⚠️ À FAIRE AVANT LA PRODUCTION

1. **Changer les mots de passe par défaut**
```
❌ Super-Admin: "Super-Admin"
❌ Admins: "Admin"

✅ Utiliser des mots de passe forts !
```

2. **Sécuriser la clé JWT**
```
❌ appsettings.json (visible)
✅ Variables d'environnement
✅ Azure Key Vault
```

3. **Configurer HTTPS**
```
❌ HTTP en production
✅ HTTPS uniquement
```

---

## 📊 Monitoring

### Vérifier les logs de l'API
Les logs s'affichent dans la console où vous avez lancé `dotnet run`.

**Informations disponibles** :
- Connexions à la base de données
- Authentifications
- Erreurs
- Requêtes SQL exécutées

### Logs MariaDB
```bash
# Localisation (Windows)
C:\Program Files\MariaDB 10.11\data\*.err

# Voir les dernières erreurs
Get-Content "C:\Program Files\MariaDB 10.11\data\*.err" -Tail 50
```

---

## ❓ FAQ

### Q: Comment ajouter un nouvel endpoint ?
**R:** Créez un nouveau contrôleur dans le dossier `Controllers/`.

### Q: Comment modifier une table ?
**R:** Créez une migration EF Core :
```bash
dotnet ef migrations add NomDeLaMigration
dotnet ef database update
```

### Q: L'API ne démarre pas ?
**R:** Vérifiez :
1. MariaDB est démarré
2. La chaîne de connexion est correcte
3. Le port 5002 est disponible

### Q: Erreur de connexion à MariaDB ?
**R:** Testez la connexion manuellement :
```bash
mysql -u kansa -pkansa2025 -e "SELECT VERSION();"
```

---

## 🎯 Prochaines étapes

### Pour tester l'API complètement

1. ✅ **Ouvrez Swagger** : http://localhost:5002/swagger
2. ✅ **Testez l'authentification** avec Super-Admin
3. ✅ **Créez une école de test**
4. ✅ **Créez un agent/enseignant**
5. ✅ **Explorez les autres endpoints**

### Pour développer un frontend

1. 📖 Consultez `FRONTEND_DEVELOPER_GUIDE.md`
2. 📖 Consultez `API_DOCUMENTATION.md`
3. 🧪 Testez tous les endpoints dans Swagger
4. 💻 Commencez à coder !

---

## 🎉 Conclusion

Votre API **KelasiNaBiso** est maintenant :
- ✅ **Fonctionnelle** avec MariaDB 10.11
- ✅ **Propre** sans code obsolète
- ✅ **Documentée** exhaustivement
- ✅ **Testable** via Swagger
- ✅ **Prête** pour le développement

**Commencez à tester dès maintenant !** 🚀

---

## 🌟 Support

### Documentation
- Tous les fichiers `.md` dans le projet
- Collection Postman : `KelasiNaBiso_API_Collection.postman_collection.json`

### Outils recommandés
- **Swagger** : http://localhost:5002/swagger (intégré)
- **Postman** : Pour tester les API
- **HeidiSQL** : Pour gérer MariaDB
- **VS Code** : Pour le développement

---

**Bon développement ! 🚀**

---

*Guide créé le: 23 octobre 2025*  
*API: KelasiNaBiso v1*  
*Base de données: MariaDB 10.11 LTS*  
*Status: ✅ OPÉRATIONNEL*

